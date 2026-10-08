using Rasm.Imaging.Output;

namespace Rasm.Rhino.Viewport.Publishing;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    HeadlessCapture = 1,
    PublishRefused,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record HeadlessCapture() : Expected("A headless document captures no page", (int)Codes.HeadlessCapture) {
    public static Fin<Unit> Unless(bool windowed) => windowed ? unit : new HeadlessCapture();
}

public sealed record PublishRefused : Expected {
    public PublishRefused(OutputPath path, Error cause) : base("Writing {Path} failed", (int)Codes.PublishRefused, cause) => Path = path;

    public OutputPath Path { get; }
}
