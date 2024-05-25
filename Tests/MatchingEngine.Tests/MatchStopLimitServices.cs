using MatchingX.Core.Interfaces;
using MatchingX.Core.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using SharedX.Core.Bus;
namespace MatchingX.Tests;
public class MatchStopLimitServices
{
    private readonly Mock<IMatchingRepository> _matchingRepositoryMock;
    private readonly Mock<IExecutedTradeRepository> _executedTradeRepositoryMock;
    private readonly Mock<ILogger<MatchStopLimitServices>> _loggerOrderBookServiceMock;
    private readonly Mock<IMediatorHandler> _mediatorMock;
    public MatchStopLimitServices()
    {
        _matchingRepositoryMock = new();
        _executedTradeRepositoryMock = new();
        _loggerOrderBookServiceMock = new();
        _mediatorMock = new();
    }






}