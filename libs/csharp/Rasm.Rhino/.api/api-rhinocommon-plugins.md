# [RASM_RHINO_API_RHINOCOMMON_PLUGINS]

`Rhino.PlugIns` binds managed code into the host through one subclassed `PlugIn` per package, its render, digitizer, and file-dialog specializations, and the license surface that owns the Zoo/CloudZoo entitlement rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon`
- license: proprietary host SDK
- namespace: `Rhino.PlugIns`, `Rhino.FileIO` (`FileType`)
- asset: `RhinoCommon.dll` — the in-process managed host assembly
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plug-in base and specializations

| [INDEX] | [SYMBOL]                     | [KIND]           | [CAPABILITY]                                                            |
| :-----: | :--------------------------- | :--------------- | :---------------------------------------------------------------------- |
|  [01]   | `PlugIn`                     | class            | plug-in base — load, commands, document I/O, options pages, settings    |
|  [02]   | `RenderPlugIn`               | abstract plug-in | render engine — render, preview, material UI, render panels/tabs        |
|  [03]   | `DigitizerPlugIn`            | abstract plug-in | 3D digitizer — enable, unit system, point/ray input                     |
|  [04]   | `FileImportPlugIn`           | abstract plug-in | import dispatch base declaring handled extensions and reading a file    |
|  [05]   | `FileExportPlugIn`           | abstract plug-in | export dispatch base declaring handled extensions and writing a file    |
|  [06]   | `PlugInInfo`                 | class            | registry-level plug-in descriptor (id, paths, organization, load state) |
|  [07]   | `PreviewNotification`        | sealed class     | render-preview intermediate-update notifier                             |
|  [08]   | `PlugInDescriptionAttribute` | sealed attribute | declarative organization/contact metadata keyed by `DescriptionType`    |
|  [09]   | `LicenseIdAttribute`         | sealed attribute | binds a license id to a plug-in for entitlement discovery               |

[PUBLIC_TYPE_SCOPE]: file-dialog registration and dispatch carriers

| [INDEX] | [SYMBOL]           | [KIND] | [CAPABILITY]                                                         |
| :-----: | :----------------- | :----- | :------------------------------------------------------------------- |
|  [01]   | `FileTypeList`     | class  | ordered file-type registration list populated during `AddFileTypes`  |
|  [02]   | `FileType`         | class  | one extension/description pair (`Rhino.FileIO`)                      |
|  [03]   | `FileReadOptions`  | class  | host read-dispatch mode passed into import (`ImportMode`/`OpenMode`) |
|  [04]   | `FileWriteOptions` | class  | host write-dispatch mode passed into export                          |
|  [05]   | `WriteFileResult`  | enum   | export outcome reported back to the host dialog                      |

[PUBLIC_TYPE_SCOPE]: license surface

| [INDEX] | [SYMBOL]                       | [KIND]     | [CAPABILITY]                                                        |
| :-----: | :----------------------------- | :--------- | :------------------------------------------------------------------ |
|  [01]   | `LicenseUtils`                 | static     | license acquisition, checkout/checkin, CloudZoo login, status reads |
|  [02]   | `LicenseData`                  | class      | product license/serial/title/build/count/expiry validation payload  |
|  [03]   | `LicenseStatus`                | class      | resolved license state — type, owner, expiry, CloudZoo lease flags  |
|  [04]   | `LicenseLease`                 | class      | CloudZoo lease identity — group, user, product, issue/expiry window |
|  [05]   | `LicenseChangedEventArgs`      | event args | license change payload                                              |
|  [06]   | `LicenseLeaseChangedEventArgs` | event args | CloudZoo lease change payload                                       |

[PUBLIC_TYPE_SCOPE]: policy vocabularies
- `PlugInType` — flags plug-in kind (`Render`/`FileImport`/`FileExport`/`Digitizer`/`Utility`/`DisplayPipeline`/`DisplayEngine`/`Any`)
- `PlugInLoadTime` — load schedule (`Disabled`/`AtStartup`/`WhenNeeded`/`WhenNeededIgnoreDockingBars`/`WhenNeededOrOptionsDialog`/`WhenNeededOrTabbedDockBar`)
- `LoadReturnCode` (`Success`/`ErrorShowDialog`/`ErrorNoDialog`), `LoadPlugInResult` (`Success`/`SuccessAlreadyLoaded`/`ErrorUnknown`), `ValidateResult` (`Success`/`ErrorShowMessage`/`ErrorHideMessage`)
- `LicenseBuildType` (`Unspecified`/`Release`/`Evaluation`/`Beta`), `LicenseType` (`Standalone`/`Network`/`NetworkLoanedOut`/`NetworkCheckedOut`/`CloudZoo`), `LicenseCapabilities` (flags: `CanBePurchased`/`CanBeEvaluated`/`SupportsRhinoAccounts`/`SupportsStandalone`/`SupportsZooPerUser`/`SupportsZooPerCore`/`SupportsLicenseDiscovery`)
- `DescriptionType` (`Organization`/`Address`/`Country`/`Phone`/`WebSite`/`Email`/`UpdateUrl`/`Fax`/`Icon`) keying `PlugInDescriptionAttribute`, `RenderPlugIn.RenderFeature`, `RenderPlugIn.PreviewRenderTypes`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plug-in identity, discovery, and load
- `PlugIn.Id : Guid` / `PlugInId : Guid` / `Name : string` / `Version : string` / `Description : string` / `Assembly : Assembly` / `LoadTime : PlugInLoadTime` / `AddToHelpMenu : bool` — plug-in facts
- `PlugIn.Find(Guid plugInId) : PlugIn` / `Find(Assembly pluginAssembly) : PlugIn` / `GetPlugInInfo(Guid pluginId) : PlugInInfo` — resolve a loaded plug-in
- `PlugIn.LoadPlugIn(string path, out Guid plugInId) : LoadPlugInResult` / `LoadPlugIn(Guid pluginId[, bool loadQuietly, bool forceLoad]) : bool` / `PlugInExists(Guid id, out bool loaded, out bool loadProtected) : bool`
- `PlugIn.GetInstalledPlugIns([bool localizedPlugInName]) : Dictionary<Guid, string>` / `GetInstalledPlugInNames([PlugInType typeFilter, bool loaded, bool unloaded[, bool localizedPlugInName]]) : string[]` / `GetInstalledPlugInFolders() : string[]` / `InstalledPlugInCount : int`
- `PlugIn.IdFromName(string pluginName) : Guid` / `IdFromPath(string pluginPath) : Guid` / `IdFromFileName(string filename) : Guid` / `NameFromPath(string pluginPath) : string` / `PathFromId(Guid pluginId) : string` / `PathFromName(string pluginName) : string`
- `PlugIn.SetLoadProtection(Guid pluginId, bool loadSilently) : void` / `GetLoadProtection(Guid pluginId, out bool loadSilently) : bool` / `GetEnglishCommandNames(Guid pluginId) : string[]`

[ENTRYPOINT_SCOPE]: plug-in lifecycle overrides
- `protected virtual OnLoad(ref string errorMessage) : LoadReturnCode` / `OnShutdown() : void` / `ResetMessageBoxes() : void` — load and shutdown hooks
- `protected virtual CreateCommands() : void` / `RegisterCommand(Command command) : bool`; `GetCommands() : Command[]`; `HostUtils.RegisterDynamicCommand` composes the runtime command family of `api-rhinocommon-commands.md`
- `protected virtual GetPlugInObject() : object` / `DisplayHelp(nint windowHandle) : bool` / `Icon(Size size) : Bitmap`
- `protected virtual ShouldCallWriteDocument(FileWriteOptions options) : bool` / `WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) : void` / `ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) : void` — per-plug-in document user-data serialization
- `protected virtual OptionsDialogPages(List<OptionsDialogPage> pages) : void` / `DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) : void` / `ObjectPropertiesPages(ObjectPropertiesPageCollection collection) : void`

[ENTRYPOINT_SCOPE]: plug-in settings
- `PlugIn.GetPluginSettings(Guid plugInId, bool load) : PersistentSettings` / `SavePluginSettings(Guid plugInId) : void` / `SaveSettings() : void` / `CommandSettings(string name) : PersistentSettings` — the settings tree, whose typed writer surface `api-rhinocommon-persistence.md` owns
- `PlugIn.SettingsSaved : EventHandler<PersistentSettingsSavedEventArgs>` — fires the change notification `api-rhinocommon-persistence.md` owns; `RaiseOnPlugInSettingsSavedEvent()` / `FlushSettingsSavedQueue()` raise and drain it

[ENTRYPOINT_SCOPE]: render plug-in
- `protected abstract Render(RhinoDoc doc, RunMode mode, bool fastPreview) : Result` / `protected virtual RenderWindow(RhinoDoc doc, RunMode modes, bool fastPreview, RhinoView view, Rectangle rect, bool inWindow[, bool blowup]) : Result` — the render entry
- `protected virtual CreatePreview(CreatePreviewEventArgs args) : void` / `CreateTexture2dPreview(CreateTexture2dPreviewEventArgs args) : void` / `PreviewRenderType() : PreviewRenderTypes`
- `protected virtual SupportsFeature(RenderFeature feature) : bool` / `CurrentRendererSupportsFeature(RenderFeature feature) : bool` (static) / `SupportedOutputTypes() : List<Rhino.FileIO.FileType>` / `SupportedChannels : Guid[]` / `InitialChannelToDisplay : Guid` / `CustomChannelName(Guid id) : string`
- `protected virtual RegisterRenderPanels(RenderPanels panels) : void` / `RegisterRenderTabs(RenderTabs tabs) : void` / `RenderSettingsSections() : List<Guid>` / `RenderContentSerializers() : IEnumerable<RenderContentSerializer>` / `UiContentTypes() : List<Guid>` / `RegisterCustomRenderSaveFileTypes(CustomRenderSaveFileTypes saveFileTypes) : void`
- material UI: `OnAssignMaterial(nint parent, RhinoDoc doc, ref Material material) : bool` / `OnEditMaterial(...)` / `OnCreateMaterial(...)` and their `Enable*Button` gates; `SunCustomSections(List<ICollapsibleSection> sections)` / `RenderSettingsCustomSections(...)`; the render-content and change-queue surfaces are owned by `api-rhinocommon-rendercontent.md` and the render catalogs

[ENTRYPOINT_SCOPE]: digitizer plug-in
- `protected abstract DigitizerUnitSystem : UnitSystem` / `PointTolerance : double` / `EnableDigitizer(bool enable) : bool` — the digitizer contract
- `DigitizerPlugIn.SendPoint(Point3d point, MouseButton mousebuttons, bool shiftKey, bool controlKey) : void` / `SendRay(Ray3d ray, MouseButton mousebuttons, bool shiftKey, bool controlKey) : void` — feed digitized input into the host

[ENTRYPOINT_SCOPE]: file-dialog registration and dispatch
- `new FileTypeList(string description, string extension)` — a registration list seeded with one type
- `FileTypeList.AddFileType(string description, string extension) : int` / `AddFileType(string description, string extension, bool showOptionsButtonInFileDialog) : int` / `AddFileType(string description, string extension1, string extension2) : int` / `AddFileType(string description, IEnumerable<string> extensions, bool showOptionsButtonInFileDialog) : int` — each returns the dispatch index the later read/write receives
- `new FileType(string extension, string description)` / `FileType.Description : string` / `FileType.Extension : string`
- `protected abstract FileImportPlugIn.AddFileTypes(FileReadOptions options) : FileTypeList` / `ReadFile(string filename, int index, RhinoDoc doc, FileReadOptions options) : bool` / `protected virtual DisplayOptionsDialog(nint parent, string description, string extension) : void` / `MakeReferenceTableName(RhinoDoc doc, string nameToPrefix) : string`
- `protected abstract FileExportPlugIn.AddFileTypes(FileWriteOptions options) : FileTypeList` / `WriteFile(string filename, int index, RhinoDoc doc, FileWriteOptions options) : WriteFileResult` / `ShouldDisplayOptionsDialog : bool`
- `new FileReadOptions()` / `Dispose() : void`; `ImportMode` / `OpenMode` / `NewMode` / `InsertMode` / `ImportReferenceMode` / `BatchMode : bool` (get/set) — the host intent axes an engine consults for merge-versus-open behavior; `UseScaleGeometry` / `ScaleGeometry : bool`; `OptionsDictionary : ArchivableDictionary` — the per-format option payload the dialog lane threads

[ENTRYPOINT_SCOPE]: licensing
- `PlugIn.GetLicense(LicenseCapabilities licenseCapabilities, string textMask, ValidateProductKeyDelegate validateProductKeyDelegate, OnLeaseChangedDelegate leaseChanged) : bool` / the `LicenseBuildType` overload / `AskUserForLicense(...) : bool` / `ReturnLicense() : bool` / `GetLicenseOwner(out string registeredOwner, out string registeredOrganization) : bool` / `SetLicenseCapabilities(string textMask, LicenseCapabilities capabilities, Guid licenseId) : void`
- `LicenseUtils.GetLicenseStatus() : LicenseStatus[]` / `GetOneLicenseStatus(Guid productid) : LicenseStatus` / `GetLicenseCapabilities(int filter) : LicenseCapabilities` / `CheckOutLicense(Guid productId) : bool` / `CheckInLicense(Guid productId) : bool` / `ReturnLicense(Guid productId) : bool` / `ConvertLicense(Guid productId) : bool` / `IsCheckOutEnabled() : bool`
- `LicenseUtils.LoginToCloudZoo() : bool` / `LogoutOfCloudZoo() : bool` / `ShowBuyLicenseUi(Guid productId) : void` / `ShowLicenseValidationUi(string cdkey) : bool`
- `LicenseData.IsValid([bool ignoreExpirationDate]) : bool` / `IsValid(LicenseData data) : bool` (static) / the `ProductLicense`/`SerialNumber`/`LicenseTitle`/`BuildType`/`LicenseCount`/`DateToExpire`/`RequiresOnlineValidation` get/set surface; `LicenseStatus.LicenseType : LicenseType` / `ExpirationDate : DateTime?` / `CloudZooLeaseIsValid : bool`; `LicenseLease.LeaseId` / `GroupName` / `UserName` / `Expiration : DateTime`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one `PlugIn` per package: identity a `Guid`, capability a `protected virtual` hook the host invokes on the declared `PlugInLoadTime` schedule, never a scattered registration call
- `RenderPlugIn`, `DigitizerPlugIn`, `FileImportPlugIn`, and `FileExportPlugIn` subclass `PlugIn` with their own abstract contract; a package picks one base per concern
- file-dialog dispatch is index-keyed: `AddFileType` returns the index the later `ReadFile`/`WriteFile` receives, and the plug-in discriminates on that index, never a re-parsed path extension
- per-plug-in document state serializes through `ShouldCallWriteDocument`→`WriteDocument`/`ReadDocument` against the host `BinaryArchive*`
- settings are a `PersistentSettings` tree keyed by plug-in id; license capability is a `LicenseCapabilities` flag set acquired through the Zoo/CloudZoo rail, never a hand-rolled key check

[STACKING]:
- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): the `ReadFile` `Result` and `WriteFile` `WriteFileResult` fold to `Fin<A>`, the registered-index-to-handler binding is a `HashMap` lookup, `Find`/`GetPluginSettings` nullable resolves fold to `Option<A>`, and the license `GetLicense`/`CheckOutLicense` bool outcomes fold to `Fin<Unit>` carrying the fault
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the registered file-type roster wraps as a keyed `SmartEnum` indexed by the `AddFileType` return, collapsing extension, description, and index into one owner the dispatch switches on; `PlugInType`, `PlugInLoadTime`, `LicenseType`, `LicenseBuildType`, and `LicenseCapabilities` map to `SmartEnum`/flag owners, and a plug-in `Guid` wraps as a `ValueObject<Guid>`
- `api-rhinocommon-fileio.md`: `FileWriteOptions`/`WriteFileResult` write-policy detail and the direct format engines compose there; this catalog owns only their dispatch-contract role
- `api-rhinocommon-persistence.md`: `PersistentSettings` node and `SettingsSaved` event compose there; `api-rhinocommon-rendercontent.md` and the render catalogs own the `RenderPlugIn` content-registration and change-queue surfaces

[LOCAL_ADMISSION]:
- a package binds into the host through exactly one `PlugIn` subclass per concern, overriding its virtual hooks, never a free-standing registration call
- file-dialog participation enters through one `FileImportPlugIn`/`FileExportPlugIn` declaring its `FileTypeList`; the registered index is the single dispatch key
- license acquisition enters through `PlugIn.GetLicense`/`LicenseUtils`, and settings custody routes to `api-rhinocommon-persistence.md`; direct `File3dm` and engine invocation stay the package-driven path of `api-rhinocommon-fileio.md`

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: the plug-in base and its render/digitizer/file-import/file-export specializations, native file-dialog registration and dispatch, and the full license surface
- Accept: schedule-driven load, virtual-hook capability, index-keyed file dispatch, per-plug-in document serialization, Zoo/CloudZoo license acquisition
- Reject: scattered registration outside the plug-in owner, path-string dispatch, standalone archive reads, and re-documentation of the write-policy, persistence, and render-content surfaces other catalogs own
