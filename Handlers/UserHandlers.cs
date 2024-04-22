using Microsoft.EntityFrameworkCore;
using UserServiceTemplate.Libs;
using UserServiceTemplate.Helpers;
using UserServiceTemplate.Mappers;
using UserServiceTemplate.Models;
using AlfaMicroserviceMesh.Extentions;
using AlfaMicroserviceMesh.Models.ReqRes;
using AlfaMicroserviceMesh.Models.Service;
using AlfaMicroserviceMesh.Models.Service.Handler;
using AlfaMicroserviceMesh.Exceptions;
using AlfaMicroserviceMesh.Constants;

namespace UserServiceTemplate.Handlers;

public class UserHandlers(DataBase db) {
    public NewAction SignUp = new() {
        Route = new {
            Method = "POST",
            Path = "Users/SignUp",
            Description = "Creating new user profile"
        },
        Params = new {
            UserName = new { Type = "String", Required = true, Description = "John" },
            Email = new { Type = "Email", Required = true, Description = "john@mail.com" },
            Password = new { Type = "Password", Required = true, Description = "P@ssw0rd123456" },
            Role = new { Allowed = new List<string> { "USER", "MANAGER", "ADMIN" } },
        },
        Handler = async (Context ctx) => {
            User user = await ctx.Request.Parameters.ConvertToModel<User>();

            var existedUser = await db.Users.AnyAsync(u => u.Email == user.Email);

            if (existedUser)
                throw new MicroserviceException($"User with email '{user.Email}' already exists", ErrorTypes.ArgumentError);

            var newUser = new User {
                UserName = user.UserName,
                Uid = Guid.NewGuid().ToString(),
                Email = user.Email,
                Password = Helper.GetHashPassword(user.Password!),
                Role = user.Role,
            };

            await db.Users.AddAsync(newUser);
            await db.SaveChangesAsync();

            return new Response {
                Data = newUser.ToReadDTO()
            };
        }
    };

    public NewAction SignIn = new() { //use the name "SignIn" or "Login" to provide the JWT in the response
        Route = new {
            Method = "POST",
            Path = "Users/SignIn",
            Description = "Login user"
        },
        Params = new {
            Email = new { Type = "Email", Required = true, Description = "john@mail.com" },
            Password = new { Type = "Password", Required = true, Description = "P@ssw0rd123456" },
        },
        Handler = async (Context ctx) => {
            User user = await ctx.Request.Parameters.ConvertToModel<User>();

            var existedUser = await db.Users.FirstOrDefaultAsync(u => u.Email == user.Email) ??
                throw new MicroserviceException($"User with email '{user.Email}' not found", ErrorTypes.ArgumentError);

            if (Helper.GetHashPassword(user.Password!) != existedUser.Password)
                throw new MicroserviceException($"Incorrect password", ErrorTypes.ArgumentError);

            return new Response {
                Data = existedUser.ToReadDTO()
            };
        }
    };

    public NewAction GetUsers = new() {
        Route = new {
            Method = "GET",
            Path = "Users/GetUsers",
            Description = "Get users list"
        },
        Params = new {
            OrderBy = new { Type = "String" },
            SortBy = new { Type = "String" },
            Limit = new { Type = "Number" },
            Offset = new { Type = "Number" },
        },
        Access = ["USER", "MANAGER", "ADMIN"],
        Handler = async (Context ctx) => {
            QueryObject parameters = await ctx.Request.Parameters.ConvertToModel<QueryObject>();

            var queryBuilder = new QueryBuilder<User>(db.Users.AsQueryable());

            return new Response {
                Data = await queryBuilder
                .Filter(parameters)
                .Build()
                .Select(u => u.ToReadDTO())
                .ToListAsync()
            };
        },
    };

    public NewAction GetById = new() {
        Route = new {
            Method = "GET",
            Path = "Users/GetById",
            Description = "Get user by ID"
        },
        Params = new {
            Id = new { Type = "Number", Required = true },
        },
        RequestTimeout = 2000,
        RetryPolicy = new RetryPolicy {
            MaxAttempts = 3,
            Delay = 300
        },
        Caching = false,
        Access = ["USER", "MANAGER", "ADMIN"],
        Handler = async (Context ctx) => {
            User user = await ctx.Request.Parameters.ConvertToModel<User>();

            var existedUser = await db.Users.FindAsync(user.Id);
                
            if (existedUser == null) {
                return new Response {};
            }

            await Task.Delay(new Random().Next(0, 2) == 0 ? 500 : 3000); // imitation of request delay

            return new Response {
                Data = existedUser.ToReadDTO()
            };
        },
    };
};
