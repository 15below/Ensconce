namespace Ensconce.NDjango.Core

open System
open System.Collections

module internal For =

///    for
///    Loop over each item in an array. For example, to display a list of athletes provided in athlete_list:
///
///    <ul>
///    {% for athlete in athlete_list %}
///        <li>{{ athlete.name }}</li>
///    {% endfor %}
///    </ul>
///    You can loop over a list in reverse by using {% for obj in list reversed %}.
///
///    New in Django 1.0.
///    If you need to loop over a list of lists, you can unpack the values in each sub-list into individual variables. For example, if your context contains a list of (x,y) coordinates called points, you could use the following to output the list of points:
///
///    {% for x, y in points %}
///        There is a point at {{ x }},{{ y }}
///    {% endfor %}
///    This can also be useful if you need to access the items in a dictionary. For example, if your context contained a dictionary data, the following would display the keys and values of the dictionary:
///
///    {% for key, value in data.items %}
///        {{ key }}: {{ value }}
///    {% endfor %}
///    The for loop sets a number of variables available within the loop:
///
///    Variable Description
///    forloop.counter The current iteration of the loop (1-indexed)
///    forloop.counter0 The current iteration of the loop (0-indexed)
///    forloop.revcounter The number of iterations from the end of the loop (1-indexed)
///    forloop.revcounter0 The number of iterations from the end of the loop (0-indexed)
///    forloop.first True if this is the first time through the loop
///    forloop.last True if this is the last time through the loop
///    forloop.parentloop For nested loops, this is the loop "above" the current one
///
///    for ... empty¶
///    New in Django 1.1.
///    The for tag can take an optional {% empty %} clause that will be displayed if the given array is empty or could not be found:
///
///    <ul>
///    {% for athlete in athlete_list %}
///        <li>{{ athlete.name }}</li>
///    {% empty %}
///        <li>Sorry, no athlete in this list!</li>
///    {% endfor %}
///    <ul>
///    The above is equivalent to -- but shorter, cleaner, and possibly faster than -- the following:
///
///    <ul>
///      {% if athlete_list %}
///        {% for athlete in athlete_list %}
///          <li>{{ athlete.name }}</li>
///        {% endfor %}
///      {% else %}
///        <li>Sorry, no athletes in this list.</li>
///      {% endif %}
///    </ul>

    type ForContext =
        {
        counter:     int
        counter0:    int
        revcounter:  int
        revcounter0: int
        first:       bool
        last:        bool
        parentloop:  obj
        }

    type private ParentContextDescriptor(orig: TypeResolver.IDjangoType) =
        interface TypeResolver.IDjangoType with
            member x.Name = "parent"
            member x.Type = TypeResolver.DjangoType
            member x.Members = orig.Members
            member x.IsList = false
            member x.IsDictionary = false

    and private ForContextDescriptor(context) =
        interface TypeResolver.IDjangoType with
            member x.Name = "forloop"
            member x.Type = TypeResolver.DjangoType
            member x.IsList = false
            member x.IsDictionary = false
            member x.Members =

                let rec parent_lookup (context:IParsingContext) =
                    match context.Model.Members |> Seq.tryFind (fun var -> var.Type = TypeResolver.DjangoType && var.Name = "forloop") with
                    | Some forloop -> Some forloop
                    | None ->
                        match context.Parent with
                        | Some context -> parent_lookup context
                        | None -> None

                let parent =
                    match parent_lookup context with
                    | Some parent -> Seq.ofList [ParentContextDescriptor(parent) :> TypeResolver.IDjangoType]
                    | None -> Seq.empty

                seq [
                    TypeResolver.ValueDjangoType("counter") :> TypeResolver.IDjangoType;
                    TypeResolver.ValueDjangoType("counter0") :> TypeResolver.IDjangoType;
                    TypeResolver.ValueDjangoType("revcounter") :> TypeResolver.IDjangoType;
                    TypeResolver.ValueDjangoType("revcounter0") :> TypeResolver.IDjangoType;
                    TypeResolver.ValueDjangoType("first") :> TypeResolver.IDjangoType;
                    TypeResolver.ValueDjangoType("last") :> TypeResolver.IDjangoType;
                    ] |> Seq.append parent

    type TagNode(
                provider,
                token,
                tag,
                enumerator : Expressions.FilterExpression,
                variables : string list,
                bodyNodes : INodeImpl list,
                emptyNodes: INodeImpl list,
                reversed: bool
                ) =
        inherit ParserNodes.TagNode(provider, token, tag)

        /// Creates a new ForContext object to represent the current iteration
        /// for the loop. The third parameter (context) is a context for the
        /// parent loop if this is the first iteration of the loop
        /// or the conetxt from the previous iteration
        let createVars (first:bool) (size:int option) (context:obj) =
            let context =
                match context with
                | null -> null
                | _ as o ->
                    match o with
                    | :? ForContext as c -> c :> obj
                    | _ -> null

            match first with
            | true ->
                {
                    counter = 1;
                    counter0 = 0;
                    revcounter =
                        match size with
                        | Some s -> s
                        | None -> 0
                    revcounter0 =
                        -1 +
                            match size with
                            | Some s -> s
                            | None -> 0
                    first = true;
                    last = false;
                    parentloop = context
                }
            | false ->
                match context with
                | :? ForContext as context ->
                    {
                        counter = context.counter+1;
                        counter0 = context.counter0+1;
                        revcounter = context.revcounter-1 ;
                        revcounter0 = context.revcounter0-1;
                        first = false;
                        last =
                            match size with
                            | Some s -> context.counter = s-1;
                            | None -> false
                        parentloop = context.parentloop
                    }
                | _ -> raise (new RenderingError("Context should always be available after the first iteration"))

        override this.walk manager walker =

            match fst <| enumerator.Resolve walker.context false with
            |Some o ->
                match o with
                | :? IEnumerable as loop when not (loop |> Seq.cast |> Seq.isEmpty) ->

                    let size = Some (Seq.length(loop |> Seq.cast))

                    /// create context with the loop variables and the forloop
                    /// record reflecting the current loop interation
                    let createContext (first:bool) (walker:Walker) (item:obj) =
                        // add the loop variable(s) to the context
                        let c =
                            if List.length variables = 1 then
                                walker.context.add(variables.[0], item)
                            else
                                match item with
                                | :? IEnumerable as list ->
                                    Seq.zip (Seq.cast list) variables |> Seq.fold
                                        (fun context var -> context.add(snd var, fst var))
                                        walker.context
                                | _ ->
                                    variables |> List.fold
                                        (fun context var ->
                                            let value = match Variables.resolve_members item [var] with | Some o -> o | None -> None :> obj
                                            context.add(var, value))
                                        walker.context

                        c.add("forloop",
                            ( (match walker.context.tryfind "forloop" with
                                  | Some forContext -> forContext
                                  | None -> null
                                  )
                                |> createVars first size :> obj))

                    let rec createWalker (first:bool) (walker:Walker) (enumerator: obj seq) =
                        {walker with
                            parent = Some walker;
                            nodes = bodyNodes @ [(Repeater(provider, token, tag, Seq.skip 1 enumerator, createWalker false) :> INodeImpl)];
                            context = enumerator |> Seq.head |> createContext first walker
                            }

                    if reversed then
                        loop |> Seq.cast |> Seq.toList |> List.rev |> createWalker true walker
                    else
                        loop |> Seq.cast |> createWalker true walker

                | _ -> {walker with parent=Some walker; nodes=emptyNodes}
            | None -> {walker with parent=Some walker; nodes=emptyNodes}

        override this.Nodes =
                base.Nodes
                    |> Map.add (Constants.NODELIST_FOR_BODY) (bodyNodes |> Seq.map (fun node -> (node :?> INode)))
                    |> Map.add (Constants.NODELIST_FOR_EMPTY) (emptyNodes |> Seq.map (fun node -> (node :?> INode)))

    /// this is a for loop helper node. The real loop node <see cref="TagNode"/> places a list of nodes
    /// for the loop body into the walker, it adds the Repeater as the last one. The repeater checks for
    /// the endloop condition and if another iteration is necessary re-adds the list of nodes and itself to
    /// the walker
    and Repeater(provider, token, tag, enumerator, createWalker) =
        inherit ParserNodes.TagNode(provider, token, tag)

        override this.walk manager walker =
            if Seq.isEmpty enumerator then
                walker
            else
                createWalker walker enumerator

    [<ParserNodes.Description("Loops over each item in a collection.")>]
    type Tag() =

        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                let enumerator, variables, reversed =
                    match List.rev token.Args with
                        | var::Lexer.MatchToken("in")::syntax ->
                            Some var,
                            syntax,
                            false
                        | Lexer.MatchToken("reversed")::var::Lexer.MatchToken("in")::syntax ->
                            Some var,
                            syntax,
                            true
                        | _ ->
                            None,
                            [],
                            false

                let enumerator =
                    match enumerator with
                    | Some e -> Some (Expressions.FilterExpression(context, e))
                    | None -> None

                // extract variable names from tokens, (including splitting apart names stuck together in a single token separated by ',')
                let variables =
                    variables |>
                    List.rev |>
                    List.fold
                        (fun external_accumulator token -> // for every token
                            external_accumulator @
                                (token.RawText.Split([|','|], StringSplitOptions.RemoveEmptyEntries) |>
                                    Array.toList |>
                                    List.fold
                                        (fun internal_accumulator variable -> // for every variable name
                                            match external_accumulator |> List.tryFind (fun item -> fst item = variable),
                                                  internal_accumulator |> List.tryFind (fun item -> fst item = variable),
                                                  variable = "forloop" with
                                            | _, _, true ->
                                                internal_accumulator @
                                                [
                                                    (variable,
                                                           Some ({new ParserNodes.ErrorNode(context,Lexer.Text token,
                                                                    new Error(1, "Loop variable named 'forloop' will not be availble in the for loop body"))
                                                                  with override x.elements = []
                                                                  } :> INode)
                                                    )
                                                    ]
                                            | None, None, false ->
                                                internal_accumulator @ [(variable, None)]
                                            | _, _, _ ->
                                                internal_accumulator @
                                                [
                                                    (variable,
                                                           Some ({new ParserNodes.ErrorNode(context,Lexer.Text token,
                                                                    new Error(1, "Duplicate name '" + variable + "' in the variable list"))
                                                                  with override x.elements = []
                                                                  } :> INode)
                                                    )
                                                    ]
                                            )
                                        []
                                )
                            )
                        []

                let variable_descriptors =
                    variables |>
                        List.filter (fun var -> (snd var).IsNone) |> List.map (fun var -> TypeResolver.ValueDjangoType(fst var) :> TypeResolver.IDjangoType)
                let variable_descriptors = (ForContextDescriptor(context) :> TypeResolver.IDjangoType) :: variable_descriptors

                let node_list_body, remaining =
                    (context.Provider :?> IParser).Parse (Some token) tokens
                        (context.WithClosures(["empty"; "endfor"]).WithNewModel(context.Model.Add(variable_descriptors)))
                let node_list_empty, remaining2 =
                    match node_list_body.[node_list_body.Length-1].Token with
                    | Lexer.Block b ->
                        if b.Verb.RawText = "empty" then
                            (context.Provider :?> IParser).Parse (Some token) remaining
                                (context.WithClosures(["endfor"]).WithNewModel(context.Model.Add(variable_descriptors)))
                        else
                            [], remaining
                    | _ -> [], remaining

                match enumerator with
                | Some enumerator
                    ->
                         let dups = variables |> List.filter (fun var -> (snd var).IsSome) |> List.map (fun var -> (snd var).Value)
                         let variables = variables |> List.map (fun var -> fst var)
                         (({
                            new TagNode(context, token, (this :> ITag), enumerator, variables, node_list_body, node_list_empty, reversed)
                                with
                                    override x.elements =
                                        (enumerator :> INode) :: dups @ base.elements
                            } :> INodeImpl),
                            context, remaining2)
                | None ->
                    raise (SyntaxError ("malformed 'for' tag",
                                            List.append node_list_body node_list_empty,
                                            remaining2))



