using AlfaMicroserviceMesh;
using AlfaMicroserviceMesh.Models;
using AlfaMicroserviceMesh.Models.Service;
using AlfaMicroserviceMesh.Models.Service.Handler;
using AlfaMicroserviceMesh.Registry;
using Microsoft.EntityFrameworkCore;
using UserServiceTemplate.Handlers;
using UserServiceTemplate.Libs;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddDbContext<DataBase>(o => o.UseMySQL(config["Connections:MySql"]!));

builder.Services.AddScoped<UserHandlers>();
// builder.Services.AddScoped<OtherHandlers>();

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

var app = builder.Build();

using var serviceScope = app.Services.CreateScope(); {
    serviceScope.ServiceProvider.GetRequiredService<DataBase>().Database.Migrate();

    var userHandlers = serviceScope.ServiceProvider.GetRequiredService<UserHandlers>();
    // var otherHandlers = serviceScope.ServiceProvider.GetRequiredService<otherHandlers>();

    Handlers.Add([userHandlers, /*otherHandlers*/]); 
}

app.Run();