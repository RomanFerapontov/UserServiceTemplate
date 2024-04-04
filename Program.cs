using AlfaMicroserviceMesh;
using AlfaMicroserviceMesh.Models;
using AlfaMicroserviceMesh.Utils;
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
    Transport = config["Connections:RabbitMQ"]!
};

ServiceBroker.CreateService(builder, options);

var app = builder.Build();

using var serviceScope = app.Services.CreateScope(); {
    serviceScope.ServiceProvider.GetRequiredService<DataBase>().Database.Migrate();

    var userHandlers = serviceScope.ServiceProvider.GetRequiredService<UserHandlers>();
    // var otherHandlers = serviceScope.ServiceProvider.GetRequiredService<otherHandlers>();

    HandlersRegistry.AddHandlers([userHandlers, /*otherHandlers*/]); 
}

app.Run();