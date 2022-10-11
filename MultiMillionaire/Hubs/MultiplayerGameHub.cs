using Microsoft.AspNetCore.SignalR;
using MultiMillionaire.Models;

namespace MultiMillionaire.Hubs;

public interface IMultiplayerGameHub
{
    Task Message(string message);
    Task JoinSuccessful(string gameId);
    Task JoinGameIdNotFound();
    Task PopulatePlayerList(IEnumerable<UserViewModel> players);
    Task PlayerJoined(UserViewModel player);
    Task PlayerLeft(UserViewModel player);
    Task GameEnded();

    Task Show(string elementId, string display = "block");
    Task Hide(string elementId);
    Task SetText(string elementId, string text);
    Task SetAnswerText(string elementId, string text);
    Task SetOnClick(string elementId, string onclick);
    Task Lock(string elementId);
    Task Unlock(string elementId);
    Task SetBackground(int imageNumber, bool useRedVariant = false);

    Task StartFastestFinger(Dictionary<char, string> answers);
    Task EnableFastestFingerAnswering();
    Task DisableFastestFingerAnswering();
    Task ShowFastestFingerAnswer(int answerIndex, char letter, string answerText);
    Task PopulateFastestFingerResults(IEnumerable<UserViewModel> players);
    Task RevealCorrectFastestFingerPlayers(Dictionary<string, double> correctUserTimes);
    Task HighlightFastestFingerWinner(string connectionId);
    Task ResetFastestFinger();
}

public class MultiplayerGameHub : Hub<IMultiplayerGameHub>
{
    private static List<MultiplayerGame> Games { get; } = new();
    private static List<User> Users { get; } = new();

    // TEMP
    public async Task JoinRandomAudience()
    {
        await JoinGameAudience(Games.First().Id);
    }

    #region MiscellaneousMethods

    public async Task Echo(string message)
    {
        await Clients.Caller.Message(message);
    }

    #endregion


    #region UserMethods

    private User? GetCurrentUser()
    {
        lock (Users)
        {
            return Users.SingleOrDefault(u => u.ConnectionId == Context.ConnectionId);
        }
    }

    public override async Task OnConnectedAsync()
    {
        lock (Users)
        {
            if (GetCurrentUser() == null) Users.Add(new User(Context.ConnectionId));
        }

        Console.WriteLine($"User connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = GetCurrentUser();
        if (user != null)
        {
            await LeaveGame();
            Users.Remove(user);
        }

        Console.WriteLine($"User disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    private async Task AddUserToGame(User user, MultiplayerGame game, UserRole role)
    {
        await Groups.AddToGroupAsync(user.ConnectionId, game.Id);
        user.Game = game;
        user.Role = role;
        switch (role)
        {
            case UserRole.Audience:
                game.Audience.Add(user);
                break;
            case UserRole.Spectator:
                game.Spectators.Add(user);
                break;
            case UserRole.Host:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }
    }

    private async Task RemoveUserFromGame(User user)
    {
        if (user.Game == null) return;
        await Groups.RemoveFromGroupAsync(user.ConnectionId, user.Game.Id);
        user.Game = null;
    }

    public void SetName(string name)
    {
        var user = GetCurrentUser();
        if (user == null) return;

        user.Name = name;
        Console.WriteLine($"Registered player: {name}");
    }

    #endregion


    #region GameMethods

    private static MultiplayerGame? GetGameById(string gameId)
    {
        return Games.SingleOrDefault(g => g.Id == gameId);
    }

    private MultiplayerGame? GetCurrentGame()
    {
        return GetCurrentUser()?.Game;
    }

    private static MultiplayerGame CreateGame(User host)
    {
        // Make sure ID is unique
        string gameId;
        do
        {
            gameId = MultiplayerGame.GenerateRoomId();
        } while (Games.Any(g => g.Id == gameId));

        return new MultiplayerGame
        {
            Id = gameId,
            Host = host
        };
    }

    public async Task HostGame()
    {
        var user = GetCurrentUser();
        if (user == null || user.Name == null) return;

        // Create game
        var game = CreateGame(user);
        Games.Add(game);

        // Join game
        await LeaveGame();
        await AddUserToGame(user, game, UserRole.Host);
        await JoinSuccessful(game);
    }

    public async Task JoinGameAudience(string gameId)
    {
        var user = GetCurrentUser();
        if (user == null || user.Name == null) return;

        var game = GetGameById(gameId);
        if (game == null)
        {
            await Clients.Caller.JoinGameIdNotFound();
            return;
        }

        if (game.Audience.Contains(user)) return;

        // Join game
        await LeaveGame();
        await AddUserToGame(user, game, UserRole.Audience);

        await Clients.OthersInGroup(game.Id).PlayerJoined(user.ToViewModel());
        await Clients.OthersInGroup(game.Id).Message($"{user.Name} has joined the game.");
        await JoinSuccessful(game);
    }

    public async Task SpectateGame(string gameId)
    {
        var user = GetCurrentUser();
        if (user == null) return;

        var game = GetGameById(gameId);
        if (game == null)
        {
            await Clients.Caller.JoinGameIdNotFound();
            return;
        }

        if (game.Spectators.Contains(user)) return;

        // Join game
        await LeaveGame();
        await AddUserToGame(user, game, UserRole.Spectator);
        await JoinSuccessful(game);
    }

    private async Task LeaveGame()
    {
        var user = GetCurrentUser();
        if (user == null) return;

        var game = user.Game;
        if (game != null)
        {
            if (game.Host == user) await EndGame(game);

            await RemoveUserFromGame(user);
            if (user.Name != null) await Clients.Group(game.Id).PlayerLeft(user.ToViewModel());
            await Clients.OthersInGroup(game.Id).Message($"{user.Name} has left the game.");
            await Clients.Caller.Message($"You have left game {game.Id}");
        }
    }

    private async Task EndGame(MultiplayerGame game)
    {
        await Clients.Group(game.Id).Message("Game ended by host");
        await Clients.GroupExcept(game.Id, game.Host.ConnectionId).GameEnded();

        foreach (var user in game.Audience) await RemoveUserFromGame(user);
        foreach (var user in game.Spectators) await RemoveUserFromGame(user);
        await RemoveUserFromGame(game.Host);

        Games.Remove(game);
    }

    private async Task JoinSuccessful(MultiplayerGame game)
    {
        await Clients.Caller.Message($"Welcome to game {game.Id}. Your host is {game.Host.Name}.");
        var players = game.GetPlayers().Select(u => u.ToViewModel());
        await Clients.Caller.PopulatePlayerList(players);
        await Clients.Caller.JoinSuccessful(game.Id);
    }

    #endregion


    #region GameSetupMethods

    public async Task UpdateSwitchSetting(string settingName, bool value)
    {
        var game = GetCurrentGame();
        if (game == null) return;

        switch (settingName)
        {
            case "muteHostSound":
                game.Settings.MuteHostSound = value;
                break;
            case "muteAudienceSound":
                game.Settings.MuteAudienceSound = value;
                break;
            case "muteSpectatorSound":
                game.Settings.MuteSpectatorSound = value;
                break;
            default:
                await Clients.Caller.Message("Setting not found");
                return;
        }

        await Clients.Caller.Message("Settings updated successfully");
    }

    #endregion


    #region FastestFingerMethods

    public async Task RequestFastestFinger()
    {
        var game = GetCurrentGame();
        if (game == null || !game.IsReadyForNewRound()) return;

        game.SetupFastestFingerRound();

        await Clients.Group(game.Id).SetBackground(0);
        await Clients.Group(game.Id).Hide("gameSetupPanels");
        await Clients.Group(game.Id).Show("fastestFingerPanels", "flex");

        var players = ((FastestFingerFirst)game.Round!).GetPlayerIds();
        await Clients.Clients(players).Show("fastestFingerInput");
        await Clients.Clients(players).Show("fastestFingerBtns", "flex");

        await Clients.Caller.Hide("hostMenu");
        await Clients.Group(game.Id).Show("questionAndAnswers");
    }

    public async Task FetchFastestFingerQuestion()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.Setup } round)
        {
            if (round.Question == null)
            {
                await Clients.Caller.Message("Question has not been set for this round.");
            }
            else
            {
                await Clients.Group(game.Id).SetBackground(1, true);

                await Clients.Group(game.Id).SetText("question", round.Question.Question);
                await Clients.Group(game.Id).SetText("fffQuestion", round.Question.Question);
                await Clients.Caller.SetOnClick("fffNextBtn", "StartFastestFinger");
            }
        }
    }

    public async Task StartFastestFinger()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.Setup } round)
        {
            if (round.Question == null)
            {
                await Clients.Caller.Message("Question has not been set for this round.");
            }
            else
            {
                await Clients.Group(game.Id).SetBackground(0, true);

                await Clients.Caller.Lock("fffNextBtn");
                await Clients.Group(game.Id).StartFastestFinger(round.Question.Answers);
                await Clients.Clients(round.GetPlayerIds()).EnableFastestFingerAnswering();
                await round.StartRoundAndWait();
                await StopFastestFinger();
            }
        }
    }

    public async Task SubmitFastestFingerAnswer(IEnumerable<char> answerOrder, double time)
    {
        var user = GetCurrentUser();
        var game = user?.Game;

        if (game?.Round is FastestFingerFirst round)
        {
            if (round.State == FastestFingerFirst.RoundState.InProgress)
                round.SubmitAnswer(user!, answerOrder, time);
            else
                await Clients.Caller.Message("The round is not currently in progress.");
        }
    }

    private async Task StopFastestFinger()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.AnswerReveal })
        {
            await Clients.Group(game.Id).SetBackground(1);

            var players = ((FastestFingerFirst)game.Round!).GetPlayerIds();
            await Clients.Clients(players).DisableFastestFingerAnswering();

            await Clients.Caller.SetOnClick("fffNextBtn", "ShowFastestFingerAnswerPanel");
            await Clients.Caller.Unlock("fffNextBtn");
        }
    }

    public async Task ShowFastestFingerAnswerPanel()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.AnswerReveal })
        {
            var players = ((FastestFingerFirst)game.Round!).GetPlayerIds();

            await Clients.Group(game.Id).Hide("questionAndAnswers");
            await Clients.Clients(players).Hide("fastestFingerBtns");
            await Clients.Clients(players).Hide("fastestFingerInput");
            await Clients.Group(game.Id).Show("fffAnswerPanel");
            await Clients.Caller.SetOnClick("fffNextBtn", "RevealNextFastestFingerAnswer");
        }
    }

    public async Task RevealNextFastestFingerAnswer()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst
            {
                State: FastestFingerFirst.RoundState.AnswerReveal, Question: { }
            } round)
            try
            {
                var index = round.AnswerRevealIndex;
                var letter = round.GetNextAnswer();
                var answer = round.Question.Answers[letter];
                await Clients.Group(game.Id).ShowFastestFingerAnswer(index, letter, answer);

                if (index == 3) await Clients.Caller.SetOnClick("fffNextBtn", "ShowFastestFingerResultsPanel");
            }
            catch (ArgumentOutOfRangeException)
            {
                await Clients.Caller.Message("All answers have been revealed.");
            }
    }

    public async Task ShowFastestFingerResultsPanel()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst
            {
                State: FastestFingerFirst.RoundState.AnswerReveal, Question: { }
            } round)
        {
            round.State = FastestFingerFirst.RoundState.ResultsReveal;

            var players = ((FastestFingerFirst)game.Round!).GetPlayerIds();
            await Clients.Group(game.Id)
                .PopulateFastestFingerResults(round.Players.Select(u => u.ToViewModel()).OrderBy(u => u.Name));
            await Clients.Group(game.Id).Show("fffResultsPanel", "flex");
            await Clients.Clients(players).Hide("fffDefaultPanel");
            await Clients.Caller.SetOnClick("fffNextBtn", "RevealCorrectFastestFingerPlayers");
        }
    }

    public async Task RevealCorrectFastestFingerPlayers()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.ResultsReveal } round)
        {
            var correctUserTimes =
                round.GetTimesForCorrectPlayers().ToDictionary(x => x.Key.ConnectionId, v => v.Value);
            await Clients.Group(game.Id).RevealCorrectFastestFingerPlayers(correctUserTimes);
            await Clients.Caller.SetOnClick("fffNextBtn", "RevealFastestFingerWinners");
        }
    }

    public async Task RevealFastestFingerWinners()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.ResultsReveal } round)
        {
            var winners = round.GetWinners();

            if (winners.Count > 0)
                await Clients.Group(game.Id).SetBackground(0);

            foreach (var winner in winners)
                await Clients.Group(game.Id).HighlightFastestFingerWinner(winner.ConnectionId);

            await Clients.Caller.SetOnClick("fffNextBtn", "EndFastestFingerRound");
        }
    }

    public async Task EndFastestFingerRound()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.ResultsReveal } round)
        {
            var winners = round.GetWinners();
            game.NextPlayer = winners.Count == 1 ? winners.First() : null;
            game.ResetRound();

            var players = game.GetPlayers().Select(u => u.ToViewModel()).ToList();
            foreach (var winner in winners)
                players.Single(u => u.ConnectionId == winner.ConnectionId).IsFastestFingerWinner = true;
            await Clients.Group(game.Id).PopulatePlayerList(players);

            // Reset UI
            await Clients.Group(game.Id).SetBackground(3);

            await Clients.Group(game.Id).Hide("fffResultsPanel");
            await Clients.Group(game.Id).Hide("fffAnswerPanel");
            await Clients.Group(game.Id).Hide("fastestFingerPanels");
            await Clients.Group(game.Id).Show("gameSetupPanels");
            await Clients.Caller.Show("hostMenu");

            await Clients.Group(game.Id).SetText("question", "");
            await Clients.Group(game.Id).SetAnswerText("answerA", "");
            await Clients.Group(game.Id).SetAnswerText("answerB", "");
            await Clients.Group(game.Id).SetAnswerText("answerC", "");
            await Clients.Group(game.Id).SetAnswerText("answerD", "");

            await Clients.Group(game.Id).SetText("fffQuestion", "");
            await Clients.Group(game.Id).SetOnClick("fffNextBtn", "FetchFastestFingerQuestion");
            await Clients.Group(game.Id).ResetFastestFinger();
        }
    }

    #endregion
}