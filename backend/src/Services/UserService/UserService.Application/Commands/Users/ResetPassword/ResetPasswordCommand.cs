using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using UserService.Application.Dtos.Responses;

namespace UserService.Application.Commands.Users.ResetPassword;

public class ResetPasswordCommand : IRequest<Result<ResetPasswordResponse>>
{
    
}