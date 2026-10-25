using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Shared.Cargo.Prototypes;

[TypeSerializer]
public sealed class CargoBountyEntryTypeSerializer : ITypeReader<CargoBountyEntry, MappingDataNode>
{

    private static Type? GetEntryType(MappingDataNode node)
    {
        if (node.Has("whitelist"))
            return typeof(CargoBountyItemEntry);

        if (node.Has("reagent"))
            return typeof(CargoBountyReagentEntry);

        return null;
    }

    public CargoBountyEntry Read(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<CargoBountyEntry>? instanceProvider = null)
    {
        var type = GetEntryType(node);

        if (type == typeof(CargoBountyItemEntry))
        {
            return serializationManager.Read<CargoBountyItemEntry>(node, hookCtx, context, notNullableOverride: true);
        }

        if (type == typeof(CargoBountyReagentEntry))
        {
            return serializationManager.Read<CargoBountyReagentEntry>(node, hookCtx, context, notNullableOverride: true);
        }

        throw new ArgumentException("Tried to convert invalid YAML node mapping to CargoBountyEntry!");
    }

    public ValidationNode Validate(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        var type = GetEntryType(node);

        if (type == null)
            return new ErrorNode(node, "No cargo bounty entry type found.");

        return serializationManager.ValidateNode(type, node, context);
    }
}
