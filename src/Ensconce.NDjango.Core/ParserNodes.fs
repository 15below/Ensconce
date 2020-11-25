namespace Ensconce.NDjango.Core

open System.Collections.Generic

module public ParserNodes =

    /// Django construct bracket type
    type private BracketType =
        /// Open bracket
        |Open
        /// Close bracket
        |Close

    /// Base class for all Django syntax nodes.
    [<AbstractClass>]
    type Node(context, token) =

        /// The token that defined the node
        member x.Token = token

        /// Advances the walker as a part of the tag rendering process
        abstract member walk: ITemplateManager -> Walker -> Walker
        default  x.walk manager walker = walker

        /// List of child nodes used by the tags with a single list of child nodes e.g. spaceless, with or escape
        abstract member nodelist: INodeImpl list
        default x.nodelist = []

        /// Methods/Properties for the INode interface
        /// Node type - only nodes of NodeType.Construct are important for rendering.
        /// The rest of them are used for diagnostic
        abstract member node_type: NodeType

        /// A dictionary of all lists of child nodes
        /// by iterating through the dictionary a complete list of all elements and child nodes can be retrieved
        abstract member Nodes: Map<string, IEnumerable<INode>>
        default x.Nodes =
            new Map<string, IEnumerable<INode>>([])
                |> Map.add Constants.NODELIST_TAG_CHILDREN (x.nodelist |> Seq.map (fun node -> (node :?> INode)))
                |> Map.add Constants.NODELIST_TAG_ELEMENTS (x.elements :> IEnumerable<INode>)

        /// A list of nodes representing django construct elements including construct markers, tag name , variable, etc.
        abstract member elements: INode list
        default x.elements
            with get() =
                [
                    (new ConstructBracketNode(context, token, Open) :> INode);
                    (new ConstructBracketNode(context, token, Close) :> INode)
                ]

        /// Error message represented by this node
        abstract member ErrorMessage: Error
        default x.ErrorMessage = Error.None

        /// Description to be shown for this node
        abstract member Description: string
        default x.Description = ""

        interface INode with

            member x.NodeType = x.node_type
            /// Position - the position of the first character of the token
            member x.Position = token.Position
            /// Length - length of the token
            member x.Length = token.Length
            member x.ErrorMessage = x.ErrorMessage
            member x.Description = x.Description
            member x.Nodes = x.Nodes :> IDictionary<string, IEnumerable<INode>>
            member x.Context = context

        interface INodeImpl with
            member x.Token = x.Token
            member x.walk manager walker = x.walk manager walker

    /// Node representing a django construct bracket
    and private ConstructBracketNode(context, token: Lexer.Token, bracketType: BracketType) =

        interface INode with

            /// TagNode type = marker
            member x.NodeType = NodeType.Marker

            /// Position - start position for the open bracket, endposition - 2 for the close bracket
            member x.Position =
                match bracketType with
                | Open -> token.Position
                | Close -> token.Position + token.Length - 2

            /// Length of the marker = 2
            member x.Length = 2

            /// No message associated with the node
            member x.ErrorMessage = Error.None

            /// No description
            member x.Description = ""

            /// node lists are empty
            member x.Nodes = Map.empty :> IDictionary<string, IEnumerable<INode>>

            member x.Context = context

    /// Value list node is a node carrying a list of values which will be used by code completion
    /// it can be used either directly or through several node classes inherited from the Value list node
    type ValueListNode(context, nodeType, token: Lexer.Token, values)  =

        override x.ToString() = token.TextToken.RawText

        interface INode with
            member x.NodeType = nodeType
            member x.Position = token.Position
            member x.Length = token.Length
            /// No message associated with the node
            member x.ErrorMessage = Error.None
            /// No description
            member x.Description = ""
            /// node list is empty
            member x.Nodes = Map.empty :> IDictionary<string, IEnumerable<INode>>

            member x.Context = context

        interface ICompletionValuesProvider with
            member x.Values = values

    /// a node representing a tag name. carries a list of valid tag names
    type TagNameNode (context, token) =
        inherit ValueListNode
            (
                context,
                NodeType.TagName,
                token,
                context.Provider.Tags |> Map.toSeq |> Seq.map (fun tag -> fst tag)
            )

    /// a node representing a keyword - i.e. on/off values for the autoescape tag
    type KeywordNode (context, token, values) =
        inherit ValueListNode
            (
                context,
                NodeType.Keyword,
                Lexer.Text token,
                values
            )

    /// a node representing a filter name
    type FilterNameNode (context, token, values) =
        inherit ValueListNode
            (
                context,
                NodeType.FilterName,
                Lexer.Text token,
                values
            )

    /// a node representing type name. the list of available type names is generated by the designer
    type TypeNameNode(context, token: Lexer.Token) =

        override x.ToString() = token.TextToken.RawText

        interface INode with
            member x.NodeType = NodeType.TypeName
            member x.Position = token.Position
            member x.Length = token.Length
            /// No message associated with the node
            member x.ErrorMessage = Error.None
            /// No description
            member x.Description = ""
            /// node list is empty
            member x.Nodes = Map.empty :> IDictionary<string, IEnumerable<INode>>

            member x.Context = context

        interface ICompletionProvider

    /// a node representing the parsing context. Carries information
    /// necessary for the designer to show information specific to the parsing
    /// context as well as boundaries of the context.
    /// Ignore during rendering
    type ParsingContextNode (context: IParsingContext, position, length) =
        abstract member Values: seq<string>
        default x.Values = context.Provider.Tags |> Map.toSeq |> Seq.map (fun tag -> fst tag)

        abstract member NodeType: NodeType
        default x.NodeType = NodeType.ParsingContext

        interface INode with
            member x.NodeType = x.NodeType
            /// Position - the position of the first character of the context
            member x.Position = position
            /// Length - length of the context
            member x.Length = length
            member x.ErrorMessage = Error.None
            member x.Description = ""
            member x.Nodes = Map.empty :> IDictionary<string, IEnumerable<INode>>
            member x.Context = context

        interface ICompletionValuesProvider with
            member x.Values = x.Values

        interface INodeImpl with
            member x.Token = failwith ("Token on the ParsingContextNode should not be accessed")
            member x.walk manager walker = walker

    type CommentContextNode (context, position, length) =
        inherit ParsingContextNode(context, position, length)

        override x.NodeType = NodeType.CommentContext

        override x.Values = Seq.empty

    /// For tags decorated with this attribute the string given as a parmeter for the attribute
    /// will be shown in the tooltip for the tag
    type DescriptionAttribute(description: string) =
        inherit System.Attribute()

        member x.Description = description

    /// Base class for all syntax nodes representing django tags
    type TagNode(context, token, tag: ITag) =
        inherit Node(context, Lexer.Block token)

        member x.Tag = tag

        /// NodeType = Tag
        override x.node_type = NodeType.Construct

        override x.elements = TagNameNode(context, Lexer.Text token.Verb) :> INode :: base.elements

        override x.Description =
            match context.Provider.Tags.TryFind(token.Verb.RawText) with
            | None -> ""
            | Some tag ->
                let attrs = tag.GetType().GetCustomAttributes(typeof<DescriptionAttribute>, false)
                attrs |> Array.fold (fun text attr -> text + (attr :?> DescriptionAttribute).Description ) ""

    type CloseTagNode(context, token) =
        inherit Node(context, Lexer.Block token)

        override x.node_type = NodeType.CloseTag

        /// Add TagName node to the list of elements
        override x.elements =
            (new TagNameNode(context, Lexer.Text token.Verb) :> INode) :: base.elements

        override x.Description = "end of the nested node list"

    /// Error nodes
    type ErrorNode(context, token: Lexer.Token, error: Error) =
        inherit Node(context, token)

        // in some cases (like an empty tag) we need this for proper colorization
        // if the colorization is already there it does not hurt
        override x.node_type = NodeType.Construct

        override x.ErrorMessage = error

        /// Walking an error node throws an error
        override x.walk manager walker =
            raise (SyntaxException(error.Message, token))
