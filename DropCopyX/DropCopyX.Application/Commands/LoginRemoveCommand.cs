using SharedX.Core.Commands;
using SharedX.Core.Entities;

namespace DropCopyX.Application.Commands;
public class LoginRemoveCommand : Command
{
    public readonly Login login;
    public DateTime Timestamp { get; private set; }
    public LoginRemoveCommand(Login login)
    {
        Timestamp = DateTime.Now;
        this.login = login;
    }
}
