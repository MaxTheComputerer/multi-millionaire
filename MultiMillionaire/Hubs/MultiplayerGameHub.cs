using System.Net;
using Microsoft.AspNetCore.SignalR;
using MultiMillionaire.Models;
using MultiMillionaire.Models.Lifelines;
using MultiMillionaire.Models.Questions;
using MultiMillionaire.Models.Rounds;
using MultiMillionaire.Services;

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
    Task ShowToastMessage(string message);

    Task StartFastestFinger(Dictionary<char, string> answers);
    Task EnableFastestFingerAnswering();
    Task DisableFastestFingerAnswering();
    Task StopFastestFingerVoteMusic();
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

    Task UseFiftyFifty(IEnumerable<char> answersToRemove);
    Task UsePhoneAFriend();
    Task UseAskTheAudience();
    Task StartPhoneClock();
    Task StopPhoneClockMusic();
    Task SetAudienceAnswersOnClick();
    Task ResetAudienceAnswersOnClick();
    Task ResetLifelines();
    Task DrawAudienceGraphGrid();
    Task DrawAudienceGraphResults(Dictionary<char, int> percentages);
    Task LockAudienceSubmission();
    Task ResetAudienceGraph();

    Task PlaySound(string path, double attack = 40);
    Task StopSound(string path);
    Task FadeOutSound(string path, double duration = 400);
    Task UnloadSounds();
    Task LoadSounds();
}

public class MultiplayerGameHub : Hub<IMultiplayerGameHub>
{
    private readonly IOrderQuestionsService _orderQuestionsService;
    private readonly IMultiplayerHubStorage _storage;

    public MultiplayerGameHub(IMultiplayerHubStorage multiplayerHubStorage,
        IOrderQuestionsService orderQuestionsService)
    {
        _storage = multiplayerHubStorage;
        _orderQuestionsService = orderQuestionsService;
    }

    // TEMP
    public async Task JoinRandomAudience()
    {
        await JoinGameAudience(_storage.Games.First().Id);
    }

    public async Task JoinRandomSpectators()
    {
        await SpectateGame(_storage.Games.First().Id);
    }

    public async Task GetAllOrderQuestions()
    {
        var question = OrderQuestion.FromDbModel(await _orderQuestionsService.GetRandom());
        Console.WriteLine(question);
    }

    #region MiscellaneousMethods

    public async Task Echo(string message)
    {
        await Clients.Caller.Message(message);
    }

    private async Task SetBackground(int imageNumber, bool useRedVariant = false)
    {
        var game = GetCurrentGame();
        if (game == null) return;

        await Everyone(game).SetBackground(imageNumber, useRedVariant);

        if (game.Settings.UseLifxLight && game.Light != null)
            await game.Light.SetColourFromBackgroundImage(imageNumber, useRedVariant);
    }

    #endregion


    #region ClientsMethods

    private IMultiplayerGameHub Spectators(MultiplayerGame game)
    {
        return game.Settings.AudienceAreSpectators
            ? Everyone(game)
            : Clients.GroupExcept(game.Id, game.Audience.Select(u => u.ConnectionId));
    }

    private IMultiplayerGameHub SpectatorsOnly(MultiplayerGame game)
    {
        return Clients.Clients(game.Spectators.Select(u => u.ConnectionId));
    }

    private IMultiplayerGameHub SpectatorsAndPlayers(MultiplayerGame game, FastestFingerFirst round)
    {
        var nonPlayingAudience = game.Audience.Where(u => !round.Players.Contains(u));
        return game.Settings.AudienceAreSpectators
            ? Everyone(game)
            : Clients.GroupExcept(game.Id, nonPlayingAudience.Select(u => u.ConnectionId));
    }

    private IMultiplayerGameHub SpectatorsExceptHost(MultiplayerGame game)
    {
        return game.Settings.AudienceAreSpectators
            ? Clients.GroupExcept(game.Id, game.Host.ConnectionId)
            : Clients.GroupExcept(game.Id, game.Audience.Append(game.Host).Select(u => u.ConnectionId));
    }

    private IMultiplayerGameHub Players(FastestFingerFirst round)
    {
        return Clients.Clients(round.GetPlayerIds());
    }

    private IMultiplayerGameHub AudienceExceptPlayer(MultiplayerGame game, MillionaireRound round)
    {
        return Clients.Clients(game.Audience.Where(u => u != round.Player).Select(u => u.ConnectionId));
    }

    private IMultiplayerGameHub Host(MultiplayerGame game)
    {
        return Clients.Client(game.Host.ConnectionId);
    }

    private IMultiplayerGameHub Everyone(MultiplayerGame game)
    {
        return Clients.Group(game.Id);
    }

    private IMultiplayerGameHub Listeners(MultiplayerGame game)
    {
        return Clients.Clients(game.GetListeners().Select(u => u.ConnectionId));
    }

    #endregion


    #region UserMethods

    private User? GetCurrentUser()
    {
        return _storage.GetUserById(Context.ConnectionId);
    }

    public override async Task OnConnectedAsync()
    {
        lock (_storage.Users)
        {
            if (GetCurrentUser() == null) _storage.Users.Add(new User(Context.ConnectionId));
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
            _storage.Users.Remove(user);
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

    private MultiplayerGame? GetGameById(string gameId)
    {
        return _storage.GetGameById(gameId);
    }

    private MultiplayerGame? GetCurrentGame()
    {
        return GetCurrentUser()?.Game;
    }

    private MultiplayerGame CreateGame(User host)
    {
        // Make sure ID is unique
        string gameId;
        do
        {
            gameId = MultiplayerGame.GenerateRoomId();
        } while (_storage.Games.Any(g => g.Id == gameId));

        return new MultiplayerGame
        {
            Id = gameId,
            Host = host
        };
    }

    public async Task HostGame()
    {
        var user = GetCurrentUser();
        if (user?.Name == null) return;

        // Create game
        var game = CreateGame(user);
        _storage.Games.Add(game);

        // Join game
        await LeaveGame();
        await AddUserToGame(user, game, UserRole.Host);
        await JoinSuccessful(game);
    }

    public async Task JoinGameAudience(string gameId)
    {
        var user = GetCurrentUser();
        if (user?.Name == null) return;

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
        await Clients.GroupExcept(game.Id, user.ConnectionId, game.Host.ConnectionId)
            .Message($"{user.Name} has joined the game.");
        await Host(game).ShowToastMessage($"{user.Name} has joined the game.");
        await SynchroniseView(game);
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
        await SynchroniseView(game);
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

            await Clients.GroupExcept(game.Id, user.ConnectionId, game.Host.ConnectionId)
                .Message($"{user.Name} has left the game.");
            await Host(game).ShowToastMessage($"{user.Name} has left the game.");
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

        _storage.Games.Remove(game);
    }

    private async Task JoinSuccessful(MultiplayerGame game)
    {
        await Clients.Caller.Message($"Welcome to game {game.Id}. Your host is {game.Host.Name}.");
        var players = game.GetPlayers().Select(u => u.ToViewModel());
        await Clients.Caller.PopulatePlayerList(players);
        await Clients.Caller.JoinSuccessful(game.Id);
    }

    private async Task SynchroniseView(MultiplayerGame game)
    {
        switch (game.Round)
        {
            case MillionaireRound round:
            {
                await SynchroniseMillionaireRoundView(game, round);
                break;
            }
            case FastestFingerFirst round:
                await SynchroniseFastestFingerRoundView(game, round);
                break;
        }
    }

    private async Task SynchroniseMillionaireRoundView(MultiplayerGame game, MillionaireRound round)
    {
        var user = GetCurrentUser();
        if (user?.Role is UserRole.Spectator ||
            (user?.Role is UserRole.Audience && game.Settings.AudienceAreSpectators))
        {
            if (round.QuestionNumber > 1) await Clients.Caller.SetMoneyTree(round.QuestionNumber - 1);
            await Clients.Caller.Show("moneyTreePanel");
            await Clients.Caller.Show("questionAndAnswers");
        }

        await Clients.Caller.SetText("question", round.GetCurrentQuestion().Question);
        foreach (var letter in round.GetRemainingAnswers())
            await Clients.Caller.SetAnswerText($"answer{letter}", round.GetCurrentQuestion().Answers[letter]);

        if (round.SubmittedAnswer != null && !round.HasWalkedAway && game.GetListeners().Contains(user!))
            await Clients.Caller.PlaySound($"questions.music.{round.QuestionNumber}");

        await Clients.Caller.SetBackground(round.GetBackgroundNumber());
        await Clients.Caller.Hide("gameSetupPanels");
        await Clients.Caller.Show("mainGamePanels", "flex");
        if (user?.Role is UserRole.Spectator) await Clients.Caller.Hide("menu");
    }

    private async Task SynchroniseFastestFingerRoundView(MultiplayerGame game, FastestFingerFirst round)
    {
        var user = GetCurrentUser();
        var isListener = user != null && game.GetListeners().Contains(user);
        if (user?.Role is UserRole.Spectator ||
            (user?.Role is UserRole.Audience && game.Settings.AudienceAreSpectators))
        {
            switch (round.State)
            {
                case FastestFingerFirst.RoundState.InProgress:
                    await Clients.Caller.SetText("question", round.Question?.Question ?? "");
                    foreach (var letter in MultiplayerGame.AnswerLetters)
                        await Clients.Caller.SetAnswerText($"answer{letter}", round.Question?.Answers[letter] ?? "");
                    await Clients.Caller.Show("questionAndAnswers");

                    if (isListener)
                        await Clients.Caller.PlaySound("fastestFinger.vote");
                    await Clients.Caller.SetBackground(0, true);
                    break;
                case FastestFingerFirst.RoundState.AnswerReveal or FastestFingerFirst.RoundState.ResultsReveal:
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var letter = round.Question!.CorrectOrder[i];
                        var answer = round.Question.Answers[letter];
                        await Clients.Caller.ShowFastestFingerAnswer(i, letter, answer);
                    }

                    await Clients.Caller.Show("fffAnswerPanel");

                    if (round.State is FastestFingerFirst.RoundState.ResultsReveal)
                    {
                        await Clients.Caller
                            .PopulateFastestFingerResults(round.Players.Select(u => u.ToViewModel())
                                .OrderBy(u => u.Name));
                        await Clients.Caller.Show("fffResultsPanel", "flex");
                        await Clients.Caller.Hide("fffDefaultPanel");
                    }

                    if (isListener && round.State is FastestFingerFirst.RoundState.AnswerReveal)
                        await Clients.Caller.PlaySound("fastestFinger.answers.background");
                    await Clients.Caller.SetBackground(1);
                    break;
                }
                default:
                    if (isListener)
                        await Clients.Caller.PlaySound("fastestFinger.question");
                    await Clients.Caller.SetBackground(1, true);
                    break;
            }

            await Clients.Caller.SetText("fffQuestion", round.Question?.Question ?? "");
        }

        await Clients.Caller.Hide("gameSetupPanels");
        await Clients.Caller.Show("fastestFingerPanels", "flex");
        if (user?.Role is UserRole.Spectator) await Clients.Caller.Hide("menu");
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
                await LoadUnloadSounds(new[] { game.Host }, value);
                break;
            case "muteAudienceSound":
                await LoadUnloadSounds(game.Audience, value);
                game.Settings.MuteAudienceSound = value;
                break;
            case "muteSpectatorSound":
                await LoadUnloadSounds(game.Spectators, value);
                game.Settings.MuteSpectatorSound = value;
                break;
            case "useLifxLight":
                game.Settings.UseLifxLight = value;
                if (!value) game.DisconnectFromLight();
                break;
            default:
                await Clients.Caller.Message("Setting not found");
                return;
        }

        await Clients.Caller.Message("Setting updated successfully.");
    }

    public async Task UpdateTextSetting(string settingName, string value)
    {
        var game = GetCurrentGame();
        if (game == null) return;

        switch (settingName)
        {
            case "lifxLightIp":
                var parseSuccess = IPAddress.TryParse(value, out var parsedAddress);
                if (parseSuccess)
                {
                    game.Settings.LifxLightIp = parsedAddress!;
                    if (game.Settings.UseLifxLight)
                        await ConnectToLifxLight(game);
                    else
                        await Clients.Caller.ShowToastMessage(
                            "Failed to connect to light. Use Lifx light setting is false.");
                }
                else
                {
                    await Clients.Caller.ShowToastMessage("Invalid IP address entered.");
                    return;
                }

                break;
            default:
                await Clients.Caller.Message("Setting not found");
                return;
        }

        await Clients.Caller.Message("Setting updated successfully.");
    }

    private async Task LoadUnloadSounds(IEnumerable<User> listenersToChange, bool value)
    {
        var userIds = listenersToChange.Select(u => u.ConnectionId);
        if (value)
            await Clients.Clients(userIds).UnloadSounds();
        else
            await Clients.Clients(userIds).LoadSounds();
    }

    private async Task ConnectToLifxLight(MultiplayerGame game)
    {
        await Clients.Caller.ShowToastMessage("Connecting to light...");
        try
        {
            await game.ConnectToLight();
        }
        catch (Exception e)
        {
            await Clients.Caller.ShowToastMessage("Failed to connect to Lifx light.");
            await Clients.Caller.Message("Failed to connect to Lifx light: " + e.Message);
            return;
        }

        await game.Light!.SetColourFromBackgroundImage(3);
        await game.Light!.OnAsync();
        await Clients.Caller.ShowToastMessage("Successfully connected to Lifx light.");
    }

    #endregion


    #region FastestFingerMethods

    public async Task RequestFastestFinger()
    {
        var game = GetCurrentGame();
        if (game == null || !game.IsReadyForNewRound()) return;

        var round = game.SetupFastestFingerRound();

        await Listeners(game).PlaySound("fastestFinger.start");
        await Listeners(game).FadeOutSound("music.closing");
        await SetBackground(0);

        await Everyone(game).Hide("gameSetupPanels");
        await Everyone(game).Show("fastestFingerPanels", "flex");

        await Players(round).Show("fastestFingerInput");
        await Players(round).Show("fastestFingerBtns", "flex");

        await Host(game).Hide("menu");
        await SpectatorsOnly(game).Hide("menu");
        await SpectatorsAndPlayers(game, round).Show("questionAndAnswers");
    }

    public async Task FetchFastestFingerQuestion()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.Setup } round)
        {
            if (round.Question == null)
            {
                await Host(game).ShowToastMessage("Question has not been set for this round.");
            }
            else
            {
                await Host(game).Disable("fffNextBtn");
                await Listeners(game).PlaySound("fastestFinger.question");
                await Listeners(game).FadeOutSound("fastestFinger.start");
                await SetBackground(1, true);

                await Task.Delay(2000);

                await Host(game).Enable("fffNextBtn");
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
                await Host(game).ShowToastMessage("Question has not been set for this round.");
            }
            else
            {
                await Listeners(game).PlaySound("fastestFinger.vote");
                await Listeners(game).StopSound("fastestFinger.question");
                await SetBackground(0, true);

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
                await Clients.Caller.ShowToastMessage("The round is not currently in progress.");
        }
    }

    private async Task StopFastestFinger()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.AnswerReveal } round)
        {
            await Players(round).DisableFastestFingerAnswering();

            if (round.HaveAllPlayersAnswered())
                await Listeners(game).StopFastestFingerVoteMusic();
            await SetBackground(1);

            await Host(game).SetOnClick("fffNextBtn", "ShowFastestFingerAnswerPanel");
            await Host(game).Enable("fffNextBtn");
        }
    }

    public async Task ShowFastestFingerAnswerPanel()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.AnswerReveal } round)
        {
            await Listeners(game).PlaySound("fastestFinger.answers.background");
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
                await Listeners(game).PlaySound($"fastestFinger.answers.{index}");
                await Spectators(game).ShowFastestFingerAnswer(index, letter, answer);

                if (index == 3) await Host(game).SetOnClick("fffNextBtn", "ShowFastestFingerResultsPanel");
            }
            catch (ArgumentOutOfRangeException)
            {
                await Clients.Caller.ShowToastMessage("All answers have been revealed.");
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
            await SpectatorsExceptHost(game).Hide("fffDefaultPanel");
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

            await Listeners(game).FadeOutSound("fastestFinger.answers.background");
            await Listeners(game).PlaySound("fastestFinger.resultsReveal");

            await Spectators(game).RevealCorrectFastestFingerPlayers(correctUserTimes);
            await Host(game).SetOnClick("fffNextBtn", "RevealFastestFingerWinners");
            if (correctUserTimes.Count == 0) await Host(game).ShowToastMessage("No players answered correctly.");
        }
    }

    public async Task RevealFastestFingerWinners()
    {
        var game = GetCurrentGame();
        if (game?.Round is FastestFingerFirst { State: FastestFingerFirst.RoundState.ResultsReveal } round)
        {
            var winners = round.GetWinners();

            if (winners.Count > 0)
            {
                await Listeners(game).PlaySound("fastestFinger.winner");
                await SetBackground(0);
            }
            else
            {
                await Host(game).ShowToastMessage("There are no winners.");
            }

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
            await SetBackground(3);

            await Spectators(game).Hide("fffResultsPanel");
            await Spectators(game).Hide("fffAnswerPanel");
            await Everyone(game).Hide("fastestFingerPanels");
            await Everyone(game).Show("gameSetupPanels");
            await Host(game).Show("menu");
            await SpectatorsOnly(game).Show("menu");

            await ResetQuestion();

            await Spectators(game).SetText("fffQuestion", "");
            await Host(game).SetOnClick("fffNextBtn", "FetchFastestFingerQuestion");
            await SpectatorsExceptHost(game).Show("fffDefaultPanel");
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

        var nextPlayer = _storage.Users.SingleOrDefault(u => u.ConnectionId == connectionId);
        if (nextPlayer == null) return;

        game.NextPlayer = nextPlayer;
        await StartMainGame();
    }

    private async Task StartMainGame()
    {
        var game = GetCurrentGame();
        game!.SetupMillionaireRound();

        await Clients.Caller.DismissChoosePlayerModal();

        await Listeners(game).FadeOutSound("music.closing");
        await Listeners(game).PlaySound("music.hotSeat");
        await SetBackground(0);

        var player = (game.Round as MillionaireRound)!.Player;
        await Host(game).SetText("contestantName", player!.Name!);

        await Spectators(game).Show("moneyTreePanel");
        await Everyone(game).Hide("gameSetupPanels");
        await Everyone(game).Show("mainGamePanels", "flex");

        await Host(game).Hide("menu");
        await SpectatorsOnly(game).Hide("menu");
        await Everyone(game).ResetAnswerBackgrounds();
        await Spectators(game).Show("questionAndAnswers");
    }

    public async Task LightsDown()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            var questionNumber = round.QuestionNumber;
            if (questionNumber is 1 or > 5)
            {
                await Listeners(game).PlaySound($"questions.lightsDown.{questionNumber}");

                if (questionNumber == 1)
                    await Listeners(game).FadeOutSound("music.hotSeat");
                else
                    await Listeners(game).FadeOutSound($"questions.correct.{questionNumber - 1}");

                await Task.Delay(1000);
                await SetBackground(round.GetBackgroundNumber());
            }

            await Host(game).SetOnClick("nextBtn", "FetchNextQuestion");
        }
    }

    public async Task FetchNextQuestion()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            await Listeners(game).PlaySound($"questions.music.{round.QuestionNumber}");
            await Listeners(game).FadeOutSound($"questions.lightsDown.{round.QuestionNumber}");

            await Everyone(game).SetText("question", round.GetCurrentQuestion().Question);
            await Host(game).SetOnClick("nextBtn", "FetchAnswer", 'A');
        }
    }

    private async Task Lock(MillionaireRound round)
    {
        var ids = new List<string>
        {
            "answerA", "answerB", "answerC", "answerD",
            "walkAwayBtn",
            "lifeline-5050", "lifeline-phone", "lifeline-audience"
        };
        foreach (var id in ids) await Clients.Caller.Disable(id);
        round.Locked = true;
    }

    private async Task Unlock(MillionaireRound round)
    {
        var ids = new List<string> { "walkAwayBtn", "lifeline-5050", "lifeline-phone", "lifeline-audience" };
        foreach (var id in ids) await Clients.Caller.Enable(id);

        foreach (var letter in round.GetRemainingAnswers()) await Clients.Caller.Enable($"answer{letter}");
        round.Locked = false;
    }

    public async Task FetchAnswer(char letter)
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            var question = round.GetCurrentQuestion();
            await Everyone(game).SetAnswerText($"answer{letter}", question.Answers[letter]);
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
            if (round.Locked && !round.HasWalkedAway) return;
            if (round.FiftyFifty.IsAnswerRemoved(letter)) return;

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

            if (round.QuestionNumber > 5 && !round.HasWalkedAway)
            {
                await Listeners(game).PlaySound($"questions.final.{round.QuestionNumber}");
                await Listeners(game).FadeOutSound($"questions.music.{round.QuestionNumber}");
            }
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
            if (round.QuestionNumber >= 5) await SetBackground(0);

            if (round.HasWalkedAway)
            {
                await Host(game).SetOnClick("nextBtn", "GameOver");
                await Host(game).Enable("nextBtn");
            }
            else
            {
                if (round.QuestionNumber == 5)
                    await Listeners(game).StopSound($"questions.music.{round.QuestionNumber}");

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
            var questionNumber = round.QuestionNumber;
            await Listeners(game).PlaySound($"questions.correct.{questionNumber}");
            await Listeners(game).FadeOutSound($"questions.final.{questionNumber}");

            await Task.Delay(3000);
            await Spectators(game).SetMoneyTree(questionNumber);

            if (questionNumber == 15)
            {
                await Win();
                return;
            }

            await Spectators(game).ShowWinnings(round.GetWinnings());
            await Task.Delay(questionNumber is 5 or 10 ? 5000 : 2000);
            await ResetQuestion();
            await Spectators(game).HideWinnings();

            round.FinishQuestion();

            await LightsDown();
            await Host(game).Enable("nextBtn");

            await Host(game).SetText("questionNumber", round.QuestionNumber.ToString());
            await Host(game).SetText("questionsAway", round.GetQuestionsAway().ToString());
            await Host(game).SetText("unsafeAmount", round.GetUnsafeAmount());
        }
    }

    private async Task WrongAnswer()
    {
        var game = GetCurrentGame();
        if (game?.Round is not MillionaireRound round) return;

        var questionNumber = round.QuestionNumber;
        await Listeners(game).PlaySound($"questions.incorrect.{questionNumber}");
        await Listeners(game).FadeOutSound($"questions.final.{questionNumber}");
        await Listeners(game).StopSound($"questions.music.{questionNumber}");

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
            await Listeners(game).FadeOutSound($"questions.music.{round.QuestionNumber}");
            await SetBackground(0);
            foreach (var letter in round.GetRemainingAnswers())
                await Host(game).Enable($"answer{letter}");
        }
    }

    public async Task GameOver()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
        {
            await Listeners(game).PlaySound(round.HasWalkedAway ? "music.walk" : "music.gameOver");
            await SetBackground(3);
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
            await SetBackground(3);
            await Listeners(game).PlaySound("music.closing");
            await Listeners(game).StopSound("questions.correct.15");
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
            await Host(game).Show("menu");
            await SpectatorsOnly(game).Show("menu");

            await ResetQuestion();
            await ResetStatusBox();
            await ResetPhoneAFriend();
            await ResetAskTheAudience();

            await Spectators(game).ResetLifelines();
            await Spectators(game).ResetMoneyTree();

            await Host(game).SetOnClick("nextBtn", "LightsDown");
            await Spectators(game).Hide("totalPrize");
            await Spectators(game).Hide("millionairePrize");
            await Spectators(game).Hide("moneyTreePanel");
        }
    }

    #endregion


    #region LifelineMethods

    public async Task RequestFiftyFifty()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound { Locked: false, FiftyFifty.IsUsed: false } round)
        {
            var answersToRemove = round.GetFiftyFiftyAnswers();
            await Listeners(game).PlaySound("lifelines.fiftyFifty");
            await Everyone(game).UseFiftyFifty(answersToRemove);
        }
    }

    public async Task RequestPhoneAFriend()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound { Locked: false, PhoneAFriend.IsUsed: false } round)
        {
            await Lock(round);
            round.StartPhoneAFriend();
            await Spectators(game).UsePhoneAFriend();
            await Host(game).Hide("phoneClockPanel");
            await Host(game).Hide("defaultPanel");
            await Host(game).Show("phonePanel");
        }
    }

    public async Task ChooseWhoToPhone(bool useAi)
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound { Locked: true, PhoneAFriend.InProgress: true } round)
        {
            if (useAi)
            {
                round.PhoneAFriend.UseAi = true;
                await Spectators(game).Show("phoneAiResponse", "flex");
            }

            await Listeners(game).PlaySound("lifelines.phone.start");
            await Listeners(game).FadeOutSound($"questions.music.{round.QuestionNumber}");

            await Host(game).Hide("phoneSetupPanel");
            await Host(game).Show("phoneClockPanel");
            await SpectatorsExceptHost(game).Hide("defaultPanel");
            await SpectatorsExceptHost(game).Show("phonePanel");
        }
    }

    public async Task PhoneStartClock()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound { Locked: true, PhoneAFriend.InProgress: true } round)
        {
            await Host(game).Disable("phoneStartBtn");
            await Listeners(game).StopSound("lifelines.phone.start");
            await Listeners(game).PlaySound("lifelines.phone.clock");
            await Spectators(game).StartPhoneClock();

            if (round.PhoneAFriend.UseAi)
            {
                await Spectators(game).SetText("phoneAiResponseText", "Thinking...");
                await Task.Delay(Random.Shared.Next(5000, 10000));

                var response = round.GeneratePhoneAiResponse();
                await Spectators(game).SetText("phoneAiResponseText", response);
            }

            await Host(game).Enable("phoneDismissBtn");
        }
    }

    public async Task RequestQuestionMusic()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound round)
            await Listeners(game).PlaySound($"questions.music.{round.QuestionNumber}");
    }

    public async Task DismissPhoneAFriend()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound { Locked: true, PhoneAFriend.InProgress: true } round)
        {
            round.EndPhoneAFriend();
            await Listeners(game).StopPhoneClockMusic();
            await Spectators(game).Hide("phonePanel");
            await Spectators(game).Show("defaultPanel", "flex");
            await Unlock(round);
        }
    }

    private async Task ResetPhoneAFriend()
    {
        var game = GetCurrentGame();
        if (game == null) return;

        await Spectators(game).SetText("phoneAiResponseText", "Ringing...");
        await Spectators(game).Hide("phoneAiResponse");
        await Host(game).Show("phoneSetupPanel");
        await Host(game).Disable("phoneDismissBtn");
        await Host(game).Enable("phoneStartBtn");
    }

    public async Task RequestAskTheAudience()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound { Locked: false, AskTheAudience.IsUsed: false } round)
        {
            await Lock(round);
            round.StartAskTheAudience();
            await Spectators(game).UseAskTheAudience();

            if (game.Audience.Count <= 1) await Host(game).Disable("chooseLiveAudienceBtn");

            await Host(game).Hide("audienceGraphPanel");
            await Host(game).Hide("defaultPanel");
            await Host(game).Show("audiencePanel");
        }
    }

    public async Task ChooseAudienceToAsk(bool useAi)
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound
            {
                Locked: true, AskTheAudience.CurrentState: AskTheAudience.State.Setup
            } round)
        {
            if (useAi)
            {
                round.AskTheAudience.UseAi = useAi;
            }
            else
            {
                await AudienceExceptPlayer(game, round).SetAudienceAnswersOnClick();
                await AudienceExceptPlayer(game, round).Show("questionAndAnswers");
            }

            await Listeners(game).PlaySound("lifelines.audience.start");
            await Listeners(game).FadeOutSound($"questions.music.{round.QuestionNumber}");

            await Spectators(game).DrawAudienceGraphGrid();

            await Host(game).Hide("audienceSetupPanel");
            await Host(game).Show("audienceGraphPanel");
            await SpectatorsExceptHost(game).Hide("defaultPanel");
            await SpectatorsExceptHost(game).Show("audiencePanel");
            await SetBackground(0);
        }
    }

    public async Task StartAudienceVoting()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound
            {
                Locked: true, AskTheAudience.CurrentState: AskTheAudience.State.Setup
            } round)
        {
            await Host(game).Disable("audienceStartBtn");

            await Listeners(game).PlaySound("lifelines.audience.vote");
            await Listeners(game).FadeOutSound("lifelines.audience.start");

            if (round.AskTheAudience.UseAi)
            {
                await Task.Delay(Random.Shared.Next(5000, 10000));
                round.GenerateAudienceAiResponse();
            }
            else
            {
                foreach (var letter in round.GetRemainingAnswers())
                    await AudienceExceptPlayer(game, round).Enable($"answer{letter}");

                await round.AskTheAudience.StartVotingAndWait(game.Audience.Count - 1);
                await StopAudienceVoting(game, round);
            }

            await DisplayAudienceGraphResults();
        }
    }

    public async Task SubmitAudienceGuess(char letter)
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound
            {
                AskTheAudience.UseAi: false, AskTheAudience.CurrentState: AskTheAudience.State.InProgress
            } round)
        {
            await Clients.Caller.LockAudienceSubmission();
            round.AskTheAudience.SubmitVote(letter);
        }
    }

    private async Task StopAudienceVoting(MultiplayerGame game, MillionaireRound round)
    {
        await AudienceExceptPlayer(game, round).LockAudienceSubmission();
        if (!game.Settings.AudienceAreSpectators) await AudienceExceptPlayer(game, round).Hide("questionAndAnswers");
        await AudienceExceptPlayer(game, round).ResetAudienceAnswersOnClick();
    }

    private async Task DisplayAudienceGraphResults()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound { AskTheAudience.CurrentState: AskTheAudience.State.ResultsReveal } round)
        {
            await Listeners(game).PlaySound("lifelines.audience.results");
            await Listeners(game).StopSound("lifelines.audience.vote");
            await SetBackground(round.GetBackgroundNumber());

            var percentages = round.AskTheAudience.GetPercentages();
            await Spectators(game).DrawAudienceGraphResults(percentages);
            await Host(game).Enable("audienceDismissBtn");

            await Task.Delay(1000);
            await Listeners(game).PlaySound($"questions.music.{round.QuestionNumber}");
        }
    }

    public async Task DismissAskTheAudience()
    {
        var game = GetCurrentGame();
        if (game?.Round is MillionaireRound
            {
                Locked: true, AskTheAudience.CurrentState: AskTheAudience.State.ResultsReveal
            } round)
        {
            await Spectators(game).Hide("audiencePanel");
            await Spectators(game).Show("defaultPanel", "flex");
            await Unlock(round);
        }
    }

    private async Task ResetAskTheAudience()
    {
        var game = GetCurrentGame();
        if (game == null) return;

        await Spectators(game).ResetAudienceGraph();
        foreach (var letter in MultiplayerGame.AnswerLetters)
            await Spectators(game).SetText($"audienceResults{letter}", "\u00a0");

        await Host(game).Show("audienceSetupPanel");
        await Host(game).Disable("audienceDismissBtn");
        await Host(game).Enable("audienceStartBtn");
    }

    #endregion
}