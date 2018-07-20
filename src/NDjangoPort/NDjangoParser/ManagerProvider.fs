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

namespace NDjango

open System.IO
open System.Text
open NDjango.Lexer
open NDjango.Interfaces
open NDjango.Filters
open NDjango.Tags
open NDjango.Tags.Misc
open NDjango.ASTNodes
open NDjango.ParserNodes
open NDjango.Expressions
open NDjango.TypeResolver
open Microsoft.FSharp.Collections

type Filter = {name:string; filter:ISimpleFilter}

type Tag = {name:string; tag:ITag}

type Setting = {name:string; value:obj}

module Defaults =

    let private (++) map (key: 'a, value: 'b) = Map.add key value map

    let internal standardFilters =
        new Map<string, ISimpleFilter>([])
            ++ ("date", (new Now.DateFilter() :> ISimpleFilter))
            ++ ("escape", (new EscapeFilter() :> ISimpleFilter))
            ++ ("force_escape", (new ForceEscapeFilter() :> ISimpleFilter))
            ++ ("slugify", (new Slugify() :> ISimpleFilter))
            ++ ("truncatewords" , (new TruncateWords() :> ISimpleFilter))
            ++ ("urlencode", (new UrlEncodeFilter() :> ISimpleFilter))
            ++ ("urlize", (new UrlizeFilter() :> ISimpleFilter))
            ++ ("urlizetrunc", (new UrlizeTruncFilter() :> ISimpleFilter))
            ++ ("wordwrap", (new WordWrapFilter() :> ISimpleFilter))
            ++ ("default_if_none", (new DefaultIfNoneFilter() :> ISimpleFilter))
            ++ ("linebreaks", (new LineBreaksFilter() :> ISimpleFilter))
            ++ ("linebreaksbr", (new LineBreaksBrFilter() :> ISimpleFilter))
            ++ ("striptags", (new StripTagsFilter() :> ISimpleFilter))
            ++ ("join", (new JoinFilter() :> ISimpleFilter))
            ++ ("yesno", (new YesNoFilter() :> ISimpleFilter))
            ++ ("dictsort", (new DictSortFilter() :> ISimpleFilter))
            ++ ("dictsortreversed", (new DictSortReversedFilter() :> ISimpleFilter))
            ++ ("time", (new Now.TimeFilter() :> ISimpleFilter))
            ++ ("timesince", (new Now.TimeSinceFilter() :> ISimpleFilter))
            ++ ("timeuntil", (new Now.TimeUntilFilter() :> ISimpleFilter))
            ++ ("pluralize", (new PluralizeFilter() :> ISimpleFilter))
            ++ ("phone2numeric", (new Phone2numericFilter() :> ISimpleFilter))
            ++ ("filesizeformat", (new FileSizeFormatFilter() :> ISimpleFilter))


    let internal standardTags =
        new Map<string, ITag>([])
            ++ ("autoescape", (new AutoescapeTag() :> ITag))
            ++ ("block", (new LoaderTags.BlockTag() :> ITag))
            ++ ("comment", (new CommentTag() :> ITag))
            ++ ("cycle", (new Cycle.Tag() :> ITag))
            ++ ("debug", (new DebugTag() :> ITag))
            ++ ("extends", (new LoaderTags.ExtendsTag() :> ITag))
            ++ ("filter", (new Filter.FilterTag() :> ITag))
            ++ ("firstof", (new FirstOfTag() :> ITag))
            ++ ("for", (new For.Tag() :> ITag))
            ++ ("if", (new If.Tag() :> ITag))
            ++ ("ifchanged", (new IfChanged.Tag() :> ITag))
            ++ ("ifequal", (new IfEqual.Tag(true) :> ITag))
            ++ ("ifnotequal", (new IfEqual.Tag(false) :> ITag))
            ++ ("include", (new LoaderTags.IncludeTag() :> ITag))
            ++ ("model", (new Model.Tag() :> ITag))
            ++ ("now", (new Now.Tag() :> ITag))
            ++ ("regroup", (new RegroupTag() :> ITag))
            ++ ("spaceless", (new SpacelessTag() :> ITag))
            ++ ("ssi", (new LoaderTags.SsiTag() :> ITag))
            ++ ("templatetag", (new TemplateTag() :> ITag))
            ++ ("widthratio", (new WidthRatioTag() :> ITag))
            ++ ("with", (new WithTag() :> ITag))

    let internal defaultSettings =
        new Map<string, obj>([])
            ++ (Constants.DEFAULT_AUTOESCAPE, (true :> obj))
            ++ (Constants.RELOAD_IF_UPDATED, (true :> obj))
            ++ (Constants.EXCEPTION_IF_ERROR, (true :> obj))
            ++ (Constants.TEMPLATE_STRING_IF_INVALID, ("" :> obj))
            ++ (Constants.USE_I18N, (false :> obj))

    let internal defaultDictionary =
        new Map<string, Map<string,string>>([])

type private DefaultLoader() =
    interface ITemplateLoader with
        member this.GetTemplate source =
            if source.StartsWith("temp://") then
                let  byteArray = Encoding.ASCII.GetBytes(source.Remove(0,7))
                (new StreamReader(new MemoryStream(byteArray)) :> TextReader)
            else if not <| File.Exists(source) then
                raise (FileNotFoundException (sprintf "Could not locate template '%s'" source))
            else
                (new StreamReader(source) :> TextReader)

        member this.IsUpdated (source, timestamp) =
            if not <| source.StartsWith("temp://") then
                File.GetLastWriteTime(source) > timestamp
            else false

///<summary>
/// Manager Provider object serves as a container for all the environment variables controlling
/// template processing.
///</summary>
///<remarks>
/// Various methods of the object provide different ways to change them,
/// namely add tag and/or filter definitions, change settings or switch the loader. All such
/// methods DO NOT affect the provider they are called on, but rather create a new clean provider
/// instance with modified variables.
/// Method GetManager on the provider can be used to create an instance of TemplateManager
/// TemplateManagers can be used to render templates.
/// All methods and properties of the Manager Provider use locking as necessary and are thread safe.
///</remarks>
type TemplateManagerProvider (settings:Map<string,obj>, tags, filters, loader:ITemplateLoader, dictionary) =

    let (++) map (key: 'a, value: 'b) = Map.add key value map

    /// global lock
    let lockProvider = new obj()

    /// global map of loaded templates shared among all template managers
    let templates = ref Map.empty

    /// function used to validate if a template needs to be reloaded
    /// it is calculated when the template manager provider is created
    let validate_template =
        if (settings.[Constants.RELOAD_IF_UPDATED] :?> bool) then loader.IsUpdated
        else (fun (name,ts) -> false)

    /// converts an exception into a syntax node to make diagnostic messages the exception
    /// carries available to the designer. The exception can carry additional infromation
    /// about the elements of the node at fault in form of additional nodes. It is
    /// passed on to the designer by making these nodes children of the error node.
    /// This function is used inside 'parse_token' function, where we need just one node,
    /// carrying all necessary information. All already parsed nodes are contained
    /// in the nodelist of just created ErrorNode.
    let generate_error_node_for_tag (ex: System.Exception) token context tokens =
        match (token, ex) with
        | (Some blockToken, (:? SyntaxError as syntax_error)) ->
            if (settings.[Constants.EXCEPTION_IF_ERROR] :?> bool)
            then
                raise (SyntaxException(syntax_error.Message, Block blockToken))
            else
                Some ( ({
                            new ErrorNode(context, Block blockToken, new Error(2, syntax_error.Message))
                                with
                                    /// Add TagName node to the list of elements
                                    override x.elements =
                                        (new TagNameNode(context, Text blockToken.Verb) :> INode)
                                         :: syntax_error.Pattern @ base.elements
                                    /// Add parsed nodes, gathered from syntax_error to the nodelist of
                                    /// this ErrorNode
                                    override x.nodelist = List.ofSeq syntax_error.Nodes
                            } :> INodeImpl),
                        match syntax_error.Remaining with
                        | Some remaining -> remaining
                        | None -> tokens
                        )
        |_  -> None

    ///This function is used inside 'parse' function, where we need the list of nodes.
    ///The head of this list is a node with token necessary to create ParsingContextNode.
    ///We also create ErrorNode with some diag information, but nodelist of this ErrorNode is empty
    let generate_diag_for_tag (ex: System.Exception) token context tokens =
        match (token, ex) with
        | (Some blockToken, (:? SyntaxError as syntax_error)) ->
            if (settings.[Constants.EXCEPTION_IF_ERROR] :?> bool)
            then
                raise (SyntaxException(syntax_error.Message, Block blockToken))
            else
                Some ( List.ofSeq syntax_error.Nodes @
                        [({
                            new ErrorNode(context, Block blockToken, new Error(2, syntax_error.Message))
                                with
                                    /// Add TagName node to the list of elements
                                    override x.elements =
                                        (new TagNameNode(context, Text blockToken.Verb) :> INode)
                                         :: syntax_error.Pattern @ base.elements
                            } :> INodeImpl)],
                        match syntax_error.Remaining with
                        | Some remaining -> remaining
                        | None -> tokens
                        )
        |_  -> None

    ///<summary>
    /// Parses a single token, returning an AST TagNode list.
    ///</summary>
    ///<remarks>
    /// This function may advance the token stream if an element consuming multiple tokens is encountered.
    /// In this scenario, the TagNode list returned will contain nodes for all of the advanced tokens.
    ///</remarks>
    let parse_token (context:IParsingContext) tokens token =

        match token with
        | Lexer.Text textToken ->
            ({new Node(context, token)
                with
                    override x.node_type = NodeType.Text

                    override x.elements = []

                    override x.walk manager walker =
                        {walker with buffer = textToken.RawText}
            } :> INodeImpl), context, tokens
        | Lexer.Variable var ->
            // var.Expression is a raw string - the string as is on the template before any parsing or substitution
            let expression = new FilterExpression(context, var.Expression)

            ({new Node(context, token)
                with
                    override x.node_type = NodeType.Expression

                    override x.walk manager walker =
                        match expression.ResolveForOutput manager walker with
                        | Some w -> w
                        | None -> walker

                    override x.elements = (expression :> INode) :: base.elements
            } :> INodeImpl), context.BodyContext, tokens

        | Lexer.Block block ->
            try
                match Map.tryFind block.Verb.RawText tags with
                | None -> raise (SyntaxError ("Tag is unknown or out of context: " + block.Verb.RawText, None, None, Some tokens))
                | Some (tag: ITag) ->
                    if not context.IsInHeader && tag.is_header_tag
                    then raise (SyntaxError ("Header tag in the template body: " + block.Verb.RawText, None, None, Some tokens))
                    let node, context, tokens = tag.Perform block context tokens
                    if tag.is_header_tag
                    then (node, context, tokens)
                    else (node, context.BodyContext, tokens)
            with
                |_ as ex ->
                    // here we need to squeeze all available information into the mold
                    // of the return value expected from this function. In other words
                    // the node list returned by diag here should always have only one node
                    match generate_error_node_for_tag ex (Some block) context tokens with
                    | Some result ->
                        let node, remainder = result
                        node, context, remainder
                    | None -> reraise()
        | Lexer.Comment comment ->
            // include it in the output to cover all scenarios, but don't actually bring the comment across
            // the default behavior of the walk override is to return the same walker
            // Considering that when walk is called the buffer is empty, this will
            // work for the comment node, so overriding the walk method here is unnecessary
            ({new Node(context, token) with override x.node_type = NodeType.Comment} :> INodeImpl), context, tokens

        | Lexer.Error error ->
            if (settings.[Constants.EXCEPTION_IF_ERROR] :?> bool)
                then raise (SyntaxException(error.ErrorMessage, Token.Error error))
            ({
                        new ErrorNode(context, token, new Error(2, error.ErrorMessage))
                            with
                                override x.elements =
                                    match error.RawText.[0..1] with
                                    | "{%" ->
                                        /// this is a tag node - add a TagName node to the list of elements
                                        /// so that tag name code completion can be triggered
                                        let offset = error.RawText.Length - error.RawText.[2..].TrimStart([|' ';'\t'|]).Length
                                        let name_token = error.CreateToken(offset, 0)
                                        (new TagNameNode(context, Text name_token )
                                                :> INode) :: base.elements
                                    | _ -> base.elements
                        } :> INodeImpl), context, tokens

    /// builds diagnostic message from the list of possibke closing tags
    let fail_closing parse_until =
                sprintf "Missing closing tag. Available tags: %s"
                    (snd
                        (List.fold
                            (fun acc elem -> (", ", (snd acc) + (fst acc) + elem))
                            ("","")
                            parse_until))

    /// builds a string, that consists of block's verb and arguments, separated with whitespace(s).
    /// i.e. {% endif a  b %} -> "endif a b"
    let terminals (block : BlockToken) =
        String.concat " " ((block.Verb :: block.Args) |> List.map(fun text_token -> text_token.Value))

    /// recursively parses the token stream until the token(s) listed in parsing context TagClosures are encountered.
    /// this function returns the node list and the unparsed remainder of the token stream.
    /// The list is returned in the reversed order
    let rec parse_internal (context:IParsingContext) nodes tokens =
       match tokens with
       | LazyList.Cons(token, tokens) ->
            match token with
            | Lexer.Block block when context.TagClosures |> List.exists (terminals block).Equals ->
                 (context, (new CloseTagNode(context, block) :> INodeImpl) :: nodes, tokens)
            | _ ->
                let node, context, tokens = parse_token context tokens token
                parse_internal context (node :: nodes) tokens
       | LazyList.Nil ->
            if not <| List.isEmpty context.TagClosures
                then raise (SyntaxError(fail_closing context.TagClosures, nodes, tokens))
            (context, nodes, LazyList.empty<Lexer.Token>)


    /// tries to return a list positioned just after one of the elements of parse_until. Returns None
    /// if no such element was found.
    let rec seek_internal (context:IParsingContext) length tokens =
        match tokens with
        | LazyList.Nil ->
            length, None, LazyList.empty
        | LazyList.Cons(token, tokens) ->
            match token with
            | Lexer.Block block when context.TagClosures |> List.exists block.Verb.Value.Equals ->
                 length, Some (CloseTagNode(context, block) :> INodeImpl), tokens
            | _ ->
                seek_internal context (length+token.Length) tokens

    public new () =
        new TemplateManagerProvider(Defaults.defaultSettings, Defaults.standardTags, Defaults.standardFilters, new DefaultLoader(), Defaults.defaultDictionary)

    member x.WithSetting name value = new TemplateManagerProvider( settings++(name, value), tags, filters, loader, dictionary)

    member x.WithTag name tag = new TemplateManagerProvider(settings, tags++(name, tag) , filters, loader, dictionary)

    member x.WithFilter name filter = new TemplateManagerProvider(settings, tags, filters++(name, filter) , loader, dictionary)

    member x.WithSettings new_settings =
        new TemplateManagerProvider(
            new_settings |> Seq.fold (fun settings setting -> settings++(setting.name, setting.value) ) settings
            , tags, filters, loader, dictionary)

    member x.WithTags (new_tags : Tag seq) =
        new TemplateManagerProvider(settings,
            new_tags |> Seq.fold (fun tags tag -> tags++(tag.name, tag.tag)) tags
            , filters, loader, dictionary)

    member x.WithLibrary (assembly: System.Reflection.Assembly) =

        let ttags, ffilters =
            assembly.GetTypes()
            |> Seq.fold
                (fun (tags, filters) _type ->
                    let attrs = _type.GetCustomAttributes(typeof<NameAttribute>, false)
                    if _type.IsAbstract || _type.IsInterface || attrs.Length = 0 then (tags, filters)
                    else
                        try
                            if (typeof<ITag>.IsAssignableFrom(_type))
                            then (tags ++ ((attrs.[0] :?> NameAttribute).Name, (System.Activator.CreateInstance(_type) :?> ITag)), filters)
                            elif (typeof<ISimpleFilter>.IsAssignableFrom(_type))
                                then(tags, filters ++ ((attrs.[0] :?> NameAttribute).Name, (System.Activator.CreateInstance(_type) :?> ISimpleFilter)))
                                else
                                    (tags, filters)
                        with
                        | _ -> (tags, filters)
                ) (tags, filters)

        new TemplateManagerProvider(settings, ttags, ffilters, loader, dictionary)

    member x.WithLibrary (assembly: System.Reflection.AssemblyName) =
        x.WithLibrary(System.Reflection.Assembly.Load(assembly))

    member x.WithLibrary (assembly: string) =
        x.WithLibrary(new System.Reflection.AssemblyName(assembly))

    member x.WithFilters (new_filters : Filter seq)  =
        new TemplateManagerProvider(settings, tags,
            new_filters |> Seq.fold (fun filters filter -> Map.add filter.name filter.filter filters) filters
            , loader, dictionary)

    member x.WithLoader new_loader = new TemplateManagerProvider(settings, tags, filters, new_loader, dictionary)

    member public x.GetNewManager() = lock lockProvider (fun() -> new Template.Manager(x, !templates) :> ITemplateManager)

    interface ITemplateManagerProvider with

        member x.GetTemplate template =
            let name, _, _ = template
            lock lockProvider
                (fun() ->
                    match !templates |> Map.tryFind name with
                    | None ->
                        (x :> ITemplateManagerProvider).LoadTemplate template
                    | Some (t, timestamp) ->
                        if (validate_template(name, timestamp)) then
                            (x :> ITemplateManagerProvider).LoadTemplate template
                        else
                            (!templates, t)
                )

        member x.LoadTemplate ((name, resolver, model)) =
            lock lockProvider
                (fun() ->
                    let t = ((new NDjango.Template.Impl((x :> ITemplateManagerProvider), loader.GetTemplate(name), resolver, model) :> ITemplate), System.DateTime.Now)
                    templates := Map.add name t !templates
                    (!templates, fst t)
                )

        member x.Tags = tags

        member x.Filters = filters

        member x.Settings = settings

        member x.Loader = loader

        member x.CreateTranslator language =
            if settings.[Constants.USE_I18N] :?> bool then
                try
                    let local_culture = System.Globalization.CultureInfo.GetCultureInfo(language)
                    match dictionary.TryFind local_culture.Name with
                    | Some local_dictionary ->
                        let neutral_culture = local_culture.Parent
                        match dictionary.TryFind neutral_culture.Name with
                        | Some neutral_dictionary ->
                            fun value ->
                                match local_dictionary.TryFind value with
                                | Some v -> v
                                | None ->
                                    match neutral_dictionary.TryFind value with
                                    | Some v -> v
                                    | None -> value
                        | None ->
                            fun value ->
                                match local_dictionary.TryFind value with
                                | Some v -> v
                                | None -> value
                    | None -> fun value -> value
                with
                | _ -> fun value -> value
            else
                fun value -> value

    interface IParser with

        /// Parses the sequence of tokens until one of the given tokens is encountered
        member x.Parse parent tokens context =

            let context = context.ChildOf

            // take a note of the current position - this will be the
            // start position for the parsing context being built
            let start_pos = (tokens |> LazyList.head).Position

            // do the parsing. If an exception is thrown we still need
            // a list of all nodes with as much info about the template
            // as the parser could squeeze out of the source
            let context, nodes, tokens =
                try
                    parse_internal context [] tokens
                with
                | _ as ex ->
                    match generate_diag_for_tag ex parent context tokens with
                    | Some (nodes, tokens) -> (context, nodes, tokens)
                    | None -> reraise()

            match nodes with
            // no nodes - empty template. I do not think it can happen, still...
            | [] -> [], tokens
            // wrap up the parsing by building the ParsingContextNode.
            | hd::_ ->
                // calculate the end position for the context as a position of the first character
                // after the last token. Keep in mind that the nodes are added to the node list
                // in REVERESED order - the first node is the last added, hence it corresponds to the last
                // token
                let end_pos = hd.Token.Position + hd.Token.Length
                let result = nodes |> List.rev
                ((new ParsingContextNode(context, start_pos, end_pos - start_pos) :> INodeImpl) :: result, tokens)

        /// Parses the template From the source in the reader into the node list
        member x.ParseTemplate template =
            (x :> IParser).ParseTemplate (template, new NDjango.TypeResolver.DefaultTypeResolver() :> ITypeResolver, NDjango.TypeResolver.ModelDescriptor(Seq.empty))

        /// Parses the template From the source in the reader into the node list
        member x.ParseTemplate (template, resolver, model) =
            // this will cause the TextReader to be closed when the template goes out of scope
            use template = template
            (x :> IParser).Parse None (NDjango.Lexer.tokenize template) (ParsingContext.Implementation(x, resolver, model)) |> fst

        /// Repositions the token stream after the first token found from the parse_until list
        member x.Seek tokens context parse_until =
            if List.length parse_until = 0 then
                raise (SyntaxError("Seek must have at least one termination tag"))
            else
                let context = context.ChildOf.WithClosures(parse_until)

                // take a note of the current position - this will be the
                // start position for the parsing context being built
                let start_pos = (tokens |> LazyList.head).Position

                let length, close_tag, remainder = seek_internal context 0 tokens
                match close_tag with
                | Some tag -> CommentContextNode(context, start_pos, length) :> INodeImpl, tag, remainder
                | None -> raise (SyntaxError (fail_closing parse_until, [ParsingContextNode(context, start_pos, length) :> INodeImpl]))


