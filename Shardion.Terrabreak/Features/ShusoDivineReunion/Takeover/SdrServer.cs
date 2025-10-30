using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;

[Index(nameof(ServerId), IsUnique = true)]
public class SdrServer
{
    [Key] public Guid Id { get; set; }

    /// <summary>
    /// The ID of the relevant server.
    /// </summary>
    public ulong ServerId { get; set; }

    public IList<int> PassagesUnlocked { get; set; } = [];
    public bool TakenOver { get; set; } = false;
}
