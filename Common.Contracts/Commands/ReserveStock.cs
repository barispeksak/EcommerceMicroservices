using System;
using System.Collections.Generic;
namespace Common.Contracts.Commands;

public record ReserveStock : Base.Message
{
    public ReserveStock(Guid cartId, List<Guid>? items = null)
    {
        CartId = cartId;
        Items = items ?? new List<Guid>();
    }
    
    public Guid CartId { get; init; }
    public List<Guid> Items { get; init; }
}