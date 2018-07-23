namespace Ensconce.NDjango

module internal ASTWalker =

    /// Implements a TextReader to walk the template. An instance of this class is returned when
    /// TemplateManager.Render method is called (see the Template.fs)
    type Reader(manager:ITemplateManager, walker: Walker) =
        inherit System.IO.TextReader()

        /// current walker
        let mutable walker = walker
        let buffer = Array.create 4096 ' '

        /// Retrieves current character from the walker buffer, getting as necessary new walkers
        /// from the nodes
        let rec getChar() =
            if walker.bufferIndex >= walker.buffer.Length
            then
                // processing of the buffer in the current walker is completed - get a new one
                // and then get our character from the new buffer by recursive call to itself
                match walker.nodes with
                | [] ->
                    // we are done with the nodes of the current walker - pop the parent from the stack
                    match walker.parent with
                    | Some w ->
                        // reset the walker to the parent
                        walker <- w
                        // call itself on the parent
                        getChar()
                    | None ->
                        // we are done - nothing more to walk
                        -1
                | node :: nodes ->
                    try
                        // get a new walker from the node at the head of the node list
                        // and advance the list to the next node
//                        if not <| node.GetType().Name.Equals("ParsingContextNode") then
//                            System.Diagnostics.Debug.WriteLine ("walking " + node.Token.DiagInfo )
                        walker <- node.walk manager {walker with nodes = nodes; buffer=""; bufferIndex = 0}
                    with
                        // intercept rendering errors and reraise them with additional diagnostic info
                        | :? RenderingError as r ->
                            raise (new RenderingException(r.Message, node.Token, r.InnerException))
                        | _ -> reraise()
                    // call itself on the new walker
                    getChar()
            else
                // get a character from the current buffer
                int walker.buffer.[walker.bufferIndex]

        let read (buffer: char[]) index count =
            let mutable transferred = 0;
            while getChar() <> -1 && transferred < count do
                let mutable index = walker.bufferIndex
                while index < walker.buffer.Length && transferred < count do
                    buffer.[transferred] <- walker.buffer.[index]
                    transferred <- transferred+1
                    index <- index+1
                walker <- {walker with bufferIndex = index}
            transferred

        let rec read_to_end (buffers:System.Text.StringBuilder) =
            match read buffer 0 buffer.Length with
            | 0 -> buffers
            | _ as l ->
                if l = buffer.Length then
                    buffers.Append(new System.String(buffer)) |> ignore
                    read_to_end buffers
                else
                    buffers.Append(new System.String(buffer), 0, l) |> ignore
                    read_to_end buffers

        override this.Peek() = getChar()

        override this.Read() =
            let result = getChar()
            if result <> -1 then
                walker <- {walker with bufferIndex = walker.bufferIndex+1}
            result

        override this.Read(buffer: char[], index: int, count: int) = read buffer index count

        override this.ReadToEnd() = read_to_end (new System.Text.StringBuilder()) |> string
