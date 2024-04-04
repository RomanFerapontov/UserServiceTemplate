using UserServiceTemplate.Dtos;
using UserServiceTemplate.Models;

namespace UserServiceTemplate.Mappers;

public static class UserMappers {
    public static UserReadDTO ToReadDTO(this User user) {
        return new UserReadDTO {
            Uid = user.Uid,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}
