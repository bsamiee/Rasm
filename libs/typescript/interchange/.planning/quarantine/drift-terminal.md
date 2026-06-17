# [INTERCHANGE_DRIFT_TERMINAL]

The contract-drift tolerance terminal for untrusted ingress: the decode fold that classifies every decode outcome as `Identical`, `Additive`, or `Breaking` drift, emits a structured drift-report built from the Effect Schema `ArrayFormatter` error paths, enforces the `refinement/schema-refinement.md` decode budgets, and sanitizes DOM-bound text through `isomorphic-dompurify`. `QuarantineFold` is the single tolerance terminal every decode passes through; a `Breaking` drift surfaces as a typed `FaultDetail.Quarantine` carrying the exact failing field path the UI renders and the platform telemetry-ships.

## [1]-[INDEX]

One cluster: `[2]-[DRIFT_TERMINAL]` owns the drift classifier, the structured report, the budget gate, and the sanitizer.

## [2]-[DRIFT_TERMINAL]

- Owner: `QuarantineFold`, the single tolerance terminal every decode passes through, classifying the decode outcome into the `ContractDrift` `Data.TaggedEnum` and emitting a `DriftReport` from the `ParseError` paths. The fold layers tolerance over the settled wire shape, never modifying it; an `Additive` drift skip-decodes and survives, a `Breaking` drift faults through `faults/fault-family.md`, an `Identical` decode passes through unchanged.
- Cases: the fold decodes once leniently (`onExcessProperty: "preserve"`) for the value and once strictly (`onExcessProperty: "error"`) for the drift probe, so an extra additive member classifies `Additive` and the stream survives with the decoded value and the report attached, a structural `Breaking` drift (a removed required field, a changed type, a budget breach) carries the `DriftReport` and faults, and a clean decode classifies `Identical`. The `ISSUE_DRIFT` vocabulary keys each `ArrayFormatter` issue tag to its `DriftClass` — an `Unexpected` excess-property issue is additive, a `Missing`/`Type`/`Forbidden` issue is breaking — so the classifier never restates the tag knowledge inline and a `Breaking` drift carries the exact failing field path the UI renders and the platform telemetry-ships, never an opaque marker discarding the diagnostic. A transport disconnect is a `transport/transport.md` `FaultDetail`, never a drift class on this fold.
- Entry: the three-case fold is total over every decode outcome — `Identical` for a clean decode, `Additive` for a tolerated excess member, `Breaking` for a structural mismatch — and a `Breaking` drift surfaces as the `faults/fault-family.md` `FaultDetail.Quarantine` carrying the report. The fold gates the two untrusted paths — the structured-text decode passes through `refinement/schema-refinement.md` `decodeBounded` so a recursion-depth breach faults before exhaustion, and the artifact stream passes through `boundedFrames` so a frame-count or assembled-byte breach faults before the sink allocates. Every DOM-bound text field on a decoded value passes through `sanitize` (`isomorphic-dompurify` `DOMPurify.sanitize`) before it reaches a render binding, so a mXSS payload in a wire string never reaches the DOM; the sanitizer runs at the quarantine terminal once, never re-applied per render.
- Packages: `effect` for `Schema.decodeUnknownEither` and `ArrayFormatter`, the `Record` vocabulary dispatch, `Either`, and `Data.TaggedEnum`; `isomorphic-dompurify` for the DOM-bound text sanitization on both the browser and the node-prerender path.
- Growth: a new union case the C# side adds lands as one literal on the owning rail and one `Additive` fold arm; a new ingress budget lands as one `DecodeBudget` row gated here; a new DOM-bound field lands as one `sanitize` row; zero second tolerance terminal.
- Boundary: tolerance is a consumption behavior layered over the settled shape, never a modification of it; the drift classifier reads the `ParseError` paths and never re-validates a decoded value; the sanitizer is the rendered-text ceiling and the budgets are the byte and shape ceilings — the two ingress ceilings the charter owns; a `Breaking` drift never silently survives and an `Additive` drift never silently faults.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
type DriftClass = "identical" | "additive" | "breaking";

type ContractDrift<Domain> = Data.TaggedEnum<{
  readonly Identical: { readonly value: Domain };
  readonly Additive: { readonly value: Domain; readonly report: DriftReport };
  readonly Breaking: { readonly report: DriftReport };
}>;

// --- [MODELS] ------------------------------------------------------------------------
interface DriftPath {
  readonly path: ReadonlyArray<string | number>;
  readonly message: string;
}

interface DriftReport {
  readonly drift: DriftClass;
  readonly paths: ReadonlyArray<DriftPath>;
}

// --- [SERVICES] ----------------------------------------------------------------------
interface QuarantineFold<Domain> {
  readonly fold: (input: unknown) => ContractDrift<Domain>;
}

// --- [OPERATIONS] --------------------------------------------------------------------
const ISSUE_DRIFT = { Unexpected: "additive", Missing: "breaking", Type: "breaking", Forbidden: "breaking" } as const satisfies Record<string, DriftClass>;

const driftReportOf = (error: ParseResult.ParseError): DriftReport => {
  const issues = ArrayFormatter.formatErrorSync(error);
  const breaking = issues.some((i) => (Record.get(ISSUE_DRIFT, i._tag).pipe(Option.getOrElse(() => "breaking" as const))) === "breaking");
  return { drift: breaking ? "breaking" : "additive", paths: issues.map((i) => ({ path: i.path, message: i.message })) };
};

const ContractDrift = Data.taggedEnum<ContractDrift<unknown>>();

const quarantineFold = <A, I>(schema: Schema.Schema<A, I>): QuarantineFold<A> => {
  const lenient = Schema.decodeUnknownEither(schema, { onExcessProperty: "preserve" });
  const strict = Schema.decodeUnknownEither(schema, { onExcessProperty: "error" });
  return {
    fold: (input) =>
      Either.match(lenient(input), {
        onLeft: (error) => ContractDrift.Breaking({ report: driftReportOf(error) }) as ContractDrift<A>,
        onRight: (value) =>
          Either.match(strict(input), {
            onLeft: (error) => ContractDrift.Additive({ value, report: driftReportOf(error) }) as ContractDrift<A>,
            onRight: () => ContractDrift.Identical({ value }) as ContractDrift<A>,
          }),
      }),
  };
};

const sanitize = (text: string): string => DOMPurify.sanitize(text, { USE_PROFILES: { html: false } });
```
