namespace Ensconce.NDjango.Core

open System.Text.RegularExpressions

module internal Misc =

    /// Forces autoescape behavior for this block
    [<ParserNodes.Description("Forces autoescape behavior for enclosed tags")>]
    type AutoescapeTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                let nodes, remaining = (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["endautoescape"]))

                let fail (fail_token:Lexer.TextToken) =
                        raise (SyntaxError(
                                "invalid arguments for 'Autoescape' tag",
                                Some (Seq.ofList nodes),
                                Some [(new ParserNodes.KeywordNode(context, fail_token, ["on";"off"]) :> INode)],
                                Some remaining
                                ))

                let autoescape_flag =
                    match token.Args with
                    | Lexer.MatchToken("on")::[] -> true
                    | Lexer.MatchToken("off")::[] -> false
                    | arg::list -> fail arg
                    | _ -> fail <| token.CreateToken(token.Location.Length - 2, 0)

                (({
                    new ParserNodes.TagNode(context, token, this) with
                        override x.walk manager walker =
                            {walker with
                                parent=Some walker;
                                context = walker.context.WithAutoescape autoescape_flag;
                                nodes=nodes}
                        override x.nodelist with get() = nodes
                        override x.elements =
                            (new ParserNodes.KeywordNode(context, List.head token.Args, ["on";"off"]) :> INode)
                                ::base.elements
                   } :> INodeImpl),
                   context, remaining)

    /// Ignores everything between ``{% comment %}`` and ``{% endcomment %}``.
    [<ParserNodes.Description("Ignores everything between ``{% comment %}`` and ``{% endcomment %}``")>]
    type CommentTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                let context_node, close_tag, remaining = (context.Provider :?> IParser).Seek tokens context ["endcomment"]
                ((
                    {
                        new ParserNodes.TagNode(context, token, this) with
                            override x.nodelist = [context_node; close_tag]
                    }
                    :> INodeImpl), context, remaining)

    /// Outputs a whole load of debugging information, including the current
    /// context and imported modules.
    ///
    /// Sample usage::
    ///
    ///     <pre>
    ///         {% debug %}
    ///     </pre>
    [<ParserNodes.Description("Outputs debug information")>]
    type DebugTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                ({new ParserNodes.TagNode(context, token, this)
                    with
                        override x.walk manager walker =
                            // the actual debug output is built by the context ToString method
                            {walker with buffer = walker.context.ToString()}
                    } :> INodeImpl), context, tokens

    /// Outputs the first variable passed that is not False.
    ///
    /// Outputs nothing if all the passed variables are False.
    ///
    /// Sample usage::
    ///
    ///     {% firstof var1 var2 var3 %}
    ///
    /// This is equivalent to::
    ///
    ///     {% if var1 %}
    ///         {{ var1 }}
    ///     {% else %}{% if var2 %}
    ///         {{ var2 }}
    ///     {% else %}{% if var3 %}
    ///         {{ var3 }}
    ///     {% endif %}{% endif %}{% endif %}
    ///
    /// but obviously much cleaner!
    ///
    /// You can also use a literal string as a fallback value in case all
    /// passed variables are False::
    ///
    ///     {% firstof var1 var2 var3 "fallback value" %}
    [<ParserNodes.Description("Outputs the first variable passed that is not False")>]
    type FirstOfTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                match token.Args with
                    | [] -> raise (SyntaxError ("'firstof' tag requires at least one argument"))
                    | _ ->
                        let variables = token.Args |> List.map (fun (expression) -> new Expressions.FilterExpression(context, expression))
                        ({
                            new ParserNodes.TagNode(context, token, this)
                            with
                                override this.walk manager walker =
                                    match variables |> List.tryPick (fun var -> var.ResolveForOutput manager walker ) with
                                    | None -> walker
                                    | Some w -> w
                                override this.elements
                                    with get()=
                                        List.append (variables |> List.map (fun node -> (node :> INode))) base.elements
                        } :> INodeImpl), context, tokens

/// regroup¶
/// Regroup a list of alike objects by a common attribute.
///
/// This complex tag is best illustrated by use of an example: say that people is a list of people represented by dictionaries with first_name, last_name, and gender keys:
///
/// people = [
///     {'first_name': 'George', 'last_name': 'Bush', 'gender': 'Male'},
///     {'first_name': 'Bill', 'last_name': 'Clinton', 'gender': 'Male'},
///     {'first_name': 'Margaret', 'last_name': 'Thatcher', 'gender': 'Female'},
///     {'first_name': 'Condoleezza', 'last_name': 'Rice', 'gender': 'Female'},
///     {'first_name': 'Pat', 'last_name': 'Smith', 'gender': 'Unknown'},
/// ]
/// ...and you'd like to display a hierarchical list that is ordered by gender, like this:
///
///
/// Male:
/// George Bush
/// Bill Clinton
///
/// Female:
/// Margaret Thatcher
/// Condoleezza Rice
///
/// Unknown:
/// Pat Smith
/// You can use the {% regroup %} tag to group the list of people by gender. The following snippet of template code would accomplish this:
///
/// {% regroup people by gender as gender_list %}
///
/// <ul>
/// {% for gender in gender_list %}
///     <li>{{ gender.grouper }}
///     <ul>
///         {% for item in gender.list %}
///         <li>{{ item.first_name }} {{ item.last_name }}</li>
///         {% endfor %}
///     </ul>
///     </li>
/// {% endfor %}
/// </ul>
/// Let's walk through this example. {% regroup %} takes three arguments: the list you want to regroup, the attribute to group by, and the name of the resulting list. Here, we're regrouping the people list by the gender attribute and calling the result gender_list.
///
/// {% regroup %} produces a list (in this case, gender_list) of group objects. Each group object has two attributes:
///
/// grouper -- the item that was grouped by (e.g., the string "Male" or "Female").
/// list -- a list of all items in this group (e.g., a list of all people with gender='Male').
/// Note that {% regroup %} does not order its input! Our example relies on the fact that the people list was ordered by gender in the first place. If the people list did not order its members by gender, the regrouping would naively display more than one group for a single gender. For example, say the people list was set to this (note that the males are not grouped together):
///
/// people = [
///     {'first_name': 'Bill', 'last_name': 'Clinton', 'gender': 'Male'},
///     {'first_name': 'Pat', 'last_name': 'Smith', 'gender': 'Unknown'},
///     {'first_name': 'Margaret', 'last_name': 'Thatcher', 'gender': 'Female'},
///     {'first_name': 'George', 'last_name': 'Bush', 'gender': 'Male'},
///     {'first_name': 'Condoleezza', 'last_name': 'Rice', 'gender': 'Female'},
/// ]
/// With this input for people, the example {% regroup %} template code above would result in the following output:
///
///
/// Male:
/// Bill Clinton
///
/// Unknown:
/// Pat Smith
///
/// Female:
/// Margaret Thatcher
///
/// Male:
/// George Bush
///
/// Female:
/// Condoleezza Rice
/// The easiest solution to this gotcha is to make sure in your view code that the data is ordered according to how you want to display it.
///
/// Another solution is to sort the data in the template using the dictsort filter, if your data is in a list of dictionaries:
///
/// {% regroup people|dictsort:"gender" by gender as gender_list %}

    type Grouper =
        {
            grouper: obj
            list: obj list
        }
        member this.Append(o) = {this with list=this.list @ o}

    [<ParserNodes.Description("Regroups a list of alike objects by a common attribute.")>]
    type RegroupTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                match token.Args with
                | source::Lexer.MatchToken("by")::grouper::Lexer.MatchToken("as")::result::[] ->
                    let value = new Expressions.FilterExpression(context, source)
                    let regroup context =
                        match fst <| value.Resolve context false with
                        | Some o ->
                            match o with
                            | :? System.Collections.IEnumerable as loop ->
                                let groupers =
                                    loop |> Seq.cast |>
                                        Seq.fold
                                            // this function takes a tuple with the first element representing the grouper
                                            // currently under construction and the second the list of groupers built so far
                                            (fun (groupers:Grouper option*Grouper list) item ->
                                                match Variables.resolve_members item [grouper.RawText] with
                                                | Some value ->  // this is the current value to group by
                                                    match fst groupers with
                                                    | Some group -> // group is a group currently being built
                                                        if value = group.grouper then
                                                            (Some {group with list=group.list @ [item]}, snd groupers)
                                                        else
                                                            (Some {grouper=value; list=[item]}, snd groupers@[group])
                                                    | None -> (Some {grouper=value; list=[item]}, []) // No group - we are just starting
                                                | None -> groupers
                                                )

                                            (None, [])  // start expression for seq.fold
                                match fst groupers with
                                | None -> []
                                | Some grouper -> snd groupers @ [grouper]
                            | _ -> []
                        | None -> []
                    ({
                        new ParserNodes.TagNode(context, token, this)
                        with
                            override this.walk manager walker =
                                match regroup walker.context with
                                | [] -> walker
                                | _ as list -> {walker with context=walker.context.add(result.RawText, (list :> obj))}

                            override this.elements
                                with get() =
                                    (value :> INode) :: base.elements
                    } :> INodeImpl), context, tokens

                | _ -> raise (SyntaxError ("malformed 'regroup' tag"))

/// spaceless¶
/// Removes whitespace between HTML tags. This includes tab characters and newlines.
///
/// Example usage:
///
/// {% spaceless %}
///     <p>
///         <a href="foo/">Foo</a>
///     </p>
/// {% endspaceless %}
/// This example would return this HTML:
///
/// <p><a href="foo/">Foo</a></p>
/// Only space between tags is removed -- not space between tags and text. In this example, the space around Hello won't be stripped:
///
/// {% spaceless %}
///     <strong>
///         Hello
///     </strong>
/// {% endspaceless %}

    [<ParserNodes.Description("Removes whitespace characters between HTML tags.")>]
    type SpacelessTag() =
        let spaces_re = new Regex("(?'spaces'>\s+<)", RegexOptions.Compiled)
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                let nodes, remaining = (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["endspaceless"]))

                match token.Args with
                | [] ->
                    ({
                        new ParserNodes.TagNode(context, token, this)
                        with
                            override this.walk manager walker =
                                let reader =
                                    new ASTWalker.Reader(manager,{walker with parent=None; nodes=nodes; context=walker.context})
                                let buf = spaces_re.Replace(reader.ReadToEnd(), "><")
                                {walker with buffer = buf}
                            override this.nodelist with get() = nodes
                    } :> INodeImpl), context, remaining

                | _ -> raise (SyntaxError (
                                "'spaceless' tag should not have any arguments",
                                nodes,
                                remaining))

    ///Output one of the syntax characters used to compose template tags.
    ///
    ///Since the template system has no concept of "escaping", to display one of the bits used in template tags, you must use the {% templatetag %} tag.
    ///
    ///The argument tells which template bit to output:
    ///
    ///Argument Outputs
    ///openblock {%
    ///closeblock %}
    ///openvariable {{
    ///closevariable }}
    ///openbrace {
    ///closebrace }
    ///opencomment {#
    ///closecomment #}

    [<ParserNodes.Description("Outputs one of the syntax characters used to compose template tags.")>]
    type TemplateTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                let buf =
                    match token.Args with
                        | Lexer.MatchToken("openblock")::[] -> "{%"
                        | Lexer.MatchToken("closeblock")::[] -> "%}"
                        | Lexer.MatchToken("openvariable")::[] -> "{{"
                        | Lexer.MatchToken("closevariable")::[] -> "}}"
                        | Lexer.MatchToken("openbrace")::[] -> "{"
                        | Lexer.MatchToken("closebrace")::[] -> "}"
                        | Lexer.MatchToken("opencomment")::[] -> "{#"
                        | Lexer.MatchToken("closecomment")::[] -> "#}"
                        | _ -> raise (SyntaxError ("invalid format for 'template' tag"))
                //???let variables = token.Args |> List.map (fun (name) -> new FilterExpression(context, name))
                ({
                    new ParserNodes.TagNode(context, token, this)
                    with
                        override this.walk manager walker =
                            {walker with buffer = buf}
                } :> INodeImpl), context, tokens

    /// For creating bar charts and such, this tag calculates the ratio of a given
    /// value to a maximum value, and then applies that ratio to a constant.
    ///
    /// For example::
    ///
    ///     <img src='bar.gif' height='10' width='{% widthratio this_value max_value 100 %}' />
    ///
    /// Above, if ``this_value`` is 175 and ``max_value`` is 200, the the image in
    /// the above example will be 88 pixels wide (because 1.6.300 = .875;
    ///
    [<ParserNodes.Description("Calculates the ratio of a given value to a max value, and applies it to a constant.")>]
    type WidthRatioTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =

                let toFloat (v:obj option) =
                    match v with
                    | None -> raise (SyntaxError ("'widthratio' cannot convert empty value to a numeric"))
                    | Some value ->
                        try
                            System.Convert.ToDouble(value)
                        with |_ -> raise (SyntaxError (sprintf "'widthratio' cannot convert value %s to a numeric" (System.Convert.ToString(value))))

                match token.Args with
                | value::maxValue::maxWidth::[] ->
                    let value = new Expressions.FilterExpression(context, value)
                    let maxValue = new Expressions.FilterExpression(context, maxValue)
                    let width = try System.Int32.Parse(maxWidth.RawText) |> float with | _  -> raise (SyntaxError ("'widthratio' 3rd argument must be integer"))
                    (({
                        new ParserNodes.TagNode(context, token, this) with
                            override this.walk manager walker =
                                let ratio = toFloat (fst <| value.Resolve walker.context false)
                                            / toFloat (fst <| maxValue.Resolve walker.context false)
                                            * width + 0.5
                                {walker with buffer = ratio |> int |> string}

                            override this.elements
                                with get()=
                                    (value :> INode) :: (maxValue :> INode) :: base.elements
                       } :> INodeImpl),
                       context, tokens)
                | _ -> raise (SyntaxError ("'widthratio' takes three arguments"))

    /// Adds a value to the context (inside of this block) for caching and easy
    /// access.
    ///
    /// For example::
    ///
    ///     {% with person.some_sql_method as total %}
    ///         {{ total }} object{{ total|pluralize }}
    ///     {% endwith %}
    [<ParserNodes.Description("Adds a value to the context for the enclosed tags.")>]
    type WithTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                let expression, name =
                    match token.Args with
                    | var::Lexer.MatchToken("as")::name::[] ->
                        Some (Expressions.FilterExpression(context, var)), name.RawText
                    | _ -> None, ""
                let extra_vars =
                    match expression with
                    | Some expression -> [Expressions.ExpressionType(expression, name):>TypeResolver.IDjangoType]
                    | None -> []
                let nodes, remaining =
                    (context.Provider :?> IParser).Parse
                        (Some token)
                        tokens
                        (context
                            .WithClosures(["endwith"])
                            .WithNewModel(context.Model.Add(extra_vars))
                        )
                match expression with
                | Some expression ->
                    (({
                        new ParserNodes.TagNode(context, token, this) with
                            override this.walk manager walker =
                                let context =
                                    match fst <| expression.Resolve walker.context false with
                                    | Some v -> walker.context.add(name, v)
                                    | None -> walker.context
                                {walker with
                                    parent=Some walker;
                                    context = context;
                                    nodes=nodes}
                            override this.nodelist = nodes
                            override x.elements = (expression :> INode)::base.elements
                       } :> INodeImpl),
                       context, remaining)
                | _ -> raise (SyntaxError (
                                "'with' expected format is 'value as name'",
                                nodes,
                                remaining))
