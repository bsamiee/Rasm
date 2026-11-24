import { basename, dirname, isAbsolute, join, posix, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { parse } from '@decimalturn/toml-patch';
import { normalizePath, readJsonFile } from '@nx/devkit';
import type { PackageJson } from '@nx/devkit/internal';
import { above, runner, spawned } from '@rasm/nx';
import { Array, Cause, Config, Context, Effect, Equivalence, FileSystem, Layer, Option, Predicate, Record, Result, Schema, String, UndefinedOr } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Reading = Effect.Success<ReturnType<typeof read>>;
type Workspace = ReturnType<typeof resolved>;
type Member = Workspace['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Cargo.toml';
const LOCK = 'Cargo.lock';
const CRATE = 'nx_cargo';
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const ACTIONS = `${PLUGIN}/release/version-actions`;
const SETTINGS = ['.cargo/config.toml', '.cargo/config'] as const;
const CONFIG = [...SETTINGS, 'rust-toolchain.toml', 'rust-toolchain'] as const;
const CLIPPY = ['.clippy.toml', 'clippy.toml'] as const;
const PROFILES = ['dev', 'release', 'test', 'bench'] as const;
const COMPILER = ['RUSTC', 'RUSTC_WRAPPER', 'RUSTC_WORKSPACE_WRAPPER', 'RUSTDOC', 'RUSTFLAGS', 'CARGO_ENCODED_RUSTFLAGS', 'RUSTDOCFLAGS', 'CARGO_ENCODED_RUSTDOCFLAGS', 'RUSTUP_TOOLCHAIN'] as const;
const KEYS = {
    BUILD: ['TARGET', 'RUSTC', 'RUSTC_WRAPPER', 'RUSTC_WORKSPACE_WRAPPER', 'RUSTDOC', 'RUSTFLAGS', 'RUSTDOCFLAGS'],
    TARGET: ['LINKER', 'RUSTFLAGS'],
    PROFILE: ['INHERITS', 'OPT_LEVEL', 'DEBUG', 'SPLIT_DEBUGINFO', 'STRIP', 'DEBUG_ASSERTIONS', 'OVERFLOW_CHECKS', 'LTO', 'PANIC', 'CODEGEN_UNITS', 'RPATH', 'TRIM_PATHS', 'CODEGEN_BACKEND'],
    RESOLVER: ['FEATURE_UNIFICATION'],
} as const;
const NEXTEST = '.config/nextest.toml';
const DENY = Record.map({ config: 'deny', exceptions: 'deny.exceptions' }, (stem) => [`${stem}.toml`, `.${stem}.toml`, `.cargo/${stem}.toml`, `.config/${stem}.toml`]);

// --- [MODELS] --------------------------------------------------------------------------

class Target extends Schema.Opaque<Target, unknown>()(Schema.Struct({ kind: Schema.Array(Schema.String), crate_types: Schema.Array(Schema.String), name: Schema.String, src_path: Schema.String, test: Schema.Boolean, doctest: Schema.Boolean })) {
    static readonly own = ({ kind, crate_types }: Target) => Array.makeEquivalence(Equivalence.String)(kind, crate_types);
    static readonly binary = ({ kind }: Target) => kind.includes('bin');
    static readonly example = ({ kind }: Target) => kind.includes('example');
    static readonly library = (candidate: Target) => Target.own(candidate) && !Target.binary(candidate);
}
const Package = Schema.Struct({
    name: Schema.String,
    version: Schema.String,
    manifest_path: Schema.String,
    readme: Schema.NullOr(Schema.String),
    publish: Schema.NullOr(Schema.Array(Schema.String)),
    dependencies: Schema.Array(Schema.Struct({ name: Schema.String, path: Schema.optionalKey(Schema.String) })),
    targets: Schema.Array(Target),
    features: Schema.Record(Schema.String, Schema.Array(Schema.String)),
    metadata: Schema.NullOr(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ implicitDependencies: Schema.optionalKey(Schema.Array(Schema.String)) })) })),
});
const Metadata = Schema.fromJsonString(
    Schema.Struct({
        packages: Schema.Array(Package),
        workspace_root: Schema.String,
        target_directory: Schema.String,
        metadata: Schema.NullOr(Schema.Struct({ dylint: Schema.optionalKey(Schema.Struct({ libraries: Schema.Array(Schema.Struct({ path: Schema.optionalKey(Schema.String) })) })) })),
    }),
);
const Crate = Schema.Struct({ root: Schema.String, name: Schema.String }).annotate({ description: 'Workspace crate by its folder, relative to the Nx workspace root, and package name' });
const Unification = Schema.Literals(['selected', 'workspace', 'package']).annotate({ description: 'Cargo resolver.feature-unification: features unify over the packages one invocation selects, over every workspace member, or per package' });
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Profiles = Schema.Struct({ profile: Schema.Record(Schema.String, Schema.Struct({ inherits: Schema.optionalKey(Schema.String), panic: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) });
const Settings = Schema.Struct({
    env: Schema.Record(Schema.String, Schema.Union([Schema.String, Schema.Struct({ value: Schema.optionalKey(Schema.String), force: Schema.optionalKey(Schema.Boolean), relative: Schema.optionalKey(Schema.Boolean) })])).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    unstable: Schema.Record(Schema.String, Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    build: Schema.Struct({ target: Schema.optionalKey(Schema.Union([Schema.String, Schema.Array(Schema.String)])) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    resolver: Schema.Struct({ 'feature-unification': Schema.optionalKey(Unification) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
}).annotate({
    description:
        'The [env], [unstable], [build], and [resolver] tables of a Cargo configuration file, an [env] table row merging field by field across layers and yielding to the process environment unless force = true, an [env] value with relative = true and a build.target spec file resolving against the folder holding its .cargo folder',
});

// --- [SERVICES] ------------------------------------------------------------------------

class Links extends Context.Service<Links, Map<string, ReadonlyMap<string, Member>>>()(`${PLUGIN}/workspace/Links`) {
    static readonly layer = Layer.sync(Links, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const tokened = (file: string) => posix.join('{workspaceRoot}', file);

const nearest = (files: ReadonlySet<string>, folder: string, names: readonly string[]) => Array.findFirst(above(folder), (at) => Array.findFirst(names, (name) => files.has(posix.join(at, name))).pipe(Option.map((name) => posix.join(at, name))));

const parsed = (text: string, subject: string) => Effect.try({ try: (): unknown => parse(text), catch: (cause) => new Cause.UnknownError(cause, `${subject} does not read as TOML`) });

const toml = <S extends Schema.Decoder<unknown>>(schema: S, file: string) =>
    FileSystem.FileSystem.pipe(
        Effect.flatMap((fs) => fs.readFileString(file)),
        Effect.flatMap((text) => parsed(text, file)),
        Effect.flatMap(Schema.decodeUnknownEffect(schema)),
    );

const read = Effect.fnUntraced(function* (workspaceRoot: string, manifest: string, files: ReadonlySet<string>) {
    const metadata = yield* Effect.flatMap(spawned(['cargo', 'metadata', '--format-version', '1', '--no-deps'], { cwd: join(workspaceRoot, posix.dirname(manifest)) }), Schema.decodeEffect(Metadata));
    const root = normalizePath(relative(workspaceRoot, metadata.workspace_root)) || '.';
    const types = Array.difference(Array.dedupe(metadata.packages.flatMap(({ targets }) => targets.flatMap((target) => target.crate_types))), ['lib', 'rlib']);
    const found = (stems: readonly string[]) => above(root).flatMap((at) => stems.map((stem) => posix.join(at, stem)).filter((file) => files.has(file)));
    const settings = found(SETTINGS);
    const [{ profile }, host, denied, layers, exported, resolving, gated, directed] = yield* Effect.all(
        [
            toml(Profiles, join(metadata.workspace_root, MANIFEST)),
            Effect.map(spawned(['rustc', '--print', 'host-tuple'], { cwd: metadata.workspace_root }), String.trim),
            Effect.transposeOption(Option.map(nearest(files, root, DENY.config), (file) => Effect.map(toml(Table, join(workspaceRoot, file)), (tables) => ({ files: [file, ...Option.toArray(nearest(files, root, DENY.exceptions))], tables: Record.keys(tables) })))),
            Effect.forEach(settings, (file) => toml(Settings, join(workspaceRoot, file)), { concurrency: 'unbounded' }),
            Config.option(Config.String('CARGO_BUILD_TARGET')),
            Config.option(Config.Literals(Unification.literals, 'CARGO_RESOLVER_FEATURE_UNIFICATION')),
            Config.option(Config.Boolean('CARGO_UNSTABLE_FEATURE_UNIFICATION')),
            Config.option(Config.String('CLIPPY_CONF_DIR')),
        ],
        { concurrency: 'unbounded' },
    );
    const enabled = Option.contains(
        Option.orElse(gated, () => Array.findFirst(layers, ({ unstable }) => Record.get(unstable, 'feature-unification'))),
        true,
    );
    const configured = Array.findFirst(Array.zip(settings, layers), ([file, { build }]) => Option.map(Option.fromUndefinedOr(build.target), (targets) => ({ targets: Array.ensure(targets), from: join(workspaceRoot, posix.dirname(posix.dirname(file))) })));
    const chosen = Option.orElse(
        Option.map(exported, (target) => ({ targets: [target], from: metadata.workspace_root })),
        () => configured,
    );
    const layouts = yield* Effect.forEach(
        Option.match(chosen, { onNone: () => [{ argument: [], folder: [] }], onSome: ({ targets, from }) => targets.map((target) => (target.endsWith('.json') ? { argument: ['--target', resolve(from, target)], folder: [basename(target, '.json')] } : { argument: ['--target', target], folder: [target] })) }),
        ({ argument, folder }) => Effect.map(spawned(['rustc', ...argument, '--print', 'file-names', '--crate-name', CRATE, ...types.flatMap((type) => ['--crate-type', type]), '-'], { cwd: metadata.workspace_root }), (stdout) => ({ folder, uplifted: Record.fromEntries(Array.zip(types, String.split(stdout, '\n'))) })),
        { concurrency: 'unbounded' },
    );
    const rows = Array.getSomes(Array.zip(settings, layers).map(([file, { env }]) => Option.map(Record.get(env, 'CLIPPY_CONF_DIR'), (row) => ({ ...(Predicate.isString(row) ? { value: row, force: false, relative: false } : row), from: join(workspaceRoot, posix.dirname(posix.dirname(file))) }))));
    const merged = (option: 'force' | 'relative') =>
        Option.contains(
            Array.findFirst(rows, (row) => Option.fromUndefinedOr(row[option])),
            true,
        );
    const layered = Array.findFirst(rows, ({ value, from }) => Option.map(Option.fromUndefinedOr(value), (at) => resolve(merged('relative') ? from : metadata.workspace_root, at)));
    const inherited = Option.map(directed, (at) => resolve(metadata.workspace_root, at));
    const clippy = merged('force') ? Option.orElse(layered, () => inherited) : Option.orElse(inherited, () => layered);
    return {
        metadata,
        profile,
        host,
        configs: found(CONFIG),
        clippy: Option.toArray(Option.map(clippy, (at) => normalizePath(relative(workspaceRoot, at)) || '.')),
        unstable: Array.dedupe(layers.flatMap(({ unstable }) => Record.keys(unstable))),
        unification: Option.orElse(resolving, () => Array.findFirst(layers, ({ resolver }) => Option.fromUndefinedOr(resolver['feature-unification']))).pipe(
            Option.filter(() => enabled),
            Option.getOrElse((): typeof Unification.Type => 'selected'),
        ),
        layouts,
        deny: Option.toArray(denied),
    };
});

const moved = ({ metadata, ...reading }: Reading, to: (at: string) => string): Reading => ({
    ...reading,
    metadata: {
        ...metadata,
        workspace_root: to(metadata.workspace_root),
        target_directory: to(metadata.target_directory),
        packages: metadata.packages.map((crate) => ({
            ...crate,
            manifest_path: to(crate.manifest_path),
            targets: crate.targets.map((target) => ({ ...target, src_path: to(target.src_path) })),
            dependencies: crate.dependencies.map((dependency) => (dependency.path === undefined ? dependency : { ...dependency, path: to(dependency.path) })),
        })),
    },
});

const placed = (workspaceRoot: string) => ({ stored: (reading: Reading) => moved(reading, (at) => relative(workspaceRoot, at)), loaded: (reading: Reading) => moved(reading, (at) => resolve(workspaceRoot, at)) });

const variable = (key: string) => key.toUpperCase().replaceAll(/[-.]/gu, '_');

const resolved = (workspaceRoot: string, { metadata, profile, host, configs, clippy, unstable, unification, layouts, deny }: Reading, files: ReadonlySet<string>) => {
    const within = (at: string) => normalizePath(relative(workspaceRoot, at)) || '.';
    const inside = (at: string) => Option.liftPredicate(within(at), (folder) => !(folder.startsWith('..') || isAbsolute(folder)));
    const root = within(metadata.workspace_root);
    const profiles = Record.keys(Record.filter(profile, ({ inherits }) => inherits !== undefined));
    const strategy = (name: string): string => profile[name]?.panic ?? UndefinedOr.match(profile[name]?.inherits, { onUndefined: () => 'unwind', onDefined: strategy });
    const crates = metadata.packages.map((crate) => ({ crate, root: within(dirname(crate.manifest_path)), manifest: within(crate.manifest_path), versioned: crate.version !== '0.0.0' || crate.publish === null }));
    const reached = (names: ReadonlySet<string>): ReadonlySet<string> => {
        const next = names.union(new Set(crates.filter(({ crate }) => names.has(crate.name)).flatMap(({ crate }) => crate.dependencies.filter(({ path: to }) => to !== undefined).map(({ name }) => name))));
        return next.size === names.size ? names : reached(next);
    };
    return {
        root,
        members: crates.map((member) => {
            const needed = reached(new Set([member.crate.name]));
            const reachable = crates.filter((other) => other.crate.name !== member.crate.name && needed.has(other.crate.name));
            return {
                ...member,
                packages: reachable.map((other) => ({ root: other.root, name: other.crate.name })),
                packable: reachable.every(({ crate }) => crate.publish?.length !== 0),
                linted: Option.toArray(nearest(files, clippy[0] ?? member.root, CLIPPY)).map(tokened),
            };
        }),
        libraries: Array.getSomes((metadata.metadata?.dylint?.libraries ?? []).map(({ path: at }) => Option.map(Option.fromUndefinedOr(at), (folder) => posix.join(root, folder)))).flatMap((folder) => {
            const [sources, others] = Array.partition(
                crates.filter((member) => member.root === folder).flatMap(({ crate }) => crate.targets.map((target) => [target, posix.join(folder, normalizePath(relative(dirname(crate.manifest_path), dirname(target.src_path))))] as const)),
                ([target, at]) => (Target.library(target) ? Result.succeed(at) : Result.fail(at)),
            );
            const found = Array.dedupe(sources);
            const included = Array.isReadonlyArrayNonEmpty(found) ? found : [folder];
            const excluded = Array.difference(Array.dedupe([...others, ...crates.map((member) => member.root).filter((nested) => nested.startsWith(`${folder}/`))]), included);
            return [...included.map((at) => tokened(posix.join(at, '**/*'))), ...excluded.map((at) => `!${tokened(posix.join(at, '**/*'))}`)];
        }),
        configs: configs.map(tokened),
        unification,
        unified: unification === 'workspace' ? crates.map(({ manifest }) => tokened(manifest)) : [],
        compiler: [
            ...COMPILER,
            ...(
                [
                    [['BUILD'], KEYS.BUILD],
                    [Array.dedupe([host, ...layouts.flatMap(({ folder }) => folder)]).map((triple) => `TARGET_${variable(triple)}`), KEYS.TARGET],
                    [[...PROFILES, ...profiles].map((name) => `PROFILE_${variable(name)}`), KEYS.PROFILE],
                    [['UNSTABLE'], unstable.map(variable)],
                    [['RESOLVER'], KEYS.RESOLVER],
                ] as const
            ).flatMap(([scopes, keys]) => Array.cartesianWith(scopes, keys, (scope, key) => `CARGO_${scope}_${key}`)),
        ],
        nextest: Option.liftPredicate(posix.join(root, NEXTEST), (file) => files.has(file)),
        profiles,
        strategies: Array.dedupeWith(['release', ...profiles], (left, right) => strategy(left) === strategy(right)).filter((name) => strategy(name) !== strategy('dev')),
        deny: Array.head(deny),
        placed: inside(metadata.target_directory),
        uplifted: (target: Target) => layouts.flatMap(({ folder, uplifted }) => Array.getSomes(target.crate_types.map((type) => Option.map(Record.get(uplifted, type), (file) => ({ folder, file: file.replace(CRATE, target.name) }))))),
    };
};

const workspaces = (workspaceRoot: string, manifests: readonly string[], files: ReadonlySet<string>) =>
    Effect.reduce(
        manifests.toSorted((left, right) => left.split('/').length - right.split('/').length || left.localeCompare(right)),
        (): readonly Result.Result<Reading, [string, Error]>[] => [],
        (done, manifest) =>
            done.some((reading) => Result.isSuccess(reading) && reading.success.metadata.packages.some(({ manifest_path }) => normalizePath(relative(workspaceRoot, manifest_path)) === manifest))
                ? Effect.succeed(done)
                : Effect.map(Effect.result(Effect.mapError(read(workspaceRoot, manifest, files), (error): [string, Error] => [manifest, error])), (reading) => [...done, reading]),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = runner(Links.layer);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ACTIONS, CLIPPY, CONFIG, Crate, DENY, Links, LOCK, MANIFEST, type Member, NEXTEST, parsed, PLUGIN, placed, type Reading, resolved, run, Table, Target, tokened, toml, Unification, type Workspace, workspaces };
