import { Array, Bytes, Cause, Data, DateTime, Effect, ErrorReporter, Formatter, Option, Record, Redacted, Result, Schema, SchemaGetter, SchemaTransformation, UndefinedOr } from 'effect';
import { type CryptoError, Mac, Signature } from 'effect/crypto';
import { Base64, type EncodingError, Hex } from 'effect/encoding';
import { Headers, HttpServerRespondable, HttpServerResponse, HttpStatus } from 'effect/http';

// --- [TYPES] ---------------------------------------------------------------------------

type Part = (typeof PARTS)[number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const PARTS = ['id', 't'] as const;

// --- [TABLES] --------------------------------------------------------------------------

const keying = {
    symmetric: Effect.fnUntraced(function* (bytes: Uint8Array) {
        const [mac, key] = yield* Effect.all([Mac.Mac, Mac.key(Mac.hmacSha256, bytes)]);
        return { sign: (content: Uint8Array) => mac.sign(key, content), verify: (signature: Uint8Array, content: Uint8Array) => mac.verify(key, signature, content) };
    }),
    signing: Effect.fnUntraced(function* (bytes: Uint8Array) {
        const [signer, key] = yield* Effect.all([Signature.Signature, Signature.privateKey(Signature.ed25519, bytes)]);
        const open = yield* signer.publicKeyOf(key);
        return { sign: (content: Uint8Array) => signer.sign(key, content), verify: (signature: Uint8Array, content: Uint8Array) => signer.verify(open, signature, content) };
    }),
    verifying: Effect.fnUntraced(function* (bytes: Uint8Array) {
        const [signer, key] = yield* Effect.all([Signature.Signature, Signature.publicKey(Signature.ed25519, bytes)]);
        return {
            sign: (): Effect.Effect<Uint8Array, CryptoError.CryptoError | Cause.IllegalArgumentError> => Effect.fail(new Cause.IllegalArgumentError('Received a public key to sign with, accepted a shared secret or a private key')),
            verify: (signature: Uint8Array, content: Uint8Array) => signer.verify(key, signature, content),
        };
    }),
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const encoder = new TextEncoder();
const Stamp = Schema.FiniteFromString.check(Schema.isInt()).pipe(Schema.decodeTo(Schema.DateTimeUtc, SchemaTransformation.transform({ decode: DateTime.fromEpochSeconds, encode: DateTime.toEpochSeconds })));

class Scheme extends Data.Class<{
    readonly signature: string;
    readonly named: { readonly [P in Part]?: string };
    readonly signed: readonly Part[];
    readonly separator: string;
    readonly assign: string;
    readonly encoding: Pick<typeof Hex, 'decode' | 'encode'>;
    readonly material: (secret: string) => Result.Result<Uint8Array, EncodingError.EncodingError>;
    readonly keys: Record.ReadonlyRecord<string, { readonly label: string; readonly key: (typeof keying)[keyof typeof keying] }>;
}> {
    // --- [ROWS]

    static readonly prefixed = (prefix: string) =>
        new Scheme({
            signature: `${prefix}-signature`,
            named: { id: `${prefix}-id`, t: `${prefix}-timestamp` },
            signed: PARTS,
            separator: ' ',
            assign: ',',
            encoding: Base64,
            material: Base64.decode,
            keys: { whsec_: { label: 'v1', key: keying.symmetric }, whsk_: { label: 'v1a', key: keying.signing }, whpk_: { label: 'v1a', key: keying.verifying } },
        });
    static readonly keyed = (signature: string, label: string, signed: readonly Part[], named: Scheme['named'] = {}) => new Scheme({ signature, named, signed, separator: ',', assign: '=', encoding: Hex, material: (secret) => Result.succeed(encoder.encode(secret)), keys: { '': { label, key: keying.symmetric } } });
    static readonly standard = Scheme.prefixed('webhook');
    static readonly svix = Scheme.prefixed('svix');
    static readonly tailscale = Scheme.keyed('tailscale-webhook-signature', 'v1', ['t']);
    static readonly stripe = Scheme.keyed('stripe-signature', 'v1', ['t']);
    static readonly github = Scheme.keyed('x-hub-signature-256', 'sha256', [], { id: 'x-github-delivery' });

    // --- [WIRE]

    readonly entries = Schema.String.pipe(
        Schema.decodeTo(Schema.Array(Schema.TemplateLiteralParser([Schema.String, this.assign, Schema.String])), {
            decode: SchemaGetter.split({ separator: this.separator }),
            encode: SchemaGetter.transform(Array.join(this.separator)),
        }),
    );
    readonly content = (values: { readonly [P in Part]?: string }, body: Uint8Array) => Bytes.concat([encoder.encode([...this.signed.flatMap((part) => Array.fromNullishOr(values[part])), ''].join('.')), body]);
    static readonly read = Effect.fnUntraced(
        function* ({ signature, named, signed, entries, content, encoding }: Scheme, headers: Headers.Headers, body: Uint8Array) {
            const found = (name: string, value: Option.Option<string>) => Effect.fromOption(value, WebhookError.of({ _tag: 'Unsigned' }, name));
            const written = yield* Effect.flatMap(found(signature, Headers.get(headers, signature)), Schema.decodeEffect(entries));
            const listed = Record.fromIterableWith(written, ([label, , value]) => [label, value]);
            const held = yield* Effect.all(Record.fromIterableWith(signed, (part) => [part, UndefinedOr.match(named[part], { onDefined: (header) => found(header, Headers.get(headers, header)), onUndefined: () => found(signature, Record.get(listed, part)) })]));
            const timestamp = yield* Effect.transposeOption(Option.map(Record.get(held, 't'), Schema.decodeEffect(Stamp)));
            const signatures = Array.filterMap(written, ([label, , value]) => Result.map(encoding.decode(value), (bytes) => ({ label, bytes })));
            return { id: Record.get(held, 'id'), timestamp, content: content(held, body), signatures };
        },
        (effect, { signature }) => Effect.catchTag(effect, 'SchemaError', WebhookError.of({ _tag: 'Unsigned' }, signature)),
    );
    write(values: { readonly [P in Part]: string }, signatures: readonly { readonly label: string; readonly bytes: Uint8Array }[]) {
        const written = [...this.signed.filter((part) => this.named[part] === undefined).map((part) => [part, this.assign, values[part]] as const), ...signatures.map(({ label, bytes }) => [label, this.assign, this.encoding.encode(bytes)] as const)];
        return Effect.map(Effect.orDie(Schema.encodeEffect(this.entries)(written)), (text) => Headers.fromInput([...Record.toEntries(values).flatMap(([part, value]) => Array.fromNullishOr(this.named[part]).map((header) => [header, value] as const)), [this.signature, text]]));
    }

    // --- [KEYS]

    static readonly key = Effect.fnUntraced(function* ({ signature, keys, material }: Scheme, secret: Redacted.Redacted) {
        const text = Redacted.value(secret);
        const [prefix, { label, key }] = yield* Effect.fromOption(
            Array.findFirst(Record.toEntries(keys), ([opening]) => text.startsWith(opening)),
            () => new Cause.IllegalArgumentError(`Received a ${signature} secret with an unknown prefix, accepted ${Record.keys(keys).join(', ')}`),
        );
        const built = yield* key(yield* Effect.fromResult(material(text.slice(prefix.length))));
        return { label, verify: built.verify, sign: (content: Uint8Array) => Effect.map(built.sign(content), (bytes) => ({ label, bytes })) };
    });
}

// --- [ERRORS] --------------------------------------------------------------------------

class WebhookError extends Schema.TaggedError<WebhookError, { readonly brand: unique symbol }>()('WebhookError', {
    reason: Schema.TaggedUnion({ Unsigned: {}, Mismatch: {}, Stale: { skew: Schema.Duration } }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly of = (reason: WebhookError['reason'], subject: string) => (cause?: unknown) => WebhookError.make({ reason, subject, ...(cause === undefined ? {} : { cause }) });
    override readonly [ErrorReporter.ignore] = true;

    [HttpServerRespondable.symbol]() {
        return Effect.succeed(HttpServerResponse.text(this.message, { status: HttpStatus.fromLiteral('Unauthorized') }));
    }
    override get message() {
        return [`${this.reason._tag}: ${this.subject}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Scheme, Stamp, WebhookError };
