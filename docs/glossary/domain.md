# [DOMAIN_GLOSSARY]

Domain vocabulary carries the AEC, fabrication, geometry, geospatial, building-physics, and surface-appearance concepts every discipline owner assumes settled.

## [01]-[BUILDING_MODEL]

- `element`: Names one addressable thing in the building model, carrying typed payloads and relationships rather than geometry.
    - [NOT]: XML and DOM elements, and array elements; only the AEC thing-model node carries this word.
- `property graph`: Models the building as typed nodes and typed relationships, each carrying its own property payload.
- `BIM`: Manages a building as semantic objects with relationships and properties, not as drafted lines.
- `IFC`: Standardizes building data as an entity-and-relationship schema every AEC exchange reads and writes.
- `LOD`: Declares how developed a model element's geometry and data are at one project stage.
    - [NOT]: Render level of detail, which selects a display budget under its own qualified spelling.
- `LOIN`: Specifies which information one exchange requires, at what stage, from which actor, for which purpose.
- `quantity takeoff`: Derives measured quantities — length, area, volume, mass — from model geometry and material data for costing and carbon.

## [02]-[FABRICATION]

- `CAM`: Computes machine programs from model geometry, and every program answers to verified machine truth.
- `subtractive`: Removes stock material to reach the part, so toolpath, kerf, and stock model govern the outcome.
- `additive`: Deposits material to build the part, so layer, path, and support strategy govern the outcome.
    - [NOT]: Additive as a filler adjective; only material deposition carries this word.
- `nesting`: Packs part outlines into stock sheets or lengths at true shape, minimizing waste under kerf and grain constraints.
    - [NOT]: Nested data structures and nested graph containers; only true-shape part packing carries this word.
- `posting`: Lowers a verified toolpath into one controller family's own program dialect.
    - [NOT]: Accounting posting and HTTP posts; only controller-dialect emission carries this word.
- `controller family`: Names one machine control's program dialect and capability set, and each family is one posting row.

## [03]-[DRAWING]

- `detail view`: Places a clipped model view at a fixed scale inside a sheet.
    - [NOT]: User-interface detail panes; only the sheet viewport carries this word.
- `layout`: Names the drawing-sheet coordinate space a detail view sits in.
    - [NOT]: Graph layout, which assigns node coordinates, and memory layout, which orders fields.
- `sheet`: Carries one plotted page — title block, detail views, annotation — at declared media size.
    - [NOT]: Spreadsheet sheets and stock sheets; only the plotted drawing page carries this word.
- `Make2D`: Extracts a hidden-line drawing from three-dimensional geometry against one camera.
- `sheet size`: Names one published drawing extent off the kernel sheet roster — series and index, or an admitted custom pair under its standard.
    - [NOT]: Host page-view width/height, which the document mutates as its own state; roster rows carry the standard's fact.
- `PdfPolicy`: Names the Rhino stratum's PDF publish capability, whose AppUi peer spells `PdfExport`, both riding the kernel `PlotPolicy` defaults.
    - [NOT]: One shared PDF owner across strata — the capability stays plural per stratum by ruling, only the NAME disambiguates.
- `icon source`: Resolves through the kernel `AssetOrigin` family alone, and each carrier shape lands as one case on that family.
    - [NOT]: Boundary-local origin unions and filename-scale conventions; scale rides a column on the raster case, never a parsed suffix.
- `user text`: Carries the host document's per-object and per-document user STRINGS (`UserTextValue`/`UserTextAnswer`).
    - [NOT]: Annotation text runs (`Annotation/text`) and command-prompt default text (`Commands`); three senses, three owners, no shared carrier.

## [04]-[GEOMETRY]

- `mesh`: Represents a surface as vertices, normals, and face indices, and every runtime meets at that triple.
    - [NOT]: Service meshes; only a polygon surface representation carries this word.
- `NURBS`: Represents a curve or surface as control points, weights, knots, and degree, exactly reproducing conics.
- `Brep`: Represents a solid as its bounding faces, edges, and vertices with explicit topology between them.
- `tessellation`: Converts exact geometry into a mesh at a declared tolerance, and that mesh crosses the content-keyed rail.
- `GLB`: Packages a glTF scene — nodes, meshes, materials, buffers — as one binary container.

## [05]-[GEOSPATIAL]

- `CRS`: Declares the coordinate reference system a coordinate is measured in, including datum, projection, and units.
- `WKT`: Encodes geometry or a coordinate reference system as declared text one reader parses.
- `georeferencing`: Binds model coordinates to earth coordinates through a map conversion and true north.
- `earth anchor`: Fixes the one model-to-earth binding every consumer reads, and a second derivation forks it.

## [06]-[BUILDING_PHYSICS]

- `daylight`: Measures interior illuminance from sky and sun over an occupied year, scored as autonomy and glare metrics.
- `irradiance`: Measures radiant power arriving per unit area, split into direct, diffuse, and global components.
- `solar altitude`: Measures the sun's angle above the horizon at one instant and location.
- `solar azimuth`: Measures the sun's compass bearing at one instant and location.
- `thermal zone`: Groups spaces sharing one conditioning setpoint and schedule into one energy-model unit.
- `EPW`: Carries one location's hourly weather year as the canonical simulation input.
- `IDF`: Describes one energy-model run's geometry, constructions, loads, and systems for the simulation engine.
- `U-value`: Measures heat transmittance through an assembly per unit area and temperature difference.
- `R-value`: Measures an assembly's thermal resistance, the reciprocal of its transmittance.
- `infiltration`: Measures uncontrolled air leakage into a zone, driven by pressure difference and building-envelope tightness.
- `psychrometrics`: Relates air temperature, humidity, enthalpy, and pressure so comfort and coil loads compute from any two.
- `PMV`: Predicts occupant thermal sensation from air temperature, radiant temperature, humidity, air speed, clothing, and metabolic rate.
- `UTCI`: Scores outdoor thermal stress as an equivalent temperature from air, radiant, wind, and humidity conditions.
- `MEP`: Names the mechanical, electrical, and plumbing disciplines whose distribution networks thread the building.
- `distribution system`: Names one connected network of ports and segments carrying a medium through the building.
- `VAV`: Conditions a zone by modulating supply air volume at constant temperature.

## [07]-[APPEARANCE]

- `PBR`: Describes a surface by measurable optics — reflectance, roughness, metalness, transmission — so it shades alike in every rig and renderer.
- `texture plane`: Holds one channel's texel raster at one extent and depth — the addressable byte unit a container stores and a content key digests.
- `texture channel`: Names one optical or geometric quantity a texture plane carries, and decides that plane's transfer, neutral, unit, and mip fold.
- `texture set`: Gathers the channel planes describing one surface under one key, and the set is the addressable unit a consumer binds.
    - [NOT]: Texture atlases, which share one plane across several sets by content address and merge no set.
- `texture bake`: Evaluates a shading description into texel planes at a declared extent, freezing procedural and layered appearance as sampled data.
    - [NOT]: Element-graph baking, folding one graph root into a flat element, and the host bake gate, falling a live evaluator to a simulation.
- `channel packing`: Stores three single-component channels in one plane's RGB slots under a fixed order, so one fetch serves three quantities.
- `normal convention`: Fixes the green-channel polarity of a tangent-space normal plane, so the two values invert each other's apparent lighting.
- `seamless tiling`: Makes a plane's opposing edges continuous so repeating it shows no seam, and every channel of one set takes identical geometry.
- `mip`: Holds one pre-filtered level of a plane's resolution pyramid, and a level is folded in the linear domain under its channel's own kernel.
- `UDIM`: Indexes a surface's UV space as a grid of unit tiles, so one channel spans several planes addressed by tile number.
- `HDRI`: Captures a scene's full luminance range as one image, and the stored number is a light quantity rather than a display code value.
- `IBL`: Lights a surface from an environment image, prefiltered once into a diffuse irradiance term and a roughness-indexed specular term.
- `KTX2`: Packages texture planes with their pyramid, layers, and block-compressed payload as one GPU-ready container.
- `EXR`: Stores half and float texel planes with named channels, so a deep-pixel product survives without quantization.
- `scene-linear`: Measures colour as a linear light quantity in a declared working space, so light arithmetic composes correctly.
    - [NOT]: Display-referred sRGB, whose stored number is an encoded code value that decodes to a light quantity on read.
