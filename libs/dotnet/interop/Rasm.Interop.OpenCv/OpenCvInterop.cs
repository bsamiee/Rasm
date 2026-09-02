using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Rasm.Interop.OpenCv;

/// <summary>OpenCV error policy and process-global configuration; executables call <see cref="Initialize"/> once at the composition root</summary>
/// <remarks>
/// <para>No native error handler survives a native error on .NET 10: the default <see cref="CvInvoke.CvErrorHandlerThrowException"/> raises a managed exception
/// that escapes the reverse P/Invoke frame and terminates the process, and <see cref="CvInvoke.CvErrorHandlerIgnoreError"/> aborts through libc++abi, the
/// facade contract is managed pre-validation: callers prove every argument valid before a native call; <see cref="Initialize"/> installs the throw-exception
/// handler only to make that policy explicit, sets <see cref="CvInvoke.LogLevel"/>, which works on every platform, and sets <see cref="CvInvoke.NumThreads"/>,
/// a no-op on macOS builds that schedule through Grand Central Dispatch; the CLR caches a failed type initializer, unlike the other facades a failed
/// <see cref="Initialize"/> stays failed for the process lifetime and the remedy is referencing the runtime package and restarting</para>
/// <para><see cref="MissingModules"/> answers capability questions from the "To be built:" and "Disabled:" lines of <see cref="CvInvoke.BuildInformation"/>,
/// because OpenCVModuleList returns library names rather than built modules, and even the full builds omit modules, such as freetype on macOS; the facade
/// references Emgu.CV alone because the native library travels in Rasm.Native.EmguCv on macOS, a full build with the contrib modules, and in the full
/// runtime packages Emgu.CV.runtime.windows, Emgu.CV.runtime.ubuntu-26.04-x64, and Emgu.CV.runtime.debian-trixie-arm64 elsewhere, referenced by each
/// executable to match the RIDs it publishes for; Linux x64 publishes RID ubuntu-x64, the Ubuntu package's non-portable RID, with the direct reference
/// supplying UseRidGraph through the package's own build props</para>
/// </remarks>
public static class OpenCvInterop {
    /// <summary>Installs the throw-exception error handler and sets the process-global log level and thread count</summary>
    public static void Initialize() {
        try {
            _ = CvInvoke.RedirectError(CvInvoke.CvErrorHandlerThrowException, IntPtr.Zero, IntPtr.Zero);
        } catch (TypeInitializationException exception) {
            throw new InvalidOperationException(
                "OpenCV native runtime failed to load; reference the OpenCV native runtime package for this RID and restart, the CLR caches the failed type initializer",
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

    /// <summary>Splits the module names listed after one labeled build information line</summary>
    private static IEnumerable<string> ModuleNames(ImmutableArray<string> lines, string label) =>
        lines.Where(line => line.StartsWith(label, StringComparison.Ordinal))
            .Take(1)
            .SelectMany(line => line[label.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
