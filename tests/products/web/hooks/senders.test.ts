import { createHash, createHmac } from 'node:crypto';
import { createServer } from 'node:net';
import { join } from 'node:path';
import process from 'node:process';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layer as services } from '@effect/platform-node/NodeServices';
import { PgClient } from '@effect/sql-pg';
import { assert, layer } from '@effect/vitest';
import { App } from '@rasm/hosting';
import { routed } from '@rasm/testing/playwright';
import { PostgreSqlContainer } from '@testcontainers/postgresql';
import { Cause, DateTime, Effect, Layer, Redacted, Schedule, Schema } from 'effect';
import { HttpBody, HttpClient, HttpClientRequest } from 'effect/http';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { SqlClient, SqlSchema } from 'effect/sql';

// --- [CONSTANTS] -----------------------------------------------------------------------

const sender = { secret: 'tskey-webhook-kExample1CNTRL-0123456789abcdef', header: 'tailscale-webhook-signature', path: '/tailscale', queue: 'tailscale' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const event = (message: string) => JSON.stringify([{ timestamp: '2022-09-21T13:37:51.658918-04:00', version: 1, type: 'test', tailnet: 'example.com', message, data: null }]);
const signed = (at: number, body: string, secret: string = sender.secret) => ({ [sender.header]: [`t=${at}`, createHmac('sha256', secret).update(`${at}.${body}`).digest('hex')].join(',v1=') });
const allocated = Effect.callback<number>((resume) => {
    const probe = createServer().listen(0, '127.0.0.1', () => {
        const address = probe.address();
        probe.close(() => resume(Effect.succeed(typeof address === 'object' && address !== null ? address.port : 0)));
    });
});
const posted = (body: string, headers: Readonly<Record<string, string>>) => Effect.map(HttpClient.post(sender.path, { headers, body: HttpBody.text(body, 'application/json') }), ({ status }) => status);
const digest = (body: string) => createHash('sha256').update(body).digest('hex');
const settled = SqlSchema.findAll({
    Request: Schema.Void,
    Result: Schema.Struct({ id: Schema.String, state: Schema.String, attempts: Schema.Int, updated: Schema.Unknown }),
    execute: () => SqlClient.SqlClient.use((sql) => sql`SELECT id, state, attempts, updated_at AS updated FROM effect_queue WHERE queue_name = ${sender.queue} ORDER BY id`),
})(undefined).pipe(Effect.repeat({ until: (rows) => rows.every(({ state }) => state === 'completed'), schedule: Schedule.spaced('100 millis') }), Effect.timeout('10 seconds'));
const added = Effect.fnUntraced(function* <A, E, R>(post: Effect.Effect<A, E, R>) {
    const known = new Set((yield* settled).map(({ id }) => id));
    const answered = yield* post;
    return { answered, added: (yield* settled).filter(({ id }) => !known.has(id)).map(({ id, state, attempts }) => ({ id, state, attempts })) };
});

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
            ChildProcess.make(process.execPath, [join(bundle, App.bundle.environment, `${App.bundle.entry}.js`)], { env: { [App.bundle.portVariable]: `${port}`, POSTGRES_URL: database.getConnectionUri(), TAILSCALE_WEBHOOK_SECRET: sender.secret }, extendEnv: true, stdout: 'ignore', stderr: 'ignore' }),
        );
        const client = HttpClient.mapRequest(yield* HttpClient.HttpClient, HttpClientRequest.prependUrl(`http://127.0.0.1:${port}`));
        yield* Effect.raceFirst(
            Effect.retry(Effect.timeout(HttpClient.filterStatusOk(client).get(App.bundle.readiness), '1 second'), { schedule: Schedule.spaced('100 millis') }),
            Effect.flatMap(server.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `@rasm/hooks exits ${code} before it answers ${App.bundle.readiness}`))),
        );
        return Layer.merge(Layer.succeed(HttpClient.HttpClient, client), PgClient.layer({ url: Redacted.make(database.getConnectionUri()) }));
    }),
).pipe(Layer.provide([layerUndici, services]));

layer(receiver, { timeout: '1 minute', excludeTestServices: true })('webhook receiver', (it) => {
    it.effect.each([
        { label: 'now', offset: 0, secret: sender.secret, status: 204, worked: true },
        { label: '290 seconds ago, inside 5 minutes', offset: -290, secret: sender.secret, status: 204, worked: true },
        { label: '310 seconds ago, past 5 minutes', offset: -310, secret: sender.secret, status: 401, worked: false },
        { label: 'now with another secret', offset: 0, secret: 'tskey-webhook-another', status: 401, worked: false },
    ])(
        'answers a delivery signed $label with $status, and adds to its Postgres queue only a delivery it accepts, worked once',
        Effect.fnUntraced(function* ({ label, offset, secret, status, worked }) {
            const body = event(`delivery ${label}`);
            const now = DateTime.toEpochSeconds(yield* DateTime.now);
            assert.deepStrictEqual(yield* added(posted(body, signed(now + offset, body, secret))), { answered: status, added: worked ? [{ id: digest(body), state: 'completed', attempts: 1 }] : [] });
        }),
    );
    it.effect(
        'answers a delivery a sender repeats after its work without working it again',
        Effect.fnUntraced(function* () {
            const body = event('repeated delivery');
            const headers = signed(DateTime.toEpochSeconds(yield* DateTime.now), body);
            const first = yield* added(posted(body, headers));
            const worked = (yield* settled).filter(({ id }) => id === digest(body));
            const repeated = yield* added(posted(body, headers));
            assert.deepStrictEqual({ first, repeated, replayed: (yield* settled).filter(({ id }) => id === digest(body)) }, { first: { answered: 204, added: [{ id: digest(body), state: 'completed', attempts: 1 }] }, repeated: { answered: 204, added: [] }, replayed: worked });
        }),
    );
});
