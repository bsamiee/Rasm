using Rhino.Commands;
using Rhino.PlugIns;

namespace Rasm.Rhino.Plugin;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int CommandRefused = 2100;

    public const int LoadRefused = 2101;

    public const int Unloaded = 2102;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record CommandRefused(string EnglishName, Guid Id) : Expected("RegisterCommand refused {EnglishName} {Id}", Codes.CommandRefused) {
    public static Fin<Unit> Unless(bool registered, Command command) => registered ? unit : new CommandRefused(command.EnglishName, command.Id);
}

public sealed record LoadRefused(string Path, LoadPlugInResult Result) : Expected("LoadPlugIn returned {Result} for {Path}", Codes.LoadRefused);

public sealed record Unloaded(Guid Id) : Expected("Plug-in {Id} is not loaded", Codes.Unloaded) {
    public static Fin<Unit> Unless(bool loaded, Guid id) => loaded ? unit : new Unloaded(id);
}
