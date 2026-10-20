import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Effect, FileSystem, Layer, Option, Record, Schema } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { findMaxSatisfying, isValid } from 'verkit';
import { parse } from 'yaml';
import { stdout } from '../process.ts';
import { Capture, newest, splice } from './bounds.ts';

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
            Effect.flatMap(
                newest(`https://github.com/${repository}`, (tag) => isValid(tag) && tag.endsWith('-with-generated-files')),
                Effect.fromOption,
            ),
            stdout('pnpm', ['view', 'typescript', 'versions', '--json']).pipe(
                Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Array(Schema.String)))),
                Effect.flatMap((versions) => Effect.fromOption(Option.fromNullOr(findMaxSatisfying(versions, `^${docgen}`)))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const rows = {
        typescript: ['catalog', 'typescript', typescript],
        'tree-sitter-swift': ['catalog', 'tree-sitter-swift', `github:${repository}#${grammar}`],
        docgen: ['docgen', 'typescript', `npm:typescript@${compiler}`],
    } as const;
    const found = yield* Effect.flatMap(
        stdout('ast-grep', [
            'scan',
            '--inline-rules',
            Object.entries(rows)
                .map(([id, [parent, key]]) => `{id: ${id}, language: yaml, severity: info, rule: {pattern: "${key}: $VALUE", inside: {kind: block_mapping_pair, has: {field: key, regex: "^${parent}$"}, stopBy: end}}}`)
                .join('\n---\n'),
            '--json=compact',
            file,
        ]),
        Schema.decodeEffect(Schema.fromJsonString(Schema.Array(Schema.Struct({ ruleId: Schema.Literals(Record.keys(rows)), metaVariables: Schema.Struct({ single: Schema.Struct({ VALUE: Capture }) }) })))),
    );
    yield* splice(
        file,
        found.map(({ ruleId, metaVariables: { single } }) => [single.VALUE.range.byteOffset, rows[ruleId][2]] as const),
    );
}).pipe(Effect.provide(Layer.merge(layer, FetchHttpClient.layer)), runMain);
