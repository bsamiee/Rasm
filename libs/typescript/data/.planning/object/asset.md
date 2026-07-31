# [DATA_ASSET]

Delivered assets admit, optimize, and land here through ONE category-general plane. An asset CATEGORY is a row — its declaration shape, its admission arm, its transform vocabulary, its `Derive.Plane` — and the spine, the entry pair, the receipts, the store, and the instrument rows are category-blind. The GPU family — glTF containers and KTX2 texture planes — is the FIRST row family, never the plane's identity: `Asset.gate` dispatches on the `category` tag the declaration already carries, each category's optimization is its own closed row vocabulary (the gltf-transform property graph, the `ktx` subcommand family), the provisioned `ktx` validator settles Khronos conformance on every KTX2 byte this plane mints, and every category plane is a `Derive.Plane` row on `object/file.md`'s one derivative spine, so container surgery and texture encodes land exactly like raster renditions. The proof of the shape is the next category's diff — a font atlas, a video container, a point cloud, a document family is one gate arm, one category plane, one `Asset.Row` kind, zero pipeline change; no second fanout, second libvips encoder, or second address scheme exists.

Frozen law KEYS the ktx category's tables, so a drifted spelling is a type error rather than a runtime refusal: every row table closes against the exported `Texture` anchor at its own declaration — `_PAYLOADS` on `Texture.Payload`, `_TRANSFERS` on `Texture.Transfer`, `_STORES` on `Texture.PlaneFormat`, `_LAYERS` on `Texture.LayerLaw`, `_MIPS` on `Texture.MipPolicy` — so a misspelled tag fails the mapped key, a dropped row fails completeness, and an invented row fails excess, all at the anchor and none at a call site. Wire legality is `Texture.WirePayload` read from that anchor rather than re-derived from a local column, and `_PRIMARIES` is the one leaf roster with no frozen twin — its keys are the vocabulary, and `_TRANSFERS` types its `primaries` column against them so the pair closes on itself. Payload class discriminates on the DFD `colorModel`, never `vkFormat`. `ktx` is every branch's KTX2-encode floor: block compression admits only 8-bit stores — unrepresentable otherwise — and a deep plane ships uncompressed with its Vulkan format set. Rosters are stated closed, never `ALL_EXTENSIONS`; raster re-encoding rides the one sharp owner and `ktx-parse` never writes (both refused at `RULINGS.md`); `assets/<digest>/<file>` is the iac `_addressedAll` ↔ ui `Glb.assetDir` pair — this plane emits blobs and receipts, never addresses.

## [01]-[INDEX]

- [02]-[ASSET_GATE]: the frozen plane tables and the per-category admission arms over them.
- [03]-[TRANSFORM_ROWS]: the container transform vocabulary and the `ktx` subcommand family.
- [04]-[ENGINE_PLANES]: the category rows — plane, gate arm, guard closure — and the entry pair.

## [02]-[ASSET_GATE]

- Owner: the six frozen plane tables and the per-category admission arms over them — `Asset.gate(bytes, declared, key)` reads the `category` tag the declaration carries and routes to that category's own proof: the `ktx` arm (`_ktx2`) classifies KTX2 bytes through `ktx-parse` against the plane's wire declaration, and the `container` arm (`_opened`) builds glTF bytes through the layer-held `NodeIO` and proves their extension vocabulary against the closed roster. `AssetFault` is the page's one reason-discriminated family, its class rows closed through the core `FaultClass.family` seam.
- Packages: `ktx-parse` (`read`, `KTX2Container`, `keyValue`, the `KHR_DF_MODEL_*`/`KHR_DF_TRANSFER_*`/`KHR_DF_PRIMARIES_*`/`KHR_DF_FLAG_ALPHA_*`/`KHR_SUPERCOMPRESSION_*`/`VK_FORMAT_*` constant vocabularies); `@gltf-transform/core` (`NodeIO` — `readBinary`, `writeBinary`, `setAllowNetwork`, `registerExtensions`, `registerDependencies`; `Root.listExtensionsUsed`; `ImageUtils.getChannels`/`getVRAMByteLength` over the `image/ktx2` impl the roster's own static installs); `@gltf-transform/extensions` (`KHRTextureBasisu`, `KHRTextureTransform`, `KHRMeshQuantization`, `EXTMeshoptCompression`, `EXTMeshGPUInstancing`); `meshoptimizer/encoder` + `meshoptimizer/decoder` + `meshoptimizer/simplifier` (`ready`, `supported`); `@rasm/ts/core` (`FaultClass`; `Texture` — the frozen anchor every row table keys against).
- Entry: `Asset.gate(bytes, declared, key)` before any delivered asset reaches the store or a viewer manifest; the declaration's `category` selects the arm, and the ktx read re-runs inside the `ktx` engine's emit, so the encoder's own product proves itself before admission.
- Receipt: per category — the `ktx` arm answers the classification `{ payload, primaries, width, height, levels, layers, layerLaw, transcodes, vram }`, where `transcodes` derives from the payload CLASS (`uastc` and `etc1s` transcode, `none` uploads direct) never from `vkFormat`, and `vram` is the uncompressed GPU footprint the transfer byte count cannot state; the `container` arm answers the census `{ extensions, report, textures }` off the proven document.
- Law: the gate proves CHANNEL COUNT on the two block payloads its own law leaves unproven — `vkFormat` is `VK_FORMAT_UNDEFINED` on every transcoding payload, so nothing else on a `uastc` or `etc1s` container states how many channels it carries, and an `rgba8`-declared plane whose encoder emitted three admits silently into a material that samples alpha. `ImageUtils.getChannels` reads the DFD sample layout through the `image/ktx2` impl `KHRTextureBasisu.register()` installs, and its own contract is a CONSERVATIVE estimate, so the clause is a FLOOR: a container reporting fewer channels than the declared store names refuses, an over-report admits, and the impl's throw on an unrostered colorModel lifts through `Effect.try` exactly as the header read does.
- Law: origin and swizzle are `keyValue` facts, never DFD facts, so they are proven where the encode leg STATED them — a create row carries what it assigned beside the declaration it proves, and the emit-side re-gate reads `KTXorientation` and `KTXswizzle` back off the container; a caller-side gate over a delivered file states no framing and proves the DFD facts alone. The two axes the plane spells flags for were the two it never read back, so a tool-version change in flag handling produced a silently flipped or re-ordered texture every other clause passed.
- Law: the classification carries the GPU footprint beside the landed byte count — a Basis payload's transfer cost and its uploaded cost diverge by an order of magnitude, which is the whole reason this branch encodes Basis, and `ImageUtils.getVRAMByteLength` is the ONE correct read: summing `levels[].uncompressedByteLength` is documented ZERO under BASISLZ and may read zero under UASTC, so that sum is a forged measure and never written.
- Growth: within the ktx family a payload class is one `_PAYLOADS` row, a transfer tag one `_TRANSFERS` row, a working space one `_PRIMARIES` row, a storage format one `_STORES` row, a layer law one `_LAYERS` row — every declaration type, wire-legal subset, and encoder argument derives from those tables, so a row is the only edit; a new CATEGORY's admission is one `[04]` gate arm beside one `Declared` member, never a second entry.
- Law: the two proofs split by what each can know — the header read settles DECLARATION AGREEMENT (payload, store, transfer, primaries, alpha, layer shape, mips, extent are facts the caller declared and the container must match), and the provisioned `ktx validate` settles KHRONOS CONFORMANCE (level arithmetic, `typeSize`, DFD sample layout, BasisLZ global data, `KHR_texture_basisu` compatibility). Re-deriving the validator's arithmetic here forks the specification against the tool the whole estate encodes with.
- Law: the payload discriminant is `dataFormatDescriptor[0].colorModel` with `supercompressionScheme` — `uastc` rides `NONE`/`ZSTD`/`ZLIB`, `etc1s` rides `BASISLZ`, `none` is `RGBSDA` uncompressed-or-deflated, `astc` is the branch-local LDR acceleration — and every colorModel the roster names no row for lands on the `rawBcn` RESIDUE row, so a BC block file, an ETC2 file, and a corrupt descriptor all classify honestly and carry their raw model number in the refusal detail. Readers branching on `vkFormat` class every wire-legal payload as malformed.
- Law: wire legality and declaration agreement are ONE comparison — `Texture.WirePayload` is read from the frozen anchor, so a `rawBcn` or `astc` file cannot equal any declared payload and refuses on the same equality the drift check runs; the branch's own basis-transcoder path cannot consume either.
- Law: `vkFormat` is PROVEN, never read as the class — a transcoding payload MUST report `VK_FORMAT_UNDEFINED` (a supercompressed file carries no Vulkan format until transcode) and a `none` payload MUST report the enum its declared `_STORES` row names, so the deep store and the stored bytes cannot disagree.
- Law: the DFD carries the color science the plane declaration must agree with — `transferFunction` against the declared transfer tag, `colorPrimaries` against the working space that tag names, the `flags` alpha association against the declared alpha mode — and disagreement refuses; the gate never re-tags silently, because a silently re-tagged plane forks the shading value from the stored value. `linear` names the ONE scene-linear working space and lands on the AP1 primaries row, `srgb` on BT.709, `raw` on UNSPECIFIED because a parameter plane carries no chromaticity at all.
- Law: an SRGB target exists only on an 8-bit store row, so a float or half plane declaring `srgb` is UNREPRESENTABLE — `Asset.Ktx` unions the transfer against the store set each admits, and a color channel at integer depth encodes `srgb` where the same channel at float depth encodes `linear`.
- Law: layer shape is proven against the declared law — `faceCount` is 6 on a cubemap and 1 everywhere else, `layerCount` and `pixelDepth` are ZERO on a plain 2D file and carry the count on an array or volume — so `_LAYERS` reads all three and a cubemap can never admit as a 2D plane; level arithmetic reads `Math.max(count, 1)` and never a bare `layerCount` multiplier.
- Law: `levelCount` is mip truth with two zero-adjacent readings — `0` declares a base level whose pyramid the loader generates, `1` declares that no other level is meant to exist — so `mips` is compared RAW and the reported `levels` clamps; a block-compressed payload at `levelCount` 0 is disallowed by the container specification and refuses here, which is why the runtime-pyramid posture reaches the deep store alone.
- Law: the glTF roster is CLOSED and proven — `registerExtensions` states exactly what this branch honors, `Root.listExtensionsUsed()` must fall inside it on the document read AND on the document each pipeline emits, and a foreign name refuses with the `extension` reason; the proof exists because an IO whose roster omits an extension the source used DROPS that extension's properties on the round trip, and `ALL_EXTENSIONS` is the refused roster because honoring vocabulary no consumer renders is the same silent loss deferred to the viewer.
- Law: layer construction proves every seam the roster demands — every `_CODECS` module `ready` awaited and `supported` read as one capability gate (a kernel added to the roster is proven by construction, never by a second boot leg), `watlas.Initialize()` awaited on its own because the atlas module publishes readiness without a `supported` flag, `KHRTextureBasisu.register()` called so `ImageUtils` answers KTX2 extent, channels, and VRAM (the roster alone installs no impl), the meshopt encode METHOD pinned at construction so it is never a side effect of a level knob, and `setAllowNetwork(false)` pinned because the object plane supplies every resource by key — so no fold fails halfway through a document mutation and no sidecar ever fetches.
- Boundary: bytes in hand alone — `readBinary`/`writeBinary` are the admitted IO pair; `read(uri)`/`NodeIO.write(uri, doc)` never enter a plane whose addresses are content keys.

```typescript signature
import { Array, Effect, Option, Predicate, Record, Schema } from "effect"
import { ContentKey, FaultClass, Texture } from "@rasm/ts/core"
import {
  KHR_DF_FLAG_ALPHA_PREMULTIPLIED, KHR_DF_MODEL_ASTC, KHR_DF_MODEL_ETC1S, KHR_DF_MODEL_RGBSDA, KHR_DF_MODEL_UASTC,
  KHR_DF_PRIMARIES_ACESCC, KHR_DF_PRIMARIES_BT2020, KHR_DF_PRIMARIES_BT709, KHR_DF_PRIMARIES_UNSPECIFIED,
  KHR_DF_TRANSFER_HLG_EOTF, KHR_DF_TRANSFER_LINEAR, KHR_DF_TRANSFER_PQ_EOTF, KHR_DF_TRANSFER_SRGB,
  KHR_SUPERCOMPRESSION_BASISLZ, KHR_SUPERCOMPRESSION_NONE, KHR_SUPERCOMPRESSION_ZLIB, KHR_SUPERCOMPRESSION_ZSTD,
  VK_FORMAT_R16G16B16A16_SFLOAT, VK_FORMAT_R16G16B16A16_UNORM, VK_FORMAT_R16G16_SFLOAT, VK_FORMAT_R16G16_UNORM,
  VK_FORMAT_R16_SFLOAT, VK_FORMAT_R16_UNORM, VK_FORMAT_R32G32B32A32_SFLOAT, VK_FORMAT_R32G32_SFLOAT,
  VK_FORMAT_R32_SFLOAT, VK_FORMAT_R8G8B8A8_SRGB, VK_FORMAT_R8G8B8A8_UNORM, VK_FORMAT_R8G8_SRGB, VK_FORMAT_R8G8_UNORM,
  VK_FORMAT_R8_SRGB, VK_FORMAT_R8_UNORM, VK_FORMAT_UNDEFINED, read, type KTX2Container,
} from "ktx-parse"
import { ImageUtils, NodeIO, type Document } from "@gltf-transform/core"
import {
  EXTMeshGPUInstancing, EXTMeshoptCompression, KHRMeshQuantization, KHRTextureBasisu, KHRTextureTransform,
} from "@gltf-transform/extensions"
import { MeshoptDecoder } from "meshoptimizer/decoder"
import { MeshoptEncoder } from "meshoptimizer/encoder"
import { MeshoptSimplifier } from "meshoptimizer/simplifier"

const _family = FaultClass.family(
  ["gate", "payload", "transfer", "alpha", "extension", "codec", "tool", "transform", "emit"] as const,
  {
    gate: { class: "malformed" },
    payload: { class: "invalid" },
    transfer: { class: "invalid" },
    alpha: { class: "invalid" },
    extension: { class: "invalid" },
    codec: { class: "unavailable" },
    tool: { class: "unavailable" },
    transform: { class: "invalid" },
    emit: { class: "unavailable" },
  },
)

class AssetFault extends Schema.TaggedError<AssetFault>()("AssetFault", {
  reason: _family.schema,
  key: Schema.String,
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<asset:${this.reason}> ${this.detail}`
  }
}

// the row contracts every table below closes against: `as const satisfies` validates WITHOUT widening, so each
// table keeps its literals for the derived subsets while a misspelled key, a dropped row, and an invented row all
// fail at the declaration — the anchor-keyed mapped contract is what makes the drift claim structural
type _Target = { readonly vk: number; readonly cli: string }
type _PayloadRow = { readonly models: ReadonlyArray<number>; readonly schemes: ReadonlyArray<number>; readonly transcodes: boolean }
type _StoreRow = { readonly channels: 1 | 2 | 4; readonly block: boolean; readonly linear: _Target; readonly srgb?: _Target }
type _LayerRow = {
  readonly faces: number
  readonly arrayed: boolean
  readonly volumetric: boolean
  readonly flag: Option.Option<string>
  readonly origin: Option.Option<"topLeft">
}

// [03.7] transcription: colorModel classifies and `Texture.WirePayload` refuses; rawBcn is the RESIDUE row every
// unrostered model lands on, so BC block data, ETC2, and a corrupt descriptor all classify without a hand union.
// The wire subset is READ from the frozen anchor rather than re-derived from a local column — the interchange
// owner already closed that subset both ways, and a second derivation here is the fork the anchor exists to stop.
const _PAYLOADS = {
  uastc: {
    models: [KHR_DF_MODEL_UASTC],
    schemes: [KHR_SUPERCOMPRESSION_NONE, KHR_SUPERCOMPRESSION_ZSTD, KHR_SUPERCOMPRESSION_ZLIB],
    transcodes: true,
  },
  etc1s: { models: [KHR_DF_MODEL_ETC1S], schemes: [KHR_SUPERCOMPRESSION_BASISLZ], transcodes: true },
  none: {
    models: [KHR_DF_MODEL_RGBSDA],
    schemes: [KHR_SUPERCOMPRESSION_NONE, KHR_SUPERCOMPRESSION_ZSTD, KHR_SUPERCOMPRESSION_ZLIB],
    transcodes: false,
  },
  astc: {
    models: [KHR_DF_MODEL_ASTC],
    schemes: [KHR_SUPERCOMPRESSION_NONE, KHR_SUPERCOMPRESSION_ZSTD, KHR_SUPERCOMPRESSION_ZLIB],
    transcodes: false,
  },
  rawBcn: { models: [], schemes: [], transcodes: false }, // ktx-parse names no BC colorModel constant: the empty row IS the residue
} as const satisfies { readonly [K in Texture.Payload]: _PayloadRow }

// The ONE leaf roster with no frozen twin: chromaticity is a CLI-and-DFD column the shared fragment never froze,
// so these keys ARE the vocabulary and `_TRANSFERS` types its own column against them — the pair closes on itself
// and a drifted primaries spelling fails at the transfer row rather than at a spawn.
const _PRIMARIES = {
  none: { cli: "none", dfd: KHR_DF_PRIMARIES_UNSPECIFIED },
  bt709: { cli: "bt709", dfd: KHR_DF_PRIMARIES_BT709 },
  bt2020: { cli: "bt2020", dfd: KHR_DF_PRIMARIES_BT2020 },
  acescc: { cli: "acescc", dfd: KHR_DF_PRIMARIES_ACESCC }, // AP1: the ONE scene-linear working space, spelled by its ACES family name
} as const

// [03.1] transcription: each frozen tag with the DFD value it must read, the `--assign-tf` spelling it assigns,
// and the working space it names — so a plane never carries a second, caller-stated primaries declaration
const _TRANSFERS = {
  linear: { dfd: KHR_DF_TRANSFER_LINEAR, tf: "linear", primaries: "acescc" },
  srgb: { dfd: KHR_DF_TRANSFER_SRGB, tf: "srgb", primaries: "bt709" },
  raw: { dfd: KHR_DF_TRANSFER_LINEAR, tf: "linear", primaries: "none" }, // no transfer, no color management — KTX2 spells non-color data LINEAR with no chromaticity
  pq: { dfd: KHR_DF_TRANSFER_PQ_EOTF, tf: "pq_eotf", primaries: "bt2020" },
  hlg: { dfd: KHR_DF_TRANSFER_HLG_EOTF, tf: "hlg_eotf", primaries: "bt2020" },
} as const satisfies {
  readonly [K in Texture.Transfer]: { readonly dfd: number; readonly tf: string; readonly primaries: keyof typeof _PRIMARIES }
}

// [03.6] transcription: `block` carries the MEASURED encode bound — `ktx create --encode` admits R8* targets alone,
// so the eight-bit rows are the only ones a block leg can name and the deep rows carry no srgb target at all
const _STORES = {
  r8: { channels: 1, block: true, linear: { vk: VK_FORMAT_R8_UNORM, cli: "R8_UNORM" }, srgb: { vk: VK_FORMAT_R8_SRGB, cli: "R8_SRGB" } },
  rg8: { channels: 2, block: true, linear: { vk: VK_FORMAT_R8G8_UNORM, cli: "R8G8_UNORM" }, srgb: { vk: VK_FORMAT_R8G8_SRGB, cli: "R8G8_SRGB" } },
  rgba8: { channels: 4, block: true, linear: { vk: VK_FORMAT_R8G8B8A8_UNORM, cli: "R8G8B8A8_UNORM" }, srgb: { vk: VK_FORMAT_R8G8B8A8_SRGB, cli: "R8G8B8A8_SRGB" } },
  r16: { channels: 1, block: false, linear: { vk: VK_FORMAT_R16_UNORM, cli: "R16_UNORM" } },
  r16f: { channels: 1, block: false, linear: { vk: VK_FORMAT_R16_SFLOAT, cli: "R16_SFLOAT" } },
  r32f: { channels: 1, block: false, linear: { vk: VK_FORMAT_R32_SFLOAT, cli: "R32_SFLOAT" } },
  rg16: { channels: 2, block: false, linear: { vk: VK_FORMAT_R16G16_UNORM, cli: "R16G16_UNORM" } },
  rg16f: { channels: 2, block: false, linear: { vk: VK_FORMAT_R16G16_SFLOAT, cli: "R16G16_SFLOAT" } },
  rg32f: { channels: 2, block: false, linear: { vk: VK_FORMAT_R32G32_SFLOAT, cli: "R32G32_SFLOAT" } },
  rgba16: { channels: 4, block: false, linear: { vk: VK_FORMAT_R16G16B16A16_UNORM, cli: "R16G16B16A16_UNORM" } },
  rgba16f: { channels: 4, block: false, linear: { vk: VK_FORMAT_R16G16B16A16_SFLOAT, cli: "R16G16B16A16_SFLOAT" } },
  rgba32f: { channels: 4, block: false, linear: { vk: VK_FORMAT_R32G32B32A32_SFLOAT, cli: "R32G32B32A32_SFLOAT" } },
} as const satisfies { readonly [K in Texture.PlaneFormat]: _StoreRow }

// [04.1] transcription: the container shape each law proves, the create flag each law spells, and the texcoord
// origin it pins — `--cubemap` refuses any origin but top-left, so the pin lives with the law, not the caller
const _LAYERS = {
  none: { faces: 1, arrayed: false, volumetric: false, flag: Option.none<string>(), origin: Option.none<"topLeft">() },
  cubeFaces: { faces: 6, arrayed: false, volumetric: false, flag: Option.some("--cubemap"), origin: Option.some("topLeft" as const) },
  array: { faces: 1, arrayed: true, volumetric: false, flag: Option.some("--layers"), origin: Option.none<"topLeft">() },
  frames: { faces: 1, arrayed: true, volumetric: false, flag: Option.some("--layers"), origin: Option.none<"topLeft">() }, // a flipbook IS an array at the container: the law records intent
  volume: { faces: 1, arrayed: false, volumetric: true, flag: Option.some("--depth"), origin: Option.none<"topLeft">() },
} as const satisfies { readonly [K in Texture.LayerLaw]: _LayerRow }

declare namespace Asset {
  // the data plane's ONE derived subset — `block` is THIS table's measured encode legality, never a frozen fact,
  // so it derives here while payload, store, transfer, layer, and alpha vocabularies read the anchor directly and
  // the local aliases that used to restate them are gone: one hop from every site to the frozen roster
  type Block = { readonly [K in Texture.PlaneFormat]: (typeof _STORES)[K]["block"] extends true ? K : never }[Texture.PlaneFormat]
  type Primaries = keyof typeof _PRIMARIES
  // the ktx category's declaration: the category tag IS the gate's dispatch evidence, so admission
  // discriminates on a fact the value carries, never a mode knob beside it
  type Ktx =
    & {
      readonly category: "ktx"
      readonly alphaMode: Texture.AlphaMode
      readonly ktxPayload: Texture.WirePayload
      readonly mips: number // RAW level truth: 0 is the loader-generated pyramid, never a clamped 1
      readonly width: number
      readonly height: number
      readonly layers: number
      readonly layerLaw: Texture.LayerLaw
    }
    & (
      // an SRGB Vulkan target exists on the eight-bit rows alone, so a deep plane declaring srgb never spells
      | { readonly colorSpace: "srgb"; readonly store: Block }
      | { readonly colorSpace: Exclude<Texture.Transfer, "srgb">; readonly store: Texture.PlaneFormat }
    )
  // what a create leg ASSIGNED, carried beside the declaration it proves — origin and swizzle ride `keyValue`
  // and no DFD clause can reach them, so the emit-side re-gate is the only seam where they are readable at all
  type Stated = { readonly origin: keyof typeof _ORIGINS; readonly swizzle: Option.Option<Swizzle> }
  type Proof = { readonly declared: Ktx; readonly stated: Option.Option<Stated> }
  // the container category declares its category alone — the closed extension roster is the arm's own law,
  // so a delivered glTF carries no per-file declaration to drift from
  type Container = { readonly category: "container" }
  // the raster category's declaration: a delivered rendition states the codec and extent its consumer binds
  // against, and the census that proves it comes back through `object/file.md`'s ONE libvips composer
  type Raster = { readonly category: "raster"; readonly format: keyof Derive.Codec; readonly width: number; readonly height: number }
  // the point-cloud category's declaration: a scan states its point census and the attribute semantics its
  // consumer binds, so the gate proves topology and attribute roster against facts the value itself carries
  type Points = { readonly category: "points"; readonly count: number; readonly attributes: ReadonlyArray<string> }
  type Classified = {
    readonly payload: Texture.Payload
    readonly primaries: Primaries
    readonly width: number
    readonly height: number
    readonly levels: number
    readonly layers: number
    readonly layerLaw: Texture.LayerLaw
    readonly transcodes: boolean
    readonly vram: Option.Option<number> // the uploaded footprint; none where the impl reads no payload it knows
  }
}

const _KTX2_MIME = "image/ktx2" as const // the impl key KHRTextureBasisu.register() installs; ImageUtils answers null under any other

// the two metadata keys the create leg's own flags write, named once so the read-back and the flag builder cannot
// drift to different spellings of one container field
const _KEY_VALUE = { orientation: "KTXorientation", swizzle: "KTXswizzle" } as const

const _target = (declared: Asset.Ktx) =>
  declared.colorSpace === "srgb" ? _STORES[declared.store].srgb : _STORES[declared.store].linear

const _space = (declared: Asset.Ktx) => _PRIMARIES[_TRANSFERS[declared.colorSpace].primaries]

const _classified = (held: KTX2Container): Texture.Payload =>
  Option.match(
    Record.findFirst(
      _PAYLOADS,
      (row) =>
        Array.some(row.models, (model) => model === held.dataFormatDescriptor[0]?.colorModel)
        && Array.some(row.schemes, (scheme) => scheme === held.supercompressionScheme),
    ),
    { onNone: () => "rawBcn" as const, onSome: ([payload]) => payload },
  )

const _refuse = (reason: (typeof _family.reasons)[number], key: string, detail: string) =>
  Effect.fail(new AssetFault({ reason, key, detail }))

// keyValue is an open string map, so the read is index trust lifted at the seam and narrowed to text before any
// comparison — a binary value where the container spells a text key is itself malformed and folds to none
const _stated = (held: KTX2Container, field: string) =>
  Option.filter(Option.fromNullable(held.keyValue[field]), Predicate.isString)

const _probe = <A>(key: string, read: () => A | null) =>
  Effect.map(
    // the impl THROWS on a colorModel its table does not carry, exactly as the header read throws on a bad
    // identifier, so both lift on the same rail and a corrupt descriptor never escapes as a defect
    Effect.try({ try: read, catch: (defect) => new AssetFault({ reason: "payload", key, detail: String(defect) }) }),
    Option.fromNullable,
  )

const _ktx2 = (bytes: Uint8Array, declared: Asset.Ktx, key: string, stated: Option.Option<Asset.Stated>) =>
  Effect.gen(function* () {
    const held = yield* Effect.try({
      try: () => read(bytes), // throws on a failed KTX 2.0 identifier: the magic IS the gate
      catch: (defect) => new AssetFault({ reason: "gate", key, detail: String(defect) }),
    })
    const descriptor = held.dataFormatDescriptor[0]
    const payload = _classified(held)
    const shape = _LAYERS[declared.layerLaw]
    // one equality carries both laws: a non-wire class equals no declarable payload, so refusal and drift share a branch
    yield* payload === declared.ktxPayload
      ? Effect.void
      : _refuse("payload", key, `${payload}<>${declared.ktxPayload} model ${descriptor?.colorModel} scheme ${held.supercompressionScheme}`)
    yield* (_PAYLOADS[payload].transcodes ? held.vkFormat === VK_FORMAT_UNDEFINED : held.vkFormat === _target(declared).vk)
      ? Effect.void
      : _refuse("payload", key, `vkFormat ${held.vkFormat}`) // proving the declared store, never classifying by it
    yield* descriptor?.transferFunction === _TRANSFERS[declared.colorSpace].dfd
      ? Effect.void
      : _refuse("transfer", key, declared.colorSpace)
    yield* descriptor?.colorPrimaries === _space(declared).dfd
      ? Effect.void
      : _refuse("transfer", key, `primaries ${descriptor?.colorPrimaries}<>${_space(declared).cli}`)
    const premultiplied = ((descriptor?.flags ?? 0) & KHR_DF_FLAG_ALPHA_PREMULTIPLIED) !== 0
    yield* premultiplied === (declared.alphaMode === "associated")
      ? Effect.void
      : _refuse("alpha", key, declared.alphaMode)
    const layers = Math.max(held.layerCount, held.pixelDepth, 1) // both read ZERO on a plain 2D file: never a bare multiplier
    yield* held.faceCount === shape.faces
        && (held.layerCount > 0) === shape.arrayed
        && (held.pixelDepth > 0) === shape.volumetric
        && layers === (shape.arrayed || shape.volumetric ? declared.layers : 1)
      ? Effect.void
      : _refuse("gate", key, `${declared.layerLaw} faces ${held.faceCount} layers ${held.layerCount} depth ${held.pixelDepth}`)
    yield* held.levelCount === declared.mips && !(held.levelCount === 0 && _PAYLOADS[payload].transcodes)
      ? Effect.void
      : _refuse("gate", key, `mips ${held.levelCount}<>${declared.mips} payload ${payload}`) // levelCount 0 is disallowed on a block payload
    yield* held.pixelWidth === declared.width && held.pixelHeight === declared.height
      ? Effect.void
      : _refuse("gate", key, `${held.pixelWidth}x${held.pixelHeight}`)
    // the ONE channel proof: on uastc and etc1s the vkFormat clause above proved UNDEFINED and nothing else on the
    // container states channel count, so a declared rgba8 whose encoder emitted three would admit into a material
    // that samples alpha. The member's own contract is a conservative estimate, so this is a FLOOR — a container
    // proving fewer channels than the store names refuses, an over-report admits, and null is no evidence at all.
    const channels = yield* _probe(key, () => ImageUtils.getChannels(bytes, _KTX2_MIME))
    yield* Option.match(channels, {
      onNone: () => Effect.void,
      onSome: (held_) =>
        held_ >= _STORES[declared.store].channels
          ? Effect.void
          : _refuse("payload", key, `channels ${held_}<${_STORES[declared.store].channels}`),
    })
    // origin and swizzle are keyValue facts the DFD cannot carry, so they prove ONLY where a create leg stated
    // them; a caller-side gate over a delivered file passes none and proves the descriptor facts alone
    yield* Option.match(stated, {
      onNone: () => Effect.void,
      onSome: (assigned) =>
        Effect.zipRight(
          Option.match(_stated(held, _KEY_VALUE.orientation), {
            onNone: () => _refuse("gate", key, `<${_KEY_VALUE.orientation}-absent> ${assigned.origin}`),
            onSome: (carried) =>
              carried === _ORIGINS[assigned.origin].metadata ? Effect.void : _refuse("gate", key, `orientation ${carried}`),
          }),
          Option.match(assigned.swizzle, {
            onNone: () => Effect.void, // an unassigned swizzle writes no key: absence IS agreement
            onSome: (spelled) =>
              Option.match(_stated(held, _KEY_VALUE.swizzle), {
                onNone: () => _refuse("gate", key, `<${_KEY_VALUE.swizzle}-absent> ${spelled}`),
                onSome: (carried) => (carried === spelled ? Effect.void : _refuse("gate", key, `swizzle ${carried}<>${spelled}`)),
              }),
          }),
        ),
    })
    return {
      payload,
      primaries: _TRANSFERS[declared.colorSpace].primaries,
      width: held.pixelWidth,
      height: held.pixelHeight,
      levels: Math.max(held.levelCount, 1), // the reported depth clamps; the proof above read the raw value
      layers,
      layerLaw: declared.layerLaw,
      transcodes: _PAYLOADS[payload].transcodes, // derived from the payload CLASS, never from vkFormat
      // the uploaded footprint the landed byte count cannot state: the impl falls back to per-level block
      // arithmetic exactly because levels[].uncompressedByteLength reads 0 under BASISLZ and may under UASTC
      vram: yield* _probe(key, () => ImageUtils.getVRAMByteLength(bytes, _KTX2_MIME)),
    } satisfies Asset.Classified
  })

const _EXTENSIONS = [
  EXTMeshGPUInstancing, EXTMeshoptCompression, KHRMeshQuantization, KHRTextureBasisu, KHRTextureTransform,
] as const

// every module carrying the `ready` + `supported` pair joins the ONE proof: a kernel added to the roster is
// readied and capability-gated by construction, so a new transform row buys no second boot leg
const _CODECS = [MeshoptClusterizer, MeshoptDecoder, MeshoptEncoder, MeshoptSimplifier, MeshoptTangents] as const

const _ROSTER = Array.map(_EXTENSIONS, (extension) => extension.EXTENSION_NAME)

const _io = Effect.gen(function* () {
  yield* Effect.tryPromise({
    try: () => Promise.all(Array.map(_CODECS, (codec) => codec.ready)),
    catch: (defect) => new AssetFault({ reason: "codec", key: "meshopt", detail: String(defect) }),
  })
  yield* Array.every(_CODECS, (codec) => codec.supported) ? Effect.void : _refuse("codec", "meshopt", "<unsupported>")
  // watlas gates on Initialize() alone and publishes no `supported` flag, so it stands outside the codec roster
  // and takes its own await; an Atlas constructed before it resolves reaches an uninstantiated wasm table
  yield* Effect.tryPromise({
    try: () => watlas.Initialize(),
    catch: (defect) => new AssetFault({ reason: "codec", key: "watlas", detail: String(defect) }),
  })
  KHRTextureBasisu.register() // roster registration installs no ImageUtils impl: absent this static a KTX2 texture answers getSize() null
  // the encode METHOD is a posture the row states, never a side effect of the level knob: `meshopt` picks
  // QUANTIZE at level "medium" and FILTER at the default "high", and because emitted bytes are content-addressed
  // a package-default shift in that mapping silently re-keys every previously encoded container
  EXTMeshoptCompression.setEncoderOptions({ method: EXTMeshoptCompression.EncoderMethod.FILTER })
  return new NodeIO()
    .setAllowNetwork(false)
    .registerExtensions([..._EXTENSIONS])
    .registerDependencies({ "meshopt.decoder": MeshoptDecoder, "meshopt.encoder": MeshoptEncoder })
})

const _vocabulary = (handle: Document, key: string) =>
  Effect.suspend(() => {
    const foreign = Array.filter(
      Array.map(handle.getRoot().listExtensionsUsed(), (extension) => extension.extensionName),
      (name) => !Array.contains(_ROSTER, name),
    )
    return Array.isNonEmptyReadonlyArray(foreign) ? _refuse("extension", key, foreign.join(" ")) : Effect.void
  })

const _opened = (io: NodeIO, bytes: Uint8Array, source: string) =>
  Effect.gen(function* () {
    const handle = yield* Effect.tryPromise({
      try: () => io.readBinary(bytes),
      catch: (defect) => new AssetFault({ reason: "gate", key: source, detail: String(defect) }),
    })
    yield* _vocabulary(handle, source)
    return handle
  })
```

## [03]-[TRANSFORM_ROWS]

- Owner: two closed row vocabularies over one shape — `_STEPS`/`_TRANSFORMS`, where each container row takes its package option record and answers the step value the one `document.transform(...)` entry folds, and `_VERBS`/`_KTX`, where each `ktx` subcommand takes its own option record and answers the `Asset.Spawn` value the engine spawns beside the row-derived veto that admits its staged input. `_MIPS`, `_ORIGINS`, and `_DEFLATE` are the axis tables those spawn rows read; `_INPUTS` is the input-class roster; `_Report` decodes the validator's own JSON verdict.
- Packages: `@gltf-transform/functions` (`prune`, `dedup`, `instance`, `unwrap`, `quantize`, `weld`, `join`, `flatten`, `palette`, `reorder`, `meshopt`, `simplify`, `cloneDocument`, `inspect`, `createTransform`, the graph readers `listTextureSlots`/`getTextureColorSpace`/`getTextureChannelMask`/`listTextureChannels`, their option records, `InspectReport`); `meshoptimizer/encoder` + `meshoptimizer/simplifier` + `meshoptimizer/tangents` + `meshoptimizer/clusterizer` (`MeshoptEncoder`, `MeshoptSimplifier`, `MeshoptTangents`, `MeshoptClusterizer` — the SAME instances the IO dependency map and the codec proof hold); `watlas` (`Initialize`, the module injected whole as `UnwrapOptions.watlas`); `@effect/platform` (`Command.make`, `Command.string`, `Command.exitCode` — the `CommandExecutor` requirement the composition root satisfies); `effect` (`Schema.parseJson` over the validator report, `Config` for the spawn governance).
- Entry: a pipeline builds its steps from `_TRANSFORMS` rows in fold order; an encode, transcode, deflate, or extract builds one `Asset.Spawn` from its `_KTX` row — both are data on the `[04]` plane rows, never imperative call chains.
- Growth: a container optimization is one `_STEPS` entry with its `_TRANSFORMS` row; a tool operation is one `_VERBS` entry with its `_KTX` row, and the presence proof, the argv, the input veto, and the plane's product handling all derive from that row — the governed record refuses a table that misses the tuple or exceeds it. A kernel with no shipped transform is the same row over `createTransform`, and a kernel whose product no glTF vocabulary can hold is a policy COLUMN answering receipt evidence instead.
- Law: a step carries its row's WHOLE knob surface — `quantize` bit widths, `meshopt` level, `reorder` target, `prune` property types, `simplify` ratio and error, `join` and `palette` posture — because each option record merges over its exported defaults, so a pipeline states policy where a zero-argument thunk freezes one posture into the vocabulary and strands every other; a step value exists only where a table row minted it, so the vocabulary closes at construction rather than at a name check.
- Law: codecs are INJECTED, never imported by the fold — `meshopt`/`reorder` take the `MeshoptEncoder` instance the IO's dependency map registered and `simplify` the `MeshoptSimplifier` the same proof readied, so encode, decimation, and extension write share one wasm lineage, and a `meshopt` step obligates `EXTMeshoptCompression` + `KHRMeshQuantization` on the roster because the row attaches both as required.
- Law: `textureCompress` and `compressTexture` are REFUSED here — a second libvips composer beside `object/file.md`'s one sharp owner, with an encoder-less fallback that silently drops every quality option; raster re-encoding rides the raster plane, KTX2 rides the tool rows, and this vocabulary contributes container surgery alone.
- Law: a kernel this branch loads but glTF-Transform ships no transform for lands as a BRANCH-OWNED `createTransform` row over the same injected-instance law — `tangents` mints the `TANGENT` accessor from `MeshoptTangents`, and the shipped `tangents` row is refused beside it because its `generateTangents` callback takes three arguments with no index array, forcing an unweld that multiplies vertices, and because satisfying it admits a second unadmitted wasm lineage the meshopt roster already covers. Every meshopt stride counts FLOAT32 ELEMENTS, never bytes.
- Law: the `TANGENT` accessor is the campaign's own quality fork — a normal-mapped material with no tangent frame falls back to screen-space derivative tangents in the consuming renderer, which is exactly the silent-quality divergence the colour-science laws refuse everywhere else, so the row exists wherever this branch delivers normal maps.
- Law: a cluster read is a COLUMN, never a step — glTF carries no meshlet vocabulary, so writing the kernel's product into the property graph would mint a name no consumer renders and the emit-side vocabulary proof would refuse the plane's own product; the batch and its per-meshlet sphere-and-cone bound ride the receipt, computed off the FOLDED document so they describe the topology the store holds rather than the one the fold started from.
- Law: `unwrap` takes the whole initialized `watlas` module as its injected instance exactly as `meshopt` takes its encoder, so the transform owns the document-to-declaration flatten and the `xref` gather that rebuilds every attribute across the vertices a seam split; driving `Atlas` by hand against a glTF document re-derives what the row already composes, and an atlas run that ignores `xref` mis-gathers every attribute after the first split.
- Law: the tool is a SUBCOMMAND FAMILY, not one call — `create` mints a container from images, `encode` block-encodes one already minted, `transcode` lowers a Basis payload to the desktop-native block class the wire refuses, `deflate` supercompresses in place, `extract` returns one level, layer, or face as an image the raster plane can read, and `validate` settles conformance; the flag builder is a column on each row, so a spawn site never spells a subcommand's arguments and a second tool entry cannot exist.
- Law: the validator is the CONFORMANCE OWNER and its verdict rides its own report, never the exit code — `Command.string` collects stdout and never reads process status, and the measured `--format mini-json` payload carries `valid` with a message list, so the decoded `valid` is the branch; `--gltf-basisu` spells exactly where the payload's `transcodes` column is true, because the measured refusal on an `RGBSDA` container (`error-6301`) is correct and rejects every legitimate deep plane.
- Law: flag legality is structural — `--zstd`/`--zlib` are unspellable on the `etc1s` leg (the CLI refuses either over BasisLZ), `--encode` is unreachable from a deep store because a block leg's declaration admits `_STORES` block rows alone, and `--normal-mode` rides the `uastc` leg because `[03.7]` routes every direction-carrying channel there with RDO off — the CLI honors the flag under BasisLZ too, so the narrowing is the frozen POLICY, not a tool limit.
- Law: `--raw` is the headerless posture and nothing else — the deep leg classifies its staged input through `_INPUTS`, spelling `--raw` with the required `--width`/`--height` exactly when the bytes carry no container magic, and spelling neither on a PNG or EXR input because `--width` on a container input silently RESAMPLES the plane to that extent; `--generate-mipmap` and `--normalize` are both refused over raw input by the tool, so a headerless plane supplies its own levels.
- Law: the pyramid posture reads `mips` and one row field — `mips` levels supplied as one input file per level times the declared layer count spells `--levels` alone, `generate` spells `--generate-mipmap` beside it so the encoder folds the chain from the base image under the `_MIPS` filter its policy names, and `mips` ZERO spells `--runtime-mipmap` so the loader folds at upload. `roughnessVariance` and `none` carry `folds: false` because neither is a resampling kernel the encoder owns — the first needs the paired normal channel's lost variance and the second declares no pyramid at all — so both supply their levels.
- Law: color and origin never convert silently — `--assign-tf`, `--assign-primaries`, and `--assign-texcoord-origin` RELABEL without touching a texel, `--convert-texcoord-origin` spells only where the source origin differs from the frozen `top-left` storage origin, and `--fail-on-color-conversions` turns any remaining implicit color conversion into a tool refusal rather than a re-tagged plane.
- Law: the input roster is one file per LEVEL times the layer count the shape declares — largest level first, cube faces in +X, -X, +Y, -Y, +Z, -Z order within a level — and a concatenated level stream refuses with `Too few input images`; `--levels` reads the DECLARATION, so the count the gate proves and the count the encoder writes are one value.
- Law: a deep container input carries the precision its format names — `ktx create` refuses to widen a half EXR into a 32-bit target — so the `_STORES` row a deep leg declares is the source's own depth, never an upcast.
- Law: the tool proof asserts presence and the subcommand roster derived from `_VERBS`, NEVER version text — every provisioned `ktx` binary prints `GIT-NOTFOUND` for `--version` — and a failed proof refuses the service at construction with the `tool` reason, so no request ever discovers the absence.
- Boundary: `info` and `compare` stay unspawned — the first duplicates the header read this plane already owns beside a validation the `validate` row settles, and the second is a two-file parity assertion belonging to the proof estate, not to a plane whose products are content-addressed by construction.

```typescript signature
import {
  cloneDocument, createTransform, dedup, flatten, getTextureChannelMask, getTextureColorSpace, inspect, instance,
  join, listTextureChannels, listTextureSlots, meshopt, palette, prune, quantize, reorder, simplify, unwrap, weld,
  type DedupOptions, type FlattenOptions, type InspectReport, type InstanceOptions, type JoinOptions,
  type MeshoptOptions, type PaletteOptions, type PruneOptions, type QuantizeOptions, type ReorderOptions,
  type SimplifyOptions, type UnwrapOptions, type WeldOptions,
} from "@gltf-transform/functions"
import { Accessor, type GLTF, type Primitive, type TextureChannel, type Transform } from "@gltf-transform/core"
import { MeshoptClusterizer, type Bounds } from "meshoptimizer/clusterizer"
import { MeshoptTangents, type TangentsFlags } from "meshoptimizer/tangents"
import * as watlas from "watlas" // the whole initialized module IS the injected instance UnwrapOptions.watlas takes
import { Command, type CommandExecutor, type Error as Platform } from "@effect/platform"
import { Config, Duration, Function } from "effect"
import { Derive, DeriveFault } from "./file.ts"

// the tuple IS the fold order, so an ordering law is positional rather than prose: `instance` sits after `dedup`
// because only linked duplicates collapse into one InstancedMesh, and `unwrap` precedes `tangents` because a
// tangent frame reads the texcoord the atlas mints
const _STEPS = [
  "prune", "dedup", "instance", "unwrap", "tangents", "quantize", "weld", "join", "flatten", "palette", "reorder",
  "meshopt", "simplify",
] as const

const _POINTS_MODE = 0 satisfies GLTF.MeshPrimitiveMode // WebGL POINTS; the shipped `Primitive.Mode` is a string-indexed Record, unreadable by dot under the strictness set

// meshopt strides count FLOAT32 ELEMENTS, never bytes — a tight VEC3 stream is 3 and a tight VEC2 is 2, so a
// byte stride passed here reads every fourth vertex and silently produces a frame for a mesh that is not there
const _STRIDE = { vec2: 2, vec3: 3 } as const

const _primitives = (document: Document) =>
  Array.flatMap(document.getRoot().listMeshes(), (mesh) => mesh.listPrimitives())

const _attribute = (primitive: Primitive, semantic: string) =>
  Option.flatMap(Option.fromNullable(primitive.getAttribute(semantic)), (accessor) => Option.fromNullable(accessor.getArray()))

const _indices = (primitive: Primitive) =>
  Option.map(
    Option.flatMap(Option.fromNullable(primitive.getIndices()), (accessor) => Option.fromNullable(accessor.getArray())),
    (held) => new Uint32Array(held),
  )

const _VERBS = ["create", "encode", "transcode", "deflate", "extract", "validate"] as const

// `deep` is the input's own DEPTH CLASS, the fact a leg's admission votes on: a PNG feeding a deep store would
// upcast and an EXR feeding a block leg would tone-map, and the tool refuses both far downstream of the row
const _INPUTS = [
  { magic: [0x89, 0x50], suffix: "png", deep: false },
  { magic: [0x76, 0x2f], suffix: "exr", deep: true },
] as const

// [03.8] transcription: the resampling filter each mip policy spells, and whether the encoder may fold the
// chain at all — a policy the tool cannot express supplies its levels instead of silently taking a box fold
const _MIPS = {
  box: { filter: "box", normalize: false, folds: true },
  kaiser: { filter: "kaiser", normalize: false, folds: true },
  normalRenormalize: { filter: "box", normalize: true, folds: true },
  roughnessVariance: { filter: "box", normalize: false, folds: false },
  none: { filter: "box", normalize: false, folds: false },
} as const satisfies { readonly [K in Texture.MipPolicy]: { readonly filter: string; readonly normalize: boolean; readonly folds: boolean } }

// ONE origin table, two columns: the flag the create leg spells and the KTXorientation string the tool then
// writes into the container, so the assign and the read-back close on one row rather than two vocabularies
const _ORIGINS = {
  topLeft: { cli: "top-left", metadata: "rd" },
  bottomLeft: { cli: "bottom-left", metadata: "ru" },
} as const

const _DEFLATE = {
  zstd: { flag: "--zstd", scheme: KHR_SUPERCOMPRESSION_ZSTD, floor: 1, ceiling: 22 },
  zlib: { flag: "--zlib", scheme: KHR_SUPERCOMPRESSION_ZLIB, floor: 1, ceiling: 9 },
} as const

const _STORAGE_ORIGIN = "topLeft" as const // KTX2 and glTF both read s=0,t=0 top-left, and --cubemap admits no other

const _Report = Schema.Struct({
  valid: Schema.Boolean,
  messages: Schema.Array(
    Schema.Struct({ id: Schema.Number, type: Schema.String, message: Schema.String, details: Schema.optional(Schema.String) }),
  ),
})

declare namespace Asset {
  type Step = { readonly step: (typeof _STEPS)[number]; readonly run: () => Transform }
  type Pipeline = Derive.Row & {
    readonly kind: "container"
    readonly steps: ReadonlyArray<Step>
    readonly admit?: (census: InspectReport) => boolean
    readonly clusters?: Clusters
  }
  // cluster policy is a COLUMN, never a step: glTF carries no meshlet vocabulary, so the kernel's product cannot
  // land in the property graph without minting an extension no consumer renders — it rides the receipt instead,
  // computed off the FOLDED document so the clusters describe the topology the store actually holds
  type Clusters = { readonly maxVertices: number; readonly maxTriangles: number; readonly coneWeight?: number }
  type Meshlets = { readonly meshlets: number; readonly bounds: ReadonlyArray<Bounds> }
  type Verb = (typeof _VERBS)[number]
  type Component = "r" | "g" | "b" | "a" | "0" | "1"
  type Swizzle = `${Component}${Component}${Component}${Component}` // the CLI's [rgba01]{4} alphabet as 1296 literals: an illegal swizzle never spells
  type Deflate = { readonly codec: keyof typeof _DEFLATE; readonly level: number }
  type Blocked = Asset.Ktx & { readonly store: Asset.Block; readonly ktxPayload: Exclude<Texture.WirePayload, "none"> }
  type Deep = Asset.Ktx & { readonly ktxPayload: "none" }
  type Framing = {
    readonly levels: ReadonlyArray<ContentKey> // inputs beyond the base plane, level-major then face-major
    readonly mipPolicy: keyof typeof _MIPS
    readonly generate?: boolean
    readonly origin?: keyof typeof _ORIGINS
    readonly swizzle?: Swizzle
    readonly inputSwizzle?: Swizzle
  }
  type Create =
    & Framing
    & (
      | { readonly leg: "uastc"; readonly declared: Blocked; readonly quality?: number; readonly normal?: boolean; readonly rdo?: number; readonly deflate?: Deflate }
      | { readonly leg: "etc1s"; readonly declared: Blocked; readonly quality?: number; readonly clevel?: number; readonly normal?: never; readonly deflate?: never }
      | { readonly leg: "deep"; readonly declared: Deep; readonly quality?: never; readonly normal?: never; readonly rdo?: never; readonly deflate?: Deflate }
    )
  type Spawn = {
    readonly command: Verb
    readonly sources: ReadonlyArray<ContentKey>
    readonly flags: (input: Input) => ReadonlyArray<string>
    readonly proves: Option.Option<Proof> // present exactly where the product is a KTX2 this plane declared AND stated
    // the row's OWN veto, minted where the row's knobs are known: `Derive.Plane.admit` is the spine's seam and
    // this plane returned `true` unconditionally, so a deep create over an eight-bit stage reached a spawn and
    // failed at the tool instead of at admission, with no row name on the refusal
    readonly admit: (input: Input) => boolean
    readonly emits: "container" | "image" | "report"
    readonly leaf: string // the product's own suffix: the tool answers the SOURCE's format, never the name it is handed
  }
  // the caller's override sits beside the row-derived default its sibling `Pipeline` already carries, so both
  // kinds vote through one seam and a row states policy where its knobs are, never at the spine
  type Encode = Derive.Row & {
    readonly kind: "ktx"
    readonly spawn: Spawn & { readonly emits: "container" | "image" }
    readonly admit?: (input: Input) => boolean
  }
  // the point-cloud row: LOD decimation, indexed-island pruning, and spatial order over the kernels the codec
  // proof already readies — policy per row, kernels branch-owned, because neither ships a glTF transform
  type Lod = Derive.Row & {
    readonly kind: "points"
    readonly order: boolean // reorderPoints spatial sequence, so a streamed range is a spatial neighbourhood
    readonly decimate?: { readonly ratio: number; readonly colorWeight?: number } // survivor fraction of the census; COLOR_0 weighting where declared
    readonly prune?: { readonly error: number } // MESH units — the fold divides by getScale so the kernel reads relative; indexed primitives alone
    readonly admit?: (cloud: Cloud) => boolean
  }
  type Row = Pipeline | Encode | Derive.Spec | Lod // every category's rendition travels the SAME row array
  // a headerless stage carries NO header to read a depth class off, so the column is an Option rather than a
  // forged `false`: absence admits every leg and the producer's own declared band width settles it
  type Input = { readonly suffix: string; readonly headerless: boolean; readonly deep: Option.Option<boolean> }
  type Budget = Config.Config.Success<typeof _governance>
  type Held = { readonly io: NodeIO; readonly budget: Budget }
}

// each row TAKES its package option record and ANSWERS the step value carrying the deferred apply: options type per
// row, one governed record closing the table against the key tuple, and a step exists only where a row minted it
const _TRANSFORMS = {
  prune: (options: PruneOptions): Asset.Step => ({ step: "prune", run: () => prune(options) }),
  dedup: (options: DedupOptions): Asset.Step => ({ step: "dedup", run: () => dedup(options) }),
  // an AEC container is overwhelmingly repeated geometry: meshopt shrinks bytes and does nothing for draw calls,
  // while this row collapses reused Mesh references into EXT_mesh_gpu_instancing — the extension row landed on
  // _EXTENSIONS in the same pass, because _vocabulary re-proves the EMITTED document and would refuse its own product
  instance: (options: InstanceOptions): Asset.Step => ({ step: "instance", run: () => instance(options) }),
  // the atlas kernel is an injected instance exactly like the meshopt trio; the row states chart and pack policy
  // and never drives Atlas by hand, because the transform owns the document-to-declaration flatten and the
  // xref gather that rebuilds every attribute across the vertices a seam split
  unwrap: (options: Omit<UnwrapOptions, "watlas">): Asset.Step => ({ step: "unwrap", run: () => unwrap({ ...options, watlas }) }),
  // BRANCH-OWNED row: neither meshopt kernel ships a glTF transform, so the wiring is Accessor mint plus
  // createTransform. The shipped `tangents` row is refused beside it — it takes a mikktspace callback whose
  // three-argument shape carries no indices, forcing an unweld, and admits a second unadmitted wasm lineage.
  tangents: (options: { readonly flags?: ReadonlyArray<TangentsFlags>; readonly overwrite?: boolean }): Asset.Step => ({
    step: "tangents",
    run: () =>
      createTransform("tangents", (document) => {
        // BOUNDARY ADAPTER: the package's own Transform contract is `(doc) => void` over a mutable property
        // graph, so the write is a statement by the seam's shape; the selection above it stays expression-shaped
        Array.forEach(
          Array.filterMap(_primitives(document), (primitive) =>
            // a primitive missing any input carries no derivable frame and one already holding TANGENT is left
            // whole unless the row asks — silence is the correct answer, never a forged zero vector
            primitive.getAttribute("TANGENT") !== null && options.overwrite !== true
              ? Option.none()
              : Option.map(
                  Option.all({
                    position: _attribute(primitive, "POSITION"),
                    normal: _attribute(primitive, "NORMAL"),
                    uv: _attribute(primitive, "TEXCOORD_0"),
                  }),
                  (held) => ({ primitive, held }),
                ),
          ),
          ({ held, primitive }) =>
            primitive.setAttribute(
              "TANGENT",
              document
                .createAccessor("TANGENT")
                .setType("VEC4") // xyz plus the handedness w every three shader reads off the frame
                .setArray(
                  MeshoptTangents.generateTangents(
                    Option.getOrNull(_indices(primitive)), // unindexed input is `null` by the kernel's own contract, never an empty array
                    new Float32Array(held.position),
                    _STRIDE.vec3,
                    new Float32Array(held.normal),
                    _STRIDE.vec3,
                    new Float32Array(held.uv),
                    _STRIDE.vec2,
                    options.flags === undefined ? [] : [...options.flags],
                  ),
                ),
            ),
        )
      }),
  }),
  quantize: (options: QuantizeOptions): Asset.Step => ({ step: "quantize", run: () => quantize(options) }),
  weld: (options: WeldOptions): Asset.Step => ({ step: "weld", run: () => weld(options) }),
  join: (options: JoinOptions): Asset.Step => ({ step: "join", run: () => join(options) }),
  flatten: (options: FlattenOptions): Asset.Step => ({ step: "flatten", run: () => flatten(options) }),
  palette: (options: PaletteOptions): Asset.Step => ({ step: "palette", run: () => palette(options) }),
  reorder: (options: Omit<ReorderOptions, "encoder">): Asset.Step => ({ step: "reorder", run: () => reorder({ ...options, encoder: MeshoptEncoder }) }),
  meshopt: (options: Omit<MeshoptOptions, "encoder">): Asset.Step => ({ step: "meshopt", run: () => meshopt({ ...options, encoder: MeshoptEncoder }) }),
  simplify: (options: Omit<SimplifyOptions, "simplifier">): Asset.Step => ({ step: "simplify", run: () => simplify({ ...options, simplifier: MeshoptSimplifier }) }),
} as const satisfies Record.ReadonlyRecord<(typeof _STEPS)[number], (options: never) => Asset.Step>

const _classifiedInput = (bytes: Uint8Array): Asset.Input =>
  Option.match(Array.findFirst(_INPUTS, (row) => Array.every(row.magic, (byte, at) => bytes[at] === byte)), {
    onNone: () => ({ suffix: "raw", headerless: true, deep: Option.none() }), // no container magic: the plane IS headerless pixel data
    onSome: (row) => ({ suffix: row.suffix, headerless: false, deep: Option.some(row.deep) }),
  })

const _deflated = (deflate: Asset.Deflate | undefined) =>
  deflate === undefined ? [] : [_DEFLATE[deflate.codec].flag, `${deflate.level}`]

const _framed = (options: Asset.Framing, declared: Asset.Ktx, input: Asset.Input): ReadonlyArray<string> => {
  const shape = _LAYERS[declared.layerLaw]
  const mip = _MIPS[options.mipPolicy]
  const folds = options.generate === true && mip.folds && !input.headerless // --generate-mipmap and --normalize both refuse raw input
  const source = Option.getOrElse(shape.origin, () => options.origin ?? _STORAGE_ORIGIN)
  return [
    "--format", _target(declared).cli,
    "--assign-tf", _TRANSFERS[declared.colorSpace].tf, // 8-bit input carries no transfer: unassigned, the CLI guesses srgb
    "--assign-primaries", _space(declared).cli,
    "--assign-texcoord-origin", _ORIGINS[source].cli,
    "--fail-on-color-conversions",
    ...(source === _STORAGE_ORIGIN ? [] : ["--convert-texcoord-origin", _ORIGINS[_STORAGE_ORIGIN].cli]),
    ...(declared.mips === 0 ? ["--runtime-mipmap"] : ["--levels", `${declared.mips}`]),
    ...(folds ? ["--generate-mipmap", "--mipmap-filter", mip.filter] : []),
    ...(folds && mip.normalize ? ["--normalize"] : []),
    ...Option.match(shape.flag, { onNone: () => [], onSome: (flag) => flag === "--cubemap" ? [flag] : [flag, `${declared.layers}`] }),
    ...(options.swizzle === undefined ? [] : ["--swizzle", options.swizzle]),
    ...(options.inputSwizzle === undefined ? [] : ["--input-swizzle", options.inputSwizzle]),
    // --width/--height are REQUIRED with --raw and RESAMPLE the plane without it: the input class alone decides
    ...(input.headerless ? ["--raw", "--width", `${declared.width}`, "--height", `${declared.height}`] : []),
  ]
}

// one row per subcommand: the option record is the row's own, the flag fold is deferred to the staged input class,
// and `proves` carries the declaration the emitted container must re-gate against
const _KTX = {
  create: (options: Asset.Create): Asset.Spawn => ({
    command: "create",
    sources: options.levels,
    emits: "container",
    leaf: "ktx2",
    // the leg and the input must agree on DEPTH CLASS: a headerless stage carries no header to contradict the
    // declaration and admits every leg, while a PNG feeding `deep` and an EXR feeding a block leg both refuse
    // at `gate` with the row's own name, which is where every other engine's refusals already land
    admit: (input) => Option.match(input.deep, { onNone: () => true, onSome: (deep) => deep === (options.leg === "deep") }),
    // the create leg is the ONE row that assigns origin and swizzle, so it is the one row whose proof carries
    // them: every other subcommand leaves both container fields exactly as it found them
    proves: Option.some({
      declared: options.declared,
      stated: Option.some({
        origin: Option.getOrElse(_LAYERS[options.declared.layerLaw].origin, () => options.origin ?? _STORAGE_ORIGIN),
        swizzle: Option.fromNullable(options.swizzle),
      }),
    }),
    flags: (input) => [
      ..._framed(options, options.declared, input),
      ...(options.leg === "deep" ? [] : [
        "--encode", options.leg === "uastc" ? "uastc" : "basis-lz",
        ...(options.quality === undefined ? [] : options.leg === "uastc" ? ["--uastc-quality", `${options.quality}`] : ["--qlevel", `${options.quality}`]),
        ...(options.leg === "etc1s" && options.clevel !== undefined ? ["--clevel", `${options.clevel}`] : []),
        ...(options.leg === "uastc" && options.rdo !== undefined ? ["--uastc-rdo", "--uastc-rdo-l", `${options.rdo}`] : []),
        ...(options.normal === true ? ["--normal-mode"] : []), // direction semantics alone; RDO stays off
      ]),
      ..._deflated(options.deflate),
    ],
  }),
  encode: (options: { readonly declared: Asset.Blocked; readonly codec: "uastc" | "basis-lz"; readonly quality?: number; readonly deflate?: Asset.Deflate }): Asset.Spawn => ({
    command: "encode",
    sources: [],
    admit: Function.constTrue, // a spawn over an already-minted container reads no input class to vote on
    emits: "container",
    leaf: "ktx2",
    proves: Option.some({ declared: options.declared, stated: Option.none() }), // an encode re-codes texels and touches neither metadata field
    flags: () => [
      "--codec", options.codec,
      ...(options.quality === undefined ? [] : options.codec === "uastc" ? ["--uastc-quality", `${options.quality}`] : ["--qlevel", `${options.quality}`]),
      ..._deflated(options.deflate),
    ],
  }),
  transcode: (options: { readonly target: "bc1" | "bc3" | "bc4" | "bc5" | "bc7" | "astc" | "etc-rgb" | "etc-rgba" | "eac-r11" | "eac-rg11" | "r8" | "rg8" | "rgb8" | "rgba8"; readonly deflate?: Asset.Deflate }): Asset.Spawn => ({
    command: "transcode",
    sources: [],
    admit: Function.constTrue, // a spawn over an already-minted container reads no input class to vote on
    emits: "container",
    leaf: "ktx2",
    proves: Option.none(), // the product is a desktop-native payload no wire declaration admits: the validator alone proves it
    flags: () => ["--target", options.target, ..._deflated(options.deflate)],
  }),
  deflate: (options: { readonly declared: Asset.Ktx; readonly deflate: Asset.Deflate }): Asset.Spawn => ({
    command: "deflate",
    sources: [],
    admit: Function.constTrue, // a spawn over an already-minted container reads no input class to vote on
    emits: "container",
    leaf: "ktx2",
    proves: Option.some({ declared: options.declared, stated: Option.none() }), // supercompression rewrites level data alone
    flags: () => ["--warnings-as-errors", ..._deflated(options.deflate)], // a second deflate over an already-supercompressed file is the caller's error
  }),
  extract: (options: { readonly store: Texture.PlaneFormat; readonly level?: number; readonly layer?: number; readonly face?: number; readonly transcode?: "r8" | "rg8" | "rgb8" | "rgba8"; readonly raw?: boolean }): Asset.Spawn => ({
    command: "extract",
    sources: [],
    admit: Function.constTrue, // a spawn over an already-minted container reads no input class to vote on
    emits: "image",
    // tool output answers PNG for an eight-bit store and EXR for a deep one whatever leaf it is handed, so the suffix derives from the store row
    leaf: options.raw === true ? "raw" : options.transcode !== undefined || _STORES[options.store].block ? "png" : "exr",
    proves: Option.none(),
    flags: () => [
      ...(options.level === undefined ? [] : ["--level", `${options.level}`]),
      ...(options.layer === undefined ? [] : ["--layer", `${options.layer}`]),
      ...(options.face === undefined ? [] : ["--face", `${options.face}`]),
      ...(options.transcode === undefined ? [] : ["--transcode", options.transcode]),
      ...(options.raw === true ? ["--raw"] : []),
    ],
  }),
  validate: (options: { readonly payload: Texture.Payload }): Asset.Spawn => ({
    command: "validate",
    sources: [],
    admit: Function.constTrue, // a spawn over an already-minted container reads no input class to vote on
    emits: "report",
    leaf: "ktx2",
    proves: Option.none(),
    // --gltf-basisu rides the payload's own transcodes column: a deep RGBSDA container fails error-6301 under it
    flags: () => [...(_PAYLOADS[options.payload].transcodes ? ["--gltf-basisu"] : []), "--format", "mini-json"],
  }),
} as const satisfies Record.ReadonlyRecord<Asset.Verb, (options: never) => Asset.Spawn>

const _BIN = "ktx" as const

const _governance = Config.all({
  // spawns saturate hardware concurrency through the CLI's own thread pool, so the process budget and the
  // spawn deadline are ONE service-construction fact, exactly as the raster plane governs its libvips pool
  threads: Config.integer("ASSET_ENCODE_THREADS").pipe(Config.withDefault(0)),
  deadline: Config.duration("ASSET_SPAWN_DEADLINE").pipe(Config.withDefault(Duration.minutes(10))),
})

const _CONTAINER_INPUT = { suffix: "ktx2", headerless: false, deep: Option.none() } satisfies Asset.Input // a spawn over an existing container votes on nothing // a spawn over an existing container reads no input class

const _spawn = (
  budget: Asset.Budget,
  spawn: Asset.Spawn,
  input: Asset.Input,
  paths: ReadonlyArray<string>,
  key: string,
) => {
  const command = Command.make(
    _BIN,
    spawn.command,
    ...spawn.flags(input),
    ...(budget.threads > 0 && spawn.emits !== "report" ? ["--threads", `${budget.threads}`] : []),
    ...paths,
  )
  const bound = <A>(self: Effect.Effect<A, Platform.PlatformError, CommandExecutor.CommandExecutor>) =>
    Effect.mapError(self, (fault) => new AssetFault({ reason: "tool", key, detail: fault.message })).pipe(
      Effect.timeoutFail({
        duration: budget.deadline,
        onTimeout: () => new AssetFault({ reason: "tool", key, detail: `<deadline ${spawn.command}>` }),
      }),
    )
  return { code: bound(Command.exitCode(command)), text: bound(Command.string(command)) }
}

const _conform = (budget: Asset.Budget, payload: Texture.Payload, file: string, key: string) =>
  Effect.gen(function* () {
    // Command.string collects stdout and never reads process status: the verdict is the report's own `valid` field
    const text = yield* _spawn(budget, _KTX.validate({ payload }), _CONTAINER_INPUT, [file], key).text
    const report = yield* Effect.mapError(
      Schema.decodeUnknown(Schema.parseJson(_Report))(text),
      (fault) => new AssetFault({ reason: "tool", key, detail: fault.message }),
    )
    yield* report.valid
      ? Effect.void
      : _refuse("gate", key, Array.map(report.messages, (row) => `${row.type}-${row.id} ${row.message}`).join("; "))
  })

const _proof = Effect.gen(function* () {
  const text = yield* Effect.mapError(
    Command.string(Command.make(_BIN, "--help")), // presence + roster, never --version: the binary prints GIT-NOTFOUND there
    (fault) => new AssetFault({ reason: "tool", key: _BIN, detail: fault.message }),
  )
  // anchored at the command-list line: a bare substring read passes `encode` on the `transcode` row alone
  const missing = Array.filter(_VERBS, (verb) => !new RegExp(`^\\s+${verb}\\s`, "m").test(text))
  yield* Array.isNonEmptyReadonlyArray(missing) ? _refuse("tool", _BIN, missing.join(" ")) : Effect.void
  return yield* Effect.mapError(_governance, (fault) => new AssetFault({ reason: "tool", key: _BIN, detail: fault.message }))
})
```

## [04]-[ENGINE_PLANES]

- Owner: the CATEGORY rows and the service over them — each category binds one `Derive.Plane` (`_container`, whose open builds the gated graph and whose emit clones the document per pipeline row, folds its steps, re-proves the emitted vocabulary, re-encodes, and reads its cluster census; `_ktx`, whose open classifies and stages the fetched plane to a scoped temp file and whose emit spawns one subcommand per row, conforms and re-gates its own product, and lands it; `Derive.raster`, the derivative plane's own row seated here unchanged; `_points`, whose open proves the graph once and whose emit clones per row and folds the branch-owned point kernels), one `_ENGINES` fold arm, and one `_GATES` admission arm; `Asset` is the service whose construction proves every category's seams once and whose `pipe`/`gate` pair is the whole entry surface.
- Packages: `object/file.md` (`Derive.fanout`, `Derive.Plane`, `Derive.Row`, `Derive.Receipt` — the one spine); `object/store.md` (`ObjectStore` — put, get, reference, grant ride the spine); `@effect/platform` (`FileSystem.FileSystem`, `Path.Path`, `Command`); `@rasm/ts/core` (`Convention` — the `assetTransformed`/`assetTranscodeDuration` instrument rows).
- Entry: `Asset.pipe(sourceKey, rows)` after a container or plane lands; the engine table folds every kind present and each engine runs the same spine, so re-running is a proven noop end to end — every re-put lands 412 and every grant re-mints against the same keys.
- Receipt: the spine's `Derive.Receipt` per row — container evidence carries the landed byte count beside the cluster census its row's policy asked for, `ktx` evidence carries the byte count beside the classification a container product proves and an extracted image cannot, and raster evidence is the derivative plane's own codec provenance and source measures — and both ride the derivative reference (`derivative:<sourceKey>`) so source release cascades. Every `meshopt` product's evidence also carries its encoder lineage as `meshoptimizer@<pin>` — the installed distribution version read once at boot from the package's own manifest, because the wasm surface exposes `ready`/`supported` and no version readout (the `version?` parameters on `encodeVertexBufferLevel`/`encodeGltfBuffer` select the ENCODING format version, never report the library's) — so the pin rides the receipt beside the byte count, and a catalog bump re-keying meshopt-encoded derivatives is a readable divergence accepted by design, never a silent one.
- Growth: an asset category is one `_ENGINES` row over its own `Derive.Plane`, one `_GATES` arm, one `Asset.Row` kind, and one `Asset.Declared` member; the guard pairs close engines, gates, rows, and declarations against one another in BOTH directions, so any missing quarter fails at the namespace declaration while `pipe` and `gate` never change.
- Law: the plane is category-general and the next category proves it — the spine, the entry pair, the store put, the `assets/<digest>` address law, and the `assetTransformed` partition all take a new category as data, so a font atlas, a video container, or a point cloud family lands as its row set with every consumer untouched; a category noun inside the entry surface, the receipt shape, or the service body is the first-consumer capture this law forecloses. The raster plane is that proof already run — the FIRST engine on the spine was the one category the entry pair could not name — and the points plane is its second run: each seating cost one `_ENGINES` row, one `_GATES` arm, one `Asset.Row` kind, and one `Asset.Declared` member with `pipe` and `gate` untouched.
- Law: the point category's kernels read in their own frame — `getScale` reads BEFORE any `target_error` so a prune error states in MESH units and the fold converts to the relative value the kernel takes (an absolute error passed as relative silently decimates by orders of magnitude); decimation runs first so order and every attribute gather operate on the survivor set; prune rides the index array alone, because an unindexed cloud has no islands to disconnect and a forged identity index would spend the kernel to prove nothing.
- Law: the fold is fault-blind as well as kind-blind — each engine raises its OWN family on the spine's `E` channel and the entry states the union, so a category whose plane is owned by a sibling page joins without a translation layer and without widening any other engine's channel.
- Law: decode once, clone N — the container opens through `readBinary` once, `inspect` lifts the census once for every row's `admit` vote, and each pipeline row folds over `cloneDocument`, because `document.transform` MUTATES the graph and two pipelines sharing one document corrupt each other.
- Law: every engine proves its own product before the store sees a byte — a container re-proves its extension vocabulary after the fold (a step may write vocabulary the read never carried), and a `ktx` container runs the Khronos validator then the header gate against the row's declaration; a drifted flag set or a foreign extension refuses at emit, never at a consumer.
- Law: the `ktx` engine is file-in/file-out — the entry scopes the whole pass so staged input and output die with the call rather than with the caller's graph, spawns run ONE at a time because the CLI already saturates hardware concurrency through its own thread pool, and every spawn carries the governance deadline so a wedged binary fails the row instead of the fiber.
- Law: every input of one encode shares the base plane's class — a level or face fetched into a headerless set beside a container-borne base refuses at `gate`, because `--raw` reads every input the same way and a mixed set encodes garbage the extent check alone passes.
- Law: emitted bytes are ordinary objects — the spine mints each product's `ContentKey` over encoded bytes, conditional re-put makes replays idempotent, and the receipts carry no served path: the manifest names leafs, the iac/ui pair owns the address join.
- Law: every terminal disposition taps the convention rows — the `assetTransformed` counter partitions on engine plane AND outcome, where outcome is `landed` or the fault's own `FaultClass` kind, so a boot-refused gate, an absent decoder, and a spawned-encoder failure all land on the counter that carries the success share; the `assetTranscodeDuration` histogram brackets the emit untagged. Names and dimensions read off the core `Convention` owner, no signal-site literal.

```typescript signature
import { Cause, Exit, Metric, type Scope } from "effect"
import { type CommandExecutor, FileSystem, Path } from "@effect/platform"
import { Convention } from "@rasm/ts/core"
import { ObjectStore } from "./store.ts"

const _PIPE = { flight: 2 } as const // half the raster fan: a container fold holds a whole property graph per row

const _transformed = Convention.mount(Convention.metric.assetTransformed)
const _transcoded = Convention.mount(Convention.metric.assetTranscodeDuration)

const _outcome = (plane: Asset.Kind, outcome: string) =>
  Metric.increment(Metric.tagged(Metric.tagged(_transformed, Convention.rasm.assetEngine, plane), Convention.rasm.assetOutcome, outcome))

// onExit fires ONCE after the outcome settles — the single emission point for an outcome dimension; a
// defect or interrupt is no disposition and lands on the span, never the counter
const _counted = <A, R>(plane: Asset.Kind, self: Effect.Effect<A, AssetFault, R>) =>
  Effect.onExit(self, (exit) =>
    Exit.match(exit, {
      onFailure: (cause) =>
        Option.match(Cause.failureOption(cause), {
          onNone: () => Effect.void,
          onSome: (fault) => _outcome(plane, fault.class),
        }),
      onSuccess: () => _outcome(plane, "landed"),
    }))

// a boot refusal carries no landed half, and its own reason routes it: an absent wasm decoder is the container
// plane's, an absent binary the ktx plane's, so the share stays honest and no construction double-counts
const _refused = <A, R>(plane: Asset.Kind, self: Effect.Effect<A, AssetFault, R>) =>
  Effect.tapError(self, (fault) => _outcome(plane, fault.class))

// BRANCH-OWNED census: the clusterizer ships no glTF transform, so the wiring is the kernel over the folded
// document's own accessors — one meshlet batch per indexed primitive with its per-meshlet sphere-and-cone bound,
// which is exactly the culling evidence a renderer needs and the byte count cannot state
const _clustered = (twin: Document, row: Asset.Clusters): ReadonlyArray<Asset.Meshlets> =>
  Array.filterMap(_primitives(twin), (primitive) =>
    Option.map(Option.all({ indices: _indices(primitive), position: _attribute(primitive, "POSITION") }), (held) => {
      const positions = new Float32Array(held.position)
      const buffers = MeshoptClusterizer.buildMeshlets(
        held.indices,
        positions,
        _STRIDE.vec3,
        row.maxVertices,
        row.maxTriangles,
        row.coneWeight,
      )
      return { meshlets: buffers.meshletCount, bounds: MeshoptClusterizer.computeMeshletBounds(buffers, positions, _STRIDE.vec3) }
    }))

// the cloud tally both the gate and the fold read, so declaration proof and emitted evidence share one census
const _tallied = (document: Document): Asset.Cloud => ({
  points: Array.reduce(_primitives(document), 0, (total, primitive) =>
    total + Option.match(Option.fromNullable(primitive.getAttribute("POSITION")), { onNone: () => 0, onSome: (accessor) => accessor.getCount() })),
  primitives: _primitives(document).length,
})

// BRANCH-OWNED point fold: neither kernel ships a glTF transform, so the wiring is the accessors' own arrays.
// KERNEL — the permutations below are kernel-minted (simplifyPoints answers survivor indices, reorderPoints a
// spatial order over exactly the array it was handed), so every composed index is in-range by the kernels' own
// contracts and the unchecked-index fallback is unreachable.
const _gathered = (accessor: Accessor, picks: Uint32Array): void => {
  const held = accessor.getArray()
  if (held === null) return
  const size = accessor.getElementSize()
  // BOUNDARY ADAPTER: the typed-array constructor reach is the one platform seam a generic gather needs
  const next = new (held.constructor as new (length: number) => typeof held)(picks.length * size)
  picks.forEach((pick, rank) => next.set(held.subarray(pick * size, (pick + 1) * size), rank * size))
  accessor.setArray(next)
}

const _cloud = (twin: Document, row: Asset.Lod): void =>
  Array.forEach(_primitives(twin), (primitive) =>
    Option.match(_attribute(primitive, "POSITION"), {
      onNone: Function.constVoid,
      onSome: (position) => {
        const positions = new Float32Array(position)
        const census = positions.length / _STRIDE.vec3
        // decimate first: the survivor set is what order and every attribute gather operate on
        const survivors = row.decimate === undefined
          ? Uint32Array.from({ length: census }, (_, rank) => rank)
          : MeshoptSimplifier.simplifyPoints(
              positions,
              _STRIDE.vec3,
              Math.max(1, Math.round(census * row.decimate.ratio)),
              Option.getOrUndefined(Option.map(_attribute(primitive, "COLOR_0"), (held) => new Float32Array(held))),
              _STRIDE.vec3,
              row.decimate.colorWeight,
            )
        // prune rides the index array alone — an unindexed cloud has no islands to disconnect, so the row's
        // error is meaningless there and the fold skips rather than forging an identity index; the error is
        // stated in MESH units and getScale reads BEFORE it, because an absolute error passed as relative
        // silently decimates by orders of magnitude
        Option.match(row.prune === undefined ? Option.none() : _indices(primitive), {
          onNone: Function.constVoid,
          onSome: (indices) => {
            const kept = MeshoptSimplifier.simplifyPrune(
              indices,
              positions,
              _STRIDE.vec3,
              (row.prune?.error ?? 0) / MeshoptSimplifier.getScale(positions, _STRIDE.vec3),
            )
            primitive.getIndices()?.setArray(kept)
          },
        })
        const staged = new Float32Array(survivors.length * _STRIDE.vec3)
        survivors.forEach((pick, rank) =>
          staged.set(positions.subarray(pick * _STRIDE.vec3, (pick + 1) * _STRIDE.vec3), rank * _STRIDE.vec3))
        const sequence = row.order ? MeshoptEncoder.reorderPoints(staged, _STRIDE.vec3) : undefined
        const picks = sequence === undefined
          ? survivors
          : Uint32Array.from(sequence, (at) => survivors[at] ?? 0) // unreachable fallback: kernel-minted permutation
        Array.forEach(primitive.listAttributes(), (accessor) => _gathered(accessor, picks))
      },
    }))

// the points plane: open proves the graph once, emit clones per row exactly as the container plane does, and
// the evidence re-tallies the TWIN so the receipt describes the cloud the store actually holds
const _points = (io: NodeIO): Derive.Plane<
  Asset.Lod,
  Asset.Cloud,
  Document,
  { readonly bytes: number; readonly cloud: Asset.Cloud },
  AssetFault,
  ObjectStore
> => ({
  name: "points",
  open: (bytes, source) => Effect.map(_opened(io, bytes, source), (handle) => ({ facts: _tallied(handle), handle })),
  admit: (row, cloud) => row.admit === undefined || row.admit(cloud),
  emit: (handle, rows, _cloudFacts, source) =>
    Effect.forEach(rows, (row) =>
      _counted("points", Effect.gen(function* () {
        const store = yield* ObjectStore
        const twin = cloneDocument(handle)
        yield* Effect.try({
          try: () => _cloud(twin, row),
          catch: (defect) => new AssetFault({ reason: "transform", key: source, detail: String(defect) }),
        })
        const emitted = yield* Effect.tryPromise({
          try: () => io.writeBinary(twin),
          catch: (defect) => new AssetFault({ reason: "emit", key: source, detail: String(defect) }),
        })
        const landed = yield* Effect.mapError(store.put(emitted), (fault) => new AssetFault({ reason: "emit", key: source, detail: fault.detail }))
        return { row, key: landed.key, evidence: { bytes: landed.bytes, cloud: _tallied(twin) } }
      })), { concurrency: _PIPE.flight }),
})

const _container = (io: NodeIO): Derive.Plane<
  Asset.Pipeline,
  InspectReport,
  Document,
  { readonly bytes: number; readonly clusters: Option.Option<ReadonlyArray<Asset.Meshlets>> },
  AssetFault,
  ObjectStore
> => ({
  name: "container",
  open: (bytes, source) => Effect.map(_opened(io, bytes, source), (handle) => ({ facts: inspect(handle), handle })),
  admit: (row, census) => row.admit === undefined || row.admit(census),
  emit: (handle, rows, _census, source) =>
    Effect.forEach(rows, (row) =>
      _counted("container", Effect.gen(function* () {
        const store = yield* ObjectStore
        const twin = cloneDocument(handle) // transform mutates the graph: clone per pipeline row, exactly the raster clone-N law
        yield* Effect.tryPromise({
          try: () => twin.transform(...Array.map(row.steps, (step) => step.run())),
          catch: (defect) => new AssetFault({ reason: "transform", key: source, detail: String(defect) }),
        })
        yield* _vocabulary(twin, source) // a step may write vocabulary the read never carried: the fold proves its own product
        const emitted = yield* Effect.tryPromise({
          try: () => io.writeBinary(twin),
          catch: (defect) => new AssetFault({ reason: "emit", key: source, detail: String(defect) }),
        })
        const landed = yield* Effect.mapError(store.put(emitted), (fault) => new AssetFault({ reason: "emit", key: source, detail: fault.detail }))
        // the cluster read runs on the TWIN, after the fold: a meshlet batch built before `simplify` describes
        // a topology the store never receives
        return {
          row,
          key: landed.key,
          evidence: { bytes: landed.bytes, clusters: Option.map(Option.fromNullable(row.clusters), (spec) => _clustered(twin, spec)) },
        }
      })), { concurrency: _PIPE.flight }),
})

const _staged = (leaf: string, bytes: Uint8Array) =>
  Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem
    const path = yield* Path.Path
    const home = yield* fs.makeTempDirectoryScoped()
    const staged = path.join(home, leaf)
    yield* Effect.mapError(fs.writeFile(staged, bytes), (fault) => new AssetFault({ reason: "emit", key: leaf, detail: fault.message }))
    return staged
  })

const _ktx = (budget: Asset.Budget): Derive.Plane<
  Asset.Encode,
  Asset.Input,
  string,
  { readonly bytes: number; readonly classified: Option.Option<Asset.Classified> },
  AssetFault,
  ObjectStore | FileSystem.FileSystem | Path.Path | CommandExecutor.CommandExecutor | Scope.Scope
> => ({
  name: "ktx",
  open: (bytes, source) =>
    Effect.suspend(() => {
      const input = _classifiedInput(bytes)
      return Effect.map(_staged(`${source}.${input.suffix}`, bytes), (staged) => ({ facts: input, handle: staged }))
    }),
  admit: (row, input) => (row.admit === undefined ? row.spawn.admit(input) : row.admit(input)), // the caller's override, else the row's own derived veto
  emit: (staged, rows, input, source) =>
    Effect.forEach(rows, (row) =>
      _counted("ktx", Effect.scoped(Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem
        const path = yield* Path.Path
        const store = yield* ObjectStore
        const out = path.join(yield* fs.makeTempDirectoryScoped(), `${row.name}.${row.spawn.leaf}`)
        // one file per level times the declared layer count, largest level first, cube faces in +X -X +Y -Y +Z -Z order
        const inputs = [staged, ...(yield* Effect.forEach(row.spawn.sources, (leaf) =>
          Effect.gen(function* () {
            const plane = yield* Effect.mapError(store.get(leaf), (fault) => new AssetFault({ reason: "emit", key: leaf, detail: fault.detail }))
            const held = _classifiedInput(plane)
            yield* held.headerless === input.headerless ? Effect.void : _refuse("gate", leaf, `${held.suffix}<>${input.suffix}`)
            return yield* _staged(`${leaf}.${held.suffix}`, plane)
          }))]
        const code = yield* _spawn(budget, row.spawn, input, [...inputs, out], source).code
        yield* code === 0 ? Effect.void : _refuse("tool", source, `${row.spawn.command} exit ${code}`)
        const bytes = yield* Effect.mapError(fs.readFile(out), (fault) => new AssetFault({ reason: "emit", key: source, detail: fault.message }))
        const classified = yield* Option.match(row.spawn.proves, {
          onNone: () => Effect.succeedNone,
          // encoders prove themselves twice before the store sees a byte: Khronos conformance, then declaration
          // agreement — and the emit side is the ONE seam that can also read back what its own flags assigned
          onSome: (proof) =>
            Effect.zipRight(
              _conform(budget, proof.declared.ktxPayload, out, source),
              Effect.asSome(_ktx2(bytes, proof.declared, source, proof.stated)),
            ),
        })
        const landed = yield* Effect.mapError(store.put(bytes), (fault) => new AssetFault({ reason: "emit", key: source, detail: fault.detail }))
        return { row, key: landed.key, evidence: { bytes: landed.bytes, classified } }
      }).pipe(Metric.trackDuration(_transcoded)))), { concurrency: 1 }), // spawns run serial: the CLI saturates its own thread pool
})

// the fold is fault-blind as well as kind-blind: the raster plane raises its own `DeriveFault` family and the
// spine already carries a foreign engine's channel, so the one entry needs no per-engine error translation
const _fan = <R extends Derive.Row, F, H, I, E, Env>(
  plane: Derive.Plane<R, F, H, I, E, Env>,
  source: ContentKey,
  rows: ReadonlyArray<R>,
) => Array.isNonEmptyReadonlyArray(rows) ? Derive.fanout(plane, source, rows) : Effect.succeed<ReadonlyArray<Derive.Receipt<I>>>([])

// each row owns its own narrowing, so the fold is kind-blind and a third category adds no branch to the entry
const _ENGINES = {
  container: (held: Asset.Held, source: ContentKey, rows: ReadonlyArray<Asset.Row>) =>
    _fan(_container(held.io), source, Array.filter(rows, (row): row is Asset.Pipeline => row.kind === "container")),
  ktx: (held: Asset.Held, source: ContentKey, rows: ReadonlyArray<Asset.Row>) =>
    _fan(_ktx(held.budget), source, Array.filter(rows, (row): row is Asset.Encode => row.kind === "ktx")),
  // the FIRST engine on the spine was the one category the entry pair could not name: a caller mixing a
  // thumbnail rendition with a KTX2 encode stated two calls and reconciled two receipt arrays by hand
  raster: (_held: Asset.Held, source: ContentKey, rows: ReadonlyArray<Asset.Row>) =>
    _fan(Derive.raster, source, Array.filter(rows, (row): row is Derive.Spec => row.kind === "raster")),
  points: (held: Asset.Held, source: ContentKey, rows: ReadonlyArray<Asset.Row>) =>
    _fan(_points(held.io), source, Array.filter(rows, (row): row is Asset.Lod => row.kind === "points")),
} as const

// the category admission record: one proof arm per category, the mapped contract making a missing arm a
// compile error and the generic indexed call correlating each declaration to its own proof shape — the
// gate itself never branches, because a category discriminates on evidence its declaration carries
// the graph reads that turn a caller's re-declaration into a derived fact — the functions catalog rules a KTX2
// texture classifies through `listTextureSlots` + `getTextureColorSpace` rather than a re-encode, and the mask
// is what picks a store row: a texture read only on R needs one channel, and nothing else on the plane computes it
const _textured = (handle: Document): ReadonlyArray<Asset.Textured> =>
  Array.map(handle.getRoot().listTextures(), (texture) => ({
    slots: listTextureSlots(texture),
    colorSpace: Option.fromNullable(getTextureColorSpace(texture)), // `null` is "no slot implies a space", never "linear"
    mask: getTextureChannelMask(texture),
    channels: listTextureChannels(texture),
  }))

const _GATES: {
  readonly [K in Asset.Kind]: (
    held: Asset.Held,
    bytes: Uint8Array,
    declared: Extract<Asset.Declared, { readonly category: K }>,
    key: string,
  ) => Effect.Effect<Asset.Proved[K], AssetFault>
} = {
  container: (held, bytes, _declared, key) =>
    Effect.map(_opened(held.io, bytes, key), (handle) => ({
      extensions: Array.map(handle.getRoot().listExtensionsUsed(), (extension) => extension.extensionName),
      report: inspect(handle), // the proven document's own census: the same facts every pipeline row's admit votes on
      textures: _textured(handle),
    })),
  ktx: (_held, bytes, declared, key) => _ktx2(bytes, declared, key, Option.none()), // a delivered file states no framing: the DFD clauses alone
  // the census crosses back as this plane's own family: the raster arm proves a codec and an extent through the
  // ONE libvips composer, so no image library is imported here and the two planes cannot drift on gate posture
  raster: (_held, bytes, declared, key) =>
    Effect.flatMap(
      Effect.mapError(Derive.probe(bytes, key), (fault) => new AssetFault({ reason: "gate", key, detail: fault.message })),
      (census) =>
        census.format === declared.format && census.width === declared.width && census.height === declared.height
          ? Effect.succeed(census)
          : _refuse("gate", key, `${census.format} ${census.width}x${census.height}`),
    ),
  // topology, census, and attribute roster all prove against the declaration: one non-POINTS primitive, one
  // absent semantic, or a point tally disagreeing with the declared count each refuses with its own evidence
  points: (held, bytes, declared, key) =>
    Effect.flatMap(_opened(held.io, bytes, key), (handle) => {
      const primitives = _primitives(handle)
      const alien = Array.some(primitives, (primitive) => primitive.getMode() !== _POINTS_MODE)
      const absent = Array.findFirst(declared.attributes, (semantic) =>
        Array.some(primitives, (primitive) => primitive.getAttribute(semantic) === null))
      const cloud = _tallied(handle)
      return alien
        ? _refuse("gate", key, "non-points primitive")
        : Option.match(absent, {
            onSome: (semantic) => _refuse("gate", key, `attribute ${semantic}`),
            onNone: () => cloud.points === declared.count ? Effect.succeed(cloud) : _refuse("gate", key, `${cloud.points} points`),
          })
    }),
}

declare namespace Asset {
  type Kind = keyof typeof _ENGINES
  // the declaration union is seated WITH the categories rather than beside the first one: a new category adds a
  // member here, an `_ENGINES` row, a `_GATES` arm, and an `Asset.Row` kind, and the guard sextet below fails
  // the namespace on any missing quarter
  type Declared = Container | Ktx | Raster | Points
  // the proven document ALREADY knows what a caller was being asked to re-declare: a texture bound to BaseColor
  // is sRGB, one bound to Normal or Occlusion is linear, and its channel mask is the evidence for r8 vs rg8 vs
  // rgba8 — so the census carries those facts and the ktx declaration stops being a second, caller-stated truth
  type Textured = {
    readonly slots: ReadonlyArray<string>
    readonly colorSpace: Option.Option<"srgb">
    readonly mask: number
    readonly channels: ReadonlyArray<TextureChannel>
  }
  type Census = {
    readonly extensions: ReadonlyArray<string>
    readonly report: InspectReport
    readonly textures: ReadonlyArray<Textured>
  }
  type Cloud = { readonly points: number; readonly primitives: number } // the point census the gate proves and the fold re-tallies on its own product
  type Proved = { readonly container: Asset.Census; readonly ktx: Asset.Classified; readonly raster: Derive.Probe; readonly points: Asset.Cloud } // per-category proof shapes, keyed by the one category vocabulary
  type Receipt = Effect.Effect.Success<ReturnType<(typeof _ENGINES)[Kind]>>[number] // the shipped extractor over the engine table: no hand-listed evidence union
  type Fan = Effect.Effect<
    ReadonlyArray<Receipt>,
    AssetFault | DeriveFault,
    ObjectStore | FileSystem.FileSystem | Path.Path | CommandExecutor.CommandExecutor
  >
  type _Served<K extends Kind = Row["kind"]> = K // guard closure over the category surfaces: a row kind with no engine fails here
  type _Total<K extends Row["kind"] = Kind> = K // an engine with no row kind fails here
  type _Gated<K extends Kind = keyof typeof _GATES> = K // a gate arm outside the category vocabulary fails here
  type _Gating<K extends keyof typeof _GATES = Kind> = K // a category with no admission arm fails here
  type _Declares<K extends Kind = Declared["category"]> = K // a declaration outside the vocabulary fails here
  type _Declaring<K extends Declared["category"] = Kind> = K // a category no declaration can name fails here
}

class Asset extends Effect.Service<Asset>()("data/Asset", {
  effect: Effect.gen(function* () {
    const budget = yield* _refused("ktx", _proof) // tool refusal and spawn budget both derive from construction, never a caller flag
    const io = yield* _refused("container", _io)
    const held: Asset.Held = { io, budget }
    return {
      // the one category-polymorphic admission: the declaration's own tag selects the arm and the mapped
      // contract types the proof, so a category-specific entry never grows beside this one
      gate: <K extends Asset.Kind>(bytes: Uint8Array, declared: Extract<Asset.Declared, { readonly category: K }>, key: string) =>
        _GATES[declared.category](held, bytes, declared, key),
      pipe: (sourceKey: ContentKey, rows: ReadonlyArray<Asset.Row>): Asset.Fan =>
        Effect.forEach(Record.values(_ENGINES), (run) => run(held, sourceKey, rows)).pipe(
          Effect.map(Array.flatten),
          Effect.scoped, // the staged input dies with the pass, never with the caller's graph
          Effect.withSpan("data.asset", { attributes: { source: sourceKey } }),
        ),
    }
  }),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Asset, AssetFault }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
