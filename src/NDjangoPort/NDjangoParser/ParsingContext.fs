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

namespace NDjango.Interfaces

open NDjango.Interfaces
open NDjango.TypeResolver

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
    

