# [RASM_RHINO_API_RHINOCOMMON_RUNTIME]

`Rhino.Runtime` owns the in-process host environment beneath every other catalog: platform-service resolution, assembly loading, the native-pointer marshal seam, and the `CommonObject` const/non-const lifetime base every geometry and document handle derives from. Every managed handle crosses to native as an `nint` marshalled through `Interop`, and the domain never touches a native pointer the `InteropWrappers` disposable family does not first wrap.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon host-runtime surface
- host: Rhino host runtime, in-process (proprietary McNeel SDK)
- assembly: `RhinoCommon.dll` — in-process managed host runtime
- namespaces: `Rhino.Runtime`, `Rhino.Runtime.InProcess`, `Rhino.Runtime.InteropWrappers`, `Rhino.Runtime.Notifications`, `Rhino.Runtime.RhinoAccounts`, `Rhino.NodeInCode`
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: host environment and platform services

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :------------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `HostUtils`                | static class  | runtime environment, assembly loading, named callbacks, compute endpoints       |
|  [02]   | `Interop`                  | static class  | managed-handle to native-pointer marshal seam across every host type            |
|  [03]   | `AssemblyResolver`         | static class  | search-folder/search-file assembly resolution hooks                             |
|  [04]   | `IPlatformServiceLocator`  | interface     | platform-service provider `HostUtils.GetPlatformService<T>` resolves            |
|  [05]   | `IShrinkWrapService`       | interface     | shrink-wrap mesh service contract resolved through the locator                  |
|  [06]   | `IZooClientUtilities`      | interface     | Zoo network-license client-utility service contract                             |
|  [07]   | `CommonObject`             | class         | abstract const/non-const native-lifetime base for geometry and doc handles      |
|  [08]   | `RiskyAction`              | class         | disposable caller-scoped guard bracketing a native call that may fault          |
|  [09]   | `NamedParametersEventArgs` | class         | disposable typed named-parameter dictionary for host named callbacks            |
|  [10]   | `Mode`                     | enum          | runtime execution mode (`NormalMode`/`ViewerMode`/`BetaMode`)                   |
|  [11]   | `AdvancedSetting`          | enum          | advanced-settings key vocabulary for `HostUtils` advanced reads                 |
|  [12]   | `ImportOptionsSections`    | enum          | options-dialog section discriminant for import-options routing                  |
|  [13]   | `HostUtils.LogMessageType` | enum          | cloud log-message severity (`unknown`/`information`/`warning`/`error`/`assert`) |

[PUBLIC_TYPE_SCOPE]: scripting, skin, capture, and licensing

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `PythonScript`                 | class         | abstract Python host — compile, variable scope, execute, evaluate                      |
|  [02]   | `PythonCompiledCode`           | class         | abstract compiled-script handle executed against a `PythonScript` scope                |
|  [03]   | `Skin`                         | class         | abstract application-skin singleton — splash, icon, load-phase hooks                   |
|  [04]   | `ViewCaptureWriter`            | class         | abstract vector-capture sink turning a print frame into drawn primitives               |
|  [05]   | `CloudHostUtils`               | static class  | compute-cloud entitlement and signature gate                                           |
|  [06]   | `ZooClientParameters`          | class         | Zoo product/license descriptor carrying validation delegates                           |
|  [07]   | `LicenseStateChangedEventArgs` | class         | payload flagging whether calling RhinoCommon is currently allowed                      |
|  [08]   | `LicenseTypes`                 | enum          | license origin (`Undefined`/`Standalone`/`ZooAutoDetect`/`ZooManualDetect`/`CloudZoo`) |

[PUBLIC_TYPE_SCOPE]: typed host failures

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `CorruptGeometryException`   | class         | native geometry-corruption fault surfaced managed  |
|  [02]   | `DocumentCollectedException` | class         | collected-document access fault surfaced managed   |
|  [03]   | `NotLicensedException`       | class         | licensing fault raised at the host boundary        |
|  [04]   | `RdkNotLoadedException`      | class         | RDK-availability fault raised at the host boundary |

[PUBLIC_TYPE_SCOPE]: in-process host boot (`Rhino.Runtime.InProcess`)

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :-------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `RhinoCore`           | class         | disposable in-process headless Rhino boot with host-context marshalling  |
|  [02]   | `Interop`             | static class  | `StartupInProcess`/`LaunchInProcess` native startup extern surface       |
|  [03]   | `Interop.HRESULT`     | static class  | native `S_OK`/`E_*` result constants                                     |
|  [04]   | `Interop.StartupInfo` | struct        | Win32 `STARTUPINFO` mirror passed into `StartupInProcess`                |
|  [05]   | `WindowStyle`         | enum          | boot window style (`NoWindow`/`Normal`/`Hidden`/`Minimized`/`Maximized`) |
|  [06]   | `StartupOrigin`       | enum          | boot origin (`NotStarted`/`Application`/`Library`)                       |

[PUBLIC_TYPE_SCOPE]: native marshal family (`Rhino.Runtime.InteropWrappers`)

`SimpleArray*` and `StdVector*` are the disposable managed↔native array bridges every host P/Invoke threads geometry, primitive, and pointer collections through, each holding a `ConstPointer()`/`NonConstPointer()` pair, an `Add`, and a materializing `ToArray()`. `StringHolder`/`StringWrapper` bridge native `ON_wString` text, `ClassArray*` bridge managed reference collections, and the value structs carry native measurement payloads.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :---------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `SimpleArray<T>` family | class         | disposable primitive/geometry/pointer native-array marshal bridges |
|  [02]   | `StdVector<T>` family   | class         | disposable `std::vector` marshal bridges                           |
|  [03]   | `ClassArrayString`      | class         | disposable native string-array bridge                              |
|  [04]   | `ClassArrayObjRef`      | class         | disposable native `ObjRef`-array bridge                            |
|  [05]   | `ClassArrayOnObjRef`    | class         | disposable native `ON_ObjRef`-array bridge                         |
|  [06]   | `StringHolder`          | class         | disposable native `ON_wString` reader                              |
|  [07]   | `StringWrapper`         | class         | disposable read/write native `ON_wString` bridge                   |
|  [08]   | `CurveSegment`          | struct        | curve-region segment (`Index`/`SubDomain`/`Reversed`)              |
|  [09]   | `MeshPointDataStruct`   | struct        | native mesh-point evaluation payload                               |
|  [10]   | `RhDisplayPoint`        | struct        | native display-point marshal payload                               |

[PUBLIC_TYPE_SCOPE]: notifications and node-in-code

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `Notifications.NotificationCenter`            | static class  | one `public static readonly` set; membership is what renders       |
|  [02]   | `Notifications.Notification`                  | class         | assembly-restricted notification with modal show/hide and metadata |
|  [03]   | `Notifications.Notification.Severity`         | nested enum   | `Debug`/`Info`/`Warning`/`Serious`/`Critical`                      |
|  [04]   | `TrulyObservableOrderedSet<T>`                | class         | the center's `IList<T>` + `INotifyCollectionChanged` backing store |
|  [05]   | `Notifications.NotificationButtonClickedArgs` | class         | free `EventArgs` carrier; NOT the `ButtonClicked` delegate payload |
|  [06]   | `Notifications.ButtonType`                    | enum          | notification button (`CancelOrClose`/`Confirm`/`Alternate`)        |
|  [07]   | `Notifications.IAssemblyRestrictedObject`     | interface     | sole member `Editable() -> bool`; the mutation guard's contract    |
|  [08]   | `NodeInCode.Components`                       | class         | abstract node-in-code component root exposing the function table   |
|  [09]   | `NodeInCode.ComponentFunctionInfo`            | class         | callable Grasshopper-component descriptor with typed IO metadata   |
|  [10]   | `NodeInCode.NodeInCodeTable`                  | class         | dynamic dispatch table of component functions keyed by full name   |

[PUBLIC_TYPE_SCOPE]: account tokens (`Rhino.Runtime.RhinoAccounts`)

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                                   |
| :-----: | :--------------------------- | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `RhinoAccountsManager`       | static class  | secret-key-scoped OAuth2/OpenID token acquisition and revoke                   |
|  [02]   | `IRhinoAccountsManager`      | interface     | accounts-manager service contract                                              |
|  [03]   | `IOAuth2Token`               | interface     | OAuth2 access-token handle                                                     |
|  [04]   | `IOpenIDConnectToken`        | interface     | OpenID Connect identity-token handle                                           |
|  [05]   | `SecretKey`                  | class         | scoped secret-key capability handed to protected code                          |
|  [06]   | `RhinoAccountsGroup`         | class         | account group identity (`Id`/`Name`)                                           |
|  [07]   | `RhinoAccoountsProgressInfo` | class         | login-progress payload (host-misspelled type name)                             |
|  [08]   | `ProgressState`              | enum          | login progress (`AwaitingLogin`/`RetrievingTokens`/`AwaitingRedirect`/`Other`) |
|  [09]   | `RhinoAccountsException`     | class         | base of the accounts fault hierarchy                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: platform services, assembly loading, and process facts
- `HostUtils.GetPlatformService<T>(string, string) -> T where T : class` — resolve a platform service, the single seam `IShrinkWrapService`/`IZooClientUtilities` and OS-info reads route through
- `IPlatformServiceLocator.GetService<T>() -> T where T : class` — the locator's sole member; a platform assembly implements it once and `GetPlatformService` reaches every service through that one call
- `HostUtils.LoadAssemblyFrom(string) -> Assembly` / `LoadAssemblyFromStream(Stream) -> Assembly` / `LoadAssemblyFromName(AssemblyName) -> Assembly` — collectible-context assembly loading
- `HostUtils.RunningInMono -> bool` / `RunningOnServer -> bool` / `RunningInDarkMode -> bool` / `IsPreRelease -> bool` / `CurrentOSLanguage -> uint` — process discriminants
- `HostUtils.OperatingSystemEdition` / `OperatingSystemProductName` / `OperatingSystemBuildNumber` / `OperatingSystemInstallationType -> string` — OS identity via the platform service
- `HostUtils.GetSystemProcessorCount() -> int` / `GetCurrentProcessInfo(out string, out Version) -> void` / `GetAssemblySearchPaths() -> string[]` / `GetSystemReferenceAssemblies() -> IEnumerable<string>`
- `HostUtils.GetPrinterNames() -> string[]` / `GetPrinterFormNames(string) -> string[]` / `GetPrinterFormSize(string, string, out double widthMm, out double heightMm) -> bool` / `GetPrinterFormMargins(...) -> bool` / `GetPrinterDPI(string, bool) -> double`
- `AssemblyResolver.AddSearchFolder(string) -> void` / `AddSearchFile(string) -> void`; `CurrentDomainAssemblyResolve` / `CurrentDomainReflectionOnlyAssemblyResolve -> ResolveEventHandler` — the resolve hooks the host installs

[ENTRYPOINT_SCOPE]: native-pointer marshal seam
- `Interop.NativeGeometryConstPointer(GeometryBase) -> nint` / `NativeGeometryNonConstPointer(GeometryBase) -> nint` / `CreateFromNativePointer(nint) -> GeometryBase` — geometry pointer round-trip
- `Interop.NativeRhinoDocPointer(RhinoDoc) -> nint` / `RhinoObjectConstPointer(RhinoObject) -> nint` / `RhinoObjectFromPointer(nint) -> RhinoObject`
- `Interop.NativeNonConstPointer(ViewCaptureSettings)` / `(ViewportInfo)` / `(RhinoViewport)` / `(DisplayPipeline)` / `(GetPoint) -> nint` — polymorphic host-handle pointer accessor
- `Interop.FileWriteOptionsConstPointer(FileWriteOptions)` / `FileReadOptionsConstPointer(FileReadOptions)` / `PlugInPointer(PlugIn) -> nint`
- `Interop.FontFromPointer(nint) -> Rhino.DocObjects.Font` / `ViewCaptureFromPointer(nint) -> ViewCaptureSettings` / `NSFontFromFont(Rhino.DocObjects.Font[, double]) -> nint`
- `Interop.FromOnBrep(object) -> Brep` / `FromOnMesh(object) -> Mesh` / `FromOnSurface(object) -> Surface` / `FromOnCurve(object) -> Curve` and the `ToOnBrep`/`ToOnMesh`/`ToOnSurface`/`ToOnCurve`/`ToOnXform` reverse family bridging opennurbs `ON_*` objects
- `SimpleArrayPoint3d()` / `SimpleArrayPoint3d(IEnumerable<Point3d>)` / `Add(Point3d) -> void` / `ToArray() -> Point3d[]` / `ConstPointer() -> nint` / `NonConstPointer() -> nint` / `Dispose() -> void` — the representative `SimpleArray*` marshal shape every family member repeats
- `StringHolder.GetString(nint) -> string` / `ToStringSafe() -> string`; `StringWrapper(string)` / `SetString(string) -> void` / `GetStringFromPointer(nint) -> string`

[ENTRYPOINT_SCOPE]: named callbacks and compute endpoints
- `HostUtils.RegisterNamedCallback(string, EventHandler<NamedParametersEventArgs>) -> void` / `RemoveNamedCallback(string) -> void` / `ExecuteNamedCallback(string, NamedParametersEventArgs) -> bool`
- `HostUtils.RegisterComputeEndpoint(string, Type) -> void` / `GetCustomComputeEndpoints() -> Tuple<string, Type>[]`
- `HostUtils.OnExceptionReport -> ExceptionReportDelegate` / `ExceptionReport(string, Exception) -> void` / `OnSendLogMessageToCloud -> SendLogMessageToCloudDelegate` — process-wide exception and cloud-log taps
- `HostUtils.CheckForRdk(bool, bool) -> bool` / `RegisterStaticIDisposable(IDisposable) -> void`
- `NamedParametersEventArgs()` / `Dispose() -> void`; no generic accessor exists — reads are the NAMED per-type roster `TryGetString`/`TryGetStrings`/`TryGetBool`/`TryGetInt`/`TryGetUnsignedInt`/`TryGetUints`/`TryGetDouble`/`TryGetGuid`/`TryGetGuids`/`TryGetColor`/`TryGetPoint2i`/`TryGetPoint`/`TryGetPoints`/`TryGetVector`/`TryGetLine`/`TryGetArc`/`TryGetPlane`/`TryGetViewport`/`TryGetGeometry`/`TryGetObjRefs`/`TryGetRhinoObjects`/`TryGetMeshParameters`/`TryGetUnmangedPointer` (each `(string, out T) -> bool`; the host misspells `Unmanged`), and writes are overloaded `Set(string, T)` spanning `string`/`IEnumerable<string>`/`bool`/`int`/`uint`/`IEnumerable<uint>`/`double`/`Guid`/`IEnumerable<Guid>`/`Color`/`System.Drawing.Point`/`Point3d`/`Point3d[]`/`Vector3d`/`Line`/`Arc`/`Plane`/`GeometryBase`/`IEnumerable<GeometryBase>`/`IEnumerable<ObjRef>`/`MeshingParameters`, with `SetWindowHandle`/`SetWindowImageHandle`/`TryGetWindowHandle`/`TryGetWindowImageHandle` for native handles; `ViewportInfo` is read-only (`TryGetViewport` has no `Set` counterpart)

[ENTRYPOINT_SCOPE]: scripting host
- `PythonScript.Create() -> PythonScript` — mint the host scripting engine, null when the host ships without the Python plug-in; `RuntimeAssemblies() -> Assembly[]` / `AddRuntimeAssembly(Assembly) -> void` / `SearchPaths -> string[]` static get-set
- `PythonScript.Compile(string) -> PythonCompiledCode` / `ExecuteScript(string) -> bool` / `ExecuteFile(string) -> bool` / `ExecuteFileInScope(string) -> bool` / `EvaluateExpression(string, string) -> object`
- `PythonScript.SetVariable(string, object) -> void` / `GetVariable(string) -> object` / `ContainsVariable(string) -> bool` / `GetVariableNames() -> IEnumerable<string>` / `RemoveVariable(string) -> void`
- `PythonScript.Output -> Action<string>` / `ScriptContextDoc -> object` / `ScriptContextCommand -> Command` / `ContextId -> int`; `GetStackTraceFromException(Exception) -> string`; `PythonCompiledCode.Execute(PythonScript) -> void`

[ENTRYPOINT_SCOPE]: skin, vector capture, and risky-call guard
- `Skin.ActiveSkin -> Skin` — process skin singleton; `protected virtual MainRhinoIcon -> Bitmap` / `ApplicationName -> string`; the load-phase overrides `OnMainFrameWindowCreated`/`OnLicenseCheckCompleted`/`OnBuiltInCommandsRegistered`/`OnBeginLoadAtStartPlugIns(int)`/`OnBeginLoadPlugIn(string)`/`OnEndLoadPlugIn`/`OnEndLoadAtStartPlugIns`, with `ShowSplash`/`HideSplash`/`ShowHelp`
- `ViewCaptureWriter(double, Size)` / `Draw(nint constPtrPrintInfo, RhinoDoc) -> void` — subclass and override the `protected abstract` primitive sinks `DrawPath`/`DrawCircle`/`DrawRectangle`/`DrawBitmap`/`DrawScreenText`/`FillPolygon`/`SetClipPath`/`DrawGradientHatch`, with `protected virtual SupportsArc()` gating arc primitives, `protected Dpi`/`PageSize` and `Flush`/`PushClipPath`/`PopClipPath` helpers, and the public nested `Pen`/`PathPoint`/`PointType` carriers — `ViewCaptureBrush` is a PRIVATE nested class and is not a carrier a subclass can name. UNREACHABLE by construction: `Draw` is the only drive entry, it takes the native `CRhinoPrintInfo` pointer, `Rhino.Display.ViewCaptureSettings.ConstPointer()` is `internal`, and no public member of `Rhino.Display.ViewCapture` accepts a `ViewCaptureWriter`, so a subclass compiles and can never be handed a frame
- `new RiskyAction(string description, [CallerFilePath] string = "", [CallerMemberName] string = "", [CallerLineNumber] int = 0)` / `Dispose() -> void` — brackets a native call that may fault so the host records provenance

[ENTRYPOINT_SCOPE]: in-process headless boot
- `new RhinoCore()` / `RhinoCore(string[])` / `RhinoCore(string[], WindowStyle)` / `RhinoCore(string[], WindowStyle, nint)` — boot an in-process Rhino
- `RhinoCore.Run() -> int` / `RaiseIdle() -> void` / `DoIdle() -> bool` / `DoEvents() -> bool` / `Dispose() -> void`
- `RhinoCore.InvokeInHostContext(Action) -> void` / `InvokeInHostContext<T>(Func<T>) -> T` — marshal a call onto the host thread
- `InProcess.Interop.StartupInProcess(int, string[], ref StartupInfo, nint) -> void` / `LaunchInProcess(int, int) -> int` / `RunMessageLoop() -> int` / `ShutdownInProcess() -> void` / `EnterHostContext()` / `ExitHostContext()` / `StartedAsRhinoExe() -> bool`

[ENTRYPOINT_SCOPE]: notifications and node-in-code
- `NotificationCenter.Notifications -> TrulyObservableOrderedSet<Notification>` — one `static readonly` set holds every notification; `Add(Notification) -> void` (no guard check, no-op on a duplicate) and `Remove(Notification) -> bool` / `RemoveAt(int)` / `Clear()` (each `_ThrowIfIllegalAssembly` first) are membership, and MEMBERSHIP IS RENDERING — a notification never added renders nowhere and its `ShowModal` queues against nothing
- `new Notification()` / `Notification(IEnumerable<Assembly> allowedAssemblies)` — `Notification()` delegates `null`, so `AllowedAssemblies` lands empty and every assembly may edit; a non-empty set restricts editing to those assemblies
- `Notification` mutable surface, EVERY setter routed through `_ThrowIfIllegalAssembly`: `Title` / `Description` / `Message` / `ConfirmButtonTitle` / `CancelButtonTitle` / `AlternateButtonTitle -> string` get-set, `SeverityLevel -> Notification.Severity` get-set (host default `Info`), `ButtonClicked -> Action<ButtonType>` get-set (a BARE `ButtonType`, never `NotificationButtonClickedArgs`), `this[string key] -> string` metadata get-set, and `RemoveMetadata(string) -> bool`
- `Notification.HideModal() -> void` is guarded, `ShowModal() -> void` is NOT (it writes `ShowEventId` with the assembly check off), so hide and every field write need the guard and show does not
- `Notification` read surface: `AllowedAssemblies -> ICollection<Assembly>` get-only (a `ReadOnlyCollection` of the distinct ctor set) / `MetadataCopy -> IDictionary<string, string>` get-only detached copy / `ShowEventId -> Guid?` get-only / `DateUpdated -> DateTime` get-only (visible-property writes stamp it, metadata writes do not); `PropertyChanged -> PropertyChangedEventHandler` fires per visible-property change
- `Notification.ExecuteAssemblyProtectedCode(Action) -> void` / `ExecuteAssemblyProtectedCode<TResult>(Func<TResult>) -> TResult` — both `public static`, NOT instance. Each pushes the delegate's `MethodBase` on a process-static stack for the call's duration, and `Editable()` passes only when some frame on that stack declares in an allowed assembly, so the guard admits by the LAMBDA'S OWN assembly: a restricted notification whose allowed set omits the mutating assembly throws `InvalidOperationException` even inside the wrapper
- `Components.NodeInCodeFunctions -> NodeInCodeTable` / `Components.FindComponent(string) -> ComponentFunctionInfo`
- `ComponentFunctionInfo.Invoke(params object[]) -> object[]` / `InvokeKeepTree(params object[]) -> object[]` / `InvokeSilenceWarnings(...)` / `Evaluate(IEnumerable, bool, out string[]) -> object[]`; `Name` / `Namespace` / `Description -> string` / `InputNames` / `OutputNames -> IReadOnlyList<string>` / `InputsOptional -> IReadOnlyList<bool>` / `ComponentGuid -> Guid` / `Delegate` / `DelegateTree -> Delegate`
- `NodeInCodeTable.Contains(string) -> bool` / `Add(ComponentFunctionInfo) -> void` / `GetDynamicMembers() -> IEnumerable<ComponentFunctionInfo>` / `Count -> int` — the `DynamicObject` dispatch reaches functions by member name

[ENTRYPOINT_SCOPE]: account tokens and licensing
- `RhinoAccountsManager.ExecuteProtectedCode(Action<SecretKey>) -> void` / `ExecuteProtectedCodeAsync(Func<SecretKey, Task>) -> Task` — the `SecretKey` only exists inside the protected callback
- `RhinoAccountsManager.GetAuthTokensAsync(string, string, SecretKey, CancellationToken) -> Task<Tuple<IOpenIDConnectToken, IOAuth2Token>>` / the scoped/prompt overload / `TryGetAuthTokens(string, SecretKey) -> Tuple<IOpenIDConnectToken, IOAuth2Token>`
- `RhinoAccountsManager.RevokeAuthTokenAsync(IOAuth2Token, SecretKey, CancellationToken) -> Task` / `UpdateOpenIDConnectTokenAsync(IOpenIDConnectToken, IOAuth2Token, SecretKey, CancellationToken) -> Task<IOpenIDConnectToken>`
- `IRhinoAccountsManager` restates that roster as the service contract — `ExecuteProtectedCode(Action<SecretKey>) -> void` / `ExecuteProtectedCodeAsync(Func<SecretKey, Task>) -> Task` / `TryGetAuthTokens(string, [IEnumerable<string> scope,] SecretKey) -> Tuple<IOpenIDConnectToken, IOAuth2Token>` / `GetAuthTokensAsync(string clientId, string clientSecret, [IEnumerable<string> scope, string prompt, int? maxAge, bool showUI, IProgress<RhinoAccoountsProgressInfo> progress,] SecretKey, CancellationToken) -> Task<Tuple<IOpenIDConnectToken, IOAuth2Token>>` / `UpdateOpenIDConnectTokenAsync(...)` / `RevokeAuthTokenAsync(...)` — the static class forwards to it, so a `SecretKey` argument is unforgeable outside the protected callback and the progress overload is the only one reporting
- `RhinoAccoountsProgressInfo(ProgressState state, Dictionary<object, object> metadata = null, string customDescription = null)` / `State -> ProgressState` / `Description -> string` / `Metadata -> Dictionary<object, object>` — the progress payload; `Metadata` hands back a fresh copy on every read and a null `customDescription` derives a localized description from `State`
- `ProgressState` — `AwaitingLogin`, `RetrievingTokens`, `AwaitingRedirect`, `Other`; `AwaitingRedirect` is a distinct browser-handoff stage, not a synonym of `AwaitingLogin`
- `RhinoAccountsGroup(string id, string name)` / `Id -> string` / `Name -> string` — a get-only account-group identity pair with a public constructor
- `RhinoAccountsException(string message, Exception innerException = null)` / `RhinoAccountsException(Exception innerException = null)` — `[Serializable]`, the message-free overload supplying a localized default; the concrete faults are `RhinoAccountsAuthTokenMismatchException`, `RhinoAccountsCannotListenException`, `RhinoAccountsInvalidResponseException`, `RhinoAccountsInvalidStateException`, `RhinoAccountsInvalidTokenException`, `RhinoAccountsOperationInProgressException`, `RhinoAccountsProxyException`, `RhinoAccountsServerException`, and `RhinoAccountsServerNotReachableException`, so a rail discriminates on the subclass and never on message text
- `CloudHostUtils.IsEntitled -> bool` / `DenyReason -> string` / `Signature -> string` / `CheckEntitlement() -> void` — the compute-cloud entitlement gate

[ENTRYPOINT_SCOPE]: the Zoo client contract and its parameter carrier
- `ZooClientParameters(Guid productGuid, Guid licenseGuid, string productTitle, int productBuildType, LicenseCapabilities capabilities, string licenseEntryTextMask, string productPath, object parentWindow, LicenseTypes selectedLicenseType, ValidateProductKeyDelegate, OnLeaseChangedDelegate, VerifyLicenseKeyDelegate, VerifyPreviousVersionLicenseDelegate)` — the whole carrier is constructor-seeded
- `ZooClientParameters.ProductGuid` / `LicenseGuid -> Guid` / `ProductTitle` / `LicenseEntryTextMask` / `ProductPath -> string` / `ProductBuildType -> int` / `ParentWindow -> object` / `OnLeaseChanged -> OnLeaseChangedDelegate` — get-only identity, presentation, and lease-callback slots
- `ZooClientParameters.Capabilities -> LicenseCapabilities` / `SelectedLicenseType -> LicenseTypes` — the two SETTABLE slots; the carrier is documented read-only so the flow it propagates through cannot be mutated mid-acquisition, and these two are the exception the license UI writes
- `ZooClientParameters.VerifyLicenseKey(string licenseKey, string validationCode, DateTime validationCodeInstallDate, bool gracePeriodExpired, out LicenseData licenseData) -> ValidateResult` — prefers `VerifyLicenseKeyDelegate`, falls back to `ValidateProductKeyDelegate`, and throws `InvalidOperationException` when the constructor seeded neither, so the delegate pair is a construction-time obligation
- `ZooClientParameters.VerifyPreviousVersionLicense(string license, string previousVersionLicense, out string errorMessage) -> bool` — answers `false` with a localized `errorMessage` when the upgrade delegate is unseeded rather than throwing
- `LicenseTypes` — `Undefined`, `Standalone`, `ZooAutoDetect`, `ZooManualDetect`, `CloudZoo`; the auto- and manual-detect Zoo rows are distinct acquisition paths
- `LicenseStateChangedEventArgs(bool callingRhinoCommonAllowed)` / `CallingRhinoCommonAllowed -> bool` — get-only; the event is the gate telling managed code whether any further RhinoCommon call is admitted
- `IZooClientUtilities.Initialize(object verify) -> bool` / `Echo(object verify, string message) -> string` / `GetCurrentTime() -> DateTime` — client bring-up, round-trip probe, and the authoritative clock; the leading `object verify` token gates every member
- `IZooClientUtilities.GetLicense(object verify, ZooClientParameters parameters) -> bool` / `AskUserForLicense(object, ZooClientParameters) -> bool` / `LicenseOptionsHandler(object, ZooClientParameters) -> bool` — the three acquisition entries, each threading one parameter carrier
- `IZooClientUtilities.ReturnLicense(object verify, [string productPath,] Guid productId) -> bool` / `CheckOutLicense` / `CheckInLicense` / `ConvertLicense` / `DeleteLicense(object verify, Guid productId) -> bool` / `IsCheckOutEnabled(object verify) -> bool` — the entitlement lifecycle keyed by product id
- `IZooClientUtilities.GetLicenseStatus(object verify) -> LicenseStatus[]` / `GetOneLicenseStatus(object verify, Guid productId) -> LicenseStatus` / `GetLicenseType(object verify, Guid productId) -> int` / `GetRegisteredOwnerInfo(object verify, Guid productId, ref string registeredOwner, ref string registeredOrganization) -> bool` — status reads; `GetLicenseType` answers a bare `int` the caller maps onto `LicenseTypes`
- `IZooClientUtilities.LoggedInUserName -> string` / `LoggedInUserAvatar -> Image` / `UserIsLoggedIn -> bool` / `LoginToCloudZoo() -> bool` / `LogoutOfCloudZoo() -> bool` — CloudZoo session state and the two login members, which take no `verify` token
- `IZooClientUtilities.ShowRhinoExpiredMessage(Mode mode, ref int result) -> bool` / `ShowBuyLicenseUi(object verify, Guid productId) -> void` / `ShowLicenseValidationUi(object verify, string cdkey) -> bool` — the host-driven license UI

[ENTRYPOINT_SCOPE]: `CommonObject` lifetime base
- `CommonObject.IsValidWithLog(out string) -> bool` — per-object validity read every geometry and document handle inherits; `IsDocumentControlled -> bool` distinguishes a native handle the document owns from a private copy
- `CommonObject.EnsurePrivateCopy() -> void` — detach a document-controlled handle into a writable private copy before mutation
- `CommonObject.ToJSON(SerializationOptions) -> string` / `FromJSON(string) -> CommonObject` / `FromBase64String(int, int, string) -> CommonObject` / `ComputeMemoryEstimate(CommonObject) -> uint`
- `CommonObject.UserData -> UserDataList` / `UserDictionary` custody routes to `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md`; this catalog names the base, that catalog owns the attachment surface

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one host boundary crosses through `Interop`: a managed handle projects to an `nint` for a native call and a returned `nint` reconstructs a managed object; the `InteropWrappers` `SimpleArray*`/`StdVector*`/`StringHolder` disposables own every native collection or string across that call, released on a matched `Dispose`
- `CommonObject` is the const/non-const lifetime root: a document-controlled handle stays read-only until `EnsurePrivateCopy` detaches it, and `IsValidWithLog` is the one validity read the whole geometry and document hierarchy inherits
- platform capability resolves once through `HostUtils.GetPlatformService<T>`; a service such as `IShrinkWrapService` or `IZooClientUtilities` is located, never newed, so the host owns the single implementation
- `RhinoCore` is the in-process headless boot seam; every call into a booted core marshals through `InvokeInHostContext` onto the host thread rather than touching host state from an arbitrary thread
- named callbacks and compute endpoints register by string key into `HostUtils`, and `NamedParametersEventArgs` is the one typed payload crossing that key

[STACKING]:
- `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-plugins.md`: the license family splits at the caller — this catalog owns the Zoo and Rhino Accounts state (`ZooClientParameters`, `IZooClientUtilities`, `LicenseTypes`, `LicenseStateChangedEventArgs`, and the accounts token rail), and the plug-in catalog owns the `PlugIn`-facing entitlement rail above it (`PlugIn.GetLicense`, `LicenseUtils`, and the license data/status/lease carriers)
- `RhinoCommon` value substrate(`libs/dotnet/.api/api-rhinocommon.md`): the `Point3d`/`Vector3d`/`Plane`/`Line` carriers this boundary threads cross the wire from the substrate; it composes them and re-derives none.
- `LanguageExt`(`libs/dotnet/.api/api-languageext.md`): every `HostUtils`/`Interop` call returning a nullable handle or an out-`bool` folds to `Fin<A>`/`Option<A>` at the boundary, `NamedParametersEventArgs.TryGet<T>` out-parameters lift to `Option<A>`, and a host call raising `CorruptGeometryException`/`NotLicensedException`/`RdkNotLoadedException` crosses through `Op.Catch` so the exact exception-backed `Error` reaches the rail and exception control flow never enters domain code
- `Thinktecture.Runtime.Extensions`(`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`): `Mode`, `LicenseTypes`, `Notifications.ButtonType`, `RhinoAccounts.ProgressState`, `InProcess.WindowStyle`, and `AdvancedSetting` map at the edge to keyed `SmartEnum` owners, and a plugin or product `Guid` handed to `RhinoAccountsManager` wraps as a `ValueObject<Guid>` so the bare host `Guid` never leaks
- `Hashing`(`libs/dotnet/.api/api-hashing.md`): a `CommonObject.ToJSON`/`FromBase64String` serialization projects into the `XxHash128` content key the persistence artifact index dedupes host objects on
- `libs/dotnet/Rasm.Rhino/.api/api-macos-native.md`: in-process `RhinoCore.InvokeInHostContext` marshal composes the Rasm host main-thread rail rather than a bespoke dispatcher; a `ViewCaptureWriter` subclass is NOT a composition route — vector egress composes `ViewCapture.CaptureToSvg` in `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-display.md`, and a page seating the writer as a delivery row publishes capability nothing can drive

[LOCAL_ADMISSION]:
- native-pointer traffic enters through `Interop` and the `InteropWrappers` disposable family; a raw `nint` never appears in domain code, and every marshal wrapper is disposed on its owning scope
- platform capability enters through `HostUtils.GetPlatformService<T>`, never a direct service construction
- headless runtimes enter through one `RhinoCore` owner with host-context marshalling, never a per-call thread hop
- `CommonObject.UserData`/`UserDictionary` custody routes to `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md`; text-field evaluation to `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-annotation.md` and block-attribute fields to `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-blocks.md`

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: host environment and platform services, the native marshal seam and disposable array family, the `CommonObject` lifetime base, the scripting/skin/vector-capture surface, the in-process headless boot, the assembly-scoped notification center, node-in-code, and the account-token rail
- Accept: platform-service resolution, marshalled native traffic through disposable wrappers, headless boot with host-context marshalling, secret-key-scoped token acquisition, named-callback dispatch
- Reject: a raw native pointer in a domain signature, an undisposed marshal wrapper, a service construction bypassing the locator, a cross-thread host call outside `InvokeInHostContext`, and re-documentation of the user-data, text-field, or block-attribute slices other catalogs own
