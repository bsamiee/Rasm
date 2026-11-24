import { createRequire } from 'node:module';
import { dirname, isAbsolute, join, relative } from 'node:path';
import { detectPackageManager } from '@nx/devkit';
import { dried, executor, spawned } from '@rasm/nx';
import { Array, Boolean, Cause, DateTime, Effect, FileSystem, Match, Option, Order, Predicate, Record, Result, Schema, SemVer } from 'effect';
import { CommandLine } from 'effect/process';
import npa from 'npm-package-arg';
import { packageRegistryView, parseRegistryViewJson } from 'nx/src/utils/package-manager';
import { Composer, CST, Parser, Scalar, YAMLMap } from 'yaml';
import { MANAGERS } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SECTIONS = [['catalog'], ['catalogs', '*'], ['workspaces', 'catalog'], ['workspaces', 'catalogs', '*'], ['dependencies'], ['devDependencies'], ['optionalDependencies']] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Upgrade = Schema.Struct({
    catalogs: Schema.Array(Schema.String).annotate({ description: 'Workspace files holding catalogs beside the package manifests, relative to the workspace root', default: [] }),
    supported: Schema.Record(
        Schema.String,
        Schema.Struct({
            package: Schema.String.annotate({ description: 'Installed package listing the versions it supports, resolved from the workspace root' }),
            file: Schema.String.annotate({ description: 'JSON file inside that package, or inside its optional dependency installed for this platform' }),
            path: Schema.Array(Schema.String).annotate({ description: 'Keys from the file root to the object holding one key per supported version' }),
        }),
    ).annotate({ description: 'Pinned names, as a catalog or manifest spells them, whose moves stay among the versions an installed package lists in place of the dist-tag heads', default: {} }),
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each move and makes none, as NX_DRY_RUN=true does' }),
}).annotate({
    title: 'Upgrade',
    description:
        "Moves every exact version a catalog or a package manifest of the workspace pins, npm: aliases included, to the newest of the package's undeprecated dist-tag heads, or of the versions an installed package lists for a supported name, among those published at or after the pinned version, ranked by release, then stable before prerelease, then publish time, ranges staying as floors, then installs and deduplicates to move the lockfile, then removes the packages no installed project uses where the package manager registers the workspace with its store, as pnpm store prune does for a store outside the workspace",
});
const Packument = Schema.Struct({ 'dist-tags': Schema.Record(Schema.String, Schema.String), time: Schema.Record(Schema.String, Schema.DateTimeUtcFromString) }).annotate({ description: 'The dist-tags and publish times the registry holds for a package' });
const Release = Schema.Struct({ version: Schema.String, deprecated: Schema.optionalKey(Schema.String) }).annotate({ description: 'The deprecation message the registry holds for one version, absent where the version stands' });
const Lister = Schema.fromJsonString(Schema.Struct({ optionalDependencies: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) })).annotate({ description: 'The optional dependencies of a package listing supported versions, one of them its platform build' });
const Listing = Schema.Record(Schema.String, Schema.Unknown).annotate({ description: 'One object of a JSON file listing supported versions, keyed by name or version' });
const Listed = Schema.fromJsonString(Listing).annotate({ description: 'A JSON file listing supported versions, its root an object' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const ranked = Order.combineAll<{ readonly version: SemVer.SemVer; readonly at: DateTime.Utc }>([Order.mapInput(SemVer.Order, ({ version }) => SemVer.release(version)), Order.mapInput(Boolean.Order, ({ version }) => !SemVer.isPrerelease(version)), Order.mapInput(DateTime.Order, ({ at }) => at)]);

const pinned = (name: string, spec: string) =>
    Option.flatMap(Result.getSuccess(Result.try(() => npa.resolve(name, spec))), (parsed) =>
        Match.value(parsed).pipe(
            Match.when({ type: 'version' }, ({ fetchSpec }) => Option.map(Result.getSuccess(SemVer.parse(fetchSpec)), (version) => ({ registry: name, version, render: SemVer.format }))),
            Match.when({ type: 'alias', subSpec: { type: 'version' } }, ({ subSpec }) =>
                Option.map(Option.all({ registry: Option.fromNullishOr(subSpec.name), version: Result.getSuccess(SemVer.parse(subSpec.fetchSpec ?? '')) }), ({ registry, version }) => ({ registry, version, render: (to: SemVer.SemVer) => `npm:${registry}@${SemVer.format(to)}` })),
            ),
            Match.orElse(() => Option.none()),
        ),
    );

const sections = (node: unknown, pattern: readonly string[]): readonly YAMLMap[] =>
    node instanceof YAMLMap
        ? Array.match(pattern, {
              onEmpty: () => [node],
              onNonEmpty: ([head, ...rest]) => {
                  const children: readonly unknown[] = head === '*' ? Array.fromIterable(node.pairs()).map((pair): unknown => pair.value) : [node.get(head)];
                  return children.flatMap((child) => sections(child, rest));
              },
          })
        : [];

const eligible = (current: SemVer.SemVer, time: Readonly<Record<string, DateTime.Utc>>, heads: readonly (readonly [string, string])[]) => {
    const since = Record.get(time, SemVer.format(current));
    const pin = { version: current, at: Option.getOrElse(since, () => DateTime.makeUnsafe(0)) };
    return Array.getSomes(heads.map(([tag, version]) => Option.map(Option.all({ version: Result.getSuccess(SemVer.parse(version)), at: Record.get(time, version) }), (head) => ({ tag, ...head })))).filter(
        (head) => Order.isGreaterThan(ranked)(head, pin) && Option.match(since, { onNone: () => true, onSome: (held) => DateTime.isGreaterThanOrEqualTo(head.at, held) }),
    );
};

const viewed = <S extends Schema.Top>(registry: string, version: string, fields: readonly string[], schema: S) =>
    Effect.flatMap(Effect.tryPromise({ try: () => packageRegistryView(registry, version, [...fields, '--json']), catch: (cause) => new Cause.UnknownError(cause, `The registry answers no ${fields.join(', ')} of ${registry}@${version || 'latest'}`) }), (raw) =>
        Schema.decodeUnknownEffect(schema)(parseRegistryViewJson(raw)),
    );

const listed = Effect.fnUntraced(function* (root: string, owner: string, file: string, path: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = yield* Effect.try({ try: () => createRequire(join(root, 'package.json')).resolve(`${owner}/package.json`), catch: (cause) => new Cause.UnknownError(cause, `${root} resolves no ${owner}/package.json`) });
    const { optionalDependencies = {} } = yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeUnknownEffect(Lister));
    const folders = [dirname(manifest), ...Array.getSomes(Record.keys(optionalDependencies).map((name) => Result.getSuccess(Result.try(() => dirname(createRequire(manifest).resolve(`${name}/package.json`))))))];
    const at = yield* Effect.flatMap(
        Effect.findFirst(
            folders.map((folder) => join(folder, file)),
            (candidate) => fs.exists(candidate),
        ),
        (found) => Effect.fromOption(found, () => new Cause.UnknownError(folders, `${owner} and its installed optional dependencies hold no ${file}`)),
    );
    const document = yield* Effect.flatMap(fs.readFileString(at), Schema.decodeUnknownEffect(Listed));
    const object = path.reduce((node, key) => Option.flatMap(Option.flatMap(node, Record.get(key)), Schema.decodeUnknownOption(Listing)), Option.some(document));
    const versions = yield* Effect.fromOption(object, () => new Cause.UnknownError(document, `${at} holds no object at ${path.join('.')}`));
    return Record.keys(versions).map((version) => [owner, version] as const);
});

const resolved = Effect.fnUntraced(function* <P extends { readonly name: string; readonly version: SemVer.SemVer; readonly render: (to: SemVer.SemVer) => string }>(registry: string, pins: readonly P[], bounds: Readonly<Record<string, readonly (readonly [string, string])[]>>) {
    const { 'dist-tags': tags, time } = yield* viewed(registry, '', ['dist-tags', 'time'], Packument);
    const candidates = pins.map((pin) => ({
        pin,
        heads: eligible(
            pin.version,
            time,
            Option.getOrElse(Record.get(bounds, pin.name), () => Record.toEntries(tags)),
        ),
    }));
    const versions = Array.dedupe(candidates.flatMap(({ heads }) => heads.map(({ version }) => SemVer.format(version))));
    const releases = yield* Effect.forEach(versions, (version) => viewed(registry, version, ['deprecated', 'version'], Release), { concurrency: 4 });
    const deprecated = new Set(releases.filter(({ deprecated: message }) => message !== undefined).map(({ version }) => version));
    return Array.getSomes(
        candidates.map(({ pin, heads }) =>
            Option.map(
                Array.match(
                    heads.filter(({ version }) => !deprecated.has(SemVer.format(version))),
                    { onEmpty: () => Option.none(), onNonEmpty: (held) => Option.some(Array.max(held, ranked)) },
                ),
                (head) => ({ ...pin, head, spec: pin.render(head.version) }),
            ),
        ),
    );
});

const opened = Effect.fnUntraced(function* (root: string, file: string) {
    const fs = yield* FileSystem.FileSystem;
    const tokens = Array.fromIterable(new Parser().parse(yield* fs.readFileString(join(root, file))));
    const [document] = Array.fromIterable(new Composer({ keepSourceTokens: true }).compose(tokens));
    const composed = yield* Effect.fromNullishOr(document);
    const rows = SECTIONS.flatMap((pattern) => sections(composed.value, pattern))
        .flatMap((section) => Array.fromIterable(section.pairs()))
        .flatMap(({ key, value }) => (key instanceof Scalar && value instanceof Scalar && Predicate.isString(key.value) && Predicate.isString(value.value) && value.srcToken !== undefined ? [{ name: key.value, spec: value.value, token: value.srcToken }] : []))
        .flatMap(({ name, spec, token }) => Option.toArray(Option.map(pinned(name, spec), (pin) => ({ file, name, token, ...pin }))));
    return { file, tokens, rows };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Upgrade, function* ({ catalogs, supported, dryRun }, context) {
    const [fs, dry, bounds] = yield* Effect.all([
        FileSystem.FileSystem,
        dried(dryRun),
        Effect.all(
            Record.map(supported, ({ package: owner, file, path }) => listed(context.root, owner, file, path)),
            { concurrency: 'unbounded' },
        ),
    ]);
    const held = yield* Effect.filter(Array.dedupe([...catalogs, ...Object.values(context.projectsConfigurations.projects).map(({ root }) => join(root, 'package.json'))]), (at) => fs.exists(join(context.root, at)));
    const files = yield* Effect.forEach(held, (file) => opened(context.root, file));
    const grouped = Array.groupBy(
        files.flatMap(({ rows }) => rows),
        ({ registry }) => registry,
    );
    const [resolutions, failures] = yield* Effect.partition(Record.toEntries(grouped), ([registry, pins]) => resolved(registry, pins, bounds), { concurrency: 16 });
    const moves = resolutions.flat();
    const moved = new Set(moves.map(({ file }) => file));
    yield* Effect.forEach(moves, ({ file, name, version, head }) => Effect.logInfo(`${name} ${SemVer.format(version)} -> ${SemVer.format(head.version)} (${head.tag}, published ${DateTime.formatIso(head.at)}) in ${file}`), { discard: true });
    yield* Effect.forEach(failures, (failure) => Effect.logError(failure), { discard: true });
    const manager = detectPackageManager(context.root);
    const { lock, prune, stored } = MANAGERS[manager];
    const shell = { cwd: context.root, stdout: 'inherit', stderr: 'inherit' } as const;
    const pruning = [manager, ...prune] as const;
    const relocked = Effect.andThen(
        Effect.forEach(moves, ({ token, spec }) => Effect.sync(() => CST.setScalarValue(token, spec)), { discard: true }),
        Effect.forEach(
            files.filter(({ file }) => moved.has(file)),
            ({ file, tokens }) => fs.writeFileString(join(context.root, file), tokens.map((token) => CST.stringify(token)).join('')),
            { discard: true },
        ),
    ).pipe(Effect.andThen(Effect.forEach(lock, (step) => spawned([manager, step], shell), { discard: true })));
    const freed = Array.match(stored, {
        onEmpty: () => Effect.void,
        onNonEmpty: (args) =>
            Effect.flatMap(
                Effect.map(spawned([manager, ...args], { cwd: context.root }), (at) => relative(context.root, at.trim())),
                (at) =>
                    Match.value({ kept: !(at.startsWith('..') || isAbsolute(at)), dry }).pipe(
                        Match.when({ kept: true }, () => Effect.logInfo(`Keeps ${at} whole, as ${manager} registers no workspace holding its store and ${CommandLine.display(pruning)} there removes every package the workspace installed`)),
                        Match.when({ dry: true }, () => Effect.logInfo(`Would run ${CommandLine.display(pruning)}`)),
                        Match.orElse(() => spawned(pruning, shell)),
                    ),
            ),
    });
    yield* Array.isReadonlyArrayEmpty(moves) ? Effect.void : Effect.andThen(dry ? Effect.void : relocked, freed);
    return { success: failures.length === 0 };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Upgrade };
