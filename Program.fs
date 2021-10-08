module backend.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open UserHandlers;
open TableHandlers;
open JWTHandlers;
open System.Text
open Microsoft.IdentityModel.Tokens
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Http



// ---------------------------------
// Web app
// ---------------------------------
let authorize : HttpFunc -> HttpContext -> HttpFuncResult =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let webApp =
    choose [ subRoute
                 "/api"
                 (choose [ POST
                           >=> choose [ route "/token" >=> JWTHandlers.handlePostToken
                                        route "/user/add"  >=> authorize >=> UserHandlers.handleAddUser
                                        route "/table/create">=> authorize >=>  TableHandlers.handleAddTable
                                        route "/table/addData">=> authorize>=> TableHandlers.handleAddTableData
                                        route "/user" >=> authorize >=> UserHandlers.handleGetUser
                                        route "/tables">=> authorize>=> TableHandlers.handleGetTableNames
                                        route "/table/getData" >=> authorize >=> TableHandlers.handleGetTableData
                                      ]])
             setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")

    clearResponse
    >=> setStatusCode 500
    >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder: CorsPolicyBuilder) =
    builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    let env =
        app.ApplicationServices.GetService<IWebHostEnvironment>()

    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false ->
         app.UseCors(configureCors)
            .UseAuthentication()
            .UseGiraffeErrorHandler(errorHandler)
            .UseHttpsRedirection())
            .UseGiraffe(webApp)


let configureServices (services: IServiceCollection) =
    let sp = services.BuildServiceProvider()
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(fun options ->
            options.TokenValidationParameters <-
                TokenValidationParameters(
                    ValidateActor = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "http://localhost:5001",
                    ValidAudience = "http://localhost:5000",
                    IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTHandlers.secret))
                ))|> ignore

    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main args =
    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
