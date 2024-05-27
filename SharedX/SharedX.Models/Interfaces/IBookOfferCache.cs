using FluentResults;
using SharedX.Core.Matching;
namespace SharedX.Core.Interfaces;
public interface IBookOfferCache
{
    Task<Result<OrderBook>> GetBookAsync(string symbol);
    Task<Result<bool>> AddBookItemAsync(Book book);
    Task<Result<bool>> RemoveBookItemAsync(Book book);
}