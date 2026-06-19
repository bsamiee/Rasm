# [INTERCHANGE_FAULT]

The exhaustive fault reconstruction: one `Data.TaggedEnum` family rebuilding every .NET fault the wire emits across all four C# packages, the `faultTagOf` and `faultOf` total projections from the `grpc-status-details-bin` trailer, and one `Match.tagsExhaustive` render table the SPA reads through so no surface hand-rolls fault rendering per rail. `FaultDetailRail` reads the typed trailer riding every `proto` call rather than a `CodecKey` row — the fault detail is not a codec format but the trailer-decode owner the `Codec/codec.md` `DecodeRail` proto row's calls carry. The family is ONE tagged owner whose constructors and error channel are the same symbol — never an empty `Data.TaggedError` class beside a parallel enum and never four parallel error rails.

## [1]-[INDEX]

- [1]-[FAULT_FAMILY]: the tagged fault family, the total wire projections, the render table.
- [2]-[TS_PROJECTION]: the fault-detail trailer wire shape the rail decodes.


## [2]-[FAULT_FAMILY]

- Owner: `FaultDetail`, the single `Data.TaggedEnum` family reconstructing every .NET fault across the four C# packages, with one `Match.tagsExhaustive` table the SPA renders through. The family is the full cross-package fault case set — `ComputeFault`, `StoreFault`, `HopFault`, `ConfigError`, and the `Quarantine` landing for the not-yet-enumerated wire `case`. `FaultDetailRail` is the boundary that reads the trailer and the total cause fold every transport call site stamps.
- Cases: `FaultDetailRail` reads the `grpc-status-details-bin` trailer through `ConnectError.findDetails(FaultDetailSchema)` and decodes the `FaultDetailWire` (package, code, case, message, evidence, correlation, hlcPhysical, hlcLogical) into the matching `FaultDetail` tagged case through `faultTagOf`, the `PACKAGE_TAG` vocabulary `Record` keying every enumerated .NET package to its `_tag`; a package the vocabulary does not enumerate folds to `FaultDetail.Quarantine` through the `Record.get` default rather than throwing, so the wire→tag hop is itself total and an unmapped package is a typed quarantine fault, never a silent miss. `faultOf` then constructs the exact tagged case through one `Match.value(faultTagOf(wire))` fold whose `Match.exhaustive` terminal proves at compile time that every tag has an arm, never a parallel `Record<FaultTag, (w) => FaultDetail>` constructor table restating the case list the fold already owns and never a `Match.when` chain over `wire.package` re-encoding the `PACKAGE_TAG` package→tag knowledge. The exhaustive render proves at compile time through `Match.tagsExhaustive` that every tagged case has a render arm, so adding a fault tag without a TS arm is a typecheck failure. `HopFault` is the dual-minted case — the wire-derived path lands `reason: "wire"` carrying the numeric connect code in the `code` evidence row, and the branch-local folds mint a typed `HopReason` against the same closed vocabulary rather than a raw string: the artifact and gateway folds mint `frame-crc-mismatch`, `empty-artifact`, and `command-disabled`, the `Codec/patch.md` recorded-intent fold mints `patch-apply-failed`/`patch-test-mismatch` (the `applyRecordedIntent` one-slot-per-op result keyed by the failing op index) and `field-mask-empty` (the `fieldMaskLower` empty-mask guard), and the `platform` browser-tier local hops mint `uncaught`/`render` (the `fault-capture` crash and error-boundary sink reconstructing an uncaught browser exception) and `worker-reassemble`/`worker-decode`/`worker-protocol` (the `worker/` `DecodeWorkerPool` marshal seam). Every local hop carries its discriminant in `reason` and never a `code` field — `HopFault` carries no numeric `code`, only the wire-derived case folds the connect code into the `code` evidence row — so a `code`-keyed `HopFault` construction is the named consumer defect.
- Entry: `fromConnect` is the one polymorphic cause→FaultDetail entry keyed by error shape — the total infallible projection every transport call site folds through. `ConnectError.from` normalizes any cause to a `ConnectError` (a fetch `AbortError` and `TimeoutError` alike land `Code.Canceled`, a server-enforced deadline lands `Code.DeadlineExceeded`); `findDetails(FaultDetailSchema)` reads the typed trailer through the generated descriptor when present and `faultTagOf` maps the pair total; a cause carrying no trailer lands `FaultDetail.Quarantine` carrying the numeric connect `Code` with its name in the `connectCode` evidence row, so a deadline-exceeded leg surfaces distinctly from a client-side abort through that name, never a throw and never a leaked `ParseResult.ParseError`. `fromConnect` is the single infallible boundary fold every transport and gateway call site stamps onto its `E` channel — one cause→FaultDetail entrypoint, never a second typed-failure member beside it. `faultOf` constructs the exact tagged case per tag rather than a dynamic indexed `FaultDetail[tag](...)` that violates each case's strict field shape — `ComputeFault`/`StoreFault` carry the numeric wire code plus correlation, `ConfigError`/`Quarantine` carry the numeric wire code plus evidence, and `HopFault` carries the closed `HopReason` discriminant plus evidence with the wire-derived path setting `reason: "wire"` and folding the numeric connect code into the `code` evidence row.
- Packages: `@connectrpc/connect` for `findDetails` and `ConnectError.from`, `@bufbuild/protobuf` for the `FaultDetail` descriptor, `effect` for `Data.TaggedEnum`/`Data.taggedEnum`, the `Record` vocabulary dispatch, and `Match.tagsExhaustive`.
- Growth: a new .NET package lands as one `PACKAGE_TAG` row and, when it carries a new tag, one `Data.TaggedEnum` case, one `faultOf` `Match.when` arm, and one `Match.tagsExhaustive` render arm — the two exhaustive folds break at compile time until the arms land; a new branch-local hop fault lands as one `HopReason` literal on the closed vocabulary; a sixth fault rail is the named defect — every fault is one case on the one family and every wire package is one vocabulary row.
- Boundary: the family is the only fault rail in the branch and `FaultDetail` is BOTH the error-channel type and the constructor namespace (`FaultDetail.HopFault({...})`), never an empty `Data.TaggedError` class shadowing a separate enum; a generic reported-value or `IReceipt`-style abstraction replacing the typed cases is the named defect; the `faultTagOf` projection makes the wire→tag hop total so the exhaustiveness guarantee reaches the wire boundary; the render exhaustiveness is a typecheck obligation, never a runtime default arm.

```ts contract
type HopReason =
  | "wire"
  | "frame-crc-mismatch"
  | "empty-artifact"
  | "command-disabled"
  | "patch-apply-failed"
  | "patch-test-mismatch"
  | "field-mask-empty"
  | "uncaught"
  | "render"
  | "worker-reassemble"
  | "worker-decode"
  | "worker-protocol";

type FaultDetail = Data.TaggedEnum<{
  readonly ComputeFault: { readonly code: number; readonly evidence: Record<string, string>; readonly correlation: string };
  readonly StoreFault: { readonly code: number; readonly evidence: Record<string, string>; readonly correlation: string };
  readonly HopFault: { readonly reason: HopReason; readonly evidence: Record<string, string> };
  readonly ConfigError: { readonly code: number; readonly evidence: Record<string, string> };
  readonly Quarantine: { readonly code: number; readonly evidence: Record<string, string> };
}>;
const FaultDetail = Data.taggedEnum<FaultDetail>();
type FaultTag = FaultDetail["_tag"];
type FaultWire = typeof FaultDetailWire.Type;

const PACKAGE_TAG = { "Rasm.Compute": "ComputeFault", "Rasm.Persistence": "StoreFault", "Rasm.AppHost": "HopFault", "Rasm.AppUi": "ConfigError" } as const satisfies Record<string, FaultTag>;

const faultTagOf = (wire: { readonly package: string }): FaultTag =>
  Record.get(PACKAGE_TAG, wire.package).pipe(Option.getOrElse(() => "Quarantine" as const));

const faultOf = (wire: FaultWire): FaultDetail =>
  Match.value(faultTagOf(wire)).pipe(
    Match.withReturnType<FaultDetail>(),
    Match.when("ComputeFault", () => FaultDetail.ComputeFault({ code: wire.code, evidence: wire.evidence, correlation: wire.correlation })),
    Match.when("StoreFault", () => FaultDetail.StoreFault({ code: wire.code, evidence: wire.evidence, correlation: wire.correlation })),
    Match.when("HopFault", () => FaultDetail.HopFault({ reason: "wire", evidence: { ...wire.evidence, code: String(wire.code) } })),
    Match.when("ConfigError", () => FaultDetail.ConfigError({ code: wire.code, evidence: wire.evidence })),
    Match.when("Quarantine", () => FaultDetail.Quarantine({ code: wire.code, evidence: wire.evidence })),
    Match.exhaustive,
  );

const renderFault = (fault: FaultDetail): string =>
  Match.value(fault).pipe(
    Match.tagsExhaustive({
      ComputeFault: (f) => `compute:${f.code}`,
      StoreFault: (f) => `store:${f.code}`,
      HopFault: (f) => `hop:${f.reason}`,
      ConfigError: (f) => `config:${f.code}`,
      Quarantine: (f) => `quarantine:${f.evidence.connectCode ?? f.code}`,
    }),
  );

interface FaultDetailRail {
  readonly fromConnect: (cause: unknown) => FaultDetail;
}

const faultDetailRail: FaultDetailRail = {
  fromConnect: (cause: unknown): FaultDetail => {
    const error = ConnectError.from(cause);
    return Option.match(Array.head(error.findDetails(FaultDetailSchema)), {
      onNone: () => FaultDetail.Quarantine({ code: error.code, evidence: { connectCode: Code[error.code], message: error.rawMessage } }),
      onSome: faultOf,
    });
  },
};
```

## [3]-[TS_PROJECTION]

- Owner: the `FaultDetailWire` trailer shape the rail decodes, sourced from `csharp:Rasm.Compute/Runtime/channels#TS_PROJECTION` (the `FAULT_PROJECTION` cluster's one `FaultDetail` family carrying every typed fault across the wire). `FaultWire` derives from this shape through `typeof FaultDetailWire.Type`, never a parallel hand-written trailer type; the `package` field composes the branch-owned `RasmPackage` four-package literal from `Contract/inventory#WIRE_LAW`, never re-spelling the package set the inventory page already fixes.
- Entry: the trailer carries `package=1 string`, `code=2 int32`, `case=3 string`, `message=4 string`, `evidence=5 map`, `correlation=6 string`, `hlcPhysical=7 Timestamp`, and `hlcLogical=8 uint64`; `code` decodes as `Schema.Int` and `hlcLogical` as bigint against the `uint64` width; the `RasmPackage` literal gates the `faultTagOf` projection.
- Packages: `effect` `Schema` for the trailer surface; the `RasmPackage` four-package literal composed from `Contract/inventory#WIRE_LAW`.

```ts contract
import { RasmPackage } from "@rasm/ts";

const FaultDetailWire = Schema.Struct({
  package: RasmPackage,
  code: Schema.Int,
  case: Schema.String,
  message: Schema.String,
  evidence: Schema.Record({ key: Schema.String, value: Schema.String }),
  correlation: Schema.String,
  hlcPhysical: Schema.String,
  hlcLogical: Schema.BigIntFromSelf,
});
```
