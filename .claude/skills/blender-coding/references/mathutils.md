# [MATHUTILS]

## [01]-[VALUES]

`mathutils` values store `float32` (`Vector((1 / 3, 0, 0)).x != 1 / 3`), and values read from RNA write through to their owner:
- Writes through a nested read reach the owner (`obj.matrix_world.translation.x = 7` moves the object)
- `np.asarray(<value>)` reads a read-only `float32` array, a live view of a detached value and a snapshot of an owned one
- `value.copy().freeze()` is the hashable form for dict and set keys, `freeze()` on an owned value raising `TypeError`
- `angle(other, <fallback>)` on a zero vector and `inverted(<fallback>)` on a singular matrix return the fallback, `inverted(None)` raising `ValueError`
- `normalized()` returns a zero vector and `rotation_difference` an identity rotation on degenerate input

## [02]-[TRANSFORMS]

`Matrix @ Vector` takes a 3D vector through a 4x4 matrix as a point, directions through `matrix.to_3x3()`, and normals through `matrix.inverted_safe().transposed().to_3x3()` then `normalized()`. Stored `MATRIX` buffers are the operand numpy's row-vector product takes with no transpose, and world corners of many objects read `matrix_world` and `bound_box` once each through `column`:

```python
def corners(objects: Elements[bpy.types.Object], /) -> Buffer:
    worlds, boxes = (column(objects, bpy.types.Object, member) for member in ("matrix_world", "bound_box"))
    return boxes @ worlds[:, :3, :3] + worlds[:, None, 3, :3]
```

`corners(scene.objects)` returns `(objects, 8, 3)` world corners in `scene.objects` order, an object without geometry holding eight zero corners that land on its origin.

## [03]-[ROTATIONS]

Rotations keep their branch and sign through the members that state them:
- `Euler.make_compatible(<prior>)` and `to_euler(<order>, <prior>)` keep successive keyed rotations on one branch, a `z` of `-3.1` after `3.1` reading `3.18`
- `Quaternion.make_compatible(<prior>)` flips a quaternion into its prior's hemisphere
- `decompose()` and `to_scale()` on a mirrored matrix negate every scale component under a compensating rotation, `is_negative` naming the mirror and `LocRotScale(*matrix.decompose())` rebuilding the matrix

## [04]-[SPATIAL_QUERIES]

Spatial questions take the member that answers them:

| [INDEX] | [QUESTION]                        | [MEMBER]                      |
| :-----: | :-------------------------------- | :---------------------------- |
|  [01]   | N-gon outline into triangles      | `geometry.tessellate_polygon` |
|  [02]   | Constrained 2D triangulation      | `geometry.delaunay_2d_cdt`    |
|  [03]   | Rectangles packed into a square   | `geometry.box_pack_2d`        |
|  [04]   | Barycentric weights over an n-gon | `interpolate.poly_3d_calc`    |

Queries follow host rules:
- `KDTree(<size>)` fixes its capacity, an insert past it and a `find` before `balance()` raising `RuntimeError`, and `insert` takes a 3D numpy row
- `KDTree.find(co, filter=<index predicate>)` returns the nearest point whose index passes, one balanced tree serving every subset
- Misses return `None` (`geometry.intersect_*`) or a tuple of `None` (`ray_cast`, `find_nearest` within a distance, `find` on an empty tree), read through `Option.of_optional` or a `case (Vector() as location, Vector() as normal, int() as face, float() as distance):` arm
- `BVHTree.FromObject(obj, depsgraph)` builds from the evaluated mesh in object space and raises `ValueError` on an object without a mesh
- `mathutils.noise` shares no basis with the Noise Texture node, and values matching the node come from the node itself, evaluated in a geometry node tree and read back as an attribute

`BVHTree.FromPolygons` takes a positions array and one index array per face, and `overlap` returns face index pairs between trees built from each `surface` result of world positions, corner vertices, and face starts:

```python
def touching(first: tuple[Buffer, Buffer, Buffer], second: tuple[Buffer, Buffer, Buffer], /) -> tuple[tuple[int, int], ...]:
    left, right = (BVHTree.FromPolygons(positions, np.split(corners, starts[1:].tolist())) for positions, corners, starts in (first, second))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    return tuple(left.overlap(right))
```
