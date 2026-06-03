# Bricks Catalogue And Layout Boundary

## Status

The `Bricks/` folder is a **brick-owned catalogue and layout foundation under source-truth refinement**. It owns brick material facts plus pure scalar layout data needed to assemble those facts into downstream document geometry.

This folder remains host-free. It does not draw, bake, instance, project, mutate documents, or create native geometry.

Current foundation:

- `Brick.cs` is the single catalogue source for units, bonds, shapes, joints, policies, and source basis.
- `Layout.cs` is the single scalar layout source for straight and curved brick runs.
- `BrickDesignation` attaches each `Brick` as `Unit` and exposes delegate properties such as `.Specified`, `.Region`, `.Coursing`, `.Coring`, `.Type`, and `.Source`.
- `BondName` carries `BondKind`, `ClosureRule`, optional `AspectRatio`, closure fraction, and optional static course seeds. Template bonds return `Some` from `Course(index)`; generated bonds return `None`.
- `BrickAssembly.Layout(BrickRun)` consumes template bonds and emits `BrickPlacement` rows in millimetres. Generated bonds fail explicitly until a real typed generator algebra exists.
- `BrickPath.Line` and `BrickPath.Arc` share one station-space layout algorithm. Arc output adds path angle data; it does not create native curved geometry.
- `ArchRule` stays catalogue constraint data. Arch layout is deferred because TN31 gives detailing constraints, not a complete scalar placement algorithm.

## Source Basis

Primary references for this foundation:

- [BIA Technical Notes](https://www.gobrick.com/resources/technical-notes)
- [BIA TN10](https://www.gobrick.com/media/file/10-dimensioning-and-estimating-brick-masonry.pdf)
- [BIA TN9A](https://www.gobrick.com/media/file/9a-specifications-for-and-classification-of-brick.pdf)
- [BIA TN30](https://www.gobrick.com/media/file/30-bonds-and-patterns-in-brickwork.pdf)
- [BIA TN31](https://www.gobrick.com/media/file/31-brick-masonry-arches.pdf)
- [BIA TN18A](https://www.gobrick.com/media/file/18a-tn18a.pdf)
- [Ibstock TIS-A4](https://assets.ctfassets.net/eta2vegx3yuv/OJ7h3HfyLcsH5vjPKB2Iy/c8187f3d295d5bd08f0f83098b15b98f/TIS-A4-BRICKWORK-BONDS.pdf)
- [BS EN 771-1 context](https://www.wienerberger.co.uk/content/dam/wienerberger/united-kingdom/marketing/documents-magazines/technical/brick-technical-guidance-sheets/UK_MKT_DOC_Tech%20Guidance%20Sheet%20Dimensions%20and%20Tolerances.pdf)
- [Thinktecture Runtime Extensions](https://github.com/pawelgerr/Thinktecture.Runtime.Extensions)

## Layout Rail

`BrickAssembly.Layout` is the one public layout entrypoint. It accepts `BrickRun` and returns `Fin<BrickAssembly>`.

`BrickRun` carries:

- `BrickDesignation Unit`
- `BondName Bond`
- `BrickPath Path`
- `double HeightMm`
- `JointProfile Joint`
- optional head and bed joint overrides

`BrickAssembly` returns the resolved path length, course count, joint values, and `Seq<BrickPlacement>`.

`BrickPlacement` is scalar output only: course, sequence, station, elevation, unit run/rise, path angle, orientation, and cut. V1 emits full untrimmed units and sets cut to `Cut.None`.

## Extension Discipline

Extend layout by normalizing every new concern into station-space before placement emission. Keep `BrickAssembly.Layout(BrickRun)` as the single public rail.

Add new layout concerns in this order:

1. Extend the existing input rail with a typed union/member only when implementation lands.
2. Normalize the concern into station/elevation intervals, path metrics, or station modifiers.
3. Feed normalized values through the existing course fold and sequence fold.
4. Emit `BrickPlacement` rows from one placement pass.
5. Extend `BrickFault` for failures instead of adding a second layout error family.

Concern-specific route:

- Openings become station/elevation interruptions plus edge-cut requests.
- Corners become endpoint conditions, turn metadata, and closure modifiers.
- Arches become source-backed profile constraints plus a station-normalized path rule.
- Generated bonds become typed generator algebra owned by `BondName`.
- Warnings become assembly diagnostics, separate from hard layout failure.

Refactor before adding a new body of code:

- Move path metrics onto `BrickPath` when a third path shape lands.
- Collapse repeated placement filters into one constraint union.
- Collapse repeated station transforms into one modifier union.
- Extend `BrickPlacement` once when consumers need a new scalar.
- Split `Layout.cs` only after one file stops expressing one coherent rail.

## Deferred Work

Deferred brick-owned work:

- opening subtraction and opening-edge cuts
- closure solving beyond existing bond offsets
- typed generated-bond interpreters
- arch placement
- pier layout
- layout warnings that distinguish source-backed rules from heuristics

Deferred downstream work:

- document placement and native object creation
- block or instance definition reuse
- component surfaces
- projection, section, and drawing output

## Consumer Example

```csharp
using Rasm.Materials.Bricks;

BrickRun run = new(
    Unit: BrickDesignation.UsModular,
    Bond: BondName.Running,
    Path: new BrickPath.Line(LengthMm: 2400.0),
    HeightMm: 1200.0,
    Joint: JointProfile.Concave,
    HeadJointMm: None,
    BedJointMm: None);

Fin<BrickAssembly> assembly = BrickAssembly.Layout(run: run);
```

## Verification

Use static validation for this slice:

```bash
uv run python -m tools.quality static fix
uv run python -m tools.quality static build
```

Do not run runtime host checks for this pure materials folder.
