import { createHmac } from 'node:crypto';
import { layer } from '@effect/platform-node/NodeCrypto';
import { layerTest } from '@effect/platform-node/NodeHttpServer';
import { assert, it } from '@effect/vitest';
import { paths, routes } from '@rasm/hooks/senders';
import { ConfigProvider, DateTime, Duration, Effect, Layer, type Record } from 'effect';
import { HttpBody, HttpClient, HttpRouter } from 'effect/http';
import { PersistedQueue } from 'effect/persistence';

// --- [CONSTANTS] -----------------------------------------------------------------------

const documented = {
    secret: 'tskey-webhook-kExample1CNTRL-0123456789abcdef',
    at: 1_663_781_880,
    body: '[{"timestamp":"2022-09-21T13:37:51.658918-04:00","version":1,"type":"test","tailnet":"example.com","message":"This is a test event","data":null}]',
    signature: 't=1663781880,v1=1d3b2cdcad5c8c64af84284e332b174a087ebf35d68a417c46371ec97574d3f5',
    header: 'tailscale-webhook-signature',
} as const;
const text = 'not JSON';
const hour = Duration.toSeconds('1 hour');

// --- [OPERATIONS] ----------------------------------------------------------------------

const signed = (at: number, body: string, secret: string = documented.secret) => [`t=${at}`, createHmac('sha256', secret).update(`${at}.${body}`).digest('hex')].join(',v1=');
const receiver = HttpRouter.serve(routes, { disableLogger: true, disableListenLog: true }).pipe(
    Layer.provide([layer, Layer.provide(PersistedQueue.layer, PersistedQueue.layerStoreMemory), ConfigProvider.layer(ConfigProvider.fromEnv({ env: { TAILSCALE_WEBHOOK_SECRET: documented.secret, TAILSCALE_HOSTS: 'alice-workstation1.yak-bebop.ts.net' } }))]),
);
const posted = (path: string, body: string, headers: Record.ReadonlyRecord<string, string>) => Effect.map(HttpClient.post(path, { headers, body: HttpBody.text(body, 'application/json') }), ({ status }) => status);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.live.each([
    { label: 'a delivery signed now', path: paths.tailscale, body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now, documented.body) }), status: 204 },
    { label: 'a delivery signed at the tolerance of 5 minutes', path: paths.tailscale, body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now - Duration.toSeconds('4 minutes'), documented.body) }), status: 204 },
    { label: 'a signed delivery with a changed body', path: paths.tailscale, body: documented.body.replace('test event', 'changed event'), sign: (now: number) => ({ [documented.header]: signed(now, documented.body) }), status: 401 },
    { label: 'the documented delivery, signed in 2022', path: paths.tailscale, body: documented.body, sign: () => ({ [documented.header]: documented.signature }), status: 401 },
    { label: 'a delivery signed an hour ago', path: paths.tailscale, body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now - hour, documented.body) }), status: 401 },
    { label: 'a delivery signed an hour ahead', path: paths.tailscale, body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now + hour, documented.body) }), status: 401 },
    { label: 'a delivery another secret signed', path: paths.tailscale, body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now, documented.body, 'tskey-webhook-another') }), status: 401 },
    { label: 'an unsigned delivery', path: paths.tailscale, body: documented.body, sign: () => ({}), status: 401 },
    { label: 'an unsigned body that is no JSON', path: paths.tailscale, body: text, sign: () => ({}), status: 401 },
    { label: 'a body that is no JSON under a signature of another body', path: paths.tailscale, body: text, sign: (now: number) => ({ [documented.header]: signed(now, documented.body) }), status: 401 },
    { label: 'a signed body that is no JSON', path: paths.tailscale, body: text, sign: (now: number) => ({ [documented.header]: signed(now, text) }), status: 400 },
    { label: 'a signed delivery to a path of no sender', path: '/unknown', body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now, documented.body) }), status: 404 },
])(
    'answers $label $status',
    Effect.fnUntraced(function* ({ path, body, sign, status }) {
        yield* Layer.build(receiver);
        const now = DateTime.toEpochSeconds(yield* DateTime.now);
        assert.deepStrictEqual([signed(documented.at, documented.body), yield* posted(path, body, sign(now))], [documented.signature, status]);
    }, Effect.provide(layerTest)),
);

it.live(
    'accepts a delivery a sender repeats',
    Effect.fnUntraced(function* () {
        yield* Layer.build(receiver);
        const headers = { [documented.header]: signed(DateTime.toEpochSeconds(yield* DateTime.now), documented.body) };
        assert.deepStrictEqual(yield* Effect.all({ sent: posted(paths.tailscale, documented.body, headers), repeated: posted(paths.tailscale, documented.body, headers) }), { sent: 204, repeated: 204 });
    }, Effect.provide(layerTest)),
);
