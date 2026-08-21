# [BIM_APPEARANCE]

`AppearanceProjection` is the IFC surface-style PROJECTOR lowering the live GeometryGym `IfcStyledItem`/`IfcSurfaceStyle` presentation graph onto the `Rasm.Element` seam `Graph/element#NODE_MODEL` `Node.Appearance` carrying the neutral `AppearanceSummary`: `AppearanceProjection.Project` extracts the front-face `IfcSurfaceStyle`, folds its `IfcSurfaceStyleRendering`/`IfcSurfaceStyleShading`/`IfcSurfaceStyleRefraction` element selects onto one neutral PBR vector (scene-linear base colour + metalness + roughness + opacity + a `Transmissive` refractive flag DISTINCT from opacity, so an opaque-alpha glass still carries transmission), classifies the `IfcSurfaceStyleWithTextures` select onto the `SurfaceTexture` roster, and mints the content-keyed seam `Node.Appearance` the `Bake` fold reads through the `Relations/relation#EDGE_ALGEBRA` `Associate` edge into `element.Appearance`. `Rasm.Element` OWNS the `AppearanceSummary` PBR vocabulary and the `Projection/address#CONTENT_ADDRESS` content-key derivation at that seam; this page owns ONLY the GeometryGym discrimination that fills it, the `ReflectanceModel` `[SmartEnum<string>]` IFC reflectance-method roster, and the `TextureMode`/`SurfaceTexture`/`UvTransform` IFC texture vocabulary, never re-declaring an appearance record. `AppearanceProjection` runs BIDIRECTIONAL: `AppearanceProjection.Author` is the inverse half — re-authoring the `IfcSurfaceStyleRendering` (diffuse/specular/highlight/reflectance-method/transparency), an `IfcSurfaceStyleWithTextures` for a textured appearance, and an `IfcSurfaceStyleRefraction` for a transmissive one, all combined in ONE `IfcSurfaceStyle` through the five-slot constructor, with `AppearanceProjection.Bind` landing the mesh's own seam attribute lanes onto the authored representation item as an `IfcIndexedTriangleTextureMap` and an `IfcIndexedColourMap`, so the surface style, its texture coordinates, and its per-face radiometry all round-trip; both egress entries are COMPLETE and ARM on a body-representation author joining the IFC emit rail — `Projection/egress#IFC_EGRESS` `Emit` re-authors semantics alone and geometry egress rides the glTF/3dm deliverables, so the pair's standing consumer is the ingest round trip and the styled-item bind fires the moment a face set is authored on that rail.

`StyledAppearance` carries the `SurfaceTexture` roster BESIDE the summary on the ingest product and projected through `RosterOf` onto the Element `TextureRoster` seam row, because a texture is a FIELD the frozen seven-value `AppearanceSummary` preimage structurally cannot carry: folding a map into a scalar is the averaged-map defect, and an eighth summary column re-keys every stored `Node.Appearance` and forks this projector's own cross-folder dedup key in one edit. Each `SurfaceTexture` names its canonical channel through its `TextureMode` row and carries the gloss/transparency polarity IFC declares and the channel name does not, so the texture-set owner binds and inverts decoded texels this host-neutral leaf never opens. `IfcSurfaceSide` sidedness rides beside them for a second reason the same preimage law covers: it is a render-representation toggle selecting WHICH faces a style paints, where every summary channel answers how a painted face reflects — so it round-trips through `Side` on egress and reaches the glTF rail as `Exchange/export#EXPORT_RAIL` `MaterialFinish.DoubleSided` rather than as an eighth scalar no BSDF reads.

IFC presentation colours are display-referred sRGB; the projector lowers each channel to scene-linear through the sRGB EOTF read off the kernel `Numerics/atoms#SCALAR_FLOOR` `RgbProfile.Srgb` row (a pure host-neutral transfer, IEC 61966-2-1, held by the row every `PerceptualColor` crossing reads and copied nowhere) so the seam `BaseColor` aligns with the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` scene-linear convention, and the egress encodes back through the inverse OETF — the working-space PRIMARIES conversion (Rec.709→AP1/ACEScg) stays the `Rasm.Materials` Unicolour owner's concern, never re-derived here. `AppearanceSummary.AppearanceKey` reconciles this appearance with the `Rasm.Materials` OpenPBR owner at the content key: it is the kernel seed-zero `XxHash128` over the neutral PBR vector — the shared seam-owned `AppearanceSummary.Of` derivation the `Rasm.Materials` `ComponentProjector` composes identically, so a BIM-imported `IfcSurfaceStyleRendering` style and a `Rasm.Materials` OpenPBR row describing the same surface dedup to one content key, the `Rasm.Materials` owner the authority for the full BSDF and this page producing only the IFC-derived neutral summary. `Rasm.Materials` holds the `surface#OPENPBR_SLAB` `OpenPbrSurface` vector, the `surface#CONDUCTOR_IOR` conductor-IOR table, and the OpenPBR slab algebra, so re-minting any of them in this owner is the named cross-folder seam violation.

`AppearanceProjection` keeps every signature HOST-NEUTRAL — no `Rhino.Geometry`, no `Unicolour`, no `System.Drawing.Color` crosses one (the GeometryGym `IfcColourRgb.Color()`/`IfcColourRgb(DatabaseIfc, Color)` host-coupled members are the deleted form, only the `Red`/`Green`/`Blue` doubles + the `(DatabaseIfc, double, double, double)` ctor are read/authored). `Object` carries appearance element-scoped through the `Associate` edge — a seam `Node.Appearance`, never a record nested in the retired `BimMaterial` — so the retired `BimAppearance`/`AppearanceColor` records and the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial.Option<BimAppearance>` carrier are GONE, mirroring how the rebuilt `Semantics/classification#CLASSIFICATION_AXIS` lowers onto the seam `Classification` value and `Semantics/composition#MATERIAL_COMPOSITION` onto the seam `Node.Material`. `Project` rails a malformed presentation graph onto `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected` (`surface-style-miss`), lifted BARE onto the `Fin` rail.

## [01]-[INDEX]

- [02]-[APPEARANCE_PROJECTION]: `AppearanceProjection.Project` the `IfcStyledItem`→`StyledAppearance` ingress fold over the presentation graph, the `ReflectanceModel` `[SmartEnum<string>]` IFC reflectance-method roster carrying the typed PBR bias (`Metalness`/`RoughnessHint`/`Transmissive`) with the `ForPbr` reverse classifier, the `TextureMode` mode→canonical-channel roster carrying the seam `ChannelPolarity` column, `TextureTrait` the capability vocabulary a carried texture answers over with `Rowable` the seam roster demand, the `SurfaceTexture` `[Union]` over the three concrete IFC texture payloads with the seam `TextureWrap` axis pair and the `UvTransform` frame they carry, the `RosterOf` mint projecting a styled appearance onto the Element `TextureRoster` seam row in the seam own typed vocabulary, the sRGB `Linearize`/`Encode` transfer pair, the `AppearanceKey` content-key derivation shared with `Rasm.Materials`, the inverse `AppearanceProjection.Author` egress re-authoring the `IfcSurfaceStyleRendering`+`IfcSurfaceStyleWithTextures`+`IfcSurfaceStyleRefraction` surface style (armed on the emit rail's body-representation author), the `AppearanceProjection.Bind` representation-item binder driving each seam attribute lane off its `Binders` row, the `IndexedColour` per-face radiometry value both directions share, and the `IfcInternals` `[UnsafeAccessor]` capsule through which they reach GeometryGym's sealed presentation payloads.

## [02]-[APPEARANCE_PROJECTION]

- Owner: `AppearanceProjection` the static BIDIRECTIONAL GeometryGym↔seam surface-style projector — the `Project` ingress folding one `IfcStyledItem`'s front-face `IfcSurfaceStyle` into one `StyledAppearance` (the content-keyed seam `Node.Appearance` beside its texture roster), and the `Author` egress re-authoring a seam `AppearanceSummary` and that roster back onto the GeometryGym presentation graph the `Emit` composes; `ReflectanceModel` the `[SmartEnum<string>]` IFC reflectance-method roster the projection folds the method onto without re-reading the enum string; `TextureMode` the IFC texture-mode roster carrying each token's canonical channel name and its gloss/transparency polarity; `SurfaceTexture` the `[Union]` over the three concrete `IfcSurfaceTexture` payloads with `UvTransform` the UV frame both projector halves carry; `StyledAppearance` the ingest product pairing the summary with its texture roster, its `SurfaceLighting` coefficient set, and the `IfcSurfaceSide` sidedness bit — the three facts the frozen preimage cannot carry; `IndexedColour` the IFC per-face radiometry value BOTH directions share — palette, one-based per-face run, single alpha — carrying the `Of` read off a face set, the `Of` fold off a seam colour lane keyed on the ONE byte quantizer, the `Rgba` per-face resolve the geometry walk reads, and the `Author` map write; `Bind` the representation-item egress binder over the `Binders` channel-to-author table; `IfcInternals` the `[UnsafeAccessor]` capsule that is this branch's ONE reach into a GeometryGym `internal` presentation payload, projecting the colour palette and index runs as detached `Seq` values and owning the `IfcIndexedTriangleTextureMap` mint whole, pinned to the manifest package version. `Rasm.Element` owns the `AppearanceSummary` neutral PBR record and its key derivation at the seam — the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` codec hashed through the `Projection/address#CONTENT_ADDRESS` `ContentAddress` — and this page declares neither, composing the seam vocabulary, mapping the GeometryGym presentation entities onto it and back.
- Cases: `ReflectanceModel` arms `Blinn`/`Flat`/`Glass`/`Matt`/`Metal`/`Mirror`/`Phong`/`Plastic`/`Strauss`/`NotDefined` (10), the full IFC4.3 `IfcReflectanceMethodEnum` partition keyed on the schema constant, each carrying its typed PBR bias — `Metalness` (`METAL`/`MIRROR` → 1.0, `STRAUSS` → 0.5, every dielectric → 0.0), `RoughnessHint` (the fallback when the style supplies no `IfcSpecularHighlight` — `MIRROR` → 0.0, `MATT`/`FLAT` → 1.0), and `Transmissive` (`GLASS` → true); `TextureMode` spans BOTH the IFC2x3 `IfcSurfaceTextureEnum` tokens and the IFC4 free-identifier spellings authoring tools emit, each row naming the canonical channel it resolves to and its seam `ChannelPolarity` (`SHININESS` gloss and `TRANSPARENCYMAP` transparency `Inverted`, every other row `Direct`), `NotDefined` the unresolved row; `TextureTrait` rows `Classified`/`Addressable`/`Embedded`/`Transformed`, the four facts a downstream gate asks before it acts, held as one `CapabilitySet<TextureTrait>` derived per case; `SurfaceTexture` arms `Url`/`Blob`/`Pixels` over `IfcImageTexture`/`IfcBlobTexture`/`IfcPixelTexture`, each sharing the mode, the seam `TextureWrap` axis pair, and the optional UV frame through the root positional columns; the appearance is the seam's ONE `AppearanceSummary`, never a `RenderingAppearance`/`ShadingAppearance`/`TexturedAppearance` sibling triple and never a Bim `BimAppearance`/`AppearanceColor` record beside the seam.
- Entry: `AppearanceProjection.Project(IfcStyledItem styledItem, double tolerance, Op key)` returning `Fin<StyledAppearance>` is the per-styled-item leaf the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes from its per-`Object` representation walk — a dedicated appearance fold (the sibling of `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.MaterialEdges`) that discovers each object's styled items through the GeometryGym `IfcRepresentationItem.StyledByItem` inverse, calls `Project`, dedups the minted node by id, authors the `Object`→`Appearance` `Associate` edge against the object's rooted `NodeId` with `MaterialUsage.Unbound` (the appearance `Associate` edge carries no material usage), and projects the `StyledAppearance` texture roster through `RosterOf` onto the Element `TextureRoster` seam row keyed by the minted appearance node id — the roster never enters the seam graph, which owns the summary alone, and `Rasm.Materials` `SetIngest.Roster` classifies the row with no app-root relay; `AppearanceProjection.TextureSetOf(ElementGraph, NodeId)` is the READER half of the `Rasm.Materials` link, resolving the `Associate`-linked `DetailSchema.Appearance` bag off the appearance node and reading `DetailSchema.TextureSet` back as the baked-set `ContentAddress` the Materials `Projection/component#COMPONENT_PROJECTOR` `BindTextureSet` wrote — the address rides BESIDE the frozen seven-value `AppearanceSummary` preimage and never inside it, because widening that preimage re-keys every stored `Node.Appearance`; `AppearanceProjection.DoubleSidedOf(ElementGraph, NodeId)` is its sibling projection over the SAME `AppearanceRow` walk, reading `DetailSchema.DoubleSided` back as the sidedness a `Rasm.Materials` OpenPBR thin-walled row wrote, so a Materials-authored shell reaches `Exchange/export#EXPORT_RAIL` `MaterialFinish.DoubleSided` by the route an IFC-declared one already takes — precedence is by ORIGIN, the `IfcSurfaceSide` bit on this projector's own `StyledAppearance` authoritative for an appearance this page minted and the bag row answering for one no IFC style described, so the two producers never contend over a node and `Option<bool>` keeps an undeclared fact distinct from a declared single-sided one; a `Material`-scoped style instead rides the `Rasm.Materials` `ComponentProjector`, which authors its own `element→appearance` edge — extracting the front-face `IfcSurfaceStyle` (`Side` `BOTH`/`POSITIVE`) through `BaseClassIfc.Extract<IfcSurfaceStyle>()` (version-agnostic: it flattens an IFC2x3 `IfcPresentationStyleAssignment` wrapper), folding its element selects onto the neutral PBR vector, and minting the content-keyed seam `Node.Appearance`; `Fin<T>` aborts on a presentation graph carrying no front-face surface style (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected` `surface-style-miss`) and carries the seam factory's own `ElementFault.ValueRejected` through on an out-of-unit channel, both lifted BARE (the `Fault`-derived band is the `Error`, no `.ToError()` hop). `AppearanceProjection.Author(DatabaseIfc db, AppearanceSummary summary, Seq<SurfaceTexture> textures, Option<SurfaceLighting> lighting, bool doubleSided)` is the egress entry for an `Object` node carrying an appearance — re-authoring the `IfcSurfaceStyleRendering` from the neutral summary, the `IfcSurfaceStyleWithTextures` from a non-empty roster, and the `IfcSurfaceStyleRefraction` from a transmissive summary, all through ONE five-slot `IfcSurfaceStyle` constructor; total (authoring from a valid summary cannot fail), returning the `IfcStyledItem` a representation author binds onto its item. `AppearanceProjection.Bind(IfcStyledItem styled, IfcTriangulatedFaceSet faceSet, Seq<(EncodingChannel Channel, float[] Lane)> attributes, long[] corners, Op key)` returning `Fin<Seq<(EncodingChannel, IfcPresentationItem)>>` is the second egress entry — railed because the colour arm's palette holds raw decoded floats the kernel byte-leg admission can refuse — called once per authored triangulated face set, handing that mesh's OWN seam attribute roster — these payloads bind on the REPRESENTATION ITEM, not the style, so they cannot be columns on `Author`, and one entry driving the `Binders` table means a lane IFC learns to bind is a row rather than a third public member; an empty receipt is the ordinary untextured, unpainted case.
- Auto: `Project` reads the front-face `IfcSurfaceStyle.Styles` element selects — the `IfcSurfaceStyleRendering` (an `IfcSurfaceStyleShading` subtype, so it supplies the inherited `SurfaceColour`/`Transparency` with the `DiffuseColour`/`ReflectanceMethod`/`SpecularHighlight` rendering channels), a bare `IfcSurfaceStyleShading` fallback (colour/transparency only), and the `IfcSurfaceStyleRefraction` optical signal — and folds the channel precedence: the rendering `DiffuseColour` overrides the shading `SurfaceColour` for the base colour through BOTH `IfcColourOrFactor` select arms — an `IfcColourRgb` replaces (each channel lowered to scene-linear through `Linearize`), an `IfcNormalisedRatioMeasure` SCALES the linearized surface colour (reflectance is linear-domain energy; the GG ctor clamps the ratio [0,1]; the `as IfcColourRgb` cast that ignored the factor arm is the deleted form), defaulting grey when absent — the opacity is the transparency complement (a `double.NaN` transparency, the GeometryGym unset sentinel, defaulting to opaque), the metalness is `ReflectanceModel.FromIfc(ReflectanceMethod).Metalness`, the roughness reads the `IfcSpecularHighlightSelect` (`IfcSpecularRoughness` directly as a [0,1] roughness, `IfcSpecularExponent` converted through the Phong `α = √(2/(n+2))`) and falls back to the row's `RoughnessHint`, and the transmissive flag is the REFRACTIVE signal (the row's `Transmissive` GLASS method or a present `IfcSurfaceStyleRefraction`, NEVER a sub-unit opacity — IFC `Transparency` is the alpha/opacity channel, physically distinct from transmission), PERSISTED on the summary apart from opacity (so an opaque-alpha refractive glass keeps its transmission, the round-trip symmetric with the egress `IfcSurfaceStyleRefraction`); the seam-owned `AppearanceSummary.Of` then admits the seven frozen preimage values under this call's `Op` key and derives the `AppearanceKey` itself as the kernel seed-zero `XxHash128` over the canonical PBR bytes — the factory's own writer runs at raw IEEE bits because PBR scalars are not `Header`-quantized measures, so no tolerance argument exists to pass and none forks the shared dedup key (the seam `CanonicalWriter` → `ContentAddress.Of`, the ONE hasher the `Rasm.Materials` owner composes identically, this page assembling no key bytes of its own); `TexturesOf` folds every `IfcSurfaceStyleWithTextures` select's `Textures` list through `SurfaceTexture.Of` into the roster riding beside the summary — the concrete subtype discriminates the payload case, the `Mode` token resolves its canonical channel and polarity, and an optional `IfcCartesianTransformationOperator2D` lifts to the `UvTransform` frame — while the neutral scalars stay untouched, so a textured style never averages a map into a scalar; `Mint` content-keys the seam `Node.Appearance` whose id is the seam `Node.ToCanonicalBytes` (id excluded) re-stamped through the seam `Node.Relabel` — the class-root `[Union]` `Node` case generates no `with`, so the `draft with { Id = … }` spelling is the deleted form, the mint the `Rasm.Materials` `ComponentProjector` composes identically — so two structurally-identical appearances dedup to one node. `Author` encodes the scene-linear base colour back to display sRGB through `Encode`, picks the `IfcReflectanceMethodEnum` from the neutral PBR through `ReflectanceModel.ForPbr` (a transmissive surface `GLASS`, a metallic mirror `MIRROR`, a rough metal `METAL`, a matte dielectric `MATT` so an imported `MATT`/`FLAT` finish round-trips via its `1.0` `RoughnessHint`, every remaining dielectric `PLASTIC` — the modern method subset only, a superseded `BLINN`/`PHONG`/`STRAUSS` being import vocabulary the neutral vector absorbs, never re-authored), tints the specular from the base colour for a metal and reflects neutral for a dielectric, authors `IfcSpecularRoughness` from the summary roughness, re-authors each `SurfaceTexture` through its own total `Switch` into one `IfcSurfaceStyleWithTextures`, and lands every element select through the ONE five-slot `IfcSurfaceStyle` constructor whose null slots carry the absent axes — a ternary ladder growing an arm per transmission×texturing combination is the deleted form. Egress runs PAYLOAD-COMPLETE and BINDING-COMPLETE: `Bind` reads the mesh's seam attribute roster and lands each lane through its `Binders` row — the `Uv` lane minting an `IfcIndexedTriangleTextureMap` through the `IfcInternals` capsule, naming the extracted `IfcSurfaceTexture` rows its parameterization serves, writing the public `TexCoords` vertex list and the per-triangle index triples, and letting the `MappedTo` setter self-register the map into the face set's `HasTextures`; the `ColorRgba` lane folding through `IndexedColour` into a deduped palette with a per-face index run and authoring an `IfcIndexedColourMap` on public surface alone. Both rows read an arity guard before binding, because a lane whose corner run walks off its own end authors a file that faults in the RECEIVING application. Rasm-authored elements therefore carry their per-vertex UV BINDING and its radiometry, and `Exchange/import#EXPLICIT_TESSELLATION` re-reads exactly what this half wrote.
- Receipt: the seam `Node.Appearance` is the appearance evidence the `Projection/semantic#SEMANTIC_PROJECTOR` projector lands (authoring the `Object`→`Appearance` `Associate` edge) and the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads into `element.Appearance` (an `Option<AppearanceSummary>` a consumer reads flat), the `Exchange/export#EXPORT_RAIL` `MaterialFinish` carries whole — its scene-linear base colour and opacity entering `baseColorFactor` unencoded, its metalness and roughness written on every material because the glTF defaults are both unity, and its `Transmissive` bit writing `KHR_materials_transmission` while the opacity alone drives `AlphaMode` — and the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` OpenPBR owner reconciles at the `AppearanceKey` content key; the `ReflectanceModel` typed PBR bias is the IFC-side mapping a downstream metallic/dielectric author folds, never a re-read of the IFC enum string.
- Growth: a new IFC reflectance method is one `ReflectanceModel` row carrying its schema constant and typed PBR bias; a new texture-mode spelling is one `TextureMode` row naming its canonical channel and its seam `ChannelPolarity`; a new capability a consumer gates on is one `TextureTrait` row with its per-case derivation, never a second predicate at the gate; a new IFC texture payload subtype is one `SurfaceTexture` case the `Of` fold and the `Author` `Switch` are both compiler-forced to route; a new SCALAR presentation-channel read is one more `Styles` element-select arm folding onto the neutral summary, a new FIELD-valued read is one more carrier beside the textures on `StyledAppearance` (the `SurfaceLighting` four-colour coefficient set is the landed one, its egress arm the `IfcSurfaceStyle` lighting slot), and a new RENDER-REPRESENTATION toggle (the sidedness bit is the landed one) is one more column there whose egress arm is a single `IfcSurfaceStyle` slot — never a summary column in either case; a further Materials-authored fact a consumer keys on is one owner-declared `PropertyName` at the `Rasm.Element` seam with one projection over the existing `AppearanceRow` walk, never a second edge traversal; the seam `AppearanceSummary` absorbs the neutral vector with no seam edit; a seam attribute lane IFC learns to bind to a representation item is one `Binders` row and the author it names, never a second public binding member; a further GeometryGym payload the assembly seals is one accessor row with one value-projecting member on `IfcInternals`, so the pinned-version surface stays countable in one class; never a per-style appearance class, never a Bim appearance record beside the seam node, and never a second accessor capsule.
- Boundary: the appearance model is the seam `Node.Appearance` + `AppearanceSummary` and a Bim `BimAppearance`/`AppearanceColor`/`RenderingAppearance`/`ShadingAppearance`/`TexturedAppearance` re-declaration is the deleted form — the seam owns the neutral PBR record, this page owns only the GeometryGym discrimination that fills it, so the appearance lowers onto the one seam summary with the absent channels defaulted, never a parallel per-style class; the retired `BimAppearance` record and the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial.Option<BimAppearance>` carrier are GONE, appearance being element-scoped (a seam `Node.Appearance` the `Object` carries through the `Associate` edge the `Bake` fold reads into `element.Appearance`), never a record nested in a material; the projection rides the GeometryGym `IfcStyledItem`/`IfcSurfaceStyle`/`IfcSurfaceStyleRendering`/`IfcColourRgb` surface consumed as settled vocabulary (`.api/api-geometrygym-ifc` presentation rows) through `BaseClassIfc.Extract<IfcSurfaceStyle>()`, and a hand-rolled STEP presentation-style reader is the deleted form; the page is HOST-NEUTRAL — a `Rhino.Geometry` colour, a `System.Drawing.Color` (the `IfcColourRgb.Color()`/`IfcColourRgb(DatabaseIfc, Color)` host-coupled members), or a `Unicolour` object crossing a signature is the named host-coupling defect, only the `Red`/`Green`/`Blue` scene-linear doubles cross; the ONE exception is a construction argument, not a crossing — `IfcColourRgbList` publishes no neutral author at all, so `IndexedColour.Author` spells `System.Drawing.Color.FromArgb` fully qualified at that single site inside the value that owns the palette, and the discriminant is exact: a host colour type is forbidden where a neutral member exists (`IfcColourRgb` has its double triple) and confined to the boundary owner where it is the only expression that constructs the entity; IFC presentation colour is display-referred sRGB lowered to scene-linear through the `Linearize` EOTF and encoded back through the `Encode` OETF, and BOTH read the kernel `RgbProfile.Srgb` row's own transfer rather than spelling the curve — a raw-channel pass-through that calls the unlinearized value "scene-linear" and a hand-written piecewise IEC 61966-2-1 body beside the row that publishes the same curve are the two deleted forms; the working-space PRIMARIES conversion stays the `Rasm.Materials` Unicolour owner's concern, never re-derived here; the OpenPBR reconciliation rides the `AppearanceKey` content key — a re-mint of the `Rasm.Materials/Appearance/surface#OPENPBR_SLAB` `OpenPbrSurface` vector, the `surface#CONDUCTOR_IOR` conductor-IOR table, or the OpenPBR slab algebra in this owner is the named cross-folder seam violation; the rich IFC rendering channels (`SpecularColour`/`TransmissionColour`/`ReflectionColour`/`DiffuseTransmissionColour`, the `IfcSurfaceStyleRefraction` IOR/dispersion MAGNITUDE) are NOT retained by the thin seam summary — a Bim-imported style collapses DELIBERATELY to base colour/metalness/roughness/opacity + a transmissive flag (the refraction PRESENCE is kept as the transmissive bit DISTINCT from the opacity/alpha channel so opaque-alpha glass round-trips, its IOR/dispersion magnitude dropped; the seam's chosen shape, lossy by design, NOT an unintended gap), and full specular/reflection/transmission-colour + dispersion BSDF fidelity exists ONLY when the `Rasm.Materials` owner AUTHORS the appearance and holds the lobe graph keyed by the shared `AppearanceKey` (claiming a Bim round-trip preserves the dropped colour/IOR channels is the deleted overclaim); TEXTURES and the `IfcSurfaceStyleLighting` four-colour coefficient set are the FIELD-valued presentation facts this projector retains, and both retain BESIDE the summary, never inside it — `StyledAppearance` pairs the content-keyed node with the `SurfaceTexture` roster and the `Option<SurfaceLighting>` carrier, the lighting egress landing in the `IfcSurfaceStyle` lighting slot, the frozen seven-value `AppearanceSummary` preimage stays sealed (an eighth column re-keys every stored `Node.Appearance` and forks the dedup key this page mints against), and folding a map mean into a scalar channel is the averaged-map defect the roster exists to refuse; the roster row is TYPED at its seam owner and this producer mints it in the seam own vocabulary — `TextureMode.Polarity` is `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `ChannelPolarity` and the `SurfaceTexture` axis pair is that owner `TextureWrap`, so a Bim-local `WrapMode` or invert-bool twin beside them is the deleted form that lets one edit fork two packages that never reference each other; NAMED LOSS on the wrap axis, one-way only — IFC states addressing as a single two-state repeat flag per axis, so `MirroredRepeat` is unreachable from an IFC ingest and reaches the seam row only from the glTF/USD producers it also serves, and the inverse folds it back onto the repeating flag with the mirror dropped AT THE FILE; a texture rows onto the seam when its `CapabilitySet<TextureTrait>` admits `Rowable` (`Classified` + `Addressable`) and a compound case-and-derived-predicate test at each gate is the deleted form the roster filter and this boundary already drifted apart under — the drop of a short texture is DELIBERATE and diagnostic-free because the seam `TextureRoster` row carries no refusal channel, `Missing(Rowable)` naming the shape such a channel takes when the seam grows one; the surviving `bool` columns are seam-fixed and stay: `ReflectanceModel.Transmissive` feeds the frozen `AppearanceSummary.Of` transmissive slot, `StyledAppearance.DoubleSided` is the whole admitted domain after the `NEGATIVE` filter and the `DetailSchema.DoubleSided` bag row it reconciles with, and `IfcInternals.StiffnessFixed` is a GeometryGym field this capsule detaches and never widens; this leaf CLASSIFIES and CARRIES alone — it opens no image, so a texel decode, the `TextureMode` gloss/transparency inversion, and any resampling ride the texture-set owner the seam `TextureRoster` row hands the classifiable roster to, and an `IfcPixelTexture` egresses its declared extent with an empty pixel run because GeometryGym exposes that run only through its constructor; `IfcInternals` is the ONLY place in this branch that names a GeometryGym `internal` member, and it binds through `[UnsafeAccessor]` alone — compile-time, reflection-free, trim- and AOT-safe, and loud at the first call when a release moves a member — pinned to the `Directory.Packages.props` `GeometryGymIFC_Core` version so a bump re-probes every binding; a hand-emitted STEP fragment injected beside the authored database, a reflection or IL-emit path, and a vendored fork are each the deleted form because each mints a second IFC reader or writer inside the package that owns exactly one, and a second copy of any accessor it carries is the divergence this single capsule exists to foreclose; the `ReflectanceModel` keys its `FromIfc` resolution through the `Items`-derived frozen index on the typed `Method` constant — no `switch` over the enum, no `ToString` hop, `NotDefined` the total fallback, and `TextureMode.From` mirrors that admission through the generated `Validate` with `NotDefined` the unresolved row rather than a guessed channel; faults route through the `Fin` rail and lift BARE (the `Fault`-derived `BimFault.Refused` with `BimReason.Rejected` IS the `Error`, never a `.ToError()` hop and never an exception across a domain signature).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;                       // CultureInfo — the invariant parse the baked-set address admission takes
using System.Numerics;                            // Vector2 — the UV offset/scale frame both projector halves carry
using System.Runtime.CompilerServices;            // [UnsafeAccessor] — the pinned GeometryGym internal-member binding
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Domain;
using Rasm.Drawing;                               // EncodingChannel — the seam lane vocabulary the binding table keys on
using Rasm.Element.Graph;
using Rasm.Element.Projection;                    // ContentAddress — the baked-set address the Materials appearance bag carries
using Rasm.Element.Properties;                    // DetailSchema, PropertyName, PropertyValue — the seam-owned appearance-bag vocabulary the Materials-link readers key on
using Rasm.Numerics;                              // RgbProfile + PerceptualColor + RgbTransfer — the kernel colour row, the railed ingress, and the ONE federation byte leg this page's crossing composes
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// Each row carries its metalness, its roughness fallback (read when the style supplies no IfcSpecularHighlight),
// and whether the method is transmissive — the IFC reflectance vocabulary PBR meaning captured once as
// POLICY_VALUES, so import folds the method onto metalness/roughness and ForPbr picks it back at egress.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ReflectanceModel {
    public static readonly ReflectanceModel Blinn      = new("BLINN",      IfcReflectanceMethodEnum.BLINN,      metalness: 0.0, roughnessHint: 0.5,  transmissive: false);
    public static readonly ReflectanceModel Flat       = new("FLAT",       IfcReflectanceMethodEnum.FLAT,       metalness: 0.0, roughnessHint: 1.0,  transmissive: false);
    public static readonly ReflectanceModel Glass      = new("GLASS",      IfcReflectanceMethodEnum.GLASS,      metalness: 0.0, roughnessHint: 0.05, transmissive: true);
    public static readonly ReflectanceModel Matt       = new("MATT",       IfcReflectanceMethodEnum.MATT,       metalness: 0.0, roughnessHint: 1.0,  transmissive: false);
    public static readonly ReflectanceModel Metal      = new("METAL",      IfcReflectanceMethodEnum.METAL,      metalness: 1.0, roughnessHint: 0.3,  transmissive: false);
    public static readonly ReflectanceModel Mirror     = new("MIRROR",     IfcReflectanceMethodEnum.MIRROR,     metalness: 1.0, roughnessHint: 0.0,  transmissive: false);
    public static readonly ReflectanceModel Phong      = new("PHONG",      IfcReflectanceMethodEnum.PHONG,      metalness: 0.0, roughnessHint: 0.4,  transmissive: false);
    public static readonly ReflectanceModel Plastic    = new("PLASTIC",    IfcReflectanceMethodEnum.PLASTIC,    metalness: 0.0, roughnessHint: 0.3,  transmissive: false);
    public static readonly ReflectanceModel Strauss    = new("STRAUSS",    IfcReflectanceMethodEnum.STRAUSS,    metalness: 0.5, roughnessHint: 0.4,  transmissive: false);
    public static readonly ReflectanceModel NotDefined = new("NOTDEFINED", IfcReflectanceMethodEnum.NOTDEFINED, metalness: 0.0, roughnessHint: 0.5,  transmissive: false);

    public IfcReflectanceMethodEnum Method { get; }
    public double Metalness { get; }
    public double RoughnessHint { get; }
    public bool Transmissive { get; }

    // Keys chain through the [SmartEnum<string>] generator this(key) overload, and the row carries its typed
    // IfcReflectanceMethodEnum so ToIfc is a field read, never an Enum.Parse over the key.
    private ReflectanceModel(string key, IfcReflectanceMethodEnum method, double metalness, double roughnessHint, bool transmissive) : this(key) =>
        (Method, Metalness, RoughnessHint, Transmissive) = (method, metalness, roughnessHint, transmissive);

    // Indexing Items on the typed schema constant resolves without the enum-to-string hop — the method value IS the
    // symbol — with NotDefined the total fallback for a future schema member.
    private static readonly Lazy<FrozenDictionary<IfcReflectanceMethodEnum, ReflectanceModel>> ByMethod =
        new(static () => Items.ToFrozenDictionary(static row => row.Method));

    public static ReflectanceModel FromIfc(IfcReflectanceMethodEnum method) =>
        ByMethod.Value.GetValueOrDefault(method, NotDefined);

    public IfcReflectanceMethodEnum ToIfc() => Method;

    // Author emits ONLY this modern subset — a superseded BLINN/PHONG/STRAUSS is import vocabulary the neutral
    // vector absorbs, never re-authored — and an imported MATT/FLAT round-trips MATT via its 1.0 RoughnessHint.
    public static ReflectanceModel ForPbr(double metallic, double roughness, bool transmissive) =>
        transmissive          ? Glass
        : metallic >= 0.5     ? (roughness <= 0.05 ? Mirror : Metal)
        : roughness >= 0.9    ? Matt
        : Plastic;
}

// IFC4 types Mode as a free IfcIdentifier while IFC2x3 constrained it to IfcSurfaceTextureEnum, so the roster spans
// BOTH under one case-insensitive key. Exchange/export#EXPORT_RAIL GltfChannel.From resolves the SAME canonical
// names onto its glTF targets, so an IFC mode token reaches a glTF slot through two rosters over one name and never
// through a call-site correspondence either roster could contradict.
// Polarity TRAVELS on the row because inversion evaluates in the LINEAR domain over decoded texels — this page
// opens no image, so applying it here is unspellable and dropping it is the silent-roughness-fork defect.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class TextureMode {
    public static readonly TextureMode Texture          = new("TEXTURE",          channel: "base_color",          polarity: ChannelPolarity.Direct);
    public static readonly TextureMode Diffuse          = new("DIFFUSE",          channel: "base_color",          polarity: ChannelPolarity.Direct);
    public static readonly TextureMode Specular         = new("SPECULAR",         channel: "specular_color",      polarity: ChannelPolarity.Direct);
    public static readonly TextureMode Reflection       = new("REFLECTION",       channel: "specular_color",      polarity: ChannelPolarity.Direct);
    public static readonly TextureMode Shininess        = new("SHININESS",        channel: "specular_roughness",  polarity: ChannelPolarity.Inverted);
    public static readonly TextureMode Roughness        = new("ROUGHNESS",        channel: "specular_roughness",  polarity: ChannelPolarity.Direct);
    // Packed resolves the glTF-aligned spelling to the ORM pack name, not a channel — the pack roster owns the
    // slot order, so a packed stem never resolves to one of its components.
    public static readonly TextureMode Packed           = new("METALLICROUGHNESS", channel: "orm",                polarity: ChannelPolarity.Direct);
    public static readonly TextureMode Normal           = new("NORMAL",           channel: "geometry_normal",     polarity: ChannelPolarity.Direct);
    public static readonly TextureMode Bump             = new("BUMP",             channel: "height",              polarity: ChannelPolarity.Direct);
    public static readonly TextureMode Occlusion        = new("OCCLUSION",        channel: "occlusion",           polarity: ChannelPolarity.Direct);
    public static readonly TextureMode Opacity          = new("OPACITY",          channel: "geometry_opacity",    polarity: ChannelPolarity.Direct);
    public static readonly TextureMode TransparencyMap  = new("TRANSPARENCYMAP",  channel: "geometry_opacity",    polarity: ChannelPolarity.Inverted);
    public static readonly TextureMode SelfIllumination = new("SELFILLUMINATION", channel: "emission_color",      polarity: ChannelPolarity.Direct);
    public static readonly TextureMode Emissive         = new("EMISSIVE",         channel: "emission_color",      polarity: ChannelPolarity.Direct);
    public static readonly TextureMode NotDefined       = new("NOTDEFINED",       channel: "",                    polarity: ChannelPolarity.Direct);

    public string Channel { get; }
    public ChannelPolarity Polarity { get; }
    public bool Resolved => Channel.Length > 0;

    private TextureMode(string key, string channel, ChannelPolarity polarity) : this(key) => (Channel, Polarity) = (channel, polarity);

    // From admits a raw IFC token in one hop through the generated keyed lookup under the row comparer's own
    // case-insensitivity; an unmatched token is NotDefined, never a fabricated channel — a guessed binding
    // lights the wrong slot with no diagnostic and no consumer can tell it from a correct one.
    public static TextureMode From(string mode) => TryGet(mode, out TextureMode? row) && row is not null ? row : NotDefined;
}

// LocalOrigin is the offset, Scale the uniform scale, and Axis1 the rotated U direction whose atan2 IS the
// rotation. This carrier is not a STRATA_TWIN of the Materials `texture#TEXTURE_UV` `UvFrame` it lowers onto: it
// holds the IfcCartesianTransformationOperator2D decode Materials never sees, and a texture transform is a binding
// fact rather than a Materials set-payload column.
public readonly record struct UvTransform(Vector2 Offset, Vector2 Scale, double Rotation) {
    public static readonly UvTransform Identity = new(Vector2.Zero, Vector2.One, 0d);

    // GeometryGym leaves Axis1/Axis2/LocalOrigin null on an unconstrained operator and Scale 0 when the STEP
    // field was unset, so the absent-axis default is +U and a non-positive scale reads as unity — a raw 0 scale
    // would collapse every texel of a texture whose author simply omitted the optional field.
    public static UvTransform Of(IfcCartesianTransformationOperator2D operatorRef) {
        double scale = operatorRef.Scale > 0d ? operatorRef.Scale : 1d;
        Vector2 offset = Optional(operatorRef.LocalOrigin)
            .Map(static point => new Vector2((float)point.CoordinateX, (float)point.CoordinateY))
            .IfNone(Vector2.Zero);
        (double u, double v) = Optional(operatorRef.Axis1)
            .Map(static axis => (axis.DirectionRatioX, axis.DirectionRatioY))
            .IfNone((1d, 0d));
        return new UvTransform(offset, new Vector2((float)scale, (float)scale), Math.Atan2(v, u));
    }

    // Frame lands the five DOUBLES in the exact column order and sign convention the Materials `UvFrame` declares,
    // so the RosterOf mint lowers in ONE hop and a call site destructuring the Vector2 pair by hand is the
    // divergence this member deletes. Rotation is the SAME angle both sides read: the operator Axis1 atan2 and the
    // sampler forward rotation share one sense, so a round-tripped frame re-samples the texels it started on.
    public (double OffsetU, double OffsetV, double ScaleU, double ScaleV, double Rotation) Frame =>
        (Offset.X, Offset.Y, Scale.X, Scale.Y, Rotation);
}

// TextureTrait is the capability vocabulary a carried texture answers over — the four facts a downstream gate asks
// before it acts, held as ONE combinable column rather than as a case test plus a derived predicate at each site.
[SmartEnum<string>]
public sealed partial class TextureTrait : ICapability<TextureTrait> {
    public static readonly TextureTrait Classified = new("classified");
    public static readonly TextureTrait Addressable = new("addressable");
    public static readonly TextureTrait Embedded = new("embedded");
    public static readonly TextureTrait Transformed = new("transformed");
}

// Cases declaring a same-named column instead of forwarding the root positional suppress the root property and
// silently drops the argument, which is the one shape this family forecloses by construction. An IfcPixelTexture
// own pixel run stays behind a GeometryGym ctor-only field, so the Pixels case declares the grid EXTENT alone.
// CoordinateSet is the ordinal of the representation-item map that CLAIMS this texture — a GEOMETRY-side fact this
// style-side projector cannot see — so `Of` admits the DECLARED set 0 and `At` re-stamps it once
// `Exchange/import#EXPLICIT_TESSELLATION` resolves the join.
[Union]
public abstract partial record SurfaceTexture(TextureMode Mode, TextureWrap WrapU, TextureWrap WrapV, Option<UvTransform> Uv, int CoordinateSet) {
    public sealed record Url(TextureMode Mode, TextureWrap WrapU, TextureWrap WrapV, Option<UvTransform> Uv, int CoordinateSet, string Reference)
        : SurfaceTexture(Mode, WrapU, WrapV, Uv, CoordinateSet);
    public sealed record Blob(TextureMode Mode, TextureWrap WrapU, TextureWrap WrapV, Option<UvTransform> Uv, int CoordinateSet, string RasterFormat, ReadOnlyMemory<byte> Raster)
        : SurfaceTexture(Mode, WrapU, WrapV, Uv, CoordinateSet);
    public sealed record Pixels(TextureMode Mode, TextureWrap WrapU, TextureWrap WrapV, Option<UvTransform> Uv, int CoordinateSet, int Width, int Height, int Components)
        : SurfaceTexture(Mode, WrapU, WrapV, Uv, CoordinateSet);

    // Consumers state a requirement as a VALUE instead of re-spelling the compound case-and-derived-property
    // test each gate grew, so a new payload subtype declares its own traits and every gate holds.
    public CapabilitySet<TextureTrait> Traits =>
        Shared.With(Switch<TextureTrait>(
            url:    static _ => TextureTrait.Addressable,
            blob:   static _ => TextureTrait.Embedded,
            pixels: static _ => TextureTrait.Embedded));

    // Universal columns answer their own traits, so a case row states only what its PAYLOAD adds.
    CapabilitySet<TextureTrait> Shared =>
        (Mode.Resolved, Uv.IsSome) switch {
            (true, true)   => CapabilitySet<TextureTrait>.Of(TextureTrait.Classified, TextureTrait.Transformed),
            (true, false)  => CapabilitySet<TextureTrait>.Of(TextureTrait.Classified),
            (false, true)  => CapabilitySet<TextureTrait>.Of(TextureTrait.Transformed),
            (false, false) => CapabilitySet<TextureTrait>.None,
        };

    // Seam rosters demand BOTH: a channel token the classifier keys on and a reference an app root resolves.
    // Missing(Rowable) names the diagnostic shape the seam TextureRoster row carries no channel for.
    public static readonly CapabilitySet<TextureTrait> Rowable =
        CapabilitySet<TextureTrait>.Of(TextureTrait.Classified, TextureTrait.Addressable);

    // Concrete subtype IS the discriminant, so an unrecognized future subtype is None rather than a lossy Url
    // with an empty reference. GeometryGym spells the URL field UrlReference (never URLReference) and types
    // RasterCode as IfcBinary whose `Binary` member is the raw byte[] — the ValueString hex render is the STEP wire
    // form, so lifting it as text would re-parse bytes the decoder already holds.
    public static Option<SurfaceTexture> Of(IfcSurfaceTexture texture) {
        TextureMode mode = TextureMode.From(texture.Mode);
        Option<UvTransform> uv = Optional(texture.TextureTransform).Map(UvTransform.Of);
        (TextureWrap u, TextureWrap v) = (Wrap(texture.RepeatS), Wrap(texture.RepeatT));
        return texture switch {
            IfcImageTexture image => Some<SurfaceTexture>(new Url(mode, u, v, uv, 0, image.UrlReference)),
            IfcBlobTexture blob   => Some<SurfaceTexture>(new Blob(mode, u, v, uv, 0, blob.RasterFormat, Optional(blob.RasterCode).Map(static code => (ReadOnlyMemory<byte>)code.Binary).IfNone(ReadOnlyMemory<byte>.Empty))),
            IfcPixelTexture grid  => Some<SurfaceTexture>(new Pixels(mode, u, v, uv, 0, grid.Width, grid.Height, grid.ColourComponents)),
            _                     => Option<SurfaceTexture>.None,
        };
    }

    // IFC states addressing as ONE two-state repeat flag per axis, so the seam three-row TextureWrap is WIDER than
    // this ingest and MirroredRepeat is unreachable from an IFC file. NAMED LOSS on the inverse: Repeats folds
    // MirroredRepeat back onto the repeating flag, so a mirrored wrap round-trips as plain repeat.
    static TextureWrap Wrap(bool repeat) => repeat ? TextureWrap.Repeat : TextureWrap.ClampToEdge;

    static bool Repeats(TextureWrap wrap) => wrap != TextureWrap.ClampToEdge;

    // Roots are RECORD `[Union]`s, so `with` regenerates the active case whole — the one expression that re-seats
    // a universal column without a per-case ladder. The composition edge calls it once, folding the
    // `Exchange/import#EXPLICIT_TESSELLATION` map ordinal on before the texture-set owner reads it.
    public SurfaceTexture At(int coordinateSet) => this with { CoordinateSet = coordinateSet };

    // Total over the closed family, so a new case breaks the build. A Pixels case re-authors its EXTENT with an
    // empty pixel run — GeometryGym exposes the run only through the ctor and this page never held the texels — so
    // a pixel-texture round-trip claiming the raster back is the deleted overclaim.
    public IfcSurfaceTexture Author(DatabaseIfc db) {
        IfcSurfaceTexture authored = Switch<IfcSurfaceTexture>(
            url:    u => new IfcImageTexture(db, Repeats(u.WrapU), Repeats(u.WrapV), u.Reference),
            blob:   b => new IfcBlobTexture(db, Repeats(b.WrapU), Repeats(b.WrapV), b.RasterFormat, new IfcBinary(b.Raster.ToArray())),
            pixels: p => new IfcPixelTexture(db, Repeats(p.WrapU), Repeats(p.WrapV), p.Width, p.Height, p.Components, []));
        authored.Mode = Mode.Key;
        Uv.IfSome(uv => authored.TextureTransform = Operator(db, uv));
        // Parameter is the schema ONLY per-texture record of which parameterization a style samples, so writing the
        // canonical U/V variable names round-trips the binding even where a reader never reaches the item.
        authored.Parameter = [$"U{CoordinateSet}", $"V{CoordinateSet}"];
        return authored;
    }

    // IfcCartesianTransformationOperator2D(DatabaseIfc) seeds an origin-anchored operator, so the frame writes
    // through the mutable Axis1/LocalOrigin/Scale members; a uniform Scale is the whole IFC 2D affine axis, so a
    // non-uniform glTF scale narrows to its U component here and the divergence rides the texture-set owner.
    static IfcCartesianTransformationOperator2D Operator(DatabaseIfc db, UvTransform uv) =>
        new(db) {
            LocalOrigin = new IfcCartesianPoint(db, uv.Offset.X, uv.Offset.Y),
            Axis1 = new IfcDirection(db, Math.Cos(uv.Rotation), Math.Sin(uv.Rotation)),
            Scale = uv.Scale.X,
        };
}

// All four attributes are MANDATORY on IfcSurfaceStyleLighting — its own STEP writer dereferences each
// unconditionally — so a body missing one is malformed and reads None rather than lowering a fabricated black
// coefficient a renderer cannot distinguish from a declared one. Channels are SCENE-LINEAR here and
// display-referred sRGB on the wire, the same transfer pair every other colour on this page crosses.
public readonly record struct SurfaceLighting(
    (double R, double G, double B) DiffuseTransmission,
    (double R, double G, double B) DiffuseReflection,
    (double R, double G, double B) Transmission,
    (double R, double G, double B) Reflectance) {
    public static Option<SurfaceLighting> Of(IfcSurfaceStyleLighting lighting) =>
        lighting is { DiffuseTransmissionColour: { } dt, DiffuseReflectionColour: { } dr, TransmissionColour: { } t, ReflectanceColour: { } r }
            ? Some(new SurfaceLighting(
                (AppearanceProjection.Linearize(dt.Red), AppearanceProjection.Linearize(dt.Green), AppearanceProjection.Linearize(dt.Blue)),
                (AppearanceProjection.Linearize(dr.Red), AppearanceProjection.Linearize(dr.Green), AppearanceProjection.Linearize(dr.Blue)),
                (AppearanceProjection.Linearize(t.Red), AppearanceProjection.Linearize(t.Green), AppearanceProjection.Linearize(t.Blue)),
                (AppearanceProjection.Linearize(r.Red), AppearanceProjection.Linearize(r.Green), AppearanceProjection.Linearize(r.Blue))))
            : Option<SurfaceLighting>.None;

    // Four-colour ctor is the entity ONLY public author and it resolves its database from the first colour, so
    // every triple encodes back through the sRGB OETF and the four IfcColourRgb entities are built here in order.
    public IfcSurfaceStyleLighting Author(DatabaseIfc db) =>
        new(Colour(db, DiffuseTransmission), Colour(db, DiffuseReflection), Colour(db, Transmission), Colour(db, Reflectance));

    static IfcColourRgb Colour(DatabaseIfc db, (double R, double G, double B) c) =>
        new(db, AppearanceProjection.Encode(c.R), AppearanceProjection.Encode(c.G), AppearanceProjection.Encode(c.B));
}

// StyledAppearance is the INGEST product: the content-keyed seam node with the three facts frozen AppearanceSummary
// cannot carry, each riding BESIDE it because widening that preimage re-keys every stored Node.Appearance.
// DoubleSided is the discriminant the ingest filter already computes and threw away: `Side` distinguishes BOTH from
// POSITIVE and the filter admitted both alike, so a single-sided IFC style imported and re-authored came back
// double-sided and its glTF material rendered its interior faces. Three consumers read the bit — the egress `Side`
// write, the glTF `MaterialBuilder.WithDoubleSide` stamp, and the `DetailSchema.DoubleSided` bag row `DoubleSidedOf`
// answers from — so none re-derives sidedness from a geometry probe.
public readonly record struct StyledAppearance(Node.Appearance Appearance, Seq<SurfaceTexture> Textures, Option<SurfaceLighting> Lighting, bool DoubleSided);

// Rows are TYPED at the seam and this mint fills them in the seam own vocabulary, the UvTransform lowered onto the
// neutral frame columns AT THIS MINT so no IfcCartesianTransformationOperator2D decode crosses. An Embedded
// Blob/Pixels payload stays on StyledAppearance for the root own byte admission.
public static TextureRoster RosterOf(StyledAppearance styled) =>
    new(styled.Appearance.Id, styled.Textures.Choose(static texture =>
        texture.Traits.AdmitsAll(SurfaceTexture.Rowable) && texture is SurfaceTexture.Url url
            ? Some(Candidate(url))
            : Option<TextureCandidate>.None));

static TextureCandidate Candidate(SurfaceTexture.Url url) {
    (double offsetU, double offsetV, double scaleU, double scaleV, double rotation) = url.Uv.IfNone(UvTransform.Identity).Frame;
    return new TextureCandidate(
        url.Mode.Channel, url.Reference, url.Mode.Polarity,
        url.WrapU, url.WrapV, url.CoordinateSet,
        offsetU, offsetV, scaleU, scaleV, rotation);
}

// Index runs are ONE-BASED with one entry per FACE and one Alpha the schema applies to every face alike. The seam
// carries colour PER VERTEX and IFC has no per-vertex form, so the correspondence is stated once here: the ingest
// broadcasts a face colour onto that face own corners and the fold reads a face FIRST corner back — exact for the
// unwelded emit a colour map already forces, per-face by the schema own limit for a welded gradient.
public readonly record struct IndexedColour(Seq<(double R, double G, double B)> Palette, Seq<int> Face, double Alpha) {
    // Of reads the map: MappedTo/Colours/Opacity resolve the slot publicly and the two payload runs cross IfcInternals.
    // Opacity is GeometryGym's NaN unset sentinel, which the schema defines as fully opaque.
    public static Option<IndexedColour> Of(IfcTessellatedFaceSet faceSet) =>
        Optional(faceSet.HasColours)
            .Bind(static map => Optional(map.Colours).Map(list => new IndexedColour(
                IfcInternals.Palette(list).Map(static triple => (
                    AppearanceProjection.Linearize(triple.R), AppearanceProjection.Linearize(triple.G), AppearanceProjection.Linearize(triple.B))),
                IfcInternals.ColourRun(map),
                double.IsNaN(map.Opacity) ? 1.0 : Math.Clamp(map.Opacity, 0.0, 1.0))))
            .Filter(static colour => !colour.Palette.IsEmpty && !colour.Face.IsEmpty);

    // Memo keys on the SAME AppearanceProjection.Bytes quantizer that writes the palette out, so two triangles
    // collapse EXACTLY when they will emit identical bytes; a SECOND quantizer on a different curve let a highlight
    // pair the egress collapses hold two rows. A failed byte projection keys on its own row so the palette never
    // merges values the egress could not measure. Alpha takes the first face: IFC publishes ONE opacity slot per
    // map, so per-face varying alpha collapses by the schema limit.
    public static Option<IndexedColour> Of(float[] lane, long[] corners, Op key) {
        var memo = new Dictionary<(byte R, byte G, byte B), int>();
        var run = new List<int>(corners.Length / 3);
        var palette = new List<(double R, double G, double B)>();
        for (int t = 0; t < corners.Length / 3; t++) {
            int at = (int)corners[t * 3] * 4;
            Option<(byte R, byte G, byte B)> encoded = AppearanceProjection
                .Bytes(lane[at], lane[at + 1], lane[at + 2], 1.0, key)
                .ToOption().Map(static rgba => (rgba.Red, rgba.Green, rgba.Blue));
            int row = encoded.Match(
                Some: k => memo.TryGetValue(k, out int seated) ? seated : Seat(memo, palette, k, lane, at),
                None: () => Seat(palette, lane, at));
            run.Add(row);
        }
        return palette.Count > 0
            ? Some(new IndexedColour(toSeq(palette), toSeq(run), lane[((int)corners[0] * 4) + 3]))
            : Option<IndexedColour>.None;
    }

    static int Seat(Dictionary<(byte R, byte G, byte B), int> memo, List<(double R, double G, double B)> palette,
                    (byte R, byte G, byte B) key, float[] lane, int at) {
        int row = Seat(palette, lane, at);
        memo[key] = row;
        return row;
    }

    static int Seat(List<(double R, double G, double B)> palette, float[] lane, int at) {
        palette.Add((lane[at], lane[at + 1], lane[at + 2]));
        return palette.Count;
    }

    // Rgba resolves one FACE's colour for the per-vertex lane: the run's one-based ordinal indexes the palette and the
    // map's single Opacity is the alpha every corner shares. An ordinal past the palette or a run shorter than the face
    // count is a malformed file and throws inside the caller's own Op.Catch envelope.
    public (double R, double G, double B, double A) Rgba(int face) =>
        Palette[Face[face] - 1] switch { var (r, g, b) => (r, g, b, Alpha) };

    // IfcColourRgbList own ctor divides each channel by 255 and IfcIndexedColourMap three-argument ctor
    // self-registers through its MappedTo setter, so this writes through PUBLIC surface alone. The rail is real
    // admission: the palette holds RAW decoded file floats, so a NaN or out-of-range channel faults typed here
    // instead of silently clamping into a byte that renders. System.Drawing.Color is spelled HERE and nowhere else
    // and stays fully qualified — a construction argument inside a boundary owner because that ctor is the list
    // ONLY public author, never a type crossing a projector signature.
    public Fin<IfcIndexedColourMap> Author(IfcTessellatedFaceSet faceSet, Op key) =>
        Palette
            .TraverseM(c => AppearanceProjection.Bytes(c.R, c.G, c.B, Alpha, key)
                .Map(static b => System.Drawing.Color.FromArgb(b.Red, b.Green, b.Blue)))
            .As()
            .Map(colors => new IfcIndexedColourMap(faceSet, new IfcColourRgbList(faceSet.Database, colors), Face) { Opacity = Alpha });

}

// --- [BOUNDARIES] -------------------------------------------------------------------------
// Every binding is an [UnsafeAccessor] extern the COMPILER resolves — no reflection, no IL emit, trim-safe and
// AOT-safe — and a release renaming a member fails at the FIRST call with MissingFieldException/
// MissingMethodException, so drift is a caught break rather than a silent wrong render. It is PINNED to the
// Directory.Packages.props GeometryGymIFC_Core version and a bump re-probes it. Callers receive DETACHED VALUES:
// a palette and an index run lift to immutable Seq before returning, so no live List<T> field escapes and the
// Tuple<> shapes GeometryGym stores those fields as are named here and nowhere else.
internal static class IfcInternals {
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mColourList")]
    static extern ref List<Tuple<double, double, double>> ColourList(IfcColourRgbList list);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mColourIndex")]
    static extern ref List<int> ColourIndex(IfcIndexedColourMap map);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    static extern IfcIndexedTriangleTextureMap Mint(DatabaseIfc db);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mTexCoordList")]
    static extern ref List<Tuple<int, int, int>> TexCoordList(IfcIndexedTriangleTextureMap map);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mWarpingStiffness")]
    static extern ref IfcWarpingStiffnessSelect WarpingStiffness(IfcBoundaryNodeConditionWarping condition);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mFixed")]
    static extern ref bool StiffnessFixed(IfcWarpingStiffnessSelect select);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mStiffness")]
    static extern ref double StiffnessValue(IfcWarpingStiffnessSelect select);

    // IfcColourRgbList stores its triples ALREADY on the unit interval — its own Color ctor divides each channel by
    // 255 on the way in — so the palette crosses UNSCALED and a second /255 here would black every ingested colour.
    public static Seq<(double R, double G, double B)> Palette(IfcColourRgbList list) =>
        toSeq(ColourList(list)).Map(static triple => (triple.Item1, triple.Item2, triple.Item3));

    // ColourIndex is ONE-BASED with one entry per FACE, and the run crosses exactly as the schema spells it — the
    // decrement to a palette ordinal belongs to the fold that indexes, never to a reader that would hide the origin.
    public static Seq<int> ColourRun(IfcIndexedColourMap map) => toSeq(ColourIndex(map));

    // TexCoordIndex is one triple per TRIANGLE, each ordinate a one-based index into the map's TexCoords list, so a
    // triangulated face set carries a per-CORNER parameterization the per-coordinate form cannot express.
    public static Seq<(int A, int B, int C)> TexCoordRun(IfcIndexedTriangleTextureMap map) =>
        toSeq(TexCoordList(map)).Map(static triple => (triple.Item1, triple.Item2, triple.Item3));

    // READ projection only: IfcBoundaryNodeConditionWarping seals mWarpingStiffness AND every
    // IfcWarpingStiffnessSelect constructor is internal, so the select cannot be built from outside the assembly at
    // all — its public 9-argument condition constructor demands a value no caller can construct. A null field (a
    // base IfcBoundaryNodeCondition parsed under the warping subtype, or an unset optional) reads None rather than
    // a fabricated free end. The magnitude is NATIVE-unit: coercion belongs to the structural reader that threads a
    // UnitScheme, never to this capsule, which detaches values and converts nothing.
    public static Option<(bool Rigid, double Native)> Warping(IfcBoundaryNodeConditionWarping condition) =>
        WarpingStiffness(condition) is { } select
            ? Some((StiffnessFixed(select), StiffnessValue(select)))
            : Option<(bool, double)>.None;

    // Maps is a read-only LIST mutated through AddRange, TexCoords a public setter, and MappedTo the setter that
    // SELF-REGISTERS the map into the face set HasTextures — so it lands LAST and the face set never observes a
    // half-filled map. IfcTextureVertexList(DatabaseIfc, IEnumerable<Tuple<double, double>>) is public, so only the
    // mint and the triple list ride accessors.
    public static IfcIndexedTriangleTextureMap Bind(
        IfcTriangulatedFaceSet faceSet, Seq<IfcSurfaceTexture> textures,
        Seq<(double U, double V)> coordinates, Seq<(int A, int B, int C)> triples) {
        IfcIndexedTriangleTextureMap map = Mint(faceSet.Database);
        map.Maps.AddRange(textures);
        map.TexCoords = new IfcTextureVertexList(faceSet.Database, coordinates.Map(static uv => Tuple.Create(uv.U, uv.V)));
        TexCoordList(map).AddRange(triples.Map(static triple => Tuple.Create(triple.A, triple.B, triple.C)));
        map.MappedTo = faceSet;
        return map;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Rasm.Element OWNS the AppearanceSummary PBR vocabulary and its content-key derivation; this projector
// discriminates the IFC presentation graph and folds it onto the neutral vector, never re-minting the Materials OpenPBR
// vector, conductor-IOR table, or slab algebra (the named cross-folder seam violation).
public static class AppearanceProjection {
    // DefaultRefractionIndex is the egress optical index for a transmissive style whose summary carries no IOR (the thin seam
    // drops the magnitude — a Rasm.Materials BSDF concern): crown-glass 1.5, the conventional neutral.
    const double DefaultRefractionIndex = 1.5;

    // BOTH and POSITIVE are NOT interchangeable, so reading `Side` as a predicate and dropping its value is the
    // discarded-discriminant defect: the projector already touched the one attribute that answers sidedness.
    // NEGATIVE styles paint the back face alone and stay filtered out — admitting one hands an element the colour
    // of a face no viewer shows.
    public static Fin<StyledAppearance> Project(IfcStyledItem styledItem, double tolerance, Op key) =>
        styledItem.Extract<IfcSurfaceStyle>().AsIterable()
            .Filter(static surface => surface.Side is IfcSurfaceSide.BOTH or IfcSurfaceSide.POSITIVE)
            .Head
            .ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "surface-slot-miss", "style", styledItem.StepId.ToString(CultureInfo.InvariantCulture) })))
            .Bind(surface => SummaryOf(surface, key).Map(summary =>
                new StyledAppearance(Mint(summary, tolerance), TexturesOf(surface), LightingOf(surface), surface.Side is IfcSurfaceSide.BOTH)));

    // Styles declare at most one lighting item, so the first is the whole read; a body missing one of the four
    // mandatory colours reads None through SurfaceLighting.Of.
    static Option<SurfaceLighting> LightingOf(IfcSurfaceStyle surface) =>
        toSeq(surface.Styles)
            .Choose(static style => style is IfcSurfaceStyleLighting lighting ? Some(lighting) : Option<IfcSurfaceStyleLighting>.None)
            .Head
            .Bind(SurfaceLighting.Of);

    // Multiple texture styles on one surface concatenate in Styles order, and an
    // unclaimed Mode rides its NotDefined row so the seam projection rows nothing for it rather than
    // binding a guessed channel.
    static Seq<SurfaceTexture> TexturesOf(IfcSurfaceStyle surface) =>
        toSeq(surface.Styles)
            .Choose(static style => style is IfcSurfaceStyleWithTextures textured ? Some(textured) : Option<IfcSurfaceStyleWithTextures>.None)
            .Bind(static textured => toSeq(textured.Textures))
            .Choose(SurfaceTexture.Of);

    // Walking OWNER-WARD is the only shape the edge algebra admits: an Object ASSOCIATES the
    // appearance (Relating the Object, Related the appearance — an Appearance may never relate an edge), and the
    // Materials bag ASSIGNS off that same Object under PropertyDefinition, so the reader steps appearance -> owning
    // Object(s) -> Assigned Appearance-set bag through the uniform Kind/Relating/Related accessors. Every owner of
    // one content-keyed appearance carries the same refinement bag, so first evidence answers, and absence is the
    // ordinary case an IFC-imported appearance takes.
    static Option<PropertyValue> AppearanceRow(ElementGraph graph, NodeId appearance, PropertyName row) =>
        toSeq(graph.EdgesAt(appearance))
            .Filter(edge => edge.Kind == RelationshipKind.Associate && edge.Related == appearance)
            .Choose(edge => toSeq(graph.EdgesAt(edge.Relating))
                .Filter(assign => assign.Kind == RelationshipKind.Assign && assign.Relating == edge.Relating)
                .Choose(assign => graph.Find(assign.Related))
                .Choose(node => node is Node.PropertySet set && set.Bag.SetName == DetailSchema.Appearance.SetName
                    ? set.Bag.Find(row)
                    : Option<PropertyValue>.None)
                .Head)
            .Head;

    public static Option<ContentAddress> TextureSetOf(ElementGraph graph, NodeId appearance) =>
        AppearanceRow(graph, appearance, DetailSchema.TextureSet)
            .Bind(static value => value is PropertyValue.Text text
                && ContentAddress.Validate(text.Value, CultureInfo.InvariantCulture, out ContentAddress? admitted) is null
                    ? Optional(admitted)
                    : None);

    // PRECEDENCE is by ORIGIN, not by rank: an appearance this projector minted carries the IfcSurfaceSide bit on
    // its own StyledAppearance and stays authoritative for that node, while the bag row answers for a
    // Materials-authored appearance no IFC style ever described, so the two never contend over one node. Option is
    // an honest return: defaulting an absent row to false asserts single-sided over a producer that wrote nothing.
    public static Option<bool> DoubleSidedOf(ElementGraph graph, NodeId appearance) =>
        AppearanceRow(graph, appearance, DetailSchema.DoubleSided)
            .Bind(static value => value is PropertyValue.Boolean flag ? Some(flag.Value) : Option<bool>.None);

    // IfcSurfaceStyleRendering is itself an IfcSurfaceStyleShading, so it supplies the colour/transparency base
    // beside the reflectance method and specular highlight; a bare shading supplies colour/transparency alone.
    static Fin<AppearanceSummary> SummaryOf(IfcSurfaceStyle surface, Op key) {
        // First<IfcSurfaceStyleShading> resolves the rendering as the shading fallback when no bare shading is present.
        Option<T> First<T>() where T : class =>
            surface.Styles.AsIterable().Choose(static s => s is T t ? Some(t) : Option<T>.None).Head;
        Option<IfcSurfaceStyleRendering> rendering = First<IfcSurfaceStyleRendering>();
        Option<IfcSurfaceStyleShading> shading = First<IfcSurfaceStyleShading>();
        Option<IfcSurfaceStyleRefraction> refraction = First<IfcSurfaceStyleRefraction>();

        // SurfaceColour is MANDATORY on IfcSurfaceStyleShading, and a substituted mid-grey does not degrade
        // gracefully because it enters the FROZEN AppearanceSummary preimage: a truncated style and a genuine
        // 0.5-linear grey would mint ONE AppearanceKey and dedup together across the whole content space.
        return shading.Bind(static sh => Optional(sh.SurfaceColour)).Map(Lin)
            .ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "surface-slot-miss", "colour", surface.StepId.ToString(CultureInfo.InvariantCulture) })))
            .Bind(surfaceBase => Neutral(rendering, shading, refraction, surfaceBase, key));
    }

    // Past admission, every remaining PBR channel reads off the three style selects and the admitted base.
    static Fin<AppearanceSummary> Neutral(
        Option<IfcSurfaceStyleRendering> rendering, Option<IfcSurfaceStyleShading> shading,
        Option<IfcSurfaceStyleRefraction> refraction, (double R, double G, double B) surfaceBase, Op key) {
        // IfcColourOrFactor is a TWO-arm select: an IfcColourRgb REPLACES the surface colour, an
        // IfcNormalisedRatioMeasure SCALES it — reflectance is linear-domain energy, so the ratio (GG-clamped
        // [0,1] at its ctor) multiplies the linearized triple; the `as IfcColourRgb` cast that silently ignored the
        // factor arm is the deleted form.
        (double R, double G, double B) baseColor = rendering.Bind(static r => Optional(r.DiffuseColour)).Map(diffuse => diffuse switch {
            IfcColourRgb rgb                => Lin(rgb),
            IfcNormalisedRatioMeasure ratio => (surfaceBase.R * ratio.Measure, surfaceBase.G * ratio.Measure, surfaceBase.B * ratio.Measure),
            _                               => surfaceBase,
        }).IfNone(surfaceBase);

        ReflectanceModel reflectance = rendering.Map(static r => ReflectanceModel.FromIfc(r.ReflectanceMethod)).IfNone(ReflectanceModel.NotDefined);
        double opacity = shading.Map(static sh => double.IsNaN(sh.Transparency) ? 1.0 : 1.0 - Math.Clamp(sh.Transparency, 0.0, 1.0)).IfNone(1.0);
        double roughness = rendering.Bind(static r => RoughnessOf(r.SpecularHighlight)).IfNone(reflectance.RoughnessHint);
        // IfcSurfaceStyleShading.Transparency IS the alpha/opacity channel carried by `opacity`, physically distinct
        // from transmission — conflating the two is the deleted form, since a half-alpha plastic is not glass.
        bool transmissive = reflectance.Transmissive || refraction.IsSome;

        // AppearanceSummary.Of owns the whole key derivation and its writer runs at raw IEEE bits because PBR
        // scalars are not Header-quantized measures, so no tolerance argument exists here to fork the shared key.
        return AppearanceSummary.Of(baseColor.R, baseColor.G, baseColor.B, reflectance.Metalness, Math.Clamp(roughness, 0.0, 1.0), opacity, transmissive, key);
    }

    // IfcSpecularRoughness is a [0,1] roughness read directly; IfcSpecularExponent is a Phong exponent converted
    // through the standard alpha = sqrt(2/(n+2)).
    static Option<double> RoughnessOf(IfcSpecularHighlightSelect? highlight) => highlight switch {
        IfcSpecularRoughness r => Some(Math.Clamp(r.SpecularRoughness, 0.0, 1.0)),
        IfcSpecularExponent e  => Some(Math.Clamp(Math.Sqrt(2.0 / (Math.Max(0.0, e.SpecularExponent) + 2.0)), 0.0, 1.0)),
        _                      => Option<double>.None,
    };

    // Node cases are class-root [Union] arms generating no `with`, so the content id re-stamps on the seam
    // Node.Relabel — the SAME Mint the Rasm.Materials ComponentProjector composes, so two structurally-identical
    // appearances dedup to one node. `tolerance` threads because the seam `ToCanonicalBytes(tolerance)` arity
    // demands it: the appearance ARM writes only the raw-bit AppearanceKey, so it cannot fork this node identity.
    static Node.Appearance Mint(AppearanceSummary summary, double tolerance) {
        Node.Appearance draft = new(NodeId.Of(new NodeSeed.Placement()), summary);
        return (Node.Appearance)draft.Relabel(NodeId.Of(new NodeSeed.Content(draft, tolerance)));
    }

    static (double R, double G, double B) Lin(IfcColourRgb c) => (Linearize(c.Red), Linearize(c.Green), Linearize(c.Blue));

    // --- [EGRESS] -------------------------------------------------------------------------
    // Neutral summaries carry no IOR, so a transmissive appearance authors its IfcSurfaceStyleRefraction at
    // DefaultRefractionIndex and only the transmissive SIGNAL round-trips — the precise index and dispersion are a
    // Rasm.Materials BSDF concern the thin summary deliberately drops.
    // ONE ctor spelling, always the 5-arg overload: transmission and texturing are INDEPENDENT axes, so a ternary
    // ladder grows an arm per combination while the 5-arg form takes each axis as a null-or-value slot and stays one
    // expression as the axes grow. GeometryGym resolves the ctor DatabaseIfc from the first non-null slot, so the
    // rendering leads and every other slot may be null. `doubleSided` arrives REQUIRED beside the roster because the
    // caller holding a roster holds the StyledAppearance that carries both: a defaulted slot re-mints the hardcoded
    // BOTH that re-authored every single-sided style as two-sided, which neither a round-trip nor a diff detects.
    public static IfcStyledItem Author(DatabaseIfc db, AppearanceSummary summary, Seq<SurfaceTexture> textures, Option<SurfaceLighting> lighting, bool doubleSided) {
        IfcColourRgb surfaceColour = new(db, Encode(summary.BaseColorR), Encode(summary.BaseColorG), Encode(summary.BaseColorB));
        bool transmissive = summary.Transmissive;  // the refractive signal, DISTINCT from the opacity/alpha channel
        ReflectanceModel reflectance = ReflectanceModel.ForPbr(summary.Metallic, summary.Roughness, transmissive);
        IfcColourRgb specular = summary.Metallic >= 0.5 ? surfaceColour : new IfcColourRgb(db, 1.0, 1.0, 1.0);
        IfcSurfaceStyleRendering rendering = new(surfaceColour) {
            Transparency = 1.0 - summary.Opacity,
            ReflectanceMethod = reflectance.ToIfc(),
            DiffuseColour = surfaceColour,
            SpecularColour = specular,
            SpecularHighlight = new IfcSpecularRoughness(summary.Roughness),
        };
        IfcSurfaceStyle style = new(
            rendering,
            // Appearances carrying no lighting leave the slot null rather than authoring a fabricated black
            // lighting model a receiving renderer would apply.
            lighting.Match(Some: row => row.Author(db), None: static () => (IfcSurfaceStyleLighting?)null),
            // IfcSurfaceStyleWithTextures admits only a NON-EMPTY list (its ctor reads textures[0] for the database).
            textures.IsEmpty ? null : new IfcSurfaceStyleWithTextures([.. textures.Map(texture => texture.Author(db))]),
            null,
            transmissive ? new IfcSurfaceStyleRefraction(db) { RefractionIndex = DefaultRefractionIndex } : null) {
            Side = doubleSided ? IfcSurfaceSide.BOTH : IfcSurfaceSide.POSITIVE,
        };
        return new IfcStyledItem(style);
    }

    // Bind is a second call rather than a column on Author because every one of these payloads binds to the
    // REPRESENTATION ITEM: a surface style knows its texture bytes and nothing about the mesh that samples them. The
    // receipt is the bound pairs, so the egress reads which lanes survived onto the file instead of inferring it
    // from the roster it passed in. The colour arm palette holds RAW decoded floats whose kernel-quantizer
    // admission can refuse, so Bind rides Fin.
    public static Fin<Seq<(EncodingChannel Channel, IfcPresentationItem Item)>> Bind(
        IfcStyledItem styled, IfcTriangulatedFaceSet faceSet, Seq<(EncodingChannel Channel, float[] Lane)> attributes, long[] corners, Op key) =>
        // Corner MAXIMUM folds ONCE in the guard that already walks the run shape and threads into every lane
        // arity check — a per-lane Max re-scanned the whole run once per bound channel for an unchanging number.
        corners.Length >= 3 && corners.Length % 3 == 0
            ? corners.Max() switch { var reach => attributes
                .TraverseM(entry => Optional(Binders.Value.GetValueOrDefault(entry.Channel)).Match(
                    Some: bind => bind(styled, faceSet, entry.Lane, corners, reach, key).Map(item => item.Map(bound => (entry.Channel, Item: bound))),
                    None: () => Fin.Succ(Option<(EncodingChannel Channel, IfcPresentationItem Item)>.None)))
                .As()
                .Map(static bound => bound.Somes()) }
            : Fin.Succ(Seq<(EncodingChannel Channel, IfcPresentationItem Item)>.Empty);

    // Rows are frozen once because the channel vocabulary is closed at composition; every row rides the
    // Fin-in-Option shape — Fin the admission rail, Option the ordinary lane-binds-nothing case.
    static readonly Lazy<FrozenDictionary<EncodingChannel, Func<IfcStyledItem, IfcTriangulatedFaceSet, float[], long[], long, Op, Fin<Option<IfcPresentationItem>>>>> Binders =
        new(static () => new Dictionary<EncodingChannel, Func<IfcStyledItem, IfcTriangulatedFaceSet, float[], long[], long, Op, Fin<Option<IfcPresentationItem>>>> {
            [EncodingChannel.Uv] = static (styled, faceSet, lane, corners, reach, _) => Fin.Succ(Mapped(styled, faceSet, lane, corners, reach).Map(static map => (IfcPresentationItem)map)),
            [EncodingChannel.ColorRgba] = static (_, faceSet, lane, corners, reach, key) => Coloured(faceSet, lane, corners, reach, key).Map(static map => map.Map(static item => (IfcPresentationItem)item)),
        }.ToFrozenDictionary());

    // Rasm-authored meshes are per-VERTEX parameterized, so the corner run IS the UV index run and the vertex list
    // is the lane in vertex order — no re-index and no vertex split. IfcSurfaceTexture rows come back off the
    // styled item through the same BaseClassIfc.Extract<T> traversal the ingest reads, so no roster threads twice.
    static Option<IfcIndexedTriangleTextureMap> Mapped(IfcStyledItem styled, IfcTriangulatedFaceSet faceSet, float[] lane, long[] corners, long reach) =>
        toSeq(styled.Extract<IfcSurfaceTexture>()) is { IsEmpty: false } textures && Indexable(lane, reach, arity: 2)
            ? Some(IfcInternals.Bind(faceSet, textures, Pairs(lane), Triples(corners)))
            : Option<IfcIndexedTriangleTextureMap>.None;

    static Fin<Option<IfcIndexedColourMap>> Coloured(IfcTriangulatedFaceSet faceSet, float[] lane, long[] corners, long reach, Op key) =>
        Indexable(lane, reach, arity: 4)
            ? IndexedColour.Of(lane, corners, key).Match(
                Some: colour => colour.Author(faceSet, key).Map(Some),
                None: static () => Fin.Succ(Option<IfcIndexedColourMap>.None))
            : Fin.Succ(Option<IfcIndexedColourMap>.None);

    // Every corner the run names must index a whole tuple inside the lane: a lane shorter than its own index run is
    // a producer defect, and binding it authors a map whose indices walk off the end in the RECEIVING application.
    static bool Indexable(float[] lane, long reach, int arity) =>
        lane.Length >= arity && reach < lane.Length / arity;

    // IFC index attributes are ONE-BASED, so every corner ordinal increments exactly here — the one site in this
    // page that knows the schema index origin.
    static Seq<(double U, double V)> Pairs(float[] lane) =>
        toSeq(Enumerable.Range(0, lane.Length / 2).Select(v => ((double)lane[v * 2], (double)lane[(v * 2) + 1])));

    static Seq<(int A, int B, int C)> Triples(long[] corners) =>
        toSeq(Enumerable.Range(0, corners.Length / 3)
            .Select(t => ((int)corners[t * 3] + 1, (int)corners[(t * 3) + 1] + 1, (int)corners[(t * 3) + 2] + 1)));

    // Numerics/atoms#SCALAR_FLOOR RgbProfile.Srgb carries the working-space Configuration whose own Rgb transfer
    // pair IS the IEC 61966-2-1 curve, so this package holds the estate one sRGB spelling by READING the row — a
    // hand-written piecewise body clamped to [0,1] flattened the negative-input reflection the row models, so an
    // extended-range channel silently read zero. Both stay TOTAL and per-channel because the unit-range gate is the
    // seam factory own at AppearanceSummary.Of; ALPHA never takes the curve, coverage being linear by definition.
    public static double Linearize(double channel) =>
        RgbProfile.Srgb.Configuration.Rgb.ToLinear(channel, RgbProfile.Srgb.Configuration.DynamicRange);

    public static double Encode(double channel) =>
        RgbProfile.Srgb.Configuration.Rgb.FromLinear(channel, RgbProfile.Srgb.Configuration.DynamicRange);

    // Bytes IS the kernel federation quantizer: raw file floats fault typed on a non-finite or out-of-range channel
    // and quantize through PerceptualColor.ToRgb, whose byte leg is the content-key quantizer the federation
    // addresses against — so an IFC palette byte, a dotbim colour byte, and a content-key byte agree bit for bit by
    // CONSTRUCTION. Decode is its inverse, the dotbim import lane typed re-entry.
    public static Fin<(byte Red, byte Green, byte Blue, byte Alpha)> Bytes(double r, double g, double b, double alpha, Op key) =>
        PerceptualColor.OfRgb(r, g, b, RgbProfile.Srgb, alpha, key).Map(static colour => colour.ToRgb());

    public static (double R, double G, double B, double A) Decode(byte red, byte green, byte blue, byte alpha) =>
        PerceptualColor.Of(red, green, blue, alpha / 255.0).ToRgb(RgbProfile.Srgb, transfer: RgbTransfer.Linear);
}
```

## [03]-[RESEARCH]

(none)
