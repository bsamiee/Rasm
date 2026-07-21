# [RASM_RHINO_API_RHINOCOMMON_RUNTIME]

`Rhino.Runtime` owns the in-process host environment beneath every other catalog: platform-service resolution and assembly loading through `HostUtils`, the native-pointer marshal seam of `Interop` and the `InteropWrappers` disposable-array family, the `CommonObject` const/non-const lifetime base every geometry and document handle derives from, the scripting host (`PythonScript`), the application skin and license/entitlement surface, the vector `ViewCaptureWriter` sink, the in-process headless boot (`Rhino.Runtime.InProcess.RhinoCore`), the assembly-scoped `NotificationCenter`, the Grasshopper node-in-code function table, and the OAuth account-token rail. Every managed host handle crosses this boundary as an `nint` marshalled through `Interop`, and the domain never touches a native pointer the marshal family does not first wrap.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon`
- license: proprietary host SDK
- namespace: `Rhino.Runtime`, `Rhino.Runtime.InProcess`, `Rhino.Runtime.InteropWrappers`, `Rhino.Runtime.Notifications`, `Rhino.Runtime.RhinoAccounts`, `Rhino.NodeInCode`
- asset: `RhinoCommon.dll` — the in-process managed host assembly, verified by direct decompile
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: host environment and platform services
- rail: host

| [INDEX] | [SYMBOL]                   | [KIND]    | [CAPABILITY]                                                                    |
| :-----: | :------------------------- | :-------- | :------------------------------------------------------------------------------ |
|  [01]   | `HostUtils`                | static    | runtime environment, assembly loading, named callbacks, compute endpoints       |
|  [02]   | `Interop`                  | static    | managed-handle to native-pointer marshal seam across every host type            |
|  [03]   | `AssemblyResolver`         | static    | search-folder/search-file assembly resolution hooks                             |
|  [04]   | `IPlatformServiceLocator`  | interface | platform-service provider `HostUtils.GetPlatformService<T>` resolves            |
|  [05]   | `IShrinkWrapService`       | interface | shrink-wrap mesh service contract resolved through the locator                  |
|  [06]   | `IZooClientUtilities`      | interface | Zoo network-license client-utility service contract                             |
|  [07]   | `CommonObject`             | class     | abstract const/non-const native-lifetime base for geometry and doc handles      |
|  [08]   | `RiskyAction`              | class     | disposable caller-scoped guard bracketing a native call that may fault          |
|  [09]   | `NamedParametersEventArgs` | class     | disposable typed named-parameter dictionary for host named callbacks            |
|  [10]   | `Mode`                     | enum      | runtime execution mode (`NormalMode`/`ViewerMode`/`BetaMode`)                   |
|  [11]   | `AdvancedSetting`          | enum      | advanced-settings key vocabulary for `HostUtils` advanced reads                 |
|  [12]   | `ImportOptionsSections`    | enum      | options-dialog section discriminant for import-options routing                  |
|  [13]   | `HostUtils.LogMessageType` | enum      | cloud log-message severity (`unknown`/`information`/`warning`/`error`/`assert`) |

[PUBLIC_TYPE_SCOPE]: scripting, skin, capture, and licensing
- rail: host

| [INDEX] | [SYMBOL]                       | [KIND] | [CAPABILITY]                                                             |
| :-----: | :----------------------------- | :----- | :----------------------------------------------------------------------- |
|  [01]   | `PythonScript`                 | class  | abstract Python host — compile, variable scope, execute, evaluate        |
|  [02]   | `PythonCompiledCode`           | class  | abstract compiled-script handle executed against a `PythonScript` scope  |
|  [03]   | `Skin`                         | class  | abstract application-skin singleton — splash, icon, load-phase hooks     |
|  [04]   | `ViewCaptureWriter`            | class  | abstract vector-capture sink turning a print frame into drawn primitives |
|  [05]   | `CloudHostUtils`               | static | compute-cloud entitlement and signature gate                             |
|  [06]   | `ZooClientParameters`          | class  | Zoo product/license descriptor carrying validation delegates             |
|  [07]   | `LicenseStateChangedEventArgs` | class  | payload flagging whether calling RhinoCommon is currently allowed        |
|  [08]   | `LicenseTypes`                 | enum   | license origin (`Standalone`/`ZooAutoDetect`/`CloudZoo`)                 |

[PUBLIC_TYPE_SCOPE]: typed host failures
- rail: host

- `CorruptGeometryException` / `DocumentCollectedException` — native corruption and collected-document faults surfaced as managed exceptions
- `NotLicensedException` / `RdkNotLoadedException` — licensing and RDK-availability faults raised at the host boundary

[PUBLIC_TYPE_SCOPE]: in-process host boot (`Rhino.Runtime.InProcess`)
- rail: host

| [INDEX] | [SYMBOL]              | [KIND] | [CAPABILITY]                                                             |
| :-----: | :-------------------- | :----- | :----------------------------------------------------------------------- |
|  [01]   | `RhinoCore`           | class  | disposable in-process headless Rhino boot with host-context marshalling  |
|  [02]   | `Interop`             | static | `StartupInProcess`/`LaunchInProcess` native startup extern surface       |
|  [03]   | `Interop.HRESULT`     | static | native `S_OK`/`E_*` result constants                                     |
|  [04]   | `Interop.StartupInfo` | struct | Win32 `STARTUPINFO` mirror passed into `StartupInProcess`                |
|  [05]   | `WindowStyle`         | enum   | boot window style (`NoWindow`/`Normal`/`Hidden`/`Minimized`/`Maximized`) |
|  [06]   | `StartupOrigin`       | enum   | boot origin (`NotStarted`/`Application`/`Library`)                       |

[PUBLIC_TYPE_SCOPE]: native marshal family (`Rhino.Runtime.InteropWrappers`)
- rail: host

`SimpleArray*` and `StdVector*` are the disposable managed↔native array bridges every host P/Invoke threads geometry, primitive, and pointer collections through; each holds a `ConstPointer()`/`NonConstPointer()` pair, admits `Add`, and materializes a managed `ToArray()`. `StringHolder`/`StringWrapper` bridge native `ON_wString` text, `ClassArray*` bridge managed reference collections, and the value structs carry native measurement payloads.

| [INDEX] | [SYMBOL]                | [KIND] | [CAPABILITY]                                                       |
| :-----: | :---------------------- | :----- | :----------------------------------------------------------------- |
|  [01]   | `SimpleArray<T>` family | class  | disposable primitive/geometry/pointer native-array marshal bridges |
|  [02]   | `StdVector<T>` family   | class  | disposable `std::vector` marshal bridges                           |
|  [03]   | `ClassArrayString`      | class  | disposable native string-array bridge                              |
|  [04]   | `ClassArrayObjRef`      | class  | disposable native `ObjRef`-array bridge                            |
|  [05]   | `ClassArrayOnObjRef`    | class  | disposable native `ON_ObjRef`-array bridge                         |
|  [06]   | `StringHolder`          | class  | disposable native `ON_wString` reader                              |
|  [07]   | `StringWrapper`         | class  | disposable read/write native `ON_wString` bridge                   |
|  [08]   | `CurveSegment`          | struct | curve-region segment (`Index`/`SubDomain`/`Reversed`)              |
|  [09]   | `MeshPointDataStruct`   | struct | native mesh-point evaluation payload                               |
|  [10]   | `RhDisplayPoint`        | struct | native display-point marshal payload                               |

[PUBLIC_TYPE_SCOPE]: notifications and node-in-code
- rail: host

| [INDEX] | [SYMBOL]                                      | [KIND]    | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------------- | :-------- | :----------------------------------------------------------------- |
|  [01]   | `Notifications.NotificationCenter`            | static    | application notification set observed by the host UI               |
|  [02]   | `Notifications.Notification`                  | class     | assembly-restricted notification with modal show/hide and metadata |
|  [03]   | `Notifications.NotificationButtonClickedArgs` | class     | payload naming the notification and clicked `ButtonType`           |
|  [04]   | `Notifications.ButtonType`                    | enum      | notification button (`CancelOrClose`/`Confirm`/`Alternate`)        |
|  [05]   | `Notifications.IAssemblyRestrictedObject`     | interface | assembly-restriction contract guarding notification mutation       |
|  [06]   | `NodeInCode.Components`                       | class     | abstract node-in-code component root exposing the function table   |
|  [07]   | `NodeInCode.ComponentFunctionInfo`            | class     | callable Grasshopper-component descriptor with typed IO metadata   |
|  [08]   | `NodeInCode.NodeInCodeTable`                  | class     | dynamic dispatch table of component functions keyed by full name   |

[PUBLIC_TYPE_SCOPE]: account tokens (`Rhino.Runtime.RhinoAccounts`)
- rail: host

| [INDEX] | [SYMBOL]                     | [KIND]    | [CAPABILITY]                                                 |
| :-----: | :--------------------------- | :-------- | :----------------------------------------------------------- |
|  [01]   | `RhinoAccountsManager`       | static    | secret-key-scoped OAuth2/OpenID token acquisition and revoke |
|  [02]   | `IRhinoAccountsManager`      | interface | accounts-manager service contract                            |
|  [03]   | `IOAuth2Token`               | interface | OAuth2 access-token handle                                   |
|  [04]   | `IOpenIDConnectToken`        | interface | OpenID Connect identity-token handle                         |
|  [05]   | `SecretKey`                  | class     | scoped secret-key capability handed to protected code        |
|  [06]   | `RhinoAccountsGroup`         | class     | account group identity (`Id`/`Name`)                         |
|  [07]   | `RhinoAccoountsProgressInfo` | class     | login-progress payload (host-misspelled type name)           |
|  [08]   | `ProgressState`              | enum      | login progress (`AwaitingLogin`/`RetrievingTokens`/`Other`)  |
|  [09]   | `RhinoAccountsException`     | class     | base of the accounts fault hierarchy                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: platform services, assembly loading, and process facts
- rail: host

- `HostUtils.GetPlatformService<T>(string assemblyPath = null, string typeFullName = null) : T where T : class` — resolve a platform service, the single seam `IShrinkWrapService`/`IZooClientUtilities` and OS-info reads route through
- `HostUtils.LoadAssemblyFrom(string path) : Assembly` / `LoadAssemblyFromStream(Stream stream) : Assembly` / `LoadAssemblyFromName(AssemblyName assemblyName) : Assembly` — collectible-context assembly loading
- `HostUtils.RunningInMono : bool` / `RunningOnServer : bool` / `RunningInDarkMode : bool` / `IsPreRelease : bool` / `CurrentOSLanguage : uint` — process discriminants
- `HostUtils.OperatingSystemEdition` / `OperatingSystemProductName` / `OperatingSystemBuildNumber` / `OperatingSystemInstallationType : string` — OS identity via the platform service
- `HostUtils.GetSystemProcessorCount() : int` / `GetCurrentProcessInfo(out string processName, out Version processVersion) : void` / `GetAssemblySearchPaths() : string[]` / `GetSystemReferenceAssemblies() : IEnumerable<string>`
- `HostUtils.GetPrinterNames() : string[]` / `GetPrinterFormNames(string printerName) : string[]` / `GetPrinterFormSize(string, string, out double widthMillimeters, out double heightMillimeters) : bool` / `GetPrinterFormMargins(...) : bool` / `GetPrinterDPI(string printerName, bool horizontal) : double`
- `AssemblyResolver.AddSearchFolder(string folder) : void` / `AddSearchFile(string file) : void`; `AssemblyResolver.CurrentDomainAssemblyResolve` / `CurrentDomainReflectionOnlyAssemblyResolve : ResolveEventHandler` — the resolve hooks the host installs

[ENTRYPOINT_SCOPE]: native-pointer marshal seam
- rail: host

- `Interop.NativeGeometryConstPointer(GeometryBase geometry) : nint` / `NativeGeometryNonConstPointer(GeometryBase geometry) : nint` / `CreateFromNativePointer(nint pGeometry) : GeometryBase` — geometry pointer round-trip
- `Interop.NativeRhinoDocPointer(RhinoDoc doc) : nint` / `RhinoObjectConstPointer(RhinoObject rhinoObject) : nint` / `RhinoObjectFromPointer(nint pRhinoObject) : RhinoObject`
- `Interop.NativeNonConstPointer(ViewCaptureSettings settings)` / `NativeNonConstPointer(ViewportInfo viewport)` / `NativeNonConstPointer(RhinoViewport viewport)` / `NativeNonConstPointer(DisplayPipeline pipeline)` / `NativeNonConstPointer(GetPoint getPoint) : nint` — the polymorphic host-handle pointer accessor
- `Interop.FileWriteOptionsConstPointer(FileWriteOptions options)` / `FileReadOptionsConstPointer(FileReadOptions options)` / `PlugInPointer(PlugIn plugin) : nint`
- `Interop.FontFromPointer(nint ptrManagedFont) : Rhino.DocObjects.Font` / `ViewCaptureFromPointer(nint ptrViewCapture) : ViewCaptureSettings` / `NSFontFromFont(Rhino.DocObjects.Font font[, double pointSize]) : nint`
- `Interop.FromOnBrep(object source) : Brep` / `FromOnMesh(object source) : Mesh` / `FromOnSurface(object) : Surface` / `FromOnCurve(object) : Curve` and the `ToOnBrep`/`ToOnMesh`/`ToOnSurface`/`ToOnCurve`/`ToOnXform` reverse family bridging opennurbs `ON_*` objects
- `SimpleArrayPoint3d()` / `SimpleArrayPoint3d(IEnumerable<Point3d> pts)` / `Add(Point3d pt) : void` / `ToArray() : Point3d[]` / `ConstPointer() : nint` / `NonConstPointer() : nint` / `Dispose() : void` — the representative `SimpleArray*` marshal shape every family member repeats
- `StringHolder.GetString(nint pStringHolder) : string` / `ToStringSafe() : string`; `StringWrapper(string s)` / `SetString(string s) : void` / `GetStringFromPointer(nint pConstON_wString) : string`

[ENTRYPOINT_SCOPE]: named callbacks and compute endpoints
- rail: host

- `HostUtils.RegisterNamedCallback(string name, EventHandler<NamedParametersEventArgs> callback) : void` / `RemoveNamedCallback(string name) : void` / `ExecuteNamedCallback(string name, NamedParametersEventArgs args) : bool`
- `HostUtils.RegisterComputeEndpoint(string endpointPath, Type t) : void` / `GetCustomComputeEndpoints() : Tuple<string, Type>[]`
- `HostUtils.OnExceptionReport : ExceptionReportDelegate` / `ExceptionReport(string source, Exception ex) : void` / `OnSendLogMessageToCloud : SendLogMessageToCloudDelegate` — process-wide exception and cloud-log taps
- `HostUtils.CheckForRdk(bool throwOnFalse, bool usePreviousResult) : bool` / `RegisterStaticIDisposable(IDisposable disposable) : void`
- `NamedParametersEventArgs()` / `Dispose() : void`; no generic accessor exists — reads are the NAMED per-type roster `TryGetString`/`TryGetStrings`/`TryGetBool`/`TryGetInt`/`TryGetUnsignedInt`/`TryGetUints`/`TryGetDouble`/`TryGetGuid`/`TryGetGuids`/`TryGetColor`/`TryGetPoint2i`/`TryGetPoint`/`TryGetPoints`/`TryGetVector`/`TryGetLine`/`TryGetArc`/`TryGetPlane`/`TryGetViewport`/`TryGetGeometry`/`TryGetObjRefs`/`TryGetRhinoObjects`/`TryGetMeshParameters`/`TryGetUnmangedPointer` (each `(string name, out T value) : bool`; the host misspells `Unmanged`), and writes are overloaded `Set(string name, T value)` spanning `string`/`IEnumerable<string>`/`bool`/`int`/`uint`/`IEnumerable<uint>`/`double`/`Guid`/`IEnumerable<Guid>`/`Color`/`System.Drawing.Point`/`Point3d`/`Point3d[]`/`Vector3d`/`Line`/`Arc`/`Plane`/`GeometryBase`/`IEnumerable<GeometryBase>`/`IEnumerable<ObjRef>`/`MeshingParameters`, with `SetWindowHandle`/`SetWindowImageHandle`/`TryGetWindowHandle`/`TryGetWindowImageHandle` for native handles; `ViewportInfo` is read-only (`TryGetViewport` has no `Set` counterpart)

[ENTRYPOINT_SCOPE]: scripting host
- rail: host

- `PythonScript.Create() : PythonScript` — mint the host scripting engine, null when the host ships without the Python plug-in; `RuntimeAssemblies() : Assembly[]` / `AddRuntimeAssembly(Assembly assembly) : void` / `SearchPaths : string[]` static get-set
- `PythonScript.Compile(string script) : PythonCompiledCode` / `ExecuteScript(string script) : bool` / `ExecuteFile(string path) : bool` / `ExecuteFileInScope(string path) : bool` / `EvaluateExpression(string statements, string expression) : object`
- `PythonScript.SetVariable(string name, object value) : void` / `GetVariable(string name) : object` / `ContainsVariable(string name) : bool` / `GetVariableNames() : IEnumerable<string>` / `RemoveVariable(string name) : void`
- `PythonScript.Output : Action<string>` / `ScriptContextDoc : object` / `ScriptContextCommand : Command` / `ContextId : int`; `GetStackTraceFromException(Exception ex) : string`; `PythonCompiledCode.Execute(PythonScript scope) : void`

[ENTRYPOINT_SCOPE]: skin, vector capture, and risky-call guard
- rail: host

- `Skin.ActiveSkin : Skin` — process skin singleton; `protected virtual MainRhinoIcon : Bitmap` / `ApplicationName : string`; the load-phase overrides `OnMainFrameWindowCreated`/`OnLicenseCheckCompleted`/`OnBuiltInCommandsRegistered`/`OnBeginLoadAtStartPlugIns(int expectedCount)`/`OnBeginLoadPlugIn(string description)`/`OnEndLoadPlugIn`/`OnEndLoadAtStartPlugIns`, with `ShowSplash`/`HideSplash`/`ShowHelp`
- `ViewCaptureWriter(double dpi, Size pageSize)` / `Draw(nint constPtrPrintInfo, RhinoDoc doc) : void` — subclass and override the `protected abstract` primitive sinks `DrawPath`/`DrawCircle`/`DrawRectangle`/`DrawBitmap`/`DrawScreenText`/`FillPolygon`/`SetClipPath`/`DrawGradientHatch`, with `SupportsArc()` gating arc primitives and nested `Pen`/`PathPoint`/`ViewCaptureBrush`/`PointType` carriers
- `new RiskyAction(string description, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)` / `Dispose() : void` — brackets a native call that may fault so the host records provenance

[ENTRYPOINT_SCOPE]: in-process headless boot
- rail: host

- `new RhinoCore()` / `RhinoCore(string[] args)` / `RhinoCore(string[] args, WindowStyle windowStyle)` / `RhinoCore(string[] args, WindowStyle windowStyle, nint hostWnd)` — boot an in-process Rhino
- `RhinoCore.Run() : int` / `RaiseIdle() : void` / `DoIdle() : bool` / `DoEvents() : bool` / `Dispose() : void`
- `RhinoCore.InvokeInHostContext(Action action) : void` / `InvokeInHostContext<T>(Func<T> func) : T` — marshal a call onto the host thread
- `InProcess.Interop.StartupInProcess(int argc, string[] argv, ref StartupInfo info, nint hostWnd) : void` / `LaunchInProcess(int mode, int reserved) : int` / `RunMessageLoop() : int` / `ShutdownInProcess() : void` / `EnterHostContext()` / `ExitHostContext()` / `StartedAsRhinoExe() : bool`

[ENTRYPOINT_SCOPE]: notifications and node-in-code
- rail: host

- `NotificationCenter.Notifications` — observable application notification set the host UI renders
- `new Notification()` / `Notification(IEnumerable<Assembly> allowedAssemblies)` / `ShowModal() : void` / `HideModal() : void` / `RemoveMetadata(string key) : bool` / `Editable() : bool`; `Notification.AllowedAssemblies : ICollection<Assembly>` / `MetadataCopy : IDictionary<string, string>` / `ShowEventId : Guid?` / `DateUpdated : DateTime`
- `Notification.ExecuteAssemblyProtectedCode(Action action) : void` / `ExecuteAssemblyProtectedCode<TResult>(Func<TResult> func) : TResult` — run inside the assembly-restriction guard
- `Components.NodeInCodeFunctions : NodeInCodeTable` / `Components.FindComponent(string fullName) : ComponentFunctionInfo`
- `ComponentFunctionInfo.Invoke(params object[] args) : object[]` / `InvokeKeepTree(params object[] args) : object[]` / `InvokeSilenceWarnings(...)` / `Evaluate(IEnumerable args, bool keepTree, out string[] warnings) : object[]`; `Name` / `Namespace` / `Description : string` / `InputNames` / `OutputNames : IReadOnlyList<string>` / `InputsOptional : IReadOnlyList<bool>` / `ComponentGuid : Guid` / `Delegate` / `DelegateTree : Delegate`
- `NodeInCodeTable.Contains(string fullName) : bool` / `Add(ComponentFunctionInfo item) : void` / `GetDynamicMembers() : IEnumerable<ComponentFunctionInfo>` / `Count : int` — the `DynamicObject` dispatch reaches functions by member name

[ENTRYPOINT_SCOPE]: account tokens and licensing
- rail: host

- `RhinoAccountsManager.ExecuteProtectedCode(Action<SecretKey> protectedCode) : void` / `ExecuteProtectedCodeAsync(Func<SecretKey, Task> protectedCode) : Task` — the `SecretKey` only exists inside the protected callback
- `RhinoAccountsManager.GetAuthTokensAsync(string clientId, string clientSecret, SecretKey secretKey, CancellationToken) : Task<Tuple<IOpenIDConnectToken, IOAuth2Token>>` / the scoped/prompt overload / `TryGetAuthTokens(string clientId, SecretKey secretKey) : Tuple<IOpenIDConnectToken, IOAuth2Token>`
- `RhinoAccountsManager.RevokeAuthTokenAsync(IOAuth2Token oauth2Token, SecretKey secretKey, CancellationToken) : Task` / `UpdateOpenIDConnectTokenAsync(IOpenIDConnectToken currentToken, IOAuth2Token oauth2Token, SecretKey secretKey, CancellationToken) : Task<IOpenIDConnectToken>`
- `CloudHostUtils.IsEntitled : bool` / `DenyReason : string` / `Signature : string` / `CheckEntitlement() : void` — the compute-cloud entitlement gate

[ENTRYPOINT_SCOPE]: `CommonObject` lifetime base
- rail: host

- `CommonObject.IsValidWithLog(out string log) : bool` — per-object validity read every geometry and document handle inherits; `IsDocumentControlled : bool` distinguishes a native handle the document owns from a private copy
- `CommonObject.EnsurePrivateCopy() : void` — detach a document-controlled handle into a writable private copy before mutation
- `CommonObject.ToJSON(SerializationOptions options) : string` / `FromJSON(string json) : CommonObject` / `FromBase64String(int archive3dm, int opennurbs, string base64Data) : CommonObject` / `ComputeMemoryEstimate(CommonObject obj) : uint`
- `CommonObject.UserData : UserDataList` / `UserDictionary` and the per-object user-string custody are owned by `api-rhinocommon-persistence.md`; this catalog names the base, that catalog owns the attachment surface

## [04]-[IMPLEMENTATION_LAW]

[RUNTIME_TOPOLOGY]:
- one host boundary crosses through `Interop`: a managed handle projects to an `nint` for a native call and a returned `nint` reconstructs a managed object, and the `InteropWrappers` `SimpleArray*`/`StdVector*`/`StringHolder` disposables own every native collection or string across that call, released on a matched `Dispose`
- `CommonObject` is the const/non-const lifetime root: a document-controlled handle is read-only until `EnsurePrivateCopy` detaches it, and `IsValidWithLog` is the one validity read the whole geometry and document hierarchy inherits
- platform capability resolves once through `HostUtils.GetPlatformService<T>`; a service such as `IShrinkWrapService` or `IZooClientUtilities` is never newed, only located, so the host owns the single implementation
- `RhinoCore` is the in-process headless boot seam, and every call into a booted core marshals through `InvokeInHostContext` onto the host thread rather than touching host state from an arbitrary thread
- named callbacks and compute endpoints register by string key into `HostUtils`, and `NamedParametersEventArgs` is the one typed payload crossing that key, never a bespoke argument object

[STACKING]:
- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): every `HostUtils`/`Interop` call returning a nullable handle or an out-`bool` folds to `Fin<A>`/`Option<A>` at the boundary, `NamedParametersEventArgs.TryGet<T>` out-parameters lift to `Option<A>`, and a host call raising `CorruptGeometryException`/`NotLicensedException`/`RdkNotLoadedException` wraps through `Try.lift(...).Run()` so exception control flow never enters domain code
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `Mode`, `LicenseTypes`, `Notifications.ButtonType`, `RhinoAccounts.ProgressState`, `InProcess.WindowStyle`, and `AdvancedSetting` map at the edge to keyed `SmartEnum` owners, and a plugin or product `Guid` handed to `RhinoAccountsManager` wraps as a `ValueObject<Guid>` so the bare host `Guid` never leaks
- `Hashing`(`libs/csharp/.api/api-hashing.md`): a `CommonObject.ToJSON`/`FromBase64String` serialization projects into the `XxHash128` content key the persistence artifact index dedupes host objects on
- `api-macos-native.md`: in-process `RhinoCore.InvokeInHostContext` marshal composes the Rasm host main-thread rail rather than a bespoke dispatcher, and `ViewCaptureWriter` primitive sinks feed the kernel vector-drawing owner, never a host-side re-derivation

[LOCAL_ADMISSION]:
- native-pointer traffic enters through `Interop` and the `InteropWrappers` disposable family; a raw `nint` never appears in domain code, and every marshal wrapper is disposed on its owning scope
- platform capability enters through `HostUtils.GetPlatformService<T>`, never a direct service construction
- a headless runtime enters through one `RhinoCore` owner with host-context marshalling, never a per-call thread hop
- `CommonObject.UserData`/`UserDictionary` custody routes to `api-rhinocommon-persistence.md`; text-field evaluation routes to `api-rhinocommon-annotation.md` and block-attribute fields to `api-rhinocommon-blocks.md`; this catalog never re-documents those slices

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: host environment and platform services, the native marshal seam and disposable array family, the `CommonObject` lifetime base, the scripting/skin/vector-capture surface, the in-process headless boot, the assembly-scoped notification center, node-in-code, and the account-token rail
- Accept: platform-service resolution, marshalled native traffic through disposable wrappers, headless boot with host-context marshalling, secret-key-scoped token acquisition, named-callback dispatch
- Reject: a raw native pointer in a domain signature, an undisposed marshal wrapper, a service construction bypassing the locator, a cross-thread host call outside `InvokeInHostContext`, and any re-documentation of the user-data, text-field, or block-attribute slices other catalogs own

[NAMESPACE_CENSUS]:
- covered public surface: `Rhino.Runtime` (host services, marshal seam, scripting, skin, capture, licensing, `CommonObject` base), `Rhino.Runtime.InProcess`, `Rhino.Runtime.InteropWrappers`, `Rhino.Runtime.Notifications`, `Rhino.Runtime.RhinoAccounts`, `Rhino.NodeInCode`
- access-excluded (internal, never lands): `AdvancedSettings`, `ConstOperationAttribute`, `LicenseManager`, `SharedPtrCommonObject`, `ShrinkWrap`, `CloudHostUtils` internals (`ICloudHost`/`DoNothingCloudHost`), `ISettingsService`/`IOperatingSystemInformation`/`FileSettingsService`/`MetadataReaderHelper`/`MonoHost`, and the `InteropWrappers` internal glue (`ClassArrayCurveRegion`, `IntPtrSafeHandle`, `RhinoDib`, `SimpleArrayFonts`, `ArrayOfTArrayMarshal`, `INTERNAL_ComponentIndexArray`, `NullItemsResponse`)
- truth-rail-unresolvable, excluded as phantom: `Rhino.Notifications` (`SuspendCollectionChangedNotifications`/`SuspendCollectionChangedEventHandler`) and the `Rhino.Runtime.Notifications` collection backings (`TrulyObservableOrderedSet`, `NonNullableDictionary`) — generic collection-suspension infrastructure the `--symbol` decompile rail does not resolve
- cross-referenced, never duplicated: `Rhino.Runtime.TextFields` (owned by `api-rhinocommon-annotation.md` + `api-rhinocommon-blocks.md`), `CommonObject.UserData`/`UserDictionary` (owned by `api-rhinocommon-persistence.md`)
