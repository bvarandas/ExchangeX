﻿using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.ValueObjects;
namespace SharedX.Core.Interfaces;
public interface IManagerOutboxApp<T> where T : class
{
    void SetActivity(ActivityOutbox activity);
    Task<bool> AddActivityAsync(EnvelopeOutbox<T> envelope);
}