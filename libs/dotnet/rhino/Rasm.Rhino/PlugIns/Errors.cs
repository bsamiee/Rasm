using Rhino.Commands;

namespace Rasm.Rhino.PlugIns;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    CommandRefused = 1,
    UnmatchedFormat = 2,
    NotInstalled = 3,
    LoadRefused = 4,
    NotRhinoPlugIn = 5,
    LoadDisabled = 6,
    LoadFailed = 7,
    RestartRequired = 8,
    NotHeld = 9,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record CommandRefused(string EnglishName, Guid Id) : Expected("RegisterCommand refused {EnglishName} {Id}", (int)Codes.CommandRefused) {
    public static Fin<Unit> Unless(bool registered, Command command) => registered ? unit : new CommandRefused(command.EnglishName, command.Id);
}

public sealed record UnmatchedFormat(string Description, string Extension) : Expected("No file format row matches {Description} {Extension}", (int)Codes.UnmatchedFormat);

public sealed record NotInstalled(Guid Id) : Expected("No installed plug-in holds id {Id}", (int)Codes.NotInstalled);

public sealed record LoadRefused(string Path) : Expected("Rhino refused to load {Path}", (int)Codes.LoadRefused);

public sealed record NotRhinoPlugIn(string Path) : Expected("{Path} holds no Rhino plug-in", (int)Codes.NotRhinoPlugIn);

public sealed record LoadDisabled(Guid Id) : Expected("Plug-in {Id} is disabled", (int)Codes.LoadDisabled);

public sealed record LoadFailed(Guid Id) : Expected("Plug-in {Id} failed to load", (int)Codes.LoadFailed);

public sealed record RestartRequired(Guid Id) : Expected("Plug-in {Id} is loaded, so its load protection stays unchanged", (int)Codes.RestartRequired);

public sealed record NotHeld(Type Service) : Expected("This plug-in holds no {Service}", (int)Codes.NotHeld);
