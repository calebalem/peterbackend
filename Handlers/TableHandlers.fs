namespace TableHandlers

module TableHandlers =
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks
    open Giraffe
    open TableModel
    open UserTypes
    open TableTypes

    let handleAddTable (next: HttpFunc) (ctx: HttpContext) =
        task {
            let! table = ctx.BindJsonAsync<Table>()
            TableModel.addTable table
            // let loggerB = ctx.GetLogger()
            // let string = sprintf "Caleb: %A" data.tableHeader
            // loggerB.LogInformation(string)
            return! text ("Created table") next ctx
        }

    let handleAddTableData (next: HttpFunc) (ctx: HttpContext) =
        task {
            let! tableData = ctx.BindJsonAsync<Table>()
            TableModel.addTableData tableData
            // let loggerB = ctx.GetLogger()
            // let string = sprintf "Caleb: %A" data.tableHeader
            // loggerB.LogInformation(string)
            return! text ("Inserted data") next ctx
        }

    let handleGetTableNames (next: HttpFunc) (ctx: HttpContext) =
        task {
            let! user = ctx.BindJsonAsync<User>()
            let tableNames = TableModel.getTableNames user

            return! json tableNames next ctx
        }

    let handleGetTableData (next : HttpFunc) (ctx: HttpContext) =
        task {
            let! user = ctx.BindJsonAsync<SimpleTypes.getTableData>()
            let tableData = TableModel.getTableData user
            return! json tableData next ctx 
        }
    let handleGetLog (next : HttpFunc) (ctx: HttpContext) =
        task {
            let! logInfo = ctx.BindJsonAsync<SimpleTypes.Log>()
            let log = TableModel.getLog logInfo
            printf "Json %A" (json log)
            return! json log next ctx 
        }
    
  