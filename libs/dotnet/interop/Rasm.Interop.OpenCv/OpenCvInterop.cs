using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Rasm.Interop.OpenCv;

/// <summary>OpenCV error policy and process-global configuration for executables to call once at the composition root</summary>
/// <remarks>
/// <para>No native error handler survives a native error on .NET 10: the default <see cref="CvInvoke.CvErrorHandlerThrowException"/> raises a managed exception that escapes the reverse P/Invoke frame and terminates the process, and <see cref="CvInvoke.CvErrorHandlerIgnoreError"/> aborts through libc++abi. Callers prove every argument valid before a native call, and <see cref="Initialize"/> installs the throw-exception handler only to make that policy explicit</para>
/// <para><see cref="CvInvoke.LogLevel"/> works on every platform, and macOS builds schedule through Grand Central Dispatch and ignore <see cref="CvInvoke.NumThreads"/>. The CLR caches a failed type initializer, a failed <see cref="Initialize"/> stays failed for the process lifetime until a runtime package reference and a restart</para>
/// <para><see cref="MissingModules"/> reads the "To be built:" and "Disabled:" lines of <see cref="CvInvoke.BuildInformation"/> because OpenCVModuleList returns library names rather than built modules and even full builds omit modules, for example freetype on macOS</para>
/// <para>The facade references Emgu.CV alone, the native library sits in Rasm.Native.EmguCv on macOS, a full build with the contrib modules, and in the runtime packages Emgu.CV.runtime.windows, Emgu.CV.runtime.ubuntu-26.04-x64, and Emgu.CV.runtime.debian-trixie-arm64 elsewhere, each referenced by the executables that publish for its RID. Linux x64 publishes RID ubuntu-x64, the Ubuntu package's non-portable RID, and the direct reference supplies UseRidGraph through the package's own build props</para>
/// </remarks>
public static class OpenCvInterop {
    /// <summary>Installs the throw-exception error handler and sets the process-global log level and thread count</summary>
    public static void Initialize() {
        try {
            _ = CvInvoke.RedirectError(CvInvoke.CvErrorHandlerThrowException, IntPtr.Zero, IntPtr.Zero);
        } catch (TypeInitializationException exception) {
            throw new InvalidOperationException(
                "OpenCV native runtime failed to load. The CLR caches the failed type initializer, reference the runtime package for this RID and restart",
                exception);
        }
        CvInvoke.LogLevel = LogLevel.Warning;
        CvInvoke.NumThreads = Environment.ProcessorCount;
    }

    /// <summary>Returns the required OpenCV module names absent from the loaded native build</summary>
    public static ImmutableSortedSet<string> MissingModules(params ImmutableArray<string> requiredModules) {
        ImmutableHashSet<string> present = PresentModules(CvInvoke.BuildInformation);
        return requiredModules.Where(module => !present.Contains(module)).ToImmutableSortedSet(StringComparer.Ordinal);
    }

    /// <summary>Parses built module names from the build information lines</summary>
    private static ImmutableHashSet<string> PresentModules(string buildInformation) {
        ImmutableArray<string> lines = [.. buildInformation.Split('\n', StringSplitOptions.TrimEntries)];
        return ModuleNames(lines, "To be built:")
            .Except(ModuleNames(lines, "Disabled:"), StringComparer.Ordinal)
            .ToImmutableHashSet(StringComparer.Ordinal);
    }

    /// <summary>Splits the module names listed after a labeled build information line</summary>
    private static IEnumerable<string> ModuleNames(ImmutableArray<string> lines, string label) =>
        lines.Where(line => line.StartsWith(label, StringComparison.Ordinal))
            .Take(1)
            .SelectMany(line => line[label.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
