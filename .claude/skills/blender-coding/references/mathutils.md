# [MATHUTILS]

## [01]-[ARRAYS]

`mathutils` values store `float32` and expose the buffer protocol, and work over many values runs as numpy arrays:
- `np.asarray(<Matrix>)` reads a `(4, 4)` `float32` array in row order, and `Matrix(<array>)` and `Vector(<array>)` take `float64` arrays
- `Matrix @ <list of Vector>` raises `TypeError`, point sets transforming as `points @ matrix[:3, :3].T + matrix[:3, 3]`
- `freeze()` returns a hashable `Vector`, `Matrix`, `Euler`, or `Quaternion` for dict and set keys
- `bl_math.clamp`, `lerp`, and `smoothstep` take and return Python floats, `clamp` over an array answering one float

Transforms of many objects read through one `foreach_get` per member, `matrix_world` arriving in stored column order and `bound_box` as 24 floats per object, and `np.matvec` broadcasts each matrix over its points:

```python
type Objects = bpy.types.bpy_prop_collection[bpy.types.Object]


def corners(objects: Objects) -> NDArray[np.float32]:
    count = len(objects)
    worlds, boxes = np.empty(count * 16, np.float32), np.empty(count * 24, np.float32)
    objects.foreach_get("matrix_world", worlds)
    objects.foreach_get("bound_box", boxes)
    matrices = worlds.reshape(count, 4, 4).transpose(0, 2, 1)
    placed: NDArray[np.float32] = np.matvec(matrices[:, None, :3, :3], boxes.reshape(count, 8, 3)) + matrices[:, None, :3, 3]
    return placed
```

`corners(scene.objects)` answers `(objects, 8, 3)` world corners in `scene.objects` order, one call in place of eight `matrix_world @ Vector(corner)` products per object.

## [02]-[ROTATIONS]

Rotation values follow their own continuity and decomposition members:
- `Euler.make_compatible(<prior>)` and `Quaternion.make_compatible(<prior>)` keep successive keyed rotations on one branch, an Euler `z` of `-3.1` after `3.1` reading `3.18`
- `Matrix.LocRotScale(<location>, <rotation>, <scale>)` builds a transform from parts, `None` holding an identity part, and `decompose()` answers location, `Quaternion`, and scale
- `Vector.rotation_difference`, `Quaternion.to_swing_twist(<axis>)`, and `Vector.to_track_quat(<track>, <up>)` answer rotations between directions

## [03]-[SPATIAL_QUERIES]

Spatial questions take the member that answers them:

| [INDEX] | [QUESTION]                         | [MEMBER]                                                                    |
| :-----: | :--------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | Nearest points to a query          | `kdtree.KDTree`, `insert`, `balance()`, then `find`, `find_n`, `find_range` |
|  [02]   | Ray hits, nearest surface, overlap | `bvhtree.BVHTree.FromPolygons`, `FromObject(obj, depsgraph)`, `FromBMesh`   |
|  [03]   | N-gon outline into triangles       | `geometry.tessellate_polygon`                                               |
|  [04]   | Constrained 2D triangulation       | `geometry.delaunay_2d_cdt`                                                  |
|  [05]   | Rectangles packed into a square    | `geometry.box_pack_2d`                                                      |
|  [06]   | 2D convex hull                     | `geometry.convex_hull_2d`                                                   |
|  [07]   | Barycentric weights over an n-gon  | `interpolate.poly_3d_calc`                                                  |

Queries follow host rules:
- `KDTree` takes 3D coordinates alone, a 2D `insert` raising `ValueError`
- `mathutils.noise` values differ from the shader Noise Texture node, node values coming from an evaluated node tree

`BVHTree.FromPolygons` takes a positions array and one index array per face, and `overlap` answers face index pairs between trees:

```python
type Surface = tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.int32]]


def touching(first: Surface, second: Surface) -> tuple[tuple[int, int], ...]:
    left, right = (BVHTree.FromPolygons(positions, np.split(corners, starts[1:])) for positions, corners, starts in (first, second))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    return tuple(left.overlap(right))
```

Trees built from world positions overlap in world space, and `FromObject` trees stay in object space.
