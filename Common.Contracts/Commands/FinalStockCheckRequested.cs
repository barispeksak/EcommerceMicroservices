using System;
using System.Collections.Generic;
namespace Common.Contracts.Commands;

public record FinalStockCheckRequested : Base.Message
{
    public FinalStockCheckRequested(Guid cartId, List<Guid>? items = null)
    {
        CartId = cartId;
        Items = items ?? new List<Guid>();
    }
    
    public Guid CartId { get; init; }
    public List<Guid> Items { get; init; }
}
