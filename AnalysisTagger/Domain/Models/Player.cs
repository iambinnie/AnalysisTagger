using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTagger.Domain.Models
{
    public class Player
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int ShirtNumber { get; set; }
        public string Position { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
        public Dictionary<string, string> CustomAttributes { get; set; } = new();
    }
}
