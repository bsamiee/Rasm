namespace Rasm.Rhino.Plugin.Hosting;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    NotInstalled = 1,
    LoadRefused = 2,
    NotRhinoPlugIn = 3,
    LoadDisabled = 4,
    LoadFailed = 5,
    RestartRequired = 6,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record NotInstalled(Guid Id) : Expected("No installed plug-in holds id {Id}", (int)Codes.NotInstalled);

public sealed record LoadRefused(string Path) : Expected("Rhino refused to load {Path}", (int)Codes.LoadRefused);

public sealed record NotRhinoPlugIn(string Path) : Expected("{Path} holds no Rhino plug-in", (int)Codes.NotRhinoPlugIn);

public sealed record LoadDisabled(Guid Id) : Expected("Plug-in {Id} is disabled", (int)Codes.LoadDisabled);

public sealed record LoadFailed(Guid Id) : Expected("Plug-in {Id} failed to load", (int)Codes.LoadFailed);

public sealed record RestartRequired(Guid Id) : Expected("Plug-in {Id} is loaded, so its load protection stays unchanged", (int)Codes.RestartRequired);
