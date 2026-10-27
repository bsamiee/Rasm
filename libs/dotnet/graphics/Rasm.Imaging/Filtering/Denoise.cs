using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Rasm.Imaging.Pixels;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]

namespace Rasm.Imaging.Filtering;

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

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class GuidePrefilter {
    public static readonly GuidePrefilter Off = new("off", prefilters: false, cleans: true);
    public static readonly GuidePrefilter Fast = new("fast", prefilters: false, cleans: false);
    public static readonly GuidePrefilter Accurate = new("accurate", prefilters: true, cleans: true);

    public bool Prefilters { get; }
    public bool Cleans { get; }
}

[SmartEnum<string>]
public sealed partial class DenoiseQuality {
    public static readonly DenoiseQuality High = new("high", 6, 20000);
    public static readonly DenoiseQuality Balanced = new("balanced", 5, 20000);
    public static readonly DenoiseQuality Fast = new("fast", 4, 20300);

    public int Value { get; }
    public int Since { get; }
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

public sealed partial class Denoiser : IDisposable {
    // --- [NATIVE]
    private const string Library = "OpenImageDenoise.2";
    private static readonly PixelExtent Largest = PixelExtent.Validated(65536, 65536).ThrowIfFail();

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
            OidnError.Cancelled => Errors.Cancelled,
            (OidnError.UnsupportedHardware or OidnError.OutOfMemory) and var refused => new ImagingFault($"Open Image Denoise call answering {refused} {Marshal.PtrToStringUTF8(message)}", ImagingReason.Refused),
            (OidnError.Unknown or OidnError.InvalidArgument or OidnError.InvalidOperation) and var failed =>
                throw new InvalidOperationException($"Open Image Denoise failed with {failed}. {Marshal.PtrToStringUTF8(message)}"),
        };

    // --- [LIFECYCLE]
    private readonly SafeOidnDeviceHandle device;
    private readonly Lock gate = new();

    private Denoiser(SafeOidnDeviceHandle device) => this.device = device;

    public static Fin<Denoiser> Open() {
        SafeOidnDeviceHandle created = oidnNewDevice();
        oidnSetDeviceBool(created, "setAffinity", value: false);
        oidnCommitDevice(created);
        return Status(created).MapFail(fault => {
            created.Dispose();
            return fault;
        }).Map(_ => new Denoiser(created));
    }

    public void Dispose() {
        using Lock.Scope serialized = gate.EnterScope();
        device.Dispose();
    }

    // --- [RUN]
    public IO<PixelFrame> Run(PixelFrame color, Option<(PixelFrame Albedo, Option<PixelFrame> Normal)> auxiliary, GuidePrefilter prefilter, DenoiseQuality quality, IProgress<int> rows) =>
        from selected in IO.lift(auxiliary.ToSeq()
            .Bind(static images => (Role: "albedo", Frame: images.Albedo).Cons(images.Normal.Map(static normal => (Role: "normal", Frame: normal)).ToSeq()))
            .Traverse(input => (View: input.Frame.Window, Target: color.Window) switch {
                var (view, target) when input.Frame.Extent == color.Extent && view.Left <= target.Left && view.Top <= target.Top && view.Right >= target.Right && view.Bottom >= target.Bottom =>
                    Fin.Succ(input),
                var (view, target) => new ImagingFault($"denoise {input.Role} guide window {view} against color window {target}", ImagingReason.Mismatch),
            }).As())
        from token in cancelToken
        from result in IO.lift(() => Denoised(color, selected, prefilter, quality, token, rows))
        select result;

    private Fin<PixelFrame> Denoised(PixelFrame color, Seq<(string Role, PixelFrame Frame)> selected, GuidePrefilter prefilter, DenoiseQuality quality, CancellationToken token, IProgress<int> rows) {
        using Lock.Scope serialized = gate.EnterScope();
        int version = oidnGetDeviceInt(device, "version");
        return (ImagingFault.Unless(quality.Since <= version, ImagingReason.Unsupported, (quality.Since, version)),
                ImagingFault.Unless(color.Size.Fitted(Largest) == color.Size, ImagingReason.Oversize, (color.Size, Largest)))
            .Apply(static (_, _) => unit).As().Bind(_ => Filtered(color, selected, prefilter, quality, token, rows));
    }

    private Fin<PixelFrame> Filtered(PixelFrame color, Seq<(string Role, PixelFrame Frame)> selected, GuidePrefilter prefilter, DenoiseQuality quality, CancellationToken token, IProgress<int> rows) {
        nuint bytes = (nuint)color.Block.Length * sizeof(float);
        (nuint size, Seq<(string Role, PixelFrame Frame, nuint Offset)> inputs) = (Role: "color", Frame: color).Cons(selected).Fold(
            (Size: (nuint)0, Inputs: Seq<(string Role, PixelFrame Frame, nuint Offset)>()),
            static (packed, input) => (packed.Size + ((nuint)input.Frame.Block.Length * sizeof(float)), packed.Inputs.Add((input.Role, input.Frame, packed.Size))));
        Seq<(string Role, PixelFrame Frame, nuint Offset)> prefiltered = prefilter.Prefilters ? inputs.Tail : [];
        using SafeOidnBufferHandle buffer = oidnNewBuffer(device, size);
        nuint pixelStride = (nuint)Marshal.SizeOf<Vector4>();
        ProgressMonitor monitor = (step, n) => {
            rows.Report((int)((step + n) / (prefiltered.Count + 1) * color.Size.Height));
            return !token.IsCancellationRequested;
        };
        Fin<Unit> Execute(Seq<(string Role, PixelFrame Frame, nuint Offset)> images, (PixelFrame Frame, nuint Offset) target, int step, Seq<(string Name, bool Value)> flags) {
            const int FormatFloat3 = 3;
            using SafeOidnFilterHandle filter = oidnNewFilter(device, "RT");
            _ = images.Add(("output", target.Frame, target.Offset)).Iter(image => oidnSetFilterImage(
                filter, image.Role, buffer, FormatFloat3, (nuint)color.Size.Width, (nuint)color.Size.Height,
                image.Offset + (((nuint)(((long)color.Origin.Y - image.Frame.Origin.Y) * image.Frame.Size.Width) + (nuint)((long)color.Origin.X - image.Frame.Origin.X)) * pixelStride),
                pixelStride, (nuint)image.Frame.Size.Width * pixelStride));
            _ = flags.Iter(flag => oidnSetFilterBool(filter, flag.Name, flag.Value));
            oidnSetFilterInt(filter, "quality", quality.Value);
            oidnSetFilterProgressMonitorFunction(filter, Marshal.GetFunctionPointerForDelegate(monitor), step);
            oidnCommitFilter(filter);
            oidnExecuteFilter(filter);
            GC.KeepAlive(monitor);
            return Status(device);
        }
        _ = inputs.Iter(input => oidnWriteBuffer(buffer, input.Offset, (nuint)input.Frame.Block.Length * sizeof(float), input.Frame.Block));
        return from written in Status(device)
               from filtered in prefiltered.Map(static (guide, step) => (Guide: guide, Step: step))
                   .TraverseM(input => Execute([input.Guide], (input.Guide.Frame, input.Guide.Offset), input.Step, [])).As()
               from denoised in Execute(inputs, (color, 0), prefiltered.Count, Seq(("hdr", true), ("cleanAux", inputs.Count > 1 && prefilter.Cleans)))
               let output = new PixelFrame(color.Origin, color.Size, color.Extent, block => oidnReadBuffer(buffer, 0, bytes, block))
               from read in Status(device)
               select output;
    }
}
