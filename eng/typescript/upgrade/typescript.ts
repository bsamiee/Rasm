import { NodeHttpClient, NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Effect, FileSystem, Layer, Option, Predicate, Schema, String } from 'effect';
import { HttpClient, HttpClientResponse } from 'effect/http';
import { findMaxSatisfying, isValid } from 'verkit';
import { CST, Parser } from 'yaml';
import { Yaml } from '#/eng/typescript/parsed';
import { run } from '#/eng/typescript/process';
import { newest } from '#/eng/typescript/upgrade/newest';

// --- [MODELS] --------------------------------------------------------------------------

const Catalog = Yaml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            catalog: Schema.Struct({ '@effect/tsgo': Schema.String, 'tree-sitter-swift': Schema.TemplateLiteralParser(['github:', Schema.String, '#', Schema.String]) }),
            catalogs: Schema.Struct({ docgen: Schema.Struct({ typescript: Schema.TemplateLiteralParser(['npm:typescript@', Schema.String]) }) }),
        }),
    ),
);
const Upstream = Schema.Struct({ tags: Schema.Struct({ typescript: Schema.Struct({ next: Schema.String }) }) });
const Versions = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const locate = (token: CST.Token | undefined, key: string) => (CST.isCollection(token) ? token.items.find((item) => CST.resolveAsScalar(item.key)?.value === key)?.value : undefined);

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const file = 'pnpm-workspace.yaml';
    const text = yield* fs.readFileString(file);
    const { catalog, catalogs } = yield* Schema.decodeEffect(Catalog)(text);
    const tokens = Array.from(new Parser().parse(text));
    const document = tokens.find((token) => token.type === 'document')?.value;
    const [, repository] = catalog['tree-sitter-swift'];
    const range = `^${catalogs.docgen.typescript[1]}`;
    const [{ tags }, grammar, compiler] = yield* Effect.all(
        [
            HttpClient.get(`https://raw.githubusercontent.com/Effect-TS/tsgo/@effect/tsgo@${catalog['@effect/tsgo']}/_packages/tsgo/upstream.json`).pipe(Effect.flatMap(HttpClientResponse.filterStatusOk), Effect.flatMap(HttpClientResponse.schemaBodyJson(Upstream))),
            newest(`https://github.com/${repository}`, Option.liftPredicate(Predicate.and(isValid, String.endsWith('-with-generated-files')))),
            run('pnpm', ['view', 'typescript', 'versions', '--json']).pipe(
                Effect.flatMap(Schema.decodeEffect(Versions)),
                Effect.flatMap((versions) => Effect.fromOption(Option.fromNullishOr(findMaxSatisfying(versions, range)), () => new Cause.NoSuchElementError(`npm typescript ${range} lists no release`))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const rows = [
        [['catalog', 'typescript'], tags.typescript.next],
        [['catalog', 'tree-sitter-swift'], `github:${repository}#${grammar}`],
        [['catalogs', 'docgen', 'typescript'], `npm:typescript@${compiler}`],
    ] as const;
    yield* Effect.forEach(rows, ([path, value]) => Effect.map(Effect.fromNullishOr(path.reduce<CST.Token | undefined>(locate, document)), (token) => CST.setScalarValue(token, value)), { discard: true });
    yield* fs.writeFileString(file, tokens.map(CST.stringify).join(''));
}).pipe(Effect.provide(Layer.merge(NodeServices.layer, NodeHttpClient.layerUndici)), NodeRuntime.runMain);
