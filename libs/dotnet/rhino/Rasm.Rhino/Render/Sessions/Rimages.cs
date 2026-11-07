using System.Buffers.Binary;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Rhino.Render.Effects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Rimage(string FileName, Size Size, Seq<EffectEntry> Effects, HashMap<Guid, HashMap<string, string>> State) {
    // --- [CONSTANTS]
    private const int Version = 2;
    private const string Start = "[RIMAGE-START]";
    private const string Body = "[RIMAGE-BODY]";
    private const string Chan = "[RIMAGE-CHAN]";
    private const string Width = "width";
    private const string Height = "height";
    private const string ChannelList = "channels";
    private const string ChannelEntry = "channel";
    private const string Uuid = "uuid";
    private const string Offset = "start";
    private const string PostEffects = "post-effects-new";
    private const string PlugIn = "plug-in";
    private const string ToneMapping = "tone-mapping";
    private const string Selection = "selection";
    private const string Id = "id";
    private const string Name = "name";
    private const string On = "on";
    private const string Shown = "shown";
    private const string Fields = "state";

    // --- [READS]
    public Seq<Guid> Running => Effects.Filter(static effect => effect.Runs).Map(static effect => effect.Id);

    public static IO<Rimage> Open(string fileName) =>
        Read(fileName, (_, index) =>
            from size in Measured(index.Xml, fileName)
            from effects in Optional(index.Xml.Element(PostEffects)).ToFin(new RhinoFault(fileName, RhinoReason.Unreadable))
            from entries in Products.Each(
                from stage in toSeq(effects.Elements())
                from entry in toSeq(stage.Elements(PlugIn))
                where entry.Attribute(Name) is not null
                select (Stage: stage, Entry: entry),
                (row, _) => Entry(fileName, effects, row.Stage, row.Entry))
            select new Rimage(fileName, size, entries.Map(static entry => entry.Effect), toHashMap(entries.Map(static entry => (entry.Effect.Id, entry.Fields)))));

    public IO<Seq<RenderWindow.StandardChannels>> Available =>
        Read(FileName, (_, index) => Offsets(index.Xml, FileName).Map(static offsets =>
            toSeq(Enum.GetValues<RenderWindow.StandardChannels>())
                .Filter(channel => channel is not RenderWindow.StandardChannels.None && Channels.Components(channel).ForAll(component => offsets.ContainsKey(RenderWindow.ChannelId(component))))
                .Strict()));

    public IO<Memory2D<float>> Channel(RenderWindow.StandardChannels channel) =>
        Read(FileName, (file, index) => Offsets(index.Xml, FileName).Bind(offsets => Filled(file, index.Body, offsets, channel)));

    private static IO<A> Read<A>(string fileName, Func<FileStream, (XElement Xml, long Body), Fin<A>> body) =>
        use(() => new FileStream(fileName, new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read, Share = FileShare.Read, Options = FileOptions.RandomAccess }))
            .Bind(file => IO.lift(Indexed(file, fileName).Bind(index => body(file, index))))
            .Bracket();

    private static Fin<(XElement Xml, long Body)> Indexed(FileStream file, string fileName) {
        Span<byte> lead = stackalloc byte[Start.Length + (2 * sizeof(int))];
        int read = file.ReadAtLeast(lead, lead.Length, throwOnEndOfStream: false);
        if (read != lead.Length || !Ascii.Equals(lead[..Start.Length], Start) || BinaryPrimitives.ReadInt32LittleEndian(lead[Start.Length..]) != Version)
            return new RhinoFault(fileName, RhinoReason.Unreadable);
        int length = BinaryPrimitives.ReadInt32LittleEndian(lead[^sizeof(int)..]);
        if (length < 0 || length % sizeof(char) is not 0 || length > System.Array.MaxLength - sizeof(char) - Body.Length
            || (long)length + sizeof(char) + Body.Length > file.Length - lead.Length)
            return new RhinoFault(fileName, RhinoReason.Unreadable);
        byte[] header = new byte[length + sizeof(char) + Body.Length];
        long body = (long)lead.Length + header.Length;
        return file.ReadAtLeast(header, header.Length, throwOnEndOfStream: false) == header.Length && Ascii.Equals(header.AsSpan(length + sizeof(char)), Body)
            ? Try.lift(() => XElement.Parse(Encoding.Unicode.GetString(header, 0, length))).Run()
                .MapFail(error => error.HasException<XmlException>() ? new RhinoFault(fileName, RhinoReason.Unreadable, Inner: error) : error)
                .Map(xml => (xml, body))
            : new RhinoFault(fileName, RhinoReason.Unreadable);
    }

    private static Fin<Size> Measured(XElement xml, string fileName) =>
        (Child(xml, Width, parseInt).ToFin(new RhinoFault(fileName, RhinoReason.Unreadable)),
         Child(xml, Height, parseInt).ToFin(new RhinoFault(fileName, RhinoReason.Unreadable)))
            .Apply(static (width, height) => new Size(width, height))
            .As()
            .Bind(size => size is { Width: > 0, Height: > 0 } && (long)size.Width * size.Height <= System.Array.MaxLength / 4
                ? Fin.Succ(size)
                : new RhinoFault(fileName, RhinoReason.Unreadable));

    private static Fin<(EffectEntry Effect, HashMap<string, string> Fields)> Entry(string fileName, XElement effects, XElement stage, XElement entry) =>
        from id in Optional(entry.Attribute(Id)).Bind(static attribute => parseGuid(attribute.Value)).ToFin(new RhinoFault(fileName, RhinoReason.Unreadable))
        from effect in string.Equals(stage.Name.LocalName, ToneMapping, StringComparison.Ordinal)
            ? (Optional(stage.Attribute(Selection)).Map(static attribute => attribute.Value) | Optional(effects.Element($"{stage.Name.LocalName}-{Selection}")).Map(static element => element.Value))
                .Traverse(text => parseGuid(text).ToFin(new RhinoFault(fileName, RhinoReason.Unreadable))).As()
                .Map(selected => (EffectEntry)new EffectEntry.Selectable(id, EffectList.ToneMapper(selected) == id))
            : (Flag(entry, On, fileName), Flag(entry, Shown, fileName)).Apply((enabled, shown) => (EffectEntry)new EffectEntry.Listed(id, enabled, shown)).As()
        let fields = toHashMap(Optional(entry.Element(Fields)).ToSeq().Bind(static state => toSeq(state.Elements())).Map(static field => (field.Name.LocalName, field.Value)))
        select (effect, fields);

    private static Fin<bool> Flag(XElement entry, string part, string fileName) =>
        Optional(entry.Attribute(part)).Traverse(attribute => parseBool(attribute.Value).ToFin(new RhinoFault(fileName, RhinoReason.Unreadable))).As()
            .Map(static value => value.IfNone(noneValue: false));

    private static Fin<HashMap<Guid, long>> Offsets(XElement xml, string fileName) =>
        Optional(xml.Element(ChannelList))
            .Bind(static channels => toSeq(channels.Elements(ChannelEntry))
                .Traverse(static channel =>
                    from id in Child(channel, Uuid, parseGuid)
                    from start in Child(channel, Offset, parseLong)
                    select (Id: id, Start: start))
                .As())
            .ToFin(new RhinoFault(fileName, RhinoReason.Unreadable))
            .Map(static listed => toHashMap(listed));

    private Fin<Memory2D<float>> Filled(FileStream file, long body, HashMap<Guid, long> offsets, RenderWindow.StandardChannels channel) =>
        from starts in Channels.Components(channel).Traverse(component => offsets.Find(RenderWindow.ChannelId(component)).ToFin(new RhinoFault(FileName, RhinoReason.Unreadable))).As()
        let bytes = (long)sizeof(float) * Size.Width * Size.Height
        let marker = Chan.Length + Unsafe.SizeOf<Guid>()
        let length = file.Length
        from bounded in RhinoFault.Unless(!starts.IsEmpty && starts.ForAll(start => start >= 0 && start <= length - body - marker - bytes), RhinoReason.Unreadable, FileName)
        select Planes(file, starts.Map(start => body + start + marker).Strict());

    private Memory2D<float> Planes(FileStream file, Seq<long> planes) {
        int count = Size.Width * Size.Height;
        float[] rgba = GC.AllocateUninitializedArray<float>(4 * count);
        Span2D<float> pixels = new(rgba, count, 4);
        using SpanOwner<float> plane = SpanOwner<float>.Allocate(count);
        for (int channel = 0; channel < planes.Count; channel++) {
            file.Position = planes[channel];
            file.ReadExactly(MemoryMarshal.AsBytes(plane.Span));
            if (!BitConverter.IsLittleEndian) BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<float, int>(plane.Span), MemoryMarshal.Cast<float, int>(plane.Span));
            plane.Span.AsSpan2D(count, 1).CopyTo(pixels[.., channel..(channel + 1)]);
        }
        Channels.Widen(pixels, planes.Count);
        return new Memory2D<float>(rgba, Size.Height, 4 * Size.Width);
    }

    private static Option<A> Child<A>(XElement parent, string part, Func<string, Option<A>> parse) =>
        Optional(parent.Element(part)).Bind(element => parse(element.Value));
}
