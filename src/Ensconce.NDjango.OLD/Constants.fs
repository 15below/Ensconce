namespace Ensconce.NDjango

open System.Text.RegularExpressions

[<AutoOpen>]
module Constants =

    /// escapes the regular expression-specific elements of the parameter. !!parm == Regex.Escape(parm)
    let internal (!!) parm = Regex.Escape(parm)

    // template syntax constants
    let internal FILTER_SEPARATOR = "|"
    let internal FILTER_ARGUMENT_SEPARATOR = ":"
    let internal VARIABLE_ATTRIBUTE_SEPARATOR = "."
    let internal BLOCK_TAG_START = "{%"
    let internal BLOCK_TAG_END = "%}"
    let internal VARIABLE_TAG_START = "{{"
    let internal VARIABLE_TAG_END = "}}"
    let internal COMMENT_TAG_START = "{#"
    let internal COMMENT_TAG_END = "#}"
    let internal SINGLE_BRACE_START = "{"
    let internal SINGLE_BRACE_END = "}"
    let internal I18N_OPEN = "_("
    let internal I18N_CLOSE = ")"

    //let ALLOWED_VARIABLE_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_."

    // what to report as the origin for templates that come from non-loader sources
    // (e.g. strings)
    let internal UNKNOWN_SOURCE = "&lt;unknown source&gt;"

    /// <summary>
    /// Regular expression to extract django tags from the source of the template.
    /// In compliance with django specs it is built to ensure that a character
    /// sequence starting with a tag start and ending with a tag end is considered
    /// to be a tag ONLY if both tag start and start end are located on the SAME line
    /// </summary>
    let internal tag_re =
        new Regex(
            "(" + !!BLOCK_TAG_START + ".*?" + !!BLOCK_TAG_END + "|"
             + !!VARIABLE_TAG_START + ".*?" + !!VARIABLE_TAG_END + "|"
              + !!COMMENT_TAG_START + ".*?" + !!COMMENT_TAG_END + ")",
            RegexOptions.Compiled)

    /// <summary>
    /// Regular expression parsing a filter expression in raw (Python) format
    /// This expression was modified from the original python version
    /// to allow $ as the first character of variable names. Such names will
    /// be used for internal purposes
    /// </summary>
    let internal filter_raw_string =
        @"
        ^(?P<var>%(i18n_open)s`%(str)s`%(i18n_close)s|
                 `%(str)s`|
                 '%(strs)s'|
                 \$?[%(var_chars)s]+)|
         (?:%(filter_sep)s
             (?P<filter_name>\w+)
                 (?:%(arg_sep)s
                     (?:
                      (?P<arg>%(i18n_open)s`%(str)s`%(i18n_close)s|
                              `%(str)s`|
                              '%(strs)s'|
                              [%(var_chars)s]+)
                     )
                 )?
         )"

    /// <summary>
    // Regular expression for parsing a filter expression in the .NET format
    /// </summary>
    let internal filter_re =
        new Regex(
            filter_raw_string.
                Replace("`", "\"").
                Replace("\r", "").
                Replace("\n", "").
                Replace("\t", "").
                Replace(" ", "").
                Replace("?P<", "?'"). // keep the source regex in Python format for better source code comparability
                Replace(">", "'"). // ditto [^"\]*
                Replace("%(str)s", "[^\"\\\\]*(?:\\\\.[^\"\\\\]*)*").
                Replace("%(strs)s", "[^'\\\\]*(?:\\\\.[^'\\\\]*)*").
                Replace("%(var_chars)s", @"\w\.-"). // apparently the python \w also captures the (-) sign, whereas the .net doesn't. including this here means we also need to check for it in Variable as an illegal first character
                Replace("%(filter_sep)s", "(\s*)" + !!FILTER_SEPARATOR + "(\s*)").
                Replace("%(arg_sep)s", !!FILTER_ARGUMENT_SEPARATOR).
                Replace("%(i18n_open)s", !!I18N_OPEN).
                Replace("%(i18n_close)s", !!I18N_CLOSE), RegexOptions.Compiled)


    /// <summary>
    /// Names for the standard settings - settings defined in the parser core
    /// Default value for the autoescape setting
    /// </summary>
    let DEFAULT_AUTOESCAPE = "settings.DEFAULT_AUTOESCAPE"

    /// <summary>
    /// The value inserted in the rendering result in the cases if
    /// the actual value could not be retrieved
    /// </summary>
    let TEMPLATE_STRING_IF_INVALID = "settings.TEMPLATE_STRING_IF_INVALID"

    /// <summary>
    /// Flag indicating whether the templates should be checked for updates
    /// before rendering. If set to <b>false<b> all modifications to a template
    /// after the first successful parse will be igonred
    /// </summary>
    let RELOAD_IF_UPDATED = "settings.RELOAD_IF_UPDATED"

    /// <summary>
    /// Flag indicating whether template parsing should stop after the first
    /// encountered syntax error
    /// </summary>
    let EXCEPTION_IF_ERROR = "settings.EXCEPTION_IF_ERROR"

    /// <summary>
    /// Flag indicating whether internationalization support should be enabled
    /// set it to 'false' to avoid overhead introduced by transaltion dictionary processing
    /// </summary>
    let USE_I18N = "settings.USE_I18N"


    /// <summary>
    /// List nodes representing the elements of the tag itself, including
    /// markers, tag name, tag paremeters, etc
    /// </summary>
    let NODELIST_TAG_ELEMENTS = "standard.elements";

    /// <summary>
    /// Stadard list of nodes representing child tags
    /// </summary>
    let NODELIST_TAG_CHILDREN = "standard.children";

    /// <summary>
    /// List of nodes representing the <b>true</b> branch of the if tag and similar tags
    /// </summary>
    let NODELIST_IFTAG_IFTRUE = "if.true.children";

    /// <summary>
    /// List of nodes representing the <b>false</b> branch of the if tag and similar tags
    /// </summary>
    let NODELIST_IFTAG_IFFALSE = "if.false.children";

    /// <summary>
    /// List of nodes representing the <b>body</b> branch of the for tag
    /// </summary>
    let NODELIST_FOR_BODY = "for.body.children";

    /// <summary>
    /// List of nodes representing the <b>empty</b> branch of the for tag
    /// </summary>
    let NODELIST_FOR_EMPTY = "for.empty.children";

    /// <summary>
    /// List of nodes blocks in the inherited template
    /// </summary>
    let NODELIST_EXTENDS_BLOCKS = "extends.blocks";
