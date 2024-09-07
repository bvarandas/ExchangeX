using ProtoBuf;

namespace SharedX.Core.ValueObjects;
[ProtoContract()]
public class EnvelopeOutbox<T> where T : class
{
    [ProtoMember(1)]
    public long Id { get; set; }
    [ProtoMember(2)]
    public ActivityOutbox ActivityOutbox { get; set; } = null!;

    [ProtoMember(3)]
    public T Body { get; set; } = null!;

    [ProtoMember(4)]
    public DateTime LastTransaction { get; set; } = DateTime.Now;
    //[ProtoMember(5)]
    //public ConnectionZmq ConnectionZmq { get; set;} 
}