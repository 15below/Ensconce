namespace Ensconce.NDjango.Core

open System
open System.Text
open System.Text.RegularExpressions
open System.Collections
open System.Collections.Generic
open System.Web

module internal Filters =

    type IEscapeFilter = interface end

    /// Escapes a string's HTML. Specifically, it makes these replacements:
    ///    * < is converted to &lt;
    ///    * > is converted to &gt;
    ///    * ' (single quote) is converted to &#39;
    ///    * " (double quote) is converted to &quot;
    ///    * & is converted to &amp;
    ///
    ///The escaping is only applied when the string is output, so it does not matter
    ///where in a chained sequence of filters you put escape: it will always be applied
    ///as though it were the last filter. If you want escaping to be applied immediately,
    ///use the force_escape filter.
    ///
    ///Applying escape to a variable that would normally have auto-escaping applied to the
    ///result will only result in one round of escaping being done. So it is safe to use
    ///this function even in auto-escaping environments. If you want multiple escaping
    ///passes to be applied, use the force_escape filter.
    ///
    ///Changed in Django 1.0: Due to auto-escaping, the behavior of this filter has changed
    ///slightly. The replacements are only made once, after all other filters are applied --
    ///including filters before and after it.
    type EscapeFilter() =
        interface IEscapeFilter
        interface ISimpleFilter with
            member x.Perform value = value

    /// Applies HTML escaping to a string (see the escape filter for details). This filter
    ///is applied immediately and returns a new, escaped string. This is useful in the rare
    ///cases where you need multiple escaping or want to apply other filters to the escaped
    ///results. Normally, you want to use the escape filter.
    type ForceEscapeFilter() =
        interface ISimpleFilter with
            member x.Perform value = Utilities.escape (Convert.ToString (value)):> obj

    /// Converts to lowercase, removes non-word characters (alphanumerics and underscores)
    ///and converts spaces to hyphens. Also strips leading and trailing whitespace.
    ///
    ///For example:
    ///{{ value|slugify }}
    ///If value is "Joel is a slug", the output will be "joel-is-a-slug".
    type Slugify() =
        let sl1Regex = Regex("[^\w\s-]",RegexOptions.Compiled)
        let sl2Regex = Regex("[-\s]+",RegexOptions.Compiled)
        interface ISimpleFilter with
            member x.Perform value =
                 let s = Convert.ToString (value) |> Encoding.ASCII.GetBytes |> Encoding.ASCII.GetString
                 let s = (s.Normalize NormalizationForm.FormKD).ToLower()
                 let s = sl1Regex.Replace(s, String.Empty)
                 let s = s.Trim()
                 sl2Regex.Replace(s, "-") :> obj

    /// Truncates a string after a certain number of words.
    ///
    ///Argument: Number of words to truncate after
    ///For example:
    ///{{ value|truncatewords:2 }}
    ///If value is "Joel is a slug", the output will be "Joel is ...".
    type TruncateWords() =
        let wsRegex = Regex("\S([,\.\s]+|\z)", RegexOptions.Compiled)
        interface IFilter with
            member x.DefaultValue = null
            member x.Perform value = raise (RenderingError("TruncateWords requires a parameter"))
            member x.PerformWithParam (value, arg) =
                let success, limit = Utilities.get_int arg
                if success then
                    let value = Convert.ToString (value)
                    let rec truncate limit start (words:Match seq) =
                        if Seq.isEmpty words then ""
                        elif limit = 0 then "..."
                        else
                            let m = words |> Seq.head
                            let s = value.[start..(m.Index + m.Length)-1]
                            s + truncate (limit-1) (m.Index + m.Length) (Seq.skip 1 words)
                    wsRegex.Matches value |> Seq.cast |> truncate limit 0 :> obj
                else
                    value

    /// Escapes a value for use in a URL.
    type UrlEncodeFilter() =
        interface ISimpleFilter with
            member x.Perform value =
                let strVal = value |> Convert.ToString |> HttpUtility.UrlEncode
                strVal :> obj

    /// function for Urlize and UrlizeTrunc
    let processUrlize (url:string) (trimCount) =
        let ProcessUrlizeString (trimlimit : int option) (str : string) =
            let LEADING_PUNCTUATION  = ["\\("; "<"; "&lt;"]
            let TRAILING_PUNCTUATION = ["\\."; ","; "\\)"; ">"; "\\n"; "&gt;"]
            let punctuation_re_input = Printf.sprintf "^(?<lead>(?:%s)*)(?<middle>.*?)(?<trail>(?:%s)*)$" (String.Join("|",List.toArray LEADING_PUNCTUATION) ) (String.Join("|", List.toArray TRAILING_PUNCTUATION) )
            let punctuation_re = new Regex ( punctuation_re_input , RegexOptions.ExplicitCapture  )
            let simple_email_re = new Regex (@"^\S+@[a-zA-Z0-9._-]+\.[a-zA-Z0-9._-]+$")

            let trim_url (url:string) (trimCount:int option) =
                if (trimCount.IsNone) then url
                    else
                        let trimCount = trimCount.Value
                        if (trimCount < url.Length)
                            then
                                if (trimCount >=3) then (Printf.sprintf "%s..." (url.Substring(0, (trimCount - 3))))
                                else "..."
                            else url

            let rec FindSubString (str : string) subsList =
                match subsList with
                    | (sub : string) :: tail -> if str.Contains sub then true else FindSubString str tail
                    | _ -> false

            let rec FindExtension (str: string) extList =
                match extList with
                    | (sub : string) :: tail -> if str.EndsWith sub then true else FindExtension str tail
                    | _ -> false

            if (FindSubString str ["."; "@"; ":"])
                then
                    let matchStr = punctuation_re.Match str
                    if not matchStr.Success
                        then str
                        else
                            let groups = matchStr.Groups
                            let lead = groups.["lead"].Value
                            let middle = groups.["middle"].Value
                            let trail = groups.["trail"].Value
                            let url =
                                if (middle.StartsWith "http://" || middle.StartsWith "https://")
                                    then middle
                                elif ((middle.StartsWith "www.") || (not (middle.Contains "@") && (FindExtension middle [".org"; ".net"; ".com"]) ))
                                    then sprintf "http://%s" middle
                                elif middle.Contains "@" && (not (middle.Contains ":")) && simple_email_re.IsMatch middle
                                    then sprintf "mailto:%s" middle
                                else String.Empty

                            if url = String.Empty
                                then str
                                else
                                    let trimmed = trim_url middle trimlimit
                                    let middle = sprintf "<a href=\"%s\">%s</a>" url trimmed
                                    sprintf "%s%s%s" lead middle trail
                else str

        let words_split_re = new Regex("(\\s+)")
        let words = words_split_re.Split(url)
        let wordsList = [for word in words do yield word] |> List.map (ProcessUrlizeString trimCount)
        String.Join (String.Empty,List.toArray wordsList)

    /// Converts URLs in plain text into clickable links.
    ///
    ///Note that if urlize is applied to text that already contains HTML markup, things won't work as expected. Apply this filter only to plain text.
    ///For example:
    ///{{ value|urlize }}
    ///If value is "Check out www.djangoproject.com", the output will be "Check out <a href="http://www.djangoproject.com">www.djangoproject.com</a>".
    type UrlizeFilter() =
        interface ISimpleFilter with
            member x.Perform value =
                let strVal = Convert.ToString (value)
                processUrlize strVal None :> obj

    /// Converts URLs into clickable links, truncating URLs longer than the given character limit.
    ///
    ///As with urlize, this filter should only be applied to plain text.
    ///Argument: Length to truncate URLs to
    ///For example:
    ///{{ value|urlizetrunc:15 }}
    ///If value is "Check out www.djangoproject.com", the output would be 'Check out <a href="http://www.djangoproject.com">www.djangopr...</a>'.
    type UrlizeTruncFilter() =
        interface IFilter with
            member x.DefaultValue = null
            member x.Perform value = raise (RenderingError("UrlizeTruncFilter requires a parameter"))
            member x.PerformWithParam (value, arg) =
                let strVal = Convert.ToString (value)
                let success, limit = Utilities.get_int arg
                if success then
                    processUrlize strVal (Some limit) :> obj
                else
                    processUrlize strVal None :> obj

    /// Wraps words at specified line length.
    ///
    ///Argument: number of characters at which to wrap the text
    ///For example:
    ///{{ value|wordwrap:5 }}
    ///If value is Joel is a slug, the output would be:
    ///
    ///Joel
    ///is a
    ///slug
    ///
    type WordWrapFilter() =
        let splitWordRe = new Regex (@"((?:\S+)|(?:[\f\t\v\x85\p{Z}]+)|(?:[\r\n]+))", RegexOptions.Compiled)
        let spacesRe = new Regex (@"(\A[\f\t\v\x85\p{Z}]+\z)", RegexOptions.Compiled)
        let crlfRe = new Regex (@"(\A[\r\n]+\z)", RegexOptions.Compiled)
        let ProcessNextString (outStr:string,wrapSize:int,charsAmount:int) (nextStr:string) =
            let nextSize = String.length nextStr
            let charsAmount, outStr =
                if charsAmount+nextSize > wrapSize then
                    match nextStr with
                    | _ when (spacesRe.IsMatch nextStr) ->
                        0,
                        (outStr.TrimEnd null) + "\r\n"
                    | _ when (crlfRe.IsMatch nextStr) ->
                        0,
                        (outStr.TrimEnd null) + "\r\n"
                    | _ ->
                        nextSize,
                        (outStr.TrimEnd null) + "\r\n" + nextStr
                else
                    match nextStr with
                    | _ when (crlfRe.IsMatch nextStr) ->
                        0,
                        (outStr.TrimEnd null) + nextStr
                    | _ ->
                        charsAmount + nextSize,
                        outStr + nextStr
            (outStr,wrapSize, charsAmount)

        interface IFilter with
            member x.DefaultValue = null
            member x.Perform value = raise (RenderingError("WordWrapFilter requires a parameter"))
            member x.PerformWithParam (value, arg) =
                let strVal = Convert.ToString (value)
                let success, limit = Utilities.get_int arg
                if success then
                    let splittedText = splitWordRe.Matches strVal
                    let resultWords =
                        [for wordMatch in splittedText do
                             yield wordMatch.Value
                        ]
                    let result, useless1, useless2 = List.fold ProcessNextString ("",limit,0) resultWords
                    result :> obj
                else
                    value

    /// Replaces line breaks in plain text with appropriate HTML; a single newline becomes an HTML line break (<br />) and a new line followed by a blank line becomes a paragraph break (</p>).
    ///
    ///For example:
    ///{{ value|linebreaks }}
    ///If value is Joel\nis a slug, the output will be <p>Joel<br />is a slug</p>.
    type LineBreaksFilter() =
        let replaceNRRe = new Regex (@"\r\n|\r|\n", RegexOptions.Compiled)
        let splitRegex = new Regex (@"(?:\r\n){2,}", RegexOptions.Compiled)
        interface ISimpleFilter with
            member x.Perform value =
                let paragraphs = replaceNRRe.Replace ((Convert.ToString value), "\r\n") |> splitRegex.Split
                Seq.fold (fun retLine (paragraph: string) -> retLine + "<p>" + paragraph.Replace("\r\n", "<br />") + "</p>") "" paragraphs :> obj

    /// Converts all newlines in a piece of plain text to HTML line breaks (<br />).
    type LineBreaksBrFilter() =
        let replaceNRRe = new Regex (@"\r\n|\r|\n", RegexOptions.Compiled)
        interface ISimpleFilter with
            member x.Perform value =
                replaceNRRe.Replace ((Convert.ToString (value)), @"<br />") :> obj

    /// Strips all [X]HTML tags.
    ///
    ///For example:
    ///{{ value|striptags }}
    ///If value is "<b>Joel</b> <button>is</button> a <span>slug</span>", the output will be "Joel is a slug".
    type StripTagsFilter() =
        let nextTagItemRegex = new Regex (@"</?[A-Za-z][A-Za-z0-9]*[^<>]*>", RegexOptions.Compiled)
        interface ISimpleFilter with
            member x.Perform value =
                nextTagItemRegex.Replace((Convert.ToString (value)), "") :> obj

    /// Joins a list with a string, like Python's str.join(list)
    ///
    ///For example:
    ///{{ value|join:" // " }}
    ///If value is the list ['a', 'b', 'c'], the output will be the string "a // b // c".
    type JoinFilter() =
        interface IFilter with
            member x.DefaultValue = null
            member x.Perform value = raise (RenderingError("JoinFilter requires a parameter"))
            member x.PerformWithParam (value, arg) =
                match value with
                    | :? IEnumerable ->
                        let strArr = Seq.map (fun (item: Object)  -> (Convert.ToString (item)) ) (Seq.cast (value :?> IEnumerable) ) |> Seq.toArray
                        String.Join (Convert.ToString arg, strArr) :> obj
                    | _ -> raise (new RenderingError("Type not supported"))

    /// Given a string mapping values for true, false and (optionally) None, returns one of those strings according to the value:
    ///
    ///Value 	Argument 	Outputs
    ///True 	"yeah,no,maybe" 	yeah
    ///False 	"yeah,no,maybe" 	no
    ///None 	"yeah,no,maybe" 	maybe
    ///None 	"yeah,no" 	"no" (converts None to False if no mapping for None is given)
    ///
    ///Note: Does NOT work for None at this time.
    type YesNoFilter() =
        interface IFilter with
            member x.DefaultValue = "yes,no,maybe" :> obj
            member x.Perform value = raise (RenderingError("YesNoFilter requires a parameter"))
            member x.PerformWithParam (value,arg) =
                let strYesNoMaybe = (Convert.ToString (arg)).Split([|','|]) |> List.ofArray
                if strYesNoMaybe.Length < 2 then
                    value
                else
                    let strYesNoMaybe =
                        if strYesNoMaybe.Length<3 then
                             (List.append strYesNoMaybe [strYesNoMaybe.[1]])
                        else
                            strYesNoMaybe

                    let retValue =
                        match value with
                            | :? Boolean when ((value :?> Boolean) = true) -> strYesNoMaybe.[0]
                            | :? (obj option) when ((value :?> (obj option)) = None) -> strYesNoMaybe.[2]
                            | _ -> strYesNoMaybe.[1]
                    retValue :> obj

    /// Takes a list of dictionaries and returns that list sorted by the key given in the argument.
    ///
    ///For example:
    ///
    ///{{ value|dictsort:"name" }}
    ///If value is:
    ///
    ///[
    ///    {'name': 'zed', 'age': 19},
    ///    {'name': 'amy', 'age': 22},
    ///    {'name': 'joe', 'age': 31},
    ///]
    ///
    ///then the output would be:
    ///
    ///[
    ///    {'name': 'amy', 'age': 22},
    ///    {'name': 'joe', 'age': 31},
    ///    {'name': 'zed', 'age': 19},
    ///]
    ///
    type DictSortFilter() =
        interface IFilter with
            member x.DefaultValue = null
            member x.Perform value = raise (RenderingError("DictSortFilter requires a parameter"))
            member x.PerformWithParam (value, arg) =
                let arg = Convert.ToString (arg)
                match value with
                    | :? IEnumerable ->
                        let value = (value :?> IEnumerable) |> Seq.cast
                        let value = Seq.sortBy (fun (elem: IDictionary<string,IComparable>) -> elem.[arg]) value
                        value :> obj
                    | _ ->
                        value

    /// Takes a list of dictionaries and returns that list sorted in reverse order by the key given
    ///in the argument. This works exactly the same as the above filter, but the returned value will be
    ///in reverse order.
    type DictSortReversedFilter() =
        interface IFilter with
            member x.DefaultValue = null
            member x.Perform value = raise (RenderingError("DictSortReversedFilter requires a parameter"))
            member x.PerformWithParam (value, arg) =
                let arg = Convert.ToString (arg)
                match value with
                    | :? IEnumerable ->
                        let value = (value :?> IEnumerable) |> Seq.cast
                        let value = Seq.sortBy (fun (elem: IDictionary<string,IComparable>) -> elem.[arg]) value
                        let value = Seq.toList value |> List.rev
                        value :> obj
                    | _ ->
                        value

    ///        Returns a plural suffix if the value is not 1. By default, 's' is used as
    ///    the suffix:
    ///
    ///    * If value is 0, vote{{ value|pluralize }} displays "0 votes".
    ///    * If value is 1, vote{{ value|pluralize }} displays "1 vote".
    ///    * If value is 2, vote{{ value|pluralize }} displays "2 votes".
    ///
    ///    If an argument is provided, that string is used instead:
    ///
    ///    * If value is 0, class{{ value|pluralize:"es" }} displays "0 classes".
    ///    * If value is 1, class{{ value|pluralize:"es" }} displays "1 class".
    ///    * If value is 2, class{{ value|pluralize:"es" }} displays "2 classes".
    ///
    ///    If the provided argument contains a comma, the text before the comma is
    ///    used for the singular case and the text after the comma is used for the
    ///    plural case:
    ///
    ///    * If value is 0, cand{{ value|pluralize:"y,ies" }} displays "0 candies".
    ///    * If value is 1, cand{{ value|pluralize:"y,ies" }} displays "1 candy".
    ///    * If value is 2, cand{{ value|pluralize:"y,ies" }} displays "2 candies".

    type PluralizeFilter() =
        interface IFilter with
            member x.DefaultValue = "s" :> obj
            member x.Perform value = raise (RenderingError("PluralizeFilter requires a parameter"))
            member x.PerformWithParam (value, arg) =
                let strPlurals = (Convert.ToString (arg)).Split([|','|]) |> List.ofArray
                if strPlurals.Length > 2 then
                    "" :> obj
                else
                    let multiPlural,singlePlural =
                        match strPlurals.Length with
                            | 2 -> strPlurals.[1],strPlurals.[0]
                            | _ -> strPlurals.[0],""

                    let success, intNum = Utilities.get_int value
                    let numVal =
                        match value with
                            | _ when success -> intNum
                            | :? IEnumerable -> Seq.cast (value :?> IEnumerable) |> Seq.length
                            | _ -> 1

                    if numVal = 1 then
                        singlePlural :> obj
                    else
                        multiPlural :> obj

    ///Converts a phone number with letters into its numeric equivalent.

    type Phone2numericFilter() =
        let convertMap =
            [('a', '2'); ('c', '2'); ('b', '2'); ('e', '3');
             ('d', '3'); ('g', '4'); ('f', '3'); ('i', '4'); ('h', '4'); ('k', '5');
             ('j', '5'); ('m', '6'); ('l', '5'); ('o', '6'); ('n', '6'); ('p', '7');
             ('s', '7'); ('r', '7'); ('u', '8'); ('t', '8'); ('w', '9'); ('v', '8');
             ('y', '9'); ('x', '9');
             ('A', '2'); ('C', '2'); ('B', '2'); ('E', '3');
             ('D', '3'); ('G', '4'); ('F', '3'); ('I', '4'); ('H', '4'); ('K', '5');
             ('J', '5'); ('M', '6'); ('L', '5'); ('O', '6'); ('N', '6'); ('P', '7');
             ('S', '7'); ('R', '7'); ('U', '8'); ('T', '8'); ('W', '9'); ('V', '8');
             ('Y', '9'); ('X', '9')
             ] |> Map.ofList
        interface ISimpleFilter with
            member x.Perform value =
                let getNumFromChar (inputChar:char) =
                    match inputChar with
                        | _ when convertMap.ContainsKey inputChar ->
                            convertMap.[inputChar]
                        | _ -> inputChar
                Convert.ToString(value) |> String.map getNumFromChar :>obj

    ///If value is None, use given default.
    type DefaultIfNoneFilter() =
        interface IFilter with
            member x.DefaultValue = null
            member x.Perform value = raise (RenderingError("DefaultIfNoneFilter requires a parameter"))
            member x.PerformWithParam (value, arg) =
                let objNull = None :> obj
                if (objNull = value) then
                    arg
                else
                    value

    ///Formats the value like a 'human-readable' file size (i.e. 13 KB, 4.1 MB,
    ///102 bytes, etc).

    type FileSizeFormatFilter() =
        interface ISimpleFilter with
            member x.Perform value =
                let success,dblSize = Utilities.get_double value
                if success then
                    match dblSize with
                        | _ when (dblSize < 1024.0) ->
                            let singleStr =
                                if ((Convert.ToInt32 dblSize)= 1) then ""
                                else "s"
                            let output = sprintf "%.0f byte%s" dblSize singleStr
                            output :> obj
                        | _ when (dblSize < 1024.0*1024.0) ->
                            let output = sprintf "%.1f KB" (dblSize/1024.0)
                            output :> obj
                        | _ when (dblSize < 1024.0*1024.0*1024.0) ->
                            let output = sprintf "%.1f MB" (dblSize/(1024.0*1024.0))
                            output :> obj
                        | _ ->
                            let output = sprintf "%.1f GB" (dblSize/(1024.0*1024.0*1024.0))
                            output :> obj
                else
                    "0 bytes" :> obj
