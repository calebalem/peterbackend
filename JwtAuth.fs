namespace JwtAuth

module Auth =
    open System
    open System.Security.Claims
    open System.IdentityModel.Tokens.Jwt
    open Microsoft.IdentityModel.Tokens
    open backend.Models
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks
    open Giraffe
    open System.Text
    open DataAccess

    let secret = "spadR2dre#u-ruBrE@TepA&*Uf@U"
  

    let generateToken email = 
        let claims = [|
            Claim(JwtRegisteredClaimNames.Sub, email);
            Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) |]
        let expires = Nullable(DateTime.UtcNow.AddHours(1.0))
        let notBefore = Nullable(DateTime.UtcNow)
        let securityKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        let signingCredentials = SigningCredentials(key = securityKey, algorithm = SecurityAlgorithms.HmacSha256)

        let token =
            JwtSecurityToken(
                issuer = "http://localhost:5001",
                audience = "http://localhost:5000",
                claims = claims,
                expires = expires,
                notBefore = notBefore,
                signingCredentials = signingCredentials)

        let tokenResult = {
            Token = JwtSecurityTokenHandler().WriteToken(token)
        }

        tokenResult

    let handleGetSecured =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let email = ctx.User.FindFirst ClaimTypes.NameIdentifier
                
            text ("User " + email.Value + " is authorized to access this resource.") next ctx

    let handlePostToken =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! user = ctx.BindJsonAsync<LoginModel>()
                let result = UserAccess.authUser user
                if result.Length < 1 then
                    return! text (sprintf "User doesn't exsist") next ctx
                else
                    let tokenResult = generateToken user.email
                    return! json tokenResult next ctx
            }