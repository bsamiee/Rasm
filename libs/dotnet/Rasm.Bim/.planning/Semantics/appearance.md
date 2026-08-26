# [BIM_APPEARANCE]

`AppearanceProjection` is the IFC surface-style PROJECTOR lowering the live GeometryGym `IfcStyledItem`/`IfcSurfaceStyle` presentation graph onto the `Rasm.Element` shared `Graph/element#NODE_MODEL` `Node.Appearance` carrying the neutral `AppearanceSummary`: `AppearanceProjection.Project` extracts the front-face `IfcSurfaceStyle`, folds its `IfcSurfaceStyleRendering`/`IfcSurfaceStyleShading`/`IfcSurfaceStyleRefraction` element selects onto one neutral PBR vector (scene-linear base colour + metalness + roughness + opacity + a `Transmissive` refractive flag DISTINCT from opacity, so an opaque-alpha glass still carries transmission), classifies the `IfcSurfaceStyleWithTextures` select onto the `SurfaceTexture` roster, and mints the content-keyed shared `Node.Appearance` the `Bake` fold reads through the `Relations/relation#EDGE_ALGEBRA` `Associate` edge into `element.Appearance`. `Rasm.Element` OWNS the `AppearanceSummary` PBR vocabulary and the `Projection/address#CONTENT_ADDRESS` content-key derivation at that boundary; this page owns ONLY the GeometryGym discrimination that fills it, the `ReflectanceModel` `[SmartEnum<string>]` IFC reflectance-method roster, and the `TextureMode`/`SurfaceTexture`/`UvTransform` IFC texture vocabulary, never re-declaring an appearance record. `AppearanceProjection` runs BIDIRECTIONAL: `AppearanceProjection.Author` is the inverse half — re-authoring the `IfcSurfaceStyleRendering` (diffuse/specular/highlight/reflectance-method/transparency), an `IfcSurfaceStyleWithTextures` for a textured appearance, and an `IfcSurfaceStyleRefraction` for a transmissive one, all combined in ONE `IfcSurfaceStyle` through the five-slot constructor, with `AppearanceProjection.Bind` landing the mesh's own shared attribute lanes onto the authored representation item as an `IfcIndexedTriangleTextureMap` and an `IfcIndexedColourMap`, so the surface style, its texture coordinates, and its per-face radiometry all round-trip; both egress entries are COMPLETE and ARM on a body-representation author joining the IFC emit path — `Projection/egress#IFC_EGRESS` `Emit` re-authors semantics alone and geometry egress rides the glTF/3dm deliverables, so the pair's standing consumer is the ingest round trip and the styled-item bind fires the moment a face set is authored on that path.

`StyledAppearance` carries the `SurfaceTexture` roster BESIDE the summary on the ingest product and projected through `RosterOf` onto the Element `TextureRoster` shared row, because a texture is a FIELD the frozen seven-value `AppearanceSummary` preimage structurally cannot carry: folding a map into a scalar is the averaged-map defect, and an eighth summary column re-keys every stored `Node.Appearance` and forks this projector's own cross-folder dedup key in one edit. Each `SurfaceTexture` names its canonical channel through its `TextureMode` row and carries the gloss/transparency polarity IFC declares and the channel name does not, so the texture-set owner binds and inverts decoded texels this host-neutral leaf never opens. `IfcSurfaceSide` sidedness rides beside them for a second reason the same preimage law covers: it is a render-representation toggle selecting WHICH faces a style paints, where every summary channel answers how a painted face reflects — so it round-trips through `Side` on egress and reaches the glTF path as `Exchange/export#EXPORT_PIPELINE` `MaterialFinish.DoubleSided` rather than as an eighth scalar no BSDF reads.

IFC presentation colours are display-referred sRGB; the projector lowers each channel to scene-linear through the sRGB EOTF read off the kernel `Numerics/atoms#SCALAR_FLOOR` `RgbProfile.Srgb` row (a pure host-neutral transfer, IEC 61966-2-1, held by the row every `PerceptualColor` crossing reads and copied nowhere) so the shared `BaseColor` aligns with the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` scene-linear convention, and the egress encodes back through the inverse OETF — the working-space PRIMARIES conversion (Rec.709→AP1/ACEScg) stays the `Rasm.Materials` Unicolour owner's concern, never re-derived here. `AppearanceSummary.AppearanceKey` reconciles this appearance with the `Rasm.Materials` OpenPBR owner at the content key: it is the kernel seed-zero `XxHash128` over the neutral PBR vector — the shared contract-owned `AppearanceSummary.Of` derivation the `Rasm.Materials` `ComponentProjector` composes identically, so a BIM-imported `IfcSurfaceStyleRendering` style and a `Rasm.Materials` OpenPBR row describing the same surface dedup to one content key, the `Rasm.Materials` owner the authority for the full BSDF and this page producing only the IFC-derived neutral summary. `Rasm.Materials` holds the `surface#OPENPBR_SLAB` `OpenPbrSurface` vector, the `surface#CONDUCTOR_IOR` conductor-IOR table, and the OpenPBR slab algebra, so re-minting any of them in this owner is the named cross-folder contract violation.

`AppearanceProjection` keeps every signature HOST-NEUTRAL — no `Rhino.Geometry`, no `Unicolour`, no `System.Drawing.Color` crosses one (the GeometryGym `IfcColourRgb.Color()`/`IfcColourRgb(DatabaseIfc, Color)` host-coupled members are the deleted form, only the `Red`/`Green`/`Blue` doubles + the `(DatabaseIfc, double, double, double)` ctor are read/authored). `Object` carries appearance element-scoped through the `Associate` edge — a shared `Node.Appearance`, never a record nested in the retired `BimMaterial` — so the retired `BimAppearance`/`AppearanceColor` records and the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial.Option<BimAppearance>` carrier are GONE, mirroring how the rebuilt `Semantics/classification#CLASSIFICATION_AXIS` lowers onto the shared `Classification` value and `Semantics/composition#MATERIAL_COMPOSITION` onto the shared `Node.Material`. `Project` faults a malformed presentation graph onto `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected` (`surface-style-miss`), lifted BARE onto the `Fin` result.

## [01]-[INDEX]

- [02]-[APPEARANCE_PROJECTION]: `AppearanceProjection.Project` the `IfcStyledItem`→`StyledAppearance` ingress fold over the presentation graph, the `ReflectanceModel` `[SmartEnum<string>]` IFC reflectance-method roster carrying the typed PBR bias (`Metalness`/`RoughnessHint`/`Transmissive`) with the `ForPbr` reverse classifier, the `TextureMode` mode→canonical-channel roster carrying the shared `ChannelPolarity` column, `TextureTrait` the capability vocabulary a carried texture answers over with `Rowable` the shared roster demand, the `SurfaceTexture` `[Union]` over the three concrete IFC texture payloads with the shared `TextureWrap` axis pair and the `UvTransform` frame they carry, the `RosterOf` mint projecting a styled appearance onto the Element `TextureRoster` shared row in the contract own typed vocabulary, the sRGB `Linearize`/`Encode` transfer pair, the `AppearanceKey` content-key derivation shared with `Rasm.Materials`, the inverse `AppearanceProjection.Author` egress re-authoring the `IfcSurfaceStyleRendering`+`IfcSurfaceStyleWithTextures`+`IfcSurfaceStyleRefraction` surface style (armed on the emit path's body-representation author), the `AppearanceProjection.Bind` representation-item binder driving each shared attribute lane off its `Binders` row, the `IndexedColour` per-face radiometry value both directions share, and the `IfcInternals` `[UnsafeAccessor]` capsule through which they reach GeometryGym's sealed presentation payloads.

## [02]-[APPEARANCE_PROJECTION]

- Owner: `AppearanceProjection` the static BIDIRECTIONAL GeometryGym↔shared surface-style projector — the `Project` ingress folding one `IfcStyledItem`'s front-face `IfcSurfaceStyle` into one `StyledAppearance` (the content-keyed shared `Node.Appearance` beside its texture roster), and the `Author` egress re-authoring a shared `AppearanceSummary` and that roster back onto the GeometryGym presentation graph the `Emit` composes; `ReflectanceModel` the `[SmartEnum<string>]` IFC reflectance-method roster the projection folds the method onto without re-reading the enum string; `TextureMode` the IFC texture-mode roster carrying each token's canonical channel name and its gloss/transparency polarity; `SurfaceTexture` the `[Union]` over the three concrete `IfcSurfaceTexture` payloads with `UvTransform` the UV frame both projector halves carry; `StyledAppearance` the ingest product pairing the summary with its texture roster, its `SurfaceLighting` coefficient set, and the `IfcSurfaceSide` sidedness bit — the three facts the frozen preimage cannot carry; `IndexedColour` the IFC per-face radiometry value BOTH directions share — palette, one-based per-face run, single alpha — carrying the `Of` read off a face set, the `Of` fold off a shared colour lane keyed on the ONE byte quantizer, the `Rgba` per-face resolve the geometry walk reads, and the `Author` map write; `Bind` the representation-item egress binder over the `Binders` channel-to-author table; `IfcInternals` the `[UnsafeAccessor]` capsule that is this branch's ONE reach into a GeometryGym `internal` presentation payload, projecting the colour palette and index runs as detached `Seq` values and owning the `IfcIndexedTriangleTextureMap` mint whole, pinned to the manifest package version. `Rasm.Element` owns the `AppearanceSummary` neutral PBR record and its key derivation at the boundary — the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` codec hashed through the `Projection/address#CONTENT_ADDRESS` `ContentAddress` — and this page declares neither, composing the contract vocabulary, mapping the GeometryGym presentation entities onto it and back.
- Cases: `ReflectanceModel` arms `Blinn`/`Flat`/`Glass`/`Matt`/`Metal`/`Mirror`/`Phong`/`Plastic`/`Strauss`/`NotDefined` (10), the full IFC4.3 `IfcReflectanceMethodEnum` partition keyed on the schema constant, each carrying its typed PBR bias — `Metalness` (`METAL`/`MIRROR` → 1.0, `STRAUSS` → 0.5, every dielectric → 0.0), `RoughnessHint` (the fallback when the style supplies no `IfcSpecularHighlight` — `MIRROR` → 0.0, `MATT`/`FLAT` → 1.0), and `Transmissive` (`GLASS` → true); `TextureMode` spans BOTH the IFC2x3 `IfcSurfaceTextureEnum` tokens and the IFC4 free-identifier spellings authoring tools emit, each row naming the canonical channel it resolves to and its shared `ChannelPolarity` (`SHININESS` gloss and `TRANSPARENCYMAP` transparency `Inverted`, every other row `Direct`), `NotDefined` the unresolved row; `TextureTrait` rows `Classified`/`Addressable`/`Embedded`/`Transformed`, the four facts a downstream gate asks before it acts, held as one `CapabilitySet<TextureTrait>` derived per case; `SurfaceTexture` arms `Url`/`Blob`/`Pixels` over `IfcImageTexture`/`IfcBlobTexture`/`IfcPixelTexture`, each sharing the mode, the shared `TextureWrap` axis pair, and the optional UV frame through the root positional columns; the appearance is the contract's ONE `AppearanceSummary`, never a `RenderingAppearance`/`ShadingAppearance`/`TexturedAppearance` sibling triple and never a Bim `BimAppearance`/`AppearanceColor` record beside the contract.
- Entry: `AppearanceProjection.Project(IfcStyledItem styledItem, double tolerance, Op key)` returning `Fin<StyledAppearance>` is the per-styled-item leaf the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes from its per-`Object` representation walk — a dedicated appearance fold (the sibling of `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.MaterialEdges`) that discovers each object's styled items through the GeometryGym `IfcRepresentationItem.StyledByItem` inverse, calls `Project`, dedups the minted node by id, authors the `Object`→`Appearance` `Associate` edge against the object's rooted `NodeId` with `MaterialUsage.Unbound` (the appearance `Associate` edge carries no material usage), and projects the `StyledAppearance` texture roster through `RosterOf` onto the Element `TextureRoster` shared row keyed by the minted appearance node id — the roster never enters the element graph, which owns the summary alone, and `Rasm.Materials` `SetIngest.Roster` classifies the row with no app-root relay; `AppearanceProjection.TextureSetOf(ElementGraph, NodeId)` is the READER half of the `Rasm.Materials` link, resolving the `Associate`-linked `DetailSchema.Appearance` bag off the appearance node and reading `DetailSchema.TextureSet` back as the baked-set `ContentAddress` the Materials `Projection/component#COMPONENT_PROJECTOR` `BindTextureSet` wrote — the address rides BESIDE the frozen seven-value `AppearanceSummary` preimage and never inside it, because widening that preimage re-keys every stored `Node.Appearance`; `AppearanceProjection.DoubleSidedOf(ElementGraph, NodeId)` is its sibling projection over the SAME `AppearanceRow` walk, reading `DetailSchema.DoubleSided` back as the sidedness a `Rasm.Materials` OpenPBR thin-walled row wrote, so a Materials-authored shell reaches `Exchange/export#EXPORT_PIPELINE` `MaterialFinish.DoubleSided` by the route an IFC-declared one already takes — precedence is by ORIGIN, the `IfcSurfaceSide` bit on this projector's own `StyledAppearance` authoritative for an appearance this page minted and the bag row answering for one no IFC style described, so the two producers never contend over a node and `Option<bool>` keeps an undeclared fact distinct from a declared single-sided one; a `Material`-scoped style instead rides the `Rasm.Materials` `ComponentProjector`, which authors its own `element→appearance` edge — extracting the front-face `IfcSurfaceStyle` (`Side` `BOTH`/`POSITIVE`) through `BaseClassIfc.Extract<IfcSurfaceStyle>()` (version-agnostic: it flattens an IFC2x3 `IfcPresentationStyleAssignment` wrapper), folding its element selects onto the neutral PBR vector, and minting the content-keyed shared `Node.Appearance`; `Fin<T>` aborts on a presentation graph carrying no front-face surface style (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected` `surface-style-miss`) and carries the shared factory's own `ElementFault.ValueRejected` through on an out-of-unit channel, both lifted BARE (the `Fault`-derived band is the `Error`, no `.ToError()` hop). `AppearanceProjection.Author(DatabaseIfc db, AppearanceSummary summary, Seq<SurfaceTexture> textures, Option<SurfaceLighting> lighting, bool doubleSided)` is the egress entry for an `Object` node carrying an appearance — re-authoring the `IfcSurfaceStyleRendering` from the neutral summary, the `IfcSurfaceStyleWithTextures` from a non-empty roster, and the `IfcSurfaceStyleRefraction` from a transmissive summary, all through ONE five-slot `IfcSurfaceStyle` constructor; total (authoring from a valid summary cannot fail), returning the `IfcStyledItem` a representation author binds onto its item. `AppearanceProjection.Bind(IfcStyledItem styled, IfcTriangulatedFaceSet faceSet, Seq<(EncodingChannel Channel, float[] Lane)> attributes, long[] corners, Op key)` returning `Fin<Seq<(EncodingChannel, IfcPresentationItem)>>` is the second egress entry — result-returning because the colour arm's palette holds raw decoded floats the kernel byte-leg admission can refuse — called once per authored triangulated face set, handing that mesh's OWN shared attribute roster — these payloads bind on the REPRESENTATION ITEM, not the style, so they cannot be columns on `Author`, and one entry driving the `Binders` table means a lane IFC learns to bind is a row rather than a third public member; an empty sequence is the ordinary untextured, unpainted case.
- Auto: `Project` reads the front-face `IfcSurfaceStyle.Styles` element selects — the `IfcSurfaceStyleRendering` (an `IfcSurfaceStyleShading` subtype, so it supplies the inherited `SurfaceColour`/`Transparency` with the `DiffuseColour`/`ReflectanceMethod`/`SpecularHighlight` rendering channels), a bare `IfcSurfaceStyleShading` fallback (colour/transparency only), and the `IfcSurfaceStyleRefraction` optical signal — and folds the channel precedence: the rendering `DiffuseColour` overrides the shading `SurfaceColour` for the base colour through BOTH `IfcColourOrFactor` select arms — an `IfcColourRgb` replaces (each channel lowered to scene-linear through `Linearize`), an `IfcNormalisedRatioMeasure` SCALES the linearized surface colour (reflectance is linear-domain energy; the GG ctor clamps the ratio [0,1]; the `as IfcColourRgb` cast that ignored the factor arm is the deleted form), defaulting grey when absent — the opacity is the transparency complement (a `double.NaN` transparency, the GeometryGym unset sentinel, defaulting to opaque), the metalness is `ReflectanceModel.FromIfc(ReflectanceMethod).Metalness`, the roughness reads the `IfcSpecularHighlightSelect` (`IfcSpecularRoughness` directly as a [0,1] roughness, `IfcSpecularExponent` converted through the Phong `α = √(2/(n+2))`) and falls back to the row's `RoughnessHint`, and the transmissive flag is the REFRACTIVE signal (the row's `Transmissive` GLASS method or a present `IfcSurfaceStyleRefraction`, NEVER a sub-unit opacity — IFC `Transparency` is the alpha/opacity channel, physically distinct from transmission), PERSISTED on the summary apart from opacity (so an opaque-alpha refractive glass keeps its transmission, the round-trip symmetric with the egress `IfcSurfaceStyleRefraction`); the contract-owned `AppearanceSummary.Of` then admits the seven frozen preimage values under this call's `Op` key and derives the `AppearanceKey` itself as the kernel seed-zero `XxHash128` over the canonical PBR bytes — the factory's own writer runs at raw IEEE bits because PBR scalars are not `Header`-quantized measures, so no tolerance argument exists to pass and none forks the shared dedup key (the shared `CanonicalWriter` → `ContentAddress.Of`, the ONE hasher the `Rasm.Materials` owner composes identically, this page assembling no key bytes of its own); `TexturesOf` folds every `IfcSurfaceStyleWithTextures` select's `Textures` list through `SurfaceTexture.Of` into the roster riding beside the summary — the concrete subtype discriminates the payload case, the `Mode` token resolves its canonical channel and polarity, and an optional `IfcCartesianTransformationOperator2D` lifts to the `UvTransform` frame — while the neutral scalars stay untouched, so a textured style never averages a map into a scalar; `Mint` content-keys the shared `Node.Appearance` whose id is the shared `Node.ToCanonicalBytes` (id excluded) re-stamped through the shared `Node.Relabel` — the class-root `[Union]` `Node` case generates no `with`, so the `draft with { Id = … }` spelling is the deleted form, the mint the `Rasm.Materials` `ComponentProjector` composes identically — so two structurally-identical appearances dedup to one node. `Author` encodes the scene-linear base colour back to display sRGB through `Encode`, picks the `IfcReflectanceMethodEnum` from the neutral PBR through `ReflectanceModel.ForPbr` (a transmissive surface `GLASS`, a metallic mirror `MIRROR`, a rough metal `METAL`, a matte dielectric `MATT` so an imported `MATT`/`FLAT` finish round-trips via its `1.0` `RoughnessHint`, every remaining dielectric `PLASTIC` — the modern method subset only, a superseded `BLINN`/`PHONG`/`STRAUSS` being import vocabulary the neutral vector absorbs, never re-authored), tints the specular from the base colour for a metal and reflects neutral for a dielectric, authors `IfcSpecularRoughness` from the summary roughness, re-authors each `SurfaceTexture` through its own total `Switch` into one `IfcSurfaceStyleWithTextures`, and lands every element select through the ONE five-slot `IfcSurfaceStyle` constructor whose null slots carry the absent axes — a ternary ladder growing an arm per transmission×texturing combination is the deleted form. Egress runs PAYLOAD-COMPLETE and BINDING-COMPLETE: `Bind` reads the mesh's shared attribute roster and lands each lane through its `Binders` row — the `Uv` lane minting an `IfcIndexedTriangleTextureMap` through the `IfcInternals` capsule, naming the extracted `IfcSurfaceTexture` rows its parameterization serves, writing the public `TexCoords` vertex list and the per-triangle index triples, and letting the `MappedTo` setter self-register the map into the face set's `HasTextures`; the `ColorRgba` lane folding through `IndexedColour` into a deduped palette with a per-face index run and authoring an `IfcIndexedColourMap` on public surface alone. Both rows read an arity guard before binding, because a lane whose corner run walks off its own end authors a file that faults in the RECEIVING application. Rasm-authored elements therefore carry their per-vertex UV BINDING and its radiometry, and `Exchange/import#EXPLICIT_TESSELLATION` re-reads exactly what this half wrote.
- Output: the shared `Node.Appearance` is the appearance evidence the `Projection/semantic#SEMANTIC_PROJECTOR` projector lands (authoring the `Object`→`Appearance` `Associate` edge) and the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads into `element.Appearance` (an `Option<AppearanceSummary>` a consumer reads flat), the `Exchange/export#EXPORT_PIPELINE` `MaterialFinish` carries whole — its scene-linear base colour and opacity entering `baseColorFactor` unencoded, its metalness and roughness written on every material because the glTF defaults are both unity, and its `Transmissive` bit writing `KHR_materials_transmission` while the opacity alone drives `AlphaMode` — and the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` OpenPBR owner reconciles at the `AppearanceKey` content key; the `ReflectanceModel` typed PBR bias is the IFC-side mapping a downstream metallic/dielectric author folds, never a re-read of the IFC enum string.
- Growth: a new IFC reflectance method is one `ReflectanceModel` row carrying its schema constant and typed PBR bias; a new texture-mode spelling is one `TextureMode` row naming its canonical channel and its shared `ChannelPolarity`; a new capability a consumer gates on is one `TextureTrait` row with its per-case derivation, never a second predicate at the gate; a new IFC texture payload subtype is one `SurfaceTexture` case the `Of` fold and the `Author` `Switch` are both compiler-forced to route; a new SCALAR presentation-channel read is one more `Styles` element-select arm folding onto the neutral summary, a new FIELD-valued read is one more carrier beside the textures on `StyledAppearance` (the `SurfaceLighting` four-colour coefficient set is the landed one, its egress arm the `IfcSurfaceStyle` lighting slot), and a new RENDER-REPRESENTATION toggle (the sidedness bit is the landed one) is one more column there whose egress arm is a single `IfcSurfaceStyle` slot — never a summary column in either case; a further Materials-authored fact a consumer keys on is one owner-declared `PropertyName` at the `Rasm.Element` contract with one projection over the existing `AppearanceRow` walk, never a second edge traversal; the shared `AppearanceSummary` absorbs the neutral vector with no contract edit; a shared attribute lane IFC learns to bind to a representation item is one `Binders` row and the author it names, never a second public binding member; a further GeometryGym payload the assembly seals is one accessor row with one value-projecting member on `IfcInternals`, so the pinned-version surface stays countable in one class; never a per-style appearance class, never a Bim appearance record beside the graph node, and never a second accessor capsule.
- Boundary: the appearance model is the shared `Node.Appearance` + `AppearanceSummary` and a Bim `BimAppearance`/`AppearanceColor`/`RenderingAppearance`/`ShadingAppearance`/`TexturedAppearance` re-declaration is the deleted form — the contract owns the neutral PBR record, this page owns only the GeometryGym discrimination that fills it, so the appearance lowers onto the one shared summary with the absent channels defaulted, never a parallel per-style class; the retired `BimAppearance` record and the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial.Option<BimAppearance>` carrier are GONE, appearance being element-scoped (a shared `Node.Appearance` the `Object` carries through the `Associate` edge the `Bake` fold reads into `element.Appearance`), never a record nested in a material; the projection rides the GeometryGym `IfcStyledItem`/`IfcSurfaceStyle`/`IfcSurfaceStyleRendering`/`IfcColourRgb` surface consumed as settled vocabulary (`.api/api-geometrygym-ifc` presentation rows) through `BaseClassIfc.Extract<IfcSurfaceStyle>()`, and a hand-rolled STEP presentation-style reader is the deleted form; the page is HOST-NEUTRAL — a `Rhino.Geometry` colour, a `System.Drawing.Color` (the `IfcColourRgb.Color()`/`IfcColourRgb(DatabaseIfc, Color)` host-coupled members), or a `Unicolour` object crossing a signature is the named host-coupling defect, only the `Red`/`Green`/`Blue` scene-linear doubles cross; the ONE exception is a construction argument, not a crossing — `IfcColourRgbList` publishes no neutral author at all, so `IndexedColour.Author` spells `System.Drawing.Color.FromArgb` fully qualified at that single site inside the value that owns the palette, and the discriminant is exact: a host colour type is forbidden where a neutral member exists (`IfcColourRgb` has its double triple) and confined to the boundary owner where it is the only expression that constructs the entity; IFC presentation colour is display-referred sRGB lowered to scene-linear through the `Linearize` EOTF and encoded back through the `Encode` OETF, and BOTH read the kernel `RgbProfile.Srgb` row's own transfer rather than spelling the curve — a raw-channel pass-through that calls the unlinearized value "scene-linear" and a hand-written piecewise IEC 61966-2-1 body beside the row that publishes the same curve are the two deleted forms; the working-space PRIMARIES conversion stays the `Rasm.Materials` Unicolour owner's concern, never re-derived here; the OpenPBR reconciliation rides the `AppearanceKey` content key — a re-mint of the `Rasm.Materials/Appearance/surface#OPENPBR_SLAB` `OpenPbrSurface` vector, the `surface#CONDUCTOR_IOR` conductor-IOR table, or the OpenPBR slab algebra in this owner is the named cross-folder contract violation; the rich IFC rendering channels (`SpecularColour`/`TransmissionColour`/`ReflectionColour`/`DiffuseTransmissionColour`, the `IfcSurfaceStyleRefraction` IOR/dispersion MAGNITUDE) are NOT retained by the thin shared summary — a Bim-imported style collapses DELIBERATELY to base colour/metalness/roughness/opacity + a transmissive flag (the refraction PRESENCE is kept as the transmissive bit DISTINCT from the opacity/alpha channel so opaque-alpha glass round-trips, its IOR/dispersion magnitude dropped; the contract's chosen shape, lossy by design, NOT an unintended gap), and full specular/reflection/transmission-colour + dispersion BSDF fidelity exists ONLY when the `Rasm.Materials` owner AUTHORS the appearance and holds the lobe graph keyed by the shared `AppearanceKey` (claiming a Bim round-trip preserves the dropped colour/IOR channels is the deleted overclaim); TEXTURES and the `IfcSurfaceStyleLighting` four-colour coefficient set are the FIELD-valued presentation facts this projector retains, and both retain BESIDE the summary, never inside it — `StyledAppearance` pairs the content-keyed node with the `SurfaceTexture` roster and the `Option<SurfaceLighting>` carrier, the lighting egress landing in the `IfcSurfaceStyle` lighting slot, the frozen seven-value `AppearanceSummary` preimage stays sealed (an eighth column re-keys every stored `Node.Appearance` and forks the dedup key this page mints against), and folding a map mean into a scalar channel is the averaged-map defect the roster exists to refuse; the roster row is TYPED at its contract owner and this producer mints it in the contract own vocabulary — `TextureMode.Polarity` is `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `ChannelPolarity` and the `SurfaceTexture` axis pair is that owner `TextureWrap`, so a Bim-local `WrapMode` or invert-bool twin beside them is the deleted form that lets one edit fork two packages that never reference each other; NAMED LOSS on the wrap axis, one-way only — IFC states addressing as a single two-state repeat flag per axis, so `MirroredRepeat` is unreachable from an IFC ingest and reaches the shared row only from the glTF/USD producers it also serves, and the inverse folds it back onto the repeating flag with the mirror dropped AT THE FILE; a texture rows onto the contract when its `CapabilitySet<TextureTrait>` admits `Rowable` (`Classified` + `Addressable`) and a compound case-and-derived-predicate test at each gate is the deleted form the roster filter and this boundary already drifted apart under — the drop of a short texture is DELIBERATE and diagnostic-free because the shared `TextureRoster` row carries no refusal channel, `Missing(Rowable)` naming the shape such a channel takes when the contract grows one; the surviving `bool` columns are contract-fixed and stay: `ReflectanceModel.Transmissive` feeds the frozen `AppearanceSummary.Of` transmissive slot, `StyledAppearance.DoubleSided` is the whole admitted domain after the `NEGATIVE` filter and the `DetailSchema.DoubleSided` bag row it reconciles with, and `IfcInternals.StiffnessFixed` is a GeometryGym field this capsule detaches and never widens; this leaf CLASSIFIES and CARRIES alone — it opens no image, so a texel decode, the `TextureMode` gloss/transparency inversion, and any resampling ride the texture-set owner the shared `TextureRoster` row hands the classifiable roster to, and an `IfcPixelTexture` egresses its declared extent with an empty pixel run because GeometryGym exposes that run only through its constructor; `IfcInternals` is the ONLY place in this branch that names a GeometryGym `internal` member, and it binds through `[UnsafeAccessor]` alone — compile-time, reflection-free, trim- and AOT-safe, and loud at the first call when a release moves a member — pinned to the `Directory.Packages.props` `GeometryGymIFC_Core` version so a bump re-probes every binding; a hand-emitted STEP fragment injected beside the authored database, a reflection or IL-emit path, and a vendored fork are each the deleted form because each mints a second IFC reader or writer inside the package that owns exactly one, and a second copy of any accessor it carries is the divergence this single capsule exists to foreclose; the `ReflectanceModel` keys its `FromIfc` resolution through the `Items`-derived frozen index on the typed `Method` constant — no `switch` over the enum, no `ToString` hop, `NotDefined` the total fallback, and `TextureMode.From` mirrors that admission through the generated `Validate` with `NotDefined` the unresolved row rather than a guessed channel; faults route through the `Fin` result and lift BARE (the `Fault`-derived `BimFault.Refused` with `BimReason.Rejected` IS the `Error`, never a `.ToError()` hop and never an exception across a domain signature).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
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

    private ReflectanceModel(string key, IfcReflectanceMethodEnum method, double metalness, double roughnessHint, bool transmissive) : this(key) =>
        (Method, Metalness, RoughnessHint, Transmissive) = (method, metalness, roughnessHint, transmissive);

    private static readonly Lazy<FrozenDictionary<IfcReflectanceMethodEnum, ReflectanceModel>> ByMethod =
        new(static () => Items.ToFrozenDictionary(static row => row.Method));

    public static ReflectanceModel FromIfc(IfcReflectanceMethodEnum method) =>
        ByMethod.Value.GetValueOrDefault(method, NotDefined);

    public IfcReflectanceMethodEnum ToIfc() => Method;

    public static ReflectanceModel ForPbr(double metallic, double roughness, bool transmissive) =>
        transmissive          ? Glass
        : metallic >= 0.5     ? (roughness <= 0.05 ? Mirror : Metal)
        : roughness >= 0.9    ? Matt
        : Plastic;
}

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

    public static TextureMode From(string mode) => TryGet(mode, out TextureMode? row) && row is not null ? row : NotDefined;
}

public readonly record struct UvTransform(Vector2 Offset, Vector2 Scale, double Rotation) {
    public static readonly UvTransform Identity = new(Vector2.Zero, Vector2.One, 0d);

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

    public (double OffsetU, double OffsetV, double ScaleU, double ScaleV, double Rotation) Frame =>
        (Offset.X, Offset.Y, Scale.X, Scale.Y, Rotation);
}

[SmartEnum<string>]
public sealed partial class TextureTrait : ICapability<TextureTrait> {
    public static readonly TextureTrait Classified = new("classified");
    public static readonly TextureTrait Addressable = new("addressable");
    public static readonly TextureTrait Embedded = new("embedded");
    public static readonly TextureTrait Transformed = new("transformed");
}

[Union]
public abstract partial record SurfaceTexture(TextureMode Mode, TextureWrap WrapU, TextureWrap WrapV, Option<UvTransform> Uv, int CoordinateSet) {
    public sealed record Url(TextureMode Mode, TextureWrap WrapU, TextureWrap WrapV, Option<UvTransform> Uv, int CoordinateSet, string Reference)
        : SurfaceTexture(Mode, WrapU, WrapV, Uv, CoordinateSet);
    public sealed record Blob(TextureMode Mode, TextureWrap WrapU, TextureWrap WrapV, Option<UvTransform> Uv, int CoordinateSet, string RasterFormat, ReadOnlyMemory<byte> Raster)
        : SurfaceTexture(Mode, WrapU, WrapV, Uv, CoordinateSet);
    public sealed record Pixels(TextureMode Mode, TextureWrap WrapU, TextureWrap WrapV, Option<UvTransform> Uv, int CoordinateSet, int Width, int Height, int Components)
        : SurfaceTexture(Mode, WrapU, WrapV, Uv, CoordinateSet);

    public CapabilitySet<TextureTrait> Traits =>
        Shared.With(Switch<TextureTrait>(
            url:    static _ => TextureTrait.Addressable,
            blob:   static _ => TextureTrait.Embedded,
            pixels: static _ => TextureTrait.Embedded));

    CapabilitySet<TextureTrait> Shared =>
        (Mode.Resolved, Uv.IsSome) switch {
            (true, true)   => CapabilitySet<TextureTrait>.Of(TextureTrait.Classified, TextureTrait.Transformed),
            (true, false)  => CapabilitySet<TextureTrait>.Of(TextureTrait.Classified),
            (false, true)  => CapabilitySet<TextureTrait>.Of(TextureTrait.Transformed),
            (false, false) => CapabilitySet<TextureTrait>.None,
        };

    public static readonly CapabilitySet<TextureTrait> Rowable =
        CapabilitySet<TextureTrait>.Of(TextureTrait.Classified, TextureTrait.Addressable);

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

    static TextureWrap Wrap(bool repeat) => repeat ? TextureWrap.Repeat : TextureWrap.ClampToEdge;

    static bool Repeats(TextureWrap wrap) => wrap != TextureWrap.ClampToEdge;

    public SurfaceTexture At(int coordinateSet) => this with { CoordinateSet = coordinateSet };

    public IfcSurfaceTexture Author(DatabaseIfc db) {
        IfcSurfaceTexture authored = Switch<IfcSurfaceTexture>(
            url:    u => new IfcImageTexture(db, Repeats(u.WrapU), Repeats(u.WrapV), u.Reference),
            blob:   b => new IfcBlobTexture(db, Repeats(b.WrapU), Repeats(b.WrapV), b.RasterFormat, new IfcBinary(b.Raster.ToArray())),
            pixels: p => new IfcPixelTexture(db, Repeats(p.WrapU), Repeats(p.WrapV), p.Width, p.Height, p.Components, []));
        authored.Mode = Mode.Key;
        Uv.IfSome(uv => authored.TextureTransform = Operator(db, uv));
        authored.Parameter = [$"U{CoordinateSet}", $"V{CoordinateSet}"];
        return authored;
    }

    static IfcCartesianTransformationOperator2D Operator(DatabaseIfc db, UvTransform uv) =>
        new(db) {
            LocalOrigin = new IfcCartesianPoint(db, uv.Offset.X, uv.Offset.Y),
            Axis1 = new IfcDirection(db, Math.Cos(uv.Rotation), Math.Sin(uv.Rotation)),
            Scale = uv.Scale.X,
        };
}

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

    public IfcSurfaceStyleLighting Author(DatabaseIfc db) =>
        new(Colour(db, DiffuseTransmission), Colour(db, DiffuseReflection), Colour(db, Transmission), Colour(db, Reflectance));

    static IfcColourRgb Colour(DatabaseIfc db, (double R, double G, double B) c) =>
        new(db, AppearanceProjection.Encode(c.R), AppearanceProjection.Encode(c.G), AppearanceProjection.Encode(c.B));
}

public readonly record struct StyledAppearance(Node.Appearance Appearance, Seq<SurfaceTexture> Textures, Option<SurfaceLighting> Lighting, bool DoubleSided);

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

public readonly record struct IndexedColour(Seq<(double R, double G, double B)> Palette, Seq<int> Face, double Alpha) {
    public static Option<IndexedColour> Of(IfcTessellatedFaceSet faceSet) =>
        Optional(faceSet.HasColours)
            .Bind(static map => Optional(map.Colours).Map(list => new IndexedColour(
                IfcInternals.Palette(list).Map(static triple => (
                    AppearanceProjection.Linearize(triple.R), AppearanceProjection.Linearize(triple.G), AppearanceProjection.Linearize(triple.B))),
                IfcInternals.ColourRun(map),
                double.IsNaN(map.Opacity) ? 1.0 : Math.Clamp(map.Opacity, 0.0, 1.0))))
            .Filter(static colour => !colour.Palette.IsEmpty && !colour.Face.IsEmpty);

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

    public (double R, double G, double B, double A) Rgba(int face) =>
        Palette[Face[face] - 1] switch { var (r, g, b) => (r, g, b, Alpha) };

    public Fin<IfcIndexedColourMap> Author(IfcTessellatedFaceSet faceSet, Op key) =>
        Palette
            .TraverseM(c => AppearanceProjection.Bytes(c.R, c.G, c.B, Alpha, key)
                .Map(static b => System.Drawing.Color.FromArgb(b.Red, b.Green, b.Blue)))
            .As()
            .Map(colors => new IfcIndexedColourMap(faceSet, new IfcColourRgbList(faceSet.Database, colors), Face) { Opacity = Alpha });

}

// --- [BOUNDARIES] ----------------------------------------------------------------------
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

    public static Seq<(double R, double G, double B)> Palette(IfcColourRgbList list) =>
        toSeq(ColourList(list)).Map(static triple => (triple.Item1, triple.Item2, triple.Item3));

    public static Seq<int> ColourRun(IfcIndexedColourMap map) => toSeq(ColourIndex(map));

    public static Seq<(int A, int B, int C)> TexCoordRun(IfcIndexedTriangleTextureMap map) =>
        toSeq(TexCoordList(map)).Map(static triple => (triple.Item1, triple.Item2, triple.Item3));

    public static Option<(bool Rigid, double Native)> Warping(IfcBoundaryNodeConditionWarping condition) =>
        WarpingStiffness(condition) is { } select
            ? Some((StiffnessFixed(select), StiffnessValue(select)))
            : Option<(bool, double)>.None;

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AppearanceProjection {
    const double DefaultRefractionIndex = 1.5;

    public static Fin<StyledAppearance> Project(IfcStyledItem styledItem, double tolerance, Op key) =>
        styledItem.Extract<IfcSurfaceStyle>().AsIterable()
            .Filter(static surface => surface.Side is IfcSurfaceSide.BOTH or IfcSurfaceSide.POSITIVE)
            .Head
            .ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "surface-slot-miss", "style", styledItem.StepId.ToString(CultureInfo.InvariantCulture) })))
            .Bind(surface => SummaryOf(surface, key).Map(summary =>
                new StyledAppearance(Mint(summary, tolerance), TexturesOf(surface), LightingOf(surface), surface.Side is IfcSurfaceSide.BOTH)));

    static Option<SurfaceLighting> LightingOf(IfcSurfaceStyle surface) =>
        toSeq(surface.Styles)
            .Choose(static style => style is IfcSurfaceStyleLighting lighting ? Some(lighting) : Option<IfcSurfaceStyleLighting>.None)
            .Head
            .Bind(SurfaceLighting.Of);

    static Seq<SurfaceTexture> TexturesOf(IfcSurfaceStyle surface) =>
        toSeq(surface.Styles)
            .Choose(static style => style is IfcSurfaceStyleWithTextures textured ? Some(textured) : Option<IfcSurfaceStyleWithTextures>.None)
            .Bind(static textured => toSeq(textured.Textures))
            .Choose(SurfaceTexture.Of);

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

    public static Option<bool> DoubleSidedOf(ElementGraph graph, NodeId appearance) =>
        AppearanceRow(graph, appearance, DetailSchema.DoubleSided)
            .Bind(static value => value is PropertyValue.Boolean flag ? Some(flag.Value) : Option<bool>.None);

    static Fin<AppearanceSummary> SummaryOf(IfcSurfaceStyle surface, Op key) {
        Option<T> First<T>() where T : class =>
            surface.Styles.AsIterable().Choose(static s => s is T t ? Some(t) : Option<T>.None).Head;
        Option<IfcSurfaceStyleRendering> rendering = First<IfcSurfaceStyleRendering>();
        Option<IfcSurfaceStyleShading> shading = First<IfcSurfaceStyleShading>();
        Option<IfcSurfaceStyleRefraction> refraction = First<IfcSurfaceStyleRefraction>();

        return shading.Bind(static sh => Optional(sh.SurfaceColour)).Map(Lin)
            .ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "surface-slot-miss", "colour", surface.StepId.ToString(CultureInfo.InvariantCulture) })))
            .Bind(surfaceBase => Neutral(rendering, shading, refraction, surfaceBase, key));
    }

    static Fin<AppearanceSummary> Neutral(
        Option<IfcSurfaceStyleRendering> rendering, Option<IfcSurfaceStyleShading> shading,
        Option<IfcSurfaceStyleRefraction> refraction, (double R, double G, double B) surfaceBase, Op key) {
        (double R, double G, double B) baseColor = rendering.Bind(static r => Optional(r.DiffuseColour)).Map(diffuse => diffuse switch {
            IfcColourRgb rgb                => Lin(rgb),
            IfcNormalisedRatioMeasure ratio => (surfaceBase.R * ratio.Measure, surfaceBase.G * ratio.Measure, surfaceBase.B * ratio.Measure),
            _                               => surfaceBase,
        }).IfNone(surfaceBase);

        ReflectanceModel reflectance = rendering.Map(static r => ReflectanceModel.FromIfc(r.ReflectanceMethod)).IfNone(ReflectanceModel.NotDefined);
        double opacity = shading.Map(static sh => double.IsNaN(sh.Transparency) ? 1.0 : 1.0 - Math.Clamp(sh.Transparency, 0.0, 1.0)).IfNone(1.0);
        double roughness = rendering.Bind(static r => RoughnessOf(r.SpecularHighlight)).IfNone(reflectance.RoughnessHint);
        bool transmissive = reflectance.Transmissive || refraction.IsSome;

        return AppearanceSummary.Of(baseColor.R, baseColor.G, baseColor.B, reflectance.Metalness, Math.Clamp(roughness, 0.0, 1.0), opacity, transmissive, key);
    }

    static Option<double> RoughnessOf(IfcSpecularHighlightSelect? highlight) => highlight switch {
        IfcSpecularRoughness r => Some(Math.Clamp(r.SpecularRoughness, 0.0, 1.0)),
        IfcSpecularExponent e  => Some(Math.Clamp(Math.Sqrt(2.0 / (Math.Max(0.0, e.SpecularExponent) + 2.0)), 0.0, 1.0)),
        _                      => Option<double>.None,
    };

    static Node.Appearance Mint(AppearanceSummary summary, double tolerance) {
        Node.Appearance draft = new(NodeId.Of(new NodeSeed.Placement()), summary);
        return (Node.Appearance)draft.Relabel(NodeId.Of(new NodeSeed.Content(draft, tolerance)));
    }

    static (double R, double G, double B) Lin(IfcColourRgb c) => (Linearize(c.Red), Linearize(c.Green), Linearize(c.Blue));

    // --- [EGRESS] ----------------------------------------------------------------------
    public static IfcStyledItem Author(DatabaseIfc db, AppearanceSummary summary, Seq<SurfaceTexture> textures, Option<SurfaceLighting> lighting, bool doubleSided) {
        IfcColourRgb surfaceColour = new(db, Encode(summary.BaseColorR), Encode(summary.BaseColorG), Encode(summary.BaseColorB));
        bool transmissive = summary.Transmissive;
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
            lighting.Match(Some: row => row.Author(db), None: static () => (IfcSurfaceStyleLighting?)null),
            textures.IsEmpty ? null : new IfcSurfaceStyleWithTextures([.. textures.Map(texture => texture.Author(db))]),
            null,
            transmissive ? new IfcSurfaceStyleRefraction(db) { RefractionIndex = DefaultRefractionIndex } : null) {
            Side = doubleSided ? IfcSurfaceSide.BOTH : IfcSurfaceSide.POSITIVE,
        };
        return new IfcStyledItem(style);
    }

    public static Fin<Seq<(EncodingChannel Channel, IfcPresentationItem Item)>> Bind(
        IfcStyledItem styled, IfcTriangulatedFaceSet faceSet, Seq<(EncodingChannel Channel, float[] Lane)> attributes, long[] corners, Op key) =>
        corners.Length >= 3 && corners.Length % 3 == 0
            ? corners.Max() switch { var reach => attributes
                .TraverseM(entry => Optional(Binders.Value.GetValueOrDefault(entry.Channel)).Match(
                    Some: bind => bind(styled, faceSet, entry.Lane, corners, reach, key).Map(item => item.Map(bound => (entry.Channel, Item: bound))),
                    None: () => Fin.Succ(Option<(EncodingChannel Channel, IfcPresentationItem Item)>.None)))
                .As()
                .Map(static bound => bound.Somes()) }
            : Fin.Succ(Seq<(EncodingChannel Channel, IfcPresentationItem Item)>.Empty);

    static readonly Lazy<FrozenDictionary<EncodingChannel, Func<IfcStyledItem, IfcTriangulatedFaceSet, float[], long[], long, Op, Fin<Option<IfcPresentationItem>>>>> Binders =
        new(static () => new Dictionary<EncodingChannel, Func<IfcStyledItem, IfcTriangulatedFaceSet, float[], long[], long, Op, Fin<Option<IfcPresentationItem>>>> {
            [EncodingChannel.Uv] = static (styled, faceSet, lane, corners, reach, _) => Fin.Succ(Mapped(styled, faceSet, lane, corners, reach).Map(static map => (IfcPresentationItem)map)),
            [EncodingChannel.ColorRgba] = static (_, faceSet, lane, corners, reach, key) => Coloured(faceSet, lane, corners, reach, key).Map(static map => map.Map(static item => (IfcPresentationItem)item)),
        }.ToFrozenDictionary());

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

    static bool Indexable(float[] lane, long reach, int arity) =>
        lane.Length >= arity && reach < lane.Length / arity;

    static Seq<(double U, double V)> Pairs(float[] lane) =>
        toSeq(Enumerable.Range(0, lane.Length / 2).Select(v => ((double)lane[v * 2], (double)lane[(v * 2) + 1])));

    static Seq<(int A, int B, int C)> Triples(long[] corners) =>
        toSeq(Enumerable.Range(0, corners.Length / 3)
            .Select(t => ((int)corners[t * 3] + 1, (int)corners[(t * 3) + 1] + 1, (int)corners[(t * 3) + 2] + 1)));

    public static double Linearize(double channel) =>
        RgbProfile.Srgb.Configuration.Rgb.ToLinear(channel, RgbProfile.Srgb.Configuration.DynamicRange);

    public static double Encode(double channel) =>
        RgbProfile.Srgb.Configuration.Rgb.FromLinear(channel, RgbProfile.Srgb.Configuration.DynamicRange);

    public static Fin<(byte Red, byte Green, byte Blue, byte Alpha)> Bytes(double r, double g, double b, double alpha, Op key) =>
        PerceptualColor.OfRgb(r, g, b, RgbProfile.Srgb, alpha, key).Map(static colour => colour.ToRgb());

    public static (double R, double G, double B, double A) Decode(byte red, byte green, byte blue, byte alpha) =>
        PerceptualColor.Of(red, green, blue, alpha / 255.0).ToRgb(RgbProfile.Srgb, transfer: RgbTransfer.Linear);
}
```

## [03]-[RESEARCH]

(none)
