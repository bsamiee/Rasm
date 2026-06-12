# geo-interchange — bedrock

## one interior vocabulary, two wire projections

- NetTopologySuite geometry is the single interior geo vocabulary; GeoJSON text and the GeoPackage geometry blob are wire projections of the same `Geometry` family, converted only at boundaries.
- No parallel geo model exists inboard — coordinate DTOs, vendor geometry types, raw coordinate arrays are all rejected shapes; every geo capability (predicates, envelopes, precision, orientation) is exercised on the one vocabulary and serialized at the seam.
- The two projections split by medium, not capability: GeoJSON for text interchange with foreign consumers, the GeoPackage blob for embedded-store geometry columns.
- Both round-trip through the same interior type, so a flow from store column to text feed is decode-blob, project, encode-text — never a direct blob-to-text transcode that bypasses the vocabulary.

## GeoJSON STJ rail — admission

- The entire GeoJSON family enters serializer options as one factory row: `new GeoJsonConverterFactory(geometryFactory, writeGeometryBBox, idPropertyName, ringOrientationOption, allowModifyingAttributesTables)` added once to `JsonSerializerOptions.Converters`.
- The constructor matrix is progressive — every shorter overload defaults the remaining policies — so the factory call site is the complete, readable statement of the geo wire profile.
- The factory answers for `Geometry` and its seven concrete subtypes, `IFeature` implementations, `FeatureCollection`, and `IAttributesTable`; the per-type converters are internal and reachable only through `CreateConverter`.
- Read-side features materialize as a package-internal feature implementation surfaced through `IFeature` — consumers program against the contract interface, and code downcasting to a concrete feature type breaks by design.
- An attributes table is convertible standalone — a properties-shaped value inside a non-feature contract serializes under the same factory row, so attribute-bag fields do not need feature wrapping to cross the wire.
- Instantiating or hand-writing a geometry converter beside the factory is the named defect — five policies set once at the factory would scatter across registrations and drift independently.
- The factory composes with generated strict contexts as a runtime-converter row in the app-root options merge: geometry types never receive generated type-info, the factory supplies their converters, and the rest of the contract stays source-generated — one merge point, no second registration site.
- The default geometry factory carries a floating precision model and SRID 4326, matching the wire's fixed CRS; a caller-supplied factory swaps precision and SRID policy for every conversion the family performs — precision posture is declared once at admission, never per call.

## GeoJSON STJ rail — write law

- Coordinates write raw: the writer emits stored X/Y doubles without precision-model rounding; nothing at emission quantizes.
- Precision is admission-side only — the reader applies `PrecisionModel.MakePrecise` to X and Y as coordinates parse, and Z passes through unrounded even there.
- The asymmetry is load-bearing: to bound wire precision (payload size, stable hashing of emitted text), geometries are constructed or reduced under a fixed-precision factory before serialization.
- The third dimension is conditional and the fourth absent: Z writes only when the sequence has Z and the value is not NaN; M never crosses the wire in either direction.
- XY and XYZ round-trip; XYM and XYZM degrade silently to XY/XYZ — the wire contract is declared as XY(Z), and measure-bearing data routes through a non-GeoJSON projection.
- Ring orientation is a write-time policy enum: `DoNotModify`, `EnforceRfc9746` (exterior counter-clockwise, interior clockwise — the spec-correct posture and the default), and a legacy arm with inverted orientation for consumers built against the old misreading.
- The spec-correct member name transposes the RFC digits (`EnforceRfc9746`) — a spelling trap that survives compilation only when copied exactly.
- Enforcement checks each ring's orientation and reverses the coordinate sequence when wrong — a per-mis-oriented-ring allocation, so normalizing orientation at admission amortizes repeated writes of the same geometry.
- Orientation enforcement is write-only: the read path admits foreign polygons with whatever orientation they carry — interior predicates are orientation-agnostic, but any kernel that derives sign from ring direction normalizes at admission, because the wire law will not have done it.
- `writeGeometryBBox` adds `bbox` per geometry; features additionally write `bbox` from their `BoundingBox` when present — bbox is a write-policy row, not a per-call argument.
- Feature write order is fixed: `type`, optional `id`, optional `bbox`, `geometry`, `properties`; `geometry` and `properties` members are always present, with null geometry written as JSON null — matching consumers that pattern-match member presence.
- Point coordinates write as a single position, never a one-element array — the single/multiple distinction is structural in the writer, so consumers relying on uniform array nesting are reading the format wrong, not the emitter.
- A null coordinate sequence writes as JSON null coordinates rather than an empty array — empty and absent are distinct wire states, and admission preserves the distinction.
- The feature `id` lifts from the attribute named by the factory's id property (default `GeoJsonConverterFactory.DefaultIdPropertyName`, the literal `"_NetTopologySuite_id"`): that attribute emits as the feature-level `id` instead of a property.
- Exchanging features with a foreign id convention is one constructor argument, not an attribute-copy pass over every feature.

## GeoJSON STJ rail — read law

- Reading is member-order independent: `type`, `coordinates`, and `geometries` parse independently, then reconcile — coordinates parse into a shape-agnostic carrier whose supported geometry types are checked against the declared `type`.
- The rejection taxonomy is closed and rides the standard STJ rejection channel as `JsonException`: missing `type`, missing `coordinates`, type/coordinates incompatibility, and a feature `id` that is neither JSON string nor number.
- One rejection escapes the taxonomy: an unrecognized `type` literal parses through an enum-name lookup and throws an argument fault, not a `JsonException` — the boundary's exception capture must admit both classes, or a malformed type literal bypasses the wire-fault rail.
- Geometry collections read through a dedicated `geometries` member with recursive geometry parsing — nesting is structural, and the `coordinates`/`geometries` pair is reconciled the same way as type/coordinates.
- Rejections never yield partially constructed geometries — the carrier reconciliation happens before any geometry exists.
- JSON null reads as null geometry — the one place the rail yields null — and projects to absence at the rail bridge immediately.
- Unknown members skip without error: foreign extensions (legacy CRS members, vendor keys) pass through reads harmlessly and are dropped, so tolerant reading is structural, not configured.
- The read path explicitly skips comment tokens — geometry parsing survives comment-admitting reader options, so a relaxed document profile does not need a separate geo profile.
- The nested read is converter-recursive: a feature's `geometry` member deserializes through the same options-resolved geometry converter, so feature and geometry policy can never diverge within one options instance.
- CRS posture is fixed by the format: coordinates are longitude/latitude WGS84, no CRS member exists on the wire, and the interior SRID is metadata the wire never carries.
- A flow needing another CRS reprojects inside the interior vocabulary before emission or after admission; emitting projected coordinates into GeoJSON is a silent-corruption defect no reader can detect.
- Feature attributes deserialize lazily: read yields a read-only attributes table backed by the parsed JSON element unless the factory's mutability flag admits the mutable node-backed table with `Add`/`DeleteAttribute`.
- The mutable table also exposes its serializer options and root node; the read-only table exposes its root element — both are adapters over the parsed document, not copies.
- Typed projection goes through `IPartiallyDeserializedAttributesTable`: `TryDeserializeJsonObject<T>` converts the whole properties object, `TryGetJsonObjectPropertyValue<T>` converts one property — both options-carrying and bool-railed.
- Attribute admission therefore composes the same serializer policy as the document and fails as absence, not exception; the static extension forwarders are obsolete, and the interface cast is the route.
- The mutability flag is a read-side policy with a write-side consequence: tables read immutable cannot be enriched in place — flows that annotate features before re-emission either admit mutable tables or rebuild the feature with a new table, and the profile decides which, because rebuild preserves the original document while in-place mutation does not.
- Two partner profiles with different id conventions are two factory rows on two options instances — never one options instance with post-read id patching, which would re-open the id law per flow.
- Property bags stay JSON-typed until projected: numbers, nested objects, and arrays remain element-backed, and the loose surface (`GetOptionalValue`, `GetNames`, `GetValues`, `Exists`) exists for inspection only — the law is project-to-typed-record at admission, never walk the loose table in domain code.
- The element-backed table's `Count` re-enumerates the object per call — counting in a loop is quadratic by accident; the typed projection path never touches it.
- Collections read as one feature sequence and write feature-by-feature under the same bbox policy as single features — collection handling adds no policy of its own, so a collection profile is exactly the feature profile applied N times.
- Feature `bbox` round-trips: a wire `bbox` lands on the feature's bounding box at read and re-emits at write when present — precomputed extents survive the hop for consumers that index on them without geometry decode.
- The bbox is planar — a four-number envelope with no vertical range — so Z-bearing data carries its vertical extent in the payload only, and extent-indexed consumers see 2D bounds regardless of dimensionality.
- The id-lift is asymmetric on collision: a foreign feature carrying both a wire `id` and a property named identically to the id attribute cannot keep both through a round trip — the id attribute name is chosen to be collision-free against the property vocabulary, which is exactly what the deliberately ugly default sentinel name is for.

## GeoPackage blob codec

- The package surface is exactly the blob codec: a reader and writer over the GeoPackage binary layout — a header (magic `GP`, version, flags byte, SRID, optional envelope) followed by a WKB body.
- The flags byte packs endianness (bit 0), envelope kind (bits 1–3: none, XY, XYZ, XYM, XYZM — the Z/M kinds carrying min/max range pairs), and an empty-geometry flag (bit 4).
- The writer always emits little-endian; the reader honors the flag — cross-producer blobs decode regardless of origin endianness.
- Raw WKB readers cannot parse the blob — the header defeats them — and the `GP` magic at offset zero is the signature gate that discriminates a GeoPackage blob from raw WKB before any typed decode begins.
- Both codec directions take `byte[]` or `Stream` — column values decode without intermediate copies, and large geometries stream.
- The stream arms close the supplied stream on completion — the codec wraps it in an owning binary reader or writer — so callers hand the codec a dedicated stream per blob, never a shared or positioned one they expect back open.
- Reader policy is three knobs, each a declared row: `HandleSRID`, `RepairRings`, `HandleOrdinates`.
- `HandleSRID` stamps the header SRID onto the decoded geometry; the inner WKB read runs with its own SRID handling off, so the header — not the WKB body — is the single SRID authority.
- With `HandleSRID` off, decoded geometries carry no spatial reference at all — the off-state is for flows that overwrite SRID from container metadata anyway, never a default to leave unexamined.
- `RepairRings` forwards to the WKB layer for foreign producers with invalid rings — admission-side repair, so interior code never holds an unrepaired polygon.
- Repair is a tolerance declaration, not a default posture: a profile that repairs silently also accepts that round-tripping a repaired blob re-emits different bytes than it admitted — byte-identity flows and repair are mutually exclusive rows.
- `HandleOrdinates` caps decoded dimensions, intersected with what the coordinate-sequence factory supports (`AllowedOrdinates` is the mask of XYZM against the factory's capability) — the cap cannot exceed what the interior representation can hold.
- A reader constructor arm takes the coordinate-sequence factory and precision model — blob admission applies the same precision-at-admission law as the text rail, from the same kind of declaration.
- Empty geometries are header-coded: the writer sets the empty flag and encodes an empty point as NaN ordinates; the reader maps the empty flag on a point body to a factory-made empty point — empty round-trips by contract and consumers never see NaN coordinates.
- The empty-flag remap is point-specific by design: non-point empties decode naturally from their WKB bodies, while a NaN-coded point would otherwise decode as a NaN-coordinate point — the flag exists to repair exactly that case.
- The header is a standalone read/write pair over the binary layout — its parse does not touch the WKB body, which is what licenses header-only flows: SRID audits, envelope harvesting, and emptiness checks across a column at blob-scan cost.
- XYZM-kind envelopes carry Z and M range pairs beside the XY extent — vertical and measure bounds are available header-only where the writer's ordinate policy included them.
- Writer policy is one knob: `HandleOrdinates` restricts written dimensions within XYZM and selects the header's envelope kind.
- The written header SRID derives from the geometry's interior SRID, and the WKB body is written without any embedded SRID — the header is the blob's only SRID carrier, so interior SRID hygiene at write time is what the read-side header authority depends on.
- The body's Z/M emission flags derive from the same ordinate policy as the envelope kind — body dimensionality and header envelope kind cannot disagree by construction, which is what makes header-only dimensionality audits trustworthy.
- The header envelope is the cheap filter readers and indexers use without parsing WKB — writing it is what makes spatial-index maintenance and bbox scans header-only operations.

## GeoPackage container law

- The container is an embedded SQLite store with a declared metadata spine: a spatial-reference table, a contents table enumerating layers, and a geometry-columns table binding each feature table to exactly one geometry column and SRID.
- The container self-identifies before its tables do: a declared application identifier in the store's header marks the file as a geo container — the file-level signature gate that precedes metadata reads, mirroring the blob-level magic gate one level down.
- Undefined spatial reference is a declared state, not an absent one: the spec reserves identifiers for undefined geographic and undefined cartesian reference systems, so an admission gate distinguishes declared-undefined from missing-row — only the latter is a spine integrity fault.
- A vector layer is therefore three metadata rows plus a feature table — layer creation, enumeration, and discovery are row operations over the spine, with the blob codec handling only geometry column values.
- Store mechanics — connection, pragmas, transactions, atomic write — are the embedded-store law composed, never re-taught at this altitude.
- SRID discipline is referential: the geometry-columns row, the header SRID in every blob, and the spatial-reference table must agree.
- The admission gate for a foreign container verifies the declared SRID against sampled blob headers — desktop producers are known to write disagreeing values, and the disagreement is detectable only by sampling.
- The spatial index is an R-tree side table per indexed geometry column, kept consistent with the feature table by triggers and advertised through the container's extensions table.
- The R-tree keys the feature table's integer primary key — feature tables declare one by spec, and composite or text keys break the index correspondence; the id column is the join axis between candidate sets and feature rows.
- Layer extents denormalize into the contents row — discovery-time bounds without scanning — and stale extents mislead every consumer that trusts discovery, so write paths either maintain them or mark them unknown; a half-maintained extent is worse than none.
- The index stores envelopes — exactly what the blob header already carries — so index maintenance and rebuild read headers only, never full WKB.
- Query shape is two-phase by construction: candidate ids from the R-tree by envelope overlap, then exact predicates on decoded geometries for the candidate set only — decoding the full table to answer a spatial predicate is the foreclosed plan.
- The exact-predicate phase is mandatory, not an optimization choice: envelope overlap false-positives grow with geometry elongation and diagonal orientation, so envelope-only answers are wrong on real data in proportion to shape irregularity.
- A layer write is one transaction over three surfaces — feature row, index row, contents extent — composing the embedded store's transaction law; partial writes that update the feature but not the index are the consistency fault the single transaction forecloses.
- One geometry column per feature table is a modeling constraint with a modeling answer: multi-geometry entities become multiple layers sharing the id key, joined on read — widening a feature table with a second geometry column is the spine violation, not a shortcut.
- Round-trip fidelity is a verifiable matrix, not an assumption: empty geometries (flag-coded), Z/M presence (capped by writer ordinates and header kind), SRID (header-stamped), precision (reader model) — each has a declared behavior.
- A container exchange profile pins all four matrix axes, and the admission gate checks them on a sample before bulk decode.

## divergent — geojson-stj-rail

- The rail instantiates wire-family law for an open foreign hierarchy: the `type` member is a literal discriminant mirroring the geometry family one-to-one, absent geometry is explicit null, and coordinates are positional arrays.
- The discriminant-first, literal-tagged, explicit-null shape arrives from the format itself — which is why the family needs no hand-written discriminator and the factory is the entire registration surface.
- Maximal unification across feature flows: one factory row serves geometry-only payloads, single features, and whole collections — there is no separate feature serializer to configure.
- The profile collapse spans both rails: one geo exchange profile value carries the factory's five policies, the blob codec's knobs, and the container expectations — text emission, blob coding, and container admission all derive from the one profile, so a precision or ordinate decision changes every projection at once.
- A collection endpoint, a geometry column inside a JSON document, and a properties-typed feature feed are all the same options object exercised at different roots; the growth axis is the factory's constructor policy, and every flow inherits a policy change at once.
- Failure-mode taxonomy with boundaries: structural rejections (the `JsonException` family) are wire faults carrying document position; semantic degradations (M dropped, XYM flattened, precision quantized at admission) are lossy-by-design and exist only in the profile declaration; policy violations (mutating a read-only attributes table) are local contract faults.
- Only the structural class is detectable by a consumer at runtime — the degradation class must be declared in the exchange profile, which is why the profile is the contract artifact, not the code.
- The standard rejection channel arrives position-enriched — document path and location ride the wire-fault receipt — while the escaped argument-fault carries no position, a second reason the type-literal escape is gated at the boundary rather than diagnosed downstream.
- Performance posture: converters stream against the UTF-8 reader and writer except feature properties, which buffer one element subtree — memory scales with the largest property bag, not the document; collections stream feature-by-feature on write.
- Large-geometry payloads dominate as coordinate-array tokens, so wire-size optimization is precision reduction at admission via the fixed-precision factory, not formatting tricks at emission.
- Rejected: hand-rolled geometry-shape DTOs serialized by the generated context — forecloses the entire interior predicate surface and forks the vocabulary.
- Rejected: string-assembled GeoJSON — forecloses escaping, structural validity, and the rejection taxonomy.
- Rejected: per-type converter registration — forecloses the factory's policy coherence.
- Rejected: emitting CRS members for projected data — forecloses interoperability, because every conforming reader assumes WGS84.
- The byte-identity edge: because emission writes raw doubles, two reads of the same wire text under the same admission factory produce identical interior values, but re-emission is byte-identical only when admission did not quantize — a pass-through that must preserve foreign bytes exactly keeps the raw payload and never round-trips through the vocabulary.
- Geometry-only payloads embed anywhere STJ reaches: a geometry-typed property inside an ordinary contract record serializes through the factory row with zero feature ceremony — geo fields in non-geo contracts are the same one registration, which is the absorption proof for the options-merge composition.

## divergent — geopackage-container

- One container law absorbs the layer family: a layer is rows — contents, geometry-columns, optional index registration — plus a feature table; new layers, new attribute columns, and new indexes are all row insertions against the metadata spine with zero new code shape.
- Container enumeration is a metadata query, which makes the container the geo instance of additive-tolerant growth: consumers discover layers by reading the spine, so an added layer is invisible to existing readers and immediately visible to discovery.
- The blob codec is policy-frozen per container profile: ordinate caps, SRID handling, ring repair, and precision form one declared codec row reused for every geometry column in the container.
- The codec is lawful without the spine: a private embedded geometry column read only in-process may use the blob format bare — the metadata spine is the interop contract for foreign consumers, not a storage requirement — and the upgrade path from private column to published layer is adding spine rows, not re-encoding data.
- Container-versus-text selection is a routing table: the container for many layers, random access, and spatial indexing; the text rail for streaming consumption, human inspection, and web consumers; the same profile-derived fidelity on both, so the choice is access pattern, never fidelity.
- Per-column codec drift — one column XYZ, another silently XY-capped — is the corruption class the single profile row forecloses.
- Index maintenance composes the header design: insert and update paths write the blob (envelope included) and upsert the R-tree row from the same envelope without re-decoding.
- The envelope used for the index row and the envelope written into the header come from one interior computation — computing it once per write and feeding both consumers is the collapse; computing extents twice (once for the header, once for the index) is the drift seam.
- Bulk loads drop and rebuild the index after the load — per-row trigger maintenance dominates bulk insertion cost, and the rebuild reads only headers.
- The header-envelope-as-index-feed loop is the reason envelope kind is never set below the data's true dimensionality — an absent envelope (kind zero) forces full WKB decode for every index operation.
- Cross-projection composition: a container layer exported to a text feed is blob-decode (header-gated, SRID-stamped) → interior geometry → GeoJSON emission under the factory profile; the reverse admits text features, validates CRS posture, and encodes blobs under the container's codec row.
- Both directions pass through the one interior vocabulary, so fidelity guarantees compose from the two profiles independently — there is no third conversion profile to maintain, and a profile change on either side is visible to the other only through the vocabulary.
- Rejected: raw WKB columns standing in for GeoPackage blobs — foreclose header SRID authority, the empty-flag contract, and envelope-fed indexing.
- Rejected: per-feature GeoJSON text columns in the embedded store — foreclose typed predicates, indexing, and blob economics.
- Rejected: a second geometry column on one feature table — forecloses the one-column binding the metadata spine declares.
- Rejected: sidecar-bundle formats as the container — foreclose the single-file atomicity the embedded store provides.
- The container as delivery arm: a finished container file is a single-artifact deliverable in the destination union — its provenance row is the file path plus content hash, its admission gate is the application-identifier check plus spine verification, and its layer set is self-describing — which makes it the geo analogue of the schema-stamped tabular artifact with the stamp built into the spine.
- Failure taxonomy for container admission, in gate order: not-a-container (identifier mismatch — reject before any table read), spine violation (missing metadata tables or unbound geometry column — structural reject), SRID disagreement (declared versus sampled headers — evidence-bearing reject or declared-tolerance repair), index staleness (R-tree rows disagree with envelopes — repairable by rebuild, never silently trusted).
- The gate order is cheapest-first by construction — each verification step costs strictly less than the decode step it guards, so rejection cost is proportional to how wrong the artifact is, and a hostile or garbage file is rejected at file-header cost.
- Trigger-maintained indexing and bulk rebuild are mutually exclusive within one load: disabling triggers without rebuilding afterward is the stale-index source, so the load protocol declares one mode per transaction and the admission gate's staleness check is what catches protocol violations after the fact.
- GeoJSON text artifacts participate in the delivery algebra like any artifact: JSON text has no native metadata channel, so the stamp rides the delivery envelope and the hash covers the bytes — and raw-double emission is what makes that hash stable across re-emissions of unchanged geometry.
