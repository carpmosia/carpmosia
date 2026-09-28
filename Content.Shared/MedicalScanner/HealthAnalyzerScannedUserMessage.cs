using Robust.Shared.Serialization;
using Content.Shared.Chemistry.Components; // Carpmosia-edit - Health analyzer bloodstream reagents

namespace Content.Shared.MedicalScanner;

/// <summary>
/// On interacting with an entity retrieves the entity UID for use with getting the current damage of the mob.
/// </summary>
[Serializable, NetSerializable]
public sealed class HealthAnalyzerScannedUserMessage : BoundUserInterfaceMessage
{
    public HealthAnalyzerUiState State;

    public HealthAnalyzerScannedUserMessage(HealthAnalyzerUiState state)
    {
        State = state;
    }
}

/// <summary>
/// Contains the current state of a health analyzer control. Used for the health analyzer and cryo pod.
/// </summary>
[Serializable, NetSerializable]
public struct HealthAnalyzerUiState
{
    public readonly NetEntity? TargetEntity;
    public float Temperature;
    public float BloodLevel;
    public bool? ScanMode;
    public bool? Bleeding;
    public bool? Unrevivable;
    public readonly Solution? BloodType; // Carpmosia-edit - Health analyzer bloodstream reagents
    public readonly Solution? BloodSolution; // Carpmosia-edit - Health analyzer bloodstream reagents

    public HealthAnalyzerUiState() {}

    public HealthAnalyzerUiState(NetEntity? targetEntity, float temperature, float bloodLevel, bool? scanMode, bool? bleeding, bool? unrevivable, Solution? bloodType, Solution? bloodSolution) // Carpmosia-edit - Health analyzer bloodstream reagents
    {
        TargetEntity = targetEntity;
        Temperature = temperature;
        BloodLevel = bloodLevel;
        ScanMode = scanMode;
        Bleeding = bleeding;
        Unrevivable = unrevivable;
        BloodType = bloodType; // Carpmosia-edit - Health analyzer bloodstream reagents
        BloodSolution = bloodSolution; // Carpmosia-edit - Health analyzer bloodstream reagents
    }
}
