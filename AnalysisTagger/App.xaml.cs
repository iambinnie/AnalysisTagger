namespace AnalysisTagger;

public partial class App
{
    private readonly AppShell _appShell;

    public App(AppShell appShell)
    {
        InitializeComponent();
        _appShell = appShell;
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new Window(_appShell);
}
