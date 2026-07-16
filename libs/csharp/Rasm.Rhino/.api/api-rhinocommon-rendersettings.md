# [RASM_RHINO_API_RHINOCOMMON_RENDERSETTINGS]

`Rhino.Render` owns the document render-settings family: `RenderSettings` (a `CommonObject`) aggregates the background, antialiasing, output-image, and environment configuration and exposes the `GroundPlane`, `Skylight`, `Sun`, `LinearWorkflow`, `Dithering`, `SafeFrame`, and `RenderChannels` sub-owners. Every sub-owner derives `DocumentOrFreeFloatingBase`, whose discriminant decides one identity: a document serial number binds it to a live `RhinoDoc`, a `File3dm` attaches it to an archive, and neither leaves it free-floating over a native pointer — the one duality this catalog states as law. `api-rhinocommon-render.md` owns the disjoint renderer-lifecycle slice that consumes these settings at render time; `api-rhinocommon-rendercontent.md` owns the `RenderEnvironment` content the environment-usage binding points at; `api-rhinocommon-document.md` owns the `RhinoDoc.RenderSettings` accessor, the `MaterialTable`/`LightTable` document tables, and the georeference `Sun.Latitude`/`Longitude`/`North` slice; and `api-rhinocommon-fileio.md` owns the `File3dmSettings.RenderSettings` archive accessor.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon render-settings surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Render`
- kernel: `Rasm` (host-agnostic color, vector, and unit owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: render-settings boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: settings aggregate and duality base
- rail: render-settings boundary

| [INDEX] | [SYMBOL]                     | [KIND]         | [CAPABILITY]                                                           |
| :-----: | :--------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `RenderSettings`             | `CommonObject` | background, antialias, output-image, environment, and sub-owner roster |
|  [02]   | `DocumentOrFreeFloatingBase` | abstract base  | document-bound, archive-attached, or free-floating discriminant        |

[PUBLIC_TYPE_SCOPE]: free-floating sub-owners
- rail: render-settings boundary

Each sub-owner is `sealed : DocumentOrFreeFloatingBase, IDisposable` and clones through `CopyFrom(FreeFloatingBase src)`; `GroundPlane`/`Skylight`/`Sun`/`SafeFrame`/`RenderChannels` each add a static `Changed` broadcast, while `LinearWorkflow` and `Dithering` carry none.

| [INDEX] | [SYMBOL]         | [KIND]          | [CAPABILITY]                                                   |
| :-----: | :--------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `GroundPlane`    | ambient element | altitude, shadow-only, material, and texture-mapping placement |
|  [02]   | `Skylight`       | ambient element | skylight enable and shadow intensity                           |
|  [03]   | `Sun`            | ambient element | position, date/time/timezone, intensity, and derived direction |
|  [04]   | `LinearWorkflow` | output pipeline | pre/post gamma and framebuffer color processing                |
|  [05]   | `Dithering`      | output pipeline | dithering method toggle                                        |
|  [06]   | `SafeFrame`      | output pipeline | live, action, and title frame guides                           |
|  [07]   | `RenderChannels` | output pipeline | automatic or custom render-channel set                         |

[ENUM_ROSTERS]:
- `public enum Rhino.Render.RenderSettings.RenderingSources : uint` — `ActiveViewport`, `SpecificViewport`, `NamedView`, `SnapShot`.
- `public enum Rhino.Render.RenderSettings.EnvironmentUsage` — `Background`, `Reflection`, `Skylighting`.
- `public enum Rhino.Render.RenderSettings.EnvironmentPurpose` — `Standard`, `ForRendering`.
- `public enum Rhino.Render.Sun.Accuracies` — `Minimum`, `Maximum`.
- `public enum Rhino.Render.Dithering.Methods` — `None`, `FloydSteinberg`, `SimpleNoise`.
- `public enum Rhino.Render.RenderChannels.Modes` — `Automatic`, `Custom`.

## [03]-[ENTRYPOINTS]

[SETTINGS_AGGREGATE]:
- `Rhino.Render.RenderSettings.AmbientLight : Color` / `BackgroundColorTop : Color` / `BackgroundColorBottom : Color` / `BackgroundStyle : BackgroundStyle` / `TransparentBackground : bool` — background color and style.
- `Rhino.Render.RenderSettings.AntialiasLevel : AntialiasLevel` / `ShadowmapLevel : int` / `RenderBackfaces : bool` / `RenderCurves : bool` / `RenderPoints : bool` / `RenderMeshEdges : bool` / `RenderAnnotations : bool` / `RenderIsoparams : bool` / `UseHiddenLights : bool` / `DepthCue : bool` / `FlatShade : bool` — quality and element-inclusion flags.
- `Rhino.Render.RenderSettings.UseViewportSize : bool` / `ImageSize : Size` / `ImageDpi : double` / `ImageUnitSystem : UnitSystem` / `ScaleBackgroundToFit : bool` — output-image dimensions and scaling.
- `Rhino.Render.RenderSettings.RenderSource : RenderingSources` / `NamedView : string` / `SpecificViewport : string` / `Snapshot : string` — the view the render samples.
- `Rhino.Render.RenderSettings.GroundPlane : GroundPlane` / `Skylight : Skylight` / `Sun : Sun` / `LinearWorkflow : LinearWorkflow` / `Dithering : Dithering` / `SafeFrame : SafeFrame` / `RenderChannels : RenderChannels` / `PostEffects : PostEffectCollection` — the sub-owner roster.
- `Rhino.Render.RenderSettings.RenderEnvironment(EnvironmentUsage usage, EnvironmentPurpose purpose) : RenderEnvironment` / `SetRenderEnvironment(EnvironmentUsage, RenderEnvironment) : void` / `RenderEnvironmentId(EnvironmentUsage, EnvironmentPurpose) : Guid` / `SetRenderEnvironmentId(EnvironmentUsage, Guid) : void` — per-usage environment binding.
- `Rhino.Render.RenderSettings.RenderEnvironmentOverride(EnvironmentUsage) : bool` / `SetRenderEnvironmentOverride(EnvironmentUsage, bool) : void` — per-usage override toggle.
- `Rhino.Render.ICurrentEnvironment.ForBackground` / `ForReflectionAndRefraction` / `ForLighting : RenderEnvironment` (get/set) — the resolved current-environment triple behind `RhinoDoc.CurrentEnvironment`; the accessor is `api-rhinocommon-document.md`'s.
- `Rhino.Render.RenderSettings.Duplicate() : RenderSettings` — a detached copy; `RenderSettings : CommonObject`, so a live instance rides the document grant or a `using` window.
- `new RenderSettings()` / `new RenderSettings(RenderSettings source)` — public free-floating and copy constructors; each duality sub-owner likewise carries a public `()`/copy constructor pair, and `SafeFrame(RhinoDoc)` adds a document-bound constructor.

[DUALITY_BASE]:
- `Rhino.Render.DocumentOrFreeFloatingBase` — the base whose internal discriminant resolves the native pointer from a document serial number (document-bound), a `File3dm` (archive-attached), or neither (free-floating); the derived `GroundPlane`/`Skylight`/`Sun`/`LinearWorkflow`/`Dithering`/`SafeFrame`/`RenderChannels` share this identity resolution, and `File3dm.Settings.RenderSettings` (get-only) lazily binds the archive-attached state.
- `Rhino.Render.DocumentOrFreeFloatingBase.BeginChange(RenderContent.ChangeContexts) : void` / `EndChange() : bool` — inert no-ops (empty body; `EndChange` returns `false`), carrying no `[Obsolete]` attribute; a bound sub-owner writes in place through its resolved native pointer, and a free-floating value transfers through `CopyFrom`.
- `Rhino.Render.<sub-owner>.CopyFrom(FreeFloatingBase src) : void` — the clone every sub-owner overrides; `Changed : EventHandler<RenderPropertyChangedEvent>` is the static property-changed broadcast `GroundPlane`/`Skylight`/`Sun`/`SafeFrame`/`RenderChannels` carry, absent on `LinearWorkflow`/`Dithering`.

[AMBIENT]:
- `Rhino.Render.GroundPlane.Enabled : bool` / `ShadowOnly : bool` / `ShowUnderside : bool` / `Altitude : double` / `AutoAltitude : bool` — ground-plane placement.
- `Rhino.Render.GroundPlane.MaterialInstanceId : Guid` / `TextureOffset : Vector2d` / `TextureSize : Vector2d` / `TextureRotation : double` / `TextureOffsetLocked : bool` / `TextureSizeLocked : bool` — ground-plane material and texture mapping.
- `Rhino.Render.Skylight.Enabled : bool` / `ShadowIntensity : double` — skylight toggle and shadow strength; `CustomEnvironmentOn`/`CustomEnvironment` are obsolete.
- `Rhino.Render.Sun.Enabled : bool` / `ManualControlOn : bool` / `Intensity : double` / `Accuracy : Sun.Accuracies` / `North : double` / `Vector : Vector3d` / `Light : Light` / `Hash : uint` — sun enablement, manual override, derived scene contribution, and data CRC.
- `Rhino.Render.Sun.Azimuth : double` / `Altitude : double` / `Latitude : double` / `Longitude : double` / `TimeZone : double` / `DaylightSavingOn : bool` / `DaylightSavingMinutes : int` — sun position and time context; `api-rhinocommon-document.md` owns the georeference `Latitude`/`Longitude`/`North` slice.
- `Rhino.Render.Sun.GetDateTime(DateTimeKind) : DateTime` / `SetDateTime(DateTime, DateTimeKind) : void` — sun date/time; the `SetPosition` overloads and `ManualControl`/`SkylightOn`/`DaylightSaving` are obsolete.
- `Rhino.Render.Sun.SunDirection(double latitude, double longitude, DateTime when) : Vector3d` / `AltitudeFromValues(double, double, double, int, DateTime, double, bool) : double` / `ColorFromAltitude(double) : Color` / `JulianDay(double timezoneHours, int daylightMinutes, DateTime when, double hours) : double` / `TwilightZone() : double` / `Here(out double, out double) : bool` — static sun-position and astronomical solvers.

[OUTPUT_PIPELINE]:
- `Rhino.Render.LinearWorkflow.PreProcessColors : bool` / `PreProcessTextures : bool` / `PostProcessFrameBuffer : bool` / `PreProcessGamma : float` / `PostProcessGamma : float` / `PostProcessGammaReciprocal : float` / `PostProcessGammaOn : bool` / `Hash : uint` — the gamma/linear color pipeline; `LinearWorkflow` overrides `Equals`/`GetHashCode` off `Hash`.
- `Rhino.Render.Dithering.Method : Dithering.Methods` / `Enabled : bool` — dithering method and toggle; `On` is obsolete.
- `Rhino.Render.SafeFrame.Enabled : bool` / `PerspectiveOnly : bool` / `FieldsOn : bool` / `LiveFrameOn : bool` — frame-guide enablement.
- `Rhino.Render.SafeFrame.ActionFrameOn : bool` / `ActionFrameLinked : bool` / `ActionFrameXScale : double` / `ActionFrameYScale : double` / `TitleFrameOn : bool` / `TitleFrameLinked : bool` / `TitleFrameXScale : double` / `TitleFrameYScale : double` — action and title frame geometry.
- `Rhino.Render.RenderChannels.Mode : RenderChannels.Modes` / `CustomList : Guid[]` — automatic or custom render-channel set.

## [04]-[IMPLEMENTATION_LAW]

[SETTINGS_TOPOLOGY]:
- `RenderSettings` is the single aggregate: it holds the background, quality, output-image, and view-source configuration and exposes each ambient/output sub-owner as one property — a settings value never scatters across parallel owners, and `Duplicate` detaches a full copy.
- `DocumentOrFreeFloatingBase` is the one duality: the same `GroundPlane`/`Sun`/`LinearWorkflow` type is document-bound, archive-attached, or free-floating by its internal discriminant, so a domain owner discriminates on that origin rather than holding three parallel types; `BeginChange`/`EndChange` are inert, and a free-floating mutation commits through the document or archive accessor.
- Environment binding is per-usage: `RenderEnvironment(EnvironmentUsage, EnvironmentPurpose)` and `SetRenderEnvironmentId` bind a `RenderEnvironment` per `Background`/`Reflection`/`Skylighting` usage, and the override toggle gates each — the content itself is `api-rhinocommon-rendercontent.md`.
- Settings configure; rendering consumes: `api-rhinocommon-render.md` reads a `RenderSettings` at render time, and this catalog never runs a render.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): a `RenderSettings` or free-floating sub-owner `IDisposable` rides a `using` bounded by `Eff`; a settings read projects a detached value record, and a commit-through-accessor outcome crosses as `Fin<Unit>`.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): `RenderingSources`, `EnvironmentUsage`, `EnvironmentPurpose`, `Dithering.Methods`, `RenderChannels.Modes`, and `Sun.Accuracies` map at the edge to `[SmartEnum]` owners; the document-bound/archive-attached/free-floating origin collapses to one `[Union]` discriminant over the `DocumentOrFreeFloatingBase` duality.
- `Rasm` kernel: `AmbientLight`/`BackgroundColorTop`, gamma values, `Sun.Vector`, and texture-offset vectors compose the kernel color, scalar, and vector owners; a re-derived gamma curve or sun-direction solve beside `Sun.SunDirection` is the deleted form.
- `api-rhinocommon-rendercontent.md`: the per-usage `RenderEnvironment` binding points at content this catalog does not own.
- `api-rhinocommon-document.md`(`RhinoDoc.RenderSettings` accessor, `MaterialTable`/`LightTable`) and `api-rhinocommon-fileio.md`(`File3dmSettings.RenderSettings`): the document and archive accessors that mint and commit a `RenderSettings` are the seam, and the georeference `Sun` slice stays with the document catalog.

[LOCAL_ADMISSION]:
- A `RenderSettings` enters through the document or archive accessor, mutates inside the grant or a `using` window, and commits back through the same accessor; a detached value record leaves the boundary, never a live `RenderSettings` or sub-owner.
- `DocumentOrFreeFloatingBase` origin is the discriminant a domain owner carries; a parallel document-bound-versus-free-floating type pair beside the single duality is rejected.
- Environment usage enters through `EnvironmentUsage`/`EnvironmentPurpose`, never a stringly usage key; the render-channel set is `RenderChannels.Modes` plus `CustomList`, never an ad-hoc channel list.

[RAIL_LAW]:
- Surface: `Rhino.Render` render-settings family
- Owns: the `RenderSettings` aggregate (background, quality, output image, view source, environment binding), the `DocumentOrFreeFloatingBase` duality, and the `GroundPlane`/`Skylight`/`Sun`/`LinearWorkflow`/`Dithering`/`SafeFrame`/`RenderChannels` sub-owners.
- Accept: settings read and mutation through the document or archive accessor with a `using` window; per-usage environment binding over bounded usage/purpose enums; sub-owner origin carried as one `DocumentOrFreeFloatingBase` discriminant; settings values projected onto kernel color/vector owners and `Fin`/`Option` rails.
- Reject: a live `RenderSettings` or sub-owner escaping into a domain signature, a parallel document-bound/free-floating type pair beside the single duality, a re-derived gamma or sun-direction beside the kernel and `Sun.SunDirection`, a stringly environment-usage or render-channel key, and the `MaterialTable`/`LightTable`/georeference-`Sun` surface `api-rhinocommon-document.md` owns re-catalogued here.
