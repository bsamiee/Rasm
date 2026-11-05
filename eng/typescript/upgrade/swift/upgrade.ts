import { basename, dirname, extname } from 'node:path';
import { parse, registerDynamicLanguage, type SgNode } from '@ast-grep/napi';
import { Array, Cause, Effect, FileSystem, Function, Option, Order, Record, Result, Schema, Struct } from 'effect';
import { compare, normalizeFull } from 'verkit';
import { accumulate } from '#/eng/typescript/accumulate';
import { decodeFile, Sgconfig, XCODE_REQUIREMENT, XcodeProject, XcodeRemote } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REQUIREMENT = { rule: { any: [{ pattern: '.package(url: "$URL", from: "$FROM")' }, { pattern: '.package(url: "$URL", $$$)' }] } };

// --- [MODELS] --------------------------------------------------------------------------

const Ref = Schema.TemplateLiteralParser([Schema.String, '\trefs/tags/', Schema.String]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const released = (line: string) => Option.flatMap(Schema.decodeUnknownOption(Ref)(line), ([, , tag]) => Option.fromNullOr(normalizeFull(tag)));
const newest = Effect.fn('newest')(function* (url: string) {
    const tags = Array.getSomes(Array.map(yield* runCommand.records('\n', 'git', ['ls-remote', '--tags', '--refs', url]), released));
    return Array.max(yield* Effect.fromOption(Option.liftPredicate(tags, Array.isArrayNonEmpty), () => new Cause.NoSuchElementError(`${url} lists no release`)), compare);
});
const unsupported = (repository: string, stated: readonly string[]) => new Cause.IllegalArgumentError(`${repository} requires ${stated.join(', ')}, upgrade moves ${XCODE_REQUIREMENT} alone`);
const declared = (node: SgNode) => {
    const url = node.getMatch('URL')?.text() ?? node.text();
    return Result.fromOption(
        Option.map(Option.fromNullOr(node.getMatch('FROM')), (from) => ({ url, from })),
        () => unsupported(url, [node.text()]),
    );
};
const moved =
    (releases: Record.ReadonlyRecord<string, string>) =>
    ({ url, from }: { readonly url: string; readonly from: SgNode }) =>
        Option.map(Option.filter(Option.all([Option.fromNullOr(normalizeFull(from.text())), Record.get(releases, url)]), Function.tupled(Order.isLessThan(compare))), ([, tag]) => from.replace(tag));
const project = (file: string, { packages, targets }: typeof XcodeProject.schema.Type, releases: Record.ReadonlyRecord<string, string>) => {
    const xcodeproj = (...args: readonly string[]) => runCommand('xcrun', ['xcodeproj', ...args, '--project', dirname(file)]);
    const links = targets.flatMap(({ name, members }) => members.map(({ product, package: owner }) => ({ target: name, product, identity: owner.toLowerCase() })));
    return Effect.forEach(
        packages.filter(Schema.is(XcodeRemote)),
        Effect.fnUntraced(function* ({ repository, version }: typeof XcodeRemote.Type) {
            const identity = basename(repository, '.git').toLowerCase();
            const [current, tag] = yield* Effect.all([Effect.fromOption(Option.fromUndefinedOr(version[XCODE_REQUIREMENT]), () => unsupported(repository, Object.keys(version))), Effect.fromOption(Record.get(releases, repository))]);
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
const upgrade = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const manifest = 'Package.swift';
    const listed = Array.groupBy(yield* runCommand.records('\0', 'git', ['ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', `:(glob)**/${manifest}`, `:(glob)**/${XcodeProject.file}`]), basename);
    const [manifests, projects] = [listed[manifest] ?? [], listed[XcodeProject.file] ?? []];
    const { customLanguages } = yield* Sgconfig.read('.');
    const [language, registration] = yield* Effect.fromOption(Record.findFirst(customLanguages, ({ extensions }) => extensions.includes(extname(manifest).slice(1))));
    yield* Effect.sync(() => registerDynamicLanguage({ [language]: { ...registration, extensions: [...registration.extensions] } }));
    const [parsed, documents] = yield* Effect.all(
        [
            Effect.forEach(
                manifests,
                Effect.fnUntraced(function* (file: string) {
                    const root = parse(language, yield* fs.readFileString(file)).root();
                    return { file, root, rows: Array.partition(root.findAll(REQUIREMENT), declared) };
                }),
            ),
            Effect.validate(projects, (file) => decodeFile(XcodeProject.schema, file), { concurrency: 'unbounded' }),
        ],
        { concurrency: 'unbounded' },
    );
    yield* accumulate(
        parsed.flatMap(({ rows: [, refused] }) => refused),
        Effect.fail,
    );
    const urls = Array.dedupe([...parsed.flatMap(({ rows: [requirements] }) => requirements.map(Struct.get('url'))), ...documents.flatMap((document) => document.packages.filter(Schema.is(XcodeRemote)).map(Struct.get('repository')))]);
    const releases = Record.fromEntries(Array.zip(urls, yield* Effect.validate(urls, newest, { concurrency: 'unbounded' })));
    yield* Effect.forEach(
        parsed,
        ({ file, root, rows: [requirements] }) =>
            Array.match(Array.getSomes(requirements.map(moved(releases))), {
                onEmpty: () => Effect.void,
                onNonEmpty: (edits) => fs.writeFileString(file, root.commitEdits(edits)),
            }),
        { discard: true },
    );
    yield* accumulate(Array.zip(projects, documents), ([file, document]) => project(file, document, releases), { concurrency: 'unbounded' });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { upgrade };
