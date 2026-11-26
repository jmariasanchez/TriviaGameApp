using TriviaGameApp.Services;


namespace TriviaGameApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            TriviaSettings.LoadSettings();

            ApplyTheme(TriviaSettings.AppThemeChoice);

            MainPage = new AppShell();
        }


        public static void ApplyTheme(string themeChoice)
        {
            Current.UserAppTheme = themeChoice == "Dark" ? AppTheme.Dark : AppTheme.Light;
        }
    }

}
