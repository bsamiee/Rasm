// --- [IMPORTS] -------------------------------------------------------------------------

import { Effect, FileSystem, Option, Path, type PlatformError, Schema } from 'effect';
import type { ChildProcessSpawner } from 'effect/unstable/process';
import { BridgeError } from '../errors.ts';
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
const _ROOT = ['..', '..', '..', '..'] as const;
const _SCRIPTS = ['.artifacts', 'apps', 'creative-cloud', 'illustrator-scripts'] as const;
const _JOBS = ['.artifacts', 'creative-cloud', 'illustrator', 'jobs'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Envelope = Schema.Struct({
    kind: Schema.Literal('error'),
    name: Schema.String,
    message: Schema.String,
    number: Schema.Number,
    code: Schema.OptionFromOptionalKey(Schema.Number),
    tag: Schema.OptionFromOptionalKey(Schema.String),
    file: Schema.String,
    line: Schema.Number,
});

// --- [SITE] ----------------------------------------------------------------------------

const site: Effect.Effect<Site, never, Path.Path> = Effect.map(Path.Path, (path) => ({
    scripts: path.resolve(import.meta.dirname, ..._ROOT, ..._SCRIPTS),
    jobs: path.resolve(import.meta.dirname, ..._ROOT, ..._JOBS),
}));

// --- [DISPATCH] ------------------------------------------------------------------------

const _inaccessible =
    (path: string) =>
    (error: PlatformError.PlatformError): BridgeError =>
        BridgeError.cases.fileNotAccessible.make({ host: _HOST.id, path, reason: error.reason._tag });

const _notDecodable = (text: string, reason: string): BridgeError => BridgeError.cases.resultNotDecodable.make({ host: _HOST.id, text, reason });

const _thrown = (envelope: (typeof Envelope)['Type']): BridgeError =>
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
        yield* Effect.mapError(fs.makeDirectory(job, { recursive: true }), _inaccessible(job));
        yield* Effect.mapError(fs.writeFileString(requestPath, JSON.stringify(request)), _inaccessible(requestPath));
        const answered = yield* read(
            _HOST.id,
            _HOST.bundleId,
            timeoutMs,
            `do javascript f with arguments {${literal(requestPath)}, ${literal(responsePath)}} show debugger never`,
            Option.some(path.join(at.scripts, `${entry}.jsx`)),
        );
        const written = (yield* Effect.mapError(fs.stat(responsePath), _inaccessible(responsePath))).size.toString();
        if (answered !== written) {
            return yield* Effect.fail(_notDecodable(answered, `response.json holds ${written} bytes`));
        }
        const text = yield* Effect.mapError(fs.readFileString(responsePath), _inaccessible(responsePath));
        const decoded = yield* Effect.mapError(Schema.decodeEffect(Schema.fromJsonString(Schema.Union([schema, Envelope])))(text), (error) => _notDecodable(text, error.message));
        return decoded !== null && typeof decoded === 'object' && 'kind' in decoded && decoded.kind === 'error' ? yield* Effect.fail(_thrown(decoded as (typeof Envelope)['Type'])) : (decoded as S['Type']);
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Site };
export { dispatch, Envelope, site };
