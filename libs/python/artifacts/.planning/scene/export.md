# [PY_ARTIFACTS_SCENE_EXPORT]

The 3D scene-file export owner. `SceneTarget` is the closed file-target `StrEnum` keyed inside the `scene/render#SCENE` `SceneOp.Export` payload — `PNG`/`GLTF`/`VRML`/`OBJ`/`HTML`/`USD`/`USDZ` — each row carrying the file extension and naming the exporter arm, never a parallel per-export owner. The `render_export` worker arm dispatches the `SceneTarget` value over one total `match` against the offscreen `pyvista.Plotter` the `scene/render#SCENE` `render_plotter` capsule produces: `GLTF` surface-extracts to `PolyData` through the `FieldFilter.Surface` filter chain because the VTK glTF exporter handles only `PolyData`, `VRML`/`OBJ`/`HTML` serialize the scene through `export_vrml`/`export_obj`/`export_html`, `PNG` writes the offscreen raster, and `USD`/`USDZ` delegate to `scene/stage#STAGE` (the `vtkUSDExporter`-plus-`UsdUtils` stage-authoring arm) — the format discriminated by the typed `SceneTarget` value crossing the seam as its `.value` and re-admitted once at the worker dispatch, never a parallel per-format export surface. The owner rides the same gated `python_version<'3.13'` native VTK floor as the render owner: no cp315 VTK wheel, so `render_export` imports `pyvista` only on the sub-3.13 companion-floor worker behind the `anyio.to_process.run_sync` subprocess seam. Each export returns the serialized scene-file `bytes` the `scene/render#SCENE` `Export` arm keys through `ContentIdentity.of(target.value, data)` into one `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target)`. This owner holds the `geometry/mesh` boundary: a scene file is a rendered-scene serialization (camera/lights/PBR-mapped surfaces), the geometry mesh-file codec is a raw mesh-interchange writer — no shared owner. This page closes the scene-file-export half of the `SCENE_FILE_EXPORT` idea.

## [01]-[INDEX]

- [02]-[EXPORT]: the `SceneTarget` closed file-target `StrEnum` keyed inside the `scene/render#SCENE` `SceneOp.Export` payload, the `render_export` worker arm dispatching the `SceneTarget` value over one total `match` against the `render_plotter` offscreen capsule, the `GLTF` `FieldFilter.Surface` surface-extract pre-pass, the `VRML`/`OBJ`/`HTML` serialize arms, the `USD`/`USDZ` delegation to `scene/stage#STAGE`, and the `geometry/mesh/repair#MESH_CODEC_BOUNDARY` seam; `pyvista` `Plotter.export_gltf`/`export_vrml`/`export_obj`/`export_html`/`extract_surface` settled against the folder `.api`, the temp-file-sink-then-`read_bytes` serialize pattern, and the `Plotter.render_window` accessor feeding the `scene/stage#STAGE` USD arm carried as a [03]-[RESEARCH] catalogue-deepen seam.

## [02]-[EXPORT]

- Owner: `SceneTarget` the one closed `StrEnum` of file-export targets keyed inside the `scene/render#SCENE` `SceneOp.Export` payload — `PNG`/`GLTF`/`VRML`/`OBJ`/`HTML`/`USD`/`USDZ` — each row carrying the file extension as its string value and naming the exporter arm, the `USD` value `"usdc"` (the crate-binary suffix) and `USDZ` value `"usdz"` (the AR/Apple package suffix) distinguished from `PNG`'s `"png"`; never a parallel per-format export owner, the format axis is one enum row, the exporter arm one `match` case. `render_export` the one worker-arm function the `scene/render#SCENE` `_emit` `Export` case dispatches over `to_process.run_sync(render_export, grid, target.value, spec)`, re-admitting the crossed `target` string once through `SceneTarget(target)` and routing to the catalogued exporter; the offscreen `pyvista.Plotter` is the shared render capsule the `scene/render#SCENE` `render_plotter` produces, applying the same `RenderSpec.staged`/`added`/`viewed` projections, so a file export and an image render share one render path, never a parallel export-only plotter.
- Cases: the `SceneTarget` `match` arms — `GLTF` (surface-extracts the staged dataset to `PolyData` by appending `FieldFilter.Surface()` to the `RenderSpec.filters` chain through `msgspec.structs.replace` before `render_plotter`, because the VTK glTF exporter handles only `PolyData`; then `Plotter.export_gltf(path)` serializes the surface scene) · `VRML`/`OBJ`/`HTML` (serialize the rendered scene directly through `Plotter.export_vrml`/`export_obj`/`export_html`, no surface pre-pass, the full multi-actor scene with camera/lights/materials) · `PNG` (writes the offscreen raster through the `scene/render#SCENE` `png` sink, the same temp-file PNG artifact the `Image` arm produces) · `USD`/`USDZ` (delegate to `scene/stage#STAGE`'s `export_usd`/`package_usdz` over the `Plotter.render_window` the `vtkUSDExporter` writes the `.usdc` layer from, then `UsdUtils.CreateNewUsdzPackage` packages the `.usdz` deliverable — the stage owner holds the `pxr` import and the USD authoring law, this owner only routes the `SceneTarget` value to it) — matched by one total `match`/`case` over the re-admitted `SceneTarget`, the serialized scene-file `bytes` read from the temp-file sink, never a per-format export function family duplicating the temp-dir-then-read-bytes sink.
- Entry: `render_export(grid, target, spec)` runs on the gated sub-3.13 companion-floor worker behind the `scene/render#SCENE` `to_process.run_sync` seam; it re-admits the crossed `target` string through `SceneTarget(target)`, conditionally surfaces the spec for `GLTF`, constructs the `render_plotter` offscreen capsule, opens one `TemporaryDirectory` sink, dispatches the `SceneTarget` `match`, and reads the written file's `bytes` (the `PNG` arm returns through the `png` sink directly; the `USD`/`USDZ` arms call into `scene/stage#STAGE`). The returned `bytes` cross the seam back to the `scene/render#SCENE` `_emit` `Export` case, which keys one `ContentKey` through `ContentIdentity.of(target.value, data)` — the `SceneTarget` value (`"gltf"`/`"vrml"`/`"obj"`/`"html"`/`"usdc"`/`"usdz"`/`"png"`) the content-key format tag, so the scene file is one content-addressed unit, the format recovered from the key tag the receipt also carries.
- Auto: the `GLTF` arm's `FieldFilter.Surface()` append folds into the existing `RenderSpec.staged` reduce — the surface-extract is one more filter in the chain, not a special-case branch — so a clip-then-glTF export runs the clip filter then the surface extract then the glTF serialize through one filter fold; the `render_plotter` capsule the export shares with the image render applies the full `staged`/`added`/`viewed` projection, so a glTF export carries the same scalar coloring, overlays, and camera the image render would; the temp-file sink is opened once per export and `read_bytes` reads the serialized scene, the `pyvista` exporter writing the file the worker reads; the `USD`/`USDZ` arms hand the `Plotter.render_window` to `scene/stage#STAGE`, which writes the `.usdc` layer and packages the `.usdz`.
- Receipt: each export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target)` keyed by the content key the `scene/render#SCENE` `_emit` arm derives, the `SceneTarget` value carried as the target string; the receipt is minted once at the `scene/render#SCENE` `Export` arm over the returned `bytes`, this owner contributing the serialized scene-file payload, never a parallel per-format receipt rail.
- Packages: `pyvista` (`Plotter.export_gltf`/`Plotter.export_vrml`/`Plotter.export_obj`/`Plotter.export_html`/`DataSet.extract_surface` settled against the folder `.api`; `Plotter.render_window` feeding the `scene/stage#STAGE` `vtkUSDExporter.SetRenderWindow` argument [03]-[RESEARCH]) gated `python_version<'3.13'`; `msgspec` (`structs.replace` surfacing the `RenderSpec.filters` chain for the `GLTF` surface pre-pass); runtime (the `anyio.to_process.run_sync` lane the export crosses, the `ContentIdentity.of` keying inherited from `scene/render#SCENE`). The `USD`/`USDZ` arms compose `scene/stage#STAGE`'s `usd-core`/`vtk` surface, never importing `pxr` or `vtkmodules.vtkIOUSD` here.
- Growth: a new scene file-export is one `SceneTarget` row plus one `match` arm in `render_export`; a new serialization variant (binary glTF `.glb`, draco-compressed) is one `SceneTarget` row plus the catalogued exporter keyword; a new surface-only target follows the `GLTF` `FieldFilter.Surface` pre-pass pattern; zero new owner — the file-target axis stays one `StrEnum`, the exporter axis one `match`, the USD authoring law fully delegated to `scene/stage#STAGE`.
- Boundary: a scene file is a rendered-scene serialization (camera/lights/PBR-mapped surfaces from the offscreen render), distinct from the `geometry/mesh` raw mesh-file codec — the `geometry/mesh/repair#MESH_CODEC_BOUNDARY` seam records that scene = visualization-scene export and geometry mesh = mesh-file codec, no shared owner, so a future pass cannot collapse them into one mesh-export owner. The OBJ/glTF a scene exports carries materials, lights, and the camera the geometry mesh codec never authors; conversely the geometry codec's raw `.stl`/`.ply`/`.obj` mesh interchange carries no scene state. The offscreen software-GL render the export serializes is the host-free path; `pyvista`/`vtk` ride the gated `python_version<'3.13'` band, so `render_export` imports `pyvista` only on the sub-3.13 worker, never on the cp315-core page. The prior flat per-format export surface that re-derived the temp-dir-then-`read_bytes` sink three times and the `SceneTarget`-from-string round-trip re-deriving a discriminant the union already carried are the deleted forms — the file-target axis is now one `SceneTarget` `StrEnum` keyed inside the `Export` payload, the exporter axis one `match` over one shared `render_plotter` capsule and one shared temp-file sink, the USD authoring law fully delegated to `scene/stage#STAGE`.

```python signature
from enum import StrEnum
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import assert_never

from msgspec.structs import replace

from rasm.artifacts.scene.render import FieldFilter, RenderSpec
from rasm.artifacts.scene.render_worker import png, render_plotter
from rasm.artifacts.scene.stage import export_usd, package_usdz


class SceneTarget(StrEnum):
    PNG = "png"
    GLTF = "gltf"
    VRML = "vrml"
    OBJ = "obj"
    HTML = "html"
    USD = "usdc"
    USDZ = "usdz"


def render_export(grid: object, target: str, spec: RenderSpec) -> bytes:
    kind = SceneTarget(target)
    surfaced = replace(spec, filters=(*spec.filters, FieldFilter.Surface())) if kind is SceneTarget.GLTF else spec
    plotter = render_plotter(grid, surfaced)
    with TemporaryDirectory() as work:
        out = Path(work) / f"scene.{target}"
        match kind:
            case SceneTarget.GLTF:
                plotter.export_gltf(str(out))
            case SceneTarget.VRML:
                plotter.export_vrml(str(out))
            case SceneTarget.OBJ:
                plotter.export_obj(str(out))
            case SceneTarget.HTML:
                plotter.export_html(str(out))
            case SceneTarget.USD:
                export_usd(plotter.render_window, str(out))
            case SceneTarget.USDZ:
                usdc = out.with_suffix(".usdc")
                export_usd(plotter.render_window, str(usdc))
                package_usdz(str(usdc), str(out))
            case SceneTarget.PNG:
                return png(plotter, out, spec)
            case _:
                assert_never(kind)
        return out.read_bytes()
```

## [03]-[RESEARCH]

- [SCENE_EXPORT]: the `pyvista.Plotter.export_gltf(filename)`, `export_vrml(filename)`, `export_obj(filename)`, `export_html(filename)`, and `DataSet.extract_surface()` scene serializers verify against the folder `.api` catalogue for `pyvista` (`0.48.4` on `vtk` `9.6.2`, the gated `python_version<'3.13'` sub-3.13 companion floor) — the `pyvista.md` `[03]-[ENTRYPOINTS]` export rows carry `export_gltf(filename, inline_data=True, rotate_scene=True, save_normals=True)`, `export_html(filename) -> io.StringIO | None`, `export_obj(filename)`/`export_vrml(filename)`, and `extract_surface(pass_pointid=True, pass_cellid=True, ...) -> PolyData` (named as the surface pre-pass preceding glTF export in `[04]-[IMPLEMENTATION_LAW]`), so the `GLTF`/`VRML`/`OBJ`/`HTML` `match` arms and the `FieldFilter.Surface` surface-extract are settled fence code; the `import_gltf`/`import_obj`/`import_vrml` inverse round-trip is catalogued capacity the growth axis absorbs. The temp-file-sink-then-`read_bytes` serialize pattern is settled (the `pyvista` exporters write a file, the worker reads its bytes), and the `msgspec.structs.replace` surfacing of the `RenderSpec.filters` chain for the `GLTF` arm is settled `msgspec` fence code.
- [RENDER_WINDOW] [RESEARCH]: the `Plotter.render_window` property feeding the `scene/stage#STAGE` `vtkUSDExporter.SetRenderWindow` argument for the `USD`/`USDZ` arms is NOT in the folder `.api` catalogue for `pyvista` — the `pyvista.md` `[03]-[ENTRYPOINTS]` tables catalogue `Plotter(off_screen=...)`, `Plotter.add_mesh`, `Plotter.screenshot`, `Plotter.show`, the `export_gltf`/`export_vrml`/`export_obj`/`export_html` family, and the `import_gltf`/`import_obj`/`import_vrml` round-trip, but no `render_window` accessor exposing the underlying `vtkRenderWindow` the USD exporter requires. The `export_usd(plotter.render_window, ...)`/`package_usdz` delegation to `scene/stage#STAGE` consumes it; the access stays a marked RESEARCH catalogue-deepen seam until a `Plotter.render_window` reflection pass lands on the gated `python_version<'3.13'` band. Close-condition: `.api` catalogue carries `Plotter.render_window`.
