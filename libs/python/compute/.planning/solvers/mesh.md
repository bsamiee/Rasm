# [PY_COMPUTE_MESH]

The one simulation mesh-and-field interchange and weak-form assembly owner beside the FEM solver route. `MeshField` carries the mesh topology, the per-node and per-cell field arrays, and the reusable `assemble` that lowers a weak form to the sparse stiffness/load pair the `solvers/quadrature.md#QUADRATURE` FEM route and a Diffrax field problem both consume, never re-owning the solve. `MeshExchange` reads and writes the mesh-and-field through the meshio format registry. The assembly composes the scikit-fem `Basis`/`asm` pair over the same `ElementKind` axis the quadrature route discriminates; the solve stays on the quadrature owner, so the FEM page consumes the assembled pair rather than the reverse. `meshio` resolves on the cp315 core; `scikit-fem` rides the gated FEM `python_version<'3.15'` band.

## [01]-[INDEX]

- [01]-[MESH_FIELD]: the mesh topology, the per-node/per-cell field arrays, and the assemble fold
- [02]-[INTERCHANGE]: the meshio read/write mesh-and-field file interchange

## [02]-[MESH_FIELD]

- Owner: `MeshField` — the mesh-and-field interchange carrying the `points` node coordinates, the `cells` per-block connectivity keyed by `ElementKind`, the per-node `node_fields` and per-cell `cell_fields` array maps, and the content key over the canonical mesh-and-field buffer. `assemble(form)` lowers a `WeakForm` to the sparse `(stiffness, load)` pair through the scikit-fem `Basis`/`asm` fold over the element the `ElementKind` selects; the pair is the artifact the FEM solve and the field problem consume. The owner holds assembly and interchange only — never the solve, never a parallel mesh container beside the meshio `Mesh`.
- Element axis: `ElementKind` is the shared element/basis axis the quadrature FEM route discriminates (`solvers/quadrature.md#QUADRATURE`), so a `MeshField` assembled for `ElementKind.TRI_P1` lowers to the same `Basis(mesh, ElementTriP1())` the quadrature route reads; `WeakForm` carries the bilinear and linear integrand thunks and the boundary-facet Dirichlet condition, identical to the quadrature `FemForm` so the two never diverge on the weak-form shape.
- Entry: `MeshField.assemble` enters one `boundary("mesh.assemble", ...)` returning `RuntimeRail[AssembledSystem]`; it builds the `skfem.Basis` over the element, runs `skfem.asm(form.bilinear, basis)` for the stiffness and `skfem.asm(form.linear, basis)` for the load, and folds them into `AssembledSystem` carrying the sparse stiffness, the load vector, the Dirichlet dof indices from `basis.get_dofs`, and the dof count. The FEM solve route reads `AssembledSystem` and condenses-and-solves; the differential route reads the stiffness as a stationary field operator.
- Receipt: `AssembledSystem.contribute` emits one `Receipt.of("emitted", ...)` row carrying the element kind, the dof count, and the content key; a discretized field assembled once feeds the stiffness solve, the transient integration, and the study spine, each reading the same pair.
- Packages: `skfem` (`Basis`, `asm`, `ElementLineP1`, `ElementLineP2`, `ElementTriP1`, `ElementTriP2`, `ElementTetP1`, `ElementTetP2`, `ElementQuad1`, `ElementHex1`, `BilinearForm`, `LinearForm`, `MeshLine1`, `MeshTri1`, `MeshTet1`, `MeshQuad1`, `MeshHex1`), `meshio` (`Mesh`, `CellBlock`, `read`, `write`), `numpy` (`asarray`, `ascontiguousarray`, `concatenate`), `solvers/quadrature.md#QUADRATURE` (`ElementKind`, `FemForm` — the shared element and weak-form axes the solve consumes), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`).
- Growth: a new element is one `ElementKind` row shared with the quadrature route; a new field array is one entry in `node_fields` or `cell_fields`; a new assembled artifact is one field on `AssembledSystem`; zero new surface, never a parallel mesh container beside the meshio `Mesh` and never a solve on this owner.
- Boundary: assembly and interchange only — the mesh topology, the per-node/per-cell field arrays, the weak-form lowering, and the meshio file round-trip are in-scope; the solve stays on `solvers/quadrature.md#QUADRATURE` and the transient integration on `solvers/differential.md#DIFFERENTIAL`, so the FEM page consumes `MeshField` rather than the reverse. `meshio` is pure-Python and cp315-clean, so the interchange runs unconditionally; `scikit-fem` carries no cp315 wheel, so the `assemble` fold is authored against the documented `skfem` API on the gated band. A solve on this owner, a hand-rolled assembly loop where `BilinearForm`/`LinearForm`/`asm` own the concern, and a parallel per-format mesh container beside the meshio `Mesh` are the deleted forms; the mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

```python signature
from typing import TYPE_CHECKING

import numpy as np
from msgspec import Struct

from rasm.compute.solvers.quadrature import ElementKind, FemForm
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    import meshio


class AssembledSystem(Struct, frozen=True):
    element: ElementKind
    stiffness: object
    load: np.ndarray
    dirichlet_dofs: np.ndarray
    dof_count: int
    content_key: ContentKey

    def contribute(self) -> Receipt:
        facts = {"element": self.element.value, "dofs": str(self.dof_count), "key": str(self.content_key.value)}
        return Receipt.of("emitted", "compute.mesh", self.element.value, facts)


class MeshField(Struct, frozen=True):
    element: ElementKind
    points: np.ndarray
    cells: np.ndarray
    node_fields: dict[str, np.ndarray]
    cell_fields: dict[str, np.ndarray]

    def assemble(self, form: FemForm, /) -> "RuntimeRail[AssembledSystem]":
        return boundary("mesh.assemble", lambda: self._assemble(form))

    def _assemble(self, form: FemForm) -> AssembledSystem:
        import skfem

        mesh = getattr(skfem, _MESH_CTOR[self.element])(np.ascontiguousarray(self.points.T), np.ascontiguousarray(self.cells.T))
        element = getattr(skfem, _ELEMENT_CTOR[self.element])()
        basis = skfem.Basis(mesh, element)
        stiffness = skfem.asm(form.bilinear, basis)
        load = skfem.asm(form.linear, basis)
        dofs = basis.get_dofs(form.boundary_facets)
        return AssembledSystem(
            element=self.element,
            stiffness=stiffness,
            load=np.asarray(load),
            dirichlet_dofs=np.asarray(dofs.flatten()),
            dof_count=int(basis.N),
            content_key=self._content_key(),
        )

    def _content_key(self) -> ContentKey:
        buffers = (
            np.ascontiguousarray(self.points).tobytes(),
            np.ascontiguousarray(self.cells).tobytes(),
            *(np.ascontiguousarray(v).tobytes() for v in self.node_fields.values()),
            *(np.ascontiguousarray(v).tobytes() for v in self.cell_fields.values()),
        )
        return ContentIdentity.of("mesh-field", b"\x00".join(buffers), IdentityPolicy())


_ELEMENT_CTOR = {
    ElementKind.P1: "ElementLineP1",
    ElementKind.P2: "ElementLineP2",
    ElementKind.TRI_P1: "ElementTriP1",
    ElementKind.TRI_P2: "ElementTriP2",
    ElementKind.TET_P1: "ElementTetP1",
    ElementKind.TET_P2: "ElementTetP2",
    ElementKind.QUAD_P1: "ElementQuad1",
    ElementKind.HEX_P1: "ElementHex1",
}


_MESH_CTOR = {
    ElementKind.P1: "MeshLine1",
    ElementKind.P2: "MeshLine1",
    ElementKind.TRI_P1: "MeshTri1",
    ElementKind.TRI_P2: "MeshTri1",
    ElementKind.TET_P1: "MeshTet1",
    ElementKind.TET_P2: "MeshTet1",
    ElementKind.QUAD_P1: "MeshQuad1",
    ElementKind.HEX_P1: "MeshHex1",
}
```

## [03]-[INTERCHANGE]

- Owner: `MeshExchange` — the meshio read/write interchange between a `MeshField` and the on-disk unstructured-mesh formats; `read` parses any meshio-supported format into a `MeshField`, mapping the meshio `points` and the first `CellBlock` connectivity onto the topology and the meshio `point_data`/`cell_data` onto the field maps, and `write` serializes a `MeshField` back through the meshio format dispatch. The meshio `Mesh` is the canonical container; this owner never assembles a parallel per-format mesh shape.
- Entry: `MeshExchange.read` returns `RuntimeRail[MeshField]` through one `boundary`, calling `meshio.read(path)` and projecting the container; `MeshExchange.write` returns `RuntimeRail[None]`, building a `meshio.Mesh(points, [CellBlock(cell_type, cells)], point_data=..., cell_data=...)` and calling `meshio.write(path, mesh)` with extension-driven format detection. The `_CELL_TYPE` map keys the `ElementKind` to the meshio cell-type string (`line`/`triangle`/`tetra`).
- Packages: `meshio` (`read`, `write`, `Mesh`, `CellBlock`, `points`, `cells`, `point_data`, `cell_data`, `cells_dict`), `numpy`, runtime (`RuntimeRail`, `boundary`).
- Growth: a new element-to-meshio cell type is one `_CELL_TYPE` row; a new format is zero new surface because meshio owns the format registry; never a hand-rolled format parser.
- Boundary: file interchange through the meshio registry only — the format detection, the cell-type mapping, and the field round-trip are in-scope. A hand-rolled format parser and a wrapper-rename of `read`/`write` are the deleted forms; meshio owns the ~40-format registry and this owner composes it.

```python signature
from pathlib import Path
from typing import TYPE_CHECKING

import numpy as np

from rasm.compute.solvers.quadrature import ElementKind
from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    import meshio


_CELL_TYPE: dict[ElementKind, str] = {
    ElementKind.P1: "line",
    ElementKind.P2: "line",
    ElementKind.TRI_P1: "triangle",
    ElementKind.TRI_P2: "triangle",
    ElementKind.TET_P1: "tetra",
    ElementKind.TET_P2: "tetra",
    ElementKind.QUAD_P1: "quad",
    ElementKind.HEX_P1: "hexahedron",
}


class MeshExchange:
    @staticmethod
    def read(path: Path, element: ElementKind, /) -> "RuntimeRail[MeshField]":
        return boundary("mesh.read", lambda: MeshExchange._read(path, element))

    @staticmethod
    def write(field: "MeshField", path: Path, /) -> "RuntimeRail[None]":
        return boundary("mesh.write", lambda: MeshExchange._write(field, path))

    @staticmethod
    def _read(path: Path, element: ElementKind) -> "MeshField":
        import meshio

        mesh = meshio.read(str(path))
        cells = mesh.cells_dict[_CELL_TYPE[element]]
        return MeshField(
            element=element,
            points=np.asarray(mesh.points),
            cells=np.asarray(cells),
            node_fields={k: np.asarray(v) for k, v in mesh.point_data.items()},
            cell_fields={k: np.asarray(v[0]) for k, v in mesh.cell_data.items()},
        )

    @staticmethod
    def _write(field: "MeshField", path: Path) -> None:
        import meshio

        cell_type = _CELL_TYPE[field.element]
        mesh = meshio.Mesh(
            points=np.asarray(field.points),
            cells=[meshio.CellBlock(cell_type, np.asarray(field.cells))],
            point_data={k: np.asarray(v) for k, v in field.node_fields.items()},
            cell_data={k: [np.asarray(v)] for k, v in field.cell_fields.items()},
        )
        meshio.write(str(path), mesh)
```

## [04]-[RESEARCH]

- [SKFEM_ASSEMBLE]: the `skfem.Basis(mesh, element)`, `skfem.asm(form, basis)`, `basis.get_dofs(facets)`, `basis.N` dof-count, and the `ElementLineP1`/`ElementLineP2`/`ElementTriP1`/`ElementTriP2`/`ElementTetP1`/`ElementTetP2`/`ElementQuad1`/`ElementHex1` element and `MeshLine1`/`MeshTri1`/`MeshTet1`/`MeshQuad1`/`MeshHex1` mesh-constructor spellings verify against `compute/.api/scikit-fem.md`; every `ElementKind` row resolves a catalogued element-and-mesh constructor pair through `_ELEMENT_CTOR`/`_MESH_CTOR`, so the assemble fold spans the full P1/P2 line/tri/tet plus the bilinear-quad and trilinear-hex family rather than catalogued-but-unreached constructors. The `Mesh(points.T, cells.T)` node-major/element-major array layout (`skfem` stores `mesh.p` as `(dim, n)` and `mesh.t` as `(verts, n_elem)`) confirms the `.T` transpose against the catalogue at fence transcription. `scikit-fem` carries no cp315 wheel, so the assemble fold rides the gated `python_version<'3.15'` band.
- [MESHIO_INTERCHANGE]: the `meshio.read(path)`, `meshio.write(path, mesh)`, `meshio.Mesh(points, cells, point_data, cell_data)`, `meshio.CellBlock(cell_type, data)`, and `Mesh.cells_dict`/`point_data`/`cell_data` spellings verify against `compute/.api/meshio.md`; `cell_data` values parallel the `cells` block list as a per-block array list, so the read projects `v[0]` for the first block and the write wraps each field array in a single-element list. `meshio` is pure-Python and cp315-clean, so the interchange runs unconditionally on cp315.
- [SHARED_ELEMENT_AXIS]: `ElementKind` and `FemForm` are owned by `solvers/quadrature.md#QUADRATURE`; this owner composes them so the weak-form shape and the element vocabulary never diverge between the assembly owner and the FEM solve route. The `_ELEMENT_CTOR`/`_MESH_CTOR` string maps name the `skfem` constructors resolved by `getattr(skfem, ...)` at assemble time, keeping the gated import behind the boundary.
```
