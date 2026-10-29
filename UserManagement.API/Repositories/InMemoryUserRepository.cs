using System.Collections.Concurrent;
using UserManagement.API.Interfaces;
using UserManagement.API.Models;

namespace UserManagement.API.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new ConcurrentDictionary<Guid, User>();

    public User Add(User user)
    {
        if (!_users.TryAdd(user.UserId, user))
            throw new InvalidOperationException($"User with id {user.UserId} already exists.");

        return user;
    }

    public bool Update(User user)
    {
        while (_users.TryGetValue(user.UserId, out var existing))
        {
            if (_users.TryUpdate(user.UserId, user, existing))
                return true;
        }

        return false;
    }

    public User? GetById(Guid userId)
    {
        _users.TryGetValue(userId, out var user);
        return user;
    }

    public IEnumerable<User> GetAll()
    {
        return _users.Values.ToList();
    }
}
