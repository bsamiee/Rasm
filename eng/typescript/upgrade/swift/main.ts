import type { Rule } from '@ast-grep/napi';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Effect, Function, flow, Match, Option, Order, Path, Record, Result, Schema, Struct, Tuple } from 'effect';
import { compare, normalizeFull } from 'verkit';
import { stringify } from 'yaml';
import { accumulate } from '#/eng/typescript/accumulate';
import { decodeFile, XCODE_REQUIREMENT, XcodeProject, XcodeRemote } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { newest } from '#/eng/typescript/upgrade/swift/newest';

// --- [MODELS] --------------------------------------------------------------------------

const Capture = Schema.Struct({ text: Schema.String });
const Major = Schema.Struct({ ruleId: Schema.Literal('major'), metaVariables: Schema.Struct({ single: Schema.Struct({ URL: Capture, FROM: Capture }) }) });
const Other = Schema.Struct({ ruleId: Schema.Literal('other'), text: Schema.String, metaVariables: Schema.Struct({ single: Schema.Struct({ URL: Capture }) }) });
const Matches = Schema.fromJsonString(Schema.Array(Schema.Union([Major, Other])));
const Releases = Schema.fromJsonString(Schema.Record(Schema.String, Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const requirement = (url: string) => (from: string) => `.package(url: "${url}", from: "${from}")`;
const unsupported = (repository: string, stated: readonly string[]) => new Cause.IllegalArgumentError(`${repository} requires ${stated.join(', ')}, upgrade moves ${XCODE_REQUIREMENT} alone`);
const scan = (rules: readonly { readonly id: string; readonly rule: Rule; readonly fix?: string }[], args: readonly string[]) => runCommand('ast-grep', ['scan', '--inline-rules', rules.map((rule) => stringify({ language: 'swiftlang', severity: 'info', ...rule })).join('---\n'), '--custom-languages', 'allow', ...args]);
const project = Effect.fnUntraced(function* (file: string, { packages }: typeof XcodeProject.schema.Type, releases: Record.ReadonlyRecord<string, string>) {
    const updates = yield* Effect.validate(
        packages.filter(Schema.is(XcodeRemote)),
        Effect.fnUntraced(function* ({ repository, version }) {
            const [current, tag] = yield* Effect.all([Effect.fromOption(Option.fromUndefinedOr(version[XCODE_REQUIREMENT]), () => unsupported(repository, Object.keys(version))), Effect.fromOption(Record.get(releases, repository))]);
            return compare(tag, current) > 0 ? Option.some([repository, tag] as const) : Option.none();
        }),
    );
    const moved = Record.fromEntries(Array.getSomes(updates));
    if (Record.isEmptyRecord(moved)) {
        return;
    }
    const encoded = yield* Schema.encodeEffect(Releases)(moved);
    yield* runCommand('yq', ['-i', '-o=json', `env(RELEASES) as $releases | (.packages[] | select(.kind == "remote" and $releases[.repository] != null)) |= (.version."${XCODE_REQUIREMENT}" = $releases[.repository])`, file], { env: { RELEASES: encoded }, extendEnv: true });
    yield* runCommand('xcrun', ['xcprojformatter', '--update', file]);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const path = yield* Path.Path;
    const listed = Array.groupBy(yield* runCommand.records('\0', 'git', ['ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', ':(glob)**/Package.swift', ':(glob)**/project.xcproj']), (file) => path.basename(file));
    const [manifests, projects] = [listed['Package.swift'] ?? [], listed['project.xcproj'] ?? []];
    const major = { pattern: requirement('$URL')('$FROM') };
    const [matches, documents] = yield* Effect.all(
        [
            Array.match(manifests, {
                onEmpty: () => Effect.succeed([]),
                onNonEmpty: (files) =>
                    Effect.flatMap(
                        scan(
                            [
                                { id: 'major', rule: major },
                                { id: 'other', rule: { pattern: '.package(url: "$URL", $$$)', not: major } },
                            ],
                            ['--json=compact', ...files],
                        ),
                        Schema.decodeEffect(Matches),
                    ),
            }),
            Effect.validate(projects, (file) => decodeFile(XcodeProject.schema, file), { concurrency: 'unbounded' }),
        ],
        { concurrency: 'unbounded' },
    );
    const [requirements, refused] = Array.partition(
        matches,
        Match.type<(typeof Matches.Type)[number]>().pipe(
            Match.discriminatorsExhaustive('ruleId')({
                major: ({ metaVariables }) => Result.succeed({ url: metaVariables.single.URL.text, from: metaVariables.single.FROM.text }),
                other: ({ metaVariables, text }) => Result.fail(unsupported(metaVariables.single.URL.text, [text])),
            }),
        ),
    );
    yield* accumulate(refused, Effect.fail);
    const urls = Array.dedupe([...requirements.map(Struct.get('url')), ...documents.flatMap((document) => document.packages.filter(Schema.is(XcodeRemote)).map(Struct.get('repository')))]);
    const releases = Record.fromEntries(Array.zip(urls, yield* Effect.validate(urls, (url) => newest(url, flow(normalizeFull, Option.fromNullOr)), { concurrency: 'unbounded' })));
    const stale = Record.fromEntries(Array.getSomes(requirements.map(({ url, from }) => Option.map(Option.filter(Option.all([Option.fromNullOr(normalizeFull(from)), Record.get(releases, url)]), Function.tupled(Order.isLessThan(compare))), Tuple.evolve([Function.constant(requirement(url)(from)), requirement(url)])))));
    yield* Array.match(Record.toEntries(stale), {
        onEmpty: () => Effect.void,
        onNonEmpty: (moved) =>
            scan(
                moved.map(([pattern, fix], at) => ({ id: `requirement${at}`, rule: { pattern }, fix })),
                ['--update-all', ...manifests],
            ),
    });
    yield* accumulate(Array.zip(projects, documents), ([file, document]) => project(file, document, releases), { concurrency: 'unbounded' });
}).pipe(Effect.provide(layer), runMain);
