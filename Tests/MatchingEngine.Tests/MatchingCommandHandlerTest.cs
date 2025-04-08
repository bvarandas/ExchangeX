using FluentAssertions;
using MacthingX.Application.Commands.Match.OrderType;
using MacthingX.Application.Handlers;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Notifications;
using MatchingX.Core.Repositories;
using Medallion.Threading;
using MediatR;
using Moq;
using SharedX.Core.Bus;
using SharedX.Core.Enums;


namespace MatchingX.Test;

public class MatchingCommandHandlerTest
//:
//IClassFixture<IMatchingRepository>,
//IClassFixture<IExecutedTradeRepository>,
//IClassFixture<IMediatorHandler>,
//IClassFixture<INotificationHandler<DomainNotification>>,
//IClassFixture<IDistributedLockProvider>
{
    private readonly DomainNotificationHandler _domainNotificationHandler;
    private readonly Mock<IMatchingRepository> _mockMatchingRepository;
    private readonly Mock<IExecutedTradeRepository> _mockExecutedTradeRepository;
    private readonly Mock<IMediatorHandler> _mockMediatorHandler;
    private readonly Mock<IDistributedLockProvider> _mockDistributedLockProvider;
    private readonly Mock<INotificationHandler<DomainNotification>> _mockNotificationHandler;
    private readonly Mock<IMatchingCache> _mockMatchingCache;

    private CommandHandler commandHandler = null!;
    private MatchingCommandHandler _handler = null!;

    public MatchingCommandHandlerTest()
    {
        _domainNotificationHandler = new DomainNotificationHandler();
        _mockMatchingRepository = new Mock<IMatchingRepository>();
        _mockExecutedTradeRepository = new Mock<IExecutedTradeRepository>();
        _mockMediatorHandler = new Mock<IMediatorHandler>();
        _mockDistributedLockProvider = new Mock<IDistributedLockProvider>();
        _mockNotificationHandler = new Mock<INotificationHandler<DomainNotification>>();

        _mockMatchingCache = new Mock<IMatchingCache>();

        commandHandler = new CommandHandler(_mockMediatorHandler.Object, _domainNotificationHandler, _mockMatchingCache.Object);
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

    private MatchOrder CriaOrderMarketCompra()
        => new MatchOrder()
        {
            Symbol = "btcusd",
            Side = SideTrade.Buy,
            AccountId = 10012,
            TimeInForce = SharedX.Core.Enums.TimeInForce.FOK,
            Execution = Execution.ToOpen,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Market,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private MatchOrder CriaOrderLimitCompra()
        => new MatchOrder()
        {
            Symbol = "btcusd",
            Side = SideTrade.Buy,
            TimeInForce = SharedX.Core.Enums.TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Limit,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private MatchOrder CriaOrderMarketVenda()
        => new MatchOrder()
        {
            Symbol = "btcusd",
            Side = SideTrade.Sell,
            TimeInForce = SharedX.Core.Enums.TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,

            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Market,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private MatchOrder CriaOrderLimitVenda()
        => new MatchOrder()
        {
            Symbol = "btcusd",
            Side = SideTrade.Sell,
            TimeInForce = SharedX.Core.Enums.TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Limit,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private MatchOrder CriaOrderStopCompra()
        => new MatchOrder()
        {
            Symbol = "btcusd",
            Side = SideTrade.Buy,
            TimeInForce = SharedX.Core.Enums.TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Stop,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private MatchOrder CriaOrderStopVenda()
        => new MatchOrder()
        {
            Symbol = "btcusd",
            Side = SideTrade.Sell,
            TimeInForce = SharedX.Core.Enums.TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.Stop,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private MatchOrder CriaOrderStopLimitCompra()
        => new MatchOrder()
        {
            Symbol = "btcusd",
            Side = SideTrade.Sell,
            TimeInForce = SharedX.Core.Enums.TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.StopLimit,
            StopPrice = 0,
            OrigClOrdID = 0,
        };
    private MatchOrder CriaOrderStopLimitVenda()
        => new MatchOrder()
        {
            Symbol = "btcusd",
            Side = SideTrade.Sell,
            TimeInForce = SharedX.Core.Enums.TimeInForce.FOK,
            AccountId = 10012,
            Execution = Execution.ToOpen,
            Quantity = 0.5M,
            Price = 65802M,
            LastPrice = 0,
            ClOrdID = 123,
            LastQuantity = 0.5M,//LastQuantity = Quantity,
            OrderStatus = OrderStatus.New,
            OrderType = OrderType.StopLimit,
            StopPrice = 0,
            OrigClOrdID = 0,
        };


    [Fact(DisplayName = "Executar Ordem Compra Market com sucesso")]
    public async Task Executar_Ordem_Compra_Market_Com_Sucesso()
    {
        var command = new MatchingMarketCommand(CriaOrderMarketCompra());

        _handler = new MatchingCommandHandler(_mockMatchingRepository.Object,
                                              _mockExecutedTradeRepository.Object,
                                              _mockMediatorHandler.Object,
                                              _domainNotificationHandler,
                                              _mockDistributedLockProvider.Object,
                                              _mockMatchingCache.Object);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.PrincipalOrder.OrderStatus.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Venda Market Com sucesso")]
    public async Task Executar_Ordem_Venda_Market_Com_Sucesso()
    {
        var command = new MatchingMarketCommand(CriaOrderMarketVenda());

        _handler = new MatchingCommandHandler(_mockMatchingRepository.Object,
                                              _mockExecutedTradeRepository.Object,
                                              _mockMediatorHandler.Object,
                                              _mockNotificationHandler.Object,
                                              _mockDistributedLockProvider.Object,
                                              _mockMatchingCache.Object);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.PrincipalOrder.OrderStatus.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Compra Limit Com sucesso")]
    public async Task Executar_Ordem_Compra_Limit_Com_Sucesso()
    {
        var command = new MatchingLimitCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_mockMatchingRepository.Object,
                                              _mockExecutedTradeRepository.Object,
                                              _mockMediatorHandler.Object,
                                              _domainNotificationHandler,
                                              _mockDistributedLockProvider.Object,
                                              _mockMatchingCache.Object);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.PrincipalOrder.OrderStatus.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Venda Limit Com sucesso")]
    public async Task Executar_Ordem_Venda_Limit_Com_Sucesso()
    {
        var command = new MatchingLimitCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_mockMatchingRepository.Object,
                                              _mockExecutedTradeRepository.Object,
                                              _mockMediatorHandler.Object,
                                              _mockNotificationHandler.Object,
                                              _mockDistributedLockProvider.Object,
                                              _mockMatchingCache.Object);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.PrincipalOrder.OrderStatus.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Compra Stop Com sucesso")]
    public async Task Executar_Ordem_Compra_Stop_Com_Sucesso()
    {
        var command = new MatchingStopCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_mockMatchingRepository.Object,
                                              _mockExecutedTradeRepository.Object,
                                              _mockMediatorHandler.Object,
                                              _mockNotificationHandler.Object,
                                              _mockDistributedLockProvider.Object,
                                              _mockMatchingCache.Object);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.PrincipalOrder.OrderStatus.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Venda Stop Limit Com sucesso")]
    public async Task Executar_Ordem_Venda_Stop_Com_Sucesso()
    {
        var command = new MatchingStopCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_mockMatchingRepository.Object,
                                              _mockExecutedTradeRepository.Object,
                                              _mockMediatorHandler.Object,
                                              _mockNotificationHandler.Object,
                                              _mockDistributedLockProvider.Object,
                                              _mockMatchingCache.Object);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.PrincipalOrder.OrderStatus.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Compra Stop Limit Com sucesso")]
    public async Task Executar_Ordem_Compra_Stop_Limit_Com_Sucesso()
    {
        var command = new MatchingStopLimitCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_mockMatchingRepository.Object,
                                              _mockExecutedTradeRepository.Object,
                                              _mockMediatorHandler.Object,
                                              _mockNotificationHandler.Object,
                                              _mockDistributedLockProvider.Object,
                                              _mockMatchingCache.Object);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.PrincipalOrder.OrderStatus.Should().Be(OrderStatus.Filled);
    }

    [Fact(DisplayName = "Executar Ordem Venda Stop Limit Com sucesso")]
    public async Task Executar_Ordem_Venda_Stop_Limit_Com_Sucesso()
    {
        var command = new MatchingLimitCommand(CriaOrderLimitVenda());

        _handler = new MatchingCommandHandler(_mockMatchingRepository.Object,
                                              _mockExecutedTradeRepository.Object,
                                              _mockMediatorHandler.Object,
                                              _mockNotificationHandler.Object,
                                              _mockDistributedLockProvider.Object,
                                              _mockMatchingCache.Object);

        var result = await _handler.Handle(command, default(CancellationToken));

        result.PrincipalOrder.OrderStatus.Should().Be(OrderStatus.Filled);
    }
}