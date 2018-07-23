namespace Ensconce.NDjango

open System

module internal Cycle =
    /// Cycles among the given strings each time this tag is encountered.
    ///
    /// Within a loop, cycles among the given strings each time through
    /// the loop::
    ///
    ///     {% for o in some_list %}
    ///         <tr class="{% cycle 'row1' 'row2' %}">
    ///             ...
    ///         </tr>
    ///     {% endfor %}
    ///
    /// Outside of a loop, give the values a unique name the first time you call
    /// it, then use that name each sucessive time through::
    ///
    ///     <tr class="{% cycle 'row1' 'row2' 'row3' as rowcolors %}">...</tr>
    ///     <tr class="{% cycle rowcolors %}">...</tr>
    ///     <tr class="{% cycle rowcolors %}">...</tr>
    ///
    /// You can use any number of values, separated by spaces. Commas can also
    /// be used to separate values; if a comma is used, the cycle values are
    /// interpreted as literal strings.

    type CycleController(values : Variables.Variable list, origValues : Variables.Variable list) =

        member this.Values = values
        member this.OrigValues = origValues
        member this.Value = List.head values

    [<ParserNodes.Description("Cycles among the given strings each time this tag is encountered.")>]
    type TagNode(provider, token, tag, name: string, values: Variables.Variable list) =
        inherit ParserNodes.TagNode(provider, token, tag)

        let createController (controller: CycleController option) =
            match controller with
                | None -> new CycleController(values, values)
                | Some c ->
                    match List.tail c.Values with
                        | head::tail -> new CycleController(List.tail c.Values, c.OrigValues)
                        | [] -> new CycleController (c.OrigValues, c.OrigValues)

        override this.walk manager walker =
            let oldc =
                match walker.context.tryfind ("$cycle" + name) with
                | Some v -> Some (v :?> CycleController)
                | None ->
                    match values with
                    | [] -> raise (RenderingError(sprintf "Named cycle '%s' does not exist" name))
                    | _ -> None
            let newc =
                match oldc with
                    | None -> new CycleController(values, values)
                    | Some c ->
                        match List.tail c.Values with
                            | head::tail -> new CycleController(List.tail c.Values, c.OrigValues)
                            | [] -> new CycleController (c.OrigValues, c.OrigValues)

            let buffer = newc.Value.Resolve(walker.context) |> fst |> string
            {walker with
                buffer = buffer;
                context = (walker.context.add ("$cycle" + name, (newc :> obj))).add (name, (buffer :> obj))
                }

        override x.elements = base.elements @ (values |> List.map(fun v -> v :> INode))

    /// Note that the original django implementation returned the same instance of the
    /// CycleNode for each instance of a given named cycle tag. This implementation
    /// Relies on the CycleNode instances to communicate with each other through
    /// the context object available at render time to synchronize their activities
    type Tag() =

        let checkForOldSyntax (value:Lexer.TextToken) =
            if (String.IsNullOrEmpty value.RawText) then false
            else match value.RawText.[0] with
                    | '"' -> false
                    | '\'' -> false
                    | _ when value.RawText.Contains(",") -> true
                    | _ -> false


        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =

                let oldstyle_re
                    = new System.Text.RegularExpressions
                        .Regex("[^,]")

                let normalize (values: Lexer.TextToken list) =
                    if List.exists checkForOldSyntax values then
                        // Create a new token covering the text span from the beginning
                        // of the first parameter till the end of the last one
                        let start = values.Head.Location.Offset
                        let end_location = values.[values.Length-1].Location
                        let t1 = token.CreateToken(start - token.Location.Offset, end_location.Offset + end_location.Length - start)
                        t1.Tokenize oldstyle_re |>
                        List.map (fun t -> t.WithValue ("'" + t.Value + "'") (Some [1,false;t.Value.Length,true;1,false]))
                    else
                        values

                let name, values =
                    match List.rev token.Args with
                    | name::Lexer.MatchToken("as")::values1 ->
                        name.RawText, values1 |> List.rev |> normalize
                    | _ ->
                        match token.Args |> normalize with
                        | [] -> raise (SyntaxError ("'cycle' tag requires at least one argument"))
                        | name::[] -> name.RawText, []
                        | _ as values -> "$Anonymous$Cycle", values

                let values = List.map (fun v -> new Variables.Variable(context, v)) values
                ((new TagNode(context, token, (this :> ITag), name, values) :> INodeImpl), context, tokens)

