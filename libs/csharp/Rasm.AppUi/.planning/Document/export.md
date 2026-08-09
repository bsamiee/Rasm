# [APPUI_DOCUMENT_EXPORT]

Rasm.AppUi document export owns one paginated-output rail. MigraDoc composes flow reports, PDFsharp policies harden PDF output, OOXML writers carry XLSX/DOCX/PPTX, and lcmsNET rows carry print fidelity. This page owns destinations, `ExportFault`, support-bundle contributions, report specifications, PDF policies, Office output, and print transforms. Drafting flow reports and diagnostics evidence compose this owner; drafting sheet-PDF remains on capture's vector-print arm.

## [01]-[INDEX]

- [02]-[EXPORT_DESTINATIONS]: One destination union; the typed `ExportFault` rail; the support-bundle contributor rows.
- [03]-[FLOW_REPORT]: MigraDoc flow DOM; auto-pagination; running bands; placed visuals.
- [04]-[PDF_POLICY]: Security, signatures over the AppHost secrets lease, AcroForms, PDF-UA, the colour-mode row, cross-reference annotations.
- [05]-[OFFICE_ARM]: OOXML part-graph writers — XLSX, DOCX, PPTX.
- [06]-[PRINT_ARM]: lcmsNET device-CMYK/ICC transforms; intent proving; ink limiting; proofing; K-preservation intents.
- [07]-[SCHEDULED_EXPORT]: Consumer-owned `ScheduleEntry` rows for recurring report delivery and bounded backfill.
- [08]-[EXPORT_FORM]: Per-format option schemas, destination recency, the preflight readout, and the run-queue handoff.

## [02]-[EXPORT_DESTINATIONS]

- Owner: `VisualDestination` [Union] — the one delivery vocabulary every export arm and the capture vector-print/video arms deliver through, carrying its own `Key` case column so a delivery-keyed projection reads the owner rather than re-spelling the case literals; `ExportFault` — the typed export rail; `ExportDelivery` — the one delivery fold; `BundleMember` — the classified, content-keyed diagnostic-artifact row; `SupportBundle` — the member roster and the contribution fold onto the Bundle destination.
- Cases: FilePath · BlobLane · Bundle; bundle member rows evidence-journal · hud-samples · gpu-timelines · quality-verdicts · native-assets · proof-goldens · collab-ops — each a named factory pinning artifact name and classification.
- Entry: `public static IO<string> Deliver(VisualRuntime runtime, VisualDestination destination, ReadOnlyMemory<byte> payload)` — the ONE delivery rail at the widest payload shape, an array-holding producer binding through the implicit conversion; the FilePath arm receives its absolute path as a value from the picker intent and never computes paths; artifact scopes resolve from `ProfileRoots`; `SupportBundle.Contribute(VisualRuntime runtime, params ReadOnlySpan<BundleMember> members)` — one modality-polymorphic contribution fold delivering every member through the Bundle destination and sealing one `RenderReceipt` per member whose kind IS that destination's own `Key`.
- Auto: the Bundle arm stages every classified artifact through the runtime `BundleWrite` delegate before any receipt enters the sink, then commits receipts only for the complete delivered roster; a delivery or sink refusal stays on the IO rail for AppHost `SupportCapture` to recover as its partial manifest row and cleanup fold; every delivered payload seals a `RenderReceipt` of kind document/office/print/bundle whose `FrameHash` mints through the runtime `ContentHash` delegate bound to the kernel `Rasm.Domain` `ContentHash.Of` entry; each `BundleMember` payload arrives already serialized by its owning codec — the evidence journal off the sealed envelope stream, HUD samples and GPU timelines off the devloop feeds, quality verdicts and native-asset facts off their receipt folds, proof-golden digests off the render-hash lane, and the collab op window off the devloop `CollabJson` readable export — so assembly is a fold over settled receipt streams and no member re-measures.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm (project)
- Growth: one destination case extends delivery and breaks the dispatch at compile time; one export target is one row on the owning arm, never a second engine; one diagnostic stream is one `BundleMember` factory row; one `ExportFault` case is one `detail` ordinal under the `AppUiFaultBand.Export` row (6420); zero new surface.
- Boundary: this union is the ONE export-destination owner — capture's vector-print arm, the FFmpeg clip rows, drafting's sheet-PDF egress, and the diagnostics report-PDF all deliver through it, so a per-arm destination enum is the deleted form; `FilePath` admits only fully qualified targets whose normalized path remains under `ProfileRoots.AppRoot`, `StoreRoot`, or `SupportRoot`, rejects every symlink or junction in the selected root and existing parent chain, opens the unique pending file with create-new semantics before writing, and lands the final rename fail-closed against a parent swap — source and target resolve through one parent path in one rename syscall, the GUID-named pending sibling cannot pre-exist at a redirected parent, and the link-free parent re-walk runs after the write immediately before the rename — so a relative, linked, escaping, or mid-flight-redirected path folds to `ExportFault.DeliveryFailed`; every fault derives through `AppUiFaultBand.Export` — a bare `Error.New` is the deleted form; archive assembly and manifest custody are the AppHost support-capture fold's — contributed members cross the `BundleWrite` seam as classified payloads, the AppHost `SupportCapture` redacts, caps, and archives them, and an AppUi-local zip assembler or second manifest store is the deleted form; the contributor roster declares AppUi membership and classification, and `BundleMember.ContentKey` mints each pre-redaction payload identity through kernel `ContentHash.Of`, while AppHost `SupportManifest.Entry` carries the post-redaction, post-cap `ContentKey` over the bytes each zip member holds, so the two keys agree exactly where nothing was masked or truncated and an inequality names redaction or a cap rather than corruption.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VisualDestination {
    private VisualDestination() { }
    public sealed record FilePath(string AbsolutePath) : VisualDestination;
    public sealed record BlobLane(string ArtifactKey) : VisualDestination;
    public sealed record Bundle(string ArtifactName, DataClassification Classification) : VisualDestination;

    // The delivery-case key is OWNER-declared, exactly as every other union on this branch declares its
    // own: a consuming projection reads `Key` instead of re-spelling three literals on a `Switch` the
    // owner already exhausts, so a fourth case renames nothing at a consumer.
    public string Key => Switch(
        filePath: static _ => "file", blobLane: static _ => "blob", bundle: static _ => "bundle");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExportFault : Expected {
    private ExportFault(string detail, int code) : base(detail, code) { }
    public sealed record RenderFailed(string Stage, string Detail)
        : ExportFault($"export/render: {Stage} — {Detail}", AppUiFaultBand.Export.Code(0));
    public sealed record SignerUnavailable(string Detail)
        : ExportFault($"export/signer: {Detail}", AppUiFaultBand.Export.Code(1));
    public sealed record ProfileInvalid(string ProfileKey)
        : ExportFault($"export/icc: {ProfileKey} does not parse as an ICC profile", AppUiFaultBand.Export.Code(2));
    public sealed record PartGraphRejected(string Part, string Detail)
        : ExportFault($"export/ooxml: {Part} — {Detail}", AppUiFaultBand.Export.Code(3));
    public sealed record DeliveryFailed(string Destination, string Detail)
        : ExportFault($"export/deliver: {Destination} — {Detail}", AppUiFaultBand.Export.Code(4));
    public sealed record ContentUnsupported(string Format, string Sheet)
        : ExportFault($"export/content: {Sheet} has no {Format} materialization", AppUiFaultBand.Export.Code(5));
    public sealed record AnnotationRejected(string Kind, string Detail)
        : ExportFault($"export/annotation: {Kind} — {Detail}", AppUiFaultBand.Export.Code(6));
    public sealed record IntentUnsupported(string Role, string Intent)
        : ExportFault($"export/intent: the {Role} profile carries no {Intent} pipeline", AppUiFaultBand.Export.Code(7));
}

public static class ExportDelivery {
    // ONE entry at the widest payload shape the three arms admit. `ReadOnlyMemory<byte>` is what the file arm
    // writes natively, and the implicit `byte[]` conversion binds every array-holding producer with no adapter
    // and no call-site edit — so the array-shaped sibling that forwarded here was a hop, not a modality, and
    // its removal is what keeps a streamed export from paying a defensive copy to reach the one delivery it
    // shares with a composed report. The two delegate-backed arms materialize because their seams are
    // composition-bound delegate columns typed on the array, so the copy happens on exactly the two paths that
    // cannot avoid it rather than on all three.
    public static IO<string> Deliver(VisualRuntime runtime, VisualDestination destination, ReadOnlyMemory<byte> payload) =>
        destination.Switch(
            state: (runtime, payload),
            filePath: static (ctx, file) => AtomicFile(ctx.runtime.Roots, file.AbsolutePath, ctx.payload),
            blobLane: static (ctx, blob) => ctx.runtime.BlobWrite(blob.ArtifactKey, ctx.payload.ToArray()),
            bundle: static (ctx, bundle) => ctx.runtime.BundleWrite(bundle.ArtifactName, bundle.Classification, ctx.payload.ToArray()));

    // Parent-swap redirection fails closed by construction, not by a trusted string re-check: the
    // rename source and target share ONE parent path the OS resolves inside a single rename syscall,
    // and the pending sibling's name carries an unguessable GUID — a parent directory swapped for a
    // link after admission re-points BOTH paths, the pending byte stream is absent at the redirected
    // parent, and the rename faults instead of landing bytes outside the admitted root. The link-free
    // parent re-walk runs AFTER the write, immediately before the rename, so a long payload write
    // never widens the admission-to-rename window; no BCL directory-handle-relative rename exists,
    // and this shared-parent + unguessable-sibling shape is the fail-closed equivalent.
    static IO<string> AtomicFile(ProfileRoots roots, string destination, ReadOnlyMemory<byte> payload) =>
        (IO.lift(() => {
                string target = Path.GetFullPath(destination);
                Option<string> admittedRoot = Path.IsPathFullyQualified(destination) ? Within(roots, target) : None;
                if (admittedRoot.IsNone)
                    throw new UnauthorizedAccessException("destination is outside the configured profile roots");
                string? directory = Path.GetDirectoryName(target);
                if (string.IsNullOrWhiteSpace(directory))
                    throw new DirectoryNotFoundException("destination has no directory");
                string pending = Path.Combine(directory, $".{Path.GetFileName(target)}.{Guid.NewGuid():N}.pending");
                try {
                    using (Microsoft.Win32.SafeHandles.SafeFileHandle handle = File.OpenHandle(
                        pending, FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough)) {
                        RandomAccess.Write(handle, payload.Span, fileOffset: 0L);
                    }
                    if (!ResolvedParent(admittedRoot.IfNone(string.Empty), target))
                        throw new UnauthorizedAccessException("destination parent changed during admission");
                    File.Move(pending, target, overwrite: true);
                    return target;
                }
                finally { if (File.Exists(pending)) { File.Delete(pending); } }
            })
            | @catch<IO, string>(static _ => true,
                error => IO.fail<string>(new ExportFault.DeliveryFailed(destination, error.Message)))).As();

    static Option<string> Within(ProfileRoots roots, string target) =>
        (Seq(roots.AppRoot, roots.SupportRoot) + roots.StoreRoot.ToSeq())
            .Map(Path.GetFullPath)
            .Find(root => {
                string relative = Path.GetRelativePath(Path.GetFullPath(root), target);
                return relative != ".."
                    && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                    && !Path.IsPathFullyQualified(relative)
                    && ResolvedParent(root, target);
            });

    static bool ResolvedParent(string root, string target) {
        string? directory = Path.GetDirectoryName(target);
        if (string.IsNullOrWhiteSpace(directory)) { return false; }
        string relative = Path.GetRelativePath(root, directory);
        if (relative == ".." || relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            || Path.IsPathFullyQualified(relative)) { return false; }
        string current = root;
        foreach (string segment in relative.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries)) {
            if (!DirectoryAdmitted(current)) { return false; }
            current = Path.Join(current, segment);
        }
        return DirectoryAdmitted(current);
    }

    static bool DirectoryAdmitted(string path) {
        DirectoryInfo directory = new(path);
        return directory.Exists
            && directory.LinkTarget is null
            && (directory.Attributes & FileAttributes.ReparsePoint) == 0;
    }
}

// One classified, content-keyed diagnostic artifact: the payload arrives already serialized by its
// owning codec, the content key mints through the kernel one-hasher entry, and delivery rides the
// Bundle destination so the AppHost support-capture fold owns archive assembly and manifest custody.
public sealed record BundleMember(string ArtifactName, DataClassification Classification, ReadOnlyMemory<byte> Payload) {
    public UInt128 ContentKey => ContentHash.Of(Payload.Span);

    public static BundleMember EvidenceJournal(ReadOnlyMemory<byte> journal) => new("evidence-journal.jsonl", DataClassification.Operational, journal);
    public static BundleMember HudSamples(ReadOnlyMemory<byte> samples) => new("hud-samples.jsonl", DataClassification.Operational, samples);
    public static BundleMember GpuTimelines(ReadOnlyMemory<byte> timelines) => new("gpu-timelines.jsonl", DataClassification.Operational, timelines);
    public static BundleMember QualityVerdicts(ReadOnlyMemory<byte> verdicts) => new("quality-verdicts.jsonl", DataClassification.Operational, verdicts);
    public static BundleMember NativeAssets(ReadOnlyMemory<byte> facts) => new("native-assets.jsonl", DataClassification.HostIdentity, facts);
    public static BundleMember GoldenDigests(ReadOnlyMemory<byte> digests) => new("proof-goldens.jsonl", DataClassification.Operational, digests);
    public static BundleMember CollabOps(ReadOnlyMemory<byte> opWindow) => new("collab-ops.json", DataClassification.UserContent, opWindow);
}

public static class SupportBundle {
    // Arity rides the span: one member or the whole roster contributes through one fold, each member
    // stages as its classified Bundle artifact before any receipt commits; a refused roster returns
    // on the IO rail, and AppHost alone projects partial manifest evidence and staging cleanup. The
    // receipt kind READS the destination's own key rather than re-spelling the case literal the union
    // already exhausts, so the delivery this fold takes and the kind its receipt reports cannot fork.
    public static IO<Seq<RenderReceipt>> Contribute(VisualRuntime runtime, params ReadOnlySpan<BundleMember> members) =>
        from staged in toSeq(members.ToArray()).TraverseM(member =>
            from mark in IO.lift(runtime.Clocks.Mark)
            from payload in IO.pure(member.Payload.ToArray())
            let target = new VisualDestination.Bundle(member.ArtifactName, member.Classification)
            from destination in ExportDelivery.Deliver(runtime, target, payload)
            from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
            let receipt = new RenderReceipt(
                target.Key, Path.GetExtension(member.ArtifactName).TrimStart('.'), runtime.ContentHash(payload), None, None,
                payload.LongLength, elapsed, runtime.Correlation, Optional(destination), VisualCodec.ColorPolicy.Display.Key)
            select receipt).As()
        from _ in staged.TraverseM(runtime.Sink).As()
        select staged;
}
```

## [03]-[FLOW_REPORT]

- Owner: `ReportSpec` — the flow-report composition row; `ReportSetup` — the page-geometry policy row (dimensions, margins, orientation) applied once to the section `PageSetup`; `ReportBlock` [Union] — the typed content vocabulary the MigraDoc fold consumes; `ReportHeading` — the composed heading node the bookmark outline binds to; `FlowReport` — the one MigraDoc render surface.
- Cases: `ReportBlock` = Heading · Body · List · Callout · Code · Table · PlacedVisual · Figure · Footnote · Section · Rule · PageBreak.
- Entry: `public static IO<RenderReceipt> Render(VisualRuntime runtime, ReportSpec spec)` — IO rail; the MigraDoc `Document`/`Section` DOM composes from the block seq, `PdfDocumentRenderer` paginates, and the payload delivers through the destination union.
- Auto: the heading tree becomes the PDF bookmark outline: composition retains each heading's own layout node, the renderer's page walk answers which page that node landed on, and the level ladder nests each bookmark under its nearest shallower ancestor — so a long report navigates by structure and a heading level that skips a rung attaches sensibly rather than at the root. Every heading-bearing block STATES its own level and an untitled group states no heading at all, so a nested section bookmarks under its parent instead of flattening onto one rung and a quoted passage lowered as an untitled callout mints no blank bookmark. Pagination, widow/orphan control, running headers/footers with `PageField`/`NumPagesField`, and cross-page table breaking are the MigraDoc layout engine's — the hand-rolled `FlowBlock`/`FlowFold` pagination engine is the deleted form this owner replaces; `FormattedDocument` exposes the measured layout so a page count or block position reads from the renderer, never a local cursor fold; placed visuals enter as `PlacedVisual` rows whose `SKImage` tiles encode through the capture codec axis (`VisualCodec.Encode`) and place as MigraDoc `Image` values — capture stays the one raster owner, the report only places.
- Receipt: one `RenderReceipt` of kind document per report with whole-payload content hash through the kernel-bound delegate and the delivered destination key.
- Packages: PDFsharp-MigraDoc, PDFsharp, SkiaSharp, Rasm.AppHost (project), NodaTime, LanguageExt.Core
- Growth: one `ReportBlock` case extends the content vocabulary; one style row retunes a role's typography mapping; one bookmark-bearing block is one arm returning its own `ReportHeading`; zero new surface.
- Boundary: the report's colour model is its `PdfPolicy.Color` row bound onto the renderer document before `RenderDocument`, so a press report and a screen report are one composition under one row value; the MigraDoc flow DOM is the ONE flow-pagination owner — a bespoke page-break fold, a per-format report builder, or a second cursor algebra is the deleted form; typography roles map to MigraDoc styles from the `Theme/typography.md` role rows at composition so a report style never re-mints font literals; drafting's paginated flow reports and the diagnostics report-PDF (`EvidenceReport.Blocks` feeding this arm) compose `FlowReport.Render` with their own block seqs, while the drafting sheet-PDF is capture's vector-print arm — a sibling-page PDF writer is the deleted form; the page geometry is the `ReportSetup` policy row applied once to the section `PageSetup`, never per-block layout literals. The MigraDoc NATIVE chart DOM (`Shapes.Charts.Chart`) is a stated CARVE and stays unreached: a report chart enters as a `PlacedVisual` raster encoded through the capture codec axis, because the chart plane's own grammar — the layered series algebra, the paint resolver, the threshold family, the annotation plane, and the legend split — has no representation in the MigraDoc chart DOM, so routing a report chart through it would mean maintaining a second, weaker chart vocabulary whose output disagreed with the same chart on screen. The carve costs vector text inside a chart and buys one chart authority; a report chart that must stay vector is a `PlacedVisual` of a vector-print page rather than a native chart object.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReportBlock {
    private ReportBlock() { }
    public sealed record Heading(int Level, string Text) : ReportBlock;
    public sealed record Body(string Text) : ReportBlock;
    public sealed record List(Seq<string> Items, bool Ordered) : ReportBlock;
    public sealed record Callout(int HeadingLevel, string Title, Seq<ReportBlock> Blocks) : ReportBlock;
    public sealed record Code(string Language, string Source) : ReportBlock;
    public sealed record Table(Seq<Seq<string>> Rows, bool Header) : ReportBlock;
    public sealed record PlacedVisual(SKImage Tile, double WidthCm) : ReportBlock;
    public sealed record Figure(SKImage Tile, double WidthCm, string AltText, Option<string> Caption) : ReportBlock;
    public sealed record Footnote(string Key, string Text) : ReportBlock;
    // A section carries its own heading LEVEL exactly as a callout does, so a section nested inside a section
    // bookmarks under it instead of flattening every group onto one rung of the outline.
    public sealed record Section(int Level, string Title, Seq<ReportBlock> Blocks) : ReportBlock;
    public sealed record Rule : ReportBlock;
    public sealed record PageBreak : ReportBlock;
}

// A composed heading holds its own layout NODE, because the outline binds bookmarks to the pages the layout
// engine chose and only node identity survives two sections that happen to carry one title.
public readonly record struct ReportHeading(int Level, string Text, Paragraph Node);

// Page geometry is one policy row applied once — a per-block layout literal is the rejected form; None
// on every column keeps the MigraDoc defaults, so an inert setup skips the PageSetup pass entirely.
public sealed record ReportSetup(Option<double> PageWidthCm, Option<double> PageHeightCm, Option<double> MarginCm, bool Landscape) {
    public static readonly ReportSetup Default = new(None, None, None, Landscape: false);
    public bool IsInert => PageWidthCm.IsNone && PageHeightCm.IsNone && MarginCm.IsNone && !Landscape;
}

public sealed record ReportSpec(
    string Title,
    Seq<ReportBlock> Blocks,
    Option<string> RunningHeader,
    bool PageNumbers,
    ReportSetup Setup,
    PdfPolicy Pdf,
    VisualDestination Destination,
    bool Bookmarks = true);

public static class FlowReport {
    public const string Kind = "document";

    public static IO<RenderReceipt> Render(VisualRuntime runtime, ReportSpec spec) =>
        from mark in IO.lift(runtime.Clocks.Mark)
        from payload in (IO.lift(() => Compose(spec))
            | @catch<IO, byte[]>(static _ => true, static error => IO.fail<byte[]>(new ExportFault.RenderFailed("flow-report", error.Message)))).As()
        from sealed_ in PdfPolicies.Apply(runtime, spec.Pdf, payload)
        from destination in ExportDelivery.Deliver(runtime, spec.Destination, sealed_)
        from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
        let receipt = new RenderReceipt(
            Kind, "pdf", runtime.ContentHash(sealed_), None, None, sealed_.LongLength,
            elapsed, runtime.Correlation, Optional(destination), VisualCodec.ColorPolicy.Display.Key)
        from _ in runtime.Sink(receipt)
        select receipt;

    static byte[] Compose(ReportSpec spec) {
        Document document = new();
        Section section = document.AddSection();
        if (!spec.Setup.IsInert) { ApplySetup(section.PageSetup, spec.Setup); }
        spec.RunningHeader.Iter(header => section.Headers.Primary.AddParagraph(header));
        if (spec.PageNumbers) {
            Paragraph footer = section.Footers.Primary.AddParagraph();
            footer.AddPageField();
            footer.AddText(" / ");
            footer.AddNumPagesField();
        }
        // Heading paragraphs are RETAINED as they are composed, because the outline needs the identity of
        // each heading node to find which page the layout engine put it on — recovering headings by text
        // after the fact would bind two identically titled sections to one bookmark.
        Seq<ReportHeading> headings = spec.Blocks.Fold(Seq<ReportHeading>(), (held, block) => held + Append(section, block));
        PdfDocumentRenderer renderer = new() { Document = document };
        // Both pre-render binds land here: ColorMode governs how each XColor is written and UAManager
        // emits structure with content, so neither survives a post-render pass over sealed streams.
        renderer.PdfDocument.Options.ColorMode = spec.Pdf.Color.Mode;
        if (spec.Pdf.TaggedUa) { _ = UAManager.ForDocument(renderer.PdfDocument); }
        renderer.RenderDocument();
        if (spec.Bookmarks) { Outlined(renderer, headings); }
        using MemoryStream sink = new();
        renderer.PdfDocument.Save(sink);
        return sink.ToArray();
    }

    // The bookmark outline: the layout engine already knows which page every rendered object landed on, so
    // the heading tree binds to real pages by asking it — a page count derived from block positions would
    // re-implement pagination the flow DOM owns. Nesting rides the heading LEVEL through a running stack, so
    // a level-three heading under a level-one attaches to the level-one's own child collection and a level
    // that skips a rung attaches to the nearest shallower ancestor rather than to the document root.
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

    // Appending returns the headings it minted, so the outline builds from the same pass that laid the
    // content out and no second walk of the block tree can disagree about which nodes are headings.
    static Seq<ReportHeading> Append(Section section, ReportBlock block) {
        switch (block) {
            case ReportBlock.Heading heading:
                return Seq(new ReportHeading(
                    int.Clamp(heading.Level, 1, 6), heading.Text,
                    section.AddParagraph(heading.Text, $"Heading{int.Clamp(heading.Level, 1, 6)}")));
            case ReportBlock.Body body: section.AddParagraph(body.Text); return Seq<ReportHeading>();
            case ReportBlock.List list:
                list.Items.Map(static (item, index) => (Item: item, Index: index))
                    .Iter(row => section.AddParagraph($"{(list.Ordered ? $"{row.Index + 1}." : "•")} {row.Item}"));
                return Seq<ReportHeading>();
            // A TITLED callout heads its group and enters the outline; an untitled one is a plain block group —
            // a quoted passage, an aside — and mints neither a heading paragraph nor a bookmark, because an
            // empty bookmark is a row a reader clicks to reach nothing and every blockquote would mint one.
            case ReportBlock.Callout callout:
                return (string.IsNullOrWhiteSpace(callout.Title)
                        ? Seq<ReportHeading>()
                        : Seq(new ReportHeading(
                            int.Clamp(callout.HeadingLevel, 1, 6), callout.Title,
                            section.AddParagraph(callout.Title, $"Heading{int.Clamp(callout.HeadingLevel, 1, 6)}"))))
                    + callout.Blocks.Fold(Seq<ReportHeading>(), (held, child) => held + Append(section, child));
            case ReportBlock.Code code: section.AddParagraph(code.Source); return Seq<ReportHeading>();
            case ReportBlock.Table table: AppendTable(section, table); return Seq<ReportHeading>();
            case ReportBlock.PlacedVisual visual: AppendVisual(section, visual); return Seq<ReportHeading>();
            case ReportBlock.Figure figure:
                AppendVisual(section, new ReportBlock.PlacedVisual(figure.Tile, figure.WidthCm));
                section.AddParagraph(figure.Caption.IfNone(figure.AltText));
                return Seq<ReportHeading>();
            case ReportBlock.Footnote footnote:
                section.AddParagraph($"[{footnote.Key}] {footnote.Text}");
                return Seq<ReportHeading>();
            case ReportBlock.Section group:
                return Seq(new ReportHeading(
                        int.Clamp(group.Level, 1, 6), group.Title,
                        section.AddParagraph(group.Title, $"Heading{int.Clamp(group.Level, 1, 6)}")))
                    + group.Blocks.Fold(Seq<ReportHeading>(), (held, child) => held + Append(section, child));
            case ReportBlock.Rule: section.AddParagraph().Format.Borders.Bottom.Width = 0.5; return Seq<ReportHeading>();
            case ReportBlock.PageBreak: section.AddPageBreak(); return Seq<ReportHeading>();
            default: return Seq<ReportHeading>();
        }
    }

    // Column count is the MAX cell count across ALL rows — a ragged or empty-first-row spec never
    // under-sizes the grid; a table with zero cells renders nothing instead of an invalid zero-column table.
    static void AppendTable(Section section, ReportBlock.Table block) {
        int width = block.Rows.Fold(0, static (max, cells) => Math.Max(max, cells.Count));
        if (width == 0) { return; }
        MigraDoc.DocumentObjectModel.Tables.Table table = section.AddTable();
        for (var column = 0; column < width; column++) { table.AddColumn(); }
        block.Rows.Map(static (cells, index) => (Cells: cells, Index: index)).Iter(entry => {
            MigraDoc.DocumentObjectModel.Tables.Row row = table.AddRow();
            row.HeadingFormat = block.Header && entry.Index == 0;
            entry.Cells.Map(static (value, column) => (Value: value, Column: column))
                .Iter(cell => row.Cells[cell.Column].AddParagraph(cell.Value));
        });
    }

    // One PageSetup pass per section: dimension and margin columns land in centimeters.
    static void ApplySetup(MigraDoc.DocumentObjectModel.PageSetup setup, ReportSetup policy) {
        policy.PageWidthCm.Iter(width => setup.PageWidth = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(width));
        policy.PageHeightCm.Iter(height => setup.PageHeight = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(height));
        policy.MarginCm.Iter(margin => {
            MigraDoc.DocumentObjectModel.Unit edge = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(margin);
            setup.TopMargin = edge; setup.BottomMargin = edge; setup.LeftMargin = edge; setup.RightMargin = edge;
        });
        if (policy.Landscape) { setup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape; }
    }

    static void AppendVisual(Section section, ReportBlock.PlacedVisual visual) {
        using SKData encoded = visual.Tile.Encode(SKEncodedImageFormat.Png, 100);
        MigraDoc.DocumentObjectModel.Shapes.Image image = section.AddImage($"base64:{Convert.ToBase64String(encoded.AsSpan())}");
        image.Width = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(visual.WidthCm);
    }
}
```

## [04]-[PDF_POLICY]

- Owner: `PdfPolicy` — the one PDF-hardening policy row; `ColorTarget` [SmartEnum] — the ONE colour-model row both PDF egress legs read; `PdfIdentity` — the document-information identity columns every sealed artifact carries beside its content hash; `PdfAnnotation` [Union] · `PdfAnnotations` — the cross-reference vocabulary and its page decoration; `PdfPolicies` — the apply fold over the rendered payload.
- Cases: `ColorTarget` = screen · press · press-deep; `PdfAnnotation` = Link · Destination · Reference — the backend's whole honored-key roster, closed by it rather than by preference.
- Entry: `public static IO<byte[]> Apply(VisualRuntime runtime, PdfPolicy policy, byte[] rendered)` — IO rail; opens the rendered payload through `PdfReader`, applies the enabled arms, and re-saves; `public static Func<SKCanvas, Fin<Unit>> Decorate(params ReadOnlySpan<PdfAnnotation> rows)` — one page decoration folding straight into the `Render/capture#VECTOR_PRINT` `VisualExportSpec.Pages` seq, so arity rides the span and a single cross-reference and a whole sheet's roster take the one call.
- Auto: the colour model is the `ColorTarget` row alone — `PdfDocumentOptions.ColorMode` takes its `Mode` on the renderer document before content materializes while `[06]-[PRINT_ARM]` takes the same row's `Device`, buffer formats, and pixel strides, so a screen export and a proofed press export differ by this value and never by a second code path, and an implicit `PdfColorMode.Undefined` device default cannot arise; the security arm selects AES-256 through `PdfDocument.SecurityHandler.SetEncryptionToV5(bool encryptMetadata)` and applies permissions through `PdfDocument.SecuritySettings`; identity writes `PdfDocumentInformation`; signatures compose `DigitalSignatureHandler.ForDocument`; and AcroForm rows write through the catalogued field surface. `TaggedUa` attaches `UAManager` to the renderer document before `RenderDocument`, so structure emits with content. Annotations ride Skia's own annotation entrypoints on the paged canvas — `DrawUrlAnnotation`, `DrawNamedDestinationAnnotation`, and `DrawLinkDestinationAnnotation` mint the `SKData` the native annotation record retains, so an exported sheet's outbound links and internal cross-references survive as real PDF annotations rather than drawn text.
- Packages: PDFsharp, SkiaSharp, Rasm.AppHost (project), LanguageExt.Core
- Growth: a new hardening concern is one `PdfPolicy` field; a new permission is one column value; a new identity column is one `PdfIdentity` member; a new colour model is one `ColorTarget` row both legs read; the cross-reference family grows only when the PDF backend honors a fourth key, so growth here is a widened region admission rather than a fourth case; zero new surface.
- Boundary: the signing-credential crossing is a declared ledger row (`Document/export` -> AppHost `Runtime/secrets.md`). PDF-UA tagging and the colour model both bind before content materialization — `PdfDocumentOptions.ColorMode` governs how each `XColor` is WRITTEN, so a post-render pass setting it re-saves already-written content streams and governs nothing; the post-render policy pass therefore applies security, identity, forms, and signatures alone, without re-tagging or re-colouring the document. `ColorTarget` is the ONE colour-model authority for the whole page — a `PdfColorMode` literal at a render site, a second CMYK selector on the print arm, and a `bool cmyk` knob beside a spec are the three deleted forms. Annotations are page-composition content, so they enter through the capture vector-print page fold and never through the post-render `PdfReader` pass; the PDF backend honors exactly three annotation keys and each is reached through the named Skia entrypoint that passes it, so the family is CLOSED at three cases and the raw `DrawAnnotation(rect, key, value)` passthrough is the deleted form — an unhonored key returns void with no diagnostic, making an arbitrary-key case a silent no-op wearing a capability's name. Region shape is the backend's own discriminant: a named destination is DEFINED at a point, which is the zero-extent rect the backend requires, while an outbound url and an internal link carry a real rect, so a zero-area region on either rect-bearing case refuses at admission rather than drawing an annotation no reader can hit.

```csharp signature
// Colour is ONE row read by both PDF egress legs: PDFsharp's document colour model and the print
// transform's device space, buffer formats, and pixel strides travel together, so a screen export and
// a proofed press export differ by this value alone. Stride rides the row beside its format word
// because lcms packs bytes-per-channel into that word behind private shifters, so whichever row chooses
// a format is the one seat that can state its width.
[SmartEnum<string>]
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

    // Pixel count is the raster's own byte length over the input stride, so a raster whose length is
    // not a whole number of pixels refuses here rather than transforming a truncated final pixel.
    public Fin<int> Pixels(int rasterBytes) =>
        rasterBytes > 0 && rasterBytes % InputStride == 0
            ? Fin.Succ(rasterBytes / InputStride)
            : Fin.Fail<int>(new ExportFault.RenderFailed("raster", $"{rasterBytes} bytes is not whole {Key} pixels"));
}

// Identity metadata beside the content hash: every sealed export names itself through the catalogued
// PdfDocumentInformation columns; an inert identity skips the modify pass entirely.
public sealed record PdfIdentity(Option<string> Title, Option<string> Author, Option<string> Subject, Option<string> Keywords) {
    public static readonly PdfIdentity Inert = new(Option<string>.None, Option<string>.None, Option<string>.None, Option<string>.None);
    public bool IsInert => Title.IsNone && Author.IsNone && Subject.IsNone && Keywords.IsNone;
}

public sealed record PdfPolicy(
    ColorTarget Color,
    bool EncryptAes256,
    bool EncryptMetadata,
    Option<string> OwnerPasswordLease,
    bool AllowPrinting,
    bool AllowExtraction,
    Option<IDigitalSigner> Signer,
    Option<DigitalSignatureOptions> SignatureOptions,
    Seq<(string Field, string Value)> AcroFields,
    bool TaggedUa,
    PdfIdentity Identity) {

    public static readonly PdfPolicy Plain = new(ColorTarget.Screen, false, false, None, true, true, None, None, [], false, PdfIdentity.Inert);
    public static readonly PdfPolicy Archival = Plain with { TaggedUa = true };
    public static readonly PdfPolicy Press = Plain with { Color = ColorTarget.Press };
}

// PDF cross-references are Skia's OWN annotation records on the paged canvas: a rect carrying an
// outbound url, a point DEFINING a named destination, and a rect LINKING to one. The PDF device honors
// exactly these three annotation keys, each reached through the named entrypoint that passes it, so the
// family is closed by the backend — a fourth case could only carry a key the device drops in silence.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PdfAnnotation {
    private PdfAnnotation() { }
    public sealed record Link(SKRect Region, string Url) : PdfAnnotation;
    public sealed record Destination(SKPoint At, string Name) : PdfAnnotation;
    public sealed record Reference(SKRect Region, string Name) : PdfAnnotation;
}

public static class PdfAnnotations {
    // Decoration is a page fold, not a document pass: it returns the exact shape the vector-print arm's
    // Pages seq admits, so a sheet composes its content fold and its cross-reference fold with no
    // second paging capsule and no second SKDocument owner.
    public static Func<SKCanvas, Fin<Unit>> Decorate(params ReadOnlySpan<PdfAnnotation> rows) {
        // Exemption: the params buffer is implicitly scoped and cannot escape into the returned fold,
        // so the roster detaches into owned storage on the one line before the closure captures it.
        Seq<PdfAnnotation> admitted = toSeq(rows.ToArray());
        return canvas => admitted.Fold(Fin.Succ(unit), (rail, row) => rail.Bind(_ => Draw(canvas, row)));
    }

    // The point-bearing destination carries the backend's required zero-extent rect by construction, so
    // it presents no region to prove and only the two rect-bearing cases carry one.
    static Fin<Unit> Draw(SKCanvas canvas, PdfAnnotation row) => row.Switch(
        state: canvas,
        link:        static (c, l) => Minted("link", l.Url, Some(l.Region), () => c.DrawUrlAnnotation(l.Region, l.Url)),
        destination: static (c, d) => Minted("destination", d.Name, None, () => c.DrawNamedDestinationAnnotation(d.At, d.Name)),
        reference:   static (c, r) => Minted("reference", r.Name, Some(r.Region), () => c.DrawLinkDestinationAnnotation(r.Region, r.Name)));

    // Skia's string-shaped entrypoints MINT the SKData their native annotation record retains, so this
    // managed lease scopes to the draw and its `using` is the fold's one platform-forced statement
    // seam. A blank target draws a dead annotation and a zero-area rect draws one no reader can hit, so
    // both refuse at admission instead. Emptiness is measured on the extents rather than `SKRect.IsEmpty`,
    // which equals the origin rect alone and admits every other degenerate rect.
    static Fin<Unit> Minted(string kind, string target, Option<SKRect> region, Func<SKData> draw) {
        if (string.IsNullOrWhiteSpace(target)) { return Fin.Fail<Unit>(new ExportFault.AnnotationRejected(kind, "target is blank")); }
        if (region.Exists(static rect => rect.Width <= 0f || rect.Height <= 0f)) {
            return Fin.Fail<Unit>(new ExportFault.AnnotationRejected(kind, "region has no area"));
        }
        using SKData lease = draw();
        return Fin.Succ(unit);
    }
}

public static class PdfPolicies {
    // PdfPolicies folds native failures typed: a throw with a signer bound classifies
    // SignerUnavailable (credential lease and crypto path), anything else RenderFailed("pdf-policy").
    // Colour is absent from this pass by construction — the row bound the renderer before content
    // materialized, and re-stating it over written content streams would govern nothing.
    public static IO<byte[]> Apply(VisualRuntime runtime, PdfPolicy policy, byte[] rendered) =>
        policy is { EncryptAes256: false, Signer.IsNone: true, AcroFields.IsEmpty: true, Identity.IsInert: true }
            ? IO.pure(rendered)
            : (Modify(policy, rendered)
                | @catch<IO, byte[]>(static _ => true, error => IO.fail<byte[]>(policy.Signer.IsSome
                    ? new ExportFault.SignerUnavailable(error.Message)
                    : new ExportFault.RenderFailed("pdf-policy", error.Message)))).As();

    static IO<byte[]> Modify(PdfPolicy policy, byte[] rendered) =>
        IO.lift(() => {
            using MemoryStream source = new(rendered);
            using PdfDocument document = PdfReader.Open(source, PdfDocumentOpenMode.Modify);
            policy.Identity.Title.Iter(title => document.Info.Title = title);
            policy.Identity.Author.Iter(author => document.Info.Author = author);
            policy.Identity.Subject.Iter(subject => document.Info.Subject = subject);
            policy.Identity.Keywords.Iter(keywords => document.Info.Keywords = keywords);
            // A null-conditional member access is not an assignment target, so the field resolves onto the
            // presence rail first and the write lands inside the `Iter` — an absent AcroForm or an unnamed
            // field is a no-op write rather than a spelling that does not compile.
            policy.AcroFields.Iter(field => Optional(document.AcroForm)
                .Bind(form => Optional(form.Fields[field.Field]))
                .Iter(target => target.Value = new PdfString(field.Value)));
            if (policy.EncryptAes256) {
                document.SecurityHandler.SetEncryptionToV5(policy.EncryptMetadata);
                policy.OwnerPasswordLease.Iter(lease => document.SecuritySettings.OwnerPassword = lease);
                document.SecuritySettings.PermitPrint = policy.AllowPrinting;
                document.SecuritySettings.PermitExtractContent = policy.AllowExtraction;
            }
            using MemoryStream sink = new();
            // ForDocument ATTACHES the signing handler to the document; the subsequent Save computes
            // and embeds the signature — the handler exposes no save verb of its own.
            policy.Signer.Iter(signer => _ = DigitalSignatureHandler.ForDocument(
                document, signer, policy.SignatureOptions.IfNone(() => new DigitalSignatureOptions())));
            document.Save(sink);
            return sink.ToArray();
        });
}
```

## [05]-[OFFICE_ARM]

- Owner: `OfficeFormat` [SmartEnum] · `OfficeSpec` · `OfficeSheet` [Union] · `OfficeFidelity` the per-(format × case) materialization vocabulary carrying its own materialization column · `OfficeCell` the admitted sheet and its decided preface · `OfficeExport` — the OOXML part-graph arm.
- Cases: `OfficeFormat` = xlsx · pptx · docx; `OfficeSheet` = Table · Chart · Image · RichText; `OfficeFidelity` = Native · Declared · Unsupported.
- Entry: `public static IO<RenderReceipt> Emit(VisualRuntime runtime, OfficeSpec spec)` — the Office IO rail; admission runs the fidelity matrix over every sheet FIRST, so an `Unsupported` combination folds to `ExportFault.ContentUnsupported` before any part writes and every admitted sheet hands the write its own materialization row.
- Auto: XLSX writes through `SpreadsheetDocument.Create` and its workbook/worksheet part graph; DOCX writes through `WordprocessingDocument.Create` and its main-document part graph. PPTX remains a typed `Unsupported` fidelity row until the presentation/master/layout/slide members are catalogued; no speculative part graph survives in the body.
- Receipt: one `RenderReceipt` of kind office per emit with whole-payload content hash and the delivered destination key.
- Packages: DocumentFormat.OpenXml, SkiaSharp, Rasm.AppHost (project), NodaTime, LanguageExt.Core
- Growth: one `OfficeFormat` row admits an Office target and one `OfficeSheet` case admits a content kind; a fidelity promotion is one matrix cell flipped as the verified part members land; zero new surface.
- Boundary: the Office destination is the same `VisualDestination` union. The fidelity row CARRIES its materialization rather than naming it in prose — `Native` cells materialize their own part vocabulary, `Declared` cells preface the projection they state into the produced document, and `Unsupported` cells reject through `ExportFault.ContentUnsupported` — so the matrix cell is the dispatch, the admission's product drives the write, and a fidelity read as a bare inequality against one row is the deleted form; every PPTX cell and every image cell takes that typed rejection path.

```csharp signature
[SmartEnum<string>]
public sealed partial class OfficeFormat {
    public static readonly OfficeFormat Xlsx = new("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    public static readonly OfficeFormat Pptx = new("pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
    public static readonly OfficeFormat Docx = new("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

    public string MediaType { get; }
}

public sealed record OfficeSpec(
    OfficeFormat Format,
    Seq<OfficeSheet> Sheets,
    Seq<(string FontFamily, ReadOnlyMemory<byte> Face)> EmbeddedFonts,
    VisualDestination Destination);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OfficeSheet {
    private OfficeSheet() { }
    public sealed record Table(string Name, Seq<Seq<string>> Rows, bool Header) : OfficeSheet;
    public sealed record Chart(string Name, ChartSeriesSpec Spec, Seq<(double X, double Y)> Points) : OfficeSheet;
    public sealed record Image(string Name, SKImage Picture) : OfficeSheet;
    public sealed record RichText(string Name, Seq<ReportBlock> Blocks) : OfficeSheet;

    public string Kind => Switch(
        table: static _ => "table", chart: static _ => "chart", image: static _ => "image", richText: static _ => "richText");
}

// The fidelity row IS the materialization: `Native` names the format's own part vocabulary, `Declared`
// names a stated projection that prefaces itself in the produced document so a reader never mistakes a
// flattening for the native shape, and `Unsupported` names the refusal. One reader serves all three rows
// and the matrix cell is the dispatch — a vocabulary whose only consumer tested it for inequality against
// one row governed nothing, and `Native` and `Declared` were indistinguishable at every write site.
[SmartEnum]
public sealed partial class OfficeFidelity {
    public static readonly OfficeFidelity Native = new(
        materialized: static (_, _) => Fin.Succ(Option<string>.None));
    public static readonly OfficeFidelity Declared = new(
        materialized: static (format, sheet) => Fin.Succ(Some($"{sheet} projected into {format}")));
    public static readonly OfficeFidelity Unsupported = new(
        materialized: static (format, sheet) => Fin.Fail<Option<string>>(new ExportFault.ContentUnsupported(format, sheet)));

    [UseDelegateFromConstructor]
    public partial Fin<Option<string>> Materialized(string format, string sheet);
}

// The admitted cell: its sheet and the preface its fidelity row decided, so the write consumes admission's
// own product instead of re-deciding a verdict the admission pass already took.
public readonly record struct OfficeCell(OfficeSheet Sheet, Option<string> Preface);

public static class OfficeExport {
    public const string Kind = "office";

    // One cell per (format × sheet-kind) pair naming which materialization that combination gets; a
    // promotion is one cell flipped to a stronger row when the part members verify.
    static readonly FrozenDictionary<(string Format, string Sheet), OfficeFidelity> Support = new Dictionary<(string, string), OfficeFidelity> {
        [("xlsx", "table")] = OfficeFidelity.Native,
        [("xlsx", "chart")] = OfficeFidelity.Declared,
        [("xlsx", "richText")] = OfficeFidelity.Declared,
        [("xlsx", "image")] = OfficeFidelity.Unsupported,
        [("docx", "table")] = OfficeFidelity.Declared,
        [("docx", "chart")] = OfficeFidelity.Declared,
        [("docx", "richText")] = OfficeFidelity.Native,
        [("docx", "image")] = OfficeFidelity.Unsupported,
        [("pptx", "table")] = OfficeFidelity.Unsupported,
        [("pptx", "chart")] = OfficeFidelity.Unsupported,
        [("pptx", "richText")] = OfficeFidelity.Unsupported,
        [("pptx", "image")] = OfficeFidelity.Unsupported,
    }.ToFrozenDictionary();

    // Admission is a settled pure fold, so it lifts through the `IO.lift(Fin<A>)` rail entry — the thunk
    // overloads take `Func<A>` and `Func<Fin<A>>`, which a `Fin`-returning lambda matches identically, so
    // the deferred spelling was ambiguous and its trailing `Match` re-folded a rail `IO` already carries.
    public static IO<RenderReceipt> Emit(VisualRuntime runtime, OfficeSpec spec) =>
        from admitted in IO.lift(Admitted(spec))
        from mark in IO.lift(runtime.Clocks.Mark)
        from payload in Write(spec, admitted)
        from destination in ExportDelivery.Deliver(runtime, spec.Destination, payload)
        from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
        let receipt = new RenderReceipt(
            Kind, spec.Format.Key, runtime.ContentHash(payload), None, None, payload.LongLength,
            elapsed, runtime.Correlation, Optional(destination), VisualCodec.ColorPolicy.Display.Key)
        from _ in runtime.Sink(receipt)
        select receipt;

    // Total generated dispatch over the closed format vocabulary — a new OfficeFormat row breaks this
    // site at compile time; a part-graph throw folds typed PartGraphRejected, never an untyped Error.
    static IO<byte[]> Write(OfficeSpec spec, Seq<OfficeCell> cells) =>
        spec.Format.Switch(
            state: (Spec: spec, Cells: cells),
            xlsx: static ctx => (IO.lift(() => WriteXlsx(ctx.Cells))
                | @catch<IO, byte[]>(static _ => true, static error => IO.fail<byte[]>(new ExportFault.PartGraphRejected("xlsx", error.Message)))).As(),
            pptx: static _ => IO.fail<byte[]>(new ExportFault.ContentUnsupported("pptx", "catalogued part graph")),
            docx: static ctx => (IO.lift(() => WriteDocx(ctx.Spec, ctx.Cells))
                | @catch<IO, byte[]>(static _ => true, static error => IO.fail<byte[]>(new ExportFault.PartGraphRejected("docx", error.Message)))).As());

    // The workbook part graph carries no font-embedding part, so the spreadsheet writer takes the admitted
    // cells alone: an `EmbeddedFonts` argument threaded here would be a column this format structurally
    // cannot honour, which the preflight already reports as the format's own absent capability.
    static byte[] WriteXlsx(Seq<OfficeCell> cells) {
        using MemoryStream sink = new();
        using (SpreadsheetDocument doc = SpreadsheetDocument.Create(sink, SpreadsheetDocumentType.Workbook)) {
            WorkbookPart workbook = doc.AddWorkbookPart();
            workbook.Workbook = new Workbook();
            Sheets sheets = workbook.Workbook.AppendChild(new Sheets());
            // Indexed instance Map is (value, index) — the module spelling transposes; Iter carries no index.
            // The admitted cell's preface leads its rows, so a Declared projection states itself in the
            // produced sheet and a Native one writes its parts with no banner.
            cells.Map(static (cell, index) => (Cell: cell, Index: index)).Iter(row => {
                WorksheetPart part = workbook.AddNewPart<WorksheetPart>();
                SheetData data = new();
                (row.Cell.Preface.Map(TextRow).ToSeq() + Rows(row.Cell.Sheet)).Iter(cell => data.Append(cell));
                part.Worksheet = new Worksheet(data);
                sheets.Append(new Sheet { Id = workbook.GetIdOfPart(part), SheetId = (uint)(row.Index + 1), Name = SheetName(row.Cell.Sheet) });
            });
            workbook.Workbook.Save();
        }
        return sink.ToArray();
    }

    static byte[] WriteDocx(OfficeSpec spec, Seq<OfficeCell> cells) {
        using MemoryStream sink = new();
        using (WordprocessingDocument doc = WordprocessingDocument.Create(sink, WordprocessingDocumentType.Document)) {
            MainDocumentPart main = doc.AddMainDocumentPart();
            Body body = new();
            cells.Iter(cell => (cell.Preface.Map(static text => new Paragraph(new Run(new Text(text)))).ToSeq()
                + Paragraphs(cell.Sheet)).Iter(paragraph => body.Append(paragraph)));
            main.Document = new Document(body);
            EmbedFonts(main, spec.EmbeddedFonts);
            main.Document.Save();
        }
        return sink.ToArray();
    }

    // The matrix read fails CLOSED and its PRODUCT drives the write: an uncatalogued (format, kind) cell
    // reads Unsupported, so a new `OfficeFormat` row or `OfficeSheet` case rejects typed until its cells
    // land instead of throwing a key-not-found out of an indexer the growth line invites a caller to
    // outrun, and an admitted cell carries its fidelity's own preface forward rather than leaving the
    // verdict for a serializer to re-decide.
    static Fin<Seq<OfficeCell>> Admitted(OfficeSpec spec) =>
        spec.Sheets.TraverseM(sheet =>
            (Support.TryGetValue((spec.Format.Key, sheet.Kind), out OfficeFidelity? fidelity)
                ? fidelity
                : OfficeFidelity.Unsupported)
            .Materialized(spec.Format.Key, SheetName(sheet))
            .Map(preface => new OfficeCell(sheet, preface))).As();

    // Total dispatch per serializer: every case has an explicit arm; the image arm is unreachable past
    // admission (its matrix cell is Unsupported) and stated so, never a silent catch-all.
    static Seq<Row> Rows(OfficeSheet sheet) => sheet.Switch(
        table: static t => t.Rows.Map(CellsRow),
        chart: static c => TextRow(c.Name).Cons(c.Points.Map(PointRow)),
        image: static _ => Seq<Row>(),
        richText: static r => r.Blocks.Bind(BlockRow));

    static Seq<Row> BlockRow(ReportBlock block) => block.Switch(
        heading: static h => Seq(TextRow(h.Text)),
        body: static b => Seq(TextRow(b.Text)),
        list: static l => l.Items.Map(TextRow),
        callout: static c => TextRow(c.Title).Cons(c.Blocks.Bind(BlockRow)),
        code: static c => Seq(TextRow(c.Source)),
        table: static t => t.Rows.Map(cells => TextRow(string.Join('\t', cells))),
        placedVisual: static _ => Seq<Row>(),
        figure: static f => f.Caption.Map(TextRow).ToSeq(),
        footnote: static f => Seq(TextRow($"[{f.Key}] {f.Text}")),
        section: static s => TextRow(s.Title).Cons(s.Blocks.Bind(BlockRow)),
        rule: static _ => Seq<Row>(),
        pageBreak: static _ => Seq<Row>());

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

    static Seq<Paragraph> Paragraphs(OfficeSheet sheet) => sheet.Switch(
        table: static t => t.Rows.Map(static cells => new Paragraph(new Run(new Text(string.Join('\t', cells))))),
        chart: static c => new Paragraph(new Run(new Text(c.Name)))
            .Cons(c.Points.Map(static point => new Paragraph(new Run(new Text($"{point.X}\t{point.Y}"))))),
        image: static _ => Seq<Paragraph>(),
        richText: static r => r.Blocks.Bind(BlockParagraph));

    static Seq<Paragraph> BlockParagraph(ReportBlock block) => block.Switch(
        heading: static h => Seq(new Paragraph(new Run(new Text(h.Text)))),
        body: static b => Seq(new Paragraph(new Run(new Text(b.Text) { Space = SpaceProcessingModeValues.Preserve }))),
        list: static l => l.Items.Map(static item => new Paragraph(new Run(new Text(item)))),
        callout: static c => new Paragraph(new Run(new Text(c.Title))).Cons(c.Blocks.Bind(BlockParagraph)),
        code: static c => Seq(new Paragraph(new Run(new Text(c.Source) { Space = SpaceProcessingModeValues.Preserve }))),
        table: static t => t.Rows.Map(static cells => new Paragraph(new Run(new Text(string.Join('\t', cells))))),
        placedVisual: static _ => Seq<Paragraph>(),
        figure: static f => f.Caption.Map(static caption => new Paragraph(new Run(new Text(caption)))).ToSeq(),
        footnote: static f => Seq(new Paragraph(new Run(new Text($"[{f.Key}] {f.Text}")))),
        section: static s => new Paragraph(new Run(new Text(s.Title))).Cons(s.Blocks.Bind(BlockParagraph)),
        rule: static _ => Seq<Paragraph>(),
        pageBreak: static _ => Seq<Paragraph>());

    static string SheetName(OfficeSheet sheet) =>
        sheet.Switch(table: static t => t.Name, chart: static c => c.Name, image: static i => i.Name, richText: static r => r.Name);

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

- Owner: `PrintIntent` [SmartEnum] — the rendering-intent policy rows; `PressCeiling` [ValueObject] — the admitted total-area-coverage ceiling; `GamutAlarm` [ComplexValueObject] — the per-channel out-of-gamut marking colour; `PrintTransform` — the lcmsNET transform row; `PrintProof` — the typed proving receipt; `PrintLink` — one chain link with its own policy columns; `PrintPlate` — the converted pixels beside their proof; `PrintArm` — the device-CMYK conversion surface.
- Cases: `PrintIntent` = perceptual · relative-colorimetric · saturation · absolute-colorimetric · relative-bpc · preserve-k — K preservation, black-point compensation, and adaptation state are policy columns, never flags scattered at call sites.
- Entry: `public static IO<PrintPlate> Convert(VisualRuntime runtime, PrintTransform row, ReadOnlyMemory<byte> raster)` — IO rail; one `Context`, one proving fold, one extended `Transform.Create`, one `DoTransform` per payload.
- Auto: `PrintArm` owns its own lcmsNET `Context`, opens every profile on it, and writes the `GamutAlarm` codes to `Context.AlarmCodes` — the per-context instance property, never the `Cms.AlarmCodes` process-global twin, so one proofing arm's marking colour never leaks into a concurrent conversion; the alarm width is READ back off the context vector lcms itself sizes, so the native fixed-width refusal is unreachable. Proving runs BEFORE the build: each profile answers `IsIntentSupported` in the direction the chain uses it (source as input, destination as output, proof as proof) and an unsupported intent is `ExportFault.IntentUnsupported`, never a silent fallback; `Profile.TotalAreaCoverage` measures the destination's own coverage and a measurement above the admitted `PressCeiling` mints `Profile.CreateInkLimitingDeviceLink` on the arm's context as the chain's tail link; `DetectDestinationBlackPoint` decides whether the intent row's declared black-point compensation changes anything. One extended `Transform.Create(Context, Profile[], bool[], Intent[], double[], Profile, int, uint, uint, CmsFlags)` builds every case — per-link BPC, per-link intent, per-link adaptation, the gamut operand, and the ink-limit tail are columns of one build, so limited, unlimited, proofed, and plain conversions are one code path. Soft proofing is CHAIN SEATING, never a flag over a gamut operand: the press profile enters the chain TWICE — into-press under the document intent, back out under `Intent.RelativeColorimetric` — and the destination link then renders that simulation under `ProofIntent`, which is the chain lcms's own proofing entry expands to, admitted by `CmsFlags.SoftProofing`. The gamut operand is read only under `CmsFlags.GamutCheck`, where it builds a SEPARATE alarm lookup overwriting out-of-gamut pixels with the `GamutAlarm` codes; preview and marking are therefore independent columns of one build, and the alarm reads the PROOF gamut so an alarm with no proofing profile refuses at proving rather than resolving a flag lcms drops. Native lcms2 ships with the app.
- Receipt: one `RenderReceipt` of kind print per conversion whose `ColorSpace` field carries the `PrintTransform` row key — the identity naming source, destination, intent, and ceiling together — so a print baseline keys distinctly, and one `PrintProof` carrying the measured coverage, the admitted ceiling, the ink-limit verdict, the detected destination black point, and the resolved flag set — every field a run fact, and an undetectable black point is `None`, never a zero.
- Packages: lcmsNET, Rasm.AppHost (project), LanguageExt.Core
- Growth: a new intent is one `PrintIntent` row; a new device profile is one `PrintTransform` value from profile bytes; a new buffer depth is one `ColorTarget` row; a new chain stage is one `PrintLink` row the four build vectors project from; zero new surface.
- Boundary: lcmsNET owns device-CMYK/ICC transforms at the print boundary ONLY — Unicolour stays the suite color-model kernel and `VisualCodec.ColorPolicy` stays the capture codec gamut family, three disjoint charters; an unparseable profile folds to `ExportFault.ProfileInvalid`, never a silent sRGB fallback; buffer formats and pixel strides are the `[04]` `ColorTarget` row's columns, so a `Cms.TYPE_*` literal at this site and a `rgba.Length / 4` pixel count are the two deleted forms the 16-bit lane made wrong; the press simulation lives in the CHAIN and the gamut operand checks alone, so handing the proofing profile to the gamut slot as the simulation is the deleted form — it drops the preview entirely under anything but `CmsFlags.GamutCheck`, and neither flag set at all collapses the build to a bare source-to-destination conversion carrying no proof; per-link BPC, intent, and adaptation are columns of the same `PrintLink` row so the four positional vectors project from one ordered set and cannot fall out of step with the profiles beside them; every handle — `Context`, each `Profile`, the `Transform` — is a `CmsHandle<T>` over `SafeHandle` released in the kernel's `finally`, and the context releases last because every profile opened on it borrows its scope.

```csharp signature
// Intent carries its own colorimetric policy: the rendering intent, whether black-point compensation is
// ADMITTED for this intent, and the adaptation state lcms consumes on the absolute-colorimetric link
// alone (0 is true unadapted absolute; the other rows declare complete adaptation as the neutral).
// Compensation rides the extended build's per-link bpc vector, so no CmsFlags column restates it.
[SmartEnum<string>]
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

// Press ceilings are ADMITTED total-area coverage on lcms's own percentage scale, so a measured
// TotalAreaCoverage and its ceiling compare directly and no call site carries a bare 280 or 320.
[ValueObject<double>]
public readonly partial struct PressCeiling {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0d && value <= 400d
            ? validationError
            : new ValidationError("press ceiling is a finite total-area coverage percentage in (0, 400]");
}

// Out-of-gamut marking is DEVICE COLOUR, one code per destination channel — the proving fold refuses an
// alarm whose channel count disagrees with Cms.ChannelsOf(target.Device), and Mark folds those codes
// into whatever vector the context itself sizes, so lcms's fixed-width refusal has no reachable path.
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

// Proof-profile presence IS the soft-proof decision and alarm presence the gamut-check decision — two
// independent columns the flag set derives from, never boolean knobs beside them. The check reads the
// PROOF gamut, so an alarm without a proofing profile marks nothing and refuses at proving.
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

// Every field is a run fact the conversion measured: the destination's own coverage, the ceiling that
// admitted it, the detected destination black point, and the flag set the build resolved. An
// undetectable black point is None — never a fabricated zero. The ink-limit verdict DERIVES from the
// measured coverage against the admitted ceiling, so the chain and the receipt cannot disagree.
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
    public bool InkLimited => Ceiling.Exists(ceiling => AreaCoverage > (double)ceiling);
}

// One chain link carries its own policy columns, so the four positional vectors the extended build
// consumes project from ONE ordered row set — a profile array that grew a stage while a sibling intent
// vector did not is unrepresentable rather than a silent per-link misalignment.
public readonly record struct PrintLink(Profile Profile, Intent Rendering, bool BlackPoint, double Adaptation);

public sealed record PrintPlate(byte[] Pixels, PrintProof Proof);

public static class PrintArm {
    public const string Kind = "print";

    // Successful conversion seals one RenderReceipt through the runtime Sink with destination-profile
    // identity, content hash, byte count, and elapsed span, and hands the plate's typed proof to the
    // caller. Failed conversion stays on ExportFault with no success receipt.
    // The native kernel stays DEFERRED, so the explicit type argument selects the railed thunk overload
    // (`Func<Fin<A>>`) the bare call could not disambiguate against `Func<A>`, and its rail is the IO's own
    // — the trailing `Match` re-folded what the entry already carries. Elapsed reads through `IO.lift` on
    // the same clock crossing the report, office, and bundle arms take.
    public static IO<PrintPlate> Convert(VisualRuntime runtime, PrintTransform row, ReadOnlyMemory<byte> raster) =>
        from mark in IO.lift(runtime.Clocks.Mark)
        from plate in IO.lift<PrintPlate>(() => Transformed(row, raster))
        from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
        from _ in runtime.Sink(new RenderReceipt(
            Kind, $"{row.Target.Key}-{row.IntentRow.Key}", runtime.ContentHash(plate.Pixels), None, None, plate.Pixels.LongLength,
            elapsed, runtime.Correlation, None, row.Key))
        select plate;

    // lcms native boundary kernel — the page's ONE statement seam. The arm's own Context scopes every
    // handle; `stage` names the crossing in flight so each parse classifies its own ProfileInvalid
    // ordinal with no per-parse catch ladder, and every advertised fault case keeps a producing path.
    // One finally releases in reverse-acquisition order with the context last, because each profile
    // opened on it borrows its scope.
    static Fin<PrintPlate> Transformed(PrintTransform row, ReadOnlyMemory<byte> raster) {
        Context? context = null;
        Profile? source = null, destination = null, proofing = null;
        string stage = "context";
        try {
            context = Context.Create(IntPtr.Zero, IntPtr.Zero);
            stage = "source";
            source = Profile.Open(context, row.SourceProfile.Span.ToArray());
            stage = "destination";
            destination = Profile.Open(context, row.DestinationProfile.Span.ToArray());
            stage = "proof";
            proofing = row.ProofProfile.Match(
                Some: bytes => Profile.Open(context, bytes.Span.ToArray()),
                None: () => (Profile?)null);
            stage = "transform";
            return Proved(row, source, destination, proofing, raster.Length)
                .Bind(proof => Run(context, row, source, destination, proofing, proof, raster));
        }
        catch (Exception error) {
            return Fin.Fail<PrintPlate>(stage is "source" or "destination" or "proof"
                ? new ExportFault.ProfileInvalid($"{row.Key}:{stage}")
                : new ExportFault.RenderFailed("print", error.Message));
        }
        finally { proofing?.Dispose(); destination?.Dispose(); source?.Dispose(); context?.Dispose(); }
    }

    // Proving is a ROLE table, not a per-profile probe: lcms answers intent support in the direction the
    // chain uses each profile, so source, destination, and proof each prove their own row and the first
    // refusal names the role. Coverage, ceiling, and black point resolve here too, so the build receives
    // decided values and re-derives nothing.
    static Fin<PrintProof> Proved(PrintTransform row, Profile source, Profile destination, Profile? proofing, int rasterBytes) =>
        row.Target.Pixels(rasterBytes).Bind(pixels =>
            (Seq((Role: "source", Profile: source, Rendering: row.IntentRow.Rendering, Direction: UsedDirection.AsInput),
                 (Role: "destination", Profile: destination, Rendering: row.IntentRow.Rendering, Direction: UsedDirection.AsOutput))
             + (proofing is { } held
                 ? Seq((Role: "proof", Profile: held, Rendering: row.ProofIntent.Rendering, Direction: UsedDirection.AsProof))
                 : Seq<(string Role, Profile Profile, Intent Rendering, UsedDirection Direction)>()))
            .Find(static role => !role.Profile.IsIntentSupported(role.Rendering, role.Direction))
            .Match(
                Some: refused => Fin.Fail<PrintProof>(new ExportFault.IntentUnsupported(refused.Role, refused.Rendering.ToString())),
                // Alarm admission is a second refusal table on the same fold: the marking colour must
                // carry one code per destination channel, and it must have a proof gamut to check
                // against — an alarm with no proofing profile resolves a flag lcms silently drops.
                None: () => Seq(
                        (Refusal: "alarm-channels", Broken: row.Alarm.Exists(alarm => alarm.Ink.Count != (int)row.Target.Channels)),
                        (Refusal: "alarm-without-proof", Broken: row.Alarm.IsSome && proofing is null))
                    .Find(static gate => gate.Broken)
                    .Match(
                        Some: gate => Fin.Fail<PrintProof>(new ExportFault.ProfileInvalid($"{row.Key}:{gate.Refusal}")),
                        None: () => Fin.Succ(Resolved(row, destination, proofing, pixels)))));

    // Intent rows DECLARE black-point compensation and a detected destination black point decides
    // whether it changes anything: a destination reaching true zero black drops the compensation as the
    // no-op it is, while an undetectable black point leaves that declaration standing. The two flags are
    // independent: the proof profile admits the simulation chain, the alarm admits the check overlay, and
    // the proving fold has already refused an alarm that has no proof gamut to read.
    static PrintProof Resolved(PrintTransform row, Profile destination, Profile? proofing, int pixels) {
        Option<CIEXYZ> black = destination.DetectDestinationBlackPoint(out CIEXYZ detected, row.IntentRow.Rendering)
            ? Some(detected)
            : None;
        return new PrintProof(
            row.Key, row.Target, row.IntentRow.Rendering, destination.TotalAreaCoverage, row.Ceiling, black,
            row.IntentRow.BlackPoint && black.Match(Some: static xyz => xyz.Y > 0d, None: static () => true),
            (proofing is null ? CmsFlags.None : CmsFlags.SoftProofing)
                | (row.Alarm.IsSome ? CmsFlags.GamutCheck : CmsFlags.None),
            pixels);
    }

    // ONE extended build carries every case, and the CHAIN is where soft proofing lives. The gamut
    // operand is a separate alarm lookup lcms reads only under CmsFlags.GamutCheck, so it rides the
    // alarm and never the simulation — handing it the press profile as the preview drops the preview.
    // The PCS the alarm reads is the one after the source link, the position lcms's own proofing entry
    // pins, so it is the chain's own index and never a caller-supplied slot. This build mints and
    // releases its own limit link because it is the only reader of it.
    static Fin<PrintPlate> Run(
        Context context, PrintTransform row, Profile source, Profile destination, Profile? proofing,
        PrintProof proof, ReadOnlyMemory<byte> raster) {
        const int GamutPcs = 1;
        // The chain reads the receipt's OWN verdict and takes the ceiling off the same option, so the
        // comparison exists once: two spellings of one predicate are how a chain and its receipt come to
        // disagree about whether the plate was ink-limited.
        using Profile? limit = (proof.InkLimited ? proof.Ceiling : Option<PressCeiling>.None)
            .Match(
                Some: ceiling => Profile.CreateInkLimitingDeviceLink(context, row.Target.Device, (double)ceiling),
                None: () => (Profile?)null);
        row.Alarm.Iter(alarm => ignore(alarm.Mark(context)));
        Seq<PrintLink> chain = Chained(row, source, destination, proofing, limit, proof);
        using Transform transform = Transform.Create(
            context,
            chain.Map(static link => link.Profile).ToArray(),
            chain.Map(static link => link.BlackPoint).ToArray(),
            chain.Map(static link => link.Rendering).ToArray(),
            chain.Map(static link => link.Adaptation).ToArray(),
            row.Alarm.IsSome ? proofing : null, GamutPcs,
            row.Target.Input, row.Target.Output, proof.Flags);
        byte[] plate = new byte[(long)proof.Pixels * row.Target.OutputStride];
        transform.DoTransform(raster.Span, plate, proof.Pixels);
        return Fin.Succ(new PrintPlate(plate, proof));
    }

    // Chain order IS the colour law: source, then the press seated TWICE for a soft proof — into-press
    // under the document intent, back out under relative colorimetric, the pair that MAKES the
    // simulation — then the destination rendering that simulation under ProofIntent, then the ink-limit
    // tail. A plain conversion is the same fold with the press pair absent, so proofed and unproofed
    // builds differ by two rows rather than by a second code path. Black-point compensation lands on the
    // destination link alone because that is the only link whose endpoint the detected black measured.
    static Seq<PrintLink> Chained(
        PrintTransform row, Profile source, Profile destination, Profile? proofing, Profile? limit, PrintProof proof) =>
        Seq(new PrintLink(source, row.IntentRow.Rendering, BlackPoint: false, row.IntentRow.Adaptation))
        + (proofing is { } press
            ? Seq(new PrintLink(press, row.IntentRow.Rendering, BlackPoint: false, row.IntentRow.Adaptation),
                  new PrintLink(press, Intent.RelativeColorimetric, BlackPoint: false, row.IntentRow.Adaptation))
            : Seq<PrintLink>())
        + Seq(new PrintLink(
            destination,
            proofing is null ? row.IntentRow.Rendering : row.ProofIntent.Rendering,
            proof.BlackPointApplied,
            row.IntentRow.Adaptation))
        + (limit is { } tail
            ? Seq(new PrintLink(tail, row.IntentRow.Rendering, BlackPoint: false, row.IntentRow.Adaptation))
            : Seq<PrintLink>());
}
```

## [07]-[SCHEDULED_EXPORT]

- Owner: `ReportSubscription` — the consumer-owned recurring-delivery row that closes a report specification over the AppHost scheduler without introducing a document-local timer.
- Entry: `public ScheduleEntry Register(Func<string, IO<ReportSpec>> resolve, VisualRuntime runtime)` — contributes one `ScheduleEntry`; its work resolves the current report specification at firing time, renders through `FlowReport.Render`, and preserves the ordinary destination, receipt, deadline, lease, and failure rails.
- Auto: cadence is an `OccurrenceSpec` value, fleet distribution is `ScheduleEntry.Spread`, and bounded missed-occurrence recovery reads `SchedulePort.Window`; the subscription stores only the report key and schedule policy, so a profile reload re-resolves the live report rather than retaining a stale `ReportSpec` object graph.
- Receipt: every run returns the ordinary document `RenderReceipt` through `FlowReport.Render` and the AppHost `DeadlineReceipt` through `SchedulePort.Run`; a failed delivery remains the scheduled work failure and never advances the last-success stamp.
- Packages: Rasm.AppHost (project), LanguageExt.Core, NodaTime
- Growth: one recurring deliverable is one `ReportSubscription` value; one cadence is one existing `OccurrenceSpec` case; zero scheduler surface.
- Boundary: `SchedulePort` is the only time owner, `FlowReport` is the only pagination owner, and `VisualDestination` is the only delivery owner; a timer, login hook, or document-local retry loop is rejected.

```csharp signature
[ComplexValueObject]
public sealed partial class ReportSubscription {
    public string Key { get; }
    public string ReportKey { get; }
    public Rasm.AppHost.Runtime.OccurrenceSpec Occurrence { get; }
    public Rasm.AppHost.Runtime.DeadlineClass Deadline { get; }
    public Option<Rasm.AppHost.Runtime.LeasePolicy> Lease { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref string reportKey,
        ref Rasm.AppHost.Runtime.OccurrenceSpec occurrence,
        ref Rasm.AppHost.Runtime.DeadlineClass deadline,
        ref Option<Rasm.AppHost.Runtime.LeasePolicy> lease) =>
        validationError = string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(reportKey)
            ? new ValidationError("report subscription requires schedule and report identities")
            : validationError;

    public Rasm.AppHost.Runtime.ScheduleEntry Register(Func<string, IO<ReportSpec>> resolve, VisualRuntime runtime) =>
        new(
            Key,
            Occurrence,
            Deadline,
            Lease,
            () => resolve(ReportKey).Bind(spec => FlowReport.Render(runtime, spec).Map(static _ => unit)));

    public Seq<Instant> Backfill(Rasm.AppHost.Runtime.ScheduleEntry registered, Instant lastSuccess, Instant now) =>
        Rasm.AppHost.Runtime.SchedulePort.Window(registered, lastSuccess, now);
}
```

## [08]-[EXPORT_FORM]

- Owner: `ExportCapability` `[SmartEnum<string>]` the capability vocabulary carrying BOTH its field contribution and its preflight reading; `ExportFields` and `ExportNotes` the two column bodies those rows bind; `ExportTarget` `[SmartEnum<string>]` the per-format row carrying its honoured-capability roster and its media type; `DestinationRow` the recalled destination with its recency; `PreflightNote` the per-capability readout row; `ExportRequest` the admitted configuration a run consumes; `ExportForm` the admission, the preflight fold, and the run-queue handoff.
- Cases: `ExportTarget` = pdf · xlsx · docx · pptx · svg · dwg · dxf · png; `ExportCapability` = page-setup · colour · fonts · security · redaction · outline · tagged · line-weights · cad-version · layers · scale; `PreflightNote` = Honoured | Degraded | Refused — three readings of one capability, because "will this export keep my fonts" has exactly three honest answers.
- Entry: `public Validation<Error, FormSchema> Schema()` on `ExportTarget` — the per-format option schema through the one form grammar, every caption a label KEY the form chrome resolves, so the schema carries no culture and a locale flip re-renders one built schema; `public static Fin<ExportRequest> Admit(ExportTarget target, FormSchema schema, FormState state, DestinationRow destination)` — the configuration admission against the schema that rendered it; `public static Seq<PreflightNote> Preflight(ExportRequest request)` — the readout naming what the chosen arm honours, read off the request's own target; `public static RunCard Handoff(ExportRequest request, CorrelationId correlation, Instant at)` — the run-queue card; `public static Seq<OutputRow> Completed(RenderReceipt receipt)` — the open and reveal verbs a sealed artifact earns; `public static Seq<DestinationRow> Remember(Seq<DestinationRow> held, DestinationRow used, Instant at)` and `public static Seq<DestinationRow> For(Seq<DestinationRow> held, ExportTarget target)` — the recency fold and its per-format read.
- Auto: configuration is SCHEMA, never a per-format dialog: each `ExportTarget` row names the capability rows it honours and each of those rows contributes its own section-tagged fields — page setup for the paginated targets, colour policy for every raster and press target, security and redaction posture for PDF, version policy for the CAD pair — and the one `FormChrome` capsule renders the built schema, so adding a format adds a row rather than a screen. Section rows partition the fields exactly as the schema gate demands, so a format whose fields do not partition refuses at schema construction rather than rendering a form with an unseated field. The preflight readout is that SAME roster read a second way, each row answering itself against the admitted configuration, so it names what WILL happen — embedded fonts, colour management, an ink limit, a page cap, a redaction pass — rather than restating the options the user just set, and the two readings cannot disagree about which capabilities a format carries because they fold one list. Progress hands to the `Shell/screens#RUN_QUEUE` surface through an ordinary `RunCard` with a `RunOrigin.Verb` correlation, so a long export appears beside every other job with the same cancel and retry affordances, and completion projects `OutputRow` rows whose adopt keys are the open and reveal command intents — the queue raises a verb and this page constructs nothing. Destination rows recall through the persistence snapshot vocabulary the selection sets already use, so the last-used folder per format survives a restart and a new destination enters at the head.
- Receipt: the export itself seals its own `RenderReceipt` at its arm; this cluster seals nothing, so a configured export produces exactly one artifact receipt and the queue card reads that receipt's correlation.
- Packages: Avalonia, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project)
- Growth: a new export format is one `ExportTarget` row naming the capability rows it honours, and its schema and its readout follow from that one list; a new capability is ONE `ExportCapability` row carrying its own field contribution and its own verdict, so the schema gains its control and the preflight gains its note in the same declaration — a capability that could be named on a roster without both was the shape that rendered no control and reported itself honoured; zero new surface.
- Boundary: the form is the ONE configuration surface — a per-format options dialog, a per-format view model, and a per-format validation pass are the three deleted forms, because each is a place a format's options can drift from what its arm actually reads. Fields are `FormField` values over the settled `FieldEntry` rows, so dimensioned entry resolves through the measurement policy and expression entry through the symbolic owner exactly as every other form — an export-local unit table or number parser is rejected. The preflight NAMES capability rather than promising it: a note is `Honoured`, `Degraded` with the reason, or `Refused` with the reason, and a target that cannot answer a capability at all omits the note rather than reporting a false positive. Capability is a VOCABULARY ROW owning both readings, so a bare-constant capability roster read by two switches — each with a default arm that rendered nothing and reported honoured — is the deleted form, and the key a note carries is the row's own rather than a spelling a member repeats. Progress rides the settled run queue — an export-local progress dialog, an export-local cancel token, and an export-local retry loop are the three deleted forms — and the completion verbs are command intents the deck raises, so opening a sealed artifact goes through the same host pipe every other reveal does. Destination admission stays the `[02]` delivery gate's: a recalled row is a remembered PATH the picker produced, and this cluster never computes one.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// The per-capability readout. Three cases rather than a bool plus a message, because "the fonts embed",
// "the fonts substitute and here is why", and "this target cannot embed fonts at all" are three different
// things a user acts on differently, and a boolean collapses the middle one into whichever pole it resembles.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PreflightNote {
    private PreflightNote() { }
    public sealed record Honoured(string CapabilityKey) : PreflightNote;
    public sealed record Degraded(string CapabilityKey, string Reason) : PreflightNote;
    public sealed record Refused(string CapabilityKey, string Reason) : PreflightNote;

    public string Capability => Switch(
        honoured: static h => h.CapabilityKey, degraded: static d => d.CapabilityKey, refused: static r => r.CapabilityKey);

    public WorkSeverity Severity => Switch(
        honoured: static _ => WorkSeverity.Info, degraded: static _ => WorkSeverity.Warning, refused: static _ => WorkSeverity.Error);
}

// The capability vocabulary as ROWS carrying BOTH readings of one decision: the section-tagged fields a
// format's options render as, and the note its preflight reports against an admitted request. A capability is
// exactly the pair "what a user can set" and "what the arm will then honour", so one row owns both and a
// format's roster is the one list both consumers fold. Spelled as bare string constants the two readings
// were two switches over that vocabulary, each with a silent default arm — a new capability named on a row
// rendered NO field and reported itself Honoured, so the schema quietly lost a control and the readout
// quietly promised a capability nothing implemented.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExportCapability {
    public static readonly ExportCapability PageSetup = new("page-setup", ExportFields.PageSetup, ExportNotes.Plain);
    public static readonly ExportCapability Colour = new("colour", ExportFields.Colour, ExportNotes.Colour);
    // No field: font embedding is a property of the produced part graph rather than a value a user sets, so
    // the capability contributes a READOUT alone and the schema stays free of a control no arm consults.
    public static readonly ExportCapability Fonts = new("fonts", ExportFields.None, ExportNotes.Fonts);
    public static readonly ExportCapability Security = new("security", ExportFields.Security, ExportNotes.Security);
    public static readonly ExportCapability Redaction = new("redaction", ExportFields.Redaction, ExportNotes.Plain);
    public static readonly ExportCapability Outline = new("outline", ExportFields.Outline, ExportNotes.Outline);
    public static readonly ExportCapability Tagged = new("tagged", ExportFields.Tagged, ExportNotes.Plain);
    public static readonly ExportCapability Weights = new("line-weights", ExportFields.None, ExportNotes.Plain);
    public static readonly ExportCapability CadVersion = new("cad-version", ExportFields.CadVersion, ExportNotes.Plain);
    public static readonly ExportCapability Layers = new("layers", ExportFields.None, ExportNotes.Plain);
    public static readonly ExportCapability Scale = new("scale", ExportFields.Scale, ExportNotes.Plain);

    // The row's own field contribution, keyed on the target so every control id carries the format it
    // configures and two formats honouring one capability cannot collide at the factory.
    [UseDelegateFromConstructor]
    public partial Seq<(string Section, FormField Field)> Fields(ExportTarget target);

    // The row's own readout against an admitted request. `Plain` is the row that always honours, so a
    // capability whose verdict never varies still answers through the same column rather than falling out of
    // a switch a reader has to check for a default. The capability the note names arrives as an ARGUMENT the
    // row supplies from its own key, so a member cannot transcribe a key that names a row nothing honours.
    [UseDelegateFromConstructor]
    public partial PreflightNote Note(string capability, ExportRequest request);

    // The one reading a surface takes: the row hands its own key to its own column, so the note's capability
    // and the row that produced it are one value by construction.
    public PreflightNote Read(ExportRequest request) => Note(Key, request);
}

// The format roster. Each row carries the capability rows its options render from and its preflight reads,
// so a format is a row rather than a screen and the two facts that differ per format live beside each other
// where they cannot fall out of step.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExportTarget {
    public static readonly ExportTarget Pdf = new(
        "pdf", "application/pdf",
        Seq(ExportCapability.PageSetup, ExportCapability.Colour, ExportCapability.Fonts, ExportCapability.Security,
            ExportCapability.Redaction, ExportCapability.Outline, ExportCapability.Tagged));
    public static readonly ExportTarget Xlsx = new(
        "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Seq(ExportCapability.Fonts));
    public static readonly ExportTarget Docx = new(
        "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        Seq(ExportCapability.PageSetup, ExportCapability.Fonts));
    public static readonly ExportTarget Pptx = new(
        "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation", Seq<ExportCapability>());
    public static readonly ExportTarget Svg = new("svg", "image/svg+xml", Seq(ExportCapability.Weights));
    public static readonly ExportTarget Dwg = new(
        "dwg", "image/vnd.dwg", Seq(ExportCapability.CadVersion, ExportCapability.Layers));
    public static readonly ExportTarget Dxf = new(
        "dxf", "image/vnd.dxf", Seq(ExportCapability.CadVersion, ExportCapability.Layers));
    public static readonly ExportTarget Png = new("png", "image/png", Seq(ExportCapability.Colour, ExportCapability.Scale));

    public string MediaType { get; }

    public Seq<ExportCapability> Honours { get; }

    // The schema is BUILT from the capability roster, so a row's options and its preflight read one list and
    // a format cannot render an option its arm never consults. Sections partition the field set by
    // construction, which is exactly what the schema gate proves. Every caption is a label KEY the form chrome
    // resolves at render, so the schema carries no culture and a locale flip re-renders one built schema
    // rather than rebuilding it — a resolved-locale parameter here would be a knob no arm reads.
    public Validation<Error, FormSchema> Schema() =>
        Honours.Bind(capability => capability.Fields(this)) switch {
            var fields => FormSchema.Create(
                $"export.{Key}",
                $"export.{Key}.submit",
                $"export.{Key}.commit",
                FormGeometry.Stacked,
                fields.Map(static row => row.Field),
                Sections(fields)),
        };

    // Sections derive from the fields' own section tags, so the partition the schema gate proves is
    // constructed rather than authored and a field can never be seated twice or left unseated.
    static Seq<FormSection> Sections(Seq<(string Section, FormField Field)> fields) =>
        toSeq(fields.Map(static row => row.Section).Distinct())
            .Map(section => FormSection.Of(
                section, $"export.section.{section}",
                fields.Filter(row => row.Section == section).Map(static row => row.Field.Key)));
}

// The field contributions, one member per capability row's column. Each takes the target so a control id
// names the format it configures, and the settled entry rows carry the value shapes — so the measurement,
// expression, path, and choice rails all reach export configuration unchanged.
public static class ExportFields {
    public const double Viewport = 200d;

    public static Seq<(string Section, FormField Field)> None(ExportTarget target) => [];

    public static Seq<(string Section, FormField Field)> PageSetup(ExportTarget target) => Seq(
        ("page", FormField.Of($"{target.Key}.page.size", "export.field.page-size",
            new ControlIntent.Select($"{target.Key}.page.size", SelectPosture.Closed,
                OptionSource.Fixed(PageSizes), VirtualWindowSpec.FixedRow(Viewport),
                IntentBinding.Of(PaintRole.Well)),
            FieldEntry.Choice, static _ => Validation<Error, Unit>.Success(unit))),
        ("page", FormField.Of($"{target.Key}.page.landscape", "export.field.landscape",
            new ControlIntent.Toggle($"{target.Key}.page.landscape", "export.field.landscape", IntentBinding.Of(PaintRole.Panel)),
            FieldEntry.Flag, static _ => Validation<Error, Unit>.Success(unit))));

    public static Seq<(string Section, FormField Field)> Colour(ExportTarget target) => Seq(
        ("colour", FormField.Of($"{target.Key}.colour.target", "export.field.colour",
            new ControlIntent.Select($"{target.Key}.colour.target", SelectPosture.Closed,
                OptionSource.Fixed(ColorTarget.Items.Map(static row =>
                    new OptionRow(row.Key, $"export.colour.{row.Key}", None, None)).ToSeq()),
                VirtualWindowSpec.FixedRow(Viewport), IntentBinding.Of(PaintRole.Well)),
            FieldEntry.Choice, static _ => Validation<Error, Unit>.Success(unit))));

    public static Seq<(string Section, FormField Field)> Security(ExportTarget target) => Seq(
        ("security", FormField.Of($"{target.Key}.security.encrypt", "export.field.encrypt",
            new ControlIntent.Toggle($"{target.Key}.security.encrypt", "export.field.encrypt", IntentBinding.Of(PaintRole.Panel)),
            FieldEntry.Flag, static _ => Validation<Error, Unit>.Success(unit))),
        ("security", FormField.Of($"{target.Key}.security.print", "export.field.allow-print",
            new ControlIntent.Toggle($"{target.Key}.security.print", "export.field.allow-print", IntentBinding.Of(PaintRole.Panel)),
            FieldEntry.Flag, static _ => Validation<Error, Unit>.Success(unit))));

    public static Seq<(string Section, FormField Field)> Redaction(ExportTarget target) => Seq(
        ("security", FormField.Of($"{target.Key}.redaction.posture", "export.field.redaction",
            new ControlIntent.Select($"{target.Key}.redaction.posture", SelectPosture.Closed,
                OptionSource.Fixed(DataClassification.Items.Map(static row =>
                    new OptionRow(row.Key, $"export.redaction.{row.Key}", None, None)).ToSeq()),
                VirtualWindowSpec.FixedRow(Viewport), IntentBinding.Of(PaintRole.Well)),
            FieldEntry.Choice, static _ => Validation<Error, Unit>.Success(unit))));

    public static Seq<(string Section, FormField Field)> Outline(ExportTarget target) => Seq(
        ("structure", FormField.Of($"{target.Key}.outline.bookmarks", "export.field.bookmarks",
            new ControlIntent.Toggle($"{target.Key}.outline.bookmarks", "export.field.bookmarks", IntentBinding.Of(PaintRole.Panel)),
            FieldEntry.Flag, static _ => Validation<Error, Unit>.Success(unit))));

    public static Seq<(string Section, FormField Field)> Tagged(ExportTarget target) => Seq(
        ("structure", FormField.Of($"{target.Key}.tagged.ua", "export.field.tagged",
            new ControlIntent.Toggle($"{target.Key}.tagged.ua", "export.field.tagged", IntentBinding.Of(PaintRole.Panel)),
            FieldEntry.Flag, static _ => Validation<Error, Unit>.Success(unit))));

    public static Seq<(string Section, FormField Field)> CadVersion(ExportTarget target) => Seq(
        ("format", FormField.Of($"{target.Key}.cad.version", "export.field.cad-version",
            new ControlIntent.Select($"{target.Key}.cad.version", SelectPosture.Closed,
                OptionSource.Fixed(CadVersions), VirtualWindowSpec.FixedRow(Viewport),
                IntentBinding.Of(PaintRole.Well)),
            FieldEntry.Choice, static _ => Validation<Error, Unit>.Success(unit))));

    public static Seq<(string Section, FormField Field)> Scale(ExportTarget target) => Seq(
        ("format", FormField.Of($"{target.Key}.raster.scale", "export.field.scale",
            new ControlIntent.Slider($"{target.Key}.raster.scale", 1d, 4d, 0.5d, IntentBinding.Of(PaintRole.Accent)),
            FieldEntry.Scalar, static _ => Validation<Error, Unit>.Success(unit))));

    static Seq<OptionRow> PageSizes =>
        Seq(new OptionRow("a4", "export.page.a4", None, None),
            new OptionRow("a3", "export.page.a3", None, None),
            new OptionRow("letter", "export.page.letter", None, None),
            new OptionRow("tabloid", "export.page.tabloid", None, None));

    static Seq<OptionRow> CadVersions =>
        Seq(new OptionRow("ac1032", "export.cad.ac1032", None, None),
            new OptionRow("ac1027", "export.cad.ac1027", None, None),
            new OptionRow("ac1021", "export.cad.ac1021", None, None));
}

// The readout contributions, one member per capability row's column. Each names what the chosen arm WILL do
// against the admitted configuration, so a note cannot promise a capability the row does not claim; the
// capability key arrives from the row that folds the member and is never spelled here.
public static class ExportNotes {
    public static PreflightNote Plain(string capability, ExportRequest request) =>
        new PreflightNote.Honoured(capability);

    // A vector export references its faces rather than embedding them, which is a real degradation a reader
    // acts on by installing the face — not a refusal, because the artifact still carries every glyph run.
    public static PreflightNote Fonts(string capability, ExportRequest request) =>
        request.Target == ExportTarget.Svg
            ? new PreflightNote.Degraded(capability, "export.preflight.svg-font-reference")
            : new PreflightNote.Honoured(capability);

    public static PreflightNote Colour(string capability, ExportRequest request) =>
        ExportForm.Chose(request, "colour.target", ColorTarget.Press.Key)
            ? new PreflightNote.Degraded(capability, "export.preflight.press-proof-required")
            : new PreflightNote.Honoured(capability);

    public static PreflightNote Security(string capability, ExportRequest request) =>
        ExportForm.Chose(request, "security.encrypt", "true")
            ? new PreflightNote.Honoured(capability)
            : new PreflightNote.Refused(capability, "export.preflight.encryption-off");

    public static PreflightNote Outline(string capability, ExportRequest request) =>
        ExportForm.Chose(request, "outline.bookmarks", "true")
            ? new PreflightNote.Honoured(capability)
            : new PreflightNote.Refused(capability, "export.preflight.bookmarks-off");
}

// --- [MODELS] ---------------------------------------------------------------------------

// A remembered destination. `LastUsed` orders the recall and `Format` scopes it, so a PDF picker offers the
// last PDF folder rather than the last folder of any kind — one roster, scoped at read.
public readonly record struct DestinationRow(string Key, string AbsolutePath, string Format, Instant LastUsed);

// The admitted configuration a run consumes: the target, the validated form state, and the destination the
// picker produced. The state is the FORM's own admitted value, so an export cannot run on a configuration
// the schema refused.
public sealed record ExportRequest(ExportTarget Target, FormState State, DestinationRow Destination);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class ExportForm {
    public const string OpenIntent = "export.artifact.open";
    public const string RevealIntent = "export.artifact.reveal";
    public const string RunIntent = "export.run";

    // Admission is the SCHEMA's own gate: every visible field admits, every required value is present, and
    // the destination carries an absolute path the delivery gate will accept — so a request that reaches a
    // run cannot fail on configuration.
    public static Fin<ExportRequest> Admit(ExportTarget target, FormSchema schema, FormState state, DestinationRow destination) =>
        string.IsNullOrWhiteSpace(destination.AbsolutePath) || !Path.IsPathFullyQualified(destination.AbsolutePath)
            ? Fin.Fail<ExportRequest>(new ExportFault.DeliveryFailed(destination.AbsolutePath, "destination is not a fully qualified path"))
            : schema.Admit(state).ToFin().Map(admitted => new ExportRequest(target, admitted, destination));

    // The readout is a fold over the target's OWN capability rows, each row reading itself against the
    // admitted configuration — so it cannot promise a capability the row does not claim, a capability the
    // target never claims produces no note at all rather than a false negative, and a capability row added to
    // the vocabulary carries its verdict here with no arm to forget. A guard ladder over the key was the
    // deleted form: its trailing arm reported every unmatched capability as honoured, so a newly declared
    // capability announced itself satisfied by an arm that had never been written.
    public static Seq<PreflightNote> Preflight(ExportRequest request) =>
        request.Target.Honours.Map(capability => capability.Read(request));

    // The handoff is an ORDINARY run card, so a long export sits in the one queue beside every other job with
    // the same cancel and retry affordances — an export-local progress dialog is the deleted form.
    public static RunCard Handoff(ExportRequest request, CorrelationId correlation, Instant at) =>
        new(new RunOrigin.Verb(correlation),
            RunIntent,
            $"export.job.{request.Target.Key}",
            WorkStatus.Queued,
            WorkStatus.Queued,
            RunDirection.Outbound,
            new FanOut(1, 0, 0),
            Preflight(request).Map(static note => new StateStrip(
                $"export.preflight.{note.Capability}", note.Capability, note.Severity)),
            Seq(new StepRow(
                request.Target.Key, $"export.step.{request.Target.Key}", WorkStatus.Queued, None, Seq<string>(), Seq<OutputRow>())),
            at);

    // Completion projects the two verbs a sealed artifact earns. `Adopt` is the intent key the queue raises,
    // so opening and revealing go through the host pipe every other reveal takes and this page constructs no
    // process start.
    public static Seq<OutputRow> Completed(RenderReceipt receipt) =>
        receipt.Destination.Map(static destination => Seq(
                new OutputRow(destination, "export.output.open", "artifact", Sealed: true, Some(OpenIntent)),
                new OutputRow(destination, "export.output.reveal", "artifact", Sealed: true, Some(RevealIntent))))
            .IfNone(Seq<OutputRow>());

    // Recency is most-recent-first, scoped by FORMAT, and deduplicated on the path — so a PDF picker offers
    // the last PDF folder and re-using one moves its row to the head instead of stacking duplicates.
    public static Seq<DestinationRow> Remember(Seq<DestinationRow> held, DestinationRow used, Instant at) =>
        (Seq(used with { LastUsed = at })
            + held.Filter(row => !(row.Format == used.Format
                && string.Equals(row.AbsolutePath, used.AbsolutePath, StringComparison.Ordinal))))
            .Take(RecencyDepth);

    public static Seq<DestinationRow> For(Seq<DestinationRow> held, ExportTarget target) =>
        toSeq(held.Filter(row => row.Format == target.Key).OrderByDescending(static row => row.LastUsed));

    const int RecencyDepth = 16;

    // The one admitted-state read every capability row's readout takes: the field id is the target's own key
    // plus the row's suffix, exactly as the schema minted it, so a note reads the value the form wrote and a
    // divergent value read is a control that never existed rather than a silent false.
    public static bool Chose(ExportRequest request, string suffix, string expected) =>
        request.State.Values.Find($"{request.Target.Key}.{suffix}")
            .Bind(static value => value.Uniform)
            .Map(value => string.Equals(value.ToString(), expected, StringComparison.OrdinalIgnoreCase))
            .IfNone(false);
}
```

## [09]-[RESEARCH]

(none)
