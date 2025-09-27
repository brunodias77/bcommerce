using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using UserService.Application.Dtos.Responses;

namespace UserService.Application.Commands.Users.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
{
    public Task<Result<ResetPasswordResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
 
        
        
        throw new NotImplementedException();
    }
}