# [PY_DATA_GRAPH_MESH]

Graph payloads and mesh-file exchange. `GraphPayload` carries graph kind/nodes/edges/attrs/directionality/algorithm-metadata over networkx with tabular/JSON/GraphML/bundle egress; `MeshPayload` carries mesh-file identity/cell-block topology/units/metadata/preview-export over meshio + trimesh. This is file exchange — the IFC to GLB tessellation rail belongs to the geometry package, and remote AEC streams are a runtime `TransportResource` row.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                          |
| :-----: | :-------- | :------------------------------------------------------------- |
|   [1]   | GRAPH     | graph payloads and graph egress                                |
|   [2]   | MESH      | mesh-file identity, cell-block topology, preview export        |

## [2]-[GRAPH]

- Owner: `GraphPayload` — graph kind/nodes/edges/attrs/directionality/algorithm-metadata over networkx; `GraphEgress` the tabular/JSON/GraphML/bundle export.
- Entry: `GraphPayload.of` admits a `networkx.Graph`/`DiGraph` and returns the frozen owner; `GraphEgress.write` emits the node-link JSON, GraphML, or a tabular edge list keyed by `ContentIdentity`; `GraphPayload.analyze` runs the named algorithm (shortest-path, centrality, components) over the networkx surface.
- Packages: `networkx` (`Graph`/`DiGraph`/`node_link_data`/`write_graphml`/`shortest_path`/`betweenness_centrality`/`connected_components`), runtime (`ContentIdentity`/`ReceiptContributor`).
- Growth: a new graph kind is one column on `GraphPayload`; a new algorithm is one `analyze` dispatch row; zero new surface.
- Boundary: no product collaboration store, no bridge lifecycle; graph algorithms, not product state; a per-algorithm `get_*` family is the deleted form. `networkx` is cp315-installed and reflected, so this owner is FINALIZED.

```python signature
from enum import StrEnum

import networkx as nx
from msgspec import Struct


class GraphKind(StrEnum):
    UNDIRECTED = "undirected"
    DIRECTED = "directed"
    MULTI = "multi"


class GraphPayload(Struct, frozen=True):
    kind: GraphKind
    node_count: int
    edge_count: int
    directed: bool

    @classmethod
    def of(cls, graph: nx.Graph) -> "GraphPayload":
        return cls(
            kind=GraphKind.DIRECTED if graph.is_directed() else GraphKind.UNDIRECTED,
            node_count=graph.number_of_nodes(),
            edge_count=graph.number_of_edges(),
            directed=graph.is_directed(),
        )


def write_node_link(graph: nx.Graph) -> "RuntimeRail[bytes]":
    return boundary("graph.egress", lambda: _encode(nx.node_link_data(graph)))
```

## [3]-[MESH]

- Owner: `MeshPayload` — mesh-file identity/cell-block topology/units/metadata/preview-export over meshio + trimesh.
- Entry: `MeshPayload.read` admits a mesh file (via `meshio.read` for FE meshes, `trimesh.load` for surface meshes) and returns the frozen owner keyed by `ContentIdentity`; `MeshPayload.preview` emits a preview render; `MeshPayload.write` round-trips through the format the kind selects.
- Packages: `meshio` (`read`/`write`/`Mesh`/`CellBlock`), `trimesh` (`load`/`Trimesh`/`export`), runtime (`ContentIdentity`/`ReceiptContributor`).
- Growth: a new mesh format is one `meshio`/`trimesh` format string; zero new surface.
- Boundary: no geometry kernel (that is the geometry package), no bridge lifecycle; the IFC to GLB tessellation rail is geometry-owned, never re-derived here; a hand-rolled mesh parser is the deleted form. This owner is `SPIKE` on the wheel floor.

```python signature
import meshio
from msgspec import Struct


class MeshPayload(Struct, frozen=True):
    content_key: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    units: str

    @classmethod
    def read(cls, ref: ResourceRef) -> "RuntimeRail[MeshPayload]":
        return boundary("mesh.read", lambda: cls._of(meshio.read(str(ref.path))))

    @staticmethod
    def _of(mesh: "meshio.Mesh") -> "MeshPayload":
        raw = mesh.points.tobytes()
        return MeshPayload(
            content_key=ContentIdentity.key("mesh", raw),
            point_count=len(mesh.points),
            cell_blocks=tuple(block.type for block in mesh.cells),
            units="m",
        )
```

## [4]-[RESEARCH]

- [MESHIO_CELLBLOCK]: the `meshio.Mesh.cells` `CellBlock.type` field and the `trimesh.load`/`export` format dispatch are verified against `.api/api-meshio.md` and `.api/api-trimesh.md` once a cp315 wheel installs the engines; `networkx.node_link_data`/`write_graphml` are reflected against the installed `networkx==3.6.1` (`.api/api-networkx.md`, no gap).
