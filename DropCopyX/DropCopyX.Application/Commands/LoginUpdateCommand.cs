using DropCopyX.Core.Entities;
using SharedX.Core.Commands;
namespace DropCopyX.Application.Commands;
public class LoginUpdateCommand : Command
{
    public readonly Login login;
    public DateTime Timestamp { get; private set; }
    public LoginUpdateCommand(Login login)
    {
        Timestamp = DateTime.Now;
        this.login = login;
    }
}
