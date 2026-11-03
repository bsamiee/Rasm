import type { Rule } from '@ast-grep/napi';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Effect, FileSystem, flow, Option, Path, Record, Result, Schema, Struct } from 'effect';
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
const Member = Schema.Struct({ package: Schema.String, 'product-name': Schema.String, 'build-phase': Schema.Struct({ 'build-phase': Schema.Literal('frameworks') }) });
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
    const links = targets.flatMap(({ name, 'package-product-members': members }) => members.map((member) => ({ target: name, product: member['product-name'], identity: member.package.toLowerCase() })));
    yield* Effect.forEach(
        packages.filter(Schema.is(Remote)),
        Effect.fnUntraced(function* ({ repository, version }: typeof Remote.Type) {
            const identity = path.basename(repository, '.git').toLowerCase();
            const [current, tag] = yield* Effect.all([Effect.fromOption(Option.fromUndefinedOr(version[MAJOR]), () => Unsupported.make({ repository, requirement: Object.keys(version) })), Effect.fromOption(Record.get(releases, repository))]);
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
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const listed = Array.groupBy(yield* records('\0', 'git', ['ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', ':(glob)**/Package.swift', ':(glob)**/project.xcproj']), (file) => path.basename(file));
    const [manifests, projects] = [listed['Package.swift'] ?? [], listed['project.xcproj'] ?? []];
    const major = { pattern: requirement('$URL', '$FROM') };
    const [matches, documents] = yield* Effect.all(
        [
            Array.match(manifests, { onEmpty: () => Effect.succeed([]), onNonEmpty: (files) => Effect.flatMap(scan([{ id: 'major', rule: major }, { id: 'other', rule: { pattern: '.package(url: "$URL", $$$)', not: major } }], ['--json=compact', ...files]), Schema.decodeEffect(Matches)) }),
            Effect.validate(projects, (file) => Effect.flatMap(fs.readFileString(file), Schema.decodeEffect(Project)), { concurrency: 'unbounded' }),
        ],
        { concurrency: 'unbounded' },
    );
    const [requirements, unsupported] = Array.partition(matches, (match) =>
        match.ruleId === 'major' ? Result.succeed({ url: match.metaVariables.single.URL.text, from: match.metaVariables.single.FROM.text }) : Result.fail(Unsupported.make({ repository: match.metaVariables.single.URL.text, requirement: [match.text] })),
    );
    yield* accumulate(unsupported, Effect.fail);
    const urls = Array.dedupe([...requirements.map(Struct.get('url')), ...documents.flatMap((document) => document.packages.filter(Schema.is(Remote)).map(Struct.get('repository')))]);
    const releases = Record.fromEntries(yield* Effect.validate(urls, (url) => Effect.map(newest(url, flow(normalizeFull, Option.fromNullOr)), (tag) => [url, tag] as const), { concurrency: 'unbounded' }));
    const stale = Record.fromEntries(Array.getSomes(requirements.map(({ url, from }) => Option.flatMap(Option.all([Record.get(releases, url), Option.fromNullOr(normalizeFull(from))]), ([tag, held]) => (compare(tag, held) > 0 ? Option.some([requirement(url, from), requirement(url, tag)] as const) : Option.none())))));
    yield* Array.match(Record.toEntries(stale), {
        onEmpty: () => Effect.void,
        onNonEmpty: (moved) =>
            scan(
                moved.map(([pattern, fix], at) => ({ id: `requirement${at}`, rule: { pattern }, fix })),
                ['--update-all', ...manifests],
            ),
    });
    yield* accumulate(Array.zip(projects, documents), ([file, document]) => project(file, document, releases), { concurrency: 'unbounded' });
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
