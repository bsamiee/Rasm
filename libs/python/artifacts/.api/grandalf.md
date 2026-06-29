# grandalf

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| Graph structure | `grandalf.graphs.Vertex`, `grandalf.graphs.Edge`, `grandalf.graphs.Graph` | `.planning/visualization/diagram/layout.md` |
| Sugiyama hierarchy layout | `grandalf.layouts.SugiyamaLayout` | `.planning/visualization/diagram/layout.md` |
| Vertex geometry adapter | `grandalf.layouts.VertexViewer` | `.planning/visualization/diagram/layout.md` |
| Edge geometry adapter | `grandalf.routing.EdgeViewer` | `.planning/visualization/diagram/layout.md` |
| Edge routing | `grandalf.routing.route_with_lines`, `grandalf.routing.route_with_splines` | `.planning/visualization/diagram/layout.md` |
| Constraint-oriented layout | `grandalf.layouts.DigcoLayout` | `.planning/visualization/diagram/layout.md` |

## Integration Rules

- `visualization/diagram/layout` owns `grandalf`; draw code receives only `LayoutMap`, glyph coordinates, and routed edge points.
- `rustworkx` remains the graph algorithm substrate; `grandalf` is a hierarchical coordinate and routing engine selected by local `HierarchyEngine`.
- Provider graph, vertex, edge, and viewer objects are built and discarded inside the layout boundary.
