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
        <li><a href="#how-to-create-an-action">How to create an action</a></li>
        <li><a href="#how-to-create-an-event">How to create an event</a></li>
        <li><a href="#how-to-parse-request-parameters">How to parse request parameters</a></li>
        <li><a href="#how-to-communicate-with-other-microservices">How to communicate with other microservices</a></li>
        <li><a href="#how-to-register-handlers">How to register handlers</a></li>
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
    "RabbitMQ": {
      "Host": "host_name",
      "Port": "port"
    },
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

In `Program.cs` set new Service options and pass them to `CreateService` method of ServiceBroker:

```csharp
var options = new ServiceOptions {
    Name = config["ServiceName"]!,
    Version = config["Version"]!,
    Transport = new ServerAddress {
        Host = config["Connections:RabbitMQ:Host"]!,
        Port = config["Connections:RabbitMQ:Port"]!,
    },
    RetryPolicy = new RetryPolicy { },
    Logging = true,
    RequestTimeout = 4000,
};

ServiceBroker.CreateService(builder, options);
```

Properties of **ServiceOptions**:

| Property       | Description                     | Required | Default                    |                   
| -----------    | ------------------------------- | -------- | -------------------------- |                   
| Name           | Name of microservice            | ✔        | ""                         |
| Version        | Version of microservice         | ✔        | ""                         |
| Transport      | Address of RabbitMQ server      | ✔        | null                       |
| RequestTimeout | Request timeout                 | -        | 5000                       |
| RetryPolicy    | Default attempts - 1, delay - 0 | -        | MaxAttempts - 1, Delay - 0 |
| Logging        | Use Serilog                     | -        | false                      |

---
If you have MySql server, make migration and data base updating firstly:

```sh
dotnet ef migrations add <migration_name>
```

```sh
dotnet ef database update
```

## Running service

Run microservice by command, input your host and port:

```sh
dotnet run --urls=http://<host>:<port>
```

## Usage

### How to create an action

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
        RequestTimeout = 3000,
        RetryPolicy = new RetryPolicy {
            MaxAttempts = 3,
            Delay = 500
        },
        Caching = false,
        Access = ["USER", "MANAGER", "ADMIN"],
        Handler = async (Context ctx) => {
            User user = await ctx.Request.Parameters.ConvertToModel<User>();
            
            var existedUser = await db.Users.FindAsync(user.Id);

            if (existedUser == null) {
              return new Response {};
            }

            return existedUser.ToReadDTO();
        },
    };
```

**NewAction** properties:

| Property       | Description                      | Required | Default                    |
| -------------- | -------------------------------- | -------- | -------------------------- |
| Route          | Information about route          | -        | null                       |
| Params         | Parameters that will be received | -        | null                       |
| RequestTimeout | Request timeout limit            | -        | 5000 ms                    |
| RetryPolicy    | Retry policy                     | -        | MaxAttempts = 1, Delay = 0 |
| Caching        | Enable request cache             | -        | false                      |
| Access         | Roles that allow this route      | -        | ["ALL"]                    |
| Handler        | Action handler function          | ✔        | -                          |

---

Inside **Params** there is an object with fields, the names of the fields coincide with the names of the request parameters.

```csharp
        Params = new {
            UserName = new { Type = "String", Required = true, Description = "John" },
            Email = new { Type = "Email", Required = true, Description = "john@mail.com" },
            Password = new { Type = "Password", Required = true, Description = "P@ssw0rd123456" },
            Role = new { Allowed = new List<string> { "USER", "MANAGER", "ADMIN" } },
        },
```

**Params** properties:

| Property    | Description                  | Required | Default |
| ----------- | ---------------------------- | -------- | ------- |
| Type        | Parameter data type          | ✔        | -       |
| Required    | Make parameter required            | -        | false   |
| Description | Description of parameter     | -        | null    |
| Allowed     | Allowed parameters variables | -        | [ ]     |

_You can add any additional information to object. All this information will be presented in internal microservices documentation (Services Registry)_

### How to create an event

Create new event listener using **NewEvent** class within your Handler class:

```csharp
public NewEvent UserCreated = new() {
        Handler = async (Context ctx) => {
          // some logic
        }
      }
```

The Handler function will be called after the `User Created` event fires. Use method `Broadcast` of Services Registry to trigger event. Every service which has event `User Created` will immediately react.

```csharp
await Services.Broadcast("UserCreated");
```

## How to parse request parameters

```csharp
User user = await ctx.Request.Parameters.ConvertToModel<User>();
```

## How to communicate with other microservices

Use **NodeRegestry** class to send request to other microservice with necessary parameters as object:

```csharp
var parameters = new {
  UserId = 1,
  Text = "Some text"
  ...
};

Response data = await Services.Call("Microservice_Name", "Action_Name", parameters);

var responseModel = await sendingData.Data.ConvertToModel<Some_Model>();
```

## How to register handlers

In `Program.cs`:

- Register handlers in DI container:

```csharp
builder.Services.AddScoped<UserHandlers>();
```

- Add handlers to registry as a list:

```csharp
using var serviceScope = app.Services.CreateScope(); {
    var userHandlers = serviceScope
        .ServiceProvider
        .GetRequiredService<UserHandlers>();

    Handlers.Add([userHandlers]);
}
```
---
