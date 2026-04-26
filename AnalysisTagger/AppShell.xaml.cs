using AnalysisTagger.UI.Pages;

namespace AnalysisTagger;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ProjectsPage),     typeof(ProjectsPage));
        Routing.RegisterRoute(nameof(AnalysisPage),     typeof(AnalysisPage));
        Routing.RegisterRoute(nameof(TeamsPage),        typeof(TeamsPage));
        Routing.RegisterRoute(nameof(AddEditTeamPage),  typeof(AddEditTeamPage));
        Routing.RegisterRoute(nameof(TeamRosterPage),   typeof(TeamRosterPage));
        Routing.RegisterRoute(nameof(AddEditPlayerPage),typeof(AddEditPlayerPage));
    }
}
