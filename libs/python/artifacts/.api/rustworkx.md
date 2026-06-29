# rustworkx

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| Graph substrate | `rustworkx.PyGraph`, `rustworkx.PyDiGraph` | `.planning/visualization/diagram/layout.md` |
| Force-directed layout | `rustworkx.spring_layout` | `.planning/visualization/diagram/layout.md` |
| Graph algorithms | `rustworkx.topological_sort`, path and traversal algorithms | `.planning/visualization/diagram/layout.md` |
| Position mappings | `rustworkx.Pos2DMapping` | `.planning/visualization/diagram/layout.md` |

## Integration Rules

- `visualization/diagram/layout` owns `rustworkx` graph and algorithm use for diagram coordinates.
- The `grandalf` row handles hierarchical coordinate solving; `rustworkx` remains the graph and force/radial substrate.
- Provider graph objects do not cross into `diagram/draw` or `diagram/glyphset`.
