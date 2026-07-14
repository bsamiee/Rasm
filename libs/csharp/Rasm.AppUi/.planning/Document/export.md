# [APPUI_DOCUMENT_EXPORT]

Rasm.AppUi document export is one paginated-output owner: the MigraDoc flow DOM composes reports with auto-pagination and running header/footer bands, PDFsharp policy rows harden the PDF arm (AES-256 security, digital signatures whose credential material rides the AppHost secrets lease, AcroForm fields, PDF-UA tagged output), the OOXML part-graph writers carry the XLSX/DOCX/PPTX arm, and the lcmsNET device-CMYK transform rows carry the print-fidelity arm. The page owns the destination union, the typed `ExportFault` rail, the flow-report spec, the PDF policy rows, the Office arm, and the print arm — drafting's paginated flow reports and the diagnostics report-PDF (`Diagnostics/evidence.md#CORRELATION_JOIN` `EvidenceReport.Blocks` over the evidence timeline) compose THIS owner; the drafting sheet-PDF rides capture's pure-visual `SKDocument` vector-print arm, and capture keeps that arm plus the FFmpeg encode rows.

## [01]-[INDEX]

- [02]-[EXPORT_DESTINATIONS]: The one destination union; the typed `ExportFault` rail.
- [03]-[FLOW_REPORT]: MigraDoc flow DOM; auto-pagination; running bands; placed visuals.
- [04]-[PDF_POLICY]: Security, signatures over the AppHost secrets lease, AcroForms, PDF-UA.
- [05]-[OFFICE_ARM]: OOXML part-graph writers — XLSX, DOCX, PPTX.
- [06]-[PRINT_ARM]: lcmsNET device-CMYK/ICC transforms; proofing; K-preservation intents.
- [07]-[SCHEDULED_EXPORT]: Consumer-owned `ScheduleEntry` rows for recurring report delivery and bounded backfill.

## [02]-[EXPORT_DESTINATIONS]

- Owner: `VisualDestination` [Union] — the one delivery vocabulary every export arm and the capture vector-print/video arms deliver through; `ExportFault` — the typed export rail; `ExportDelivery` — the one delivery fold.
- Cases: FilePath · BlobLane · Bundle.
- Entry: `public static IO<string> Deliver(VisualRuntime runtime, VisualDestination destination, byte[] payload)` — IO rail; the FilePath arm receives its absolute path as a value from the picker intent and never computes paths; artifact scopes resolve from `ProfileRoots`.
- Auto: the Bundle arm delivers artifacts through the runtime `BundleWrite` delegate with their classification — the support-contributor consequence; every delivered payload seals a `RenderReceipt` of kind document/office/print whose `FrameHash` mints through the runtime `ContentHash` delegate bound to the kernel `Rasm.Domain` `ContentHash.Of` entry.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm (project)
- Growth: one destination case extends delivery and breaks the dispatch at compile time; one export target is one row on the owning arm, never a second engine; one `ExportFault` case is one `detail` ordinal under the `AppUiFaultBand.Export` row (6420); zero new surface.
- Boundary: this union is the ONE export-destination owner — capture's vector-print arm, the FFmpeg clip rows, drafting's sheet-PDF egress, and the diagnostics report-PDF all deliver through it, so a per-arm destination enum is the deleted form; every fault on this page derives through the `Diagnostics/evidence.md#FAULT_TABLES` registry — a bare `Error.New` is the deleted form.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VisualDestination {
    private VisualDestination() { }
    public sealed record FilePath(string AbsolutePath) : VisualDestination;
    public sealed record BlobLane(string ArtifactKey) : VisualDestination;
    public sealed record Bundle(string ArtifactName, DataClassification Classification) : VisualDestination;
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
}

public static class ExportDelivery {
    public static IO<string> Deliver(VisualRuntime runtime, VisualDestination destination, byte[] payload) =>
        destination.Switch(
            state: (runtime, payload),
            filePath: static (ctx, file) => AtomicFile(file.AbsolutePath, ctx.payload),
            blobLane: static (ctx, blob) => ctx.runtime.BlobWrite(blob.ArtifactKey, ctx.payload),
            bundle: static (ctx, bundle) => ctx.runtime.BundleWrite(bundle.ArtifactName, bundle.Classification, ctx.payload));

    static IO<string> AtomicFile(string destination, byte[] payload) {
        string? directory = Path.GetDirectoryName(destination);
        if (string.IsNullOrWhiteSpace(directory)) { return IO.fail<string>(new ExportFault.DeliveryFailed(destination, "destination has no directory")); }
        string pending = Path.Combine(directory, $".{Path.GetFileName(destination)}.{Guid.NewGuid():N}.pending");
        return IO.lift(() => {
                try { File.WriteAllBytes(pending, payload); File.Move(pending, destination, overwrite: true); return destination; }
                finally { if (File.Exists(pending)) { File.Delete(pending); } }
            })
            | @catch<IO, string>(static _ => true, error => IO.fail<string>(new ExportFault.DeliveryFailed(destination, error.Message)));
    }
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
- Boundary: the MigraDoc flow DOM is the ONE flow-pagination owner — a bespoke page-break fold, a per-format report builder, or a second cursor algebra is the deleted form; typography roles map to MigraDoc styles from the `Theme/typography.md` role rows at composition so a report style never re-mints font literals; drafting's paginated flow reports and the diagnostics report-PDF (`EvidenceReport.Blocks` feeding this arm) compose `FlowReport.Render` with their own block seqs, while the drafting sheet-PDF is capture's vector-print arm — a sibling-page PDF writer is the deleted form; the page geometry is the `ReportSetup` policy row applied once to the section `PageSetup`, never per-block layout literals.

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
        let receipt = new RenderReceipt(Kind, "pdf", runtime.ContentHash(sealed_), sealed_.LongLength, elapsed, runtime.Correlation, Optional(destination), VisualCodec.ColorPolicy.Display.Key)
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

    // One PageSetup pass per section: dimension and margin columns land in centimeters; the exact
    // MigraDoc PageSetup member spellings bind per the [MIGRADOC_STYLE_MAP] research row.
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

- Owner: `PdfPolicy` — the one PDF-hardening policy row; `PdfIdentity` — the document-information identity columns every sealed artifact carries beside its content hash; `PdfPolicies` — the apply fold over the rendered payload.
- Entry: `public static IO<byte[]> Apply(VisualRuntime runtime, PdfPolicy policy, byte[] rendered)` — IO rail; opens the rendered payload through `PdfReader`, applies the enabled arms, and re-saves.
- Auto: the security arm selects AES-256 through `PdfDocument.SecurityHandler.SetEncryptionToV5(bool encryptMetadata)` and applies permissions through `PdfDocument.SecuritySettings`; identity writes `PdfDocumentInformation`; signatures compose `DigitalSignatureHandler.ForDocument`; and AcroForm rows write through the catalogued field surface. `TaggedUa` attaches `UAManager` to the renderer document before `RenderDocument`, so structure emits with content.
- Packages: PDFsharp, Rasm.AppHost (project), LanguageExt.Core
- Growth: a new hardening concern is one `PdfPolicy` field; a new permission is one column value; a new identity column is one `PdfIdentity` member; zero new surface.
- Boundary: the signing-credential crossing is a declared ledger row (`Document/export` -> AppHost `Runtime/secrets.md`). PDF-UA tagging composes before content materialization; the post-render policy pass applies security, identity, forms, and signatures without re-tagging the document.

```csharp signature
// Identity metadata beside the content hash: every sealed export names itself through the catalogued
// PdfDocumentInformation columns; an inert identity skips the modify pass entirely.
public sealed record PdfIdentity(Option<string> Title, Option<string> Author, Option<string> Subject, Option<string> Keywords) {
    public static readonly PdfIdentity Inert = new(Option<string>.None, Option<string>.None, Option<string>.None, Option<string>.None);
    public bool IsInert => Title.IsNone && Author.IsNone && Subject.IsNone && Keywords.IsNone;
}

public sealed record PdfPolicy(
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

    public static readonly PdfPolicy Plain = new(false, false, None, true, true, None, None, [], false, PdfIdentity.Inert);
    public static readonly PdfPolicy Archival = new(false, false, None, true, true, None, None, [], TaggedUa: true, Identity: PdfIdentity.Inert);
}

public static class PdfPolicies {
    // The modify pass folds its native failures typed: a throw with a signer bound classifies
    // SignerUnavailable (credential lease and crypto path), anything else RenderFailed("pdf-policy").
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
            policy.AcroFields.Iter(field => document.AcroForm?.Fields[field.Field]?.Value = new PdfString(field.Value));
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

    // The fidelity matrix is the honesty law: Native = own part vocabulary, Declared = stated projection,
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

    public static IO<RenderReceipt> Emit(VisualRuntime runtime, OfficeSpec spec) =>
        from _admit in IO.lift(() => Admitted(spec)).Bind(static fin => fin.Match(Succ: IO.pure, Fail: IO.fail<Unit>))
        from mark in IO.lift(runtime.Clocks.Mark)
        from payload in Write(spec)
        from destination in ExportDelivery.Deliver(runtime, spec.Destination, payload)
        from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
        let receipt = new RenderReceipt(Kind, spec.Format.Key, runtime.ContentHash(payload), payload.LongLength, elapsed, runtime.Correlation, Optional(destination), VisualCodec.ColorPolicy.Display.Key)
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

    static Fin<Unit> Admitted(OfficeSpec spec) =>
        spec.Sheets.TraverseM(sheet => Support[(spec.Format.Key, sheet.Kind)] == OfficeFidelity.Unsupported
            ? Fin.Fail<Unit>(new ExportFault.ContentUnsupported(spec.Format.Key, SheetName(sheet)))
            : Fin.Succ(unit)).As().Map(static _ => unit);

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

- Owner: `PrintIntent` [SmartEnum] — the rendering-intent policy rows; `PrintTransform` — the lcmsNET profile-to-profile transform row; `PrintArm` — the device-CMYK conversion surface.
- Cases: `PrintIntent` = perceptual · relative-colorimetric · saturation · absolute-colorimetric · relative-bpc · preserve-k — the K-preservation and black-point-compensation intents are policy rows, never flags scattered at call sites.
- Entry: `public static IO<byte[]> Convert(VisualRuntime runtime, PrintTransform row, ReadOnlyMemory<byte> rgba)` — IO rail; one `Transform.Create` per row composed once, `DoTransform` per payload.
- Auto: source and destination ICC profiles parse through `Profile.Open`; the proofing arm dispatches on the row's `ProofProfile` — Some selects the three-profile soft-proofing `Transform.Create` overload (proofing profile + separate proofing intent under `CmsFlags.SoftProofing`) so a soft-proof preview renders the print gamut on the display without a second conversion path, None selects the two-profile device conversion; the `GamutCheck` column ORs `CmsFlags.GamutCheck` onto the proofing flags so out-of-gamut pixels mark with the `Cms.AlarmCodes` alarm colors — the prepress gamut-warning preview is one policy column, never a second transform path; the native lcms2 ships with the app.
- Receipt: one `RenderReceipt` of kind print per conversion; the `ColorSpace` field carries the destination profile key so a print baseline keys distinctly.
- Packages: lcmsNET, Rasm.AppHost (project), LanguageExt.Core
- Growth: a new intent is one `PrintIntent` row; a new device profile is one `PrintTransform` value from profile bytes; zero new surface.
- Boundary: lcmsNET owns device-CMYK/ICC transforms at the print boundary ONLY — Unicolour stays the suite color-model kernel and `VisualCodec.ColorPolicy` stays the capture codec gamut family, three disjoint charters; an unparseable profile folds to `ExportFault.ProfileInvalid`, never a silent sRGB fallback; `Cms.TYPE_RGBA_8` -> `Cms.TYPE_CMYK_8` is the standard lane and the 16-bit lane is one row value.

```csharp signature
[SmartEnum<string>]
public sealed partial class PrintIntent {
    public static readonly PrintIntent Perceptual = new("perceptual", Intent.Perceptual, CmsFlags.None);
    public static readonly PrintIntent Relative = new("relative-colorimetric", Intent.RelativeColorimetric, CmsFlags.None);
    public static readonly PrintIntent Saturation = new("saturation", Intent.Saturation, CmsFlags.None);
    public static readonly PrintIntent Absolute = new("absolute-colorimetric", Intent.AbsoluteColorimetric, CmsFlags.None);
    public static readonly PrintIntent RelativeBpc = new("relative-bpc", Intent.RelativeColorimetric, CmsFlags.BlackPointCompensation);
    public static readonly PrintIntent PreserveK = new("preserve-k", Intent.PreserveKPlaneRelativeColorimetric, CmsFlags.BlackPointCompensation);

    public Intent Rendering { get; }
    public CmsFlags Flags { get; }
}

public sealed record PrintTransform(
    string Key,
    ReadOnlyMemory<byte> SourceProfile,
    ReadOnlyMemory<byte> DestinationProfile,
    PrintIntent IntentRow,
    PrintIntent ProofIntent,
    Option<ReadOnlyMemory<byte>> ProofProfile,
    bool GamutCheck);

public static class PrintArm {
    public const string Kind = "print";

    // The proofing dispatch is the row's ProofProfile: Some builds the three-profile soft-proofing
    // Transform.Create overload (separate proofing intent, SoftProofing flag, GamutCheck marking
    // out-of-gamut pixels with the alarm colors), None the two-profile device conversion — one Create
    // name, argument-shape discrimination, K/BPC riding the intent row. A successful conversion seals
    // one RenderReceipt of kind print through the runtime Sink — destination-profile identity in
    // ColorSpace, whole-payload content hash, byte count, elapsed — so print baselines key distinctly on
    // the shared render-evidence rail; a failed conversion stays on ExportFault with no success receipt.
    public static IO<byte[]> Convert(VisualRuntime runtime, PrintTransform row, ReadOnlyMemory<byte> rgba) =>
        from mark in IO.lift(runtime.Clocks.Mark)
        from cmyk in IO.lift(() => Transformed(row, rgba)).Bind(static fin => fin.Match(Succ: IO.pure, Fail: IO.fail<byte[]>))
        from _ in runtime.Sink(new RenderReceipt(
            Kind, $"cmyk-{row.IntentRow.Key}", runtime.ContentHash(cmyk), cmyk.Length,
            runtime.Clocks.Elapsed(mark), runtime.Correlation, None, row.Key))
        select cmyk;

    // lcms native boundary kernel: each profile parse classifies its own ProfileInvalid ordinal and a
    // transform failure classifies RenderFailed — every advertised fault case has a producing path.
    static Fin<byte[]> Transformed(PrintTransform row, ReadOnlyMemory<byte> rgba) {
        Profile source, destination;
        Option<Profile> proof;
        try { source = Profile.Open(row.SourceProfile.Span.ToArray()); }
        catch { return Fin.Fail<byte[]>(new ExportFault.ProfileInvalid($"{row.Key}:source")); }
        try { destination = Profile.Open(row.DestinationProfile.Span.ToArray()); }
        catch { source.Dispose(); return Fin.Fail<byte[]>(new ExportFault.ProfileInvalid($"{row.Key}:destination")); }
        try { proof = row.ProofProfile.Map(bytes => Profile.Open(bytes.Span.ToArray())); }
        catch { source.Dispose(); destination.Dispose(); return Fin.Fail<byte[]>(new ExportFault.ProfileInvalid($"{row.Key}:proof")); }
        try {
            using Transform transform = proof.Match(
                Some: proofProfile => Transform.Create(
                    source, Cms.TYPE_RGBA_8, destination, Cms.TYPE_CMYK_8, proofProfile,
                    row.IntentRow.Rendering, row.ProofIntent.Rendering,
                    row.IntentRow.Flags | CmsFlags.SoftProofing | (row.GamutCheck ? CmsFlags.GamutCheck : CmsFlags.None)),
                None: () => Transform.Create(
                    source, Cms.TYPE_RGBA_8, destination, Cms.TYPE_CMYK_8, row.IntentRow.Rendering, row.IntentRow.Flags));
            byte[] cmyk = new byte[rgba.Length];
            transform.DoTransform(rgba.Span.ToArray(), cmyk, rgba.Length / 4);
            return Fin.Succ(cmyk);
        }
        catch (Exception error) { return Fin.Fail<byte[]>(new ExportFault.RenderFailed("print", error.Message)); }
        finally { source.Dispose(); destination.Dispose(); proof.Iter(static held => held.Dispose()); }
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
