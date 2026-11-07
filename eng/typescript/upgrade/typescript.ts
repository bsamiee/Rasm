import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Effect, FileSystem, Layer, Path, Predicate, Schema } from 'effect';
import { HttpClient, HttpClientResponse } from 'effect/http';
import { findMaxSatisfying } from 'verkit';
import { CST, parseDocument, Scalar, YAMLMap } from 'yaml';
import { runCommand } from '#/eng/typescript/run-command';

// --- [MODELS] --------------------------------------------------------------------------

const Docgen = Schema.TemplateLiteralParser(['npm:typescript@', Schema.String]);
const Upstream = Schema.Struct({ tags: Schema.Struct({ typescript: Schema.Struct({ next: Schema.String }) }) });
const Versions = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const scalar = (document: ReturnType<typeof parseDocument>, keys: readonly [string, ...string[]]) => {
    const [key, ...rest] = keys;
    const node = rest.reduce<unknown>((map, member) => (map instanceof YAMLMap ? map.get(member) : undefined), document.get(key));
    if (node instanceof Scalar && typeof node.value === 'string' && Predicate.isNotNullish(node.range) && CST.isScalar(node.srcToken) && node.srcToken.type !== 'block-scalar') {
        return Effect.succeed({ value: node.value, range: node.range, token: node.srcToken });
    }
    return Effect.fail(new Cause.IllegalArgumentError(`pnpm-workspace.yaml requires a string at ${keys.join('.')}`));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const upgrade = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const file = path.resolve('pnpm-workspace.yaml');
    const text = yield* fs.readFileString(file);
    const document = parseDocument(text, { keepSourceTokens: true });
    if (document.errors.length > 0) {
        return yield* new Cause.IllegalArgumentError(document.errors.map(({ message }) => message).join('\n'));
    }
    const edits = yield* Effect.validate(
        [
            [
                ['catalog', 'typescript'],
                () =>
                    Effect.gen(function* () {
                        const { value: tsgo } = yield* scalar(document, ['catalog', '@effect/tsgo']);
                        const response = yield* HttpClient.get(`https://raw.githubusercontent.com/Effect-TS/tsgo/@effect/tsgo@${tsgo}/_packages/tsgo/upstream.json`).pipe(Effect.flatMap(HttpClientResponse.filterStatusOk));
                        const { tags } = yield* HttpClientResponse.schemaBodyJson(Upstream)(response);
                        return tags.typescript.next;
                    }),
            ],
            [
                ['catalogs', 'docgen', 'typescript'],
                (current: string) =>
                    Effect.gen(function* () {
                        const [, installed] = yield* Schema.decodeUnknownEffect(Docgen)(current);
                        const range = `^${installed}`;
                        const versions = yield* runCommand('pnpm', ['view', 'typescript', 'versions', '--json']).pipe(Effect.flatMap(Schema.decodeEffect(Versions)));
                        const version = yield* Effect.fromNullishOr(findMaxSatisfying(versions, range)).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`npm typescript ${range} lists no release`)));
                        return `npm:typescript@${version}`;
                    }),
            ],
        ] as const,
        Effect.fnUntraced(function* ([keys, release]) {
            const row = yield* scalar(document, keys);
            const version = yield* release(row.value);
            return { ...row, version };
        }),
        { concurrency: 'unbounded' },
    );
    const changed = edits.filter(({ value, version }) => value !== version);
    if (!Array.isReadonlyArrayNonEmpty(changed)) {
        return;
    }
    const next = changed
        .sort(({ range: [left] }, { range: [right] }) => right - left)
        .reduce((source, { token, range: [start, end], version }) => {
            CST.setScalarValue(token, version);
            return `${source.slice(0, start)}${token.source}${source.slice(end)}`;
        }, text);
    const [temporary, stat] = yield* Effect.all([fs.makeTempFileScoped({ directory: path.dirname(file) }), fs.stat(file)]);
    yield* fs.writeFileString(temporary, next);
    yield* fs.chmod(temporary, stat.mode);
    yield* fs.rename(temporary, file);
}).pipe(Effect.scoped, Effect.provide(Layer.merge(layer, layerUndici)));

if (import.meta.main) {
    runMain(upgrade);
}
