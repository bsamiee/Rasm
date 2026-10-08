using Rasm.Imaging.Output;

namespace Rasm.Rhino.Document.Files;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    NotModelFile = 1,
    OutsideScriptRunner,
    HeadlessScript,
    UnqualifiedPath,
    FileMissing,
    FolderMissing,
    DocumentUnsaved,
    StepFailed,
    VersionsExhausted,
    DestinationOccupied,
    FolderRefused,
    DocumentReadOnly,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record NotModelFile(string Path) : Expected("Template {Path} is no .3dm file", (int)Codes.NotModelFile);

public sealed record OutsideScriptRunner() : Expected("A script runs to completion inside a script runner command alone", (int)Codes.OutsideScriptRunner) {
    public static Fin<Unit> Unless(bool inside) => inside ? unit : new OutsideScriptRunner();
}

public sealed record HeadlessScript(uint Serial) : Expected("Document {Serial} is headless and feeds a script no input", (int)Codes.HeadlessScript) {
    public static Fin<Unit> Unless(bool windowed, uint serial) => windowed ? unit : new HeadlessScript(serial);
}

public sealed record UnqualifiedPath(string Path) : Expected("{Path} is not a fully qualified path", (int)Codes.UnqualifiedPath) {
    public static Fin<string> Unless(bool qualified, string path) => qualified ? path : new UnqualifiedPath(path);
}

public sealed record FileMissing(string Path) : Expected("{Path} names no file", (int)Codes.FileMissing) {
    public static Fin<string> Unless(bool exists, string path) => exists ? path : new FileMissing(path);
}

public sealed record FolderMissing(string Folder) : Expected("{Folder} names no folder", (int)Codes.FolderMissing) {
    public static Fin<string> Unless(bool exists, string folder) => exists ? folder : new FolderMissing(folder);
}

public sealed record DocumentUnsaved() : Expected("Document has no saved file", (int)Codes.DocumentUnsaved);

public sealed record StepFailed(int Index) : Expected("Step {Index} failed", (int)Codes.StepFailed);

public sealed record VersionsExhausted(NamePart Stem, Seq<NamePart> Scope) : Expected("{Stem} {Scope} holds the highest output version", (int)Codes.VersionsExhausted);

public sealed record DestinationOccupied(OutputPath Path) : Expected("{Path} already exists", (int)Codes.DestinationOccupied) {
    public static Fin<Unit> Unless(bool free, OutputPath path) => free ? unit : new DestinationOccupied(path);
}

public sealed record FolderRefused : Expected {
    public FolderRefused(string folder, Error cause) : base("Listing or creating {Folder} failed", (int)Codes.FolderRefused, cause) => Folder = folder;

    public string Folder { get; }
}

public sealed record DocumentReadOnly(string Path) : Expected("Document opened read-only writes no file over {Path}", (int)Codes.DocumentReadOnly) {
    public static Fin<string> Unless(bool writable, string path) => writable ? path : new DocumentReadOnly(path);
}
