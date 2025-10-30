using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;

[Index(nameof(ChannelId), IsUnique = true)]
[Index(nameof(ServerId))]
public class SdrChannel
{
    [Key]
    public Guid Id { get; set; }
    /// <summary>
    /// The ID of the relevant channel.
    /// </summary>
    public ulong ChannelId { get; set; }

    /// <summary>
    /// The ID of the relevant server.
    /// </summary>
    public ulong ServerId { get; set; }
    /// <summary>
    /// Is this channel currently taken over by its captor?
    /// </summary>
    public bool TakenOver { get; set; }
    /// <summary>
    /// The ID of the type of enemy that holds this channel captive.
    /// </summary>
    public required string CaptorId { get; set; }
    /// <summary>
    /// The type of enemy that holds this channel captive.
    /// </summary>
    public IEnemy Captor => SdrRegistries.Enemies.Forward[CaptorId];
    /// <summary>
    /// The name this channel had before it was taken over.
    /// </summary>
    public required string OriginalName { get; set; }
}
