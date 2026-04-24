// Domain/Models/Category.cs
using System;
using System.Collections.Generic;
using System.Text;

// Represents a tagging button definition — e.g. "Shot on Goal"

namespace AnalysisTagger.Domain.Models
{
    public class Category
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#3498DB"; // hex color for timeline display
        public TimeSpan DefaultLeadTime { get; set; } = TimeSpan.FromSeconds(2);
        public TimeSpan DefaultLagTime { get; set; } = TimeSpan.FromSeconds(3);
        public List<string> SubCategories { get; set; } = new();
        public int SortOrder { get; set; }
    }
}
