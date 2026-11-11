namespace iSing.Services;

public sealed class SettingsService(IPreferencesStore preferences, IThemeService theme) : ISettingsService
{
    public double FontSize
    {
        get
        {
            var size = preferences.Get("fontSize", 18.0);
            return size is 18 or 22 or 27 ? size : 18;
        }
        set
        {
            if (value is not (18 or 22 or 27)) throw new ArgumentOutOfRangeException(nameof(value));
            preferences.Set("fontSize", value);
        }
    }
    public AppDesign Design
    {
        get
        {
            var value = preferences.Get("theme", 0);
            return Enum.IsDefined((AppDesign)value) ? (AppDesign)value : AppDesign.System;
        }
        set
        {
            if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
            preferences.Set("theme", (int)value);
            theme.Apply(value);
        }
    }
}
