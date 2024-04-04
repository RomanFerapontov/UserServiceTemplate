using Microsoft.EntityFrameworkCore;
using UserServiceTemplate.Models;

namespace UserServiceTemplate.Libs;

public class DataBase(DbContextOptions<DataBase> options) : DbContext(options) {
    public DbSet<User> Users { get; set; }
}
