using System.Drawing.Imaging;
using System.Globalization;
using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;
using Rhino.Collections;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using DetailViewGeometry = Rhino.Geometry.DetailView;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using IOFileInfo = System.IO.FileInfo;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "runtime")]
public abstract partial record FileExchange {
    private FileExchange() { }
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Import(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Export(FileEndpoint Target, Option<DocumentTarget> Objects, FileProfile Profile) : FileExchange;
    public sealed record Save : FileExchange;
    public sealed record SaveAs(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record WriteFile(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record Write3dmFile(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record SaveTemplate(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record Publish(FilePublish Spec) : FileExchange;
    public sealed record NamedLayerState(FileLayerState Change) : FileExchange;
    public sealed record ArchiveRead(FileArchiveSource Source, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveExtract(FileArchiveSource Source, FileEndpoint Target, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveUpdate(FileArchiveSource Source, FileEndpoint Target, Exchange.ArchiveUpdate Update, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveInspect(FileEndpoint Source) : FileExchange;
    public sealed record ArchiveValidate(FileArchiveSource Source, ArchiveProfile Profile) : FileExchange;
    public sealed record SheetEdit(FileSheetEdit Edit) : FileExchange;
}

public readonly record struct FilePdfPage(
    int WidthDots,
    int HeightDots,
    int Dpi,
    Option<Func<FilePdf, int, Op, Fin<Unit>>> Annotate = default);

[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record FilePublishTarget {
    private FilePublishTarget() { }
    // BOUNDARY ADAPTER — `FilePdf.SetCustomPages` REPLACES previously-added pages, so it cannot be
    // mixed with `AddPage(ViewCaptureSettings)`. `Prefix` and `Suffix` use the blank-page
    // `AddPage(width, height, dpi)` overload sequenced before/after sheet captures; each page
    // carries its own annotation callback for cover/title/signature stamping. The top-level
    // `Annotate` applies to sheet pages only.
    public sealed record Pdf(
        FileEndpoint Target,
        Seq<FilePdfPage> Prefix = default,
        Seq<FilePdfPage> Suffix = default,
        Option<Func<FilePdf, int, Op, Fin<Unit>>> Annotate = default) : FilePublishTarget;
    public sealed record Printer(string Name, int Copies = 1) : FilePublishTarget;
    public sealed record Raster(FileEndpoint Target, FileRasterEncoding? Encoding = null, FileRasterSettings Settings = default) : FilePublishTarget {
        internal FileRasterEncoding ResolvedEncoding => Encoding ?? FileRasterEncoding.Png;
    }
    public sealed record Svg(FileEndpoint Target) : FilePublishTarget;

    internal Fin<FilePublishResult> Write(Seq<FileSheetPage> pages, bool layers, Op op) =>
        Switch(
            (Pages: pages, Layers: layers, Op: op),
            pdf: static (ctx, target) => WritePdf(target: target, pages: ctx.Pages, layers: ctx.Layers, op: ctx.Op),
            printer: static (ctx, target) => WritePrinter(target: target, pages: ctx.Pages, op: ctx.Op),
            raster: static (ctx, target) => WriteRaster(target: target, pages: ctx.Pages, op: ctx.Op),
            svg: static (ctx, target) => WriteSvg(target: target, pages: ctx.Pages, op: ctx.Op));

    private static Fin<FilePublishResult> WritePdf(Pdf target, Seq<FileSheetPage> pages, bool layers, Op op) =>
        from endpoint in target.Target.WithFormat(format: FileFormat.Pdf).Output(op: op)
        from pdf in Optional(FilePdf.Create()).ToFin(Fail: op.InvalidResult())
        from _prefix in target.Prefix.TraverseM(spec => AddBlankPdfPage(pdf: pdf, spec: spec, op: op)).As()
        from _sheets in pages.TraverseM(page =>
            from settings in page.Sheet.Settings(page: page.Page, op: op)
            from index in op.Catch(() => {
                using ViewCaptureSettings owned = settings;
                pdf.LayersAsOptionalContentGroups = layers && !owned.RasterMode;
                return pdf.AddPage(settings: owned) switch {
                    int value when value >= 0 => Fin.Succ(value: value),
                    _ => Fin.Fail<int>(error: op.InvalidResult()),
                };
            })
            from annotated in op.Catch(() => target.Annotate.Map(annotate => annotate(arg1: pdf, arg2: index, arg3: op)).IfNone(Fin.Succ(value: unit)))
            select unit).As()
        from _suffix in target.Suffix.TraverseM(spec => AddBlankPdfPage(pdf: pdf, spec: spec, op: op)).As()
        from _write in op.Catch(() => { pdf.Write(filename: endpoint.Path); return Fin.Succ(value: unit); })
        from _verify in Optional(new IOFileInfo(fileName: endpoint.Path))
            .Filter(static info => info.Exists && info.Length > 0)
            .ToFin(Fail: op.InvalidResult())
        select new FilePublishResult(
            Target: Some(endpoint),
            Format: Some(FileFormat.Pdf),
            Receipt: DocumentReceipt.Empty,
            Sheets: pages.Map(SheetReportOf));

    private static Fin<Unit> AddBlankPdfPage(FilePdf pdf, FilePdfPage spec, Op op) =>
        from index in op.Catch(() => pdf.AddPage(widthInDots: spec.WidthDots, heightInDots: spec.HeightDots, dotsPerInch: spec.Dpi) switch {
            int value when value >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.InvalidResult()),
        })
        from annotated in op.Catch(() => spec.Annotate.Map(annotate => annotate(arg1: pdf, arg2: index, arg3: op)).IfNone(Fin.Succ(value: unit)))
        select unit;

    private static Fin<FilePublishResult> WritePrinter(Printer target, Seq<FileSheetPage> pages, Op op) =>
        from name in FileEndpoint.NonBlank(value: target.Name, op: op)
        from copies in target.Copies switch {
            > 0 => Fin.Succ(value: target.Copies),
            _ => Fin.Fail<int>(error: op.InvalidInput()),
        }
        from settings in pages.TraverseM(page => page.Sheet.Settings(page: page.Page, op: op)).As()
        from result in op.Catch(() => {
            ViewCaptureSettings[] captures = [.. settings];
            try {
                return op.Confirm(success: ViewCapture.SendToPrinter(printerName: name, settings: captures, copies: copies))
                    .Map(_ => new FilePublishResult(
                        Target: Option<FileEndpoint>.None,
                        Format: Option<FileFormat>.None,
                        Receipt: DocumentReceipt.Empty,
                        Sheets: pages.Map(SheetReportOf),
                        NativeLog: Some(string.Create(CultureInfo.InvariantCulture, $"printer:{name};copies:{copies};sheets:{pages.Count}"))));
            } finally {
                _ = toSeq(captures).Iter(static settings => settings.Dispose());
            }
        })
        select result;

    private static Fin<FilePublishResult> WriteRaster(Raster target, Seq<FileSheetPage> pages, Op op) {
        FileRasterEncoding encoding = target.ResolvedEncoding;
        FileRasterSettings settings = target.Settings;
        return from endpoint in target.Target.WithFormat(format: encoding.Format).Output(op: op)
               from written in pages.Map((page, index) => (Index: index, Page: page))
                   .TraverseM(item =>
                       from captureSettings in item.Page.Sheet.Settings(page: item.Page.Page, op: op)
                       from rendered in op.Catch(() => {
                           using ViewCaptureSettings owned = captureSettings;
                           DrawingBitmap? bitmap = ViewCapture.CaptureToBitmap(settings: owned);
                           try {
                               string path = pages.Count == 1 ? endpoint.Path : FileEndpoint.NumberedPath(path: endpoint.Path, index: item.Index + 1);
                               return Optional(bitmap)
                                   .ToFin(Fail: op.InvalidResult())
                                   .Map(active => {
                                       _ = settings.ExifDpi.Iter(dpi => active.SetResolution(xDpi: (float)dpi, yDpi: (float)dpi));
                                       return active;
                                   })
                                   .Bind(active => SaveBitmap(bitmap: active, encoding: encoding, settings: settings, path: path, op: op));
                           } finally {
                               bitmap?.Dispose();
                           }
                       })
                       select rendered).As()
               select new FilePublishResult(
                   Target: Some(endpoint),
                   Format: Some(encoding.Format),
                   Receipt: DocumentReceipt.Empty,
                   Sheets: pages.Map(SheetReportOf),
                   NativeLog: Some(string.Create(CultureInfo.InvariantCulture, $"raster:{encoding.Key};quality:{settings.JpegQuality};sheets:{pages.Count}")));
    }

    private static Fin<FilePublishResult> WriteSvg(Svg target, Seq<FileSheetPage> pages, Op op) =>
        from endpoint in target.Target.WithFormat(format: FileFormat.Svg).Output(op: op)
        from written in pages.Map((page, index) => (Index: index, Page: page))
            .TraverseM(item =>
                from settings in item.Page.Sheet.Settings(page: item.Page.Page, op: op)
                from rendered in op.Catch(() => {
                    using ViewCaptureSettings owned = settings;
                    string path = pages.Count == 1 ? endpoint.Path : FileEndpoint.NumberedPath(path: endpoint.Path, index: item.Index + 1);
                    return Optional(ViewCapture.CaptureToSvg(settings: owned))
                        .ToFin(Fail: op.InvalidResult())
                        .Bind(document => op.Catch(() => {
                            document.Save(filename: path);
                            return Optional(new IOFileInfo(fileName: path)).Filter(static info => info.Exists && info.Length > 0).Map(static _ => unit).ToFin(Fail: op.InvalidResult());
                        }));
                })
                select rendered).As()
        select new FilePublishResult(
            Target: Some(endpoint),
            Format: Some(FileFormat.Svg),
            Receipt: DocumentReceipt.Empty,
            Sheets: pages.Map(SheetReportOf),
            NativeLog: Some(string.Create(CultureInfo.InvariantCulture, $"svg;sheets:{pages.Count}")));

    private static Fin<Unit> SaveBitmap(DrawingBitmap bitmap, FileRasterEncoding encoding, FileRasterSettings settings, string path, Op op) =>
        op.Catch(() => {
            DrawingImageFormat image = encoding.Image;
            Seq<(Encoder Encoder, long Value)> codecParams = encoding.Parameters(settings: settings);
            ImageCodecInfo? codec = codecParams.IsEmpty
                ? null
                : toSeq(ImageCodecInfo.GetImageEncoders()).Find(c => c.FormatID == image.Guid).Case as ImageCodecInfo;
            _ = codec switch {
                ImageCodecInfo active => Op.Side(() => {
                    EncoderParameter[] entries = [.. codecParams.AsIterable().Select(static pair => new EncoderParameter(pair.Encoder, pair.Value))];
                    try {
                        using EncoderParameters parameters = new(entries.Length) { Param = entries };
                        bitmap.Save(filename: path, encoder: active, encoderParams: parameters);
                    } finally {
                        System.Array.ForEach(array: entries, action: static entry => entry.Dispose());
                    }
                }),
                _ => Op.Side(() => bitmap.Save(filename: path, format: image)),
            };
            return Optional(new IOFileInfo(fileName: path))
                .Filter(static info => info.Exists && info.Length > 0)
                .Map(static _ => unit)
                .ToFin(Fail: op.InvalidResult());
        });

    private static FileSheetReport SheetReportOf(FileSheetPage entry) {
        DetailViewObject[] details = entry.Page.GetDetailViews() ?? [];
        Option<string> scale = details.Length > 0 && details[0].GetFormattedScale(format: DetailViewObject.ScaleFormat.OneToModelLength, out string formatted)
            ? Some(formatted)
            : Option<string>.None;
        return new FileSheetReport(PageName: entry.Page.PageName, Scale: scale, DetailCount: details.Length);
    }
}

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record FileLayerState {
    private FileLayerState() { }
    public sealed record Save(string Name, Option<Guid> Viewport = default) : FileLayerState;
    public sealed record Restore(string Name, RestoreLayerProperties Properties, Option<Guid> Viewport = default) : FileLayerState;
    public sealed record Rename(string Name, string Next) : FileLayerState;
    public sealed record Delete(string Name) : FileLayerState;
    public sealed record Import(FileEndpoint Source) : FileLayerState;

    internal Fin<DocumentResourceChange> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            save: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Document.NamedLayerStates.Save(name: name, viewportId: change.Viewport.IfNone(Guid.Empty)) switch {
                    >= 0 => Fin.Succ(value: unit),
                    _ => Fin.Fail<Unit>(error: state.Op.InvalidResult()),
                }
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedLayerState, Name: name),
            restore: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Restore(name: name, properties: change.Properties, viewportId: change.Viewport.IfNone(Guid.Empty)))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedLayerState, Name: name),
            rename: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from next in FileEndpoint.NonBlank(value: change.Next, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Rename(oldName: name, newName: next))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedLayerState, Name: next),
            delete: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Delete(name: name))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedLayerState, Name: name),
            import: static (state, change) =>
                from source in change.Source.Input(op: state.Op)
                from _ in state.Document.NamedLayerStates.Import(filename: source.Path) switch {
                    >= 0 => Fin.Succ(value: unit),
                    _ => Fin.Fail<Unit>(error: state.Op.InvalidResult()),
                }
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedLayerState, Name: source.Path));
}

[Union]
internal abstract partial record HeadlessExchange {
    private HeadlessExchange() { }
    public sealed record Create : HeadlessExchange;
    public sealed record CreateFromTemplate(FileEndpoint Template) : HeadlessExchange;
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : HeadlessExchange;
}

public sealed record FileWatch(
    Option<Action<DocumentOpenEventArgs>> OnBeginOpen = default,
    Option<Action<DocumentOpenEventArgs>> OnEndOpen = default,
    Option<Action<DocumentSaveEventArgs>> OnBeginSave = default,
    Option<Action<DocumentSaveEventArgs>> OnEndSave = default,
    Option<Action<DocumentEventArgs>> OnClose = default,
    Option<Action<DocumentEventArgs>> OnNew = default);

public sealed record FilePlugInRegistration(
    string Key,
    string DisplayName,
    Seq<string> Extensions,
    FileCapability Capability,
    Func<RhinoDoc, FileEndpoint, FileProfile, Op, Fin<Unit>> Read,
    Func<RhinoDoc, FileEndpoint, FileProfile, Op, Fin<Unit>> Write) {
    internal Fin<FileFormat> Register() =>
        FileFormat.Custom(key: Key, extensions: Extensions, capability: Capability);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class FileOp {
    internal static Eff<FileRuntime, T> Lift<T>(Func<FileRuntime, Fin<T>> run) =>
        Eff<FileRuntime, T>.Lift(runtime =>
            Optional(run)
                .ToFin(Fail: Op.Of(name: nameof(Lift)).InvalidInput())
                .Bind(valid => valid(arg: runtime)));

    public static Eff<FileRuntime, FileReport> Do(FileExchange exchange) =>
        Lift(run: runtime =>
            from active in Optional(exchange).ToFin(Fail: Op.Of(name: nameof(Do)).InvalidInput())
            from result in active.Switch(
                runtime,
                open: static (_, open) => OpenCore(source: open.Source, profile: open.Profile),
                import: static (runtime, import) => ImportCore(runtime: runtime, source: import.Source, profile: import.Profile),
                export: static (runtime, export) => ExportCore(runtime: runtime, target: export.Target, objects: export.Objects, profile: export.Profile),
                save: static (runtime, _) => WriteCore(runtime: runtime, target: Option<FileEndpoint>.None, profile: FileProfile.Model, phase: FilePhase.Save),
                saveAs: static (runtime, saveAs) => WriteCore(runtime: runtime, target: Some(saveAs.Target), profile: saveAs.Profile, phase: FilePhase.SaveAs),
                writeFile: static (runtime, write) => WriteCore(runtime: runtime, target: Some(write.Target), profile: write.Profile, phase: FilePhase.WriteFile),
                write3dmFile: static (runtime, write) => WriteCore(runtime: runtime, target: Some(write.Target), profile: write.Profile, phase: FilePhase.Write3dmFile),
                saveTemplate: static (runtime, template) => WriteCore(runtime: runtime, target: Some(template.Target), profile: template.Profile, phase: FilePhase.SaveTemplate),
                publish: static (runtime, publish) => PublishCore(runtime: runtime, publish: publish.Spec),
                namedLayerState: static (runtime, state) => NamedLayerStateCore(runtime: runtime, change: state.Change),
                archiveRead: static (_, archive) => FileArchiveOps.Read(source: archive.Source, profile: archive.Profile),
                archiveExtract: static (_, archive) => FileArchiveOps.Extract(source: archive.Source, target: archive.Target, profile: archive.Profile),
                archiveUpdate: static (_, archive) => FileArchiveOps.Update(source: archive.Source, target: archive.Target, update: archive.Update, profile: archive.Profile),
                archiveInspect: static (_, archive) => FileArchiveOps.Inspect(source: archive.Source).Map(metadata => FileReport.Of(
                    phase: FilePhase.ArchiveInspect,
                    source: Some(archive.Source),
                    archive: Some(new FileArchive(Source: new FileArchiveSource.Path(Value: archive.Source), Metadata: metadata, Resources: default, Objects: Seq<FileObjectManifest>())))),
                archiveValidate: static (_, archive) => FileArchiveOps.Validate(source: archive.Source, profile: archive.Profile),
                sheetEdit: static (runtime, sheet) => SheetEditCore(runtime: runtime, edit: sheet.Edit))
            select result);

    public static Eff<FileRuntime, byte[]> ArchiveBytes(FileArchiveSource source, ArchiveProfile profile) =>
        Lift(run: _ => FileArchiveOps.Bytes(source: source, profile: profile));

    public static Eff<FileRuntime, FileReport> Batch(Seq<Eff<FileRuntime, FileReport>> items, FileBatchPolicy policy) =>
        Lift(run: runtime => items switch {
            Seq<Eff<FileRuntime, FileReport>> values when !values.IsEmpty => values.TraverseM(operation =>
                operation.Run(runtime).BindFail(error => policy.ContinueOnError switch {
                    true => Fin.Succ(value: FileReport.Empty(phase: FilePhase.Batch) with { Issues = Seq(FileIssue.Of(code: FileIssueCode.BatchFailure, message: error.Message)) }),
                    false => Fin.Fail<FileReport>(error: error),
                })).As().Map(reports => FileReport.Of(phase: FilePhase.Batch, issues: reports.Bind(static report => report.Issues), children: reports)),
            _ => Fin.Fail<FileReport>(error: Op.Of(name: nameof(Batch)).InvalidInput()),
        });

    public static Eff<FileRuntime, T> Headless<T>(Eff<FileRuntime, T> body) =>
        HeadlessOp(scope: new HeadlessExchange.Create(), body: body);

    public static Eff<FileRuntime, T> Headless<T>(FileEndpoint source, FileProfile profile, Eff<FileRuntime, T> body) =>
        HeadlessOp(scope: new HeadlessExchange.Open(Source: source, Profile: profile), body: body);

    public static Eff<FileRuntime, T> HeadlessTemplate<T>(FileEndpoint template, Eff<FileRuntime, T> body) =>
        HeadlessOp(scope: new HeadlessExchange.CreateFromTemplate(Template: template), body: body);

    public static Eff<FileRuntime, Seq<FileEndpoint>> Prompt(FilePrompt prompt) =>
        Lift(run: runtime =>
            from active in Optional(prompt).ToFin(Fail: Op.Of(name: nameof(Prompt)).InvalidInput())
            from ui in runtime.Ui.ToFin(Fail: Op.Of(name: nameof(Prompt)).MissingContext())
            from result in ui.Use(intent: UiIntent.ExchangeFile(prompt: active))
            select result);

    public static Eff<FileRuntime, IDisposable> Subscribe(FileWatch watch) =>
        Lift(run: _ => FileWatchSubscription.Of(watch: watch));

    public static Fin<FileFormat> Register(FilePlugInRegistration registration) =>
        from active in Optional(registration).ToFin(Fail: Op.Of(name: nameof(Register)).InvalidInput())
        from format in active.Register()
        select format;

    internal static Eff<FileRuntime, T> HeadlessOp<T>(HeadlessExchange scope, Eff<FileRuntime, T> body) =>
        Lift(run: _ => HeadlessScope(scope: scope, body: body));

    private static Fin<FileReport> OpenCore(FileEndpoint source, FileProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(FileExchange.Open)))
        from format in FileFormatProjection.Require(endpoint: endpoint, profile: profile, phase: FilePhase.Open, op: Op.Of(name: nameof(FileExchange.Open)))
        from _ in format == FileFormat.ThreeDm ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: Op.Of(name: nameof(FileExchange.Open)).InvalidInput())
        from report in Op.Of(name: nameof(FileExchange.Open)).Catch(() => {
            RhinoDoc? opened = RhinoDoc.Open(filePath: endpoint.Path, wasAlreadyOpen: out bool wasAlreadyOpen);
            return Optional(opened)
                .ToFin(Fail: Op.Of(name: nameof(FileExchange.Open)).InvalidResult())
                .Map(document => FileReport.Of(
                    phase: FilePhase.Open,
                    source: Some(endpoint),
                    target: Option<FileEndpoint>.None,
                    format: Some(format),
                    issues: wasAlreadyOpen ? Seq(FileIssue.Of(code: FileIssueCode.AlreadyOpen, message: "Document was already open.")) : Seq<FileIssue>()));
        })
        select report;

    private static Fin<FileReport> ImportCore(FileRuntime runtime, FileEndpoint source, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(FileExchange.Import)))
        from endpoint in source.Input(op: Op.Of(name: nameof(FileExchange.Import)))
        from receipt in live.Edit.Commit(
            name: nameof(FileExchange.Import),
            redraw: DocumentRedraw.After,
            undoRecorded: true,
            run: (document, _, op) =>
                from before in DocumentEdit.LiveObjectIds(document: document)
                from imported in FileFormatProjection.Import(document: document, source: endpoint, profile: profile, op: op)
                from after in DocumentEdit.LiveObjectIds(document: document)
                select DocumentReceipt.Empty with { Created = after.Filter(id => !before.Exists(item => item == id)) })
        select FileReport.Of(phase: FilePhase.Import, source: Some(endpoint), target: Option<FileEndpoint>.None, format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> ExportCore(FileRuntime runtime, FileEndpoint target, Option<DocumentTarget> objects, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(FileExchange.Export)))
        from endpoint in target.Output(op: Op.Of(name: nameof(FileExchange.Export)))
        from receipt in live.Edit.Commit(
            name: nameof(FileExchange.Export),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) => objects.Case switch {
                DocumentTarget selection => ExportTarget(document: document, target: selection, endpoint: endpoint, profile: profile, op: op),
                _ => FileFormatProjection.Write(document: document, target: Some(endpoint), profile: profile, phase: FilePhase.Export, selected: false, op: op).Map(static _ => DocumentReceipt.Empty),
            })
        select FileReport.Of(phase: FilePhase.Export, source: Option<FileEndpoint>.None, target: Some(endpoint), format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> WriteCore(FileRuntime runtime, Option<FileEndpoint> target, FileProfile profile, FilePhase phase) {
        Op op = Op.Of(name: nameof(WriteCore));
        bool archive = phase == FilePhase.SaveAs || phase == FilePhase.Write3dmFile || phase == FilePhase.SaveTemplate;
        return from live in Live(runtime: runtime, op: op)
               from endpoint in phase == FilePhase.Save
                   ? Fin.Succ(Option<FileEndpoint>.None)
                   : target.ToFin(Fail: op.InvalidInput())
                       .Bind(value => (archive ? value.WithFormat(format: FileFormat.ThreeDm) : value).Output(op: op))
                       .Map(Some)
               from receipt in live.Edit.Commit(
                   name: nameof(WriteCore),
                   redraw: DocumentRedraw.None,
                   undoRecorded: false,
                   run: (document, _, inner) => FileFormatProjection.Write(document: document, target: endpoint, profile: profile, phase: phase, selected: false, op: inner).Map(static _ => DocumentReceipt.Empty))
               select FileReport.Of(
                   phase: phase,
                   target: endpoint,
                   format: endpoint.Case switch {
                       FileEndpoint value => FileFormatProjection.Resolve(endpoint: value, profile: profile),
                       _ => profile.Format,
                   },
                   receipt: Some(receipt));
    }

    private static Fin<FileReport> PublishCore(FileRuntime runtime, FilePublish publish) {
        Op op = Op.Of(name: nameof(FileExchange.Publish));
        return from live in Live(runtime: runtime, op: op)
               from active in Optional(publish).ToFin(Fail: op.InvalidInput())
               from target in Optional(active.Target).ToFin(Fail: op.InvalidInput())
               from views in Optional(toSeq(live.Document.Views.GetPageViews()))
                   .Filter(static pages => !pages.IsEmpty)
                   .ToFin(Fail: op.InvalidInput())
               let sheets = active.Sheets.IsEmpty ? views.Map(static page => new FileSheet(Id: Some(page.MainViewport.Id))) : active.Sheets
               from pages in sheets.TraverseM(sheet => MatchSheet(document: live.Document, views: views, sheet: sheet, op: op)).As().Map(static groups => groups.Bind(static items => items))
               from _ in guard(!pages.IsEmpty, op.InvalidInput())
               from result in RhinoUi.Protect(valid: () => target.Write(pages: pages, layers: active.Layers, op: op))
               select FileReport.Of(phase: FilePhase.Publish, target: result.Target, format: result.Format, issues: result.Issues, nativeLog: result.NativeLog, receipt: Some(result.Receipt), sheets: result.Sheets);
    }

    private static Fin<Seq<FileSheetPage>> MatchSheet(RhinoDoc document, Seq<RhinoPageView> views, FileSheet sheet, Op op) {
        Option<int> groupIndex = sheet.Group.Bind(name => Optional(document.PageViewGroups.FindName(name: name)).Map(static active => active.Index));
        bool filterless = sheet.Id.IsNone && sheet.Name.IsNone && sheet.Group.IsNone && sheet.Predicate.IsNone;
        Seq<FileSheetPage> matches = (sheet.Group.Case, groupIndex.Case, filterless) switch {
            (string, not int, _) => Seq<FileSheetPage>(),
            (_, _, true) => views.Head.Map(page => new FileSheetPage(Page: page, Sheet: sheet)).ToSeq(),
            _ => views
                .Filter(page =>
                    sheet.Id.Map(id => page.MainViewport.Id == id).IfNone(noneValue: true)
                    && sheet.Name.Map(name => string.Equals(a: page.PageName, b: name, comparisonType: StringComparison.OrdinalIgnoreCase)).IfNone(noneValue: true)
                    && groupIndex.Map(index => page.IsInPageViewGroup(pageViewGroupIndex: index)).IfNone(noneValue: true)
                    && sheet.Predicate.Map(predicate => predicate(arg: page)).IfNone(noneValue: true))
                .Map(page => new FileSheetPage(Page: page, Sheet: sheet)),
        };
        return matches.IsEmpty
            ? Fin.Fail<Seq<FileSheetPage>>(error: op.InvalidInput())
            : Fin.Succ(value: matches);
    }

    private static Fin<FileReport> NamedLayerStateCore(FileRuntime runtime, FileLayerState change) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(FileExchange.NamedLayerState)))
        from active in Optional(change).ToFin(Fail: Op.Of(name: nameof(FileExchange.NamedLayerState)).InvalidInput())
        from receipt in live.Edit.Commit(
            name: nameof(FileExchange.NamedLayerState),
            redraw: DocumentRedraw.After,
            undoRecorded: true,
            run: (document, _, op) =>
                active.Apply(document: document, op: op)
                    .Map(static changed => DocumentReceipt.Empty with { ResourceChanged = Seq(changed) }))
        select FileReport.Of(phase: FilePhase.NamedLayerState, receipt: Some(receipt));

    private static Fin<FileReport> SheetEditCore(FileRuntime runtime, FileSheetEdit edit) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(FileExchange.SheetEdit)))
        from active in Optional(edit).ToFin(Fail: Op.Of(name: nameof(FileExchange.SheetEdit)).InvalidInput())
        from receipt in live.Edit.Commit(
            name: nameof(FileExchange.SheetEdit),
            redraw: DocumentRedraw.After,
            undoRecorded: true,
            run: (document, _, op) => SheetEditOps.Apply(document: document, edit: active, op: op))
        select FileReport.Of(phase: FilePhase.SheetEdit, receipt: Some(receipt));

    private static Fin<T> HeadlessScope<T>(HeadlessExchange scope, Eff<FileRuntime, T> body) =>
        Op.Of(name: nameof(Headless)).Catch(() => {
            RhinoDoc? headless = null;
            try {
                // BOUNDARY ADAPTER — RhinoDoc is disposable resource owning native handle
                Fin<RhinoDoc> opened = scope.Switch(
                    create: static _ =>
                        Optional(RhinoDoc.CreateHeadless(file3dmTemplatePath: null)).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidResult()),
                    createFromTemplate: static create =>
                        from template in create.Template.Input(op: Op.Of(name: nameof(Headless)))
                        from document in Optional(RhinoDoc.CreateHeadless(file3dmTemplatePath: template.Path)).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidResult())
                        select document,
                    open: static open =>
                        from source in open.Source.Input(op: Op.Of(name: nameof(Headless)))
                        from options in FileFormatProjection.Dictionary(endpoint: source, profile: open.Profile, phase: FilePhase.Headless, op: Op.Of(name: nameof(Headless)))
                            .BindFail(error => source.Format.Case switch {
                                FileFormat => Fin.Fail<ArchivableDictionary>(error: error),
                                _ => Fin.Succ(value: new ArchivableDictionary()),
                            })
                        from document in Optional(RhinoDoc.OpenHeadless(filePath: source.Path, options: options)).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidResult())
                        select document);
                return opened.Bind(document => {
                    headless = document;
                    return Context.Of(doc: document).ToFin().Bind(domain => body.Run(new FileRuntime(
                        document: Some(document),
                        mode: RunMode.Scripted,
                        domain: Some(domain),
                        edit: Some(new DocumentEdit(document: document, domain: domain)),
                        ui: Some(new RhinoUi(document: document, mode: RunMode.Scripted)))));
                });
            } finally {
                headless?.Dispose();
            }
        });

    private static Fin<DocumentReceipt> ExportTarget(RhinoDoc document, DocumentTarget target, FileEndpoint endpoint, FileProfile profile, Op op) =>
        // BOUNDARY ADAPTER — export selected mutates Rhino selection temporarily and must restore it.
        op.Catch(() => DocumentEdit.SelectedIds(document: document, op: op).Bind(before => {
            try {
                return from ids in target.Ids(document: document, op: op)
                       from _ in op.Confirm(success: document.Objects.UnselectAll(ignorePersistentSelections: false) >= 0)
                       from __ in document.Objects.SetSelectedObjects(objectIds: ids.AsIterable(), syncHighlight: true, persistentSelect: false, ignoreGripsState: true, ignoreLayerLocking: false, ignoreLayerVisibility: false) switch {
                           int count when count == ids.Count => Fin.Succ(value: unit),
                           _ => Fin.Fail<Unit>(error: op.InvalidResult()),
                       }
                       from ___ in DocumentEdit.SelectedIds(document: document, op: op).Bind(active => active.Order().AsIterable().SequenceEqual(second: ids.Order().AsIterable()) ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: op.InvalidResult()))
                       from ____ in FileFormatProjection.Write(document: document, target: Some(endpoint), profile: profile, phase: FilePhase.Export, selected: true, op: op)
                       select DocumentReceipt.Empty with { Selected = ids };
            } finally {
                _ = document.Objects.UnselectAll(ignorePersistentSelections: false);
                _ = before.IsEmpty
                    ? 0
                    : document.Objects.SetSelectedObjects(objectIds: before.AsIterable(), syncHighlight: true, persistentSelect: true, ignoreGripsState: true, ignoreLayerLocking: false, ignoreLayerVisibility: false);
            }
        }));

    private static Fin<(RhinoDoc Document, Context Domain, DocumentEdit Edit)> Live(FileRuntime runtime, Op op) =>
        (runtime.Document.Case, runtime.Domain.Case, runtime.Edit.Case) switch {
            (RhinoDoc document, Context domain, DocumentEdit edit) => Fin.Succ(value: (document, domain, edit)),
            _ => Fin.Fail<(RhinoDoc Document, Context Domain, DocumentEdit Edit)>(error: op.MissingContext()),
        };
}

// --- [COMPOSITION] ------------------------------------------------------------------------
internal static class SheetEditOps {
    internal static Fin<DocumentReceipt> Apply(RhinoDoc document, FileSheetEdit edit, Op op) =>
        edit.Switch(
            (Document: document, Op: op),
            create: static (ctx, edit) => CreateSheet(document: ctx.Document, spec: edit.Spec, op: ctx.Op),
            remove: static (ctx, edit) => RemoveSheet(document: ctx.Document, sheetName: edit.SheetName, op: ctx.Op),
            duplicate: static (ctx, edit) => DuplicateSheet(document: ctx.Document, sheetName: edit.SheetName, withGeometry: edit.WithGeometry, op: ctx.Op),
            rename: static (ctx, edit) => RenameSheet(document: ctx.Document, sheetName: edit.SheetName, newName: edit.NewName, op: ctx.Op),
            reorder: static (ctx, edit) => ReorderSheets(document: ctx.Document, sheetNames: edit.SheetNames, op: ctx.Op),
            addDetail: static (ctx, edit) => AddDetail(document: ctx.Document, sheetName: edit.SheetName, spec: edit.Spec, op: ctx.Op),
            removeDetail: static (ctx, edit) => RemoveDetail(document: ctx.Document, sheetName: edit.SheetName, detailName: edit.DetailName, op: ctx.Op),
            activateDetail: static (ctx, edit) => ActivateDetail(document: ctx.Document, sheetName: edit.SheetName, detailName: edit.DetailName, op: ctx.Op),
            layerOverride: static (ctx, edit) => ApplyLayerOverride(document: ctx.Document, sheetName: edit.SheetName, detailName: edit.DetailName, layerPath: edit.LayerPath, color: edit.Color, visible: edit.Visible, op: ctx.Op),
            clippingOverride: static (ctx, edit) => ApplyClippingOverride(document: ctx.Document, sheetName: edit.SheetName, detailName: edit.DetailName, box: edit.Box, op: ctx.Op),
            refreshLinks: static (ctx, edit) => RefreshLinks(document: ctx.Document, archives: edit.Archives, skipUpToDate: edit.SkipUpToDate, op: ctx.Op));

    private static Fin<DocumentReceipt> CreateSheet(RhinoDoc document, FileSheetSpec spec, Op op) =>
        from name in FileEndpoint.NonBlank(value: spec.Name, op: op)
        from unique in guard(!toSeq(document.Views.GetPageViews()).Exists(view => string.Equals(a: view.PageName, b: name, comparisonType: StringComparison.OrdinalIgnoreCase)), op.InvalidInput())
        from page in op.Catch(() => {
            RhinoPageView? created = (spec.WidthMillimeters.Case, spec.HeightMillimeters.Case) switch {
                (double width, double height) => document.Views.AddPageView(title: name, pageWidth: width, pageHeight: height),
                _ => document.Views.AddPageView(title: name),
            };
            return Optional(created).ToFin(Fail: op.InvalidResult());
        })
        from grouped in op.Catch(() => {
            _ = spec.Group.Case switch {
                string groupName => document.PageViewGroups.FindName(name: groupName) switch {
                    PageViewGroup => 0,
                    _ => document.PageViewGroups.Add(groupMembers: Seq(page).AsIterable()),
                },
                _ => 0,
            };
            return Fin.Succ(value: unit);
        })
        select DocumentReceipt.Empty with {
            Created = Seq(page.MainViewport.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
        };

    private static Fin<DocumentReceipt> RemoveSheet(RhinoDoc document, string sheetName, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        let pageId = page.MainViewport.Id
        from _ in op.Confirm(success: page.Close())
        select DocumentReceipt.Empty with {
            Deleted = Seq(pageId),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: sheetName)),
        };

    private static Fin<DocumentReceipt> DuplicateSheet(RhinoDoc document, string sheetName, bool withGeometry, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from copy in Optional(page.Duplicate(duplicatePageGeometry: withGeometry)).ToFin(Fail: op.InvalidResult())
        select DocumentReceipt.Empty with {
            Created = Seq(copy.MainViewport.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: copy.PageName)),
        };

    private static Fin<DocumentReceipt> RenameSheet(RhinoDoc document, string sheetName, string newName, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: newName, op: op)
        from _ in op.Catch(() => { page.PageName = name; return Fin.Succ(value: unit); })
        select DocumentReceipt.Empty with {
            AttributeChanged = Seq(page.MainViewport.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
        };

    // BOUNDARY ADAPTER — RhinoCommon exposes no ViewTable.Reorder/MoveTo; rebinding page numbers
    // is the only public surface that preserves identity. PageNumber is a writable property.
    private static Fin<DocumentReceipt> ReorderSheets(RhinoDoc document, Seq<string> sheetNames, Op op) =>
        from names in sheetNames.TraverseM(name => FileEndpoint.NonBlank(value: name, op: op)).As()
        from pages in names.TraverseM(name => Sheet(document: document, name: name, op: op)).As()
        from _ in op.Catch(() => {
            _ = pages.AsIterable().Select((page, index) => (Page: page, Index: index)).Iter(static item => item.Page.PageNumber = item.Index);
            document.Views.Redraw();
            return Fin.Succ(value: unit);
        })
        select DocumentReceipt.Empty with {
            AttributeChanged = pages.Map(static page => page.MainViewport.Id),
            ResourceChanged = pages.Map(static page => new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: page.PageName)),
        };

    private static Fin<DocumentReceipt> AddDetail(RhinoDoc document, string sheetName, FileDetailSpec spec, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: spec.Name, op: op)
        from detail in Optional(page.AddDetailView(title: name, corner0: spec.Corner, corner1: spec.Opposite, initialProjection: spec.Projection)).ToFin(Fail: op.InvalidResult())
        from _ in op.Catch(() => {
            _ = detail.DetailGeometry switch {
                DetailViewGeometry geometry => Op.Side(() => geometry.IsProjectionLocked = spec.ProjectionLocked),
                _ => unit,
            };
            _ = spec.DisplayMode.Iter(id => Optional(DisplayModeDescription.GetDisplayMode(id: id)).Iter(mode => detail.Viewport.DisplayMode = mode));
            _ = detail.CommitViewportChanges();
            return Fin.Succ(value: unit);
        })
        select DocumentReceipt.Empty with {
            Created = Seq(detail.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
        };

    private static Fin<DocumentReceipt> RemoveDetail(RhinoDoc document, string sheetName, string detailName, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: detailName, op: op)
        from detail in Detail(page: page, name: name, op: op)
        let detailId = detail.Id
        from _ in op.Confirm(success: document.Objects.Delete(obj: detail, quiet: true))
        select DocumentReceipt.Empty with {
            Deleted = Seq(detailId),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
        };

    private static Fin<DocumentReceipt> ActivateDetail(RhinoDoc document, string sheetName, Option<string> detailName, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from target in detailName.Case switch {
            string name => Detail(page: page, name: name, op: op).Bind(detail =>
                op.Catch(() => { detail.IsActive = true; return Fin.Succ(value: Some(detail.Id)); })),
            _ => op.Catch(() => { page.SetPageAsActive(); return Fin.Succ(value: Option<Guid>.None); }),
        }
        select DocumentReceipt.Empty with {
            AttributeChanged = target.Map(Seq).IfNone(Seq(page.MainViewport.Id)),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: detailName.IfNone(sheetName))),
        };

    private static Fin<DocumentReceipt> ApplyLayerOverride(RhinoDoc document, string sheetName, string detailName, string layerPath, Option<System.Drawing.Color> color, Option<bool> visible, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: detailName, op: op)
        from path in FileEndpoint.NonBlank(value: layerPath, op: op)
        from detail in Detail(page: page, name: name, op: op)
        from layer in Layer(document: document, path: path, op: op)
        from _ in op.Catch(() => {
            Guid viewportId = detail.Viewport.Id;
            _ = color.Iter(value => layer.SetPerViewportColor(viewportId: viewportId, color: value));
            _ = visible.Iter(value => layer.SetPerViewportVisible(viewportId: viewportId, visible: value));
            _ = detail.CommitViewportChanges();
            return Fin.Succ(value: unit);
        })
        select DocumentReceipt.Empty with {
            AttributeChanged = Seq(detail.Id),
            ResourceChanged = Seq(
                new DocumentResourceChange(Kind: DocumentResourceKind.Layer, Name: path),
                new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
        };

    private static Fin<DocumentReceipt> ApplyClippingOverride(RhinoDoc document, string sheetName, string detailName, BoundingBox box, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: detailName, op: op)
        from valid in box.IsValid ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: op.InvalidInput())
        from detail in Detail(page: page, name: name, op: op)
        from applied in op.Catch(() => {
            detail.Viewport.SetClippingPlanes(box: box);
            _ = detail.CommitViewportChanges();
            return Fin.Succ(value: unit);
        })
        select DocumentReceipt.Empty with {
            AttributeChanged = Seq(detail.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
        };

    // BOUNDARY ADAPTER — InstanceDefinitionTable.RefreshLinkedBlock is synchronous; reload + redraw
    // happens on the caller's thread. `UpdateType` discriminates linked-vs-embedded definitions
    // (SourceArchive alone is unreliable). `SkipUpToDate` consults `ArchiveFileStatus` to avoid
    // reloading definitions whose source has not changed. TraverseM short-circuits on first failed
    // refresh so callers see an InvalidResult error rather than silently dropped definitions.
    private static Fin<DocumentReceipt> RefreshLinks(RhinoDoc document, Option<Seq<string>> archives, bool skipUpToDate, Op op) =>
        op.Catch(() =>
            toSeq(document.InstanceDefinitions)
                .Filter(static definition => definition.UpdateType is InstanceDefinitionUpdateType.Linked or InstanceDefinitionUpdateType.LinkedAndEmbedded)
                .Filter(definition => archives.Case switch {
                    Seq<string> filter => filter.Exists(path => string.Equals(a: path, b: definition.SourceArchive ?? string.Empty, comparisonType: StringComparison.OrdinalIgnoreCase)),
                    _ => true,
                })
                .Filter(definition => !skipUpToDate || definition.ArchiveFileStatus != InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate)
                .TraverseM(definition => document.InstanceDefinitions.RefreshLinkedBlock(definition: definition)
                    ? Fin.Succ(value: definition)
                    : Fin.Fail<InstanceDefinition>(error: op.InvalidResult())).As()
                .Map(refreshed => DocumentReceipt.Empty with {
                    AttributeChanged = refreshed.Map(static definition => definition.Id),
                    ResourceChanged = refreshed.Map(static definition => new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: definition.Name ?? string.Empty)),
                }));

    private static Fin<T> Resolve<T>(IEnumerable<T> source, Func<T, bool> match, Op op) =>
        toSeq(source).Find(match).ToFin(Fail: op.InvalidInput());

    private static Fin<RhinoPageView> Sheet(RhinoDoc document, string name, Op op) =>
        from valid in FileEndpoint.NonBlank(value: name, op: op)
        from page in Resolve(source: document.Views.GetPageViews(), match: view => string.Equals(a: view.PageName, b: valid, comparisonType: StringComparison.OrdinalIgnoreCase), op: op)
        select page;

    private static Fin<DetailViewObject> Detail(RhinoPageView page, string name, Op op) =>
        Resolve(source: page.GetDetailViews(), match: detail => string.Equals(a: detail.Attributes.Name, b: name, comparisonType: StringComparison.OrdinalIgnoreCase), op: op);

    private static Fin<Layer> Layer(RhinoDoc document, string path, Op op) =>
        document.Layers.FindByFullPath(layerPath: path, notFoundReturnValue: -1) switch {
            int index when index >= 0 => Optional(document.Layers[index]).ToFin(Fail: op.InvalidInput()),
            _ => Fin.Fail<Layer>(error: op.InvalidInput()),
        };
}

internal sealed class FileWatchSubscription : IDisposable {
    private readonly Seq<Action> detachers;
    private FileWatchSubscription(Seq<Action> detachers) => this.detachers = detachers;

    internal static Fin<IDisposable> Of(FileWatch watch) =>
        Optional(watch).ToFin(Fail: Op.Of(name: nameof(FileWatchSubscription)).InvalidInput())
            .Map<IDisposable>(active => new FileWatchSubscription(detachers:
                Attach(handler: active.OnBeginOpen, add: static h => RhinoDoc.BeginOpenDocument += h, remove: static h => RhinoDoc.BeginOpenDocument -= h)
                + Attach(handler: active.OnEndOpen, add: static h => RhinoDoc.EndOpenDocument += h, remove: static h => RhinoDoc.EndOpenDocument -= h)
                + Attach(handler: active.OnBeginSave, add: static h => RhinoDoc.BeginSaveDocument += h, remove: static h => RhinoDoc.BeginSaveDocument -= h)
                + Attach(handler: active.OnEndSave, add: static h => RhinoDoc.EndSaveDocument += h, remove: static h => RhinoDoc.EndSaveDocument -= h)
                + Attach(handler: active.OnClose, add: static h => RhinoDoc.CloseDocument += h, remove: static h => RhinoDoc.CloseDocument -= h)
                + Attach(handler: active.OnNew, add: static h => RhinoDoc.NewDocument += h, remove: static h => RhinoDoc.NewDocument -= h)));

    private static Seq<Action> Attach<TArgs>(Option<Action<TArgs>> handler, Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove) where TArgs : EventArgs =>
        handler.Map(action => {
            void Wrapper(object? _, TArgs args) => action(args);
            add(Wrapper);
            return Seq(() => remove(Wrapper));
        }).IfNone(Seq<Action>());

    public void Dispose() => detachers.Iter(detach => detach()).Ignore();
}
