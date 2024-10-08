﻿using FluentResults;
using SharedX.Core.Matching.MarketData;
namespace MarketDataX.Core.Interfaces;
public interface ISecurityCache
{
    Task<Result<bool>> UpsertSecurity(Security security);
    Task<Result<Security>> GetSecurity(string symbol, string securityId);
    Task<Result<Dictionary<long, Security>>> GetSnapShotSecuritiesAsync();
    Task<Result<bool>> RemoveSecurity(Security security);
    bool TryDequeueSecurityStatus(out Security security);
}
