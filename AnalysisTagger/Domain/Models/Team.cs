// Domain/Models/Team.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTagger.Domain.Models
{
    public class Team
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? ShieldImagePath { get; set; }
        public List<Player> Players { get; set; } = new();

        public Player? FindPlayer(Guid playerId) =>
            Players.FirstOrDefault(p => p.Id == playerId);
    }
}
