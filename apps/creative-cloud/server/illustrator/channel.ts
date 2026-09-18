// --- [IMPORTS] -------------------------------------------------------------------------

import { type Config, Effect, FileSystem, Option, Path, type PlatformError, Schema } from 'effect';
import type { ChildProcessSpawner } from 'effect/unstable/process';
import { BridgeError, HostRejection, inaccessible, notDecodable } from '../errors.ts';
import { type Discovered, discover } from '../hosts.ts';
import { artifacts, root } from '../jobs.ts';
import { literal, read } from '../osascript.ts';
import { ARTIFACTS, HOSTS, OptionalNumber, OptionalString, type TimeoutMs } from '../values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Site {
    readonly host: Discovered;
    readonly scripts: string;
    readonly jobs: string;
}

interface Script<Request extends Schema.Codec<unknown, unknown, never, never>, Response extends Schema.Codec<unknown, unknown, never, never>> {
    readonly entry: string;
    readonly request: Request;
    readonly response: Response;
}

// --- [MODELS] --------------------------------------------------------------------------

const _failure = { name: Schema.String, message: Schema.String, number: Schema.Number, code: OptionalNumber, tag: OptionalString, fileName: Schema.String, line: Schema.Number };

const _envelope = Schema.decodeUnknownOption(Schema.Struct({ kind: Schema.Literal('error'), ..._failure }).pipe(Schema.encodeKeys({ fileName: 'file' })));

const Unavailable: Schema.encodeKeys<
    Schema.Struct<{
        readonly name: Schema.String;
        readonly message: Schema.String;
        readonly number: Schema.Number;
        readonly code: Schema.OptionFromOptionalKey<Schema.Number>;
        readonly tag: Schema.OptionFromOptionalKey<Schema.String>;
        readonly fileName: Schema.String;
        readonly line: Schema.Number;
        readonly path: Schema.String;
    }>,
    { readonly fileName: 'file' }
> = Schema.Struct({ ..._failure, path: Schema.String }).pipe(Schema.encodeKeys({ fileName: 'file' }));

// --- [SITE] ----------------------------------------------------------------------------

const site: Effect.Effect<Site, BridgeError | PlatformError.PlatformError | Schema.SchemaError | Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> = Effect.map(
    Effect.all([discover(HOSTS.illustrator), Path.Path, root, artifacts(HOSTS.illustrator.id, 'jobs')]),
    ([host, path, base, jobs]) => ({ host, scripts: path.join(base, ARTIFACTS, 'apps', 'creative-cloud', 'illustrator-scripts'), jobs }),
);

// --- [DISPATCH] ------------------------------------------------------------------------

const dispatch = <Request extends Schema.Codec<unknown, unknown, never, never>, Response extends Schema.Codec<unknown, unknown, never, never>>(
    at: Site,
    timeoutMs: TimeoutMs,
    script: Script<Request, Response>,
    request: Request['Type'],
): Effect.Effect<Response['Type'], BridgeError, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const job = path.join(at.jobs, script.entry);
        const requestPath = path.join(job, 'request.json');
        const responsePath = path.join(job, 'response.json');
        const encoded = yield* Effect.orDie(Schema.encodeEffect(Schema.fromJsonString(script.request))(request));
        yield* Effect.mapError(Effect.andThen(fs.makeDirectory(job, { recursive: true }), fs.writeFileString(requestPath, encoded)), inaccessible(HOSTS.illustrator.id));
        const answered = yield* Effect.flatMap(
            read(
                HOSTS.illustrator.id,
                at.host.bundleId,
                timeoutMs,
                `do javascript f with arguments {${literal(requestPath)}, ${literal(responsePath)}} show debugger never`,
                Option.some(path.join(at.scripts, `${script.entry}.jsx`)),
            ),
            (reply) => Effect.mapError(Schema.decodeEffect(Schema.NumberFromString)(reply), notDecodable(HOSTS.illustrator.id, reply)),
        );
        const bytes = yield* Effect.mapError(fs.readFile(responsePath), inaccessible(HOSTS.illustrator.id)).pipe(
            Effect.filterOrFail(
                (written) => written.length === answered,
                (written) => notDecodable(HOSTS.illustrator.id, answered)({ written: written.length }),
            ),
        );
        const text = new TextDecoder().decode(bytes);
        const parsed = yield* Effect.mapError(Schema.decodeEffect(Schema.fromJsonString(Schema.Json))(text), notDecodable(HOSTS.illustrator.id, text));
        return yield* Option.match(_envelope(parsed), {
            onNone: () => Effect.mapError(Schema.decodeUnknownEffect(script.response)(parsed), notDecodable(HOSTS.illustrator.id, parsed)),
            onSome: (envelope) =>
                Effect.fail(
                    BridgeError.cases.hostThrew.make({
                        host: HOSTS.illustrator.id,
                        rejection: HostRejection.cases.scriptThrew.make({
                            name: envelope.name,
                            message: envelope.message,
                            stack: Option.none(),
                            line: Option.some(envelope.line),
                            fileName: Option.some(envelope.fileName),
                            number: Option.some(envelope.number),
                            code: envelope.code,
                            tag: envelope.tag,
                        }),
                        autocorrections: Option.none(),
                    }),
                ),
        });
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Script, Site };
export { dispatch, site, Unavailable };
