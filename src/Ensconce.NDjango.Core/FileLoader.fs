#light

namespace Django

open System.IO
open System.Collections.Generic

open Django.Template
open Filters
open DjangoParser

type FileLoader() =
    //interface ILoader with
    member this.GetTemplate source = 
        if not <| File.Exists(source) then
            raise (FileNotFoundException (sprintf "Could not locate template '%s'" source))
        else
            let template_src = File.ReadAllText(source)
            
            new DjangoTemplate.Impl(template_src, 
                (new Dictionary<string, ISimpleFilter>()), 
                (new Dictionary<string, ITag>()), 
                (new Map<string, obj>([]))) :> ITemplate
