# [RASM_APPHOST_API_VELOPACK]

`Velopack` admits the install/update/uninstall hook gate, the feed-checked update manager, typed release assets, channel and downgrade options, and the locator contract into the runtime provisioning and self-update rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Velopack`

- package: `Velopack`
- assembly: `Velopack`
- namespace: `Velopack`
- namespace: `Velopack.Locators`
- namespace: `Velopack.Sources`
- namespace: `Velopack.NuGet`
- asset: runtime library
- rail: runtime provisioning and update

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: startup hook gate

- rail: runtime provisioning and update

| [INDEX] | [SYMBOL]       | [PACKAGE_ROLE] | [CAPABILITY]                      |
| :-----: | :------------- | :------------- | :-------------------------------- |
|  [01]   | `VelopackApp`  | builder gate   | install/update lifecycle dispatch |
|  [02]   | `VelopackHook` | hook delegate  | versioned startup-event callback  |

[PUBLIC_TYPE_SCOPE]: update manager and feed values

- rail: runtime provisioning and update

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]  | [CAPABILITY]                         |
| :-----: | :------------------ | :-------------- | :----------------------------------- |
|  [01]   | `UpdateManager`     | manager service | feed check, download, apply, restart |
|  [02]   | `UpdateInfo`        | feed result     | target release plus delta chain      |
|  [03]   | `VelopackAsset`     | release record  | identified package entry with hashes |
|  [04]   | `VelopackAssetType` | asset kind enum | full/delta/portable/installer/msi    |

[PUBLIC_TYPE_SCOPE]: option and locator contracts

- rail: runtime provisioning and update

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]   | [CAPABILITY]                          |
| :-----: | :----------------- | :--------------- | :------------------------------------ |
|  [01]   | `UpdateOptions`    | option value     | channel, downgrade, delta fallback    |
|  [02]   | `IVelopackLocator` | locator contract | install paths, version, channel       |
|  [03]   | `IUpdateSource`    | source contract  | release feed and entry download       |
|  [04]   | `SemanticVersion`  | version value    | comparable major/minor/patch identity |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: startup gate (`VelopackApp`, `Velopack`; `VelopackHook` = `void (SemanticVersion version)`)

- rail: runtime provisioning and update

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]    | [RAIL]                                           |
| :-----: | :------------------------------------------------- | :---------------- | :----------------------------------------------- |
|  [01]   | `VelopackApp.Build()`                              | builder factory   | opens the fluent hook gate                       |
|  [02]   | `SetArgs(string[] args)`                           | builder option    | startup-argument override                        |
|  [03]   | `SetAutoApplyOnStartup(bool autoApply)`            | builder option    | pending-update auto-apply toggle (default on)    |
|  [04]   | `SetAppUserModelId(string aumid)`                  | builder option    | Windows AUMID identity                           |
|  [05]   | `SetLocator(IVelopackLocator locator)`             | builder option    | locator override                                 |
|  [06]   | `SetLogger(IVelopackLogger logger)`                | builder option    | hook-process logging                             |
|  [07]   | `OnFirstRun(VelopackHook hook)`                    | hook registration | first-run callback                               |
|  [08]   | `OnRestarted(VelopackHook hook)`                   | hook registration | post-restart callback                            |
|  [09]   | `OnAfterInstallFastCallback(VelopackHook hook)`    | hook registration | post-install fast hook                           |
|  [10]   | `OnAfterUpdateFastCallback(VelopackHook hook)`     | hook registration | post-update fast hook                            |
|  [11]   | `OnBeforeUpdateFastCallback(VelopackHook hook)`    | hook registration | pre-update fast hook                             |
|  [12]   | `OnBeforeUninstallFastCallback(VelopackHook hook)` | hook registration | pre-uninstall fast hook                          |
|  [13]   | `Run()`                                            | gate dispatch     | `--veloapp-*` dispatch; once at earliest startup |

[ENTRYPOINT_SCOPE]: update manager (`UpdateManager`, `Velopack`)

- rail: runtime provisioning and update

[UPDATE_MANAGER_SIGNATURES]:

- URL/path constructor: `new UpdateManager(string urlOrPath, UpdateOptions? = null, IVelopackLocator? = null)`
- source constructor: `new UpdateManager(IUpdateSource source, UpdateOptions? = null, IVelopackLocator? = null)`
- synchronous download: `DownloadUpdates(UpdateInfo, Action<int>? progress)`
- asynchronous download: `DownloadUpdatesAsync(UpdateInfo, Action<int>?, CancellationToken)`
- restart apply: `ApplyUpdatesAndRestart(VelopackAsset? toApply, string[]? restartArgs)`
- exit apply: `ApplyUpdatesAndExit(VelopackAsset? toApply)`
- deferred apply: `WaitExitThenApplyUpdates(VelopackAsset? toApply, bool silent, bool restart, string[]? restartArgs)`

Feed checks return `UpdateInfo?`; null denotes no newer release on the resolved channel.

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------- | :------------- | :------------------------------------- |
|  [01]   | `UpdateManager(urlOrPath)` | ctor           | feed URL/path binding                  |
|  [02]   | `UpdateManager(source)`    | ctor           | custom `IUpdateSource` binding         |
|  [03]   | `AppId`                    | state read     | installed identity (`string?`)         |
|  [04]   | `CurrentVersion`           | state read     | installed version (`SemanticVersion?`) |
|  [05]   | `IsInstalled`              | state read     | installed-mode flag                    |
|  [06]   | `IsPortable`               | state read     | portable-mode flag                     |
|  [07]   | `IsUpdatePendingRestart`   | state read     | staged-asset flag (`bool`)             |
|  [08]   | `UpdatePendingRestart`     | state read     | staged asset (`VelopackAsset?`)        |
|  [09]   | `CheckForUpdates()`        | feed check     | synchronous result                     |
|  [10]   | `CheckForUpdatesAsync()`   | feed check     | asynchronous result                    |
|  [11]   | `DownloadUpdates`          | download       | target materialization with progress   |
|  [12]   | `DownloadUpdatesAsync`     | download       | cancellable target materialization     |
|  [13]   | `ApplyUpdatesAndRestart`   | apply          | bootstrapper handoff and relaunch      |
|  [14]   | `ApplyUpdatesAndExit`      | apply          | headless apply then exit               |
|  [15]   | `WaitExitThenApplyUpdates` | apply          | deferred apply on process exit         |

[ENTRYPOINT_SCOPE]: feed values and options (`UpdateInfo`/`VelopackAsset`/`VelopackAssetType`/`UpdateOptions`, `Velopack`)

- rail: runtime provisioning and update

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]   | [RAIL]                                                |
| :-----: | :---------------------------------------------------- | :--------------- | :---------------------------------------------------- |
|  [01]   | `UpdateInfo.TargetFullRelease`                        | feed result      | target full-release asset                             |
|  [02]   | `UpdateInfo.BaseRelease` / `DeltasToTarget`           | feed result      | delta chain (`VelopackAsset?` / `VelopackAsset[]`)    |
|  [03]   | `UpdateInfo.IsDowngrade`                              | feed result      | downgrade flag                                        |
|  [04]   | `UpdateInfo` implicit `VelopackAsset` conversion      | conversion       | coerces to `TargetFullRelease`                        |
|  [05]   | `VelopackAsset.PackageId`/`Version`/`Type`/`FileName` | release record   | release identity fields                               |
|  [06]   | `VelopackAsset.SHA1`/`SHA256`/`Size`                  | release evidence | integrity hashes + byte size                          |
|  [07]   | `VelopackAsset.NotesMarkdown`/`NotesHTML`             | release record   | release notes                                         |
|  [08]   | `VelopackAssetType`                                   | asset kind enum  | `Full=1`/`Delta=2`/`Portable=3`/`Installer=4`/`Msi=5` |
|  [09]   | `UpdateOptions.AllowVersionDowngrade`                 | option value     | older-target admission                                |
|  [10]   | `UpdateOptions.ExplicitChannel`                       | option value     | non-default channel selection (`string?`)             |
|  [11]   | `UpdateOptions.MaximumDeltasBeforeFallback`           | option value     | delta-chain cap before full-package fallback (`int?`) |

[ENTRYPOINT_SCOPE]: locator contract (`IVelopackLocator`, `Velopack.Locators`)

- rail: runtime provisioning and update

Identity and install-path reads return `string?`.

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :---------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `AppId`                             | locator read   | application identity                    |
|  [02]   | `Channel`                           | locator read   | release channel                         |
|  [03]   | `AppUserModelId`                    | locator read   | Windows application identity            |
|  [04]   | `RootAppDir`                        | locator read   | root application path                   |
|  [05]   | `PackagesDir`                       | locator read   | package path                            |
|  [06]   | `AppContentDir`                     | locator read   | application-content path                |
|  [07]   | `AppTempDir`                        | locator read   | application-temporary path              |
|  [08]   | `UpdateExePath`                     | locator read   | update executable path                  |
|  [09]   | `ThisExeRelativePath`               | locator read   | application-relative executable path    |
|  [10]   | `CurrentlyInstalledVersion`         | locator read   | installed version (`SemanticVersion?`)  |
|  [11]   | `IsPortable`                        | locator read   | portable-mode flag                      |
|  [12]   | `AddLogger(IVelopackLogger logger)` | locator op     | logger attachment                       |
|  [13]   | `GetLocalPackages()`                | locator op     | local releases (`List<VelopackAsset>`)  |
|  [14]   | `GetLatestLocalFullPackage()`       | locator op     | latest local release (`VelopackAsset?`) |
|  [15]   | `GetOrCreateStagedUserId()`         | locator op     | staged-rollout identity (`Guid?`)       |

## [04]-[IMPLEMENTATION_LAW]

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
