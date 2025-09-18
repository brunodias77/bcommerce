using BuildingBlocks.Mediator;

namespace UserService.Api.Configurations;

public static class ApplicationDependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddMediator(services);
    }
    
    private static void AddMediator(IServiceCollection services)
    {
        // Register our custom Mediator implementation
        services.AddMediator(
            typeof(UserService.Application.Commands.Users.CreateUser.CreateUserCommand).Assembly, // Application assembly
            typeof(BuildingBlocks.Abstractions.IMediator).Assembly, // BuildingBlocks assembly
            typeof(BuildingBlocks.Mediator.Mediator).Assembly // Mediator implementation assembly
        );
    }
}