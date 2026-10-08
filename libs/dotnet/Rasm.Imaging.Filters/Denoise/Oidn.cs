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
    None,
    Unknown,
    InvalidArgument,
    InvalidOperation,
    OutOfMemory,
    UnsupportedHardware,
    Cancelled,
}

internal enum OidnFormat {
    Float3 = 3,
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
    public static readonly DenoiseParameter Blend = new("blend", new StateParameter<DenoiseState>.Bounded<Mix, float, InvalidGrade>(
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
        SafeOidnFilterHandle filter, string name, SafeOidnBufferHandle buffer, OidnFormat format,
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
            OidnError.InvalidArgument or OidnError.InvalidOperation or OidnError.Cancelled => throw new InvalidOperationException(Marshal.PtrToStringUTF8(message)),
        };

    // --- [LIFECYCLE]
    private readonly SafeOidnDeviceHandle device;
    private readonly Lock gate = new();

    private Denoiser(SafeOidnDeviceHandle device) => this.device = device;

    public static IO<Denoiser> Open() =>
        use(static () => oidnNewDevice()).Bind(static device => IO.lift(() => {
            oidnSetDeviceBool(device, "setAffinity", value: false);
            oidnCommitDevice(device);
            return Status(device).Map(_ => new Denoiser(device));
        })).BracketFail();

    public void Dispose() => device.Dispose();

    // --- [RUN]
    public static Seq<GuideChannel> Channels(DenoiseState state) => state.Guides.Channels;

    public IO<PixelFrame> Run(DenoiseState state, PixelFrame color, HashMap<GuideChannel, PixelFrame> guides, IProgress<int> rows) => IO.lift(() => {
        using Lock.Scope serialized = gate.EnterScope();
        int version = oidnGetDeviceInt(device, "version");
        if (state.Quality.Since > version)
            return Fin.Fail<PixelFrame>(new OidnQualityUnsupported(state.Quality, version));
        nuint bytes = (nuint)color.Block.Length * sizeof(float);
        Seq<(string Role, PixelFrame Frame, nuint Offset)> inputs = Seq<(string Role, Option<PixelFrame> Frame)>(("color", color), ("albedo", guides.Find(GuideChannel.Albedo)), ("normal", guides.Find(GuideChannel.Normal)))
            .Choose(static input => input.Frame.Map(frame => (input.Role, frame)))
            .Map((input, index) => (input.Role, input.frame, (nuint)index * bytes));
        Seq<(string Role, PixelFrame Frame, nuint Offset)> prefiltered = state.Prefilter == GuidePrefilter.Accurate ? inputs.Tail : [];
        using SafeOidnBufferHandle buffer = oidnNewBuffer(device, (nuint)inputs.Count * bytes);
        ProgressMonitor monitor = (step, n) => {
            rows.Report((int)((step + n) / (prefiltered.Count + 1) * color.Size.Height));
            return true;
        };
        void Execute(Seq<(string Role, PixelFrame Frame, nuint Offset)> images, nuint target, int step, Seq<(string Name, bool Value)> flags) {
            using SafeOidnFilterHandle filter = oidnNewFilter(device, "RT");
            _ = images.Map(static image => (image.Role, image.Offset)).Add(("output", target)).Iter(image => oidnSetFilterImage(
                filter, image.Role, buffer, OidnFormat.Float3, (nuint)color.Size.Width, (nuint)color.Size.Height, image.Offset, (nuint)Marshal.SizeOf<Vector4>(), 0));
            _ = flags.Iter(flag => oidnSetFilterBool(filter, flag.Name, flag.Value));
            oidnSetFilterInt(filter, "quality", state.Quality.Value);
            oidnSetFilterProgressMonitorFunction(filter, Marshal.GetFunctionPointerForDelegate(monitor), step);
            oidnCommitFilter(filter);
            oidnExecuteFilter(filter);
        }
        _ = inputs.Iter(input => oidnWriteBuffer(buffer, input.Offset, bytes, input.Frame.Block));
        _ = prefiltered.Iter((step, guide) => Execute([guide], guide.Offset, step, []));
        Execute(inputs, 0, prefiltered.Count, Seq(("hdr", true), ("cleanAux", state.Prefilter != GuidePrefilter.Fast)));
        GC.KeepAlive(monitor);
        PixelFrame output = new(color.Origin, color.Size, color.Extent, block => {
            oidnReadBuffer(buffer, 0, bytes, block);
            if (state.Blend == Mix.Full)
                return;
            using SpanOwner<float> weights = SpanOwner<float>.Allocate(color.Size.Width * color.Size.Height);
            weights.Span.Fill(state.Blend);
            BlendingMode.Mix.Mixed(MemoryMarshal.Cast<float, Vector4>(color.Block), MemoryMarshal.Cast<float, Vector4>(block.AsSpan()), weights.Span);
        });
        return Status(device).Map(_ => output);
    });
}
