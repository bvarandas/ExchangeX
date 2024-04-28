using DropCopyX.Core.Repositories;
using MediatR;
using SharedX.Core.Bus;
namespace DropCopyX.Application.Commands;
public class ExecutionReportCommandHandler : 
    IRequestHandler<ExecutionReportCommand, bool>
{
    private readonly IDropCopyRepository _repository;
    private readonly IMediatorHandler _bus;

    private ExecutionReportCommandHandler(
        IDropCopyRepository repository, 
        IMediatorHandler bus)
    {
        _repository = repository;
        _bus = bus;
    }
    public async Task<bool> Handle(ExecutionReportCommand request, CancellationToken cancellationToken)
    {
        await _repository.AddExecutionReports(request.ExecutionReports, cancellationToken);
        return true;
    }
}