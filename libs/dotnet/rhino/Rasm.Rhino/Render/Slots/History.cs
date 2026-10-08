using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.Win32.SafeHandles;
using NodaTime.TimeZones;
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
    Guid Session, string Engine, Guid EngineId, Duration Elapsed, ValueSet Grading, Seq<Guid> Chain, Option<ValueSet> Settings, Option<string> Note) {
    public ContentKey Key => ContentKey.Of(KeyDomain.HistorySlot, stream => stream.Id(Session));

    public Seq<EffectKind> Running(Seq<EffectKind> kinds) => Chain.Choose(id => kinds.Find(kind => kind.Id == id));
}

[SmartEnum]
public sealed partial class SlotPart {
    public static readonly SlotPart Settings = new(static state => state.Settings);
    public static readonly SlotPart Grading = new(static state => Some(state.Grading));

    [UseDelegateFromConstructor]
    public partial Option<ValueSet> Of(SlotState state);
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
}

public sealed record RenderSlot(string Rendering, Instant Saved, PixelExtent Extent, Option<SlotState> State) {
    public static IO<PixelExtent> Header(string rendering) => Read(rendering, (_, index) => Measured(index.Xml, rendering));

    public IO<PixelFrame> Frame => Read(Rendering, Filled);

    public Option<string> Note => State.Bind(static state => state.Note);

    private static IO<A> Read<A>(string rendering, Func<SafeFileHandle, (XElement Xml, long Body), Fin<A>> body) =>
        use(() => File.OpenHandle(rendering, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.RandomAccess))
            .Bind(file => IO.lift(() => Indexed(file, rendering).Bind(index => body(file, index))))
            .Bracket();

    private static Fin<(XElement Xml, long Body)> Indexed(SafeFileHandle file, string rendering) {
        const int Version = 2;
        int start = RimagePart.Start.Key.Length;
        Span<byte> lead = stackalloc byte[start + (2 * sizeof(int))];
        int read = RandomAccess.Read(file, lead, 0L);
        int length = BinaryPrimitives.ReadInt32LittleEndian(lead[^sizeof(int)..]);
        if (read != lead.Length || !Ascii.Equals(lead[..start], RimagePart.Start.Key) || BinaryPrimitives.ReadInt32LittleEndian(lead[start..]) != Version || length < 0)
            return new MalformedRendering(rendering, RimagePart.Start);
        byte[] header = new byte[length + sizeof(char) + RimagePart.Body.Key.Length];
        return RandomAccess.Read(file, header, lead.Length) == header.Length && Ascii.Equals(header.AsSpan(length + sizeof(char)), RimagePart.Body.Key)
            ? Fin.Succ((Xml: XElement.Parse(Encoding.Unicode.GetString(header, 0, length)), Body: (long)lead.Length + header.Length))
            : new MalformedRendering(rendering, RimagePart.Body);
    }

    private static Fin<PixelExtent> Measured(XElement xml, string rendering) =>
        (Child(xml, RimagePart.Width, parseInt).ToValidation<Error>(new MalformedRendering(rendering, RimagePart.Width)),
         Child(xml, RimagePart.Height, parseInt).ToValidation<Error>(new MalformedRendering(rendering, RimagePart.Height)))
            .Apply(static (width, height) => (Width: width, Height: height))
            .As()
            .ToFin()
            .Bind(static size => RenderWindows.Extent(size.Width, size.Height));

    private Fin<PixelFrame> Filled(SafeFileHandle file, (XElement Xml, long Body) index) =>
        from listed in Optional(index.Xml.Element(RimagePart.Channels.Key))
            .Bind(static channels => toSeq(channels.Elements(RimagePart.Channel.Key))
                .Traverse(static channel =>
                    from id in Child(channel, RimagePart.Uuid, parseGuid)
                    from start in Child(channel, RimagePart.Offset, parseLong)
                    select (Id: id, Start: start))
                .As())
            .ToFin(new MalformedRendering(Rendering, RimagePart.Channels))
        let offsets = toHashMap(listed)
        from starts in Seq(RenderWindow.StandardChannels.Red, RenderWindow.StandardChannels.Green, RenderWindow.StandardChannels.Blue, RenderWindow.StandardChannels.Alpha)
            .Map(channel => offsets.Find(RenderWindow.ChannelId(channel)).ToValidation<Error>(new MissingPlane(Rendering, channel)))
            .Traverse(static plane => plane)
            .As()
            .ToFin()
        let planes = starts.Map(start => index.Body + start + RimagePart.Chan.Key.Length + Unsafe.SizeOf<Guid>()).Strict()
        let length = RandomAccess.GetLength(file)
        from bounded in guard<Error>(planes.ForAll(plane => plane + ((long)sizeof(float) * Extent.Width * Extent.Height) <= length), new MalformedRendering(Rendering, RimagePart.Chan))
        select new PixelFrame(System.Drawing.Point.Empty, Extent, Extent, block => Fill(file, planes, block));

    private void Fill(SafeFileHandle file, Seq<long> planes, float[] block) {
        Span<byte> row = new byte[sizeof(float) * Extent.Width];
        for (int line = 0; line < planes.Count * Extent.Height; line++) {
            (int lane, int y) = Math.DivRem(line, Extent.Height);
            _ = RandomAccess.Read(file, row, planes[lane] + ((long)y * row.Length));
            for (int x = 0; x < Extent.Width; x++)
                block[(planes.Count * ((y * Extent.Width) + x)) + lane] = BinaryPrimitives.ReadSingleLittleEndian(row[(sizeof(float) * x)..]);
        }
    }

    private static Option<A> Child<A>(XElement parent, RimagePart part, Func<string, Option<A>> parse) =>
        Optional(parent.Element(part.Key)).Bind(element => parse(element.Value));
}

internal sealed record SlotFile(Guid Session, string Engine, Guid EngineId, double ElapsedSeconds, ValueSet Grading, Guid[] Chain, ValueSet? Settings, string? Note) {
    public SlotState State =>
        new(Session, Engine, EngineId, Duration.FromSeconds(ElapsedSeconds), Grading, toSeq(Chain), Optional(Settings), Optional(Note));

    public static SlotFile Of(SlotState state) =>
        new(state.Session, state.Engine, state.EngineId, state.Elapsed.TotalSeconds, state.Grading, [.. state.Chain], state.Settings.ValueUnsafe(), state.Note.ValueUnsafe());
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
    private readonly Atom<Ledger> held = Atom(Ledger.Empty);

    private RenderHistory(IPlugInRendering plugIn, string suffix, View.Section section, ViewStore store) =>
        (this.plugIn, this.suffix, this.section, this.store) = (plugIn, suffix, section, store);

    private sealed record Ledger(
        Option<string> Folder, Option<string> Claimed, HashMap<string, RenderSlot> Slots, Option<(RhinoDoc Doc, ValueSet Settings)> Pending, Option<(RhinoDoc Doc, ValueSet Settings)> Taken) {
        public static Ledger Empty { get; } = new(None, None, HashMap<string, RenderSlot>(), None, None);

        public Seq<RenderSlot> Renamed(Seq<RenderSlot> items) => items.Filter(item => Slots.Find(item.Rendering).Exists(stored => stored.Note != item.Note));
    }

    // --- [ROWS]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(View.Section settings, Func<RenderHistory, Seq<Func<PlugIn, IPlugInSink, IO<IDisposable>>>> readers) =>
        (plugIn, sink) =>
            from store in IO.lift(settings.Store.ToFin(new Missing(nameof(View.Store))))
            from history in IO.lift(() => new RenderHistory((IPlugInRendering)sink, $".{plugIn.Id:N}.json", settings, store))
            let cell = history.plugIn.History
            from acquired in DisposalOps.AcquireAll(
                Seq(
                        EventKind.ImageFileSaved.Inline(history.Saved, sink),
                        EventKind.ImageFileLoaded.Inline(args => history.Listed(args.FileName), sink),
                        EventKind.ImageFileDeleted.Inline(history.Deleted, sink))
                    .Concat(readers(history).Map(row => row(plugIn, sink)))
                    .Add(cell.SwapIO(_ => Some(history)).Map(_ => (IDisposable)new Disposal<Atom<Option<RenderHistory>>>(cell, static filled => filled.Swap(static _ => None)))),
                DisposalOps.Release)
            select DisposalOps.Composite(acquired, new CallbackSite(sink, typeof(RenderHistory), nameof(Register)));

    // --- [BINDINGS]
    public Validation<Error, BindingGroup> Grading(RhinoDoc doc) =>
        BindingGroup.Of(plugIn.Effects.Map(kind => kind.Binding(doc)));

    public Validation<Error, BindingGroup> Bindings(RhinoDoc doc) =>
        (SceneSources.Bindings(doc), Grading(doc)).Apply(static (scene, grading) => scene.Bindings + grading.Bindings).As().Bind(BindingGroup.Of);

    public Fin<HistoryBinding> Settings(RhinoDoc doc) =>
        Bindings(doc).ToFin().Bind(bound => ViewOps.History((IPlugInViews)plugIn, section, store, bound, Some(doc)));

    // --- [READS]
    public IO<Seq<RenderSlot>> Slots =>
        from ledger in held.ValueIO
        from keep in IO.lift(SupportOptions.AutoSaveKeepAmount)
        select toSeq(ledger.Slots.Values.OrderByDescending(static slot => slot.Saved).ThenBy(static slot => slot.Rendering, StringComparer.Ordinal)).Take(keep);

    // --- [WRITES]
    public IO<T> Launched<T>(RhinoDoc doc, IO<T> render) =>
        from bound in IO.lift(Bindings(doc).ToFin())
        from captured in bound.Capture
        from pending in held.SwapIO(ledger => ledger with { Pending = Some((doc, captured)) })
        from rendered in IO.pure(unit).Bind(_ => render).Finally(held.SwapIO(static ledger => ledger with { Pending = None }))
        select rendered;

    public IO<ValueDiff> Restore(RhinoDoc doc, RenderSlot slot, SlotPart part) =>
        from restored in IO.lift((from state in slot.State from set in part.Of(state) select (state.Chain, Set: set)).ToFin(new Missing(nameof(Restore))))
        from binding in IO.lift(Settings(doc))
        from locks in ViewOps.Locks(section, Some(doc)).Match(
            Some: static locked => locked.Read,
            None: static () => IO.pure(Option<LanguageExt.HashSet<EntryKey>>.None))
        from diff in part.Switch(
            (Binding: binding, Doc: doc, Restored: restored, Locks: locks.IfNone(LanguageExt.HashSet.empty<EntryKey>())),
            settings: static restore => restore.Binding.Commit(restore.Restored.Set, restore.Locks),
            grading: static restore => Commits.Commit(restore.Doc, restore.Binding.Caption, new RedrawPolicy.Silent(),
                    from chained in Rechained(restore.Doc, restore.Restored.Chain)
                    from written in restore.Binding.Commit(restore.Restored.Set, restore.Locks)
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

    private static IO<Seq<EffectEntry>> Rechained(RhinoDoc doc, Seq<Guid> chain) =>
        SceneSources.Edit(doc, window =>
            from entries in EffectRegistry.Apply(window.Settings, [])
            from applied in EffectRegistry.Apply(window.Settings, Requests(entries, chain))
            select applied);

    private static Seq<EffectRequest> Requests(Seq<EffectEntry> entries, Seq<Guid> chain) =>
        entries.Choose(entry => entry.Switch(
                chain,
                listed: static (running, listed) => Some<EffectRequest>(new EffectRequest.Listing(listed with { On = running.Exists(id => id == listed.Id) })),
                selectable: static (running, selectable) => running.Exists(id => id == selectable.Id) ? Some<EffectRequest>(new EffectRequest.Selecting(selectable)) : None))
            .Add(new EffectRequest.Ordering(chain.Choose(id => entries.Find(entry => entry.Id == id && entry.Switch(listed: static _ => true, selectable: static _ => false)))));

    // --- [EVENTS]
    private IO<Unit> Saved(ImageFileEventArgs args) =>
        from listed in Listed(args.FileName)
        from ledger in held.SwapIO(static ledger => ledger with { Pending = None, Taken = ledger.Pending })
        from doc in IO.lift(() => (ledger.Taken.Map(static taken => taken.Doc) || Optional(RhinoDoc.ActiveDoc)).ToFin(new Missing(nameof(RhinoDoc.ActiveDoc))))
        from grading in IO.lift(Grading(doc).ToFin()).Bind(static bound => bound.Capture)
        from entries in SceneSources.Read(doc, static window => EffectRegistry.Apply(window.Settings, []))
        let state = new SlotState(
            args.SessionId, args.RenderEngine, args.RenderEngineId, Duration.FromSeconds(args.EllapsedTime), grading,
            entries.Filter(static entry => entry.Runs).Map(static entry => entry.Id).Strict(), ledger.Taken.Map(static taken => taken.Settings), None)
        from forked in Forked(Slot(args.FileName, None).Bind(slot => Recorded(slot, state)).Map(static _ => unit), nameof(Saved))
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
        work.Catch(error => IO.lift(() => plugIn.Report(error, typeof(RenderHistory), member))).As().Fork().Map(static _ => unit);

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
        from extent in RenderSlot.Header(rendering)
        select new RenderSlot(rendering, saved, extent, state);

    private static IO<Option<SlotState>> Stated(string sidecar) =>
        IO.lift(() => File.ReadAllBytes(sidecar))
            .Bind(static bytes => IO.lift(() => Missing.Unless(JsonSerializer.Deserialize(bytes, SlotFileContext.Default.SlotFile), nameof(JsonSerializer.Deserialize))))
            .Map(static file => Some(file.State))
            .Catch(static error => error.HasException<FileNotFoundException>(), static _ => IO.pure(Option<SlotState>.None))
            .Catch(static error => error.HasException<JsonException>(), error => IO.fail<Option<SlotState>>(new MalformedSidecar(sidecar, error)));

    private IO<RenderSlot> Recorded(RenderSlot slot, SlotState state) =>
        Sidecar(slot.Rendering) switch {
            var sidecar =>
                from staged in IO.lift(() => File.WriteAllBytes($"{sidecar}.tmp", JsonSerializer.SerializeToUtf8Bytes(SlotFile.Of(state), SlotFileContext.Default.SlotFile)))
                from moved in IO.lift(() => File.Move($"{sidecar}.tmp", sidecar, overwrite: true))
                let noted = slot with { State = Some(state) }
                from listed in held.SwapIO(ledger => ledger with { Slots = ledger.Slots.AddOrUpdate(noted.Rendering, noted) })
                select noted,
        };

    private string Sidecar(string rendering) => Path.ChangeExtension(rendering, suffix);

    // --- [BROWSER]
    public static ControlRow Browser { get; } =
        ListRows.Browser(
            RowText.Localize("Render History").Local,
            RowText.Localize("Completed renders of this plug-in, newest first").Local,
            new BrowserRow<RenderSlot, string>(
                Key: static slot => slot.Rendering,
                Label: Label,
                Tile: static (scope, view, slot, size) =>
                    from tiles in IPlugInRendering.Served(((IPlugInRendering)scope.Sink).Tiles, nameof(IPlugInRendering.Tiles))
                    from thumbnail in tiles.Thumbnail(view, TileRequest.Recorded(slot, ((IPlugInRendering)scope.Sink).Effects), size)
                    select thumbnail,
                Items: static scope => ValueStore.Of(
                    HistoryOf(scope).Bind(static history => history.Slots).Map(static slots => Some(slots)),
                    items => HistoryOf(scope).Bind(history => history.Noted(items.IfNone(Seq<RenderSlot>()))),
                    Applied.Live,
                    None),
                Current: static scope => ValueStore.Of(
                    ComparisonOf(scope).Bind(static comparison => comparison.Held).Map(static side => side.Map(static held => held.Slot.Rendering)),
                    key => key.Match(
                        Some: rendering =>
                            from history in HistoryOf(scope)
                            from slots in history.Slots
                            from slot in IO.lift(slots.Find(slot => string.Equals(slot.Rendering, rendering, StringComparison.Ordinal)).ToFin(new Missing(nameof(Slots))))
                            from chosen in Compared(scope, slot)
                            select chosen,
                        None: () => ComparisonOf(scope).Bind(static comparison => comparison.Clear)),
                    Applied.Live,
                    None),
                Mode: BrowserMode.List,
                Extent: new ReadoutSource<PixelExtent>.Model(Framing.Extent),
                Empty: RowText.Localize("Completed renders appear here, newest first").Local) {
                Numbered = true,
                Search = Some(new ListSearch<RenderSlot>(RowText.Localize("Search history").Local, Label, None)),
                Rename = Some<Func<RenderSlot, string, Fin<RenderSlot>>>(static (slot, text) =>
                    slot.State.ToFin(new Missing(nameof(Annotate))).Map(state => slot with { State = Some(state with { Note = Conversions.Present(text) }) })),
                Commands = static selection =>
                    Seq<CommandRow>(
                        Command("history-compare", RowText.Localize("Compare").Local,
                            RowText.Localize("Compare the selected slot against the current frame through its own grading").Local, Headed(selection, Compared)),
                        Command("history-compare-before", RowText.Localize("Compare Before").Local,
                            RowText.Localize("Compare the selected slot's frame before its chain against the current frame").Local, Headed(selection, Before)))
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

    private IO<Unit> Noted(Seq<RenderSlot> items) =>
        from ledger in held.ValueIO
        from noted in ledger.Renamed(items).TraverseM(item => Annotate(item, item.Note)).As()
        select unit;

    private static IO<RenderHistory> HistoryOf(RowScope scope) =>
        IPlugInRendering.Served(((IPlugInRendering)scope.Sink).History, nameof(IPlugInRendering.History));

    private static IO<SlotComparison> ComparisonOf(RowScope scope) =>
        IPlugInRendering.Served(((IPlugInRendering)scope.Sink).Comparison, nameof(IPlugInRendering.Comparison));

    private static IO<Unit> Compared(RowScope scope, RenderSlot slot) =>
        slot.State.Match(
            Some: state => ComparisonOf(scope).Bind(comparison => comparison.Choose(new CompareSide.Recorded(slot, state), Label(scope, slot))),
            None: () => Before(scope, slot));

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
