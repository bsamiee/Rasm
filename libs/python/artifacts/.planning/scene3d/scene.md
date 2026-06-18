# [PY_ARTIFACTS_SCENE]

The 3D scientific-visualization owner. `Scene3d` renders pyvista datasets on the VTK engine — `UnstructuredGrid`/`PolyData`, scalar fields, slices, isosurfaces — to a host-free offscreen image and exports glTF/VRML scene files. The render path is offscreen software GL (osmesa/EGL) so a scene rasterizes with zero display, browser, or GPU. The owner rides the gated `python_version<'3.13'` native VTK floor: no cp315 VTK wheel, so the cp315-core process imports neither package and the whole render crosses the runtime subprocess lane onto the sub-3.13 companion-floor worker — floor-gated planned capability, not a blocked spike. Every render returns a `RuntimeRail[ContentKey]`.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[SCENE]`, the pyvista/VTK offscreen 3D render and glTF/VRML scene export.

## [2]-[SCENE]

- Owner: `Scene3d` the one 3D-scene owner over pyvista on the VTK engine; `SceneTarget` the closed `StrEnum` of render and export targets; the offscreen plotter is the one render capsule.
- Cases: `SceneTarget` rows `PNG` (offscreen `Plotter.screenshot`) · `GLTF` (`Plotter.export_gltf`, PolyData only — `UnstructuredGrid` surface-extracted via `extract_surface` first) · `VRML` (`Plotter.export_vrml`) — matched by `match`/`case`.
- Entry: `Scene3d.render` is `async` over the runtime `async_boundary`, dispatching the whole render onto the sub-3.13 companion-floor worker (the cp315-core process imports neither `pyvista` nor `vtk`), keyed by the content key; the worker surface-extracts the grid to PolyData when the target is glTF (the VTK glTF exporter handles only PolyData), configures an offscreen `pyvista.Plotter(off_screen=True)`, adds the mesh with its active scalar field and colormap, and folds the target through `_render_target` — PNG to screenshot bytes, glTF/VRML to the serialized scene.
- Auto: dataset construction folds the array payload into a `pyvista.UnstructuredGrid`/`PolyData`; scalar coloring binds the active scalar name and the colormap from `color-management/colorimetry#COLOR`; the glTF target surface-extracts an `UnstructuredGrid` because the VTK glTF exporter handles only PolyData.
- Receipt: each render contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Scene` keyed by the content key.
- Packages: `pyvista` (`UnstructuredGrid`/`PolyData`/`Plotter`/`extract_surface`/`export_gltf`/`export_vrml`), `vtk` (the native render engine) gated `python_version<'3.13'`; runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane)).
- Growth: a new scene export is one `SceneTarget` row; a new field-visualization mode is one acceptor arm; zero new surface.
- Boundary: no interactive viewer, no UI window, no display server; the dataset arrays arrive from data/compute outputs as inputs and own no mesh-file interchange (that stays at `data`); the offscreen software-GL render is the host-free path. `pyvista`/`vtk` ride the gated `python_version<'3.13'` band and never resolve in the cp315-core process, so the entire render runs through the runtime subprocess lane (`anyio.to_process.run_sync`) onto the sub-3.13 companion-floor worker that imports them at module scope — neither a module-top nor a lazy gated import lands on the core page.

```python signature
from enum import StrEnum

from anyio import to_process
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary


class SceneTarget(StrEnum):
    PNG = "png"
    GLTF = "gltf"
    VRML = "vrml"


class Scene3d(Struct, frozen=True):
    grid: object
    scalars: str
    colormap: str
    target: SceneTarget

    async def render(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"scene.{self.target}", self._emit)

    async def _emit(self) -> ContentKey:
        data = await to_process.run_sync(
            _render_scene, self.target.value, self.grid, self.scalars, self.colormap
        )
        return ContentIdentity.of(f"scene-{self.target}", data)
```

## [3]-[RESEARCH]

- [SCENE_WORKER]: `_render_scene` runs on the `python_version<'3.13'` companion floor through `anyio.to_process.run_sync`, importing `pyvista`/`vtk` at module scope inside the gated-band worker, never on the cp315-core owner. The worker surface-extracts the grid (`extract_surface` to PolyData for the glTF target), builds the offscreen `pyvista.Plotter(off_screen=True)`, adds the mesh under the active scalar field and colormap, and folds the target — `Plotter.screenshot` bytes for PNG, `export_gltf`/`export_vrml` serialization for the scene targets. The pyvista/vtk call spellings and the osmesa/EGL offscreen software-GL backend selection verify against the folder `.api` catalogues for `pyvista`/`vtk` once the sub-3.13 native floor installs.
