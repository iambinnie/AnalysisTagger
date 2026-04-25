namespace AnalysisTagger.Application.Interfaces;

public interface IDialogService
{
    Task<string?> PromptAsync(string title, string message, int maxLength = 100);
    Task AlertAsync(string title, string message, string cancel = "OK");
}
