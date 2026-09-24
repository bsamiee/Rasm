using Rasm.Rhino.Document;
using Rhino.Input;

namespace Rasm.Rhino.Commands;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnexpectedGetResult(GetResult Result) : Expected("Getter returned the unhandled GetResult {Result}", ErrorOps.Code<UnexpectedGetResult>());

public sealed record OptionNotAdded(string EnglishName) : Expected("Option {EnglishName} was not added", ErrorOps.Code<OptionNotAdded>());

public sealed record InvalidOptionName(string Name) : Expected("{Name} is not a valid option name", ErrorOps.Code<InvalidOptionName>()) {
    public static Fin<Unit> Unless(bool valid, string name) => valid ? unit : new InvalidOptionName(name);
}
