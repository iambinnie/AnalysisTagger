namespace AnalysisTagger.Application.Interfaces;

public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoBackAsync();
}
