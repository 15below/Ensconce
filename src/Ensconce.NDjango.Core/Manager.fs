#light

namespace Django

open System.IO

open Django.Interfaces
open DjangoParser

module Manager = 
    
    type DefaultLoader() =
        interface ITemplateLoader with
            member this.GetTemplate source = 
                if not <| File.Exists(source) then
                    raise (FileNotFoundException (sprintf "Could not locate template '%s'" source))
                else
                    new StreamReader(source) :> TextReader
    
    /// pointer to the most recent template manager. don't want to have an
    /// interface for the sake of an interface, so we'll make it obj to
    /// avoid dealing with circular references
    let (active: obj option ref) = ref None
    

    type TemplateManager (filters: Map<string, ISimpleFilter>, tags: Map<string, ITag>, templates: Map<string, ITemplate>, loader: ITemplateLoader) =
        let filters = filters
        let tags = tags
        let templates = templates
        let loader = loader
        
        member private this.Templates = templates
        member private this.Filters = filters
        member private this.Tags = tags
        member private this.Loader = loader
        
        /// Creates a new filter manager with the filter registered 
        static member RegisterFilter name filter (mgr: TemplateManager) =
            let new_filters = Map.add name filter mgr.Filters
            new TemplateManager(new_filters, mgr.Tags, mgr.Templates, mgr.Loader)
        
        /// Creates a new filter manager with the tag registered 
        static member RegisterTag name tag (mgr: TemplateManager) =
            let new_tags = Map.add name tag mgr.Tags
            new TemplateManager(mgr.Filters, new_tags, mgr.Templates, mgr.Loader)
        
        /// Creates a new filter manager with the tempalte registered 
        static member RegisterTemplate name template (mgr: TemplateManager) =
            let new_templates = Map.add name template mgr.Templates
            new TemplateManager(mgr.Filters, mgr.Tags, new_templates, mgr.Loader)
            
        /// Retrieves the current active template manager
        static member GetActiveManager = 
            lock (active) ( 
                fun () -> 
                    match !active with 
                    | Some v -> Some (v :?> TemplateManager) 
                    | None -> 
                        Some (new TemplateManager(
                                Map<string, ISimpleFilter>([]),
                                Map<string, ITag>([]),
                                Map<string, ITemplate>([]),
                                new DefaultLoader())) 
                    )
            
        /// Retrieves a compiled instance of the template, along with
        /// the instance of the template manager that contains the template
        member this.GetTemplate full_name =
            match Map.tryfind full_name templates with
            | Some t -> t, this
            | None -> 
                lock (typeof<TemplateManager>) (fun () -> 
                    let mgr = (!active).Value :?> TemplateManager
                    match Map.tryfind full_name mgr.Templates with
                    | Some t -> t, mgr
                    | None ->
                        let template = new Django.Template.Impl(loader.GetTemplate full_name, this)
                        let new_mgr = TemplateManager.RegisterTemplate full_name template mgr
                        active := Some (new_mgr :> obj)
                        template, new_mgr)
    
