# [APPUI_DOCUMENT_EXPORT]

Rasm.AppUi document export owns one paginated-output rail. MigraDoc composes flow reports, PDFsharp policies harden PDF output, OOXML writers carry XLSX/DOCX/PPTX, and lcmsNET rows carry print fidelity. This page owns destinations, `ExportFault`, support-bundle contributions, report specifications, PDF policies, Office output, and print transforms. Drafting flow reports and diagnostics evidence compose this owner; drafting sheet-PDF remains on capture's vector-print arm.

## [01]-[INDEX]

- [02]-[EXPORT_DESTINATIONS]: One destination union; the typed `ExportFault` rail; the support-bundle contributor rows.
- [03]-[FLOW_REPORT]: MigraDoc flow DOM; auto-pagination; running bands; placed visuals.
- [04]-[PDF_POLICY]: Security, signatures over the AppHost secrets lease, AcroForms, PDF-UA, the colour-mode row, cross-reference annotations.
- [05]-[OFFICE_ARM]: OOXML part-graph writers — XLSX, DOCX, PPTX.
- [06]-[PRINT_ARM]: lcmsNET device-CMYK/ICC transforms; intent proving; ink limiting; proofing; K-preservation intents.
- [07]-[SCHEDULED_EXPORT]: Consumer-owned `ScheduleEntry` rows for recurring report delivery and bounded backfill.

## [02]-[EXPORT_DESTINATIONS]

- Owner: `VisualDestination` [Union] — the one delivery vocabulary every export arm and the capture vector-print/video arms deliver through, carrying its own `Key` case column so a delivery-keyed projection reads the owner rather than re-spelling the case literals; `ExportFault` — the typed export rail; `ExportDelivery` — the one delivery fold; `BundleMember` — the classified, content-keyed diagnostic-artifact row; `SupportBundle` — the member roster and the contribution fold onto the Bundle destination.
- Cases: FilePath · BlobLane · Bundle; bundle member rows evidence-journal · hud-samples · gpu-timelines · quality-verdicts · native-assets · proof-goldens · collab-ops — each a named factory pinning artifact name and classification.
- Entry: `public static IO<string> Deliver(VisualRuntime runtime, VisualDestination destination, byte[] payload)` — IO rail; the FilePath arm receives its absolute path as a value from the picker intent and never computes paths; artifact scopes resolve from `ProfileRoots`; `SupportBundle.Contribute(VisualRuntime runtime, params ReadOnlySpan<BundleMember> members)` — one modality-polymorphic contribution fold delivering every member through the Bundle destination and sealing one bundle-kind `RenderReceipt` per member.
- Auto: the Bundle arm stages every classified artifact through the runtime `BundleWrite` delegate before any receipt enters the sink, then commits receipts only for the complete delivered roster; a delivery or sink refusal stays on the IO rail for AppHost `SupportCapture` to recover as its partial manifest row and cleanup fold; every delivered payload seals a `RenderReceipt` of kind document/office/print/bundle whose `FrameHash` mints through the runtime `ContentHash` delegate bound to the kernel `Rasm.Domain` `ContentHash.Of` entry; each `BundleMember` payload arrives already serialized by its owning codec — the evidence journal off the sealed envelope stream, HUD samples and GPU timelines off the devloop feeds, quality verdicts and native-asset facts off their receipt folds, proof-golden digests off the render-hash lane, and the collab op window off the devloop `CollabJson` readable export — so assembly is a fold over settled receipt streams and no member re-measures.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm (project)
- Growth: one destination case extends delivery and breaks the dispatch at compile time; one export target is one row on the owning arm, never a second engine; one diagnostic stream is one `BundleMember` factory row; one `ExportFault` case is one `detail` ordinal under the `AppUiFaultBand.Export` row (6420); zero new surface.
- Boundary: this union is the ONE export-destination owner — capture's vector-print arm, the FFmpeg clip rows, drafting's sheet-PDF egress, and the diagnostics report-PDF all deliver through it, so a per-arm destination enum is the deleted form; `FilePath` admits only fully qualified targets whose normalized path remains under `ProfileRoots.AppRoot`, `StoreRoot`, or `SupportRoot`, rejects every symlink or junction in the selected root and existing parent chain, opens the unique pending file with create-new semantics before writing, and lands the final rename fail-closed against a parent swap — source and target resolve through one parent path in one rename syscall, the GUID-named pending sibling cannot pre-exist at a redirected parent, and the link-free parent re-walk runs after the write immediately before the rename — so a relative, linked, escaping, or mid-flight-redirected path folds to `ExportFault.DeliveryFailed`; every fault derives through `AppUiFaultBand.Export` — a bare `Error.New` is the deleted form; archive assembly and manifest custody are the AppHost support-capture fold's — contributed members cross the `BundleWrite` seam as classified payloads, the AppHost `SupportCapture` redacts, caps, and archives them, and an AppUi-local zip assembler or second manifest store is the deleted form; the contributor roster declares AppUi membership and classification, and `BundleMember.ContentKey` mints each pre-redaction payload identity through kernel `ContentHash.Of`; AppHost `SupportManifest.Entry` omits the post-redaction, post-cap content key, so the content-hashed manifest claim and its `[SUPPORT_BUNDLE]`/`[BUNDLE_MANIFEST]` cards remain blocked at that owner while `BundleShape` pins roster and tree completeness only.

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
    public static IO<string> Deliver(VisualRuntime runtime, VisualDestination destination, byte[] payload) =>
        destination.Switch(
            state: (runtime, payload),
            filePath: static (ctx, file) => AtomicFile(ctx.runtime.Roots, file.AbsolutePath, ctx.payload),
            blobLane: static (ctx, blob) => ctx.runtime.BlobWrite(blob.ArtifactKey, ctx.payload),
            bundle: static (ctx, bundle) => ctx.runtime.BundleWrite(bundle.ArtifactName, bundle.Classification, ctx.payload));

    // Parent-swap redirection fails closed by construction, not by a trusted string re-check: the
    // rename source and target share ONE parent path the OS resolves inside a single rename syscall,
    // and the pending sibling's name carries an unguessable GUID — a parent directory swapped for a
    // link after admission re-points BOTH paths, the pending byte stream is absent at the redirected
    // parent, and the rename faults instead of landing bytes outside the admitted root. The link-free
    // parent re-walk runs AFTER the write, immediately before the rename, so a long payload write
    // never widens the admission-to-rename window; no BCL directory-handle-relative rename exists,
    // and this shared-parent + unguessable-sibling shape is the fail-closed equivalent.
    static IO<string> AtomicFile(ProfileRoots roots, string destination, byte[] payload) =>
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
                        RandomAccess.Write(handle, payload, fileOffset: 0L);
                    }
                    if (!ResolvedParent(admittedRoot.IfNone(string.Empty), target))
                        throw new UnauthorizedAccessException("destination parent changed during admission");
                    File.Move(pending, target, overwrite: true);
                    return target;
                }
                finally { if (File.Exists(pending)) { File.Delete(pending); } }
            })
            | @catch<IO, string>(static _ => true, error => IO.fail<string>(new ExportFault.DeliveryFailed(destination, error.Message)));

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
    public const string Kind = "bundle";

    // Arity rides the span: one member or the whole roster contributes through one fold, each member
    // stages as its classified Bundle artifact before any receipt commits; a refused roster returns
    // on the IO rail, and AppHost alone projects partial manifest evidence and staging cleanup.
    public static IO<Seq<RenderReceipt>> Contribute(VisualRuntime runtime, params ReadOnlySpan<BundleMember> members) =>
        from staged in toSeq(members.ToArray()).TraverseM(member =>
            from mark in IO.lift(runtime.Clocks.Mark)
            from payload in IO.pure(member.Payload.ToArray())
            from destination in ExportDelivery.Deliver(runtime, new VisualDestination.Bundle(member.ArtifactName, member.Classification), payload)
            from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
            let receipt = new RenderReceipt(Kind, Path.GetExtension(member.ArtifactName).TrimStart('.'), runtime.ContentHash(payload), None, payload.LongLength, elapsed, runtime.Correlation, Optional(destination), VisualCodec.ColorPolicy.Display.Key)
            select receipt).As()
        from _ in staged.TraverseM(runtime.Sink).As()
        select staged;
}
```

## [03]-[FLOW_REPORT]

- Owner: `ReportSpec` — the flow-report composition row; `ReportSetup` — the page-geometry policy row (dimensions, margins, orientation) applied once to the section `PageSetup`; `ReportBlock` [Union] — the typed content vocabulary the MigraDoc fold consumes; `FlowReport` — the one MigraDoc render surface.
- Cases: `ReportBlock` = Heading · Body · List · Callout · Code · Table · PlacedVisual · Figure · Footnote · Section · Rule · PageBreak.
- Entry: `public static IO<RenderReceipt> Render(VisualRuntime runtime, ReportSpec spec)` — IO rail; the MigraDoc `Document`/`Section` DOM composes from the block seq, `PdfDocumentRenderer` paginates, and the payload delivers through the destination union.
- Auto: pagination, widow/orphan control, running headers/footers with `PageField`/`NumPagesField`, and cross-page table breaking are the MigraDoc layout engine's — the hand-rolled `FlowBlock`/`FlowFold` pagination engine is the deleted form this owner replaces; `FormattedDocument` exposes the measured layout so a page count or block position reads from the renderer, never a local cursor fold; placed visuals enter as `PlacedVisual` rows whose `SKImage` tiles encode through the capture codec axis (`VisualCodec.Encode`) and place as MigraDoc `Image` values — capture stays the one raster owner, the report only places.
- Receipt: one `RenderReceipt` of kind document per report with whole-payload content hash through the kernel-bound delegate and the delivered destination key.
- Packages: PDFsharp-MigraDoc, PDFsharp, SkiaSharp, Rasm.AppHost (project), NodaTime, LanguageExt.Core
- Growth: one `ReportBlock` case extends the content vocabulary; one style row retunes a role's typography mapping; zero new surface.
- Boundary: the report's colour model is its `PdfPolicy.Color` row bound onto the renderer document before `RenderDocument`, so a press report and a screen report are one composition under one row value; the MigraDoc flow DOM is the ONE flow-pagination owner — a bespoke page-break fold, a per-format report builder, or a second cursor algebra is the deleted form; typography roles map to MigraDoc styles from the `Theme/typography.md` role rows at composition so a report style never re-mints font literals; drafting's paginated flow reports and the diagnostics report-PDF (`EvidenceReport.Blocks` feeding this arm) compose `FlowReport.Render` with their own block seqs, while the drafting sheet-PDF is capture's vector-print arm — a sibling-page PDF writer is the deleted form; the page geometry is the `ReportSetup` policy row applied once to the section `PageSetup`, never per-block layout literals.

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
    public sealed record Section(string Title, Seq<ReportBlock> Blocks) : ReportBlock;
    public sealed record Rule : ReportBlock;
    public sealed record PageBreak : ReportBlock;
}

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
    VisualDestination Destination);

public static class FlowReport {
    public const string Kind = "document";

    public static IO<RenderReceipt> Render(VisualRuntime runtime, ReportSpec spec) =>
        from mark in IO.lift(runtime.Clocks.Mark)
        from payload in (IO.lift(() => Compose(spec))
            | @catch<IO, byte[]>(static _ => true, static error => IO.fail<byte[]>(new ExportFault.RenderFailed("flow-report", error.Message)))).As()
        from sealed_ in PdfPolicies.Apply(runtime, spec.Pdf, payload)
        from destination in ExportDelivery.Deliver(runtime, spec.Destination, sealed_)
        from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
        let receipt = new RenderReceipt(Kind, "pdf", runtime.ContentHash(sealed_), None, sealed_.LongLength, elapsed, runtime.Correlation, Optional(destination), VisualCodec.ColorPolicy.Display.Key)
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
        spec.Blocks.Iter(block => Append(section, block));
        PdfDocumentRenderer renderer = new() { Document = document };
        // Both pre-render binds land here: ColorMode governs how each XColor is written and UAManager
        // emits structure with content, so neither survives a post-render pass over sealed streams.
        renderer.PdfDocument.Options.ColorMode = spec.Pdf.Color.Mode;
        if (spec.Pdf.TaggedUa) { _ = UAManager.ForDocument(renderer.PdfDocument); }
        renderer.RenderDocument();
        using MemoryStream sink = new();
        renderer.PdfDocument.Save(sink);
        return sink.ToArray();
    }

    static void Append(Section section, ReportBlock block) {
        switch (block) {
            case ReportBlock.Heading heading: section.AddParagraph(heading.Text, $"Heading{int.Clamp(heading.Level, 1, 6)}"); break;
            case ReportBlock.Body body: section.AddParagraph(body.Text); break;
            case ReportBlock.List list: list.Items.Map(static (item, index) => (Item: item, Index: index)).Iter(row => section.AddParagraph($"{(list.Ordered ? $"{row.Index + 1}." : "•")} {row.Item}")); break;
            case ReportBlock.Callout callout: section.AddParagraph(callout.Title, $"Heading{int.Clamp(callout.HeadingLevel, 1, 6)}"); callout.Blocks.Iter(child => Append(section, child)); break;
            case ReportBlock.Code code: section.AddParagraph(code.Source); break;
            case ReportBlock.Table table: AppendTable(section, table); break;
            case ReportBlock.PlacedVisual visual: AppendVisual(section, visual); break;
            case ReportBlock.Figure figure: AppendVisual(section, new ReportBlock.PlacedVisual(figure.Tile, figure.WidthCm)); section.AddParagraph(figure.Caption.IfNone(figure.AltText)); break;
            case ReportBlock.Footnote footnote: section.AddParagraph($"[{footnote.Key}] {footnote.Text}"); break;
            case ReportBlock.Section group: section.AddParagraph(group.Title, "Heading2"); group.Blocks.Iter(child => Append(section, child)); break;
            case ReportBlock.Rule: section.AddParagraph().Format.Borders.Bottom.Width = 0.5; break;
            case ReportBlock.PageBreak: section.AddPageBreak(); break;
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
- Cases: `ColorTarget` = screen · press · press-deep; `PdfAnnotation` = Link · Destination · Reference · Keyed.
- Entry: `public static IO<byte[]> Apply(VisualRuntime runtime, PdfPolicy policy, byte[] rendered)` — IO rail; opens the rendered payload through `PdfReader`, applies the enabled arms, and re-saves; `public static Func<SKCanvas, Fin<Unit>> Decorate(params ReadOnlySpan<PdfAnnotation> rows)` — one page decoration folding straight into the `Render/capture#VECTOR_PRINT` `VisualExportSpec.Pages` seq, so arity rides the span and a single cross-reference and a whole sheet's roster take the one call.
- Auto: the colour model is the `ColorTarget` row alone — `PdfDocumentOptions.ColorMode` takes its `Mode` on the renderer document before content materializes while `[06]-[PRINT_ARM]` takes the same row's `Device`, buffer formats, and pixel strides, so a screen export and a proofed press export differ by this value and never by a second code path, and an implicit `PdfColorMode.Undefined` device default cannot arise; the security arm selects AES-256 through `PdfDocument.SecurityHandler.SetEncryptionToV5(bool encryptMetadata)` and applies permissions through `PdfDocument.SecuritySettings`; identity writes `PdfDocumentInformation`; signatures compose `DigitalSignatureHandler.ForDocument`; and AcroForm rows write through the catalogued field surface. `TaggedUa` attaches `UAManager` to the renderer document before `RenderDocument`, so structure emits with content. Annotations ride Skia's own annotation entrypoints on the paged canvas — `DrawUrlAnnotation`, `DrawNamedDestinationAnnotation`, and `DrawLinkDestinationAnnotation` mint the `SKData` the native annotation record retains, so an exported sheet's outbound links and internal cross-references survive as real PDF annotations rather than drawn text.
- Packages: PDFsharp, SkiaSharp, Rasm.AppHost (project), LanguageExt.Core
- Growth: a new hardening concern is one `PdfPolicy` field; a new permission is one column value; a new identity column is one `PdfIdentity` member; a new colour model is one `ColorTarget` row both legs read; a new cross-reference kind is one `PdfAnnotation` case breaking the decoration at compile time; zero new surface.
- Boundary: the signing-credential crossing is a declared ledger row (`Document/export` -> AppHost `Runtime/secrets.md`). PDF-UA tagging and the colour model both bind before content materialization — `PdfDocumentOptions.ColorMode` governs how each `XColor` is WRITTEN, so a post-render pass setting it re-saves already-written content streams and governs nothing; the post-render policy pass therefore applies security, identity, forms, and signatures alone, without re-tagging or re-colouring the document. `ColorTarget` is the ONE colour-model authority for the whole page — a `PdfColorMode` literal at a render site, a second CMYK selector on the print arm, and a `bool cmyk` knob beside a spec are the three deleted forms. Annotations are page-composition content, so they enter through the capture vector-print page fold and never through the post-render `PdfReader` pass — the three named cases carry Skia's own entrypoints and materialize as PDF annotations, while `Keyed` is the raw `DrawAnnotation(rect, key, value)` passthrough whose materialization is the PDF backend's key contract, so an unhonored key drops with no diagnostic and the case admits only a key the backend is proven to honor.

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
// outbound url, a point DEFINING a named destination, and a rect LINKING to one. Each case names the
// entrypoint that materializes it, so a sheet's cross-references leave as navigable PDF annotations.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PdfAnnotation {
    private PdfAnnotation() { }
    public sealed record Link(SKRect Region, string Url) : PdfAnnotation;
    public sealed record Destination(SKPoint At, string Name) : PdfAnnotation;
    public sealed record Reference(SKRect Region, string Name) : PdfAnnotation;
    public sealed record Keyed(SKRect Region, string Key, ReadOnlyMemory<byte> Value) : PdfAnnotation;
}

public static class PdfAnnotations {
    // Decoration is a page fold, not a document pass: it returns the exact shape the vector-print arm's
    // Pages seq admits, so a sheet composes its content fold and its cross-reference fold with no
    // second paging capsule and no second SKDocument owner.
    public static Func<SKCanvas, Fin<Unit>> Decorate(params ReadOnlySpan<PdfAnnotation> rows) {
        // Exemption: the params buffer is implicitly scoped and cannot escape into the returned fold,
        // so the roster detaches into owned storage on the one line before the closure captures it.
        Seq<PdfAnnotation> admitted = toSeq(rows.ToArray());
        return canvas => admitted.Fold(FinSucc(unit), (rail, row) => rail.Bind(_ => Draw(canvas, row)));
    }

    static Fin<Unit> Draw(SKCanvas canvas, PdfAnnotation row) => row.Switch(
        state: canvas,
        link:        static (c, l) => Minted("link", l.Url, () => c.DrawUrlAnnotation(l.Region, l.Url)),
        destination: static (c, d) => Minted("destination", d.Name, () => c.DrawNamedDestinationAnnotation(d.At, d.Name)),
        reference:   static (c, r) => Minted("reference", r.Name, () => c.DrawLinkDestinationAnnotation(r.Region, r.Name)),
        keyed:       static (c, k) => Copied(c, k));

    // Skia's string-shaped entrypoints MINT the SKData their native annotation record retains, so this
    // managed lease scopes to the draw and its `using` is the fold's one platform-forced statement
    // seam. A blank target draws a dead annotation, so it refuses at admission instead.
    static Fin<Unit> Minted(string kind, string target, Func<SKData> draw) {
        if (string.IsNullOrWhiteSpace(target)) { return Fin.Fail<Unit>(new ExportFault.AnnotationRejected(kind, "target is blank")); }
        using SKData lease = draw();
        return Fin.Succ(unit);
    }

    static Fin<Unit> Copied(SKCanvas canvas, PdfAnnotation.Keyed row) {
        if (string.IsNullOrWhiteSpace(row.Key)) { return Fin.Fail<Unit>(new ExportFault.AnnotationRejected("keyed", "key is blank")); }
        using SKData payload = SKData.CreateCopy(row.Value.Span);
        canvas.DrawAnnotation(row.Region, row.Key, payload);
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

- Owner: `OfficeFormat` [SmartEnum] · `OfficeSpec` · `OfficeSheet` [Union] · `OfficeFidelity` the per-(format × case) materialization vocabulary · `OfficeExport` — the OOXML part-graph arm.
- Cases: `OfficeFormat` = xlsx · pptx · docx; `OfficeSheet` = Table · Chart · Image · RichText; `OfficeFidelity` = Native · Declared · Unsupported.
- Entry: `public static IO<RenderReceipt> Emit(VisualRuntime runtime, OfficeSpec spec)` — the Office IO rail; admission runs the fidelity matrix over every sheet FIRST, so an `Unsupported` combination folds to `ExportFault.ContentUnsupported` before any part writes.
- Auto: XLSX writes through `SpreadsheetDocument.Create` and its workbook/worksheet part graph; DOCX writes through `WordprocessingDocument.Create` and its main-document part graph. PPTX remains a typed `Unsupported` fidelity row until the presentation/master/layout/slide members are catalogued; no speculative part graph survives in the body.
- Receipt: one `RenderReceipt` of kind office per emit with whole-payload content hash and the delivered destination key.
- Packages: DocumentFormat.OpenXml, SkiaSharp, Rasm.AppHost (project), NodaTime, LanguageExt.Core
- Growth: one `OfficeFormat` row admits an Office target and one `OfficeSheet` case admits a content kind; a fidelity promotion is one matrix cell flipped as the verified part members land; zero new surface.
- Boundary: the Office destination is the same `VisualDestination` union. `Native` cells materialize their own part vocabulary, `Declared` cells state their projection, and `Unsupported` cells reject through `ExportFault.ContentUnsupported`; every PPTX cell and every image cell takes that typed rejection path.

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

[SmartEnum]
public sealed partial class OfficeFidelity {
    public static readonly OfficeFidelity Native = new();
    public static readonly OfficeFidelity Declared = new();
    public static readonly OfficeFidelity Unsupported = new();
}

public static class OfficeExport {
    public const string Kind = "office";

    // Support is the fidelity honesty law: Native = own part vocabulary, Declared = stated projection,
    // Unsupported = typed rejection; a promotion is one cell flip when the part members verify.
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
        from _admit in IO.lift(Admitted(spec))
        from mark in IO.lift(runtime.Clocks.Mark)
        from payload in Write(spec)
        from destination in ExportDelivery.Deliver(runtime, spec.Destination, payload)
        from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
        let receipt = new RenderReceipt(Kind, spec.Format.Key, runtime.ContentHash(payload), None, payload.LongLength, elapsed, runtime.Correlation, Optional(destination), VisualCodec.ColorPolicy.Display.Key)
        from _ in runtime.Sink(receipt)
        select receipt;

    // Total generated dispatch over the closed format vocabulary — a new OfficeFormat row breaks this
    // site at compile time; a part-graph throw folds typed PartGraphRejected, never an untyped Error.
    static IO<byte[]> Write(OfficeSpec spec) =>
        spec.Format.Switch(
            state: spec,
            xlsx: static s => (IO.lift(() => WriteXlsx(s))
                | @catch<IO, byte[]>(static _ => true, static error => IO.fail<byte[]>(new ExportFault.PartGraphRejected("xlsx", error.Message)))).As(),
            pptx: static _ => IO.fail<byte[]>(new ExportFault.ContentUnsupported("pptx", "catalogued part graph")),
            docx: static s => (IO.lift(() => WriteDocx(s))
                | @catch<IO, byte[]>(static _ => true, static error => IO.fail<byte[]>(new ExportFault.PartGraphRejected("docx", error.Message)))).As());

    static byte[] WriteXlsx(OfficeSpec spec) {
        using MemoryStream sink = new();
        using (SpreadsheetDocument doc = SpreadsheetDocument.Create(sink, SpreadsheetDocumentType.Workbook)) {
            WorkbookPart workbook = doc.AddWorkbookPart();
            workbook.Workbook = new Workbook();
            Sheets sheets = workbook.Workbook.AppendChild(new Sheets());
            // Indexed instance Map is (value, index) — the module spelling transposes; Iter carries no index.
            spec.Sheets.Map(static (sheet, index) => (Sheet: sheet, Index: index)).Iter(row => {
                WorksheetPart part = workbook.AddNewPart<WorksheetPart>();
                SheetData data = new();
                Rows(row.Sheet).Iter(cells => data.Append(cells));
                part.Worksheet = new Worksheet(data);
                sheets.Append(new Sheet { Id = workbook.GetIdOfPart(part), SheetId = (uint)(row.Index + 1), Name = SheetName(row.Sheet) });
            });
            workbook.Workbook.Save();
        }
        return sink.ToArray();
    }

    static byte[] WriteDocx(OfficeSpec spec) {
        using MemoryStream sink = new();
        using (WordprocessingDocument doc = WordprocessingDocument.Create(sink, WordprocessingDocumentType.Document)) {
            MainDocumentPart main = doc.AddMainDocumentPart();
            Body body = new();
            spec.Sheets.Iter(sheet => Paragraphs(sheet).Iter(p => body.Append(p)));
            main.Document = new Document(body);
            EmbedFonts(main, spec.EmbeddedFonts);
            main.Document.Save();
        }
        return sink.ToArray();
    }

    // The matrix read fails CLOSED: an uncatalogued (format, kind) cell is unsupported, so a new
    // `OfficeFormat` row or `OfficeSheet` case rejects typed until its cells land instead of throwing a
    // key-not-found out of an indexer the growth line invites a caller to outrun.
    static Fin<Unit> Admitted(OfficeSpec spec) =>
        spec.Sheets.TraverseM(sheet =>
            Support.TryGetValue((spec.Format.Key, sheet.Kind), out OfficeFidelity? fidelity) && fidelity != OfficeFidelity.Unsupported
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ExportFault.ContentUnsupported(spec.Format.Key, SheetName(sheet)))).As().Map(static _ => unit);

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

- Owner: `PrintIntent` [SmartEnum] — the rendering-intent policy rows; `PressCeiling` [ValueObject] — the admitted total-area-coverage ceiling; `GamutAlarm` [ComplexValueObject] — the per-channel out-of-gamut marking colour; `PrintTransform` — the lcmsNET transform row; `PrintProof` — the typed proving receipt; `PrintPlate` — the converted pixels beside their proof; `PrintArm` — the device-CMYK conversion surface.
- Cases: `PrintIntent` = perceptual · relative-colorimetric · saturation · absolute-colorimetric · relative-bpc · preserve-k — K preservation, black-point compensation, and adaptation state are policy columns, never flags scattered at call sites.
- Entry: `public static IO<PrintPlate> Convert(VisualRuntime runtime, PrintTransform row, ReadOnlyMemory<byte> raster)` — IO rail; one `Context`, one proving fold, one extended `Transform.Create`, one `DoTransform` per payload.
- Auto: `PrintArm` owns its own lcmsNET `Context`, opens every profile on it, and writes the `GamutAlarm` codes to `Context.AlarmCodes` — the per-context instance property, never the `Cms.AlarmCodes` process-global twin, so one proofing arm's marking colour never leaks into a concurrent conversion; the alarm width is READ back off the context vector lcms itself sizes, so the native fixed-width refusal is unreachable. Proving runs BEFORE the build: each profile answers `IsIntentSupported` in the direction the chain uses it (source as input, destination as output, proof as proof) and an unsupported intent is `ExportFault.IntentUnsupported`, never a silent fallback; `Profile.TotalAreaCoverage` measures the destination's own coverage and a measurement above the admitted `PressCeiling` mints `Profile.CreateInkLimitingDeviceLink` on the arm's context as the chain's tail link; `DetectDestinationBlackPoint` decides whether the intent row's declared black-point compensation changes anything. One extended `Transform.Create(Context, Profile[], bool[], Intent[], double[], Profile, int, uint, uint, CmsFlags)` builds every case — per-link BPC, per-link intent, per-link adaptation, the optional gamut profile, and the ink-limit tail are columns of one build, so limited, unlimited, proofed, and plain conversions are one code path. Native lcms2 ships with the app.
- Receipt: one `RenderReceipt` of kind print per conversion whose `ColorSpace` field carries the `PrintTransform` row key — the identity naming source, destination, intent, and ceiling together — so a print baseline keys distinctly, and one `PrintProof` carrying the measured coverage, the admitted ceiling, the ink-limit verdict, the detected destination black point, and the resolved flag set — every field a run fact, and an undetectable black point is `None`, never a zero.
- Packages: lcmsNET, Rasm.AppHost (project), LanguageExt.Core
- Growth: a new intent is one `PrintIntent` row; a new device profile is one `PrintTransform` value from profile bytes; a new buffer depth is one `ColorTarget` row; zero new surface.
- Boundary: lcmsNET owns device-CMYK/ICC transforms at the print boundary ONLY — Unicolour stays the suite color-model kernel and `VisualCodec.ColorPolicy` stays the capture codec gamut family, three disjoint charters; an unparseable profile folds to `ExportFault.ProfileInvalid`, never a silent sRGB fallback; buffer formats and pixel strides are the `[04]` `ColorTarget` row's columns, so a `Cms.TYPE_*` literal at this site and a `rgba.Length / 4` pixel count are the two deleted forms the 16-bit lane made wrong; every handle — `Context`, each `Profile`, the `Transform` — is a `CmsHandle<T>` over `SafeHandle` released in the kernel's `finally`, and the context releases last because every profile opened on it borrows its scope.

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

// Alarm presence IS the gamut-check decision: Some marks out-of-gamut pixels with these codes under
// CmsFlags.GamutCheck, None leaves them unmarked — the boolean knob the value already encodes is gone.
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
            Kind, $"{row.Target.Key}-{row.IntentRow.Key}", runtime.ContentHash(plate.Pixels), None, plate.Pixels.LongLength,
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
                None: () => row.Alarm.Exists(alarm => alarm.Ink.Count != (int)row.Target.Channels)
                    ? Fin.Fail<PrintProof>(new ExportFault.ProfileInvalid($"{row.Key}:alarm-channels"))
                    : Fin.Succ(Resolved(row, destination, proofing, pixels))));

    // Intent rows DECLARE black-point compensation and a detected destination black point decides
    // whether it changes anything: a destination reaching true zero black drops the compensation as the
    // no-op it is, while an undetectable black point leaves that declaration standing.
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

    // ONE extended build carries every case. Chain order is source-then-destination by construction
    // with an ink-limit link appended, so whichever PCS the gamut check reads is the destination link's
    // — index 1 — and never a caller-supplied slot. Per-link bpc, intent, and adaptation vectors match
    // chain length, gamut stays null unless proofing (the extended build documents null as "no gamut
    // check"), and an admitted alarm marks on the arm's own context. This build mints and releases its
    // own limit link because it is the only reader of it.
    static Fin<PrintPlate> Run(
        Context context, PrintTransform row, Profile source, Profile destination, Profile? proofing,
        PrintProof proof, ReadOnlyMemory<byte> raster) {
        const int DestinationLink = 1;
        using Profile? limit = proof.Ceiling
            .Filter(ceiling => proof.AreaCoverage > (double)ceiling)
            .Match(
                Some: ceiling => Profile.CreateInkLimitingDeviceLink(context, row.Target.Device, (double)ceiling),
                None: () => (Profile?)null);
        row.Alarm.Iter(alarm => ignore(alarm.Mark(context)));
        Profile[] chain = limit is null ? [source, destination] : [source, destination, limit];
        bool[] bpc = [.. chain.Select((_, link) => link == DestinationLink && proof.BlackPointApplied)];
        Intent[] intents = [.. chain.Select(_ => row.IntentRow.Rendering)];
        double[] adaptation = [.. chain.Select(_ => row.IntentRow.Adaptation)];
        using Transform transform = Transform.Create(
            context, chain, bpc, intents, adaptation, proofing, DestinationLink,
            row.Target.Input, row.Target.Output, proof.Flags);
        byte[] plate = new byte[(long)proof.Pixels * row.Target.OutputStride];
        transform.DoTransform(raster.Span, plate, proof.Pixels);
        return Fin.Succ(new PrintPlate(plate, proof));
    }
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

## [08]-[RESEARCH]

- [PRINT_PROOF_CHAIN]-[OPEN]: does the extended `Transform.Create`'s `gamut` argument reproduce `cmsCreateProofingTransform`'s in-chain press simulation, or perform gamut CHECKING alone while simulation seats the proofing profile inside `profiles` with its own intent slot? lcmsNET documents `gamut` as gamut-check information read only under `CmsFlags.GamutCheck`, so out-of-gamut marking holds while a soft-proof PREVIEW stays unproven. Route: read `cmsCreateProofingTransformTHR` and `cmsCreateExtendedTransform` in the `lcms2` native source for the vectors its proofing entry composes; in-chain seating lands the proofing profile as a chain link with its own `PrintIntent` slot, check-only pins a proofing `ColorTarget` row whose destination IS the press profile.
- [PDF_ANNOTATION_KEYS]-[OPEN]: which annotation keys does Skia's PDF backend honor through `SKCanvas.DrawAnnotation(SKRect, string key, SKData)`? Three named entrypoints pass fixed internal keys and materialize, while an arbitrary key reaching the generic entry drops at a backend that does not recognize it — no return value, no diagnostic. Route: read the key roster the Skia PDF device switches on in the native `SkAnnotationKeys`/`SkPDFDevice` source; a roster becomes an admitted `PdfAnnotation.Keyed` key vocabulary, and an empty roster past those three deletes the `Keyed` case outright.
