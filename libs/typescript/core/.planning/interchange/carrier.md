# [CORE_CARRIER]

W3C propagation crosses the interchange plane as ONE typed `traceparent`/`tracestate`/`baggage` value, total parse/print folds, `rasm.tenant` promotion, a closed transport table, and the Connect `-bin` typed-metadata lane. HTTP, Connect, NATS, MQTT v5, CloudEvents, and Kafka inject and extract through one codec. Malformed input folds to absence under the restart posture, while ordered, bounded folds keep output byte-stable. Module `core/src/interchange/carrier.ts` admits a transport as one dialect row, a baggage axis as one member key, and a typed family as one name row.

One module seats both owners, since a message envelope's extension slot IS a carrier frame. `Carrier` composes only the `value` floor's `Identity.Tenant` and hands dialect frames to the runtime wave as data. `Event` composes `Digest.Key`, `observe`'s severity vocabulary, and the `cloudevents` message-envelope class, then publishes the grammar, the roster, and the ONE mint entry every producer reaches. Kafka, NATS, MQTT, and CloudEvents realize their rows, while `interchange/invoke` composes Connect. Frame values recover the dialect discriminant, so one mapped handler record owns dispatch.

## [01]-[INDEX]

- [02]-[CONTEXT_VALUE]: typed triple, brands, total parse/print folds, span lift; `Carrier`.
- [03]-[TENANT_BAGGAGE]: `rasm.tenant` promotion and scoped recovery decode; `Carrier`.
- [04]-[DIALECT_TABLE]: closed frame rows, inject/extract dispatch, `-bin` typed-metadata lane; `Carrier`.
- [05]-[EVENT_ENVELOPE]: attribute grammar, closed extension roster, and the mint and read pair; `Event`.

## [02]-[CONTEXT_VALUE]

- Owner: `Traceparent`, `Carrier.State`, `Carrier.Member`, and `Carrier.Context` own parent, tracestate, baggage, and optionality.
- Law: Malformed parents restart; invalid state or baggage members drop independently, and every drop RETURNS as a `Fault.Drop` occurrence.
- Law: Baggage properties prove delimiter-safe W3C grammar before entering context.
- Law: Baggage admits 64 members, 4096 encoded bytes per member, and 8192 encoded bytes total.
- Law: Version `ff`, invalid version-zero flags, extensions on version zero, and all-zero identities refuse.
- Law: Parent print emits the supported version-zero spelling and sampled flag.
- Law: `_stateRows` enforces grammar, first-key-wins uniqueness, member count, and aggregate text bounds in one fold, each arm naming its own drop reason.
- Law: Baggage print uses Effect `Encoding` before member and aggregate encoded-byte admission.
- Law: `Carrier.span` lifts structural span fields; `Carrier.Current` scopes ingress; `Carrier.current` overlays the live parent and preserves lists.
- Growth: a new context list (a fourth W3C header) is one field on the triple with its parse/print row; a new parse bound is one `_CEILING` field.
- Boundary: Tracer owns the live span; data owns persisted contexts; `Carrier` owns pure context values and folds.
- Packages: `effect` (`Array`, `Effect`, `Either`, `Encoding`, `Option`, `Schema`, `String`, `pipe`); `../value/fault.ts` (`Fault.Drop`, `Fault.Ledger`); `../value/identity.ts` (`Identity.Tenant`).

```typescript signature
import { decodeBinaryHeader, encodeBinaryHeader } from "@connectrpc/connect"
import type { Wire } from "./codec.ts" // type-only: the census family union, carrying no runtime edge to the codec owner
import { Headers, HttpTraceContext } from "@effect/platform"
import { Array, Context, Effect, Either, Encoding, Option, Predicate, Record, Schema, String, pipe } from "effect"
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
    Option.filter(([, version, , , flags, extension]) =>
      version !== "ff" && (version !== "00" || (extension === undefined && (flags === "00" || flags === "01")))),
    Option.flatMap(([, , traceId, spanId, flags]) =>
      _decodedParent({ traceId, spanId, sampled: (Number.parseInt(flags ?? "00", 16) & 1) === 1 })),
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
  `00-${parent.traceId}-${parent.spanId}-${parent.sampled ? "01" : "00"}`

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
  parent: _decodedParent({ traceId: span.traceId, spanId: span.spanId, sampled: span.sampled }),
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
  record: _record,
  parse: { baggage: _baggage, traceparent: _parent, tracestate: _state },
  print: { baggage: _printedBaggage, traceparent: _printedParent, tracestate: _printedState },
  promote: _promote,
  span: _span,
  tenant: _tenant,
}
```

## [05]-[EVENT_ENVELOPE]

- Owner: `Event` owns the attribute grammar, the closed extension roster, the branch's ONE mint entry, and the typed read.
- Owner: `_classes` closes the handling grades `dataclassification` names, carrying the redaction and broker-crossing gate each binding row reads.
- Law: `mint` supplies `id`, `time`, and `specversion` at every call, so the package never reaches `uuid.v4()` or the wall clock from a constructor no `Clock` or `Random` service enters.
- Law: `mint` catches `TypeError`, since `ValidationError` extends it and the package's cross-version guard throws the base class alone.
- Law: `mint` writes the addressed attributes AFTER the injected carrier record, so a peer's `traceparent` can never shadow an addressed attribute.
- Law: `type` reads `rasm.<domain>.<subject>.<fact>.v<N>` and its `<domain>` segment CLOSES against `Convention.domain`, so an announced fact and a board join one vocabulary by proof rather than by prose; `v<N>` moves only with a breaking `dataschema` generation, and `deprecation` names a superseding `type` through the same refinement.
- Law: `id` is the producer's operation identity and never a digest, so `(source, id)` is the uniqueness composite every dedup and idempotency key reads.
- Law: `subject` and `dataref` publish the content key as 32 LOWERCASE hex through `_EVENT_KEY`, the boundary mapping over `Digest.Key.content` whose upper-encoding interchange codec stays untouched, so an externalized payload's reference IS the digest its residence resolves and one spelling crosses every peer.
- Law: `datacontenttype` and `dataschema` arrive as row data off the caller's serdes arrow; a literal at either field states a payload encoding the arrow already decided.
- Law: the roster IS the name ceiling — every row conforms to `[a-z0-9]` within 20 characters by declaration, since the package proves the alphabet alone and only names the ceiling inside the message it throws.
- Law: `read` decodes the whole roster on every call and returns every unrostered peer name as a `Fault.Drop` occurrence, dropping it rather than faulting the message.
- Law: `severity` admits the one branch severity vocabulary, so an announced fact's grade routes through the same rows an objective's burn does.
- Law: `signed` marks every row the DSSE digest folds; `dssematerial` is the one exclusion, because a signature cannot cover the attribute carrying it.
- Law: roster declaration order IS the published canonical digest order, alphabetical so three branches transcribe one sequence rather than each sorting its own map at signing time.
- Law: binding and content-mode selection stays typed data at the consuming seat, since `emitterFor` reads both off an unchecked options bag where a misspelled key silently takes its HTTP-binary default.
- Growth: an extension is one `_extensionRows` row; a handling grade is one `_classRows` row; an addressed attribute is one `Fact` field with its projection.
- Boundary: bindings, content modes, batch framing, filters, and subscriptions seat at their consuming packages; this cluster owns the message envelope, the roster, and the grammar alone.
- Packages: `cloudevents` (`CloudEvent`, `CloudEventV1`, `V1`); `effect`; `../observe/convention.ts` (`Convention`); `../observe/slo.ts` (`Reliability`); `../value/contentKey.ts` (`Digest`); `../value/fault.ts` (`Fault`); `../value/schema.ts` (`Shape`).

```typescript signature
import { CloudEvent, type CloudEventV1, V1 } from "cloudevents"
import { DateTime, type ParseResult } from "effect"
import { Reliability } from "../observe/slo.ts"
import { Digest } from "../value/contentKey.ts"
import { Shape } from "../value/schema.ts"

// `<domain>` is the capability subject `Convention` fixes for metric names, so a board and a subscription read one
// vocabulary; `<fact>` reads past tense and `v<N>` moves only with a breaking `dataschema` generation.
const _TYPE = /^rasm\.([a-z0-9]+)\.[a-z0-9]+\.[a-z0-9]+\.v[1-9][0-9]*$/
const _SEQUENCE = /^-?(0|[1-9][0-9]*)$/

// Grammar alone proves the SHAPE; closing the domain segment against the roster is what makes the join a fact rather
// than a claim, so a type naming a segment no capability mints refuses at the mint instead of reaching a subscription
// that keys on it and a board that can never answer it.
const _EVENT_TYPE = Schema.String.pipe(
  Schema.pattern(_TYPE),
  Schema.filter((type: string) => Record.has(Convention.domain, _TYPE.exec(type)?.[1] ?? ""), {
    message: () => "<type-domain-unrostered>",
  }),
)

// BOUNDARY MAPPING: the event wire carries the content key in 32 LOWERCASE hex, because the C# `EventKey` renders
// `x32` and the python `WireKey` admits `[0-9a-f]{32}` — one spelling reaches a `subject` join, a `dataref` tail, and
// a dedup key, all compared as text. `Digest.codecs.content` ENCODES upper for the interchange frame, so mapping here
// keeps the shared codec's own spelling intact; re-casing that row respells every appearance address the corpus
// already froze. Decode lowercases exactly as the shared codec does, so the branded key stays one value.
const _EVENT_KEY = Schema.transform(Schema.String.pipe(Schema.pattern(/^[0-9a-f]{32}$/)), Digest.Key.content, {
  strict: true,
  decode: (wire) => wire,
  encode: (key) => key.toLowerCase(),
})

const _classKinds = ["public", "internal", "restricted", "secret"] as const
const _classRows = {
  public: { redact: false, broker: true },
  internal: { redact: false, broker: true },
  restricted: { redact: true, broker: true },
  // Secret payloads cross no broker at all: a binding refuses this grade at admission and ships the `dataref`
  // alone, so the classification gate is a row a binding reads rather than a check it re-implements.
  secret: { redact: true, broker: false },
} as const
const _classes = Shape.vocabulary(_classKinds, _classRows)

const _extensionNames = [
  "authcontext", "baggage", "correlation", "dataclassification", "dataref", "deprecation", "dssematerial",
  "expirytime", "partitionkey", "recordedtime", "sampledrate", "sequence", "sequencetype", "severity",
  "traceparent", "tracestate",
] as const

// Declaration order IS the published canonical order the DSSE digest folds, spelled alphabetically so the C# and
// Python rosters transcribe one sequence instead of each sorting an unordered map at its own signing seam.
const _extensionRows = {
  authcontext: { admit: Schema.NonEmptyString, signed: true },
  baggage: { admit: Schema.String, signed: true },
  correlation: { admit: Schema.NonEmptyString, signed: true },
  dataclassification: { admit: _classes.schema, signed: true },
  dataref: { admit: _EVENT_KEY, signed: true },
  deprecation: { admit: _EVENT_TYPE, signed: true },
  // No signature covers the attribute carrying it, so this row is the roster's ONE digest exclusion.
  dssematerial: { admit: Schema.Uint8ArrayFromBase64, signed: false },
  expirytime: { admit: Schema.DateTimeUtc, signed: true },
  partitionkey: { admit: Schema.NonEmptyString, signed: true },
  recordedtime: { admit: Schema.DateTimeUtc, signed: true },
  sampledrate: { admit: Schema.Int.pipe(Schema.greaterThanOrEqualTo(1)), signed: true },
  sequence: { admit: Schema.String.pipe(Schema.pattern(_SEQUENCE)), signed: true },
  // Exactly one sequence domain is registered, so the value is a literal rather than an open string.
  sequencetype: { admit: Schema.Literal("Integer"), signed: true },
  severity: { admit: Reliability.Alert.Severity.schema, signed: true },
  // W3C context arrives and leaves through the dialect row's own folds; these rows admit the printed text and
  // leave the parse to `Carrier.parse`, so no second grammar exists beside it.
  traceparent: { admit: Schema.String, signed: true },
  tracestate: { admit: Schema.String, signed: true },
} as const
const _extensions = Shape.vocabulary(_extensionNames, _extensionRows)

// Each reason renders its OWN subject, so the attribute column stops riding an `Option` that only the envelope arm
// ever leaves empty: an attribute refusal NAMES its attribute by declaration and an envelope refusal has no attribute
// to name. `attribute` grades `invalid` because the value refused the schema its own roster row declares, while
// `extension` grades `malformed` because the NAME reached a roster that holds no row for it.
const _refusals = Fault.Class.family(["attribute", "envelope", "extension"] as const, {
  attribute: Fault.Class.row({
    class: "invalid",
    leg: "admission",
    detail: Schema.Struct({ attribute: Schema.NonEmptyString, issue: Schema.NonEmptyString }),
    render: ({ attribute, issue }) => `${attribute} refused the schema its roster row declares — ${issue}`,
  }),
  envelope: Fault.Class.row({
    class: "invalid",
    leg: "mint",
    detail: Schema.Struct({ issue: Schema.NonEmptyString }),
    render: ({ issue }) => `envelope construction refused — ${issue}`,
  }),
  extension: Fault.Class.row({
    class: "malformed",
    leg: "admission",
    detail: Schema.Struct({ attribute: Schema.NonEmptyString, issue: Schema.NonEmptyString }),
    render: ({ attribute, issue }) => `${attribute} names no rostered extension — ${issue}`,
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

class Fact extends Schema.Class<Fact>("Event.Fact")({
  // Brands refine IN PLACE, so no branded scalar exports beside the owner that admits it.
  id: Schema.NonEmptyString.pipe(Schema.brand("EventId")),
  source: Schema.NonEmptyString.pipe(Schema.brand("EventSource")),
  type: _EVENT_TYPE.pipe(Schema.brand("EventType")),
  time: Schema.DateTimeUtc,
  subject: Schema.optionalWith(_EVENT_KEY, { as: "Option" }),
  dataschema: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  datacontenttype: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  data: Schema.Unknown,
}) {}

declare namespace Event {
  type Class = (typeof _classKinds)[number]
  type ClassRow = (typeof _classRows)[Class]
  type Extension = (typeof _extensionNames)[number]
  type Value<Name extends Extension> = Schema.Schema.Type<(typeof _extensionRows)[Name]["admit"]>
  type Held = { readonly [Name in Extension]?: Value<Name> }
  type Roster = { readonly [Name in Extension]: Option.Option<Value<Name>> }
  type Read = { readonly roster: Roster; readonly dropped: ReadonlyArray<Fault.Drop.Fact> }
  type Refusal = EventRefusal
  type Shape = {
    readonly Fact: typeof Fact
    readonly Refusal: typeof EventRefusal
    readonly classes: typeof _classes
    readonly extensions: typeof _extensions
    readonly digested: ReadonlyArray<Extension> // the signed rows in published order, the DSSE fold's own input
    readonly mint: (fact: Fact, held: Held, context: Carrier.Context) => Effect.Effect<CloudEvent<unknown>, Refusal>
    readonly at: <Name extends Extension>(envelope: CloudEventV1<unknown>, name: Name) => Option.Option<Value<Name>>
    readonly read: (envelope: CloudEventV1<unknown>) => Read
  }
}

// Each row's codec pair derives at the ONE table where its schema is still concrete, so no call site re-reads a
// union of schemas and no arm spells its own encode.
type _Printers = { readonly [Name in Event.Extension]: (value: unknown) => Effect.Effect<unknown, ParseResult.ParseError> }
type _Readers = { readonly [Name in Event.Extension]: (value: unknown) => Option.Option<Event.Value<Name>> }
const _printers = Record.map(_extensionRows, (row) => Schema.encodeUnknown(row.admit)) as unknown as _Printers
const _readers = Record.map(_extensionRows, (row) => Schema.decodeUnknownOption(row.admit)) as unknown as _Readers

const _held = (held: Event.Held): Effect.Effect<Record.ReadonlyRecord<string, unknown>, Event.Refusal> =>
  Effect.map(
    Effect.forEach(
      Array.filterMap(_extensionNames, (name) =>
        Option.map(Option.fromNullable(held[name]), (value) => [name, value] as const)),
      ([name, value]) =>
        Effect.mapBoth(_printers[name](value), {
          onFailure: (issue) =>
            new EventRefusal({ case: { reason: "attribute", attribute: name, issue: issue.message } }),
          onSuccess: (encoded) => [name, encoded] as const,
        }),
    ),
    Record.fromEntries,
  )

const _mint = (
  fact: Fact,
  held: Event.Held,
  context: Carrier.Context,
): Effect.Effect<CloudEvent<unknown>, Event.Refusal> =>
  Effect.flatMap(_held(held), (extensions) =>
    Effect.try({
      // Every addressed attribute is supplied: an omitted `id` mints a v4 UUID and an omitted `time` reads the
      // wall clock, both inside a constructor no service can reach, so absence here is an unowned identity.
      // Addressed fields land LAST so an injected carrier key cannot shadow one.
      try: () =>
        new CloudEvent<unknown>({
          ...Carrier.inject("cloudevents", context, extensions),
          ...Record.getSomes({
            datacontenttype: fact.datacontenttype,
            dataschema: fact.dataschema,
            subject: fact.subject,
          }),
          data: fact.data,
          id: fact.id,
          source: fact.source,
          specversion: V1,
          time: DateTime.formatIso(fact.time),
          type: fact.type,
        }),
      // `ValidationError` extends `TypeError` and the specversion guard throws the base class, so the wider
      // narrowing is the only one keeping both arms on the rail.
      catch: (caught) =>
        new EventRefusal({
          case: { reason: "envelope", issue: caught instanceof Error ? caught.message : String(caught) },
        }),
    }))

const _at = <Name extends Event.Extension>(
  envelope: CloudEventV1<unknown>,
  name: Name,
): Option.Option<Event.Value<Name>> => _readers[name](envelope[name])

// Construction writes `specversion`, `data`, and `data_base64` beside the fields `Fact` addresses, so the drop
// census subtracts both owned sets and reports only names neither this roster nor `Fact` itself holds.
const _addressed: ReadonlyArray<string> = ["data", "data_base64", "specversion", ...Record.keys(Fact.fields)]

// Decoders without the roster read every declared extension as an unknown string, so the whole roster decodes on
// every read and the drop band carries what the peer sent that this roster does not name. Names alone said only THAT
// something went missing; each name now returns as a `Fault.Drop` occurrence carrying its measured extent, so a
// consumer folds the band through `Fault.Ledger` and reads one census rather than sizing an anonymous list.
const _read = (envelope: CloudEventV1<unknown>): Event.Read => ({
  roster: Record.map(_readers, (read, name) => read(envelope[name])) as unknown as Event.Roster,
  dropped: Array.filterMap(
    Object.keys(envelope),
    (key) => _extensions.is(key) || Array.contains(_addressed, key)
      ? Option.none()
      : Option.some(Fault.Drop.fact("foreign", key, 1)),
  ),
})

const Event: Event.Shape = {
  Fact,
  Refusal: EventRefusal,
  classes: _classes,
  extensions: _extensions,
  digested: Array.filter(_extensionNames, (name) => _extensions.at(name).signed),
  mint: _mint,
  at: _at,
  read: _read,
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
