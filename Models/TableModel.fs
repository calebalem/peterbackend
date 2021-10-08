namespace TableModel

module TableModel =
    open Npgsql.FSharp
    open TableTypes
    open UserTypes
    open SimpleTypes
    open SqlConnection

    let private listToStringArray (list :list<string>) = 
        let stringArray = [|for i in list -> i|]
        stringArray

    let private dynamiclyCreateTableQuery (table: Table) =
        let queryStart =
            sprintf
                "CREATE TABLE %s ( row_id serial PRIMARY KEY"
                (table.tableName + table.userId.ToString())

        let mutable dynamicQuery = ""

        for i in 0 .. table.tableHeaders.Length - 1 do
            dynamicQuery <-
                dynamicQuery
                + sprintf ",%s  %s" (table.tableHeaders.Item(i)) (table.tableDataTypes.Item(i))

        let queryEnd = ");"
        let finalQuery = queryStart + dynamicQuery + queryEnd
        finalQuery

    let private dynamiclyInsertTableDataQuery (tableData: TableData) =
        let insertQuery =
            sprintf "INSERT INTO %s (" (tableData.tableName + tableData.userId.ToString())

        let mutable columnsQuery = ""

        for tableHeader in 0 .. tableData.tableHeaders.Length - 1 do
            if tableHeader = tableData.tableHeaders.Length - 1 then
                columnsQuery <-
                    columnsQuery
                    + sprintf "%s) VALUES " (tableData.tableHeaders.Item(tableHeader))
            else
                columnsQuery <-
                    columnsQuery
                    + sprintf "%s, " (tableData.tableHeaders.Item(tableHeader))


        let mutable valuesQuery = ""

        for i in 0 .. tableData.tableDatas.Length - 1 do
            valuesQuery <- valuesQuery + "("

            for j in 0 .. tableData.tableDatas.Item(i).Length - 1 do
                if j = tableData.tableDatas.Item(i).Length - 1 then
                    valuesQuery <-
                        valuesQuery
                        + sprintf "%s)" (tableData.tableDatas.Item(i).Item(j).ToString())
                else
                    valuesQuery <-
                        valuesQuery
                        + sprintf "%s, " (tableData.tableDatas.Item(i).Item(j).ToString())

            if i = tableData.tableDatas.Length - 1 then
                valuesQuery <- valuesQuery + ";"
            else
                valuesQuery <- valuesQuery + ","

        let finalQuery = insertQuery + columnsQuery + valuesQuery
        finalQuery


    let addTable (table: Table) =
        let query = dynamiclyCreateTableQuery table
        printfn "%s" query

        SqlConnection.connectionString
        |> Sql.connect
        |> Sql.query query
        |> Sql.executeNonQuery

        SqlConnection.connectionString
        |> Sql.connect
        |> Sql.query "INSERT INTO user_table_map(user_id, table_name, tableHeaders) VALUES (@user_id,@table_name,@table_headers)"
        |> Sql.parameters [ "user_id", Sql.int table.userId;
                            "table_name", Sql.string table.tableName;
                            "table_headers", Sql.stringArray (listToStringArray(table.tableHeaders)) ]
        |> Sql.executeNonQuery

    let addTableData (tableData: TableData) =
        let query = dynamiclyInsertTableDataQuery tableData
        printfn "%s" query

        SqlConnection.connectionString
        |> Sql.connect
        |> Sql.query query
        |> Sql.executeNonQuery



    let getTableNames (user: User) =
        let query =
            "SELECT table_name from user_table_map WHERE user_id = @USER_ID"

        let tableNames = ResizeArray<string>()

        SqlConnection.connectionString
        |> Sql.connect
        |> Sql.query query
        |> Sql.parameters [ "USER_ID", Sql.int user.userId ]
        |> Sql.iter (fun read -> tableNames.Add(read.text "table_name"))

        tableNames

    let getTableData (tableInfo : getTableData)=
        let query = 
            sprintf "SELECT * from %s AS table_data" (tableInfo.tableName + tableInfo.id.ToString())
        let tableData = ResizeArray<string>()

        SqlConnection.connectionString
        |>Sql.connect
        |> Sql.query query
        |>Sql.executeRow(fun read -> tableData.Add(read.text "table_data"))
        tableData

    let getTableHeaders (tableInfo : getTableData)=
        let tableHeaders = ResizeArray<string[]>()
        SqlConnection.connectionString
        |>Sql.connect
        |>Sql.query "SELECT table_headers FROM user_table_map WHERE table_name = @TABLE_NAME AND user_id = @USER_ID"
        |>Sql.parameters["TABLE_NAME",Sql.text tableInfo.tableName ;"USER_ID", Sql.int tableInfo.id]
        |>Sql.execute(fun read -> tableHeaders.Add(read.stringArray "tableHeaders"))
    
   