namespace UserHandlers

module UserHandlers = 
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks
    open Giraffe
    open UserModel
    open UserTypes
    open SimpleTypes
    
    let handleAddUser (next: HttpFunc) (ctx: HttpContext) = 
        task {
            let! user = ctx.BindJsonAsync<User>()
            UserModel.addUser user
            return! text(sprintf "Added %s to user_table." user.userEmail) next ctx
        }
    let handleGetUser (next: HttpFunc) (ctx: HttpContext) = 
        task {
            let! user = ctx.BindJsonAsync<LoginModel>()
            let result = UserModel.getUser user
            return! json result next ctx
        }
