using Rhino.Commands;

namespace Rasm.Rhino.Plugin;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    CommandRefused = 1,
    UnmatchedFormat,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record CommandRefused(string EnglishName, Guid Id) : Expected("RegisterCommand refused {EnglishName} {Id}", (int)Codes.CommandRefused) {
    public static Fin<Unit> Unless(bool registered, Command command) => registered ? unit : new CommandRefused(command.EnglishName, command.Id);
}

public sealed record UnmatchedFormat(string Description, string Extension) : Expected("No file format row matches {Description} {Extension}", (int)Codes.UnmatchedFormat);
