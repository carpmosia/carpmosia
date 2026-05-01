using System.Numerics; // Carpmosia-edit - Lagomorph
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings
{
    [Prototype]
    public sealed partial class MarkingPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = "uwu";

        public string Name { get; private set; } = default!;

        [DataField("bodyPart", required: true)]
        public HumanoidVisualLayers BodyPart { get; private set; } = default!;

        [DataField]
        public List<ProtoId<MarkingsGroupPrototype>>? GroupWhitelist;

        [DataField("sexRestriction")]
        public Sex? SexRestriction { get; private set; }

        [DataField("forcedColoring")]
        public bool ForcedColoring { get; private set; } = false;

        [DataField("coloring")]
        public MarkingColors Coloring { get; private set; } = new();

        /// <summary>
        /// Do we need to apply any displacement maps to this marking? Set to false if your marking is incompatible
        /// with a standard human doll, and is used for some special races with unusual shapes
        /// </summary>
        [DataField]
        public bool CanBeDisplaced { get; private set; } = true;

        [DataField("sprites", required: true)]
        public List<SpriteSpecifier> Sprites { get; private set; } = default!;

        // Carpmosia-start - Lagomorph
        /// <summary>
        /// Sprite offset to apply to the entire marking
        /// </summary>
        /// <remarks>
        /// in YAML offset: "0, 1" shifts it up by 1 full tile
        /// 1/32 or 0.03125 corresponds to one pixel
        /// </remarks>
        [DataField("offset")]
        public Vector2 Offset = Vector2.Zero;
        // Carpmosia-end - Lagomorph

        public Marking AsMarking()
        {
            return new Marking(ID, Sprites.Count);
        }
    }
}
