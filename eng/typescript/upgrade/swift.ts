import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { readJsonFile } from '@nx/devkit';
import { Array, Effect, FileSystem, flow, Option, Path, Record, Schema, String } from 'effect';
import { isValid, normalizeFull } from 'verkit';
import { stdout } from '../process.ts';
import { Capture, newest, splice } from './bounds.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Requirements = Schema.fromJsonString(Schema.Array(Schema.Struct({ file: Schema.String, metaVariables: Schema.Struct({ single: Schema.Struct({ URL: Capture, FROM: Capture }) }) })));
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
            packages: document.packages.map((row) => (remote(row) ? { ...row, version: { 'up-to-next-major-version': Option.getOrElse(Record.get(tags, row.repository), () => row.version['up-to-next-major-version']) } } : row)),
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
                onNonEmpty: (files) => Effect.flatMap(stdout('ast-grep', ['scan', '--custom-languages', 'allow', '--inline-rules', '{id: requirement, language: swiftlang, severity: info, rule: {pattern: \'.package(url: "$URL", from: "$FROM")\'}}', '--json=compact', ...files]), Schema.decodeEffect(Requirements)),
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
    const tags = Record.getSomes(Object.fromEntries(yield* Effect.validate(urls, (url) => Effect.map(newest(url, isValid), (tag) => [url, Option.flatMap(tag, flow(normalizeFull, Option.fromNullOr))] as const), { concurrency: 'unbounded' })));
    const edits = requirements.flatMap(({ file, metaVariables: { single } }) => Option.toArray(Record.get(tags, single.URL.text)).map((tag) => ({ file, span: single.FROM.range.byteOffset, tag })));
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
