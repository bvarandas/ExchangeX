using DropCopyX.Core.Entities;
using SharedX.Core.Commands;
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
