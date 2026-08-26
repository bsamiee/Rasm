# [CORE_FRAME]

`Frame` owns bounded artifact reassembly, residency admission, and IFC container admission. Interleaved bands fold by artifact and generation under one ingress budget, verification gates the joined allocation, and each residency manifest replaces whole against the producer's pinned cluster roster and the budget it declares. Module `core/src/interchange/frame.ts` admits an arrival class as one refusal row, a residency payload as one kind row, and an IFC serialization as one admission row.

`Frame` composes the `value` floor's `Digest` identity and `Shape.Ingress` ceilings, the `codec` owner's fault, gap, parity, and quarantine policies, and the `format` owner's proto suite and JSON schema mints. Producers own every payload axis crossing this plane, so `Frame` folds arrivals into artifacts, views, censuses, and admissions and mints no payload axis of its own.

## [01]-[INDEX]

- [02]-[FRAME_PROTOCOL]: bounded keyed frame assembly and sequence evidence; `Frame.Artifact`.
- [03]-[KEY_VERIFY]: delegated verification and single-allocation joins; `Frame.Artifact`.
- [04]-[RESIDENCY_MANIFEST]: viewport manifest admission, per-kind census, and budget grading; `Frame.Residency`.
- [05]-[IFC_ADMISSION]: serialization rows and their two direction verdicts, the sparse container and release crossings, the release-header read; `Frame.Ifc`.

## [02]-[FRAME_PROTOCOL]

- Owner: `Frame.Artifact` folds interleaved dense frame bands by artifact and generation.
- Law: ordinal gaps, total drift, budget overruns, and unfinished tails emit `Wire.Fault` evidence.
- Law: raw bands remain unchanged until the verified single-allocation join.
- Law: `Shape.Ingress` owns frame and byte ceilings.
- Packages: `effect`; `./codec.ts` (`Wire`); `./format.ts` (`Format`); value `Digest` and `Shape`.

```typescript
import type { MessageShape, MessageValidType } from "@bufbuild/protobuf"
import { Array, Chunk, Effect, Either, Encoding, HashMap, Option, pipe, Ref, Schema, Stream } from "effect"
import { Digest } from "../value/contentKey.ts"
import { Shape } from "../value/schema.ts"
import { Wire } from "./codec.ts"
import { Format } from "./format.ts"

class ArtifactBand extends Schema.Class<ArtifactBand>("ArtifactBand")({
  artifact: Digest.codecs.content.bytes,
  generation: Shape.Refined.OrdinalKey,
  ordinal: Shape.Refined.OrdinalKey,
  total: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
  band: Schema.Uint8ArrayFromSelf,
}) {}

type _Held = { readonly expect: number; readonly extent: number; readonly total: number; readonly bands: Chunk.Chunk<Uint8Array> }
type _Coordinate = `${Digest.Key<"content">}:${number}`
type _State = { readonly seen: number; readonly held: HashMap.HashMap<_Coordinate, _Held> }
type _Emit = Either.Either<
  Option.Option<{ readonly key: Digest.Key<"content">; readonly generation: number; readonly bands: Chunk.Chunk<Uint8Array> }>,
  Wire.Fault
>

const _SEED: _State = { seen: 0, held: HashMap.empty() }

const _overrun = (
  family: Wire.FaultFamily,
  axis: Wire.OverrunAxis,
  actual: number,
  expected: number,
  at: Option.Option<{ readonly artifact: Digest.Key<"content">; readonly generation: number }>,
): Wire.Fault => new Wire.Fault({ family, case: { reason: "overrun", axis, actual, expected, at } })

const _coordinateOf = (key: Digest.Key<"content">, generation: number): _Coordinate => `${key}:${generation}`
const _coordinate = (frame: ArtifactBand): _Coordinate => _coordinateOf(frame.artifact, frame.generation)
const _next = (state: _State, held: HashMap.HashMap<_Coordinate, _Held>): _State => ({ seen: state.seen + 1, held })

type _OpenRefusal = { readonly when: (frame: ArtifactBand) => boolean; readonly fault: (frame: ArtifactBand) => Wire.Fault }
type _HeldRefusal = {
  readonly when: (frame: ArtifactBand, held: _Held) => boolean
  readonly fault: (frame: ArtifactBand, held: _Held) => Wire.Fault
}

const _gathered = (budget: Shape.Ingress) => {
  const opening: ReadonlyArray<_OpenRefusal> = [
    {
      when: (frame) => frame.ordinal !== 0,
      fault: (frame) => Wire.Gap.evidence("ArtifactAssembly", "ordinal", 0n, BigInt(frame.ordinal)),
    },
    {
      when: (frame) => frame.band.length > budget.bytes,
      fault: (frame) => _overrun("ArtifactAssembly", "assembly", frame.band.length, budget.bytes, Option.some(frame)),
    },
  ]
  const holding: ReadonlyArray<_HeldRefusal> = [
    {
      when: (frame, held) => frame.ordinal !== held.expect,
      fault: (frame, held) => Wire.Gap.evidence("ArtifactAssembly", "ordinal", BigInt(held.expect), BigInt(frame.ordinal)),
    },
    {
      when: (frame, held) => frame.total !== held.total,
      fault: (frame, held) => Wire.Gap.evidence("ArtifactAssembly", "total", BigInt(held.total), BigInt(frame.total)),
    },
    {
      when: (frame, held) => held.extent + frame.band.length > budget.bytes,
      fault: (frame, held) =>
        _overrun("ArtifactAssembly", "assembly", held.extent + frame.band.length, budget.bytes, Option.some(frame)),
    },
  ]
  return (state: _State, frame: ArtifactBand): readonly [_State, _Emit] =>
    state.seen >= budget.frames
      ? ([_next(state, state.held), Either.left(_overrun("ArtifactAssembly", "frames", state.seen + 1, budget.frames,
          Option.some(frame)))] as const)
      : Option.match(HashMap.get(state.held, _coordinate(frame)), {
          onNone: () =>
            Option.match(Option.map(Array.findFirst(opening, (row) => row.when(frame)), (row) => row.fault(frame)), {
              onSome: (fault) => [_next(state, state.held), Either.left(fault)] as const,
              onNone: () =>
                frame.total === 1
                  ? ([_next(state, state.held), Either.right(Option.some({ key: frame.artifact, generation: frame.generation, bands: Chunk.of(frame.band) }))] as const)
                  : ([
                      _next(state, HashMap.set(state.held, _coordinate(frame), {
                        expect: 1,
                        extent: frame.band.length,
                        total: frame.total,
                        bands: Chunk.of(frame.band),
                      })),
                      Either.right(Option.none()),
                    ] as const),
            }),
          onSome: (held) =>
            Option.match(
              Option.map(Array.findFirst(holding, (row) => row.when(frame, held)), (row) => row.fault(frame, held)),
              {
                onSome: (fault) => [_next(state, HashMap.remove(state.held, _coordinate(frame))), Either.left(fault)] as const,
                onNone: () =>
                  frame.ordinal + 1 === held.total
                    ? ([
                        _next(state, HashMap.remove(state.held, _coordinate(frame))),
                        Either.right(Option.some({ key: frame.artifact, generation: frame.generation, bands: Chunk.append(held.bands, frame.band) })),
                      ] as const)
                    : ([
                        _next(state, HashMap.set(state.held, _coordinate(frame), {
                          ...held,
                          expect: held.expect + 1,
                          extent: held.extent + frame.band.length,
                          bands: Chunk.append(held.bands, frame.band),
                        })),
                        Either.right(Option.none()),
                      ] as const),
              },
            ),
        })
}
```

## [03]-[KEY_VERIFY]

- Owner: `Frame.Artifact` verifies held bands before joining and emits the verified `Artifact` beside its octets.
- Law: verification covers the complete artifact, and parity failure prevents the joined allocation.
- Exemption: `_joined` performs one bounded allocation and ordinal copy.

```typescript
class Artifact extends Schema.Class<Artifact>("Artifact")({
  key: Digest.codecs.content.bytes,
  generation: Shape.Refined.OrdinalKey,
  extent: Shape.Refined.OrdinalKey,
  frames: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
}) {}

const _joined = (bands: Chunk.Chunk<Uint8Array>): Uint8Array => {
  const extent = Chunk.reduce(bands, 0, (total, band) => total + band.length)
  const out = new Uint8Array(extent)
  let at = 0
  for (const band of bands) {
    out.set(band, at)
    at += band.length
  }
  return out
}

const _verifiedArtifact = (
  key: Digest.Key<"content">,
  generation: number,
  bands: Chunk.Chunk<Uint8Array>,
): Effect.Effect<readonly [Artifact, Uint8Array], Wire.Fault> =>
  Wire.Parity.verified("ArtifactAssembly", key, bands).pipe(
    Effect.map(() => {
      const octets = _joined(bands)
      return [new Artifact({ key, generation, extent: octets.length, frames: Chunk.size(bands) }), octets] as const
    }),
  )

const _unfinished = (state: _State): ReadonlyArray<_Emit> =>
  Array.map(Array.fromIterable(HashMap.values(state.held)), (held) =>
    Either.left(Wire.Gap.evidence("ArtifactAssembly", "tail", BigInt(held.total), BigInt(held.expect))))

const _artifactWire = <F, FI, S, SI>(
  format: Schema.Schema<F, FI>,
  revision: Schema.Schema<S, SI>,
) => Schema.Struct({
  format,
  bytes: Schema.Uint8ArrayFromBase64,
  schema: revision,
  content: Digest.codecs.content.wire,
  at: Schema.DateTimeUtc,
})

const ArtifactAssembly: {
  readonly Frame: typeof ArtifactBand
  readonly Artifact: typeof Artifact
  readonly wire: typeof _artifactWire
  readonly frame: Schema.Schema<Frame, Uint8Array>
  readonly reassembled: <E, R>(
    frames: Stream.Stream<ArtifactBand, E, R>,
    budget?: Shape.Ingress,
  ) => Stream.Stream<Either.Either<readonly [Artifact, Uint8Array], Wire.Fault>, E, R>
} = {
  Frame: ArtifactBand,
  Artifact,
  wire: _artifactWire,
  frame: Format.msgpack.schema(ArtifactBand),
  reassembled: (frames, budget = Shape.Ingress.floor) =>
    Stream.unwrap(
      Ref.make(_SEED).pipe(
        Effect.map((state) => {
          const step = _gathered(budget)
          return frames.pipe(
            Stream.mapEffect((frame) =>
              Ref.modify(state, (current) => {
                const [next, emit] = step(current, frame)
                return [emit, next] as const
              })),
            Stream.concat(Stream.fromEffect(Ref.get(state)).pipe(Stream.flatMap((settled) => Stream.fromIterable(_unfinished(settled))))),
            Stream.filterMap((emit) =>
              Either.match(emit, {
                onLeft: (fault) => Option.some(Effect.succeed(Either.left(fault))),
                onRight: (held) => Option.map(held, (ready) =>
                  Effect.either(_verifiedArtifact(ready.key, ready.generation, ready.bands))),
              })),
            Stream.mapEffect((settle) => settle, { concurrency: 1 }),
          )
        }),
      ),
    ),
}
```

## [04]-[RESIDENCY_MANIFEST]

- Owner: `Frame.Residency` admits the producer's viewport residency manifest and grades it against the budget it declares.
- Law: the manifest REPLACES — the producer mints the whole resident tile set for one viewpoint on every emission, so no held state exists for an arrival to patch and this crossing carries no delta arm.
- Law: duplicate content keys refuse, and the collection ceiling is `Shape.Ingress`; every other field rule is the corpus's `buf.validate` rule on the generated `GeometryResidency`, evaluated once at the descriptor admission.
- Law: `kind` decides cull and draw posture, and the same rows carry the per-kind census the declared VRAM budget is judged against.

```typescript
import * as appuiResidency from "@rasm\/contracts/rasm/contracts/render/residency_pb"

const _payloadStream = <A>(
  family: Wire.FaultFamily,
  schema: Schema.Schema<A, Uint8Array>,
  frames: AsyncIterable<Uint8Array>,
): Stream.Stream<Either.Either<A, Wire.Fault>, Wire.Fault, Wire.Quarantine> =>
  Stream.fromAsyncIterable(frames, (defect) =>
    new Wire.Fault({ family, case: { reason: "malformed", at: "source", issue: String(defect) } })).pipe(
    Stream.mapEffect((octets) =>
      Schema.decodeUnknown(schema)(octets).pipe(
        Effect.mapError((issue) =>
          new Wire.Fault({ family, case: { reason: "malformed", at: "decode", issue: issue.message } })),
        Wire.Quarantine.divert({ family, octets: () => octets }),
      ), { concurrency: 1 }),
  )


const _kinds = [
  appuiResidency.ResidencyKind.MESHLET_CLUSTER,
  appuiResidency.ResidencyKind.QUANTIZED_VERTEX,
  appuiResidency.ResidencyKind.POINT_SPLAT,
  appuiResidency.ResidencyKind.GAUSSIAN_SPLAT,
] as const
const _kind = Schema.is(Schema.Literal(..._kinds))

const _kindRows = {
  [appuiResidency.ResidencyKind.MESHLET_CLUSTER]: { coneCullable: true, splatBorne: false },
  [appuiResidency.ResidencyKind.QUANTIZED_VERTEX]: { coneCullable: false, splatBorne: false },
  [appuiResidency.ResidencyKind.POINT_SPLAT]: { coneCullable: false, splatBorne: false },
  [appuiResidency.ResidencyKind.GAUSSIAN_SPLAT]: { coneCullable: false, splatBorne: true },
} as const satisfies { readonly [K in Residency.Kind]: { readonly coneCullable: boolean; readonly splatBorne: boolean } }

type _Wire = MessageShape<typeof appuiResidency.GeometryResidencySchema>
type _Landed = MessageValidType<typeof appuiResidency.GeometryResidencySchema>
type _Tile = _Landed["tiles"][number] & { readonly kind: Residency.Kind }

const Manifest: Schema.Schema<Residency.Manifest, _Wire> = Format.proto.message(appuiResidency.GeometryResidencySchema).pipe(
  Schema.filter((manifest): manifest is Residency.Manifest => Array.every(manifest.tiles, (tile) => _kind(tile.kind)), {
    identifier: "ResidencyKinds",
  }),
  Schema.filter((manifest) => manifest.tiles.length <= Shape.Ingress.floor.collection || "<residency-collection>"),
  Schema.filter((manifest) =>
    Array.dedupe(Array.map(manifest.tiles, (tile) => Encoding.encodeHex(tile.artifact.artifactId))).length === manifest.tiles.length
      || "<duplicate-residency-key>"),
)

declare namespace Residency {
  type Kind = (typeof _kinds)[number]
  type Manifest = Omit<_Landed, "tiles"> & { readonly tiles: ReadonlyArray<_Tile> }
  type Tile = _Tile
  type Viewpoint = _Landed["viewpoint"]
  type Meshlet = _Tile["meshlets"][number]
  type StreamRow = _Tile["streams"][number]
  type Tally = { readonly count: number; readonly bytes: bigint; readonly meshlets: number }
  type Census = { readonly [K in Kind]: Tally }
  type View = { readonly manifest: Manifest; readonly resident: bigint; readonly census: Census }
  type _Kinds<T extends Record<Kind, { readonly coneCullable: boolean; readonly splatBorne: boolean }> = typeof _kindRows> = T
}

const _EMPTY_TALLY: Residency.Tally = { count: 0, bytes: 0n, meshlets: 0 }

const _EMPTY_CENSUS: Residency.Census = {
  [appuiResidency.ResidencyKind.MESHLET_CLUSTER]: _EMPTY_TALLY,
  [appuiResidency.ResidencyKind.QUANTIZED_VERTEX]: _EMPTY_TALLY,
  [appuiResidency.ResidencyKind.POINT_SPLAT]: _EMPTY_TALLY,
  [appuiResidency.ResidencyKind.GAUSSIAN_SPLAT]: _EMPTY_TALLY,
}


const _tallied = (manifest: Residency.Manifest): Residency.View =>
  Array.reduce(
    manifest.tiles,
    { manifest, resident: 0n, census: _EMPTY_CENSUS } satisfies Residency.View,
    (view, tile) =>
      pipe(view.census[tile.kind], (tally) => ({
        manifest,
        resident: view.resident + tile.artifact.artifactBytes,
        census: {
          ...view.census,
          [tile.kind]: {
            count: tally.count + 1,
            bytes: tally.bytes + tile.artifact.artifactBytes,
            meshlets: tally.meshlets + tile.meshlets.length,
          },
        },
      })),
  )

const _admitted = (manifest: Residency.Manifest): Either.Either<Residency.View, Wire.Fault> =>
  pipe(_tallied(manifest), (view) =>
    view.resident > manifest.vramBudget
      ? Either.left(_overrun("GeometryResidency", "<residency-vram-budget>", Number(view.resident), Number(manifest.vramBudget), Option.none()))
      : Either.right(view))

function _admit(manifest: Residency.Manifest): Either.Either<Residency.View, Wire.Fault>
function _admit<E, R>(arrivals: Stream.Stream<Residency.Manifest, E, R>): Stream.Stream<Either.Either<Residency.View, Wire.Fault>, E, R>
function _admit<E, R>(
  input: Residency.Manifest | Stream.Stream<Residency.Manifest, E, R>,
): Either.Either<Residency.View, Wire.Fault> | Stream.Stream<Either.Either<Residency.View, Wire.Fault>, E, R> {
  return Stream.isStream(input) ? Stream.map(input, _admitted) : _admitted(input)
}

const _envelope: Schema.Schema<Residency.Manifest, Uint8Array> = Format.proto.family(
  appuiResidency.GeometryResidencySchema,
  Manifest,
  "json",
)

const Residency: {
  readonly Manifest: typeof Manifest
  readonly kinds: typeof _kinds
  readonly kind: typeof _kindRows
  readonly envelope: typeof _envelope
  readonly stream: (
    frames: AsyncIterable<Uint8Array>,
  ) => Stream.Stream<Either.Either<Residency.Manifest, Wire.Fault>, Wire.Fault, Wire.Quarantine>
  readonly admit: typeof _admit
  readonly cullable: (view: Residency.View) => ReadonlyArray<Residency.Tile>
  readonly splatBorne: (view: Residency.View) => ReadonlyArray<Residency.Tile>
} = {
  Manifest,
  kinds: _kinds,
  kind: _kindRows,
  envelope: _envelope,
  stream: (frames) => _payloadStream("GeometryResidency", _envelope, frames),
  admit: _admit,
  cullable: (view) => Array.filter(view.manifest.tiles, (tile) => _kindRows[tile.kind].coneCullable),
  splatBorne: (view) => Array.filter(view.manifest.tiles, (tile) => _kindRows[tile.kind].splatBorne),
}

```

## [05]-[IFC_ADMISSION]

- Owner: `Frame.Ifc` owns the IFC wire form — serialization rows, the container column, and the release roster both cross sparsely.
- Law: Serialization and container stay separate axes whose product is SPARSE on both crossings — a wrapper names the serializations it carries and never the text inside them, so `zip` admits STEP and XML alone, and each serialization names the releases it publishes for, so `Ifc.admits` refuses an unpublished pair by row where a cross product admits a document no schema validates.
- Law: ADMISSION and PUBLICATION are two verdicts over one row set — `Ifc.admits` grades the bytes this decoder reads, `Ifc.seals` grades what a producer may write, and one fused predicate either forfeits a lawful read or authorizes a form no producer of this repo mints.
- Law: Direction rides the `refusal` cell, and the cell QUOTES its producer rather than re-deriving it — `libs/dotnet/Rasm.Bim/.planning/Projection/wireform.md` `IfcSerialization.Refusal` names `ifcx` unproduced, its `Published` reads that cell first and fails whatever release follows, and its `Sniff` still admits the span.
- Law: `IFC4X3` publishes under no serialization — the ISO-approved 4.3 line carries the `IFC4X3_ADD2` identifier and every published 4.3 artifact spells it, so the roster keeps the token to NAME that refusal rather than failing an unknown literal at decode.
- Law: `ifcx` is IFC5's own encoding rather than a `json` release — a document there carries its release at `header.ifcxVersion` where ifcJSON reads `schemaIdentifier`, so folding IFC5 onto the JSON row forks two vocabularies into one member read; that `json` row itself publishes against a community-maintained schema where `step` and `xml` publish against the ISO editions, a weaker claim its consumers price at the boundary.
- Law: `ifcx` reaches this branch from a foreign producer alone — the repo's own producer maps IFC5 to no writer, so the row admits on the read side and refuses on the seal side, stated as a cell on both ends rather than a shape one end silently lacks.
- Law: Each serialization declares the header member carrying its release, so admission reads a named member rather than guessing.
- Law: `sniff` prices the release read, and an inflating container raises its serialization's extent to the whole document.
- Law: Rows decide selection, admission, publication, and `degrade` alone — a wire form realizes no tenancy and ends no lifetime.
- Growth: a serialization is one `_ifcRows` row, a container one `_ifcContainerRows` row, a release one roster entry beside the row cells that publish it; a producer refusal is one `refusal` cell, never an absent row.
- Boundary: `Frame` carries the wire form and its descriptor; entity-graph authoring and release raising stay with the producing runtime.
- Packages: `effect` (`Array`, `Either`, `Option`, `Schema`); `./format.ts` (`Format`).

```typescript
const _ifcSerializations = ["step", "xml", "json", "ifcx"] as const
const _ifcContainers = ["plain", "zip"] as const
const _ifcReleases = ["IFC2X3", "IFC4", "IFC4X1", "IFC4X3", "IFC4X3_ADD2", "IFC5"] as const

const _ifcSniffs = ["line", "element", "document"] as const

const _ifcRows = {
  step: {
    extension: ".ifc",
    header: "FILE_SCHEMA",
    sniff: "line",
    releases: ["IFC2X3", "IFC4", "IFC4X1", "IFC4X3_ADD2"],
    refusal: Option.none<string>(),
    degrade: "<release-unknown-before-header-line>",
  },
  xml: {
    extension: ".ifcxml",
    header: "xmlns",
    sniff: "element",
    releases: ["IFC2X3", "IFC4", "IFC4X1", "IFC4X3_ADD2"],
    refusal: Option.none<string>(),
    degrade: "<release-unknown-before-root-element>",
  },
  json: {
    extension: ".ifcjson",
    header: "schemaIdentifier",
    sniff: "document",
    releases: ["IFC4"],
    refusal: Option.none<string>(),
    degrade: "<release-unknown-before-whole-document>",
  },
  ifcx: {
    extension: ".ifcx",
    header: "header.ifcxVersion",
    sniff: "document",
    releases: ["IFC5"],
    refusal: Option.some("ifc-form-unproduced"),
    degrade: "<release-carried-as-ifcx-version>",
  },
} as const satisfies {
  readonly [S in Ifc.Serialization]: {
    readonly extension: string
    readonly header: string
    readonly sniff: Ifc.Sniff
    readonly releases: Array.NonEmptyReadonlyArray<Ifc.Release>
    readonly refusal: Option.Option<string>
    readonly degrade: string
  }
}

const _ifcContainerRows = {
  plain: {
    extension: Option.none(),
    inflates: false,
    wraps: _ifcSerializations,
    degrade: "<none>",
  },
  zip: {
    extension: Option.some(".ifczip"),
    inflates: true,
    wraps: ["step", "xml"],
    degrade: "<sniff-extent-raised-to-document>",
  },
} as const satisfies {
  readonly [C in Ifc.Container]: {
    readonly extension: Option.Option<string>
    readonly inflates: boolean
    readonly wraps: Array.NonEmptyReadonlyArray<Ifc.Serialization>
    readonly degrade: string
  }
}

const _IfcForm = Schema.Struct({
  serialization: Schema.Literal(..._ifcSerializations),
  container: Schema.Literal(..._ifcContainers),
})

const _ifcDescriptor = (form: Ifc.Form): Ifc.Descriptor => {
  const row = _ifcRows[form.serialization]
  const wrapper = _ifcContainerRows[form.container]
  return {
    extension: Option.getOrElse(wrapper.extension, () => row.extension),
    header: row.header,
    sniff: wrapper.inflates ? "document" : row.sniff,
  }
}

const _ifcAdmits = (form: Ifc.Form, release: Ifc.Release): Either.Either<Ifc.Form, string> =>
  !Array.contains(_ifcContainerRows[form.container].wraps, form.serialization)
    ? Either.left(`<ifc-form-uncontained:${form.container}:${form.serialization}>`)
    : Array.contains(_ifcRows[form.serialization].releases, release)
    ? Either.right(form)
    : Either.left(`<ifc-form-unpublished:${form.container}:${form.serialization}:${release}>`)

const _ifcSeals = (form: Ifc.Form, release: Ifc.Release): Either.Either<Ifc.Form, string> =>
  Option.match(_ifcRows[form.serialization].refusal, {
    onSome: (detail) => Either.left(`<${detail}:${form.container}:${form.serialization}:${release}>`),
    onNone: () => _ifcAdmits(form, release),
  })

const _IfcWire = ArtifactAssembly.wire(_IfcForm, Schema.Literal(..._ifcReleases)).pipe(
  Schema.filter((payload) =>
    Either.match(_ifcAdmits(payload.format, payload.schema), { onLeft: (detail) => detail, onRight: () => true })),
)

const Ifc: {
  readonly Wire: typeof _IfcWire
  readonly Form: typeof _IfcForm
  readonly serializations: typeof _ifcSerializations
  readonly containers: typeof _ifcContainers
  readonly releases: typeof _ifcReleases
  readonly row: typeof _ifcRows
  readonly container: typeof _ifcContainerRows
  readonly descriptor: typeof _ifcDescriptor
  readonly admits: typeof _ifcAdmits
  readonly seals: typeof _ifcSeals
  readonly schema: Schema.Schema<Ifc.Payload, Uint8Array>
} = {
  Wire: _IfcWire,
  Form: _IfcForm,
  serializations: _ifcSerializations,
  containers: _ifcContainers,
  releases: _ifcReleases,
  row: _ifcRows,
  container: _ifcContainerRows,
  descriptor: _ifcDescriptor,
  admits: _ifcAdmits,
  seals: _ifcSeals,
  schema: Format.json.schema(_IfcWire),
}

declare namespace Ifc {
  type Serialization = (typeof _ifcSerializations)[number]
  type Container = (typeof _ifcContainers)[number]
  type Release = (typeof _ifcReleases)[number]
  type Sniff = (typeof _ifcSniffs)[number]
  type Form = Schema.Schema.Type<typeof _IfcForm>
  type Payload = Schema.Schema.Type<typeof _IfcWire>
  type Descriptor = { readonly extension: string; readonly header: string; readonly sniff: Sniff }
  type _Rows<
    T extends Record<
      Serialization,
      { readonly sniff: Sniff; readonly releases: Array.NonEmptyReadonlyArray<Release>; readonly refusal: Option.Option<string> }
    > = typeof _ifcRows,
  > = T
  type _Containers<
    T extends Record<Container, { readonly inflates: boolean; readonly wraps: Array.NonEmptyReadonlyArray<Serialization> }> = typeof _ifcContainerRows,
  > = T
}

const Frame = {
  Artifact: ArtifactAssembly,
  Residency,
  Ifc,
} as const

declare namespace Frame {
  type Band = ArtifactBand
  type Artifact = Schema.Schema.Type<typeof ArtifactAssembly.Artifact>
  type IfcWire = Ifc.Payload
  type IfcForm = Ifc.Form
  type IfcDescriptor = Ifc.Descriptor
  type ResidencyManifest = Manifest
  type ResidencyView = Residency.View
  type ResidencyTile = Residency.Tile
  type ResidencyViewpoint = Residency.Viewpoint
  type ResidencyMeshlet = Residency.Meshlet
  type ResidencyCensus = Residency.Census
  type ResidencyKind = Residency.Kind
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Frame }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
