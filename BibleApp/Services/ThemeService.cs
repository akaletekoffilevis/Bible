namespace BibleApp.Services;

public class ThemeService
{
    public bool IsDarkMode { get; private set; }
    public event Action? OnChange;

    public void SetDarkMode(bool dark)
    {
        IsDarkMode = dark;
        NotifyStateChanged();
    }

    public void Toggle()
    {
        IsDarkMode = !IsDarkMode;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
