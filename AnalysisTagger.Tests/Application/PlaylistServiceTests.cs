using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Services;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;
using AnalysisTagger.Tests.Application.Fakes;
using FluentAssertions;

namespace AnalysisTagger.Tests.Application;

public class PlaylistServiceTests
{
    private static (PlaylistService Service, TaggingService Tagging, InMemoryUnitOfWork Uow, Project Project) Create()
    {
        var uow = new InMemoryUnitOfWork();
        var category = new Category { Id = Guid.NewGuid(), Name = "Shot",
            DefaultLeadTime = TimeSpan.FromSeconds(2), DefaultLagTime = TimeSpan.FromSeconds(3) };
        var project = new Project
        {
            Template = new TagTemplate { Categories = new List<Category> { category } },
            HomeTeam = new Team { Name = "Home" },
            AwayTeam = new Team { Name = "Away" }
        };
        uow.ProjectRepository.AddAsync(project).GetAwaiter().GetResult();
        return (new PlaylistService(uow), new TaggingService(uow), uow, project);
    }

    private async Task<Guid> TagAt(TaggingService svc, Guid projectId, Guid categoryId, double secs)
    {
        var dto = await svc.TagEventAsync(projectId, new CreateEventTagDto
        {
            CategoryId = categoryId,
            Position = Timecode.FromSeconds(secs)
        });
        return dto.Id;
    }

    [Fact]
    public async Task CreatePlaylistAsync_CreatesPlaylistOnProject()
    {
        var (svc, _, uow, project) = Create();

        var result = await svc.CreatePlaylistAsync(project.Id, "First Half");

        result.Name.Should().Be("First Half");
        result.Id.Should().NotBeEmpty();
        var stored = await uow.Projects.GetByIdAsync(project.Id);
        stored!.Playlists.Should().ContainSingle(p => p.Id == result.Id);
    }

    [Fact]
    public async Task CreatePlaylistAsync_Throws_WhenProjectNotFound()
    {
        var (svc, _, _, _) = Create();

        var act = async () => await svc.CreatePlaylistAsync(Guid.NewGuid(), "Test");

        await act.Should().ThrowAsync<ProjectNotFoundException>();
    }

    [Fact]
    public async Task AddTagToPlaylistAsync_AddsEntryToPlaylist()
    {
        var (svc, tagging, uow, project) = Create();
        var categoryId = project.Template.Categories[0].Id;
        var playlist = await svc.CreatePlaylistAsync(project.Id, "Highlights");
        var tagId = await TagAt(tagging, project.Id, categoryId, 30);

        await svc.AddTagToPlaylistAsync(project.Id, playlist.Id, tagId);

        var stored = await uow.Projects.GetByIdAsync(project.Id);
        stored!.Playlists[0].Entries.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddTagToPlaylistAsync_Throws_WhenPlaylistNotFound()
    {
        var (svc, tagging, _, project) = Create();
        var categoryId = project.Template.Categories[0].Id;
        var tagId = await TagAt(tagging, project.Id, categoryId, 30);

        var act = async () => await svc.AddTagToPlaylistAsync(project.Id, Guid.NewGuid(), tagId);

        await act.Should().ThrowAsync<InvalidTagException>();
    }

    [Fact]
    public async Task RemoveEntryFromPlaylistAsync_RemovesEntry()
    {
        var (svc, tagging, uow, project) = Create();
        var categoryId = project.Template.Categories[0].Id;
        var playlist = await svc.CreatePlaylistAsync(project.Id, "Highlights");
        var tagId = await TagAt(tagging, project.Id, categoryId, 30);
        await svc.AddTagToPlaylistAsync(project.Id, playlist.Id, tagId);
        var storedBefore = await uow.Projects.GetByIdAsync(project.Id);
        var entryId = storedBefore!.Playlists[0].Entries[0].Id;

        await svc.RemoveEntryFromPlaylistAsync(project.Id, playlist.Id, entryId);

        var storedAfter = await uow.Projects.GetByIdAsync(project.Id);
        storedAfter!.Playlists[0].Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task ReorderEntryAsync_ChangesEntryOrder()
    {
        var (svc, tagging, uow, project) = Create();
        var categoryId = project.Template.Categories[0].Id;
        var playlist = await svc.CreatePlaylistAsync(project.Id, "Highlights");

        var id1 = await TagAt(tagging, project.Id, categoryId, 10);
        var id2 = await TagAt(tagging, project.Id, categoryId, 20);
        var id3 = await TagAt(tagging, project.Id, categoryId, 30);
        await svc.AddTagToPlaylistAsync(project.Id, playlist.Id, id1);
        await svc.AddTagToPlaylistAsync(project.Id, playlist.Id, id2);
        await svc.AddTagToPlaylistAsync(project.Id, playlist.Id, id3);

        var stored = await uow.Projects.GetByIdAsync(project.Id);
        var firstEntryId = stored!.Playlists[0].Entries[0].Id;

        await svc.ReorderEntryAsync(project.Id, playlist.Id, firstEntryId, 2);

        var reordered = await uow.Projects.GetByIdAsync(project.Id);
        reordered!.Playlists[0].Entries[2].Id.Should().Be(firstEntryId);
    }
}
