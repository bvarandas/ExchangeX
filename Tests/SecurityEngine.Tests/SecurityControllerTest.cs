using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Security.API.Controllers;
using Security.Application.Services;
using SecurityX.Core.Notifications;
using SharedX.Core.Entities;
namespace SecurityEngineTests;
public class SecurityControllerTests
{

    [Fact]
    public async void Index_ReturnsAViewResult_WithAListOfSecurities()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var mockSecurity = new Mock<ISecurityService>();
        //var mockNotifications = new Mock<INotificationHandler<DomainNotification>>();
        var mockNotifications = new Mock<DomainNotificationHandler>();

        mockSecurity.Setup(repo => repo.Get(null, cancellationToken))           

            .ReturnsAsync(GetTestSecurities());
        var controller = new SecurityController(mockSecurity.Object, mockNotifications.Object);

        // Act
        var result = await controller.Index(cancellationToken);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<SecurityEngine>>(
            viewResult.ViewData.Model);
        Assert.Equal(2, model.Count());
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