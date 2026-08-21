using System.Collections.Generic;
using Content.Server.Atmos.Monitor.Components;
using Content.Shared.Atmos.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    private List<string> TestUnlinkedAtmosDevices(YamlSequenceNode entities)
    {
        var gasPipeSensors = GetPrototypeIds<GasPipeSensorComponent>();
        var airAlarms = GetPrototypeIds<AirAlarmComponent>();
        var atmosMonitors = GetPrototypeIds<AtmosMonitorComponent>();

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            EntProtoId protoId = proto[PROTO].AsString();

            // Gas pipe sensors don't need to be linked
            if (gasPipeSensors.Contains(protoId))
                continue;

            var isAirAlarm = airAlarms.Contains(protoId);
            var isAtmosMonitor = atmosMonitors.Contains(protoId);

            // Skip unrelated entities
            if (!(isAirAlarm || isAtmosMonitor))
                continue;

            foreach (var ent in (YamlSequenceNode)proto[ENTITIES])
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                if (isAirAlarm && GetCompNode(ent, "DeviceList") is { })
                    continue;

                if (isAtmosMonitor && GetCompNode(ent, "DeviceNetwork") is { })
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that doesn't have any connections at {trans.Item2}");
            }
        }

        return errors;
    }
}
