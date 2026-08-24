# [CORE_CARRIER]

W3C propagation crosses the interchange plane as ONE typed `traceparent`/`tracestate`/`baggage` value, total parse/print folds, `rasm.tenant` promotion, a closed transport table, and the Connect `-bin` typed-metadata lane. HTTP, Connect, NATS, MQTT v5, CloudEvents, and Kafka inject and extract through one codec. Malformed input folds to absence under the restart posture, while ordered, bounded folds keep output byte-stable. Module `core/src/interchange/carrier.ts` admits a transport as one dialect row, a baggage axis as one member key, and a typed family as one name row.

One module seats both owners, since a message envelope's extension slot IS a carrier frame. `Carrier` composes only the `value` floor's `Identity.Tenant` and hands dialect frames to the runtime wave as data. `Event` composes `Digest.Key`, the local classification policy, and the `cloudevents` message-envelope class, then publishes the generic admission bridge and the Rasm profile's grammar, generated extension roster, and mint. Kafka, NATS, MQTT, and CloudEvents realize their rows, while `interchange/invoke` composes Connect. Frame values recover the dialect discriminant, so one mapped handler record owns dispatch.

## [01]-[INDEX]

- [02]-[CONTEXT_VALUE]: typed triple, brands, total parse/print folds, span lift; `Carrier`.
- [03]-[TENANT_BAGGAGE]: `rasm.tenant` promotion and scoped recovery decode; `Carrier`.
- [04]-[DIALECT_TABLE]: closed frame rows, inject/extract dispatch, `-bin` typed-metadata lane; `Carrier`.
- [05]-[EVENT_ENVELOPE]: generic SDK admission and generated protobuf bridge beside the closed Rasm profile; `Event`.

## [02]-[CONTEXT_VALUE]

- Owner: `Traceparent`, `Carrier.State`, `Carrier.Member`, and `Carrier.Context` own parent, tracestate, baggage, and optionality.
- Law: Malformed parents restart; invalid state or baggage members drop independently, and every drop RETURNS as a `Fault.Drop` occurrence.
- Law: Baggage properties prove delimiter-safe W3C grammar before entering context.
- Law: Baggage admits 64 members, 4096 encoded bytes per member, and 8192 encoded bytes total.
- Law: Version `ff`, extensions on version zero, and all-zero identities refuse; current sampled/random flag bits are retained and reserved input bits are ignored as the W3C receiver rule requires.
- Law: Parent print emits the supported version-zero spelling with sampled and random-trace-id flags, clearing every input bit this implementation does not understand.
- Law: `_stateRows` enforces grammar, first-key-wins uniqueness, member count, and aggregate text bounds in one fold, each arm naming its own drop reason.
- Law: Baggage print uses Effect `Encoding` before member and aggregate encoded-byte admission.
- Law: `Carrier.span` lifts structural span fields; `Carrier.Current` scopes ingress; `Carrier.current` overlays the live parent and preserves lists.
- Tests: version-zero flags `00` through `03` preserve sampled/random bits, reserved bits admit then clear on print, version-zero extension tails refuse, higher-version tails admit, and `ff`/all-zero identities refuse.
- Growth: a new context list (a fourth W3C header) is one field on the triple with its parse/print row; a new parse bound is one `_CEILING` field.
- Boundary: Tracer owns the live span; data owns persisted contexts; `Carrier` owns pure context values and folds.
- Packages: `effect` (`Array`, `Effect`, `Either`, `Encoding`, `Option`, `Schema`, `String`, `pipe`); `../value/fault.ts` (`Fault.Drop`, `Fault.Ledger`); `../value/identity.ts` (`Identity.Tenant`).

```typescript signature
import { decodeBinaryHeader, encodeBinaryHeader } from "@connectrpc/connect"
import type { Wire } from "./codec.ts" // type-only: the census family union, carrying no runtime edge to the codec owner
import { Headers, HttpTraceContext } from "@effect/platform"
import { Array, Context, Effect, Either, Encoding, HashSet, Option, ParseResult, Predicate, Record, Schema, String, pipe } from "effect"
import { Convention } from "../observe/convention.ts"
import { Fault } from "../value/fault.ts"
import { Identity } from "../value/identity.ts"

const _CEILING = { state: 32, stateText: 512, baggage: 64, baggageMember: 4096, baggageText: 8192 } as const

const _TraceId = Schema.String.pipe(Schema.pattern(/^(?!0{32})[0-9a-f]{32}$/), Schema.brand("TraceId"))
const _SpanId = Schema.String.pipe(Schema.pattern(/^(?!0{16})[0-9a-f]{16}$/), Schema.brand("SpanId"))

class Traceparent extends Schema.Class<Traceparent>("Traceparent")({
  traceId: _TraceId,
  spanId: _SpanId,
  sampled: Schema.Boolean,
  random: Schema.Boolean,
}) {}

const _PARENT = /^([0-9a-f]{2})-([0-9a-f]{32})-([0-9a-f]{16})-([0-9a-f]{2})(-.+)?$/
const _STATE_KEY = /^[a-z][a-z0-9_\-*/]{0,255}$|^[a-z0-9][a-z0-9_\-*/]{0,240}@[a-z][a-z0-9_\-*/]{0,13}$/
const _STATE_VALUE = /^[\x20-\x2b\x2d-\x3c\x3e-\x7e]{0,255}[\x21-\x2b\x2d-\x3c\x3e-\x7e]$/
const _BAGGAGE_KEY = /^[!#$%&'*+\-.^_`|~0-9a-zA-Z]+$/
const _BAGGAGE_VALUE = /^[\x21\x23-\x2b\x2d-\x3a\x3c-\x5b\x5d-\x7e]*$/
// W3C baggage property grammar: token key, optional "=" then baggage-octets — every printable byte
// except the dquote, comma, semicolon, and backslash delimiters that would re-frame the header.
const _PROPERTY = /^[!#$%&'*+\-.^_`|~0-9a-zA-Z]+(?:=[\x21\x23-\x2b\x2d-\x3a\x3c-\x5b\x5d-\x7e]*)?$/

const _decodedParent = Schema.decodeUnknownOption(Traceparent)

// Effect Encoding owns the URI exception rail; carrier's restart boundary folds its typed Either to absence.
const _decodeUri = (value: string): Option.Option<string> => Either.getRight(Encoding.decodeUriComponent(value))
const _encodeUri = (value: string): Option.Option<string> => Either.getRight(Encoding.encodeUriComponent(value))

const _parent = (text: string): Option.Option<Traceparent> =>
  pipe(
    Option.fromNullable(_PARENT.exec(String.trim(text))),
    Option.filter(([, version, , , , extension]) =>
      version !== "ff" && (version !== "00" || extension === undefined)),
    Option.flatMap(([, , traceId, spanId, flags]) =>
      pipe(Number.parseInt(flags ?? "00", 16), (bits) =>
        _decodedParent({ traceId, spanId, sampled: (bits & 1) === 1, random: (bits & 2) === 2 }))),
  )

// Property tails re-print verbatim, so grammar is admission: a malformed tail drops here exactly as a malformed
// member drops, and the one fold serves parse and print identically. Empty segments are separator noise rather than
// material — a trailing semicolon is lawful W3C framing, so nothing was sent to drop and no occurrence names it.
const _properties = (properties: ReadonlyArray<string>): Carrier.Sifted<ReadonlyArray<string>> =>
  Array.partitionMap(
    Array.filter(Array.map(properties, String.trim), String.isNonEmpty),
    (property) =>
      _PROPERTY.test(property) ? Either.right(property) : Either.left(Fault.Drop.fact("unparsed", property, 1)),
  )

type _StateFold = {
  readonly rows: ReadonlyArray<Carrier.State>
  readonly seen: ReadonlyArray<string>
  readonly length: number
  readonly dropped: ReadonlyArray<Fault.Drop.Fact>
}

const _stateMember = ({ key, value }: Carrier.State): boolean => _STATE_KEY.test(key) && _STATE_VALUE.test(value)

// The four bounds are DEPENDENT — which rows survive uniqueness decides what the count ceiling reaches, and that
// decides what the aggregate budget can still hold — so one fold applies them in the declared order and every arm
// answers the reason its own bound means. Split across `filter`/`dedupeWith`/`take`/`reduce`, each stage discarded a
// DIFFERENT thing into the same shorter list, and a caller reading four rows where a peer sent forty could not tell a
// grammar refusal from a spent byte budget. The drop constructor is deferred, so an admitted row prices no fact.
const _stateRows = (rows: ReadonlyArray<Carrier.State>): Carrier.Sifted<ReadonlyArray<Carrier.State>> =>
  pipe(
    Array.reduce(
      rows,
      { rows: [], seen: [], length: 0, dropped: [] } satisfies _StateFold,
      (held, row): _StateFold => {
        const dropped = (reason: Fault.Drop.Reason, extent: number): ReadonlyArray<Fault.Drop.Fact> =>
          [...held.dropped, Fault.Drop.fact(reason, row.key, extent)]
        if (!_stateMember(row)) return { ...held, dropped: dropped("unparsed", 1) }
        if (Array.contains(held.seen, row.key)) return { ...held, dropped: dropped("coalesced", 1) }
        const seen = [...held.seen, row.key]
        if (held.rows.length === _CEILING.state) return { ...held, seen, dropped: dropped("truncated", 1) }
        const length = held.length + (Array.isEmptyReadonlyArray(held.rows) ? 0 : 1) + row.key.length + row.value.length + 1
        return length > _CEILING.stateText
          ? { ...held, seen, dropped: dropped("oversize", length - _CEILING.stateText) }
          : { rows: [...held.rows, row], seen, length, dropped: held.dropped }
      },
    ),
    (held) => [held.dropped, held.rows] as const,
  )

const _state = (text: string): Carrier.Sifted<ReadonlyArray<Carrier.State>> =>
  pipe(
    Array.partitionMap(
      Array.filter(Array.map(String.split(text, ","), String.trim), String.isNonEmpty),
      (entry): Either.Either<Carrier.State, Fault.Drop.Fact> =>
        pipe(
          Option.fromNullable(/^([^=]+)=(.*)$/.exec(entry)),
          Option.match({
            onNone: () => Either.left(Fault.Drop.fact("unparsed", entry, 1)),
            onSome: ([, key, value]) => Either.right({ key: key ?? "", value: value ?? "" }),
          }),
        ),
    ),
    ([unframed, framed]) =>
      pipe(_stateRows(framed), ([refused, admitted]) => [[...unframed, ...refused], admitted] as const),
  )

const _utf8 = { read: new TextDecoder(), write: new TextEncoder() } as const
const _bytes = (text: string): number => _utf8.write.encode(text).byteLength

// A member refused WHOLE takes its property tails with it, so the occurrence names the member alone: reporting the
// tails of a member no context will hold counts damage twice against one entry.
const _baggageMember = (entry: string): Carrier.Sifted<Option.Option<Carrier.Member>> =>
  pipe(String.trim(entry), (held) =>
    _bytes(held) > _CEILING.baggageMember
      ? [[Fault.Drop.fact("oversize", held, _bytes(held) - _CEILING.baggageMember)], Option.none()] as const
      : pipe(String.split(held, ";"), ([head, ...properties]) =>
          pipe(_properties(properties), ([refusedTails, tails]) =>
            pipe(
              Option.fromNullable(/^([^=]+)=(.*)$/.exec(head ?? "")),
              Option.filter(([, key, value]) =>
                key !== undefined && _BAGGAGE_KEY.test(key) && _BAGGAGE_VALUE.test(value ?? "")),
              Option.flatMap(([, key, value]) =>
                Option.map(_decodeUri(value ?? ""), (decoded) => ({ key: key ?? "", value: decoded, properties: tails }))),
              Option.match({
                onNone: (): Carrier.Sifted<Option.Option<Carrier.Member>> =>
                  [[Fault.Drop.fact("unparsed", held, 1)], Option.none()],
                onSome: (member): Carrier.Sifted<Option.Option<Carrier.Member>> => [refusedTails, Option.some(member)],
              }),
            ))))

// An over-budget header used to return `[]`, which read identically to a peer that sent no baggage at all — and
// `rasm.tenant` vanished with it. It now returns ONE occurrence naming the header and the bytes it overshot, and the
// member ceiling that follows names each entry it refused past the count rather than trimming the tail in silence.
const _baggage = (text: string): Carrier.Sifted<ReadonlyArray<Carrier.Member>> =>
  _bytes(text) > _CEILING.baggageText
    ? [[Fault.Drop.fact("oversize", "baggage", _bytes(text) - _CEILING.baggageText)], []]
    : Array.reduce(
        Array.filter(Array.map(String.split(text, ","), String.trim), String.isNonEmpty),
        [[], []] as Carrier.Sifted<ReadonlyArray<Carrier.Member>>,
        ([dropped, kept], entry): Carrier.Sifted<ReadonlyArray<Carrier.Member>> =>
          kept.length === _CEILING.baggage
            ? [[...dropped, Fault.Drop.fact("truncated", entry, 1)], kept]
            : pipe(_baggageMember(entry), ([refused, member]) => [
                [...dropped, ...refused],
                Option.match(member, { onNone: () => kept, onSome: (held) => [...kept, held] }),
              ]),
      )

const _printedParent = (parent: Traceparent): string =>
  `00-${parent.traceId}-${parent.spanId}-${(Number(parent.sampled) | (Number(parent.random) << 1)).toString(16).padStart(2, "0")}`

// Print re-applies the SAME admission to material the CALLER already holds, so its discards need no census here: the
// caller reads them off `Carrier.parse` over its own text, and every transport row states the forfeiture on `degrade`.
const _printedState = (rows: ReadonlyArray<Carrier.State>): string =>
  Array.join(Array.map(_stateRows(rows)[1], (row) => `${row.key}=${row.value}`), ",")

const _printedMember = ({ key, value, properties }: Carrier.Member): Option.Option<string> =>
  pipe(
    Option.some(key),
    Option.filter((held) => _BAGGAGE_KEY.test(held)),
    Option.flatMap(() => _encodeUri(value)),
    Option.map((encoded) => Array.join([`${key}=${encoded}`, ..._properties(properties)[1]], ";")),
    Option.filter((member) => _bytes(member) <= _CEILING.baggageMember),
  )

type _BaggageFold = { readonly members: ReadonlyArray<string>; readonly bytes: number }

const _printedMembers = (
  members: ReadonlyArray<Carrier.Member>,
  reserved = 0,
  limit = _CEILING.baggage,
): _BaggageFold =>
  pipe(
    members,
    Array.filterMap(_printedMember),
    Array.take(limit),
    Array.reduce({ members: [], bytes: reserved } satisfies _BaggageFold, (held, member) => {
      const bytes = held.bytes + (held.bytes > reserved || reserved > 0 ? 1 : 0) + _bytes(member)
      return bytes <= _CEILING.baggageText ? { members: [...held.members, member], bytes } : held
    }),
  )

const _printedBaggage = (members: ReadonlyArray<Carrier.Member>): string =>
  Array.join(_printedMembers(members).members, ",")
```

## [03]-[TENANT_BAGGAGE]

- Owner: `_TENANT` names the one tenancy baggage member.
- Owner: `promote` upserts `Identity.Tenant`; `tenant` decodes it through `Identity.Tenant.FromScope`.
- Law: `promote` replaces `rasm.tenant` with `Identity.Tenant.scope`, reserves its slot, and enforces member and byte ceilings.
- Law: `Identity.Tenant.FromScope` re-proves both key alphabets; malformed scope folds to `Option.none`.
- Law: `withoutTenant` removes only the promoted tenant member when a transport authenticates no tenant inverse; the remaining baggage and its occurrence evidence survive unchanged.
- Law: Resource stamping and propagation carry the same identity value.
- Growth: a second promoted axis (a deployment ring, a request class) is one member-key constant with its promote/read pair beside this one.
- Boundary: `Dial.Ambient` holds fiber tenancy; security consumes the recovered `Identity.Tenant`.
- Packages: `effect` (`Array`, `Option`); `../observe/convention.ts` (`Convention`); `../value/identity.ts` (`Identity.Tenant`).

```typescript signature
// Observe owns this baggage key outright, so one spelling site serves the branch and a rename there lands here
// rather than orphaning a twin no consumer can see diverge.
const _TENANT = Convention.rasm.tenant

const _decodedTenant = Schema.decodeUnknownOption(Identity.Tenant.FromScope)

const _promote = (context: Carrier.Context, tenant: Identity.Tenant): Carrier.Context =>
  pipe(
    { key: _TENANT, value: tenant.scope, properties: [] } satisfies Carrier.Member,
    (promoted) => ({
      promoted,
      reserved: Option.match(_printedMember(promoted), { onNone: () => _CEILING.baggageText, onSome: _bytes }),
    }),
    ({ promoted, reserved }) => ({
      promoted,
      admitted: _printedMembers(
        Array.filter(context.baggage, (member) => member.key !== _TENANT),
        reserved,
        _CEILING.baggage - 1,
      ),
    }),
    ({ promoted, admitted }) => ({
      ...context,
      baggage: [...Array.filterMap(admitted.members, _baggageMember), promoted],
    }),
  )

const _tenant = (context: Carrier.Context): Option.Option<Identity.Tenant> =>
  Option.flatMap(
    Array.findFirst(context.baggage, (member) => member.key === _TENANT),
    (member) => _decodedTenant(member.value),
  )

const _withoutTenant = (context: Carrier.Context): Carrier.Context => ({
  ...context,
  baggage: Array.filter(context.baggage, (member) => member.key !== _TENANT),
})
```

## [04]-[DIALECT_TABLE]

- Owner: `Carrier.Frame` maps each transport to its actual header value shape.
- Owner: `_dialects` and `Carrier.record` share immutable record-header read and write rows.
- Owner: `_BIN` carries typed metadata as base64 octets through Connect `-bin` headers.
- Law: `_dialects` maps one W3C context to string, repeated-string, byte, or extension-record frames; consumers use only that row.
- Law: `inject` omits absent fields and prints `tracestate` beneath a printed parent alone; `extract` always returns `Carrier.Extraction` — the total context beside the census of everything the parse refused.
- Law: a restarted parent, a refused traceparent, and an over-budget baggage header each name a MEASURED occurrence, so an empty list never reads as a peer that sent nothing.
- Law: print re-admits the caller's own material and publishes no census — the transport row's `degrade` states that forfeiture and `Carrier.parse` answers it on demand.
- Law: Missing or malformed W3C parents fall through to platform `b3` then `xb3` decoders; injection remains W3C-only.
- Law: Restart discards `tracestate`, so vendor rows survive the W3C parent alone and never a `b3`-recovered one.
- Law: B3 projection reads through the selected dialect row once, so every transport shares the same fallback.
- Law: `Row.read` accepts W3C and B3 keys; `Row.write` accepts only `Carrier.Injected` W3C keys.
- Law: `_BIN` maps a metadata name to its `Wire` family; `bin.set/get` carry that family's own octets and fold decode failure to absence.
- Law: Dialect rows decide selection, injection, and `degrade` alone — tenancy realizes through `promote`, and a context's lifetime ends with the `Carrier.Current` scope its caller opened.
- Law: Attribute-name prefixing is the BINDING's, per transport and never shared — HTTP prefixes `ce-`, Kafka `ce_`, MQTT nothing at all — so a binding seat spells its own and no constant here spans the three.
- Law: Kafka and `Carrier.record` share one `TextEncoder` and `TextDecoder` kernel.
- Law: `record.read` selects the first repeated value and detaches bytes.
- Law: `record.write` emits a fresh string record without importing a host byte type.
- Growth: Add a transport as one `Carrier.Frame` field and dialect row; add typed metadata or fallback as one table row.
- Boundary: Runtime owns clients and bindings; invoke composes `Carrier.current`, tenant promotion, Connect injection, and `_BIN`.
- Packages: `@connectrpc/connect`, `@effect/platform`, and `effect`.

```typescript signature
const _KEYS = ["traceparent", "tracestate", "baggage"] as const

// Zipkin ingress names, READ-only: the single-header form and the multi-header form the platform decoders parse.
const _B3 = ["b3", "x-b3-traceid", "x-b3-spanid", "x-b3-sampled", "x-b3-parentspanid"] as const

declare namespace Carrier {
  type Key = (typeof _KEYS)[number] | (typeof _B3)[number] // every header name a row READS
  type Injected = (typeof _KEYS)[number] // the write half is the W3C three alone, so "inject prints nothing else" is a parameter type rather than a discipline
  type State = { readonly key: string; readonly value: string }
  type Member = { readonly key: string; readonly value: string; readonly properties: ReadonlyArray<string> }
  type Context = {
    readonly parent: Option.Option<Traceparent>
    readonly state: ReadonlyArray<State>
    readonly baggage: ReadonlyArray<Member>
  }
  type Frame = {
    readonly cloudevents: Record.ReadonlyRecord<string, unknown>
    readonly connect: Headers.Headers
    readonly fanout: Record.ReadonlyRecord<string, string>
    readonly http: Record.ReadonlyRecord<string, string>
    readonly kafka: Record.ReadonlyRecord<string, Uint8Array>
    readonly mqtt: Record.ReadonlyRecord<string, string | ReadonlyArray<string>>
    readonly nats: Record.ReadonlyRecord<string, string>
  }
  // ONE carrier for every sifting fold on this page: the occurrences it refused beside whatever it admitted, whether
  // that is a roster or a single member. A second shape per arity would be two names for one answer.
  type Sifted<A> = readonly [dropped: ReadonlyArray<Fault.Drop.Fact>, kept: A]
  // The context is the value consumers thread; the census is what the parse refused reaching it. Fusing the two into
  // one widened context would put a decode ledger on a value `inject` also writes.
  type Extraction = { readonly context: Context; readonly dropped: Fault.Ledger.Census }
  type RecordHeader = Uint8Array | string | ReadonlyArray<Uint8Array | string> | undefined
  type RecordHeaders = Record.ReadonlyRecord<string, RecordHeader>
  type Dialect = keyof Frame
  type Row<F> = {
    readonly read: (frame: F, key: Key) => Option.Option<string>
    readonly write: (frame: F, key: Injected, value: string) => F
    readonly degrade: string // what this frame shape forfeits carrying a W3C context, stated per row and readable by consumers
  }
  type Bin = keyof typeof _BIN
  type Shape = {
    readonly Current: typeof _Current
    readonly Traceparent: typeof Traceparent
    readonly bin: {
      readonly names: typeof _BIN
      readonly get: (headers: Headers.Headers, name: Bin) => Option.Option<Uint8Array>
      readonly set: (headers: Headers.Headers, name: Bin, octets: Uint8Array) => Headers.Headers
    }
    readonly current: Effect.Effect<Context>
    readonly dialects: typeof _dialects // consumers read a row's `degrade` before selecting the transport that carries their context
    readonly empty: Context
    readonly extract: <K extends Dialect>(dialect: K, frame: Frame[K]) => Extraction
    readonly inject: <K extends Dialect>(dialect: K, context: Context, frame: Frame[K]) => Frame[K]
    readonly keys: typeof _KEYS
    readonly record: {
      readonly read: (headers: RecordHeaders) => Frame["kafka"]
      readonly write: (frame: Frame["kafka"]) => Record.ReadonlyRecord<string, string>
    }
    readonly parse: {
      readonly baggage: (text: string) => Sifted<ReadonlyArray<Member>>
      readonly traceparent: (text: string) => Option.Option<Traceparent>
      readonly tracestate: (text: string) => Sifted<ReadonlyArray<State>>
    }
    readonly print: {
      readonly baggage: (members: ReadonlyArray<Member>) => string
      readonly traceparent: (parent: Traceparent) => string
      readonly tracestate: (rows: ReadonlyArray<State>) => string
    }
    readonly promote: (context: Context, tenant: Identity.Tenant) => Context
    readonly span: (span: { readonly traceId: string; readonly spanId: string; readonly sampled: boolean }) => Context
    readonly tenant: (context: Context) => Option.Option<Identity.Tenant>
    readonly withoutTenant: (context: Context) => Context
  }
  type _Rows<T extends { readonly [K in Dialect]: Row<Frame[K]> } = typeof _dialects> = T
}

// Values name CENSUS families, so the guard binds this table to the codec roster: a renamed family fails at this
// declaration rather than attaching a typed header whose message class no producer mints.
const _BIN = {
  "rasm-stamp-bin": "HlcStampWire",
  "rasm-tenant-bin": "TenantContextWire",
} as const satisfies Record.ReadonlyRecord<`rasm-${string}-bin`, Wire.Family>

const _recordRow = <Value>(
  degrade: string,
  read: (value: Value) => Option.Option<string>,
  write: (value: string) => Value,
): Carrier.Row<Record.ReadonlyRecord<string, Value>> => ({
  degrade,
  read: (frame, key) => Option.flatMap(Option.fromNullable(frame[key]), read),
  write: (frame, key, value) => ({ ...frame, [key]: write(value) }),
})

const _records = {
  text: _recordRow("<repeated-values-unrepresentable>", (value: string) => Option.some(value), (value) => value),
  bytes: _recordRow(
    "<header-octets-read-as-utf8>",
    (value: Uint8Array) => Option.some(_utf8.read.decode(value)),
    (value) => _utf8.write.encode(value),
  ),
  read: (headers: Carrier.RecordHeaders): Carrier.Frame["kafka"] =>
    Record.fromEntries(
      Object.entries(headers).flatMap(([key, value]) =>
        pipe(
          value === undefined || Predicate.isString(value) || value instanceof Uint8Array ? value : value[0],
          (head) => head === undefined
            ? []
            : [[key, Predicate.isString(head) ? _utf8.write.encode(head) : new Uint8Array(head)] as const],
        )),
    ),
  write: (frame: Carrier.Frame["kafka"]): Record.ReadonlyRecord<string, string> =>
    Record.map(frame, (value) => _utf8.read.decode(value)),
} as const

const _dialects: { readonly [K in Carrier.Dialect]: Carrier.Row<Carrier.Frame[K]> } = {
  // Events carry extension attributes UNPREFIXED, so this row reads an event's own attribute record; binary-mode
  // bindings prefix those names into a header frame the http row already serves, and that prefix stays theirs.
  cloudevents: {
    degrade: "<attribute-record-only>",
    read: (frame, key) => Option.filter(Option.fromNullable(frame[key]), Predicate.isString),
    write: (frame, key, value) => ({ ...frame, [key]: value }),
  },
  connect: {
    degrade: "<repeats-flattened-before-arrival>", // `Headers` indexes one string per name, so a repeat never reaches this row
    read: (frame, key) => Headers.get(frame, key),
    write: (frame, key, value) => Headers.set(frame, key, value),
  },
  fanout: _records.text,
  http: _records.text,
  kafka: _records.bytes,
  mqtt: {
    degrade: "<repeated-user-property-first-wins>",
    read: (frame, key) =>
      Option.flatMap(Option.fromNullable(frame[key]), (held) =>
        Predicate.isString(held) ? Option.some(held) : Array.head(held)),
    write: (frame, key, value) => ({ ...frame, [key]: value }),
  },
  nats: _records.text,
}

const _record = {
  read: _records.read,
  write: _records.write,
} as const

const _inject = <K extends Carrier.Dialect>(dialect: K, context: Carrier.Context, frame: Carrier.Frame[K]): Carrier.Frame[K] =>
  pipe(
    { row: _dialects[dialect], baggage: _printedBaggage(context.baggage) },
    ({ row, baggage }) =>
      pipe(
        Option.match(context.parent, {
          onNone: () => frame, // no parent prints no state: vendor rows anchor to the parent whose trace minted them
          onSome: (parent) =>
            pipe(
              row.write(frame, "traceparent", _printedParent(parent)),
              (held) => Array.isNonEmptyReadonlyArray(context.state)
                ? row.write(held, "tracestate", _printedState(context.state))
                : held,
            ),
        }),
        (held) => baggage.length > 0 ? row.write(held, "baggage", baggage) : held,
      ),
  )

// Platform B3 decoders read a header frame, so the Zipkin names project off the row's own value codec into one
// `Headers` map — the dialect stays a value codec, and the two Zipkin grammars stay the platform's own parse.
const _zipkin = <K extends Carrier.Dialect>(dialect: K, frame: Carrier.Frame[K]): Headers.Headers =>
  Headers.fromInput(
    Record.fromEntries(
      Array.filterMap(_B3, (key) => Option.map(_dialects[dialect].read(frame, key), (value) => [key, value] as const)),
    ),
  )

// EXTRACTION, never a bare context: every bound this parse applies discards material a peer actually sent, so the
// occurrences RETURN beside the value and a caller reading an empty list reads WHY. `Carrier.Context` is unchanged and
// stays the value consumers thread; the census is the ledger's own fold over the three legs' occurrences, so no tally
// rides beside the parse and a fourth bound lands as one more fact rather than as a counter nothing reconciles.
const _extract = <K extends Carrier.Dialect>(dialect: K, frame: Carrier.Frame[K]): Carrier.Extraction =>
  pipe(
    { row: _dialects[dialect], w3c: Option.flatMap(_dialects[dialect].read(frame, "traceparent"), _parent) },
    ({ row, w3c }) => {
      // one projection feeds both Zipkin grammars: the fallback is already lazy, so re-projecting per decoder would
      // rebuild the same header map twice on every W3C-absent hop
      const parent = Option.orElse(w3c, () =>
        pipe(_zipkin(dialect, frame), (zipkin) =>
          Option.flatMap(
            Option.orElse(HttpTraceContext.b3(zipkin), () => HttpTraceContext.xb3(zipkin)),
            (external) => _span(external).parent, // the recovered external span lifts through the one structural span fold
          )))
      const offered = row.read(frame, "tracestate")
      // Restart drops what it cannot anchor: vendor rows key to the W3C parent that carried them, so a refused parent
      // and a b3-recovered one both admit an empty list rather than lineage naming a trace this hop no longer
      // continues. That discard is now a MEASURED occurrence carrying the bytes it refused, where the bare `[]` it
      // replaces read identically to a peer that shipped no vendor rows at all.
      const [refusedState, state]: Carrier.Sifted<ReadonlyArray<Carrier.State>> = Option.isSome(w3c)
        ? Option.match(offered, { onNone: () => [[], []], onSome: _state })
        : Option.match(offered, {
            onNone: (): Carrier.Sifted<ReadonlyArray<Carrier.State>> => [[], []],
            onSome: (text) => [[Fault.Drop.fact("unanchored", "tracestate", _bytes(text))], []],
          })
      // Baggage travels on its own W3C specification and survives a restarted parent, so its admission stays unconditional.
      const [refusedBaggage, baggage] = Option.match(row.read(frame, "baggage"), {
        onNone: (): Carrier.Sifted<ReadonlyArray<Carrier.Member>> => [[], []],
        onSome: _baggage,
      })
      // A parent the frame OFFERED and no decoder recovered is the loss the restart posture used to swallow whole: an
      // absent header and a refused one both produced `Option.none()` and nothing downstream could separate them.
      const refusedParent: ReadonlyArray<Fault.Drop.Fact> =
        Option.isNone(parent) && Option.isSome(row.read(frame, "traceparent"))
          ? [Fault.Drop.fact("unparsed", "traceparent", 1)]
          : []
      return {
        context: { parent, state, baggage },
        dropped: Fault.Ledger.from([...refusedParent, ...refusedState, ...refusedBaggage]),
      }
    },
  )

const _empty: Carrier.Context = { parent: Option.none(), state: [], baggage: [] }

class _Current extends Context.Reference<_Current>()("core/Carrier/Current", {
  defaultValue: () => _empty,
}) {}

const _current: Effect.Effect<Carrier.Context> = Effect.map(
  Effect.all({ carried: _Current, span: Effect.option(Effect.currentSpan) }),
  ({ carried, span }) =>
    pipe(Option.match(span, { onNone: () => _empty, onSome: _span }), (live) => ({
      parent: Option.orElse(live.parent, () => carried.parent),
      state: carried.state,
      baggage: carried.baggage,
    })),
)

const _span = (span: { readonly traceId: string; readonly spanId: string; readonly sampled: boolean }): Carrier.Context => ({
  ..._empty,
  // Effect exposes sampled but not W3C's generation provenance, so a live span cannot assert the random flag.
  parent: _decodedParent({ traceId: span.traceId, spanId: span.spanId, sampled: span.sampled, random: false }),
})

const Carrier: Carrier.Shape = {
  Current: _Current,
  Traceparent,
  bin: {
    names: _BIN,
    get: (headers, name) =>
      Option.flatMap(Headers.get(headers, name), (value) =>
        Either.getRight(Either.try(() => decodeBinaryHeader(value)))), // the codec's DataLoss throw folds to absence by the restart posture
    set: (headers, name, octets) => Headers.set(headers, name, encodeBinaryHeader(octets)),
  },
  current: _current,
  dialects: _dialects,
  empty: _empty,
  extract: _extract,
  inject: _inject,
  keys: _KEYS,
  record: _record,
  parse: { baggage: _baggage, traceparent: _parent, tracestate: _state },
  print: { baggage: _printedBaggage, traceparent: _printedParent, tracestate: _printedState },
  promote: _promote,
  span: _span,
  tenant: _tenant,
  withoutTenant: _withoutTenant,
}
```

## [05]-[EVENT_ENVELOPE]

- Owner: `Event.admit` and `Event.mint` are the strict SDK boundary; `Event.rasm` owns the estate profile without narrowing generic CloudEvents.
- Owner: `Event.schema` is the semantic Schema boundary; consumers may add generation metadata but never another predicate or admission transform over a CloudEvents envelope.
- Owner: `Event.fromProto` and `Event.toProto` are the generated-message ↔ SDK semantic bridge; `Event.format.protobuf` owns publisher Protobuf single/batch octets and `Event.format.json` owns JSON, each with strict admission once for every binding.
- Entry: `_eventProtobuf` is the publisher-Protobuf semantic adapter published as `Event.format.protobuf`; `Event.fromProto` and `Event.toProto` preserve the generated publisher message semantics on either side of it.
- Owner: `_gradePolicy` is the Rasm profile's one closed classification policy over the standard open-string `dataclassification` extension; the generated contract owns the extension's spelling and validation, not the estate policy.
- Law: generic `mint` supplies `id` and `specversion` and preserves optional CloudEvents `time`; Rasm mint requires `Fact.time`. Neither path reaches `uuid.v4()` or the wall clock from a constructor no `Clock` or `Random` enters.
- Law: SDK construction remains strict on mint and admission; admission refuses absent `id` or `specversion` before the SDK can synthesize either, rejects non-v1 events the SDK reports as merely false, and keeps malformed required attributes or non-absolute `dataschema` values on `EventRefusal`.
- Law: raw `data` and `data_base64` are exclusive. Admission decodes the base64 arm into canonical `Uint8Array` before SDK construction and repairs binding-produced non-byte typed arrays from that authoritative arm.
- Law: `mint` writes the addressed attributes AFTER the injected carrier record, so a peer's `traceparent` can never shadow an addressed attribute.
- Law: the Rasm profile admits `rasm.<domain>.<subject>.<fact>` only when `<domain>` is a `Convention.domain` capability; event-type evolution remains independent of the payload-schema URI.
- Law: generic admission delegates the required `source` URI-reference to the strict SDK; `Event.rasm.source(type, capability)` derives the profile's absolute `rasm:<domain>/<capability>` identity from two admitted axes, and `Fact` requires only that source and type share their domain. The event-type subject never re-authors producer capability.
- Law: `id` is the producer's operation identity and never a digest, so `(source, id)` is the uniqueness composite every dedup and idempotency key reads. `Event.address` length-frames those UTF-8 arms and digests the frame into the bounded branch coordinate; transports and ingress consume that one mint rather than maintaining private concatenations.
- Law: `subject` publishes the content key as 32 LOWERCASE hex through `_EVENT_KEY`, the boundary mapping over `Digest.Key.content` whose upper-encoding interchange codec stays untouched. Generated `dataref` remains the standard open URI-reference on generic and Rasm events; core validates and preserves the attribute but owns no residence, authorization, dereference, or inline/reference equality policy. A binding that spends it composes the data plane's confined capability before application settlement.
- Law: `datacontenttype` and `dataschema` arrive as row data off the caller's serdes arrow; a literal at either field states a payload encoding the arrow already decided.
- Law: `ExtensionsSchema` is the only extension roster and validator; mint and read cross its generated ProtoJSON mapping through `Format.proto`, so no field schema or name table exists here.
- Law: descriptor field kinds project a validated extension message into SDK-native scalars, bytes, timestamps, and generated enum names; ProtoJSON remains the inverse admission mapping, never the in-memory extension type model.
- Law: `read` decodes the whole generated message on every call and returns every unrostered peer name as a `Fault.Drop` occurrence, dropping it rather than faulting the message.
- Law: Protobuf conversion reads every official attribute-value arm; egress accepts the SDK-native scalar kinds plus binary data, text data, or `Any`, and refuses arbitrary payloads.
- Law: the publisher descriptor derives the SDK core-property set used by conversion and drop census; no hand core roster can misclassify a later publisher field as an extension.
- Law: the generated codec is the lossless Protobuf wire surface; the SDK bridge keeps URI and URI-reference arm provenance in a private `WeakMap` keyed by the admitted SDK envelope, and `Event.clone` propagates only that publisher-owned oneof fact. Decode then encode therefore preserves every generic URI arm without a second envelope object, while newly minted SDK strings still derive their arm from the addressed core/profile fields.
- Growth: an extension changes `event.proto`; a Rasm handling grade changes the single policy projection here; an addressed attribute is one `Fact` field.
- Tests: strict mint/admit reject synthesized required identity, non-v1 events, malformed source, and relative `dataschema`; the Rasm profile admits distinct type-subject/source-capability pairs in one rostered domain, refuses unrostered or mismatched domains, dual body arms, malformed `dataref`, and an unrostered `dataclassification`; base64 admission lands exact `Uint8Array`; generated conversion covers all seven attribute arms, preserves generic URI-arm identity through repository-owned clones, binary/text/`Any` data, batch members, unsupported-data refusal, and structural-then-semantic refusal.
- Boundary: binding and media routing seat at runtime; this cluster owns strict SDK admission, generated conversion, the Rasm profile, and no transport.
- Packages: `cloudevents`; `@bufbuild/protobuf`; `effect`; generated CloudEvents and estate event modules; `./format.ts`; `../value/contentKey.ts`; `../value/fault.ts`.

```typescript signature
import { CloudEvent, type CloudEventV1, V1 } from "cloudevents"
import { isMessage, type MessageInitShape, type MessageShape, type MessageValidType } from "@bufbuild/protobuf"
import {
  CloudEvent_CloudEventAttributeValueSchema,
  CloudEventBatchSchema,
  CloudEventSchema,
} from "@rasm\/contracts/io/cloudevents/v1/cloudevents_pb"
import { ExtensionsSchema } from "@rasm\/contracts/rasm/contracts/event/event_pb"
import { AnySchema, TimestampSchema, timestampDate, timestampFromDate } from "@bufbuild/protobuf/wkt"
import { DateTime } from "effect"
import { Format } from "./format.ts"
import { Digest } from "../value/contentKey.ts"

// Type subject and producer capability are independent axes inside one Convention-owned capability domain; `<fact>`
// reads past tense and carries the announced semantics whole, independently of the payload schema `dataschema` names.
const _SEGMENT = "[a-z0-9]+(?:-[a-z0-9]+)*"
const _TYPE = new RegExp(`^rasm\\.(${_SEGMENT})\\.(${_SEGMENT})\\.(${_SEGMENT})$`)
const _SOURCE = new RegExp(`^rasm:(${_SEGMENT})/(${_SEGMENT})$`)

const _typeDomain = (type: string): string => {
  const matched = _TYPE.exec(type)
  return matched?.[1] ?? ""
}
const _eventDomains = HashSet.fromIterable<string>(Record.keys(Convention.domain))
const _EVENT_TYPE = Schema.String.pipe(
  Schema.pattern(_TYPE),
  Schema.filter((type) => HashSet.has(_eventDomains, _typeDomain(type)) || "<unrostered-event-domain>"),
)
const _EVENT_CAPABILITY = Schema.String.pipe(Schema.pattern(new RegExp(`^${_SEGMENT}$`)))
const _EVENT_SOURCE = Schema.String.pipe(
  Schema.pattern(_SOURCE),
  Schema.brand("EventSource"),
)
const _sourceDomain = (source: string): string => {
  const matched = _SOURCE.exec(source)
  return matched?.[1] ?? ""
}
const _source = (type: string, capability: string) =>
  Effect.flatMap(
    Effect.all({
      type: Schema.decode(_EVENT_TYPE)(type),
      capability: Schema.decode(_EVENT_CAPABILITY)(capability),
    }),
    (admitted) => Schema.decode(_EVENT_SOURCE)(`rasm:${_typeDomain(admitted.type)}/${admitted.capability}`),
  )

const _datarefName = Option.map(
  Array.findFirst(ExtensionsSchema.fields, (field) => field.name === "dataref"),
  (field) => field.localName,
)

const _addressUtf8 = new TextEncoder()
const _addressTag = _addressUtf8.encode("rasm:event-address:v1")
const _address = (envelope: CloudEventV1<unknown>): Effect.Effect<Digest.Key<"content">> => {
  const source = _addressUtf8.encode(envelope.source)
  const id = _addressUtf8.encode(envelope.id)
  const lengths = new Uint8Array(8)
  const view = new DataView(lengths.buffer)
  view.setUint32(0, source.byteLength)
  view.setUint32(4, id.byteLength)
  return Digest.mint("content", [_addressTag, lengths, source, id])
}

// BOUNDARY MAPPING: the event wire carries the content key in 32 LOWERCASE hex, because C# `ContentHash.Hex` renders
// `x32` and the python `WireKey` admits `[0-9a-f]{32}` — one spelling reaches a `subject` join and a dedup key, both
// compared as text. `Digest.codecs.content` ENCODES upper for the interchange frame, so mapping here
// keeps the shared codec's own spelling intact; re-casing that row respells every appearance address the corpus
// already froze. Decode lowercases exactly as the shared codec does, so the branded key stays one value.
const _EVENT_KEY = Schema.transform(Schema.String.pipe(Schema.pattern(/^[0-9a-f]{32}$/)), Digest.Key.content, {
  strict: true,
  decode: (wire) => wire,
  encode: (key) => key.toLowerCase(),
})

const _gradePolicy = {
  public: { redact: false, broker: true },
  internal: { redact: false, broker: true },
  restricted: { redact: true, broker: true },
  secret: { redact: true, broker: false },
} as const satisfies Record<string, {
  readonly redact: boolean
  readonly broker: boolean
}>

type _EventClass = keyof typeof _gradePolicy
const _classKinds = Record.keys(_gradePolicy)
const _isClass = (value: string): value is _EventClass =>
  Record.has(_gradePolicy, value)
const _Class = Schema.String.pipe(Schema.filter(_isClass, { message: () => "<unrostered-data-grade>" }))
const _classes = {
  kinds: _classKinds,
  schema: _Class,
  at: (name: _EventClass) => _gradePolicy[name],
} as const

type _EventExtension = Exclude<keyof MessageValidType<typeof ExtensionsSchema>, "$typeName">
const _isExtension = (name: string): name is _EventExtension =>
  Array.some(ExtensionsSchema.fields, (field) => field.localName === name)
const _extensionNames = Array.filter(Array.map(ExtensionsSchema.fields, (field) => field.localName), _isExtension)
const _extensionSet = HashSet.fromIterable<string>(_extensionNames)
const _extensions = { kinds: _extensionNames, is: _isExtension } as const
const _refusals = Fault.Class.family(["envelope"] as const, {
  envelope: Fault.Class.row({
    class: "invalid",
    leg: "admission",
    detail: Schema.Struct({ issue: Schema.NonEmptyString }),
    render: ({ issue }) => `event envelope refused — ${issue}`,
  }),
})

class EventRefusal extends Schema.TaggedError<EventRefusal>()("EventRefusal", {
  case: _refusals.payload,
}) {
  get class(): Fault.Class.Kind {
    return _refusals.classOf(this.case.reason)
  }
  override get message(): string {
    return _refusals.render(this.case)
  }
}

const _refused = (issue: unknown): EventRefusal =>
  new EventRefusal({ case: { reason: "envelope", issue: String(issue) } })

const _strict = (envelope: unknown): Effect.Effect<CloudEvent<unknown>, EventRefusal> => {
  if (
    !Predicate.isRecord(envelope)
    || !Object.hasOwn(envelope, "id")
    || !Predicate.isString(envelope.id)
    || envelope.id.length === 0
    || !Object.hasOwn(envelope, "specversion")
    || envelope.specversion !== V1
  ) return Effect.fail(_refused("<required-attributes-must-be-explicit>"))
  if (!(envelope instanceof CloudEvent) && Object.hasOwn(envelope, "data") && Object.hasOwn(envelope, "data_base64")) {
    return Effect.fail(_refused("<data-and-data_base64-are-exclusive>"))
  }
  const canonical = Predicate.isString(envelope.data_base64) && !(envelope.data instanceof Uint8Array)
    ? Effect.mapError(
      Effect.map(Effect.fromEither(Encoding.decodeBase64(envelope.data_base64)), (data) => ({
        ...envelope,
        data,
        data_base64: undefined,
      })),
      _refused,
    )
    : Effect.succeed(envelope)
  return Effect.flatMap(canonical, (offered) => Effect.try({
      try: () => {
        const admitted = new CloudEvent<unknown>(offered as Partial<CloudEventV1<unknown>>, true)
        return envelope instanceof CloudEvent && offered === envelope ? envelope : admitted
      },
      catch: _refused,
    }))
}

const _admit = (envelope: unknown): Effect.Effect<CloudEvent<unknown>, EventRefusal> => _strict(envelope)
const _mintEvent = <T>(attributes: CloudEventV1<T>): Effect.Effect<CloudEvent<unknown>, EventRefusal> => _strict(attributes)

class _Fact extends Schema.Class<_Fact>("Event.Fact")({
  // Brands refine IN PLACE, so no branded scalar exports beside the owner that admits it.
  id: Schema.NonEmptyString.pipe(Schema.brand("EventId")),
  source: _EVENT_SOURCE,
  type: _EVENT_TYPE.pipe(Schema.brand("EventType")),
  time: Schema.DateTimeUtc,
  subject: Schema.optionalWith(_EVENT_KEY, { as: "Option" }),
  dataschema: Schema.optionalWith(Schema.URL, { as: "Option" }),
  datacontenttype: Schema.optionalWith(Format.event.Media, { as: "Option" }),
  data: Schema.Unknown,
}) {}

const Fact = _Fact.pipe(
  Schema.filter((fact) =>
    _sourceDomain(fact.source) === _typeDomain(fact.type) || "<source-and-type-domain-must-agree>"),
)

declare namespace Event {
  type Class = _EventClass
  type ClassRow = ReturnType<typeof _classes.at>
  type Extension = _EventExtension
  type Value<Name extends Extension> = NonNullable<Roster[Name]>
  type Held = Omit<MessageInitShape<typeof ExtensionsSchema>, "dataclassification"> & {
    readonly dataclassification?: Class
  }
  type Roster = Omit<MessageValidType<typeof ExtensionsSchema>, "dataclassification"> & {
    readonly dataclassification?: Class
  }
  type Read = { readonly roster: Roster; readonly dropped: ReadonlyArray<Fault.Drop.Fact> }
  type Refusal = EventRefusal
  type Json = {
    readonly media: typeof Format.event.json.media
    readonly single: Schema.Schema<CloudEvent<unknown>, typeof Format.event.json.single.Encoded>
    readonly batch: Option.Option<{
      readonly media: string
      readonly codec: Schema.Schema<ReadonlyArray<CloudEvent<unknown>>, Uint8Array>
    }>
  }
  type Protobuf = {
    readonly media: typeof Format.event.protobuf.media
    readonly single: Schema.Schema<CloudEvent<unknown>, Uint8Array>
    readonly batch: Option.Option<{
      readonly media: string
      readonly codec: Schema.Schema<ReadonlyArray<CloudEvent<unknown>>, Uint8Array>
    }>
  }
  type Format = { readonly json: Json; readonly protobuf: Protobuf }
  type Rasm = {
    readonly Fact: typeof Fact
    readonly classes: typeof _classes
    readonly extensions: typeof _extensions
    readonly source: typeof _source
    readonly subject: typeof _EVENT_KEY
    readonly mint: (fact: Schema.Schema.Type<typeof Fact>, held: Held, context: Carrier.Context) => Effect.Effect<CloudEvent<unknown>, Refusal>
    readonly extend: (envelope: CloudEvent<unknown>, held: Held) => Effect.Effect<CloudEvent<unknown>, Refusal>
    readonly at: <Name extends Extension>(
      envelope: CloudEvent<unknown>,
      name: Name,
    ) => Effect.Effect<Option.Option<Value<Name>>, Refusal>
    readonly read: (envelope: CloudEvent<unknown>) => Effect.Effect<Read, Refusal>
  }
  type Shape = {
    readonly Refusal: typeof EventRefusal
    readonly address: typeof _address
    readonly admit: typeof _admit
    readonly clone: typeof _clone
    readonly fromProto: (
      wire: MessageValidType<typeof CloudEventSchema>,
    ) => Effect.Effect<CloudEvent<unknown>, EventRefusal>
    readonly mint: typeof _mintEvent
    readonly schema: Schema.Schema<CloudEvent<unknown>, unknown>
    readonly format: Format
    readonly rasm: Rasm
    readonly toProto: (
      offered: CloudEvent<unknown>,
    ) => Effect.Effect<MessageValidType<typeof CloudEventSchema>, EventRefusal>
  }
}

const _extensionMessage = (held: Event.Held) => Format.proto.create(ExtensionsSchema, held)

const _held = (held: Event.Held): Effect.Effect<Record.ReadonlyRecord<string, unknown>, Event.Refusal> => {
  if (Option.isNone(_datarefName) && Object.hasOwn(held, "dataref")) {
    return Effect.fail(_refused("<dataref-descriptor-absent>"))
  }
  return Schema.decode(Format.proto.message(ExtensionsSchema))(_extensionMessage(held)).pipe(
    Effect.flatMap((roster) => Effect.try({
      try: () => Record.fromEntries(Array.filterMap(ExtensionsSchema.fields, (field) => {
        if (!_isExtension(field.localName)) return Option.none()
        const value = roster[field.localName]
        if (value === undefined) return Option.none()
        return Option.some([
          field.localName,
          isMessage(value, TimestampSchema) ? timestampDate(value) : value,
        ] as const)
      })),
      catch: _refused,
    })),
    Effect.mapError(_refused),
  )
}

type _UriArm = "ceUri" | "ceUriRef"
type _UriArms = Record.ReadonlyRecord<string, _UriArm>
const _uriArms = new WeakMap<CloudEvent<unknown>, _UriArms>()

const _rememberUriArms = (envelope: CloudEvent<unknown>, arms: _UriArms): CloudEvent<unknown> => {
  if (Object.keys(arms).length > 0) _uriArms.set(envelope, arms)
  return envelope
}

const _clone = (
  envelope: CloudEvent<unknown>,
  changed: Record.ReadonlyRecord<string, unknown>,
  removed: ReadonlyArray<string> = [],
): Effect.Effect<CloudEvent<unknown>, Event.Refusal> =>
  Effect.flatMap(_admit(envelope), (admitted) =>
    Effect.flatMap(
      Effect.try({ try: () => admitted.cloneWith(changed, true), catch: _refused }),
      (cloned) => Effect.map(_admit(removed.length === 0
        ? cloned
        : Record.fromEntries(Array.filter(Object.entries(cloned), ([key]) => !Array.contains(removed, key)))), (strict) => {
        const inherited = _uriArms.get(admitted)
        return inherited === undefined
          ? strict
          : _rememberUriArms(strict, Record.fromEntries(Array.filter(
            Object.entries(inherited),
            ([key]) => !Object.hasOwn(changed, key) && !Array.contains(removed, key),
          )))
      }),
    ))

const _extend = (envelope: CloudEvent<unknown>, held: Event.Held): Effect.Effect<CloudEvent<unknown>, EventRefusal> =>
  Effect.flatMap(_admit(envelope), (admitted) =>
    Effect.flatMap(_held(held), (extensions) =>
      _clone(admitted, extensions)))

const _mint = (
  fact: Schema.Schema.Type<typeof Fact>,
  held: Event.Held,
  context: Carrier.Context,
): Effect.Effect<CloudEvent<unknown>, Event.Refusal> =>
  Effect.flatMap(_held(held), (extensions) =>
    _mintEvent({
      ...Carrier.inject("cloudevents", context, extensions),
      ...Record.getSomes({
        datacontenttype: fact.datacontenttype,
        dataschema: Option.map(fact.dataschema, (uri) => uri.href),
        subject: fact.subject,
      }),
      data: fact.data,
      id: fact.id,
      source: fact.source,
      specversion: V1,
      time: DateTime.formatIso(fact.time),
      type: fact.type,
    }))

type _Attribute = MessageValidType<typeof CloudEvent_CloudEventAttributeValueSchema>
type _DecodedAttribute = { readonly value: unknown; readonly uriArm: Option.Option<_UriArm> }

const _attributeDecoded = (attribute: _Attribute): Effect.Effect<_DecodedAttribute, EventRefusal> => {
  switch (attribute.attr.case) {
    case "ceBoolean": return Effect.succeed({ value: attribute.attr.value, uriArm: Option.none() })
    case "ceInteger": return Effect.succeed({ value: attribute.attr.value, uriArm: Option.none() })
    case "ceString": return Effect.succeed({ value: attribute.attr.value, uriArm: Option.none() })
    case "ceBytes": return Effect.succeed({ value: attribute.attr.value, uriArm: Option.none() })
    case "ceUri": return Effect.succeed({ value: attribute.attr.value, uriArm: Option.some("ceUri") })
    case "ceUriRef": return Effect.succeed({ value: attribute.attr.value, uriArm: Option.some("ceUriRef") })
    case "ceTimestamp": return Effect.try({
      try: () => ({ value: timestampDate(attribute.attr.value), uriArm: Option.none() }),
      catch: _refused,
    })
    case undefined: return Effect.fail(_refused("<attribute-value-absent>"))
  }
}

const _attributeEncoded = (key: string, value: unknown, uriArm: _UriArm | undefined) => {
  const attr =
    uriArm !== undefined && Predicate.isString(value) ? { case: uriArm, value }
        : Predicate.isBoolean(value) ? { case: "ceBoolean" as const, value }
        : Predicate.isNumber(value) && Number.isSafeInteger(value) && value >= -2_147_483_648 && value <= 2_147_483_647
          ? { case: "ceInteger" as const, value }
          : value instanceof Date
            ? { case: "ceTimestamp" as const, value: timestampFromDate(value) }
            : value instanceof Uint8Array
              ? { case: "ceBytes" as const, value }
              : value instanceof URL
                ? { case: "ceUri" as const, value: value.href }
                : Predicate.isString(value) && key === "time"
                  ? { case: "ceTimestamp" as const, value: timestampFromDate(new Date(value)) }
                : Predicate.isString(value) && key === "dataschema"
                    ? { case: "ceUri" as const, value }
                    : Predicate.isString(value) && Option.contains(_datarefName, key)
                      ? { case: "ceUriRef" as const, value }
                    : Predicate.isString(value)
                      ? { case: "ceString" as const, value }
                      : undefined
  return attr === undefined
    ? Effect.fail(_refused(`<unsupported-attribute:${key}>`))
    : Effect.succeed(Format.proto.create(CloudEvent_CloudEventAttributeValueSchema, { attr }))
}

// Publisher descriptors own the core field roster. Map fields are the generated attribute container rather than an
// SDK property; oneof members collapse onto their generated oneof name and its SDK JSON base64 companion.
const _sdkCore = HashSet.fromIterable(Array.flatMap(CloudEventSchema.fields, (field) =>
  field.fieldKind === "map"
    ? Array.empty<string>()
    : field.oneof === undefined
      ? [field.name.replaceAll("_", "")]
      : [field.oneof.localName, `${field.oneof.localName}_base64`],
))

const _attributesDecoded = (
  attributes: MessageValidType<typeof CloudEventSchema>["attributes"],
): Effect.Effect<{
  readonly values: Record.ReadonlyRecord<string, unknown>
  readonly uriArms: _UriArms
}, EventRefusal> =>
  Effect.map(
    Effect.forEach(
      Object.entries(attributes),
      ([key, attribute]) => Effect.map(_attributeDecoded(attribute), ({ value, uriArm }) => ({
        key,
        uriArm,
        value: key === "time" && value instanceof Date ? value.toISOString() : value,
      })),
    ),
    (decoded) => ({
      values: Record.fromEntries(Array.map(decoded, ({ key, value }) => [key, value] as const)),
      uriArms: Record.fromEntries(Array.filterMap(decoded, ({ key, uriArm }) =>
        Option.map(uriArm, (arm) => [key, arm] as const))),
    }),
  )

const _attributesEncoded = (
  envelope: CloudEvent<unknown>,
): Effect.Effect<Record.ReadonlyRecord<string, MessageShape<typeof CloudEvent_CloudEventAttributeValueSchema>>, EventRefusal> =>
  pipe(_uriArms.get(envelope), (remembered) => Effect.map(
    Effect.forEach(
      Array.filterMap(
        Object.entries(envelope),
        ([key, value]) => HashSet.has(_sdkCore, key) || value === undefined
          ? Option.none()
          : Option.some([key, value] as const),
      ),
      ([key, value]) => Effect.map(
        _attributeEncoded(key, value, remembered?.[key]),
        (attribute) => [key, attribute] as const,
      ),
    ),
    Record.fromEntries,
  ))

const _dataDecoded = (
  data: MessageValidType<typeof CloudEventSchema>["data"],
): Effect.Effect<unknown, EventRefusal> => {
  switch (data.case) {
    case "binaryData": return Effect.succeed(data.value)
    case "textData": return Effect.succeed(data.value)
    case "protoData": return Effect.succeed(data.value)
    case undefined: return Effect.succeed(undefined)
  }
}

const _dataEncoded = (
  envelope: CloudEvent<unknown>,
): Effect.Effect<MessageInitShape<typeof CloudEventSchema>["data"], EventRefusal> => {
  if (Predicate.isString(envelope.data_base64)) {
    return Effect.mapError(
      Effect.map(Effect.fromEither(Encoding.decodeBase64(envelope.data_base64)),
        (value) => ({ case: "binaryData" as const, value })),
      _refused,
    )
  }
  if (envelope.data === undefined) return Effect.succeed({ case: undefined })
  if (envelope.data instanceof Uint8Array) {
    return Effect.succeed({ case: "binaryData", value: envelope.data })
  }
  if (Predicate.isString(envelope.data)) return Effect.succeed({ case: "textData", value: envelope.data })
  if (isMessage(envelope.data, AnySchema)) return Effect.succeed({ case: "protoData", value: envelope.data })
  return Effect.fail(_refused("<protobuf-data-requires-bytes-text-or-any>"))
}

const _fromProtobuf = (
  wire: MessageValidType<typeof CloudEventSchema>,
): Effect.Effect<CloudEvent<unknown>, EventRefusal> =>
  Effect.flatMap(
    Effect.all({ attributes: _attributesDecoded(wire.attributes), data: _dataDecoded(wire.data) }),
    ({ attributes, data }) => Effect.map(_admit({
      ...attributes.values,
      data,
      id: wire.id,
      source: wire.source,
      specversion: wire.specVersion,
      type: wire.type,
    }), (envelope) => _rememberUriArms(envelope, attributes.uriArms)),
  )

const _toProtobuf = (
  offered: CloudEvent<unknown>,
): Effect.Effect<MessageValidType<typeof CloudEventSchema>, EventRefusal> =>
  Effect.flatMap(_admit(offered), (envelope) =>
    Effect.flatMap(
      Effect.all({ attributes: _attributesEncoded(envelope), data: _dataEncoded(envelope) }),
      ({ attributes, data }) => Schema.decode(Format.proto.message(CloudEventSchema))(
        Format.proto.create(CloudEventSchema, {
          attributes,
          data,
          id: envelope.id,
          source: envelope.source,
          specVersion: envelope.specversion,
          type: envelope.type,
        }),
      ).pipe(Effect.mapError(_refused)),
    ))

const _EventDomain = Schema.declare(
  (input: unknown): input is CloudEvent<unknown> => input instanceof CloudEvent,
  { identifier: "CloudEventSdkObject" },
)

const _eventSchema = Schema.transformOrFail(Schema.Unknown, _EventDomain, {
  strict: true,
  decode: (offered, _options, ast) =>
    Effect.mapError(_admit(offered), (refusal) => new ParseResult.Type(ast, offered, refusal.message)),
  encode: (envelope, _options, ast) =>
    Effect.mapError(_admit(envelope), (refusal) => new ParseResult.Type(ast, envelope, refusal.message)),
})

const _eventJsonSingle = Format.event.json.single.pipe(Schema.compose(_eventSchema, { strict: false }))
const _eventJsonBatch = Option.map(Format.event.json.batch, ({ media, codec }) => ({
  media,
  codec: codec.pipe(Schema.compose(Schema.Array(_eventSchema), { strict: false })),
}))

const _eventJson: Event.Json = {
  media: Format.event.json.media,
  single: _eventJsonSingle,
  batch: _eventJsonBatch,
}

const _protobufSingle = Schema.transformOrFail(Format.proto.message(CloudEventSchema), _EventDomain, {
  strict: true,
  decode: (wire, _options, ast) =>
    Effect.mapError(_fromProtobuf(wire), (refusal) => new ParseResult.Type(ast, wire, refusal.message)),
  encode: (envelope, _options, ast) =>
    Effect.mapError(_toProtobuf(envelope), (refusal) => new ParseResult.Type(ast, envelope, refusal.message)),
})

const _protobufBatch = Schema.transformOrFail(
  Format.proto.message(CloudEventBatchSchema),
  Schema.Array(_EventDomain),
  {
    strict: true,
    decode: (wire, _options, ast) =>
      Effect.mapError(
        Effect.forEach(wire.events, _fromProtobuf),
        (refusal) => new ParseResult.Type(ast, wire, refusal.message),
      ),
    encode: (events, _options, ast) =>
      Effect.mapError(
        Effect.flatMap(Effect.forEach(events, _toProtobuf), (held) =>
          Schema.decode(Format.proto.message(CloudEventBatchSchema))(
            Format.proto.create(CloudEventBatchSchema, { events: held }),
          )),
        (issue) => new ParseResult.Type(ast, events, String(issue)),
      ),
  },
)

const _eventProtobuf: Event.Protobuf = {
  media: Format.event.protobuf.media,
  single: Format.event.protobuf.single.pipe(Schema.compose(_protobufSingle, { strict: false })),
  batch: Option.map(Format.event.protobuf.batch, ({ media, codec }) => ({
    media,
    codec: codec.pipe(Schema.compose(_protobufBatch, { strict: false })),
  })),
}

const _eventFormat: Event.Format = { json: _eventJson, protobuf: _eventProtobuf }

// Construction writes `specversion`, `data`, and `data_base64` beside the fields `Fact` addresses, so the drop
// census subtracts both owned sets and reports only names neither this roster nor `Fact` itself holds.
const _addressed = HashSet.union(_sdkCore, HashSet.fromIterable(Record.keys(_Fact.fields)))

const _extensionJson = (envelope: CloudEventV1<unknown>): Record.ReadonlyRecord<string, unknown> =>
  Record.fromEntries(Array.filterMap(
    _extensionNames,
    (name) => Option.map(Option.fromNullable(envelope[name]), (value) => [
      name,
      value instanceof Date
        ? value.toISOString()
        : value instanceof Uint8Array
          ? Encoding.encodeBase64(value)
          : value,
    ] as const),
  ))

const _profileRoster = (
  roster: MessageValidType<typeof ExtensionsSchema>,
): Effect.Effect<Event.Roster, Event.Refusal> => {
  const { dataclassification, ...held } = roster
  return Option.match(Option.fromNullable(dataclassification), {
    onNone: () => Effect.succeed(held),
    onSome: (offered) => Schema.decode(_Class)(offered).pipe(
      Effect.map((admitted) => ({ ...held, dataclassification: admitted })),
      Effect.mapError(_refused),
    ),
  })
}

const _read = (envelope: CloudEventV1<unknown>): Effect.Effect<Event.Read, Event.Refusal> =>
  Schema.decode(Format.proto.json(ExtensionsSchema))(_extensionJson(envelope)).pipe(
    Effect.flatMap(_profileRoster),
    Effect.map((roster) => ({
      roster,
      dropped: Array.filterMap(
        Object.entries(envelope),
        ([key, value]) => value === undefined || HashSet.has(_extensionSet, key) || HashSet.has(_addressed, key)
          ? Option.none()
          : Option.some(Fault.Drop.fact("foreign", key, 1)),
      ),
    })),
    Effect.mapError(_refused),
  )

const _at = <Name extends Event.Extension>(
  envelope: CloudEventV1<unknown>,
  name: Name,
): Effect.Effect<Option.Option<Event.Value<Name>>, Event.Refusal> =>
  Effect.map(_read(envelope), ({ roster }) => Option.fromNullable(roster[name]))

const Event: Event.Shape = {
  Refusal: EventRefusal,
  address: _address,
  admit: _admit,
  clone: _clone,
  format: _eventFormat,
  fromProto: _fromProtobuf,
  mint: _mintEvent,
  schema: _eventSchema,
  rasm: {
    Fact,
    classes: _classes,
    extensions: _extensions,
    source: _source,
    subject: _EVENT_KEY,
    mint: _mint,
    extend: _extend,
    at: _at,
    read: _read,
  },
  toProto: _toProtobuf,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Carrier, Event }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
