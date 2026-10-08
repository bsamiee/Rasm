namespace Rasm.Rhino.Viewport;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    CameraLocked = 1,
    MotionEnded,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record CameraLocked() : Expected("Viewport camera is locked", (int)Codes.CameraLocked);

public sealed record MotionEnded() : Expected("Motion has ended", (int)Codes.MotionEnded);
