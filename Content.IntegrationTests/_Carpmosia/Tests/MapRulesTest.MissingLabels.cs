using System.Collections.Generic;
using Content.Server.Atmos.Monitor.Components;
using Content.Server.DeviceLinking.Components;
using Content.Server.Power.Components;
using Content.Shared.Labels.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    /// <summary>
    /// Ensures that all power network related, air alarm, and switch entities are labelled
    /// </summary>
    private List<string> TestMissingLabels(ParsedRoot root)
    {
        List<EntProtoId> targets = [
            ..GetPrototypeIds<PowerNetworkBatteryComponent>(),
            ..GetPrototypeIds<AirAlarmComponent>(),
            ..GetPrototypeIds<FireAlarmComponent>(),
            ..GetPrototypeIds<SignalSwitchComponent>()
        ];

        var errors = new List<string>();

        foreach (var (protoId, entities) in root.Entities)
        {
            // Skip unrelated entities
            if (!targets.Contains(protoId))
                continue;

            foreach (var (uid, ent) in entities)
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                if (GetCompNode<LabelComponent>(ent) is { } label && (label.HasNode("currentLabel") || label.HasNode("localizedLabel")))
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({uid}) that is missing a label at {trans.Item2}");
            }
        }

        return errors;
    }

}
