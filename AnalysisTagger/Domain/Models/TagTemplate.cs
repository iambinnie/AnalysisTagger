// Domain/Models/TagTemplate.cs
// A reusable set of categories for a specific sport/analysis type

using AnalysisTagger.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTagger.Domain.Models
{
    public class TagTemplate
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public SportType Sport { get; set; } = SportType.Generic;
        public List<Category> Categories { get; set; } = new();
        public bool IsBuiltIn { get; set; } // true for shipped defaults

        public static TagTemplate CreateDefault(SportType sport) => sport switch
        {
            SportType.Football => CreateFootballTemplate(),
            _ => CreateGenericTemplate()
        };

        private static TagTemplate CreateFootballTemplate() => new()
        {
            Name = "Football",
            Sport = SportType.Football,
            Categories = new List<Category>
        {
            new() { Name = "Shot on Goal", Color = "#E74C3C", SortOrder = 1 },
            new() { Name = "Shot off Target", Color = "#E67E22", SortOrder = 2 },
            new() { Name = "Corner", Color = "#2ECC71", SortOrder = 3 },
            new() { Name = "Foul", Color = "#9B59B6", SortOrder = 4 },
            new() { Name = "Yellow Card", Color = "#F1C40F", SortOrder = 5 },
            new() { Name = "Red Card", Color = "#C0392B", SortOrder = 6 },
            new() { Name = "Goal", Color = "#1ABC9C", SortOrder = 7 },
        }
        };

        private static TagTemplate CreateGenericTemplate() => new()
        {
            Name = "Generic",
            Sport = SportType.Generic,
            Categories = new List<Category>
        {
            new() { Name = "Event A", Color = "#3498DB", SortOrder = 1 },
            new() { Name = "Event B", Color = "#E74C3C", SortOrder = 2 },
        }
        };
    }
}
