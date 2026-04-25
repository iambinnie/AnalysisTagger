using CommunityToolkit.Mvvm.ComponentModel;

namespace AnalysisTagger.UI.ViewModels;

[QueryProperty("ProjectId", "projectId")]
public partial class AnalysisViewModel : ObservableObject
{
    [ObservableProperty]
    private string _projectId = string.Empty;

    [ObservableProperty]
    private string _projectTitle = "Analysis";
}
