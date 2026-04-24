// Domain/Models/Project.cs
// The top level container — equivalent to a match or analysis session

using AnalysisTagger.Domain.Enums;
using AnalysisTagger.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTagger.Domain.Models
{
    public class Project
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Competition { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public DateTime MatchDate { get; set; } = DateTime.Today;
        public SportType Sport { get; set; } = SportType.Generic;
        public TaggingMode TaggingMode { get; set; } = TaggingMode.PostTagging;

        public Team HomeTeam { get; set; } = new();
        public Team AwayTeam { get; set; } = new();

        public string VideoFilePath { get; set; } = string.Empty;
        public TagTemplate Template { get; set; } = new();

        public List<EventTag> Events { get; set; } = new();
        public List<Playlist> Playlists { get; set; } = new();

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

        // Domain queries — keeps logic in the domain, not scattered in services
        public IEnumerable<EventTag> GetEventsByCategory(Guid categoryId) =>
            Events.Where(e => e.Category.Id == categoryId);

        public IEnumerable<EventTag> GetEventsByPlayer(Guid playerId) =>
            Events.Where(e => e.TaggedPlayers.Any(p => p.Id == playerId));

        public IEnumerable<EventTag> GetEventsInWindow(Timecode start, Timecode end) =>
            Events.Where(e => e.StartTime.IsAfter(start) && e.EndTime.IsBefore(end));

        public void AddEvent(EventTag tag)
        {
            Events.Add(tag);
            LastModifiedAt = DateTime.UtcNow;
        }
    }
}
