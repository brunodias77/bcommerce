namespace BuildingBlocks.Abstractions;

/// <summary>
/// Interface para publicação de eventos de domínio e notificações
/// Responsável por distribuir eventos para todos os handlers registrados
/// Implementa o padrão Observer para comunicação assíncrona entre componentes
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publica uma notificação para todos os handlers registrados (versão genérica)
    /// Todos os handlers que implementam INotificationHandler&lt;TNotification&gt; serão executados
    /// </summary>
    /// <typeparam name="TNotification">Tipo da notificação a ser publicada</typeparam>
    /// <param name="notification">Notificação/evento a ser publicado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Publica uma notificação para todos os handlers registrados (versão não-genérica)
    /// Utilizada quando o tipo da notificação é determinado em tempo de execução
    /// </summary>
    /// <param name="notification">Notificação/evento a ser publicado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    Task Publish(object notification, CancellationToken cancellationToken = default);
}