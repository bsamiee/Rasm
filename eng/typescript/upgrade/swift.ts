import type { Rule } from '@ast-grep/napi';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Effect, FileSystem, Function, flow, Option, Path, Record, Result, Schema, Struct } from 'effect';
import { compare, normalizeFull } from 'verkit';
import { stringify } from 'yaml';
import { accumulate } from '#/eng/typescript/accumulate';
import { Yaml } from '#/eng/typescript/parsed';
import { records, run } from '#/eng/typescript/process';
import { newest } from '#/eng/typescript/upgrade/newest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MAJOR = 'up-to-next-major-version';

// --- [MODELS] --------------------------------------------------------------------------

const Capture = Schema.Struct({ text: Schema.String });
const Major = Schema.Struct({ ruleId: Schema.Literal('major'), metaVariables: Schema.Struct({ single: Schema.Struct({ URL: Capture, FROM: Capture }) }) });
const Other = Schema.Struct({ ruleId: Schema.Literal('other'), text: Schema.String, metaVariables: Schema.Struct({ single: Schema.Struct({ URL: Capture }) }) });
const Matches = Schema.fromJsonString(Schema.Array(Schema.Union([Major, Other])));
const Remote = Schema.Struct({ kind: Schema.Literal('remote'), repository: Schema.String, version: Schema.StructWithRest(Schema.Struct({ [MAJOR]: Schema.optionalKey(Schema.String) }), [Schema.Record(Schema.String, Schema.Json)]) });
const Member = Schema.Struct({ package: Schema.optionalKey(Schema.String), 'product-name': Schema.String, 'build-phase': Schema.Struct({ 'build-phase': Schema.Literal('frameworks') }) });
const Project = Yaml.pipe(Schema.decodeTo(Schema.Struct({ packages: Schema.Array(Schema.Union([Remote, Schema.JsonObject])), targets: Schema.Array(Schema.Struct({ name: Schema.String, 'package-product-members': Schema.Array(Member).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) })) })));

// --- [ERRORS] --------------------------------------------------------------------------

class Unsupported extends Schema.TaggedError<Unsupported, { readonly brand: unique symbol }>()('Unsupported', { repository: Schema.String, requirement: Schema.Array(Schema.String) }) {
    override get message(): string {
        return `${this.repository} requires ${this.requirement.join(', ')}, upgrade moves ${MAJOR} alone`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const requirement = (url: string, from: string) => `.package(url: "${url}", from: "${from}")`;
const scan = (rules: readonly { readonly id: string; readonly rule: Rule; readonly fix?: string }[], args: readonly string[]) => run('ast-grep', ['scan', '--inline-rules', rules.map((rule) => stringify({ language: 'swiftlang', severity: 'info', ...rule })).join('---\n'), '--custom-languages', 'allow', ...args]);
const project = Effect.fn('project')(function* (file: string, { packages, targets }: typeof Project.Type, releases: Record.ReadonlyRecord<string, string>) {
    const path = yield* Path.Path;
    const xcodeproj = (...args: readonly string[]) => run('xcrun', ['xcodeproj', ...args, '--project', path.dirname(file)]);
    const links = targets.flatMap(({ name, 'package-product-members': members }) => members.map((member) => ({ target: name, product: member['product-name'], identity: member.package?.toLowerCase() })));
    const moves = packages.filter(Schema.is(Remote)).flatMap((remote) => Option.toArray(Record.get(releases, remote.repository)).map((tag) => ({ ...remote, tag })));
    yield* Effect.forEach(
        moves,
        ({ repository, version, tag }) => {
            const identity = path.basename(repository, '.git').toLowerCase();
            const remove = xcodeproj('pkg', 'remove', identity);
            const linked = Effect.forEach(
                links.filter((link) => link.identity === identity),
                ({ target, product }) => xcodeproj('pkg', 'link', product, '--package', identity, '--target', target),
                { discard: true },
            );
            const attach = (from: string) => xcodeproj('pkg', 'add', repository, '--from', from).pipe(Effect.andThen(linked.pipe(Effect.onError(Function.constant(Effect.orDie(remove))))));
            return Effect.fromNullishOr(version[MAJOR]).pipe(
                Effect.mapError(() => Unsupported.make({ repository, requirement: Object.keys(version) })),
                Effect.flatMap((current) => (compare(tag, current) === 1 ? remove.pipe(Effect.andThen(attach(tag).pipe(Effect.onError(Function.constant(Effect.orDie(attach(current))))))) : Effect.void)),
            );
        },
        { discard: true },
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const listed = Array.groupBy(yield* records('\0', 'git', ['ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', ':(glob)**/Package.swift', ':(glob)**/project.xcproj']), (file) => path.basename(file));
    const manifests = listed['Package.swift'] ?? [];
    const [matches, documents] = yield* Effect.all(
        [
            Array.match(manifests, {
                onEmpty: () => Effect.succeed([]),
                onNonEmpty: (files) =>
                    scan(
                        [
                            { id: 'major', rule: { pattern: requirement('$URL', '$FROM') } },
                            { id: 'other', rule: { pattern: '.package(url: "$URL", $$$)', not: { pattern: requirement('$URL', '$FROM') } } },
                        ],
                        ['--json=compact', ...files],
                    ).pipe(Effect.flatMap(Schema.decodeEffect(Matches))),
            }),
            Effect.validate(
                listed['project.xcproj'] ?? [],
                (file) =>
                    fs.readFileString(file).pipe(
                        Effect.flatMap(Schema.decodeEffect(Project)),
                        Effect.map((document) => [file, document] as const),
                    ),
                { concurrency: 'unbounded' },
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const [requirements, unsupported] = Array.partition(matches, (match) =>
        match.ruleId === 'major' ? Result.succeed({ url: match.metaVariables.single.URL.text, from: match.metaVariables.single.FROM.text }) : Result.fail(Unsupported.make({ repository: match.metaVariables.single.URL.text, requirement: [match.text] })),
    );
    yield* accumulate(unsupported, Effect.fail);
    const urls = Array.dedupe([...requirements.map(Struct.get('url')), ...documents.flatMap(([, document]) => document.packages.filter(Schema.is(Remote)).map(Struct.get('repository')))]);
    const releases = Record.fromEntries(yield* Effect.validate(urls, (url) => Effect.map(newest(url, flow(normalizeFull, Option.fromNullOr)), (tag) => [url, tag] as const), { concurrency: 'unbounded' }));
    const stale = Record.fromEntries(
        requirements.flatMap(({ url, from }) =>
            Option.toArray(Option.all([Record.get(releases, url), Option.fromNullOr(normalizeFull(from))]))
                .filter(([tag, normal]) => compare(tag, normal) === 1)
                .map(([tag]) => [requirement(url, from), requirement(url, tag)] as const),
        ),
    );
    yield* Array.match(Record.toEntries(stale), {
        onEmpty: () => Effect.void,
        onNonEmpty: (moved) =>
            scan(
                moved.map(([pattern, fix], at) => ({ id: `requirement${at}`, rule: { pattern }, fix })),
                ['--update-all', ...manifests],
            ),
    });
    yield* accumulate(documents, ([file, document]) => project(file, document, releases), { concurrency: 'unbounded' });
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
