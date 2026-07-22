# [RASM_BIM_API_SUBTREE]

`subtree` owns the 3D-Tiles implicit-tiling `.subtree` availability bitstream — the Morton-ordered tile, content, and child-subtree bitstreams that tell a client which implicit nodes exist. It authors and reads tileset AVAILABILITY structure only, the interchange-rail complement to `SharpGLTF.Ext.3DTiles`, which owns per-tile glTF CONTENT. `SubtreeCreator` folds a `List<Tile>` (quadtree) or `List<Tile3D>` (octree) into a binary `.subtree` buffer, `ImplicitSubdivisionScheme` selecting the level-offset and Morton arithmetic.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `subtree`
- package: `subtree`
- license: MIT
- assembly: `subtree`
- namespace: `subtree`
- asset: net8.0 single TFM; the net10.0 consumer binds `lib/net8.0` — pure-managed AnyCPU, no `runtimes/` folder
- depends: transitive `Newtonsoft.Json`, the `SubtreeJson` JSON-chunk serializer
- rail: interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tile authoring nodes and scheme axis

| [INDEX] | [SYMBOL]                    | [CAPABILITY]                                                                       |
| :-----: | :-------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Tile`                      | quadtree authoring node; `Z` = Morton subdivision level, `X`/`Y` the in-level cell |
|  [02]   | `Tile3D`                    | octree authoring node; `Level` = subdivision level, `Z` a true third spatial axis  |
|  [03]   | `ImplicitSubdivisionScheme` | `Quadtree`/`Octree` level/offset/Morton discriminant (default `Quadtree`)          |

- [01]-[TILE]: ctor `(int z, int x, int y)` or `(…, bool available)`; `GetChildren` descends `Z+1`, `Parent` climbs `Z-1`; settable `Available`/`ContentUri`/`GeometricError`/`ZMin?`/`ZMax?`/`BoundingBox`/`Children`. `MortonIndex` and `SubtreeCreator` read `Z`, never the auxiliary `Lod`.
- [02]-[TILE3D]: ctor `(int level, int x, int y, int z)` — `Level` the subdivision level, `Z` a true third spatial axis; `Parent` halves `X`/`Y`/`Z`; `Available` settable; `HasChild`/`Equals`/`GetHashCode` topology.

[PUBLIC_TYPE_SCOPE]: subtree binary record graph

| [INDEX] | [SYMBOL]                   | [CAPABILITY]                                                                                            |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `Subtree`                  | `record`: availability `BitArray`s + `*Constant`, `SubtreeBinary` bytes, `SubtreeJson`, `SubtreeHeader` |
|  [02]   | `SubtreeHeader`            | 24-byte binary header (magic/version/json-byte-length/binary-byte-length)                               |
|  [03]   | `SubtreeJson`              | JSON-chunk model (buffers/bufferViews + availability descriptors)                                       |
|  [04]   | `Buffer` / `Bufferview`    | `record` buffer + buffer-view descriptors inside `SubtreeJson`                                          |
|  [05]   | `Tileavailability`         | tile availability descriptor (bitstream byte-offset or constant)                                        |
|  [06]   | `Contentavailability`      | content availability descriptor (byte-offset or constant)                                               |
|  [07]   | `Childsubtreeavailability` | child-subtree availability descriptor (byte-offset or constant)                                         |

[PUBLIC_TYPE_SCOPE]: bit-array and availability-level model

| [INDEX] | [SYMBOL]                                      | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `BitArray2D` / `BitArray3D`                   | per-level dense availability grids (quadtree 2D / octree 3D)      |
|  [02]   | `AvailabilityLevel` / `AvailabilityLevel3D`   | one subdivision level's availability row                          |
|  [03]   | `AvailabilityLevels` / `AvailabilityLevels3D` | `List<AvailabilityLevel[3D]>` — the full level stack of a subtree |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `SubtreeCreator` folds a quadtree `List<Tile>`, `SubtreeCreator3D` an octree `List<Tile3D>`, each into one or many `.subtree` binaries across multi-subtree subdivision (all static)

| [INDEX] | [SURFACE]                                                                          | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `SubtreeCreator.GenerateSubtreefile(List<Tile>) -> byte[]`                         | one `.subtree` binary from a quadtree tile list |
|  [02]   | `SubtreeCreator.GenerateSubtreefiles(List<Tile>) -> Dictionary<Tile,byte[]>`       | one binary per overflowing root tile            |
|  [03]   | `SubtreeCreator.GetSubtreeTiles(List<Tile>, Tile) -> List<Tile>`                   | tiles of one subtree rooted at `tile`           |
|  [04]   | `SubtreeCreator.GetRelativeTile(Tile, Tile) -> Tile`                               | re-bases a tile coord to a subtree-local frame  |
|  [05]   | `SubtreeCreator3D.GenerateSubtreefile(List<Tile3D>) -> byte[]`                     | authors one octree `.subtree` binary            |
|  [06]   | `SubtreeCreator3D.GenerateSubtreefiles(List<Tile3D>) -> Dictionary<Tile3D,byte[]>` | multi-subtree octree authoring                  |

[ENTRYPOINT_SCOPE]: read, serialize, and Morton arithmetic (all static)

| [INDEX] | [SURFACE]                                                                              | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `SubtreeReader.ReadSubtree(Stream)` / `(BinaryReader) -> Subtree`                      | decode `.subtree` to record graph    |
|  [02]   | `SubtreeWriter.ToBytes(Subtree) -> byte[]`                                             | `Subtree` record to binary container |
|  [03]   | `SubtreeWriter.ToBytes(string, string?, string?) -> byte[]`                            | from raw availability bit-strings    |
|  [04]   | `SubtreeWriter.ToSubtreeBinary(Subtree) -> (byte[], SubtreeJson)`                      | binary + parsed JSON header          |
|  [05]   | `MortonOrder.Encode3D(ulong,ulong,ulong)` / `Encode2D(uint,uint) -> uint`              | z-order encode (octree/quadtree)     |
|  [06]   | `MortonOrder.Decode3D` / `Decode2D(uint) -> (uint x, y[, z])`                          | inverse z-order decode               |
|  [07]   | `MortonIndex.GetMortonIndices[3D](List<Tile[3D]>) -> (string tile, content)`           | bit-strings from a tile list         |
|  [08]   | `MortonIndex.GetMortonIndexAsBytes[3D](List<Tile[3D]>) -> (byte[] tile, content)`      | packed-byte availability buffers     |
|  [09]   | `Availability.GetLevelAvailability(string, int, ImplicitSubdivisionScheme) -> string`  | slice one subdivision level          |
|  [10]   | `Level.GetLevel` / `LevelOffset.GetLevelOffset(int, ImplicitSubdivisionScheme) -> int` | length-to-level, level-to-offset     |
|  [11]   | `BitstreamReader.Read(byte[], int, int) -> BitArray`                                   | bitstream slice from the buffer      |
|  [12]   | `BufferPadding.AddPadding` / `AddBinaryPadding(byte[], int) -> byte[]`                 | 8-byte padding (3D-Tiles layout)     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every availability op folds through the Morton index `ImplicitSubdivisionScheme` selects; a tile is available exactly when its Morton bit is set in the `.subtree` bitstream.
- `Subtree` is the record root; subdivision scheme, level count, and available-tile count are the receipt facts the EXPORT fold records.

[STACKING]:
- `SharpGLTF.Ext.3DTiles`(`.api/api-sharpgltf-3dtiles`): SharpGLTF owns per-tile glTF CONTENT and `EXT_structural_metadata`, `subtree` the tileset AVAILABILITY bitstream; both key off the shared `MortonOrder` index, so a tile is "available with content" exactly when both bitstreams set the same Morton bit.
- EXPORT authoring fold: selects `SubtreeCreator`/`SubtreeCreator3D` and its `ImplicitSubdivisionScheme` once from the subdivision kind, emits each `GenerateSubtreefiles` overflow binary keyed by its root tile with `GetSubtreeTiles`/`GetRelativeTile` re-basing coordinates so child-subtree pointers resolve, and asserts receipts via `SubtreeReader.ReadSubtree` round-trip and `BitstreamReader.Read` level slices.

[LOCAL_ADMISSION]:
- `Rasm.Bim` admits `subtree` for `.subtree` availability authoring and read only; glTF tile content, tileset.json hierarchy, and bounding-volume geometry stay `SharpGLTF`/EXPORT concerns.

[RAIL_LAW]:
- Package: `subtree`
- Owns: 3D-Tiles implicit-tiling `.subtree` availability-bitstream authoring and read
- Accept: the EXPORT per-tile availability leg complementing the glTF content leg
- Reject: glTF tile content, tileset.json hierarchy, bounding-volume geometry
