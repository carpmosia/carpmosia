using System.Collections.Generic;
using Content.Server.Atmos.Monitor.Components;
using Content.Shared.Atmos.Components;
using Content.Shared.DeviceNetwork.Components;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    /// <summary>
    /// Checks for any unlinked atmospheric devices except gas pipe sensors
    /// </summary>
    private List<string> TestUnlinkedAtmosDevices(ParsedRoot root)
    {
        var gasPipeSensors = GetPrototypeIds<GasPipeSensorComponent>();
        var airAlarms = GetPrototypeIds<AirAlarmComponent>();
        var atmosMonitors = GetPrototypeIds<AtmosMonitorComponent>();

        var errors = new List<string>();

        foreach (var (protoId, entities) in root.Entities)
        {
            // Gas pipe sensors don't need to be linked
            if (gasPipeSensors.Contains(protoId))
                continue;

            var isAirAlarm = airAlarms.Contains(protoId);
            var isAtmosMonitor = atmosMonitors.Contains(protoId);

            // Skip unrelated entities
            if (!(isAirAlarm || isAtmosMonitor))
                continue;

            foreach (var (uid, ent) in entities)
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                if (isAirAlarm && GetCompNode<DeviceListComponent>(ent) is { } deviceList
                    && deviceList.TryGetNode<YamlSequenceNode>("devices", out var devices) && devices.Children.Count != 0)
                    continue;

                if (isAtmosMonitor && GetCompNode<DeviceNetworkComponent>(ent) is { } deviceNet
                    && deviceNet.TryGetNode<YamlSequenceNode>("deviceLists", out var lists) && lists.Children.Count != 0)
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({uid}) that doesn't have any connections at {trans.Item2}");
            }
        }

        return errors;
    }
}
