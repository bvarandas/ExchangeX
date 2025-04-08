using MacthingX.Application.Commands.Match;
using MatchingX.Core.Entities;
using MediatR;
namespace MatchingX.Application.Handlers;
public interface IMediatorHandler : ISender, IPublisher
{
    Task<MatchingEngine> SendMatchCommand<T>(T command) where T : MatchCommand;
}