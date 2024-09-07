using ProtoBuf;
namespace SharedX.Core.ValueObjects;
[ProtoContract()]
public record ActivityOutbox
{
    [ProtoMember(1)]
    public string Activity { get; set; } = string.Empty;
}