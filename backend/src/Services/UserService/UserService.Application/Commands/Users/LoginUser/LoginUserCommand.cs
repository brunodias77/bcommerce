using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using UserService.Application.Dtos.Responses;

namespace UserService.Application.Commands.Users.LoginUser;


public class LoginUserCommand : IRequest<Result<LoginUserResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

