using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Content;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    Unattached = 1,
    ContentFileFailed,
    SerializerContent,
    OcsChannelMismatch,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record Unattached(Guid Content) : Expected("Content {Content} belongs to no document", (int)Codes.Unattached) {
    public static Fin<Unit> Unless(bool attached, Guid content) => attached ? unit : new Unattached(content);
}

public sealed record ContentFileFailed : Expected {
    public ContentFileFailed(string path, Error cause) : base("{Path} failed", (int)Codes.ContentFileFailed, cause) => Path = path;

    public string Path { get; }
}

public sealed record SerializerContent(Guid Content) : Expected("Content {Content} has no managed serializer identity", (int)Codes.SerializerContent);

public sealed record OcsChannelMismatch(int Channel, TextureMappingType Kind) : Expected("Channel {Channel} cannot hold a {Kind} mapping", (int)Codes.OcsChannelMismatch) {
    public static Fin<Unit> Unless(TextureMappingType kind, int channel) =>
        (kind is TextureMappingType.OcsMapping) == (channel == ObjectAttributes.OCSMappingChannelId) ? unit : new OcsChannelMismatch(channel, kind);
}
