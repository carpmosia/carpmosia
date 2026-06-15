using System.Linq;
using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Discord.WebhookMessages;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.Commands;

[AdminCommand(AdminFlags.Round)]
public sealed partial class ProtovoteCommand : LocalizedEntityCommands
{
    [Dependency] private IVoteManager _voteManager = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private VoteWebhooks _voteWebhooks = default!;
    [Dependency] private IConfigurationManager _cfg = default!;

    private const int MaxArgCount = 10;

    public override string Command => "protovote";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 3 || args.Length > MaxArgCount)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 3), ("upper", 10)));
            return;
        }

        var title = args[0];

        var options = new VoteOptions
        {
            Title = title,
            Duration = TimeSpan.FromSeconds(30),
        };

        for (var i = 1; i < args.Length; i++)
        {
            EntProtoId arg = args[i];
            if (!_prototype.TryIndex<EntityPrototype>(arg, out var proto))
            {
                shell.WriteError(Loc.GetString("cmd-tippy-error-no-prototype", ("proto", arg)));
                return;
            }
            options.Options.Add(((proto.Name, (string?)null, (EntProtoId?)arg), i));
        }

        options.SetInitiatorOrServer(shell.Player);

        if (shell.Player != null)
            _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"{shell.Player} initiated a custom vote: {options.Title} - {string.Join("; ", options.Options.Select(x => x.text))}");
        else
            _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Initiated a custom vote: {options.Title} - {string.Join("; ", options.Options.Select(x => x.text))}");

        var vote = _voteManager.CreateVote(options);

        var webhookState = _voteWebhooks.CreateWebhookIfConfigured(options, _cfg.GetCVar(CCVars.DiscordVoteWebhook));

        vote.OnFinished += (_, eventArgs) =>
        {
            if (eventArgs.Winner == null)
            {
                var ties = string.Join(", ", eventArgs.Winners.Select(c => args[(int) c]));
                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Custom vote {options.Title} finished as tie: {ties}");
                _chatManager.DispatchServerAnnouncement(Loc.GetString("cmd-customvote-on-finished-tie", ("title", options.Title), ("ties", ties)));
            }
            else
            {
                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Custom vote {options.Title} finished: {args[(int) eventArgs.Winner]}");
                _chatManager.DispatchServerAnnouncement(Loc.GetString("cmd-customvote-on-finished-win", ("title", options.Title), ("winner", args[(int) eventArgs.Winner])));
            }

            _voteWebhooks.UpdateWebhookIfConfigured(webhookState, eventArgs);
        };

        vote.OnCancelled += _ =>
        {
            _voteWebhooks.UpdateCancelledWebhookIfConfigured(webhookState);
        };
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHint(Loc.GetString("cmd-customvote-arg-title"));

        if (args.Length > MaxArgCount)
            return CompletionResult.Empty;

        var n = args.Length - 1;
        return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIdsLimited<EntityPrototype>(args[n], _prototype),
                Loc.GetString("cmd-customvote-arg-option-n", ("n", n)));
    }
}
