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
    Task SetOnClick(string elementId, string onclick, char charArg);
    Task Disable(string elementId);
    Task Enable(string elementId);
    Task SetBackground(int imageNumber, bool useRedVariant = false);

    Task StartFastestFinger(Dictionary<char, string> answers);
    Task EnableFastestFingerAnswering();
    Task DisableFastestFingerAnswering();
    Task ShowFastestFingerAnswer(int answerIndex, char letter, string answerText);
    Task PopulateFastestFingerResults(IEnumerable<UserViewModel> players);
    Task RevealCorrectFastestFingerPlayers(Dictionary<string, double> correctUserTimes);
    Task HighlightFastestFingerWinner(string connectionId);
    Task ResetFastestFinger();
    Task ResetFastestFingerInput();

    Task NoNextPlayer(IEnumerable<UserViewModel> players);
    Task DismissChoosePlayerModal();
    Task SelectAnswer(char letter);
    Task FlashCorrectAnswer(char letter);
    Task HighlightCorrectAnswer(char letter);
    Task ShowWinnings(string amount);
    Task HideWinnings();
    Task SetMoneyTree(int questionNumber);
    Task ResetMoneyTree();
    Task ResetAnswerBackgrounds();
    Task ShowTotalPrize(string amount);
    Task ShowMillionaireBanner(string playerName);
}

public class MultiplayerGameHub : Hub<IMultiplayerGameHub>
{
    private static List<MultiplayerGame> Games { get; } = new();
    private static List<User> Users { get; } = new();

    private static List<string> LockUnlockIds { get; } = new()
    {
        "answerA", "answerB", "answerC", "answerD",
        "walkAwayBtn",
        "lifeline-5050", "lifeline-phone", "lifeline-audience"
    };

    // TEMP
    public async Task JoinRandomAudience()
    {
        await JoinGameAudience(Games.First().Id);
    }

    public async Task JoinRandomSpectators()
    {
        await SpectateGame(Games.First().Id);
    }

    #region MiscellaneousMethods

    public async Task Echo(string message)
    {
        await Clients.Caller.Message(message);
    }

    private IMultiplayerGameHub Spectators(MultiplayerGame game)
    {
        return game.Settings.AudienceAreSpectators
            ? Everyone(game)
            : Clients.GroupExcept(game.Id, game.Audience.Select(u => u.ConnectionId));
    }

    private IMultiplayerGameHub SpectatorsAndPlayers(MultiplayerGame game, FastestFingerFirst round)
    {
        var nonPlayingAudience = game.Audience.Where(u => !round.Players.Contains(u));
        return game.Settings.AudienceAreSpectators
            ? Everyone(game)
            : Clients.GroupExcept(game.Id, nonPlayingAudience.Select(u => u.ConnectionId));
    }

    private IMultiplayerGameHub SpectatorsExceptHost(MultiplayerGame game, FastestFingerFirst round)
    {
        return game.Settings.AudienceAreSpectators
            ? Clients.GroupExcept(game.Id, game.Host.ConnectionId)
            : Clients.GroupExcept(game.Id, game.Audience.Append(game.Host).Select(u => u.ConnectionId));
    }

    private IMultiplayerGameHub Players(FastestFingerFirst round)
    {
        return Clients.Clients(round.GetPlayerIds());
    }

    private IMultiplayerGameHub Host(MultiplayerGame game)
    {
        return Clients.Client(game.Host.ConnectionId);
    }

    private IMultiplayerGameHub Everyone(MultiplayerGame game)
    {
        return Clients.Group(game.Id);
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
        user.Role = null;
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

        await Host(game).Enable("playFastestFingerBtn");
        await Host(game).Enable("playMainGameBtn");

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
            game.RemoveUser(user);
            if (user.Name != null) await Everyone(game).PlayerLeft(user.ToViewModel());
            if (game.Audience.Count < 1)
            {
                await Host(game).Disable("playFastestFingerBtn");
                await Host(game).Disable("playMainGameBtn");
            }

            await Clients.OthersInGroup(game.Id).Message($"{user.Name} has left the game.");
            await Clients.Caller.Message($"You have left game {game.Id}");
        }
    }

    private async Task EndGame(MultiplayerGame game)
    {
        await Everyone(game).Message("Game ended by host");
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
            case "audienceAreSpectators":
                game.Settings.AudienceAreSpectators = value;
                break;
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

        var round = game.SetupFastestFingerRound();

        await Everyone(game).SetBackground(0);
        await Everyone(game).Hide("gameSetupPanels");
        await Everyone(game).Show("fastestFingerPanels", "flex");

        await Players(round).Show("fastestFingerInput");
        await Players(round).Show("fastestFingerBtns", "flex");

        await Host(game).Hide("hostMenu");
        await SpectatorsAndPlayers(game, round).Show("questionAndAnswers");
    }

    public async Task FetchFastestFingerQuestion()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.Setup } round)
        {
            if (round.Question == null)
            {
                await Host(game).Message("Question has not been set for this round.");
            }
            else
            {
                await Everyone(game).SetBackground(1, true);

                await SpectatorsAndPlayers(game, round).SetText("question", round.Question.Question);
                await Spectators(game).SetText("fffQuestion", round.Question.Question);
                await Host(game).SetOnClick("fffNextBtn", "StartFastestFinger");
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
                await Host(game).Message("Question has not been set for this round.");
            }
            else
            {
                await Everyone(game).SetBackground(0, true);

                await Host(game).Disable("fffNextBtn");
                await SpectatorsAndPlayers(game, round).StartFastestFinger(round.Question.Answers);
                await Players(round).EnableFastestFingerAnswering();
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
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.AnswerReveal } round)
        {
            await Everyone(game).SetBackground(1);

            await Players(round).DisableFastestFingerAnswering();

            await Host(game).SetOnClick("fffNextBtn", "ShowFastestFingerAnswerPanel");
            await Host(game).Enable("fffNextBtn");
        }
    }

    public async Task ShowFastestFingerAnswerPanel()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.AnswerReveal } round)
        {
            await SpectatorsAndPlayers(game, round).Hide("questionAndAnswers");
            await Players(round).Hide("fastestFingerBtns");
            await Players(round).Hide("fastestFingerInput");
            await Spectators(game).Show("fffAnswerPanel");
            await Host(game).SetOnClick("fffNextBtn", "RevealNextFastestFingerAnswer");
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
                await Spectators(game).ShowFastestFingerAnswer(index, letter, answer);

                if (index == 3) await Host(game).SetOnClick("fffNextBtn", "ShowFastestFingerResultsPanel");
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

            await Spectators(game)
                .PopulateFastestFingerResults(round.Players.Select(u => u.ToViewModel()).OrderBy(u => u.Name));
            await Spectators(game).Show("fffResultsPanel", "flex");
            await SpectatorsExceptHost(game, round).Hide("fffDefaultPanel");
            await Host(game).SetOnClick("fffNextBtn", "RevealCorrectFastestFingerPlayers");
        }
    }

    public async Task RevealCorrectFastestFingerPlayers()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.ResultsReveal } round)
        {
            var correctUserTimes =
                round.GetTimesForCorrectPlayers().ToDictionary(x => x.Key.ConnectionId, v => v.Value);
            await Spectators(game).RevealCorrectFastestFingerPlayers(correctUserTimes);
            await Host(game).SetOnClick("fffNextBtn", "RevealFastestFingerWinners");
        }
    }

    public async Task RevealFastestFingerWinners()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.ResultsReveal } round)
        {
            var winners = round.GetWinners();

            if (winners.Count > 0)
                await Everyone(game).SetBackground(0);

            foreach (var winner in winners)
                await Spectators(game).HighlightFastestFingerWinner(winner.ConnectionId);

            await Host(game).SetOnClick("fffNextBtn", "EndFastestFingerRound");
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
            await Everyone(game).PopulatePlayerList(players);

            // Reset UI
            await Everyone(game).SetBackground(3);

            await Spectators(game).Hide("fffResultsPanel");
            await Spectators(game).Hide("fffAnswerPanel");
            await Everyone(game).Hide("fastestFingerPanels");
            await Everyone(game).Show("gameSetupPanels");
            await Host(game).Show("hostMenu");

            await ResetQuestion();

            await Spectators(game).SetText("fffQuestion", "");
            await Host(game).SetOnClick("fffNextBtn", "FetchFastestFingerQuestion");
            await SpectatorsExceptHost(game, round).Show("fffDefaultPanel");
            await SpectatorsAndPlayers(game, round).ResetFastestFinger();
            await Players(round).ResetFastestFingerInput();
        }
    }

    #endregion


    #region MainGameMethods

    public async Task RequestMainGame()
    {
        var game = GetCurrentGame();
        if (game == null || !game.IsReadyForNewRound()) return;

        if (game.NextPlayer == null)
            await Clients.Caller.NoNextPlayer(game.Audience.Select(u => u.ToViewModel()));
        else
            await StartMainGame();
    }

    public async Task SetPlayerAndStart(string connectionId)
    {
        var game = GetCurrentGame();
        if (game == null || !game.IsReadyForNewRound()) return;

        var nextPlayer = Users.SingleOrDefault(u => u.ConnectionId == connectionId);
        if (nextPlayer == null) return;

        game.NextPlayer = nextPlayer;
        await StartMainGame();
    }

    private async Task StartMainGame()
    {
        var game = GetCurrentGame();
        game!.SetupMillionaireRound();

        await Clients.Caller.DismissChoosePlayerModal();
        await Everyone(game).SetBackground(0);

        var player = (game.Round as MillionaireRound)!.Player;
        await Host(game).SetText("contestantName", player!.Name!);

        await Spectators(game).Show("moneyTreePanel");
        await Everyone(game).Hide("gameSetupPanels");
        await Everyone(game).Show("mainGamePanels", "flex");

        await Host(game).Hide("hostMenu");
        await Spectators(game).Show("questionAndAnswers");
    }

    public async Task LetsPlay()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            await Everyone(game).SetBackground(round.GetBackgroundNumber());
            await Host(game).SetOnClick("nextBtn", "FetchNextQuestion");
        }
    }

    public async Task FetchNextQuestion()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            await Spectators(game).SetText("question", round.GetCurrentQuestion().Question);
            await Host(game).SetOnClick("nextBtn", "FetchAnswer", 'A');
        }
    }

    private async Task Lock(MillionaireRound round)
    {
        foreach (var id in LockUnlockIds) await Clients.Caller.Disable(id);
        round.Locked = true;
    }

    private async Task Unlock(MillionaireRound round)
    {
        foreach (var id in LockUnlockIds) await Clients.Caller.Enable(id);
        round.Locked = false;
    }

    public async Task FetchAnswer(char letter)
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            var question = round.GetCurrentQuestion();
            await Spectators(game).SetAnswerText($"answer{letter}", question.Answers[letter]);
            if (letter == 'D')
            {
                await Host(game).Disable("nextBtn");
                await Unlock(round);
            }
            else
            {
                await Host(game).SetOnClick("nextBtn", "FetchAnswer", ++letter);
            }
        }
    }

    public async Task SubmitAnswer(char letter)
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            await SelectAnswer(letter);

            if (round.QuestionNumber > 5 || round.HasWalkedAway)
                await ShowHostCorrectAnswer();
            else
                await RevealAnswer();
        }
    }

    private async Task SelectAnswer(char letter)
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            await Lock(round);
            round.SubmittedAnswer = letter;
            await Spectators(game).SelectAnswer(letter);
        }
    }

    private async Task ShowHostCorrectAnswer()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            var correctLetter = round.GetCurrentQuestion().CorrectLetter;
            await Host(game).HighlightCorrectAnswer(correctLetter);
            await Host(game).SetOnClick("nextBtn", "RevealAnswer");
            await Host(game).Enable("nextBtn");
        }
    }

    public async Task RevealAnswer()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            await Host(game).Disable("nextBtn");

            var correctLetter = round.GetCurrentQuestion().CorrectLetter;
            await Spectators(game).FlashCorrectAnswer(correctLetter);
            if (round.QuestionNumber >= 5) await Everyone(game).SetBackground(0);

            if (round.HasWalkedAway)
            {
                await Host(game).SetOnClick("nextBtn", "GameOver");
                await Host(game).Enable("nextBtn");
            }
            else
            {
                if (round.SubmittedAnswer == correctLetter)
                    await CorrectAnswer();
                else
                    await WrongAnswer();
            }
        }
    }

    private async Task CorrectAnswer()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            await Task.Delay(3000);
            await Spectators(game).SetMoneyTree(round.QuestionNumber);

            if (round.QuestionNumber == 15)
            {
                await Win();
                return;
            }

            await Spectators(game).ShowWinnings(round.GetWinnings());
            await Task.Delay(round.QuestionNumber is 5 or 10 ? 5000 : 2000);
            await ResetQuestion();
            await Spectators(game).HideWinnings();

            round.FinishQuestion();

            await LetsPlay();
            await Host(game).Enable("nextBtn");

            await Host(game).SetText("questionNumber", round.QuestionNumber.ToString());
            await Host(game).SetText("questionsAway", round.GetQuestionsAway().ToString());
            await Host(game).SetText("unsafeAmount", round.GetUnsafeAmount());
        }
    }

    private async Task WrongAnswer()
    {
        var game = GetCurrentGame();
        if (game?.Round is not MillionaireRound) return;

        await Host(game).Message("wrong answer");

        await Host(game).SetOnClick("nextBtn", "GameOver");
        await Host(game).Enable("nextBtn");
    }

    private async Task ResetQuestion()
    {
        var game = GetCurrentGame();
        if (game == null) return;

        await Everyone(game).SetText("question", "");
        await Everyone(game).SetAnswerText("answerA", "\u00a0");
        await Everyone(game).SetAnswerText("answerB", "\u00a0");
        await Everyone(game).SetAnswerText("answerC", "\u00a0");
        await Everyone(game).SetAnswerText("answerD", "\u00a0");
        await Everyone(game).ResetAnswerBackgrounds();
    }

    public async Task WalkAway()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound { Locked: false } round)
        {
            await Lock(round);
            round.HasWalkedAway = true;
            foreach (var id in new List<string> { "answerA", "answerB", "answerC", "answerD" })
                await Host(game).Enable(id);
            await Everyone(game).SetBackground(0);
        }
    }

    public async Task GameOver()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            await Everyone(game).SetBackground(3);
            await Spectators(game).ShowTotalPrize(round.GetTotalPrizeString());
            await Host(game).SetOnClick("nextBtn", "EndMainGameRound");
        }
    }

    private async Task Win()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound { QuestionNumber: 15 } round)
        {
            await Spectators(game).ShowMillionaireBanner(round.Player?.Name ?? "");
            await Host(game).SetOnClick("nextBtn", "EndMainGameRound");
            await Host(game).Enable("nextBtn");
            await Task.Delay(21000);
            await Everyone(game).SetBackground(3);
        }
    }

    private async Task ResetStatusBox()
    {
        var game = GetCurrentGame();
        if (game == null) return;
        await Host(game).SetText("questionNumber", "1");
        await Host(game).SetText("questionsAway", "15");
        await Host(game).SetText("unsafeAmount", "£0");
    }

    public async Task EndMainGameRound()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound { Player: { } } round)
        {
            game.SaveScore(round.Player!, round.GetTotalPrize());
            game.ResetRound();
            game.NextPlayer = null;

            var players = game.GetPlayers().Select(u => u.ToViewModel()).ToList();
            await Everyone(game).PopulatePlayerList(players);

            await Everyone(game).Hide("mainGamePanels");
            await Everyone(game).Show("gameSetupPanels");
            await Host(game).Show("hostMenu");

            await ResetQuestion();
            await ResetStatusBox();

            await Spectators(game).ResetMoneyTree();
            await Host(game).SetOnClick("nextBtn", "LetsPlay");
            await Spectators(game).Hide("totalPrize");
            await Spectators(game).Hide("millionairePrize");
            await Spectators(game).Hide("moneyTreePanel");
        }
    }

    #endregion
}