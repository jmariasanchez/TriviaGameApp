# Trivia Game (トリビアゲーム)

I started this trivia game in 2023. One to four players share a device and take turns racing the clock. I built the interface in C# and XAML with .NET MAUI, pairing English and Japanese labels with light and dark themes. The Git history for this version begins in December 2024, with updates through January 2025.

## Play a round

1. Open **Settings** to choose a category, difficulty, question type (multiple choice or true/false), and number of questions per player. The default is 10 questions per player with no filters.
2. Choose **New Game**, select the number of players, and enter their names. An internet connection is needed to load the questions.
3. Each player finishes their questions before passing the device. Answer within 30 seconds or choose **Skip**. Correct answers earn 1 point on easy, 2 on medium, and 3 on hard.
4. Dismiss **Game Over!** to see the local leaderboard. Search by player name or filter by category and difficulty to compare scores across rounds.

For a short first game, try two players and two questions each. Keep the total (players × questions per player) at 50 or fewer, the [Open Trivia DB request limit](https://opentdb.com/api_config.php). If a round ends without questions, check the connection and try a broader category or difficulty.

## How it works

[`TriviaService`](TriviaGameApp/Services/TriviaService.cs) requests the whole group's questions in one call and decodes HTML entities in the text. [`GamePage`](TriviaGameApp/Views/GamePage.xaml.cs) divides the questions among players and handles the timer and scoring. The app stores scores in `leaderboard.json` and settings in `trivia_settings.json` in the app data directory. Settings also use MAUI Preferences as a fallback.

## Run locally

Use the .NET 8 SDK, the MAUI workload for your target, and its platform toolchain. For example, to run the Mac Catalyst target on a Mac with compatible Xcode installed, start in the repository root:

```sh
dotnet workload install maui-maccatalyst
dotnet build TriviaGameApp/TriviaGameApp.csproj -t:Run -f net8.0-maccatalyst -p:TargetFrameworks=net8.0-maccatalyst
```

The [project file](TriviaGameApp/TriviaGameApp.csproj) also declares `net8.0-android`, `net8.0-ios`, and, on Windows, `net8.0-windows10.0.19041.0`.

## Credits and license

Questions and answers come from the [Open Trivia Database](https://opentdb.com/) under [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/). The application code is distributed under the [MIT License](LICENSE.txt).
