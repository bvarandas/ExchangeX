using FluentResults;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using SharedX.Core.Bus;

namespace MatchingX.Tests;
public class MatchStopServices
{
    //private readonly Mock<IMatchingRepository> _matchingRepositoryMock;
    //private readonly Mock<IExecutedTradeRepository> _executedTradeRepositoryMock;
    //private readonly Mock<ILogger<MatchStopServices>> _loggerOrderBookServiceMock;
    //private readonly Mock<IMediatorHandler> _mediatorMock;
    //public MatchStopServices()
    //{
    //    _matchingRepositoryMock = new();
    //    _executedTradeRepositoryMock = new();
    //    _loggerOrderBookServiceMock = new();
    //    _mediatorMock = new();
    //}

    //[Fact]
    //public async Task AddMatchingCacheAsync_Should_ReturnFalseResult_When_Insert_OrderBookWith_TickerEmpty()
    //{
    //    // Arrange
    //    var ticker = string.Empty;

    //    var orderBook = new Application.Responses.Books.OrderBook();
    //    orderBook.Ticker = ticker;
    //    orderBook.Timestamp = DateTime.UtcNow;
    //    orderBook.Asks = null!;
    //    orderBook.Bids = null!;

    //    var service = new OrderBookService(_mapperMock.Object, _mediatorMock.Object, _loggerOrderBookServiceMock.Object);
    //    // Action
    //    Result<bool> result = await service.AddOrderBookCacheAsync(orderBook);

    //    // Assert
    //    result.IsFailed.Should().BeTrue();
    //}


    //[Fact]
    //public async Task AddMatchingCacheAsync_Should_ReturnSuccessResult_When_Insert_OrderBook()
    //{
    //    // Arrange
    //    var ticker = "btcusd";
    //    UpdateBidsAsksAsync();
    //    var orderBook = new Application.Responses.Books.OrderBook();
    //    orderBook.Ticker = ticker;
    //    orderBook.Timestamp = DateTime.UtcNow;
    //    orderBook.Asks = _asks;
    //    orderBook.Bids = _bids;

    //    var service = new OrderBookService(_mapperMock.Object, _mediatorMock.Object, _loggerOrderBookServiceMock.Object);
    //    // Action
    //    Result<bool> result = await service.AddOrderBookCacheAsync(orderBook);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //}

    //[Fact]
    //public async Task SendOrderTradeAsync_Should_ReturnFailedResult_When_InsertOrderTrade_Hasnt_Quotes()
    //{
    //    // Arrange
    //    var id = ObjectId.GenerateNewId();
    //    var orderBookCommand = new InsertOrderTradeCommand(id.ToString(), "btcusd", 10, TradeSide.Buy, null!, 0.0, 0.0);
    //    var service = new OrderBookService(_mapperMock.Object, _mediatorMock.Object, _loggerOrderBookServiceMock.Object);
    //    // Action
    //    var result = await service.SendOrderTradeAsync(orderBookCommand);

    //    // Assert
    //    Assert.IsType(typeof(OrderTradeViewModel), result);
    //    Assert.Equal(string.Empty, result.Id);
    //    Assert.Equal(0.0, result.TotalPriceShaved);
    //    Assert.Null(result.Quotes);
    //}

    //[Fact]
    //public async Task SendOrderTradeAsync_Should_ReturnFailedResult_When_InsertOrderTrade_Hasnt_Ticker()
    //{
    //    // Arrange
    //    var id = ObjectId.GenerateNewId();
    //    var orderBookCommand = new InsertOrderTradeCommand(id.ToString(), "", 10, TradeSide.Buy, null!, 0.0, 0.0);
    //    var service = new OrderBookService(_mapperMock.Object, _mediatorMock.Object, _loggerOrderBookServiceMock.Object);
    //    // Action
    //    var result = await service.SendOrderTradeAsync(orderBookCommand);

    //    // Assert
    //    Assert.IsType(typeof(OrderTradeViewModel), result);
    //    Assert.Equal(string.Empty, result.Id);
    //    Assert.Equal(0.0, result.TotalPriceShaved);
    //    Assert.Null(result.Quotes);
    //}

    //[Fact]
    //public async Task SendOrderTradeAsync_Should_ReturnFailedResult_When_InsertOrderTrade_Hasnt_QuanityRequest()
    //{
    //    // Arrange
    //    var id = ObjectId.GenerateNewId();
    //    var orderBookCommand = new InsertOrderTradeCommand(id.ToString(), "btcusd", 0, TradeSide.Buy, null!, 0.0, 0.0);
    //    var service = new OrderBookService(_mapperMock.Object, _mediatorMock.Object, _loggerOrderBookServiceMock.Object);
    //    // Action
    //    var result = await service.SendOrderTradeAsync(orderBookCommand);

    //    // Assert
    //    Assert.IsType(typeof(OrderTradeViewModel), result);
    //    Assert.Equal(string.Empty, result.Id);
    //    Assert.Equal(0.0, result.TotalPriceShaved);
    //    Assert.Null(result.Quotes);
    //}

    //[Fact]
    //public async Task SendOrderTradeAsync_Should_ReturnFailedResult_When_InsertOrderTrade_Hasnt_QuanityRequest_IsNegative()
    //{
    //    // Arrange
    //    var id = ObjectId.GenerateNewId();
    //    var orderBookCommand = new InsertOrderTradeCommand(id.ToString(), "btcusd", -10, TradeSide.Buy, null!, 0.0, 0.0);
    //    var service = new OrderBookService(_mapperMock.Object, _mediatorMock.Object, _loggerOrderBookServiceMock.Object);
    //    // Action
    //    var result = await service.SendOrderTradeAsync(orderBookCommand);

    //    // Assert
    //    Assert.IsType(typeof(OrderTradeViewModel), result);
    //    Assert.Equal(string.Empty, result.Id);
    //    Assert.Equal(0.0, result.TotalPriceShaved);
    //    Assert.Null(result.Quotes);
    //}

    //[Fact]
    //public async Task SendOrderTradeAsync_Should_ReturnSuccessResult_When_InsertOrderTrade()
    //{
    //    // Arrange
    //    UpdateBidsAsksAsync();

    //    var id = ObjectId.GenerateNewId();
    //    var orderBookCommand = new InsertOrderTradeCommand(id.ToString(), "btcusd", 10, TradeSide.Buy, null!, 0.0, 0.0);
    //    var service = new OrderBookService(_mapper, _mediatorMock.Object, _loggerOrderBookServiceMock.Object);

    //    // Action
    //    var orderBook = new Application.Responses.Books.OrderBook();
    //    orderBook.Ticker = "btcusd";
    //    orderBook.Timestamp = DateTime.UtcNow;
    //    orderBook.Asks = _asks;
    //    orderBook.Bids = _bids;

    //    await service.AddOrderBookCacheAsync(orderBook);

    //    //_mapperMock.Setup(x => x.Map<IList<OrderBook.Application.Responses.Books.BookLevel>, IList<BookLevelCommand>>(It.IsAny<IList<BookLevel>>())).Returns();

    //    var result = await service.SendOrderTradeAsync(orderBookCommand);

    //    // Assert
    //    Assert.IsType(typeof(OrderTradeViewModel), result);
    //    Assert.True(result.TotalPriceShaved >= 0);
    //    Assert.NotNull(result.Quotes);
    //}



}