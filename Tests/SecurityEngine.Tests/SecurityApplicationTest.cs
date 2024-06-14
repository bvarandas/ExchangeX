using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Security.API.Controllers;
using Security.Application.Commands;
using Security.Application.Services;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SharedX.Core.Bus;
using SharedX.Core.Entities;
namespace SecurityEngineTests;
public class SecurityApplicationTests
{
    private readonly Mock<ISecurityEngineRepository> _securityRepositoryMock = null!;
    private readonly Mock<IMediatorHandler> _mediatorMock;
    private readonly Mock<ILogger<SecurityService>> _loggerSecurityServiceMock;
    private readonly Mock<SecurityEngineCommandHandler> _handlerSecurityMock;
    public SecurityApplicationTests()
    {
        _securityRepositoryMock = new();
        _mediatorMock = new();
        _loggerSecurityServiceMock = new();
        _handlerSecurityMock = new();
    }

    [Fact]
    public async void Get_ReturnsAViewResult_WithAListOfSecurities()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockNotifications = new Mock<DomainNotificationHandler>();

        string id1 = "btcbrl", id2= "btcusd";

        _securityRepositoryMock.Setup(repo=>repo.GetAllSecurityiesAsync(cancellationToken))
            .ReturnsAsync(GetTestSecurities());
        var service = new SecurityService(_mediatorMock.Object, _securityRepositoryMock.Object, _loggerSecurityServiceMock.Object);

        // Act
        var ids = new string[] { id1, id2 };
        var result = await service.Get(ids, cancellationToken);

        // Assert
        var viewResult = Assert.IsType<Result<Dictionary<string, SecurityEngine>>>(result);
        var model = Assert.IsAssignableFrom<Dictionary<string, SecurityEngine>>(viewResult.Value);
        
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async void Detail_ReturnsAViewResult_WithAListOfSecurities()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string id1 = "btcbrl", id2 = "btcusd";
        var mockNotifications = new Mock<DomainNotificationHandler>();

        var ids = new string[] { id1, id2 };
        _securityRepositoryMock.Setup(repo => repo.GetAllSecurityiesAsync(cancellationToken))
            .ReturnsAsync(GetTestSecurities());
        var service = new SecurityService(_mediatorMock.Object, _securityRepositoryMock.Object, _loggerSecurityServiceMock.Object);

        // Act
        var result = await service.Get(ids, cancellationToken);

        // Assert
        
        var viewResult = Assert.IsType<Result<Dictionary<string, SecurityEngine>>>(result);
        var model = Assert.IsAssignableFrom<Dictionary<string, SecurityEngine>>(viewResult.Value);

        Assert.Equal(2, model.Count());
    }

    
    [Fact]
    public async void Add_ReturnsAViewResult_WhenFail()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        string id = Guid.NewGuid().ToString();
        var securityEngine = GetTestSecurity(id);

        var mockNotifications = new Mock<DomainNotificationHandler>();
        
        _securityRepositoryMock.Setup(repo => repo.UpsertSecurityAsync(securityEngine, cancellationToken))
            .ReturnsAsync(GetResultAddFail);

        _mediatorMock.Setup(m => m.Send(It.IsAny<SecurityNewCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GetResultSendFail);
            
        var service = new SecurityService(_mediatorMock.Object, _securityRepositoryMock.Object, _loggerSecurityServiceMock.Object);
        
        // Act
        var result = await service.Add( securityEngine, cancellationToken);

        // Assert
        var failResult = Assert.IsType<Result<bool>>(result);
        Assert.IsType<bool>(failResult.IsSuccess);
        Assert.True(failResult.IsFailed);
    }
    [Fact]
    public async void Add_ReturnsAViewResult_WhenIsValid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockNotifications = new Mock<DomainNotificationHandler>();
        string id = Guid.NewGuid().ToString();
        var securityEngine = GetTestSecurity(id);

        _securityRepositoryMock.Setup(repo => repo.UpsertSecurityAsync(securityEngine, cancellationToken))
            .ReturnsAsync(GetResultAddOk);

        _mediatorMock.Setup(m => m.Send(It.IsAny<SecurityNewCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GetResultSendOK);

        var service = new SecurityService(_mediatorMock.Object, _securityRepositoryMock.Object, _loggerSecurityServiceMock.Object);

        // Act
        var result = await service.Add(securityEngine, cancellationToken);

        // Assert
        var failResult = Assert.IsType<Result<bool>>(result);
        Assert.IsType<bool>(failResult.IsSuccess);
        Assert.True(failResult.IsSuccess);
        _securityRepositoryMock.Verify();
    }

    [Fact]
    public async void Update_ReturnsAViewResult_WhenIsInvalid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var mockNotifications = new Mock<DomainNotificationHandler>();
        
        string id = Guid.NewGuid().ToString();
        var securityEngine = GetTestSecurity(id);

        _securityRepositoryMock.Setup(repo => repo.UpsertSecurityAsync(securityEngine, cancellationToken))
            .ReturnsAsync(GetResultAddOk);

        _mediatorMock.Setup(m => m.Send(It.IsAny<SecurityUpdateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GetResultSendFail);

        var service = new SecurityService(_mediatorMock.Object, _securityRepositoryMock.Object, _loggerSecurityServiceMock.Object);
        // Act
        var result = await service.Update(securityEngine, cancellationToken);

        // Assert
        var failResult = Assert.IsType<Result<bool>>(result);
        Assert.IsType<bool>(failResult.IsSuccess);
        Assert.True(failResult.IsFailed);
        _securityRepositoryMock.Verify();
    }

    [Fact]
    public async void Update_ReturnsAViewResult_WhenIsValid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var mockNotifications = new Mock<DomainNotificationHandler>();

        string id = Guid.NewGuid().ToString();
        var securityEngine = GetTestSecurity(id);

        _securityRepositoryMock.Setup(repo => repo.UpsertSecurityAsync(securityEngine, cancellationToken))
            .ReturnsAsync(GetResultAddOk);

        _mediatorMock.Setup(m => m.Send(It.IsAny<SecurityUpdateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GetResultSendOK);

        var service = new SecurityService(_mediatorMock.Object, _securityRepositoryMock.Object, _loggerSecurityServiceMock.Object);
        // Act
        var result = await service.Update(securityEngine, cancellationToken);

        // Assert
        var failResult = Assert.IsType<Result<bool>>(result);
        Assert.IsType<bool>(failResult.IsSuccess);
        Assert.True(failResult.IsSuccess);
        _securityRepositoryMock.Verify();
    }

    [Fact]
    public async void Remove_ReturnsAViewResult_WhenIsInvalid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var mockNotifications = new Mock<DomainNotificationHandler>();

        string id = Guid.NewGuid().ToString();
        var securityEngine = GetTestSecurity(id);

        _securityRepositoryMock.Setup(repo => repo.UpsertSecurityAsync(securityEngine, cancellationToken))
            .ReturnsAsync(GetResultAddFail);

        _mediatorMock.Setup(m => m.Send(It.IsAny<SecurityRemoveCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GetResultSendFail);

        var service = new SecurityService(_mediatorMock.Object, _securityRepositoryMock.Object, _loggerSecurityServiceMock.Object);
        // Act
        var result = await service.Delete(securityEngine, cancellationToken);

        // Assert
        var failResult = Assert.IsType<Result<bool>>(result);
        Assert.IsType<bool>(failResult.IsSuccess);
        Assert.True(failResult.IsFailed);
        _securityRepositoryMock.Verify();
    }

    [Fact]
    public async void Remove_ReturnsAViewResult_WhenIsValid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var mockNotifications = new Mock<DomainNotificationHandler>();

        string id = Guid.NewGuid().ToString();
        var securityEngine = GetTestSecurity(id);

        _securityRepositoryMock.Setup(repo => repo.UpsertSecurityAsync(securityEngine, cancellationToken))
            .ReturnsAsync(GetResultAddFail);

        _mediatorMock.Setup(m => m.Send(It.IsAny<SecurityRemoveCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GetResultSendOK);

        var service = new SecurityService(_mediatorMock.Object, _securityRepositoryMock.Object, _loggerSecurityServiceMock.Object);
        // Act
        var result = await service.Delete(securityEngine, cancellationToken);

        // Assert
        var failResult = Assert.IsType<Result<bool>>(result);
        Assert.IsType<bool>(failResult.IsSuccess);
        Assert.True(failResult.IsSuccess);
        _securityRepositoryMock.Verify();
    }

    private Result<bool> GetResultAddOk()
    {
        var result = Result.Ok(true);
        return result;
    }

    private Result GetResultSendOK()
    {
        var result = Result.Ok();
        return result;
    }

    private Result GetResultSendFail()
    {
        var result = Result.Fail("dont work");
        return result;
    }

    private Result<bool> GetResultAddFail()
    {
        var result = Result.Ok(false);
        return  result;
    }

    private SecurityEngine GetTestSecurity(string id)
    {
        return new SecurityEngine()
        {
            CFICode = "",
            ContractMultiplier = 1,
            Currency = "BRL", // USD, 
            ExerciseStyle = 0, //0 = European 1 = American 2 = Bermuda 99 = Other
            HighLimitPrice = 10000000,
            IssueDate = "20240810", //Trading is allowed from this time
            LowLimitPrice = 1,
            MaturityDate = "20290811",//Trading is allowed up to this time.
            MaturityTime = "00:00:00:000",
            MaxTradeVol = 10.0M,
            MinTradeVol = 0.00000100M,
            PutOrCall = 2, // In case SecurityType(167) = OPT  Case subCategory :Call options(C) = 1 Put options(P) = 0 Other(miscellaneous)(M)=2
            SecurityDesc = "btcblr  - Btc com compra e venda em BRL, para compra em dolar use o btcusd",
            SecurityExchange = "SA",            //Sao Paulo Stock Exchange
            SecurityID = Guid.NewGuid().ToString(),
            SecurityIDSource = "8",
            SecurityStatus = "1", //2 = Inactive (applicable only in case instrument was disabled after subscription)       1 = Active
            SecuritySubType = "", //Foreign exchange (F) Commodities(T) Financial futures(F) Commodities futures(C) Call options(C) Put options(P) Other(miscellaneous)(M)
            Id = id,
            SecurityType = "SPOT", // FUT (Future)  OPT(Option) SPOT MLEG(Multileg Instrument) CS(Common Stock) Other
            Symbol = "btcblr",
            SettlementMethod = 'S',
            StrikePrice = 0,
            TradeVolType = 0



        };
    }
    private Result<Dictionary<string, SecurityEngine>> GetTestSecurities()
    {
        var securities = new Dictionary<string,SecurityEngine>();
        securities.Add("btcbrl", new SecurityEngine()
        {
            CFICode = "",
            ContractMultiplier = 1,
            Currency = "BRL", // USD, 
            ExerciseStyle = 0, //0 = European 1 = American 2 = Bermuda 99 = Other
            HighLimitPrice = 10000000,
            IssueDate = "20240810", //Trading is allowed from this time
            LowLimitPrice = 1,
            MaturityDate = "20290811",//Trading is allowed up to this time.
            MaturityTime = "00:00:00:000",
            MaxTradeVol = 10.0M,
            MinTradeVol = 0.00000100M,
            PutOrCall = 2, // In case SecurityType(167) = OPT  Case subCategory :Call options(C) = 1 Put options(P) = 0 Other(miscellaneous)(M)=2
            SecurityDesc = "btcblr  - Btc com compra e venda em BRL, para compra em dolar use o btcusd",
            SecurityExchange = "SA",            //Sao Paulo Stock Exchange
            SecurityID = Guid.NewGuid().ToString(),
            SecurityIDSource = "8",
            SecurityStatus = "1", //2 = Inactive (applicable only in case instrument was disabled after subscription)       1 = Active
            SecuritySubType = "", //Foreign exchange (F) Commodities(T) Financial futures(F) Commodities futures(C) Call options(C) Put options(P) Other(miscellaneous)(M)
            Id = Guid.NewGuid().ToString(),
            SecurityType = "SPOT", // FUT (Future)  OPT(Option) SPOT MLEG(Multileg Instrument) CS(Common Stock) Other
            Symbol = "btcblr",
            SettlementMethod = 'S',
            StrikePrice=0,
            TradeVolType = 0



        });
        securities.Add("btcusd",new SecurityEngine()
        {
            CFICode = "",
            ContractMultiplier = 1,
            Currency = "BRL", // USD, 
            ExerciseStyle = 0, //0 = European 1 = American 2 = Bermuda 99 = Other
            HighLimitPrice = 10000000,
            IssueDate = "20240810", //Trading is allowed from this time
            LowLimitPrice = 1,
            MaturityDate = "20290811",//Trading is allowed up to this time.
            MaturityTime = "00:00:00:000",
            MaxTradeVol = 10.0M,
            MinTradeVol = 0.00000100M,
            PutOrCall = 2, // In case SecurityType(167) = OPT  Case subCategory :Call options(C) = 1 Put options(P) = 0 Other(miscellaneous)(M)=2
            SecurityDesc = "btcusd  - Btc com compra e venda em USD, para compra em reais use o btcbrl",
            SecurityExchange = "SA",            //Sao Paulo Stock Exchange
            SecurityID = Guid.NewGuid().ToString(),
            SecurityIDSource = "8",
            SecurityStatus = "1", //2 = Inactive (applicable only in case instrument was disabled after subscription)       1 = Active
            SecuritySubType = "", //Foreign exchange (F) Commodities(T) Financial futures(F) Commodities futures(C) Call options(C) Put options(P) Other(miscellaneous)(M)
            Id = Guid.NewGuid().ToString(),
            SecurityType = "SPOT", // FUT (Future)  OPT(Option) SPOT MLEG(Multileg Instrument) CS(Common Stock) Other
            Symbol = "btcblr",
            SettlementMethod = 'S',
            StrikePrice = 0,
            TradeVolType =0
        });
        return Result.Ok(securities);
    }
}