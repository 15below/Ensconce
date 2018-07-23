namespace Ensconce.NDjango

module internal Filter =

    let FILTER_VARIABLE_NAME = "$filter"

    type FilterNode(provider, token, tag, filter: Expressions.FilterExpression, node_list) =
        inherit ParserNodes.TagNode(provider, token, tag)

        override this.walk manager walker =
            let reader =
                new ASTWalker.Reader (manager, {walker with parent=None; nodes=node_list; context=walker.context})

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
    [<ParserNodes.Description("Filters the contents of the block through variable filters.")>]
    type FilterTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform (token:Lexer.BlockToken) (context:IParsingContext) (tokens:LazyList<Lexer.Token>) =
                let node_list, remaining = (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["endfilter"]))
                match token.Args with
                | filter::[] ->
// TODO: ExpressionToken
                    let prefix = FILTER_VARIABLE_NAME + "|"
                    let map = Some [prefix.Length, false; filter.Value.Length, true]
                    let filter_expr = new Expressions.FilterExpression(context, filter.WithValue(prefix + filter.Value) map)
                    (new FilterNode(context, token, (this :> ITag), filter_expr, node_list) :> INodeImpl), context, remaining
                | _ -> raise (SyntaxError (
                                "'filter' tag requires one argument",
                                node_list,
                                remaining))


