using FluentAssertions;
using MacthingX.Application.Commands.Match.OrderType;
using MacthingX.Application.Handlers;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Notifications;
using MatchingX.Core.Repositories;
using Medallion.Threading;
using MediatR;
using SharedX.Core.Account;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Test;

public class MatchingCommandHandlerTest :
    IClassFixture<IMatchingRepository>,
    IClassFixture<IExecutedTradeRepository>,
    IClassFixture<IMediatorHandler>,
    IClassFixture<INotificationHandler<DomainNotification>>,
    IClassFixture<IDistributedLockProvider>
{
    private readonly IMatchingRepository _matchingRepository;
    private readonly IExecutedTradeRepository _executedTradeRepository;
    private readonly IMediatorHandler _mediatorHandler;
    private readonly IDistributedLockProvider _distributedLockProvider;
    private readonly INotificationHandler<DomainNotification> _notificationHandler;

    private MatchingCommandHandler _handler = null!;

    public MatchingCommandHandlerTest(IMatchingRepository matchingRepository,
        IExecutedTradeRepository executedTradeRepository,
        IMediatorHandler mediatorHandler,
        IDistributedLockProvider distributedLockProvider,
        INotificationHandler<DomainNotification> notificationHandler
        )
    {
        _matchingRepository = matchingRepository;
        _executedTradeRepository = executedTradeRepository;
        _mediatorHandler = mediatorHandler;
        _distributedLockProvider = distributedLockProvider;
        _notificationHandler = notificationHandler;
    }

    /*
    public enum OrderType
    {
        Market='1',
        Limit='2',
        Stop ='3',
        StopLimit = '4'
    }
     */

    private OrderEngine CriaOrderMarketCompra()
        => new OrderEngine()
        {
            Symbol = "btcusd",
            Side = SideTrade.Buy,
            Account = new Limit() { AccountId = 10012, },
            TimeInForce = TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            MinQty = 0,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Market,
            ParticipatorId = 10,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private OrderEngine CriaOrderLimitCompra()
        => new OrderEngine()
        {
            Symbol = "btcusd",
            Side = SideTrade.Buy,
            Account = new Limit() { AccountId = 10012, },
            TimeInForce = TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            MinQty = 0,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Limit,
            ParticipatorId = 10,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private OrderEngine CriaOrderMarketVenda()
        => new OrderEngine()
        {
            Symbol = "btcusd",
            Side = SideTrade.Sell,
            Account = new Limit() { AccountId = 10012, },
            TimeInForce = TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            MinQty = 0,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Market,
            ParticipatorId = 10,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private OrderEngine CriaOrderLimitVenda()
        => new OrderEngine()
        {
            Symbol = "btcusd",
            Side = SideTrade.Sell,
            Account = new Limit() { AccountId = 10012, },
            TimeInForce = TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            MinQty = 0,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Limit,
            ParticipatorId = 10,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private OrderEngine CriaOrderStopCompra()
        => new OrderEngine()
        {
            Symbol = "btcusd",
            Side = SideTrade.Buy,
            Account = new Limit() { AccountId = 10012, },
            TimeInForce = TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            MinQty = 0,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Stop,
            ParticipatorId = 10,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private OrderEngine CriaOrderStopVenda()
        => new OrderEngine()
        {
            Symbol = "btcusd",
            Side = SideTrade.Sell,
            Account = new Limit() { AccountId = 10012, },
            TimeInForce = TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            MinQty = 0,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Stop,
            ParticipatorId = 10,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private OrderEngine CriaOrderStopLimitCompra()
        => new OrderEngine()
        {
            Symbol = "btcusd",
            Side = SideTrade.Sell,
            Account = new Limit() { AccountId = 10012, },
            TimeInForce = TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            MinQty = 0,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.StopLimit,
            ParticipatorId = 10,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private OrderEngine CriaOrderStopLimitVenda()
        => new OrderEngine()
        {
            Symbol = "btcusd",
            Side = SideTrade.Sell,
            Account = new Limit() { AccountId = 10012, },
            TimeInForce = TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            MinQty = 0,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.StopLimit,
            ParticipatorId = 10,
            StopPrice = 0,
            OrigClOrdID = 0,
        };


    [Fact(DisplayName = "Executar Ordem Compra Market com sucesso")]
    public async Task Executar_Ordem_Compra_Market_Com_Sucesso()
    {
        var command = new MatchingMarketCommand(CriaOrderMarketCompra());

        _handler = new MatchingCommandHandler(_matchingRepository,
                                              _executedTradeRepository,
                                              _mediatorHandler,
                                              _notificationHandler,
                                              _distributedLockProvider);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.Item1.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Venda Market Com sucesso")]
    public async Task Executar_Ordem_Venda_Market_Com_Sucesso()
    {
        var command = new MatchingMarketCommand(CriaOrderMarketVenda());

        _handler = new MatchingCommandHandler(_matchingRepository,
                                              _executedTradeRepository,
                                              _mediatorHandler,
                                              _notificationHandler,
                                              _distributedLockProvider);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.Item1.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Compra Limit Com sucesso")]
    public async Task Executar_Ordem_Compra_Limit_Com_Sucesso()
    {
        var command = new MatchingLimitCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_matchingRepository,
                                              _executedTradeRepository,
                                              _mediatorHandler,
                                              _notificationHandler,
                                              _distributedLockProvider);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.Item1.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Venda Limit Com sucesso")]
    public async Task Executar_Ordem_Venda_Limit_Com_Sucesso()
    {
        var command = new MatchingLimitCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_matchingRepository,
                                              _executedTradeRepository,
                                              _mediatorHandler,
                                              _notificationHandler,
                                              _distributedLockProvider);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.Item1.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Compra Stop Com sucesso")]
    public async Task Executar_Ordem_Compra_Stop_Com_Sucesso()
    {
        var command = new MatchingStopCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_matchingRepository,
                                              _executedTradeRepository,
                                              _mediatorHandler,
                                              _notificationHandler,
                                              _distributedLockProvider);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.Item1.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Venda Stop Limit Com sucesso")]
    public async Task Executar_Ordem_Venda_Stop_Com_Sucesso()
    {
        var command = new MatchingStopCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_matchingRepository,
                                              _executedTradeRepository,
                                              _mediatorHandler,
                                              _notificationHandler,
                                              _distributedLockProvider);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.Item1.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Compra Stop Limit Com sucesso")]
    public async Task Executar_Ordem_Compra_Stop_Limit_Com_Sucesso()
    {
        var command = new MatchingStopLimitCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_matchingRepository,
                                              _executedTradeRepository,
                                              _mediatorHandler,
                                              _notificationHandler,
                                              _distributedLockProvider);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.Item1.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Venda Stop Limit Com sucesso")]
    public async Task Executar_Ordem_Venda_Stop_Limit_Com_Sucesso()
    {
        var command = new MatchingLimitCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_matchingRepository,
                                              _executedTradeRepository,
                                              _mediatorHandler,
                                              _notificationHandler,
                                              _distributedLockProvider);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.Item1.Should().Be(OrderStatus.Filled);
    }
}