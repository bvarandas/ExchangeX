using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Security.API.Controllers;
using Security.Application.Services;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SharedX.Core.Entities;
namespace SecurityEngineTests;
public class SecurityControllerTests
{
    

    [Fact]
    public async void Get_ReturnsAViewResult_WithAListOfSecurities()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockSecurity = new Mock<ISecurityService>();
        var mockSecurityCache = new Mock<ISecurityCache>();

        //var mockNotifications = new Mock<INotificationHandler<DomainNotification>>();
        var mockNotifications = new Mock<DomainNotificationHandler>();

        mockSecurity.Setup(repo => repo.Get(null!, cancellationToken))

            .ReturnsAsync(GetTestSecurities());
        var controller = new SecurityController(mockSecurity.Object, mockSecurityCache.Object, mockNotifications.Object);

        // Act
        var result = await controller.GetAll(cancellationToken);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<SecurityEngine>>(
            viewResult.ViewData.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async void Detail_ReturnsAViewResult_WithAListOfSecurities()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockSecurity = new Mock<ISecurityService>();
        var mockSecurityCache = new Mock<ISecurityCache>();

        //var mockNotifications = new Mock<INotificationHandler<DomainNotification>>();
        var mockNotifications = new Mock<DomainNotificationHandler>();

        var ids = new string[] { "323" };
        mockSecurity.Setup(repo => repo.Get(ids, cancellationToken))

            .ReturnsAsync(GetTestSecurities());
        var controller = new SecurityController(mockSecurity.Object, mockSecurityCache.Object, mockNotifications.Object);

        // Act
        
        var result = await controller.Detail( "323", cancellationToken);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<SecurityEngine>>(
            viewResult.ViewData.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async void Index_ReturnsAViewResult_WithAListOfSecurities()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockSecurity = new Mock<ISecurityService>();
        var mockSecurityCache = new Mock<ISecurityCache>();

        //var mockNotifications = new Mock<INotificationHandler<DomainNotification>>();
        var mockNotifications = new Mock<DomainNotificationHandler>();

        mockSecurity.Setup(repo => repo.Get(null, cancellationToken))           

            .ReturnsAsync(GetTestSecurities());
        var controller = new SecurityController(mockSecurity.Object, mockSecurityCache.Object, mockNotifications.Object);

        // Act
        var result = await controller.Index(cancellationToken);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<SecurityEngine>>(
            viewResult.ViewData.Model);
        Assert.Equal(2, model.Count());
    }
    [Fact]
    public async void Add_ReturnsAViewResult_WhenMOdelStateIsInvalid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockSecurity = new Mock<ISecurityService>();
        var mockSecurityCache = new Mock<ISecurityCache>();

        //var mockNotifications = new Mock<INotificationHandler<DomainNotification>>();
        var mockNotifications = new Mock<DomainNotificationHandler>();

        mockSecurity.Setup(repo => repo.Add(GetTestSecurity(), mockSecurityCache.Object, cancellationToken))
            .ReturnsAsync(GetResultAddOk);
            //.ReturnsAsync(GetResultAddOk());

        var controller = new SecurityController(mockSecurity.Object, mockSecurityCache.Object, mockNotifications.Object);
        controller.ModelState.AddModelError("SessionName", "Required");

        // Act
        var result = await controller.Add(GetTestSecurity(),cancellationToken);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }
    [Fact]
    public async void Add_ReturnsAViewResult_WhenMOdelStateIsValid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockSecurity = new Mock<ISecurityService>();
        var mockSecurityCache = new Mock<ISecurityCache>();

        //var mockNotifications = new Mock<INotificationHandler<DomainNotification>>();
        var mockNotifications = new Mock<DomainNotificationHandler>();

        mockSecurity.Setup(repo => repo.Add(GetTestSecurity(), mockSecurityCache.Object, cancellationToken))
            .ReturnsAsync(GetResultAddOk);
        //.ReturnsAsync(GetResultAddOk());

        var controller = new SecurityController(mockSecurity.Object, mockSecurityCache.Object, mockNotifications.Object);
        //controller.ModelState.AddModelError("SessionName", "Required");

        // Act
        var result = await controller.Add(GetTestSecurity(), cancellationToken);
        
        // Assert
        var ActionResult = Assert.IsType<OkObjectResult>(result);
        //Assert.Null(redirectToActionResult.ControllerName);
        //Assert.Equal("Post", ActionResult.ActionName);
        mockSecurity.Verify();
    }

    [Fact]
    public async void Update_ReturnsAViewResult_WhenMOdelStateIsInvalid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockSecurity = new Mock<ISecurityService>();
        var mockSecurityCache = new Mock<ISecurityCache>();

        //var mockNotifications = new Mock<INotificationHandler<DomainNotification>>();
        var mockNotifications = new Mock<DomainNotificationHandler>();

        mockSecurity.Setup(repo => repo.Update(GetTestSecurity(), mockSecurityCache.Object, cancellationToken))
            .ReturnsAsync(GetResultAddOk);
        //.ReturnsAsync(GetResultAddOk());

        var controller = new SecurityController(mockSecurity.Object, mockSecurityCache.Object, mockNotifications.Object);
        controller.ModelState.AddModelError("SessionName", "Required");

        // Act
        var result = await controller.Update(GetTestSecurity(), cancellationToken);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact]
    public async void Update_ReturnsAViewResult_WhenMOdelStateIsValid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockSecurity = new Mock<ISecurityService>();
        var mockSecurityCache = new Mock<ISecurityCache>();

        //var mockNotifications = new Mock<INotificationHandler<DomainNotification>>();
        var mockNotifications = new Mock<DomainNotificationHandler>();

        mockSecurity.Setup(repo => repo.Update(GetTestSecurity(), mockSecurityCache.Object, cancellationToken))
            .ReturnsAsync(GetResultAddOk);
        //.ReturnsAsync(GetResultAddOk());

        var controller = new SecurityController(mockSecurity.Object, mockSecurityCache.Object, mockNotifications.Object);
        //controller.ModelState.AddModelError("SessionName", "Required");

        // Act
        var result = await controller.Update(GetTestSecurity(), cancellationToken);

        // Assert
        // Assert
        var ActionResult = Assert.IsType<OkObjectResult>(result);
        //Assert.Null(redirectToActionResult.ControllerName);
        //Assert.Equal("Post", ActionResult.ActionName);
        mockSecurity.Verify();
    }

    [Fact]
    public async void Remove_ReturnsAViewResult_WhenMOdelStateIsInvalid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockSecurity = new Mock<ISecurityService>();
        var mockSecurityCache = new Mock<ISecurityCache>();

        //var mockNotifications = new Mock<INotificationHandler<DomainNotification>>();
        var mockNotifications = new Mock<DomainNotificationHandler>();

        mockSecurity.Setup(repo => repo.Delete(GetTestSecurity(), mockSecurityCache.Object, cancellationToken))
            .ReturnsAsync(GetResultAddOk);
        //.ReturnsAsync(GetResultAddOk());

        var controller = new SecurityController(mockSecurity.Object, mockSecurityCache.Object, mockNotifications.Object);
        controller.ModelState.AddModelError("SessionName", "Required");

        // Act
        var result = await controller.Remove(GetTestSecurity(), cancellationToken);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact]
    public async void Remove_ReturnsAViewResult_WhenMOdelStateIsValid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockSecurity = new Mock<ISecurityService>();
        var mockSecurityCache = new Mock<ISecurityCache>();

        //var mockNotifications = new Mock<INotificationHandler<DomainNotification>>();
        var mockNotifications = new Mock<DomainNotificationHandler>();

        mockSecurity.Setup(repo => repo.Delete(GetTestSecurity(), mockSecurityCache.Object, cancellationToken))
            .ReturnsAsync(GetResultAddOk);
        //.ReturnsAsync(GetResultAddOk());

        var controller = new SecurityController(mockSecurity.Object, mockSecurityCache.Object, mockNotifications.Object);
        //controller.ModelState.AddModelError("SessionName", "Required");

        // Act
        var result = await controller.Remove(GetTestSecurity(), cancellationToken);

        // Assert
        var ActionResult = Assert.IsType<OkObjectResult>(result);
        //Assert.Null(redirectToActionResult.ControllerName);
        //Assert.Equal("Post", ActionResult.ActionName);
        mockSecurity.Verify();
    }

    private Result<bool> GetResultAddOk()
    {
        var result = Result.Ok(true);
        return result;
    }
    private SecurityEngine GetTestSecurity()
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
            Id = Guid.NewGuid().ToString(),
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