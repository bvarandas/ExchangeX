using DropCopyX.Core.Entities;
using SharedX.Core.Commands;
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