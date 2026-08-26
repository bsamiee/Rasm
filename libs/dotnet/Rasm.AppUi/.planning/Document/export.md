# [APPUI_DOCUMENT_EXPORT]

Rasm.AppUi document export owns one paginated-output pipeline. MigraDoc composes flow reports, PDFsharp policies harden PDF output, OOXML writers carry XLSX/DOCX, and lcmsNET rows carry print fidelity. This page owns destinations, the direct generated `ExportFault` union, support-bundle contributions, report specifications, PDF hardening, Office output, print transforms, and the per-format configuration form whose admitted request lowers into those arms. Drafting flow reports and diagnostics evidence compose this owner; drafting sheet-PDF remains on capture's vector-print arm.

## [01]-[INDEX]

- [02]-[EXPORT_DESTINATIONS]: One destination union; the `[FaultCase]`/`ExportFault` family; the one delivery-and-seal fold; the support-bundle contributor rows.
- [03]-[FLOW_REPORT]: MigraDoc flow DOM; the one block-to-line fold and its emitter rows; the kernel-issued page geometry; auto-pagination; running bands; placed visuals.
- [04]-[PDF_POLICY]: `PdfExport` — the security union, the `CapabilitySet<PdfTrait>` conformance, signatures over the AppHost secrets lease, AcroForms, the colour-mode row, cross-reference annotations.
- [05]-[OFFICE_ARM]: OOXML part-graph writers carried as a column on the format row — XLSX, DOCX.
- [06]-[PRINT_ARM]: lcmsNET device-CMYK/ICC transforms; intent proving; ink limiting; proofing; K-preservation intents.
- [07]-[SCHEDULED_EXPORT]: Consumer-owned `ScheduleEntry` rows for recurring report delivery under a real re-drive curve.
- [08]-[EXPORT_FORM]: The capability roster, its per-format schema and preflight readout, the run-queue handoff, and the lowering that carries an admitted request into `[03]`/`[04]`/`[05]`.

## [02]-[EXPORT_DESTINATIONS]

- Owner: `VisualDestination` [Union] — the one delivery vocabulary every export arm and the capture vector-print/video arms deliver through; `ExportArm` [SmartEnum] — the target-family roster the export form reads; `ExportFault` — the direct generated `[Union]` with one `[FaultCase]` leaf per export failure; `ExportDelivery` — the one delivery entry and the measured delivery fold that publishes a `VisualArtifact`; `BundleMember` — the classified, content-keyed diagnostic-artifact row; `SupportBundle` — the contribution fold onto the Bundle destination.
- Cases: `VisualDestination` = FilePath · BlobLane · Bundle; `ExportArm` = document · office · print · bundle; `[FaultCase]` = RenderFailed · SignerUnavailable · ProfileInvalid · PartGraphRejected · DeliveryFailed · ContentUnsupported · AnnotationRejected · IntentUnsupported; bundle member rows evidence-journal · hud-samples · gpu-timelines · quality-verdicts · native-assets · proof-digests · collab-ops.
- Entry: `public static IO<string> Deliver(VisualRuntime runtime, VisualDestination destination, ReadOnlyMemory<byte> payload)` — the ONE delivery entry at the widest payload shape; `public static IO<VisualArtifact> Landed(VisualRuntime runtime, ArtifactKind kind, string format, string colour, Option<VisualDestination> destination, IO<ReadOnlyMemory<byte>> produce)` — the one measured deliver-and-publish fold; `SupportBundle.Contribute(VisualRuntime runtime, params ReadOnlySpan<BundleMember> members)` — one modality-polymorphic contribution fold.
- Law: `DeliveryFailed` and `SignerUnavailable` publish `Retriability.Transient`, so a scheduled delivery re-drives on the fault's own declaration and no consumer classifies by message.
- Law: the payload crosses as ONE `ReadOnlyMemory<byte>` from compose to write — both delegate ports on `VisualRuntime` already take memory, so the defensive `ToArray` on the blob and bundle arms copied against a signature that never demanded it.
- Auto: the Bundle arm stages every classified artifact through the runtime `BundleWrite` delegate and publishes each delivered `VisualArtifact` through the AppUi hook dispatch; a delivery or publication refusal stays on the `IO` effect for AppHost `SupportCapture` to recover as its partial manifest row; each `BundleMember` payload arrives already serialized by its owning codec, so assembly is a fold over settled artifact results and no member re-measures.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm (project — `Domain/results`: `FaultBand`, `Fault`, `Custody`, `Retriability`; `Domain/identity`: `ContentHash`; `Parametric/projections`: `MonotonicTimeline`)
- Growth: one destination case extends delivery and breaks the dispatch at compile time; one export target is one row on `[08]`'s format roster, never a second engine; one diagnostic stream is one `BundleMember` factory row; one fault case is one `[FaultCase]` leaf.
- Boundary: this union is the ONE export-destination owner, so a per-arm destination enum is the deleted form. `FilePath` admits only fully qualified targets whose normalized path stays under `ProfileRoots.AppRoot`, `StoreRoot`, or `SupportRoot`, rejects every symlink or junction in the selected root and existing parent chain, opens the unique pending file with create-new semantics before writing, and lands the final rename fail-closed against a parent swap — source and target resolve through one parent path in one rename syscall, the GUID-named pending sibling cannot pre-exist at a redirected parent, and the link-free parent re-walk runs after the write immediately before the rename. Path admission is PURE and refuses on `Fin` naming which segment refused, so the lift carries the OS write alone. Archive assembly and manifest custody are the AppHost support-capture fold's — an AppUi-local zip assembler or second manifest store is the deleted form; `BundleMember.ContentKey` mints each pre-redaction payload identity through kernel `ContentHash.Of` while AppHost `SupportManifest.Entry` carries the post-redaction key, so an inequality names redaction or a cap rather than corruption.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VisualDestination {
    private VisualDestination() { }
    public sealed record FilePath(string AbsolutePath) : VisualDestination;
    public sealed record BlobLane(string ArtifactKey) : VisualDestination;
    public sealed record Bundle(string ArtifactName, DataClassification Classification) : VisualDestination;

    public string Key => Switch(
        filePath: static _ => "file", blobLane: static _ => "blob", bundle: static _ => ExportArm.Bundle.Key);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExportArm {
    public static readonly ExportArm Document = new("document");
    public static readonly ExportArm Office = new("office");
    public static readonly ExportArm Print = new("print");
    public static readonly ExportArm Bundle = new("bundle");
}

// --- [ERRORS] --------------------------------------------------------------------------



[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExportFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Export;
    private ExportFault() { }

    [FaultCase(0)]
    public sealed partial record RenderFailed(string Stage, string Detail) : ExportFault() {
        public override string Message => $"export/render: {Stage} — {Detail}";
    }
    [FaultCase(1)]
    public sealed partial record SignerUnavailable(string Detail) : ExportFault() {
        public override string Message => $"export/signer: {Detail}";
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(2)]
    public sealed partial record ProfileInvalid(string ProfileKey) : ExportFault() {
        public override string Message => $"export/icc: {ProfileKey} does not parse as an ICC profile";
    }
    [FaultCase(3)]
    public sealed partial record PartGraphRejected(string Part, string Detail) : ExportFault() {
        public override string Message => $"export/ooxml: {Part} — {Detail}";
    }
    [FaultCase(4)]
    public sealed partial record DeliveryFailed(string Destination, string Detail) : ExportFault() {
        public override string Message => $"export/deliver: {Destination} — {Detail}";
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(5)]
    public sealed partial record ContentUnsupported(string Format, string Sheet) : ExportFault() {
        public override string Message => $"export/content: {Sheet} has no {Format} materialization";
    }
    [FaultCase(6)]
    public sealed partial record AnnotationRejected(string Kind, string Detail) : ExportFault() {
        public override string Message => $"export/annotation: {Kind} — {Detail}";
    }
    [FaultCase(7)]
    public sealed partial record IntentUnsupported(string Role, string Intent) : ExportFault() {
        public override string Message => $"export/intent: the {Role} profile carries no {Intent} pipeline";
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ExportDelivery {
    static readonly Op Write = Op.Of(name: "appui.export.deliver");

    public static IO<string> Deliver(VisualRuntime runtime, VisualDestination destination, ReadOnlyMemory<byte> payload) =>
        destination.Switch(
            state: (runtime, payload),
            filePath: static (ctx, file) => AtomicFile(ctx.runtime.Roots, file.AbsolutePath, ctx.payload),
            blobLane: static (ctx, blob) => ctx.runtime.BlobWrite(blob.ArtifactKey, ctx.payload),
            bundle: static (ctx, bundle) => ctx.runtime.BundleWrite(bundle.ArtifactName, bundle.Classification, ctx.payload));

    public static IO<VisualArtifact> Landed(
        VisualRuntime runtime, ArtifactKind kind, string format, string colour,
        Option<VisualDestination> destination, IO<ReadOnlyMemory<byte>> produce) =>
        from start in Marked(runtime)
        from payload in produce
        from landed in destination.Match(
            Some: target => Deliver(runtime, target, payload).Map(Some),
            None: static () => IO.pure(Option<string>.None))
        from end in Marked(runtime)
        from elapsed in Spanned(runtime, start, end)
        let artifact = VisualArtifact.Of(
            kind, format, payload.Span, None, None, elapsed, runtime.Correlation, landed, colour)
        from published in runtime.Publish(artifact)
        select published;

    static IO<MonotonicStamp> Marked(VisualRuntime runtime) => IO.lift<MonotonicStamp>(() => runtime.Line.Capture(Write));

    static IO<Duration> Spanned(VisualRuntime runtime, MonotonicStamp start, MonotonicStamp end) =>
        IO.lift<Duration>(() => runtime.Line.Elapsed(start, end, Write).Map(Duration.FromTimeSpan));

    static IO<string> AtomicFile(ProfileRoots roots, string destination, ReadOnlyMemory<byte> payload) =>
        Admitted(roots, destination).Match(
            Succ: seat => IO.lift<string>(() => Written(seat, payload)),
            Fail: IO.fail<string>);

    static Fin<string> Written((string Root, string Directory, string Target) seat, ReadOnlyMemory<byte> payload) {
        string pending = Path.Combine(seat.Directory, $".{Path.GetFileName(seat.Target)}.{Guid.NewGuid():N}.pending");
        return Custody.Bracket(
                acquire: () => File.OpenHandle(pending, FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough),
                project: handle => Write.Catch(() => { RandomAccess.Write(handle, payload.Span, fileOffset: 0L); return Fin.Succ(unit); }),
                key: Write)
            .Bind(_ => LinkFreeChain(seat.Root, seat.Directory))
            .Bind(_ => Write.Catch(() => { File.Move(pending, seat.Target, overwrite: true); return Fin.Succ(seat.Target); }))
            .Rollback(() => Write.Catch(() => { if (File.Exists(pending)) { File.Delete(pending); } return Fin.Succ(unit); }));
    }

    static Fin<(string Root, string Directory, string Target)> Admitted(ProfileRoots roots, string destination) =>
        from qualified in Refusable(destination, Path.IsPathFullyQualified(destination), "destination is not fully qualified")
        let target = Path.GetFullPath(qualified)
        from directory in Refusable(target, Path.GetDirectoryName(target), "destination has no directory")
        from root in Within(roots, target, directory)
        select (root, directory, target);

    static Fin<string> Within(ProfileRoots roots, string target, string directory) =>
        (Seq(roots.AppRoot, roots.SupportRoot) + roots.StoreRoot.ToSeq())
            .Map(Path.GetFullPath)
            .Find(root => Contained(root, directory) && LinkFreeChain(root, directory).IsSucc)
            .ToFin(new ExportFault.DeliveryFailed(target, "destination is outside the configured profile roots"));

    static bool Contained(string root, string directory) =>
        Path.GetRelativePath(root, directory) is var relative
        && relative != ".."
        && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        && !Path.IsPathFullyQualified(relative);

    static Fin<Unit> LinkFreeChain(string root, string directory) =>
        toSeq(Path.GetRelativePath(root, directory).Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries))
            .Fold(Fin.Succ(root), (held, segment) => held.Bind(current =>
                Admits(current) ? Fin.Succ(Path.Join(current, segment)) : Refused(current)))
            .Bind(leaf => Admits(leaf) ? Fin.Succ(unit) : Refused(leaf).Map(static _ => unit));

    static Fin<string> Refused(string segment) =>
        Fin.Fail<string>(new ExportFault.DeliveryFailed(segment, "path segment is absent, a link, or a reparse point"));

    static bool Admits(string path) =>
        new DirectoryInfo(path) is { Exists: true, LinkTarget: null } directory
        && (directory.Attributes & FileAttributes.ReparsePoint) == 0;

    static Fin<string> Refusable(string destination, bool held, string requirement) =>
        held ? Fin.Succ(destination) : Fin.Fail<string>(new ExportFault.DeliveryFailed(destination, requirement));

    static Fin<string> Refusable(string destination, string? value, string requirement) =>
        string.IsNullOrWhiteSpace(value)
            ? Fin.Fail<string>(new ExportFault.DeliveryFailed(destination, requirement))
            : Fin.Succ(value);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record BundleMember(string ArtifactName, DataClassification Classification, ReadOnlyMemory<byte> Payload) {
    public UInt128 ContentKey => ContentHash.Of(Payload.Span);

    public static BundleMember EvidenceJournal(ReadOnlyMemory<byte> journal) => new("evidence-journal.jsonl", DataClassification.Operational, journal);
    public static BundleMember HudSamples(ReadOnlyMemory<byte> samples) => new("hud-samples.jsonl", DataClassification.Operational, samples);
    public static BundleMember GpuTimelines(ReadOnlyMemory<byte> timelines) => new("gpu-timelines.jsonl", DataClassification.Operational, timelines);
    public static BundleMember QualityVerdicts(ReadOnlyMemory<byte> verdicts) => new("quality-verdicts.jsonl", DataClassification.Operational, verdicts);
    public static BundleMember NativeAssets(ReadOnlyMemory<byte> facts) => new("native-assets.jsonl", DataClassification.HostIdentity, facts);
    public static BundleMember ProofDigests(ReadOnlyMemory<byte> digests) => new("proof-digests.jsonl", DataClassification.Operational, digests);
    public static BundleMember CollabOps(ReadOnlyMemory<byte> opWindow) => new("collab-ops.json", DataClassification.UserContent, opWindow);
}

public static class SupportBundle {
    public static IO<Seq<VisualArtifact>> Contribute(VisualRuntime runtime, params ReadOnlySpan<BundleMember> members) =>
        toSeq(members.ToArray()).TraverseM(member => ExportDelivery.Landed(
            runtime, ArtifactKind.Create(ExportArm.Bundle.Key), Path.GetExtension(member.ArtifactName).TrimStart('.'),
            VisualCodec.ColorPolicy.Display.Key,
            Some<VisualDestination>(new VisualDestination.Bundle(member.ArtifactName, member.Classification)),
            IO.pure(member.Payload))).As();
}
```

## [03]-[FLOW_REPORT]

- Owner: `ReportBlock` [Union] — the typed content vocabulary the branch's five report producers compose; `TableBody` — the header-and-rows table payload both `ReportBlock` and `OfficeSheet` carry; `ListStyle` — the list marker row; `BlockLine` [Union] with `LineRole` — the target-neutral line vocabulary ONE block walk produces; `LineEmitter<TNode>` — the per-target emitter row; `BlockLines` — the one polymorphic fold from a report block or an office cell onto lines; `ReportHeading` — the composed heading node the bookmark outline binds to; `ReportTrait` — the report-composition capability axis; `ReportSetup` — the kernel-issued page geometry; `ReportSpec` — the flow-report composition row; `FlowReport` — the one MigraDoc render surface.
- Cases: `ReportBlock` = Heading · Body · List · Callout · Code · Table · PlacedVisual · Figure · Footnote · Section · Rule · PageBreak; `BlockLine` = Text · Grid · Series · Tile · Divider · Split; `LineRole` = heading · body · code · caption · footnote; `ListStyle` = ordered · bulleted; `ReportTrait` = page-numbers · bookmarks.
- Entry: `public static IO<VisualArtifact> Render(VisualRuntime runtime, ReportSpec spec)` — IO effect; `public static Fin<ReportSetup> Issue(SheetSize size, Op? key = null)` on `ReportSetup` — the kernel-issued geometry; `public static Seq<BlockLine> Of(ReportBlock block)` and `public static Seq<BlockLine> Of(OfficeCell cell)` on `BlockLines` — one entrypoint discriminating on input shape.
- Law: page geometry is the kernel `Rasm/Drawing/sheet` owner's — `ReportSetup` carries one admitted `PlotPolicy` and the standard's own binding-aware `SheetMargin`, so a report page states a published size, an orientation ROW, and four published edges rather than a free centimetre scalar drawn on all four sides. An absent setup IS `Option.None`: a record whose every column was absent while still answering `IsInert` carried two absence regimes for one question.
- Law: the block walk happens ONCE. `BlockLines.Of` is the only traversal of `ReportBlock` and `OfficeSheet`, and MigraDoc, XLSX, and DOCX are three `LineEmitter` rows over the produced lines — three full parallel walks of one twelve-case union could disagree about a case, and one of them carried a `default:` arm that turned a new case into a silent drop.
- Law: a series is its OWN line case, so the XLSX emitter writes typed numeric cells; folding chart points into a string table would have lost the numeric cell type the workbook part graph carries.
- Auto: the heading tree becomes the PDF bookmark outline — composition retains each heading's own layout node, the renderer's page walk answers which page that node landed on, and the level ladder nests each bookmark under its nearest shallower ancestor. Every heading-bearing block STATES its own level and an untitled group states no heading at all, so a quoted passage lowered as an untitled callout mints no blank bookmark. Pagination, widow/orphan control, running headers/footers with `PageField`/`NumPagesField`, and cross-page table breaking are the MigraDoc layout engine's; `FormattedDocument` exposes the measured layout so a page count reads from the renderer, never a local cursor fold; placed visuals encode through the capture codec axis and place as MigraDoc `Image` values.
- Output: one `VisualArtifact` of kind document per report, delivered and published through `ExportDelivery.Landed`.
- Packages: PDFsharp-MigraDoc, PDFsharp, SkiaSharp, UnitsNet (`Length.Centimeters`), Rasm (project — `Drawing/sheet`: `SheetSize`, `SheetOrientation`, `SheetMargin`, `SheetFrame`, `PlotPolicy`, `IssuePosture`, `ScaleLadder`; `Domain/validation`: `CapabilitySet`), Rasm.AppHost (project), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: one `ReportBlock` case extends the content vocabulary and breaks `BlockLines.Of` at compile time; one `BlockLine` case breaks all three emitter rows at once; one report-composition posture is one `ReportTrait` row; zero new surface.
- Boundary: the report's colour model is its `PdfExport.Color` row bound onto the renderer document before `RenderDocument`, so a press report and a screen report are one composition under one row value; the MigraDoc flow DOM is the ONE flow-pagination owner — a bespoke page-break fold, a per-format report builder, or a second cursor algebra is the deleted form; typography roles map to MigraDoc styles from the `Theme/typography.md` role rows at composition so a report style never re-mints font literals; drafting's paginated flow reports and the diagnostics report-PDF compose `FlowReport.Render` with their own block seqs, while the drafting sheet-PDF is capture's vector-print arm. The MigraDoc NATIVE chart DOM (`Shapes.Charts.Chart`) is a stated CARVE and stays unreached: a report chart enters as a `PlacedVisual` raster encoded through the capture codec axis, because the chart plane's own grammar — the layered series algebra, the paint resolver, the threshold family, the annotation plane, and the legend split — has no representation in the MigraDoc chart DOM, so routing a report chart through it would mean maintaining a second, weaker chart vocabulary whose output disagreed with the same chart on screen. The carve costs vector text inside a chart and buys one chart authority.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

public sealed record TableBody(Option<Seq<string>> Header, Seq<Seq<string>> Rows) {
    public static TableBody Of(Seq<Seq<string>> rows) => new(None, rows);
    public static TableBody Headed(Seq<string> header, Seq<Seq<string>> rows) => new(Some(header), rows);
    public Seq<Seq<string>> Lines => Header.ToSeq() + Rows;
    public int Width => Lines.Fold(0, static (max, cells) => Math.Max(max, cells.Count));
    public bool IsEmpty => Width == 0;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ListStyle {
    public static readonly ListStyle Ordered = new("ordered", marker: static index => $"{index + 1}.");
    public static readonly ListStyle Bulleted = new("bulleted", marker: static _ => "•");
    [UseDelegateFromConstructor] public partial string Marker(int index);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LineRole {
    public static readonly LineRole Heading = new("heading", style: "Heading", outlines: true, preserve: false);
    public static readonly LineRole Body = new("body", style: "Normal", outlines: false, preserve: false);
    public static readonly LineRole Code = new("code", style: "Normal", outlines: false, preserve: true);
    public static readonly LineRole Caption = new("caption", style: "Normal", outlines: false, preserve: false);
    public static readonly LineRole Footnote = new("footnote", style: "Normal", outlines: false, preserve: false);

    public string Style { get; }
    public bool Outlines { get; }
    public bool Preserve { get; }

    public string StyleAt(int level) => Outlines ? $"{Style}{level}" : Style;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockLine {
    private BlockLine() { }
    public sealed record Text(string Value, LineRole Role, int Level) : BlockLine;
    public sealed record Grid(TableBody Body) : BlockLine;
    public sealed record Series(string Name, Seq<(double X, double Y)> Points) : BlockLine;
    public sealed record Tile(SKImage Image, double WidthCm, Option<string> Caption) : BlockLine;
    public sealed record Divider : BlockLine;
    public sealed record Split : BlockLine;

    public static BlockLine Head(int level, string text) => new Text(text, LineRole.Heading, int.Clamp(level, 1, 6));
    public static Seq<BlockLine> Titled(int level, string title) =>
        string.IsNullOrWhiteSpace(title) ? Seq<BlockLine>() : Seq(Head(level, title));
}

public sealed record LineEmitter<TNode>(
    Func<BlockLine.Text, Seq<TNode>> Text,
    Func<BlockLine.Grid, Seq<TNode>> Grid,
    Func<BlockLine.Series, Seq<TNode>> Series,
    Func<BlockLine.Tile, Seq<TNode>> Tile,
    Func<BlockLine.Divider, Seq<TNode>> Divider,
    Func<BlockLine.Split, Seq<TNode>> Split) {
    public Seq<TNode> Emit(Seq<BlockLine> lines) => lines.Bind(line => line.Switch(
        state: this,
        text: static (emitter, row) => emitter.Text(row),
        grid: static (emitter, row) => emitter.Grid(row),
        series: static (emitter, row) => emitter.Series(row),
        tile: static (emitter, row) => emitter.Tile(row),
        divider: static (emitter, row) => emitter.Divider(row),
        split: static (emitter, row) => emitter.Split(row)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReportBlock {
    private ReportBlock() { }
    public sealed record Heading(int Level, string Text) : ReportBlock;
    public sealed record Body(string Text) : ReportBlock;
    public sealed record List(Seq<string> Items, ListStyle Style) : ReportBlock;
    public sealed record Callout(int HeadingLevel, string Title, Seq<ReportBlock> Blocks) : ReportBlock;
    public sealed record Code(string Language, string Source) : ReportBlock;
    public sealed record Table(TableBody Body) : ReportBlock;
    public sealed record PlacedVisual(SKImage Tile, double WidthCm) : ReportBlock;
    public sealed record Figure(SKImage Tile, double WidthCm, string AltText, Option<string> Caption) : ReportBlock;
    public sealed record Footnote(string Key, string Text) : ReportBlock;
    public sealed record Section(int Level, string Title, Seq<ReportBlock> Blocks) : ReportBlock;
    public sealed record Rule : ReportBlock;
    public sealed record PageBreak : ReportBlock;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReportTrait : ICapability<ReportTrait> {
    public static readonly ReportTrait PageNumbers = new("page-numbers");
    public static readonly ReportTrait Bookmarks = new("bookmarks");
}

public readonly record struct ReportHeading(int Level, string Text, Paragraph Node);

// --- [MODELS] --------------------------------------------------------------------------

public sealed record ReportSetup(PlotPolicy Plot, SheetMargin Margin) {
    public static Fin<ReportSetup> Issue(SheetSize size, Op? key = null) =>
        from plot in PlotPolicy.Issue(size, key)
        from margin in plot.Frame.Margin(size, key)
        select new ReportSetup(plot, margin);

    public (double Width, double Height) Extent {
        get {
            (Length width, Length height) = Plot.Orientation.Extent(Plot.Size);
            return (width.Centimeters, height.Centimeters);
        }
    }

    public (double Left, double Top, double Right, double Bottom) Edges =>
        (Margin.Left.Centimeters, Margin.Top.Centimeters, Margin.Right.Centimeters, Margin.Bottom.Centimeters);
}

public sealed record ReportSpec(
    string Title,
    Seq<ReportBlock> Blocks,
    Option<string> RunningHeader,
    Option<ReportSetup> Setup,
    PdfExport Pdf,
    VisualDestination Destination,
    CapabilitySet<ReportTrait> Traits);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class BlockLines {
    public static Seq<BlockLine> Of(ReportBlock block) => block.Switch(
        heading: static h => Seq(BlockLine.Head(h.Level, h.Text)),
        body: static b => Seq<BlockLine>(new BlockLine.Text(b.Text, LineRole.Body, 0)),
        list: static l => l.Items.Map((item, index) => (BlockLine)new BlockLine.Text($"{l.Style.Marker(index)} {item}", LineRole.Body, 0)),
        callout: static c => BlockLine.Titled(c.HeadingLevel, c.Title) + c.Blocks.Bind(Of),
        code: static c => Seq<BlockLine>(new BlockLine.Text(c.Source, LineRole.Code, 0)),
        table: static t => Seq<BlockLine>(new BlockLine.Grid(t.Body)),
        placedVisual: static v => Seq<BlockLine>(new BlockLine.Tile(v.Tile, v.WidthCm, None)),
        figure: static f => Seq<BlockLine>(new BlockLine.Tile(f.Tile, f.WidthCm, Some(f.Caption.IfNone(f.AltText)))),
        footnote: static f => Seq<BlockLine>(new BlockLine.Text($"[{f.Key}] {f.Text}", LineRole.Footnote, 0)),
        section: static s => Seq(BlockLine.Head(s.Level, s.Title)) + s.Blocks.Bind(Of),
        rule: static _ => Seq<BlockLine>(new BlockLine.Divider()),
        pageBreak: static _ => Seq<BlockLine>(new BlockLine.Split()));

    public static Seq<BlockLine> Of(OfficeCell cell) =>
        cell.Preface.Map(static text => (BlockLine)new BlockLine.Text(text, LineRole.Caption, 0)).ToSeq()
        + cell.Sheet.Switch(
            table: static t => Seq<BlockLine>(new BlockLine.Grid(t.Body)),
            chart: static c => Seq<BlockLine>(new BlockLine.Series(c.Name, c.Points)),
            image: static _ => Seq<BlockLine>(),
            richText: static r => r.Blocks.Bind(Of));
}

public static class FlowReport {
    static readonly Op Compose = Op.Of(name: "appui.export.report");

    public static IO<VisualArtifact> Render(VisualRuntime runtime, ReportSpec spec) =>
        ExportDelivery.Landed(
            runtime, ArtifactKind.Document, "pdf", spec.Pdf.Color.Key, Some(spec.Destination),
            from rendered in IO.lift<ReadOnlyMemory<byte>>(() => Composed(spec))
            from hardened in PdfHardening.Apply(spec.Pdf, rendered)
            select hardened);

    static Fin<ReadOnlyMemory<byte>> Composed(ReportSpec spec) =>
        Compose.Catch<ReadOnlyMemory<byte>>(() => {
            Document document = new();
            Section section = document.AddSection();
            spec.Setup.Iter(setup => ApplySetup(section.PageSetup, setup));
            spec.RunningHeader.Iter(header => section.Headers.Primary.AddParagraph(header));
            if (spec.Traits.Admits(ReportTrait.PageNumbers)) {
                Paragraph footer = section.Footers.Primary.AddParagraph();
                footer.AddPageField();
                footer.AddText(" / ");
                footer.AddNumPagesField();
            }
            Seq<ReportHeading> headings = Seat(section).Emit(spec.Blocks.Bind(BlockLines.Of));
            PdfDocumentRenderer renderer = new() { Document = document };
            renderer.PdfDocument.Options.ColorMode = spec.Pdf.Color.Mode;
            if (spec.Pdf.Tagged) { _ = UAManager.ForDocument(renderer.PdfDocument); }
            renderer.RenderDocument();
            if (spec.Traits.Admits(ReportTrait.Bookmarks)) { Outlined(renderer, headings); }
            using MemoryStream sink = new();
            renderer.PdfDocument.Save(sink);
            return Fin.Succ<ReadOnlyMemory<byte>>(sink.ToArray());
        });

    static LineEmitter<ReportHeading> Seat(Section section) => new(
        Text: row => row.Role.Outlines
            ? Seq(new ReportHeading(row.Level, row.Value, section.AddParagraph(row.Value, row.Role.StyleAt(row.Level))))
            : Nothing(() => section.AddParagraph(row.Value)),
        Grid: row => Nothing(() => AppendTable(section, row.Body)),
        Series: row => Nothing(() => AppendTable(section, TableBody.Headed(
            Seq(row.Name),
            row.Points.Map(static point => Seq(
                point.X.ToString(CultureInfo.InvariantCulture), point.Y.ToString(CultureInfo.InvariantCulture)))))),
        Tile: row => Nothing(() => {
            AppendVisual(section, row.Image, row.WidthCm);
            row.Caption.Iter(caption => section.AddParagraph(caption));
        }),
        Divider: static _ => Nothing(static () => { }),
        Split: _ => Nothing(() => section.AddPageBreak()));

    static Seq<ReportHeading> Nothing(Action seat) { seat(); return Seq<ReportHeading>(); }
    static Seq<ReportHeading> Nothing(Func<Paragraph> seat) { _ = seat(); return Seq<ReportHeading>(); }

    static void Outlined(PdfDocumentRenderer renderer, Seq<ReportHeading> headings) {
        HashMap<DocumentObject, int> pages = toSeq(Enumerable.Range(1, renderer.PageCount)).Fold(
            HashMap<DocumentObject, int>(),
            (held, page) => toSeq(renderer.DocumentRenderer.GetDocumentObjectsFromPage(page))
                .Fold(held, (inner, node) => inner.ContainsKey(node) ? inner : inner.Add(node, page)));
        Seq<(int Level, PdfOutline Node)> stack = Seq<(int, PdfOutline)>();
        headings.Iter(heading => {
            int page = pages.Find(heading.Node).IfNone(1);
            PdfOutlineCollection parent = stack
                .Filter(row => row.Level < heading.Level)
                .Rev()
                .Head
                .Map(static row => row.Node.Outlines)
                .IfNone(renderer.PdfDocument.Outlines);
            PdfOutline minted = parent.Add(heading.Text, renderer.PdfDocument.Pages[page - 1], opened: heading.Level <= 2);
            stack = stack.Filter(row => row.Level < heading.Level).Add((heading.Level, minted));
        });
    }

    static void AppendTable(Section section, TableBody body) {
        if (body.IsEmpty) { return; }
        MigraDoc.DocumentObjectModel.Tables.Table table = section.AddTable();
        for (var column = 0; column < body.Width; column++) { table.AddColumn(); }
        body.Lines.Map(static (cells, index) => (Cells: cells, Index: index)).Iter(entry => {
            MigraDoc.DocumentObjectModel.Tables.Row row = table.AddRow();
            row.HeadingFormat = body.Header.IsSome && entry.Index == 0;
            entry.Cells.Map(static (value, column) => (Value: value, Column: column))
                .Iter(cell => row.Cells[cell.Column].AddParagraph(cell.Value));
        });
    }

    static void ApplySetup(MigraDoc.DocumentObjectModel.PageSetup setup, ReportSetup policy) {
        (double width, double height) = policy.Extent;
        setup.PageWidth = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(width);
        setup.PageHeight = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(height);
        (double left, double top, double right, double bottom) = policy.Edges;
        setup.LeftMargin = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(left);
        setup.TopMargin = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(top);
        setup.RightMargin = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(right);
        setup.BottomMargin = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(bottom);
        setup.Orientation = policy.Plot.Orientation.Equals(SheetOrientation.Landscape)
            ? MigraDoc.DocumentObjectModel.Orientation.Landscape
            : MigraDoc.DocumentObjectModel.Orientation.Portrait;
    }

    static void AppendVisual(Section section, SKImage tile, double widthCm) {
        using SKData encoded = tile.Encode(SKEncodedImageFormat.Png, 100);
        MigraDoc.DocumentObjectModel.Shapes.Image image = section.AddImage($"base64:{Convert.ToBase64String(encoded.AsSpan())}");
        image.Width = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(widthCm);
    }
}
```

## [04]-[PDF_POLICY]

- Owner: `PdfExport` — the one PDF-hardening policy row this stratum keeps beside the kernel `PlotPolicy` it composes; `ColorTarget` [SmartEnum] — the ONE colour-model row both PDF egress legs read; `PdfSecurity` [Union] with `PdfPermission` and `MetadataPosture` — the encryption posture and its permission set; `PdfIdentity` — the document-information columns; `PdfCredentials` — the half of a hardening policy no form control can set; `PdfAnnotation` [Union] with `PdfAnnotations` — the cross-reference vocabulary and its page decoration; `PdfHardening` — the apply fold over the rendered payload.
- Cases: `ColorTarget` = screen · press · press-deep; `PdfSecurity` = Open | Encrypted; `PdfPermission` = print · extract; `MetadataPosture` = encrypted · clear; `PdfAnnotation` = Link · Destination · Reference — the backend's whole honored-key roster, closed by it rather than by preference.
- Entry: `public static Fin<PdfExport> Of(ColorTarget colour, CapabilitySet<PdfTrait> conformance, PdfSecurity security, PdfCredentials credentials)` — the ONE mint, running the kernel `PdfTrait.Law` over the conformance claim; `public static Fin<PdfExport> Issued(PlotPolicy plot, ColorTarget colour, PdfSecurity security, PdfCredentials credentials)` — the sheet-issued twin whose conformance IS the plot's; `public static IO<ReadOnlyMemory<byte>> Apply(PdfExport policy, ReadOnlyMemory<byte> rendered)` — opens the rendered payload through `PdfReader`, applies the enabled arms, and re-saves; `public static Func<SKCanvas, Fin<Unit>> Decorate(params ReadOnlySpan<PdfAnnotation> rows)` — one page decoration folding straight into the `Render/capture#VECTOR_PRINT` `VisualExportSpec.Pages` seq.
- Law: conformance is the kernel `CapabilitySet<PdfTrait>` and its `CapabilityLaw` — PDF/A-2b, PDF/A-3, and PDF/UA are orthogonal claims one file routinely carries together, the two archival LEVELS are the one illegal corner the law forecloses, and the single `bool TaggedUa` they were spelled as could name only one of three while a row NAMED archival set accessibility and no archival trait at all.
- Law: the ISSUED mint is the only place a sheet-bearing policy takes its conformance, so a report stating a conformance beside a `ReportSetup` that already issued one is the deleted form.
- Auto: the colour model is the `ColorTarget` row alone — `PdfDocumentOptions.ColorMode` takes its `Mode` on the renderer document before content materializes while `[06]-[PRINT_ARM]` takes the same row's `Device`, buffer formats, and pixel strides, so a screen export and a proofed press export differ by this value and never by a second code path; the security arm selects AES-256 through `PdfDocument.SecurityHandler.SetEncryptionToV5(bool encryptMetadata)` and applies permissions through `PdfDocument.SecuritySettings`; identity writes `PdfDocumentInformation`; signatures compose `DigitalSignatureHandler.ForDocument`; AcroForm rows write through the catalogued field surface; the accessibility trait attaches `UAManager` to the renderer document before `RenderDocument`. Annotations ride Skia's own annotation entrypoints on the paged canvas — `DrawUrlAnnotation`, `DrawNamedDestinationAnnotation`, and `DrawLinkDestinationAnnotation` mint the `SKData` the native annotation record retains.
- Packages: PDFsharp, SkiaSharp, Rasm (project — `Drawing/sheet`: `PdfTrait`, `PlotPolicy`; `Domain/validation`: `CapabilitySet`, `CapabilityLaw`), Rasm.AppHost (project), LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new hardening concern is one `PdfExport` column; a new permission is one `PdfPermission` row; a new identity column is one `PdfIdentity` member; a new colour model is one `ColorTarget` row both legs read; a new conformance claim is one kernel `PdfTrait` row; the cross-reference family grows only when the PDF backend honors a fourth key.
- Boundary: the signing-credential crossing is a declared ledger row (`Document/export` -> AppHost `Runtime/secrets.md`), and `PdfCredentials` is the typed carve naming exactly what the `[08]` form cannot set. PDF-UA tagging and the colour model both bind before content materialization — `PdfDocumentOptions.ColorMode` governs how each `XColor` is WRITTEN, so a post-render pass setting it re-saves already-written content streams and governs nothing; the post-render pass therefore applies security, identity, forms, and signatures alone. `ColorTarget` is the ONE colour-model authority for the whole page — a `PdfColorMode` literal at a render site, a second CMYK selector on the print arm, and a `bool cmyk` knob beside a spec are the three deleted forms. Annotations are page-composition content, so they enter through the capture vector-print page fold and never through the post-render `PdfReader` pass; the PDF backend honors exactly three annotation keys and each is reached through the named Skia entrypoint that passes it, so the family is CLOSED at three cases and the raw `DrawAnnotation(rect, key, value)` passthrough is the deleted form — an unhonored key returns void with no diagnostic. Region shape is the backend's own discriminant: a named destination is DEFINED at a point, which is the zero-extent rect the backend requires, while an outbound url and an internal link carry a real rect, so a zero-area region on either rect-bearing case refuses at admission.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColorTarget {
    public static readonly ColorTarget Screen = new(
        "screen", PdfColorMode.Rgb, ColorSpaceSignature.RgbData, Cms.TYPE_RGBA_8, Cms.TYPE_RGBA_8, inputStride: 4, outputStride: 4);
    public static readonly ColorTarget Press = new(
        "press", PdfColorMode.Cmyk, ColorSpaceSignature.CmykData, Cms.TYPE_RGBA_8, Cms.TYPE_CMYK_8, inputStride: 4, outputStride: 4);
    public static readonly ColorTarget PressDeep = new(
        "press-deep", PdfColorMode.Cmyk, ColorSpaceSignature.CmykData, Cms.TYPE_RGBA_8, Cms.TYPE_CMYK_16, inputStride: 4, outputStride: 8);

    public PdfColorMode Mode { get; }
    public ColorSpaceSignature Device { get; }
    public uint Input { get; }
    public uint Output { get; }
    public int InputStride { get; }
    public int OutputStride { get; }

    public uint Channels => Cms.ChannelsOf(Device);

    public Fin<int> Pixels(int rasterBytes) =>
        rasterBytes > 0 && rasterBytes % InputStride == 0
            ? Fin.Succ(rasterBytes / InputStride)
            : Fin.Fail<int>(new ExportFault.RenderFailed("raster", $"{rasterBytes} bytes is not whole {Key} pixels"));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PdfPermission : ICapability<PdfPermission> {
    public static readonly PdfPermission Print = new("print");
    public static readonly PdfPermission Extract = new("extract");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MetadataPosture {
    public static readonly MetadataPosture Encrypted = new("encrypted", encrypts: true);
    public static readonly MetadataPosture Clear = new("clear", encrypts: false);
    public bool Encrypts { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PdfSecurity {
    private PdfSecurity() { }
    public sealed record OpenCase : PdfSecurity;
    public sealed record Encrypted(Option<string> OwnerPasswordLease, CapabilitySet<PdfPermission> Permits, MetadataPosture Metadata) : PdfSecurity;
    public static readonly PdfSecurity Open = new OpenCase();
    public static PdfSecurity Locked(Option<string> lease, params ReadOnlySpan<PdfPermission> permits) =>
        new Encrypted(lease, CapabilitySet<PdfPermission>.Of(permits), MetadataPosture.Encrypted);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record PdfIdentity(Option<string> Title, Option<string> Author, Option<string> Subject, Option<string> Keywords) {
    public static Option<PdfIdentity> Of(
        Option<string> title = default, Option<string> author = default,
        Option<string> subject = default, Option<string> keywords = default) =>
        title.IsNone && author.IsNone && subject.IsNone && keywords.IsNone
            ? None
            : Some(new PdfIdentity(title, author, subject, keywords));
}

public sealed record PdfCredentials(
    Option<PdfIdentity> Identity,
    Option<IDigitalSigner> Signer,
    Option<DigitalSignatureOptions> SignatureOptions,
    Seq<(string Field, string Value)> AcroFields) {
    public static readonly PdfCredentials None = new(Option<PdfIdentity>.None, Option<IDigitalSigner>.None, Option<DigitalSignatureOptions>.None, []);
    public bool IsInert => Identity.IsNone && Signer.IsNone && AcroFields.IsEmpty;
}

public sealed record PdfExport {
    private PdfExport(ColorTarget colour, CapabilitySet<PdfTrait> conformance, PdfSecurity security, PdfCredentials credentials) =>
        (Color, Conformance, Security, Credentials) = (colour, conformance, security, credentials);

    public ColorTarget Color { get; }
    public CapabilitySet<PdfTrait> Conformance { get; }
    public PdfSecurity Security { get; }
    public PdfCredentials Credentials { get; }

    public bool Tagged => Conformance.Admits(PdfTrait.Accessible);
    public bool IsInert => Security is PdfSecurity.OpenCase && Credentials.IsInert;

    public static Fin<PdfExport> Of(ColorTarget colour, CapabilitySet<PdfTrait> conformance, PdfSecurity security, PdfCredentials credentials) =>
        PdfTrait.Law.Admit(conformance).Map(admitted => new PdfExport(colour, admitted, security, credentials));

    public static Fin<PdfExport> Issued(PlotPolicy plot, ColorTarget colour, PdfSecurity security, PdfCredentials credentials) =>
        Of(colour, plot.Conformance, security, credentials);

    public static readonly PdfExport Plain =
        Of(ColorTarget.Screen, CapabilitySet<PdfTrait>.None, PdfSecurity.Open, PdfCredentials.None).ThrowIfFail();
    public static readonly PdfExport Archival =
        Of(ColorTarget.Screen, CapabilitySet<PdfTrait>.Of(PdfTrait.ArchivalA2b, PdfTrait.Accessible), PdfSecurity.Open, PdfCredentials.None).ThrowIfFail();
    public static readonly PdfExport Press =
        Of(ColorTarget.Press, CapabilitySet<PdfTrait>.None, PdfSecurity.Open, PdfCredentials.None).ThrowIfFail();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PdfAnnotation {
    private PdfAnnotation() { }
    public sealed record Link(SKRect Region, string Url) : PdfAnnotation;
    public sealed record Destination(SKPoint At, string Name) : PdfAnnotation;
    public sealed record Reference(SKRect Region, string Name) : PdfAnnotation;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class PdfAnnotations {
    public static Func<SKCanvas, Fin<Unit>> Decorate(params ReadOnlySpan<PdfAnnotation> rows) {
        Seq<PdfAnnotation> admitted = toSeq(rows.ToArray());
        return canvas => admitted.Fold(Fin.Succ(unit), (acc, row) => acc.Bind(_ => Draw(canvas, row)));
    }

    static Fin<Unit> Draw(SKCanvas canvas, PdfAnnotation row) => row.Switch(
        state: canvas,
        link:        static (c, l) => Minted("link", l.Url, Some(l.Region), () => c.DrawUrlAnnotation(l.Region, l.Url)),
        destination: static (c, d) => Minted("destination", d.Name, None, () => c.DrawNamedDestinationAnnotation(d.At, d.Name)),
        reference:   static (c, r) => Minted("reference", r.Name, Some(r.Region), () => c.DrawLinkDestinationAnnotation(r.Region, r.Name)));

    static Fin<Unit> Minted(string kind, string target, Option<SKRect> region, Func<SKData> draw) {
        if (string.IsNullOrWhiteSpace(target)) { return Fin.Fail<Unit>(new ExportFault.AnnotationRejected(kind, "target is blank")); }
        if (region.Exists(static rect => rect.Width <= 0f || rect.Height <= 0f)) {
            return Fin.Fail<Unit>(new ExportFault.AnnotationRejected(kind, "region has no area"));
        }
        using SKData lease = draw();
        return Fin.Succ(unit);
    }
}

public static class PdfHardening {
    static readonly Op Harden = Op.Of(name: "appui.export.pdf");

    public static IO<ReadOnlyMemory<byte>> Apply(PdfExport policy, ReadOnlyMemory<byte> rendered) =>
        policy.IsInert ? IO.pure(rendered) : IO.lift<ReadOnlyMemory<byte>>(() => Modify(policy, rendered));

    static Fin<ReadOnlyMemory<byte>> Modify(PdfExport policy, ReadOnlyMemory<byte> rendered) =>
        Harden.Catch<ReadOnlyMemory<byte>>(() => {
            using MemoryStream source = new(rendered.ToArray());
            using PdfDocument document = PdfReader.Open(source, PdfDocumentOpenMode.Modify);
            policy.Credentials.Identity.Iter(identity => {
                identity.Title.Iter(title => document.Info.Title = title);
                identity.Author.Iter(author => document.Info.Author = author);
                identity.Subject.Iter(subject => document.Info.Subject = subject);
                identity.Keywords.Iter(keywords => document.Info.Keywords = keywords);
            });
            policy.Credentials.AcroFields.Iter(field => Optional(document.AcroForm)
                .Bind(form => Optional(form.Fields[field.Field]))
                .Iter(target => target.Value = new PdfString(field.Value)));
            policy.Security.Switch(
                openCase: static _ => unit,
                encrypted: locked => {
                    document.SecurityHandler.SetEncryptionToV5(locked.Metadata.Encrypts);
                    locked.OwnerPasswordLease.Iter(lease => document.SecuritySettings.OwnerPassword = lease);
                    document.SecuritySettings.PermitPrint = locked.Permits.Admits(PdfPermission.Print);
                    document.SecuritySettings.PermitExtractContent = locked.Permits.Admits(PdfPermission.Extract);
                    return unit;
                });
            using MemoryStream sink = new();
            policy.Credentials.Signer.Iter(signer => _ = DigitalSignatureHandler.ForDocument(
                document, signer, policy.Credentials.SignatureOptions.IfNone(static () => new DigitalSignatureOptions())));
            document.Save(sink);
            return Fin.Succ<ReadOnlyMemory<byte>>(sink.ToArray());
        });
}
```

## [05]-[OFFICE_ARM]

- Owner: `OfficeSheet` [Union] — the content kinds an Office artifact carries; `OfficeFidelity` [SmartEnum] — the per-(format × kind) materialization vocabulary carrying its own materialization column; `OfficeCell` — the admitted sheet and its decided preface; `OfficeWrite` — the part-graph writer a format row carries; `OfficeSpec` — the emit request; `OfficeExport` — the OOXML part-graph arm and the two writer bodies the `[08]` format rows bind.
- Cases: `OfficeSheet` = Table · Chart · Image · RichText; `OfficeFidelity` = native · declared · unsupported.
- Entry: `public static IO<VisualArtifact> Emit(VisualRuntime runtime, OfficeSpec spec)` — the Office IO effect; admission runs the target's own fidelity column over every sheet FIRST, so an `Unsupported` combination folds to `ExportFault.ContentUnsupported` before any part writes and every admitted sheet hands the write its own materialization row.
- Law: the format vocabulary is `[08]`'s `ExportTarget` and this arm declares none of its own — the three OOXML media-type literals stood character-identical on two rosters, and the sibling roster's closed switch is recovered as the target row's own `Arm` column and its `Option<OfficeWrite>`, so a non-office target and a catalogued-but-unwritten one both refuse typed.
- Law: PPTX refuses by ABSENCE — its row carries no writer — so the speculative presentation/master/layout/slide part graph has no arm to sit in and a promotion is one column filled rather than a switch arm rewritten.
- Auto: XLSX writes through `SpreadsheetDocument.Create` and its workbook/worksheet part graph; DOCX writes through `WordprocessingDocument.Create` and its main-document part graph. Both consume the ONE `BlockLines` fold through their own `LineEmitter` row, so the workbook and the word document cannot disagree about what a rich-text block contains.
- Output: one document `VisualArtifact` per emit, delivered and published through `ExportDelivery.Landed`.
- Packages: DocumentFormat.OpenXml, Rasm.AppHost (project), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: one Office target is one `[08]` `ExportTarget` row carrying its fidelity column and its writer; one `OfficeSheet` case admits a content kind and breaks the block fold at compile time; a fidelity promotion is one matrix cell flipped as the verified part members land.
- Boundary: the Office destination is the same `VisualDestination` union. The fidelity row CARRIES its materialization rather than naming it in prose — `Native` cells materialize their own part vocabulary, `Declared` cells preface the projection they state into the produced document, and `Unsupported` cells reject through `ExportFault.ContentUnsupported` — so the matrix cell is the dispatch and a fidelity read as a bare inequality against one row is the deleted form. The workbook part graph carries no font-embedding part, so the spreadsheet writer takes the admitted cells alone: an `EmbeddedFonts` argument threaded there would be a column the format structurally cannot honour, which `[08]`'s preflight already reports as the format's own absent capability.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OfficeSheet {
    private OfficeSheet() { }
    public sealed record Table(string Name, TableBody Body) : OfficeSheet;
    public sealed record Chart(string Name, ChartSeriesKind Kind, Seq<(double X, double Y)> Points) : OfficeSheet;
    public sealed record Image(string Name, SKImage Picture) : OfficeSheet;
    public sealed record RichText(string Name, Seq<ReportBlock> Blocks) : OfficeSheet;

    public string Kind => Switch(
        table: static _ => "table", chart: static _ => "chart", image: static _ => "image", richText: static _ => "richText");

    public string Name => Switch(
        table: static t => t.Name, chart: static c => c.Name, image: static i => i.Name, richText: static r => r.Name);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OfficeFidelity {
    public static readonly OfficeFidelity Native = new(
        "native", materialized: static (_, _) => Fin.Succ(Option<string>.None));
    public static readonly OfficeFidelity Declared = new(
        "declared", materialized: static (format, sheet) => Fin.Succ(Some($"{sheet} projected into {format}")));
    public static readonly OfficeFidelity Unsupported = new(
        "unsupported", materialized: static (format, sheet) => Fin.Fail<Option<string>>(new ExportFault.ContentUnsupported(format, sheet)));

    [UseDelegateFromConstructor]
    public partial Fin<Option<string>> Materialized(string format, string sheet);
}

public delegate Fin<ReadOnlyMemory<byte>> OfficeWrite(OfficeSpec spec, Seq<OfficeCell> cells);

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct OfficeCell(OfficeSheet Sheet, Option<string> Preface);

public sealed record OfficeSpec(
    ExportTarget Target,
    Seq<OfficeSheet> Sheets,
    Seq<(string FontFamily, ReadOnlyMemory<byte> Face)> EmbeddedFonts,
    VisualDestination Destination);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class OfficeExport {
    static readonly Op Part = Op.Of(name: "appui.export.office");

    public static IO<VisualArtifact> Emit(VisualRuntime runtime, OfficeSpec spec) =>
        ExportDelivery.Landed(
            runtime, ArtifactKind.Document, spec.Target.Key, VisualCodec.ColorPolicy.Display.Key, Some(spec.Destination),
            IO.lift<ReadOnlyMemory<byte>>(() => Written(spec)));

    static Fin<ReadOnlyMemory<byte>> Written(OfficeSpec spec) =>
        from cells in Admitted(spec)
        from write in spec.Target.Office.ToFin(new ExportFault.ContentUnsupported(spec.Target.Key, "catalogued part graph"))
        from payload in write(spec, cells)
        select payload;

    static Fin<Seq<OfficeCell>> Admitted(OfficeSpec spec) =>
        spec.Sheets.TraverseM(sheet => spec.Target
            .Materializes(sheet.Kind)
            .Materialized(spec.Target.Key, sheet.Name)
            .Map(preface => new OfficeCell(sheet, preface))).As();

    // --- [WRITERS]

    public static Fin<ReadOnlyMemory<byte>> Xlsx(OfficeSpec spec, Seq<OfficeCell> cells) =>
        Native("xlsx", () => {
            using MemoryStream sink = new();
            using (SpreadsheetDocument doc = SpreadsheetDocument.Create(sink, SpreadsheetDocumentType.Workbook)) {
                WorkbookPart workbook = doc.AddWorkbookPart();
                workbook.Workbook = new Workbook();
                Sheets sheets = workbook.Workbook.AppendChild(new Sheets());
                cells.Map(static (cell, index) => (Cell: cell, Index: index)).Iter(row => {
                    WorksheetPart part = workbook.AddNewPart<WorksheetPart>();
                    SheetData data = new();
                    Cells().Emit(BlockLines.Of(row.Cell)).Iter(cell => data.Append(cell));
                    part.Worksheet = new Worksheet(data);
                    sheets.Append(new Sheet {
                        Id = workbook.GetIdOfPart(part), SheetId = (uint)(row.Index + 1), Name = row.Cell.Sheet.Name,
                    });
                });
                workbook.Workbook.Save();
            }
            return sink.ToArray();
        });

    public static Fin<ReadOnlyMemory<byte>> Docx(OfficeSpec spec, Seq<OfficeCell> cells) =>
        Native("docx", () => {
            using MemoryStream sink = new();
            using (WordprocessingDocument doc = WordprocessingDocument.Create(sink, WordprocessingDocumentType.Document)) {
                MainDocumentPart main = doc.AddMainDocumentPart();
                Body body = new();
                cells.Iter(cell => Blocks().Emit(BlockLines.Of(cell)).Iter(paragraph => body.Append(paragraph)));
                main.Document = new Document(body);
                EmbedFonts(main, spec.EmbeddedFonts);
                main.Document.Save();
            }
            return sink.ToArray();
        });

    static Fin<ReadOnlyMemory<byte>> Native(string part, Func<byte[]> write) =>
        Part.Catch<ReadOnlyMemory<byte>>(() => Fin.Succ<ReadOnlyMemory<byte>>(write()));

    // --- [EMITTERS]

    static LineEmitter<Row> Cells() => new(
        Text: static row => Seq(TextRow(row.Value)),
        Grid: static row => row.Body.Lines.Map(CellsRow),
        Series: static row => TextRow(row.Name).Cons(row.Points.Map(PointRow)),
        Tile: static row => row.Caption.Map(TextRow).ToSeq(),
        Divider: static _ => Seq<Row>(),
        Split: static _ => Seq<Row>());

    static LineEmitter<Paragraph> Blocks() => new(
        Text: static row => Seq(Runs(row.Value, row.Role.Preserve)),
        Grid: static row => row.Body.Lines.Map(static cells => Runs(string.Join('\t', cells), preserve: false)),
        Series: static row => Runs(row.Name, preserve: false).Cons(row.Points.Map(static point =>
            Runs(string.Create(CultureInfo.InvariantCulture, $"{point.X}\t{point.Y}"), preserve: false))),
        Tile: static row => row.Caption.Map(static caption => Runs(caption, preserve: false)).ToSeq(),
        Divider: static _ => Seq<Paragraph>(),
        Split: static _ => Seq<Paragraph>());

    static Paragraph Runs(string value, bool preserve) =>
        new(new Run(preserve ? new Text(value) { Space = SpaceProcessingModeValues.Preserve } : new Text(value)));

    static Row CellsRow(Seq<string> cells) {
        Row row = new();
        cells.Iter(value => row.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue(value) }));
        return row;
    }

    static Row PointRow((double X, double Y) point) {
        Row row = new();
        row.Append(new Cell { DataType = CellValues.Number, CellValue = new CellValue(point.X) });
        row.Append(new Cell { DataType = CellValues.Number, CellValue = new CellValue(point.Y) });
        return row;
    }

    static Row TextRow(string value) {
        Row row = new();
        row.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue(value) });
        return row;
    }

    static void EmbedFonts(MainDocumentPart main, Seq<(string FontFamily, ReadOnlyMemory<byte> Face)> fonts) =>
        fonts.Iter(font => {
            FontTablePart table = main.FontTablePart ?? main.AddNewPart<FontTablePart>();
            FontPart part = table.AddFontPart("application/x-font-ttf");
            using MemoryStream source = new(font.Face.ToArray());
            part.FeedData(source);
        });
}
```

## [06]-[PRINT_ARM]

- Owner: `PrintIntent` [SmartEnum] — the rendering-intent policy rows; `PressCeiling` [ValueObject] — the admitted total-area-coverage ceiling; `GamutAlarm` [ComplexValueObject] — the per-channel out-of-gamut marking colour; `PrintTransform` — the lcmsNET transform row; `PrintProof` — the typed measured result; `PrintLink` — one chain link with its own policy columns; `PrintPlate` — the converted pixels beside their proof; `PrintArm` — the device-CMYK conversion surface.
- Cases: `PrintIntent` = perceptual · relative-colorimetric · saturation · absolute-colorimetric · relative-bpc · preserve-k — K preservation, black-point compensation, and adaptation state are policy columns, never flags scattered at call sites.
- Entry: `public static IO<PrintPlate> Convert(VisualRuntime runtime, PrintTransform row, ReadOnlyMemory<byte> raster)` — IO effect; one `Context`, one accumulating proving fold, one extended `Transform.Create`, one `DoTransform` per payload.
- Law: every native handle is acquired through ONE bracket door — the acquisition's own typed refusal, then the kernel `Custody.Bracket` release — so acquisition order IS release order, the context releases last because every profile opened on it borrows its scope, and the mutable `stage` string that named the crossing in flight and then DISPATCHED the fault off its own value has no spelling left.
- Law: proving accumulates. Three intent roles and two alarm gates report through ONE `Validation`, so a chain refused on two counts names both instead of hiding the second behind whichever ran first.
- Auto: `PrintArm` opens every profile on its own lcmsNET `Context` and writes the `GamutAlarm` codes to `Context.AlarmCodes` — the per-context instance property, never the `Cms.AlarmCodes` process-global twin — and the alarm width is READ back off the context vector lcms itself sizes. Proving runs BEFORE the build: each profile answers `IsIntentSupported` in the direction the chain uses it and an unsupported intent is `ExportFault.IntentUnsupported`, never a silent fallback; `Profile.TotalAreaCoverage` measures the destination's own coverage and a measurement above the admitted `PressCeiling` mints `Profile.CreateInkLimitingDeviceLink` on the arm's context as the chain's tail link; `DetectDestinationBlackPoint` decides whether the intent row's declared black-point compensation changes anything. One extended `Transform.Create(Context, Profile[], bool[], Intent[], double[], Profile, int, uint, uint, CmsFlags)` builds every case, so limited, unlimited, proofed, and plain conversions are one code path. Soft proofing is CHAIN SEATING, never a flag over a gamut operand: the press profile enters the chain TWICE — into-press under the document intent, back out under `Intent.RelativeColorimetric` — and the destination link renders that simulation under `ProofIntent`, admitted by `CmsFlags.SoftProofing`. The gamut operand is read only under `CmsFlags.GamutCheck`, where it builds a SEPARATE alarm lookup overwriting out-of-gamut pixels with the `GamutAlarm` codes. Native lcms2 ships with the app.
- Output: one `PrintPlate` carrying converted pixels and the `PrintProof` measured by the transform; the proof carries coverage, the admitted ceiling, the ink-limit verdict, the detected destination black point, and the resolved flag set, with an undetectable black point represented by `None`.
- Packages: lcmsNET, Rasm (project — `Domain/results`: `Custody`, `Op`), Rasm.AppHost (project), LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new intent is one `PrintIntent` row; a new device profile is one `PrintTransform` value from profile bytes; a new buffer depth is one `ColorTarget` row; a new chain stage is one `PrintLink` row the four build vectors project from.
- Boundary: lcmsNET owns device-CMYK/ICC transforms at the print boundary ONLY — Unicolour stays the suite color-model kernel and `VisualCodec.ColorPolicy` stays the capture codec gamut family, three disjoint charters; an unparseable profile folds to `ExportFault.ProfileInvalid`, never a silent sRGB fallback; buffer formats and pixel strides are the `[04]` `ColorTarget` row's columns, so a `Cms.TYPE_*` literal at this site and a `rgba.Length / 4` pixel count are the two deleted forms the 16-bit lane made wrong; the press simulation lives in the CHAIN and the gamut operand checks alone, so handing the proofing profile to the gamut slot as the simulation is the deleted form — it drops the preview under anything but `CmsFlags.GamutCheck`; per-link BPC, intent, and adaptation are columns of the same `PrintLink` row so the four positional vectors project from one ordered set; the admitted ceiling reads through its value-object `Value`, so no raw cast leaves the typed magnitude at the native edge.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PrintIntent {
    public static readonly PrintIntent Perceptual  = new("perceptual", Intent.Perceptual, blackPoint: false, adaptation: 1d);
    public static readonly PrintIntent Relative    = new("relative-colorimetric", Intent.RelativeColorimetric, blackPoint: false, adaptation: 1d);
    public static readonly PrintIntent Saturation  = new("saturation", Intent.Saturation, blackPoint: false, adaptation: 1d);
    public static readonly PrintIntent Absolute    = new("absolute-colorimetric", Intent.AbsoluteColorimetric, blackPoint: false, adaptation: 0d);
    public static readonly PrintIntent RelativeBpc = new("relative-bpc", Intent.RelativeColorimetric, blackPoint: true, adaptation: 1d);
    public static readonly PrintIntent PreserveK   = new("preserve-k", Intent.PreserveKPlaneRelativeColorimetric, blackPoint: true, adaptation: 1d);

    public Intent Rendering { get; }
    public bool BlackPoint { get; }
    public double Adaptation { get; }
}

[ValueObject<double>]
public readonly partial struct PressCeiling {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0d && value <= 400d
            ? validationError
            : new ValidationError("press ceiling is a finite total-area coverage percentage in (0, 400]");
}

[ComplexValueObject]
public sealed partial class GamutAlarm {
    public Seq<ushort> Ink { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<ushort> ink) =>
        validationError = ink.IsEmpty
            ? new ValidationError("gamut alarm carries one code per destination channel")
            : validationError;

    public Unit Mark(Context context) {
        ushort[] codes = context.AlarmCodes;
        Ink.Map(static (code, channel) => (Code: code, Channel: channel)).Iter(row => codes[row.Channel] = row.Code);
        context.AlarmCodes = codes;
        return unit;
    }
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record PrintTransform(
    string Key,
    ColorTarget Target,
    ReadOnlyMemory<byte> SourceProfile,
    ReadOnlyMemory<byte> DestinationProfile,
    PrintIntent IntentRow,
    PrintIntent ProofIntent,
    Option<ReadOnlyMemory<byte>> ProofProfile,
    Option<PressCeiling> Ceiling,
    Option<GamutAlarm> Alarm);

public sealed record PrintProof(
    string Key,
    ColorTarget Target,
    Intent Rendering,
    double AreaCoverage,
    Option<PressCeiling> Ceiling,
    Option<CIEXYZ> DestinationBlack,
    bool BlackPointApplied,
    CmsFlags Flags,
    int Pixels) {
    public bool InkLimited => Ceiling.Exists(ceiling => AreaCoverage > ceiling.Value);
}

public readonly record struct PrintLink(Profile Profile, Intent Rendering, bool BlackPoint, double Adaptation);

public sealed record PrintPlate(byte[] Pixels, PrintProof Proof);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class PrintArm {
    static readonly Op Convert = Op.Of(name: "appui.export.print");

    public static IO<PrintPlate> Convert(PrintTransform row, ReadOnlyMemory<byte> raster) =>
        IO.lift<PrintPlate>(() => Transformed(row, raster));

    static Fin<PrintPlate> Transformed(PrintTransform row, ReadOnlyMemory<byte> raster) =>
        Bracketed(
            () => Context.Create(IntPtr.Zero, IntPtr.Zero),
            context => Opened(context, row.SourceProfile, source =>
                Opened(context, row.DestinationProfile, destination =>
                    Proofing(context, row, proofing =>
                        Proved(row, source, destination, proofing, raster.Length)
                            .Bind(proof => Run(context, row, source, destination, proofing, proof, raster))))));

    static Fin<T> Bracketed<TNative, T>(Func<TNative> acquire, Func<TNative, Fin<T>> project)
        where TNative : class, IDisposable =>
        Convert.Catch(() => Fin.Succ(acquire()))
            .Bind(native => Custody.Bracket(() => project(native), native));

    static Fin<T> Opened<T>(Context context, ReadOnlyMemory<byte> bytes, Func<Profile, Fin<T>> project) =>
        Bracketed(() => Profile.Open(context, bytes.Span.ToArray()), project);

    static Fin<T> Proofing<T>(Context context, PrintTransform row, Func<Option<Profile>, Fin<T>> project) =>
        row.ProofProfile.Match(
            Some: bytes => Opened(context, bytes, profile => project(Some(profile))),
            None: () => project(None));

    static Fin<PrintProof> Proved(PrintTransform row, Profile source, Profile destination, Option<Profile> proofing, int rasterBytes) =>
        row.Target.Pixels(rasterBytes).Bind(pixels =>
            Gates(row, source, destination, proofing)
                .Traverse(static gate => gate.Held
                    ? Validation<Error, Unit>.Success(unit)
                    : Validation<Error, Unit>.Fail(gate.Refuse()))
                .As()
                .ToFin()
                .Map(_ => Resolved(row, destination, proofing, pixels)));

    static Seq<(bool Held, Func<Error> Refuse)> Gates(PrintTransform row, Profile source, Profile destination, Option<Profile> proofing) =>
        Seq(
            (source.IsIntentSupported(row.IntentRow.Rendering, UsedDirection.AsInput),
             (Func<Error>)(() => new ExportFault.IntentUnsupported("source", row.IntentRow.Rendering.ToString()))),
            (destination.IsIntentSupported(row.IntentRow.Rendering, UsedDirection.AsOutput),
             () => new ExportFault.IntentUnsupported("destination", row.IntentRow.Rendering.ToString())))
        + proofing.Map(held => (
            held.IsIntentSupported(row.ProofIntent.Rendering, UsedDirection.AsProof),
            (Func<Error>)(() => new ExportFault.IntentUnsupported("proof", row.ProofIntent.Rendering.ToString())))).ToSeq()
        + Seq(
            (row.Alarm.ForAll(alarm => alarm.Ink.Count == (int)row.Target.Channels),
             (Func<Error>)(() => new ExportFault.ProfileInvalid($"{row.Key}:alarm-channels"))),
            (row.Alarm.IsNone || proofing.IsSome,
             () => new ExportFault.ProfileInvalid($"{row.Key}:alarm-without-proof")));

    static PrintProof Resolved(PrintTransform row, Profile destination, Option<Profile> proofing, int pixels) {
        Option<CIEXYZ> black = destination.DetectDestinationBlackPoint(out CIEXYZ detected, row.IntentRow.Rendering)
            ? Some(detected)
            : None;
        return new PrintProof(
            row.Key, row.Target, row.IntentRow.Rendering, destination.TotalAreaCoverage, row.Ceiling, black,
            row.IntentRow.BlackPoint && black.Match(Some: static xyz => xyz.Y > 0d, None: static () => true),
            (proofing.IsNone ? CmsFlags.None : CmsFlags.SoftProofing)
                | (row.Alarm.IsSome ? CmsFlags.GamutCheck : CmsFlags.None),
            pixels);
    }

    static Fin<PrintPlate> Run(
        Context context, PrintTransform row, Profile source, Profile destination, Option<Profile> proofing,
        PrintProof proof, ReadOnlyMemory<byte> raster) {
        const int GamutPcs = 1;
        return Limited(context, row, proof, limit => {
            row.Alarm.Iter(alarm => ignore(alarm.Mark(context)));
            Seq<PrintLink> chain = Chained(row, source, destination, proofing, limit, proof);
            return Bracketed(
                () => Transform.Create(
                    context,
                    chain.Map(static link => link.Profile).ToArray(),
                    chain.Map(static link => link.BlackPoint).ToArray(),
                    chain.Map(static link => link.Rendering).ToArray(),
                    chain.Map(static link => link.Adaptation).ToArray(),
                    row.Alarm.IsSome ? proofing.Match(Some: static held => held, None: static () => (Profile?)null) : null,
                    GamutPcs, row.Target.Input, row.Target.Output, proof.Flags),
                transform => Convert.Catch(() => {
                    byte[] plate = new byte[(long)proof.Pixels * row.Target.OutputStride];
                    transform.DoTransform(raster.Span, plate, proof.Pixels);
                    return Fin.Succ(new PrintPlate(plate, proof));
                }));
        });
    }

    static Fin<T> Limited<T>(Context context, PrintTransform row, PrintProof proof, Func<Option<Profile>, Fin<T>> project) =>
        (proof.InkLimited ? proof.Ceiling : Option<PressCeiling>.None).Match(
            Some: ceiling => Bracketed(
                () => Profile.CreateInkLimitingDeviceLink(context, row.Target.Device, ceiling.Value),
                link => project(Some(link))),
            None: () => project(None));

    static Seq<PrintLink> Chained(
        PrintTransform row, Profile source, Profile destination, Option<Profile> proofing, Option<Profile> limit, PrintProof proof) =>
        Seq(new PrintLink(source, row.IntentRow.Rendering, BlackPoint: false, row.IntentRow.Adaptation))
        + proofing.Map(press => Seq(
            new PrintLink(press, row.IntentRow.Rendering, BlackPoint: false, row.IntentRow.Adaptation),
            new PrintLink(press, Intent.RelativeColorimetric, BlackPoint: false, row.IntentRow.Adaptation))).IfNone(Seq<PrintLink>())
        + Seq(new PrintLink(
            destination,
            proofing.IsNone ? row.IntentRow.Rendering : row.ProofIntent.Rendering,
            proof.BlackPointApplied,
            row.IntentRow.Adaptation))
        + limit.Map(tail => Seq(new PrintLink(tail, row.IntentRow.Rendering, BlackPoint: false, row.IntentRow.Adaptation))).IfNone(Seq<PrintLink>());
}
```

## [07]-[SCHEDULED_EXPORT]

- Owner: `ReportSubscription` — the consumer-owned recurring-delivery row that closes a report specification over the AppHost scheduler without introducing a document-local timer.
- Entry: `public static Validation<Error, ReportSubscription> Of(string key, string reportKey, OccurrenceSpec occurrence, Option<LeasePolicy> lease, RedrivePolicy redrive)` — the accumulating admission; `public ScheduleEntry Register(Func<string, IO<ReportSpec>> resolve, VisualRuntime runtime)` — contributes one `ScheduleEntry` whose work resolves the current report specification at firing time, renders through `FlowReport.Render`, and preserves the ordinary destination, artifact, deadline, lease, and failure types.
- Law: a scheduled delivery carries a REAL re-drive curve. `RedrivePolicy.None` on a network-or-filesystem delivery meant a transient refusal ended the occurrence, and the two fault cases that publish `Retriability.Transient` had no policy to be re-driven under; the default is the kernel exponential curve and a caller states its own.
- Law: identity admission ACCUMULATES — the schedule key and the report key are independent columns, so a value missing both names both rather than reporting the first and hiding the second. The `[ComplexValueObject]` generator's single-slot `ValidateFactoryArguments` hook cannot express that, which is why the mint is the `Validation` fold and the record carries its own private constructor. NAMED LOSS: the generated `Create`/`TryCreate` pair — `Of` is the one mint, and record equality survives.
- Auto: cadence is an `OccurrenceSpec` value, fleet distribution is `ScheduleEntry.Spread`, bounded missed-occurrence recovery is `SchedulePort.Missed` read at the caller; the subscription stores only the report key and the schedule policy, so a profile reload re-resolves the live report rather than retaining a stale `ReportSpec` object graph.
- Output: every run returns the ordinary document `VisualArtifact` through `FlowReport.Render`; `SchedulePort.Run` carries the work outcome beside its `GaugedSpan<DeadlineClass>`, and a failed delivery remains the scheduled work failure.
- Packages: Rasm.AppHost (project — `Runtime/time`: `OccurrenceSpec`, `DeadlineClass`, `LeasePolicy`, `ScheduleEntry`), Rasm (project — `Domain/results`: `RedrivePolicy`), LanguageExt.Core, NodaTime
- Growth: one recurring deliverable is one `ReportSubscription` value; one cadence is one existing `OccurrenceSpec` case; zero scheduler surface.
- Boundary: `SchedulePort` is the only time owner, `FlowReport` the only pagination owner, and `VisualDestination` the only delivery owner; a timer, login hook, or document-local retry loop is rejected. The missed-occurrence window is `SchedulePort.Missed`, read by whichever surface owns the last-success stamp — the pass-through accessor that stood here named a `SchedulePort.Window` member the owner does not declare and re-published a fold with no consumer of its own.

```csharp
// --- [MODELS] --------------------------------------------------------------------------

public sealed record ReportSubscription {
    private ReportSubscription(string key, string reportKey, OccurrenceSpec occurrence, DeadlineClass deadline,
        Option<LeasePolicy> lease, RedrivePolicy redrive) =>
        (Key, ReportKey, Occurrence, Deadline, Lease, Redrive) = (key, reportKey, occurrence, deadline, lease, redrive);

    public string Key { get; }
    public string ReportKey { get; }
    public OccurrenceSpec Occurrence { get; }
    public DeadlineClass Deadline { get; }
    public Option<LeasePolicy> Lease { get; }
    public RedrivePolicy Redrive { get; }

    public static readonly DeadlineClass Allotment = DeadlineClass.LaneFold;

    public static readonly RedrivePolicy Curve = RedrivePolicy.Of(law: Schedule.exponential(Duration.FromSeconds(30)), bound: 4);

    public static Validation<Error, ReportSubscription> Of(
        string key, string reportKey, OccurrenceSpec occurrence,
        Option<LeasePolicy> lease = default, RedrivePolicy? redrive = null) =>
        (Named(key, "schedule identity"), Named(reportKey, "report identity"))
            .Apply((schedule, report) => new ReportSubscription(schedule, report, occurrence, Allotment, lease, redrive ?? Curve))
            .As();

    static Validation<Error, string> Named(string value, string axis) =>
        string.IsNullOrWhiteSpace(value)
            ? Validation<Error, string>.Fail(new ExportFault.RenderFailed("subscription", $"{axis} is blank"))
            : Validation<Error, string>.Success(value);

    public ScheduleEntry Register(Func<string, IO<ReportSpec>> resolve, VisualRuntime runtime) =>
        new(Key, Occurrence, Deadline, Lease, Redrive,
            () => resolve(ReportKey).Bind(spec => FlowReport.Render(runtime, spec).Map(static _ => unit)));
}
```

## [08]-[EXPORT_FORM]

- Owner: `ExportSection` [SmartEnum] — the schema partition rows; `ExportField` [SmartEnum] — the field vocabulary, each row owning its id suffix, its label key, its section, its entry path, and its own control mint; `ExportCapability` [SmartEnum] — the capability vocabulary carrying the field rows it renders and the readout it reports; `PreflightNote` [Union] — the per-capability readout carrying the capability ROW; `ExportNotes` — the readout bodies those rows bind; `ExportTarget` [SmartEnum] — the per-format row carrying media type, honoured capabilities, arm, Office fidelity column, and Office writer; `DestinationRow` — the recalled destination with its recency; `ExportRequest` — the admitted configuration; `ExportHandoff` with `ExportCardMap` — the run-queue card mapper; `ExportPlan` — the lowering that carries an admitted request into `[03]`, `[04]`, and `[05]`; `ExportForm` — admission, preflight, recency, and completion.
- Cases: `ExportTarget` = pdf · xlsx · docx · pptx · svg · dwg · dxf · png; `ExportCapability` = page-setup · colour · fonts · security · redaction · outline · tagged · line-weights · cad-version · layers · scale; `ExportSection` = page · colour · security · structure · format; `PreflightNote` = Honoured | Degraded | Refused — three readings of one capability, because "will this export keep my fonts" has exactly three honest answers.
- Entry: `public Validation<Error, FormSchema> Schema()` on `ExportTarget` — the per-format option schema through the one form grammar; `public static Fin<ExportRequest> Admit(ExportTarget target, FormSchema schema, FormState state, DestinationRow destination)` — the accumulating configuration admission; `public static Seq<PreflightNote> Preflight(ExportRequest request)` — the readout read off the request's own target; `ExportPlan.Report(...)`, `ExportPlan.Office(...)` — the two lowerings into this page's arms; `ExportCardMap.ToCard(ExportHandoff handoff)` — the run-queue card; `public static Seq<OutputRow> Completed(VisualArtifact artifact)`; `public static Seq<DestinationRow> Remember(Seq<DestinationRow> held, DestinationRow used, Instant at)` and `For(Seq<DestinationRow> held, ExportTarget target)`.
- Law: an admitted request REACHES an arm. `ExportPlan` lowers the page size, orientation, colour, security, outline, and tagged fields onto the kernel-issued `ReportSetup` and the `PdfExport` the report arm consumes, and onto the `OfficeSpec` the office arm consumes — a form whose fields no arm ever read reported over a configuration nothing consumed, which is the one defect a preflight cannot detect.
- Law: field identity is the ROW's. `ExportField.Id(target)` is the one spelling the schema mints, the note reads, and the lowering reads, so a control the schema never rendered refuses at the read instead of resolving to a silent default, and the two spellings of one field id that stood at the mint and at the readout have one authority.
- Law: a capability row carries EVERY reading — the field rows it renders and the verdict it reports — so a capability naming neither is a row rendering no control while reporting itself honoured, which is decorative density. `line-weights` binds the kernel `LineGroup` the issued policy already derives and `layers` binds the kernel `LayerEmission`, both read back as their own readouts rather than answering Honoured unconditionally.
- Law: a field rule the schema evaluates is a DECLARED edge — the permission toggles are visible only under encryption through `FieldRule.WhenSet`, so the constant success validator that stood on every field is replaced by an edge the schema gate actually reads.
- Auto: configuration is SCHEMA, never a per-format dialog: each `ExportTarget` row names the capability rows it honours, each capability names the field rows it renders, and the one `FormChrome` capsule renders the built schema — so adding a format adds a row rather than a screen. Section rows partition the fields by construction, which is exactly what the schema gate proves. The preflight is that SAME roster read a second way, each row answering itself against the admitted configuration. Progress hands to the `Shell/screens#RUN_QUEUE` surface through an ordinary `RunCard` with a `RunOrigin.Verb` correlation, and completion projects `OutputRow` rows whose adopt keys are the open and reveal command intents. Destination rows recall through the persistence snapshot vocabulary the selection sets already use.
- Output: the selected export arm returns its own `VisualArtifact`; this cluster produces no parallel result.
- Packages: Avalonia, NodaTime, Thinktecture.Runtime.Extensions, Riok.Mapperly, LanguageExt.Core, DocumentFormat.OpenXml, Rasm (project — `Drawing/sheet`: `SheetSize`, `SheetSeries`, `SheetOrientation`, `PlotPolicy`, `IssuePosture`, `ScaleLadder`, `LineGroup`, `LayerEmission`, `PdfTrait`; `Interaction/control`: `FieldValue`, `FieldTag`), Rasm.AppHost (project)
- Growth: a new export format is one `ExportTarget` row naming the capability rows it honours, its arm, and its writer; a new capability is ONE `ExportCapability` row carrying its field rows and its verdict; a new control is one `ExportField` row; a new partition is one `ExportSection` row.
- Boundary: the form is the ONE configuration surface — a per-format options dialog, a per-format view model, and a per-format validation pass are the three deleted forms. Fields are `FormField` values over the settled `FieldEntry` rows, so dimensioned entry resolves through the measurement policy and expression entry through the symbolic owner exactly as every other form. The preflight NAMES capability rather than promising it, and a target that cannot answer a capability at all omits the note rather than reporting a false positive. Offered page sizes and CAD releases are CURATED seats of their owners' rosters — each page option is a kernel `SheetSize` whose key round-trips through the owner's own admission and each CAD option is an `ACadVersion` the writer policy admits — so a free paper token and a free version string are the two deleted forms. Progress rides the settled run queue, and the completion verbs are command intents the deck raises. Destination admission stays the `[02]` delivery gate's: a recalled row is a remembered PATH the picker produced, and this cluster never computes one.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExportSection {
    public static readonly ExportSection Page = new("page");
    public static readonly ExportSection Colour = new("colour");
    public static readonly ExportSection Security = new("security");
    public static readonly ExportSection Structure = new("structure");
    public static readonly ExportSection Format = new("format");

    public string TitleKey => $"export.section.{Key}";
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExportField {
    public const double Viewport = 200d;

    public static readonly ExportField PageSize = new(
        "page.size", ExportSection.Page, FieldEntry.Choice,
        static (field, target) => Chosen(field, target, ExportOptions.PageSizes));
    public static readonly ExportField Landscape = new(
        "page.landscape", ExportSection.Page, FieldEntry.Flag, static (field, target) => Toggled(field, target));
    public static readonly ExportField Colour = new(
        "colour.target", ExportSection.Colour, FieldEntry.Choice,
        static (field, target) => Chosen(field, target, ExportOptions.Colours));
    public static readonly ExportField Encrypt = new(
        "security.encrypt", ExportSection.Security, FieldEntry.Flag, static (field, target) => Toggled(field, target));
    public static readonly ExportField AllowPrint = new(
        "security.print", ExportSection.Security, FieldEntry.Flag, static (field, target) => Toggled(field, target));
    public static readonly ExportField AllowExtract = new(
        "security.extract", ExportSection.Security, FieldEntry.Flag, static (field, target) => Toggled(field, target));
    public static readonly ExportField Redaction = new(
        "redaction.posture", ExportSection.Security, FieldEntry.Choice,
        static (field, target) => Chosen(field, target, ExportOptions.Classifications));
    public static readonly ExportField Bookmarks = new(
        "outline.bookmarks", ExportSection.Structure, FieldEntry.Flag, static (field, target) => Toggled(field, target));
    public static readonly ExportField Tagged = new(
        "tagged.ua", ExportSection.Structure, FieldEntry.Flag, static (field, target) => Toggled(field, target));
    public static readonly ExportField LineGroupRung = new(
        "weights.group", ExportSection.Format, FieldEntry.Choice,
        static (field, target) => Chosen(field, target, ExportOptions.LineGroups));
    public static readonly ExportField LayerEmissionMode = new(
        "layers.emission", ExportSection.Format, FieldEntry.Choice,
        static (field, target) => Chosen(field, target, ExportOptions.Emissions));
    public static readonly ExportField CadRelease = new(
        "cad.version", ExportSection.Format, FieldEntry.Choice,
        static (field, target) => Chosen(field, target, ExportOptions.CadReleases));
    public static readonly ExportField Scale = new(
        "raster.scale", ExportSection.Format, FieldEntry.Scalar,
        static (field, target) => new ControlIntent.Slider(field.Id(target), 1d, 4d, 0.5d, IntentBinding.Of(PaintRole.Accent)));

    public ExportSection Section { get; }

    public FieldEntry Entry { get; }

    [UseDelegateFromConstructor]
    public partial ControlIntent Control(ExportField field, ExportTarget target);

    public string Id(ExportTarget target) => $"{target.Key}.{Key}";

    public string LabelKey => $"export.field.{Key}";

    public Option<ExportField> DependsOn =>
        Equals(AllowPrint) || Equals(AllowExtract) ? Some(Encrypt) : None;

    public Validation<Error, (ExportSection Section, FormField Field)> Seat(ExportTarget target) =>
        (FormSchema.Tag(Id(target)).ToValidation(), DependsOn.Traverse(row => FormSchema.Tag(row.Id(target)).ToValidation()).As())
            .Apply((key, gate) => (Section, new FormField(
                key, LabelKey, Control(this, target), Entry,
                gate.ToSeq(),
                gate.Match(Some: static held => (FieldRule)new FieldRule.WhenSet(held), None: static () => new FieldRule.Always()),
                new FieldRule.Never(),
                Option<FieldValue>.None, Option<string>.None, Option<FieldMeasure>.None,
                CommitPosture.Deferred,
                static _ => Validation<Error, Unit>.Success(unit))))
            .As();

    static ControlIntent Chosen(ExportField field, ExportTarget target, Seq<OptionRow> options) =>
        new ControlIntent.Select(
            field.Id(target), SelectPosture.Closed, new OptionSource.Inline(options),
            VirtualWindowSpec.FixedRow(Viewport), IntentBinding.Of(PaintRole.Well));

    static ControlIntent Toggled(ExportField field, ExportTarget target) =>
        new ControlIntent.Toggle(field.Id(target), field.LabelKey, IntentBinding.Of(PaintRole.Panel));
}

public static class ExportOptions {
    static readonly Seq<(SheetSeries Series, int Index)> PaperSeats =
        Seq((SheetSeries.IsoA, 4), (SheetSeries.IsoA, 3), (SheetSeries.Ansi, 0), (SheetSeries.Ansi, 1));

    static readonly Seq<ACadVersion> CadSeats = Seq(ACadVersion.AC1032, ACadVersion.AC1027, ACadVersion.AC1021);

    public static Seq<OptionRow> PageSizes =>
        PaperSeats.Choose(static seat => SheetSize.Of(seat.Series, seat.Index).ToOption()).Map(static size => Row("page", size.Key));

    public static Seq<OptionRow> Colours => toSeq(ColorTarget.Items).Map(static row => Row("colour", row.Key));

    public static Seq<OptionRow> LineGroups => toSeq(LineGroup.Items).Map(static row => Row("weights", row.Key));

    public static Seq<OptionRow> Emissions => toSeq(LayerEmission.Items).Map(static row => Row("layers", row.Key));

    public static Seq<OptionRow> CadReleases =>
        CadSeats.Map(static release => Row("cad", release.ToString().ToLowerInvariant()));

    public static Seq<OptionRow> Classifications => toSeq(DataClassification.Items).Map(static row => Row("redaction", row.Key));

    static OptionRow Row(string family, string key) => new(key, $"export.{family}.{key}", None, None);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PreflightNote {
    private PreflightNote() { }
    public sealed record Honoured(ExportCapability Capability) : PreflightNote;
    public sealed record Degraded(ExportCapability Capability, string Reason) : PreflightNote;
    public sealed record Refused(ExportCapability Capability, string Reason) : PreflightNote;

    public ExportCapability Capability => Switch(
        honoured: static h => h.Capability, degraded: static d => d.Capability, refused: static r => r.Capability);

    public Severity Severity => Switch(
        honoured: static _ => Severity.Info, degraded: static _ => Severity.Warning, refused: static _ => Severity.Critical);

    public string LabelKey => $"export.preflight.{Capability.Key}";

    public string ValueKey => Switch(
        honoured: static _ => "export.preflight.honoured",
        degraded: static d => d.Reason,
        refused: static r => r.Reason);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExportCapability {
    public static readonly ExportCapability PageSetup = new(
        "page-setup", Seq(ExportField.PageSize, ExportField.Landscape), ExportNotes.PageSetup);
    public static readonly ExportCapability Colour = new(
        "colour", Seq(ExportField.Colour), ExportNotes.Colour);
    public static readonly ExportCapability Fonts = new("fonts", Seq<ExportField>(), ExportNotes.Fonts);
    public static readonly ExportCapability Security = new(
        "security", Seq(ExportField.Encrypt, ExportField.AllowPrint, ExportField.AllowExtract), ExportNotes.Security);
    public static readonly ExportCapability Redaction = new(
        "redaction", Seq(ExportField.Redaction), ExportNotes.Redaction);
    public static readonly ExportCapability Outline = new(
        "outline", Seq(ExportField.Bookmarks), ExportNotes.Outline);
    public static readonly ExportCapability Tagged = new(
        "tagged", Seq(ExportField.Tagged), ExportNotes.Tagged);
    public static readonly ExportCapability Weights = new(
        "line-weights", Seq(ExportField.LineGroupRung), ExportNotes.Weights);
    public static readonly ExportCapability CadVersion = new(
        "cad-version", Seq(ExportField.CadRelease), ExportNotes.CadVersion);
    public static readonly ExportCapability Layers = new(
        "layers", Seq(ExportField.LayerEmissionMode), ExportNotes.Layers);
    public static readonly ExportCapability Scale = new(
        "scale", Seq(ExportField.Scale), ExportNotes.Scale);

    public Seq<ExportField> Controls { get; }

    [UseDelegateFromConstructor]
    public partial PreflightNote Note(ExportCapability capability, ExportRequest request);

    public PreflightNote Read(ExportRequest request) => Note(this, request);
}

public static class ExportNotes {
    public static PreflightNote PageSetup(ExportCapability capability, ExportRequest request) =>
        ExportPlan.Paper(request).Match(
            Succ: static _ => (PreflightNote)new PreflightNote.Honoured(capability),
            Fail: _ => new PreflightNote.Refused(capability, "export.preflight.page-unresolved"));

    public static PreflightNote Fonts(ExportCapability capability, ExportRequest request) =>
        request.Target.Equals(ExportTarget.Svg)
            ? new PreflightNote.Degraded(capability, "export.preflight.svg-font-reference")
            : request.Target.Equals(ExportTarget.Xlsx)
                ? new PreflightNote.Refused(capability, "export.preflight.workbook-no-font-part")
                : new PreflightNote.Honoured(capability);

    public static PreflightNote Colour(ExportCapability capability, ExportRequest request) =>
        ExportPlan.Colour(request).Match(
            Succ: row => row.Equals(ColorTarget.Screen)
                ? (PreflightNote)new PreflightNote.Honoured(capability)
                : new PreflightNote.Degraded(capability, "export.preflight.press-proof-required"),
            Fail: _ => new PreflightNote.Refused(capability, "export.preflight.colour-unresolved"));

    public static PreflightNote Security(ExportCapability capability, ExportRequest request) =>
        ExportPlan.Security(request).Match(
            Succ: static security => security is PdfSecurity.Encrypted
                ? (PreflightNote)new PreflightNote.Honoured(capability)
                : new PreflightNote.Refused(capability, "export.preflight.encryption-off"),
            Fail: _ => new PreflightNote.Refused(capability, "export.preflight.security-unresolved"));

    public static PreflightNote Redaction(ExportCapability capability, ExportRequest request) =>
        ExportPlan.Chosen(request, ExportField.Redaction).Match(
            Succ: key => DataClassification.TryGet(key, out DataClassification? row) && row is not null
                ? (PreflightNote)new PreflightNote.Honoured(capability)
                : new PreflightNote.Refused(capability, "export.preflight.redaction-unknown"),
            Fail: _ => new PreflightNote.Refused(capability, "export.preflight.redaction-unset"));

    public static PreflightNote Outline(ExportCapability capability, ExportRequest request) =>
        Flagged(capability, request, ExportField.Bookmarks, "export.preflight.bookmarks-off");

    public static PreflightNote Tagged(ExportCapability capability, ExportRequest request) =>
        Flagged(capability, request, ExportField.Tagged, "export.preflight.tagging-off");

    public static PreflightNote Weights(ExportCapability capability, ExportRequest request) =>
        (from picked in ExportPlan.Chosen(request, ExportField.LineGroupRung)
         from setup in ExportPlan.Setup(request)
         select (Picked: picked, Derived: setup.Plot.Group.Key)).Match(
            Succ: read => read.Picked == read.Derived
                ? (PreflightNote)new PreflightNote.Honoured(capability)
                : new PreflightNote.Degraded(capability, "export.preflight.weights-resolved-by-extent"),
            Fail: _ => new PreflightNote.Refused(capability, "export.preflight.weights-unresolved"));

    public static PreflightNote Layers(ExportCapability capability, ExportRequest request) =>
        ExportPlan.Emission(request).Match(
            Succ: static row => row.Equals(LayerEmission.OptionalContent)
                ? (PreflightNote)new PreflightNote.Honoured(capability)
                : new PreflightNote.Degraded(capability, "export.preflight.layers-flattened"),
            Fail: _ => new PreflightNote.Refused(capability, "export.preflight.layers-unresolved"));

    public static PreflightNote CadVersion(ExportCapability capability, ExportRequest request) =>
        ExportPlan.Release(request).Match(
            Succ: static _ => (PreflightNote)new PreflightNote.Honoured(capability),
            Fail: _ => new PreflightNote.Refused(capability, "export.preflight.cad-version-unresolved"));

    public static PreflightNote Scale(ExportCapability capability, ExportRequest request) =>
        ExportPlan.Scale(request).Match(
            Succ: static _ => (PreflightNote)new PreflightNote.Honoured(capability),
            Fail: _ => new PreflightNote.Refused(capability, "export.preflight.scale-unresolved"));

    static PreflightNote Flagged(ExportCapability capability, ExportRequest request, ExportField field, string off) =>
        ExportPlan.Flagged(request, field).Match(
            Succ: held => held
                ? (PreflightNote)new PreflightNote.Honoured(capability)
                : new PreflightNote.Refused(capability, off),
            Fail: _ => new PreflightNote.Refused(capability, off));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExportTarget {
    public static readonly ExportTarget Pdf = new(
        "pdf", "application/pdf", Some(ExportArm.Document),
        Seq(ExportCapability.PageSetup, ExportCapability.Colour, ExportCapability.Fonts, ExportCapability.Security,
            ExportCapability.Redaction, ExportCapability.Outline, ExportCapability.Tagged, ExportCapability.Layers),
        fidelity: Seq<(string, OfficeFidelity)>(), office: None);
    public static readonly ExportTarget Xlsx = new(
        "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Some(ExportArm.Office),
        Seq(ExportCapability.Fonts),
        fidelity: Seq(("table", OfficeFidelity.Native), ("chart", OfficeFidelity.Declared), ("richText", OfficeFidelity.Declared)),
        office: Some<OfficeWrite>(OfficeExport.Xlsx));
    public static readonly ExportTarget Docx = new(
        "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Some(ExportArm.Office),
        Seq(ExportCapability.PageSetup, ExportCapability.Fonts),
        fidelity: Seq(("table", OfficeFidelity.Declared), ("chart", OfficeFidelity.Declared), ("richText", OfficeFidelity.Native)),
        office: Some<OfficeWrite>(OfficeExport.Docx));
    public static readonly ExportTarget Pptx = new(
        "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation", Some(ExportArm.Office),
        Seq<ExportCapability>(), fidelity: Seq<(string, OfficeFidelity)>(), office: None);
    public static readonly ExportTarget Svg = new(
        "svg", "image/svg+xml", None, Seq(ExportCapability.Fonts, ExportCapability.Weights),
        fidelity: Seq<(string, OfficeFidelity)>(), office: None);
    public static readonly ExportTarget Dwg = new(
        "dwg", "image/vnd.dwg", None, Seq(ExportCapability.CadVersion, ExportCapability.Layers, ExportCapability.Weights),
        fidelity: Seq<(string, OfficeFidelity)>(), office: None);
    public static readonly ExportTarget Dxf = new(
        "dxf", "image/vnd.dxf", None, Seq(ExportCapability.CadVersion, ExportCapability.Layers, ExportCapability.Weights),
        fidelity: Seq<(string, OfficeFidelity)>(), office: None);
    public static readonly ExportTarget Png = new(
        "png", "image/png", None, Seq(ExportCapability.Colour, ExportCapability.Scale),
        fidelity: Seq<(string, OfficeFidelity)>(), office: None);

    public string MediaType { get; }

    public Option<ExportArm> Arm { get; }

    public Seq<ExportCapability> Honours { get; }

    public Seq<(string Kind, OfficeFidelity Row)> Fidelity { get; }

    public Option<OfficeWrite> Office { get; }

    public OfficeFidelity Materializes(string sheetKind) =>
        Fidelity.Find(row => row.Kind == sheetKind).Map(static row => row.Row).IfNone(OfficeFidelity.Unsupported);

    public Validation<Error, FormSchema> Schema() =>
        Honours.Bind(static capability => capability.Controls)
            .Traverse(field => field.Seat(this)).As()
            .Bind(seats => FormSchema.Create(
                $"export.{Key}", $"export.{Key}.submit", $"export.{Key}.commit", FormGeometry.Stacked,
                seats.Map(static seat => seat.Field),
                Sections(seats)));

    static Seq<FormSection> Sections(Seq<(ExportSection Section, FormField Field)> seats) =>
        toSeq(seats.Map(static seat => seat.Section).Distinct())
            .Map(section => FormSection.Of(
                section.Key, section.TitleKey,
                seats.Filter(seat => seat.Section.Equals(section)).Map(static seat => seat.Field.Key)));
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct DestinationRow(string Key, string AbsolutePath, ExportTarget Format, Instant LastUsed);

public sealed record ExportRequest(ExportTarget Target, FormState State, DestinationRow Destination);

public sealed record ExportHandoff(RunOrigin Origin, ExportRequest Request, Instant At);

// --- [COMPOSITION] ---------------------------------------------------------------------

[Mapper(
    RequiredMappingStrategy = RequiredMappingStrategy.Target,
    EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
public static partial class ExportCardMap {
    [MapValue(nameof(RunCard.JobIntent), ExportForm.RunIntent)]
    [MapValue(nameof(RunCard.Job), Use = nameof(Queued))]
    [MapValue(nameof(RunCard.Run), Use = nameof(Queued))]
    [MapValue(nameof(RunCard.Direction), Use = nameof(Outbound))]
    [MapValue(nameof(RunCard.Fan), Use = nameof(Single))]
    [MapValue(nameof(RunCard.Fault), Use = nameof(NoFault))]
    [MapValue(nameof(RunCard.Redrive), Use = nameof(NoRedrive))]
    [MapPropertyFromSource(nameof(RunCard.LabelKey), Use = nameof(Label))]
    [MapPropertyFromSource(nameof(RunCard.Strips), Use = nameof(Strips))]
    [MapPropertyFromSource(nameof(RunCard.Steps), Use = nameof(Steps))]
    public static partial RunCard ToCard(ExportHandoff handoff);

    static WorkStatus Queued() => WorkStatus.Queued;
    static RunDirection Outbound() => RunDirection.Outbound;
    static FanOut Single() => new(1, 0, 0);
    static Option<Fault> NoFault() => None;
    static Option<RedriveMark> NoRedrive() => None;

    static string Label(ExportHandoff handoff) => $"export.job.{handoff.Request.Target.Key}";

    static Seq<StateStrip> Strips(ExportHandoff handoff) =>
        ExportForm.Preflight(handoff.Request).Map(static note => new StateStrip(note.LabelKey, note.ValueKey, note.Severity));

    static Seq<StepRow> Steps(ExportHandoff handoff) =>
        Seq(new StepRow(
            handoff.Request.Target.Key, $"export.step.{handoff.Request.Target.Key}",
            WorkStatus.Queued, None, Seq<string>(), Seq<OutputRow>()));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ExportPlan {
    static readonly Op Lower = Op.Of(name: "appui.export.lower");

    public static Fin<FieldValue> Read(ExportRequest request, ExportField field) =>
        FormSchema.Tag(field.Id(request.Target))
            .Bind(tag => request.State.Values.Find(tag).ToFin(Refusal(field, "carries no admitted value")))
            .Bind(cell => cell.Uniform.ToFin(Refusal(field, "diverges across targets")));

    public static Fin<string> Chosen(ExportRequest request, ExportField field) =>
        Read(request, field).Bind(value => value is FieldValue.Pick pick
            ? Fin.Succ(pick.Text)
            : Fin.Fail<string>(Refusal(field, "is not a choice")));

    public static Fin<bool> Flagged(ExportRequest request, ExportField field) =>
        Read(request, field).Bind(value => value is FieldValue.Flag flag
            ? flag.Value.ToFin(Refusal(field, "carries no flag state"))
            : Fin.Fail<bool>(Refusal(field, "is not a flag")));

    public static Fin<double> Scale(ExportRequest request) =>
        Read(request, ExportField.Scale).Bind(value => value is FieldValue.Number number
            ? Fin.Succ(number.Value)
            : Fin.Fail<double>(Refusal(ExportField.Scale, "is not a scalar")));

    public static Fin<SheetSize> Paper(ExportRequest request) =>
        Chosen(request, ExportField.PageSize).Bind(key => Lower.AcceptValidated<SheetSize>(key));

    public static Fin<ColorTarget> Colour(ExportRequest request) =>
        Chosen(request, ExportField.Colour).Bind(key =>
            Op.Probe(() => (ColorTarget.TryGet(key, out ColorTarget? row), row)).Bind(Optional)
                .ToFin(Refusal(ExportField.Colour, $"names no colour row: {key}")));

    public static Fin<LayerEmission> Emission(ExportRequest request) =>
        Chosen(request, ExportField.LayerEmissionMode).Bind(key =>
            Op.Probe(() => (LayerEmission.TryGet(key, out LayerEmission? row), row)).Bind(Optional)
                .ToFin(Refusal(ExportField.LayerEmissionMode, $"names no emission row: {key}")));

    public static Fin<ACadVersion> Release(ExportRequest request) =>
        Chosen(request, ExportField.CadRelease).Bind(key =>
            Op.Probe(() => (Enum.TryParse(key, ignoreCase: true, out ACadVersion parsed), parsed))
                .ToFin(Refusal(ExportField.CadRelease, $"names no admitted release: {key}")));

    public static Fin<PdfSecurity> Security(ExportRequest request) =>
        Flagged(request, ExportField.Encrypt).Bind(encrypted => encrypted
            ? Grants(request).Map(static permits => (PdfSecurity)new PdfSecurity.Encrypted(None, permits, MetadataPosture.Encrypted))
            : Fin.Succ(PdfSecurity.Open));

    static readonly Seq<(PdfPermission Row, ExportField Field)> PermissionRows =
        Seq((PdfPermission.Print, ExportField.AllowPrint), (PdfPermission.Extract, ExportField.AllowExtract));

    static Fin<CapabilitySet<PdfPermission>> Grants(ExportRequest request) =>
        PermissionRows
            .TraverseM(row => Flagged(request, row.Field).Map(held => (row.Row, Held: held)))
            .As()
            .Map(static read => read.Filter(static grant => grant.Held).Map(static grant => grant.Row))
            .Map(static granted => CapabilitySet<PdfPermission>.Of(granted.ToArray()));

    public static Fin<ReportSetup> Setup(ExportRequest request) =>
        from size in Paper(request)
        from landscape in Flagged(request, ExportField.Landscape)
        from tagged in Flagged(request, ExportField.Tagged)
        let convention = IssuePosture.For(size.Standard)
        let emission = Emission(request).IfFail(convention.Emission)
        from plot in PlotPolicy.Of(
            size: size,
            orientation: landscape ? SheetOrientation.Landscape : SheetOrientation.Portrait,
            scale: ScaleLadder.For(size.Standard).Nearest(convention.Scale),
            posture: convention.Posture,
            resolution: convention.Resolution,
            emission: emission,
            conformance: tagged ? convention.Conformance.With(PdfTrait.Accessible) : convention.Conformance,
            styles: None,
            key: Lower)
        from margin in plot.Frame.Margin(size, Lower)
        select new ReportSetup(plot, margin);

    public static Fin<PdfExport> Hardening(ExportRequest request, ReportSetup setup, PdfCredentials credentials) =>
        from colour in Colour(request)
        from security in Security(request)
        from policy in PdfExport.Issued(setup.Plot, colour, security, credentials)
        select policy;

    public static Fin<ReportSpec> Report(
        ExportRequest request, string title, Seq<ReportBlock> blocks,
        Option<string> header, PdfCredentials credentials, CapabilitySet<ReportTrait> baseline) =>
        from setup in Setup(request)
        from pdf in Hardening(request, setup, credentials)
        from bookmarks in Flagged(request, ExportField.Bookmarks)
        select new ReportSpec(
            title, blocks, header, Some(setup), pdf,
            new VisualDestination.FilePath(request.Destination.AbsolutePath),
            bookmarks ? baseline.With(ReportTrait.Bookmarks) : baseline.Without(ReportTrait.Bookmarks));

    public static Fin<OfficeSpec> Office(
        ExportRequest request, Seq<OfficeSheet> sheets, Seq<(string FontFamily, ReadOnlyMemory<byte> Face)> fonts) =>
        request.Target.Arm.Filter(static arm => arm.Equals(ExportArm.Office)).IsSome
            ? Fin.Succ(new OfficeSpec(
                request.Target, sheets, fonts, new VisualDestination.FilePath(request.Destination.AbsolutePath)))
            : Fin.Fail<OfficeSpec>(new ExportFault.ContentUnsupported(request.Target.Key, "office part graph"));

    static Error Refusal(ExportField field, string requirement) =>
        new ExportFault.RenderFailed("export-form", $"{field.Key} {requirement}");
}

public static class ExportForm {
    public const string OpenIntent = "export.artifact.open";
    public const string RevealIntent = "export.artifact.reveal";
    public const string RunIntent = "export.run";

    const int RecencyDepth = 16;

    public static Fin<ExportRequest> Admit(ExportTarget target, FormSchema schema, FormState state, DestinationRow destination) =>
        (Landing(destination), schema.Admit(state))
            .Apply((seat, admitted) => new ExportRequest(target, admitted, seat))
            .As()
            .ToFin();

    static Validation<Error, DestinationRow> Landing(DestinationRow destination) =>
        !string.IsNullOrWhiteSpace(destination.AbsolutePath) && Path.IsPathFullyQualified(destination.AbsolutePath)
            ? Validation<Error, DestinationRow>.Success(destination)
            : Validation<Error, DestinationRow>.Fail(
                new ExportFault.DeliveryFailed(destination.AbsolutePath, "destination is not a fully qualified path"));

    public static Seq<PreflightNote> Preflight(ExportRequest request) =>
        request.Target.Honours.Map(capability => capability.Read(request));

    public static Severity Standing(ExportRequest request) =>
        Severity.Worst(Preflight(request), static note => note.Severity);

    public static Seq<OutputRow> Completed(VisualArtifact artifact) =>
        artifact.Destination.Map(static destination => Seq(
                new OutputRow(destination, "export.output.open", "artifact", OutputState.Sealed(Some(OpenIntent))),
                new OutputRow(destination, "export.output.reveal", "artifact", OutputState.Sealed(Some(RevealIntent)))))
            .IfNone(Seq<OutputRow>());

    public static Seq<DestinationRow> Remember(Seq<DestinationRow> held, DestinationRow used, Instant at) =>
        (Seq(used with { LastUsed = at })
            + held.Filter(row => !(row.Format.Equals(used.Format)
                && string.Equals(row.AbsolutePath, used.AbsolutePath, StringComparison.Ordinal))))
            .Take(RecencyDepth);

    public static Seq<DestinationRow> For(Seq<DestinationRow> held, ExportTarget target) =>
        toSeq(held.Filter(row => row.Format.Equals(target)).OrderByDescending(static row => row.LastUsed));
}
```

## [09]-[RESEARCH]

(none)
