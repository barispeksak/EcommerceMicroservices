using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Dtos;
using UserMicroservice.Service.Interfaces;

namespace UserMicroservice.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllUsersAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _service.GetUserByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState); //  Hatalıysa 400 dön

        var result = await _service.CreateUserAsync(dto);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState); //  Hatalıysa 400 dön

        var updated = await _service.UpdateUserAsync(dto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteUserAsync(id) ? NoContent() : NotFound();
}
