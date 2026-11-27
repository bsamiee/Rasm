import { Scheme } from '@rasm/webhooks/scheme';
import { Webhook } from '@rasm/webhooks/webhook';
import { Array, Duration, Effect, Function, type Layer, Redacted, Schema } from 'effect';
import type { Digest, Mac, Signature } from 'effect/crypto';

// --- [TYPES] ---------------------------------------------------------------------------

type Crypto = Layer.Layer<Mac.Mac | Signature.Signature | Digest.Digest>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const encoder = new TextEncoder();
const tolerance = Duration.fromInputUnsafe('5 minutes');
const keyed = (signed: string) => `${signed.replace(',', ',v1=00ff,v0=abcd,')},v1=abcd`;
const standard = {
    scheme: Scheme.standard,
    secret: 'whsec_MfKQ9r8GKYqrTwjUPD8ILPZIo2LaLaSw',
    foreign: 'whsec_C2FVsBQIhrscChlQIMV+b5sSYspob7oD',
    id: 'msg_p5jXN8AQM9LWM0D4loKWxJek',
    at: 1_614_265_330,
    body: '{"test": 2432232314}',
    accepted: 'verified',
    late: 'Stale',
    crowded: (signed: string) => `v1,AAAA v9,BBBB ${signed} v1,CCCC`,
} as const;
const pair = { private: 'whsk_nWGxne/9WmC6hEr0kuwsxERJxWl7MmkZcDusAxyuf2A=', public: 'whpk_11qYAYKxCrfVS/7TyWQHOg7hcvPapiMlrwIaaPcHURo=', foreign: 'whsk_AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=' } as const;
const hub = { ...standard, scheme: Scheme.github, late: 'verified', crowded: (signed: string) => `sha256=00ff,${signed},sha256=abcd` } as const;
const vectors = {
    standard: { ...standard, bytes: encoder.encode(standard.body), signed: 'v1,g0hM9SsE+OTPJTGt/tmIKtSyZlE3uFJELVlNIOLJ1OE=' },
    binary: { ...standard, bytes: Uint8Array.from({ length: 256 }, (_, index) => index), signed: 'v1,sEcZ042G8Crqgxbybb3y0eDxQR3TcfYsk11I1XULhSI=', accepted: 'SchemaError' },
    rust: { ...standard, secret: standard.foreign, foreign: standard.secret, id: 'msg_27UH4WbU6Z5A5EzD8u03UvzRbpk', at: 1_649_367_553, bytes: encoder.encode('{"email":"test@example.com","username":"test_user"}'), signed: 'v1,tZ1I4/hDygAJgO5TYxiSd6Sd0kDW6hPenDe+bTa3Kkw=' },
    ed25519: { ...standard, secret: pair.private, foreign: pair.foreign, bytes: encoder.encode(standard.body), signed: 'v1a,fldxM4gAKugP6nnt1hdz3sgGfZ6d99nzrMFnZOELIxbzEHoVmAb2ADpkJK7zgPePmPsle0zV9jSeGlHFG2NVAw==' },
    svix: { ...standard, scheme: Scheme.svix, secret: 'whsec_plJ3nmyCDGBKInavdOK15jsl', id: 'msg_loFOjxBNrRLzqYUf', at: 1_731_705_121, bytes: encoder.encode('{"event_type":"ping","data":{"success":true}}'), signed: 'v1,rAvfW3dJ/X/qxhsaXPOyyCGmRKsaKWcsNccKXlIktD0=' },
    github: { ...hub, secret: "It's a Secret to Everybody", bytes: encoder.encode('Hello, World!'), signed: 'sha256=757107ea0eb2509fc211221cce984b8a37570b6d7586c22c46f4379c8b043e17', accepted: 'SchemaError', late: 'SchemaError' },
    octokit: { ...hub, secret: 'mysecret', foreign: 'foo', bytes: encoder.encode('{"foo":"bar"}'), signed: 'sha256=4864d2759938a15468b5df9ade20bf161da9b4f737ea61794142f3484236bda3' },
    tailscale: {
        ...standard,
        scheme: Scheme.tailscale,
        secret: 'tskey-webhook-kExample1CNTRL-0123456789abcdef',
        at: 1_663_781_880,
        bytes: encoder.encode('[{"timestamp":"2022-09-21T13:37:51.658918-04:00","version":1,"type":"test","tailnet":"example.com","message":"This is a test event","data":null}]'),
        signed: 't=1663781880,v1=1d3b2cdcad5c8c64af84284e332b174a087ebf35d68a417c46371ec97574d3f5',
        crowded: keyed,
    },
    stripe: { ...standard, scheme: Scheme.stripe, secret: 'whsec_stripeexamplesecret', at: 1_492_774_577, bytes: encoder.encode('{"id":"evt_1","object":"event"}'), signed: 't=1492774577,v1=3c9d0dd9cf25b86ddd4ce4a4d6adbf58ea11e924b829fd7d28481310f7411b9e', crowded: keyed },
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const made = (scheme: Scheme, ...secrets: Array.NonEmptyReadonlyArray<string>) => Webhook({ scheme, tolerance, payload: Schema.Json, secrets: Array.map(secrets, (secret) => Redacted.make(secret)) });
const outcome = <A, R>(effect: Effect.Effect<A, Effect.Error<ReturnType<Webhook<typeof Schema.Json>['verify']>>, R>) => effect.pipe(Effect.unwrapReason('WebhookError'), Effect.match({ onSuccess: Function.constant('verified'), onFailure: ({ _tag }) => _tag }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Crypto, encoder, made, outcome, pair, standard, tolerance, vectors };
