using Robust.Shared.Serialization;
using Content.Shared.Chemistry.Components;

namespace Content.Shared.MedicalScannerPlus;

/// <summary>
/// On interacting with an entity retrieves the entity UID for use with getting the current damage of the mob.
/// </summary>
[Serializable, NetSerializable]
public sealed class HealthAnalyzerPlusScannedUserMessage : BoundUserInterfaceMessage
{
    public HealthAnalyzerPlusUiState State;

    public HealthAnalyzerPlusScannedUserMessage(HealthAnalyzerPlusUiState state)
    {
        State = state;
    }
}

/// <summary>
/// Contains the current state of a health analyzer control. Used for the health analyzer and cryo pod.
/// </summary>
[Serializable, NetSerializable]
public struct HealthAnalyzerPlusUiState
{
    public readonly NetEntity? TargetEntity;
    public float Temperature;
    public float BloodLevel;
    public bool? ScanMode;
    public bool? Bleeding;
    public bool? Unrevivable;
    public readonly Solution? BloodType;
    public readonly Solution? BloodSolution;

    public HealthAnalyzerPlusUiState() {}

    public HealthAnalyzerPlusUiState(NetEntity? targetEntity, float temperature, float bloodLevel, bool? scanMode, bool? bleeding, bool? unrevivable, Solution? bloodType, Solution? bloodSolution)
    {
        TargetEntity = targetEntity;
        Temperature = temperature;
        BloodLevel = bloodLevel;
        ScanMode = scanMode;
        Bleeding = bleeding;
        Unrevivable = unrevivable;
        BloodType = bloodType;
        BloodSolution = bloodSolution;
    }
}
