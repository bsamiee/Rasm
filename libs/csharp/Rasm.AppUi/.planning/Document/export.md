# [APPUI_DOCUMENT_EXPORT]

Rasm.AppUi document export is one paginated-output owner: the MigraDoc flow DOM composes reports with auto-pagination and running header/footer bands, PDFsharp policy rows harden the PDF arm (AES-256 security, digital signatures whose credential material rides the AppHost secrets lease, AcroForm fields, PDF-UA tagged output), the OOXML part-graph writers carry the XLSX/DOCX/PPTX arm, and the lcmsNET device-CMYK transform rows carry the print-fidelity arm. The page owns the destination union, the typed `ExportFault` rail, the flow-report spec, the PDF policy rows, the Office arm, and the print arm — drafting's sheet-PDF and the diagnostics report-PDF compose THIS owner; capture keeps only the pure-visual `SKDocument` vector-print arm and the FFmpeg encode rows.

## [01]-[INDEX]

- [02]-[EXPORT_DESTINATIONS]: The one destination union; the typed `ExportFault` rail.
- [03]-[FLOW_REPORT]: MigraDoc flow DOM; auto-pagination; running bands; placed visuals.
- [04]-[PDF_POLICY]: Security, signatures over the AppHost secrets lease, AcroForms, PDF-UA.
- [05]-[OFFICE_ARM]: OOXML part-graph writers — XLSX, DOCX, PPTX.
- [06]-[PRINT_ARM]: lcmsNET device-CMYK/ICC transforms; proofing; K-preservation intents.

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
            filePath: static (ctx, file) => IO.lift(() => { File.WriteAllBytes(file.AbsolutePath, ctx.payload); return file.AbsolutePath; }),
            blobLane: static (ctx, blob) => ctx.runtime.BlobWrite(blob.ArtifactKey, ctx.payload),
            bundle: static (ctx, bundle) => ctx.runtime.BundleWrite(bundle.ArtifactName, bundle.Classification, ctx.payload));
}
```

## [03]-[FLOW_REPORT]

- Owner: `ReportSpec` — the flow-report composition row; `ReportBlock` [Union] — the typed content vocabulary the MigraDoc fold consumes; `FlowReport` — the one MigraDoc render surface.
- Cases: `ReportBlock` = Heading · Body · Table · PlacedVisual · Rule · PageBreak.
- Entry: `public static IO<RenderReceipt> Render(VisualRuntime runtime, ReportSpec spec)` — IO rail; the MigraDoc `Document`/`Section` DOM composes from the block seq, `PdfDocumentRenderer` paginates, and the payload delivers through the destination union.
- Auto: pagination, widow/orphan control, running headers/footers with `PageField`/`NumPagesField`, and cross-page table breaking are the MigraDoc layout engine's — the hand-rolled `FlowBlock`/`FlowFold` pagination engine is the deleted form this owner replaces; `FormattedDocument` exposes the measured layout so a page count or block position reads from the renderer, never a local cursor fold; placed visuals enter as `PlacedVisual` rows whose `SKImage` tiles encode through the capture codec axis (`VisualCodec.Encode`) and place as MigraDoc `Image` values — capture stays the one raster owner, the report only places.
- Receipt: one `RenderReceipt` of kind document per report with whole-payload content hash through the kernel-bound delegate and the delivered destination key.
- Packages: PDFsharp-MigraDoc, PDFsharp, SkiaSharp, Rasm.AppHost (project), NodaTime, LanguageExt.Core
- Growth: one `ReportBlock` case extends the content vocabulary; one style row retunes a role's typography mapping; zero new surface.
- Boundary: the MigraDoc flow DOM is the ONE flow-pagination owner — a bespoke page-break fold, a per-format report builder, or a second cursor algebra is the deleted form; typography roles map to MigraDoc styles from the `Theme/typography.md` role rows at composition so a report style never re-mints font literals; the drafting sheet-PDF and the diagnostics report-PDF compose `FlowReport.Render` with their own block seqs — a sibling-page PDF writer is the deleted form.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReportBlock {
    private ReportBlock() { }
    public sealed record Heading(int Level, string Text) : ReportBlock;
    public sealed record Body(string Text) : ReportBlock;
    public sealed record Table(Seq<Seq<string>> Rows, bool Header) : ReportBlock;
    public sealed record PlacedVisual(SKImage Tile, double WidthCm) : ReportBlock;
    public sealed record Rule : ReportBlock;
    public sealed record PageBreak : ReportBlock;
}

public sealed record ReportSpec(
    string Title,
    Seq<ReportBlock> Blocks,
    Option<string> RunningHeader,
    bool PageNumbers,
    PdfPolicy Pdf,
    VisualDestination Destination);

public static class FlowReport {
    public const string Kind = "document";

    public static IO<RenderReceipt> Render(VisualRuntime runtime, ReportSpec spec) =>
        from mark in IO.lift(runtime.Clocks.Mark)
        from payload in IO.lift(() => Compose(spec).ThrowIfFail())
        from sealed_ in PdfPolicies.Apply(runtime, spec.Pdf, payload)
        from destination in ExportDelivery.Deliver(runtime, spec.Destination, sealed_)
        from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
        let receipt = new RenderReceipt(Kind, "pdf", runtime.ContentHash(sealed_), sealed_.LongLength, elapsed, runtime.Correlation, Optional(destination), VisualCodec.ColorPolicy.Display.Key)
        from _ in runtime.Sink(receipt)
        select receipt;

    static Fin<byte[]> Compose(ReportSpec spec) {
        Document document = new();
        Section section = document.AddSection();
        spec.RunningHeader.Iter(header => section.Headers.Primary.AddParagraph(header));
        if (spec.PageNumbers) {
            Paragraph footer = section.Footers.Primary.AddParagraph();
            footer.AddPageField();
            footer.AddText(" / ");
            footer.AddNumPagesField();
        }
        spec.Blocks.Iter(block => Append(section, block));
        PdfDocumentRenderer renderer = new() { Document = document };
        renderer.RenderDocument();
        using MemoryStream sink = new();
        renderer.PdfDocument.Save(sink, closeStream: false);
        return Fin.Succ(sink.ToArray());
    }

    static void Append(Section section, ReportBlock block) {
        switch (block) {
            case ReportBlock.Heading heading: section.AddParagraph(heading.Text, $"Heading{int.Clamp(heading.Level, 1, 6)}"); break;
            case ReportBlock.Body body: section.AddParagraph(body.Text); break;
            case ReportBlock.Table table: AppendTable(section, table); break;
            case ReportBlock.PlacedVisual visual: AppendVisual(section, visual); break;
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
        block.Rows.Iter((cells, index) => {
            MigraDoc.DocumentObjectModel.Tables.Row row = table.AddRow();
            row.HeadingFormat = block.Header && index == 0;
            cells.Iter((value, column) => row.Cells[column].AddParagraph(value));
        });
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
- Auto: the security arm sets `PdfSecuritySettings` with AES-256 encryption through the catalogued `SetEncryptionToV5(bool encryptMetadata)` — the metadata-encryption decision is the `EncryptMetadata` policy column, never an implicit default — plus the owner/user permission columns; the identity arm writes `PdfDocumentInformation` `Title`/`Author`/`Subject`/`Keywords` from the `PdfIdentity` columns so every sealed export carries its identity metadata; the signature arm composes `DigitalSignatureHandler.ForDocument(PdfDocument, IDigitalSigner, DigitalSignatureOptions)` — the signer is COMPOSED at the boundary and its credential material rides the AppHost `Runtime/secrets.md` lease lifecycle (acquire/renew/zeroize; the settled AppHost export naming AppUi), never AppUi-held key bytes, so an absent or expired lease folds to `ExportFault.SignerUnavailable` and a raw key byte field on any row is the deleted form; the AcroForm arm adds typed field rows; the accessibility arm routes composition through `UAManager.ForDocument` (`PdfSharp.UniversalAccessibility`) so tagged structure emits with the content, never as a post-pass.
- Packages: PDFsharp, Rasm.AppHost (project), LanguageExt.Core
- Growth: a new hardening concern is one `PdfPolicy` field; a new permission is one column value; a new identity column is one `PdfIdentity` member; zero new surface.
- Boundary: the signing-credential crossing is a declared ledger row (`Document/export` -> AppHost `Runtime/secrets.md`); PDF-UA tagging composes at document build for the flow-report arm (the `UAManager` wraps the document before content lands) — a tag-after-render pass is the honest degrade for the placed-visual-only arm and is stated per row, never silent.

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
    public static IO<byte[]> Apply(VisualRuntime runtime, PdfPolicy policy, byte[] rendered) =>
        policy is { EncryptAes256: false, Signer.IsNone: true, AcroFields.IsEmpty: true, TaggedUa: false, Identity.IsInert: true }
            ? IO.pure(rendered)
            : IO.lift(() => {
                using MemoryStream source = new(rendered);
                using PdfDocument document = PdfReader.Open(source, PdfDocumentOpenMode.Modify);
                if (policy.TaggedUa) { _ = UAManager.ForDocument(document); }
                policy.Identity.Title.Iter(title => document.Info.Title = title);
                policy.Identity.Author.Iter(author => document.Info.Author = author);
                policy.Identity.Subject.Iter(subject => document.Info.Subject = subject);
                policy.Identity.Keywords.Iter(keywords => document.Info.Keywords = keywords);
                policy.AcroFields.Iter(field => document.AcroForm?.Fields[field.Field]?.Value = new PdfString(field.Value));
                if (policy.EncryptAes256) {
                    document.SecuritySettings.SetEncryptionToV5(policy.EncryptMetadata);
                    policy.OwnerPasswordLease.Iter(lease => document.SecuritySettings.OwnerPassword = lease);
                    document.SecuritySettings.PermitPrint = policy.AllowPrinting;
                    document.SecuritySettings.PermitExtractContent = policy.AllowExtraction;
                }
                using MemoryStream sink = new();
                // ForDocument ATTACHES the signing handler to the document; the subsequent Save computes
                // and embeds the signature — the handler exposes no save verb of its own.
                policy.Signer.Iter(signer => _ = DigitalSignatureHandler.ForDocument(
                    document, signer, policy.SignatureOptions.IfNone(() => new DigitalSignatureOptions())));
                document.Save(sink, closeStream: false);
                return sink.ToArray();
            });
}
```

## [05]-[OFFICE_ARM]

- Owner: `OfficeFormat` [SmartEnum] · `OfficeSpec` · `OfficeSheet` [Union] · `OfficeFidelity` the per-(format × case) materialization vocabulary · `OfficeExport` — the OOXML part-graph arm.
- Cases: `OfficeFormat` = xlsx · pptx · docx; `OfficeSheet` = Table · Chart · Image · RichText; `OfficeFidelity` = Native · Declared · Unsupported.
- Entry: `public static IO<RenderReceipt> Emit(VisualRuntime runtime, OfficeSpec spec)` — the Office IO rail; admission runs the fidelity matrix over every sheet FIRST, so an `Unsupported` combination folds to `ExportFault.ContentUnsupported` before any part writes.
- Auto: XLSX writes through `SpreadsheetDocument.Create`/`WorkbookPart`/`WorksheetPart`/`SheetData`/`Row`/`Cell`; DOCX through `WordprocessingDocument.Create`/`MainDocumentPart`/`Body`/`Paragraph`/`Run`/`Text`; PPTX through `PresentationDocument.Create` and the presentation -> master (+theme, +layout) -> slide part-graph chain, one slide per sheet; all three deliver through the typed part-graph factory and never a raw ZIP/XML write; embedded font faces pack through `FontTablePart.GetStream` so a report renders identically off-machine; rich-text sheets compose the same `ReportBlock` vocabulary as the flow arm so a DOCX section and a PDF section share one content model; every serializer switch is total over the four sheet cases — a silent `_` arm dropping a payload is the deleted form.
- Receipt: one `RenderReceipt` of kind office per emit with whole-payload content hash and the delivered destination key.
- Packages: DocumentFormat.OpenXml, SkiaSharp, Rasm.AppHost (project), NodaTime, LanguageExt.Core
- Growth: one `OfficeFormat` row admits an Office target and one `OfficeSheet` case admits a content kind; a fidelity promotion is one matrix cell flipped as the verified part members land; zero new surface.
- Boundary: the Office destination is the same `VisualDestination` union so the Office emit mints no second destination owner; the fidelity matrix is the honesty law — `Native` cells materialize the payload in its own part vocabulary, `Declared` cells state their projection (a chart lands as its typed point table under a series-name header, a rich-text or table sheet in a text-first format lands as its stated text projection), and `Unsupported` cells (image payloads pending the media-part member verification) reject typed rather than degrade silently — so the union never presents capability its bodies omit; the PPTX arm is a REAL serializer row — the master/theme/layout chain is minimal-valid and the deck theme's scheme colors track the token vocabulary at composition; chart sheets project the `ChartSeriesSpec` points as data rows so the chart vocabulary stays Charts-owned.

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
        [("pptx", "table")] = OfficeFidelity.Declared,
        [("pptx", "chart")] = OfficeFidelity.Declared,
        [("pptx", "richText")] = OfficeFidelity.Declared,
        [("pptx", "image")] = OfficeFidelity.Unsupported,
    }.ToFrozenDictionary();

    public static IO<RenderReceipt> Emit(VisualRuntime runtime, OfficeSpec spec) =>
        from _admit in IO.lift(() => Admitted(spec).ThrowIfFail())
        from mark in IO.lift(runtime.Clocks.Mark)
        from payload in Write(spec)
        from destination in ExportDelivery.Deliver(runtime, spec.Destination, payload)
        from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
        let receipt = new RenderReceipt(Kind, spec.Format.Key, runtime.ContentHash(payload), payload.LongLength, elapsed, runtime.Correlation, Optional(destination), VisualCodec.ColorPolicy.Display.Key)
        from _ in runtime.Sink(receipt)
        select receipt;

    static IO<byte[]> Write(OfficeSpec spec) =>
        spec.Format.Key switch {
            "xlsx" => IO.lift(() => WriteXlsx(spec)),
            "docx" => IO.lift(() => WriteDocx(spec)),
            _ => IO.lift(() => WritePptx(spec)),
        };

    static byte[] WriteXlsx(OfficeSpec spec) {
        using MemoryStream sink = new();
        using (SpreadsheetDocument doc = SpreadsheetDocument.Create(sink, SpreadsheetDocumentType.Workbook)) {
            WorkbookPart workbook = doc.AddWorkbookPart();
            workbook.Workbook = new Workbook();
            Sheets sheets = workbook.Workbook.AppendChild(new Sheets());
            spec.Sheets.Iter((sheet, index) => {
                WorksheetPart part = workbook.AddNewPart<WorksheetPart>();
                SheetData data = new();
                Rows(sheet).Iter(row => data.Append(row));
                part.Worksheet = new Worksheet(data);
                sheets.Append(new Sheet { Id = workbook.GetIdOfPart(part), SheetId = (uint)(index + 1), Name = SheetName(sheet) });
            });
            EmbedFonts(workbook, spec.EmbeddedFonts);
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
        table: static t => t.Rows.Map(cells => TextRow(string.Join('\t', cells))),
        placedVisual: static _ => Seq<Row>(),
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
        table: static t => t.Rows.Map(static cells => new Paragraph(new Run(new Text(string.Join('\t', cells))))),
        placedVisual: static _ => Seq<Paragraph>(),
        rule: static _ => Seq<Paragraph>(),
        pageBreak: static _ => Seq<Paragraph>());

    static string SheetName(OfficeSheet sheet) =>
        sheet.Switch(table: static t => t.Name, chart: static c => c.Name, image: static i => i.Name, richText: static r => r.Name);

    static void EmbedFonts(WorkbookPart workbook, Seq<(string FontFamily, ReadOnlyMemory<byte> Face)> fonts) =>
        fonts.Iter(font => {
            FontTablePart part = workbook.AddNewPart<FontTablePart>();
            using Stream stream = part.GetStream();
            stream.Write(font.Face.Span);
        });

    // The PPTX slide part-graph: presentation -> master (+theme, +layout) -> one slide per sheet, every
    // sheet case landing as the slide's one text body — the same content union, a third serializer row.
    // P = DocumentFormat.OpenXml.Presentation, D = DocumentFormat.OpenXml.Drawing (fence-local aliases).
    static byte[] WritePptx(OfficeSpec spec) {
        using MemoryStream sink = new();
        using (PresentationDocument doc = PresentationDocument.Create(sink, PresentationDocumentType.Presentation)) {
            PresentationPart presentation = doc.AddPresentationPart();
            SlideMasterPart master = presentation.AddNewPart<SlideMasterPart>();
            master.SlideMaster = new P.SlideMaster(new P.CommonSlideData(ShapeRoot(None)), DeckColorMap());
            master.AddNewPart<ThemePart>().Theme = DeckTheme();
            SlideLayoutPart layout = master.AddNewPart<SlideLayoutPart>();
            layout.SlideLayout = new P.SlideLayout(new P.CommonSlideData(ShapeRoot(None)), new P.ColorMapOverride(new D.MasterColorMapping()));
            master.SlideMaster.Append(new P.SlideLayoutIdList(new P.SlideLayoutId { Id = 2147483649U, RelationshipId = master.GetIdOfPart(layout) }));
            P.SlideIdList slideIds = new();
            spec.Sheets.Iter((index, sheet) => {
                SlidePart slide = presentation.AddNewPart<SlidePart>();
                slide.Slide = new P.Slide(new P.CommonSlideData(ShapeRoot(Some(SlideText(sheet)))));
                slide.AddPart(layout);
                slideIds.Append(new P.SlideId { Id = (uint)(256 + index), RelationshipId = presentation.GetIdOfPart(slide) });
            });
            presentation.Presentation = new P.Presentation(
                new P.SlideMasterIdList(new P.SlideMasterId { Id = 2147483648U, RelationshipId = presentation.GetIdOfPart(master) }),
                slideIds,
                new P.SlideSize { Cx = 12192000, Cy = 6858000 });
            presentation.Presentation.Save();
        }
        return sink.ToArray();
    }

    static P.ShapeTree ShapeRoot(Option<P.Shape> body) {
        P.ShapeTree tree = new(
            new P.NonVisualGroupShapeProperties(
                new P.NonVisualDrawingProperties { Id = 1U, Name = string.Empty },
                new P.NonVisualGroupShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()),
            new P.GroupShapeProperties(new D.TransformGroup()));
        body.Iter(shape => tree.Append(shape));
        return tree;
    }

    static P.Shape SlideText(OfficeSheet sheet) =>
        new(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties { Id = 2U, Name = SheetName(sheet) },
                new P.NonVisualShapeDrawingProperties(new D.ShapeLocks { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties(new P.PlaceholderShape())),
            new P.ShapeProperties(),
            new P.TextBody(
                new D.BodyProperties(),
                new D.ListStyle(),
                [.. SlideLines(sheet).Map(static line => new D.Paragraph(new D.Run(new D.Text(line))))]));

    static Seq<string> SlideLines(OfficeSheet sheet) => sheet.Switch(
        table: static t => t.Rows.Map(static cells => string.Join('\t', cells)),
        chart: static c => c.Name.Cons(c.Points.Map(static point => $"{point.X}\t{point.Y}")),
        image: static _ => Seq<string>(),
        richText: static r => r.Blocks.Bind(BlockLine));

    static Seq<string> BlockLine(ReportBlock block) => block.Switch(
        heading: static h => Seq(h.Text),
        body: static b => Seq(b.Text),
        table: static t => t.Rows.Map(static cells => string.Join('\t', cells)),
        placedVisual: static _ => Seq<string>(),
        rule: static _ => Seq<string>(),
        pageBreak: static _ => Seq<string>());

    static P.ColorMap DeckColorMap() => new() {
        Background1 = D.ColorSchemeIndexValues.Light1, Text1 = D.ColorSchemeIndexValues.Dark1,
        Background2 = D.ColorSchemeIndexValues.Light2, Text2 = D.ColorSchemeIndexValues.Dark2,
        Accent1 = D.ColorSchemeIndexValues.Accent1, Accent2 = D.ColorSchemeIndexValues.Accent2,
        Accent3 = D.ColorSchemeIndexValues.Accent3, Accent4 = D.ColorSchemeIndexValues.Accent4,
        Accent5 = D.ColorSchemeIndexValues.Accent5, Accent6 = D.ColorSchemeIndexValues.Accent6,
        Hyperlink = D.ColorSchemeIndexValues.Hyperlink, FollowedHyperlink = D.ColorSchemeIndexValues.FollowedHyperlink,
    };

    // Minimal-valid theme: the scheme colors track the token vocabulary at composition; the format scheme
    // rows are the three mandated placeholder styles per list.
    static D.Theme DeckTheme() =>
        new(new D.ThemeElements(
            new D.ColorScheme(
                new D.Dark1Color(new D.SystemColor { Val = D.SystemColorValues.WindowText }),
                new D.Light1Color(new D.SystemColor { Val = D.SystemColorValues.Window }),
                new D.Dark2Color(new D.RgbColorModelHex { Val = "1F3864" }),
                new D.Light2Color(new D.RgbColorModelHex { Val = "EEECE1" }),
                new D.Accent1Color(new D.RgbColorModelHex { Val = "4F81BD" }),
                new D.Accent2Color(new D.RgbColorModelHex { Val = "C0504D" }),
                new D.Accent3Color(new D.RgbColorModelHex { Val = "9BBB59" }),
                new D.Accent4Color(new D.RgbColorModelHex { Val = "8064A2" }),
                new D.Accent5Color(new D.RgbColorModelHex { Val = "4BACC6" }),
                new D.Accent6Color(new D.RgbColorModelHex { Val = "F79646" }),
                new D.Hyperlink(new D.RgbColorModelHex { Val = "0000FF" }),
                new D.FollowedHyperlinkColor(new D.RgbColorModelHex { Val = "800080" })) { Name = "rasm" },
            new D.FontScheme(
                new D.MajorFont(new D.LatinFont { Typeface = "Inter" }, new D.EastAsianFont { Typeface = "" }, new D.ComplexScriptFont { Typeface = "" }),
                new D.MinorFont(new D.LatinFont { Typeface = "Inter" }, new D.EastAsianFont { Typeface = "" }, new D.ComplexScriptFont { Typeface = "" })) { Name = "rasm" },
            new D.FormatScheme(
                new D.FillStyleList(SchemeFills()),
                new D.LineStyleList(SchemeLines()),
                new D.EffectStyleList(SchemeEffects()),
                new D.BackgroundFillStyleList(SchemeFills())) { Name = "rasm" })) { Name = "rasm" };

    static IEnumerable<OpenXmlElement> SchemeFills() =>
        Enumerable.Range(0, 3).Select(static _ => (OpenXmlElement)new D.SolidFill(new D.SchemeColor { Val = D.SchemeColorValues.PhColor }));

    static IEnumerable<OpenXmlElement> SchemeLines() =>
        Enumerable.Range(0, 3).Select(static _ => (OpenXmlElement)new D.Outline(new D.SolidFill(new D.SchemeColor { Val = D.SchemeColorValues.PhColor })));

    static IEnumerable<OpenXmlElement> SchemeEffects() =>
        Enumerable.Range(0, 3).Select(static _ => (OpenXmlElement)new D.EffectStyle(new D.EffectList()));
}
```

## [06]-[PRINT_ARM]

- Owner: `PrintIntent` [SmartEnum] — the rendering-intent policy rows; `PrintTransform` — the lcmsNET profile-to-profile transform row; `PrintArm` — the device-CMYK conversion surface.
- Cases: `PrintIntent` = perceptual · relative-colorimetric · saturation · absolute-colorimetric · relative-bpc · preserve-k — the K-preservation and black-point-compensation intents are policy rows, never flags scattered at call sites.
- Entry: `public static IO<byte[]> Convert(VisualRuntime runtime, PrintTransform row, ReadOnlyMemory<byte> rgba)` — IO rail; one `Transform.Create` per row composed once, `DoTransform` per payload.
- Auto: source and destination ICC profiles parse through `Profile.Open`; the proofing arm dispatches on the row's `ProofProfile` — Some selects the three-profile soft-proofing `Transform.Create` overload (proofing profile + separate proofing intent under `CmsFlags.SoftProofing`) so a soft-proof preview renders the print gamut on the display without a second conversion path, None selects the two-profile device conversion; the native lcms2 ships with the app.
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

public sealed record PrintTransform(string Key, ReadOnlyMemory<byte> SourceProfile, ReadOnlyMemory<byte> DestinationProfile, PrintIntent IntentRow, Option<ReadOnlyMemory<byte>> ProofProfile);

public static class PrintArm {
    public const string Kind = "print";

    // The proofing dispatch is the row's ProofProfile: Some builds the three-profile soft-proofing
    // Transform.Create overload (separate proofing intent, SoftProofing flag), None the two-profile
    // device conversion — one Create name, argument-shape discrimination, K/BPC riding the intent row.
    public static IO<byte[]> Convert(VisualRuntime runtime, PrintTransform row, ReadOnlyMemory<byte> rgba) =>
        IO.lift(() => {
            using Profile source = Profile.Open(row.SourceProfile.Span.ToArray());
            using Profile destination = Profile.Open(row.DestinationProfile.Span.ToArray());
            using Transform transform = row.ProofProfile.Match(
                Some: proofBytes => {
                    using Profile proof = Profile.Open(proofBytes.Span.ToArray());
                    return Transform.Create(
                        source, Cms.TYPE_RGBA_8, destination, Cms.TYPE_CMYK_8, proof,
                        row.IntentRow.Rendering, row.IntentRow.Rendering, row.IntentRow.Flags | CmsFlags.SoftProofing);
                },
                None: () => Transform.Create(
                    source, Cms.TYPE_RGBA_8, destination, Cms.TYPE_CMYK_8, row.IntentRow.Rendering, row.IntentRow.Flags));
            byte[] cmyk = new byte[rgba.Length];
            transform.DoTransform(rgba.Span.ToArray(), cmyk, rgba.Length / 4);
            return cmyk;
        });
}
```

## [07]-[RESEARCH]

- [OFFICE_OPENXML]: the residual OOXML surface is the cell-style and run-formatting member spellings (`CellFormats`/`RunProperties` numbering for a styled report), the deterministic-ordering knobs (`OpenSettings`/relationship-id ordering) that make the OOXML byte-stream byte-reproducible across runs, the `Wordprocessing` table part members that promote the docx table cell from `Declared` tab-joined text to `Native`, and the image/drawing part members that promote the image cells off `Unsupported` — each verified member flips exactly one fidelity-matrix cell; the `OfficeFormat` axis, the `OfficeSheet` content union, the `OfficeFidelity` matrix, the `OfficeSpec`, the `FontTablePart` embedded-font packing, and the PPTX presentation/master/theme/layout/slide part-graph chain are settled.
- [MIGRADOC_STYLE_MAP]: the exact MigraDoc style-name registration for the `Theme/typography.md` role rows (per-role `Style` objects on the `Document.Styles` collection) binds at implementation; the role-to-style mapping shape is settled.
