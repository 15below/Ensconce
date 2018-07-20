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

open System
open System.Collections
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions
open Microsoft.FSharp.Collections

module Lexer =

    /// <summary>
    /// This esception is thrown if a problem encountered while lexing the template
    /// Lexer errors never leave the lexer, they are caught and converted into Error tokens
    /// </summary>
    type LexerError (message) =
        inherit System.ApplicationException(message)

    /// <summary>
    /// Object describing the token location in the original source string
    /// </summary>
    type Location(offset:int, length:int, line:int, position:int) =

        public new(parent:Location, (offset:int, length:int)) =
            Location(parent.Offset + offset, length, parent.Line, parent.Position + offset)

        /// <summary>
        /// 0 based offset of the text from the begining of the source file
        /// </summary>
        member x.Offset = offset

        /// <summary>
        /// Length of the text fragment from which the token was generated
        /// may or may not match the length of the actual text
        /// </summary>
        member x.Length = length

        /// <summary>
        /// 0 based line number
        /// </summary>
        member x.Line = line

        /// <summary>
        /// 0 based position of the starting position of the text within the line it belongs to
        /// </summary>
        member x.Position = position

    /// <summary>
    /// Provides mapping from the spans of the actual text to the spans of the original text
    /// </summary>
    /// <remarks>
    /// Mapping is only provided for the purposes of diagnostic to accomodate the situations where
    /// some of the text is coming from a template source, while some of it is generated, i.e. for filter
    /// tag the name of the variable is generated, while filters are coming directly from the template
    /// The constructor takes a list of mapping spans, each defined by its length and a boolean set to true
    /// if this span originated from the original string and to false otherwise
    /// offset adjustment is calculated as a sum of lengths of all non-mapped mapping spans with start offset
    /// less or equal to the offset of the span being mapped
    /// mapped length is caluclated as a sum of all intervals of the span being mapped which fall on the
    /// mapped mapping spans
    /// there should be no adjacent mapping spans with the same value of 'mapped'. Also mapping a span into
    /// more than one original span is not supported
    /// </remarks>
    type private SpanMap (map :(int*bool) list) =
        member x.Map (position, length) =
            let _, offset, mapped_length, new_map =
                map |> List.fold
                    (fun (index, mapped_position, mapped_length, new_map) (span_length, mapped) ->
                        if index + span_length <= position
                        // process the mapping span to the left of the start of the span being mapped
                        then
                            if mapped
                            then index + span_length, mapped_position, mapped_length, new_map
                            else index + span_length, mapped_position - span_length, mapped_length, new_map
                        elif index <= position
                        // if the offset falls on the crack, it belongs to the span to the right
                        then
                            // calculate the length of the ovelap between the mapping span and the span being mapped
                            let interval_length =
                                if index + span_length < position + length
                                then index + span_length - position
                                else length
                            // process the mapping span where the start of the span being mapped is located
                            if mapped
                            then index + span_length, mapped_position, mapped_length, (interval_length, true) :: new_map
                            // adjust offset to push the span start to the beginning of the next span
                            else
                                index + span_length, mapped_position - (span_length - interval_length),
                                if position + length > index + span_length
                                then mapped_length - interval_length
                                else 0 // mapped_length can be less than interval_length, but the length cannot be negative
                                , (interval_length, false) :: new_map
                        elif index + span_length < position + length
                        then
                            // process the mapping span where the end of the span being mapped is located
                            // it is ok to calculate interval_length because span being mapped cannot span more then 2 mapping spans
                            let interval_length = position - index + length
                            if mapped
                            then index + span_length, mapped_position, mapped_length, (interval_length, true) :: new_map
                            else index + span_length, mapped_position, position + length - index, (interval_length, false) :: new_map
                        // this is the final value for the offset - further spans have no effect
                        else index, mapped_position, mapped_length, new_map
                    ) (0,position,length, [])

            (offset, mapped_length), List.rev new_map

    /// <summary>
    /// Base class for text tokens.
    /// </summary>
    /// <remarks>Its main purpose is to keep track of bits of text
    /// participating in the template parsing in their relationship with the original
    /// location of the text in the template. As the text is going through various
    /// stages of parsing it may or may not differ from the original text in the source
    /// </remarks>
    type TextToken(text:string, value:string, location: Location, map:(int*bool) list option) =
        let map =
            match map with
            | Some mapping -> Some (new SpanMap(mapping))
            | None -> None

        /// <summary>
        /// Builds a location object based on a regexp match
        /// </summary>
        let location_ofMatch (m:Match) = new Location(location, (m.Groups.[0].Index, m.Groups.[0].Length))

        new (text:string, location: Location, ?map) =
            new TextToken(text, text, location, map)

        /// <summary>
        /// The original, unmodified text as it is in the source
        /// </summary>
        member x.RawText = text

        /// <summary>
        /// The value after modifications
        /// </summary>
        member x.Value = value

        /// <summary>
        /// Token location
        /// </summary>
        member x.Location = location

        /// <summary>
        /// Creates a new token from the existing one
        /// Use this method when you need to create a new token from a part of the text of an existing one
        /// </summary>
        member x.CreateToken (capture:Capture) = x.CreateToken (capture.Index, capture.Length)

        /// <summary>
        /// Creates a new token from the existing one
        /// Use this method when you need to create a new token from a part of the text of an existing one
        /// </summary>
        member x.CreateToken (offset, length) =
                match map with
                | Some m ->
                    let mapped_location, new_map =  m.Map (offset, length)
                    new TextToken(value.Substring(offset, length), new Location(x.Location, mapped_location), new_map)
                | None ->
                    new TextToken(value.Substring(offset, length), new Location(x.Location, (offset, length)))

        /// <summary>
        /// Creates a new token bound to the same location in the source, but with a different value
        /// Use this method when you need to modify the token value but keep its binding to the source
        /// </summary>
        member x.WithValue new_value map = new TextToken(text, new_value, location, map)

        /// <summary>
        /// Creates a list of new tokens by applying a regular expression to the text of the existing one
        /// </summary>
        member x.Tokenize (regex:Regex) =
            [for m in regex.Matches(text) -> new TextToken(m.Groups.[0].Value, location_ofMatch m)]

    /// <summary>
    /// generates a list of tokens by applying the regexp
    /// </summary>
    /// <remarks>
    /// Compare this function with the Tokenize method. The do the same thing. The reason I have both is that
    /// the method is better from the encapsulation standpoint, but I could not find a way to call it from
    /// within the BlockToken constructor - and it is needed there. If you find a way around this - kill
    /// the function
    /// </remarks>
    let private tokenize_for_token location (regex:Regex) value =
            let location_ofMatch (m:Match) = new Location(location, (m.Groups.[0].Index, m.Groups.[0].Length))
            [for m in regex.Matches(value) -> new TextToken(m.Groups.[0].Value, location_ofMatch m)]

    /// <summary>
    /// Determines the text of the tag body by stripping off
    /// the first two and the last two characters
    /// </summary>
    let block_body (text:string) (location:Location) =
        let body = text.[2..text.Length-3].Trim()
        let text = text.[2..]
        body, new Location(location, (2 + text.IndexOf body, body.Length))

    /// <summary>
    /// RegExp to locate the tag fragments: the tag name and tag arguments by splitting the text into pieces by whitespaces
    /// Whitespaces inside strings in double- or single quotes remain unaffected
    /// </summary>
    let private split_tag_re = new Regex(@"(""(?:[^""\\]*(?:\\.[^""\\]*)*)""|'(?:[^'\\]*(?:\\.[^'\\]*)*)'|[^\s]+)", RegexOptions.Compiled)

    /// <summary>
    /// Represents a token for a django tag block
    /// </summary>
    type BlockToken(text, location) =
        inherit TextToken(text, location)

        // parse the token body to extract the verb (tag name) and the tag arguments
        let verb,args =
            let body, location = block_body text location
            match tokenize_for_token location split_tag_re body with
            | [] -> raise (LexerError("Empty tag block"))
            | verb::args -> verb, args

        /// <summary>
        /// A.K.A. tag name
        /// </summary>
        member x.Verb = verb

        /// <summary>
        /// List of arguments - can be empty
        /// </summary>
        member x.Args = args

    /// <summary>
    /// Represents a syntax error in the syntax node tree. These tokens are generated in response
    /// to exceptions thrown during lexing of the template, so that the actual exception throwing can be delayed
    /// till parsing stage
    /// </summary>
    type ErrorToken(text, error:string, location) =
        inherit TextToken(text, location)

        /// <summary>
        /// Error message taken from the exception
        /// </summary>
        member x.ErrorMessage = error

    /// <summary>
    /// Represents a token for a variable django block
    /// </summary>
    type VariableToken(text:string, location) =
        inherit TextToken(text, location)

        let expression =
            let body, body_location = block_body text location
            if (body = "")
                then new TextToken(text.[2..text.Length-3], new Location(location, (2, text.Length-4)))
                else new TextToken(body, body_location)

        /// <summary>
        /// token representing the expression
        /// </summary>
        member x.Expression = expression

    /// <summary>
    /// Represents a token for a django comment tag
    /// </summary>
    type CommentToken(text, location) =
        inherit TextToken(text, location)

    /// <summary>
    /// A generic lexer token produced through the tokenize function
    /// </summary>
    type Token =
        | Block of BlockToken
        | Variable of VariableToken
        | Comment of CommentToken
        | Error of ErrorToken
        | Text of TextToken

        member x.TextToken =
            match x with
            | Block b -> b :> TextToken
            | Error e -> e :> TextToken
            | Variable v -> v :> TextToken
            | Comment c -> c :> TextToken
            | Text t -> t

        member x.Position = x.TextToken.Location.Offset
        member x.Length = x.TextToken.Location.Length

        /// <summary>
        /// provides additional diagnostic information for the token
        /// </summary>
        member x.DiagInfo =
            sprintf " in token: \"%s\" at line %d pos %d "
                x.TextToken.RawText x.TextToken.Location.Line x.TextToken.Location.Position

    /// <summary>
    /// Active Pattern matching the Token to a string constant. Uses the Token.RawText to do the match
    /// </summary>
    let (|MatchToken|) (t:TextToken) = t.RawText

    /// <summary> generates sequence of tokens out of template TextReader </summary>
    /// <remarks>the type implements the IEnumerable interface (sequence) of templates
    /// It reads the template text from the text reader one buffer at a time and
    /// returns tokens in batches - a batch is a sequence of the tokens generated
    /// off the content of the buffer </remarks>
    type private Tokenizer (template:TextReader) =
        let mutable current: Token list = []
        let mutable line = 0
        let mutable pos = 0
        let mutable linePos = 0
        let mutable tail = ""
        let buffer = Array.create 4096 ' '

        /// <summary>
        /// converts supplied string into an appropriate token. It is assumed that
        /// the strings representing django tags in the template are identified
        /// before calling of this function.
        /// Every string passed to this function will be converted into a single
        /// token based on the 1st two characters of the string.
        /// </summary>
        let create_token in_tag (text:string) =
            in_tag := not !in_tag
            let currentPos = pos
            let currentLine = line
            let currentLinePos = linePos
            Seq.iter
                (fun ch ->
                    pos <- pos + 1
                    if ch = '\n' then
                        line <- line + 1
                        linePos <- 0
                    else linePos <- linePos + 1
                    )
                text
            let location = new Location(currentPos, text.Length, currentLine, currentLinePos)
            if not !in_tag then
                Text(new TextToken(text, location))
            else
                try
                    match text.[0..1] with
                    | "{{" -> Variable (new VariableToken(text, location))
                    | "{%" -> Block (new BlockToken(text, location))
                    | "{#" -> Comment (new CommentToken(text, location))
                    | _ -> Text (new TextToken(text, location))
                with
                | :? LexerError as ex ->
                    Error (new ErrorToken(text, ex.Message, location))
                | _ -> reraise()

        /// <summary>
        ///
        /// </summary>
        interface IEnumerator<Token seq> with
            member this.Current = Seq.ofList current

        interface IEnumerator with
            member this.Current = Seq.ofList current :> obj

            member this.MoveNext() =
                match tail with
                | null ->
                    false
                | _ ->
                    let count = template.ReadBlock(buffer, 0, buffer.Length)
                    // Split the tail of the old buffer appended by the newly read buffer
                    // into django tags
                    let strings = (Constants.tag_re.Split(tail + new String(buffer, 0, count)))
                    let t, strings =
                        if (count > 0) then strings.[strings.Length-1], strings.[0..strings.Length - 2]
                        else null, strings
                    // Keep the tail for the next pass
                    tail <- t
                    let in_tag = ref true
                    // convert strings to tokens
                    current <- strings |> List.ofArray |> List.map (create_token in_tag)
                    true

            /// we cannot reset the enumerator, because resetting the template text reader
            /// may or may not be possible. We are lucky it is not necessary
            member this.Reset() = failwith "Reset is not supported by Tokenizer"

        interface IEnumerable<Token seq> with
            member this.GetEnumerator():IEnumerator<Token seq> =
                 this :> IEnumerator<Token seq>

        interface IEnumerable with
            member this.GetEnumerator():IEnumerator =
                this :> IEnumerator

        interface IDisposable with
            member this.Dispose() = ()

    /// <summary>
    /// Produces a sequence of token objects based on the template text
    /// </summary>
    let internal tokenize (template:TextReader) =
        LazyList.ofSeq <| Seq.fold (fun (s:Token seq) (item:Token seq) -> Seq.append s item) (seq []) (new Tokenizer(template))
