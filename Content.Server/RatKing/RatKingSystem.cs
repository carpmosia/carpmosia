using System.Numerics;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Popups;
using Content.Shared.Atmos;
using Content.Shared.Chat;
using Content.Shared.Damage.Components; // Carpmosia-edit - Remove and replace Domain
using Content.Shared.Damage.Systems; // Carpmosia-edit - Remove and replace Domain
using Content.Shared.Dataset;
using Content.Shared.FixedPoint; // Carpmosia-edit - Remove and replace Domain
using Content.Shared.Gibbing; // Carpmosia-edit - Remove and replace Domain
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Pointing;
using Content.Shared.Popups; // Carpmosia-edit - Remove and replace Domain
using Content.Shared.Random.Helpers;
using Content.Shared.RatKing;
using Robust.Shared.Map;
using Robust.Shared.Random; // Carpmosia-edit - Remove and replace Domain

namespace Content.Server.RatKing
{
    /// <inheritdoc/>
    public sealed partial class RatKingSystem : SharedRatKingSystem
    {
        [Dependency] private AtmosphereSystem _atmos = default!;
        [Dependency] private ChatSystem _chat = default!;
        // Carpmosia-start - Remove and replace Domain
        [Dependency] private DamageableSystem _damageableSystem = default!;
        [Dependency] private GibbingSystem _gibbingSystem = default!;
        // Carpmosia-end - Remove and replace Domain
        [Dependency] private HTNSystem _htn = default!;
        [Dependency] private HungerSystem _hunger = default!;
        [Dependency] private IRobustRandom _random = default!; // Carpmosia-edit - Remove and replace Domain
        [Dependency] private NPCSystem _npc = default!;
        [Dependency] private PopupSystem _popup = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RatKingComponent, RatKingRaiseArmyActionEvent>(OnRaiseArmy);
            SubscribeLocalEvent<RatKingComponent, RatKingDomainActionEvent>(OnDomain);
            SubscribeLocalEvent<RatKingComponent, AfterPointedAtEvent>(OnPointedAt);
            SubscribeLocalEvent<RatKingComponent, RatKingSacrificeActionEvent>(OnSacrifice); // Carpmosia-edit - Remove and replace Domain
        }

        /// <summary>
        /// Summons an allied rat servant at the King, costing a small amount of hunger
        /// </summary>
        private void OnRaiseArmy(EntityUid uid, RatKingComponent component, RatKingRaiseArmyActionEvent args)
        {
            if (args.Handled)
                return;

            if (!TryComp<HungerComponent>(uid, out var hunger))
                return;

            //make sure the hunger doesn't go into the negatives
            if (_hunger.GetHunger(hunger) < component.HungerPerArmyUse)
            {
                _popup.PopupEntity(Loc.GetString("rat-king-too-hungry"), uid, uid);
                return;
            }
            args.Handled = true;
            _hunger.ModifyHunger(uid, -component.HungerPerArmyUse, hunger);
            var servant = Spawn(component.ArmyMobSpawnId, Transform(uid).Coordinates);
            var comp = EnsureComp<RatKingServantComponent>(servant);
            comp.King = uid;
            Dirty(servant, comp);

            component.Servants.Add(servant);
            _npc.SetBlackboard(servant, NPCBlackboard.FollowTarget, new EntityCoordinates(uid, Vector2.Zero));
            UpdateServantNpc(servant, component.CurrentOrder);
        }

        /// <summary>
        /// uses hunger to release a specific amount of ammonia into the air. This heals the rat king
        /// and his servants through a specific metabolism.
        /// </summary>
        private void OnDomain(EntityUid uid, RatKingComponent component, RatKingDomainActionEvent args)
        {
            if (args.Handled)
                return;

            if (!TryComp<HungerComponent>(uid, out var hunger))
                return;

            //make sure the hunger doesn't go into the negatives
            if (_hunger.GetHunger(hunger) < component.HungerPerDomainUse)
            {
                _popup.PopupEntity(Loc.GetString("rat-king-too-hungry"), uid, uid);
                return;
            }
            args.Handled = true;
            _hunger.ModifyHunger(uid, -component.HungerPerDomainUse, hunger);

            _popup.PopupEntity(Loc.GetString("rat-king-domain-popup"), uid);
            var tileMix = _atmos.GetTileMixture(uid, excite: true);
            tileMix?.AdjustMoles(Gas.Ammonia, component.MolesAmmoniaPerDomain);
        }

        private void OnPointedAt(EntityUid uid, RatKingComponent component, ref AfterPointedAtEvent args)
        {
            if (component.CurrentOrder != RatKingOrderType.CheeseEm)
                return;

            foreach (var servant in component.Servants)
            {
                _npc.SetBlackboard(servant, NPCBlackboard.CurrentOrderedTarget, args.Pointed);
            }
        }

        // Carpmosia-start - Remove and replace Domain
        private void OnSacrifice(Entity<RatKingComponent> ent, ref RatKingSacrificeActionEvent args)
        {
            if (args.Handled)
                return;

            if (ent.Comp.Servants.Count <= 0)
            {
                _popup.PopupEntity(Loc.GetString("rat-king-sacrifice-fail-servants"), ent, ent);
                return;
            }

            _gibbingSystem.Gib(_random.Pick(ent.Comp.Servants), false);
            _damageableSystem.HealDistributed(ent.Owner, ent.Comp.SacrificeHeal);

            _popup.PopupEntity(Loc.GetString("rat-king-sacrifice-succeeds"), ent, ent, PopupType.Large);

            args.Handled = true;
        }
        // Carpmosia-end - Remove and replace Domain

        public override void UpdateServantNpc(EntityUid uid, RatKingOrderType orderType)
        {
            base.UpdateServantNpc(uid, orderType);

            if (!TryComp<HTNComponent>(uid, out var htn))
                return;

            if (htn.Plan != null)
                _htn.ShutdownPlan(htn);

            _npc.SetBlackboard(uid, NPCBlackboard.CurrentOrders, orderType);
            _htn.Replan(htn);
        }

        public override void DoCommandCallout(EntityUid uid, RatKingComponent component)
        {
            base.DoCommandCallout(uid, component);

            if (!component.OrderCallouts.TryGetValue(component.CurrentOrder, out var datasetId) ||
                !ProtoMan.TryIndex<LocalizedDatasetPrototype>(datasetId, out var datasetPrototype))
                return;

            var msg = Random.Pick(datasetPrototype);
            _chat.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, true);
        }
    }
}
