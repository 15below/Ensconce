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

namespace NDjango.Compatibility

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.Expressions
open NDjango.ParserNodes

///<summary>
/// Abstract tag implementation designed for consumption outside of F#. 
///</summary>
///<remarks>
/// This class will handle interaction with the expression system and the parser, providing
/// the abstract 'ProcessTag' method with the execution-time values of the supplied
/// parameters, along with the fully resolved text of the nested value (if in nested mode).
/// Concrete implementations are required to define a 0-parameter constructor, and
/// pass in relevant values for 'nested' and 'name'. The value of 'name' must match
/// the name the tag is registered under, but is only applicable when 'nested' is true.
///</remarks>
[<AbstractClass>]
type public SimpleTag(nested, name, num_params) =

    /// Resolves all expressions in the list against the context
    let resolve_all list (context: IContext) =
        list |>
        List.map (fun (e: FilterExpression) -> 
                    match fst <| e.Resolve context true with
                    | None -> null
                    | Some v -> v) |>
        List.toArray

    /// Evaluates the contents of the nodelist against the given walker. This function
    /// effectively renders the nested tags within the simple tag.
    let read_walker manager walker nodelist =
        let reader = 
            new NDjango.ASTWalker.Reader (manager, {walker with parent=None; nodes=nodelist; context=walker.context})
        reader.ReadToEnd()

    new(nested, num_params) =
        SimpleTag(nested, null, num_params) 

    /// Tag implementation. This method will receive the fully-evaluated nested contents for nested tag
    /// along with fully resolved values of the parameters supplied to the tag. Parameters in the template
    /// source may follow standard parameter conventions, e.g. they can be variables or literals, with 
    /// filters.
    abstract member ProcessTag: context:IContext -> content:string -> parms:obj array -> string 
    
    member x.GetFromContext (context:IContext) (key:string) =
        match context.tryfind key with
        | None -> null
        | Some v -> v
    
    interface ITag with
        member x.is_header_tag = false
        member x.Perform token context tokens = 
            let parms = 
                token.Args |>
                List.map (fun elem -> new FilterExpression(context, elem))
            
            if not (parms.Length = num_params || num_params = -1) then
                raise (SyntaxError(sprintf "%s expects %d parameters, but was given %d." name num_params (parms.Length)))
            else
                let nodelist, tokens =
                    if nested then
                        if name = null then
                            let attrs = x.GetType().GetCustomAttributes(typeof<NameAttribute>, false) 
                            if attrs.Length = 0 then raise (SyntaxError("No name provided for the tag - are you missing the Name attribute?" ))
                            let name = (attrs.[0] :?> NameAttribute).Name
                            (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["end" + name]))
                        else
                            (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["end" + name]))
                    else [], tokens
                    
                ({new TagNode(context, token, x)
                    with
                        override this.walk manager walker =
                            let resolved_parms =  resolve_all parms walker.context
                            if not nested then
                                {walker with buffer = (x.ProcessTag walker.context "" resolved_parms)}
                            else
                                {walker with buffer = (x.ProcessTag walker.context (read_walker manager walker nodelist) resolved_parms)}
                                
                        override this.nodelist = nodelist

                } :> INodeImpl), context, tokens
