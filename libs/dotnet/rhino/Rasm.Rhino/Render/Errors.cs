namespace Rasm.Rhino.Render;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UnknownPreset = 1,
    MalformedPreset,
    UnknownRenderSet,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnknownPreset(PresetName Name) : Expected("No preset is named {Name}", (int)Codes.UnknownPreset);

public sealed record MalformedPreset : Expected {
    public MalformedPreset(string path, Error cause) : base("{Path} holds no preset", (int)Codes.MalformedPreset, cause) => Path = path;

    public string Path { get; }
}

public sealed record UnknownRenderSet(SetName Name) : Expected("No render set is named {Name}", (int)Codes.UnknownRenderSet);
