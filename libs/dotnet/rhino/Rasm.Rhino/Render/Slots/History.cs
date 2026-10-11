using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using NodaTime.TimeZones;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render;

namespace Rasm.Rhino.Render.Slots;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SlotState(
    Guid Session, string Engine, Guid EngineId, Duration Elapsed, Option<ValueSet> Settings, Option<string> Note) {
    public ContentKey Key => ContentKey.Of(KeyDomain.HistorySlot, stream => stream.Id(Session));
}

[SmartEnum]
public sealed partial class SlotPart {
    public static readonly SlotPart Settings = new(static slot => slot.State.Bind(static state => state.Settings));
    public static readonly SlotPart Grading = new(static slot => Some(slot.Grading));

    [UseDelegateFromConstructor]
    public partial Option<ValueSet> Of(RenderSlot slot);
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

public sealed record RenderSlot(string Rendering, Instant Saved, PixelExtent Extent, ValueSet Grading, Seq<Guid> Chain, Option<SlotState> State) {
    public Seq<EffectKind> Running(Seq<EffectKind> kinds) => Chain.Choose(id => kinds.Find(kind => kind.Id == id));

    public static IO<(PixelExtent Extent, ValueSet Grading, Seq<Guid> Chain)> Header(string rendering) =>
        Read(rendering, (_, index) =>
            from extent in Measured(index.Xml, rendering)
            from effects in Optional(index.Xml.Element(RimagePart.PostEffects.Key)).ToFin(new MalformedRendering(rendering, RimagePart.PostEffects))
            from entries in Callbacks.Each(
                from stage in toSeq(effects.Elements())
                from entry in toSeq(stage.Elements(RimagePart.PlugIn.Key))
                where entry.Attribute(RimagePart.Name.Key) is not null
                select (Stage: stage, Entry: entry),
                (row, _) => Entry(rendering, effects, row))
            select (extent, new ValueSet(toHashMap(entries.Bind(static entry => entry.Fields))), entries.Filter(static entry => entry.Running).Map(static entry => entry.Id)));

    public IO<PixelFrame> Frame => Channel(RenderWindow.StandardChannels.RGBA);

    public IO<PixelFrame> Channel(RenderWindow.StandardChannels channel) =>
        Read(Rendering, (file, index) => Offsets(index.Xml).Bind(offsets => Filled(file, index.Body, offsets, channel)));

    public IO<HashMap<RenderWindow.StandardChannels, PixelFrame>> Channels =>
        Read(Rendering, (file, index) =>
            from offsets in Offsets(index.Xml)
            from frames in toSeq(SavedChannel.Items).Filter(channel => Components(channel.Key).ForAll(component => offsets.ContainsKey(RenderWindow.ChannelId(component))))
                .Traverse(channel => Filled(file, index.Body, offsets, channel.Key).Map(frame => (channel.Key, frame)).ToValidation()).As().ToFin()
            select toHashMap(frames));

    public Option<string> Note => State.Bind(static state => state.Note);

    private static IO<A> Read<A>(string rendering, Func<FileStream, (XElement Xml, long Body), Fin<A>> body) =>
        DisposalOps.Within(
            () => new FileStream(rendering, new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read, Share = FileShare.Read, Options = FileOptions.RandomAccess }),
            file => IO.lift(() => Indexed(file, rendering).Bind(index => body(file, index))),
            static file => IO.lift(file.Dispose));

    private static Fin<(XElement Xml, long Body)> Indexed(FileStream file, string rendering) {
        const int Version = 2;
        int start = RimagePart.Start.Key.Length;
        Span<byte> lead = stackalloc byte[start + (2 * sizeof(int))];
        int read = file.ReadAtLeast(lead, lead.Length, throwOnEndOfStream: false);
        if (read != lead.Length || !Ascii.Equals(lead[..start], RimagePart.Start.Key) || BinaryPrimitives.ReadInt32LittleEndian(lead[start..]) != Version)
            return new MalformedRendering(rendering, RimagePart.Start);
        int length = BinaryPrimitives.ReadInt32LittleEndian(lead[^sizeof(int)..]);
        if (length < 0 || length % sizeof(char) != 0 || length > System.Array.MaxLength - sizeof(char) - RimagePart.Body.Key.Length
            || (long)length + sizeof(char) + RimagePart.Body.Key.Length > file.Length - lead.Length)
            return new MalformedRendering(rendering, RimagePart.Start);
        byte[] header = new byte[length + sizeof(char) + RimagePart.Body.Key.Length];
        long body = (long)lead.Length + header.Length;
        return file.ReadAtLeast(header, header.Length, throwOnEndOfStream: false) == header.Length && Ascii.Equals(header.AsSpan(length + sizeof(char)), RimagePart.Body.Key)
            ? Try.lift(() => XElement.Parse(Encoding.Unicode.GetString(header, 0, length))).Run()
                .BindFail(error => error.HasException<XmlException>() ? new MalformedRendering(rendering, RimagePart.Body) : error)
                .Map(xml => (Xml: xml, Body: body))
            : new MalformedRendering(rendering, RimagePart.Body);
    }

    private static Fin<PixelExtent> Measured(XElement xml, string rendering) =>
        (Child(xml, RimagePart.Width, parseInt).ToValidation<Error>(new MalformedRendering(rendering, RimagePart.Width)),
         Child(xml, RimagePart.Height, parseInt).ToValidation<Error>(new MalformedRendering(rendering, RimagePart.Height)))
            .Apply(static (width, height) => (Width: width, Height: height))
            .As()
            .ToFin()
            .Bind(static size => RenderWindows.Extent(size.Width, size.Height));

    private static Fin<(Guid Id, bool Running, Seq<(EntryKey Key, string Text)> Fields)> Entry(string rendering, XElement effects, (XElement Stage, XElement Entry) row) =>
        from id in Optional(row.Entry.Attribute(RimagePart.Id.Key)).Bind(static attribute => parseGuid(attribute.Value))
            .ToFin(new MalformedRendering(rendering, RimagePart.Id))
        from running in string.Equals(row.Stage.Name.LocalName, RimagePart.ToneMapping.Key, StringComparison.Ordinal)
            ? (Optional(row.Stage.Attribute(RimagePart.Selection.Key)).Map(static attribute => attribute.Value)
               | Optional(effects.Element($"{row.Stage.Name.LocalName}-{RimagePart.Selection.Key}")).Map(static element => element.Value))
                .Traverse(text => parseGuid(text).ToFin(new MalformedRendering(rendering, RimagePart.Selection))).As()
                .Map(selected => selected.Filter(static value => value != Guid.Empty).IfNone(static () => BuiltinEffect.Clamp.Id) == id)
            : Callbacks.Each(Seq(RimagePart.On, RimagePart.Shown), (part, _) => Optional(row.Entry.Attribute(part.Key))
                    .Traverse(attribute => parseBool(attribute.Value).ToFin(new MalformedRendering(rendering, part))).As()
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
            .ToFin(new MalformedRendering(Rendering, RimagePart.Channels))
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
            .Map(component => offsets.Find(RenderWindow.ChannelId(component)).ToValidation<Error>(new MissingPlane(Rendering, component)))
            .Traverse(static plane => plane)
            .As()
            .ToFin()
        let length = file.Length
        let bytes = (long)sizeof(float) * Extent.Width * Extent.Height
        let marker = RimagePart.Chan.Key.Length + Unsafe.SizeOf<Guid>()
        from bounded in guard<Error>(starts.ForAll(start => start >= 0 && start <= length - body - marker - bytes), new MalformedRendering(Rendering, RimagePart.Chan))
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

internal sealed record SlotFile(Guid Session, string Engine, Guid EngineId, double ElapsedSeconds, ValueSet? Settings, string? Note) {
    [JsonIgnore]
    public SlotState State =>
        new(Session, Engine, EngineId, Duration.FromSeconds(ElapsedSeconds), Optional(Settings), Optional(Note));

    public static SlotFile Of(SlotState state) =>
        new(state.Session, state.Engine, state.EngineId, state.Elapsed.TotalSeconds, state.Settings.ValueUnsafe(), state.Note.ValueUnsafe());
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true, Converters = [typeof(ValueSetConverter)])]
[JsonSerializable(typeof(SlotFile))]
internal sealed partial class SlotFileContext : JsonSerializerContext;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RenderHistory {
    // --- [LEDGER]
    private readonly IPlugInRendering plugIn;
    private readonly string suffix;
    private readonly View.Section section;
    private readonly ViewStore store;
    private readonly Action<IDisposable> retain;
    private readonly Atom<Ledger> held = Atom(Ledger.Empty);

    private RenderHistory(IPlugInRendering plugIn, string suffix, View.Section section, ViewStore store, Action<IDisposable> retain) =>
        (this.plugIn, this.suffix, this.section, this.store, this.retain) = (plugIn, suffix, section, store, retain);

    private sealed record Ledger(Option<string> Folder, Option<string> Claimed, HashMap<string, RenderSlot> Slots) {
        public static Ledger Empty { get; } = new(None, None, HashMap<string, RenderSlot>());

        public Seq<RenderSlot> Listed(int keep) =>
            toSeq(Slots.Values.OrderByDescending(static slot => slot.Saved).ThenBy(static slot => slot.Rendering, StringComparer.Ordinal)).Take(keep);

        public Seq<(RenderSlot Slot, Option<string> Note)> Renamed(Seq<RenderSlot> items) =>
            items.Bind(item => Slots.Find(item.Rendering).Filter(stored => stored.Note != item.Note).Map(stored => (stored, item.Note)).ToSeq());
    }

    // --- [ROWS]
    public static Func<PlugIn, IPlugInSink, Action<IDisposable>, IO<Unit>> Register(
        View.Section settings, Func<RenderHistory, Seq<Func<PlugIn, IPlugInSink, Action<IDisposable>, IO<Unit>>>> readers) =>
        (plugIn, sink, retain) => DisposalOps.Acquire(owned =>
            from store in IO.lift(settings.Store.ToFin(new Missing(nameof(View.Store))))
            from history in IO.lift(() => new RenderHistory((IPlugInRendering)sink, $".{plugIn.Id:N}.json", settings, store, owned))
            from saved in EventKind.ImageFileSaved.Inline(history.Saved, sink, owned)
            from loaded in EventKind.ImageFileLoaded.Inline(args => history.Listed(args.FileName), sink, owned)
            from deleted in EventKind.ImageFileDeleted.Inline(history.Deleted, sink, owned)
            from reading in readers(history).TraverseM(row => row(plugIn, sink, owned)).As()
            let cell = history.plugIn.History
            from published in DisposalOps.Acquire(
                    IO.lift(() => new Disposal<Atom<Option<RenderHistory>>>(cell, static filled => filled.Swap(static _ => None))),
                    static lease => IO.lift(lease.Dispose), owned)
                .Bind(_ => cell.SwapIO(_ => Some(history))).Uninterruptible()
            select unit, (_, lease) => retain(lease));

    // --- [BINDINGS]
    public BindingGroup Grading(RhinoDoc doc) =>
        new(plugIn.Effects.Map(kind => kind.Binding(doc)));

    public BindingGroup Bindings(RhinoDoc doc) =>
        new(SceneSources.Bindings(doc).Bindings + Grading(doc).Bindings);

    public Fin<HistoryBinding> Settings(RhinoDoc doc) =>
        ViewOps.History((IPlugInViews)plugIn, section, store, Bindings(doc), Some(doc));

    // --- [READS]
    public IO<Seq<RenderSlot>> Slots =>
        from ledger in held.ValueIO
        from keep in IO.lift(SupportOptions.AutoSaveKeepAmount)
        select ledger.Listed(keep);

    // --- [WRITES]
    public IO<T> Launched<T>(RhinoDoc doc, IO<(ImageFileEventArgs Saved, T Value)> render) =>
        from captured in Bindings(doc).Capture
        from rendered in render
        from recorded in Recorded(rendered.Saved, Some(captured))
        select rendered.Value;

    public IO<ValueDiff> Restore(RhinoDoc doc, RenderSlot slot, SlotPart part) =>
        from restored in IO.lift(part.Of(slot).ToFin(new Missing(nameof(Restore))))
        from binding in IO.lift(Settings(doc))
        from locks in ViewOps.Locks(section, Some(doc)).Match(
            Some: static locked => locked.Read,
            None: static () => IO.pure(Option<LanguageExt.HashSet<EntryKey>>.None))
        from diff in part.Switch(
            (Binding: binding, Doc: doc, Set: restored, slot.Chain, Locks: locks.IfNone(LanguageExt.HashSet.empty<EntryKey>())),
            settings: static restore => restore.Binding.Commit(restore.Set, restore.Locks),
            grading: static restore => Commits.Commit(restore.Doc, restore.Binding.Caption, new RedrawPolicy.Silent(),
                    from chained in Rechained(restore.Doc, restore.Chain)
                    from written in restore.Binding.Commit(restore.Set, restore.Locks)
                    select written)
                .Map(static committed => committed.Value))
        select diff;

    public IO<RenderSlot> Annotate(RenderSlot slot, Option<string> note) =>
        IO.lift(slot.State.ToFin(new Missing(nameof(Annotate)))).Bind(state => Recorded(slot, state with { Note = note }));

    public IO<Unit> Delete(RenderSlot slot) =>
        from rendering in IO.lift(() => File.Delete(slot.Rendering))
        from sidecar in IO.lift(() => File.Delete(Sidecar(slot.Rendering)))
        from removed in held.SwapIO(ledger => ledger with { Slots = ledger.Slots.Remove(slot.Rendering) })
        select unit;

    private static IO<Unit> Rechained(RhinoDoc doc, Seq<Guid> chain) =>
        SceneSources.Edit(doc, window =>
            from entries in EffectRegistry.Read(window.Settings)
            from applied in EffectRegistry.Apply(window.Settings, Requests(entries, chain))
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
            IPlugInRendering.Served(((IPlugInRendering)site.Sink).History, nameof(IPlugInRendering.History))
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
        from removed in held.SwapIO(ledger => ledger with { Slots = ledger.Slots.Remove(args.FileName) })
        from forked in Forked(IO.lift(() => File.Delete(Sidecar(args.FileName))), nameof(Deleted))
        select unit;

    private IO<Unit> Listed(string rendering) =>
        from folder in IO.lift(() => Missing.Unless(Path.GetDirectoryName(rendering), nameof(ImageFileEventArgs.FileName)))
        from ledger in held.SwapIO(prior => prior.Folder.IsSome ? prior with { Claimed = None } : prior with { Folder = folder, Claimed = folder })
        from scanned in ledger.Claimed.Match(
            Some: claimed => Forked(Scanned(claimed), nameof(Listed)),
            None: static () => IO.pure(unit))
        select unit;

    private IO<Unit> Forked(IO<Unit> work, string member) =>
        DisposalOps.Fork(work.Catch(error => IO.lift(() => plugIn.Report(error, typeof(RenderHistory), member))).As(), retain).Map(static _ => unit);

    // --- [FILES]
    private IO<Unit> Scanned(string folder) =>
        from renderings in IO.lift(() => toSeq(Directory.EnumerateFiles(folder, "*.rimage")).Strict())
        from sidecars in IO.lift(() => toSeq(Directory.EnumerateFiles(folder, $"*{suffix}")).Strict())
        let kept = toHashSet(renderings.Map(Sidecar))
        from orphans in sidecars.Filter(sidecar => !kept.Contains(sidecar)).TraverseM(static sidecar => IO.lift(() => File.Delete(sidecar))).As()
        from read in renderings.Map<K<IO, RenderSlot>>(rendering => Stated(Sidecar(rendering)).Bind(state => Slot(rendering, state))).PartitionFallible().As()
        from listed in held.SwapIO(ledger => ledger with { Slots = ledger.Slots.TryAddRange(read.Succs.Map(static slot => (slot.Rendering, slot))) })
        from failed in unless(read.Fails.IsEmpty, IO.fail<Unit>(Error.Many(read.Fails))).As()
        select unit;

    private static IO<RenderSlot> Slot(string rendering, Option<SlotState> state) =>
        from saved in IO.lift(() => File.GetLastWriteTimeUtc(rendering).ToInstant())
        from header in RenderSlot.Header(rendering)
        select new RenderSlot(rendering, saved, header.Extent, header.Grading, header.Chain, state);

    private static IO<Option<SlotState>> Stated(string sidecar) =>
        IO.lift(() => File.ReadAllBytes(sidecar))
            .Bind(static bytes => IO.lift(() => Missing.Unless(JsonSerializer.Deserialize(bytes, SlotFileContext.Default.SlotFile), nameof(JsonSerializer.Deserialize))))
            .Map(static file => Some(file.State))
            .Catch(static error => error.HasException<FileNotFoundException>(), static _ => IO.pure(Option<SlotState>.None))
            .Catch(static error => error.HasException<JsonException>(), error => IO.fail<Option<SlotState>>(new MalformedSidecar(sidecar, error)));

    private IO<RenderSlot> Recorded(ImageFileEventArgs args, Option<ValueSet> settings) =>
        from saved in IO.lift(() => File.GetLastWriteTimeUtc(args.FileName).ToInstant())
        from header in RenderSlot.Header(args.FileName)
        let state = new SlotState(args.SessionId, args.RenderEngine, args.RenderEngineId, Duration.FromSeconds(args.EllapsedTime),
            settings, None)
        from recorded in Recorded(new RenderSlot(args.FileName, saved, header.Extent, header.Grading, header.Chain, None), state)
        select recorded;

    private IO<RenderSlot> Recorded(RenderSlot slot, SlotState state) =>
        from path in IO.lift(Conversions.Validated<OutputPath, string, InvalidOutput>(Sidecar(slot.Rendering)))
        from written in FilePublication.Write(path, overwrite: true, stream => IO.liftAsync(async environment => {
            await JsonSerializer.SerializeAsync(stream, SlotFile.Of(state), SlotFileContext.Default.SlotFile, environment.Token).ConfigureAwait(false);
            return unit;
        }))
        let noted = slot with { State = Some(state) }
        from listed in held.SwapIO(ledger => ledger with { Slots = ledger.Slots.AddOrUpdate(noted.Rendering, noted) })
        select noted;

    private string Sidecar(string rendering) => Path.ChangeExtension(rendering, suffix);

    // --- [BROWSER]
    public static ControlRow Browser { get; } =
        ListRows.Browser(
            RowText.Localize("Render History").Local,
            RowText.Localize("Completed renders of this plug-in, newest first").Local,
            new BrowserRow<RenderSlot, string>(
                Key: static slot => slot.Rendering,
                Label: Label,
                Tile: static (scope, view, slot, size, retain) =>
                    from tiles in IPlugInRendering.Served(((IPlugInRendering)scope.Sink).Tiles, nameof(IPlugInRendering.Tiles))
                    from thumbnail in tiles.Thumbnail(view, TileRequest.Recorded(slot, ((IPlugInRendering)scope.Sink).Effects), size, retain)
                    select thumbnail,
                Items: static scope => new ValueStore<Seq<RenderSlot>>(
                    HistoryOf(scope).Bind(static history => history.Slots).Map(static slots => Some(slots)),
                    items => items.Match(
                        Some: named => HistoryOf(scope).Bind(history => history.Noted(named)),
                        None: static () => IO.pure(Change<Seq<RenderSlot>>.None)),
                    EqualityComparer<Seq<RenderSlot>>.Default.Equals,
                    Applied.Live,
                    None),
                Current: static scope =>
                    ComparisonOf(scope).Bind(static comparison => comparison.Held).Map(static side => side.Map(static held => held.Slot.Rendering)),
                Choose: static (scope, rendering) =>
                    from history in HistoryOf(scope)
                    from slots in history.Slots
                    from slot in IO.lift(slots.Find(slot => string.Equals(slot.Rendering, rendering, StringComparison.Ordinal)).ToFin(new Missing(nameof(Slots))))
                    from chosen in Compared(scope, slot)
                    select chosen,
                Mode: BrowserMode.List,
                Extent: new ReadoutSource<PixelExtent>.Model(Framing.Extent),
                Empty: RowText.Localize("Completed renders appear here, newest first").Local) {
                Numbered = true,
                Search = Some(new ListSearch<RenderSlot>(RowText.Localize("Search history").Local, Label, None)),
                Rename = Some<Func<RenderSlot, string, Fin<RenderSlot>>>(static (slot, text) =>
                    slot.State.ToFin(new Missing(nameof(Annotate))).Map(state => slot with { State = Some(state with { Note = Conversions.Present(text) }) })),
                Reloads = Seq(Changed),
                Commands = static selection =>
                    Seq<CommandRow>(
                        Command("history-compare", RowText.Localize("Compare").Local,
                            RowText.Localize("Compare the selected slot against the current frame through its own grading").Local, Headed(selection, Compared)),
                        Command("history-compare-before", RowText.Localize("Compare Before").Local,
                            RowText.Localize("Compare the selected slot's frame before its chain against the current frame").Local, Headed(selection, Before)),
                        Command("history-export", RowText.Localize("Export").Local,
                            RowText.Localize("Write the selected history frame through the output targets and its recorded grading").Local,
                            Headed(selection, static (scope, slot) =>
                                from document in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
                                from frame in slot.Frame
                                from book in TargetRows.Book(document)
                                from origin in FileOrigin.At(scope.Sink)
                                let request = TileRequest.Recorded(slot, ((IPlugInRendering)scope.Sink).Effects)
                                from written in Saves.Write(document, origin, book,
                                    Right<(Guid Rendering, RenderRun Run), (PixelFrame Scene, TileRequest Request)>((frame, request)))
                                select unit)))
                    + toSeq(SlotPart.Items).Map<CommandRow>(part => Restoring(selection, part))
                    + Seq<CommandRow>(
                        Command("history-delete", RowText.Localize("Delete").Local, RowText.Localize("Delete the selected slots' renderings and their state").Local,
                            scope =>
                                from history in HistoryOf(scope)
                                from chosen in selection
                                from deleted in chosen.TraverseM(slot => history.Delete(slot)).As()
                                select unit),
                        Command("history-clear-comparison", RowText.Localize("Clear Comparison").Local, RowText.Localize("Drop the slot the frame view compares against").Local,
                            static scope => ComparisonOf(scope).Bind(static comparison => comparison.Clear))),
                Wording = Wording.Localized,
            },
            new RowRules(None, None));

    private IO<Change<Seq<RenderSlot>>> Noted(Seq<RenderSlot> items) =>
        from ledger in held.ValueIO
        from keep in IO.lift(SupportOptions.AutoSaveKeepAmount)
        let before = ledger.Listed(keep)
        from noted in ledger.Renamed(items).TraverseM(item => Annotate(item.Slot, item.Note)).As()
        let written = toHashMap(noted.Map(static slot => (slot.Rendering, slot)))
        select ValueStore.Between(Some(before), Some(before.Map(slot => written.Find(slot.Rendering).IfNone(slot))));

    private static IO<RenderHistory> HistoryOf(RowScope scope) =>
        IPlugInRendering.Served(((IPlugInRendering)scope.Sink).History, nameof(IPlugInRendering.History));

    private static IO<SlotComparison> ComparisonOf(RowScope scope) =>
        IPlugInRendering.Served(((IPlugInRendering)scope.Sink).Comparison, nameof(IPlugInRendering.Comparison));

    private static IO<Unit> Compared(RowScope scope, RenderSlot slot) =>
        ComparisonOf(scope).Bind(comparison => comparison.Choose(new CompareSide.Recorded(slot), Label(scope, slot)));

    private static IO<Unit> Before(RowScope scope, RenderSlot slot) =>
        ComparisonOf(scope).Bind(comparison => comparison.Choose(new CompareSide.Before(slot), RowText.Localize("Before").Local));

    private static string Label(RowScope scope, RenderSlot slot) =>
        slot.Note.IfNone(() =>
            slot.Saved.InZone(BclDateTimeZone.FromTimeZoneInfo(((IPlugInViews)scope.Sink).Clock.LocalTimeZone)).LocalDateTime.ToString("g", RowText.Culture));

    private static Func<RowScope, IO<Unit>> Headed(IO<Seq<RenderSlot>> selection, Func<RowScope, RenderSlot, IO<Unit>> act) =>
        scope => selection.Bind(chosen => chosen.Head.Traverse(slot => act(scope, slot)).As()).Map(static _ => unit);

    private static CommandRow.Run Restoring(IO<Seq<RenderSlot>> selection, SlotPart part) =>
        part.Map(settings: RowText.Localize("Render Settings").Local, grading: RowText.Localize("Grading").Local) switch {
            var caption => Command(
                part.Map(settings: "history-restore-settings", grading: "history-restore-grading"),
                RowText.Localize("Restore {0}", arguments: [caption]).Local,
                RowText.Localize("Write the selected slot's {0} into the document as one commit", arguments: [caption]).Local,
                Headed(selection, (scope, slot) =>
                    from doc in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
                    from history in HistoryOf(scope)
                    from restored in history.Restore(doc, slot, part)
                    select unit)),
        };

    private static CommandRow.Run Command(string name, string caption, string help, Func<RowScope, IO<Unit>> execute) =>
        new(new CommandFace(name, caption, help, None, None) { Wording = Wording.Localized }, None, execute);
}
