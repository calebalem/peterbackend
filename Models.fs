namespace backend.Models


[<CLIMutable>]
type TableNames = 
    {
        TableName : list<string>
    }

[<CLIMutable>]
type LoginModel = 
    {
        email : string
        password : string
    }

[<CLIMutable>]
type TokenResult = 
    {
        Token : string
    }
[<CLIMutable>]
 type UserModel = 
    {   
        id : int
        email : string
        userName : string
    }