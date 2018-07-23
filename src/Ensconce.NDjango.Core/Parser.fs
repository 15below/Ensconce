(****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Parser.
 *
 *  The NDjango Parser is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  The NDjango Parser is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NDjango Parser.  If not, see <http://www.gnu.org/licenses/>.
 *  
 ***************************************************************************)

/// This is the Django template system.
/// 
/// How it works:
/// 
/// The Lexer.tokenize() function converts a template string (i.e., a string containing
/// markup with custom template tags) to tokens, which can be either plain text
/// (TOKEN_TEXT), variables (TOKEN_VAR) or block statements (TOKEN_BLOCK).
/// 
/// The Parser() class takes a list of tokens in its constructor, and its parse()
/// method returns a compiled template -- which is, under the hood, a list of
/// Node objects.
/// 
/// Each Node is responsible for creating some sort of output -- e.g. simple text
/// (TextNode), variable values in a given context (VariableNode), results of basic
/// logic (IfNode), results of looping (ForNode), or anything else. The core Node
/// types are TextNode, VariableNode, IfNode and ForNode, but plugin modules can
/// define their own custom node types.
/// 
/// Each Node has a render() method, which takes a Context and returns a string of
/// the rendered node. For example, the render() method of a Variable Node returns
/// the variable's value as a string. The render() method of an IfNode returns the
/// rendered output of whatever was inside the loop, recursively.
/// 
/// The Template class is a convenient wrapper that takes care of template
/// compilation and rendering.
/// 
/// Usage:
/// 
/// The only thing you should ever use directly in this file is the Template class.
/// Create a compiled template object with a template_string, then call render()
/// with a context. In the compilation stage, the TemplateSyntaxError exception
/// will be raised if the template doesn't have proper syntax.
/// 
/// Sample code:
/// 
/// >>> from django import template
/// >>> s = u'<html>{% if test %}<h1>{{ varvalue }}</h1>{% endif %}</html>'
/// >>> t = template.Template(s)
/// 
/// (t is now a compiled template, and its render() method can be called multiple
/// times with multiple contexts)
/// 
/// >>> c = template.Context({'test':True, 'varvalue': 'Hello'})
/// >>> t.render(c)
/// u'<html><h1>Hello</h1></html>'
/// >>> c = template.Context({'test':False, 'varvalue': 'Hello'})
/// >>> t.render(c)
/// u'<html></html>'


namespace NDjango

open System.Text
open System.Text.RegularExpressions
open System.Collections
open System.Reflection

open NDjango.Interfaces
open NDjango.ASTNodes
open Expressions
open OutputHandling

module internal Parser =

    /// Default parser implementation
    type public DefaultParser(provider: ITemplateManagerProvider) =

        /// parses a single token, returning an AST Node list. this function may advance the token stream if an 
        /// element consuming multiple tokens is encountered. In this scenario, the Node list returned will
        /// contain nodes for all of the advanced tokens.
        let parse_token (parser: DefaultParser) tokens (token:NDjango.Lexer.Token) = 
            match token with

            | Lexer.Text textToken -> 
                ({new Node(token)
                    with 
                        override this.walk manager walker = 
                            {walker with buffer = textToken.Text}
                } :> INode), tokens
                
            | Lexer.Variable var -> 
                let expression = new FilterExpression(provider, Lexer.Variable var, var.Expression)
                ({new Node(token)
                    with 
                        override this.walk manager walker = 
                            match expression.ResolveForOutput manager walker with
                            | Some w -> w
                            | None -> walker
                        override this.GetVariables = expression.GetVariables
                } :> INode), tokens
                
            | Lexer.Block block -> 
                match Map.tryFind block.Verb provider.Tags with 
                | None -> raise (TemplateSyntaxError ("Invalid block tag:" + block.Verb, Some (block:>obj)))
                | Some (tag: ITag) -> tag.Perform block (parser :> IParser) tokens
            
            | Lexer.Comment comment -> 
                // include it in the output to cover all scenarios, but don't actually bring the comment across
                // the default behavior of the walk override is to return the same walker
                // Considering that when walk is called the buffer is empty, this will 
                // work for the comment node, so overriding the walk method here is unnecessary
                (new Node(token) :> INode), tokens 

        /// determines whether the given element is included in the termination token list
        let is_term_token elem (parse_until:string list) =
            if parse_until = [] then false else
                match elem with 
                | Lexer.Block block -> parse_until |> List.exists block.Verb.Equals
                | _ -> false
                
        let fail_unclosed_tags tag_list = raise (TemplateSyntaxError (sprintf "Unclosed tags %s " (List.fold (fun acc elem -> acc + ", " + elem) "" tag_list), None))

        /// recursively parses the token stream until the token(s) listed in parse_until are encountered.
        /// this function returns the node list and the unparsed remainder of the token stream
        /// the list is returned in the reversed order
        let rec parse_internal parser (nodes:INode list) (tokens : LazyList<Lexer.Token>) parse_until =
           match tokens with
           | LazyList.Nil ->  
                if not <| List.isEmpty parse_until then
                    fail_unclosed_tags parse_until
                (nodes, LazyList.empty<Lexer.Token>())
           | LazyList.Cons(token, tokens) -> 
                if is_term_token token parse_until then
                    ((new Node(token) :> INode) :: nodes, tokens)
                else
                    if List.isEmpty parse_until || LazyList.nonempty tokens || is_term_token token parse_until then
                        let node, tokens = parse_token parser tokens token
                        parse_internal parser (node :: nodes) tokens parse_until
                    else 
                        fail_unclosed_tags parse_until
                
        /// tries to return a list positioned just after one of the elements of parse_until. Returns None
        /// if no such element was found.
        let rec seek_internal parse_until tokens = 
            match tokens with 
            | LazyList.Nil -> fail_unclosed_tags parse_until
            | LazyList.Cons(token, tokens) -> 
                if is_term_token token parse_until then tokens
                else seek_internal parse_until tokens
        
        interface IParser with
            /// Parses the sequence of tokens until one of the given tokens is encountered
            member this.Parse tokens parse_until =
                let nodes, tokens = parse_internal this [] tokens parse_until
                (nodes |> List.rev, tokens)
            
            /// Repositions the token stream after the first token found from the parse_until list
            member this.Seek tokens parse_until = 
                if List.length parse_until = 0 then failwith "Seek must have at least one termination tag"
                else
                    seek_internal parse_until tokens
            
