using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shardion.Terrabreak.Features.Bags;

public class BagEntry
{
    [Key] public Guid Id { get; init; }
    public required string Text { get; init; }
    public Guid BagId { get; init; }
    public required Bag Bag { get; init; }
}
