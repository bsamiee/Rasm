import { createHmac } from 'node:crypto';
import { createServer } from 'node:net';
import { join } from 'node:path';
import process from 'node:process';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layer as services } from '@effect/platform-node/NodeServices';
import { assert, layer } from '@effect/vitest';
import { App } from '@rasm/hosting';
import { routed } from '@rasm/testing/playwright';
import { PostgreSqlContainer } from '@testcontainers/postgresql';
import { Cause, DateTime, Duration, Effect, Layer, type Record, Schedule } from 'effect';
import { HttpBody, HttpClient, HttpClientRequest } from 'effect/http';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const documented = {
    secret: 'tskey-webhook-kExample1CNTRL-0123456789abcdef',
    body: '[{"timestamp":"2022-09-21T13:37:51.658918-04:00","version":1,"type":"test","tailnet":"example.com","message":"This is a test event","data":null}]',
    signature: 't=1663781880,v1=1d3b2cdcad5c8c64af84284e332b174a087ebf35d68a417c46371ec97574d3f5',
    header: 'tailscale-webhook-signature',
    path: '/tailscale',
} as const;
const hour = Duration.toSeconds('1 hour');

// --- [OPERATIONS] ----------------------------------------------------------------------

const signed = (at: number, body: string, secret: string = documented.secret) => [`t=${at}`, createHmac('sha256', secret).update(`${at}.${body}`).digest('hex')].join(',v1=');
const allocated = Effect.callback<number>((resume) => {
    const probe = createServer().listen(0, '127.0.0.1', () => {
        const address = probe.address();
        probe.close(() => resume(Effect.succeed(typeof address === 'object' && address !== null ? address.port : 0)));
    });
});
const posted = (path: string, body: string, headers: Record.ReadonlyRecord<string, string>) => Effect.map(HttpClient.post(path, { headers, body: HttpBody.text(body, 'application/json') }), ({ status }) => status);

// --- [COMPOSITION] ---------------------------------------------------------------------

const receiver = Layer.unwrap(
    Effect.gen(function* () {
        const [spawner, bundle] = yield* Effect.all([ChildProcessSpawner.ChildProcessSpawner, Effect.fromOption(routed('@rasm/nx-vite', '@rasm/hooks'), () => new Cause.UnknownError('@rasm/hooks', 'nx.json states no Vite outputPath to run @rasm/hooks from'))]);
        const [database, port] = yield* Effect.all([
            Effect.acquireRelease(
                Effect.promise(() => new PostgreSqlContainer('postgres:19.0-alpine3.24').start()),
                (started) => Effect.promise(() => started.stop()),
            ),
            allocated,
        ]);
        const server = yield* spawner.spawn(
            ChildProcess.make(process.execPath, [join(bundle, App.bundle.environment, `${App.bundle.entry}.js`)], { env: { [App.bundle.portVariable]: `${port}`, POSTGRES_URL: database.getConnectionUri(), TAILSCALE_WEBHOOK_SECRET: documented.secret }, extendEnv: true, stdout: 'ignore', stderr: 'ignore' }),
        );
        const client = HttpClient.mapRequest(yield* HttpClient.HttpClient, HttpClientRequest.prependUrl(`http://127.0.0.1:${port}`));
        yield* Effect.raceFirst(
            Effect.retry(Effect.timeout(HttpClient.filterStatusOk(client).get(App.bundle.readiness), '1 second'), { schedule: Schedule.spaced('100 millis') }),
            Effect.flatMap(server.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `@rasm/hooks exits ${code} before it answers ${App.bundle.readiness}`))),
        );
        return Layer.succeed(HttpClient.HttpClient, client);
    }),
).pipe(Layer.provide([layerUndici, services]));

layer(receiver, { timeout: '1 minute', excludeTestServices: true })('webhook receiver', (it) => {
    const text = 'not JSON';
    it.effect.each([
        { label: 'a delivery signed now', path: documented.path, body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now, documented.body) }), status: 204 },
        { label: 'a delivery signed at the tolerance of 5 minutes', path: documented.path, body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now - Duration.toSeconds('4 minutes'), documented.body) }), status: 204 },
        { label: 'a signed delivery with a changed body', path: documented.path, body: documented.body.replace('test event', 'changed event'), sign: (now: number) => ({ [documented.header]: signed(now, documented.body) }), status: 401 },
        { label: 'the documented delivery, signed in 2022', path: documented.path, body: documented.body, sign: () => ({ [documented.header]: documented.signature }), status: 401 },
        { label: 'a delivery signed an hour ago', path: documented.path, body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now - hour, documented.body) }), status: 401 },
        { label: 'a delivery signed an hour ahead', path: documented.path, body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now + hour, documented.body) }), status: 401 },
        { label: 'a delivery another secret signed', path: documented.path, body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now, documented.body, 'tskey-webhook-another') }), status: 401 },
        { label: 'an unsigned delivery', path: documented.path, body: documented.body, sign: () => ({}), status: 401 },
        { label: 'an unsigned body that is no JSON', path: documented.path, body: text, sign: () => ({}), status: 401 },
        { label: 'a body that is no JSON under a signature of another body', path: documented.path, body: text, sign: (now: number) => ({ [documented.header]: signed(now, documented.body) }), status: 401 },
        { label: 'a signed body that is no JSON', path: documented.path, body: text, sign: (now: number) => ({ [documented.header]: signed(now, text) }), status: 400 },
        { label: 'a signed delivery to a path of no sender', path: '/unknown', body: documented.body, sign: (now: number) => ({ [documented.header]: signed(now, documented.body) }), status: 404 },
    ])(
        'answers $label $status',
        Effect.fnUntraced(function* ({ path, body, sign, status }) {
            const now = DateTime.toEpochSeconds(yield* DateTime.now);
            assert.strictEqual(yield* posted(path, body, sign(now)), status);
        }),
    );

    it.effect(
        'accepts a delivery a sender repeats',
        Effect.fnUntraced(function* () {
            const headers = { [documented.header]: signed(DateTime.toEpochSeconds(yield* DateTime.now), documented.body) };
            assert.deepStrictEqual(yield* Effect.all({ sent: posted(documented.path, documented.body, headers), repeated: posted(documented.path, documented.body, headers) }), { sent: 204, repeated: 204 });
        }),
    );
});
