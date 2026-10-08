using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
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
    private const int FormatFloat3 = 3;
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
    private static partial void oidnUnsetFilterImage(SafeOidnFilterHandle filter, string name);

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
    private static partial void oidnWriteBuffer(SafeOidnBufferHandle buffer, nuint byteOffset, nuint byteSize, nint srcHostPtr);

    [LibraryImport(Library)]
    private static partial void oidnReadBuffer(SafeOidnBufferHandle buffer, nuint byteOffset, nuint byteSize, nint dstHostPtr);

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
            OidnError.InvalidArgument => new OidnInvalidArgument(),
            OidnError.InvalidOperation or OidnError.Cancelled => throw new InvalidOperationException(Marshal.PtrToStringUTF8(message)),
        };

    private static void Image(SafeOidnFilterHandle filter, string name, SafeOidnBufferHandle buffer, PixelExtent size, nuint offset) =>
        oidnSetFilterImage(filter, name, buffer, FormatFloat3, (nuint)size.Width, (nuint)size.Height, offset, (nuint)Unsafe.SizeOf<Vector4>(), 0);

    // --- [LIFECYCLE]
    private readonly SafeOidnDeviceHandle device;
    private readonly SafeOidnFilterHandle filter;
    private readonly Seq<(GuideChannel Channel, string Name, SafeOidnFilterHandle Handle)> guideFilters;
    private readonly Lock gate = new();

    private Denoiser(SafeOidnDeviceHandle device, SafeOidnFilterHandle filter, Seq<(GuideChannel Channel, string Name, SafeOidnFilterHandle Handle)> guideFilters) =>
        (this.device, this.filter, this.guideFilters) = (device, filter, guideFilters);

    public static IO<Denoiser> Open() =>
        (from device in use(static () => oidnNewDevice())
         from created in IO.lift(() => Status(device))
         from committed in IO.lift(() => {
             oidnSetDeviceBool(device, "setAffinity", value: false);
             oidnCommitDevice(device);
             return Status(device);
         })
         from filter in Filter(device)
         from hdr in IO.lift(() => oidnSetFilterBool(filter, "hdr", value: true))
         from guides in Seq((Channel: GuideChannel.Albedo, Name: "albedo"), (Channel: GuideChannel.Normal, Name: "normal"))
             .TraverseM(role => Filter(device).Map(handle => (role.Channel, role.Name, handle))).As()
         select new Denoiser(device, filter, guides)).BracketFail();

    public void Dispose() {
        lock (gate) {
            foreach (SafeOidnFilterHandle guide in guideFilters.Map(static role => role.Handle))
                guide.Dispose();
            filter.Dispose();
            device.Dispose();
        }
    }

    private static IO<SafeOidnFilterHandle> Filter(SafeOidnDeviceHandle device) =>
        from filter in use(() => oidnNewFilter(device, "RT"))
        from allocated in IO.lift(() => Status(device))
        select filter;

    // --- [RUN]
    public static Seq<GuideChannel> Channels(DenoiseState state) => state.Guides.Channels;

    public IO<PixelFrame> Run(DenoiseState state, PixelFrame color, HashMap<GuideChannel, PixelFrame> guides, IProgress<int> rows) => IO.lift(() => {
        lock (gate) {
            int version = oidnGetDeviceInt(device, "version");
            if (state.Quality.Since > version)
                return Fin.Fail<PixelFrame>(new OidnQualityUnsupported(state.Quality, version));
            nuint bytes = (nuint)color.Block.Length * sizeof(float);
            (string Name, SafeOidnFilterHandle Handle, PixelFrame Frame, nuint Offset) beauty = ("color", filter, color, 0);
            Seq<(string Name, SafeOidnFilterHandle Handle, PixelFrame Frame, nuint Offset)> auxiliaries = guideFilters
                .Choose(role => guides.Find(role.Channel).Map(frame => (role.Name, role.Handle, Frame: frame)))
                .Map((input, index) => (input.Name, input.Handle, input.Frame, Offset: (nuint)(index + 1) * bytes));
            Seq<(string Name, SafeOidnFilterHandle Handle, PixelFrame Frame, nuint Offset)> inputs = auxiliaries.Add(beauty);
            using SafeOidnBufferHandle buffer = oidnNewBuffer(device, (nuint)inputs.Count * bytes);
            oidnSetFilterBool(filter, "cleanAux", state.Prefilter != GuidePrefilter.Fast);
            return (from allocated in Status(device)
                    let executions = (state.Prefilter == GuidePrefilter.Accurate ? auxiliaries : []).Add(beauty)
                    from written in inputs.TraverseM(input => {
                        oidnWriteBuffer(buffer, input.Offset, bytes, input.Frame.Address);
                        GC.KeepAlive(input.Frame);
                        return Status(device);
                    }).As()
                    let configured = guideFilters.Iter(role => auxiliaries.Find(input => input.Handle == role.Handle).Match(
                        Some: input => Image(filter, role.Name, buffer, color.Size, input.Offset),
                        None: () => oidnUnsetFilterImage(filter, role.Name)))
                    let monitor = (ProgressMonitor)((step, n) => {
                        rows.Report((int)((step + n) / executions.Count * color.Size.Height));
                        return true;
                    })
                    from denoised in executions.Map(static (input, step) => (Input: input, Step: step)).TraverseM(work => {
                        (string name, SafeOidnFilterHandle handle, PixelFrame _, nuint offset) = work.Input;
                        Image(handle, name, buffer, color.Size, offset);
                        Image(handle, "output", buffer, color.Size, offset);
                        oidnSetFilterInt(handle, "quality", state.Quality.Value);
                        oidnSetFilterProgressMonitorFunction(handle, Marshal.GetFunctionPointerForDelegate(monitor), work.Step);
                        oidnCommitFilter(handle);
                        return Status(device).Bind(_ => {
                            oidnExecuteFilter(handle);
                            GC.KeepAlive(monitor);
                            return Status(device);
                        });
                    }).As()
                    let output = new PixelFrame(color.Origin, color.Size, color.Extent, block => {
                        oidnReadBuffer(buffer, beauty.Offset, bytes, Marshal.UnsafeAddrOfPinnedArrayElement(block, 0));
                        GC.KeepAlive(block);
                    })
                    from read in Status(device)
                    select output).Map(output => {
                        if (state.Blend == Mix.Full)
                            return output;
                        using Mat noisy = new(color.Size.Width * color.Size.Height, 3, DepthType.Cv32F, 1, color.Address, Unsafe.SizeOf<Vector4>());
                        using Mat denoised = new(noisy.Rows, noisy.Cols, noisy.Depth, noisy.NumberOfChannels, output.Address, noisy.Step);
                        CvInvoke.AddWeighted(noisy, 1f - state.Blend, denoised, state.Blend, 0, denoised);
                        GC.KeepAlive(color);
                        return output;
                    });
        }
    });
}
