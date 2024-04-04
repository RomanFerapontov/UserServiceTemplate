# UserServiceTemplate

<a name="readme-top"></a>

  <summary></summary>
    <li>
      <a href="#about">About</a>
    </li>
    <li>
      <a href="#installation">Installation</a>
    </li>
    <li>
      <a href="#running-service">Running service</a>
    </li>
    <li>
      <a href="#usage">Usage</a>
        <ul>
        <li><a href="#establish-api-gateway">How to create action</a></li>
        <li><a href="#establish-api-gateway">How to parse request parameters</a></li>
        <li><a href="#running-application">How to communicate with other microservices</a></li>
        <li><a href="#add-new-microservice">How to register handlers</a></li>
        </ul>
    </li>

## About

This is ready-made template of microservice for [AlfaMicroservicesMesh](https://github.com/RomanFerapontov/AlfaMicroserviceMesh) framework. Use this tempate to build your own services.

## Installation

Clone repo:

```sh
git clone git@github.com:RomanFerapontov/UserServiceTemplate.git
```

Install NuGet packages:

```sh
dotnet restore
```

Make sure you enter the variables in `appsettings.js`

```json
{
  "ServiceName": "Users",
  "Version": "dev",
  "Connections": {
    "RabbitMQ": "http://host_name:port",
    "MySql": "Server=<DB_Host>;Port=<DB_Port>;Database=<DB_Name>;Uid=root;Pwd=<DB_Password>;"
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Information"
      }
    },
    "WriteTo": [{ "Name": "Console" }],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

If you have MySql server, make migration and data base updating firstly:

```sh
dotnet ef migrations add <migration_name>
```

```sh
dotnet ef database update
```

## Running service

Run microservice by command:

```sh
dotnet run
```

## Usage

### How to create action

Create new action using **NewAction** class within your Handler class:

```csharp
public NewAction GetById = new() {
        Route = new {
            Method = "GET",
            Path = "Users/GetById",
            Description = "Get user by ID"
        },
        Params = new {
            Id = new { Type = "Number", Required = true },
        },
        Access = ["USER", "MANAGER", "ADMIN"],
        Handler = async (Context ctx) => {
            User user = await ctx.Request.Parameters.ConvertToModel<User>();

            var existedUser = await db.Users.FindAsync(user.Id) ??
                throw new MicroserviceException([$"User with ID {user.Id} not found"], 400, "ARGUMENT_ERROR");

            return existedUser.ToReadDTO();
        },
    };
```

**NewAction** properties:

| Property | Description                      | Required |
| -------- | -------------------------------- | -------- |
| Route    | Information about route          | -        |
| Params   | Parameters that will be received | ✔        |
| Access   | Roles that allow this route      | -        |
| Handler  | Action handler function          | ✔        |

---

Inside **Params** there is an object with fields, the names of the fields coincide with the names of the request parameters.

```csharp
    Params = new {
        Id = new { Type = "Number", Required = true },
        Name = new { Type = "String", Desctription = "Name of user" },
    },
```

**Params** properties:

| Property    | Description              | Required |
| ----------- | ------------------------ | -------- |
| Type        | Parameter data type      | ✔        |
| Required    | Default - false          | -        |
| Description | Description of parameter | -        |

_You can add any additional information to object. All this information will be presented in internal microservices documentation (Nodes Registry)_

## How to parse request parameters

```csharp
User user = await ctx.Request.Parameters.ConvertToModel<User>();
```

## How to communicate with other microservices

Use **NodeRegestry** class to send request to other microservice with necessary parameters as object:

```csharp
object sendingData = await NodesRegistry.Call("Mail", "SendMail", new {
    user.Id,
    Text = "SomeText"
});

var mail = await sendingData.ConvertToModel<Mail>();
```

## How to register actions

In `Program.cs`:
* Register handlers in DI container:

```csharp
builder.Services.AddScoped<UserHandlers>();
```

* Add handlers to registry as a list:
```csharp
using var serviceScope = app.Services.CreateScope(); {
    var userHandlers = serviceScope
        .ServiceProvider
        .GetRequiredService<UserHandlers>();

    HandlersRegistry.AddHandlers([userHandlers]); 
}
```
---
