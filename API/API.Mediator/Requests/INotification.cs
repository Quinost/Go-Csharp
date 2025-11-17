namespace API.Mediator.Requests;
public interface INotification { }

public interface INotificationHandler<TNotification> where TNotification : INotification 
{ 
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}