﻿using MarketDataX.Core.Entities;
using SharedX.Core.Commands;
namespace MarketDataX.Application.Commands;
public class LoginAddCommand : Command
{
    public readonly Login login;
    public DateTime Timestamp { get; private set; }
    public LoginAddCommand(Login login)
    {
        Timestamp = DateTime.Now;
        this.login = login;
    }

    public override bool IsValid()
    {
        throw new NotImplementedException();
    }
}