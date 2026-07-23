using System.Linq;
using Content.Server.Administration;
using Content.Server.Maps;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Maps;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.Commands;

[AdminCommand(AdminFlags.Round)]
public sealed partial class ForceMapsCommand : LocalizedCommands
{
    [Dependency] private IConfigurationManager _configurationManager = default!;
    [Dependency] private IGameMapManager _gameMapManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    private const int MaxArgCount = 10;

    public override string Command => "forcemaps";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2 || args.Length > MaxArgCount)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 2), ("upper", 10)));
            return;
        }

        var maps = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!_gameMapManager.CheckMapExists(arg))
            {
                shell.WriteError(Loc.GetString("cmd-protovote-error-no-prototype", ("proto", arg)));
                return;
            }
            maps.Add(arg);
        }

        _configurationManager.SetCVar(CCVars.GameMap, string.Join(";", maps));

        shell.WriteLine(Loc.GetString("cmd-forcemap-success", ("map", string.Join(" & ", maps))));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 0)
            return CompletionResult.Empty;

        if (args.Length > MaxArgCount)
            return CompletionResult.Empty;

        var n = args.Length - 1;
        var options = _prototypeManager
            .EnumeratePrototypes<GameMapPrototype>()
            .Where(p => !p.ID.StartsWith("Legacy"))
            .Where(p => !p.ID.StartsWith("Terminal"))
            .Select(p => new CompletionOption(p.ID, p.MapName))
            .OrderBy(p => p.Value);
        return CompletionResult.FromHintOptions(options, Loc.GetString($"cmd-forcemap-hint"));
    }
}
