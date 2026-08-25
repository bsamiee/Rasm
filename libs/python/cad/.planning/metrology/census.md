# [PY_CAD_CENSUS]

`GlbCensus.of` is the provider's one measurement of the EMITTED discrete result: it re-opens the GLB bytes already written to a provider-owned path and reads placements, unique mesh faces, per-body closure, and per-body volume back out of them. Reading the artifact rather than the source shape is what makes the census a measurement: a count derived from `TopoDS` topology describes what was asked for, while a count derived from the file describes what a consumer will decode.

Emitted-file authority binds both directions. `tessellation/mesh#MESH` gates its triangle budget against this census rather than its own preflight sum, and `metrology/properties#MEASURE` receives closure as a `Closure` arm this owner elects, so no leg reconstructs mesh evidence from a source estimate or publishes a sentinel count where a decode refused. Every refusal lands one `CENSUS_DECODE` row on `CadRail`, and no exception escapes into the `anyio.to_process` worker's return path.

## [01]-[INDEX]

- [02]-[CENSUS]: Decode admission, placement collapse, face sum, weld and split, and the `Closure` election `measured` consumes.

## [02]-[CENSUS]

- Owner: `GlbCensus.of` — one mint over an emitted path; `GlbCensus` has no other constructor and no caller assembles its fields.
- Law: decode reads the emitted file. Source-shape estimates, preflight sums, and a sentinel count standing in for a failed decode are each refused.
- Law: `load_scene(path, file_type="glb", process=False)` admits the bytes untouched, preserving the scene graph and the glTF primitive split a conditioning pass erases before either reaches a count.
- Law: `file_type` is a declared parameter while `process` rides `**kwargs` unvalidated, so a misspelling there is swallowed whole, conditioning silently runs, and every count drifts off the bytes on disk.
- Law: placement collapses each `from_gltf_primitive` child onto its parent node, because one source mesh split by material is one placement.
- Law: triangles sum `len(mesh.faces)` once across `Scene.geometry.values()`, so an instanced geometry counts once no matter how many nodes place it.
- Law: closure and volume read the FLATTENED scene — `to_mesh()` applies every placement, then `merge_vertices` welds the bit-identical float32 seams glTF promotion introduces, without inheriting the mutable `tol.merge` default.
- Law: `Trimesh.split` dispatches to `trimesh.graph.split`, which resolves `networkx` or `scipy` and raises `ImportError` where neither is installed.
- Law: `networkx` rides the root manifest as that engine, so its absence is a deployment defect surfacing at import rather than a silent `ImportError` inside the worker wearing a `CENSUS_DECODE` row it never earned.
- Law: per-body volume sums absolute component volumes, so an inverted component contributes magnitude instead of cancelling a sibling into a forged zero the measure delta then certifies.
- Law: an emission that passed its byte-extent gate yet decodes to zero placements or zero triangles refuses, because publishing those zeros certifies an empty artifact as a measured result.
- Law: `closure` elects the `Closure` arm `metrology/properties#MEASURE` consumes, so watertight and emitted volume are decided once here; re-deriving that pair at the measure is what let the two disagree.
- Packages: `trimesh` for decode, placement traversal, flattening, welding, closure, and volume; `networkx` as the connected-component engine `trimesh.graph.split` dispatches to.
- Growth: a new emitted-artifact fact is one `GlbCensus` field filled inside `of`; no caller grows an argument and no second scene opens.
- Boundary: this owner opens the emitted file alone. Writing those bytes belongs to `tessellation/emission#EMISSION`, gating them against the budget to `tessellation/mesh#MESH`, and publishing them to `service/spool#SPOOL`.

```python
from pathlib import Path
from typing import Final

import trimesh
from expression import Error, Ok
from msgspec import Struct

from rasm.cad.faults import CENSUS_DECODE, CadRail
from rasm.cad.metrology.properties import OPEN, Closure

# --- [CONSTANTS] ------------------------------------------------------------------------

_DECODE_RAISES: Final[tuple[type[Exception], ...]] = (AssertionError, IndexError, KeyError, OSError, TypeError, ValueError)

_WELD_DIGITS: Final[int] = 15


# --- [MODELS] ---------------------------------------------------------------------------


class GlbCensus(Struct, frozen=True, gc=False):
    instances: int
    triangles: int
    watertight: bool
    volume_m3: float

    @property
    def closure(self) -> Closure:
        return Closure(closed=self.volume_m3) if self.watertight else OPEN

    @staticmethod
    def of(path: Path, /) -> CadRail["GlbCensus"]:
        try:
            scene = trimesh.load_scene(path, file_type="glb", process=False)
            parents = {child: parent for parent, child, _attributes in scene.graph.to_edgelist()}
            placements = {
                parents[node] if scene.geometry[scene.graph[node][1]].metadata.get("from_gltf_primitive", False) else node
                for node in scene.graph.nodes_geometry
            }
            triangles = sum(len(geometry.faces) for geometry in scene.geometry.values() if isinstance(geometry, trimesh.Trimesh))
            flattened = scene.to_mesh()
            flattened.merge_vertices(merge_tex=True, merge_norm=True, digits_vertex=_WELD_DIGITS)
            bodies = tuple(flattened.split(only_watertight=False))
        except _DECODE_RAISES as cause:
            return Error(CENSUS_DECODE.at(f"glb.decode:{type(cause).__name__}"))
        return _admitted(
            GlbCensus(
                instances=len(placements),
                triangles=triangles,
                watertight=bool(bodies) and all(body.is_watertight for body in bodies),
                volume_m3=sum(abs(float(body.volume)) for body in bodies),
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _admitted(census: GlbCensus, /) -> CadRail[GlbCensus]:
    return (
        Ok(census)
        if census.instances > 0 and census.triangles > 0
        else Error(CENSUS_DECODE.at(f"glb.empty:{census.instances}/{census.triangles}"))
    )
```

## [03]-[RESEARCH]

(none)
