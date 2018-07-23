namespace Ensconce.NDjango

open Lexer
open Ensconce.NDjango.Interfaces
open Expressions
open ParserNodes

module internal ASTNodes =

    /// retrieves a template given the template name. The name is supplied as a FilterExpression
    /// which when resolved should eithter get a ready to use template, or a string (url)
    /// to the source code for the template
    let get_template (manager:ITemplateManager) resolver model (templateRef:FilterExpression) context =
        match fst (templateRef.Resolve context false) with  // ignoreFailures is false because we have to have a name.
        | Some o ->
            match o with
            | :? ITemplate as template -> template
            | :? string as name -> manager.GetTemplate(name, resolver, model)
            | _ -> raise (RenderingError (sprintf "Invalid template name in 'extends' tag. Can't construct template from %A" o))
        | _ -> raise (RenderingError (sprintf "Invalid template name in 'extends' tag. Variable %A is undefined" templateRef))

    type TemplateNameExpression(context:IParsingContext, expression: TextToken) =
        inherit FilterExpression (context, expression)

        member x.GetParentNodes =
            try
                (context.Provider.GetTemplate(expression.RawText, context.Resolver, context.Model) |> snd).Nodes
            with
            |_ -> []

        interface INode with
            member x.NodeType = NodeType.TemplateName

        interface ICompletionProvider

    type private BlockReference =
    | Block of string
    | Context of IParsingContext

    type SuperBlockPointer = {super:TagNode}

    //SuperBlock that can be inserted into context to use it while rendering {{block.super}} variable
    and SuperBlock (context, token, tag, parents: BlockNode list) =
        inherit TagNode(context, token, tag)

        let nodes, parent =
            match parents with
            | h::[] -> h.nodelist, None
            | h::t -> h.nodelist, Some <| new SuperBlock(context, token, tag, t)
            | _ -> [], None

        override this.walk manager walker =
            {walker with
                parent=Some walker;
                nodes= nodes;
                context =
                    match parent with
                        //We have to update the context, because later,
                        //while rendering another {{block.super}} variable,
                        //we should use the parent SuperBlock - not the same all the time.
                        //Avoiding this replacement may cause endless looping in case when
                        //you have chain of block.super variables (see 'extends 05-CHAIN' unit test).
                    | Some p -> walker.context.add("block", ({super = p} :> obj))
                    | None -> walker.context.remove("block")
            }

        override this.nodelist with get() = nodes


    //During parsing the templates, we build(see ExtendsNode) dictionary "__blockmap" consisting
    //of different blocks. For each block name we have a list of blocks,
    //where the most child block is in the head and the most parental - in the tail of the list.
    //During the rendering, we will use the head of each list and give the head's nodes(=final_nodelist)
    //to the walker in order to implement the blocks overriding.
    //Moreover, we will use the rest (=parents) of the list for {{super.bock}} issues.
    //This rest of the list will be added to context with a "block" key.
    and BlockNode(parsing_context, token, tag, name: TextToken, nodelist: INodeImpl list) =
        inherit TagNode(parsing_context, token, tag)

        //get the head's nodes to give them later to the walker
        //the rest of the list will be given to the context for {{super.block}} issues
        member x.MapNodes blocks =
            match Map.tryFind x.Name blocks with
            | Some (children: BlockNode list) ->
                match children with
                | active::parents ->
                    //always append current block to 'parents'. We need this block in case
                    //when {{block.super}} refers to a simple block, not another {{block.super}}
                    active.nodelist, List.append parents [x]
                | [] -> x.nodelist, []
            | None -> x.nodelist, []

        member x.Name = name.RawText

        override x.elements = BlockNameNode(parsing_context, Text name) :> INode :: base.elements

        override x.walk manager walker =

        //get the final_nodelist and parents from the "__blockmap" dictionary using MapNodes function
            let final_nodelist, parents =
                match walker.context.tryfind "__blockmap" with
                | None -> x.nodelist, []
                | Some ext ->
                    x.MapNodes (ext :?> Map<string, BlockNode list>)

            {walker with
                parent=Some walker;
                nodes=final_nodelist;
                context=
                    if  not (List.isEmpty parents) then
                        //add SuperBlockPointer to the context. Later, when we will render {{block.super}} variable,
                        //we will get inside this inserted SuperBlock and 'walk' it.
                        walker.context.add("block", ({super = new SuperBlock(parsing_context, token, tag, parents)} :> obj))
                    else
                        walker.context
            }

        override x.nodelist = nodelist

    /// a node representing a block name. carries a list of valid block names
    and BlockNameNode (context, token) =
        inherit ValueListNode
            (
                context,
                NodeType.BlockName,
                token,
                []
            )


        /// produces a flattened list of all nodes and child nodes within a 'node list'.
        /// the 'node list' is a list of all nodes collected from Nodes property of the INode interface
        let rec unfold_nodes (nodes: INode seq) =
            nodes |>
            Seq.choose
                (fun node ->
                    match node with
                    | :? Node | :? ParsingContextNode ->
                        Some (Seq.append [node] (Seq.concat node.Nodes.Values |> unfold_nodes))
                    | _ -> None)
            |> Seq.concat

        interface ICompletionValuesProvider with
            override x.Values =
                let rec blocks_of_context (context:IParsingContext) =
                    match context.Base with
                    | Some _base ->
                        let block_refs =
                            match _base with
                            | :? TemplateNameExpression as _base ->
                                _base.GetParentNodes
                                |> Seq.map (fun i -> i :?> INode)
                                |> unfold_nodes
                                |> Seq.choose
                                    (function
                                        | :? BlockNode as block -> Some (Block block.Name)
                                        | :? ParsingContextNode as context_node -> Some (Context (context_node :> INode).Context)
                                        | _ as node -> None
                                    )
                            | _ -> Seq.empty

                        let result =
                            block_refs
                            |> Seq.fold
                                (fun state block_ref ->
                                    match block_ref with
                                    | Block block_name -> (block_name :: fst state, snd state)
                                    | Context context -> (fst state, Some context)
                                )
                                ([], None)

                        match snd result with
                        | Some context -> fst result |> Seq.append (context |> blocks_of_context) |> Seq.distinct
                        | None -> fst result |> Seq.ofList

                    | None -> Seq.empty

                blocks_of_context context

    and ExtendsNode(parsing_context, token, tag, nodes: INode list, blocks: Map<string, BlockNode list>, parent: Expressions.FilterExpression) =
        inherit TagNode(parsing_context, token, tag)

        let add_if_missing key value map =
            match Map.tryFind key map with
            | Some v -> Map.add key (map.[key] @ value) map
            | None -> Map.add key value map

        let rec join_replace primary (secondary: ('a*'b list) list) =
            match secondary with
            | h::t ->
                let key,value = h
                join_replace primary t |>
                add_if_missing key value
            | [] -> primary

        override x.elements = (parent :> INode) :: base.elements
        override x.Nodes =
            base.Nodes
                |> Map.add (Constants.NODELIST_EXTENDS_BLOCKS) (Seq.ofList nodes)

        override this.walk manager walker =
            let context =
                match walker.context.tryfind "__blockmap" with
                | Some v -> walker.context.add ("__blockmap", (join_replace (v:?> Map<_,_>) (Map.toList blocks) :> obj))
                | None -> walker.context.add ("__blockmap", (blocks :> obj))

            {walker with nodes=(get_template manager parsing_context.Resolver parsing_context.Model parent context).Nodes; context = context}