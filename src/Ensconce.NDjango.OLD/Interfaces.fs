namespace Ensconce.NDjango

open System.Collections.Generic
open System.IO

[<AutoOpen>]
module public Interfaces =

    type NodeType =

            /// <summary>
            /// The whole django construct.
            /// <example>{% if somevalue %} {{ variable }} </example>
            /// </summary>
            | Construct = 0x0001

            /// <summary>
            /// The markers, which frame django construct.
            /// <example>{%, {{, %}, }}</example>
            /// </summary>
            | Marker = 0x0002

            /// <summary>
            /// Django tag name.
            /// <example> "with", "for", "ifequal"</example>
            /// </summary>
            | TagName = 0x0003

            /// <summary>
            /// The keyword, as required by some tags.
            /// <example>"and", "as"</example>
            /// </summary>
            | Keyword = 0x0004

            /// <summary>
            /// The variable definition used in tags which introduce new variables i.e.
            /// loop variable in the For tag.
            /// <example>loop_item</example>
            /// </summary>
            | VariableDefinition = 0x0005

            /// <summary>
            /// Expression, which consists of a reference followed by 0 or more filters
            /// <example>User.DoB|date:"D d M Y"</example>
            /// </summary>
            | Expression = 0x0006

            /// <summary>
            /// Reference to a value in the current context.
            /// <example>User.DoB</example>
            /// </summary>
            | Reference = 0x0007

            /// <summary>
            /// Filter with or without a parameter. Parameter can be a constant or a reference
            /// <example>default:"nothing"</example>
            /// </summary>
            | Filter = 0x0008

            /// <summary>
            /// The name of the filter.
            /// <example>"length", "first", "default"</example>
            /// </summary>
            | FilterName = 0x0009

            /// <summary>
            /// Filter parameter.
            /// <example>any valid value</example>
            /// </summary>
            | FilterParam = 0x000a

            /// <summary>
            /// Text node.
            /// <example>any valid value</example>
            /// </summary>
            | Text = 0x000b

            /// <summary>
            /// Text node.
            /// <example>any valid value</example>
            /// </summary>
            | Comment = 0x000c

            /// <summary>
            /// A special node to carry the parsing context info for code completion
            /// </summary>
            | ParsingContext = 0x000d

            /// <summary>
            /// A closing node terminating the list of the nested tags
            /// </summary>
            | CloseTag = 0x000e

            /// <summary>
            /// a special expression node representing the template name
            /// </summary>
            | TemplateName = 0x000f

            /// <summary>
            /// a special expression node representing the block name
            /// </summary>
            | BlockName = 0x0010

            /// <summary>
            /// a special expression node representing the .NET type name - i.e. in the model tag
            /// </summary>
            | TypeName = 0x0011

            /// <summary>
            /// A special node to carry the parsing context about comment info for code completion
            /// </summary>
            | CommentContext = 0x0012

    /// Error message
    type Error(severity:int, message:string) =
        /// indicates the severity of the error with 0 being the information message
        /// negative severity is used to mark a dummy message ("No messages" message)
        member x.Severity = severity
        member x.Message = message
        static member None = new Error(-1, "")

    /// A no-parameter filter
    type ISimpleFilter =
        /// Applies the filter to the value
        abstract member Perform: value: obj -> obj

    /// A filter that accepts a single parameter of type 'a
    type IFilter =
        inherit ISimpleFilter
        /// Provides the default value for the filter parameter
        abstract member DefaultValue: obj
        /// Applies the filter to the value using provided parameter value
        abstract member PerformWithParam: value:obj * parameter:obj -> obj

    /// Template loader. Retrieves the template content
    type ITemplateLoader =
        /// Given the path to the template returns the textreader to be used to retrieve the template content
        abstract member GetTemplate: path:string -> TextReader
        /// returns true if the specified template was modified since the specified time
        abstract member IsUpdated: path:string * timestamp:System.DateTime -> bool

    /// An execution context container. This interface defines a set of methods necessary
    /// for templates and external entities to exchange information.
    type IContext =
        /// Adds an object (a variable) to the context
        abstract member add:(string*obj)->IContext

        /// Attempts to find an object in the context by the key
        abstract member tryfind: string->obj option

        /// Removes an object (a variable) from the context
        abstract member remove:string->IContext

        /// Indicates that this Context is in Autoescape mode
        abstract member Autoescape: bool

        /// Returns a new Context with the specified Autoescape mode
        abstract member WithAutoescape: bool -> IContext

        /// Translation routine - when applied to the value returns it translated
        /// to the language for the user
        abstract member Translate: string -> string

        /// Type of the model to be associated with the template
        abstract member ModelType: System.Type option

        abstract member WithModelType: System.Type -> IContext

    /// Single threaded template manager. Caches templates it renders in a non-synchronized dictionary
    /// should be used only to service rendering requests from a single thread
    type ITemplateManager =

        /// given the path to the template and context returns the <see cref="System.IO.TextReader"/>
        /// that will stream out the results of the render.
        abstract member RenderTemplate: path:string * context:IDictionary<string, obj> -> TextReader

        /// given the path to the template and context returns the template implementation
        abstract member GetTemplate: template:string * resolver:TypeResolver.ITypeResolver * model:TypeResolver.ModelDescriptor -> ITemplate

        /// given the path to the template and context returns the template implementation
        abstract member GetTemplate: template:string -> ITemplate

    /// Template imeplementation. This interface effectively represents the root-level node
    /// in the Django AST.
    and ITemplate =
        /// Recursivly "walks" the AST, returning a text reader that will stream out the
        /// results of the render.
        abstract Walk: ITemplateManager -> IDictionary<string, obj> -> System.IO.TextReader

        /// A list of top level sibling nodes
        abstract Nodes: INodeImpl list

    /// Rendering state used by the ASTWalker to keep track of the rendering process as it walks through
    /// the template abstract syntax tree
    and Walker =
        {
            /// parent walker to be resumed after the processing of this one is completed
            parent: Walker option
            /// List of nodes to walk
            nodes: INodeImpl list
            /// string rendered by the last node
            buffer: string
            /// the index of the first character in the buffer yet to be sent to output
            bufferIndex: int
            /// rendering context
            context: IContext
        }

    /// A representation of a node of the template abstract syntax tree
    and INodeImpl =

        /// The token that defined the node
        abstract member Token : Lexer.Token

        /// Processes this node and advances the walker to reflect the progress made
        abstract member walk: manager:ITemplateManager -> walker:Walker -> Walker

    /// Top level object managing multi threaded access to configuration settings and template cache.
    type ITemplateManagerProvider =

        /// tag definitions available to the provider
        abstract member Tags: Map<string, ITag>

        /// filter definitions available to the provider
        abstract member Filters: Map<string, ISimpleFilter>

        /// current configuration settings
        abstract member Settings: Map<string, obj>

        /// Creates a translator for a given language
        abstract member CreateTranslator: string-> (string->string)

        /// current template loader
        abstract member Loader: ITemplateLoader

        /// Retrieves the requested template checking first the global
        /// dictionary and validating the timestamp
        abstract member GetTemplate: (string * TypeResolver.ITypeResolver * TypeResolver.ModelDescriptor) -> (Map<string, ITemplate * System.DateTime> * ITemplate)

        /// Retrieves the requested template without checking the
        /// local dictionary and/or timestamp
        /// the retrieved template is placed in the dictionary replacing
        /// the existing template with the same name (if any)
        abstract member LoadTemplate: (string * TypeResolver.ITypeResolver * TypeResolver.ModelDescriptor) -> (Map<string, ITemplate * System.DateTime> * ITemplate)

    /// A tag implementation
    and ITag =
        ///<summary>
        /// Transforms a {% %} tag into a list of nodes and uncommited token list
        ///</summary>
        ///<param name="token">token for the tag name</param>
        ///<param name="context">the parsing context for the token</param>
        ///<param name="tokens">the remainder of the token list</param>
        ///<returns>
        /// a tuple consisting of the INodeImpl object representing the result of node parsing as the first element
        /// followed by the the remainder of the token list with all the token related to the node removed
        ///</returns>
        abstract member Perform: Lexer.BlockToken -> IParsingContext -> LazyList<Lexer.Token> -> (INodeImpl * IParsingContext * LazyList<Lexer.Token>)

        /// Indicates whether this node must be the first non-text node in the template
        abstract member is_header_tag: bool

    /// Parsing context is a container for information specific to the tag being parsed
    and IParsingContext =

        /// template manager provider owning the context
        abstract member Provider: ITemplateManagerProvider

        /// creates a new parsing context as a child to the current one
        abstract member ChildOf: IParsingContext

        /// for a nested oarsing context - parent parsing context
        abstract member Parent: IParsingContext option

        /// creates a new parsing context with the flag set indicating that no more header tags are allowed
        abstract member BodyContext: IParsingContext

        /// a flag indicating if any of the non-header tags have been encountered yet
        abstract member IsInHeader: bool

        /// creates a new parsing context with a list of additional "end" tags
        abstract member WithClosures: string list -> IParsingContext

        /// a list of all closing tags for the context
        abstract member TagClosures: string list

        /// creates a new parsing context with the new model
        abstract member WithNewModel: TypeResolver.ModelDescriptor -> IParsingContext

        /// current model descriptor
        abstract member Model: TypeResolver.ModelDescriptor

        /// creates a new parsing context with the specified reference to the template being extended
        abstract member WithBase: INode -> IParsingContext

        /// for templates extending base templates - a reference to the base template
        abstract member Base: INode option

        /// a reference to the active type resolver (as originally passed to the TemplateManager.GetTemplate method
        abstract member Resolver: TypeResolver.ITypeResolver

    /// A representation of a node of the template abstract syntax tree
    and INode =

        /// TagNode type
        abstract member NodeType: NodeType

        /// Position of the first character of the node text
        abstract member Position: int

        /// Length of the node text
        abstract member Length: int

        /// message associated with the node
        abstract member ErrorMessage: Error

        /// TagNode description (will be shown in the tooltip)
        abstract member Description: string

        /// node lists
        abstract member Nodes: IDictionary<string, IEnumerable<INode>>

        /// parsing context for the node
        abstract member Context : IParsingContext

    /// Parsing interface definition
    type internal IParser =
        /// Produces a commited node list and uncommited token list as a result of parsing until
        /// a block from the string list is encotuntered
        abstract member Parse: parent: Lexer.BlockToken option -> tokens:LazyList<Lexer.Token> -> context:IParsingContext -> (INodeImpl list * LazyList<Lexer.Token>)

        /// Parses the template From the source in the reader into the node list
        abstract member ParseTemplate: template:TextReader * resolver:TypeResolver.ITypeResolver * model:TypeResolver.ModelDescriptor -> INodeImpl list

        /// Parses the template From the source in the reader into the node list
        abstract member ParseTemplate: template:TextReader -> INodeImpl list

        /// Produces an uncommited token list as a result of parsing until
        /// a block from the string list is encotuntered
        abstract member Seek: tokens:LazyList<Lexer.Token> -> context:IParsingContext -> parse_until:string list -> (INodeImpl * INodeImpl * LazyList<Lexer.Token>)

    type ICompletionProvider = interface end

    type ICompletionValuesProvider =
        inherit ICompletionProvider

        /// a list of values allowed for the node
        abstract member Values: IEnumerable<string>

    /// This exception is thrown if a problem encountered while rendering the template
    /// This exception will be later caught in the ASTWalker and re-thrown as the
    /// RenderingException
    type RenderingError (message: string, ?innerException: exn) =
            inherit System.ApplicationException(message, defaultArg innerException null)

    /// The actaual redering exception. The original RenderingError exceptions are caught and re-thrown
    /// as RenderingExceptions
    type RenderingException (message: string, token:Lexer.Token, ?innerException: exn) =
            inherit System.ApplicationException(message + token.DiagInfo, defaultArg innerException null)

    /// Exception raised when template syntax errors are encountered
    /// this exception is defined here because of its dependency on the TextToken class
    type SyntaxException (message: string, token: Lexer.Token) =
        inherit System.ApplicationException(message + token.DiagInfo)
        member x.Token = token
        member x.ErrorMessage = message

    /// This esception is thrown if a problem encountered while parsing the template
    /// This exception will be later caught and re-thrown as the SyntaxException
    type SyntaxError (message, nodes: seq<INodeImpl> option, pattern:INode list option, remaining: LazyList<Lexer.Token> option) =
        inherit System.ApplicationException(message)
        new (message) = new SyntaxError(message, None, None, None)

        /// constructor to be used when the error applies to
        /// multiple tags i.e. missing closing tag exception. Inculdes node list as an
        /// additional parameter
        new (message, nodes) = new SyntaxError(message, Some nodes, None, None)

        ///constructor to be used when it is necessary
        ///to include nodes and remaining tokens to SyntaxError.
        new (message, nodes, remaining) = new SyntaxError(message, Some nodes, None, Some remaining)

        new (message, remaining) = new SyntaxError(message, None, None, Some remaining)

        /// constructor to be used when the error applies to a partially parsed tag
        /// Inculdes a list of tag elements to be associated with the error
        new (message, pattern) = new SyntaxError(message, None, Some pattern, None)

        new (message, pattern, remaining) = new SyntaxError(message, None, Some pattern, Some remaining)

        new (message, nodes, pattern:INode list, remaining) = new SyntaxError(message, Some nodes, Some pattern, Some remaining)

        /// list (sequence) of nodes related to the error
        member x.Nodes = match nodes with | Some n -> n | None -> seq []

        /// list of tag elements from the partially parsed tag
        member x.Pattern = match pattern with | Some p -> p | None -> []

        member x.Remaining = remaining

    /// Tags and/or filters marked with this attribute will be registered under the name
    /// supplied by the attribute unless the name will be provided explicitly during the registartion
    type NameAttribute(name:string) =
        inherit System.Attribute()
        member x.Name = name