using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using NodaTime.TimeZones;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderingState(
    Guid Session, string Engine, Guid EngineId, Duration Elapsed, Option<ValueSet> Settings, Option<string> Note);

[SmartEnum]
public sealed partial class RenderingPart {
    public static readonly RenderingPart Settings = new(static rendering => rendering.State.Bind(static state => state.Settings));
    public static readonly RenderingPart Grading = new(static rendering => Some(rendering.Grading));

    [UseDelegateFromConstructor]
    public partial Option<ValueSet> Of(Rendering rendering);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RimagePart {
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

public sealed record Rendering(string FileName, Instant Saved, PixelExtent Extent, ValueSet Grading, Seq<Guid> Chain, Option<RenderingState> State) {
    public ContentKey Key =>
        ContentKey.Of(KeyDomain.Rendering, stream => State.Match(
            Some: state => stream.Id(state.Session),
            None: () => stream.Integer(Saved.ToUnixTimeMilliseconds()).Integer(Extent.Width).Integer(Extent.Height)));

    public Seq<EffectKind> Running(Seq<EffectKind> kinds) => Chain.Choose(id => kinds.Find(kind => kind.Id == id));

    public static IO<(PixelExtent Extent, ValueSet Grading, Seq<Guid> Chain)> Header(string fileName) =>
        Read(fileName, (_, index) =>
            from extent in Measured(index.Xml, fileName)
            from effects in Optional(index.Xml.Element(RimagePart.PostEffects.Key)).ToFin(new MalformedRendering(fileName, RimagePart.PostEffects))
            from entries in Callbacks.Each(
                from stage in toSeq(effects.Elements())
                from entry in toSeq(stage.Elements(RimagePart.PlugIn.Key))
                where entry.Attribute(RimagePart.Name.Key) is not null
                select (Stage: stage, Entry: entry),
                (row, _) => Entry(fileName, effects, row))
            select (extent, new ValueSet(toHashMap(entries.Bind(static entry => entry.Fields))), entries.Filter(static entry => entry.Running).Map(static entry => entry.Id)));

    public IO<PixelFrame> Frame => Channel(RenderWindow.StandardChannels.RGBA);

    public IO<PixelFrame> Channel(RenderWindow.StandardChannels channel) =>
        Read(FileName, (file, index) => Offsets(index.Xml).Bind(offsets => Filled(file, index.Body, offsets, channel)));

    public IO<HashMap<RenderWindow.StandardChannels, PixelFrame>> Channels =>
        Read(FileName, (file, index) =>
            from offsets in Offsets(index.Xml)
            from frames in toSeq(SavedChannel.Items).Filter(channel => Components(channel.Key).ForAll(component => offsets.ContainsKey(RenderWindow.ChannelId(component))))
                .Traverse(channel => Filled(file, index.Body, offsets, channel.Key).Map(frame => (channel.Key, frame)).ToValidation()).As().ToFin()
            select toHashMap(frames));

    public Option<string> Note => State.Bind(static state => state.Note);

    private static IO<A> Read<A>(string fileName, Func<FileStream, (XElement Xml, long Body), Fin<A>> body) =>
        DisposalOps.Within(
            () => new FileStream(fileName, new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read, Share = FileShare.Read, Options = FileOptions.RandomAccess }),
            file => IO.lift(() => Indexed(file, fileName).Bind(index => body(file, index))),
            static file => IO.lift(file.Dispose));

    private static Fin<(XElement Xml, long Body)> Indexed(FileStream file, string fileName) {
        const int Version = 2;
        int start = RimagePart.Start.Key.Length;
        Span<byte> lead = stackalloc byte[start + (2 * sizeof(int))];
        int read = file.ReadAtLeast(lead, lead.Length, throwOnEndOfStream: false);
        if (read != lead.Length || !Ascii.Equals(lead[..start], RimagePart.Start.Key) || BinaryPrimitives.ReadInt32LittleEndian(lead[start..]) != Version)
            return new MalformedRendering(fileName, RimagePart.Start);
        int length = BinaryPrimitives.ReadInt32LittleEndian(lead[^sizeof(int)..]);
        if (length < 0 || length % sizeof(char) != 0 || length > System.Array.MaxLength - sizeof(char) - RimagePart.Body.Key.Length
            || (long)length + sizeof(char) + RimagePart.Body.Key.Length > file.Length - lead.Length)
            return new MalformedRendering(fileName, RimagePart.Start);
        byte[] header = new byte[length + sizeof(char) + RimagePart.Body.Key.Length];
        long body = (long)lead.Length + header.Length;
        return file.ReadAtLeast(header, header.Length, throwOnEndOfStream: false) == header.Length && Ascii.Equals(header.AsSpan(length + sizeof(char)), RimagePart.Body.Key)
            ? Try.lift(() => XElement.Parse(Encoding.Unicode.GetString(header, 0, length))).Run()
                .BindFail(error => error.HasException<XmlException>() ? new MalformedRendering(fileName, RimagePart.Body) : error)
                .Map(xml => (Xml: xml, Body: body))
            : new MalformedRendering(fileName, RimagePart.Body);
    }

    private static Fin<PixelExtent> Measured(XElement xml, string fileName) =>
        (Child(xml, RimagePart.Width, parseInt).ToValidation<Error>(new MalformedRendering(fileName, RimagePart.Width)),
         Child(xml, RimagePart.Height, parseInt).ToValidation<Error>(new MalformedRendering(fileName, RimagePart.Height)))
            .Apply(static (width, height) => (Width: width, Height: height))
            .As()
            .ToFin()
            .Bind(static size => RenderWindows.Extent(size.Width, size.Height));

    private static Fin<(Guid Id, bool Running, Seq<(EntryKey Key, string Text)> Fields)> Entry(string fileName, XElement effects, (XElement Stage, XElement Entry) row) =>
        from id in Optional(row.Entry.Attribute(RimagePart.Id.Key)).Bind(static attribute => parseGuid(attribute.Value))
            .ToFin(new MalformedRendering(fileName, RimagePart.Id))
        from running in string.Equals(row.Stage.Name.LocalName, RimagePart.ToneMapping.Key, StringComparison.Ordinal)
            ? (Optional(row.Stage.Attribute(RimagePart.Selection.Key)).Map(static attribute => attribute.Value)
               | Optional(effects.Element($"{row.Stage.Name.LocalName}-{RimagePart.Selection.Key}")).Map(static element => element.Value))
                .Traverse(text => parseGuid(text).ToFin(new MalformedRendering(fileName, RimagePart.Selection))).As()
                .Map(selected => selected.Filter(static value => value != Guid.Empty).IfNone(static () => BuiltinEffect.Clamp.Id) == id)
            : Callbacks.Each(Seq(RimagePart.On, RimagePart.Shown), (part, _) => Optional(row.Entry.Attribute(part.Key))
                    .Traverse(attribute => parseBool(attribute.Value).ToFin(new MalformedRendering(fileName, part))).As()
                    .Map(static value => value.IfNone(noneValue: false)))
                .Map(static flags => flags.ForAll(static flag => flag))
        let fields = Optional(row.Entry.Element(RimagePart.State.Key)).ToSeq().Bind(static state => toSeq(state.Elements()))
            .Map(field => (Key: EntryKey.Named(EntryKey.TypeOwner(id), field.Name.LocalName), Text: field.Value))
        select (Id: id, Running: running, Fields: fields);

    private Fin<HashMap<Guid, long>> Offsets(XElement xml) =>
        Optional(xml.Element(RimagePart.Channels.Key))
            .Bind(static channels => toSeq(channels.Elements(RimagePart.Channel.Key))
                .Traverse(static channel =>
                    from id in Child(channel, RimagePart.Uuid, parseGuid)
                    from start in Child(channel, RimagePart.Offset, parseLong)
                    select (Id: id, Start: start))
                .As())
            .ToFin(new MalformedRendering(FileName, RimagePart.Channels))
            .Map(static listed => toHashMap(listed));

    private static Seq<RenderWindow.StandardChannels> Components(RenderWindow.StandardChannels channel) =>
        channel switch {
            RenderWindow.StandardChannels.RGBA => Seq(RenderWindow.StandardChannels.Red, RenderWindow.StandardChannels.Green, RenderWindow.StandardChannels.Blue, RenderWindow.StandardChannels.Alpha),
            RenderWindow.StandardChannels.RGB => Seq(RenderWindow.StandardChannels.Red, RenderWindow.StandardChannels.Green, RenderWindow.StandardChannels.Blue),
            RenderWindow.StandardChannels.NormalXYZ => Seq(RenderWindow.StandardChannels.NormalX, RenderWindow.StandardChannels.NormalY, RenderWindow.StandardChannels.NormalZ),
            RenderWindow.StandardChannels.AlbedoRGB => Seq(RenderWindow.StandardChannels.AlbedoRed, RenderWindow.StandardChannels.AlbedoGreen, RenderWindow.StandardChannels.AlbedoBlue),
            _ => Seq(channel),
        };

    private Fin<PixelFrame> Filled(FileStream file, long body, HashMap<Guid, long> offsets, RenderWindow.StandardChannels channel) =>
        from starts in Components(channel)
            .Map(component => offsets.Find(RenderWindow.ChannelId(component)).ToValidation<Error>(new MissingPlane(FileName, component)))
            .Traverse(static plane => plane)
            .As()
            .ToFin()
        let length = file.Length
        let bytes = (long)sizeof(float) * Extent.Width * Extent.Height
        let marker = RimagePart.Chan.Key.Length + Unsafe.SizeOf<Guid>()
        from bounded in guard<Error>(starts.ForAll(start => start >= 0 && start <= length - body - marker - bytes), new MalformedRendering(FileName, RimagePart.Chan))
        let planes = starts.Map(start => body + start + marker).Strict()
        select new PixelFrame(System.Drawing.Point.Empty, Extent, Extent, block => Fill(file, planes, block));

    private void Fill(FileStream file, Seq<long> planes, float[] block) {
        Span<byte> row = new byte[sizeof(float) * Extent.Width];
        for (int line = 0; line < planes.Count * Extent.Height; line++) {
            (int lane, int y) = Math.DivRem(line, Extent.Height);
            file.Position = planes[lane] + ((long)y * row.Length);
            file.ReadExactly(row);
            for (int x = 0; x < Extent.Width; x++)
                block[(4 * ((y * Extent.Width) + x)) + lane] = BinaryPrimitives.ReadSingleLittleEndian(row[(sizeof(float) * x)..]);
        }
        for (int pixel = 0; planes.Count < 4 && pixel < Extent.Width * Extent.Height; pixel++) {
            int at = 4 * pixel;
            (block[at + 1], block[at + 2], block[at + 3]) =
                (block[at + int.Min(1, planes.Count - 1)], block[at + int.Min(2, planes.Count - 1)], 1f);
        }
    }

    private static Option<A> Child<A>(XElement parent, RimagePart part, Func<string, Option<A>> parse) =>
        Optional(parent.Element(part.Key)).Bind(element => parse(element.Value));
}

internal sealed record RenderingSidecar(Guid Session, string Engine, Guid EngineId, double ElapsedSeconds, ValueSet? Settings, string? Note) {
    [JsonIgnore]
    public RenderingState State =>
        new(Session, Engine, EngineId, Duration.FromSeconds(ElapsedSeconds), Optional(Settings), Optional(Note));

    public static RenderingSidecar Of(RenderingState state) =>
        new(state.Session, state.Engine, state.EngineId, state.Elapsed.TotalSeconds, state.Settings.ValueUnsafe(), state.Note.ValueUnsafe());
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true, Converters = [typeof(ValueSetConverter)])]
[JsonSerializable(typeof(RenderingSidecar))]
internal sealed partial class RenderingSidecarContext : JsonSerializerContext;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RenderHistory {
    // --- [LEDGER]
    private readonly IPlugInViews plugIn;
    private readonly string suffix;
    private readonly View.Section section;
    private readonly ViewStore store;
    private readonly Action<IDisposable> retain;
    private readonly Atom<Ledger> held = Atom(Ledger.Empty);

    private RenderHistory(IPlugInViews plugIn, string suffix, View.Section section, ViewStore store, Action<IDisposable> retain) =>
        (this.plugIn, this.suffix, this.section, this.store, this.retain) = (plugIn, suffix, section, store, retain);

    private sealed record Ledger(Option<string> Folder, Option<string> Claimed, HashMap<string, Rendering> Renderings) {
        public static Ledger Empty { get; } = new(None, None, HashMap<string, Rendering>());

        public Seq<Rendering> Listed(int keep) =>
            toSeq(Renderings.Values.OrderByDescending(static rendering => rendering.Saved).ThenBy(static rendering => rendering.FileName, StringComparer.Ordinal)).Take(keep);

        public Seq<(Rendering Rendering, Option<string> Note)> Renamed(Seq<Rendering> items) =>
            items.Bind(item => Renderings.Find(item.FileName).Filter(stored => stored.Note != item.Note).Map(stored => (stored, item.Note)).ToSeq());
    }

    // --- [ROWS]
    public static PlugInSlot<RenderHistory> Current { get; } = new();

    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Register(
        View.Section settings, Func<RenderHistory, Seq<Func<IPlugInViews, Action<IDisposable>, IO<Unit>>>> readers) =>
        (sink, retain) => DisposalOps.Acquire(owned =>
            from store in IO.lift(settings.Store.ToFin(new Missing(nameof(View.Store))))
            from history in IO.lift(() => new RenderHistory(sink, $".{sink.PlugIn.Id:N}.json", settings, store, owned))
            from saved in EventKind.ImageFileSaved.Inline(history.Saved, sink, owned)
            from loaded in EventKind.ImageFileLoaded.Inline(args => history.Listed(args.FileName), sink, owned)
            from deleted in EventKind.ImageFileDeleted.Inline(history.Deleted, sink, owned)
            from reading in readers(history).TraverseM(row => row(sink, owned)).As()
            from published in Current.Publish(sink, history)
            select unit, (_, lease) => retain(lease));

    // --- [BINDINGS]
    public Fin<BindingGroup> Grading(RhinoDoc doc) =>
        EffectRegistry.Kinds.Of(plugIn).Map(kinds => new BindingGroup(kinds.Map(kind => kind.Binding(doc))));

    public Fin<BindingGroup> Bindings(RhinoDoc doc) =>
        Grading(doc).Map(grading => new BindingGroup(RenderSettingsSources.Bindings(doc).Bindings + grading.Bindings));

    public Fin<HistoryBinding> Settings(RhinoDoc doc) =>
        Bindings(doc).Bind(bindings => ViewOps.History(plugIn, section, store, bindings, Some(doc)));

    // --- [READS]
    public IO<Seq<Rendering>> Renderings =>
        from ledger in held.ValueIO
        from keep in IO.lift(SupportOptions.AutoSaveKeepAmount)
        select ledger.Listed(keep);

    // --- [WRITES]
    public IO<T> Launched<T>(RhinoDoc doc, IO<(ImageFileEventArgs Saved, T Value)> render) =>
        from bindings in IO.lift(() => Bindings(doc))
        from captured in bindings.Capture
        from rendered in render
        from recorded in Recorded(rendered.Saved, Some(captured))
        select rendered.Value;

    public IO<ValueDiff> Restore(RhinoDoc doc, Rendering rendering, RenderingPart part) =>
        from restored in IO.lift(part.Of(rendering).ToFin(new Missing(nameof(Restore))))
        from binding in IO.lift(Settings(doc))
        from locks in ViewOps.Locks(section, Some(doc)).Match(
            Some: static locked => locked.Read,
            None: static () => IO.pure(Option<LanguageExt.HashSet<EntryKey>>.None))
        from diff in part.Switch(
            (Binding: binding, Doc: doc, Set: restored, rendering.Chain, Locks: locks.IfNone(LanguageExt.HashSet.empty<EntryKey>())),
            settings: static restore => restore.Binding.Commit(restore.Set, restore.Locks),
            grading: static restore => Commits.Commit(restore.Doc, restore.Binding.Caption, new RedrawPolicy.Silent(),
                    from chained in Rechained(restore.Doc, restore.Chain)
                    from written in restore.Binding.Commit(restore.Set, restore.Locks)
                    select written)
                .Map(static committed => committed.Value))
        select diff;

    public IO<Rendering> Annotate(Rendering rendering, Option<string> note) =>
        IO.lift(rendering.State.ToFin(new Missing(nameof(Annotate)))).Bind(state => Recorded(rendering, state with { Note = note }));

    public IO<Unit> Delete(Rendering rendering) =>
        from deleted in IO.lift(() => File.Delete(rendering.FileName))
        from sidecar in IO.lift(() => File.Delete(Sidecar(rendering.FileName)))
        from removed in held.SwapIO(ledger => ledger with { Renderings = ledger.Renderings.Remove(rendering.FileName) })
        select unit;

    private static IO<Unit> Rechained(RhinoDoc doc, Seq<Guid> chain) =>
        RenderSettingsSources.Edit(doc, scope =>
            from entries in EffectRegistry.Read(scope.Settings)
            from applied in EffectRegistry.Apply(scope.Settings, Requests(entries, chain))
            select applied);

    private static Seq<EffectRequest> Requests(Seq<EffectEntry> entries, Seq<Guid> chain) =>
        entries.Choose(entry => entry.Switch(
                chain,
                listed: static (running, listed) => Some<EffectRequest>(new EffectRequest.Listing(listed with { On = running.Exists(id => id == listed.Id) })),
                selectable: static (running, selectable) => running.Exists(id => id == selectable.Id) ? Some<EffectRequest>(new EffectRequest.Selecting(selectable)) : None))
            .Add(new EffectRequest.Ordering(chain.Choose(id => entries.Find(entry => entry.Id == id && entry.Switch(listed: static _ => true, selectable: static _ => false)))));

    // --- [EVENTS]
    public static HostEvent<Unit> Changed { get; } = new(
        typeof(RenderHistory),
        nameof(Changed),
        static (deliver, site, retain) =>
            IO.lift(() => Current.Of(site.Sink))
                .Bind(history => Subscriptions.Host<AtomChangedEvent<Ledger>, Ledger>(
                        typeof(RenderHistory), handler => history.held.Change += handler, handler => history.held.Change -= handler,
                        static handler => ledger => handler(sender: null, ledger), nameof(Changed))
                    .Choose(static _ => Some(unit))
                    .Attach(deliver, site, retain)));

    private IO<Unit> Saved(ImageFileEventArgs args) =>
        from listed in Listed(args.FileName)
        from recorded in Recorded(args, None)
        select unit;

    private IO<Unit> Deleted(ImageFileEventArgs args) =>
        from listed in Listed(args.FileName)
        from removed in held.SwapIO(ledger => ledger with { Renderings = ledger.Renderings.Remove(args.FileName) })
        from forked in Forked(IO.lift(() => File.Delete(Sidecar(args.FileName))), nameof(Deleted))
        select unit;

    private IO<Unit> Listed(string fileName) =>
        from folder in IO.lift(() => Missing.Unless(Path.GetDirectoryName(fileName), nameof(ImageFileEventArgs.FileName)))
        from ledger in held.SwapIO(prior => prior.Folder.IsSome ? prior with { Claimed = None } : prior with { Folder = folder, Claimed = folder })
        from scanned in ledger.Claimed.Match(
            Some: claimed => Forked(Scanned(claimed), nameof(Listed)),
            None: static () => IO.pure(unit))
        select unit;

    private IO<Unit> Forked(IO<Unit> work, string member) =>
        DisposalOps.Fork(work.Catch(error => IO.lift(() => plugIn.Report(error, typeof(RenderHistory), member))).As(), retain).Map(static _ => unit);

    // --- [FILES]
    private IO<Unit> Scanned(string folder) =>
        from fileNames in IO.lift(() => toSeq(Directory.EnumerateFiles(folder, "*.rimage")).Strict())
        from sidecars in IO.lift(() => toSeq(Directory.EnumerateFiles(folder, $"*{suffix}")).Strict())
        let kept = toHashSet(fileNames.Map(Sidecar))
        from orphans in sidecars.Filter(sidecar => !kept.Contains(sidecar)).TraverseM(static sidecar => IO.lift(() => File.Delete(sidecar))).As()
        from read in fileNames.Map<K<IO, Rendering>>(fileName => Stated(Sidecar(fileName)).Bind(state => Loaded(fileName, state))).PartitionFallible().As()
        from listed in held.SwapIO(ledger => ledger with { Renderings = ledger.Renderings.TryAddRange(read.Succs.Map(static rendering => (rendering.FileName, rendering))) })
        from failed in unless(read.Fails.IsEmpty, IO.fail<Unit>(Error.Many(read.Fails))).As()
        select unit;

    private static IO<Rendering> Loaded(string fileName, Option<RenderingState> state) =>
        from saved in IO.lift(() => File.GetLastWriteTimeUtc(fileName).ToInstant())
        from header in Rendering.Header(fileName)
        select new Rendering(fileName, saved, header.Extent, header.Grading, header.Chain, state);

    private static IO<Option<RenderingState>> Stated(string sidecar) =>
        IO.lift(() => File.ReadAllBytes(sidecar))
            .Bind(static bytes => IO.lift(() => Missing.Unless(JsonSerializer.Deserialize(bytes, RenderingSidecarContext.Default.RenderingSidecar), nameof(JsonSerializer.Deserialize))))
            .Map(static file => Some(file.State))
            .Catch(static error => error.HasException<FileNotFoundException>(), static _ => IO.pure(Option<RenderingState>.None))
            .Catch(static error => error.HasException<JsonException>(), error => IO.fail<Option<RenderingState>>(new MalformedSidecar(sidecar, error)));

    private IO<Rendering> Recorded(ImageFileEventArgs args, Option<ValueSet> settings) =>
        from saved in IO.lift(() => File.GetLastWriteTimeUtc(args.FileName).ToInstant())
        from header in Rendering.Header(args.FileName)
        let state = new RenderingState(args.SessionId, args.RenderEngine, args.RenderEngineId, Duration.FromSeconds(args.EllapsedTime),
            settings, None)
        from recorded in Recorded(new Rendering(args.FileName, saved, header.Extent, header.Grading, header.Chain, None), state)
        select recorded;

    private IO<Rendering> Recorded(Rendering rendering, RenderingState state) =>
        from path in IO.lift(Conversions.Validated<OutputPath, string, InvalidFileValue>(Sidecar(rendering.FileName)))
        from written in FilePublication.Write(path, overwrite: true, stream => IO.liftAsync(async environment => {
            await JsonSerializer.SerializeAsync(stream, RenderingSidecar.Of(state), RenderingSidecarContext.Default.RenderingSidecar, environment.Token).ConfigureAwait(false);
            return unit;
        }))
        let noted = rendering with { State = Some(state) }
        from listed in held.SwapIO(ledger => ledger with { Renderings = ledger.Renderings.AddOrUpdate(noted.FileName, noted) })
        select noted;

    private string Sidecar(string fileName) => Path.ChangeExtension(fileName, suffix);

    // --- [BROWSER]
    public static ControlRow Browser { get; } =
        ListRows.Browser(
            RowText.Localize("Render History").Local,
            RowText.Localize("Completed renders of this plug-in, newest first").Local,
            new BrowserRow<Rendering, string>(
                Key: static rendering => rendering.FileName,
                Label: Label,
                Tile: static (scope, view, rendering, size, retain) =>
                    from tiles in IO.lift(() => PreviewTiles.Current.Of(scope.Sink))
                    from kinds in IO.lift(() => EffectRegistry.Kinds.Of(scope.Sink))
                    from thumbnail in tiles.Thumbnail(view, TileRequest.Recorded(rendering, kinds), size, retain)
                    select thumbnail,
                Items: static scope => new ValueStore<Seq<Rendering>>(
                    IO.lift(() => Current.Of(scope.Sink)).Bind(static history => history.Renderings).Map(static renderings => Some(renderings)),
                    items => items.Match(
                        Some: named => IO.lift(() => Current.Of(scope.Sink)).Bind(history => history.Noted(named)),
                        None: static () => IO.pure(Change<Seq<Rendering>>.None)),
                    EqualityComparer<Seq<Rendering>>.Default.Equals,
                    Applied.Live,
                    None),
                Current: static scope =>
                    IO.lift(() => RenderingComparison.Current.Of(scope.Sink)).Bind(static comparison => comparison.Held).Map(static side => side.Map(static held => held.Rendering.FileName)),
                Choose: static (scope, fileName) =>
                    from history in IO.lift(() => Current.Of(scope.Sink))
                    from renderings in history.Renderings
                    from rendering in IO.lift(renderings.Find(rendering => string.Equals(rendering.FileName, fileName, StringComparison.Ordinal)).ToFin(new Missing(nameof(Renderings))))
                    from chosen in Compared(scope, rendering)
                    select chosen,
                Mode: BrowserMode.List,
                Extent: new ReadoutSource<PixelExtent>.Model(Framing.Extent),
                Empty: RowText.Localize("Completed renders appear here, newest first").Local) {
                Numbered = true,
                Search = Some(new ListSearch<Rendering>(RowText.Localize("Search history").Local, Label, None)),
                Rename = Some<Func<Rendering, string, Fin<Rendering>>>(static (rendering, text) =>
                    rendering.State.ToFin(new Missing(nameof(Annotate))).Map(state => rendering with { State = Some(state with { Note = Conversions.Present(text) }) })),
                Reloads = Seq(Changed),
                Commands = static selection =>
                    Seq<CommandRow>(
                        Command("history-compare", RowText.Localize("Compare").Local,
                            RowText.Localize("Compare the selected rendering against the current frame through its own grading").Local, Headed(selection, Compared)),
                        Command("history-compare-before", RowText.Localize("Compare Before").Local,
                            RowText.Localize("Compare the selected rendering's frame before its chain against the current frame").Local, Headed(selection, Before)),
                        Command("history-export", RowText.Localize("Export").Local,
                            RowText.Localize("Write the selected history frame through the output targets and its recorded grading").Local,
                            Headed(selection, static (scope, rendering) =>
                                from document in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
                                from frame in rendering.Frame
                                from book in TargetRows.Book(document)
                                from origin in FileOrigin.At(scope.Sink)
                                from kinds in IO.lift(() => EffectRegistry.Kinds.Of(scope.Sink))
                                let request = TileRequest.Recorded(rendering, kinds)
                                from written in Saves.Write(document, origin, book,
                                    Right<(Guid Rendering, RenderRun Run), (PixelFrame Scene, TileRequest Request)>((frame, request)))
                                select unit)))
                    + toSeq(RenderingPart.Items).Map<CommandRow>(part => Restoring(selection, part))
                    + Seq<CommandRow>(
                        Command("history-delete", RowText.Localize("Delete").Local, RowText.Localize("Delete the selected renderings and their recorded state").Local,
                            scope =>
                                from history in IO.lift(() => Current.Of(scope.Sink))
                                from chosen in selection
                                from deleted in chosen.TraverseM(rendering => history.Delete(rendering)).As()
                                select unit),
                        Command("history-clear-comparison", RowText.Localize("Clear Comparison").Local, RowText.Localize("Drop the rendering the frame view compares against").Local,
                            static scope => IO.lift(() => RenderingComparison.Current.Of(scope.Sink)).Bind(static comparison => comparison.Clear))),
                Wording = Wording.Localized,
            },
            new RowRules(None, None));

    private IO<Change<Seq<Rendering>>> Noted(Seq<Rendering> items) =>
        from ledger in held.ValueIO
        from keep in IO.lift(SupportOptions.AutoSaveKeepAmount)
        let before = ledger.Listed(keep)
        from noted in ledger.Renamed(items).TraverseM(item => Annotate(item.Rendering, item.Note)).As()
        let written = toHashMap(noted.Map(static rendering => (rendering.FileName, rendering)))
        select ValueStore.Between(Some(before), Some(before.Map(rendering => written.Find(rendering.FileName).IfNone(rendering))));

    private static IO<Unit> Compared(RowScope scope, Rendering rendering) =>
        IO.lift(() => RenderingComparison.Current.Of(scope.Sink)).Bind(comparison => comparison.Choose(new CompareSide.Recorded(rendering), Label(scope, rendering)));

    private static IO<Unit> Before(RowScope scope, Rendering rendering) =>
        IO.lift(() => RenderingComparison.Current.Of(scope.Sink)).Bind(comparison => comparison.Choose(new CompareSide.Before(rendering), RowText.Localize("Before").Local));

    private static string Label(RowScope scope, Rendering rendering) =>
        rendering.Note.IfNone(() =>
            rendering.Saved.InZone(BclDateTimeZone.FromTimeZoneInfo(((IPlugInViews)scope.Sink).Clock.LocalTimeZone)).LocalDateTime.ToString("g", RowText.Culture));

    private static Func<RowScope, IO<Unit>> Headed(IO<Seq<Rendering>> selection, Func<RowScope, Rendering, IO<Unit>> act) =>
        scope => selection.Bind(chosen => chosen.Head.Traverse(rendering => act(scope, rendering)).As()).Map(static _ => unit);

    private static CommandRow.Run Restoring(IO<Seq<Rendering>> selection, RenderingPart part) =>
        part.Map(settings: RowText.Localize("Render Settings").Local, grading: RowText.Localize("Grading").Local) switch {
            var caption => Command(
                part.Map(settings: "history-restore-settings", grading: "history-restore-grading"),
                RowText.Localize("Restore {0}", arguments: [caption]).Local,
                RowText.Localize("Write the selected rendering's {0} into the document as one commit", arguments: [caption]).Local,
                Headed(selection, (scope, rendering) =>
                    from doc in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
                    from history in IO.lift(() => Current.Of(scope.Sink))
                    from restored in history.Restore(doc, rendering, part)
                    select unit)),
        };

    private static CommandRow.Run Command(string name, string caption, string help, Func<RowScope, IO<Unit>> execute) =>
        new(new CommandFace(name, caption, help, None, None) { Wording = Wording.Localized }, None, execute);
}
