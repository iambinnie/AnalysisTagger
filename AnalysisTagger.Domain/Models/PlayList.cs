// Domain/Models/Playlist.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTagger.Domain.Models
{
    public class Playlist
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<PlaylistEntry> Entries { get; set; } = new();
        public TimeSpan TotalDuration =>
            TimeSpan.FromTicks(Entries.Sum(e => e.EffectiveSegment.Duration.Ticks));

        public void AddEntry(EventTag tag) =>
            Entries.Add(new PlaylistEntry { Tag = tag, SortOrder = Entries.Count });

        public void Reorder(Guid entryId, int newPosition)
        {
            var entry = Entries.First(e => e.Id == entryId);
            Entries.Remove(entry);
            Entries.Insert(newPosition, entry);
            for (int i = 0; i < Entries.Count; i++)
                Entries[i].SortOrder = i;
        }
    }
}
