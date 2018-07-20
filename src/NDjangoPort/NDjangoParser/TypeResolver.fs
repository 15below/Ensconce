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

open NDjango.Lexer
open System.Reflection

module TypeResolver =
   
    type DjangoType =
        | DjangoType
        | CLRType
        | DjangoValue

    type IDjangoType =
        abstract member Name : string
        abstract member Type : DjangoType
        abstract member Members : IDjangoType seq
        abstract member IsList : bool
        abstract member IsDictionary : bool
    
    type ITypeResolver =
        abstract member Resolve: type_name: string -> System.Type
        
    /// Django type enacpsulating a value i.e. values in the loop descriptor in For tag 
    type ValueDjangoType(name) =
        interface IDjangoType with
            member x.Name = name
            member x.Type = DjangoType.DjangoValue
            member x.Members = Seq.empty
            member x.IsList = false
            member x.IsDictionary = false

    /// Django type encapsulating a member of a certain runtime type
    type CLRTypeDjangoType(name, _type) =
        
        let resolve (_type:System.Type) =

            let build_descriptor name _type mbrs =
                let result = CLRTypeDjangoType(name, _type) :> IDjangoType
                [result] |> List.toSeq |> Seq.append mbrs


            let validate_method (_method:MethodInfo) = 
                if _method.ContainsGenericParameters then false
                else if _method.IsGenericMethodDefinition then false
                else if _method.IsGenericMethod then false
                else if _method.IsConstructor then false
                else if _method.GetParameters().Length > 0 then false
                else if _method.ReturnType = null then false
                else if _type.GetProperties() |> Seq.exists (fun prop -> prop.GetGetMethod() = _method)
                    then false
                else true
                    

            if _type = null then Seq.empty
            else
                _type.GetMembers() |>
                Array.toSeq |>
                Seq.fold 
                    (fun mbrs mbr ->
                        match mbr.MemberType with
                        | MemberTypes.Field -> build_descriptor mbr.Name (mbr :?> FieldInfo).FieldType mbrs
                        | MemberTypes.Property -> build_descriptor mbr.Name (mbr :?> PropertyInfo).PropertyType mbrs
                        | MemberTypes.Method when validate_method (mbr :?> MethodInfo) -> 
                            build_descriptor mbr.Name (mbr :?> MethodInfo).ReturnType mbrs
                        | _ -> mbrs
                        ) 
                    Seq.empty

        interface IDjangoType with
            member x.Name = name
            member x.Type = DjangoType.CLRType
            member x.Members = resolve _type
            member x.IsList = 
                if _type = null then false
                else typeof<System.Collections.IEnumerable>.IsAssignableFrom(_type)
            member x.IsDictionary = 
                if _type = null then false
                else typeof<System.Collections.IDictionary>.IsAssignableFrom(_type)

    type ModelDescriptor( members: seq<IDjangoType>) =

        member internal x.Add(resolver: ITypeResolver, model_members: (string*TextToken) list) =
            x.Add(model_members
                    |> Seq.map 
                        (fun item ->
                            (CLRTypeDjangoType(fst item, resolver.Resolve((snd item).RawText)) :> IDjangoType)
                        )
            )

        member internal x.Add(model_members: seq<IDjangoType>) =
            new ModelDescriptor(
                model_members |> Seq.append 
             (members |> Seq.filter (fun mbr -> model_members |> Seq.exists (fun model_mbr -> mbr.Name = model_mbr.Name) |> not))
            )

        member x.Members = members

        interface IDjangoType with
            member x.Name = null
            member x.Type = DjangoType.DjangoType
            member x.Members = members
            member x.IsList = true
            member x.IsDictionary = true

    type internal DefaultTypeResolver() =
        interface ITypeResolver with
            member x.Resolve type_name = null
            

