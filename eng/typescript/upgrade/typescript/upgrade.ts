import { Array, Cause, Effect, FileSystem, Option, Predicate, Schema } from 'effect';
import { HttpClient, HttpClientResponse } from 'effect/http';
import { findMaxSatisfying } from 'verkit';
import { CST, Parser } from 'yaml';
import { Workspace } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { TextFormat } from '#/eng/typescript/text-format';

// --- [MODELS] --------------------------------------------------------------------------

const Catalog = TextFormat.Yaml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            catalog: Schema.Struct({ '@effect/tsgo': Schema.String }),
            catalogs: Schema.Struct({ docgen: Schema.Struct({ typescript: Schema.TemplateLiteralParser(['npm:typescript@', Schema.String]) }) }),
        }),
    ),
);
const Upstream = Schema.Struct({ tags: Schema.Struct({ typescript: Schema.Struct({ next: Schema.String }) }) });
const Versions = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const locate = (token: Option.Option<CST.Token>, key: string) =>
    token.pipe(
        Option.filter(CST.isCollection),
        Option.flatMap((collection) => Array.findFirst(collection.items, (item) => CST.resolveAsScalar(item.key)?.value === key)),
        Option.flatMap((item) => Option.fromNullishOr(item.value)),
    );
const upgrade = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const text = yield* fs.readFileString(Workspace.file);
    const { catalog, catalogs } = yield* Schema.decodeEffect(Catalog)(text);
    const tokens = Array.fromIterable(new Parser().parse(text));
    const document = Option.flatMap(
        Array.findFirst(tokens, (token) => token.type === 'document'),
        (token) => Option.fromNullishOr(token.value),
    );
    const range = `^${catalogs.docgen.typescript[1]}`;
    const [{ tags }, compiler] = yield* Effect.all(
        [
            HttpClient.get(`https://raw.githubusercontent.com/Effect-TS/tsgo/@effect/tsgo@${catalog['@effect/tsgo']}/_packages/tsgo/upstream.json`).pipe(Effect.flatMap(HttpClientResponse.filterStatusOk), Effect.flatMap(HttpClientResponse.schemaBodyJson(Upstream))),
            runCommand('pnpm', ['view', 'typescript', 'versions', '--json']).pipe(
                Effect.flatMap(Schema.decodeEffect(Versions)),
                Effect.map((versions) => findMaxSatisfying(versions, range)),
                Effect.filterOrFail(Predicate.isNotNull, () => new Cause.NoSuchElementError(`npm typescript ${range} lists no release`)),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const rows = [
        [['catalog', 'typescript'], tags.typescript.next],
        [['catalogs', 'docgen', 'typescript'], `npm:typescript@${compiler}`],
    ] as const;
    yield* Effect.forEach(
        rows,
        Effect.fnUntraced(function* ([path, value]) {
            const token = yield* Effect.fromOption(path.reduce(locate, document));
            CST.setScalarValue(token, value);
        }),
        { discard: true },
    );
    yield* fs.writeFileString(Workspace.file, tokens.map(CST.stringify).join(''));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { upgrade };
