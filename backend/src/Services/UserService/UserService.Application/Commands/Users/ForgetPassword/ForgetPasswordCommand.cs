using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;

namespace UserService.Application.Commands.Users.ForgetPassword;

public class ForgetPasswordCommand : IRequest<Result<string>>
{
    public string Email { get; set; }
}