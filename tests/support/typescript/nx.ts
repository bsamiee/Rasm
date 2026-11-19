import { Buffer } from 'node:buffer';
import { mkdirSync, writeFileSync } from 'node:fs';
import { createServer } from 'node:http';
import { constants } from 'node:http2';
import { delimiter, join } from 'node:path';
import { env } from 'node:process';
import { Array, Effect, Function, Match, Option, Predicate, Record, Schema } from 'effect';
import { onTestFinished, vi } from 'vitest';

// --- [TYPES] ---------------------------------------------------------------------------

interface Asked {
    readonly authorized: boolean;
    readonly held: Buffer | undefined;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const inherited = Record.filter(env, (value, name): value is string => Predicate.isString(value) && !['NX_', 'VITEST', 'npm_'].some((prefix) => name.startsWith(prefix)) && !['NODE_ENV', 'NO_COLOR', 'VIRTUAL_ENV', 'GITHUB_ACTIONS'].includes(name));

// --- [MODELS] --------------------------------------------------------------------------

const Listening = Schema.Struct({ port: Schema.Int }).annotate({ description: 'The address a TCP server returns once listening, its port the one the system chose' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const environment = (root: string, set: Record.ReadonlyRecord<string, string> = {}) => ({
    ...inherited,
    NX_DAEMON: 'false',
    NX_TUI: 'false',
    FORCE_COLOR: '0',
    PATH: [join(root, 'node_modules/.bin'), ...Option.toArray(Record.get(inherited, 'PATH'))].join(delimiter),
    ...set,
});

const refusing = (tool: string) => [
    { case: 'a missing executable', scripts: {}, refusal: new RegExp(`^${RegExp.escape(tool)} .*cannot start: `, 'u') },
    { case: 'a non-zero exit', scripts: { [tool]: 'echo refused >&2\nexit 3' }, refusal: new RegExp(`^${RegExp.escape(tool)} .*exits 3: refused$`, 'u') },
];

const pathed = (folder: string, scripts: Record.ReadonlyRecord<string, string>) => {
    mkdirSync(folder, { recursive: true });
    Record.toEntries(scripts).forEach(([name, body]) => writeFileSync(join(folder, name), `#!/bin/sh\n${body}\n`, { mode: 0o755 }));
    vi.stubEnv('PATH', folder);
    onTestFinished(() => {
        vi.unstubAllEnvs();
    });
};

const answered = Match.type<Asked>().pipe(
    Match.when({ authorized: false }, () => constants.HTTP_STATUS_UNAUTHORIZED),
    Match.when({ held: Match.undefined }, () => constants.HTTP_STATUS_NOT_FOUND),
    Match.orElse(() => constants.HTTP_STATUS_OK),
);

const served = (token: string, answer: (asked: Asked) => Option.Option<number>) => {
    const [held, requests] = [new Map<string, Buffer>(), Array.empty<string>()];
    const server = createServer((request, response) => {
        const chunks: Buffer[] = [];
        requests.push(`${request.method} ${request.url}`);
        request.on('data', (chunk: Buffer) => {
            chunks.push(chunk);
        });
        request.on('end', () => {
            const body = request.method === 'PUT' ? held.set(String(request.url), Buffer.concat(chunks)).get(String(request.url)) : held.get(String(request.url));
            Option.match(answer({ authorized: request.headers.authorization === `Bearer ${token}`, held: body }), {
                onNone: Function.constVoid,
                onSome: (status) => response.writeHead(status).end(request.method === 'GET' ? body : undefined),
            });
        });
    });
    return { server, requests };
};

const remote = Effect.fnUntraced(function* (token: string, answer: (asked: Asked) => Option.Option<number>) {
    const { server, requests } = yield* Effect.acquireRelease(
        Effect.callback<ReturnType<typeof served>>((resume) => {
            const listening = served(token, answer);
            listening.server.listen(0, '127.0.0.1', () => resume(Effect.succeed(listening)));
        }),
        ({ server: listening }) =>
            Effect.promise(() => {
                listening.closeAllConnections();
                return listening[Symbol.asyncDispose]();
            }),
    );
    const { port } = yield* Schema.decodeUnknownEffect(Listening)(server.address());
    return { address: `http://127.0.0.1:${port}`, requests };
}, Effect.orDie);

// --- [EXPORTS] -------------------------------------------------------------------------

export { answered, environment, pathed, refusing, remote };
