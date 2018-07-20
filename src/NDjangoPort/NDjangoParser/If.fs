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


namespace NDjango.Tags

open System
open System.Collections
open System.Diagnostics

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.Expressions
open NDjango.ParserNodes

module internal If = 
    /// The ``{% if %}`` tag evaluates a variable, and if that variable is "true"
    /// (i.e., exists, is not empty, and is not a false boolean value), the
    /// contents of the block are output:
    /// 
    /// ::
    /// 
    ///     {% if athlete_list %}
    ///         Number of athletes: {{ athlete_list|count }}
    ///     {% else %}
    ///         No athletes.
    ///     {% endif %}
    /// 
    /// In the above, if ``athlete_list`` is not empty, the number of athletes will
    /// be displayed by the ``{{ athlete_list|count }}`` variable.
    /// 
    /// As you can see, the ``if`` tag can take an option ``{% else %}`` clause
    /// that will be displayed if the test fails.
    /// 
    /// ``if`` tags may use ``or``, ``and`` or ``not`` to test a number of
    /// variables or to negate a given variable::
    /// 
    ///     {% if not athlete_list %}
    ///         There are no athletes.
    ///     {% endif %}
    /// 
    ///     {% if athlete_list or coach_list %}
    ///         There are some athletes or some coaches.
    ///     {% endif %}
    /// 
    ///     {% if athlete_list and coach_list %}
    ///         Both atheletes and coaches are available.
    ///     {% endif %}
    /// 
    ///     {% if not athlete_list or coach_list %}
    ///         There are no athletes, or there are some coaches.
    ///     {% endif %}
    /// 
    ///     {% if athlete_list and not coach_list %}
    ///         There are some athletes and absolutely no coaches.
    ///     {% endif %}
    /// 
    /// ``if`` tags do not allow ``and`` and ``or`` clauses with the same tag,
    /// because the order of logic would be ambigous. For example, this is
    /// invalid::
    /// 
    ///     {% if athlete_list and coach_list or cheerleader_list %}
    /// 
    /// If you need to combine ``and`` and ``or`` to do advanced logic, just use
    /// nested if tags. For example::
    /// 
    ///     {% if athlete_list %}
    ///         {% if coach_list or cheerleader_list %}
    ///             We have athletes, and either coaches or cheerleaders!
    ///         {% endif %}
    ///     {% endif %}
    
    type Node(elements:INode list, resolver: IContext -> bool) =

        member x.Resolve context =
            resolver context

        member x.elements = elements

    type AndNode (left:Node, right:Node)=
        inherit Node(left.elements @ right.elements, 
                fun context -> left.Resolve(context) && right.Resolve(context))

    type OrNode (left:Node, right:Node)=
        inherit Node(left.elements @ right.elements,
                fun context -> left.Resolve(context) || right.Resolve(context))

    type NotNode (value:Node)=
        inherit Node(value.elements,
                fun context -> not <| value.Resolve(context))

    type BooleanNode (value:FilterExpression)=
        inherit Node(
            [value],
            fun context -> 
                match value.Resolve context true |> fst with
                | None -> false
                | Some v -> 
                    match v with 
                    | :? System.Boolean as b -> b                           // boolean value, take literal
                    | :? System.Collections.IEnumerable as e 
                                          -> e.GetEnumerator().MoveNext()   // some sort of collection, take if empty
                    | null -> false                                         // null evaluates to false
                    | _ -> true                                             // anything else. true because it's there
            )


    let Comparer (parser, left:TextToken, right:TextToken, comparer) =
        let compare (left:FilterExpression) (right:FilterExpression) (comparer: int->int-> bool) context =
                let left = 
                    match fst (left.Resolve context true) with
                    | Some o -> o
                    | None -> null
                let right = 
                    match fst (right.Resolve context true) with
                    | Some o -> o
                    | None -> null
                comparer (System.Collections.Comparer.Default.Compare(left,right)) 0
        let left = new FilterExpression(parser, left)
        let right = new FilterExpression(parser, right)
        Node(left:>INode :: [right], compare left right comparer)

    type TagNode(
                provider,
                token,
                tag,
                expression: Node, 
                node_list_true: NDjango.Interfaces.INodeImpl list, 
                node_list_false: NDjango.Interfaces.INodeImpl list
                ) =
        inherit NDjango.ParserNodes.TagNode(provider, token, tag)
             
        override x.walk manager walker =
            match expression.Resolve walker.context with
            | true -> {walker with parent=Some walker; nodes=node_list_true}
            | false -> {walker with parent=Some walker; nodes=node_list_false}

        override x.elements =
            base.elements
                @ expression.elements
            
        override x.Nodes =
            base.Nodes 
                |> Map.add (NDjango.Constants.NODELIST_IFTAG_IFTRUE) (node_list_true |> Seq.map (fun node -> (node :?> INode)))
                |> Map.add (NDjango.Constants.NODELIST_IFTAG_IFFALSE) (node_list_false |> Seq.map (fun node -> (node :?> INode)))

    [<NDjango.ParserNodes.Description("Outputs the content of enclosed tags based on expression evaluation result.")>]
    type Tag() =

        interface ITag with 
            member x.is_header_tag = false
            member this.Perform token context tokens =

                let node_list_true, remaining = (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["else"; "endif"]))
                let node_list_false, remaining2 =
                    match node_list_true.[node_list_true.Length-1].Token with
                    | NDjango.Lexer.Block b -> 
                        if b.Verb.RawText = "else" then
                            (context.Provider :?> IParser).Parse (Some token) remaining (context.WithClosures(["endif"]))
                        else
                            [], remaining
                    | _ -> [], remaining


                let build_term parser tokens =
                    
                    let build_comparer parser left right nodeBuilder =
                        (
                            new FilterExpression(parser, left),
                            new FilterExpression(parser, right)
                        ) |> nodeBuilder :> Node

                    match tokens with
                    | left::MatchToken("==")::right::tail ->
                        Comparer(parser, left, right, (=)), tail 

                    | left::MatchToken("!=")::right::tail -> 
                        Comparer(parser, left, right, (<>)), tail 

                    | left::MatchToken("<")::right::tail -> 
                        Comparer(parser, left, right, (<)), tail 

                    | left::MatchToken(">")::right::tail -> 
                        Comparer(parser, left, right, (>)), tail 

                    | left::MatchToken(">=")::right::tail -> 
                        Comparer(parser, left, right, (>=)), tail 

                    | left::MatchToken("<=")::right::tail -> 
                        Comparer(parser, left, right, (<=)), tail

                    | MatchToken("not")::term::tail ->
                        NotNode(
                            BooleanNode(FilterExpression(parser, term))
                            ) :> Node, tail
                    
                    | term::tail ->
                        BooleanNode(FilterExpression(parser, term)) :> Node, tail

                    | _ -> raise (SyntaxError ("invalid conditional expression in 'if' tag"))
                         

                let rec build_mult parser tokens =
                    let left, tail = build_term parser tokens
                    match tail with
                    | MatchToken("and")::tail ->
                        let right, tail = build_mult parser tail
                        AndNode(left, right) :> Node,
                        tail
                    | _ ->
                        left, tail

                let rec build_expression parser tokens =
                    let left, tail = build_mult parser tokens
                    match tail with
                    | MatchToken("or")::tail ->
                        let right, tail = build_expression parser tail
                        OrNode(left, right) :> Node,
                        tail
                    | _ ->
                        left, tail
                   
                let expression, _ =
                    try
                        build_expression context token.Args
                    with
                    | :? SyntaxError as e ->
                            raise (SyntaxError(e.Message, 
                                    node_list_true @ node_list_false,
                                    remaining2))
                    |_ -> reraise()

                ((
                    new TagNode(context, token, (this :> ITag), expression, node_list_true, node_list_false)
                    :> NDjango.Interfaces.INodeImpl),
                    context, remaining2)


