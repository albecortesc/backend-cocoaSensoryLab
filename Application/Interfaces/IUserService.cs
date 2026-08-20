using System.Collections.Generic;
using UserEntity = Domain.Entities.User;

namespace Application.Interfaces
{
    public interface IUserService
    {
        System.Threading.Tasks.Task UserRegister(UserEntity user);
        System.Threading.Tasks.Task<bool> UserAuthenticate(string email, string password);
        System.Threading.Tasks.Task<IEnumerable<UserEntity>> UserGetAll();
        System.Threading.Tasks.Task UserUpdateByCedulaOrEmail(long? cedula, string? email, UserEntity updatedUser);    
        System.Threading.Tasks.Task<UserEntity?> UserFindByCedulaOrEmail(long? cedula, string? email);
        System.Threading.Tasks.Task UserDeleteByCedulaOrEmail(long? cedula, string? email);
    }
}
