using Microsoft.Maui.Controls.Shapes;
using TriviaGameApp.Models;

namespace TriviaGameApp.Views;

public static class GameState
{
    public static List<Player>? Players { get; set; }
    public static int TotalQuestionsPerPlayer { get; set; } = 10;
    public static int TimeLimitInSeconds { get; set; } = 30;
}

public partial class PlayersView : ContentPage
{
    private List<Entry> _playerNameEntries = new();

    public PlayersView()
    {
        InitializeComponent();
    }

    private void OnPlayerCountChanged(object sender, EventArgs e)
    {
        PlayerNamesContainer.Children.Clear();
        _playerNameEntries.Clear();

        if (PlayerCountPicker.SelectedItem is not int selectedCount) return;

        for (int i = 1; i <= selectedCount; i++)
        {
            var containerGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                Margin = new Thickness(0, 0, 0, 10)
            };

            var entry = new Entry
            {
                Placeholder = $"Player {i} Name (プレイヤー {i} 名前)",
                HorizontalOptions = LayoutOptions.Fill,
                Style = (Style)Resources["PlayerNameStyle"]
            };

            var border = new Border
            {
                Content = entry,
                Style = (Style)Resources["PlayerBorderStyle"],
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 0 },
                Padding = new Thickness(10, 5),
                HorizontalOptions = LayoutOptions.Center
            };

            containerGrid.Add(border);
            _playerNameEntries.Add(entry);
            PlayerNamesContainer.Children.Add(containerGrid);

            if (i < selectedCount)
            {
                var line = new Line
                {
                    Style = (Style)Resources["PlayerDividerStyle"],
                    X1 = 0,
                    Y1 = 0,
                    X2 = 100,
                    Y2 = 0,
                    StrokeThickness = 1,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 10)
                };
                PlayerNamesContainer.Children.Add(line);
            }
        }
    }

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        if (!ValidatePlayerNames())
        {
            await DisplayAlert("入力エラー / Input Error", 
                "Please enter names for all players\nすべてのプレイヤーの名前を入力してください", 
                "OK");
            return;
        }

        int selectedCount = (int)PlayerCountPicker.SelectedItem;
        var players = new List<Player>();
        
        for (int i = 0; i < selectedCount; i++)
        {
            players.Add(new Player
            {
                Name = string.IsNullOrWhiteSpace(_playerNameEntries[i].Text) ? 
                    $"Player {i+1}" : _playerNameEntries[i].Text,
                Score = 0
            });
        }

        GameState.Players = players;
        GameState.TotalQuestionsPerPlayer = 10;
        await Shell.Current.GoToAsync($"//GamePage");
    }

    private bool ValidatePlayerNames()
    {
        return _playerNameEntries.Count > 0 && _playerNameEntries.All(entry => !string.IsNullOrWhiteSpace(entry.Text));
    }
}
