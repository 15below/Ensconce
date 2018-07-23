namespace Ensconce.NDjango

open System.Text.RegularExpressions
open System.Globalization
open System

[<AutoOpen>]
module public Utilities =

    /// valid format of an int
    let private int_re = new Regex(@"^-?\d+$", RegexOptions.Compiled)

    /// valid format of a float
    let private float_re = new Regex(@"^-?\d+(?:\.\d+)?$", RegexOptions.Compiled)

    /// determines whether text is a valid integer
    let private is_int text = int_re.IsMatch(text)

    /// determines whether text is a valid float
    let private is_float text = float_re.IsMatch(text)

    /// Matches valid integers. if a match, returns Some int, otherwise returns None
    let internal (|Int|_|) text = if is_int text then Some (int text) else None

    /// Matches valid floats. if a match, returns Some float, otherwise returns None
    let internal (|Float|_|) text = if is_float text then Some (float text) else None

    /// <summary>
    /// determines whether the given string is either single or double quoted, escaped or un-escaped.
    /// returnes the string tripped off the quotes if it is ot None if it is not
    /// </summary>
    let internal (|String|_|) (text:string) =
        if text.Length < 2 then None
        else
            match text.[0] with
            | '\\' when ("\"'".Contains(string text.[1]) && text.[..1] = text.[text.Length-2..]) -> Some text.[2..text.Length-3]
            | '\'' | '"' when text.[0] = text.[text.Length-1] -> Some text.[1..text.Length-2]
            | _ -> None

    /// <summary>
    /// determines whether the given string is marked as requiring translation
    /// if it is returns the string stripped off the markers, otherwise returns None
    /// </summary>
    let (|IsI18N|_|) (text:string) =
        if text.Length < 3 then None
        else
            match text.[..1] with
            | "_(" -> Some text.[2..text.Length-2]
            | _ -> None

    /// Matches keys that are contained in the dictionary. If a match, returns Some dict[key], otherwise returns none
    let internal (|Contains|_|) key (dict: System.Collections.IDictionary) =
        match key with
        | Some v when dict.Contains(v) -> Some dict.[v]
        | _ -> None

    // method to convert input value to Int32
    let get_int (inputVal:obj) =
        let str = Convert.ToString inputVal
        let success,dblOut = Double.TryParse (str,NumberStyles.Any,CultureInfo.CurrentCulture)
        let inRange = ((Int32.MinValue |> Convert.ToDouble) < dblOut && dblOut < (Int32.MaxValue |> Convert.ToDouble))
        if (success && inRange) then
            true,(dblOut |> Convert.ToInt32)
        else
            false, 0

    // method to convert input value to Double
    let get_double (inputVal:obj) =
        let str = Convert.ToString inputVal
        Double.TryParse (str,NumberStyles.Any,CultureInfo.CurrentCulture)

    /// <summary>
    /// escapes html sensitive elements from the string
    /// </summary>
    let internal escape text =
        (string text).Replace("&","&amp;").Replace("<","&lt;").Replace(">","&gt;").Replace("'","&#39;").Replace("\"","&quot;")
