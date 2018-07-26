namespace Ensconce.NDjango.Core

module internal IfChanged =

/// Check if a value has changed from the last iteration of a loop.
///
/// The 'ifchanged' block tag is used within a loop. It has two possible uses.
///
/// Checks its own rendered contents against its previous state and only displays the content if it has changed. For example, this displays a list of days, only displaying the month if it changes:
///
/// <h1>Archive for {{ year }}</h1>
///
/// {% for date in days %}
///     {% ifchanged %}<h3>{{ date|date:"F" }}</h3>{% endifchanged %}
///     <a href="{{ date|date:"M/d"|lower }}/">{{ date|date:"j" }}</a>
/// {% endfor %}
/// If given a variable, check whether that variable has changed. For example, the following shows the date every time it changes, but only shows the hour if both the hour and the date has changed:
///
/// {% for date in days %}
///     {% ifchanged date.date %} {{ date.date }} {% endifchanged %}
///     {% ifchanged date.hour date.date %}
///         {{ date.hour }}
///     {% endifchanged %}
/// {% endfor %}

    [<ParserNodes.Description("Outputs the content of enclosed tags based on whether the value has changed.")>]
    type Tag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                let nodes_ifchanged, remaining = (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["else"; "endifchanged"]))
                let nodes_ifsame, remaining =
                    match nodes_ifchanged.[nodes_ifchanged.Length-1].Token with
                    | Lexer.Block b ->
                        if b.Verb.RawText = "else" then
                            (context.Provider :?> IParser).Parse (Some token) remaining (context.WithClosures(["endifchanged"]))
                        else
                            [], remaining
                    | _ -> [], remaining

                let variables =
                    token.Args |> List.map (fun var -> new Variables.Variable(context, var))

                let createWalker manager =
                    match variables with
                    | [] ->
                        fun walker ->
                            let reader =
                                new ASTWalker.Reader (manager, {walker with parent=None; nodes=nodes_ifchanged; context=walker.context})
                            let newValue = reader.ReadToEnd() :> obj
                            match walker.context.tryfind("$oldValue") with
                            | Some o when o = newValue -> {walker with nodes = List.append nodes_ifsame walker.nodes}
                            | _ -> {walker with buffer= string newValue; context=walker.context.add("$oldValue", newValue)}
                    | _ as vars ->
                        fun walker ->
                            let newValues = vars |> List.map (fun var -> (var.Resolve walker.context |> fst))

                            // this function returns true if there is a mismatch
                            let matchValues (oldVals:obj) newVals =
                                match oldVals with
                                | :? List<obj> as list when (list |> List.length) = (newVals |> List.length)
                                    -> List.exists2 (<>) list newVals
                                | _ -> true

                            match walker.context.tryfind("$oldValue") with
                            | Some o when not <| matchValues o newValues -> {walker with nodes = List.append nodes_ifsame walker.nodes}
                            | _ -> {walker with nodes = List.append nodes_ifchanged walker.nodes; context=walker.context.add("$oldValue", (newValues :> obj))}
                (({
                    new ParserNodes.TagNode(context, token, this)
                    with
                        override this.walk manager walker =
                            createWalker manager walker

                        override this.elements
                            with get() =
                                List.append (variables |> List.map (fun node -> (node :> INode))) base.elements

                        override this.Nodes
                            with get() =
                                base.Nodes
                                    |> Map.add (Constants.NODELIST_IFTAG_IFTRUE) (nodes_ifchanged |> Seq.map (fun node -> (node :?> INode)))
                                    |> Map.add (Constants.NODELIST_IFTAG_IFFALSE) (nodes_ifsame |> Seq.map (fun node -> (node :?> INode)))

                    } :> INodeImpl), context, remaining)
