using System.Linq;
using Content.Server.Administration;
using Content.Server.Maps;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Maps;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Commands
{
    [AdminCommand(AdminFlags.Round)]
    public sealed partial class ForceMapCommand : LocalizedCommands
    {
        [Dependency] private IConfigurationManager _configurationManager = default!;
        [Dependency] private IGameMapManager _gameMapManager = default!;
        [Dependency] private IPrototypeManager _prototypeManager = default!;

        private const int MaxArgCount = 4; // Carpmosia-edit - Multistation

        public override string Command => "forcemap";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length < 1 || args.Length > MaxArgCount) // Carpmosia-start - Multistation
            {
                shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 1), ("upper", MaxArgCount))); // Carpmosia-start - Multistation
                return;
            }

            // Carpmosia-start - Multistation
            if (string.IsNullOrEmpty(args[0]))
            {
                _configurationManager.SetCVar(CCVars.GameMap, string.Empty);
                shell.WriteLine(Loc.GetString("cmd-forcemap-cleared"));
                return;
            }

            var maps = new List<string>();

            for (var i = 0; i < args.Length; i++)
            {
                var name = args[i];
                if (!_gameMapManager.CheckMapExists(name))
                {
                    shell.WriteLine(Loc.GetString("cmd-forcemap-map-not-found", ("map", name)));
                    return;
                }
                maps.Add(name);
            }

            _configurationManager.SetCVar(CCVars.GameMap, string.Join(";", maps));
            shell.WriteLine(Loc.GetString("cmd-forcemap-success", ("map", string.Join(" & ", maps))));
            // Carpmosia-end - Multistation
        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            // Carpmosia-start - Multistation
            if (args.Length == 0)
                return CompletionResult.Empty;

            if (args.Length > MaxArgCount)
                return CompletionResult.Empty;

            var options = _prototypeManager
                .EnumeratePrototypes<GameMapPrototype>()
                .Where(p => !p.ID.StartsWith("Legacy"))
                .Where(p => !p.ID.StartsWith("Terminal"))
                .Select(p => new CompletionOption(p.ID, p.MapName))
                .OrderBy(p => p.Value);

            return CompletionResult.FromHintOptions(options, Loc.GetString($"cmd-forcemap-hint"));
            // Carpmosia-end - Multistation
        }
    }
}
