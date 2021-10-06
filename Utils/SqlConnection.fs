namespace SqlConnection

module SqlConnection =
    open Npgsql.FSharp

    let connectionString: string =
        Sql.host "localhost"
        |> Sql.database "backend"
        |> Sql.username "postgres"
        |> Sql.password "nayla"
        |> Sql.formatConnectionString
