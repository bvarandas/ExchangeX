using OrderEntryX.Core.Entities;
using SharedX.Core.Commands;
namespace OrderEntryX.Application.Commands;
public class LoginAddCommand : Command
{
    public readonly Login login;
    public DateTime Timestamp { get; private set; }
    public LoginAddCommand(Login login)
    {
        Timestamp = DateTime.Now;
        this.login = login;
    }
}