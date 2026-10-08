using Rhino.Input;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UnexpectedGetResult = 1,
    ScriptRaised,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnexpectedGetResult(GetResult Result) : Expected("Get answered {Result}, which no route takes", (int)Codes.UnexpectedGetResult);

public sealed record ScriptRaised : Expected {
    public ScriptRaised(string trace, Error cause) : base("Python raised {Trace}", (int)Codes.ScriptRaised, cause) => Trace = trace;

    public string Trace { get; }
}
