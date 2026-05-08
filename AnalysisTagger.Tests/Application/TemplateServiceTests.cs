using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Services;
using AnalysisTagger.Domain.Enums;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Tests.Application.Fakes;

namespace AnalysisTagger.Tests.Application;

public class TemplateServiceTests
{
    private static (TemplateService service, InMemoryUnitOfWork uow) Build()
    {
        var uow = new InMemoryUnitOfWork();
        return (new TemplateService(uow), uow);
    }

    private static async Task<TagTemplate> SeedTemplateAsync(InMemoryUnitOfWork uow, string name = "Test", bool isBuiltIn = false)
    {
        var t = new TagTemplate { Name = name, Sport = SportType.Football, IsBuiltIn = isBuiltIn };
        await uow.TemplateRepository.AddAsync(t);
        return t;
    }

    [Fact]
    public async Task GetAllTemplates_ReturnsAllTemplates()
    {
        var (svc, uow) = Build();
        await SeedTemplateAsync(uow, "Alpha");
        await SeedTemplateAsync(uow, "Beta");

        var result = (await svc.GetAllTemplatesAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateTemplate_AddsToRepositoryAndSaves()
    {
        var (svc, uow) = Build();

        var dto = await svc.CreateTemplateAsync(new CreateTemplateDto { Name = "My Template", Sport = SportType.Rugby });

        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("My Template", dto.Name);
        Assert.Equal(SportType.Rugby, dto.Sport);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task CreateTemplate_TrimsName()
    {
        var (svc, _) = Build();

        var dto = await svc.CreateTemplateAsync(new CreateTemplateDto { Name = "  Padded  " });

        Assert.Equal("Padded", dto.Name);
    }

    [Fact]
    public async Task GetTemplateDetail_ReturnsTemplateAndCategoriesOrderedBySortOrder()
    {
        var (svc, uow) = Build();
        var t = await SeedTemplateAsync(uow);
        t.Categories.Add(new Category { Name = "B", SortOrder = 2 });
        t.Categories.Add(new Category { Name = "A", SortOrder = 1 });

        var (tDto, cats) = await svc.GetTemplateDetailAsync(t.Id);

        Assert.Equal(t.Id, tDto.Id);
        var catList = cats.ToList();
        Assert.Equal(2, catList.Count);
        Assert.Equal("A", catList[0].Name);
        Assert.Equal("B", catList[1].Name);
    }

    [Fact]
    public async Task GetTemplateDetail_ThrowsWhenNotFound()
    {
        var (svc, _) = Build();

        await Assert.ThrowsAsync<TemplateNotFoundException>(() =>
            svc.GetTemplateDetailAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateTemplate_UpdatesFieldsAndSaves()
    {
        var (svc, uow) = Build();
        var t = await SeedTemplateAsync(uow, "Old");

        await svc.UpdateTemplateAsync(t.Id, new UpdateTemplateDto { Name = "New", Sport = SportType.Basketball });

        var updated = await uow.TemplateRepository.GetByIdAsync(t.Id);
        Assert.Equal("New", updated!.Name);
        Assert.Equal(SportType.Basketball, updated.Sport);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task UpdateTemplate_ThrowsWhenNotFound()
    {
        var (svc, _) = Build();

        await Assert.ThrowsAsync<TemplateNotFoundException>(() =>
            svc.UpdateTemplateAsync(Guid.NewGuid(), new UpdateTemplateDto { Name = "X" }));
    }

    [Fact]
    public async Task DeleteTemplate_DeletesAndSaves()
    {
        var (svc, uow) = Build();
        var t = await SeedTemplateAsync(uow);

        await svc.DeleteTemplateAsync(t.Id);

        Assert.Null(await uow.TemplateRepository.GetByIdAsync(t.Id));
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task DeleteTemplate_ThrowsForBuiltIn()
    {
        var (svc, uow) = Build();
        var t = await SeedTemplateAsync(uow, isBuiltIn: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.DeleteTemplateAsync(t.Id));
    }

    [Fact]
    public async Task DeleteTemplate_ThrowsWhenNotFound()
    {
        var (svc, _) = Build();

        await Assert.ThrowsAsync<TemplateNotFoundException>(() =>
            svc.DeleteTemplateAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AddCategory_AddsToTemplateAndSaves()
    {
        var (svc, uow) = Build();
        var t = await SeedTemplateAsync(uow);

        var cat = await svc.AddCategoryAsync(t.Id, new CreateCategoryDto
        {
            Name = "Shot",
            Color = "#E74C3C",
            LeadTimeSeconds = 3,
            LagTimeSeconds = 5
        });

        Assert.Equal("Shot", cat.Name);
        Assert.Equal("#E74C3C", cat.Color);
        Assert.Equal(3, cat.LeadTimeSeconds);
        Assert.Equal(5, cat.LagTimeSeconds);
        Assert.Equal(1, uow.SaveCount);
        Assert.Single((await uow.TemplateRepository.GetByIdAsync(t.Id))!.Categories);
    }

    [Fact]
    public async Task AddCategory_ThrowsWhenTemplateNotFound()
    {
        var (svc, _) = Build();

        await Assert.ThrowsAsync<TemplateNotFoundException>(() =>
            svc.AddCategoryAsync(Guid.NewGuid(), new CreateCategoryDto { Name = "X" }));
    }

    [Fact]
    public async Task UpdateCategory_UpdatesFieldsAndSaves()
    {
        var (svc, uow) = Build();
        var t = await SeedTemplateAsync(uow);
        var cat = new Category { Name = "Old", Color = "#000000", SortOrder = 1 };
        t.Categories.Add(cat);

        await svc.UpdateCategoryAsync(t.Id, cat.Id, new UpdateCategoryDto
        {
            Name = "New",
            Color = "#FFFFFF",
            LeadTimeSeconds = 4,
            LagTimeSeconds = 6
        });

        Assert.Equal("New", cat.Name);
        Assert.Equal("#FFFFFF", cat.Color);
        Assert.Equal(TimeSpan.FromSeconds(4), cat.DefaultLeadTime);
        Assert.Equal(TimeSpan.FromSeconds(6), cat.DefaultLagTime);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task UpdateCategory_ThrowsWhenCategoryNotFound()
    {
        var (svc, uow) = Build();
        var t = await SeedTemplateAsync(uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.UpdateCategoryAsync(t.Id, Guid.NewGuid(), new UpdateCategoryDto { Name = "X" }));
    }

    [Fact]
    public async Task RemoveCategory_RemovesFromTemplateAndSaves()
    {
        var (svc, uow) = Build();
        var t = await SeedTemplateAsync(uow);
        var cat = new Category { Name = "To Remove", SortOrder = 1 };
        t.Categories.Add(cat);

        await svc.RemoveCategoryAsync(t.Id, cat.Id);

        Assert.Empty((await uow.TemplateRepository.GetByIdAsync(t.Id))!.Categories);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task RemoveCategory_ThrowsWhenCategoryNotFound()
    {
        var (svc, uow) = Build();
        var t = await SeedTemplateAsync(uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.RemoveCategoryAsync(t.Id, Guid.NewGuid()));
    }
}
