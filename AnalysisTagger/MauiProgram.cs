using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using AnalysisTagger.Infrastructure;
using AnalysisTagger.Infrastructure.Data;
using AnalysisTagger.UI.Pages;
using AnalysisTagger.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnalysisTagger;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "analysistagger.db");
            options.UseSqlite($"Data Source={dbPath}");
        }, ServiceLifetime.Singleton);

        builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
        builder.Services.AddSingleton<ProjectService>();
        builder.Services.AddSingleton<TaggingService>();
        builder.Services.AddSingleton<PlaylistService>();

        builder.Services.AddTransient<ProjectsViewModel>();
        builder.Services.AddTransient<AnalysisViewModel>();
        builder.Services.AddTransient<ProjectsPage>();
        builder.Services.AddTransient<AnalysisPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        app.Services.GetRequiredService<AppDbContext>().Database.Migrate();

        return app;
    }
}
