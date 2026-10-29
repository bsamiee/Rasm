using System.Collections.Generic;
using System.Drawing;
using Rasm.Rhino.Render.Content;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Render;
using Rhino.UI;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderWork(IO<Unit> Render) {
    public Option<Func<Guid, IO<bool>>> Ready { get; init; }
    public Option<Func<bool, IO<Unit>>> Pause { get; init; }
}

public sealed record RenderEngine(
    LocalizeStringPair Caption,
    RenderWindow.StandardChannels Channels,
    Func<RenderStart, Option<Size>, RenderWindow, DefinedChangeFeed, IO<RenderWork>> Open) {
    // --- [ROWS]
    public bool HostWireframe { get; init; }
    public HashMap<Guid, string> CustomChannels { get; init; }
    public Option<RenderWindow.StandardChannels> InitialChannel { get; init; }
    public Seq<SerializerRow> Serializers { get; init; }
    public Seq<NamedValue> DecalDefaults { get; init; }
    public Option<Func<Seq<NamedValue>, IO<Seq<NamedValue>>>> EditDecal { get; init; }
    public Option<Func<RhinoDoc, Material, IO<Material>>> AssignMaterial { get; init; }
    public Option<Func<RhinoDoc, Material, IO<Material>>> EditMaterial { get; init; }
    public Option<Func<RhinoDoc, Material, IO<Material>>> CreateMaterial { get; init; }
    public Option<Func<bool, IO<Unit>>> Current { get; init; }
    public Func<Seq<FileType>, Seq<FileType>> OutputTypes { get; init; } = identity;
    public Func<RenderContent, bool> Choosable { get; init; } = static _ => true;

    // --- [CHANNELS]
    internal Seq<Guid> SupportedChannels =>
        toSeq(Enum.GetValues<RenderWindow.StandardChannels>())
            .Filter(channel => int.IsPow2((int)channel)
                && (Channels & ~(RenderWindow.StandardChannels.RGB | RenderWindow.StandardChannels.NormalXYZ | RenderWindow.StandardChannels.AlbedoRGB)).HasFlag(channel))
            .Choose(static channel => Conversions.Present(RenderWindow.ChannelId(channel)))
            .Concat(toSeq(CustomChannels.Keys));

    internal string CustomChannelName(Guid id) => Conversions.Unset(CustomChannels.Find(id).Map(static english => LocalText.Of(english).Local));

    internal Option<Guid> InitialChannelId => InitialChannel.Map(RenderWindow.ChannelId);

    // --- [CONTENT]
    internal IEnumerable<RenderContentSerializer> RenderContentSerializers() => Serializers.Map(static row => (RenderContentSerializer)new DefinedContentSerializer(row));

    internal List<FileType> SupportedOutputTypes(List<FileType> host) => [.. OutputTypes(toSeq(host))];

    // --- [DECALS]
    internal void InitializeDecalProperties(ref List<NamedValue> properties) => properties.AddRange(DecalDefaults);

    internal bool ShowDecalProperties(ref List<NamedValue> properties) {
        List<NamedValue> current = properties;
        return Session.Run(
            EditDecal.Map(edit => edit(toSeq(current)).Map(edited => {
                current.Clear();
                current.AddRange(edited);
                return true;
            })).IfNone(IO.pure(value: false)),
            failed: false);
    }

    // --- [MATERIALS]
    internal static bool Pressed(Option<Func<RhinoDoc, Material, IO<Material>>> button, RhinoDoc doc, ref Material material) {
        Material current = material;
        Option<Material> pressed = Session.Run(button.Map(press => press(doc, current).Map(static held => Some(held))).IfNone(IO.pure(Option<Material>.None)), Option<Material>.None);
        material = pressed.IfNone(current);
        return pressed.IsSome;
    }

    // --- [CURRENT]
    internal void OnSetCurrent(bool current) => _ = Session.Run(Current.Map(set => set(current)).IfNone(IO.pure(unit)));
}
