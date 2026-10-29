using UserManagement.API.Models;
using UserManagement.API.Repositories;

namespace UserManagement.Tests.Tests.Unit;

public class InMemoryUserRepositoryTests
{
    private static readonly DateTime CreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly InMemoryUserRepository _repository = new();

    private static User NewUser() => User.Create("RU", "ru-RU", CreatedAt);

    [Fact]
    public void Add_NewUser_ReturnsSameUserAndStoresIt()
    {
        var user = NewUser();

        var added = _repository.Add(user);

        Assert.Same(user, added);
        Assert.Same(user, _repository.GetById(user.UserId));
    }

    [Fact]
    public void Add_UserWithExistingId_ThrowsAndKeepsOriginal()
    {
        var user = NewUser();
        _repository.Add(user);

        Assert.Throws<InvalidOperationException>(() => _repository.Add(user.Update("US", "en-US", CreatedAt)));
        Assert.Same(user, _repository.GetById(user.UserId));
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        Assert.Null(_repository.GetById(Guid.NewGuid()));
    }

    [Fact]
    public void GetAll_EmptyRepository_ReturnsEmptyCollection()
    {
        Assert.Empty(_repository.GetAll());
    }

    [Fact]
    public void GetAll_ReturnsSnapshotNotAffectedByLaterChanges()
    {
        _repository.Add(NewUser());

        var snapshot = _repository.GetAll();
        _repository.Add(NewUser());

        Assert.Single(snapshot);
        Assert.Equal(2, _repository.GetAll().Count());
    }

    [Fact]
    public void Update_ExistingUser_ReturnsTrueAndReplacesUser()
    {
        var user = NewUser();
        _repository.Add(user);
        var updated = user.Update("US", "en-US", CreatedAt.AddMinutes(1));

        var result = _repository.Update(updated);

        Assert.True(result);
        Assert.Same(updated, _repository.GetById(user.UserId));
    }

    [Fact]
    public void Update_UnknownUser_ReturnsFalseAndDoesNotAddUser()
    {
        var user = NewUser();

        var result = _repository.Update(user);

        Assert.False(result);
        Assert.Null(_repository.GetById(user.UserId));
    }

    [Fact]
    public void Add_ConcurrentCalls_StoresAllUsers()
    {
        const int count = 1000;

        Parallel.For(0, count, _ => _repository.Add(NewUser()));

        Assert.Equal(count, _repository.GetAll().Count());
    }

    [Fact]
    public void Update_ConcurrentCalls_AllSucceedAndLastStateIsOneOfUpdates()
    {
        const int count = 1000;
        var user = NewUser();
        _repository.Add(user);
        var updates = Enumerable.Range(0, count)
            .Select(i => user.Update("US", "en-US", CreatedAt.AddSeconds(i)))
            .ToList();

        var results = new bool[count];
        Parallel.For(0, count, i => results[i] = _repository.Update(updates[i]));

        Assert.All(results, Assert.True);
        Assert.Contains(_repository.GetById(user.UserId), updates);
    }

    [Fact]
    public async Task GetAll_WhileAddingConcurrently_DoesNotThrow()
    {
        var writer = Task.Run(() => Parallel.For(0, 1000, _ => _repository.Add(NewUser())));

        while (!writer.IsCompleted)
            _ = _repository.GetAll().ToList();

        await writer;
        Assert.Equal(1000, _repository.GetAll().Count());
    }
}
