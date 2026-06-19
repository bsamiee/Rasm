# [INTERCHANGE_REFINEMENT]

The decode-enforcement vocabulary: the `Schema.brand` identity slots and `Schema.filter` bounds that make the versioning invariants decode-enforced rather than prose, extended with the decode-budget rows that harden the untrusted-ingress boundary. Each refinement is one row on `Codec/codec.md` `DecodeRail`; this page owns the brand, filter, and budget grammar every rail composes. The budgets are the security floor the `Ingress/quarantine.md` charter relies on — a breached recursion depth, frame count, or assembled-byte ceiling fails decode before a JSON-bomb, unbounded-recursion, or unbounded-frame-stream payload exhausts the runtime.

## [01]-[INDEX]

- [01]-[REFINEMENT]: the brand identity slots, the filter bounds, and the decode budgets.

## [02]-[REFINEMENT]

- Owner: `SchemaRefinement`, the brand-and-filter decode-enforcement vocabulary every rail carries as rows — `Schema.brand` on the identity slots, `Schema.filter` on the envelope and header bounds, and `DecodeBudget` on the ingress ceilings. One owner carries identity, bound, and budget; a parallel refinement object per concern is the named defect.
- Cases: `Schema.brand` on guid correlation identifiers, 16-byte content keys, smart-enum ordinal keys, and the RFC 6901 `JsonPointer` path so a raw string never enters an identity slot; `Schema.filter` on the HLC-logical number-envelope bound and the fixed-width snapshot-header discriminants so a breached envelope or malformed prefix fails decode rather than truncating. The `JsonPointer` brand is the path-and-from slot the `Codec/patch.md` `PatchOpWire` `move`/`copy`/`add`/`remove`/`replace`/`test` arms carry — the RFC 6901 form `Pointer.fromJSON` parses (`""` the document root, `/a/b` a token path, `~0`/`~1` the `~`/`/` escapes), so the patch rail resolves a branded `JsonPointer` against the target rather than admitting an unescaped raw `/`-joined string the `rfc6902` `Pointer` evaluator would reject. The decode budgets are three `Schema.filter`/`Stream` bounds — `maxDepth` capping the structured-text recursion the json-stj rail admits, `maxFrames` capping the artifact-frame stream length, and `maxAssembledBytes` capping the reassembled artifact ceiling — each a typed row, never a prose check.
- Entry: every identity field on a wire shape carries its brand at the decode boundary, so a downstream fold reads a `Guid`/`ContentKey`/`OrdinalKey`/`JsonPointer` and never a raw string; the budget rows gate the two untrusted-ingress paths — the structured-text decode passes through `decodeBounded` carrying the `maxDepth`/`maxAssembledBytes` budget, and the artifact-frame stream passes through `Stream.take(maxFrames)` plus the assembled-byte filter before reassembly. The DOM-bound sanitization row lives at `Ingress/quarantine.md`; the budget rows are the byte and shape ceilings, the sanitization is the rendered-text ceiling.
- Packages: `effect` for `Schema.brand`, `Schema.filter`, and the `Stream` bound primitives.
- Growth: a new versioning invariant lands as one `Schema.brand` or `Schema.filter` row; a new ingress budget lands as one `DecodeBudget` field and one bound; zero new refinement owner.
- Boundary: a refinement re-validates nothing an earlier decode admitted; the budgets gate untrusted ingress only and never the same-origin trusted lane; the content-key brand is the single 16-byte `ContentKey` notion the whole branch reads, never a second identity slot; the `JsonPointer` brand is the single RFC 6901 path notion the patch rail resolves against — a raw `string` `path`/`from` slot on `PatchOpWire` is the named defect, and a second path-identity slot beside `JsonPointer` is the rejected form.

```ts contract
const Guid = Schema.String.pipe(Schema.filter((s) => /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(s)), Schema.brand("Guid"));
const ContentKey = Schema.Uint8ArrayFromSelf.pipe(Schema.filter((b) => b.length === 16), Schema.brand("ContentKey"));
const OrdinalKey = Schema.String.pipe(Schema.brand("OrdinalKey"));
const JsonPointer = Schema.String.pipe(Schema.filter((s) => s === "" || /^(\/(?:[^/~]|~0|~1)*)+$/.test(s)), Schema.brand("JsonPointer"));
const HlcLogical = Schema.Number.pipe(Schema.filter((n) => Number.isInteger(n) && n >= 0 && n <= Number.MAX_SAFE_INTEGER), Schema.brand("HlcLogical"));
const HeaderDiscriminant = Schema.Number.pipe(Schema.filter((n) => Number.isInteger(n) && n >= 0), Schema.brand("HeaderDiscriminant"));

class SchemaRefinement extends Schema.Class<SchemaRefinement>("SchemaRefinement")({
  guid: Guid,
  contentKey: ContentKey,
  ordinalKey: OrdinalKey,
  jsonPointer: JsonPointer,
  hlcLogical: HlcLogical,
  headerDiscriminant: HeaderDiscriminant,
}) {}

type RefinedIdentity = Schema.Schema.Type<typeof SchemaRefinement>;

interface DecodeBudget {
  readonly maxDepth: number;
  readonly maxFrames: number;
  readonly maxAssembledBytes: number;
}

const INGRESS_BUDGET: DecodeBudget = { maxDepth: 64, maxFrames: 4_096, maxAssembledBytes: 268_435_456 };

const depthOf = (value: unknown, depth: number): number =>
  typeof value !== "object" || value === null
    ? depth
    : Math.max(depth, ...Object.values(value as Record<string, unknown>).map((v) => depthOf(v, depth + 1)));

const decodeBounded = <A, I>(schema: Schema.Schema<A, I>, budget: DecodeBudget) =>
  (input: unknown): Effect.Effect<A, ParseResult.ParseError> =>
    depthOf(input, 0) > budget.maxDepth
      ? Effect.fail(new ParseResult.ParseError({ issue: new ParseResult.Forbidden(Schema.encodedSchema(schema).ast, input, `depth>${budget.maxDepth}`) }))
      : Schema.decodeUnknown(schema)(input);

const boundedFrames = <E>(frames: Stream.Stream<ArtifactFrameWire, E>, budget: DecodeBudget): Stream.Stream<ArtifactFrameWire, E> =>
  frames.pipe(
    Stream.take(budget.maxFrames),
    Stream.mapAccum(0, (assembled, frame) =>
      assembled + frame.payload.byteLength > budget.maxAssembledBytes
        ? [assembled, Option.none<ArtifactFrameWire>()]
        : [assembled + frame.payload.byteLength, Option.some(frame)]),
    Stream.filterMap(identity),
  );
```
