using UserManagement.API.Interfaces;
using UserManagement.API.Models;

namespace UserManagement.Tests.Infrastructure;

public class ThrowingUserRepository : IUserRepository
{
    public const string ErrorMessage = "Simulated repository failure.";

    public User Add(User user) => throw new InvalidOperationException(ErrorMessage);

    public bool Update(User user) => throw new InvalidOperationException(ErrorMessage);

    public User? GetById(Guid userId) => throw new InvalidOperationException(ErrorMessage);

    public IEnumerable<User> GetAll() => throw new InvalidOperationException(ErrorMessage);
}
