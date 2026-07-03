import { FileSystem, Path } from '@effect/platform';
import { Array, Context, Data, Effect, HashMap, Option, pipe, Schema, String } from 'effect';
import { xxhash128 } from 'hash-wasm';

// --- [TYPES] -----------------------------------------------------------------------------

declare namespace Corpus {
    type Pin = Schema.Schema.Type<typeof _Pin>;
    type Payload = Schema.Schema.Type<typeof _Payload>;
    type Pair = { readonly message: string; readonly bin: Option.Option<string>; readonly json: Option.Option<string> };
    type Registry = HashMap.HashMap<string, Fixture>;
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const _MANIFEST = 'MANIFEST.md';
const _EXTENSIONS = { bin: '.bin', json: '.json' } as const;

// CANONICAL_BYTE_IDENTITY frozen expectation: the single-triangle canonical-adjacency stream and its seed-zero XxHash128 digest.
const _TRIANGLE_HEX = '03000000030000000000000001000000000000000200000001000000020000000100000003000000000000000100000002000000';
const _TRIANGLE_DIGEST = { canonical: '9462a71a5dd13dcfa3b1d6d225fcbe70', memoryLe: '70befc25d2d6b1a3cf3dd15d1aa76294' } as const;

// --- [MODELS] ----------------------------------------------------------------------------

const _Pin = Schema.Literal('REAL', 'DESIGN-PIN');
const _Payload = Schema.Literal('wire-bytes', 'canonical-json', 'digest', 'descriptor-set');

class Fixture extends Schema.Class<Fixture>('Fixture')({
    fixture: Schema.NonEmptyString.pipe(Schema.pattern(/^[A-Z][A-Z0-9_]+$/)),
    seam: Schema.NonEmptyString.pipe(Schema.pattern(/^[a-z][a-z0-9-]*$/)),
    producer: Schema.NonEmptyString,
    payloads: Schema.NonEmptyArray(_Payload),
    pin: _Pin,
}) {}

type Asset = Data.TaggedEnum<{
    Emitted: { readonly fixture: Fixture; readonly pairs: ReadonlyArray<Corpus.Pair> };
    Awaiting: { readonly fixture: Fixture };
    Blocked: { readonly fixture: Fixture };
}>;
const Asset: Data.TaggedEnum.Constructor<Asset> = Data.taggedEnum<Asset>();

// --- [ERRORS] ----------------------------------------------------------------------------

class CorpusFault extends Data.TaggedError('CorpusFault')<{
    readonly reason: 'unreadable' | 'malformed' | 'unregistered';
    readonly detail: string;
}> {}

// --- [SERVICES] --------------------------------------------------------------------------

// Corpus location: defaulted relative to the kit source, overridable at the Layer graph for fixture-tree specs.
class CorpusRoot extends Context.Reference<CorpusRoot>()('rasm-testkit/CorpusRoot', {
    defaultValue: (): string => new URL('../../../contracts', import.meta.url).pathname,
}) {}

// Seed-zero XxHash128: hash-wasm emits the canonical big-endian digest hex directly — live-proven against the frozen triangle vector.
class ContentDigest extends Effect.Service<ContentDigest>()('rasm-testkit/ContentDigest', {
    succeed: {
        x32: (bytes: Uint8Array): Effect.Effect<string> => Effect.promise(() => xxhash128(bytes)),
    },
    accessors: true,
}) {}

// --- [OPERATIONS] ------------------------------------------------------------------------

const _hexBytes = (hex: string): Uint8Array =>
    Uint8Array.from(
        pipe(
            Option.fromNullable(hex.match(/../g)),
            Option.getOrElse((): ReadonlyArray<string> => []),
            Array.map((pair) => Number.parseInt(pair, 16)),
        ),
    );

const _tokens = (cell: string): ReadonlyArray<string> => Array.map([...cell.matchAll(/`([a-z-]+)`/g)], (hit) => hit[1] ?? '');

const _rows = (markdown: string): ReadonlyArray<unknown> =>
    pipe(
        String.split(markdown, '\n'),
        Array.filterMap((line) =>
            pipe(
                Array.map(String.split(line, '|'), (cell) => String.trim(cell).replace(/`/g, '')),
                Option.liftPredicate((cells) => /^[A-Z][A-Z0-9_]+$/.test(Array.get(cells, 2).pipe(Option.getOrElse(() => '')))),
                Option.map((cells) => ({
                    fixture: Array.get(cells, 2).pipe(Option.getOrElse(() => '')),
                    seam: Array.get(cells, 3).pipe(Option.getOrElse(() => '')),
                    producer: Array.get(cells, 4).pipe(Option.getOrElse(() => '')),
                    payloads: _tokens(
                        pipe(
                            String.split(line, '|'),
                            Array.get(5),
                            Option.getOrElse(() => ''),
                        ),
                    ),
                    pin: Array.get(cells, 6).pipe(Option.getOrElse(() => '')),
                })),
            ),
        ),
    );

const _decoded = Schema.decodeUnknown(Schema.NonEmptyArray(Fixture), { errors: 'all' });

const _pairs = (names: ReadonlyArray<string>): ReadonlyArray<Corpus.Pair> =>
    pipe(
        Array.filter(names, (name) => name.endsWith(_EXTENSIONS.bin) || name.endsWith(_EXTENSIONS.json)),
        Array.groupBy((name) => name.replace(/\.(bin|json)$/, '')),
        (grouped) =>
            Array.map(Object.entries(grouped), ([message, files]) => ({
                message,
                bin: Array.findFirst(files, (file) => file.endsWith(_EXTENSIONS.bin)),
                json: Array.findFirst(files, (file) => file.endsWith(_EXTENSIONS.json)),
            })),
    );

const Corpus = {
    // The frozen REAL expectation the TS consumer law reproduces: bytes, canonical digest, and hash-wasm's LE hex twin.
    CANONICAL_BYTE_IDENTITY: {
        bytes: _hexBytes(_TRIANGLE_HEX),
        digest: _TRIANGLE_DIGEST.canonical,
        memoryLe: _TRIANGLE_DIGEST.memoryLe,
    },
    manifest: Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const root = yield* CorpusRoot;
        const raw = yield* Effect.mapError(
            fs.readFileString(path.join(root, _MANIFEST)),
            (fault) => new CorpusFault({ reason: 'unreadable', detail: fault.message }),
        );
        const entries = yield* Effect.mapError(_decoded(_rows(raw)), (fault) => new CorpusFault({ reason: 'malformed', detail: fault.message }));
        return HashMap.fromIterable(Array.map(entries, (entry) => [entry.fixture, entry] as const));
    }),
    resolve: (name: string): Effect.Effect<Asset, CorpusFault, FileSystem.FileSystem | Path.Path> =>
        Effect.gen(function* () {
            const registry = yield* Corpus.manifest;
            const fixture = yield* Effect.mapError(HashMap.get(registry, name), () => new CorpusFault({ reason: 'unregistered', detail: name }));
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const root = yield* CorpusRoot;
            const seam = path.join(root, fixture.seam);
            const emitted = yield* Effect.orElseSucceed(fs.readDirectory(seam), (): ReadonlyArray<string> => []);
            return Array.isNonEmptyReadonlyArray(emitted)
                ? Asset.Emitted({ fixture, pairs: _pairs(emitted) })
                : fixture.pin === 'REAL'
                  ? Asset.Awaiting({ fixture })
                  : Asset.Blocked({ fixture });
        }),
} as const;

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Asset, ContentDigest, Corpus, CorpusFault, CorpusRoot, Fixture };
