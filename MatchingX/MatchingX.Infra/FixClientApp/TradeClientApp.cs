using Microsoft.Extensions.Logging;
using QuickFix;

namespace MatchingX.Infra.FixClientApp;

public class TradeClientApp : MessageCracker, ITradeClientApp
{
    public readonly ILogger<TradeClientApp> _logger;
    Session _session = null;
    public TradeClientApp(ILogger<TradeClientApp> logger) 
    { 
        _logger = logger;
    }

    public void FromAdmin(Message message, SessionID sessionID)
    {
        throw new NotImplementedException();
    }

    public void FromApp(Message message, SessionID sessionID)
    {
        _logger.LogInformation("IN:  " + message.ToString());
        try
        {
            Crack(message, sessionID);
        }
        catch (Exception ex)
        {
            _logger.LogError("==Cracker exception==");
            _logger.LogError(ex.ToString());
            _logger.LogError(ex.StackTrace);
        }
    }

    public void OnCreate(SessionID sessionID)
    {
        _session = Session.LookupSession(sessionID);
    }

    public void OnLogon(SessionID sessionID)
    {
        _logger.LogInformation("Logon - " + sessionID.ToString());
    }

    public void OnLogout(SessionID sessionID)
    {
        _logger.LogInformation("Logout - " + sessionID.ToString());
    }

    public void ToAdmin(Message message, SessionID sessionID)
    {
        throw new NotImplementedException();
    }

    public void ToApp(Message message, SessionID sessionId)
    {
        try
        {
            bool possDupFlag = false;
            if (message.Header.IsSetField(QuickFix.Fields.Tags.PossDupFlag))
            {
                possDupFlag = QuickFix.Fields.Converters.BoolConverter.Convert(
                    message.Header.GetString(QuickFix.Fields.Tags.PossDupFlag)); /// FIXME
            }
            if (possDupFlag)
                throw new DoNotSend();
        }
        catch (FieldNotFoundException ex)
        {
            _logger.LogError(ex.Message);
        }

        _logger.LogInformation("OUT: " + message.ToString());
    }

    #region MessageCracker handlers
    public void OnMessage(QuickFix.FIX44.ExecutionReport m, SessionID s)
    {
        Console.WriteLine("Received execution report");
    }

    public void OnMessage(QuickFix.FIX44.OrderCancelReject m, SessionID s)
    {
        Console.WriteLine("Received order cancel reject");
    }
    #endregion

    public void SendMessage(Message m)
    {
        if (_session != null)
            _session.Send(m);
        else
        {
            // This probably won't ever happen.
            _logger.LogError("Can't send message: session not created.");
        }
    }

}
