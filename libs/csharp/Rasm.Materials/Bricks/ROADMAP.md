# Bricks Catalogue Boundary

## Status

The `Bricks/` catalogue is a **catalogue foundation under source-truth refinement**. It owns brick material facts only: unit dimensions, source basis, bond names and static course seeds, joint profiles, coring archetypes, special-shape vocabulary, arch rules, and regional policy defaults.

Geometry, layout, placement, block instancing, projections, Grasshopper components, and Rhino drawing behavior stay outside `Rasm.Materials`.

Current foundation:

- `Brick.cs` is the single catalogue source for this folder.
- `BrickDesignation` attaches each `Brick` as `Unit` and exposes delegate properties such as `.Specified`, `.Region`, `.Coursing`, `.Coring`, `.Type`, and `.Source`.
- `DimensionBasis` records whether a listed dimension is exact, an interval, or a midpoint derived from a published range. Queen and King units use midpoint basis instead of pretending their midpoint values are primary.
- `BondName` carries `BondKind`, `ClosureRule`, optional `AspectRatio`, closure fraction, and optional static course seeds. Template bonds return `Some` from `Course(index)`; generated bonds return `None` unless a static seed is truthful.
- `BrickType` is a classification vocabulary for ASTM FBX/FBS/FBA/HBX/HBS/HBA. It does not flatten ASTM dimensional tolerance, warpage, or chippage tables into false single values.
- `BrickSource` is a typed union: BIA TN10 Table 1/Table 2, BS EN 771-1 context, current DIN/EN 771-1 context, AS/NZS 4455.1 requirements context, and IS 1077.
- Coring notes describe typical archetypes. BIA TN9A backs void-area classes, not a specific hole count.
- Regional policy is attached to `BrickRegion`. U.S. values remain a reference baseline; tie spacing is labelled from TMS 402/602-22 prescriptive summaries and should be rechecked when official edition tables are locally available.
- Arch rules are profile-specific. Jack arches use `ArchRule.Jack`; other arch profiles currently use `ArchRule.General`.
- Ergonomic projections are `BondName.Fits(Brick)`, `BondName.Course(int)`, `ExpansionJointSpacing.Spacing(bool)`, and `BrickRegion.Units()`.

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

## Bond Coverage

The catalogue keeps 34 bond names where each name has a source, archetype, or capability note. The important distinction is now `BondKind`:

- `Template` means the course template is meaningful catalogue data and `Course(index)` returns `Some<CourseTemplate>`.
- `Generated` means the name needs runtime composition, masking, rotation, randomness, or a geometry-specific generator; `Course(index)` returns `None`.

Template bonds include Running, ThirdRunning, Common, English, Flemish, Stack, EnglishCross, FlemishCross, Sussex, Scotch, Header, Monk, RatTrap, Soldier, SingleFlemish, DoubleFlemish, DoubleStretcherGardenWall, FlemishStretcher, MixedGardenWall, DellaRobbiaWeave, AppareilALaFrancaise, DutchMulti, and Staffel.

Generated bonds currently include RunningRotated, Herringbone45, BasketWeave, Quetta, Diaper, FlemishDiagonal, FlemishSpiral, Pinwheel, TudorCrossHatch, Wild, and OpusReticulatum.

Names intentionally not promoted to static templates yet: Dearnes has BDA/Ibstock authority but needs a precise encoded template shape; Gothic is distinct from Monk but not encoded here until the static sequence is source-backed; Silverlock maps to RatTrap; Cross maps to EnglishCross; Yorkshire, HeaderGardenWall, Mughal/Persian khishti/banna'i, and Sint-Andriesverband remain outside the foundation until their source and data shape justify inclusion.

## Future Ownership

### `libs/csharp/Rasm.Masonry/`

`Rasm.Masonry` should turn `BrickDesignation x BondName x WallSpec` into layout and assembly records. It owns course walking, closure derivation, opening conflicts, curved-wall tolerance, arch voussoirs, and generated bond interpreters.

Likely material-to-layout types:

- `PlacedBrick(BrickDesignation Unit, Plane Frame, Orientation Orientation, Cut Cut, int SequenceId)`
- `WallSpec` union for planar, curved, arch, and pier cases
- `OpeningSpec` for wall openings
- `LayoutError` union for incompatibility and assembly failures

Single expected rail: `Masonry.Generate(WallSpec spec) -> Fin<MasonryAssembly>`.

Course walking should consume `bond.Course(index)` as an `Option<CourseTemplate>`. Template bonds can be walked directly; generated bonds must dispatch to the matching generator.

Arch generation should use the attached `ArchProfile.Rule`. Jack arches use the jack depth rule; other profiles use the general rule until more profile-specific source tables are encoded. Voussoir count, wedge angle, and cut geometry belong here, not in Materials.

Curved-wall validation belongs here too. A useful starting heuristic is chord-arc sagitta `s ~= L^2 / (8R)` as a driver for head-joint taper and the rectangular-to-voussoir transition.

### `libs/csharp/Rasm.Rhino/Blocks/`

`Rasm.Rhino.Blocks` should consume `Rasm.Masonry` output for live/archive block placement. It should not become a dependency of core masonry logic and should stay universal: repetitive geometry for brick, steel, timber, stone, glass, and future materials.

Expected primitive: place one shared `GeometryBase` definition at many `Transform` placements through Rhino instance definitions, for both `RhinoDoc` and `File3dm`.

### `libs/csharp/Rasm/Analysis/`

Projection and section algebra belongs in the shared geometry kernel, not in Materials. Elevation, plan, and section extraction should be universal over geometry.

### `libs/csharp/Rasm.Grasshopper/Components/`

Grasshopper components consume the masonry/block/projection rails. Keep the current flat component layout; do not introduce a `Components/Masonry/` subfolder unless the whole component taxonomy changes.

## Consumer Example

```csharp
using static LanguageExt.Prelude;
using Rasm.Materials.Bricks;

BrickRegion region = BrickDesignation.UsModular.Region;
Dim3 specified = BrickDesignation.UsModular.Specified;
double widthMm = specified.WidthMm;
double mortarMm = region.StandardJointThicknessMm;
double courseHeightMm = BrickDesignation.UsModular.Coursing.CourseHeightMm;
Coring coring = BrickDesignation.UsModular.Coring;
double voidFraction = coring.VoidFraction;

Option<CourseTemplate> row5 = BondName.English.Course(index: 5);
ClosureRule corner = BondName.English.Closure;

bool fits = BondName.Common.Fits(brick: BrickDesignation.UsModular.Unit);

RegionalMasonryPolicy policy = BrickDesignation.UsModular.Region.Policy;
double expansionSpacingMm = policy.Expansion.Spacing(hasOpenings: true);

ArchRule jackRule = ArchProfile.Jack.Rule;
double minimumJackDepthMm = jackRule.MinimumDepthMm(spanFeet: 4.0);

Seq<Coring> hollowCorings = toSeq(Coring.Items.Where(c => c.Classification == CoringClass.Hollow));
Seq<BrickDesignation> usBricks = BrickRegion.Us.Units();
FrozenSet<SpecialShape> specialShapes = SpecialShape.Catalog;

Fin<Dim3> userDim =
    Dim3.TryCreate(widthMm: userWidth, heightMm: userHeight, lengthMm: userLength, out Dim3 dim, out BrickFault? fault)
        ? Fin.Succ(value: dim)
        : Fin.Fail<Dim3>(error: fault!);
```

## Out Of Scope

Do not add to `Rasm.Materials.Bricks`:

- Geometry generation such as `Box.ToBrep` or boolean cutting
- Layout transducers, closure derivation, generated bond interpreters, or voussoir placement
- Rhino blocks, `Plane`, `Transform`, `GeometryBase`, `RhinoDoc`, or `File3dm`
- Grasshopper2 components, pears, goos, or baking logic
- `PlacedBrick`, `WallSpec`, `OpeningSpec`, `MasonryAssembly`, or layout errors
- Lintel structural design
- Manufacturer SKU, pricing, or sourcing data
- Mortar mix chemistry
- Thermal, acoustic, fire, or building-physics assembly classification

## Verification

Use static validation for this slice:

```bash
rg -n "<stale public-name pattern from the implementation plan>" libs/csharp/Rasm.Materials
bash scripts/check-cs.sh check
```

Do not run Rhino runtime checks for this catalogue-only folder.
