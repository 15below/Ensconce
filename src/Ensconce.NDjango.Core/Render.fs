#light

/// encapsulates progress through the rendering operation
type RenderState<'a> =
    | Success of string * 'a option
    | Failure of string
    
let start a = Success(System.String.Empty, a)

let always a c = Success(a, c)

type RenderBuilder() =
    member this.Bind(p: RenderState<'a>, rest: 'a -> RenderState<'b>) =
        match p with 
        | Success (text, ctx) ->
            match rest ctx.Value with
            | Success (new_text, inner_ctx) -> Success (text + new_text, inner_ctx)
            | Failure text -> Failure text
        | Failure text -> Failure text
    
    member this.Return(a) = always System.String.Empty a
    
    //member this.Delay (f) = (this.Return None) f
    
    //member this.Let(p, rest) = this.Bind((this.Return p), rest)

    member this.Combine(a,b) = this.Bind(a, fun p -> b)
    
    member this.Zero() = start None
    

let public render = new RenderBuilder()
