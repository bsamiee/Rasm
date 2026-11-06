import { basename, dirname } from 'node:path';
import type { Rule } from '@ast-grep/napi';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Effect, Function, flow, Match, Option, Order, Path, Record, Result, Schema, Struct, Tuple } from 'effect';
import { compare, normalizeFull } from 'verkit';
import { stringify } from 'yaml';
import { accumulate } from '#/eng/typescript/accumulate';
import { decodeFile } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { TextFormat } from '#/eng/typescript/text-format';
import { newest } from '#/eng/typescript/upgrade/swift/newest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MAJOR = 'up-to-next-major-version';

// --- [MODELS] --------------------------------------------------------------------------

const Capture = Schema.Struct({ text: Schema.String });
const Major = Schema.Struct({ ruleId: Schema.Literal('major'), metaVariables: Schema.Struct({ single: Schema.Struct({ URL: Capture, FROM: Capture }) }) });
const Other = Schema.Struct({ ruleId: Schema.Literal('other'), text: Schema.String, metaVariables: Schema.Struct({ single: Schema.Struct({ URL: Capture }) }) });
const Matches = Schema.fromJsonString(Schema.Array(Schema.Union([Major, Other])));
const Remote = Schema.Struct({ kind: Schema.Literal('remote'), repository: Schema.String, version: Schema.StructWithRest(Schema.Struct({ [MAJOR]: Schema.optionalKey(Schema.String) }), [Schema.Record(Schema.String, Schema.Json)]) });
const Member = Schema.Struct({ package: Schema.String, 'product-name': Schema.String, 'build-phase': Schema.Struct({ 'build-phase': Schema.Literal('frameworks') }) });
const Project = TextFormat.Yaml.pipe(Schema.decodeTo(Schema.Struct({ packages: Schema.Array(Schema.Union([Remote, Schema.JsonObject])), targets: Schema.Array(Schema.Struct({ name: Schema.String, 'package-product-members': Schema.Array(Member).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) })) })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const requirement = (url: string) => (from: string) => `.package(url: "${url}", from: "${from}")`;
const unsupported = (repository: string, stated: readonly string[]) => new Cause.IllegalArgumentError(`${repository} requires ${stated.join(', ')}, upgrade moves ${MAJOR} alone`);
const scan = (rules: readonly { readonly id: string; readonly rule: Rule; readonly fix?: string }[], args: readonly string[]) => runCommand('ast-grep', ['scan', '--inline-rules', rules.map((rule) => stringify({ language: 'swiftlang', severity: 'info', ...rule })).join('---\n'), '--custom-languages', 'allow', ...args]);
const project = (file: string, { packages, targets }: typeof Project.Type, releases: Record.ReadonlyRecord<string, string>) => {
    const xcodeproj = (...args: readonly string[]) => runCommand('xcrun', ['xcodeproj', ...args, '--project', dirname(file)]);
    const links = targets.flatMap(({ name, 'package-product-members': members }) => members.map((member) => ({ target: name, product: member['product-name'], identity: member.package.toLowerCase() })));
    return Effect.forEach(
        packages.filter(Schema.is(Remote)),
        Effect.fnUntraced(function* ({ repository, version }: typeof Remote.Type) {
            const identity = basename(repository, '.git').toLowerCase();
            const [current, tag] = yield* Effect.all([Effect.fromOption(Option.fromUndefinedOr(version[MAJOR]), () => unsupported(repository, Object.keys(version))), Effect.fromOption(Record.get(releases, repository))]);
            const remove = xcodeproj('pkg', 'remove', identity);
            const linked = Effect.forEach(
                links.filter((link) => link.identity === identity),
                ({ target, product }) => xcodeproj('pkg', 'link', product, '--package', identity, '--target', target),
                { discard: true },
            );
            const attach = (from: string) => xcodeproj('pkg', 'add', repository, '--from', from).pipe(Effect.andThen(Effect.onError(linked, () => Effect.orDie(remove))));
            return yield* compare(tag, current) > 0 ? remove.pipe(Effect.andThen(Effect.onError(attach(tag), () => Effect.orDie(attach(current))))) : Effect.void;
        }),
        { discard: true },
    ).pipe(Effect.withSpan('project'));
};

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
            Effect.validate(projects, (file) => decodeFile(Project, file), { concurrency: 'unbounded' }),
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
    const urls = Array.dedupe([...requirements.map(Struct.get('url')), ...documents.flatMap((document) => document.packages.filter(Schema.is(Remote)).map(Struct.get('repository')))]);
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
