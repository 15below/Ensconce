namespace Ensconce.NDjango

open System

module Abstract =
    /// Returns an absolute URL matching given view with its parameters.
    ///
    /// This is an adaptation of the standard django URL tag. Since this implementation
    /// isn't tied to a particular controller implementation, the structure of the parameters
    /// cannot be defined here, and is delegated to a consuming integration project.
    ///
    /// Supporting integration projects need to subclass the UrlTag class, and implement
    /// the 'string GenerateUrl(string, string[])' method to perform the necessary logic.
    /// This tag is not registered by default as the default implementation is abstract.
    [<AbstractClass>]
    type UrlTag() =
        let rec parseArgs token provider args =
            let instantiate arg = [new Expressions.FilterExpression(provider, arg)]
            match args with
            | arg::Lexer.MatchToken("as")::name::[] -> instantiate arg, (Some name)
            | arg::[] -> instantiate arg, None
            | arg::tail ->
                let list, var = parseArgs token provider tail
                instantiate arg @ list, var
            | _ -> [], None

        abstract member GenerateUrl: string * string array * IContext -> string

        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                let path, argList, var =
                    match token.Args with
                    | [] -> raise (SyntaxError ("'url' tag requires at least one parameter"))
                    | path::args ->
                        let argList, var =
                            parseArgs token context
                                (args |> List.rev |>
                                    List.fold
                                        (fun state elem ->
                                            match elem.RawText.Trim([|','|]) with
                                            | "" -> state
                                            | _ as trimmed -> (elem.WithValue trimmed None)::state )
                                        []
                                )
// TODO: ExpressionToken
                        new Expressions.FilterExpression(context, path), argList, var

                (({
                    new ParserNodes.TagNode(context, token, this) with
                        override x.walk manager walker =
                            let shortResolve (expr: Expressions.FilterExpression) =
                                match fst <| expr.Resolve walker.context false with
                                | Some v -> Convert.ToString(v)
                                | None -> System.String.Empty

                            let url = this.GenerateUrl((shortResolve path), List.toArray <| List.map (fun (elem: Expressions.FilterExpression) -> shortResolve elem) argList, walker.context)
                            match var with
                            | None -> { walker with buffer = url }
                            | Some v -> { walker with context = walker.context.add(v.Value, (url :> obj)) }
                            } :> INodeImpl),
                    context, tokens)
