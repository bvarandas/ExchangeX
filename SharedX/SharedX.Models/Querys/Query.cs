using MediatR;
using System.ComponentModel.DataAnnotations;

namespace SharedX.Core.Querys;

public class Query : INotification 
{
    public DateTime Timestamp { get; private set; }
    public ValidationResult ValidationResult { get; set; }
    protected Query()
    {
        Timestamp = DateTime.Now;
    }
    //public abstract bool IsValid();
}
