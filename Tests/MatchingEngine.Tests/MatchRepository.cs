using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
namespace MatchingX.Tests;
public class MatchRepository
{
    //private readonly Mock<IOrderBookRepository> _orderBookRepositoryMock;
    //private readonly Mock<IOrderTradeRepository> _orderTradeRepositoryMock;
    //private readonly Mock<ILogger<Infrastructure.Repositories.OrderTradeRepository>> _loggerOrderTradeMock;
    //private readonly Mock<ILogger<Infrastructure.Repositories.OrderBookRepository>> _loggerOrderBookMock;
    //private readonly Mock<IMapper> _mapperMock;
    //private readonly Mock<IOrderBookContext> _orderBookContextMock;
    //private readonly Mock<IOrderTradeContext> _orderTradeContextMock;
    //IMapper _mapper;
    //MapperConfiguration _config;
    //private BookLevel[] _bids;
    //private BookLevel[] _asks;
    //IList<BookLevel> _quotes = new List<BookLevel>();

    //private Dictionary<string, string> myConfiguration = new Dictionary<string, string>
    //{
    //    {"DatabaseSettings:ConnectionString","mongodb://root:example@mongodb:27017"},
    //    {"DatabaseSettings:DatabaseName","OrderBookStore"},
    //    {"DatabaseSettings:CollectionName","orderbook"},
    //    {"DatabaseSettings:CollectionNameTrade","ordertrade"},
    //};


    //private async void UpdateQuotes()
    //{
    //    _quotes.Clear();
    //    for (int i = 0; i < 100; i++)
    //        _quotes.Add(new BookLevel() { Amount = i + 0.56, Price = i + 0.87, Side = OrderBookSide.Ask });
    //}

    //private async void UpdateBidsAsksAsync()
    //{
    //    var bids = new List<BookLevel>();
    //    var asks = new List<BookLevel>();

    //    for (int i = 0; i < 10; i++)
    //    {
    //        bids.Add(new BookLevel() { Amount = i + 0.1544, Price = i + 0.454, Side = OrderBookSide.Bid, OrderId = 0 });
    //        asks.Add(new BookLevel() { Amount = i + 0.1544, Price = i + 0.454, Side = OrderBookSide.Ask, OrderId = 0 });
    //    }
    //    _bids = bids.ToArray();
    //    _asks = asks.ToArray();
    //}

    //public OrderBookRepository()
    //{
    //    _orderTradeRepositoryMock = new();
    //    _orderBookRepositoryMock = new();
    //    _loggerOrderTradeMock = new();
    //    _loggerOrderBookMock = new();

    //    _orderBookContextMock = new();
    //    _orderTradeContextMock = new();


    //    _config = new MapperConfiguration(cfg => cfg.AddMaps(
    //        new[] {
    //            typeof(DomainToViewModelMappingProfile),
    //            typeof(ViewModelToDomainMappingProfile)
    //        }));

    //    _mapper = _config.CreateMapper();
    //}

    //[Fact]
    //private async Task CreateOrderBookAsync__Should_ReturnFalseResult_When_Insert_OrderBook_IsNull()
    //{
    //    // Arrange
    //    var configuration = new ConfigurationBuilder()
    //        .AddInMemoryCollection(myConfiguration)
    //        .Build();
    //    var orderBookContext = new OrderBookContext(configuration);
    //    IEnumerable<WriteModel<OrderBookRoot>> list = new List<WriteModel<OrderBookRoot>>();


    //    _orderBookContextMock
    //        .Setup(x => x.OrderBooks.BulkWriteAsync(list, default, default))
    //        .ReturnsAsync(It.IsAny<BulkWriteResult<OrderBookRoot>>);

    //    // Action
    //    var orderBookRepository = new Infrastructure.Repositories
    //        .OrderBookRepository(_orderBookContextMock.Object, _loggerOrderBookMock.Object);

    //    var orderBook = orderBookRepository.CreateOrderBookAsync(It.IsAny<OrderBookRoot>()).Result;

    //    //Assert  
    //    Assert.NotNull(orderBook);
    //    Assert.IsAssignableFrom<bool>(orderBook);
    //}
    //[Fact]
    //private async Task CreateOrderBookAsync__Should_ReturnTrueResult_When_Insert_OrderBook_OK()
    //{
    //    // Arrange
    //    var orderBookRoot = new OrderBookRoot("btcusd", DateTime.Now, DateTime.Now, _bids, _asks);

    //    var configuration = new ConfigurationBuilder()
    //        .AddInMemoryCollection(myConfiguration)
    //        .Build();

    //    var list = new List<WriteModel<OrderBookRoot>>();
    //    list.Add(new InsertOneModel<OrderBookRoot>(orderBookRoot));

    //    var returnBulkResponse = (BulkWriteResult<OrderBookRoot>)new BulkWriteResult<OrderBookRoot>.Acknowledged(10, 0, 0, 10,
    //            10, new List<WriteModel<OrderBookRoot>>(), new List<BulkWriteUpsert>());

    //    var mockCollection = new Mock<IMongoCollection<OrderBookRoot>>();

    //    mockCollection.Setup(s
    //                => s.BulkWriteAsync(It.IsAny<IEnumerable<WriteModel<OrderBookRoot>>>(), null,
    //                    new CancellationToken()))
    //            .Returns(Task.FromResult(returnBulkResponse));

    //    _orderBookContextMock
    //        .Setup(x => x.OrderBooks)
    //        .Returns(mockCollection.Object);

    //    // Action
    //    var orderBookRepository = new Infrastructure.Repositories
    //        .OrderBookRepository(_orderBookContextMock.Object, _loggerOrderBookMock.Object);

    //    var orderBook = orderBookRepository.CreateOrderBookAsync(orderBookRoot).Result;

    //    //Assert  
    //    Assert.NotNull(orderBook);
    //    Assert.IsAssignableFrom<bool>(orderBook);
    //    Assert.True(orderBook);
    //}

    //[Fact]
    //private async Task UpdateOrderBookAsync_Should_ReturnFalseResult_When_Insert_OrderBook_IsNull()
    //{
    //    // Arrange
    //    var configuration = new ConfigurationBuilder()
    //        .AddInMemoryCollection(myConfiguration)
    //        .Build();
    //    var orderBookContext = new OrderBookContext(configuration);
    //    IEnumerable<WriteModel<OrderBookRoot>> list = new List<WriteModel<OrderBookRoot>>();


    //    _orderBookContextMock
    //        .Setup(x => x.OrderBooks.BulkWriteAsync(list, default, default))
    //        .ReturnsAsync(It.IsAny<BulkWriteResult<OrderBookRoot>>);

    //    // Action
    //    var orderBookRepository = new Infrastructure.Repositories
    //        .OrderBookRepository(_orderBookContextMock.Object, _loggerOrderBookMock.Object);

    //    var orderBook = orderBookRepository.UpdateOrderBookAsync(It.IsAny<OrderBookRoot>()).Result;

    //    //Assert  
    //    Assert.NotNull(orderBook);
    //    Assert.IsAssignableFrom<bool>(orderBook);
    //}
    //[Fact]
    //private async Task UpdateOrderBookAsync_Should_ReturnTrueResult_When_Insert_OrderBook_OK()
    //{
    //    // Arrange
    //    var orderBookRoot = new OrderBookRoot("btcusd", DateTime.Now, DateTime.Now, _bids, _asks);

    //    var configuration = new ConfigurationBuilder()
    //        .AddInMemoryCollection(myConfiguration)
    //        .Build();

    //    var list = new List<WriteModel<OrderBookRoot>>();
    //    list.Add(new InsertOneModel<OrderBookRoot>(orderBookRoot));

    //    var returnBulkResponse = (BulkWriteResult<OrderBookRoot>)new BulkWriteResult<OrderBookRoot>.Acknowledged(10, 0, 0, 10,
    //            10, new List<WriteModel<OrderBookRoot>>(), new List<BulkWriteUpsert>());

    //    var mockCollection = new Mock<IMongoCollection<OrderBookRoot>>();

    //    mockCollection.Setup(s
    //                => s.BulkWriteAsync(It.IsAny<IEnumerable<WriteModel<OrderBookRoot>>>(), null,
    //                    new CancellationToken()))
    //            .Returns(Task.FromResult(returnBulkResponse));

    //    _orderBookContextMock
    //        .Setup(x => x.OrderBooks)
    //        .Returns(mockCollection.Object);

    //    // Action
    //    var orderBookRepository = new Infrastructure.Repositories
    //        .OrderBookRepository(_orderBookContextMock.Object, _loggerOrderBookMock.Object);

    //    var orderBook = orderBookRepository.UpdateOrderBookAsync(orderBookRoot).Result;

    //    //Assert  
    //    Assert.NotNull(orderBook);
    //    Assert.IsAssignableFrom<bool>(orderBook);
    //    Assert.True(orderBook);
    //}


    //[Fact]
    //private async Task CreateOrderTradeAsync_Should_ReturnFalseResult_When_Insert_OrderBook_IsNull()
    //{
    //    // Arrange
    //    var configuration = new ConfigurationBuilder()
    //        .AddInMemoryCollection(myConfiguration)
    //        .Build();
    //    var orderBookContext = new OrderTradeContext(configuration);
    //    IEnumerable<WriteModel<OrderTrade>> list = new List<WriteModel<OrderTrade>>();


    //    _orderTradeContextMock
    //        .Setup(x => x.OrderTrade.BulkWriteAsync(list, default, default))
    //        .ReturnsAsync(It.IsAny<BulkWriteResult<OrderTrade>>);

    //    // Action
    //    var orderTradeRepository = new Infrastructure.Repositories
    //        .OrderTradeRepository(_orderTradeContextMock.Object, _loggerOrderTradeMock.Object);

    //    var orderTrade = orderTradeRepository.CreateOrderTradeAsync(It.IsAny<OrderTrade>()).Result;

    //    //Assert  
    //    Assert.NotNull(orderTrade);
    //    Assert.IsAssignableFrom<bool>(orderTrade);
    //}

    //[Fact]
    //private async Task CreateOrderTradeAsync_Should_ReturnTrueResult_When_Insert_OrderBook_OK()
    //{
    //    // Arrange
    //    UpdateQuotes();
    //    double quantityRequested = 100, amountShaved = 99.8989878, totalPriceShaved = 121.45;
    //    var orderTrade = new OrderTrade("btcusd", quantityRequested, TradeSide.Buy, _quotes, amountShaved, totalPriceShaved);

    //    var configuration = new ConfigurationBuilder()
    //        .AddInMemoryCollection(myConfiguration)
    //        .Build();

    //    var list = new List<WriteModel<OrderTrade>>();
    //    list.Add(new InsertOneModel<OrderTrade>(orderTrade));

    //    var returnBulkResponse = (BulkWriteResult<OrderTrade>)new BulkWriteResult<OrderTrade>.Acknowledged(10, 0, 0, 10,
    //            10, new List<WriteModel<OrderTrade>>(), new List<BulkWriteUpsert>());

    //    var mockCollection = new Mock<IMongoCollection<OrderTrade>>();

    //    mockCollection.Setup(s
    //                => s.BulkWriteAsync(It.IsAny<IEnumerable<WriteModel<OrderTrade>>>(), null,
    //                    new CancellationToken()))
    //            .Returns(Task.FromResult(returnBulkResponse));

    //    _orderTradeContextMock
    //        .Setup(x => x.OrderTrade)
    //        .Returns(mockCollection.Object);

    //    // Action
    //    var orderTradeRepository = new Infrastructure.Repositories
    //        .OrderTradeRepository(_orderTradeContextMock.Object, _loggerOrderTradeMock.Object);

    //    var orderBook = orderTradeRepository.CreateOrderTradeAsync(orderTrade).Result;

    //    //Assert  
    //    Assert.NotNull(orderBook);
    //    Assert.IsAssignableFrom<bool>(orderBook);
    //    Assert.True(orderBook);
    //}

    //[Fact]
    //private async Task GetOrderBooksAsync_Should_ReturnFalseResult_When_Insert_OrderBook_IsNull()
    //{
    //    // Arrange
    //    var configuration = new ConfigurationBuilder()
    //        .AddInMemoryCollection(myConfiguration)
    //        .Build();
    //    var orderBookContext = new OrderBookContext(configuration);
    //    IEnumerable<WriteModel<OrderBookRoot>> list = new List<WriteModel<OrderBookRoot>>();


    //    _orderBookContextMock
    //        .Setup(x => x.OrderBooks.BulkWriteAsync(list, default, default))
    //        .ReturnsAsync(It.IsAny<BulkWriteResult<OrderBookRoot>>);

    //    // Action
    //    var orderBookRepository = new Infrastructure.Repositories
    //        .OrderBookRepository(_orderBookContextMock.Object, _loggerOrderBookMock.Object);

    //    var orderBook = orderBookRepository.GetOrderBooksAsync(It.IsAny<OrderBookSpecParams>()).Result;

    //    //Assert  
    //    Assert.Null(orderBook);
    //    //Assert.IsAssignableFrom<bool>(orderBook);
    //}
    //[Fact]
    //private async Task GetOrderBooksAsync_Should_ReturnTrueResult_When_Insert_OrderBook_OK()
    //{
    //    // Arrange
    //    var orderBookRoot = new OrderBookRoot("btcusd", DateTime.Now, DateTime.Now, _bids, _asks);

    //    var configuration = new ConfigurationBuilder()
    //        .AddInMemoryCollection(myConfiguration)
    //        .Build();

    //    //var expectResult = Fixture

    //    var specParams = new OrderBookSpecParams();
    //    specParams.Search = "{Id:fasf}";
    //    var mockCollection = new Mock<IMongoCollection<OrderBookRoot>>();

    //    var asyncCursor = new Mock<IAsyncCursor<OrderBookRoot>>();

    //    asyncCursor.Setup(x => x.MoveNext(default)).Returns(true);
    //    //asyncCursor.SetupGet(x => x.Current).Returns();

    //    mockCollection.Setup(s =>
    //               s.FindSync(
    //                   Builders<OrderBookRoot>.Filter.Empty,
    //                   It.IsAny<FindOptions<OrderBookRoot>>(),
    //                   default))
    //            .Returns(asyncCursor.Object);

    //    _orderBookContextMock
    //        .Setup(x => x.OrderBooks)
    //        .Returns(mockCollection.Object);

    //    // Action
    //    var orderBookRepository = new Infrastructure.Repositories
    //        .OrderBookRepository(_orderBookContextMock.Object, _loggerOrderBookMock.Object);

    //    var orderBook = orderBookRepository.GetOrderBooksAsync(specParams).Result;

    //    //Assert  
    //    Assert.NotNull(orderBook);
    //    //Assert.IsAssignableFrom<bool>(orderBook);
    //    //Assert.RaisesAny(orderBook.ToList().Any());
    //}
}