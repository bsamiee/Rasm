# [RASM_RHINO_API_RHINOCOMMON_RENDERSETTINGS]

`Rhino.Render` owns the document render-settings family: `RenderSettings` aggregates the background, antialiasing, output-image, and environment configuration over its ambient and output sub-owners. Every sub-owner derives `DocumentOrFreeFloatingBase`, one internal discriminant resolving document-bound, archive-attached, or free-floating identity — the duality this catalog states as law.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon render-settings surface
- host: Rhino host runtime, in-process (proprietary McNeel SDK)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Render`
- kernel: `Rasm`
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: render-settings boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: settings aggregate and duality base

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                                           |
| :-----: | :--------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `RenderSettings`             | `CommonObject` | background, antialias, output-image, environment, and sub-owner roster |
|  [02]   | `DocumentOrFreeFloatingBase` | abstract base  | document-bound, archive-attached, or free-floating discriminant        |

[PUBLIC_TYPE_SCOPE]: free-floating sub-owners

Each sub-owner is `sealed : DocumentOrFreeFloatingBase, IDisposable` and clones through `CopyFrom(FreeFloatingBase)`.

That `IDisposable` is a DETACH, not a release. `DocumentOrFreeFloatingBase` derives `FreeFloatingBase`, which is NOT `IDisposable`: the only native release is `DeleteCpp()` from the FINALIZER, gated on the `m_bAutoDelete` flag that ONLY the parameterless free-floating constructor sets. Every sub-owner reached through a `RenderSettings` property is a fresh NON-OWNING wrapper minted per read through a `nint`/document/`File3dm` constructor, and each `Dispose(bool)` body is EMPTY — the public `Dispose()` runs `GC.SuppressFinalize` alone. Disposing a document-, archive-, or settings-derived sub-owner therefore frees nothing (it retires a finalizer that would have freed nothing), while disposing a genuinely free-floating one LEAKS the native by suppressing its only reclaim path. A custody bracket over any sub-owner is refused on both branches.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]   | [CAPABILITY]                                                   |
| :-----: | :--------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `GroundPlane`    | ambient element | altitude, shadow-only, material, and texture-mapping placement |
|  [02]   | `Skylight`       | ambient element | skylight enable and shadow intensity                           |
|  [03]   | `Sun`            | ambient element | position, date/time/timezone, intensity, and derived direction |
|  [04]   | `LinearWorkflow` | output pipeline | pre/post gamma and framebuffer color processing                |
|  [05]   | `Dithering`      | output pipeline | dithering method toggle                                        |
|  [06]   | `SafeFrame`      | output pipeline | live, action, and title frame guides                           |
|  [07]   | `RenderChannels` | output pipeline | automatic or custom render-channel set                         |

[PUBLIC_TYPE_SCOPE]: post-effect container
- namespace: `Rhino.Render.PostEffects`

`RenderSettings.PostEffects` is the settings-side effect roster; `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-render.md` owns the `PostEffect` authoring, execution, and gating family this container configures.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]                                      | [CAPABILITY]                                           |
| :-----: | :--------------------- | :------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `PostEffectCollection` | `sealed : DocumentOrFreeFloatingBase, IDisposable` | ordered effect roster, per-stage selection, enumerable |
|  [02]   | `PostEffectData`       | `class, IDisposable`                               | one effect's on/shown/type/name/parameter cursor       |

[ENUM_ROSTERS]:
- `RenderSettings.RenderingSources` (`uint`): `ActiveViewport` `SpecificViewport` `NamedView` `SnapShot`
- `RenderSettings.EnvironmentUsage`: `Background` `Reflection` `Skylighting`
- `RenderSettings.EnvironmentPurpose`: `Standard` `ForRendering`
- `Sun.Accuracies`: `Minimum` `Maximum`
- `Dithering.Methods`: `None` `FloydSteinberg` `SimpleNoise`
- `RenderChannels.Modes`: `Automatic` `Custom`
- `PostEffects.PostEffectType`: `Early` `ToneMapping` `Late` — the stage `GetSelectedPostEffect`/`SetSelectedPostEffect` key on and `PostEffectData.Type` reads back.

## [03]-[ENTRYPOINTS]

[SETTINGS_AGGREGATE]:
- `RenderSettings.AmbientLight -> Color` / `BackgroundColorTop -> Color` / `BackgroundColorBottom -> Color` / `BackgroundStyle -> BackgroundStyle` / `TransparentBackground -> bool` — background color and style.
- `RenderSettings.AntialiasLevel -> AntialiasLevel` / `ShadowmapLevel -> int` / `RenderBackfaces -> bool` / `RenderCurves -> bool` / `RenderPoints -> bool` / `RenderMeshEdges -> bool` / `RenderAnnotations -> bool` / `RenderIsoparams -> bool` / `UseHiddenLights -> bool` / `DepthCue -> bool` / `FlatShade -> bool` — quality and element-inclusion flags.
- `RenderSettings.UseViewportSize -> bool` / `ImageSize -> Size` / `ImageDpi -> double` / `ImageUnitSystem -> UnitSystem` / `ScaleBackgroundToFit -> bool` — output-image dimensions and scaling.
- `RenderSettings.RenderSource -> RenderingSources` / `NamedView -> string` / `SpecificViewport -> string` / `Snapshot -> string` — the view the render samples.
- `RenderSettings.GroundPlane -> GroundPlane` / `Skylight -> Skylight` / `Sun -> Sun` / `LinearWorkflow -> LinearWorkflow` / `Dithering -> Dithering` / `SafeFrame -> SafeFrame` / `RenderChannels -> RenderChannels` / `PostEffects -> PostEffectCollection` — the sub-owner roster.
- `RenderSettings.RenderEnvironment(EnvironmentUsage, EnvironmentPurpose) -> RenderEnvironment` / `SetRenderEnvironment(EnvironmentUsage, RenderEnvironment)` / `RenderEnvironmentId(EnvironmentUsage, EnvironmentPurpose) -> Guid` / `SetRenderEnvironmentId(EnvironmentUsage, Guid)` — per-usage environment binding.
- `RenderSettings.RenderEnvironmentOverride(EnvironmentUsage) -> bool` / `SetRenderEnvironmentOverride(EnvironmentUsage, bool)` — per-usage override toggle.
- `ICurrentEnvironment.ForBackground` / `ForReflectionAndRefraction` / `ForLighting -> RenderEnvironment` (get/set) — the resolved current-environment triple behind `RhinoDoc.CurrentEnvironment`; the accessor is `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-document.md`'s.
- `RenderSettings.Duplicate() -> RenderSettings` — a detached copy; `RenderSettings : CommonObject` rides the document grant or a `using` window. `RhinoDoc.RenderSettings` answers `new RenderSettings(this)` on EVERY read, so the aggregate is a fresh document-bound wrapper per access and a caller reading it twice holds two wrappers over one native — a sub-owner window reads the aggregate once and threads it, or two reads of the same sub-owner sample host state at two instants the `Changed` broadcast can move between.
- `new RenderSettings()` / `new RenderSettings(RenderSettings)` — free-floating and copy constructors; each sub-owner carries a `()`/copy pair, and `SafeFrame(RhinoDoc)` adds a document-bound constructor.

[DUALITY_BASE]:
- `DocumentOrFreeFloatingBase` — resolves the native pointer from a document serial (document-bound), a `File3dm` (archive-attached), or neither (free-floating); `File3dm.Settings.RenderSettings` (get-only) lazily binds the archive-attached state.
- `DocumentOrFreeFloatingBase.BeginChange(RenderContent.ChangeContexts)` / `EndChange() -> bool` — inert no-ops (`EndChange` returns `false`); a bound sub-owner writes in place through its resolved pointer, a free-floating value transfers through `CopyFrom`.
- `<sub-owner>.CopyFrom(FreeFloatingBase)` — every sub-owner overrides it as its clone; `Changed -> EventHandler<RenderPropertyChangedEvent>` is the static property-changed broadcast on `GroundPlane`/`Skylight`/`Sun`/`SafeFrame`/`RenderChannels`.

[AMBIENT]:
- `GroundPlane.Enabled -> bool` / `ShadowOnly -> bool` / `ShowUnderside -> bool` / `Altitude -> double` / `AutoAltitude -> bool` — ground-plane placement.
- `GroundPlane.MaterialInstanceId -> Guid` / `TextureOffset -> Vector2d` / `TextureSize -> Vector2d` / `TextureRotation -> double` / `TextureOffsetLocked -> bool` / `TextureSizeLocked -> bool` — ground-plane material and texture mapping.
- `Skylight.Enabled -> bool` / `ShadowIntensity -> double` — skylight toggle and shadow strength.
- `Sun.Enabled -> bool` / `ManualControlOn -> bool` / `Intensity -> double` / `Accuracy -> Sun.Accuracies` / `North -> double` / `Vector -> Vector3d` / `Light -> Light` / `Hash -> uint` — sun enablement, manual override, derived scene contribution, and data CRC.
- `Sun.Azimuth -> double` / `Altitude -> double` / `Latitude -> double` / `Longitude -> double` / `TimeZone -> double` / `DaylightSavingOn -> bool` / `DaylightSavingMinutes -> int` — sun position and time context; the georeference `Latitude`/`Longitude`/`North` slice is `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-document.md`'s.
- `Sun.GetDateTime(DateTimeKind) -> DateTime` / `SetDateTime(DateTime, DateTimeKind)` — sun date/time.
- `Sun.SunDirection(double, double, DateTime) -> Vector3d` / `AltitudeFromValues(double, double, double, int, DateTime, double, bool) -> double` / `ColorFromAltitude(double) -> Color` / `JulianDay(double, int, DateTime, double) -> double` / `TwilightZone() -> double` / `Here(out double, out double) -> bool` — static sun-position and astronomical solvers.

[OUTPUT_PIPELINE]:
- `LinearWorkflow.PreProcessColors -> bool` / `PreProcessTextures -> bool` / `PostProcessFrameBuffer -> bool` / `PreProcessGamma -> float` / `PostProcessGamma -> float` / `PostProcessGammaReciprocal -> float` / `PostProcessGammaOn -> bool` / `Hash -> uint` — the gamma/linear color pipeline; `LinearWorkflow` overrides `Equals`/`GetHashCode` off `Hash`.
- `Dithering.Method -> Dithering.Methods` / `Enabled -> bool` — dithering method and toggle.
- `SafeFrame.Enabled -> bool` / `PerspectiveOnly -> bool` / `FieldsOn -> bool` / `LiveFrameOn -> bool` — frame-guide enablement.
- `SafeFrame.ActionFrameOn -> bool` / `ActionFrameLinked -> bool` / `ActionFrameXScale -> double` / `ActionFrameYScale -> double` / `TitleFrameOn -> bool` / `TitleFrameLinked -> bool` / `TitleFrameXScale -> double` / `TitleFrameYScale -> double` — action and title frame geometry.
- `RenderChannels.Mode -> RenderChannels.Modes` / `CustomList -> Guid[]` — automatic or custom render-channel set.

[POST_EFFECT_CONTAINER]:
- `PostEffectCollection()` / `PostEffectCollection(PostEffectCollection c)` — the free-floating and copy constructors; every document-bound instance arrives through `RenderSettings.PostEffects`, whose backing pointer resolves per read off the document's own `RenderSettings`.
- `PostEffectCollection.PostEffectDataFromId(Guid id) -> PostEffectData` — ALWAYS returns a fresh cursor and NEVER null: the body is `new PostEffectData(this, id)` with no lookup, so an unknown id fails later at the first member read, never at this call. A null-probe on the return is dead code; admission is `MovePostEffectBefore` returning `false`, the enumerator's own membership, or the throw the cursor raises.
- `PostEffectCollection.MovePostEffectBefore(Guid id_move, Guid id_before) -> bool` — reorder; `Guid.Empty` for `id_before` moves to the end, and an unfound `id_before` fails.
- `PostEffectCollection.GetSelectedPostEffect(PostEffectType type, out Guid id) -> bool` / `SetSelectedPostEffect(PostEffectType type, Guid id) -> void` — per-stage selection read and write.
- `PostEffectCollection.GetEnumerator() -> IEnumerator<PostEffectData>` — `IEnumerable<PostEffectData>`; the walk mints one cursor per row and the collection stays the owner.
- `PostEffectCollection.CopyFrom(FreeFloatingBase src) -> void` / `Dispose() -> void` — clone and disposal; the private `Dispose(bool)` body is EMPTY, so disposing the container releases nothing and the native lives on the document's render settings.

[POST_EFFECT_DATA]:
- `PostEffectData` is a NON-OWNING CURSOR: it stores only its `Collection` and effect `Guid`, and its internal `CppPointer` re-resolves through `Collection.FindPostEffect(id)` on EVERY member access, THROWING when the id is absent. Its `Dispose(bool)` body is empty, so a `using` over one asserts custody the type does not carry and disposing it mid-enumeration invalidates nothing — the collection alone owns every row.
- `PostEffectData.Id -> Guid` / `Type -> PostEffectType` / `LocalName -> string` — identity, stage, and localized name; `Type` folds the native `2u`/`3u` codes to `ToneMapping`/`Late` and every other value to `Early`.
- `PostEffectData.On -> bool` (get/set) / `Shown -> bool` (get/set) — enablement and roster visibility.
- `PostEffectData.GetParameter(string param_name) -> IConvertible` (`[CLSCompliant(false)]`) / `SetParameter(string param_name, object param_value) -> bool` — the arbitrary-parameter pair; an effect that does not know the name answers null on the read and false on the write, so the read projects to `Option` and the write confirms.
- `PostEffectData.DataCRC(uint current_remainder) -> uint` — `[CLSCompliant(false)]` state hash over the whole effect, the change-detection key a roster diff folds.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `RenderSettings` is the single aggregate: it holds background, quality, output-image, and view-source configuration and exposes each sub-owner as one property — a settings value never scatters across parallel owners, and `Duplicate` detaches a full copy.
- `DocumentOrFreeFloatingBase` is the one duality: the same `GroundPlane`/`Sun`/`LinearWorkflow` type is document-bound, archive-attached, or free-floating by its internal discriminant, so a domain owner discriminates on origin rather than holding three parallel types; `BeginChange`/`EndChange` are inert, and a free-floating mutation commits through the document or archive accessor.
- Environment binding is per-usage: `RenderEnvironment(EnvironmentUsage, EnvironmentPurpose)` and `SetRenderEnvironmentId` bind a `RenderEnvironment` per `Background`/`Reflection`/`Skylighting` usage, gated by the override toggle.
- Settings configure, rendering consumes: `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-render.md` reads a `RenderSettings` at render time, and this catalog never runs a render.
- The post-effect container is the settings side of that split: `PostEffectCollection` orders, selects, and parameterizes the roster while `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-render.md`'s `PostEffect`/`PostEffectPipeline` executes it, so a configuration write never reaches an execute body and an execute body never edits the roster.
- Row custody is the container's alone: a `PostEffectData` is a lazily-resolving cursor whose disposal is inert, so an arm holds rows for the whole demand window and disposes none of them; a `using` over one answers an ownership question the type does not pose.

[STACKING]:
- `RhinoCommon` value substrate(`libs/csharp/.api/api-rhinocommon.md`): the `Vector3d` carriers this boundary threads cross the wire from the substrate; it composes them and re-derives none.
- `LanguageExt.Core`(`libs/csharp/.api/api-languageext.md`): the `RenderSettings` aggregate rides a `using` bounded by `Eff` while every sub-owner rides the aggregate's window UNBRACKETED — a lease over one releases nothing document-side and leaks the free-floating native; a settings read projects a detached value record, and a commit-through-accessor outcome crosses as `Fin<Unit>`.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `RenderingSources`, `EnvironmentUsage`, `EnvironmentPurpose`, `Dithering.Methods`, `RenderChannels.Modes`, and `Sun.Accuracies` map at the edge to `[SmartEnum]` owners; the document-bound/archive-attached/free-floating origin collapses to one `[Union]` over `DocumentOrFreeFloatingBase`.
- `Rasm` kernel: `AmbientLight`/`BackgroundColorTop`, gamma values, `Sun.Vector`, and texture-offset vectors compose the kernel color, scalar, and vector owners; a re-derived gamma curve or sun-direction solve beside `Sun.SunDirection` is the deleted form.
- `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-rendercontent.md`: owns the content the per-usage `RenderEnvironment` binding points at.
- `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-document.md`(`RhinoDoc.RenderSettings`, `MaterialTable`/`LightTable`) and `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-fileio.md`(`File3dmSettings.RenderSettings`): the document and archive accessors mint and commit a `RenderSettings`, and the georeference `Sun` slice stays with the document catalog.

[LOCAL_ADMISSION]:
- A `RenderSettings` enters through the document or archive accessor, mutates inside the grant or a `using` window, and commits back through the same accessor; a detached value record leaves the boundary, never a live `RenderSettings` or sub-owner.
- `DocumentOrFreeFloatingBase` origin is the discriminant a domain owner carries; a parallel document-bound-versus-free-floating type pair beside the single duality is rejected.
- Environment usage enters through `EnvironmentUsage`/`EnvironmentPurpose`, never a stringly usage key; the render-channel set is `RenderChannels.Modes` with `CustomList`.

[RAIL_LAW]:
- Surface: `Rhino.Render` render-settings family + the `Rhino.Render.PostEffects` settings-side container
- Owns: the `RenderSettings` aggregate (background, quality, output image, view source, environment binding), the `DocumentOrFreeFloatingBase` duality, its ambient and output sub-owners, and the `PostEffectCollection`/`PostEffectData` roster surface.
- Accept: settings read and mutation through the document or archive accessor with a `using` window; per-usage environment binding over bounded usage/purpose enums; sub-owner origin carried as one `DocumentOrFreeFloatingBase` discriminant; settings values projected onto kernel color/vector owners and `Fin`/`Option` rails; a post-effect row read or written through a collection-owned cursor whose failure surfaces as the throw its member access raises.
- Reject: a live `RenderSettings` or sub-owner escaping into a domain signature, a parallel document-bound/free-floating type pair, a re-derived gamma or sun-direction beside the kernel and `Sun.SunDirection`, a stringly environment-usage or render-channel key, a null-probe on `PostEffectDataFromId` where the factory cannot answer null, a `using` or lease over a sub-owner or a `PostEffectData` whose disposal is inert, two reads of one sub-owner where a threaded window samples it once, and the `MaterialTable`/`LightTable`/georeference-`Sun` surface `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-document.md` owns re-catalogued here.
