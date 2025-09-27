namespace UserService.Application.Dtos.Responses;

public record ResetPasswordResponse(
    bool Success,
    string Message
);