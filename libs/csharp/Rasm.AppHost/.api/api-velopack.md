# [RASM_APPHOST_API_VELOPACK]

`Velopack` admits the install/update/uninstall hook gate, the feed-checked update
manager, typed release assets, channel and downgrade options, and the locator
contract into the runtime provisioning and self-update rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Velopack`
- package: `Velopack`
- assembly: `Velopack`
- namespace: `Velopack`
- namespace: `Velopack.Locators`
- namespace: `Velopack.Sources`
- namespace: `Velopack.NuGet`
- asset: runtime library
- rail: runtime provisioning and update

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: startup hook gate
- rail: runtime provisioning and update

| [INDEX] | [SYMBOL]       | [PACKAGE_ROLE] | [CAPABILITY]                      |
| :-----: | :------------- | :------------- | :-------------------------------- |
|   [1]   | `VelopackApp`  | builder gate   | install/update lifecycle dispatch |
|   [2]   | `VelopackHook` | hook delegate  | versioned startup-event callback  |

[PUBLIC_TYPE_SCOPE]: update manager and feed values
- rail: runtime provisioning and update

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]  | [CAPABILITY]                         |
| :-----: | :------------------ | :-------------- | :----------------------------------- |
|   [1]   | `UpdateManager`     | manager service | feed check, download, apply, restart |
|   [2]   | `UpdateInfo`        | feed result     | target release plus delta chain      |
|   [3]   | `VelopackAsset`     | release record  | identified package entry with hashes |
|   [4]   | `VelopackAssetType` | asset kind enum | full/delta/portable/installer/msi    |

[PUBLIC_TYPE_SCOPE]: option and locator contracts
- rail: runtime provisioning and update

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]   | [CAPABILITY]                          |
| :-----: | :----------------- | :--------------- | :------------------------------------ |
|   [1]   | `UpdateOptions`    | option value     | channel, downgrade, delta fallback    |
|   [2]   | `IVelopackLocator` | locator contract | install paths, version, channel       |
|   [3]   | `IUpdateSource`    | source contract  | release feed and entry download       |
|   [4]   | `SemanticVersion`  | version value    | comparable major/minor/patch identity |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: startup gate
- rail: runtime provisioning and update

```csharp
namespace Velopack;

public delegate void VelopackHook(SemanticVersion version);

public sealed class VelopackApp
{
    public static VelopackApp Build();
    public VelopackApp SetArgs(string[] args);
    public VelopackApp SetAutoApplyOnStartup(bool autoApply);
    public VelopackApp SetAppUserModelId(string aumid);
    public VelopackApp SetLocator(IVelopackLocator locator);
    public VelopackApp SetLogger(IVelopackLogger logger);
    public VelopackApp OnFirstRun(VelopackHook hook);
    public VelopackApp OnRestarted(VelopackHook hook);
    public VelopackApp OnAfterInstallFastCallback(VelopackHook hook);
    public VelopackApp OnAfterUpdateFastCallback(VelopackHook hook);
    public VelopackApp OnBeforeUpdateFastCallback(VelopackHook hook);
    public VelopackApp OnBeforeUninstallFastCallback(VelopackHook hook);
    public void Run();
}
```

[ENTRYPOINT_SCOPE]: update manager
- rail: runtime provisioning and update

```csharp
namespace Velopack;

public class UpdateManager
{
    public UpdateManager(string urlOrPath, UpdateOptions? options = null, IVelopackLocator? locator = null);
    public UpdateManager(IUpdateSource source, UpdateOptions? options = null, IVelopackLocator? locator = null);

    public virtual string? AppId { get; }
    public virtual bool IsInstalled { get; }
    public virtual bool IsPortable { get; }
    public virtual bool IsUpdatePendingRestart { get; }
    public virtual VelopackAsset? UpdatePendingRestart { get; }
    public virtual SemanticVersion? CurrentVersion { get; }

    public UpdateInfo? CheckForUpdates();
    public virtual Task<UpdateInfo?> CheckForUpdatesAsync();
    public void DownloadUpdates(UpdateInfo updates, Action<int>? progress = null);
    public virtual Task DownloadUpdatesAsync(UpdateInfo updates, Action<int>? progress = null, CancellationToken cancelToken = default);
    public void ApplyUpdatesAndRestart(VelopackAsset? toApply, string[]? restartArgs = null);
    public void ApplyUpdatesAndExit(VelopackAsset? toApply);
    public void WaitExitThenApplyUpdates(VelopackAsset? toApply, bool silent = false, bool restart = true, string[]? restartArgs = null);
}
```

[ENTRYPOINT_SCOPE]: feed values and options
- rail: runtime provisioning and update

```csharp
namespace Velopack;

public class UpdateInfo
{
    public VelopackAsset TargetFullRelease { get; }
    public bool IsDowngrade { get; }
    public VelopackAsset? BaseRelease { get; }
    public VelopackAsset[] DeltasToTarget { get; }
    public static implicit operator VelopackAsset(UpdateInfo updateInfo);
}

public record VelopackAsset
{
    public string PackageId { get; set; }
    public SemanticVersion Version { get; set; }
    public VelopackAssetType Type { get; set; }
    public string FileName { get; set; }
    public string SHA1 { get; set; }
    public string SHA256 { get; set; }
    public long Size { get; set; }
    public string NotesMarkdown { get; set; }
    public string NotesHTML { get; set; }
}

public enum VelopackAssetType
{
    Full = 1,
    Delta = 2,
    Portable = 3,
    Installer = 4,
    Msi = 5
}

public class UpdateOptions
{
    public bool AllowVersionDowngrade { get; set; }
    public string? ExplicitChannel { get; set; }
    public int? MaximumDeltasBeforeFallback { get; set; }
}
```

[ENTRYPOINT_SCOPE]: locator contract
- rail: runtime provisioning and update

```csharp
namespace Velopack.Locators;

public interface IVelopackLocator
{
    string? AppId { get; }
    string? RootAppDir { get; }
    string? PackagesDir { get; }
    string? AppContentDir { get; }
    string? AppTempDir { get; }
    string? UpdateExePath { get; }
    SemanticVersion? CurrentlyInstalledVersion { get; }
    string? ThisExeRelativePath { get; }
    string? Channel { get; }
    string? AppUserModelId { get; }
    bool IsPortable { get; }
    void AddLogger(IVelopackLogger logger);
    List<VelopackAsset> GetLocalPackages();
    VelopackAsset? GetLatestLocalFullPackage();
    Guid? GetOrCreateStagedUserId();
}
```

## [4]-[IMPLEMENTATION_LAW]

[GATE_TOPOLOGY]:
- `VelopackApp.Build()` opens a fluent gate that registers install, update, obsolete, uninstall, first-run, and restart hooks before `Run()`.
- `Run()` reads `--veloapp-*` command arguments, dispatches the matching `VelopackHook`, applies pending updates when auto-apply is on, then returns control to normal startup.
- `Run()` must execute once at the earliest point of process startup; a second call corrupts locator state.
- `SetAutoApplyOnStartup` is on by default; install/update fast callbacks run inside the hook process and stay non-blocking.

[UPDATE_TOPOLOGY]:
- `UpdateManager` binds to a feed URL/path or `IUpdateSource` with optional `UpdateOptions` and `IVelopackLocator`.
- `CheckForUpdatesAsync` returns `UpdateInfo?`; null signals no newer release on the resolved channel.
- `UpdateInfo` carries `TargetFullRelease`, the `BaseRelease`/`DeltasToTarget` delta chain, and an `IsDowngrade` flag.
- `DownloadUpdatesAsync` materializes the target with `Action<int>` progress, after which `IsUpdatePendingRestart` is set.
- `ApplyUpdatesAndRestart` hands off to the bootstrapper and relaunches; `ApplyUpdatesAndExit` and `WaitExitThenApplyUpdates` cover headless apply paths.

[OPTION_LAW]:
- `ExplicitChannel` selects a non-default release channel; `AllowVersionDowngrade` admits older targets.
- `MaximumDeltasBeforeFallback` caps the delta chain before a full-package fallback.
- Channel and version policy bind at composition; domain code never sets channel strings ad hoc.

[LOCAL_ADMISSION]:
- Provisioning and self-update are host-level concerns; domain code never constructs `UpdateManager` directly.
- Install paths, current version, and channel are read through `IVelopackLocator`, not from the filesystem.
- `VelopackAsset` hashes and `VelopackAssetType` are read-only release evidence; the apply decision lives in the owning update policy surface.

[RAIL_LAW]:
- Package: `Velopack`
- Owns: desktop application provisioning, self-update lifecycle, and release-feed evidence
- Accept: hook-gated startup, feed-checked update apply, and locator-resolved install state
- Reject: hand-rolled installer scripting, manual binary swapping, or filesystem version probing
