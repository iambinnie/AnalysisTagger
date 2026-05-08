namespace AnalysisTagger.Application.Interfaces;

public interface IDialogService
{
    Task<string?> PromptAsync(string title, string message, int maxLength = 100);
    Task AlertAsync(string title, string message, string cancel = "OK");
    Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");
    Task<string?> ActionSheetAsync(string title, string cancel, IEnumerable<string> options);
}
