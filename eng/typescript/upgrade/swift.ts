import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { readJsonFile } from '@nx/devkit';
import { Array, Effect, flow, Option, Path, Record, Schema, String } from 'effect';
import { normalizeFull } from 'verkit';
import { stdout } from '#/eng/typescript/process';
import { Capture, newest, scan, splice } from '#/eng/typescript/upgrade/bounds';

// --- [MODELS] --------------------------------------------------------------------------

const Remote = Schema.Struct({ kind: Schema.Literal('remote'), repository: Schema.String, version: Schema.Struct({ 'up-to-next-major-version': Schema.String }) });
const Member = Schema.Struct({ package: Schema.optionalKey(Schema.String), 'product-name': Schema.String, 'build-phase': Schema.Struct({ 'build-phase': Schema.Literal('frameworks') }) });
const Project = Schema.Struct({
    packages: Schema.Array(Schema.Union([Remote, Schema.JsonObject])),
    targets: Schema.Array(Schema.Struct({ name: Schema.String, 'package-product-members': Schema.Array(Member).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = Effect.fn('project')(function* (file: string, { packages, targets }: typeof Project.Type, tags: Record.ReadonlyRecord<string, string>) {
    const path = yield* Path.Path;
    const links = targets.flatMap(({ name, 'package-product-members': members }) => members.map((member) => ({ target: name, product: member['product-name'], identity: member.package })));
    const commands = packages.filter(Schema.is(Remote)).flatMap(({ repository }) => {
        const identity = path.basename(repository, '.git');
        return [['pkg', 'remove', identity], ['pkg', 'add', repository, '--from', Option.getOrThrow(Record.get(tags, repository))], ...links.filter((link) => link.identity === identity).map(({ target, product }) => ['pkg', 'link', product, '--package', identity, '--target', target])];
    });
    yield* Effect.forEach(commands, (args) => stdout('xcrun', ['xcodeproj', ...args, '--project', path.dirname(file)]), { discard: true });
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
