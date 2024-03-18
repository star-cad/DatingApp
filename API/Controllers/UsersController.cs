using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await _userRepository.GetMembersAsync();
        return Ok(users);
    }

    [HttpGet("{userName}")] // /api/userName
    public async Task<ActionResult<MemberDto>> GetUser(string userName)
    {
        return await _userRepository.GetMemberAsync(userName);
    }

    // [HttpGet("{id}")] // /api/users/#
    // public async Task<ActionResult<AppUser>> GetUser(int id)
    // {
    //     return await _userRepository.GetUserByIdAsync(id);
    // }
}
