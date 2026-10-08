using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Objects.Shading;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes { OcsChannelMismatch = 1 }

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record OcsChannelMismatch(int Channel, TextureMappingType Kind) : Expected("Channel {Channel} cannot hold a {Kind} mapping", (int)Codes.OcsChannelMismatch) {
    public static Fin<Unit> Unless(TextureMappingType kind, int channel) =>
        kind == TextureMappingType.OcsMapping == (channel == ObjectAttributes.OCSMappingChannelId) ? unit : new OcsChannelMismatch(channel, kind);
}
