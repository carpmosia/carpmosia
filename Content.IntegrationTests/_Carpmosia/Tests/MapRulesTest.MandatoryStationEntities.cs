#nullable enable
using System.Collections.Generic;
using System.Linq;
using Content.IntegrationTests.Fixtures;
using Robust.Shared.Prototypes;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest : GameTest
{
    // I will find a better way to do this but for now it is what it is
    private static readonly Dictionary<EntProtoId[], EntProtoId[]> MandatoryRules = new()
    {
        {
            [
                "DefaultStationBeaconHOPOffice"
            ],
            [
                "UniformPrinter",
                "MaterialCloth",
                "MaterialDurathread",
                "HolopadCommandHop",
                "DogBed",
                "SpawnMobCorgi",
                "SpawnPointHeadOfPersonnel",
                "WindoorSecureHeadOfPersonnelLocked",
                "ComputerId",
                "LockerHeadOfPersonnelFilled",
                "VendingMachineCart",
                "DresserHeadOfPersonnelFilled",
                "BrbSign",
            ]
        },
        {
            [
                "DefaultStationBeaconVault"
            ],
            [
                "NuclearBomb",
                "WeaponEnergyTurretCommand",
                "HighSecCommandLocked",
                "WeaponEnergyTurretCommandControlPanel",
                "ToolboxGoldFilled",
                "HolopadCommandVault",
            ]
        },
        {
            [
                "DefaultStationBeaconCaptainOffice",
                "DefaultStationBeaconCaptainsQuarters"
            ],
            [
                "ToiletGoldenDirtyWater",
                "CaptainIDCard",
                "AirlockCaptainLocked",
                "DogBed",
                "SpawnMobFoxRenault",
                "HolopadCommandCaptain"
            ]
        },
        {
            [
                "DefaultStationBeaconAIUpload"
            ],
            [
                "HolopadAiUpload",
                "StationAiUploadComputer",
            ]
        }
    };

    private const float Threshold = 10f;

    private List<string> TestMandatoryStationEntities(YamlSequenceNode entities)
    {
        var errors = new List<string>();

        foreach (var test in MandatoryRules)
        {
            var poi = DeserializeCompNodes(entities, test.Key, GetTilePos);
            foreach (var proto in test.Value)
            {
                var eoi = DeserializeCompNodes(entities, [proto], GetTilePos);
                if (poi.Any(pos1 => eoi.Any(pos2 => GetDistance(pos1, pos2) <= Threshold)))
                    continue;

                errors.Add($"Could not find a {proto} near any of {string.Join(", ", test.Key)}]");
            }
        }

        return errors;
    }
}
