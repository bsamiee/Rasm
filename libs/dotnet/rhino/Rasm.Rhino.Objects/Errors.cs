using Rasm.Rhino.Document;

namespace Rasm.Rhino.Objects;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record WrongLightStyle(LightStyle Style, string Member) : Expected("{Member} does not apply to a {Style} light", ErrorOps.Code<WrongLightStyle>());

public sealed record HistoryValueRefused(int Id, string Member) : Expected("{Member} refused history value {Id}", ErrorOps.Code<HistoryValueRefused>());

public sealed record DuplicateHistoryValueId(int Id) : Expected("History value {Id} is set twice in one record", ErrorOps.Code<DuplicateHistoryValueId>());
