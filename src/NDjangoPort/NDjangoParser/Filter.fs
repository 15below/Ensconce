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

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.ParserNodes
open NDjango.ASTNodes
open NDjango.Expressions
open Microsoft.FSharp.Collections

module internal Filter =

    let FILTER_VARIABLE_NAME = "$filter"

    type FilterNode(provider, token, tag, filter: FilterExpression, node_list) =
        inherit TagNode(provider, token, tag)

        override this.walk manager walker =
            let reader =
                new NDjango.ASTWalker.Reader (manager, {walker with parent=None; nodes=node_list; context=walker.context})

            match filter.ResolveForOutput manager
                     {walker with context=walker.context.add(FILTER_VARIABLE_NAME, (reader.ReadToEnd():>obj))}
                with
            | Some w -> w
            | None -> walker

        override x.nodelist = node_list

        override x.elements = (filter :> INode) :: base.elements

    /// Filters the contents of the block through variable filters.
    ///
    /// Filters can also be piped through each other, and they can have
    /// arguments -- just like in variable syntax.
    ///
    /// Sample usage::
    ///
    ///     {% filter force_escape|lower %}
    ///         This text will be HTML-escaped, and will appear in lowercase.
    ///     {% endfilter %}
    [<Description("Filters the contents of the block through variable filters.")>]
    type FilterTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform (token:BlockToken) (context:IParsingContext) (tokens:LazyList<Token>) =
                let node_list, remaining = (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["endfilter"]))
                match token.Args with
                | filter::[] ->
// TODO: ExpressionToken
                    let prefix = FILTER_VARIABLE_NAME + "|"
                    let map = Some [prefix.Length, false; filter.Value.Length, true]
                    let filter_expr = new FilterExpression(context, filter.WithValue(prefix + filter.Value) map)
                    (new FilterNode(context, token, (this :> ITag), filter_expr, node_list) :> INodeImpl), context, remaining
                | _ -> raise (SyntaxError (
                                "'filter' tag requires one argument",
                                node_list,
                                remaining))


