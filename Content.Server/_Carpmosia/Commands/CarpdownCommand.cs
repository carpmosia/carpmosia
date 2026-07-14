using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server;
using Robust.Shared.Console;

namespace Content.Server.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed partial class CarpdownCommand : LocalizedCommands
{
    [Dependency] private IBaseServer _server = default!;

    public override string Command => "carpdown";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var reason = argStr.Replace("carpdown", "").Trim();
        _server.Shutdown(string.IsNullOrEmpty(reason) ? "Server restart" : reason);
    }
}
