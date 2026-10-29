using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UserManagement.API.DTOs;
using UserManagement.API.Interfaces;
using UserManagement.API.Validation;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly TimeProvider _timeProvider;

    public UsersController(IUserRepository userRepository, TimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _timeProvider = timeProvider;
    }

    [HttpPost]
    public IActionResult CreateUser(UserDto userDto)
    {
        var user = Models.User.Create(userDto.CountryIsoCode, userDto.Locale, GetUtcNow());

        var addedUser = _userRepository.Add(user);

        return CreatedAtAction(nameof(GetUser), new { userId = addedUser.UserId }, addedUser);
    }

    [HttpPut("{userId}")]
    public IActionResult UpdateUser([NotEmptyGuid] Guid userId, UserDto userDto)
    {
        var user = _userRepository.GetById(userId);
        if (user is null) return NotFound();

        var updatedUser = user.Update(userDto.CountryIsoCode, userDto.Locale, GetUtcNow());

        var updated = _userRepository.Update(updatedUser);
        if (!updated) return NotFound();

        return Ok(updatedUser);
    }

    [HttpGet("{userId}")]
    public IActionResult GetUser([NotEmptyGuid] Guid userId)
    {
        var user = _userRepository.GetById(userId);
        if (user is null) return NotFound();

        return Ok(user);
    }

    [HttpGet]
    public IActionResult GetUsers(
        [FromQuery][Range(1, 1000000)] int? page = null,
        [FromQuery][Range(1, 10)] int? pageSize = null)
    {
        var users = _userRepository.GetAll();

        users = users
            .OrderByDescending(u => u.DateCreated)
            .ThenBy(u => u.UserId);

        if (!page.HasValue && !pageSize.HasValue)
            return Ok(users);

        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? 10;
        users = users
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize);

        return Ok(users);
    }

    private DateTime GetUtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
