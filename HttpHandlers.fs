namespace backend



module HttpHandlers =

    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.Logging
    open FSharp.Control.Tasks
    open Giraffe
    open DataAccess
    open UserTypes
    open backend.Models
   



    let handleAddUser (next: HttpFunc) (ctx: HttpContext) = 
        task {
            let! user = ctx.BindJsonAsync<User>()
            UserAccess.addUser user
            return! text(sprintf "Added %s to user_table." user.userEmail) next ctx
        }
    let handleGetUser (next: HttpFunc) (ctx: HttpContext) = 
        task {
            let! user = ctx.BindJsonAsync<LoginModel>()
            let result = UserAccess.getUser user
            return! json result next ctx
        }
    
    let handleAddTable (next: HttpFunc) (ctx: HttpContext)=
        task {
            let! table = ctx.BindJsonAsync<Table>()
            TableAccess.addTable table
            // let loggerB = ctx.GetLogger()
            // let string = sprintf "Caleb: %A" data.tableHeader
            // loggerB.LogInformation(string)
            return! text("Created table") next ctx
        }

    let handleAddTableData (next: HttpFunc) (ctx: HttpContext)=
        task {
            let! tableData = ctx.BindJsonAsync<TableData>()
            TableAccess.addTableData tableData
            // let loggerB = ctx.GetLogger()
            // let string = sprintf "Caleb: %A" data.tableHeader
            // loggerB.LogInformation(string)
            return! text("Inserted data") next ctx
        }

    let handleGetTableNames (next: HttpFunc) (ctx: HttpContext)=
        task{
            let! user = ctx.BindJsonAsync<User>()
            let tableNames = TableAccess.getTableNames user

            return! json tableNames next ctx
        }
    
