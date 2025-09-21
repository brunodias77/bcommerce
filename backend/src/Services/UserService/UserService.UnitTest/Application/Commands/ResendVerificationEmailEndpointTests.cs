using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiProgram = Program;
using Bogus;
using BuildingBlocks.Validations;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UserService.Api.Endpoints;
using UserService.Application.Dtos.Keycloak;
using UserService.Application.Dtos.Requests;
using UserService.Application.Dtos.Responses;
using UserService.Application.Services.Interfaces;
using UserService.Domain.Entities;
using UserService.UnitTest.Application.Helpers;
using Xunit;



namespace UserService.UnitTest.Application.Commands;

public class ResendVerificationEmailEndpointTests
{
    private readonly Mock<IKeycloakService> _mockKeycloakService;
    private readonly Faker _faker;

    public ResendVerificationEmailEndpointTests()
    {
        _mockKeycloakService = new Mock<IKeycloakService>();
        _faker = new Faker();
    }

    private static ILogger<ApiProgram> CreateLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        return loggerFactory.CreateLogger<ApiProgram>();
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_ComEmailValido_DeveEnviarEmailComSucesso()
    {
        // Arrange
        var request = new ResendVerificationEmailRequest(_faker.Internet.Email());
        var expectedUser = new UserResponseKeycloak(
            _faker.Random.Guid().ToString(),
            request.Email,
            request.Email,
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            true,
            false,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            new List<string> { "user" },
            new Dictionary<string, List<string>>()
        );

        _mockKeycloakService
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(expectedUser);

        _mockKeycloakService
            .Setup(x => x.SendEmailVerificationAsync(expectedUser.Id))
            .ReturnsAsync(true);

        // Act
        var result = await AuthEndpoints.ResendVerificationEmailAsync(request, _mockKeycloakService.Object, CreateLogger());

        // Assert
        result.Should().BeOfType<Ok<EmailVerificationSentResponse>>();
        var okResult = result as Ok<EmailVerificationSentResponse>;
        okResult!.Value.Email.Should().Be(request.Email);
        okResult!.Value.Message.Should().Be("E-mail de verificação enviado com sucesso");
        okResult.Value.SentAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        _mockKeycloakService.Verify(x => x.GetUserByEmailAsync(request.Email), Times.Once);
        _mockKeycloakService.Verify(x => x.SendEmailVerificationAsync(expectedUser.Id), Times.Once);
    }

    [Fact]
    public async Task DeveRetornarBadRequest_QuandoEmailVazio()
    {
        // Arrange
        var request = new ResendVerificationEmailRequest("");

        // Act
        var result = await AuthEndpoints.ResendVerificationEmailAsync(request, _mockKeycloakService.Object, CreateLogger());

        // Assert
        result.Should().BeOfType<BadRequest<ProblemDetails>>();
        var badRequestResult = result as BadRequest<ProblemDetails>;
        badRequestResult.Value.Title.Should().Be("Solicitação inválida");
        badRequestResult.Value.Detail.Should().Be("Endereço de e-mail é obrigatório");
        
        _mockKeycloakService.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeveRetornarOk_QuandoUsuarioNaoEncontrado()
    {
        // Arrange
        var request = new ResendVerificationEmailRequest("inexistente@exemplo.com");
        
        _mockKeycloakService.Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync((UserResponseKeycloak)null);

        // Act
        var result = await AuthEndpoints.ResendVerificationEmailAsync(request, _mockKeycloakService.Object, CreateLogger());

        // Assert
        result.Should().BeOfType<Ok<EmailVerificationSentResponse>>();
        var okResult = result as Ok<EmailVerificationSentResponse>;
        okResult.Value.Email.Should().Be(request.Email);
        okResult.Value.Message.Should().Be("Se existir uma conta com este e-mail, um e-mail de verificação foi enviado");
        
        _mockKeycloakService.Verify(x => x.GetUserByEmailAsync(request.Email), Times.Once);
        _mockKeycloakService.Verify(x => x.SendEmailVerificationAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_ComEmailJaVerificado_DeveRetornarConflict()
    {
        // Arrange
        var request = new ResendVerificationEmailRequest(_faker.Internet.Email());
        var expectedUser = new UserResponseKeycloak(
            _faker.Random.Guid().ToString(),
            request.Email,
            request.Email,
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            true,
            true,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            new List<string> { "user" },
            new Dictionary<string, List<string>>()
        );

        _mockKeycloakService
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await AuthEndpoints.ResendVerificationEmailAsync(request, _mockKeycloakService.Object, CreateLogger());

        // Assert
        result.Should().BeOfType<Conflict<ProblemDetails>>();
        var conflictResult = result as Conflict<ProblemDetails>;
        conflictResult!.Value.Title.Should().Be("E-mail já verificado");
        conflictResult.Value.Detail.Should().Be("Este endereço de e-mail já foi verificado");
        conflictResult.Value.Status.Should().Be(409);

        _mockKeycloakService.Verify(x => x.GetUserByEmailAsync(request.Email), Times.Once);
        _mockKeycloakService.Verify(x => x.SendEmailVerificationAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_ComFalhaNoServicoKeycloak_DeveRetornarInternalServerError()
    {
        // Arrange
        var request = new ResendVerificationEmailRequest(_faker.Internet.Email());
        var expectedUser = new UserResponseKeycloak(
            _faker.Random.Guid().ToString(),
            request.Email,
            request.Email,
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            true,
            false,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            new List<string> { "user" },
            new Dictionary<string, List<string>>()
        );

        _mockKeycloakService
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(expectedUser);

        _mockKeycloakService
            .Setup(x => x.SendEmailVerificationAsync(expectedUser.Id))
            .ReturnsAsync(false); // Falha no envio

        // Act
        var result = await AuthEndpoints.ResendVerificationEmailAsync(request, _mockKeycloakService.Object, CreateLogger());

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        var problemResult = result as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(500);
        problemResult.ProblemDetails.Title.Should().Be("Falha ao enviar e-mail");
        problemResult.ProblemDetails.Detail.Should().Be("Falha ao enviar e-mail de verificação");

        _mockKeycloakService.Verify(x => x.GetUserByEmailAsync(request.Email), Times.Once);
        _mockKeycloakService.Verify(x => x.SendEmailVerificationAsync(expectedUser.Id), Times.Once);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_ComExcecaoGeral_DeveRetornarInternalServerError()
    {
        // Arrange
        var request = new ResendVerificationEmailRequest(_faker.Internet.Email());
        var expectedException = new InvalidOperationException("Erro interno do servidor");

        _mockKeycloakService
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ThrowsAsync(expectedException);

        // Act
        var result = await AuthEndpoints.ResendVerificationEmailAsync(request, _mockKeycloakService.Object, CreateLogger());

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        var problemResult = result as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(500);
        problemResult.ProblemDetails.Title.Should().Be("Resend Email Error");
        problemResult.ProblemDetails.Detail.Should().Be("An error occurred while resending verification email");
        
        _mockKeycloakService.Verify(x => x.GetUserByEmailAsync(request.Email), Times.Once);
        _mockKeycloakService.Verify(x => x.SendEmailVerificationAsync(It.IsAny<string>()), Times.Never);
    }


}

// Classe Program fictícia para compatibilidade com ILogger<Program>
public class Program
{
}