# [RASM_APPHOST_API_VELOPACK]

`Velopack` owns desktop-application provisioning and the self-update lifecycle at the host boundary: a startup hook gate dispatches install, update, and uninstall phases ahead of normal startup, and a feed-checked `UpdateManager` resolves, downloads, and applies releases against a locator-owned install state. Provisioning binds only at the host composition root; domain code never constructs it.

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

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :------------- | :------------ | :------------------------------- |
|  [01]   | `VelopackApp`  | class         | install/update lifecycle gate    |
|  [02]   | `VelopackHook` | delegate      | versioned startup-event callback |

[PUBLIC_TYPE_SCOPE]: update manager and feed values

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------ | :------------ | :----------------------------------- |
|  [01]   | `UpdateManager`     | class         | feed check, download, apply, restart |
|  [02]   | `UpdateInfo`        | class         | target release plus delta chain      |
|  [03]   | `VelopackAsset`     | class         | identified release entry with hashes |
|  [04]   | `VelopackAssetType` | enum          | full/delta/portable/installer/msi    |

[PUBLIC_TYPE_SCOPE]: option and locator contracts

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :----------------- | :------------ | :------------------------------------ |
|  [01]   | `UpdateOptions`    | class         | channel, downgrade, delta fallback    |
|  [02]   | `IVelopackLocator` | interface     | install paths, version, channel       |
|  [03]   | `IUpdateSource`    | interface     | release feed and entry download       |
|  [04]   | `SemanticVersion`  | class         | comparable major/minor/patch identity |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: startup gate — fluent hook registration then dispatch

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `VelopackApp.Build() -> VelopackApp`          | factory  | opens the fluent hook gate                       |
|  [02]   | `SetArgs(string[])`                           | instance | startup-argument override                        |
|  [03]   | `SetAutoApplyOnStartup(bool)`                 | instance | pending-update auto-apply toggle (default on)    |
|  [04]   | `SetAppUserModelId(string)`                   | instance | Windows AUMID identity                           |
|  [05]   | `SetLocator(IVelopackLocator)`                | instance | locator override                                 |
|  [06]   | `SetLogger(IVelopackLogger)`                  | instance | hook-process logging                             |
|  [07]   | `OnFirstRun(VelopackHook)`                    | instance | first-run callback                               |
|  [08]   | `OnRestarted(VelopackHook)`                   | instance | post-restart callback                            |
|  [09]   | `OnAfterInstallFastCallback(VelopackHook)`    | instance | post-install fast hook                           |
|  [10]   | `OnAfterUpdateFastCallback(VelopackHook)`     | instance | post-update fast hook                            |
|  [11]   | `OnBeforeUpdateFastCallback(VelopackHook)`    | instance | pre-update fast hook                             |
|  [12]   | `OnBeforeUninstallFastCallback(VelopackHook)` | instance | pre-uninstall fast hook                          |
|  [13]   | `Run()`                                       | instance | `--veloapp-*` dispatch, once at earliest startup |

- `VelopackHook`: `void (SemanticVersion version)`.
- `On*FastCallback`: Windows-only; the hook runs, then the process exits, terminated past its 15-30s deadline.

[ENTRYPOINT_SCOPE]: update manager — bind, check, download, apply

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `UpdateManager(string, UpdateOptions?, IVelopackLocator?)`          | ctor     | feed URL/path binding                  |
|  [02]   | `UpdateManager(IUpdateSource, UpdateOptions?, IVelopackLocator?)`   | ctor     | custom `IUpdateSource` binding         |
|  [03]   | `AppId`                                                             | property | installed identity (`string?`)         |
|  [04]   | `CurrentVersion`                                                    | property | installed version (`SemanticVersion?`) |
|  [05]   | `IsInstalled`                                                       | property | installed-mode flag                    |
|  [06]   | `IsPortable`                                                        | property | portable-mode flag                     |
|  [07]   | `IsUpdatePendingRestart`                                            | property | staged-asset flag (`bool`)             |
|  [08]   | `UpdatePendingRestart`                                              | property | staged asset (`VelopackAsset?`)        |
|  [09]   | `CheckForUpdates() -> UpdateInfo?`                                  | instance | synchronous feed check                 |
|  [10]   | `CheckForUpdatesAsync() -> UpdateInfo?`                             | instance | asynchronous feed check                |
|  [11]   | `DownloadUpdates(UpdateInfo, Action<int>?)`                         | instance | target materialization with progress   |
|  [12]   | `DownloadUpdatesAsync(UpdateInfo, Action<int>?, CancellationToken)` | instance | cancellable materialization            |
|  [13]   | `ApplyUpdatesAndRestart(VelopackAsset?, string[]?)`                 | instance | bootstrapper handoff and relaunch      |
|  [14]   | `ApplyUpdatesAndExit(VelopackAsset?)`                               | instance | headless apply then exit               |
|  [15]   | `WaitExitThenApplyUpdates(VelopackAsset?, bool, bool, string[]?)`   | instance | deferred apply on process exit         |

[ENTRYPOINT_SCOPE]: feed results, release records, and option values

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `UpdateInfo.TargetFullRelease`              | property | target full-release asset               |
|  [02]   | `UpdateInfo.BaseRelease`                    | property | delta base (`VelopackAsset?`)           |
|  [03]   | `UpdateInfo.DeltasToTarget`                 | property | ordered delta chain (`VelopackAsset[]`) |
|  [04]   | `UpdateInfo.IsDowngrade`                    | property | downgrade or lateral-move flag          |
|  [05]   | `(VelopackAsset)UpdateInfo`                 | operator | implicit coerce to `TargetFullRelease`  |
|  [06]   | `VelopackAsset.PackageId`                   | property | release package identity                |
|  [07]   | `VelopackAsset.Version`                     | property | release version (`SemanticVersion`)     |
|  [08]   | `VelopackAsset.Type`                        | property | asset kind (`VelopackAssetType`)        |
|  [09]   | `VelopackAsset.FileName`                    | property | release file name                       |
|  [10]   | `VelopackAsset.SHA1`                        | property | SHA1 integrity hash                     |
|  [11]   | `VelopackAsset.SHA256`                      | property | SHA256 integrity hash                   |
|  [12]   | `VelopackAsset.Size`                        | property | byte size (`long`)                      |
|  [13]   | `VelopackAsset.NotesMarkdown`               | property | release notes markdown                  |
|  [14]   | `VelopackAsset.NotesHTML`                   | property | release notes HTML                      |
|  [15]   | `UpdateOptions.AllowVersionDowngrade`       | property | older-target admission (`bool`)         |
|  [16]   | `UpdateOptions.ExplicitChannel`             | property | non-default channel (`string?`)         |
|  [17]   | `UpdateOptions.MaximumDeltasBeforeFallback` | property | delta cap before full fallback (`int?`) |

- `VelopackAssetType`: `Full=1` `Delta=2` `Portable=3` `Installer=4` `Msi=5`.

[ENTRYPOINT_SCOPE]: locator reads and ops — install-state resolution

Identity and install-path reads return `string?`.

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `AppId`                                         | property | application identity                   |
|  [02]   | `Channel`                                       | property | release channel                        |
|  [03]   | `AppUserModelId`                                | property | Windows application identity           |
|  [04]   | `RootAppDir`                                    | property | root application path                  |
|  [05]   | `PackagesDir`                                   | property | package path                           |
|  [06]   | `AppContentDir`                                 | property | application-content path               |
|  [07]   | `AppTempDir`                                    | property | application-temporary path             |
|  [08]   | `UpdateExePath`                                 | property | update-executable path                 |
|  [09]   | `ThisExeRelativePath`                           | property | application-relative executable path   |
|  [10]   | `CurrentlyInstalledVersion`                     | property | installed version (`SemanticVersion?`) |
|  [11]   | `IsPortable`                                    | property | portable-mode flag                     |
|  [12]   | `AddLogger(IVelopackLogger)`                    | instance | logger attachment                      |
|  [13]   | `GetLocalPackages() -> List<VelopackAsset>`     | instance | local releases                         |
|  [14]   | `GetLatestLocalFullPackage() -> VelopackAsset?` | instance | latest local release                   |
|  [15]   | `GetOrCreateStagedUserId() -> Guid?`            | instance | staged-rollout identity                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `VelopackApp.Build()` opens a fluent gate registering first-run, restart, install, update, and uninstall hooks; `Run()` dispatches the matching `--veloapp-*` hook, applies pending updates when auto-apply is on, then returns to normal startup — it executes once at the earliest point of startup, and a second `Run()` corrupts locator state.
- `UpdateManager` binds a feed URL/path or `IUpdateSource` with optional `UpdateOptions` and `IVelopackLocator`; `CheckForUpdatesAsync` returns `UpdateInfo?`, null on no newer release for the resolved channel.
- `UpdateInfo` carries `TargetFullRelease`, the `BaseRelease`/`DeltasToTarget` delta chain, and `IsDowngrade`; `DownloadUpdatesAsync` materializes the target under `Action<int>` progress and sets `IsUpdatePendingRestart`, after which `ApplyUpdatesAndRestart` hands off to the bootstrapper, with `ApplyUpdatesAndExit` and `WaitExitThenApplyUpdates` covering headless apply.
- `UpdateOptions` binds release policy at composition: `ExplicitChannel` selects a non-default channel, `AllowVersionDowngrade` admits older targets, and `MaximumDeltasBeforeFallback` caps the delta chain before a full-package fallback.

[STACKING]:
- `api-hosting`(`.api/api-hosting.md`): `VelopackApp.Build()...Run()` fires at `Program` entry before `HostApplicationBuilder` composes the host, so the gate resolves install, update, and uninstall args before any host service constructs.
- `api-di`(`.api/api-di.md`): the composition root registers `UpdateManager` and the resolved `IVelopackLocator` as singleton `ServiceDescriptor` rows, so the self-update policy surface resolves them through `GetRequiredService` rather than constructing them.
- within-lib: AppHost folds the `UpdateManager` sequence into one policy owner — `CheckForUpdatesAsync` then `DownloadUpdatesAsync` under progress then `ApplyUpdatesAndRestart` — threading `UpdateOptions` channel and downgrade policy from composition, never per call.

[LOCAL_ADMISSION]:
- Provisioning and self-update are host-level concerns; domain code never constructs `UpdateManager` and never sets channel strings.
- Install paths, current version, and channel resolve through `IVelopackLocator`, not the filesystem.
- `VelopackAsset` hashes and `VelopackAssetType` are read-only release evidence; the apply decision lives in the owning update-policy surface.

[RAIL_LAW]:
- Package: `Velopack`
- Owns: desktop application provisioning, self-update lifecycle, and release-feed evidence
- Accept: hook-gated startup, feed-checked update apply, and locator-resolved install state
- Reject: hand-rolled installer scripting, manual binary swapping, or filesystem version probing
