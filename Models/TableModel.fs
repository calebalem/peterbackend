namespace TableModel

module TableModel =
    open Npgsql.FSharp
    open TableTypes
    open UserTypes
    open SqlConnection

   

    let private dynamiclyCreateTableQuery (table: Table) =
        let queryStart =
            sprintf
                "CREATE TABLE %s ( row_id serial PRIMARY KEY, user_id INT"
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
        |> Sql.query "INSERT INTO user_table_map(user_id, table_name) VALUES (@user_id,@table_name)"
        |> Sql.parameters [ "user_id", Sql.int table.userId
                            "table_name", Sql.string table.tableName ]
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
