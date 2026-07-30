# [DOMAIN_GLOSSARY]

Domain vocabulary carries the AEC, fabrication, geometry, geospatial, building-physics, and surface-appearance concepts every discipline owner assumes settled.

## [01]-[BUILDING_MODEL]

- `element`: names one addressable thing in the building model, carrying typed payloads and relationships rather than geometry.
    - [NOT]: XML and DOM elements, and array elements; only the AEC thing-model node carries this word.
- `property graph`: models the building as typed nodes and typed relationships, each carrying its own property payload.
- `BIM`: manages a building as semantic objects with relationships and properties, not as drafted lines.
- `IFC`: standardizes building data as an entity-and-relationship schema every AEC exchange reads and writes.
- `LOD`: declares how developed a model element's geometry and data are at one project stage.
    - [NOT]: render level of detail, which selects a display budget under its own qualified spelling.
- `LOIN`: specifies which information one exchange requires, at what stage, from which actor, for which purpose.
- `quantity takeoff`: derives measured quantities — length, area, volume, mass — from model geometry and material data for costing and carbon.

## [02]-[FABRICATION]

- `CAM`: computes machine programs from model geometry, and every program answers to verified machine truth.
- `subtractive`: removes stock material to reach the part, so toolpath, kerf, and stock model govern the outcome.
- `additive`: deposits material to build the part, so layer, path, and support strategy govern the outcome.
    - [NOT]: additive as a filler adjective; only material deposition carries this word.
- `nesting`: packs part outlines into stock sheets or lengths at true shape, minimizing waste under kerf and grain constraints.
    - [NOT]: nested data structures and nested graph containers; only true-shape part packing carries this word.
- `posting`: lowers a verified toolpath into one controller family's own program dialect.
    - [NOT]: accounting posting and HTTP posts; only controller-dialect emission carries this word.
- `controller family`: names one machine control's program dialect and capability set, and each family is one posting row.

## [03]-[DRAWING]

- `detail view`: places a clipped model view at a fixed scale inside a sheet.
    - [NOT]: user-interface detail panes; only the sheet viewport carries this word.
- `layout`: names the drawing-sheet coordinate space a detail view sits in.
    - [NOT]: graph layout, which assigns node coordinates, and memory layout, which orders fields.
- `sheet`: carries one plotted page — title block, detail views, annotation — at declared media size.
    - [NOT]: spreadsheet sheets and stock sheets; only the plotted drawing page carries this word.
- `Make2D`: extracts a hidden-line drawing from three-dimensional geometry against one camera.

## [04]-[GEOMETRY]

- `mesh`: represents a surface as vertices, normals, and face indices, and every runtime meets at that triple.
    - [NOT]: service meshes; only a polygon surface representation carries this word.
- `NURBS`: represents a curve or surface as control points, weights, knots, and degree, exactly reproducing conics.
- `Brep`: represents a solid as its bounding faces, edges, and vertices with explicit topology between them.
- `tessellation`: converts exact geometry into a mesh at a declared tolerance, and that mesh crosses the content-keyed rail.
- `GLB`: packages a glTF scene — nodes, meshes, materials, buffers — as one binary container.

## [05]-[GEOSPATIAL]

- `CRS`: declares the coordinate reference system a coordinate is measured in, including datum, projection, and units.
- `WKT`: encodes geometry or a coordinate reference system as declared text one reader parses.
- `georeferencing`: binds model coordinates to earth coordinates through a map conversion and true north.
- `earth anchor`: fixes the one model-to-earth binding every consumer reads, and a second derivation forks it.

## [06]-[BUILDING_PHYSICS]

- `daylight`: measures interior illuminance from sky and sun over an occupied year, scored as autonomy and glare metrics.
- `irradiance`: measures radiant power arriving per unit area, split into direct, diffuse, and global components.
- `solar altitude`: measures the sun's angle above the horizon at one instant and location.
- `solar azimuth`: measures the sun's compass bearing at one instant and location.
- `thermal zone`: groups spaces sharing one conditioning setpoint and schedule into one energy-model unit.
- `EPW`: carries one location's hourly weather year as the canonical simulation input.
- `IDF`: describes one energy-model run's geometry, constructions, loads, and systems for the simulation engine.
- `U-value`: measures heat transmittance through an assembly per unit area and temperature difference.
- `R-value`: measures an assembly's thermal resistance, the reciprocal of its transmittance.
- `infiltration`: measures uncontrolled air leakage into a zone, driven by pressure difference and envelope tightness.
- `psychrometrics`: relates air temperature, humidity, enthalpy, and pressure so comfort and coil loads compute from any two.
- `PMV`: predicts occupant thermal sensation from air temperature, radiant temperature, humidity, air speed, clothing, and metabolic rate.
- `UTCI`: scores outdoor thermal stress as an equivalent temperature from air, radiant, wind, and humidity conditions.
- `MEP`: names the mechanical, electrical, and plumbing disciplines whose distribution networks thread the building.
- `distribution system`: names one connected network of ports and segments carrying a medium through the building.
- `VAV`: conditions a zone by modulating supply air volume at constant temperature.

## [07]-[APPEARANCE]

- `PBR`: describes a surface by measurable optics — reflectance, roughness, metalness, transmission — so it shades alike in every rig and renderer.
- `texture plane`: holds one channel's texel raster at one extent and depth — the addressable byte unit a container stores and a content key digests.
- `texture channel`: names one optical or geometric quantity a texture plane carries, and decides that plane's transfer, neutral, unit, and mip fold.
- `texture set`: gathers the channel planes describing one surface under one key, and the set is the addressable unit a consumer binds.
    - [NOT]: texture atlases, which share one plane across several sets by content address and merge no set.
- `texture bake`: evaluates a shading description into texel planes at a declared extent, freezing procedural and layered appearance as sampled data.
    - [NOT]: element-graph baking, folding one graph root into a flat element, and the host bake gate, falling a live evaluator to a simulation.
- `channel packing`: stores three single-component channels in one plane's RGB slots under a fixed order, so one fetch serves three quantities.
- `normal convention`: fixes the green-channel polarity of a tangent-space normal plane, so the two values invert each other's apparent lighting.
- `seamless tiling`: makes a plane's opposing edges continuous so repeating it shows no seam, and every channel of one set takes identical geometry.
- `mip`: holds one pre-filtered level of a plane's resolution pyramid, and a level is folded in the linear domain under its channel's own kernel.
- `UDIM`: indexes a surface's UV space as a grid of unit tiles, so one channel spans several planes addressed by tile number.
- `HDRI`: captures a scene's full luminance range as one image, and the stored number is a light quantity rather than a display code value.
- `IBL`: lights a surface from an environment image, prefiltered once into a diffuse irradiance term and a roughness-indexed specular term.
- `KTX2`: packages texture planes with their pyramid, layers, and block-compressed payload as one GPU-ready container.
- `EXR`: stores half and float texel planes with named channels, so a deep-pixel product survives without quantization.
- `scene-linear`: measures colour as a linear light quantity in a declared working space, so light arithmetic composes correctly.
    - [NOT]: display-referred sRGB, whose stored number is an encoded code value that decodes to a light quantity on read.
