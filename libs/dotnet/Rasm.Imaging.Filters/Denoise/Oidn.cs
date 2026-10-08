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
    public static readonly DenoiseQuality High = new("high", 6);
    public static readonly DenoiseQuality Balanced = new("balanced", 5);
    public static readonly DenoiseQuality Fast = new("fast", 4);

    internal int Value { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidDenoise>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GuidePrefilter {
    public static readonly GuidePrefilter Off = new("none");
    public static readonly GuidePrefilter Fast = new("fast");
    public static readonly GuidePrefilter Accurate = new("accurate");
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
    public static readonly DenoiseParameter Prefilter = new("prefilter", new StateParameter<DenoiseState>.Choice<GuidePrefilter, InvalidDenoise>(
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

internal sealed class SafeOidnDeviceHandle() : SafeOidnHandle(NativeMethods.oidnReleaseDevice);
internal sealed class SafeOidnFilterHandle() : SafeOidnHandle(NativeMethods.oidnReleaseFilter);
internal sealed class SafeOidnBufferHandle() : SafeOidnHandle(NativeMethods.oidnReleaseBuffer);

internal static partial class NativeMethods {
    internal const int DeviceTypeDefault = 0;
    internal const int FormatFloat3 = 3;
    private const string Library = "OpenImageDenoise.2";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool ProgressMonitor(nint userPtr, double n);

    [LibraryImport(Library)]
    internal static partial SafeOidnDeviceHandle oidnNewDevice(int type);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void oidnSetDeviceBool(SafeOidnDeviceHandle device, string name, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(Library)]
    internal static partial void oidnCommitDevice(SafeOidnDeviceHandle device);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int oidnGetDeviceInt(SafeOidnDeviceHandle device, string name);

    [LibraryImport(Library)]
    internal static partial OidnError oidnGetDeviceError(SafeOidnDeviceHandle device, out nint outMessage);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial SafeOidnFilterHandle oidnNewFilter(SafeOidnDeviceHandle device, string type);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void oidnSetFilterImage(
        SafeOidnFilterHandle filter, string name, SafeOidnBufferHandle buffer, int format,
        nuint width, nuint height, nuint byteOffset, nuint pixelByteStride, nuint rowByteStride);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void oidnUnsetFilterImage(SafeOidnFilterHandle filter, string name);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void oidnSetFilterBool(SafeOidnFilterHandle filter, string name, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void oidnSetFilterInt(SafeOidnFilterHandle filter, string name, int value);

    [LibraryImport(Library)]
    internal static partial void oidnSetFilterProgressMonitorFunction(SafeOidnFilterHandle filter, nint func, nint userPtr);

    [LibraryImport(Library)]
    internal static partial void oidnCommitFilter(SafeOidnFilterHandle filter);

    [LibraryImport(Library)]
    internal static partial void oidnExecuteFilter(SafeOidnFilterHandle filter);

    [LibraryImport(Library)]
    internal static partial SafeOidnBufferHandle oidnNewBuffer(SafeOidnDeviceHandle device, nuint byteSize);

    [LibraryImport(Library)]
    internal static partial nint oidnGetBufferData(SafeOidnBufferHandle buffer);

    [LibraryImport(Library)]
    internal static partial void oidnReleaseDevice(nint device);

    [LibraryImport(Library)]
    internal static partial void oidnReleaseFilter(nint filter);

    [LibraryImport(Library)]
    internal static partial void oidnReleaseBuffer(nint buffer);
}

public sealed class Denoiser : IFrameJob<Denoiser, DenoiseState> {
    private readonly SafeOidnDeviceHandle device;
    private readonly int version;
    private readonly HashMap<Option<GuideChannel>, (string Name, SafeOidnFilterHandle Handle)> filters;
    private readonly Lock gate = new();

    private Denoiser(SafeOidnDeviceHandle device, HashMap<Option<GuideChannel>, (string Name, SafeOidnFilterHandle Handle)> filters) =>
        (this.device, version, this.filters) = (device, NativeMethods.oidnGetDeviceInt(device, "version"), filters);

    public static IO<Denoiser> Open() =>
        (from device in use(static () => NativeMethods.oidnNewDevice(NativeMethods.DeviceTypeDefault))
         from created in IO.lift(() => Status(device))
         from committed in IO.lift(() => {
             NativeMethods.oidnSetDeviceBool(device, "setAffinity", value: false);
             NativeMethods.oidnCommitDevice(device);
             return Status(device);
         })
         from filters in Seq<(Option<GuideChannel> Channel, string Name)>((None, "color"), (Some(GuideChannel.Albedo), "albedo"), (Some(GuideChannel.Normal), "normal"))
             .TraverseM(role => from filter in use(() => NativeMethods.oidnNewFilter(device, "RT"))
                                from allocated in IO.lift(() => Status(device))
                                select (role.Channel, (role.Name, filter))).As()
         select new Denoiser(device, toHashMap(filters))).BracketFail();

    public static Seq<GuideChannel> Channels(DenoiseState state) => state.Guides.Channels;

    public unsafe IO<PixelFrame> Run(DenoiseState state, PixelFrame color, HashMap<GuideChannel, PixelFrame> guides, IProgress<int> rows) => IO.lift(() => {
        lock (gate) {
            const int fastSince = 20300;
            if (state.Quality == DenoiseQuality.Fast && version < fastSince)
                return Fin.Fail<PixelFrame>(new OidnQualityUnsupported(state.Quality, version));
            nuint bytes = (nuint)color.Block.Length * sizeof(float);
            Seq<(Option<GuideChannel> Key, string Name, SafeOidnFilterHandle Handle, PixelFrame Frame, nuint Offset)> inputs = filters.AsIterable().ToSeq().Bind(role => role.Key.Match(Some: guides.Find, None: () => Some(color))
                .Map(frame => (role.Key, role.Value.Name, role.Value.Handle, Frame: frame)).ToSeq())
                .Map((input, index) => (input.Key, input.Name, input.Handle, input.Frame, Offset: (nuint)index * bytes));
            using SafeOidnBufferHandle buffer = NativeMethods.oidnNewBuffer(device, (nuint)inputs.Count * bytes);
            SafeOidnFilterHandle main = filters[None].Handle;
            return from allocated in Status(device)
                   let executions = inputs.Filter(input => input.Key.IsSome && state.Prefilter == GuidePrefilter.Accurate)
                       .Concat(inputs.Filter(static input => input.Key.IsNone))
                   let data = NativeMethods.oidnGetBufferData(buffer)
                   let written = inputs.Iter(input => input.Frame.Block.CopyTo(new Span<float>((void*)(data + (nint)input.Offset), input.Frame.Block.Length)))
                   from configured in filters.AsIterable().ToSeq().TraverseM(role => {
                       _ = inputs.Find(input => input.Key == role.Key).Match(
                           Some: input => Image(main, role.Value.Name, buffer, color.Size, input.Offset),
                           None: () => NativeMethods.oidnUnsetFilterImage(main, role.Value.Name));
                       return Status(device);
                   }).As()
                   from denoised in executions.Map(static (input, step) => (Input: input, Step: step)).TraverseM(work => {
                       ((Option<GuideChannel> Key, string Name, SafeOidnFilterHandle Handle, PixelFrame Frame, nuint Offset) input, int step) = work;
                       Image(input.Handle, input.Name, buffer, color.Size, input.Offset);
                       Image(input.Handle, "output", buffer, color.Size, input.Offset);
                       NativeMethods.oidnSetFilterInt(input.Handle, "quality", state.Quality.Value);
                       NativeMethods.oidnSetFilterBool(main, "hdr", value: true);
                       NativeMethods.oidnSetFilterBool(main, "cleanAux", state.Prefilter != GuidePrefilter.Fast);
                       NativeMethods.ProgressMonitor monitor = (_, n) => {
                           rows.Report((int)((step + n) / executions.Count * color.Size.Height));
                           return true;
                       };
                       NativeMethods.oidnSetFilterProgressMonitorFunction(input.Handle, Marshal.GetFunctionPointerForDelegate(monitor), 0);
                       NativeMethods.oidnCommitFilter(input.Handle);
                       return Status(device).Bind(_ => {
                           NativeMethods.oidnExecuteFilter(input.Handle);
                           GC.KeepAlive(monitor);
                           return Status(device);
                       });
                   }).As()
                   let offsets = toHashMap(inputs.Map(static input => (input.Key, input.Offset)))
                   let answer = new PixelFrame(color.Origin, color.Size, color.Extent, block =>
                       new ReadOnlySpan<float>((void*)(data + (nint)offsets[None]), color.Block.Length).CopyTo(block))
                   select Blended(answer, color, state.Blend);
        }
    });

    public void Dispose() {
        lock (gate) {
            foreach (SafeOidnFilterHandle filter in filters.Values.Map(static role => role.Handle))
                filter.Dispose();
            device.Dispose();
        }
    }

    private static Fin<Unit> Status(SafeOidnDeviceHandle device) =>
        NativeMethods.oidnGetDeviceError(device, out nint message) switch {
            OidnError.None => unit,
            OidnError.Unknown => new OidnUnknown(),
            OidnError.OutOfMemory => new OidnOutOfMemory(),
            OidnError.UnsupportedHardware => new OidnUnsupportedHardware(),
            OidnError.InvalidArgument => new OidnInvalidArgument(),
            OidnError.InvalidOperation or OidnError.Cancelled => throw new InvalidOperationException(Marshal.PtrToStringUTF8(message)),
        };

    private static void Image(SafeOidnFilterHandle filter, string name, SafeOidnBufferHandle buffer, PixelExtent size, nuint offset) =>
        NativeMethods.oidnSetFilterImage(filter, name, buffer, NativeMethods.FormatFloat3, (nuint)size.Width, (nuint)size.Height, offset, (nuint)Unsafe.SizeOf<Vector4>(), 0);

    private static PixelFrame Blended(PixelFrame answer, PixelFrame color, Mix blend) {
        if (blend == Mix.Full)
            return answer;
        using Mat noisy = new(color.Size.Width * color.Size.Height, 3, DepthType.Cv32F, 1, color.Address, Unsafe.SizeOf<Vector4>());
        using Mat denoised = new(noisy.Rows, noisy.Cols, noisy.Depth, noisy.NumberOfChannels, answer.Address, noisy.Step);
        CvInvoke.AddWeighted(noisy, 1f - blend, denoised, blend, 0, denoised);
        return answer;
    }
}
