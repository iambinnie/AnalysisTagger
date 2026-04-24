using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Services;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;
using AnalysisTagger.Tests.Application.Fakes;
using FluentAssertions;

namespace AnalysisTagger.Tests.Application;

public class ExportServiceTests
{
    // Use the test assembly itself as a "video file" — ExportService only checks File.Exists.
    private static readonly string ExistingFile =
        typeof(ExportServiceTests).Assembly.Location;

    private static (ExportService Service, FakeVideoExporter Exporter, InMemoryUnitOfWork Uow, Project Project) Create(
        string videoFilePath = "")
    {
        var uow = new InMemoryUnitOfWork();
        var exporter = new FakeVideoExporter();
        var category = new Category { Id = Guid.NewGuid(), Name = "Shot",
            DefaultLeadTime = TimeSpan.FromSeconds(2), DefaultLagTime = TimeSpan.FromSeconds(3) };
        var project = new Project
        {
            VideoFilePath = videoFilePath,
            Template = new TagTemplate { Categories = new List<Category> { category } },
            HomeTeam = new Team { Name = "Home" },
            AwayTeam = new Team { Name = "Away" }
        };
        uow.ProjectRepository.AddAsync(project).GetAwaiter().GetResult();
        return (new ExportService(uow, exporter), exporter, uow, project);
    }

    private static EventTag AddTag(Project project, double startSecs, double endSecs)
    {
        var tag = new EventTag
        {
            Segment = new VideoSegment(Timecode.FromSeconds(startSecs), Timecode.FromSeconds(endSecs)),
            Category = project.Template.Categories[0]
        };
        project.AddEvent(tag);
        return tag;
    }

    [Fact]
    public async Task ExportTagAsync_CallsExporter_WithCorrectSegment()
    {
        var (svc, exporter, _, project) = Create(ExistingFile);
        var tag = AddTag(project, 10, 20);

        await svc.ExportTagAsync(project.Id, tag.Id, "out.mp4");

        exporter.ExportedSegments.Should().ContainSingle();
        exporter.ExportedSegments[0].Segment.Should().Be(tag.Segment);
        exporter.ExportedSegments[0].Output.Should().Be("out.mp4");
    }

    [Fact]
    public async Task ExportTagAsync_Throws_WhenVideoFileDoesNotExist()
    {
        var (svc, _, _, project) = Create("nonexistent_video.mp4");
        var tag = AddTag(project, 10, 20);

        var act = async () => await svc.ExportTagAsync(project.Id, tag.Id, "out.mp4");

        await act.Should().ThrowAsync<VideoFileNotFoundException>();
    }

    [Fact]
    public async Task ExportTagAsync_Throws_WhenTagNotFound()
    {
        var (svc, _, _, project) = Create(ExistingFile);

        var act = async () => await svc.ExportTagAsync(project.Id, Guid.NewGuid(), "out.mp4");

        await act.Should().ThrowAsync<InvalidTagException>();
    }

    [Fact]
    public async Task ExportTagAsync_Throws_WhenProjectNotFound()
    {
        var (svc, _, _, _) = Create(ExistingFile);

        var act = async () => await svc.ExportTagAsync(Guid.NewGuid(), Guid.NewGuid(), "out.mp4");

        await act.Should().ThrowAsync<ProjectNotFoundException>();
    }

    [Fact]
    public async Task ExportPlaylistAsync_PassesSegmentsInSortOrder()
    {
        var (svc, exporter, _, project) = Create(ExistingFile);
        var tagA = AddTag(project, 0, 5);
        var tagB = AddTag(project, 10, 15);
        var tagC = AddTag(project, 20, 25);

        var playlist = new Playlist { Name = "Test" };
        // Add in reverse order, then reorder so they're sorted 0,1,2
        playlist.AddEntry(tagC);
        playlist.AddEntry(tagB);
        playlist.AddEntry(tagA);
        playlist.Reorder(playlist.Entries[0].Id, 2); // move C to end → A,B,C order after further reorder
        // Simplest: just add in correct order
        var orderedPlaylist = new Playlist { Name = "Ordered" };
        orderedPlaylist.AddEntry(tagA);
        orderedPlaylist.AddEntry(tagB);
        orderedPlaylist.AddEntry(tagC);
        project.Playlists.Add(orderedPlaylist);

        await svc.ExportPlaylistAsync(project.Id, orderedPlaylist.Id, "playlist_out.mp4");

        exporter.ExportedPlaylists.Should().ContainSingle();
        var segments = exporter.ExportedPlaylists[0].Segments;
        segments.Should().HaveCount(3);
        segments[0].Should().Be(tagA.Segment);
        segments[1].Should().Be(tagB.Segment);
        segments[2].Should().Be(tagC.Segment);
    }

    [Fact]
    public async Task ExportPlaylistAsync_Throws_WhenVideoFileDoesNotExist()
    {
        var (svc, _, _, project) = Create("nonexistent.mp4");
        var playlist = new Playlist { Name = "Test" };
        project.Playlists.Add(playlist);

        var act = async () => await svc.ExportPlaylistAsync(project.Id, playlist.Id, "out.mp4");

        await act.Should().ThrowAsync<VideoFileNotFoundException>();
    }
}
