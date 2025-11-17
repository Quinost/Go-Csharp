namespace API.Mediator.Requests;
public interface ICommand { }
public interface ICommandHandler<TCommand>
{
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}
