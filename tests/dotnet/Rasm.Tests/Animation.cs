using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.Extensions;
using Rasm.UI;
using Rasm.UI.Automation;
using Rasm.UI.Controls;
using Rasm.UI.Hosting;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;

namespace Rasm.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Animation {
    // --- [SCENES]
    public static TheoryData<SKEncodedOrigin> Origins => [.. Enum.GetValues<SKEncodedOrigin>().Distinct()];

    public static TheoryData<PhysicalAxis, SKRect> SliderSpans => [
        (PhysicalAxis.Horizontal, new SKRect(0.2f, 0f, 0.8f, 1f)),
        (PhysicalAxis.Vertical, new SKRect(0f, 0.2f, 1f, 0.8f)),
    ];

    public static TheoryData<Seq<Filter>, float> FilterChains {
        get {
            Filter blur = Filter.Css.Validated(FilterFunction.Blur, 2f).ThrowIfFail();
            return [
                (Seq(blur), 0.5f),
                (Seq(blur), 1f),
                (Seq(blur, new Filter.Magnify(2f, 0f)), 1f),
            ];
        }
    }

    public static TheoryData<PhysicalAxis, float, Option<AccessValue>> ScrollRanges => [
        .. from along in PhysicalAxis.Items
           from sample in new[] {
               (Extent: 80f, Value: Option<AccessValue>.None),
               (Extent: 180f, Value: Some<AccessValue>(new AccessValue.Ranged(0d, 0d, 80d))),
           }
           select (along, sample.Extent, sample.Value),
    ];

    [Fact]
    public void StillImageCopiesUseCurrentInfo() {
        using SKImage original = SKImage.Create(new SKImageInfo(3, 2));
        using SKImage replacement = SKImage.Create(new SKImageInfo(2, 3));
        ImageSource.Still copied = new ImageSource.Still(original) with { Image = replacement };
        Assert.Equal(replacement.Info, copied.Info);
    }

    [Fact]
    public async Task FixedSizeCopiesUseCurrentExtent() {
        CancellationToken cancellation = TestContext.Current.CancellationToken;
        Host host = Hosting(new SKSizeI(100, 100));
        SceneHost scene = new(RasterBackend, new FakeTimeProvider());
        await using ConfiguredAsyncDisposable lifetime = scene.ConfigureAwait(true);
        _ = await scene.Rehost(host, StyleSheet.Empty).RunAsync(cancellation).ConfigureAwait(true);
        _ = await scene.Show(new View.Box { Aspect = Factor.Unit, Height = new Sizing.Fixed(2f) with { Extent = 5f } }).RunAsync(cancellation).ConfigureAwait(true);
        Assert.Equal(Some(new SKSize(5f, 5f)), scene.Preferred(host.Size));
    }

    [Theory(DisableDiscoveryEnumeration = true)]
    [MemberData(nameof(SliderSpans))]
    public void SliderSpanFollowsAxis(PhysicalAxis along, SKRect rectangle) {
        Host host = Hosting(new SKSizeI(100, 100));
        StyleSheet sheet = new(HashMap(
            (Roles.Window, Look.Unpainted),
            (Roles.Track, Look.Unpainted),
            (Roles.Handle, Look.Unpainted),
            (Roles.Span, Look.Unpainted with { Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed(Css.White))) })));
        View view = new Slider(along, AxisRange.Validated(0f, 10f).ThrowIfFail(), Seq(2f, 8f), static _ => unitIO).Drawn(host.Culture) with {
            Width = new Sizing.Fixed(host.Size.Width),
            Height = new Sizing.Fixed(host.Size.Height),
        };
        using Shaping shaping = new();
        using SKImage image = Capture.Image(host, sheet, view, 1f, shaping).ThrowIfFail();
        using SKBitmap actual = SKBitmap.FromImage(image);
        using SKBitmap expected = new(image.Info);
        expected.Erase(SKColors.Transparent);
        using SKCanvas canvas = new(expected);
        using SKPaint paint = new() { Color = SKColors.White };
        canvas.Scale(host.Size.Width, host.Size.Height);
        canvas.DrawRect(rectangle, paint);
        Assert.Equal(expected.Pixels, actual.Pixels);
    }

    [Theory(DisableDiscoveryEnumeration = true)]
    [MemberData(nameof(ScrollRanges))]
    public async Task ScrollValueDescribesOverflow(PhysicalAxis along, float extent, Option<AccessValue> value) {
        CancellationToken cancellation = TestContext.Current.CancellationToken;
        Host host = Hosting(new SKSizeI(100, 100));
        SKSize content = along.Map(horizontal: new SKSize(extent, host.Size.Height), vertical: new SKSize(host.Size.Width, extent));
        View view = new View.Scroll(along, new View.Box { Width = new Sizing.Fixed(content.Width), Height = new Sizing.Fixed(content.Height) }) {
            Width = new Sizing.Fixed(host.Size.Width),
            Height = new Sizing.Fixed(host.Size.Height),
        };
        SceneHost scene = new(RasterBackend, new FakeTimeProvider());
        await using ConfiguredAsyncDisposable lifetime = scene.ConfigureAwait(true);
        _ = await scene.Rehost(host, StyleSheet.Empty).RunAsync(cancellation).ConfigureAwait(true);
        _ = await scene.Show(view).RunAsync(cancellation).ConfigureAwait(true);
        AutomationElement element = Assert.Single(Assert.Single(scene.Current.ToSeq()).Tree);
        Assert.Equal(AccessRole.ScrollArea, element.Role);
        Assert.Equal(SKRect.Create(host.Size), element.Frame);
        Assert.Equal(Some(along), element.Accessible.Orientation);
        Assert.Equal(value, element.Accessible.Value);
    }

    [Theory(DisableDiscoveryEnumeration = true)]
    [MemberData(nameof(FilterChains))]
    public void IdentityMagnifierPreservesFilter(Seq<Filter> prefix, float zoom) {
        using SKSurface surface = SKSurface.Create(new SKImageInfo(32, 32));
        using SKPaint paint = new() { Color = SKColors.White };
        surface.Canvas.Clear(SKColors.Transparent);
        surface.Canvas.DrawRect(SKRect.Create(12f, 12f, 8f, 8f), paint);
        using SKImage source = surface.Snapshot();
        using Shaping shaping = new();
        using SKImage expectedImage = FilteredImage(source, prefix, shaping);
        using SKBitmap expected = SKBitmap.FromImage(expectedImage);
        using SKImage actualImage = FilteredImage(source, prefix.Add(new Filter.Magnify(zoom, 0f)), shaping);
        using SKBitmap actual = SKBitmap.FromImage(actualImage);
        Assert.Contains(expected.Pixels, static color => color.Alpha > 0);
        Assert.Equal(expected.Pixels, actual.Pixels);
    }

    [Fact]
    public async Task DisposalFrames() {
        CancellationToken cancellation = TestContext.Current.CancellationToken;
        using SKData data = AnimatedGif();
        using SKCodec codec = SKCodec.Create(data);
        ImageSource.Encoded source = ImageSource.Encoded.Validated(data).ThrowIfFail();
        Host host = Hosting(source.Info.Size);
        FakeTimeProvider clock = new(DateTimeOffset.MinValue);
        List<bool> running = [];
        Func<bool, IO<Unit>> active = Substitute.For<Func<bool, IO<Unit>>>();
        _ = active(Arg.Any<bool>()).Returns(call => IO.lift(() => {
            running.Add(call.Arg<bool>());
            return unit;
        }));
        List<Func<Duration, IO<Unit>>> ticks = [];
        Func<Func<Duration, IO<Unit>>, IO<Func<bool, IO<Unit>>>> linked = Substitute.For<Func<Func<Duration, IO<Unit>>, IO<Func<bool, IO<Unit>>>>>();
        _ = linked(Arg.Do<Func<Duration, IO<Unit>>>(ticks.Add)).Returns(IO.pure(active));
        SceneBackend backend = new(
            static info => Optional(SKSurface.Create(info)).ToFin(new UIFault(info, UIReason.Refused)),
            static _ => IO.pure(unit), linked);
        SceneHost scene = new(backend, clock);
        await using ConfiguredAsyncDisposable lifetime = scene.ConfigureAwait(true);
        using Shaping shaping = new();
        _ = await scene.Rehost(host, StyleSheet.Empty).RunAsync(cancellation).ConfigureAwait(true);
        _ = await scene.Show(Placed(source, host.Size)).RunAsync(cancellation).ConfigureAwait(true);
        Assert.True(running[^1]);
        SKSurface surface = Assert.Single((await scene.Paint(1f).RunAsync(cancellation).ConfigureAwait(true)).ToSeq());
        Func<Duration, IO<Unit>> tick = Assert.Single(ticks);
        Assert.True(running[^1]);
        Frame(0, surface);
        for (int step = 1; step < 2 * codec.FrameCount; step++) {
            clock.Advance(TimeSpan.FromMilliseconds(codec.FrameInfo[(step - 1) % codec.FrameCount].Duration));
            _ = await scene.Now.Bind(tick).RunAsync(cancellation).ConfigureAwait(true);
            Assert.True(running[^1]);
            Frame(step % codec.FrameCount, Assert.Single((await scene.Paint(1f).RunAsync(cancellation).ConfigureAwait(true)).ToSeq()));
            Assert.True(running[^1]);
        }
        running.Clear();
        _ = await scene.Released.RunAsync(cancellation).ConfigureAwait(true);
        Assert.False(Assert.Single(running));
        _ = Assert.Single(ticks);
        Assert.Empty(scene.Current.ToSeq());
        Assert.Equal(nint.Zero, surface.Handle);

        void Frame(int index, SKSurface actualSurface) {
            using SKBitmap decoded = new(codec.Info.WithAlphaType(SKAlphaType.Premul));
            Assert.Equal(SKCodecResult.Success, codec.GetPixels(decoded.Info, decoded.GetPixels(), new SKCodecOptions(index, SKCodecOptions.Default.PriorFrame)));
            using SKImage pixels = SKImage.FromBitmap(decoded);
            using SKImage expectedImage = Capture.Image(host, StyleSheet.Empty, Placed(new ImageSource.Still(pixels), host.Size), 1f, shaping).ThrowIfFail();
            using SKBitmap expected = SKBitmap.FromImage(expectedImage);
            using SKImage actualImage = actualSurface.Snapshot();
            using SKBitmap actual = SKBitmap.FromImage(actualImage);
            Assert.Equal(expected.Pixels, actual.Pixels);
        }
    }

    [Fact]
    public async Task SceneDisposalAwaitsDisplayLink() {
        CancellationToken cancellation = TestContext.Current.CancellationToken;
        using SKData data = AnimatedGif();
        ImageSource.Encoded source = ImageSource.Encoded.Validated(data).ThrowIfFail();
        Host host = Hosting(source.Info.Size);
        TaskCompletionSource stopping = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using SemaphoreSlim release = new(0, 1);
        List<bool> running = [];
        SceneBackend backend = RasterBackend with {
            DisplayLinked = _ => IO.pure<Func<bool, IO<Unit>>>(active => IO.liftAsync(async () => {
                running.Add(active);
                if (!active) {
                    stopping.SetResult();
                    await release.WaitAsync(cancellation).ConfigureAwait(false);
                }
                return unit;
            })),
        };
        SceneHost scene = new(backend, new FakeTimeProvider());
        await using ConfiguredAsyncDisposable lifetime = scene.ConfigureAwait(true);
        _ = await scene.Rehost(host, StyleSheet.Empty).RunAsync(cancellation).ConfigureAwait(true);
        _ = await scene.Show(Placed(source, host.Size)).RunAsync(cancellation).ConfigureAwait(true);
        SKSurface surface = Assert.Single((await scene.Paint(1f).RunAsync(cancellation).ConfigureAwait(true)).ToSeq());
        Assert.Contains(true, running);
        running.Clear();
        Task disposed = scene.DisposeAsync().AsTask();
        try {
            Assert.Same(stopping.Task, await Task.WhenAny(stopping.Task, disposed).WaitAsync(cancellation).ConfigureAwait(true));
            Assert.False(disposed.IsCompleted);
        } finally {
            _ = release.Release();
            await disposed.ConfigureAwait(true);
        }
        Assert.False(Assert.Single(running));
        Assert.Empty(scene.Current.ToSeq());
        Assert.Equal(nint.Zero, surface.Handle);
    }

    [Fact]
    public async Task SceneDisposalReleasesTimerAfterStopFailure() {
        CancellationToken cancellation = TestContext.Current.CancellationToken;
        using SKData data = AnimatedGif();
        ImageSource.Encoded source = ImageSource.Encoded.Validated(data).ThrowIfFail();
        Host host = Hosting(source.Info.Size);
        InvalidOperationException expected = new();
        TaskCompletionSource stopping = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        ITimer timer = Substitute.For<ITimer>();
        timer.ReturnsForAll(delegate {
            _ = stopping.TrySetResult();
            return new ValueTask(release.Task);
        });
        FakeTimeProvider clock = Substitute.ForPartsOf<FakeTimeProvider>();
        _ = clock.Configure().CreateTimer(Arg.Any<TimerCallback>(), Arg.Any<object?>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>()).Returns(timer);
        SceneBackend backend = RasterBackend with {
            DisplayLinked = _ => IO.pure<Func<bool, IO<Unit>>>(active => unless(active, IO.fail<Unit>(Error.New(expected))).As()),
        };
        SceneHost scene = new(backend, clock);
        await using ConfiguredAsyncDisposable lifetime = scene.ConfigureAwait(true);
        _ = await scene.Rehost(host, StyleSheet.Empty).RunAsync(cancellation).ConfigureAwait(true);
        _ = await scene.Show(Placed(source, host.Size)).RunAsync(cancellation).ConfigureAwait(true);
        SKSurface surface = Assert.Single((await scene.Paint(1f).RunAsync(cancellation).ConfigureAwait(true)).ToSeq());
        Task<InvalidOperationException> disposed = Assert.ThrowsAsync<InvalidOperationException>(() => scene.DisposeAsync().AsTask());
        try {
            Assert.Same(stopping.Task, await Task.WhenAny(stopping.Task, disposed).WaitAsync(cancellation).ConfigureAwait(true));
            Assert.False(disposed.IsCompleted);
        } finally {
            release.SetResult();
            Assert.Same(expected, await disposed.ConfigureAwait(true));
        }
        await timer.Received(1).DisposeAsync().ConfigureAwait(true);
        timer.DidNotReceive().Dispose();
        Assert.Empty(scene.Current.ToSeq());
        Assert.Equal(nint.Zero, surface.Handle);
    }

    [Theory]
    [MemberData(nameof(Origins))]
    public void EncodedOrientation(SKEncodedOrigin origin) {
        using SKData data = OrientedWebp(origin);
        ImageSource.Encoded source = ImageSource.Encoded.Validated(data).ThrowIfFail();
        using Image<Rgba32> oriented = Image.Load<Rgba32>(data.AsSpan());
        oriented.Mutate(static image => image.AutoOrient());
        Assert.Equal(new SKSizeI(oriented.Width, oriented.Height), source.Info.Size);
        Rgba32[] pixels = new Rgba32[oriented.Width * oriented.Height];
        oriented.CopyPixelDataTo(pixels);
        SKColor[] expected = [.. pixels.Select(static pixel => new SKColor(pixel.R, pixel.G, pixel.B, pixel.A))];
        using Shaping shaping = new();
        Host host = Hosting(source.Info.Size);
        using SKImage image = Capture.Image(host, StyleSheet.Empty, Placed(source, host.Size), 1f, shaping).ThrowIfFail();
        using SKBitmap actual = SKBitmap.FromImage(image);
        Assert.Equal(expected, actual.Pixels);
    }

    // --- [FIXTURES]
    private static SceneBackend RasterBackend { get; } = new(
        static info => Optional(SKSurface.Create(info)).ToFin(new UIFault(info, UIReason.Refused)),
        static _ => IO.pure(unit), static _ => IO.pure<Func<bool, IO<Unit>>>(static _ => IO.pure(unit)));

    private static SKData AnimatedGif() {
        using Image<Rgba32> image = new(3, 2, new Rgba32(255, 0, 0, 255));
        image.Metadata.GetGifMetadata().RepeatCount = 0;
        (FrameDisposalMode Disposal, Rgba32 Color)[] frames = [
            (FrameDisposalMode.DoNotDispose, new(255, 0, 0, 255)),
            (FrameDisposalMode.RestoreToPrevious, new(0, 255, 0, 255)),
            (FrameDisposalMode.RestoreToBackground, new(0, 0, 255, 255)),
            (FrameDisposalMode.DoNotDispose, new(255, 255, 0, 255)),
        ];
        foreach ((int index, ((FrameDisposalMode disposal, Rgba32 color), (int x, int y))) in frames.Zip(
            from y in Enumerable.Range(0, image.Height)
            from x in Enumerable.Range(0, image.Width)
            select (x, y)).Index()) {
            ImageFrame<Rgba32> frame = index is 0 ? image.Frames.RootFrame : image.Frames.CreateFrame(default);
            frame[x, y] = color;
            GifFrameMetadata metadata = frame.Metadata.GetGifMetadata();
            metadata.DisposalMode = disposal;
            metadata.FrameDelay = 10;
        }
        using MemoryStream encoded = new();
        image.SaveAsGif(encoded);
        return SKData.CreateCopy(encoded.ToArray());
    }

    private static SKData OrientedWebp(SKEncodedOrigin origin) {
        using Image<Rgba32> image = Image.LoadPixelData(new Rgba32[] {
            new(255, 0, 0, 255), new(0, 255, 0, 255), new(0, 0, 255, 255),
            new(255, 255, 0, 255), new(255, 0, 255, 255), new(0, 255, 255, 255),
        }, 3, 2);
        image.Metadata.ExifProfile = new ExifProfile();
        image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, (ushort)origin);
        using MemoryStream encoded = new();
        image.SaveAsWebp(encoded, new WebpEncoder { FileFormat = WebpFileFormatType.Lossless });
        return SKData.CreateCopy(encoded.ToArray());
    }

    private static Host Hosting(SKSizeI size) =>
        new(new SKSize(size.Width, size.Height),
            Theme.Validated(new Seeds(new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d)),
                new TextFace { Family = SKTypeface.Default.FamilyName }, 1f).ThrowIfFail(),
            Visibility.Active, Roles.Window, CultureInfo.InvariantCulture);

    private static SKImage FilteredImage(SKImage source, Seq<Filter> filters, Shaping shaping) {
        Host host = Hosting(source.Info.Size);
        StyleSheet sheet = new(HashMap((Roles.Window, Look.Unpainted), (Roles.Field, Look.Unpainted with { Filters = filters })));
        return Capture.Image(host, sheet, Placed(new ImageSource.Still(source), host.Size) with { Role = Roles.Field }, 1f, shaping).ThrowIfFail();
    }

    private static View.Image Placed(ImageSource source, SKSize size) =>
        new(source) { Width = new Sizing.Fixed(size.Width), Height = new Sizing.Fixed(size.Height), Rendering = ImageRendering.CrispEdges };
}
