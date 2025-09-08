namespace BuildingBlocks.Abstractions;

/// <summary>
/// Interface para handlers que processam notificações (eventos de domínio)
/// Diferente dos request handlers, uma notificação pode ter múltiplos handlers
/// Cada handler implementa uma reação específica ao evento ocorrido
/// Exemplo: Enviar email, atualizar cache, registrar log, etc.
/// </summary>
/// <typeparam name="TNotification">Tipo da notificação/evento a ser processado</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Processa a notificação/evento de domínio
    /// Executado de forma assíncrona quando o evento é publicado
    /// </summary>
    /// <param name="notification">Notificação/evento a ser processado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}