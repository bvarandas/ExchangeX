﻿using FluentResults;
using SharedX.Core.Matching.MarketData;
namespace MarketDataX.Core.Interfaces;
public interface IMarketDataChache
{
    void AddMarketDataIncremental(MarketData marketData);
    bool TryDequeueMarketData(out MarketData marketData);
    Task<bool> AddMarketDataBook(MarketData marketData);
    Task<Result<Dictionary<long, MarketData>>> GetSnapShotMarketData(string symbol);
}