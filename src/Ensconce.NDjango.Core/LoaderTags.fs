namespace Ensconce.NDjango.Core

open System.IO

module internal LoaderTags =

    type private SuperVarDescriptor() =
        interface TypeResolver.IDjangoType with
            member x.Name = "super"
            member x.Type = TypeResolver.DjangoType
            member x.Members = Seq.ofList [x]
            member x.IsList = false
            member x.IsDictionary = false

    type private BlockVarDescriptor() =
        interface TypeResolver.IDjangoType with
            member x.Name = "block"
            member x.Type = TypeResolver.DjangoType
            member x.Members = Seq.ofList [SuperVarDescriptor()]
            member x.IsList = false
            member x.IsDictionary = false

    /// Define a block that can be overridden by child templates.
    [<ParserNodes.Description("Defines a block that can be overridden by child templates.")>]
    type BlockTag() =
        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                match token.Args with
                | name::[] ->
                    let node_list, remaining =
                        (context.Provider :?> IParser).Parse (Some token) tokens
                            (context.WithClosures(["endblock"; "endblock " + name.RawText]).WithNewModel(context.Model.Add([BlockVarDescriptor()])))
                    (new ASTNodes.BlockNode(context, token, this, name, node_list) :> INodeImpl), context, remaining
                | _ ->
                    let node_list, remaining = (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["endblock"]))
                    raise (SyntaxError("block tag requires exactly one argument",
                            (node_list |> Seq.ofList),
                            [ASTNodes.BlockNameNode(context, Lexer.Text (token.CreateToken(token.Location.Length - 2, 0))) :> INode],
                            remaining))

    /// Signal that this template extends a parent template.
    ///
    /// This tag may be used in two ways: ``{% extends "base" %}`` (with quotes)
    /// uses the literal value "base" as the name of the parent template to extend,
    /// or ``{% extends variable %}`` uses the value of ``variable`` as either the
    /// name of the parent template to extend (if it evaluates to a string) or as
    /// the parent tempate itelf (if it evaluates to a Template object).
    [<ParserNodes.Description("Indicates that this template extends a parent template.")>]
    type ExtendsTag() =
        interface ITag with

            member x.is_header_tag = true
            member this.Perform token context tokens =
                match token.Args with
                | parent::tail ->

                    /// expression yielding the name of the parent template
                    let parent_name_expr =
                        new ASTNodes.TemplateNameExpression(context, parent)

                    let node_list, remaining = (context.Provider :?> IParser).Parse (Some token) tokens (context.WithBase(parent_name_expr :> INode))

                    /// a list of all blocks in the template starting with the extends tag
                    let node_list =
                        node_list |> List.choose
                            (fun node ->
                                match node with
                                /// we need ParsingContextNode in the nodelist for code completion issues
                                | :? ParserNodes.ParsingContextNode -> Some (node :?> INode)
                                | :? ASTNodes.BlockNode -> Some (node :?> INode)
                                | :? INode when (node :?> INode).NodeType = NodeType.Text ->
                                    let marked =
                                        let body = node.Token.TextToken.RawText.TrimStart();
                                        if (body.Trim() = "") then node :?> INode
                                        else
                                            // build a new token representing non-spaces in the text
                                            let newToken = node.Token.TextToken.CreateToken(node.Token.Length - body.Length, body.Trim().Length)
                                            {new ParserNodes.Node(context, Lexer.Text newToken) with
                                                override x.node_type = NodeType.Text
                                                override x.ErrorMessage = new Error(1, "All content except 'block' tags inside extending template is ignored")
                                                override x.elements = []
                                            } :> INode
                                    Some marked
                                | :? ParserNodes.TagNode as node when node.Tag.is_header_tag -> Some (node :> INode)
                                | _ ->
                                    if (context.Provider.Settings.[Constants.EXCEPTION_IF_ERROR] :?> bool)
                                    then None
                                    else
                                        Some ({new ParserNodes.ErrorNode
                                                (context, node.Token,
                                                 new Error(1, "All content except 'block' tags inside extending template is ignored"))
                                                 with
                                                    override x.elements = [node :?> INode]
                                                 }
                                                    :> INode)
                            )

                    let node_list =
                        match tail with
                        | [] -> node_list
                        | _ ->
                            node_list @
                            [{new ParserNodes.ErrorNode(context,
                                Lexer.Text tail.Head, new Error(1, "Excessive arguments in the extends tag are ignored"))
                                with override x.elements = []
                                } :> INode]

                    /// produces a flattened list of all nodes and child nodes within a 'node list'.
                    /// the 'node list' is a list of all nodes collected from Nodes property of the INode interface
                    let rec unfold_nodes = function
                    | (h:INode)::t ->
                        h :: unfold_nodes
                            (h.Nodes.Values |> Seq.cast |> Seq.map(fun (seq) -> (Seq.toList seq)) |>
                                List.concat |>
                                    List.filter (fun node -> match node with | :? ParserNodes.Node -> true | _ -> false))
                                         @ unfold_nodes t
                    | _ -> []

                    // even though the extends filters its node list, we still need to filter the flattened list because of nested blocks
                    let blocks = Map.ofList <| List.choose
                                    (fun (node: INode) ->  match node with | :? ASTNodes.BlockNode as block -> Some (block.Name,[block]) | _ -> None)
                                    (unfold_nodes node_list)

                    ((new ASTNodes.ExtendsNode(context, token, this, node_list, blocks, parent_name_expr) :> INodeImpl),
                        context, remaining)
                | _ ->
                    // this is a fictitious node created only for the purpose of providing the intellisense
                    // we need to position it right before the closing bracket
                    let parent_name_expr = ASTNodes.TemplateNameExpression(context, token.CreateToken(token.RawText.Length-2, 0)) :> INode

                    let node_list, remaining = (context.Provider :?> IParser).Parse (Some token) tokens context

                    raise (SyntaxError (
                                 "extends tag - missing template name",
                                 Some (Seq.ofList node_list),
                                 Some [parent_name_expr],
                                 Some remaining))

    /// Loads a template and renders it with the current context. This is a way of "including" other templates within a template.
    ///
    /// The template name can either be a variable or a hard-coded (quoted) string, in either single or double quotes.
    ///
    /// This example includes the contents of the template "foo/bar.html":
    ///
    /// {% include "foo/bar.html" %}
    /// This example includes the contents of the template whose name is contained in the variable template_name:
    ///
    /// {% include template_name %}
    /// An included template is rendered with the context of the template that's including it. This example produces the output "Hello, John":
    ///
    /// Context: variable person is set to "john".
    ///
    /// Template:
    ///
    /// {% include "name_snippet.html" %}
    /// The name_snippet.html template:
    ///
    /// Hello, {{ person }}
    /// See also: {% ssi %}.

    [<ParserNodes.Description("Loads and renders a template.")>]
    type IncludeTag() =

        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                match token.Args with
                | name::[] ->
                    let template_name =
                        new ASTNodes.TemplateNameExpression(context, name)
                    ({
                        //todo: we're not producing a node list here. may have to revisit
                        new ParserNodes.TagNode(context, token, this)
                        with
                            override this.walk manager walker =
                                {walker with parent=Some walker; nodes=(ASTNodes.get_template manager context.Resolver context.Model template_name walker.context).Nodes}
                            override this.elements
                                with get()=
                                    (template_name :> INode) :: base.elements
                    } :> INodeImpl), context, tokens
                | _ -> raise (SyntaxError ("'include' tag takes only one argument"))

/// ssi¶
/// Output the contents of a given file into the page.
///
/// Like a simple "include" tag, {% ssi %} includes the contents of another file -- which must be specified using an absolute path -- in the current page:
///
/// {% ssi /home/html/ljworld.com/includes/right_generic.html %}
/// If the optional "parsed" parameter is given, the contents of the included file are evaluated as template code, within the current context:
///
/// {% ssi /home/html/ljworld.com/includes/right_generic.html parsed %}
/// Note that if you use {% ssi %}, you'll need to define ALLOWED_INCLUDE_ROOTS in your Django settings, as a security measure.
///
/// See also: {% include %}.

    type Reader = Path of string | TextReader of System.IO.TextReader

    type SsiNode(provider, token, tag, reader: Reader, loader: string->TextReader) =
        inherit ParserNodes.TagNode(provider, token, tag)

        override this.walk manager walker =
            let templateReader =
                match reader with
                | Path path -> loader path
                | TextReader reader -> reader
            let bufarray = Array.create 4096 ' '
            let length = templateReader.Read(bufarray, 0, bufarray.Length)
            let buffer = Array.sub bufarray 0 length |> Seq.fold (fun status item -> status + string item) ""
            let nodes =
                if length = 0
                then templateReader.Close(); walker.nodes
                else (new SsiNode(provider, token, tag, TextReader templateReader, loader) :> INodeImpl) :: walker.nodes
            {walker with buffer = buffer; nodes=nodes}

    [<ParserNodes.Description("Outputs the contents of a given file into the page.")>]
    type SsiTag() =

        interface ITag with
            member x.is_header_tag = false
            member this.Perform token context tokens =
                match token.Args with
                | path::[] -> (new SsiNode(context, token, (this :> ITag), Path path.Value, context.Provider.Loader.GetTemplate) :> INodeImpl), context, tokens
                | path::Lexer.MatchToken("parsed")::[] ->
// TODO: ExpressionToken
                    let templateRef = Expressions.FilterExpression (context, path.WithValue("\"" + path.Value + "\"") (Some [1,false;path.Value.Length,true;1,false]))
                    ({
                        new ParserNodes.TagNode(context, token, (this :> ITag))
                        with
                            override this.walk manager walker =
                                {walker with parent=Some walker; nodes=(ASTNodes.get_template manager context.Resolver context.Model templateRef walker.context).Nodes}
                    } :> INodeImpl), context, tokens
                | _ ->
                    raise (SyntaxError ("malformed 'ssi' tag"))
