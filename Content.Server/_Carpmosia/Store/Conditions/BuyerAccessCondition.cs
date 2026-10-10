using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Emag.Systems;
using Content.Shared.Mind;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
//using Robust.Shared.Utility;

namespace Content.Server.Store.Conditions;

// TODO: Have emagging an AccessReader actually leave an EmagComponent so this can properly check that.

/// <summary>
/// Allows a store entry to be filtered out based on the user's access.
/// Supports entities without an <see cref="AccessReaderComponent"/> if this has an access set.
/// </summary>
public sealed partial class BuyerAccessCondition : ListingCondition
{
    private AccessReaderSystem? _accessReader;
    private EmagSystem? _emag;

    /// <summary>
    /// The access needed for this condition to pass. If null, uses the <see cref="AccessReaderComponent"/> if it's there, otherwise defaults to true.
    /// </summary>
    [DataField]
    public ProtoId<AccessLevelPrototype>? Access;

    ///// <summary>
    ///// Wether or not this should break when access broken. Needs an access set to work. If left null, defaults first to the value on <see cref="AccessReaderComponent"/>, then to true.
    ///// </summary>
    //[DataField]
    //public bool? BreakOnAccessBreaker;

    public override bool Condition(ListingConditionArgs args)
    {
        if (args.StoreEntity == null)
            return true;

        var ent = args.EntityManager;

        if (!ent.TryGetComponent<MindComponent>(args.Buyer, out var mind) || mind.CurrentEntity is null) // Buyer either as no mind or no attached entity, handle elsewhere.
            return true;


        _accessReader ??= ent.System<AccessReaderSystem>();
        _emag ??= ent.System<EmagSystem>(); // Has a public Sawmill so I figured I'd keep it for now.

        var buyer = mind.CurrentEntity.Value;
        _accessReader.GetMainAccessReader(args.StoreEntity.Value, out var accessReader);

        //DebugTools.Assert(BreakOnAccessBreaker == null || Access != null, "BuyerAccessCondtion set BreakOnAccessBreaker but not Access.");

        if (Access != null)
        {
            //var checkEmag = BreakOnAccessBreaker ?? accessReader == null || accessReader.Value.Comp.BreakOnAccessBreaker;

            //return checkEmag && _emag.CheckFlag(args.StoreEntity.Value, EmagType.Access)
            //       || _accessReader.FindAccessTags(buyer).Contains(Access.Value);

            return _accessReader.FindAccessTags(buyer).Contains(Access.Value);
        }

        if (accessReader != null)
        {
            return _accessReader.IsAllowed(buyer, accessReader.Value, accessReader);
        }

        _emag.Log.Error("BuyerAccessCondition couldn't find an access to check against.");
        return true;
    }
}
