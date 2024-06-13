using DropCopyX.Core.Repositories;
using FluentResults;
using MediatR;
using SharedX.Core.Bus;
namespace DropCopyX.Application.Commands;
public class ExecutionReportCommandHandler : 
    IRequestHandler<ExecutionReportCommand, Result>
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
    public async Task<Result> Handle(ExecutionReportCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.AddExecutionReports(request.ExecutionReports, cancellationToken);
        return result;
    }
}