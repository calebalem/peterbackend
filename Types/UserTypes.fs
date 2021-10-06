module UserTypes

type User()=
    member val userId = 0 with get, set
    member val userName = "" with get, set
    member val userEmail = "" with get, set 
    member val userPassword = "" with get, set