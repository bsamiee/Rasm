using System.Drawing.Imaging;
using System.Globalization;
using System.Text.RegularExpressions;
using Rasm.Rhino.Camera;
using Rasm.Rhino.Commands;
using Rhino.FileIO;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using DrawingPointF = System.Drawing.PointF;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSize = System.Drawing.Size;
using IOFileInfo = System.IO.FileInfo;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record FilePublishTarget {
    private FilePublishTarget() { }
    public sealed record Pdf(
        FileEndpoint Target,
        Seq<FilePdfPage> Prefix = default,
        Seq<FilePdfPage> Suffix = default,
        Seq<PdfStamp> Annotate = default,
        Seq<PdfStamp> FinalStamps = default) : FilePublishTarget;
    public sealed record Printer(string Name, int Copies = 1) : FilePublishTarget;
    public sealed record Raster(FileEndpoint Target, Option<FileRasterEncoding> Encoding = default, FileRasterSettings Settings = default) : FilePublishTarget {
        internal FileRasterEncoding ResolvedEncoding => Encoding.IfNone(FileRasterEncoding.Png);
    }
    public sealed record Svg(FileEndpoint Target) : FilePublishTarget;

    internal Fin<FilePublishResult> Write(Seq<FileViewPage> pages, bool layers, Op op) =>
        Switch(
            (Pages: pages, Layers: layers, Op: op),
            pdf: static (ctx, target) => WritePdf(target: target, pages: ctx.Pages, layers: ctx.Layers, op: ctx.Op),
            printer: static (ctx, target) => WritePrinter(target: target, pages: ctx.Pages, op: ctx.Op),
            raster: static (ctx, target) => WriteRaster(target: target, pages: ctx.Pages, op: ctx.Op),
            svg: static (ctx, target) => WriteSvg(target: target, pages: ctx.Pages, op: ctx.Op));
    private static Fin<FilePublishResult> WritePdf(Pdf target, Seq<FileViewPage> pages, bool layers, Op op) =>
        from fmtPdf in FileFormat.KnownFormat(key: "pdf", op: op)
        from endpoint in target.Target.WithFormat(format: fmtPdf).Output(op: op)
        from pdf in Optional(FilePdf.Create()).ToFin(Fail: op.InvalidResult())
        let totalPages = target.Prefix.Count + pages.Count + target.Suffix.Count
        from _prefix in target.Prefix.Map((spec, index) => (Spec: spec, Index: index + 1)).TraverseM(item => AddBlankPdfPage(pdf: pdf, spec: item.Spec, pageIndex: item.Index, pageCount: totalPages, op: op)
            .Bind(page => AnnotatePage(pdf: pdf, page: page, stamps: target.Annotate, context: new PdfStampContext(PageIndex: item.Index, PageCount: totalPages, View: Option<FileViewReport>.None), op: op))).As()
        from sheets in pages.TraverseM(page =>
            from rendered in page.Render(render: owned => op.Catch(() => {
                bool prior = pdf.LayersAsOptionalContentGroups;
                pdf.LayersAsOptionalContentGroups = layers && !page.Spec.Recipe.Raster;
                int index = pdf.AddPage(settings: owned);
                pdf.LayersAsOptionalContentGroups = prior;
                return index >= 0 ? Fin.Succ(value: index) : Fin.Fail<int>(error: op.InvalidResult());
            }), op: op)
            let pageIndex = rendered.Value + 1
            from annotated in AnnotatePage(pdf: pdf, page: rendered.Value, stamps: target.Annotate, context: new PdfStampContext(PageIndex: pageIndex, PageCount: totalPages, View: Some(rendered.Report)), op: op)
            select rendered.Report).As()
        from _suffix in target.Suffix.Map((spec, index) => (Spec: spec, Index: target.Prefix.Count + pages.Count + index + 1)).TraverseM(item => AddBlankPdfPage(pdf: pdf, spec: item.Spec, pageIndex: item.Index, pageCount: totalPages, op: op)
            .Bind(page => AnnotatePage(pdf: pdf, page: page, stamps: target.Annotate, context: new PdfStampContext(PageIndex: item.Index, PageCount: totalPages, View: Option<FileViewReport>.None), op: op))).As()
        from _verify in Flush(pdf: pdf, path: endpoint.Path, finalStamps: target.FinalStamps, totalPages: totalPages, op: op)
        select new FilePublishResult(
            Target: Some(endpoint),
            Format: Some(fmtPdf),
            Receipt: DocumentReceipt.Empty,
            Views: sheets);

    private static Fin<Unit> Flush(FilePdf pdf, string path, Seq<PdfStamp> finalStamps, int totalPages, Op op) =>
        finalStamps.IsEmpty
            ? op.Catch(() => { pdf.Write(filename: path); return VerifyFile(path: path, op: op); })
            : op.Catch(() => {
                // BOUNDARY ADAPTER — PreWrite is a static multicast event fired inside the native flush;
                // the identity guard isolates concurrent FilePublish writers and try/finally guarantees symmetric detach.
                void Stamp(object? sender, FilePdfEventArgs args) =>
                    _ = ReferenceEquals(objA: args.Pdf, objB: pdf)
                        ? toSeq(Enumerable.Range(start: 0, count: totalPages)).TraverseM(index => PdfStamp.DrawAll(
                            stamps: finalStamps, pdf: args.Pdf, page: index,
                            context: new PdfStampContext(PageIndex: index + 1, PageCount: totalPages, View: Option<FileViewReport>.None),
                            op: op)).As().Map(static _ => unit)
                        : Fin.Succ(value: unit);
                FilePdf.PreWrite += Stamp;
                try {
                    pdf.Write(filename: path);
                    return VerifyFile(path: path, op: op);
                } finally {
                    FilePdf.PreWrite -= Stamp;
                }
            });

    private static Fin<int> AddBlankPdfPage(FilePdf pdf, FilePdfPage spec, int pageIndex, int pageCount, Op op) =>
        from valid in spec.Validate(op: op)
        from index in op.Catch(() => pdf.AddPage(widthInDots: valid.WidthDots, heightInDots: valid.HeightDots, dotsPerInch: valid.Dpi) switch {
            int value when value >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.InvalidResult()),
        })
        from annotated in AnnotatePage(pdf: pdf, page: index, stamps: valid.Stamps, context: new PdfStampContext(PageIndex: pageIndex, PageCount: pageCount, View: Option<FileViewReport>.None), op: op)
        select index;

    private static Fin<Unit> AnnotatePage(FilePdf pdf, int page, Seq<PdfStamp> stamps, PdfStampContext context, Op op) =>
        PdfStamp.DrawAll(stamps: stamps, pdf: pdf, page: page, context: context, op: op);

    private static Fin<FilePublishResult> WritePrinter(Printer target, Seq<FileViewPage> pages, Op op) =>
        from name in FileEndpoint.NonBlank(value: target.Name, op: op)
        from copies in guard(target.Copies > 0, op.InvalidInput()).ToFin().Map(_ => target.Copies)
        from result in op.Catch(() =>
            pages.TraverseM(page =>
                from settings in page.Settings(op: op)
                select (Settings: settings, Report: page.ReportOf(settings: settings))).As()
            .Bind(pairs => {
                ViewCaptureSettings[] settingsArr = [.. pairs.Map(static pair => pair.Settings)];
                // BOUNDARY ADAPTER — SendToPrinter mandates a ViewCaptureSettings[]; the finally disposes every element (each owns native capture state).
                try {
                    return op.Confirm(success: ViewCapture.SendToPrinter(printerName: name, settings: settingsArr, copies: copies))
                        .Map(_ => new FilePublishResult(
                            Target: Option<FileEndpoint>.None,
                            Format: Option<FileFormat>.None,
                            Receipt: DocumentReceipt.Empty,
                            Views: pairs.Map(static pair => pair.Report),
                            NativeLog: Some(string.Create(CultureInfo.InvariantCulture, $"printer:{name};copies:{copies};views:{pages.Count}"))));
                } finally {
                    System.Array.ForEach(array: settingsArr, action: static settings => settings.Dispose());
                }
            }))
        select result;

    private static Fin<FilePublishResult> WriteRaster(Raster target, Seq<FileViewPage> pages, Op op) {
        FileRasterEncoding encoding = target.ResolvedEncoding;
        return from settings in target.Settings.Validate(encoding: encoding, op: op)
               from result in CaptureViews(target: target.Target, format: encoding.Format, pages: pages, op: op,
            render: (page, owned, path) => {
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
        from fmtSvg in FileFormat.KnownFormat(key: "svg", op: op)
        from result in CaptureViews(target: target.Target, format: fmtSvg, pages: pages, op: op,
            render: (_, owned, path) => op.Catch(() => {
                System.Xml.XmlDocument document = ViewCapture.CaptureToSvg(settings: owned);
                document.Save(filename: path);
                return VerifyFile(path: path, op: op);
            }),
            log: string.Create(CultureInfo.InvariantCulture, $"svg;views:{pages.Count}"))
        select result;

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
            Seq<(Encoder Key, long Value)> pairs = encoding.Parameters(settings: settings);
            return pairs.IsEmpty
                ? Fin.Succ(value: Op.Side(() => bitmap.Save(filename: path, format: encoding.Image))).Bind(_ => VerifyFile(path: path, op: op))
                : (toSeq(ImageCodecInfo.GetImageEncoders()).Find(c => c.FormatID == encoding.Image.Guid).Case as ImageCodecInfo) switch {
                    ImageCodecInfo active => op.Catch(() => {
                        EncoderParameter[] entries = [.. pairs.AsIterable().Select(static p => new EncoderParameter(p.Key, p.Value))];
                        try {
                            using EncoderParameters native = new(entries.Length) { Param = entries };
                            bitmap.Save(filename: path, encoder: active, encoderParams: native);
                        } finally {
                            System.Array.ForEach(array: entries, action: static e => e.Dispose());
                        }
                        return VerifyFile(path: path, op: op);
                    }),
                    _ => Fin.Fail<Unit>(error: op.InvalidResult()),
                };
        });
}

[SmartEnum<int>]
public sealed partial class FileRasterEncoding {
    public static readonly FileRasterEncoding Png = new(
        key: 0,
        format: FileFormat.KnownFormat(key: "png", op: Op.Of(name: nameof(FileRasterEncoding))).ThrowIfFail(),
        image: DrawingImageFormat.Png,
        supportsAlpha: true,
        compression: FileTiffCompression.Default,
        encode: static (_, settings) =>
            settings.PngDepth.Map(d => Seq<(Encoder, long)>((Encoder.ColorDepth, d))).IfNone(Seq<(Encoder, long)>()));
    public static readonly FileRasterEncoding Jpeg = new(
        key: 1,
        format: FileFormat.KnownFormat(key: "jpeg", op: Op.Of(name: nameof(FileRasterEncoding))).ThrowIfFail(),
        image: DrawingImageFormat.Jpeg,
        supportsAlpha: false,
        compression: FileTiffCompression.Default,
        encode: static (_, settings) => Seq<(Encoder, long)>((Encoder.Quality, settings.JpegQuality)));
    public static readonly FileRasterEncoding Tiff = new(
        key: 2,
        format: FileFormat.KnownFormat(key: "tiff", op: Op.Of(name: nameof(FileRasterEncoding))).ThrowIfFail(),
        image: DrawingImageFormat.Tiff,
        supportsAlpha: true,
        compression: FileTiffCompression.Lzw,
        encode: static (enc, settings) => Seq<(Encoder, long)>((Encoder.Compression, settings.TiffCompression.IfNone(noneValue: enc.Compression) switch {
            FileTiffCompression value when value == FileTiffCompression.None => (long)EncoderValue.CompressionNone,
            FileTiffCompression value when value == FileTiffCompression.Ccitt3 => (long)EncoderValue.CompressionCCITT3,
            FileTiffCompression value when value == FileTiffCompression.Ccitt4 => (long)EncoderValue.CompressionCCITT4,
            FileTiffCompression value when value == FileTiffCompression.Rle => (long)EncoderValue.CompressionRle,
            _ => (long)EncoderValue.CompressionLZW,
        })));
    public static readonly FileRasterEncoding Bitmap = new(
        key: 3,
        format: FileFormat.KnownFormat(key: "bmp", op: Op.Of(name: nameof(FileRasterEncoding))).ThrowIfFail(),
        image: DrawingImageFormat.Bmp,
        supportsAlpha: false,
        compression: FileTiffCompression.Default,
        encode: static (_, _) => Seq<(Encoder, long)>());

    public FileFormat Format { get; }
    public DrawingImageFormat Image { get; }
    public FileTiffCompression Compression { get; }
    public bool SupportsAlpha { get; }

    internal Seq<(Encoder Key, long Value)> Parameters(FileRasterSettings settings) =>
        encode(arg1: this, arg2: settings);

    private readonly Func<FileRasterEncoding, FileRasterSettings, Seq<(Encoder, long)>> encode;
}

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
            viewport: static (ctx, source) => source.Target.Resolve(document: ctx.Doc, op: ctx.Op)
                .Map(scopes => scopes.Map(scope => FileViewPage.Model(view: scope.View, viewport: Some(scope.Viewport), spec: ctx.Spec))));

    private static Fin<Seq<FileViewPage>> ResolvePages(RhinoDoc document, Pages source, FileView spec, Op op) =>
        from pages in source.Query.Resolve(document: document, op: op)
        from matched in guard(!pages.IsEmpty, op.InvalidInput()).ToFin().Map(_ => pages)
        select matched.Map(page => FileViewPage.Layout(page: page, spec: spec));
    private static Fin<Seq<FileViewPage>> ResolveNamed(RhinoDoc document, Named source, FileView spec, Op op) =>
        from _ in guard(!source.Names.IsEmpty, op.InvalidInput())
        from scopes in source.Target.Resolve(document: document, op: op)
        from scope in RhinoCamera.SingleScope(scopes: scopes, op: op)
        from path in source.Path.Case switch {
            CameraPath restore => restore.RequireSynchronous(op: op).Map(Some),
            _ => Fin.Succ(Option<CameraPath>.None),
        }
        from pages in source.Names.TraverseM(name => FileEndpoint.NonBlank(value: name, op: op).Map(valid =>
            FileViewPage.Model(
                view: scope.View,
                viewport: Some(scope.Viewport),
                spec: spec,
                named: Some(new FileNamedViewCapture(Scope: scope, Name: valid, Path: path, Restore: source.Restore))))).As()
        select pages;
}

[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record PdfStamp {
    private PdfStamp() { }
    public sealed record Text(string Value, double X, double Y, float HeightPoints, Font Font, DrawingColor Fill,
        Option<(DrawingColor Color, float Width)> Stroke = default, float AngleDegrees = 0f,
        TextHorizontalAlignment Horizontal = TextHorizontalAlignment.Left, TextVerticalAlignment Vertical = TextVerticalAlignment.Top) : PdfStamp;
    public sealed record Polyline(Seq<DrawingPointF> Points, Option<DrawingColor> Fill, DrawingColor Stroke, float Width) : PdfStamp;
    public sealed record Line(DrawingPointF From, DrawingPointF To, DrawingColor Stroke, float Width) : PdfStamp;
    public sealed record Image(DrawingBitmap Bitmap, float X, float Y, float Width, float Height, float AngleDegrees = 0f) : PdfStamp;

    internal Fin<Unit> Draw(FilePdf pdf, int page, PdfStampContext context, Op op) =>
        Switch((Pdf: pdf, Page: page, Context: context, Op: op),
            text: static (ctx, s) => ctx.Op.Catch(() => Fin.Succ(value: Op.Side(() => ctx.Pdf.DrawText(
                pageNumber: ctx.Page, text: ctx.Context.Format(value: s.Value), x: s.X, y: s.Y, heightPoints: s.HeightPoints, onfont: s.Font, fillColor: s.Fill,
                strokeColor: s.Stroke.Case switch { (DrawingColor c, _) => c, _ => DrawingColor.Empty },
                strokeWidth: s.Stroke.Case switch { (_, float w) => w, _ => 0f },
                angleDegrees: s.AngleDegrees, horizontalAlignment: s.Horizontal, verticalAlignment: s.Vertical)))),
            polyline: static (ctx, s) => ctx.Op.Catch(() => Fin.Succ(value: Op.Side(() => ctx.Pdf.DrawPolyline(
                pageNumber: ctx.Page, polyline: [.. s.Points], fillColor: s.Fill.IfNone(DrawingColor.Empty), strokeColor: s.Stroke, strokeWidth: s.Width)))),
            line: static (ctx, s) => ctx.Op.Catch(() => Fin.Succ(value: Op.Side(() => ctx.Pdf.DrawLine(
                pageNumber: ctx.Page, from: s.From, to: s.To, strokeColor: s.Stroke, strokeWidth: s.Width)))),
            image: static (ctx, s) => ctx.Op.Catch(() => Fin.Succ(value: Op.Side(() => ctx.Pdf.DrawBitmap(
                pageNumber: ctx.Page, bitmap: s.Bitmap, left: s.X, top: s.Y, width: s.Width, height: s.Height, rotationInDegrees: s.AngleDegrees)))));

    internal static Fin<Unit> DrawAll(Seq<PdfStamp> stamps, FilePdf pdf, int page, PdfStampContext context, Op op) =>
        stamps.TraverseM(stamp => stamp.Draw(pdf: pdf, page: page, context: context, op: op)).As().Map(static _ => unit);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct FilePdfPage(
    int WidthDots,
    int HeightDots,
    int Dpi,
    Seq<PdfStamp> Stamps = default) {
    internal Fin<FilePdfPage> Validate(Op op) =>
        WidthDots > 0 && HeightDots > 0 && Dpi > 0
            ? Fin.Succ(value: this)
            : Fin.Fail<FilePdfPage>(error: op.InvalidInput());
}

public sealed record FilePublish(FilePublishTarget Target, Seq<FileView> Views, bool Layers = true, Option<string> Snapshot = default) {
    public FilePublish WithSnapshot(string name) =>
        this with { Snapshot = string.IsNullOrWhiteSpace(value: name) ? Option<string>.None : Some(name.Trim()) };
}

public readonly record struct FileRasterSettings(
    long JpegQuality = FileRasterSettings.DefaultJpegQuality,
    Option<FileTiffCompression> TiffCompression = default,
    Option<int> PngDepth = default,
    Option<double> ExifDpi = default,
    bool Transparent = false) {
    internal const long DefaultJpegQuality = 90L;

    internal Fin<FileRasterSettings> Validate(FileRasterEncoding encoding, Op op) {
        // C# cannot capture struct `this` inside a query comprehension; a local copy is required.
        FileRasterSettings self = this;
        return from _quality in guard(encoding != FileRasterEncoding.Jpeg || self.JpegQuality is >= 0L and <= 100L, op.InvalidInput()).ToFin()
               from _depth in (encoding == FileRasterEncoding.Png, self.PngDepth.Case) switch {
                   (true, int depth) when depth > 0 => Fin.Succ(value: unit),
                   (true, int) => Fin.Fail<Unit>(error: op.InvalidInput()),
                   _ => Fin.Succ(value: unit),
               }
               from _dpi in self.ExifDpi.Case switch {
                   double dpi when RhinoMath.IsValidDouble(x: dpi) && dpi > 0d => Fin.Succ(value: unit),
                   double => Fin.Fail<Unit>(error: op.InvalidInput()),
                   _ => Fin.Succ(value: unit),
               }
               from _compress in self.TiffCompression.Case switch {
                   FileTiffCompression value when encoding != FileRasterEncoding.Tiff || value.Valid => Fin.Succ(value: unit),
                   FileTiffCompression => Fin.Fail<Unit>(error: op.InvalidInput()),
                   _ => Fin.Succ(value: unit),
               }
               from _alpha in guard(!self.Transparent || encoding.SupportsAlpha, op.InvalidInput()).ToFin()
               select self;
    }
}

public sealed partial record FileView(
    FileViewSource Source,
    CaptureRecipe Recipe = default) {
    internal const double DefaultDpi = 300.0;
    private CaptureRecipe Policy =>
        Recipe.WithPolicy(fallbackDpi: DefaultDpi, fallbackDecor: CaptureDecor.Publish, rewrite: RewriteDecor);

    internal Fin<ViewCaptureSettings> Open(RhinoView view, Option<RhinoViewport> viewport, Op op) =>
        from active in Optional(view).ToFin(Fail: op.InvalidInput())
        from settings in Policy.Open(
            view: active,
            viewport: ViewportFor(view: active, viewport: viewport),
            op: op)
        select settings;

    internal Fin<T> Render<T>(RhinoView view, Option<RhinoViewport> viewport, Func<ViewCaptureSettings, Fin<T>> project, Op op) =>
        from active in Optional(view).ToFin(Fail: op.InvalidInput())
        from result in Policy.Render(
            view: active,
            viewport: ViewportFor(view: active, viewport: viewport),
            project: project,
            op: op)
        select result;

    private Option<RhinoViewport> ViewportFor(RhinoView view, Option<RhinoViewport> viewport) =>
        viewport.Case switch {
            RhinoViewport active => Some(active),
            _ => Recipe.Viewport(view: view),
        };

    internal Fin<DrawingBitmap> TransparentBitmap(RhinoView view, ViewCaptureSettings settings, Op op) =>
        Recipe.WithPolicy(fallbackDpi: DefaultDpi, fallbackDecor: CaptureDecor.Publish with { UsePrintWidths = false }, rewrite: RewriteDecor).TransparentBitmap(
            view: view,
            settings: settings,
            op: op);

    private static CaptureDecor RewriteDecor(CaptureDecor decor, RhinoView target) =>
        decor with {
            HeaderText = decor.HeaderText.Map(text => Interpolate(template: text, document: target.Document, view: target)),
            FooterText = decor.FooterText.Map(text => Interpolate(template: text, document: target.Document, view: target)),
        };

    private static string Interpolate(string template, RhinoDoc document, RhinoView view) {
        // GeneratedRegex evaluators are static delegates; all doc/view state must be captured into
        // locals because the evaluator cannot reach instance or outer scopes through closure.
        int total = document.Views.GetPageViews()?.Length ?? 0;
        string name = (view switch { RhinoPageView page => page.PageName, _ => view.MainViewport.Name }) ?? string.Empty;
        int index = view switch { RhinoPageView page => page.PageNumber + 1, _ => 0 };
        return string.IsNullOrEmpty(value: template)
            ? template
            : PublishTokens.Pattern.Replace(input: template, evaluator: match => match.Groups["key"].Value switch {
                "page" => name,
                "index" => index.ToString(provider: CultureInfo.InvariantCulture),
                "total" => total.ToString(provider: CultureInfo.InvariantCulture),
                string key => document.Strings.GetValue(key: key) ?? string.Empty,
            });
    }
}

public readonly record struct FileViewReport(
    string Name,
    Option<string> Scale,
    int DetailCount,
    DrawingSize MediaSize = default,
    DrawingRectangle CropRectangle = default,
    double Resolution = 0.0,
    ViewCaptureSettings.ViewAreaMapping ViewArea = default,
    bool Raster = false,
    bool ScaleToFit = false,
    Option<double> ModelScale = default);

internal sealed record FileNamedViewCapture(CameraScope Scope, string Name, Option<CameraPath> Path, NamedRestorePolicy Restore) {
    internal Fin<T> Use<T>(Func<Fin<T>> body) =>
        CameraOps.RestoreThen(name: Name, capture: CameraOps.Query(_ => body()), path: Path, restore: Some(Restore))
            .Run(arg: Scope)
            .Bind(outcome => outcome.Redraw.ApplyTo(scope: Scope).Map(_ => outcome.Value));
}

internal readonly record struct FilePublishResult(
    Option<FileEndpoint> Target,
    Option<FileFormat> Format,
    DocumentReceipt Receipt,
    Seq<FileViewReport> Views = default,
    Seq<FileIssue> Issues = default,
    Option<string> NativeLog = default);

internal sealed record FileViewPage(RhinoView Target, Option<RhinoViewport> Viewport, FileView Spec, Option<FileNamedViewCapture> Named = default) {
    internal static FileViewPage Layout(RhinoPageView page, FileView spec) =>
        new(Target: page, Viewport: Option<RhinoViewport>.None, Spec: spec);

    internal static FileViewPage Model(RhinoView view, Option<RhinoViewport> viewport, FileView spec, Option<FileNamedViewCapture> named = default) =>
        new(Target: view, Viewport: viewport, Spec: spec, Named: named);
    private Fin<T> UseMaybe<T>(Func<Fin<T>> body) =>
        Named.Case switch {
            FileNamedViewCapture named => named.Use(body: body),
            _ => body(),
        };

    internal Fin<ViewCaptureSettings> Settings(Op op) =>
        UseMaybe(body: () => Spec.Open(view: Target, viewport: Viewport, op: op));

    internal Fin<(T Value, FileViewReport Report)> Render<T>(Func<ViewCaptureSettings, Fin<T>> render, Op op) =>
        UseMaybe(body: () => Spec.Render(
            view: Target,
            viewport: Viewport,
            project: settings => render(arg: settings).Map(value => (Value: value, Report: ReportOf(settings: settings))),
            op: op));

    internal FileViewReport ReportOf(ViewCaptureSettings settings) {
        static Option<string> FormatScale(double scale) =>
            RhinoMath.IsValidDouble(x: scale) && scale > 0.0
                ? Some(string.Create(CultureInfo.InvariantCulture, $"{scale:0.######}"))
                : Option<string>.None;
        static FileViewReport Build(string name, Option<string> scale, int detailCount, ViewCaptureSettings s, Option<double> raw) =>
            new(Name: name, Scale: scale, DetailCount: detailCount,
                MediaSize: s.MediaSize, CropRectangle: s.CropRectangle, Resolution: s.Resolution,
                ViewArea: s.ViewArea, Raster: s.RasterMode, ScaleToFit: s.IsScaleToFit, ModelScale: raw);
        static FileViewReport Page(RhinoPageView page, ViewCaptureSettings s) {
            DetailViewObject[] details = page.GetDetailViews() ?? [];
            Seq<string> scales = toSeq(details).Choose(FileScale.Format).Distinct();
            return Build(name: page.PageName,
                scale: scales.IsEmpty ? Option<string>.None : Some(string.Join(separator: ", ", values: scales.AsIterable())),
                detailCount: details.Length, s: s, raw: Option<double>.None);
        }
        FileViewReport Model(RhinoView view) {
            double raw = settings.GetModelScale(pageUnits: view.Document.PageUnitSystem, modelUnits: view.Document.ModelUnitSystem);
            string viewName = Viewport.Map(static vp => vp.Name).IfNone(view.MainViewport.Name ?? string.Empty);
            return (settings.IsScaleToFit, raw) switch {
                (true, _) => Build(name: viewName, scale: Some("fit"), detailCount: 0, s: settings, raw: Option<double>.None),
                (false, double scale) when RhinoMath.IsValidDouble(x: scale) && scale > 0.0
                          => Build(name: viewName, scale: FormatScale(scale), detailCount: 0, s: settings, raw: Some(scale)),
                _ => Build(name: viewName, scale: Option<string>.None, detailCount: 0, s: settings, raw: Option<double>.None),
            };
        }
        return Target switch {
            RhinoPageView page when Viewport.IsNone => Page(page: page, s: settings),
            RhinoView view => Model(view: view),
            _ => Build(name: string.Empty, scale: Option<string>.None, detailCount: 0, s: settings, raw: Option<double>.None),
        };
    }
}

internal readonly partial record struct PdfStampContext(int PageIndex, int PageCount, Option<FileViewReport> View) {
    internal string Format(string value) {
        int pageIndex = PageIndex;
        int pageCount = PageCount;
        Option<FileViewReport> view = View;
        return PublishTokens.Pattern.Replace(input: value, evaluator: match => match.Groups["key"].Value switch {
            "index" => pageIndex.ToString(provider: CultureInfo.InvariantCulture),
            "total" => pageCount.ToString(provider: CultureInfo.InvariantCulture),
            "page" => view.Map(static report => report.Name).IfNone(string.Empty),
            "scale" => view.Bind(static report => report.Scale).IfNone(string.Empty),
            "details" => view.Map(static report => report.DetailCount.ToString(provider: CultureInfo.InvariantCulture)).IfNone("0"),
            string => string.Empty,
        });
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static partial class PublishTokens {
    [GeneratedRegex(
        pattern: "\\{(?<key>[^{}]+)\\}",
        options: RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture,
        matchTimeoutMilliseconds: 250)]
    internal static partial Regex Pattern { get; }
}
