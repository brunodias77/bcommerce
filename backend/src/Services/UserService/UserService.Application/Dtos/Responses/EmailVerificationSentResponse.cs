namespace UserService.Application.Dtos.Responses;

public record EmailVerificationSentResponse(
        string Email, string Message, DateTimeOffset SentAt
    );
