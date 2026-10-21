import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Effect, FileSystem, Layer, Option, Record, Schema } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { findMaxSatisfying, isValid } from 'verkit';
import { parse } from 'yaml';
import { stdout } from '../process.ts';
import { Capture, newest, scan, splice } from './bounds.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Catalog = Schema.Struct({
    catalog: Schema.Struct({ '@effect/tsgo': Schema.String, 'tree-sitter-swift': Schema.TemplateLiteralParser(['github:', Schema.String, '#', Schema.String]) }),
    catalogs: Schema.Struct({ docgen: Schema.Struct({ typescript: Schema.TemplateLiteralParser(['npm:typescript@', Schema.String]) }) }),
});
const Upstream = Schema.Struct({ tags: Schema.Struct({ typescript: Schema.Struct({ next: Schema.String }) }) });

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const [fs, client] = yield* Effect.all([FileSystem.FileSystem, HttpClient.HttpClient]);
    const file = 'pnpm-workspace.yaml';
    const { catalog, catalogs } = yield* fs.readFileString(file).pipe(
        Effect.flatMap((text) => Effect.try(() => parse(text))),
        Effect.flatMap(Schema.decodeUnknownEffect(Catalog)),
    );
    const [, repository] = catalog['tree-sitter-swift'];
    const [, docgen] = catalogs.docgen.typescript;
    const [typescript, grammar, compiler] = yield* Effect.all(
        [
            HttpClient.filterStatusOk(client)
                .get(`https://raw.githubusercontent.com/Effect-TS/tsgo/@effect/tsgo@${catalog['@effect/tsgo']}/_packages/tsgo/upstream.json`)
                .pipe(
                    Effect.flatMap(HttpClientResponse.schemaBodyJson(Upstream)),
                    Effect.map(({ tags }) => tags.typescript.next),
                ),
            newest(
                `https://github.com/${repository}`,
                Option.liftPredicate((tag: string) => isValid(tag) && tag.endsWith('-with-generated-files')),
            ),
            stdout('pnpm', ['view', 'typescript', 'versions', '--json']).pipe(
                Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Array(Schema.String)))),
                Effect.flatMap((versions) => Effect.fromNullishOr(findMaxSatisfying(versions, `^${docgen}`))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const rows = {
        typescript: ['catalog', 'typescript', typescript],
        'tree-sitter-swift': ['catalog', 'tree-sitter-swift', `github:${repository}#${grammar}`],
        docgen: ['docgen', 'typescript', `npm:typescript@${compiler}`],
    } as const;
    const found = yield* scan(
        Record.map(rows, ([parent, key]) => `language: yaml, rule: {pattern: "${key}: $VALUE", inside: {kind: block_mapping_pair, has: {field: key, regex: "^${parent}$"}, stopBy: end}}`),
        { VALUE: Capture },
        [file],
    );
    yield* splice(
        file,
        found.map(({ ruleId, metaVariables: { single } }) => [single.VALUE.range.byteOffset, rows[ruleId][2]] as const),
    );
}).pipe(Effect.provide(Layer.merge(layer, FetchHttpClient.layer)), runMain);
