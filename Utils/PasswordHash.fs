namespace PasswordHash

module PasswordHash =
    open System.Security.Cryptography
    open System.Text

    let getPasswordHash (password: string) =
        use sha512 = SHA512.Create()

        ASCIIEncoding.UTF8.GetBytes password
        |> sha512.ComputeHash
        |> Seq.fold (fun hash byte -> hash + byte.ToString("x2")) ""
