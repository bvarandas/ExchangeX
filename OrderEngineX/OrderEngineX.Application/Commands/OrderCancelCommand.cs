﻿using OrderEngineX.Application.Validations;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Application.Commands.Order;
public class OrderCancelCommand : OrderEngineCommand
{
    private readonly IBookOfferCache _cache;
    public OrderCancelCommand(OrderEngine order, IBookOfferCache cache)
    {
        Timestamp = DateTime.Now;
        Order = order;
        _cache = cache;
    }
    public override bool IsValid()
    {
        ValidationResult = new OrderCancelRequestValidation(_cache).Validate(this);
        return ValidationResult.IsValid;
    }
}