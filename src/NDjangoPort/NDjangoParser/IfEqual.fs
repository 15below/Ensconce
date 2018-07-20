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

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.ParserNodes
open NDjango.Expressions

module internal IfEqual =

    ///    Outputs the contents of the block if the two arguments equal each other.
    ///
    ///    Examples::
    ///
    ///        {% ifequal user.id comment.user_id %}
    ///            ...
    ///        {% endifequal %}
    ///
    ///        {% ifnotequal user.id comment.user_id %}
    ///            ...
    ///        {% else %}
    ///            ...
    ///        {% endifnotequal %}

    [<Description("Outputs the content of enclosed tags based on whether the values are equal.")>]
    type Tag(not:bool) =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                let tag = token.Verb
                let node_list_true, remaining = (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["else"; "end" + tag.RawText]))
                let node_list_false, remaining =
                    match node_list_true.[node_list_true.Length-1].Token with
                    | NDjango.Lexer.Block b -> 
                        if b.Verb.RawText = "else" then
                            (context.Provider :?> IParser).Parse (Some token) remaining (context.WithClosures(["end" + tag.RawText]))
                        else
                            [], remaining
                    | _ -> [], remaining

                let getNodeList v1 v2 = 
                    match v1,v2 with
                    | None,_ | _, None -> node_list_false
                    | Some value1, Some value2 -> 
                        if not = value1.Equals value2 
                            then node_list_true
                            else node_list_false

                match token.Args with
                | var1::var2::[] ->
                    let var1 = new FilterExpression(context, var1)
                    let var2 = new FilterExpression(context, var2)
                    ({
                        new TagNode(context, token, this)
                        with 
                            override this.walk manager walker =
                                {
                                    walker 
                                    with 
                                        parent=Some walker;
                                        nodes=getNodeList (fst (var1.Resolve walker.context true)) (fst (var2.Resolve walker.context true))
                                }
                                
                            override this.elements 
                                with get() = 
                                    (var1 :> INode) :: (var2 :> INode) :: base.elements
                            
                            override this.Nodes 
                                with get() =
                                    base.Nodes 
                                        |> Map.add (NDjango.Constants.NODELIST_IFTAG_IFTRUE) (node_list_true |> Seq.map (fun node -> (node :?> INode)))
                                        |> Map.add (NDjango.Constants.NODELIST_IFTAG_IFFALSE) (node_list_false |> Seq.map (fun node -> (node :?> INode)))

                    } :> INodeImpl), context, remaining
                | _ -> raise (SyntaxError (
                                sprintf "'%s' takes two arguments" tag.RawText,
                                node_list_true @ node_list_false,
                                remaining))

                