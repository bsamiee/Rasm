using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Win32.SafeHandles;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]

namespace Rasm.Imaging.Filters.Denoise;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum OidnError {
    None = 0,
    Unknown = 1,
    InvalidArgument = 2,
    InvalidOperation = 3,
    OutOfMemory = 4,
    UnsupportedHardware = 5,
    Cancelled = 6,
}

public enum GuidePrefilter {
    Off,
    Fast,
    Accurate,
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidDenoise>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DenoiseGuides {
    public static readonly DenoiseGuides Rgb = new("rgb", Seq<GuideChannel>());
    public static readonly DenoiseGuides RgbAlbedo = new("rgb-albedo", Seq(GuideChannel.Albedo));
    public static readonly DenoiseGuides RgbAlbedoNormal = new("rgb-albedo-normal", Seq(GuideChannel.Albedo, GuideChannel.Normal));

    public Seq<GuideChannel> Channels { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidDenoise>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DenoiseQuality {
    public static readonly DenoiseQuality High = new("high", 6, 20000);
    public static readonly DenoiseQuality Balanced = new("balanced", 5, 20000);
    public static readonly DenoiseQuality Fast = new("fast", 4, 20300);

    public int Value { get; }
    public int Since { get; }
}

public sealed record DenoiseState(DenoiseGuides Guides, GuidePrefilter Prefilter, DenoiseQuality Quality, Mix Blend)
    : IStateRecord<DenoiseState, DenoiseParameter, InvalidDenoise> {
    public static DenoiseState Default { get; } = new(DenoiseGuides.RgbAlbedoNormal, GuidePrefilter.Accurate, DenoiseQuality.High, Mix.Full);
}

[SmartEnum<string>]
[ValidationError<InvalidDenoise>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DenoiseParameter : IStateParameter<DenoiseState> {
    public static readonly DenoiseParameter Guides = new("guides", new StateParameter<DenoiseState>.Choice<DenoiseGuides, InvalidDenoise>(
        Lens<DenoiseState, DenoiseGuides>.New(static state => state.Guides, static guides => state => state with { Guides = guides })));
    public static readonly DenoiseParameter Prefilter = new("prefilter", new StateParameter<DenoiseState>.Enumerated<GuidePrefilter>(
        Lens<DenoiseState, GuidePrefilter>.New(static state => state.Prefilter, static prefilter => state => state with { Prefilter = prefilter })));
    public static readonly DenoiseParameter Quality = new("quality", new StateParameter<DenoiseState>.Choice<DenoiseQuality, InvalidDenoise>(
        Lens<DenoiseState, DenoiseQuality>.New(static state => state.Quality, static quality => state => state with { Quality = quality })));
    public static readonly DenoiseParameter Blend = new("blend", StateParameter.Bounded<DenoiseState, Mix, float, InvalidGrade>(
        Lens<DenoiseState, Mix>.New(static state => state.Blend, static blend => state => state with { Blend = blend }), new()));

    public StateParameter<DenoiseState> Kind { get; }
}

// --- [SERVICES] ------------------------------------------------------------------------
internal abstract class SafeOidnHandle(Action<nint> release) : SafeHandleZeroOrMinusOneIsInvalid(ownsHandle: true) {
    protected override bool ReleaseHandle() {
        release(handle);
        return true;
    }
}

internal sealed class SafeOidnDeviceHandle() : SafeOidnHandle(Denoiser.oidnReleaseDevice);
internal sealed class SafeOidnFilterHandle() : SafeOidnHandle(Denoiser.oidnReleaseFilter);
internal sealed class SafeOidnBufferHandle() : SafeOidnHandle(Denoiser.oidnReleaseBuffer);

public sealed partial class Denoiser : IFrameJob<Denoiser, DenoiseState> {
    // --- [NATIVE]
    private const string Library = "OpenImageDenoise.2";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool ProgressMonitor(nint userPtr, double n);

    [LibraryImport(Library)]
    private static partial SafeOidnDeviceHandle oidnNewDevice(int type = 0);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void oidnSetDeviceBool(SafeOidnDeviceHandle device, string name, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(Library)]
    private static partial void oidnCommitDevice(SafeOidnDeviceHandle device);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int oidnGetDeviceInt(SafeOidnDeviceHandle device, string name);

    [LibraryImport(Library)]
    private static partial OidnError oidnGetDeviceError(SafeOidnDeviceHandle device, out nint outMessage);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial SafeOidnFilterHandle oidnNewFilter(SafeOidnDeviceHandle device, string type);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void oidnSetFilterImage(
        SafeOidnFilterHandle filter, string name, SafeOidnBufferHandle buffer, int format,
        nuint width, nuint height, nuint byteOffset, nuint pixelByteStride, nuint rowByteStride);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void oidnSetFilterBool(SafeOidnFilterHandle filter, string name, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void oidnSetFilterInt(SafeOidnFilterHandle filter, string name, int value);

    [LibraryImport(Library)]
    private static partial void oidnSetFilterProgressMonitorFunction(SafeOidnFilterHandle filter, nint func, nint userPtr);

    [LibraryImport(Library)]
    private static partial void oidnCommitFilter(SafeOidnFilterHandle filter);

    [LibraryImport(Library)]
    private static partial void oidnExecuteFilter(SafeOidnFilterHandle filter);

    [LibraryImport(Library)]
    private static partial SafeOidnBufferHandle oidnNewBuffer(SafeOidnDeviceHandle device, nuint byteSize);

    [LibraryImport(Library)]
    private static partial void oidnWriteBuffer(SafeOidnBufferHandle buffer, nuint byteOffset, nuint byteSize, ReadOnlySpan<float> srcHostPtr);

    [LibraryImport(Library)]
    private static partial void oidnReadBuffer(SafeOidnBufferHandle buffer, nuint byteOffset, nuint byteSize, Span<float> dstHostPtr);

    [LibraryImport(Library)]
    internal static partial void oidnReleaseDevice(nint device);

    [LibraryImport(Library)]
    internal static partial void oidnReleaseFilter(nint filter);

    [LibraryImport(Library)]
    internal static partial void oidnReleaseBuffer(nint buffer);

    private static Fin<Unit> Status(SafeOidnDeviceHandle device) =>
        oidnGetDeviceError(device, out nint message) switch {
            OidnError.None => unit,
            OidnError.Unknown => new OidnUnknown(),
            OidnError.OutOfMemory => new OidnOutOfMemory(),
            OidnError.UnsupportedHardware => new OidnUnsupportedHardware(),
            OidnError.Cancelled => Errors.Cancelled,
            OidnError.InvalidArgument => new OidnInvalidArgument(),
            OidnError.InvalidOperation => throw new InvalidOperationException(Marshal.PtrToStringUTF8(message)),
        };

    // --- [LIFECYCLE]
    private readonly SafeOidnDeviceHandle device;
    private readonly int version;
    private readonly Lock gate = new();

    private Denoiser(SafeOidnDeviceHandle device, int version) => (this.device, this.version) = (device, version);

    public static IO<Denoiser> Open(Action<IDisposable> retain) =>
        use(static () => oidnNewDevice()).Bind(device => IO.lift(() =>
            Status(device).Bind(_ => {
                oidnSetDeviceBool(device, "setAffinity", value: false);
                return Status(device);
            }).Bind(_ => {
                oidnCommitDevice(device);
                return Status(device);
            }).Bind(_ => oidnGetDeviceInt(device, "version") switch {
                var result => Status(device).Map(_ => {
                    Denoiser denoiser = new(device, result);
                    retain(denoiser);
                    return denoiser;
                }),
            }))).BracketFail().Uninterruptible();

    public void Dispose() {
        using Lock.Scope serialized = gate.EnterScope();
        device.Dispose();
    }

    // --- [RUN]
    public static Seq<GuideChannel> Channels(DenoiseState state) => state.Guides.Channels;

    public IO<PixelFrame> Run(DenoiseState state, PixelFrame color, HashMap<GuideChannel, PixelFrame> guides, IProgress<int> rows) =>
        from selected in IO.lift(Seq<(string Role, GuideChannel Channel)>(("albedo", GuideChannel.Albedo), ("normal", GuideChannel.Normal))
            .Filter(input => state.Guides.Channels.Exists(channel => channel == input.Channel))
            .Traverse(input =>
                from frame in guides.Find(input.Channel).ToFin(new DenoiseGuideAbsent(input.Channel))
                let view = frame.Window
                let target = color.Window
                from covered in guard<Error>(frame.Extent == color.Extent && view.Left <= target.Left && view.Top <= target.Top && view.Right >= target.Right && view.Bottom >= target.Bottom,
                    new DenoiseGuideUncovered(input.Channel))
                select (input.Role, Frame: frame)).As().Map(static selected => selected.As()))
        from token in cancelToken
        from result in IO.lift(() => Denoised(state, color, selected, token, rows))
        select result;

    private Fin<PixelFrame> Denoised(DenoiseState state, PixelFrame color, Seq<(string Role, PixelFrame Frame)> selected, CancellationToken token, IProgress<int> rows) {
        using Lock.Scope serialized = gate.EnterScope();
        if (state.Quality.Since > version)
            return Fin.Fail<PixelFrame>(new OidnQualityUnsupported(state.Quality, version));
        nuint bytes = (nuint)color.Block.Length * sizeof(float);
        (nuint size, Seq<(string Role, PixelFrame Frame, nuint Offset)> inputs) = (Role: "color", Frame: color).Cons(selected).Fold(
            (Size: (nuint)0, Inputs: Seq<(string Role, PixelFrame Frame, nuint Offset)>()),
            static (packed, input) => (packed.Size + ((nuint)input.Frame.Block.Length * sizeof(float)), packed.Inputs.Add((input.Role, input.Frame, packed.Size))));
        Seq<(string Role, PixelFrame Frame, nuint Offset)> prefiltered = state.Prefilter is GuidePrefilter.Accurate ? inputs.Tail : [];
        using SafeOidnBufferHandle buffer = oidnNewBuffer(device, size);
        nuint pixelStride = (nuint)Marshal.SizeOf<Vector4>();
        ProgressMonitor monitor = (step, n) => {
            rows.Report((int)((step + n) / (prefiltered.Count + 1) * color.Size.Height));
            return !token.IsCancellationRequested;
        };
        Fin<Unit> Execute(Seq<(string Role, PixelFrame Frame, nuint Offset)> images, (PixelFrame Frame, nuint Offset) target, int step, Seq<(string Name, bool Value)> flags) {
            const int FormatFloat3 = 3;
            using SafeOidnFilterHandle filter = oidnNewFilter(device, "RT");
            return Status(device).Bind(_ => {
                _ = images.Add(("output", target.Frame, target.Offset)).Iter(image => oidnSetFilterImage(
                    filter, image.Role, buffer, FormatFloat3, (nuint)color.Size.Width, (nuint)color.Size.Height,
                    image.Offset + (((nuint)(((long)color.Origin.Y - image.Frame.Origin.Y) * image.Frame.Size.Width) + (nuint)((long)color.Origin.X - image.Frame.Origin.X)) * pixelStride),
                    pixelStride, (nuint)image.Frame.Size.Width * pixelStride));
                _ = flags.Iter(flag => oidnSetFilterBool(filter, flag.Name, flag.Value));
                oidnSetFilterInt(filter, "quality", state.Quality.Value);
                oidnSetFilterProgressMonitorFunction(filter, Marshal.GetFunctionPointerForDelegate(monitor), step);
                return Status(device);
            }).Bind(_ => {
                oidnCommitFilter(filter);
                return Status(device);
            }).Bind(_ => {
                oidnExecuteFilter(filter);
                GC.KeepAlive(monitor);
                return Status(device);
            });
        }
        return (from allocated in Status(device)
                from written in inputs.TraverseM(input => {
                    oidnWriteBuffer(buffer, input.Offset, (nuint)input.Frame.Block.Length * sizeof(float), input.Frame.Block);
                    return Status(device);
                }).As()
                from filtered in prefiltered.Map(static (guide, step) => (Guide: guide, Step: step))
                    .TraverseM(input => Execute([input.Guide], (input.Guide.Frame, input.Guide.Offset), input.Step, [])).As()
                from denoised in Execute(inputs, (color, 0), prefiltered.Count, Seq(("hdr", true), ("cleanAux", inputs.Count > 1 && state.Prefilter is not GuidePrefilter.Fast)))
                let output = new PixelFrame(color.Origin, color.Size, color.Extent, block => oidnReadBuffer(buffer, 0, bytes, block))
                from read in Status(device)
                select output).Map(output => {
                    if (state.Blend == Mix.Full)
                        return output;
                    using SpanOwner<float> weights = SpanOwner<float>.Allocate(color.Size.Width);
                    weights.Span.Fill(state.Blend);
                    for (int line = 0; line < color.Size.Height; line++)
                        BlendingMode.Mix.Mixed(color.View.Span.GetRowSpan(line), output.View.Span.GetRowSpan(line), weights.Span);
                    return output;
                });
    }
}
