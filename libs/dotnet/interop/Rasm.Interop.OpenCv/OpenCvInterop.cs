using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Rasm.Interop.OpenCv;

// --- [OPERATIONS] ----------------------------------------------------------------------
/// <summary>Provides the OpenCV error policy and process-global configuration that executables run once at the composition root</summary>
/// <remarks>
/// <para>No error handler survives a native error on .NET 10. <see cref="CvInvoke.CvErrorHandlerThrowException"/> ends the process when its managed exception escapes the reverse P/Invoke frame, and <see cref="CvInvoke.CvErrorHandlerIgnoreError"/> aborts through libc++abi</para>
/// <para>Callers validate every argument before a native call, and <see cref="Initialize"/> installs the default throw-exception handler to state that policy</para>
/// <para><see cref="CvInvoke.LogLevel"/> works on every platform, and macOS builds schedule through Grand Central Dispatch and ignore <see cref="CvInvoke.NumThreads"/></para>
/// <para>The native library sits in Rasm.Native.EmguCv on macOS and in the Emgu.CV.runtime package for the RID elsewhere</para>
/// <para>Linux x64 publishes the Ubuntu package's non-portable RID ubuntu-x64, and the direct reference supplies UseRidGraph through its build props</para>
/// </remarks>
public static class OpenCvInterop {
    /// <summary>Installs the throw-exception error handler and sets the process-global log level and thread count</summary>
    /// <exception cref="InvalidOperationException">The OpenCV native runtime failed to load</exception>
    public static void Initialize() {
        try {
            _ = CvInvoke.RedirectError(CvInvoke.CvErrorHandlerThrowException, IntPtr.Zero, IntPtr.Zero);
        } catch (TypeInitializationException exception) {
            throw new InvalidOperationException(
                "OpenCV native runtime failed to load. The CLR caches the failed type initializer. Reference the runtime package for this RID and restart",
                exception);
        }
        CvInvoke.LogLevel = LogLevel.Warning;
        CvInvoke.NumThreads = Environment.ProcessorCount;
    }

    /// <summary>Returns the required OpenCV module names absent from the loaded native build</summary>
    /// <remarks><see cref="CvInvoke.OpenCVModuleList"/> returns library names in place of built modules, and a full build omits modules (freetype on macOS)</remarks>
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
