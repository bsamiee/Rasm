using Rhino;
using Rhino.Render;

namespace Rasm.Rhino.Render.Queue;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    QueueActive = 1,
    FrameLost,
    FrameOutsideSequence,
    ExcludedRenderSet,
    ProgressTextRefused,
    ProgressUnreadable,
    JobUnset,
    JobUnreadable,
    JobEntryRejected,
    NoticeRefused,
    NoticeUnreached,
    QueueEmpty,
    SourceViewMissing,
    OverscanSourceRefused,
    EntryRefused,
    FolderUnwritable,
    SpaceLow,
    LinkBroken,
    RendererMissing,
    CommandPending,
    RenderingsUnsaved,
    UntitledEdits,
    LinkUpdatePrompts,
    CopyTooLarge,
    LinkStale,
    LinkCycle,
    LinkUnitsDiffer,
    TextureMissing,
    FileOutsideCopy,
    RenderOnCpu,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record JobUnset(string Variable) : Expected("{Variable} names no render queue job", (int)Codes.JobUnset);

public sealed record JobUnreadable : Expected {
    public JobUnreadable(string path, Error cause) : base("{Path} holds no readable render queue job", (int)Codes.JobUnreadable, cause) => Path = path;

    public string Path { get; }
}

public sealed record JobEntryRejected : Expected {
    public JobEntryRejected(int index, Error cause) : base("Render queue job entry {Index} holds a rejected value", (int)Codes.JobEntryRejected, cause) => Index = index;

    public int Index { get; }
}

public sealed record QueueEmpty() : Expected("Render queue holds no entry", (int)Codes.QueueEmpty);

public sealed record SourceViewMissing(int Index, RenderSettings.RenderingSources Source) : Expected("Entry {Index} renders {Source}, which names no view the document holds", (int)Codes.SourceViewMissing);

public sealed record OverscanSourceRefused(int Index, RenderSettings.RenderingSources Source) : Expected("Entry {Index} widens {Source}, and overscan widens the active viewport alone", (int)Codes.OverscanSourceRefused);

public sealed record EntryRefused : Expected {
    public EntryRefused(int index, Error cause) : base("Entry {Index} holds a value its render refuses", (int)Codes.EntryRefused, cause) => Index = index;

    public int Index { get; }
}

public sealed record FolderUnwritable(string Folder) : Expected("{Folder} cannot be created or written", (int)Codes.FolderUnwritable);

public sealed record SpaceLow(string Folder, long Free) : Expected("{Folder} holds {Free} bytes free", (int)Codes.SpaceLow);

public sealed record LinkBroken : Expected {
    public LinkBroken(string definition, string file, Error cause) : base("{Definition} links {File}, which does not read", (int)Codes.LinkBroken, cause) => (Definition, File) = (definition, file);

    public string Definition { get; }

    public string File { get; }
}

public sealed record RendererMissing(Guid Renderer) : Expected("Current renderer {Renderer} is not installed", (int)Codes.RendererMissing);

public sealed record CommandPending(string Prompt) : Expected("A command waits at {Prompt}", (int)Codes.CommandPending);

public sealed record RenderingsUnsaved() : Expected("Rhino saves no finished rendering, so no queue frame lands", (int)Codes.RenderingsUnsaved);

public sealed record UntitledEdits(uint Document) : Expected("Document {Document} holds edits and no file", (int)Codes.UntitledEdits);

public sealed record LinkUpdatePrompts() : Expected("Linked block updates prompt, which stops an unattended render", (int)Codes.LinkUpdatePrompts);

public sealed record CopyTooLarge(string Folder, long Copy, long Free) : Expected("Copy of {Copy} bytes exceeds the {Free} bytes free at {Folder}", (int)Codes.CopyTooLarge);

public sealed record LinkStale(string Definition, string File) : Expected("{Definition} holds an outdated load of {File}", (int)Codes.LinkStale);

public sealed record LinkCycle(Seq<string> Files) : Expected("Linked files {Files} link each other", (int)Codes.LinkCycle);

public sealed record LinkUnitsDiffer(string Definition, string File, UnitSystem Units, UnitSystem FileUnits) : Expected("{Definition} in {Units} links {File} in {FileUnits}", (int)Codes.LinkUnitsDiffer);

public sealed record TextureMissing(string Path) : Expected("{Path} is not found", (int)Codes.TextureMissing);

public sealed record FileOutsideCopy(string Path) : Expected("{Path} stays outside the copy", (int)Codes.FileOutsideCopy);

public sealed record RenderOnCpu() : Expected("Rhino Render runs on the CPU in safe mode", (int)Codes.RenderOnCpu);
