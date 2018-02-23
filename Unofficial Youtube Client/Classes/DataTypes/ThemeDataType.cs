using Windows.UI;
using Windows.UI.Xaml;

namespace YTApp.Classes.DataTypes
{
    static public class ThemeDataType
    {
        static private Color _appBackgroundDark = Color.FromArgb(255, 34, 34, 34);
        static private Color _appBackground = Color.FromArgb(255, 37, 37, 37);
        static private Color _appBackgroundLighter = Color.FromArgb(255, 42, 42, 42);
        static private Color _appBackgroundLightest = Color.FromArgb(255, 51, 51, 51);
        static private Color _appText = Color.FromArgb(255, 255, 255, 255);
        static private Color _appTextSecondary = Color.FromArgb(255, 170, 170, 170);
        static private Color _buttonBackground = Color.FromArgb(255, 102, 102, 102);

        static public Color AppBackgroundDark { get => _appBackgroundDark; set { _appBackgroundDark = value; Application.Current.Resources["AppBackgroundDark"] = _appBackgroundDark; } }
        static public Color AppBackground { get => _appBackground; set { _appBackground = value; Application.Current.Resources["AppBackground"] = _appBackground; } }
        static public Color AppBackgroundLighter { get => _appBackgroundLighter; set { _appBackgroundLighter = value; Application.Current.Resources["AppBackgroundLighter"] = _appBackgroundLighter; } }
        static public Color AppBackgroundLightest { get => _appBackgroundLightest; set { _appBackgroundLightest = value; Application.Current.Resources["AppBackgroundLightest"] = _appBackgroundLightest; } }
        static public Color AppText { get => _appText; set { _appText = value; Application.Current.Resources["AppText"] = _appText; } }
        static public Color AppTextSecondary { get => _appTextSecondary; set { _appTextSecondary = value; Application.Current.Resources["AppTextSecondary"] = _appTextSecondary; } }
        static public Color ButtonBackground { get => _buttonBackground; set { _buttonBackground = value; Application.Current.Resources["ButtonBackground"] = _buttonBackground; } }
    }
}