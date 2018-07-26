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
open System.Text
open System.Text.RegularExpressions
open System.Collections
open NDjango.Lexer
open NDjango.Interfaces
open NDjango.ParserNodes

module internal Debug =
        
    /// Produces debug information
    type TagNode(parsing_context: ParsingContext, t: BlockToken) =
        inherit NDjango.ParserNodes.TagNode(parsing_context, t)

        override this.walk manager walker = 
            {walker with buffer = walker.context.ToString()}
     
                    

