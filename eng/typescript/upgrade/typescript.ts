import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Cause, Console, Effect, FileSystem, Layer, Path, Predicate, Schema } from 'effect';
import { Command } from 'effect/cli';
import { HttpClient, HttpClientResponse } from 'effect/http';
import nx from 'nx/package.json' with { type: 'json' };
import { CST, parseDocument, Scalar, YAMLMap } from 'yaml';

// --- [MODELS] --------------------------------------------------------------------------

const Upstream = Schema.Struct({ tags: Schema.Struct({ typescript: Schema.Struct({ next: Schema.String }) }) });
const Moves = Schema.fromJsonString(Schema.Array(Schema.Struct({ file: Schema.String, name: Schema.String, from: Schema.String, to: Schema.String })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const scalar = (document: ReturnType<typeof parseDocument>, name: string) => {
    const catalog = document.get('catalog');
    const node = catalog instanceof YAMLMap ? catalog.get(name) : undefined;
    return node instanceof Scalar && typeof node.value === 'string' && Predicate.isNotNullish(node.range) && CST.isScalar(node.srcToken) && node.srcToken.type !== 'block-scalar'
        ? Effect.succeed({ name: `catalog.${name}`, value: node.value, range: node.range, token: node.srcToken })
        : Effect.fail(new Cause.IllegalArgumentError(`pnpm-workspace.yaml requires a string at catalog.${name}`));
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
        const [tsgo, native] = yield* Effect.all([scalar(document, '@effect/tsgo'), scalar(document, '@typescript/native')]);
        const response = yield* HttpClient.get(`https://raw.githubusercontent.com/Effect-TS/tsgo/@effect/tsgo@${tsgo.value}/_packages/tsgo/upstream.json`).pipe(Effect.flatMap(HttpClientResponse.filterStatusOk));
        const { tags } = yield* HttpClientResponse.schemaBodyJson(Upstream)(response);
        const version = `npm:typescript@${tags.typescript.next}`;
        const moves = native.value === version ? [] : [{ file: path.basename(file), name: native.name, from: native.value, to: version }];
        if (moves.length > 0) {
            CST.setScalarValue(native.token, version);
            yield* fs.writeFileString(file, `${text.slice(0, native.range[0])}${native.token.source}${text.slice(native.range[1])}`);
        }
        yield* Effect.flatMap(Schema.encodeEffect(Moves)(moves), Console.log);
    }),
);

if (import.meta.main) {
    Command.run(upgrade, { version: nx.version }).pipe(Effect.provide(Layer.merge(layer, layerUndici)), runMain);
}
