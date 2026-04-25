using AnalysisTagger.Application.Interfaces;

namespace AnalysisTagger.Services;

public class DialogService : IDialogService
{
    public Task<string?> PromptAsync(string title, string message, int maxLength = 100) =>
        Shell.Current.DisplayPromptAsync(title, message, maxLength: maxLength);

    public Task AlertAsync(string title, string message, string cancel = "OK") =>
        Shell.Current.DisplayAlert(title, message, cancel);
}
