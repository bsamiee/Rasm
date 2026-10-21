import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { readJsonFile } from '@nx/devkit';
import { Array, Effect, FileSystem, flow, Option, Path, Record, Schema, String } from 'effect';
import { normalizeFull } from 'verkit';
import { stdout } from '../process.ts';
import { Capture, newest, scan, splice } from './bounds.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Remote = Schema.StructWithRest(Schema.Struct({ kind: Schema.Literal('remote'), repository: Schema.String, version: Schema.Struct({ 'up-to-next-major-version': Schema.String }) }), [Schema.JsonObject]);
const Project = Schema.StructWithRest(Schema.Struct({ packages: Schema.Array(Schema.Union([Remote, Schema.JsonObject])) }), [Schema.JsonObject]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = Effect.fn('project')(function* (file: string, document: typeof Project.Type, tags: Record.ReadonlyRecord<string, string>) {
    const fs = yield* FileSystem.FileSystem;
    const remote = Schema.is(Remote);
    yield* fs.writeFileString(
        file,
        yield* Schema.encodeEffect(Schema.fromJsonString(Project))({
            ...document,
            packages: document.packages.map((row) => (remote(row) ? { ...row, version: { 'up-to-next-major-version': Option.getOrThrow(Record.get(tags, row.repository)) } } : row)),
        }),
    );
    yield* stdout('xcrun', ['xcprojformatter', '--update', file]);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const path = yield* Path.Path;
    const listed = Array.groupBy((yield* stdout('git', ['ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', ':(glob)**/Package.swift', ':(glob)**/project.xcproj'])).split('\0').filter(String.isNonEmpty), (file) => path.basename(file));
    const [requirements, documents] = yield* Effect.all(
        [
            Array.match(listed['Package.swift'] ?? [], {
                onEmpty: () => Effect.succeed([]),
                onNonEmpty: (files) => scan({ requirement: 'language: swiftlang, rule: {pattern: \'.package(url: "$URL", from: "$FROM")\'}' }, { URL: Capture, FROM: Capture }, ['--custom-languages', 'allow', ...files]),
            }),
            Effect.validate(
                listed['project.xcproj'] ?? [],
                (file) =>
                    Effect.try(() => readJsonFile(file)).pipe(
                        Effect.flatMap(Schema.decodeUnknownEffect(Project)),
                        Effect.map((document) => [file, document] as const),
                    ),
                { concurrency: 'unbounded' },
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const urls = Array.dedupe([...requirements.map(({ metaVariables }) => metaVariables.single.URL.text), ...documents.flatMap(([, document]) => document.packages.filter(Schema.is(Remote)).map(({ repository }) => repository))]);
    const tags = Object.fromEntries(yield* Effect.validate(urls, (url) => Effect.map(newest(url, flow(normalizeFull, Option.fromNullOr)), (tag) => [url, tag] as const), { concurrency: 'unbounded' }));
    const edits = requirements.map(({ file, metaVariables: { single } }) => ({ file, span: single.FROM.range.byteOffset, tag: Option.getOrThrow(Record.get(tags, single.URL.text)) }));
    yield* Effect.forEach(
        Object.entries(Array.groupBy(edits, ({ file }) => file)),
        ([file, found]) =>
            splice(
                file,
                found.map(({ span, tag }) => [span, tag] as const),
            ),
        { discard: true },
    );
    yield* Effect.forEach(documents, ([file, document]) => project(file, document, tags), { discard: true });
}).pipe(Effect.provide(layer), runMain);
