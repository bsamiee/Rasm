using System.Buffers.Binary;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class RimagePart {
    public static readonly RimagePart Start = new("[RIMAGE-START]");
    public static readonly RimagePart Body = new("[RIMAGE-BODY]");
    public static readonly RimagePart Chan = new("[RIMAGE-CHAN]");
    public static readonly RimagePart Width = new("width");
    public static readonly RimagePart Height = new("height");
    public static readonly RimagePart Channels = new("channels");
    public static readonly RimagePart Channel = new("channel");
    public static readonly RimagePart Uuid = new("uuid");
    public static readonly RimagePart Offset = new("start");
    public static readonly RimagePart PostEffects = new("post-effects-new");
    public static readonly RimagePart PlugIn = new("plug-in");
    public static readonly RimagePart ToneMapping = new("tone-mapping");
    public static readonly RimagePart Selection = new("selection");
    public static readonly RimagePart Id = new("id");
    public static readonly RimagePart Name = new("name");
    public static readonly RimagePart On = new("on");
    public static readonly RimagePart Shown = new("shown");
    public static readonly RimagePart State = new("state");
}

public sealed record Rimage(string FileName, Size Size, Seq<EffectEntry> Effects, ValueSet State) {
    public Seq<Guid> Running => Effects.Filter(static effect => effect.Runs).Map(static effect => effect.Id);

    public static IO<Rimage> Open(string fileName) =>
        Read(fileName, (_, index) =>
            from size in Measured(index.Xml, fileName)
            from effects in Optional(index.Xml.Element(RimagePart.PostEffects.Key)).ToFin(new FileRefused(fileName, FileReason.Unreadable))
            from entries in Callbacks.Each(
                from stage in toSeq(effects.Elements())
                from entry in toSeq(stage.Elements(RimagePart.PlugIn.Key))
                where entry.Attribute(RimagePart.Name.Key) is not null
                select (Stage: stage, Entry: entry),
                (row, _) => Entry(fileName, effects, row.Stage, row.Entry))
            select new Rimage(fileName, size, entries.Map(static entry => entry.Effect), new ValueSet(toHashMap(entries.Bind(static entry => entry.Fields)))));

    public IO<Seq<RenderWindow.StandardChannels>> Available =>
        Read(FileName, (_, index) => Offsets(index.Xml, FileName).Map(static offsets =>
            toSeq(Enum.GetValues<RenderWindow.StandardChannels>())
                .Filter(channel => channel is not RenderWindow.StandardChannels.None
                    && Components(channel).ForAll(component => offsets.ContainsKey(RenderWindow.ChannelId(component))))
                .Strict()));

    public IO<float[]> Channel(RenderWindow.StandardChannels channel) =>
        Read(FileName, (file, index) => Offsets(index.Xml, FileName).Bind(offsets => Filled(file, index.Body, offsets, channel)));

    private static IO<A> Read<A>(string fileName, Func<FileStream, (XElement Xml, long Body), Fin<A>> body) =>
        DisposalOps.Within(
            () => new FileStream(fileName, new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read, Share = FileShare.Read, Options = FileOptions.RandomAccess }),
            file => IO.lift(() => Indexed(file, fileName).Bind(index => body(file, index))));

    private static Fin<(XElement Xml, long Body)> Indexed(FileStream file, string fileName) {
        const int Version = 2;
        int start = RimagePart.Start.Key.Length;
        Span<byte> lead = stackalloc byte[start + (2 * sizeof(int))];
        int read = file.ReadAtLeast(lead, lead.Length, throwOnEndOfStream: false);
        if (read != lead.Length || !Ascii.Equals(lead[..start], RimagePart.Start.Key) || BinaryPrimitives.ReadInt32LittleEndian(lead[start..]) != Version)
            return new FileRefused(fileName, FileReason.Unreadable);
        int length = BinaryPrimitives.ReadInt32LittleEndian(lead[^sizeof(int)..]);
        if (length < 0 || length % sizeof(char) is not 0 || length > System.Array.MaxLength - sizeof(char) - RimagePart.Body.Key.Length
            || (long)length + sizeof(char) + RimagePart.Body.Key.Length > file.Length - lead.Length)
            return new FileRefused(fileName, FileReason.Unreadable);
        byte[] header = new byte[length + sizeof(char) + RimagePart.Body.Key.Length];
        long body = (long)lead.Length + header.Length;
        return file.ReadAtLeast(header, header.Length, throwOnEndOfStream: false) == header.Length && Ascii.Equals(header.AsSpan(length + sizeof(char)), RimagePart.Body.Key)
            ? Try.lift(() => XElement.Parse(Encoding.Unicode.GetString(header, 0, length))).Run()
                .MapFail(error => error.HasException<XmlException>() ? new FileRefused(fileName, FileReason.Unreadable, error) : error)
                .Map(xml => (Xml: xml, Body: body))
            : new FileRefused(fileName, FileReason.Unreadable);
    }

    private static Fin<Size> Measured(XElement xml, string fileName) =>
        (Child(xml, RimagePart.Width, parseInt).ToFin(new FileRefused(fileName, FileReason.Unreadable)),
         Child(xml, RimagePart.Height, parseInt).ToFin(new FileRefused(fileName, FileReason.Unreadable)))
            .Apply(static (width, height) => new Size(width, height))
            .As()
            .Bind(size => size is { Width: > 0, Height: > 0 } && 4L * size.Width * size.Height <= System.Array.MaxLength
                ? Fin.Succ(size)
                : new FileRefused(fileName, FileReason.Unreadable));

    private static Fin<(EffectEntry Effect, Seq<(EntryKey Key, string Text)> Fields)> Entry(string fileName, XElement effects, XElement stage, XElement entry) =>
        from id in Optional(entry.Attribute(RimagePart.Id.Key)).Bind(static attribute => parseGuid(attribute.Value))
            .ToFin(new FileRefused(fileName, FileReason.Unreadable))
        from effect in string.Equals(stage.Name.LocalName, RimagePart.ToneMapping.Key, StringComparison.Ordinal)
            ? (Optional(stage.Attribute(RimagePart.Selection.Key)).Map(static attribute => attribute.Value)
               | Optional(effects.Element($"{stage.Name.LocalName}-{RimagePart.Selection.Key}")).Map(static element => element.Value))
                .Traverse(text => parseGuid(text).ToFin(new FileRefused(fileName, FileReason.Unreadable))).As()
                .Map(selected => (EffectEntry)new EffectEntry.Selectable(id, selected.Filter(static value => value != Guid.Empty).IfNone(static () => PostEffectUuids.ToneMapper_Clamp) == id))
            : (Flag(entry, RimagePart.On, fileName), Flag(entry, RimagePart.Shown, fileName))
                .Apply((enabled, shown) => (EffectEntry)new EffectEntry.Listed(id, enabled, shown))
                .As()
        let fields = Optional(entry.Element(RimagePart.State.Key)).ToSeq().Bind(static state => toSeq(state.Elements()))
            .Map(field => (Key: EntryKey.Named(EntryKey.TypeOwner(id), field.Name.LocalName), Text: field.Value))
        select (Effect: effect, Fields: fields);

    private static Fin<bool> Flag(XElement entry, RimagePart part, string fileName) =>
        Optional(entry.Attribute(part.Key))
            .Traverse(attribute => parseBool(attribute.Value).ToFin(new FileRefused(fileName, FileReason.Unreadable))).As()
            .Map(static value => value.IfNone(noneValue: false));

    private static Fin<HashMap<Guid, long>> Offsets(XElement xml, string fileName) =>
        Optional(xml.Element(RimagePart.Channels.Key))
            .Bind(static channels => toSeq(channels.Elements(RimagePart.Channel.Key))
                .Traverse(static channel =>
                    from id in Child(channel, RimagePart.Uuid, parseGuid)
                    from start in Child(channel, RimagePart.Offset, parseLong)
                    select (Id: id, Start: start))
                .As())
            .ToFin(new FileRefused(fileName, FileReason.Unreadable))
            .Map(static listed => toHashMap(listed));

    private static Seq<RenderWindow.StandardChannels> Components(RenderWindow.StandardChannels channel) =>
        channel switch {
            RenderWindow.StandardChannels.RGBA => Seq(RenderWindow.StandardChannels.Red, RenderWindow.StandardChannels.Green, RenderWindow.StandardChannels.Blue, RenderWindow.StandardChannels.Alpha),
            RenderWindow.StandardChannels.RGB => Seq(RenderWindow.StandardChannels.Red, RenderWindow.StandardChannels.Green, RenderWindow.StandardChannels.Blue),
            RenderWindow.StandardChannels.NormalXYZ => Seq(RenderWindow.StandardChannels.NormalX, RenderWindow.StandardChannels.NormalY, RenderWindow.StandardChannels.NormalZ),
            RenderWindow.StandardChannels.AlbedoRGB => Seq(RenderWindow.StandardChannels.AlbedoRed, RenderWindow.StandardChannels.AlbedoGreen, RenderWindow.StandardChannels.AlbedoBlue),
            _ => Seq(channel),
        };

    private Fin<float[]> Filled(FileStream file, long body, HashMap<Guid, long> offsets, RenderWindow.StandardChannels channel) =>
        from starts in Components(channel)
            .Traverse(component => offsets.Find(RenderWindow.ChannelId(component)).ToFin(new FileRefused(FileName, FileReason.Unreadable)))
            .As()
        let bytes = (long)sizeof(float) * Size.Width * Size.Height
        let marker = RimagePart.Chan.Key.Length + Unsafe.SizeOf<Guid>()
        from bounded in guard<Error>(starts.ForAll(start => start >= 0 && start <= file.Length - body - marker - bytes), new FileRefused(FileName, FileReason.Unreadable))
        select Fill(file, starts.Map(start => body + start + marker).Strict());

    private float[] Fill(FileStream file, Seq<long> planes) {
        float[] block = new float[4 * Size.Width * Size.Height];
        Span<byte> row = new byte[sizeof(float) * Size.Width];
        for (int lane = 0; lane < planes.Count; lane++) {
            for (int y = 0; y < Size.Height; y++) {
                file.Position = planes[lane] + ((long)y * row.Length);
                file.ReadExactly(row);
                Span<float> line = block.AsSpan(4 * Size.Width * y, 4 * Size.Width);
                for (int x = 0; x < Size.Width; x++)
                    line[(4 * x) + lane] = BinaryPrimitives.ReadSingleLittleEndian(row[(sizeof(float) * x)..]);
            }
        }
        Channels.Widen(block, planes.Count, 4, Size.Width * Size.Height);
        return block;
    }

    private static Option<A> Child<A>(XElement parent, RimagePart part, Func<string, Option<A>> parse) =>
        Optional(parent.Element(part.Key)).Bind(element => parse(element.Value));
}
