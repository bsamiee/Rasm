using Rhino.Commands;

namespace Rasm.Rhino.Plugin;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int CommandRefused = 2100;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record CommandRefused(string EnglishName, Guid Id) : Expected("RegisterCommand refused {EnglishName} {Id}", Codes.CommandRefused) {
    public static Fin<Unit> Unless(bool registered, Command command) => registered ? unit : new CommandRefused(command.EnglishName, command.Id);
}
