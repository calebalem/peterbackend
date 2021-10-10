namespace TableModel

module TableModel =
    open Npgsql.FSharp
    open TableTypes
    open UserTypes
    open SimpleTypes
    open SqlConnection
    open Npgsql

    let private dynamiclyCreateTableQuery (table: Table) =
        let queryStart =
            sprintf
                "CREATE TABLE %s ("
                (table.tableName + table.userId.ToString())

        let mutable dynamicQuery = ""

        for i in 0 .. table.tableHeaders.Length - 1 do
            if i = table.tableHeaders.Length - 1 then
                dynamicQuery <-
                    dynamicQuery
                    + sprintf "%s  %s" (table.tableHeaders.Item(i)) (table.tableDataTypes.Item(i))
            else
                dynamicQuery <-
                dynamicQuery
                + sprintf "%s  %s," (table.tableHeaders.Item(i)) (table.tableDataTypes.Item(i))

        let queryEnd = ");"
        let finalQuery = queryStart + dynamicQuery + queryEnd
        finalQuery

    let private dynamiclyInsertTableDataQuery (tableData: Table) =
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
                        + sprintf "%s)" (if tableData.tableDataTypes.Item(j) = "character varying(255)" then "'"+tableData.tableDatas.Item(i).Item(j).ToString()+"'" else tableData.tableDatas.Item(i).Item(j).ToString())
                else
                    valuesQuery <-
                        valuesQuery
                        + sprintf "%s, " (if tableData.tableDataTypes.Item(j) = "character varying(255)" then "'"+tableData.tableDatas.Item(i).Item(j).ToString()+"'" else tableData.tableDatas.Item(i).Item(j).ToString())

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
        |> Sql.parameters [ "user_id", Sql.int table.userId;
                            "table_name", Sql.string table.tableName;]
        |> Sql.executeNonQuery

    let deleteTable (table : Table) = 
        let queryDeleteTable = sprintf "DROP TABLE %s;" (table.tableName + table.userId.ToString())
        use connection = new NpgsqlConnection(SqlConnection.connectionString)
        connection.Open()
        let cmd = NpgsqlCommand(queryDeleteTable,connection)
        let deleteTableResponse = cmd.ExecuteNonQuery()
        let queryDeleteUserTableMapEntry = sprintf "DELETE FROM user_table_map WHERE user_id = %i AND LOWER(table_name) = LOWER('%s')" table.userId table.tableName
        let cmd = NpgsqlCommand(queryDeleteUserTableMapEntry,connection)
        let deleteMapEntryResponse = cmd.ExecuteNonQuery()
        connection.Close()
        deleteMapEntryResponse

    let addTableData (table: Table) =
         //Drop table
        deleteTable table

        //Create table
        addTable table

        //insertData
        let query = dynamiclyInsertTableDataQuery table
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
            sprintf "SELECT * from %s" (tableInfo.tableName + tableInfo.id.ToString())
        let tableData = ResizeArray<obj>()
        let columnNames = ResizeArray<obj>()
        let columnTypes = ResizeArray<obj>()
        use connection = new NpgsqlConnection(SqlConnection.connectionString)
        connection.Open()
        let cmd = NpgsqlCommand(query,connection)
        let reader = cmd.ExecuteReader()
        
        for i in 0 .. reader.FieldCount - 1 do
            columnTypes.Add(reader.GetDataTypeName(i))
            columnNames.Add(reader.GetName(i))

        while reader.Read() do
            let tableRow = ResizeArray<obj>()
            for i in 0 .. reader.FieldCount - 1 do
                tableRow.Add(reader.GetValue(i))
            tableData.Add(tableRow)

        reader.Close()
        connection.Close()
        let response = 
            Map.empty.
                Add("tableData",tableData).
                Add("columnNames",columnNames).
                Add("columnTypes",columnTypes)
        response 

   
    
   