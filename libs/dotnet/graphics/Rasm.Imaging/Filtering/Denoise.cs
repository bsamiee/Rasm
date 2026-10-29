using System.Numerics;
using System.Runtime.CompilerServices;
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
[SmartEnum<int>]
public sealed partial class DenoiseQuality {
    public static readonly DenoiseQuality High = new(6);
    public static readonly DenoiseQuality Balanced = new(5);
    public static readonly DenoiseQuality Fast = new(4);
}

public sealed record DenoiseGuides(PixelFrame Albedo, Option<PixelFrame> Normal, bool Clean);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed partial class Denoiser : IDisposable {
    // --- [NATIVE]
    private const string Library = "OpenImageDenoise.2";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool ProgressMonitor(nint userPtr, double n);

    private abstract class Handle(Action<nint> release) : SafeHandleZeroOrMinusOneIsInvalid(ownsHandle: true) {
        protected override bool ReleaseHandle() {
            release(handle);
            return true;
        }
    }

    private sealed class DeviceHandle() : Handle(oidnReleaseDevice);
    private sealed class FilterHandle() : Handle(oidnReleaseFilter);
    private sealed class BufferHandle() : Handle(oidnReleaseBuffer);

    [LibraryImport(Library)] private static partial DeviceHandle oidnNewDevice(int type = 0);
    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)] private static partial void oidnSetDeviceInt(DeviceHandle device, string name, int value);
    [LibraryImport(Library)] private static partial void oidnCommitDevice(DeviceHandle device);
    [LibraryImport(Library)] private static partial OidnError oidnGetDeviceError(DeviceHandle device, out nint outMessage);
    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)] private static partial FilterHandle oidnNewFilter(DeviceHandle device, string type);
    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void oidnSetFilterImage(
        FilterHandle filter, string name, BufferHandle buffer, int format, nuint width, nuint height, nuint byteOffset, nuint pixelByteStride, nuint rowByteStride);
    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)] private static partial void oidnSetFilterInt(FilterHandle filter, string name, int value);
    [LibraryImport(Library)] private static partial void oidnSetFilterProgressMonitorFunction(FilterHandle filter, nint func, nint userPtr);
    [LibraryImport(Library)] private static partial void oidnCommitFilter(FilterHandle filter);
    [LibraryImport(Library)] private static partial void oidnExecuteFilter(FilterHandle filter);
    [LibraryImport(Library)] private static partial BufferHandle oidnNewBuffer(DeviceHandle device, nuint byteSize);
    [LibraryImport(Library)] private static partial void oidnWriteBuffer(BufferHandle buffer, nuint byteOffset, nuint byteSize, ReadOnlySpan<float> srcHostPtr);
    [LibraryImport(Library)] private static partial void oidnReadBuffer(BufferHandle buffer, nuint byteOffset, nuint byteSize, Span<float> dstHostPtr);
    [LibraryImport(Library)] private static partial void oidnReleaseDevice(nint device);
    [LibraryImport(Library)] private static partial void oidnReleaseFilter(nint filter);
    [LibraryImport(Library)] private static partial void oidnReleaseBuffer(nint buffer);

    private static Fin<Unit> Status(DeviceHandle device) =>
        oidnGetDeviceError(device, out nint message) switch {
            OidnError.None => unit,
            OidnError.Cancelled => Errors.Cancelled,
            (OidnError.InvalidArgument or OidnError.OutOfMemory or OidnError.UnsupportedHardware) and var error => ImagingFault.Of(ImagingReason.Refused, (error, Marshal.PtrToStringUTF8(message))),
            (OidnError.Unknown or OidnError.InvalidOperation) and var error => throw new InvalidOperationException($"{error}: {Marshal.PtrToStringUTF8(message)}"),
        };

    // --- [LIFECYCLE]
    private readonly DeviceHandle device;

    private Denoiser(DeviceHandle device) => this.device = device;

    public static Fin<Denoiser> Open() {
        DeviceHandle created = oidnNewDevice();
        oidnSetDeviceInt(created, "setAffinity", 0);
        oidnCommitDevice(created);
        return Status(created).Map(_ => new Denoiser(created)).MapFail(fault => {
            created.Dispose();
            return fault;
        });
    }

    public void Dispose() => device.Dispose();

    // --- [RUN]
    public IO<PixelFrame> Run(PixelFrame color, Option<DenoiseGuides> guides, DenoiseQuality quality, Option<IProgress<int>> progress = default) =>
        Denoised("RT", ("color", color), guides.ToSeq().Bind(static set => ("albedo", set.Albedo).Cons(set.Normal.Map(static normal => ("normal", normal)).ToSeq())),
            [("hdr", 1), ("cleanAux", guides.Exists(static set => set.Clean) ? 1 : 0)], quality, progress);

    public IO<PixelFrame> Lightmap(PixelFrame color, bool directional, DenoiseQuality quality, Option<IProgress<int>> progress = default) =>
        Denoised("RTLightmap", ("color", color), [], [("directional", directional ? 1 : 0)], quality, progress);

    public IO<DenoiseGuides> Prefiltered(DenoiseGuides guides, DenoiseQuality quality, Option<IProgress<int>> progress = default) =>
        from albedo in Denoised("RT", ("albedo", guides.Albedo), [], [], quality, progress)
        from normal in guides.Normal.Traverse(normal => Denoised("RT", ("normal", normal), [], [], quality, progress)).As()
        select new DenoiseGuides(albedo, normal, Clean: true);

    private IO<PixelFrame> Denoised(string type, (string Role, PixelFrame Frame) main, Seq<(string Role, PixelFrame Frame)> guides, Seq<(string Name, int Value)> parameters, DenoiseQuality quality, Option<IProgress<int>> progress) =>
        from covered in IO.lift(guides.Map((guide, index) => ImagingFault.Unless(
            guide.Frame.Extent == main.Frame.Extent && guide.Frame.Window.Contains(main.Frame.Window), ImagingReason.Mismatch, (guide.Frame.Window, main.Frame.Window), index)).Traverse(identity).As())
        from token in cancelToken
        from output in IO.lift(() => {
            const int Float3 = 3;
            nuint stride = (nuint)Unsafe.SizeOf<Vector4>();
            PixelFrame target = main.Frame;
            (nuint size, Seq<(string Role, PixelFrame Frame, nuint Offset)> images) = main.Cons(guides).Fold((Size: (nuint)0, Images: Seq<(string Role, PixelFrame Frame, nuint Offset)>()),
                static (packed, image) => (packed.Size + Bytes(image.Frame), packed.Images.Add((image.Role, image.Frame, packed.Size))));
            using BufferHandle buffer = oidnNewBuffer(device, size);
            using FilterHandle filter = oidnNewFilter(device, type);
            ProgressMonitor monitor = (_, n) => {
                if (progress.Case is IProgress<int> report)
                    report.Report((int)(n * target.Size.Height));
                return !token.IsCancellationRequested;
            };
            _ = images.Iter(image => oidnWriteBuffer(buffer, image.Offset, Bytes(image.Frame), image.Frame.Block));
            _ = images.Add(("output", target, 0)).Iter(image => oidnSetFilterImage(
                filter, image.Role, buffer, Float3, (nuint)target.Size.Width, (nuint)target.Size.Height,
                image.Offset + ((((nuint)(target.Origin.Y - image.Frame.Origin.Y) * (nuint)image.Frame.Size.Width) + (nuint)(target.Origin.X - image.Frame.Origin.X)) * stride),
                stride, (nuint)image.Frame.Size.Width * stride));
            _ = parameters.Add(("quality", quality.Key)).Iter(parameter => oidnSetFilterInt(filter, parameter.Name, parameter.Value));
            oidnSetFilterProgressMonitorFunction(filter, Marshal.GetFunctionPointerForDelegate(monitor), 0);
            oidnCommitFilter(filter);
            oidnExecuteFilter(filter);
            GC.KeepAlive(monitor);
            PixelFrame denoised = new(target.Origin, target.Size, target.Extent, block => oidnReadBuffer(buffer, 0, Bytes(target), block));
            return Status(device).Map(_ => denoised);
        })
        select output;

    private static nuint Bytes(PixelFrame frame) => (nuint)frame.Block.Length * sizeof(float);
}
