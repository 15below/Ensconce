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


namespace NDjango.Tags
open System
open System.Text.RegularExpressions

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.ParserNodes
open NDjango.ASTNodes
open NDjango.Variables
open NDjango.Expressions
open NDjango.ParserNodes
open NDjango.TypeResolver

module Model =

    /// Provides the name of the model class.
    ///
    /// this name is used by the designer to provide the code completion support;
    ///
    [<Description("Provides the name of the model class to be used with the template")>]
    type internal Tag() =
        interface ITag with
            member x.is_header_tag = true
            member this.Perform token context tokens =
                let get_model_type type_name = 
                    System.AppDomain.CurrentDomain.GetAssemblies() 
                    |> Seq.tryPick 
                        (fun assembly -> 
                            match assembly.GetType(type_name) with
                            | null -> None
                            | _ as t -> Some t
                            )

                let model_regex =
                    new Regex("(?<pair>^\s*(?<model>\w+):(?<type>\w+(.\w+<*)*>*)\s*$)")

                let rec parse_args (args: TextToken list) =
                    match args with
                    | arg :: remainder ->
                        let matches = model_regex.Match(arg.RawText)
                        let pair = matches.Groups.["pair"]
                        if pair.Success && pair.Index = 0 && pair.Length = arg.RawText.Length
                        then
                            let models, remainder = parse_args remainder
                            (
                                (matches.Groups.["model"].Value, 
                                    arg.CreateToken(matches.Groups.["type"].Captures.[0])
                            ) :: models, remainder)
                        else
                            raise (SyntaxError("malformed arguments in the model tag"))
                    | [] -> ([], [])

                let model, _ = parse_args token.Args
                let model_type = 
                    match model |> List.tryPick (fun mi -> if mi |> fst = "Model" then Some (snd mi) else None) with
                    | Some token -> get_model_type token.RawText
                    | None -> None
                (
                    {new TagNode(context, token, this) with
                        override x.elements =
                            model 
                            |> List.map (fun item -> TypeNameNode(context, Text (snd item)) :> INode) 
                            |> List.append base.elements 

                        override x.walk manager walker = 
                            match model_type with
                            | Some model -> 
                                {walker with context = walker.context.WithModelType(model)}
                            | _ -> walker
                    }
                    :> INodeImpl), context.WithNewModel(context.Model.Add(context.Resolver, model)), tokens



