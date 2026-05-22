# Architectural Blueprint: A Rhino/Grasshopper C# Plugin for Brick Masonry Generation

## TL;DR

- **Decision-ready conclusion**: Architect the plugin as a four-layer monorepo — a Rhino-agnostic functional `Masonry.Domain` core (immutable records, pure layout functions, `Result<T>` railways), a `Masonry.Geometry` translator, a `Masonry.Rhino` adapter, and a thin `Masonry.Grasshopper` shell — with brick units modelled as immutable data, **orientation as metadata (not subtype)**, **pattern as data + pure function**, and **cut/closure as a transformation, not a separate type**. This is the only structure that scales from running bond on a planar wall to a 20,000-brick robotically-rotated facade.
- **Domain rule that drives the architecture**: Per BIA Technical Note 10, every modular brick is specified by W × H × L in the stretcher orientation and its actual dimension is the nominal dimension minus one mortar joint (typically 3/8 in., per TMS 602 default). All other orientations (header, soldier, sailor, rowlock, shiner) are the **same brick rotated** — never a different unit. Cuts (half-bat, three-quarter bat, queen closer, king closer) are derived from a full brick; special shapes (bullnose, plinth, voussoir, squint) are distinct catalogued units. The data model must encode this trinity (full / cut / special-shape) cleanly.
- **Non-negotiable engineering choices**: (1) closures and bond compatibility belong in a Railway-Oriented validation pipeline returning `Result<WallSpec, MasonryError>` before any geometry is generated; (2) instance Brep generation must use Rhino 8's native block-instance components to avoid memory blow-up on facades exceeding ~5,000 units; (3) for curved walls, lay out in surface UV space and validate radius against the minimum/maximum tapered-joint thickness given by BIA TN 31 (1/8 in. min, 3/4 in. max).

---

## Key Findings (Summary)

- The US authority is BIA TN 10 (sizing/coursing), TN 30 (bonds), TN 31 (arches), TN 18A (expansion joints), TN 28 series (veneer ties); the binding standards are ASTM C216/C652/C62 (units), ASTM C270 (mortar), TMS 402/602 (structural/installation), IBC 2021 Ch. 21. The architecture must treat these documents as data sources, not as code-baked constants.
- The "different brick sizes in the same run" misconception dissolves: bonds use one unit, with orientation varying; only closures and special shapes are different units.
- Functional-core / imperative-shell (Bernhardt) and Railway-Oriented Programming (Wlaschin) are the right idioms; C# 13/14 records, sealed hierarchies, switch-expression patterns, primary constructors, required members, and collection expressions make a near-F# style ergonomic without leaving the C# ecosystem.
- Contemporary parametric facades (Gramazio Kohler's Gantenbein, 2006; Kitrvs, 2019; Sstudiomm's *Negative Precision*, 2016; Wallmakers' *Pirouette House*, 2020) demonstrate that orientation must be expressible as a continuous `Plane` rotation at the extension API — not a six-valued enum.

---

## Part 1 — Brick Domain Concepts

### 1. Brick Sizes (US/Imperial, ASTM/BIA)

The authoritative US source is **BIA Technical Note 10 — Dimensioning and Estimating Brick Masonry** (Feb 2009), which distinguishes three dimension sets:

- **Nominal**: specified dimension + intended mortar joint (whole numbers fitting a 4 in. or 8 in. module).
- **Specified**: anticipated manufactured dimension excluding mortar (used in purchase orders and structural design).
- **Actual**: as-manufactured; within tolerance of specified per ASTM C216 §X9 / C652.

Convention: **W × H × L in stretcher orientation**. A modular brick is specified as 3⅝″ × 2¼″ × 7⅝″ (92 × 57 × 194 mm).

**Modular brick sizes (BIA TN 10 Table 1, 3/8″ joint)**:

| Designation | Nominal W×H×L (in.) | Specified W×H×L (in.) | Vertical Coursing |
|---|---|---|---|
| Modular | 4 × 2⅔ × 8 | 3⅝ × 2¼ × 7⅝ | 3C = 8 in. |
| Engineer Modular | 4 × 3⅕ × 8 | 3⅝ × 2¹³⁄₁₆ × 7⅝ | 5C = 16 in. |
| Closure Modular | 4 × 4 × 8 | 3⅝ × 3⅝ × 7⅝ | 1C = 4 in. |
| Roman | 4 × 2 × 12 | 3⅝ × 1⅝ × 11⅝ | 2C = 4 in. |
| Norman | 4 × 2⅔ × 12 | 3⅝ × 2¼ × 11⅝ | 3C = 8 in. |
| Engineer Norman | 4 × 3⅕ × 12 | 3⅝ × 2¹³⁄₁₆ × 11⅝ | 5C = 16 in. |
| Utility | 4 × 4 × 12 | 3⅝ × 3⅝ × 11⅝ | 1C = 4 in. |
| Meridian | 4 × 4 × 16 | 3⅝ × 3⅝ × 15⅝ | 1C = 4 in. |
| Double Meridian | 4 × 8 × 16 | 3⅝ × 7⅝ × 15⅝ | 1C = 8 in. |
| 8-in. Through-Wall Meridian | 8 × 4 × 16 | 7⅝ × 3⅝ × 15⅝ | 1C = 4 in. |

**Non-modular sizes (BIA TN 10 Table 2)**: Queen (2¾–3 × 2¾ × 7⅝–8 in.), King (2¾–3 × 2⅝–2¾ × 9⅝–9¾ in.), Standard (3⅝ × 2¼ × 8 in.), Engineer Standard, Closure Standard. Non-modular bricks have specified and actual dimensions only; vertical coursing is still defined (Queen 5C = 16 in.; Standard 3C = 8 in.).

**Why the trinity matters for parametric modelling**: At drawing scale ≤ ¼″/ft use nominal dimensions; at scales > ¼″/ft (and always in BIM/CAD), use specified dimensions, because masons adjust head-joint thickness over runs > 4 brick-lengths to absorb tolerance. Encode all three; default geometry generation to specified; expose nominal as a display projection.

**International comparators**:
- **UK (BS EN 771-1)**: 215 × 102.5 × 65 mm work size; with 10 mm joints the coordinating size is 225 × 112.5 × 75 mm, so four courses = 300 mm. Tolerance class T2 = ±4/3/2 mm (L/W/H). The older BS 3921 is superseded; size retained.
- **Germany (DIN 105 / DIN 4172 octametric)**: Normal Format (NF) 240 × 115 × 71 mm; Thin Format (DF) 240 × 115 × 52 mm; Imperial (RF) 240 × 115 × 62 mm; Double Thin (2DF) 240 × 115 × 113 mm. The octametric module is 12.5 cm (one-eighth of a metre).
- **Australia**: per AS/NZS 4455.1, "the standard brick size in Australia is 230 × 110 × 76 mm (length × width × height)" (Midland Brick / civiltechnologist.com). With 10 mm joints, 7 courses = 600 mm vertical gauge.
- **India (IS 1077)**: modular brick 190 × 90 × 90 mm; with 10 mm joints, nominal 200 × 100 × 100 mm.

### 2. Mortar Joints

**Thicknesses (US)**: 3/8″ is the default (TMS 602 / IBC reference), with 1/4″, 1/2″, and 5/8″ used variously. BIA TN 10 quantity tables tabulate both 3/8″ and 1/2″ alternatives.

**Joint types by location**:
- **Bed joint** — horizontal, between courses.
- **Head joint** — vertical, between adjacent units in same course.
- **Collar joint** — vertical longitudinal joint between wythes in multi-wythe walls (BIA TN 10 Table 5: ⅜″ collar = 3.13 ft³/100 ft²).

**Joint profile (tooling) types** (IMI 01.410.1001; Wikipedia mortar joint):

| Profile | Water Resistance | Notes |
|---|---|---|
| **Concave** | Best (compacted) | Default US exterior; struck with curved jointer |
| **V-joint** | Good | Crisp shadow line; Craftsman/Tudor |
| **Weathered** (struck-down) | Good | Sloped to shed water |
| **Struck** (struck-up) | Poor | Interior only — ledge holds water |
| **Flush** | Poor for water; good for paint/plaster | Smooth modern look |
| **Raked** | Poor | Deep recess, dramatic shadow; demands dense brick + maintenance |
| **Beaded** | Poor | Decorative bead; historic |
| **Extruded/squeezed** | Worst | Rustic; interior only |
| **Grapevine** | Moderate | Colonial revival |

For 3-D modelling, the profile manifests as a small (typically 1/16″–1/4″ deep) sectional sweep along each visible joint axis. For elevation/shadow rendering this is significant; for solid Brep wall masses it is a second-order detail.

**Joint reinforcement** (multi-wythe / veneer with CMU backup): ladder-type and truss-type (Dur-O-Wal trade name); typical 9-gauge (W1.7, 4 mm) or 3/16″ (W2.8, 5 mm) wire. Per TMS 402/602-16: allowable wire diameter is half the mortar joint width. Spacing 16″ o.c. vertically (two courses of modular).

### 3. Layout Patterns / Bonds

Per **BIA Technical Note 30 — Bonds and Patterns in Brickwork**, the word "bond" carries three meanings: (a) structural bond (interlocking units), (b) pattern bond (visual), (c) mortar bond (adhesion). Five canonical structural bonds:

| Bond | Composition | Repeat | Typical Use | Closures at corners |
|---|---|---|---|---|
| **Running (½ stretcher)** | All stretchers, ½-brick offset | 2 courses | Veneer (single wythe) | None (alternate full brick around corner) |
| **⅓ Running** | All stretchers, ⅓-brick offset | 3 courses | Utility/Norman bricks (L=3W) | None |
| **Common / American** | Stretcher courses + header course every 5th, 6th, or 7th | 6–8 courses | Solid load-bearing wall | Header per row |
| **English** | Alternating header and stretcher courses | 2 courses | Strongest historic bond | **Queen closer** placed 4 in. in from the corner in the header course |
| **Flemish** | Header + stretcher alternating in every course | 1 course | Decorative load-bearing | Queen closer or three-quarter bat ("Dutch corner") |
| **Stack** | All stretchers, vertical joints aligned | 1 course | Purely decorative, requires ties | None |

**Variants and historic bonds**:
- **English cross / Dutch bond**: like English but stretchers in alternate stretcher courses are offset by half-brick.
- **Flemish cross bond**: Flemish headers every second course.
- **Garden wall bonds**: Flemish garden wall (Sussex bond — 3 stretchers + 1 header per course); English garden wall / Scotch bond (3 stretcher courses + 1 header course); double-stretcher garden wall.
- **Header bond / Heading bond**: All headers; used for thick walls and curved walls.
- **Monk bond**: 2 stretchers + 1 header per course.
- **Rat-trap bond** (Indian; ISI): bricks placed on edge (shiner/rowlock orientations) creating internal cavities. Per Untag Architecture's on-site measurements, it "reduces the number of bricks by 27% and cement mortar by 55%"; the broader published range is 25–30% brick and 30–55% mortar savings.
- **Herringbone (45°/90°)**, **Basket weave** (and double basket weave / pinwheel), **Soldier bond**, **Della Robbia weave** — primarily pavement and decorative panel bonds.

BIA TN 10 Table 6 provides bond correction factors for quantity estimating: English bond requires ½ extra bricks; Flemish ⅓; common bond with headers every 5th course adds ⅕.

### 4. Brick Orientations (Six Canonical Positions)

| Orientation | Description | Face exposed | Long axis | Typical use |
|---|---|---|---|---|
| **Stretcher** | Laid flat, long face out | L × H | Parallel to wall | Default of every bond |
| **Header** | Laid flat, end out | W × H | Perpendicular to wall | Wythe tying (English/Flemish, header course) |
| **Soldier** | Standing on end, narrow face out | H × L (vertical) | Vertical | Lintels, decorative bands |
| **Sailor** | Standing on end, broad face out | W × L (vertical) | Vertical | Decorative panels, accent bands |
| **Rowlock** (Bull header) | On long narrow edge, end out | W × H (rotated) | Perpendicular, horizontal | Window sills with drip, garden-wall caps |
| **Shiner** (Bull stretcher / rowlock stretcher) | On long narrow edge, broad face out | W × L (horizontal) | Parallel to wall | Decorative bands |

Coursing arithmetic (Old Structures Engineering, citing BIA): a modular brick has H = 8/3 − 3/8 in., so three stretcher courses + three bed joints = exactly 8 in. — same as one soldier + one bed joint. Two rowlocks = 8 in. Sailor and shiner break the 8 in. module and require partial bricks or a geometric break.

### 5. Cut Bricks / Closures

Closures are required because **interlocking bond cannot be maintained around corners and openings using only full bricks**. They are *not* arbitrary cuts — they are pattern-driven.

| Closure | Cut | Where used |
|---|---|---|
| **Half-bat** | Half along length | Corners in running bond (Utility), short walls |
| **Three-quarter bat** | 75% along length | "Dutch corner" in Flemish/English |
| **Quarter-bat** (closer) | 25% along length | Pattern repair |
| **Queen closer** | Cut longitudinally — half-width along full length | English/Flemish header courses next to the quoin header; placed 4 in. in from the corner |
| **King closer** | One corner cut — triangle removed from center of header to center of stretcher edge | English/Flemish at jambs; gives a half-header + half-stretcher face |
| **Bevelled closer** | Stretcher face bevelled, ½ width at one end, full width at the other | Mitred returns, splayed jambs |
| **Mitred closer** | One end cut at 45°–60° | Non-orthogonal returns |

Implication for the parametric tool: **closures are pattern-bond-specific**, not user inputs. Given a bond and a wall edge condition, the closure type is determined; the algorithm must derive them, not require them as parameters.

### 6. Special-Shaped Bricks

Per BIA Technical Note 36A and manufacturer catalogues:
- **Bullnose** (single, double, cownose) — rounded edge for sills, corners, steps.
- **Coping bricks** — wall caps, often with throating/drip.
- **Plinth bricks** — plinth header, plinth stretcher, plinth external return, plinth internal return; mark the transition between thicker and thinner wall sections.
- **Capping bricks** — wall tops.
- **Squint bricks** — for non-90° corners (30°, 45°, 60° squints).
- **Radial / arch bricks (voussoirs)** — tapered; either job-site cut from rectangular bricks (with tapered mortar joints up to 3/4″) or factory-shaped (uniform 3/8″ joints; BIA TN 31).
- **Bonding bricks** — purpose-cut for specific bond pattern repairs.
- **Air/ventilation bricks** — perforated.

These are catalogue items (data, not derived); the schema must treat them as a separate category from cuts of standard bricks.

### 7. Wall Assembly Types

- **Single-wythe veneer** — one brick wide, anchored to backup with metal ties. Per TMS 402 Ch. 12, **prescriptive tie spacing is max 32″ horizontal, 25″ vertical, 1 tie per 2⅔ ft²** (≈ 16″ × 16″); in Seismic Design Category D and above this drops to 1 per 2.0 ft².
- **Multi-wythe solid** — two or more wythes bonded with header courses (≥ 4% of wall surface, max 24″ spacing per TMS 402) or grout/ties.
- **Cavity wall** — two wythes with 1″ minimum air space (typically 2″ specified; TMS 402-16 prescriptive cavity ≤ 4½″; alternative engineered cavity to 6⅝″). Provides drainage, thermal break, capillary break.
- **Composite wall** — bonded wythes acting structurally together (collar joint mortared solid or grouted).
- **Veneer with steel-stud / wood / CMU backup** — covered by BIA TN 28 series.

**Tie embedment**: BIA recommends minimum 38 mm (1½″) embedment in veneer mortar with 16 mm (5/8″) cover from exterior face.

### 8. Corner Conditions (Quoins)

- **Running bond corners** — alternating overlap.
- **English / Flemish corners** — **English corner** (queen closer placed 4 in. in from quoin in the header course) and **Dutch corner** (three-quarter bat at the quoin in the stretcher course; no closer needed but joints don't fully break). Per BIA TN 30: "The 2-in. closure should always be placed 4 in. in from the corner, never at the corner."
- **Squint corners** — for 30°/45°/60° returns; special squint bricks or mitred-and-shaved bats.
- **L-, T-, X-intersections** — compound corners; pattern continues with closers tying intersecting walls.

### 9. Openings

- **Jambs** — full bricks where possible; in English/Flemish, queen closer or king closer adjacent to the jamb maintains bond.
- **Sills** — rowlock course is standard; slope to drain, with a drip kerf.
- **Lintels** — steel angle (BIA TN 31B), precast concrete, reinforced masonry beam, or brick arch.
- **Heads** — flat soldier course (decorative only, supported by lintel above), jack arch (structural), segmental/semicircular arch.

BIA TN 10 Fig. 4: opening height = (number of courses × coursing) + one extra bed joint. The dimension stops at the bottom of the brick above the opening, **not** at the lintel.

### 10. Arches

Per **BIA Technical Note 31 — Brick Masonry Arches** (Jan 1995):

| Arch Type | Geometry | Notes |
|---|---|---|
| **Jack** | Flat / zero rise, tapered voussoirs (camber) | Min depth = greater of 4″ + 1″/ft span, or 8″ |
| **Segmental** | Intrados < semicircle | Min depth = 1″/ft span, 4″ min for spans < 4 ft; depth = 8″ for 8 ft span |
| **Semicircular** | Intrados = semicircle | Min depth = 1″/ft span; classic |
| **Multicentered** | Several tangent arcs | Includes Tudor (four-centered) |
| **Gothic** | Arcs from spring-line centres (drop, equilateral, lancet) | Pointed |
| **Bullseye / Horseshoe** | Full circle / > semicircle | Decorative |
| **Relieving / Discharging** | Arch over lintel | Load diversion |

**Voussoir geometry**: number of voussoirs is always **odd** so a single keystone sits at the crown. Joint thickness between voussoirs: min 1/8″, max 3/4″. Joints < 1/4″ require ASTM C216 FBX dimensional tolerance. Keystone height ≤ 1/3 of arch depth.

**Glossary**: **intrados** (inner curve), **extrados** (outer curve), **soffit** (intrados surface), **spring line / springing** (intrados meets abutment), **springer** (first voussoir from skewback), **skewback** (surface of abutment supporting arch), **skewback angle**, **crown** (apex of extrados), **rise** (max height of soffit above spring line), **span** (horizontal clear), **abutment**, **camber** (small rise of jack arch), **centering** (temporary form, removed ~7 days after laying).

### 11. Expansion & Control Joints

Per **BIA Technical Note 18A — Accommodating Expansion of Brickwork** (2019):

Spacing formula: **Sₑ = wⱼ × eⱼ / 0.09** where Sₑ = spacing, wⱼ = joint width (in.), eⱼ = % compressibility of sealant (typically 25–50%). For a ½″ joint at 50% compressibility, Sₑ ≈ 23 ft theoretical.

**Practical maxima**: 25 ft for walls without openings; **20 ft when openings are present**; closer near corners. Width typically 3/8″ to 1/2″.

**Mandatory locations**: at corners (offset, not at the quoin), at one or both sides of openings, at offsets/setbacks, at shelf angles (horizontal soft joints), where backup material changes. Per BIA's expansion calculation: moisture growth coefficient = 0.003 in./in. (TMS 402-16 §4.2.4); thermal coefficient = 0.000004 in./in./°F (§4.2.3).

The parametric tool must (a) accept user-specified joint locations and (b) optionally auto-generate per BIA rules. Pattern must be locally reset across joints; bricks do not span them.

### 12. Pilasters, Piers, Buttresses

- **Engaged pilaster** — projecting bond integrated with wall; reinforces and stiffens.
- **Free-standing pier** — typically bonded English, Flemish, or all-header; cross-section 1.5 × 1.5 bricks, 2 × 2, etc.
- **Buttress** — sloped or stepped exterior pier; bond same as wall.

A square pier in English bond cycles each header course rotated 90° between courses to interlock.

### 13. Course Terminology

- **Course** — horizontal row of one unit height.
- **Wythe (leaf)** — vertical layer of one unit thickness.
- **Perpend** — vertical head joint, especially aligned ones in stack/English bond.
- **Bed joint** (horizontal), **head joint** (vertical), **collar joint** (between wythes).
- **Coursing height** — H + bed joint. Modular: 3 courses = 8 in. Engineer modular: 5 courses = 16 in. Closure (4×4×8): 1 course = 4 in.

### 14. Brick Properties (Metadata)

Per **ASTM C216** (facing brick), **ASTM C652** (hollow brick), **ASTM C62** (building brick):

- **Grade**: **SW** (Severe Weathering) — exposed exterior, weathering index ≥ 50; **MW** (Moderate Weathering) — not saturated freeze-thaw; **NW** (Negligible Weathering, C62 only — C216 dropped NW).
- **Type** (C216 facing brick): **FBX** (tight tolerance, stack-bond suitable; chips: 95–100% within 0–1/8″ edge), **FBS** (general use, plain or textured), **FBA** (architectural, no chippage criteria). C652 equivalents: HBX, HBS, HBB, HBA.
- **Compressive strength** — ASTM C67 net area; typical 3000–10,000+ psi.
- **Absorption** — 24-hr cold water % and 5-hr boiling water %; saturation coefficient.
- **Initial Rate of Absorption (IRA)** — g/min/30 in². ASTM C216 recommends pre-wetting when field IRA > 30 g/min/30 in² (0.0016 g/min/mm²).
- **Coring** — solid (< 25% void), cored (25–35%), or hollow (≥ 25% per C652).
- **Color, texture, finish** — manufacturer-defined.

These attributes form the metadata layer of the `Brick` record — never affect geometry, but propagate to BIM export and quantity takeoff.

---

## Part 2 — Software Architecture

### 15. FP/OOP Hybrid Philosophy: Functional Core, Imperative Shell

Gary Bernhardt's **"Functional Core, Imperative Shell"** (*Boundaries*, Destroy All Software, 2012) maps cleanly onto the brick generator:

- **Functional core** (`Masonry.Domain`): pure functions only. Inputs: immutable specs. Outputs: `ImmutableArray<PlacedBrick>`. No Rhino types, no I/O, no side effects.
- **Imperative shell** (`Masonry.Rhino`, `Masonry.Grasshopper`): Brep generation, Rhino document mutation, Grasshopper parameter handling. Contains all boundary code.

Bernhardt's heuristic: *"the way to figure out the separation is by doing as much as you can without mutation, and then encapsulating the mutation separately."* This pattern delivers **many fast unit tests** for the core (no Rhino runtime needed) and **few integration tests** for the shell.

### 16. Separation of Concerns

Four strict domain boundaries:

1. **Domain layer** — brick concepts, bonds, joints, layout algorithms; expresses geometry through Rhino-independent primitives (`Vector3`, `Plane`, `Brick`, `PlacedBrick`). **Never references RhinoCommon.**
2. **Geometry adapter layer** — translates `PlacedBrick` records to `Brep`, `Mesh`, `Point3d`, `Plane`, `Transform`. Knows about LOD, block instances, and mortar subtraction.
3. **Application/Plugin layer** — Grasshopper components and Rhino commands, parameter parsing, geometry baking.
4. **Standards/data layer** — JSON catalogues of brick sizes, bond definitions, special shapes.

### 17. Folder Structure (Proposed Monorepo)

```
/src
  /Masonry.Domain                 // PURE - no Rhino reference
    /Bricks                       // Brick record, Orientation, Catalog interface
    /Patterns                     // Bond records, CourseTemplate, sealed hierarchy
    /Layouts                      // Pure layout functions (planar, curved, arch)
    /Joints                       // Joint spec, Profile enum
    /Walls                        // WallSpec, OpeningSpec, ExpansionJointSpec
    /Arches                       // Voussoir math, arch curves
    /Closures                     // Cut derivations
    /Validation                   // ROP pipeline, Result<T, MasonryError>
    /Functional                   // Result, Option, Bind/Map (minimal)
    /Math                         // Vector3, Plane, Transform - own primitives
  /Masonry.Standards              // DATA
    /Bricks.US.json               // BIA TN10 modular + non-modular
    /Bricks.UK.json               // BS EN 771-1
    /Bricks.DIN.json              // NF/DF/RF/2DF
    /Bricks.AU.json               // AS/NZS 4455.1
    /Bricks.IS.json               // IS 1077
    /Bonds.json                   // Course templates as data
    /SpecialShapes.json           // Bullnose, plinth, voussoir, squint
  /Masonry.Rhino                  // RhinoCommon ADAPTER
    /Geometry                     // BrickToBrep, BrickToMesh, MortarSolid
    /Instances                    // BlockDefinition factory, instance baking
    /Drawing                      // 2D projection: elevations, plans, sections
  /Masonry.Grasshopper            // GH COMPONENTS
    /Components                   // BrickWallComponent, BrickArchComponent
    /Parameters                   // GH_Goo wrappers for WallSpec, Brick
    /UI                           // Tooltips, icons, validation surface
```

**Catalogue as data, not classes**: brick sizes, bond course templates, and special shapes belong in versioned JSON files loaded at startup into a read-only catalog service. The catalogue is region-keyed: selecting "US Modular 3⅝×2¼×7⅝" or "DIN NF 240×115×71" routes through the same generator. Adding a manufacturer-custom brick is a JSON addition; no recompilation.

**Bond as data + interpreter**, not a class hierarchy. Each bond is a sequence of `CourseTemplate` records describing orientations and offset fractions. The interpreter is a single pure function `LayCourse(template, wallSegment, brick, joint) → IEnumerable<PlacedBrick>`. English bond is two alternating course templates; Flemish is one template with alternating orientation; herringbone is a 2D template stamped at 45°.

### 18. Brick / Pattern / Layout: The Key Abstraction

The user's pivotal question — "patterns may use bricks of different sizes on different parts of the run" — resolves once the abstraction is right:

- In **canonical bond patterns**, all bricks are the **same unit**; what differs is **orientation**. A header is a Modular brick rotated 90° showing its end face. The 8″ length of a Modular brick = 2 × 3⅝″ + 3/8″ joint = 7⅝″ + 3/8″, which is precisely why headers in modular bond bond two wythes (a header spans across both 3⅝″ wythes plus their collar joint).
- **Cuts (closures, bats)** are derived: a `PlacedBrick` carries an optional `Cut` value (`HalfBat | ThreeQuarterBat | QueenCloser | KingCloser | BevelledCloser | MitredCloser | CustomCut(Polyline)`). The base brick is unchanged.
- **Special shapes** are *different units entirely* — distinct `Brick` records (bullnose, plinth, voussoir-1-of-N).

Therefore the core records are:

```
Brick { Id, Family, Specified(W,H,L), Nominal, Grade, Type, IRA, ... }
Orientation { Stretcher | Header | Soldier | Sailor | Rowlock | Shiner }
Cut = None | HalfBat | ThreeQuarterBat | QuarterBat | QueenCloser |
      KingCloser | BevelledCloser | MitredCloser | Custom(Polyline)
PlacedBrick { Brick, Frame: Plane, Orientation, Cut, SequenceId }
```

A **Pattern** is a pure function: `(WallGeometry × Brick × JointSpec) → IReadOnlyList<PlacedBrick>`. The wall is composition: patterns + openings + special conditions, validated and rasterized into placements.

This separation enables the contemporary cases too: in Gramazio Kohler's **Gantenbein Winery (2006)**, 20,000 standard bricks are placed in identical units with only the **rotation parameter** varying per unit, derived from a grayscale-mapped digital simulation. The unit is fixed; orientation becomes a continuous parameter (a `Plane` rotation around the wall normal), not a six-valued enum.

### 19. Polymorphic API Design

Single entry point dispatched internally on a discriminated union:

```
public static Result<MasonryAssembly, MasonryError> Generate(WallSpec spec);
```

Where `WallSpec` is a `sealed record` with derived types `PlanarWallSpec`, `CurvedWallSpec`, `ArchSpec`, `PierSpec`. The implementation pattern-matches on the spec type and routes to the appropriate pure layout function. No `GenerateBrick`, `GenerateMany`, `GenerateArch` overloads — one signature.

### 20. Functional Layout Algorithms

Course generation as a stateless transducer over wall length:
1. From wall geometry, compute course frames (`Plane[]`) at each vertical bed-joint elevation, using the BIA TN 10 vertical coursing table.
2. For each course frame, apply the course template: scan the horizontal axis emitting `PlacedBrick` records (with the template's orientation cycle) until wall length is filled.
3. Tail handling — the residual length triggers a **closure-derivation pure function** that returns the appropriate cut consistent with the bond's corner rule.
4. Map across all courses; flatten.

Express as `Enumerable.SelectMany` / generators, not for/while loops. Pattern matching dispatches edge conditions (corner / opening / expansion joint) by composition: openings are subtractive geometry intersected against course rows, producing left-hand and right-hand sub-courses each laid independently with jamb closure rules.

### 21. Validation & ROP

Per Scott Wlaschin's **Railway-Oriented Programming** (NDC London 2014), chain validators returning `Result<TIn, MasonryError>`:

```
ParseSpec >> ValidateBrickSize >> ValidateBondCompatibility >>
ValidateJoint >> ValidatePatternRepeat >> ValidateOpenings >>
ValidateExpansionJoints >> Layout >> Materialize
```

Each step is `T → Result<U, MasonryError>`; `bind` routes errors to the failure track. The error type is a discriminated union:

```
MasonryError =
  | InvalidBrickSize(reason)
  | BondIncompatibleWithBrick(bond, brick, reason)
  | WallTooShortForPatternRepeat(required, actual)
  | OpeningConflictsWithExpansionJoint(opening, joint)
  | UnsupportedSpecialShape(shape)
  | InvalidArchGeometry(reason)
```

Wlaschin's own caveat in *Against Railway-Oriented Programming* (fsharpforfunandprofit.com) is relevant: **don't use ROP everywhere**. For genuine domain validation (the list above), `Result` is ideal; for "file not found" boundary failures in the Rhino layer, conventional exceptions are simpler and clearer. Surface domain errors as Grasshopper component runtime warnings/errors via `AddRuntimeMessage`.

### 22. Geometry Generation Strategy

**3D**:
- Each `PlacedBrick` → `Plane` → `Box` → `Brep` via `Box.ToBrep()`.
- For chamfered/textured LOD, replace the box step with a parameterized profile sweep.
- **Mortar**: prefer **subtractive inference** — generate a single wall mass sized to the union of placed bricks plus joint thickness, then boolean-subtract the brick Breps. This avoids modelling thousands of mortar prisms while preserving joint profile geometry in section.
- **Performance — block instances**: Rhino 8 exposes native Grasshopper components (`Model Block Definition`, `Model Block Instance`). For walls > ~5,000 bricks, define **one block per unique (Brick × Cut) combination** and emit instances with per-brick transforms. Memory drops from N × Brep cache to N × Transform + 1 × Brep per definition. Rhino 8's evolved block components support custom user data per instance.

**2D**:
- Elevation = project each `PlacedBrick` Brep onto the wall's elevation plane → outline curves. Group by course for DXF layering.
- Plan = horizontal section at chosen elevation → curves.
- Section = vertical cut → curves + hatch zones for cores/mortar.
- The 2D layer must re-use the same `PlacedBrick` sequence — never re-derive layout. This guarantees plan/elevation/section consistency.

### 23. Curved Wall Handling

1. **UV parameterization** of the input surface or developable strip.
2. For each course, compute the geodesic (or constant-elevation) curve on the surface.
3. Distribute brick frames along that curve, oriented to surface normal (planar wall = special case).
4. **Lipping / wrinkling**: each rectangular brick chordal-secants the curve, leaving wedge gaps. For brick length L on radius R, head-joint thickness varies by ≈ L²/(8R). When this exceeds the joint tolerance (1/8″ – 3/4″ per BIA TN 31), responses are: (a) reduce L, (b) use tapered/voussoir-like cut bricks per course, (c) introduce expansion-joint breaks.
5. For tight curves (R < ~10 × L), shift to **voussoir generation** logic — every brick is a custom cut.

Validation: report critical radius in the ROP pipeline as a warning when joint variation > 3/4″.

### 24. Extensibility

- **Custom patterns**: add a `CourseTemplate[]` data file (JSON or in-document). The interpreter is generic.
- **New brick sizes**: append to the catalogue JSON; the file is hot-reloadable.
- **Plug-in extension points**: define an `IPatternProvider` interface for advanced users to register Grasshopper-component-defined patterns. Treat orientation as a `Plane` at the extension API so robotic/parametric facades compose naturally.
- **Standards swap**: a `Region` parameter on `WallSpec` selects the catalogue (US / UK / DIN / IS / AU) without changing component code.

---

## Part 3 — Synthesis

### 25. Recommended Architecture Flow

```
User Input (GH params)
   │
   ▼
WallSpec assembly (discriminated union: Planar | Curved | Arch | Pier)
   │
   ▼  Railway-Oriented Validation Pipeline
ValidateBrick >> ValidateBond >> ValidateJoint >>
ValidatePatternRepeat >> ValidateOpenings >> ValidateExpansion
   │
   ▼  Result<WallSpec, MasonryError>
Layout (pure function) — emits ImmutableArray<PlacedBrick>
   │
   ▼
Geometry Materialization (RhinoCommon adapter)
   ├── 3D: Brep per brick or BlockInstance per brick
   ├── Mortar: subtractive Brep boolean
   └── 2D: projection layer (elevation, plan, section curves)
   │
   ▼
Grasshopper output (Brep / Mesh / Curve / metadata DataTree)
```

### 26. Key Design Decisions to Highlight

1. **Brick vs. PlacedBrick separation** — the unit (catalogue item) is data; the placement (instance with frame, orientation, cut, sequence id) is computed.
2. **Orientation as metadata (and `Plane` at the extension API)**, not subclass. A header is the same brick with a 90° rotation flag; this collapses the type hierarchy and matches contemporary parametric practice.
3. **Closure/cut as a transformation**, not a separate `Brick` type. Cuts are derived at layout time; the cut polyline lives on `PlacedBrick.Cut`.
4. **Pattern as data + pure function**, not class hierarchy. `CourseTemplate[]` + one interpreter beats `IBond` + N implementations.
5. **Mortar as geometric inference**, not modelled per joint. The wall is bricks + a subtractive Brep wall mass.
6. **Catalogue as data**, not constants. JSON files keyed by region and manufacturer; reload without recompile.
7. **Sequence ID on every PlacedBrick** — essential for robotic / fabrication output and for IFC GUID stable identity.
8. **IFC mapping**: the wall is `IfcWall`; per IFC 4.3 §6.1.3.41 use `IfcWall` (the older `IfcWallStandardCase` is deprecated). Individual bricks can be exported as `IfcBuildingElementProxy` instances with property sets, or aggregated as material layer sets if non-instanced.

---

## Part 3 (continued) — Further Considerations: High-Value Insights

1. **Robotic fabrication and unique brick IDs (Gramazio Kohler precedent)**: At the Gantenbein Winery facade (Fläsch, Switzerland, 2006), Gramazio Kohler robotically placed **20,000 bricks in 72 prefabricated wall segments across 400 m²**, each individually rotated according to a programmed angle derived from a digital simulation of "synthetic grapes" falling into a virtual basket. The architects state: *"The robotic production method that we developed at the ETH enabled us to lay each one of the 20,000 bricks precisely according to programmed parameters — at the desired angle and at the exact prescribed intervals"* (Gramazio & Kohler, project text, World-Architects). Tobias Bonwetsch: *"Because each brick has a different rotation, every single brick has a different and unique overlap with the brick below it."* The 2019 Kitrvs Winery facade (Kitros, Greece) extended this approach: per ArchDaily (19 May 2020), *"The Kitrvs winery façade, built from 13596 individually rotated and tilted bricks are currently the largest architectural project entirely assembled on-site with an augmented reality fabrication interface"*; the 225 m² facade was built by local Greek masons in under three months using an AR guidance system developed with incon.ai, a spinoff of ETH Zürich's Robotic Systems Lab. The implication for the plugin: **every `PlacedBrick` must carry a stable `SequenceId`** (lay order) and a full `Plane` frame (six-DOF placement); the six-value orientation enum is a UI convenience layered over the continuous parameter.

2. **Contemporary non-repeating / parametric perforated screen walls**: Sstudiomm's *Negative Precision* (APT no. 7, Damavand, Iran, 2016) by Hossein Naghavi is the canonical "school case" — a parametric brick screen built without robots. Per Naghavi quoted in *The Architect's Newspaper* (Aug 2016): *"The grasshopper code is in a way the simplest possible code on ever. It just rotates the bricks in a domain of a 9–27 based on their distance from a surface modeled in rhino, quite straight forward."* And: *"The gaps are never seen from the front view and from almost all of the perspectives they are hidden by staggering bricks around them. These gaps are providing the financial possibility of having a parametric wall built without a robot."* Compare with Herzog & de Meuron's **Dominus Winery (1995–1997, Yountville)**, where they describe the basalt gabion skin as *"kind of stone wickerwork with varying degrees of transparency, more like skin than like traditional masonry"* (Herzog & de Meuron, quoted in ArchDaily, 26 June 2014) — a pre-digital ancestor of attractor-driven density logic. The plugin should expose attractor-driven rotation/spacing as a first-class composition primitive.

3. **Lipping/wrinkling in curved brick walls — the computational fix**: For a rectangular brick of length L on a wall of radius R, head-joint thickness varies as ≈ L²/(8R). Below R ≈ 10L, joint variation exceeds the 3/4″ BIA TN 31 maximum and the layout must switch from rectangular bricks with tapered joints to voussoir-style tapered bricks with uniform joints. This is the exact same mathematics as arch voussoir generation, validating the architectural decision to share an `IVoussoirGenerator` interface between the curved-wall and arch modules.

4. **BIM interoperability for masonry — IFC mapping**: Per buildingSMART IFC 4.3 §6.1.3.41, the wall assembly is `IfcWall` (with `IfcMaterialLayerSetUsage` for uniform-thickness walls, or geometric `Brep` representation for varying-thickness/curved cases; *"The entity IfcWallStandardCase has been deprecated, IfcWall with IfcMaterialLayerSetUsage is used instead."* Individual bricks, when exported as discrete units, map to `IfcBuildingElementProxy` instances grouped by `IfcRelAggregates` under the parent `IfcWall`, with `IfcRelAssociatesMaterial` linking to brick material properties. Openings via `IfcOpeningElement` and `IfcRelVoidsElement`. The plugin should export both the aggregated wall (for BIM coordination) and optionally the disaggregated brick instances (for fabrication-grade models and robotic toolpaths).

5. **Weep holes, flashing, and coursing integration**: The IBC and IRC §R703.8.6 set **a maximum spacing of 33 inches on center** for weep holes; BIA Technical Note 7B is stricter, recommending **24″ o.c. max for open head joints** (16″ when rope wicks are used). Weep holes substitute for head joints at the lowest bond course above any flashing layer — at the base of cavities, above lintels, below sills, at shelf angles. The plugin must treat **weep hole positions as named placements** within the bond layout — substituting open head joints, plastic weep tubes, or proprietary weep vents at computed horizontal locations. Flashing horizontal soft joints introduce horizontal pattern breaks analogous to vertical expansion joints.

6. **Mortar shrinkage and effective dimension corrections**: TMS 402-16 §4.2.4 gives clay-brick irreversible moisture expansion = 0.003 in./in., and §4.2.3 gives thermal coefficient 0.000004 in./in./°F. Over a 25-ft wall at a 125 °F differential the combined movement is ≈ 1 in. — this is why expansion joint spacing is 25 ft. The plugin's joint generator should account for moisture + thermal in its joint-width calculation, not assume a static 3/8″.
