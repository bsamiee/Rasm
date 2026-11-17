using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using AppKit;
using Foundation;
using ObjCRuntime;
using Rhino;
using Xunit;
using Xunit.MicrosoftTestingPlatform;

[assembly: SupportedOSPlatform("macos")]

namespace Rasm.Rhino.Tests.Host;

// --- [COMPOSITION] ---------------------------------------------------------------------

public static unsafe partial class RhinoProcess {

    // --- [BUNDLE]

    internal static readonly string Frameworks = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory() switch { var runtime => runtime[..runtime.IndexOf("/RhCore.framework/", StringComparison.Ordinal)] };
    internal static readonly string Resources = Path.Combine(Frameworks, "RhCore.framework", "Resources");

    [ModuleInitializer]
    internal static void Resolving() => AssemblyLoadContext.Default.Resolving += static (context, name) =>
        Directory.EnumerateDirectories(Path.Combine(Resources, "ManagedPlugIns"), "*.rhp").Prepend(Resources)
            .Select(folder => Path.Combine(folder, $"{name.Name}.dll"))
            .Where(File.Exists)
            .Select(context.LoadFromAssemblyPath)
            .FirstOrDefault();

    // --- [PROCESS]

    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    private const string Objective = "/usr/lib/libobjc.A.dylib";
    private const int Lazy = 0x1;
    private const int Global = 0x8;
    private const int SignalAction = 16;

    private static readonly ImmutableArray<string> Libraries = ["OpenNURBS.framework/Versions/A/OpenNURBS", "libresvg_rhino.dylib", "CustomViews.framework/Versions/A/CustomViews", "Sparkle.framework/Versions/B/Sparkle", "libOpenImageIORH.dylib", "libOpenImageIO_UtilRH.dylib", "RhCore.framework/Versions/A/RhCore"];
    private static readonly ImmutableArray<int> Signals = [4, 6, 10, 11];
    private static readonly TimeSpan Queued = TimeSpan.FromMinutes(10);
    private static readonly nint Source = Sourced();

    private static Task Main(string[] args) {
        string owner = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string release = AppContext.GetData("RhinoVersion") is string version ? $"{version.Split('.')[0]}.0" : throw new InvalidOperationException("RhinoVersion");
        DirectoryInfo home = Directory.CreateTempSubdirectory(AppDomain.CurrentDomain.FriendlyName);
        DirectoryInfo data = home.CreateSubdirectory(Path.Combine("Library", "Application Support"));
        Seq(Path.Combine("McNeel", "RegInfo.json"), Path.Combine("McNeel", "Rhinoceros", release, "License Manager"), Path.Combine("McNeel", "Rhinoceros", release, "keypairs"))
            .Map(seed => Path.Combine(owner, seed))
            .Bind(static seed => File.Exists(seed) ? Seq(seed) : toSeq(Directory.EnumerateFiles(seed, "*", SearchOption.AllDirectories)))
            .Map(file => (From: file, To: new FileInfo(Path.Combine(data.FullName, Path.GetRelativePath(owner, file)))))
            .Iter(static copy => {
                copy.To.Directory?.Create();
                File.Copy(copy.From, copy.To.FullName);
            });
        Seq(("HOME", home.FullName), ("CFFIXED_USER_HOME", home.FullName),
                ("RASM_RHINO_PREFERENCES", Path.Combine(home.CreateSubdirectory(Path.Combine("Library", "Preferences")).FullName, AppDomain.CurrentDomain.FriendlyName)),
                ("RASM_RHINO_ALERTS", Path.Combine(home.FullName, "alerts.log")))
            .Iter(static variable => {
                Environment.SetEnvironmentVariable(variable.Item1, variable.Item2);
                Export(variable.Item1, variable.Item2, overwrite: 1);
            });
        CFRunLoopAddSource(CFRunLoopGetMain(), Source, Marshal.ReadIntPtr(NativeLibrary.GetExport(NativeLibrary.Load(CoreFoundation), "kCFRunLoopCommonModes")));
        Task tests = Task.Run(async () => {
            int status = await TestPlatformTestFramework.RunAsync(args, SelfRegisteredExtensions.AddSelfRegisteredExtensions).ConfigureAwait(false);
            home.Delete(recursive: true);
            Terminate(status);
        }, CancellationToken.None);
        CFRunLoopRun();
        return tests;
    }

    private static nint Sourced() {
        nint* context = stackalloc nint[10];
        context[9] = (nint)(delegate* unmanaged<nint, void>)&Booting;
        return CFRunLoopSourceCreate(0, 0, context);
    }

    [UnmanagedCallersOnly]
    private static void Booting(nint _) =>
        Started.Value.Iter(static booted => Try.lift(Boot).Run().Match(
            Succ: _ => {
                booted.SetResult();
                Looped();
            },
            Fail: error => booted.SetException(error.ToException())));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Unit Boot() {
        nint clr = NativeLibrary.Load("libcoreclr.dylib");
        Guid runtime = new(0x64F6D366, 0xD7C2, 0x4F1F, 0xB4, 0xB2, 0xE8, 0x16, 0x0C, 0xAC, 0x43, 0xAF);
        nint handle;
        Marshal.ThrowExceptionForHR(((delegate* unmanaged<Guid*, nint*, int>)NativeLibrary.GetExport(clr, "GetCLRRuntimeHost"))(&runtime, &handle));
        nint xamarin = NativeLibrary.Load(Path.Combine(Frameworks, "libxamarin-dotnet-coreclr.dylib"));
        Marshal.WriteIntPtr(NativeLibrary.GetExport(xamarin, "coreclr_handle"), handle);
        Marshal.WriteInt32(NativeLibrary.GetExport(xamarin, "coreclr_domainId"), AppDomain.CurrentDomain.Id);
        nint core = Libraries.Select(static library => Opened(Path.Combine(Frameworks, library), Lazy | Global) is not 0 and var opened ? opened : throw new DllNotFoundException(Marshal.PtrToStringUTF8(Unopened()))).ToArray()[^1];
        ((delegate* unmanaged<delegate* unmanaged<void>, void>)NativeLibrary.GetExport(core, "RhSetDotNetStartProc"))(&Initialized);
        return Mode switch {
            RhinoMode.Headless => Headless(core),
            RhinoMode.Interface => Launched(core),
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Unit Headless(nint core) {
        ((delegate* unmanaged<int, void>)NativeLibrary.GetExport(core, "RhinoMacLibraryStart"))(1);
        return Captured();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Unit Launched(nint core) {
        nint application = Send(Class.GetHandle("MRApplication"), "sharedApplication");
        nint controller = Signalled(static () => Send(Send(Class.GetHandle("AppController"), "alloc"), "init"));
        ((delegate* unmanaged<int, void>)NativeLibrary.GetExport(core, "RhinoMacLibraryStart"))(0);
        Send(application, "setSettings:", Send(Send(Class.GetHandle("MRAppSettings"), "alloc"), "init"));
        using NSString licensed = new("applicationLicenseIsValid");
        using NSNumber valid = NSNumber.FromBoolean((((delegate* unmanaged<nint, byte, ulong>)NativeLibrary.GetExport(core, "_ZN20CRhinoLicenseManager7IsValidEb"))(
            ((delegate* unmanaged<nint, nint>)NativeLibrary.GetExport(core, "_ZN9CRhinoApp14LicenseManagerEv"))(((delegate* unmanaged<nint>)NativeLibrary.GetExport(core, "_Z8RhinoAppv"))()), 0) & 1) is 1);
        (Runtime.GetNSObject<NSObject>(controller) ?? throw new EntryPointNotFoundException("AppController")).SetValueForKey(valid, licensed);
        NSNotificationCenter.DefaultCenter.PostNotificationName(
            Runtime.GetNSObject<NSString>(Marshal.ReadIntPtr(NativeLibrary.GetExport(core, "MRApplicationDidFinishLaunchingNotification")))?.ToString() ?? throw new EntryPointNotFoundException("MRApplicationDidFinishLaunchingNotification"),
            anObject: null);
        return Captured();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Looped() => NSApplication.SharedApplication.Run();

    private static nint Signalled(Func<nint> work) {
        byte[] held = new byte[Signals.Length * SignalAction];
        fixed (byte* saved = held) {
            for (int at = 0; at < Signals.Length; at++)
                Marshal.ThrowExceptionForHR(Action(Signals[at], action: null, saved + (at * SignalAction)));
            nint result = work();
            for (int at = 0; at < Signals.Length; at++)
                Marshal.ThrowExceptionForHR(Action(Signals[at], saved + (at * SignalAction), previous: null));
            return result;
        }
    }

    [UnmanagedCallersOnly]
    private static void Initialized() =>
        ignore(AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Resources, "dotnetstart.10.dll"))
            .GetType("dotnetstart.DotNetInitialization")?.GetMethod("Start")?.Invoke(null, [Mode switch { RhinoMode.Headless => "headless", RhinoMode.Interface => "" }]));

    private static async partial Task RequestedAsync() {
        Process[] running = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Environment.ProcessPath));
        Process[] hosts = [.. running.Where(static other => other.Id != Environment.ProcessId && other.Id != Parent() && string.Equals(other.MainModule?.FileName, Environment.ProcessPath, StringComparison.Ordinal))];
        try {
            await Task.WhenAll(hosts.Select(static other => other.WaitForExitAsync(TestContext.Current.CancellationToken))).WaitAsync(Queued, TimeProvider.System, TestContext.Current.CancellationToken).ConfigureAwait(false);
        } catch (TimeoutException waited) {
            throw new TimeoutException($"Rhino test hosts {string.Join(", ", hosts.Select(static other => other.Id))} still run after {Queued}, one Rhino boots at a time on a machine", waited);
        } finally {
            System.Array.ForEach(running, static other => other.Dispose());
        }
        CFRunLoopSourceSignal(Source);
        CFRunLoopWakeUp(CFRunLoopGetMain());
    }

    internal static partial RhinoDoc? Untitled() {
        using NSString key = new("runtimeSerialNumber");
        return NSDocumentController.SharedDocumentController.OpenUntitledDocument(displayDocument: false, out NSError _)?.ValueForKey(key) is NSNumber serial
            ? RhinoDoc.FromRuntimeSerialNumber(serial.UInt32Value)
            : null;
    }

    private static nint Send(nint receiver, string selector) => Sent(receiver, Selector.GetHandle(selector));

    private static nint Send(nint receiver, string selector, nint argument) => Sent(receiver, Selector.GetHandle(selector), argument);

    [LibraryImport(Objective, EntryPoint = "objc_msgSend")]
    private static partial nint Sent(nint receiver, nint selector);

    [LibraryImport(Objective, EntryPoint = "objc_msgSend")]
    private static partial nint Sent(nint receiver, nint selector, nint argument);

    [LibraryImport("libc", EntryPoint = "dlopen", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint Opened(string path, int mode);

    [LibraryImport("libc", EntryPoint = "dlerror")]
    private static partial nint Unopened();

    [LibraryImport("libc", EntryPoint = "sigaction")]
    private static partial int Action(int signal, byte* action, byte* previous);

    [LibraryImport("libc", EntryPoint = "setenv", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int Export(string name, string value, int overwrite);

    [LibraryImport("libc", EntryPoint = "getppid")]
    private static partial int Parent();

    [LibraryImport("libc", EntryPoint = "_exit")]
    private static partial void Terminate(int status);

    [LibraryImport(CoreFoundation)]
    private static partial nint CFRunLoopGetMain();

    [LibraryImport(CoreFoundation)]
    private static partial nint CFRunLoopSourceCreate(nint allocator, nint order, nint* context);

    [LibraryImport(CoreFoundation)]
    private static partial void CFRunLoopAddSource(nint loop, nint source, nint mode);

    [LibraryImport(CoreFoundation)]
    private static partial void CFRunLoopSourceSignal(nint source);

    [LibraryImport(CoreFoundation)]
    private static partial void CFRunLoopWakeUp(nint loop);

    [LibraryImport(CoreFoundation)]
    private static partial void CFRunLoopRun();

}
