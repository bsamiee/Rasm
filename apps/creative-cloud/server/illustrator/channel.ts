// --- [IMPORTS] -------------------------------------------------------------------------

import { Effect, FileSystem, Option, Path, Schema } from 'effect';
import type { ChildProcessSpawner } from 'effect/unstable/process';
import { BridgeError, inaccessible, notDecodable } from '../errors.ts';
import { artifacts } from '../jobs.ts';
import { literal, read } from '../osascript.ts';
import { HOSTS } from '../values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Site {
    readonly scripts: string;
    readonly jobs: string;
}

type Services = ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST = HOSTS.illustrator;
const _SCRIPTS = ['..', '..', '..', '..', '.artifacts', 'apps', 'creative-cloud', 'illustrator-scripts'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const _envelope = Schema.decodeUnknownOption(
    Schema.Struct({
        kind: Schema.Literal('error'),
        name: Schema.String,
        message: Schema.String,
        number: Schema.Number,
        code: Schema.OptionFromOptionalKey(Schema.Number),
        tag: Schema.OptionFromOptionalKey(Schema.String),
        file: Schema.String,
        line: Schema.Number,
    }),
);

const _json = Schema.decodeEffect(Schema.fromJsonString(Schema.Json));

// --- [SITE] ----------------------------------------------------------------------------

const site: Effect.Effect<Site, never, Path.Path> = Effect.map(Path.Path, (path) => ({ scripts: path.resolve(import.meta.dirname, ..._SCRIPTS), jobs: artifacts(path, _HOST.id, 'jobs') }));

// --- [DISPATCH] ------------------------------------------------------------------------

const _thrown = (envelope: Option.Option.Value<ReturnType<typeof _envelope>>): BridgeError =>
    BridgeError.cases.hostThrew.make({
        host: _HOST.id,
        rejection: {
            _tag: 'scriptThrew',
            name: envelope.name,
            message: envelope.message,
            stack: Option.none(),
            line: Option.some(envelope.line),
            fileName: Option.some(envelope.file),
            number: Option.some(envelope.number),
        },
        autocorrections: Option.none(),
    });

const dispatch = <S extends Schema.Codec<unknown, unknown, never, never>>(
    at: Site,
    timeoutMs: number,
    entry: string,
    jobId: string,
    request: Schema.Json,
    schema: S,
): Effect.Effect<S['Type'], BridgeError, Services> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const job = path.join(at.jobs, jobId);
        const requestPath = path.join(job, 'request.json');
        const responsePath = path.join(job, 'response.json');
        yield* Effect.mapError(Effect.andThen(fs.makeDirectory(job, { recursive: true }), fs.writeFileString(requestPath, JSON.stringify(request))), inaccessible(_HOST.id));
        const answered = yield* read(
            _HOST.id,
            _HOST.bundleId,
            timeoutMs,
            `do javascript f with arguments {${literal(requestPath)}, ${literal(responsePath)}} show debugger never`,
            Option.some(path.join(at.scripts, `${entry}.jsx`)),
        );
        const bytes = yield* Effect.mapError(fs.readFile(responsePath), inaccessible(_HOST.id)).pipe(
            Effect.filterOrFail(
                (written) => written.length.toString() === answered,
                (written) => notDecodable(_HOST.id, answered)({ written: written.length }),
            ),
        );
        const text = new TextDecoder().decode(bytes);
        const parsed = yield* Effect.mapError(_json(text), notDecodable(_HOST.id, text));
        return yield* Effect.mapError(Schema.decodeUnknownEffect(schema)(parsed), (cause) => Option.match(_envelope(parsed), { onNone: () => notDecodable(_HOST.id, parsed)(cause), onSome: _thrown }));
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Site };
export { dispatch, site };
