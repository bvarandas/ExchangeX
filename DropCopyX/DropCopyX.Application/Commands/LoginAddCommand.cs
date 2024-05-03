
using SharedX.Core.Commands;
using SharedX.Core.Entities;

namespace DropCopyX.Application.Commands;
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