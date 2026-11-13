import { constants } from 'node:http2';
import { env } from 'node:process';
import { hashArray, type PreTasksExecution } from '@nx/devkit';
import { Cause, Config, Console, Effect, Option } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientRequest } from 'effect/http';
import { promised } from '#/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CACHE = { server: 'NX_SELF_HOSTED_REMOTE_CACHE_SERVER', token: 'NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN', wait: '5 seconds', served: new Set<number>([constants.HTTP_STATUS_OK, constants.HTTP_STATUS_NOT_FOUND]) } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const preTasksExecution: PreTasksExecution = (_, { id }) =>
    promised(
        Effect.gen(function* () {
            const [server, token, annotated] = yield* Effect.all([Config.option(Config.NonEmptyString(CACHE.server)), Config.option(Config.Redacted(CACHE.token)), Config.withDefault(Config.Boolean('GITHUB_ACTIONS'), false)]);
            const stated = Option.map(server, (url) => HttpClientRequest.accept(HttpClientRequest.get(`${url}/v1/cache/${hashArray([id])}`), 'application/octet-stream'));
            const asked = Option.map(stated, (request) => HttpClient.execute(Option.match(token, { onNone: () => request, onSome: (held) => HttpClientRequest.bearerToken(request, held) })));
            const answered = yield* Effect.transposeOption(
                Option.map(asked, (sent) =>
                    Effect.matchCause(Effect.timeout(sent, CACHE.wait), {
                        onSuccess: ({ status }) => Option.liftPredicate(`answered ${status}`, () => !CACHE.served.has(status)),
                        onFailure: (cause) => Option.some(`did not answer: ${Cause.pretty(cause)}`),
                    }),
                ),
            );
            const warning = Option.map(Option.all([server, Option.flatten(answered)]), ([url, answer]) => `Nx remote cache ${url} ${answer}, this run reads and writes the local cache alone`);
            yield* Effect.transposeOption(
                Option.map(warning, (text) =>
                    Effect.andThen(
                        Effect.sync(() => Reflect.set(env, CACHE.server, '')),
                        Console.warn(annotated ? `::warning title=Nx remote cache::${text.replaceAll('%', '%25').replaceAll('\r', '%0D').replaceAll('\n', '%0A')}` : text),
                    ),
                ),
            );
        }).pipe(Effect.provide(FetchHttpClient.layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { preTasksExecution };
