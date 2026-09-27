using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Research;

[Prototype]
public sealed partial class ResearchTreeTierPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
    [DataField(required: true)]
    public string Name = default!;

    [DataField(required: true)]
    public int Position = default!;

    [DataField(required: true)]
    public string Color = default!;

    [DataField]
    public int UnlocksAt = 0;
}