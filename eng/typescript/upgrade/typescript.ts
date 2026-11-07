import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Console, Effect, FileSystem, Layer, Path, Predicate, Result, Schema } from 'effect';
import { Command } from 'effect/cli';
import { HttpClient, HttpClientResponse } from 'effect/http';
import nx from 'nx/package.json' with { type: 'json' };
import { findMaxSatisfying } from 'verkit';
import { CST, parseDocument, Scalar, YAMLMap } from 'yaml';
import { runCommand } from '#/run-command.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Aliased = Schema.TemplateLiteralParser(['npm:typescript@', Schema.String]);
const Upstream = Schema.Struct({ tags: Schema.Struct({ typescript: Schema.Struct({ next: Schema.String }) }) });
const Versions = Schema.fromJsonString(Schema.Array(Schema.String));
const Moves = Schema.fromJsonString(Schema.Array(Schema.Struct({ file: Schema.String, name: Schema.String, from: Schema.String, to: Schema.String })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const scalar = (document: ReturnType<typeof parseDocument>, keys: readonly [string, ...string[]]) => {
    const [key, ...rest] = keys;
    const node = rest.reduce<unknown>((map, member) => (map instanceof YAMLMap ? map.get(member) : undefined), document.get(key));
    if (node instanceof Scalar && typeof node.value === 'string' && Predicate.isNotNullish(node.range) && CST.isScalar(node.srcToken) && node.srcToken.type !== 'block-scalar') {
        return Effect.succeed({ name: keys.join('.'), value: node.value, range: node.range, token: node.srcToken });
    }
    return Effect.fail(new Cause.IllegalArgumentError(`pnpm-workspace.yaml requires a string at ${keys.join('.')}`));
};
const names = (document: ReturnType<typeof parseDocument>, key: string) => {
    const node = document.get(key);
    return node instanceof YAMLMap ? Array.filterMap(node.pairs(), ({ key: name }) => (name instanceof Scalar && typeof name.value === 'string' ? Result.succeed(name.value) : Result.failVoid)) : [];
};
const newestWithin = <E, R>(versions: Effect.Effect<readonly string[], E, R>, installed: string) => {
    const range = `^${installed}`;
    return Effect.flatMap(versions, (held) => Effect.fromNullishOr(findMaxSatisfying(held, range)).pipe(Effect.mapBoth({ onFailure: () => new Cause.NoSuchElementError(`npm typescript ${range} lists no release`), onSuccess: (version) => `npm:typescript@${version}` })));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const upgrade = Command.make(
    'upgrade',
    {},
    Effect.fn('upgrade')(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const file = path.resolve('pnpm-workspace.yaml');
        const text = yield* fs.readFileString(file);
        const document = parseDocument(text, { keepSourceTokens: true });
        if (document.errors.length > 0) {
            return yield* new Cause.IllegalArgumentError(document.errors.map(({ message }) => message).join('\n'));
        }
        const versions = yield* Effect.cached(runCommand('pnpm', ['view', 'typescript', 'versions', '--json']).pipe(Effect.flatMap(Schema.decodeEffect(Versions))));
        const upstream = Effect.gen(function* () {
            const { value: tsgo } = yield* scalar(document, ['catalog', '@effect/tsgo']);
            const response = yield* HttpClient.get(`https://raw.githubusercontent.com/Effect-TS/tsgo/@effect/tsgo@${tsgo}/_packages/tsgo/upstream.json`).pipe(Effect.flatMap(HttpClientResponse.filterStatusOk));
            const { tags } = yield* HttpClientResponse.schemaBodyJson(Upstream)(response);
            return tags.typescript.next;
        });
        const aliased = yield* Effect.forEach(
            names(document, 'catalogs'),
            Effect.fnUntraced(function* (name: string) {
                const row = yield* scalar(document, ['catalogs', name, 'typescript']);
                const [, installed] = yield* Schema.decodeUnknownEffect(Aliased)(row.value);
                return { ...row, release: newestWithin(versions, installed) };
            }, Effect.option),
        );
        const rows = [{ ...(yield* scalar(document, ['catalog', 'typescript'])), release: upstream }, ...Array.getSomes(aliased)];
        const edits = yield* Effect.validate(
            rows,
            Effect.fnUntraced(function* ({ release, ...row }) {
                const version = yield* release;
                return { ...row, version };
            }),
            { concurrency: 'unbounded' },
        );
        const changed = edits.filter(({ value, version }) => value !== version);
        const next = changed
            .sort(({ range: [left] }, { range: [right] }) => right - left)
            .reduce((source, { token, range: [start, end], version }) => {
                CST.setScalarValue(token, version);
                return `${source.slice(0, start)}${token.source}${source.slice(end)}`;
            }, text);
        if (Array.isReadonlyArrayNonEmpty(changed)) {
            yield* fs.writeFileString(file, next);
        }
        yield* Effect.flatMap(Schema.encodeEffect(Moves)(changed.map(({ name, value, version }) => ({ file: path.basename(file), name, from: value, to: version }))), Console.log);
    }),
);

if (import.meta.main) {
    Command.run(upgrade, { version: nx.version }).pipe(Effect.provide(Layer.merge(layer, layerUndici)), runMain);
}
