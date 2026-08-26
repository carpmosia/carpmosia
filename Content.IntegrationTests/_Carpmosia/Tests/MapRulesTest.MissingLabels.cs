using System.Collections.Generic;
using Content.Server.Atmos.Monitor.Components;
using Content.Server.DeviceLinking.Components;
using Content.Server.Power.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{

    private List<string> TestMissingLabels(YamlSequenceNode entities)
    {
        List<EntProtoId> targets = [
            ..GetPrototypeIds<PowerNetworkBatteryComponent>(),
            ..GetPrototypeIds<AirAlarmComponent>(),
            ..GetPrototypeIds<SignalSwitchComponent>()
        ];

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            EntProtoId protoId = proto[Proto].AsString();

            // Skip unrelated entities
            if (!targets.Contains(protoId))
                continue;

            foreach (var ent in (YamlSequenceNode)proto[Entities])
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                if (GetCompNode(ent, "Label") is { } label && (label.HasNode("currentLabel") || label.HasNode("localizedLabel")))
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that is missing a label at {trans.Item2}");
            }
        }

        return errors;
    }

}
