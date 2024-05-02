using SharedX.Core.Entities;
namespace MarketDataX.Core.Entities;
public class Login : BaseEntity
{
    public string UserName { get; set; }=string.Empty;
    public string Password { get; set; } = string.Empty;    
    public string ActualIP {  get; set; } = string.Empty;
    public bool Active { get; set; } 
    public IList<string> GrantedIPs { get; set; } = null!;
}