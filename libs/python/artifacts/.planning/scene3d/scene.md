# [PY_ARTIFACTS_SCENE]

The 3D scientific-visualization owner. `Scene3d` renders pyvista datasets on the VTK engine — `UnstructuredGrid`/`PolyData`, scalar fields, slices, isosurfaces — to a host-free offscreen image and exports glTF/VRML scene files. The render path is offscreen software GL (osmesa/EGL) so a scene rasterizes with zero display, browser, or GPU. The owner rides the gated `python_version < 3.13` native VTK floor: no cp315 VTK wheel yet, so it is floor-gated planned capability, not a blocked spike. Every render returns a `RuntimeRail[ContentKey]`.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[SCENE]`, the pyvista/VTK offscreen 3D render and glTF/VRML scene export.

## [2]-[SCENE]

- Owner: `Scene3d` the one 3D-scene owner over pyvista on the VTK engine; `SceneTarget` the closed `StrEnum` of render and export targets; the offscreen plotter is the one render capsule.
- Cases: `SceneTarget` rows `PNG` (offscreen `Plotter.screenshot`) · `GLTF` (`Plotter.export_gltf`, PolyData only — `UnstructuredGrid` surface-extracted via `extract_surface` first) · `VRML` (`Plotter.export_vrml`) — matched by `match`/`case`.
- Entry: `Scene3d.render` surface-extracts the grid to PolyData when the target is glTF (the VTK glTF exporter handles only PolyData), configures an offscreen `pyvista.Plotter(off_screen=True)`, adds the mesh with its active scalar field and colormap, and folds the target through `_render_target` — PNG to screenshot bytes, glTF/VRML to the serialized scene — keyed by the content key.
- Auto: dataset construction folds the array payload into a `pyvista.UnstructuredGrid`/`PolyData`; scalar coloring binds the active scalar name and the colormap from `color-management/colorimetry#COLOR`; the glTF target surface-extracts an `UnstructuredGrid` because the VTK glTF exporter handles only PolyData.
- Receipt: each render contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Scene` keyed by the content key.
- Packages: `pyvista` (`UnstructuredGrid`/`PolyData`/`Plotter`/`extract_surface`/`export_gltf`/`export_vrml`), `vtk` (the native render engine), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new scene export is one `SceneTarget` row; a new field-visualization mode is one acceptor arm; zero new surface.
- Boundary: no interactive viewer, no UI window, no display server; the dataset arrays arrive from data/compute outputs as inputs and own no mesh-file interchange (that stays at `data`); the offscreen software-GL render is the host-free path; the owner is gated on the sub-3.13 native VTK floor installing before re-reflection.

```python signature
from enum import StrEnum

import pyvista as pv
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


class SceneTarget(StrEnum):
    PNG = "png"
    GLTF = "gltf"
    VRML = "vrml"


class Scene3d(Struct, frozen=True):
    grid: object
    scalars: str
    colormap: str
    target: SceneTarget

    def render(self) -> RuntimeRail[ContentKey]:
        return boundary(f"scene.{self.target}", self._emit)

    def _emit(self) -> ContentKey:
        mesh = self.grid.extract_surface() if self.target is SceneTarget.GLTF else self.grid
        plotter = pv.Plotter(off_screen=True)
        plotter.add_mesh(mesh, scalars=self.scalars, cmap=self.colormap)
        data = _render_target(plotter, self.target)
        return ContentIdentity.key(f"scene-{self.target}", data)
```

## [3]-[RESEARCH]

- [SCENE_SPELLINGS]: the `_render_target` fold body — the pyvista `Plotter.screenshot` bytes return, the `export_gltf`/`export_vrml` serialization sink, and the `extract_surface` PolyData conversion — verifies against the branch `.api` catalogue for `pyvista`/`vtk` on the gated sub-3.13 native floor; the osmesa/EGL offscreen software-GL backend selection confirms once the native VTK floor installs.
