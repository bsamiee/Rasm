import { Buffer } from 'node:buffer';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { workspaceRoot } from '@nx/devkit';
import { XCODE_REQUIREMENT, XcodeProject, XcodeRemote } from '@rasm/applications/xcode';
import { Array, Cause, Console, Effect, FileSystem, flow, Match, Option, Record, Result, Schema, Stream, Struct, Tuple } from 'effect';
import { Command } from 'effect/cli';
import { applyEdits, type Edit, modify } from 'jsonc-parser';
import nx from 'nx/package.json' with { type: 'json' };
import { compare, normalizeFull } from 'verkit';
import { stringify } from 'yaml';
import { accumulate } from '#/eng/typescript/accumulate';
import { Sgconfig } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';

// --- [TYPES] ---------------------------------------------------------------------------

interface Requirement {
    readonly repository: string;
    readonly version: string;
    readonly edits: (version: string) => readonly Edit[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Package.swift';
const MAJOR = { pattern: '.package(url: "$URL", from: "$FROM")' };
const RULES = [
    { id: 'major', rule: MAJOR },
    { id: 'other', rule: { pattern: '.package(url: "$URL", $$$)', not: MAJOR } },
];

// --- [MODELS] --------------------------------------------------------------------------

const Ref = Schema.TemplateLiteralParser([Schema.String, '\trefs/tags/', Schema.String]);
const Capture = Schema.Struct({ text: Schema.String });
const Version = Schema.Struct({ text: Schema.String, range: Schema.Struct({ byteOffset: Schema.Struct({ start: Schema.Int, end: Schema.Int }) }) });
const Major = Schema.Struct({ ruleId: Schema.Literal('major'), metaVariables: Schema.Struct({ single: Schema.Struct({ URL: Capture, FROM: Version }) }) });
const Other = Schema.Struct({ ruleId: Schema.Literal('other'), text: Schema.String, metaVariables: Schema.Struct({ single: Schema.Struct({ URL: Capture }) }) });
const Matches = Schema.fromJsonString(Schema.Array(Schema.Union([Major, Other])));
const Moves = Schema.fromJsonString(Schema.Array(Schema.Struct({ file: Schema.String, name: Schema.String, from: Schema.String, to: Schema.String })));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [REQUIREMENTS]

const unsupported = (file: string, repository: string, requirement: string) => new Cause.IllegalArgumentError(`${file}: ${repository} uses ${requirement}, expected ${file.endsWith(MANIFEST) ? 'from' : XCODE_REQUIREMENT}`);
const manifest = Effect.fnUntraced(function* (language: string, file: string, text: string) {
    const rules = RULES.map((rule) => stringify({ language, severity: 'info', ...rule })).join('---\n');
    const matches = yield* runCommand('ast-grep', ['scan', '--inline-rules', rules, '--custom-languages', 'allow', '--stdin', '--json=compact'], { stdin: Stream.encodeText(Stream.make(text)) }).pipe(Effect.flatMap(Schema.decodeEffect(Matches)));
    const bytes = Buffer.from(text);
    const offset = (at: number) => bytes.subarray(0, at).toString().length;
    return yield* Effect.validate(
        matches,
        Match.type<(typeof Matches.Type)[number]>().pipe(
            Match.discriminatorsExhaustive('ruleId')({
                major: ({
                    metaVariables: {
                        single: { URL, FROM },
                    },
                }) => {
                    const { start, end } = FROM.range.byteOffset;
                    const position = offset(start);
                    const length = offset(end) - position;
                    return Effect.succeed({ repository: URL.text, version: FROM.text, edits: (content: string) => [{ offset: position, length, content }] } satisfies Requirement);
                },
                other: ({
                    metaVariables: {
                        single: { URL },
                    },
                    text: requirement,
                }) => Effect.fail(unsupported(file, URL.text, requirement)),
            }),
        ),
    );
});
const project = Effect.fnUntraced(function* (file: string, text: string) {
    const { packages } = yield* Schema.decodeEffect(XcodeProject.schema)(text);
    const remotes = Array.filterMap(packages, (item, index) => (Schema.is(XcodeRemote)(item) ? Result.succeed({ ...item, index }) : Result.failVoid));
    return yield* Effect.validate(remotes, ({ repository, version, index }) => {
        const stated = Object.keys(version);
        const current = stated.length === 1 ? version[XCODE_REQUIREMENT] : undefined;
        return Effect.map(
            Effect.fromOption(Option.fromUndefinedOr(current), () => unsupported(file, repository, stated.join(', '))),
            (tag) =>
                ({
                    repository,
                    version: tag,
                    edits: (next: string) => modify(text, ['packages', index, 'version', XCODE_REQUIREMENT], next, {}),
                }) satisfies Requirement,
        );
    });
});
const newest = Effect.fn('newest')(function* (url: string) {
    const tags = Array.getSomes(Array.map(yield* runCommand.records('\n', 'git', ['ls-remote', '--tags', '--refs', '--', url]), (line) => Option.flatMap(Schema.decodeUnknownOption(Ref)(line), flow(Tuple.get(2), normalizeFull, Option.fromNullOr))));
    const releases = yield* Effect.fromOption(Option.liftPredicate(tags, Array.isArrayNonEmpty), () => new Cause.NoSuchElementError(`${url} lists no release`));
    return Array.max(releases, compare);
});

// --- [FILES]

const read = Effect.fnUntraced(function* (language: string, file: string) {
    const fs = yield* FileSystem.FileSystem;
    const text = yield* fs.readFileString(file);
    const requirements = yield* file.endsWith(MANIFEST) ? manifest(language, file, text) : project(file, text);
    return { file, text, requirements };
});
const prepare = Effect.fnUntraced(function* ({ file, text, requirements }: Effect.Success<ReturnType<typeof read>>, releases: Readonly<Record<string, string>>) {
    const changes = yield* Effect.validate(requirements, ({ repository, version, edits: replace }) => {
        const tag = releases[repository];
        const current = normalizeFull(version);
        if (tag === undefined || current === null) {
            return Effect.fail(new Cause.IllegalArgumentError(`${file}: invalid release requirement ${version} for ${repository}`));
        }
        return Effect.succeed(compare(tag, current) > 0 ? { edits: replace(tag), moves: [{ file, name: repository, from: version, to: tag }] } : { edits: [], moves: [] });
    });
    const edits = changes.flatMap(Struct.get('edits'));
    if (edits.length === 0) {
        return Option.none();
    }
    const next = applyEdits(text, edits);
    return Option.some({ file, moves: changes.flatMap(Struct.get('moves')), text: file.endsWith(MANIFEST) ? next : yield* runCommand('xcrun', ['xcprojformatter'], { stdin: Stream.encodeText(Stream.make(next)) }) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const upgrade = Command.make(
    'upgrade',
    {},
    Effect.fn('upgrade')(function* () {
        const fs = yield* FileSystem.FileSystem;
        const parallel = { concurrency: 'unbounded' } as const;
        const { customLanguages } = yield* Sgconfig.read(workspaceRoot);
        const [language] = yield* Effect.fromOption(
            Record.findFirst(customLanguages, ({ extensions }) => extensions.includes('swift')),
            () => new Cause.NoSuchElementError(`${Sgconfig.file} declares no Swift grammar`),
        );
        const listed = yield* runCommand.records('\0', 'git', ['ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', `:(glob)**/${MANIFEST}`, `:(glob)**/${XcodeProject.file}`]);
        const documents = yield* Effect.validate(listed, (file) => read(language, file), parallel);
        const repositories = Array.dedupe(documents.flatMap(({ requirements }) => requirements.map(Struct.get('repository'))));
        const releases = Record.fromEntries(Array.zip(repositories, yield* Effect.validate(repositories, newest, parallel)));
        const prepared = Array.getSomes(yield* Effect.validate(documents, (document) => prepare(document, releases), parallel));
        yield* accumulate(prepared, ({ file, text }) => fs.writeFileString(file, text));
        yield* Effect.flatMap(Schema.encodeEffect(Moves)(prepared.flatMap(Struct.get('moves'))), Console.log);
    }),
);

if (import.meta.main) {
    Command.run(upgrade, { version: nx.version }).pipe(Effect.provide(layer), runMain);
}
