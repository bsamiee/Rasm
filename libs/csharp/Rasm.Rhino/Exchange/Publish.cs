using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Rasm.Rhino.Camera;
using Rasm.Rhino.Commands;
using Rhino.FileIO;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using DrawingPointF = System.Drawing.PointF;
using IOFileInfo = System.IO.FileInfo;

namespace Rasm.Rhino.Exchange;

// --- [CONSTANTS] --------------------------------------------------------------------------
internal static class FileSheetDefaults {
    internal const double DefaultPublishDpi = 300.0;
    internal const long DefaultJpegQuality = 90L;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FilePublish(FilePublishTarget Target, Seq<FileView> Views, FileProfile Profile, bool Layers = true, Option<string> Snapshot = default) {
    public FilePublish WithSnapshot(string name) =>
        this with { Snapshot = string.IsNullOrWhiteSpace(value: name) ? Option<string>.None : Some(name.Trim()) };
}

// Publish source: layout pages (matched), restored named views (onto a model viewport), or live model viewports.
// Named/Viewport resolve through the Camera sibling — restore reframes the viewport, then capture builds off it.
[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record FileViewSource {
    private FileViewSource() { }
    public sealed record Pages(SheetQuery Query = default) : FileViewSource;
    public sealed record Named(
        Seq<string> Names,
        ViewportTarget Target,
        Option<CameraPath> Path = default,
        NamedRestorePolicy Restore = default) : FileViewSource;
    public sealed record Viewport(ViewportTarget Target) : FileViewSource;

    internal Fin<Seq<FileViewPage>> Resolve(RhinoDoc document, FileView spec, Op op) =>
        Switch(
            (Doc: document, Spec: spec, Op: op),
            pages: static (ctx, source) => ResolvePages(document: ctx.Doc, source: source, spec: ctx.Spec, op: ctx.Op),
            named: static (ctx, source) => ResolveNamed(document: ctx.Doc, source: source, spec: ctx.Spec, op: ctx.Op),
            viewport: static (ctx, source) => source.Target.ResolveMany(document: ctx.Doc, op: ctx.Op)
                .Map(scopes => scopes.Map(scope => FileViewPage.Model(view: scope.View, spec: ctx.Spec, prepare: static _ => Fin.Succ(value: unit)))));

    private static Fin<Seq<FileViewPage>> ResolvePages(RhinoDoc document, Pages source, FileView spec, Op op) =>
        from pages in source.Query.Resolve(document: document, op: op)
        from matched in pages.IsEmpty ? Fin.Fail<Seq<RhinoPageView>>(error: op.InvalidInput()) : Fin.Succ(value: pages)
        select matched.Map(page => FileViewPage.Layout(page: page, spec: spec));

    // Restore runs on the pre-resolved scope directly (this IS RhinoCamera.ExecuteRunOnScope's body): the
    // closure already executes inside `target.Write` under `RhinoUi.Protect`, so wrapping in RhinoCamera.Run
    // would re-resolve the scope and nest a second UI dispatch for nothing.
    private static Fin<Seq<FileViewPage>> ResolveNamed(RhinoDoc document, Named source, FileView spec, Op op) =>
        from _ in guard(!source.Names.IsEmpty, op.InvalidInput())
        from scope in source.Target.Resolve(document: document, op: op)
        from pages in source.Names.TraverseM(name => FileEndpoint.NonBlank(value: name, op: op).Map(valid =>
            FileViewPage.Model(view: scope.View, spec: spec,
                prepare: key => (source.Path.Case switch {
                    CameraPath restore => restore.RequireSynchronous(op: key).Map<CameraPath?>(static value => value),
                    _ => Fin.Succ<CameraPath?>(value: null),
                }).Bind(path => CameraOps.RestoreNamed(
                        name: valid,
                        path: path,
                        restore: source.Restore)
                    .Run(arg: scope)
                    .Bind(outcome => outcome.Redraw.ApplyTo(scope: scope)))))).As()
        select pages;
}

internal sealed record FileViewPage(RhinoView Target, FileView Spec, Func<Op, Fin<Unit>> Prepare) {
    internal static FileViewPage Layout(RhinoPageView page, FileView spec) =>
        new(Target: page, Spec: spec, Prepare: static _ => Fin.Succ(value: unit));

    internal static FileViewPage Model(RhinoView view, FileView spec, Func<Op, Fin<Unit>> prepare) =>
        new(Target: view, Spec: spec, Prepare: prepare);

    // Prepare runs FIRST (named-view restore reframes the viewport), then settings build off the framed view.
    internal Fin<ViewCaptureSettings> Settings(Op op) =>
        from _ in Prepare(arg: op)
        from settings in Spec.Open(view: Target, op: op)
        select settings;

    // Single owner of "prepare view + capture rail + report"; CaptureRecipe owns settings construction/disposal.
    // The per-page report is built while the settings are still alive (model scale is only resolvable then),
    // so the realized scale is reported instead of being lost after disposal. Printer collects settings itself
    // (SendToPrinter needs them all alive at once) and still uses Settings/Open.
    internal Fin<(T Value, FileViewReport Report)> Render<T>(Func<ViewCaptureSettings, Fin<T>> render, Op op) =>
        from _ in Prepare(arg: op)
        from result in Spec.Render(view: Target, project: settings => render(arg: settings).Map(value => (Value: value, Report: ReportOf(settings: settings))), op: op)
        select result;

    internal FileViewReport ReportOf(ViewCaptureSettings settings) =>
        Target switch {
            RhinoPageView page => PageReportOf(page: page),
            RhinoView view => new FileViewReport(Name: view.MainViewport.Name ?? string.Empty, Scale: ScaleOf(settings: settings, view: view), DetailCount: 0),
            _ => new FileViewReport(Name: string.Empty, Scale: Option<string>.None, DetailCount: 0),
        };

    // Reports every detail's formatted scale (de-duplicated) via FileScale.Format, not just the first detail's.
    private static FileViewReport PageReportOf(RhinoPageView page) {
        DetailViewObject[] details = page.GetDetailViews() ?? [];
        Seq<string> scales = toSeq(details).Choose(FileScale.Format).Distinct();
        return new FileViewReport(Name: page.PageName, Scale: scales.IsEmpty ? Option<string>.None : Some(string.Join(separator: ", ", values: scales.AsIterable())), DetailCount: details.Length);
    }

    // Model captures realize their scale on the live settings: `GetModelScale` is the page/model unit ratio,
    // `IsScaleToFit` overrides it. Layout pages report the detail's formatted scale (PageReportOf) instead.
    private static Option<string> ScaleOf(ViewCaptureSettings settings, RhinoView view) =>
        (settings.IsScaleToFit, settings.GetModelScale(pageUnits: view.Document.PageUnitSystem, modelUnits: view.Document.ModelUnitSystem)) switch {
            (true, _) => Some("fit"),
            (false, double scale) when RhinoMath.IsValidDouble(x: scale) && scale > 0.0 => Some(string.Create(CultureInfo.InvariantCulture, $"{scale:0.######}")),
            _ => Option<string>.None,
        };
}

public readonly record struct FileViewReport(string Name, Option<string> Scale, int DetailCount);

public sealed partial record FileView(
    FileViewSource Source,
    CaptureRecipe Recipe = default) {
    internal Fin<ViewCaptureSettings> Open(RhinoView view, Op op) =>
        from active in Optional(view).ToFin(Fail: op.InvalidInput())
        from settings in Recipe.Open(
            view: active,
            viewport: Recipe.Viewport(view: active),
            fallbackDpi: FileSheetDefaults.DefaultPublishDpi,
            fallbackDecor: CaptureDecor.Publish,
            rewrite: RewriteDecor,
            op: op)
        select settings;

    internal Fin<T> Render<T>(RhinoView view, Func<ViewCaptureSettings, Fin<T>> project, Op op) =>
        from active in Optional(view).ToFin(Fail: op.InvalidInput())
        from result in Recipe.Render(
            view: active,
            viewport: Recipe.Viewport(view: active),
            fallbackDpi: FileSheetDefaults.DefaultPublishDpi,
            fallbackDecor: CaptureDecor.Publish,
            rewrite: RewriteDecor,
            project: project,
            op: op)
        select result;

    internal Fin<DrawingBitmap> TransparentBitmap(RhinoView view, ViewCaptureSettings settings, Op op) =>
        Recipe.TransparentBitmap(
            view: view,
            settings: settings,
            fallbackDecor: CaptureDecor.Publish with { UsePrintWidths = false },
            rewrite: RewriteDecor,
            op: op);

    private static CaptureDecor RewriteDecor(CaptureDecor decor, RhinoView target) =>
        decor with {
            HeaderText = decor.HeaderText.Map(text => Interpolate(template: text, document: target.Document, view: target)),
            FooterText = decor.FooterText.Map(text => Interpolate(template: text, document: target.Document, view: target)),
        };

    private static string Interpolate(string template, RhinoDoc document, RhinoView view) {
        int total = document.Views.GetPageViews()?.Length ?? 0;
        string name = (view switch { RhinoPageView page => page.PageName, _ => view.MainViewport.Name }) ?? string.Empty;
        int index = view switch { RhinoPageView page => page.PageNumber + 1, _ => 0 };
        return string.IsNullOrEmpty(value: template)
            ? template
            : TokenPattern().Replace(input: template, evaluator: match => match.Groups["key"].Value switch {
                "page" => name,
                "index" => index.ToString(provider: CultureInfo.InvariantCulture),
                "total" => total.ToString(provider: CultureInfo.InvariantCulture),
                string key => document.Strings.GetValue(key: key) ?? match.Value,
            });
    }

    [GeneratedRegex(pattern: "\\{(?<key>[^{}]+)\\}", options: RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 250)]
    private static partial Regex TokenPattern();
}

[SmartEnum<int>]
public sealed partial class FileRasterEncoding {
    public static readonly FileRasterEncoding Png = new(
        key: 0,
        format: () => FileFormat.Png,
        image: () => DrawingImageFormat.Png,
        compression: FileTiffCompression.Default,
        encode: static (_, settings) => settings.PngDepth.Map(depth => Seq(new FileRasterCodecParameter(Kind: FileRasterCodecKind.ColorDepth, Value: depth))).IfNone(Seq<FileRasterCodecParameter>()));
    public static readonly FileRasterEncoding Jpeg = new(
        key: 1,
        format: () => FileFormat.Jpeg,
        image: () => DrawingImageFormat.Jpeg,
        compression: FileTiffCompression.Default,
        encode: static (_, settings) => Seq(new FileRasterCodecParameter(Kind: FileRasterCodecKind.Quality, Value: settings.JpegQuality)));
    public static readonly FileRasterEncoding Tiff = new(
        key: 2,
        format: () => FileFormat.Tiff,
        image: () => DrawingImageFormat.Tiff,
        compression: FileTiffCompression.Lzw,
        encode: static (encoding, settings) => Seq(new FileRasterCodecParameter(Kind: FileRasterCodecKind.Compression, Value: (long)settings.TiffCompression.IfNone(noneValue: encoding.Compression))));
    public static readonly FileRasterEncoding Bitmap = new(
        key: 3,
        format: () => FileFormat.Bmp,
        image: () => DrawingImageFormat.Bmp,
        compression: FileTiffCompression.Default,
        encode: static (_, _) => Seq<FileRasterCodecParameter>());

    private readonly Func<FileFormat> format;
    private readonly Func<DrawingImageFormat> image;
    private readonly Func<FileRasterEncoding, FileRasterSettings, Seq<FileRasterCodecParameter>> encode;

    public FileFormat Format => format();
    public DrawingImageFormat Image => image();
    public FileTiffCompression Compression { get; }
    internal bool SupportsAlpha => this == Png || this == Tiff;

    internal Seq<FileRasterCodecParameter> Parameters(FileRasterSettings settings) => encode(arg1: this, arg2: settings);
}

internal enum FileRasterCodecKind { Quality, ColorDepth, Compression }

[StructLayout(LayoutKind.Auto)]
internal readonly record struct FileRasterCodecParameter(FileRasterCodecKind Kind, long Value);

public readonly record struct FileRasterSettings(
    long JpegQuality = FileSheetDefaults.DefaultJpegQuality,
    Option<FileTiffCompression> TiffCompression = default,
    Option<int> PngDepth = default,
    Option<double> ExifDpi = default,
    bool Transparent = false) {
    internal Fin<FileRasterSettings> Validate(FileRasterEncoding encoding, Op op) {
        FileRasterSettings self = this;
        return from _quality in encoding == FileRasterEncoding.Jpeg && self.JpegQuality is < 0L or > 100L ? Fin.Fail<Unit>(error: op.InvalidInput()) : Fin.Succ(value: unit)
               from _depth in (encoding, self.PngDepth.Case) switch {
                   (FileRasterEncoding value, int depth) when value == FileRasterEncoding.Png && depth > 0 => Fin.Succ(value: unit),
                   (FileRasterEncoding value, int) when value == FileRasterEncoding.Png => Fin.Fail<Unit>(error: op.InvalidInput()),
                   (_, int) => Fin.Succ(value: unit),
                   _ => Fin.Succ(value: unit),
               }
               from _dpi in self.ExifDpi.Case switch {
                   double dpi when RhinoMath.IsValidDouble(x: dpi) && dpi > 0d => Fin.Succ(value: unit),
                   double => Fin.Fail<Unit>(error: op.InvalidInput()),
                   _ => Fin.Succ(value: unit),
               }
               from _compression in self.TiffCompression.Case switch {
                   FileTiffCompression value when encoding != FileRasterEncoding.Tiff || SupportedTiffCompression(value: value) => Fin.Succ(value: unit),
                   FileTiffCompression => Fin.Fail<Unit>(error: op.InvalidInput()),
                   _ => Fin.Succ(value: unit),
               }
               from _alpha in self.Transparent && !encoding.SupportsAlpha
                   ? Fin.Fail<Unit>(error: op.InvalidInput())
                   : Fin.Succ(value: unit)
               select self;
    }

    private static bool SupportedTiffCompression(FileTiffCompression value) =>
        value is FileTiffCompression.Default or FileTiffCompression.None or FileTiffCompression.Lzw or FileTiffCompression.Ccitt3 or FileTiffCompression.Ccitt4 or FileTiffCompression.Rle;
}

public readonly record struct FilePdfPage(
    int WidthDots,
    int HeightDots,
    int Dpi,
    Option<Func<FilePdf, int, Op, Fin<Unit>>> Annotate = default) {
    internal Fin<FilePdfPage> Validate(Op op) =>
        WidthDots > 0 && HeightDots > 0 && Dpi > 0
            ? Fin.Succ(value: this)
            : Fin.Fail<FilePdfPage>(error: op.InvalidInput());
}

// Typed page-stamp algebra over the raw FilePdf draw primitives. Callers declare a `Seq<PdfStamp>` and call
// `.Annotation()` to build the `Func<FilePdf,int,Op,Fin<Unit>>` the Pdf/FilePdfPage `Annotate` field expects —
// replacing hand-rolled Font/Color/PointF plumbing per title block, page number, scale bar, or logo.
[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record PdfStamp {
    private PdfStamp() { }
    public sealed record Text(string Value, double X, double Y, float HeightPoints, Font Font, DrawingColor Fill,
        Option<(DrawingColor Color, float Width)> Stroke = default, float AngleDegrees = 0f,
        TextHorizontalAlignment Horizontal = TextHorizontalAlignment.Left, TextVerticalAlignment Vertical = TextVerticalAlignment.Top) : PdfStamp;
    public sealed record Polyline(Seq<DrawingPointF> Points, Option<DrawingColor> Fill, DrawingColor Stroke, float Width) : PdfStamp;
    public sealed record Line(DrawingPointF From, DrawingPointF To, DrawingColor Stroke, float Width) : PdfStamp;
    public sealed record Image(DrawingBitmap Bitmap, float X, float Y, float Width, float Height, float AngleDegrees = 0f) : PdfStamp;

    internal Fin<Unit> Draw(FilePdf pdf, int page, Op op) =>
        Switch((Pdf: pdf, Page: page, Op: op),
            text: static (ctx, s) => ctx.Op.Catch(() => Fin.Succ(value: Op.Side(() => ctx.Pdf.DrawText(
                pageNumber: ctx.Page, text: s.Value, x: s.X, y: s.Y, heightPoints: s.HeightPoints, onfont: s.Font, fillColor: s.Fill,
                strokeColor: s.Stroke.Case switch { (DrawingColor c, _) => c, _ => DrawingColor.Empty },
                strokeWidth: s.Stroke.Case switch { (_, float w) => w, _ => 0f },
                angleDegrees: s.AngleDegrees, horizontalAlignment: s.Horizontal, verticalAlignment: s.Vertical)))),
            polyline: static (ctx, s) => ctx.Op.Catch(() => Fin.Succ(value: Op.Side(() => ctx.Pdf.DrawPolyline(
                pageNumber: ctx.Page, polyline: [.. s.Points], fillColor: s.Fill.IfNone(DrawingColor.Empty), strokeColor: s.Stroke, strokeWidth: s.Width)))),
            line: static (ctx, s) => ctx.Op.Catch(() => Fin.Succ(value: Op.Side(() => ctx.Pdf.DrawLine(
                pageNumber: ctx.Page, from: s.From, to: s.To, strokeColor: s.Stroke, strokeWidth: s.Width)))),
            image: static (ctx, s) => ctx.Op.Catch(() => Fin.Succ(value: Op.Side(() => ctx.Pdf.DrawBitmap(
                pageNumber: ctx.Page, bitmap: s.Bitmap, left: s.X, top: s.Y, width: s.Width, height: s.Height, rotationInDegrees: s.AngleDegrees)))));
}

public static class PdfStamps {
    public static Func<FilePdf, int, Op, Fin<Unit>> Annotation(this Seq<PdfStamp> stamps) =>
        (pdf, page, op) => stamps.TraverseM(stamp => stamp.Draw(pdf: pdf, page: page, op: op)).As().Map(static _ => unit);
}

internal readonly record struct FilePublishResult(
    Option<FileEndpoint> Target,
    Option<FileFormat> Format,
    DocumentReceipt Receipt,
    Seq<FileViewReport> Views = default,
    Seq<FileIssue> Issues = default,
    Option<string> NativeLog = default);

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record FilePublishTarget {
    private FilePublishTarget() { }
    // `Prefix`/`Suffix` interleave blank pages via `AddPage(w,h,dpi)`; captured views use `AddPage(ViewCaptureSettings)`.
    // Per-page `Annotate` stamps cover/title pages; top-level `Annotate` applies to prefix, captured, and suffix pages.
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

    internal Fin<FilePublishResult> Write(Seq<FileViewPage> pages, bool layers, Op op) =>
        Switch(
            (Pages: pages, Layers: layers, Op: op),
            pdf: static (ctx, target) => WritePdf(target: target, pages: ctx.Pages, layers: ctx.Layers, op: ctx.Op),
            printer: static (ctx, target) => WritePrinter(target: target, pages: ctx.Pages, op: ctx.Op),
            raster: static (ctx, target) => WriteRaster(target: target, pages: ctx.Pages, op: ctx.Op),
            svg: static (ctx, target) => WriteSvg(target: target, pages: ctx.Pages, op: ctx.Op));

    // `LayersAsOptionalContentGroups` is toggled per page inside the render closure to mirror the native
    // SendToPdf, which reads the flag per AddPage — a single hoist would force OCG ON for raster pages in a
    // mixed document (raster captures have no layer structure to group).
    private static Fin<FilePublishResult> WritePdf(Pdf target, Seq<FileViewPage> pages, bool layers, Op op) =>
        from endpoint in target.Target.WithFormat(format: FileFormat.Pdf).Output(op: op)
        from pdf in Optional(FilePdf.Create()).ToFin(Fail: op.InvalidResult())
        from _prefix in target.Prefix.TraverseM(spec => AddBlankPdfPage(pdf: pdf, spec: spec, op: op)
            .Bind(page => op.Catch(() => target.Annotate.Map(annotate => annotate(arg1: pdf, arg2: page, arg3: op)).IfNone(Fin.Succ(value: unit))))).As()
        from sheets in pages.TraverseM(page =>
            from rendered in page.Render(render: owned => {
                pdf.LayersAsOptionalContentGroups = layers && !page.Spec.Recipe.Raster;
                return pdf.AddPage(settings: owned) switch {
                    int value when value >= 0 => Fin.Succ(value: value),
                    _ => Fin.Fail<int>(error: op.InvalidResult()),
                };
            }, op: op)
            from annotated in op.Catch(() => target.Annotate.Map(annotate => annotate(arg1: pdf, arg2: rendered.Value, arg3: op)).IfNone(Fin.Succ(value: unit)))
            select rendered.Report).As()
        from _suffix in target.Suffix.TraverseM(spec => AddBlankPdfPage(pdf: pdf, spec: spec, op: op)
            .Bind(page => op.Catch(() => target.Annotate.Map(annotate => annotate(arg1: pdf, arg2: page, arg3: op)).IfNone(Fin.Succ(value: unit))))).As()
        from _write in op.Catch(() => { pdf.Write(filename: endpoint.Path); return Fin.Succ(value: unit); })
        from _verify in VerifyFile(path: endpoint.Path, op: op)
        select new FilePublishResult(
            Target: Some(endpoint),
            Format: Some(FileFormat.Pdf),
            Receipt: DocumentReceipt.Empty,
            Views: sheets);

    private static Fin<int> AddBlankPdfPage(FilePdf pdf, FilePdfPage spec, Op op) =>
        from valid in spec.Validate(op: op)
        from index in op.Catch(() => pdf.AddPage(widthInDots: valid.WidthDots, heightInDots: valid.HeightDots, dotsPerInch: valid.Dpi) switch {
            int value when value >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.InvalidResult()),
        })
        from annotated in op.Catch(() => valid.Annotate.Map(annotate => annotate(arg1: pdf, arg2: index, arg3: op)).IfNone(Fin.Succ(value: unit)))
        select index;

    private static Fin<FilePublishResult> WritePrinter(Printer target, Seq<FileViewPage> pages, Op op) =>
        from name in FileEndpoint.NonBlank(value: target.Name, op: op)
        from copies in target.Copies > 0 ? Fin.Succ(value: target.Copies) : Fin.Fail<int>(error: op.InvalidInput())
        from prepared in pages.TraverseM(page => page.Settings(op: op).Map(settings => (Settings: settings, Report: page.ReportOf(settings: settings)))).As()
        from result in op.Catch(() => {
            // BOUNDARY ADAPTER — SendToPrinter consumes every settings at once; collect, send, then dispose all.
            ViewCaptureSettings[] captures = [.. prepared.Map(static item => item.Settings)];
            try {
                return op.Confirm(success: ViewCapture.SendToPrinter(printerName: name, settings: captures, copies: copies))
                    .Map(_ => new FilePublishResult(
                        Target: Option<FileEndpoint>.None,
                        Format: Option<FileFormat>.None,
                        Receipt: DocumentReceipt.Empty,
                        Views: prepared.Map(static item => item.Report),
                        NativeLog: Some(string.Create(CultureInfo.InvariantCulture, $"printer:{name};copies:{copies};views:{pages.Count}"))));
            } finally {
                _ = toSeq(captures).Iter(static settings => settings.Dispose());
            }
        })
        select result;

    private static Fin<FilePublishResult> WriteRaster(Raster target, Seq<FileViewPage> pages, Op op) {
        FileRasterEncoding encoding = target.ResolvedEncoding;
        return from settings in target.Settings.Validate(encoding: encoding, op: op)
               from result in CaptureViews(target: target.Target, format: encoding.Format, pages: pages, op: op,
            render: (page, owned, path) => {
                // Transparent output requires the instance ViewCapture path; the static ViewCaptureSettings
                // path ignores alpha. It trades sheet layout/crop/scale for the alpha channel.
                DrawingBitmap? bitmap = null;
                try {
                    return (settings.Transparent
                        ? page.Spec.TransparentBitmap(view: page.Target, settings: owned, op: op)
                        : Optional(ViewCapture.CaptureToBitmap(settings: owned)).ToFin(Fail: op.InvalidResult()))
                        .Map(active => {
                            bitmap = active;
                            _ = settings.ExifDpi.Iter(dpi => active.SetResolution(xDpi: (float)dpi, yDpi: (float)dpi));
                            return active;
                        })
                        .Bind(active => SaveBitmap(bitmap: active, encoding: encoding, settings: settings, path: path, op: op));
                } finally {
                    bitmap?.Dispose();
                }
            },
            log: string.Create(CultureInfo.InvariantCulture, $"raster:{encoding.Key};quality:{settings.JpegQuality};views:{pages.Count}"))
               select result;
    }

    private static Fin<FilePublishResult> WriteSvg(Svg target, Seq<FileViewPage> pages, Op op) =>
        CaptureViews(target: target.Target, format: FileFormat.Svg, pages: pages, op: op,
            render: (_, owned, path) => Optional(ViewCapture.CaptureToSvg(settings: owned))
                .ToFin(Fail: op.InvalidResult())
                .Bind(document => op.Catch(() => {
                    document.Save(filename: path);
                    return VerifyFile(path: path, op: op);
                })),
            log: string.Create(CultureInfo.InvariantCulture, $"svg;views:{pages.Count}"));

    private static Fin<FilePublishResult> CaptureViews(FileEndpoint target, FileFormat format, Seq<FileViewPage> pages, Op op, Func<FileViewPage, ViewCaptureSettings, string, Fin<Unit>> render, string log) =>
        from endpoint in target.WithFormat(format: format).Output(op: op)
        from written in pages.Map((page, index) => (Index: index, Page: page))
            .TraverseM(item => item.Page.Render(
                render: owned => render(arg1: item.Page, arg2: owned, arg3: pages.Count == 1 ? endpoint.Path : FileEndpoint.NumberedPath(path: endpoint.Path, index: item.Index + 1)),
                op: op))
            .As()
        select new FilePublishResult(
            Target: Some(endpoint),
            Format: Some(format),
            Receipt: DocumentReceipt.Empty,
            Views: written.Map(static item => item.Report),
            NativeLog: Some(log));

    private static Fin<Unit> VerifyFile(string path, Op op) =>
        Optional(new IOFileInfo(fileName: path)).Filter(static info => info.Exists && info.Length > 0).Map(static _ => unit).ToFin(Fail: op.InvalidResult());

    private static Fin<Unit> SaveBitmap(DrawingBitmap bitmap, FileRasterEncoding encoding, FileRasterSettings settings, string path, Op op) =>
        op.Catch(() => {
            DrawingImageFormat image = encoding.Image;
            Seq<FileRasterCodecParameter> codecParams = encoding.Parameters(settings: settings);
            return (codecParams.IsEmpty
                    ? Fin.Succ(value: Op.Side(() => bitmap.Save(filename: path, format: image)))
                    : SaveBitmapWithCodec(bitmap: bitmap, image: image, parameters: codecParams, path: path, op: op))
                .Bind(_ => VerifyFile(path: path, op: op));
        });

    private static Fin<Unit> SaveBitmapWithCodec(DrawingBitmap bitmap, DrawingImageFormat image, Seq<FileRasterCodecParameter> parameters, string path, Op op) {
        ImageCodecInfo? codec = toSeq(ImageCodecInfo.GetImageEncoders()).Find(c => c.FormatID == image.Guid).Case as ImageCodecInfo;
        return codec switch {
            ImageCodecInfo active => Fin.Succ(value: Op.Side(() => {
                EncoderParameter[] entries = [.. parameters.AsIterable().Select(CodecParameter)];
                try {
                    using EncoderParameters native = new(entries.Length) { Param = entries };
                    bitmap.Save(filename: path, encoder: active, encoderParams: native);
                } finally {
                    System.Array.ForEach(array: entries, action: static entry => entry.Dispose());
                }
            })),
            _ => Fin.Fail<Unit>(error: op.InvalidResult()),
        };
    }

    private static EncoderParameter CodecParameter(FileRasterCodecParameter parameter) =>
        parameter.Kind switch {
            FileRasterCodecKind.Quality => new EncoderParameter(Encoder.Quality, parameter.Value),
            FileRasterCodecKind.ColorDepth => new EncoderParameter(Encoder.ColorDepth, parameter.Value),
            FileRasterCodecKind.Compression => new EncoderParameter(Encoder.Compression, parameter.Value switch {
                (long)FileTiffCompression.None => (long)EncoderValue.CompressionNone,
                (long)FileTiffCompression.Ccitt3 => (long)EncoderValue.CompressionCCITT3,
                (long)FileTiffCompression.Ccitt4 => (long)EncoderValue.CompressionCCITT4,
                (long)FileTiffCompression.Rle => (long)EncoderValue.CompressionRle,
                _ => (long)EncoderValue.CompressionLZW,
            }),
            _ => new EncoderParameter(Encoder.Quality, parameter.Value),
        };
}
