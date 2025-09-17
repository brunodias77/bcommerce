namespace BuildingBlocks.Abstractions;

/// <summary>
/// Interface marcadora para notificações (eventos de domínio)
/// Implementada por eventos que representam algo importante que aconteceu no domínio
/// Os eventos são publicados quando uma operação é concluída e podem ter múltiplos handlers
/// Exemplo: UserCreatedEvent, BetPlacedEvent, PaymentProcessedEvent
/// </summary>
public interface INotification
{
}