using Rasm.Rhino.Document;
using Rhino.DocObjects;

namespace Rasm.Rhino.Blocks;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record DefinitionMissing(ComponentRef Key) : Expected("Definition {Key} is not in the instance definition table", ErrorOps.Code<DefinitionMissing>());

public sealed record MemberInvalid(ComponentRef Key, Guid MemberId, string Log) : Expected("Definition {Key} member {MemberId} is invalid: {Log}", ErrorOps.Code<MemberInvalid>());

public sealed record WrongUpdateType(ComponentRef Key, InstanceDefinitionUpdateType Actual, Seq<InstanceDefinitionUpdateType> Accepted) : Expected("Definition {Key} has update type {Actual} where {Accepted} is required", ErrorOps.Code<WrongUpdateType>()) {
    public static Fin<Unit> Unless(ComponentRef key, InstanceDefinitionUpdateType actual, Seq<InstanceDefinitionUpdateType> accepted) =>
        accepted.Exists(type => type == actual) ? unit : new WrongUpdateType(key, actual, accepted);
}

public sealed record TenuousDefinition(ComponentRef Key) : Expected("Definition {Key} is tenuous", ErrorOps.Code<TenuousDefinition>());

public sealed record InvalidLayerStyle(ComponentRef Key, InstanceDefinitionLayerStyle Style) : Expected("Definition {Key} does not accept layer style {Style}", ErrorOps.Code<InvalidLayerStyle>());
