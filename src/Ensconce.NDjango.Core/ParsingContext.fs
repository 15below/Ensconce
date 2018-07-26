namespace Ensconce.NDjango.Core

module ParsingContext =

    /// Parsing context is a container for information specific to the tag being parsed
    type internal Implementation private(
                                            provider,
                                            resolver,
                                            parent,
                                            closures,
                                            is_in_header,
                                            model,
                                            _base) =

        new (provider, resolver, model)
            = new Implementation(provider, resolver, None, [], true, model, None)

        interface IParsingContext with

            member x.ChildOf = new Implementation(provider, resolver, Some (x :> IParsingContext), closures, is_in_header, model, _base) :> IParsingContext

            member x.BodyContext = new Implementation(provider, resolver, parent, closures, false, model, _base) :> IParsingContext

            member x.WithClosures(new_closures) = new Implementation(provider, resolver, parent, new_closures, is_in_header, model, _base) :> IParsingContext

            member x.WithNewModel(new_model) =
                new Implementation(provider, resolver, parent, closures, is_in_header, new_model, _base) :> IParsingContext

            member
                x.WithBase(new_base) =
                    new Implementation(provider, resolver, parent, closures, is_in_header, model, Some new_base) :> IParsingContext

            /// a list of all closing tags for the context
            member x.TagClosures = closures

           /// parent provider owning the context
            member x.Provider = provider

            /// a flag indicating if any of the non-header tags have been encountered yet
            member x.IsInHeader = is_in_header

            member x.Model = model

            member x.Resolver = resolver

            /// parent parsing context
            member x.Parent = parent

            /// a reference to parsed parent template
            member x.Base = _base


