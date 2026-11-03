# [MATHUTILS]

## [01]-[VALUES]

`mathutils` values store `float32` (`Vector((1 / 3, 0, 0)).x != 1 / 3`), and values read from RNA write through to their owner:
- Writes through a nested read reach the owner (`obj.matrix_world.translation.x = 7` moves the object)
- `np.asarray(<value>)` reads a read-only `float32` array, a live view of a value with no `owner` and a snapshot of one with an `owner`
- `value.copy().freeze()` is the hashable form for dict and set keys, `freeze()` on a value with an `owner` raising `TypeError`
- Zero-vector `angle(other, <fallback>)` and singular-matrix `inverted(<fallback>)` return `<fallback>`, `inverted(None)` raising `ValueError`
- `normalized()` returns a zero vector and `rotation_difference` an identity rotation on degenerate input

## [02]-[TRANSFORMS]

`Matrix @ Vector` with a 4x4 matrix moves 3D points, directions taking `to_3x3()` and normals `inverted_safe().transposed().to_3x3()` then `normalized()`. Stored `MATRIX` buffers fit numpy's row-vector product untransposed. World corners of many objects read `matrix_world` and `bound_box` once each through `column`:

```python
def corners(objects: Elements[bpy.types.Object], /) -> Buffer:
    worlds, boxes = (column(objects, bpy.types.Object, member) for member in ("matrix_world", "bound_box"))
    return boxes @ worlds[:, :3, :3] + worlds[:, None, 3, :3]
```

`corners(scene.objects)` returns `(objects, 8, 3)` world corners in `scene.objects` order, objects without geometry holding eight zero corners at their origin.

## [03]-[ROTATIONS]

Rotations keep their branch and sign through the members that state them:
- `Euler.make_compatible(<prior>)` and `to_euler(<order>, <prior>)` keep successive keys on one branch, a `z` of `-3.1` after `3.1` reading `3.18`
- `Quaternion.make_compatible(<prior>)` flips a quaternion into its prior's hemisphere
- `decompose()` and `to_scale()` on a mirrored matrix negate every scale axis under a compensating rotation
- `is_negative` flags a mirrored matrix, and `LocRotScale(*matrix.decompose())` rebuilds it

## [04]-[SPATIAL_QUERIES]

Spatial questions take the member that answers them:

| [INDEX] | [QUESTION]                        | [MEMBER]                      |
| :-----: | :-------------------------------- | :---------------------------- |
|  [01]   | N-gon outline into triangles      | `geometry.tessellate_polygon` |
|  [02]   | Constrained 2D triangulation      | `geometry.delaunay_2d_cdt`    |
|  [03]   | Rectangles packed into a square   | `geometry.box_pack_2d`        |
|  [04]   | Barycentric weights over an n-gon | `interpolate.poly_3d_calc`    |

- `KDTree(<size>)` fixes its capacity, an insert past it and a `find` before `balance()` raising `RuntimeError`, and `insert` takes a 3D numpy row
- `KDTree.find(co, filter=<index predicate>)` returns the nearest point whose index passes, one balanced tree serving every subset
- Misses return `None` (`geometry.intersect_*`) or a `None` tuple (`ray_cast`, `find_nearest` in range, `find` on an empty tree)
- Misses read through `Option.of_optional` or a `case (Vector() as location, Vector() as normal, int() as face, float() as distance):` arm
- `BVHTree.FromObject(obj, depsgraph)` builds from the evaluated mesh in object space and raises `ValueError` on an object without a mesh
- `mathutils.noise` shares no basis with the Noise Texture node
- Values matching Noise Texture come from the node itself, evaluated in a geometry node tree and read back as an attribute

`BVHTree.FromPolygons` takes positions and one index array per face. `overlap` returns face index pairs between trees built from `surface` results:

```python
def touching(first: Surface, second: Surface, /) -> tuple[tuple[int, int], ...]:
    left, right = (BVHTree.FromPolygons(positions, np.split(corners, starts[1:].tolist())) for positions, corners, starts in (first, second))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    return tuple(left.overlap(right))
```
