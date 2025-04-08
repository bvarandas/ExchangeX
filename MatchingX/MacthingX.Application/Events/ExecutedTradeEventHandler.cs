using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedX.Core.Matching.DropCopy;
using System.Collections.Concurrent;

namespace MacthingX.Application.Events;
public class ExecutedTradeEventHandler :
    INotificationHandler<ExecutedTradeEvent>
{
    protected bool _running;
    private readonly IMatchingCache _cacheMatching;
    private readonly ILogger<ExecutedTradeEventHandler> _logger;
    private readonly Thread ThreadExecutedTrade;
    protected readonly ConcurrentQueue<MatchingEngine> QueueExecutedOrders;

    public ExecutedTradeEventHandler(ILogger<ExecutedTradeEventHandler> logger, IMatchingCache cacheMatching)
    {
        _logger = logger;
        _cacheMatching = cacheMatching;

        QueueExecutedOrders = new ConcurrentQueue<MatchingEngine>();
        ThreadExecutedTrade = new Thread(new ThreadStart(ExecutedTradeOutcome));
        ThreadExecutedTrade.Name = nameof(ExecutedTradeOutcome);
        ThreadExecutedTrade.Start();

        _running = true;
    }


    public async Task Handle(ExecutedTradeEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando executed Trade para o Cache Matching!!");

        QueueExecutedOrders.Enqueue(notification.ExecutedOrders);
    }

    private void ExecutedTradeOutcome()
    {
        while (true)
        {
            if (QueueExecutedOrders.TryDequeue(out MatchingEngine orders))
                CreateReports(orders);

            if (!_running)
                break;

            Thread.Sleep(10);
        }
    }

    public void CreateReports(MatchingEngine orders)
    {
        CreateExecutionReport(orders);
        CreateTradeCapture(orders);
    }

    private Dictionary<long, TradeReport> CreateExecutionReport(MatchingEngine orders)
    {
        var now = DateTime.Now;

        var result = new Dictionary<long, TradeReport>();

        var order = orders.PrincipalOrder;

        var report = new ExecutionReport();
        report.AccountType = order.AccountType; //1= Client and 3 = House
        report.TimeInForce = order.TimeInForce;
        report.StopPrice = order.StopPrice;
        report.Symbol = order.Symbol;
        report.Quantity = order.Quantity;
        report.Side = order.Side;
        report.OrigCLOrdID = order.ClOrdID;
        report.OrderID = order.OrderID;
        report.TradeId = order.OrderID;
        report.ExecID = order.OrderID;
        report.Price = order.Price;
        report.ExecType = 'F';      // Fully or partially 

        result.Add(report.TradeId, report);

        foreach (var orderPart in orders.PartOrders.Values)
        {
            var reportPart = new ExecutionReport();
            reportPart.AccountType = order.AccountType; //1= Client and 3 = House
            reportPart.TimeInForce = orderPart.TimeInForce;
            reportPart.StopPrice = orderPart.StopPrice;
            reportPart.Symbol = orderPart.Symbol;
            reportPart.Quantity = orderPart.Quantity;
            reportPart.Side = orderPart.Side;
            reportPart.OrigCLOrdID = orderPart.ClOrdID;
            reportPart.OrderID = orderPart.OrderID;
            reportPart.TradeId = order.OrderID;
            reportPart.ExecID = order.OrderID;
            reportPart.Price = orderPart.Price;
            reportPart.ExecType = 'F';      // Fully or partially 

            result.Add(reportPart.TradeId, reportPart);
        }

        return result;
    }
    private Dictionary<long, TradeReport> CreateTradeCapture(MatchingEngine orders)
    {
        var result = new Dictionary<long, TradeReport>();

        var order = orders.PrincipalOrder;

        var report = new TradeCaptureReport()
        {
            TradeReportTransType = 0,
            TrdType = 0,
            CopyMsgIndicator = 'Y',
            PreviouslyReported = 'N',
            TradeId = order.OrderID,
            NoSides = 1,
            OrderId = order.OrderID.ToString(),
            ClOrderId = order.ClOrdID.ToString(),
            LastQty = order.LastQuantity,
            LastPx = order.LastPrice,
            Symbol = order.Symbol,
            Side = (char)order.Side,
            Price = order.Price,
            TransactTime = order.TransactTime,
            TradeDate = DateTime.Now.ToString("yyyyMMdd"),
        };

        result.Add(report.TradeId, report);

        foreach (var orderPart in orders.PartOrders.Values)
        {
            var reportPart = new TradeCaptureReport()
            {
                TradeReportTransType = 0,
                TrdType = 0,
                CopyMsgIndicator = 'Y',
                PreviouslyReported = 'N',
                TradeId = order.OrderID,
                NoSides = 1,
                OrderId = orderPart.OrderID.ToString(),
                ClOrderId = orderPart.ClOrdID.ToString(),
                LastQty = orderPart.LastQuantity,
                LastPx = orderPart.LastPrice,
                Symbol = orderPart.Symbol,
                Side = (char)orderPart.Side,
                Price = orderPart.Price,
                TransactTime = orderPart.TransactTime,
                TradeDate = DateTime.Now.ToString("yyyyMMdd"),
            };

            result.Add(reportPart.TradeId, reportPart);
        }

        return result;
    }
}