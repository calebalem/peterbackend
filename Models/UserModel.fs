namespace UserModel

module UserModel =
    open Npgsql.FSharp
    open UserTypes
    open SimpleTypes
    open PasswordHash
    open Npgsql
    open SqlConnection


    let private connectionString: string =
        Sql.host "localhost"
        |> Sql.database "backend"
        |> Sql.username "postgres"
        |> Sql.password "nayla"
        |> Sql.formatConnectionString

   

    let addUser (user: User) =
        let logQuery = "SELECT * FROM user_table WHERE user_email = @USEREMAIL"
        use connection = new NpgsqlConnection(SqlConnection.connectionString)
        connection.Open()
        let cmd = NpgsqlCommand(logQuery,connection)
        cmd.Parameters.AddWithValue("USEREMAIL",user.userEmail)
        let result = cmd.ExecuteReader()
        printf "user login %b" result.HasRows
        if not result.HasRows then
            connection.Close()
            connectionString
            |> Sql.connect
            |> Sql.query
                "INSERT INTO user_table (user_name,user_email,user_password) VALUES(@user_name, @user_email, @user_password);"
            |> Sql.parameters [ "user_name", Sql.text user.userName
                                "user_email", Sql.text user.userEmail
                                "user_password", Sql.text (PasswordHash.getPasswordHash user.userPassword) ]
            |> Sql.executeNonQuery
        else
            connection.Close()
            0
            
    let authUser (loginModel: LoginModel) =
        let passwordHash = PasswordHash.getPasswordHash loginModel.password
        let user = User()

        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM user_table WHERE user_email=@USER_EMAIL AND user_password=@USER_PASSWORD"
        |> Sql.parameters [ "USER_EMAIL", Sql.text loginModel.email
                            "USER_PASSWORD", Sql.text passwordHash ]
        |> Sql.execute
            (fun read user ->
                { id = read.int "user_id"
                  email = read.text "user_email"
                  userName = read.text "user_name" })

    let getUser (loginModel: LoginModel) : UserModel list =
        let passwordHash = PasswordHash.getPasswordHash loginModel.password

        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM user_table WHERE user_email=@USER_EMAIL AND user_password=@USER_PASSWORD"
        |> Sql.parameters [ "USER_EMAIL", Sql.text loginModel.email
                            "USER_PASSWORD", Sql.text passwordHash ]
        |> Sql.execute
            (fun read ->
                { id = read.int "user_id"
                  email = read.text "user_email"
                  userName = read.text "user_name" })
