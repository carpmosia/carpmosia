using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Research;

[Prototype]
public sealed partial class ResearchTreeUnlockFlagPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
    [DataField(required: true)]
    public LocId Text = default!;
    [DataField]
    public IResearchTreeUnlockFlagData? Data = null;
}

public interface IResearchTreeUnlockFlagData;

[Serializable]
[DataDefinition]
public sealed partial class ResearchTreeUnlockFlagQueueSizeBonus : IResearchTreeUnlockFlagData
{
    [DataField]
    public int Bonus = 0;
}

[Serializable]
[DataDefinition]
public sealed partial class ResearchTreeUnlockFlagBankSizeBonus : IResearchTreeUnlockFlagData
{
    [DataField]
    public int Bonus = 0;
}