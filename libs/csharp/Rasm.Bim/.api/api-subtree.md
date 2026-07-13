# [RASM_BIM_API_SUBTREE]

`subtree` is the 3D-Tiles implicit-tiling `.subtree` binary
availability-bitstream codec backing the `Exchange/export#EXPORT` per-tile
authoring leg, retiring the hand-rolled implicit-tiling bitstream. It is the
tileset-side complement `SharpGLTF.Ext.3DTiles` cannot reach: that package owns
the glTF tile CONTENT (per-tile `EXT_structural_metadata`), this package owns the
tileset AVAILABILITY structure — the Morton-ordered tile/content/child-subtree
bitstreams that tell a 3D-Tiles client which implicit nodes exist. The unifying
primitive is a `List<Tile>` (quadtree) or `List<Tile3D>` (octree) authored
directly into a binary `.subtree` buffer by `SubtreeCreator`, with the
`ImplicitSubdivisionScheme` discriminant selecting the level-offset and Morton
arithmetic.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `subtree`
- package: `subtree`
- license: MIT (GitHub `LICENSE`; the nuspec ships no `<license>`/`licenseUrl` expression, so the OSS-license fact is the repository file, not package metadata)
- assembly: `subtree`
- namespace: `subtree`
- asset: net8.0 single TFM; the net10.0 consumer binds `lib/net8.0` (pure-managed AnyCPU, no `runtimes/` folder)
- asset: managed transitive `Newtonsoft.Json (the `SubtreeJson` header model), floor-pinned centrally
- rail: interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tile authoring nodes and scheme axis
- package: `subtree`
- namespace: `subtree`
- rail: interchange

| [INDEX] | [SYMBOL]                    | [CAPABILITY]                                                                           |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `Tile`                      | quadtree authoring node; `Z` = the Morton subdivision level, `X`/`Y` the in-level cell |
|  [02]   | `Tile3D`                    | octree authoring node; `Level` = subdivision level, `Z` a true third spatial axis      |
|  [03]   | `ImplicitSubdivisionScheme` | `Quadtree`/`Octree` — the level/offset/Morton discriminant (default `Quadtree`)        |

- [01]-[TILE]: ctor `(int z, int x, int y[, bool available])`; `GetChildren` emits `Z+1`, `Parent` climbs `Z-1`; settable `Available`, `ContentUri`, `GeometricError`, `ZMin?`/`ZMax?`, `BoundingBox`, nested `Children`; `Lod` is auxiliary — the `MortonIndex`/`SubtreeCreator` author reads `Z`, never `Lod`.
- [02]-[TILE3D]: ctor `(int level, int x, int y, int z)` where `Level` is the subdivision level and `Z` a true third spatial axis (`Parent` halves `X`/`Y`/`Z`); `Available` settable; `Parent()` / `HasChild(Tile3D)` / `Equals` / `GetHashCode` topology.

[PUBLIC_TYPE_SCOPE]: subtree binary record graph
- package: `subtree`
- namespace: `subtree`
- rail: interchange

| [INDEX] | [SYMBOL]                   | [CAPABILITY]                                                                                            |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `Subtree`                  | `record`: availability `BitArray`s + `*Constant`, `SubtreeBinary` bytes, `SubtreeJson`, `SubtreeHeader` |
|  [02]   | `SubtreeHeader`            | the 24-byte binary header (magic/version/json-byte-length/binary-byte-length)                           |
|  [03]   | `SubtreeJson`              | the JSON-chunk model (buffers/bufferViews + availability descriptors)                                   |
|  [04]   | `Buffer` / `Bufferview`    | `record` buffer + buffer-view descriptors inside `SubtreeJson`                                          |
|  [05]   | `Tileavailability`         | tile availability descriptor (bitstream byte-offset or constant)                                        |
|  [06]   | `Contentavailability`      | content availability descriptor (byte-offset or constant)                                               |
|  [07]   | `Childsubtreeavailability` | child-subtree availability descriptor (byte-offset or constant)                                         |

[PUBLIC_TYPE_SCOPE]: bit-array and availability-level model
- package: `subtree`
- namespace: `subtree`
- rail: interchange

| [INDEX] | [SYMBOL]                                      | [RAIL]      | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------------- | :---------- | :---------------------------------------------------------------- |
|  [01]   | `BitArray2D` / `BitArray3D`                   | interchange | per-level dense availability grids (quadtree 2D / octree 3D)      |
|  [02]   | `AvailabilityLevel` / `AvailabilityLevel3D`   | interchange | one subdivision level's availability row                          |
|  [03]   | `AvailabilityLevels` / `AvailabilityLevels3D` | interchange | `List<AvailabilityLevel[3D]>` — the full level stack of a subtree |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SubtreeCreator — quadtree/octree authoring
- package: `subtree`
- namespace: `subtree`
- rail: interchange

`SubtreeCreator` (quadtree) and `SubtreeCreator3D` (octree) are the primary
authoring entrypoints; both fold a flat tile list into one or many `.subtree`
binary buffers, handling multi-subtree subdivision.

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `SubtreeCreator.GenerateSubtreefile(List<Tile>)` → `byte[]`                          | one `.subtree` binary from a quadtree tile list |
|  [02]   | `SubtreeCreator.GenerateSubtreefiles(List<Tile>)` → `Dictionary<Tile, byte[]>`       | one binary per overflowing root tile            |
|  [03]   | `SubtreeCreator.GetSubtreeTiles(List<Tile>, Tile)` → `List<Tile>`                    | tiles of one subtree rooted at `tile`           |
|  [04]   | `SubtreeCreator.GetRelativeTile(Tile from, Tile to)` → `Tile`                        | re-bases a tile coord to a subtree-local frame  |
|  [05]   | `SubtreeCreator3D.GenerateSubtreefile(List<Tile3D>)` → `byte[]`                      | authors one octree `.subtree` binary            |
|  [06]   | `SubtreeCreator3D.GenerateSubtreefiles(List<Tile3D>)` → `Dictionary<Tile3D, byte[]>` | multi-subtree octree authoring                  |

[ENTRYPOINT_SCOPE]: read, serialize, and Morton arithmetic
- package: `subtree`
- namespace: `subtree`
- rail: interchange

| [INDEX] | [SURFACE]                                                                               | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `SubtreeReader.ReadSubtree(Stream)` / `(BinaryReader)` → `Subtree`                      | decode `.subtree` → record graph    |
|  [02]   | `SubtreeWriter.ToBytes(Subtree)` → `byte[]`                                             | `Subtree` record → binary container |
|  [03]   | `SubtreeWriter.ToBytes(string tile, string? content, string? subtree)` → `byte[]`       | from raw availability bit-strings   |
|  [04]   | `SubtreeWriter.ToSubtreeBinary(Subtree)` → `(byte[], SubtreeJson)`                      | binary + parsed JSON header         |
|  [05]   | `MortonOrder.Encode3D(ulong x, y, z)` / `Encode2D(uint x, y)` → morton index            | z-order encode (octree/quadtree)    |
|  [06]   | `MortonOrder.Decode3D` / `Decode2D(uint mortonIndex)` → `(uint x, y[, z])`              | inverse z-order decode              |
|  [07]   | `MortonIndex.GetMortonIndices[3D](List<Tile[3D]>)` → `(string tile, content)`           | bit-strings from a tile list        |
|  [08]   | `MortonIndex.GetMortonIndexAsBytes[3D](List<Tile[3D]>)` → `(byte[] tile, content)`      | packed-byte availability buffers    |
|  [09]   | `Availability.GetLevelAvailability(string, int, ImplicitSubdivisionScheme)` → `string`  | slice one subdivision level         |
|  [10]   | `Level.GetLevel` / `LevelOffset.GetLevelOffset(int, ImplicitSubdivisionScheme)` → `int` | length↔level, level→bit-offset      |
|  [11]   | `BitstreamReader.Read(byte[] subtreeBinary, int offset, int length)` → `BitArray`       | bitstream slice from the buffer     |
|  [12]   | `BufferPadding.AddPadding` / `AddBinaryPadding(byte[], int offset = 0)` → `byte[]`      | 8-byte padding (3D-Tiles layout)    |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `subtree`
- authoring root: `SubtreeCreator` (quadtree) / `SubtreeCreator3D` (octree)
- record root: `Subtree` (availability `BitArray`s + binary + JSON header)
- scheme axis: `ImplicitSubdivisionScheme` (`Quadtree` / `Octree`)
- receipt root: subdivision scheme, level count, and available-tile count

[CODEC_COMPOSE]:
- content/availability split: `SharpGLTF.Ext.3DTiles` authors the per-tile glTF CONTENT and `EXT_structural_metadata`; `subtree` authors the tileset AVAILABILITY bitstream that indexes which implicit nodes exist — the EXPORT design composes both into one tileset, where the `TileMetadata` author (SharpGLTF) and the `.subtree` author (this package) meet at the shared Morton tile index, never duplicating the availability logic the hand-rolled bitstream once carried.
- scheme-polymorphic authoring: the EXPORT fold selects `SubtreeCreator` vs. `SubtreeCreator3D` and the matching `ImplicitSubdivisionScheme` from the tileset's subdivision kind once; all downstream level/offset/Morton math takes the same discriminant, so a 2D site quadtree and a 3D volumetric octree share one authoring rail.
- multi-subtree overflow: `GenerateSubtreefiles` returns a `Dictionary<Tile, byte[]>` when a tileset exceeds one subtree's level budget — the design emits each binary as an `ExportArtifact` keyed by its root tile, and `GetSubtreeTiles`/`GetRelativeTile` re-base coordinates into subtree-local frames so the child-subtree availability pointers resolve.
- Morton as the join: `MortonOrder.Encode3D`/`Encode2D` is the z-order index that orders tile and content availability identically; the SharpGLTF tile content and the `subtree` availability bit both key off this index, so a tile is "available with content" exactly when both bitstreams set the same Morton position.
- round-trip verification: `SubtreeReader.ReadSubtree` decodes an authored binary back to the `Subtree` record for an EXPORT receipt assertion, and `BitstreamReader.Read` slices any availability buffer for a level-by-level diff.

[LOCAL_ADMISSION]:
- `subtree` authors and reads the `.subtree` availability binary only; it carries no glTF tile content, no tileset.json root, and no geometry.
- Per-tile content, bounding-volume geometry, and the tileset.json hierarchy are EXPORT/SharpGLTF concerns, never the bitstream codec's.
- Subdivision scheme, level count, and available-tile count are receipt facts the EXPORT fold records.

[RAIL_LAW]:
- Package: `subtree`
- Owns: 3D-Tiles implicit-tiling `.subtree` availability-bitstream authoring and read
- Accept: the EXPORT per-tile availability leg complementing the glTF content leg
- Reject: glTF tile content, tileset.json hierarchy, bounding-volume geometry
