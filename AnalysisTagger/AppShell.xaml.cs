using AnalysisTagger.UI.Pages;

namespace AnalysisTagger;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(AnalysisPage), typeof(AnalysisPage));
    }
}
