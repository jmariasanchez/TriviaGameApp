using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System;
using TriviaGameApp.Models;
using TriviaGameApp.Services;

namespace TriviaGameApp.Views
{
    public partial class GamePage : ContentPage
    {
        private TriviaService _triviaService = new();
        
        private Dictionary<int, List<TriviaQuestion>> _playerQuestions = new();
        private int _currentPlayerIndex = 0;
        private int _currentQuestionIndex = 0;

        private DateTime _gameStartTime;
        private int _timeLeft;
        private Microsoft.Maui.Dispatching.IDispatcherTimer? _timer;
        private int _pageVersion;

        private bool _gameEnded = false;  

        public GamePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            int version = ++_pageVersion;
            StopTimer();
            _gameStartTime = DateTime.Now;

            GameState.TimeLimitInSeconds = 30;

            if (GameState.Players == null || GameState.Players.Count == 0)
            {
                await DisplayAlert("Error", "No players found. Returning to start.", "OK");
                if (version != _pageVersion) return;
                await Shell.Current.GoToAsync("//StartPage");
                return;
            }

            List<TriviaQuestion> fetched = await _triviaService.FetchTriviaAsync(GameState.Players.Count);
            if (version != _pageVersion) return;
            int questionsPerPlayer = fetched.Count / GameState.Players.Count;
            int remainingQuestions = fetched.Count % GameState.Players.Count;

            for (int i = 0; i < GameState.Players.Count; i++)
            {
                int startIndex = i * questionsPerPlayer;
                int count = questionsPerPlayer + (i < remainingQuestions ? 1 : 0);
                _playerQuestions[i] = fetched.GetRange(startIndex, count);

                // Debug check:
                System.Diagnostics.Debug.WriteLine(
                $"DEBUG: Player '{GameState.Players[i].Name}' got {_playerQuestions[i].Count} questions."
                );
            }

            _currentPlayerIndex = 0;
            _currentQuestionIndex = 0;
            _gameEnded = false;

            LoadQuestion();
        }

        private void LoadQuestion()
        {
            if (_gameEnded) return;

            if (_currentPlayerIndex >= GameState.Players.Count)
            {
                EndGame();
                return;
            }

            var currentPlayer = GameState.Players[_currentPlayerIndex];
            var questions = _playerQuestions[_currentPlayerIndex];

            if (_currentQuestionIndex >= questions.Count)
            {
                _currentPlayerIndex++;
                _currentQuestionIndex = 0;

                if (_currentPlayerIndex >= GameState.Players.Count)
                {
                    EndGame();
                    return;
                }

                LoadQuestion();
                return;
            }

            PlayerLabel.Text = $"{currentPlayer.Name}'s Turn (Score: {currentPlayer.Score})";
            
            var question = questions[_currentQuestionIndex];
            QuestionLabel.Text = question.Question;

            var timer = Dispatcher.CreateTimer();
            _timer = timer;
            AnswersContainer.Children.Clear();
            foreach (var answer in question.AllAnswers)
            {
                var answerButton = new Button
                {
                    Text = answer,
                    Style = (Style)Resources["JapaneseButtonStyle"]
                };
                answerButton.Clicked += async (s, e) =>
                {
                    if (_timer == timer) await FinishQuestionAsync(question, answer);
                };
                AnswersContainer.Children.Add(answerButton);
            }

            _timeLeft = GameState.TimeLimitInSeconds;
            UpdateTimerLabel();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        private async Task FinishQuestionAsync(TriviaQuestion? question = null, string? answer = null, bool timedOut = false)
        {
            if (_timer == null) return;
            int version = _pageVersion;
            StopTimer();

            foreach (var view in AnswersContainer.Children)
            {
                if (view is Button btn)
                    btn.IsEnabled = false;
            }

            if (timedOut)
            {
                await DisplayAlert("Time's Up!", "You ran out of time.", "OK");
            }
            else if (question != null && answer == question.CorrectAnswer)
            {
                int points = GetPointsForDifficulty(question.Difficulty);
                GameState.Players[_currentPlayerIndex].Score += points;
                await DisplayAlert("Correct!", $"You earned {points} points.", "OK");
            }
            else if (question != null)
            {
                await DisplayAlert("Incorrect!", "Better luck next time.", "OK");
            }

            if (version != _pageVersion) return;
            _currentQuestionIndex++;
            LoadQuestion();
        }

        private int GetPointsForDifficulty(string? difficulty)
        {
            switch (difficulty?.ToLower())
            {
                case "easy": return 1;
                case "medium": return 2;
                case "hard": return 3;
                default: return 1;
            }
        }

        private async void OnTimerTick(object? sender, EventArgs e)
        {
            if (_timer == null || sender != _timer) return;
            _timeLeft--;
            UpdateTimerLabel();
            if (_timeLeft <= 0) await FinishQuestionAsync(timedOut: true);
        }

        private void StopTimer()
        {
            if (_timer == null) return;
            _timer.Tick -= OnTimerTick;
            _timer.Stop();
            _timer = null;
        }

        protected override void OnDisappearing()
        {
            _pageVersion++;
            StopTimer();
            base.OnDisappearing();
        }

        private void UpdateTimerLabel()
        {
            TimerLabel.Text = $"Time Left: {_timeLeft}";
        }

        private async void OnSkipClicked(object sender, EventArgs e)
        {
            await FinishQuestionAsync();
        }

        private async void EndGame()
        {
            if (_gameEnded) return;
            _gameEnded = true;

            var totalTime = DateTime.Now - _gameStartTime;
            int totalSeconds = (int)totalTime.TotalSeconds;

            var leaderboard = LeaderboardFileService.LoadLeaderboard();
            string categoryText = CategoryHelper.GetCategoryText(TriviaSettings.Category);
            string difficultyText = TriviaSettings.Difficulty;

            int questionsPerPlayer = _playerQuestions.Count > 0 
                ? _playerQuestions[0].Count
                : 10;

            string dateOfGame = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            foreach (var player in GameState.Players)
            {
                var entry = new LeaderboardEntry
                {
                    Name = player.Name,
                    Category = categoryText,
                    Questions = questionsPerPlayer,
                    TimeInSeconds = totalSeconds,
                    Difficulty = difficultyText,
                    TotalPoints = player.Score,
                    DateOfGame = dateOfGame
                };
                leaderboard.Add(entry);
            }

            LeaderboardFileService.SaveLeaderboard(leaderboard);

            string resultMessage = "Final Scores:\n\n";
            foreach (var p in GameState.Players)
            {
                resultMessage += $"{p.Name}: {p.Score}\n";
            }
            int version = _pageVersion;
            await DisplayAlert("Game Over!", resultMessage, "OK");
            if (version != _pageVersion) return;

            await Shell.Current.GoToAsync("LeaderboardPage");
        }
    }
}
