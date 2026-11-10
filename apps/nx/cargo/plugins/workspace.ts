import { DependencyType, type RawProjectGraphDependency } from '@nx/devkit';
import { ancestors } from '@rasm/nx-devkit/inference';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, flow, Option, Path, Predicate, Record, Result, Schema, String, Struct, UndefinedOr } from 'effect';
import { Toml } from 'effect/encoding';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Cargo.toml';
const LOCK = 'Cargo.lock';
const CRATE = 'nx_cargo';
const CONFIG = ['.cargo/config.toml', '.cargo/config'] as const;
const TOOLCHAIN = ['rust-toolchain', 'rust-toolchain.toml'] as const;
const CLIPPY_CONF_DIR = 'CLIPPY_CONF_DIR';
const DENY = Record.map({ config: 'deny', exceptions: 'deny.exceptions' }, (stem) => [`${stem}.toml`, `.${stem}.toml`, `.cargo/${stem}.toml`]);

// --- [MODELS] --------------------------------------------------------------------------

const Target = Schema.Struct({ kind: Schema.Array(Schema.String), crate_types: Schema.Array(Schema.String), name: Schema.String, src_path: Schema.String, test: Schema.Boolean, doctest: Schema.Boolean });
const Linked = Schema.Struct({ name: Schema.String, rename: Schema.optionalKey(Schema.NullOr(Schema.String)), path: Schema.String });
const Package = Schema.Struct({
    name: Schema.String,
    version: Schema.String,
    manifest_path: Schema.String,
    publish: Schema.NullOr(Schema.Array(Schema.String)),
    dependencies: Schema.Array(Linked.mapFields(Struct.assign({ path: Schema.optionalKey(Schema.String) }))),
    targets: Schema.Array(Target),
    features: Schema.Record(Schema.String, Schema.Array(Schema.String)),
});
const Metadata = Schema.fromJsonString(
    Schema.Struct({
        packages: Schema.Array(Package),
        workspace_root: Schema.String,
        target_directory: Schema.String,
        metadata: Schema.NullOr(Schema.Struct({ dylint: Schema.optionalKey(Schema.Struct({ libraries: Schema.Array(Schema.Struct({ path: Schema.optionalKey(Schema.String) })) })) })),
    }),
);
const Config = Schema.Struct({
    env: Schema.Struct({ [CLIPPY_CONF_DIR]: Schema.optionalKey(Schema.Union([Schema.String, Schema.Struct({ value: Schema.String, relative: Schema.optionalKey(Schema.Boolean) })])) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
});
const Toolchain = Schema.Struct({ toolchain: Schema.Struct({ targets: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }) });
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Tables = Table.pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Pathed = Schema.Struct({ path: Schema.String });
const Requirements = Schema.Struct({ dependencies: Tables, 'dev-dependencies': Tables, 'build-dependencies': Tables });
const Manifest = Requirements.mapFields(
    Struct.assign({
        package: Schema.optionalKey(Schema.Struct({ name: Schema.String })),
        target: Schema.Record(Schema.String, Requirements).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        profile: Schema.Record(Schema.String, Schema.Struct({ inherits: Schema.optionalKey(Schema.String), panic: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        workspace: Schema.Struct({ lints: Tables }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ lints: {} }))),
        lints: Schema.StructWithRest(Schema.Struct({ workspace: Schema.optionalKey(Schema.Boolean) }), [Schema.Record(Schema.String, Schema.Unknown)]).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const nearest = (path: Path.Path, files: ReadonlySet<string>, folder: string, names: readonly string[]) => Array.findFirst(ancestors(path, folder), (at) => Array.findFirst(names, (name) => files.has(path.join(at, name))).pipe(Option.map((name) => path.join(at, name))));

const toml = <S extends Schema.Decoder<unknown>>(schema: S, file: string) =>
    FileSystem.FileSystem.pipe(
        Effect.flatMap((fs) => fs.readFileString(file)),
        Effect.flatMap(flow(Toml.parse, Effect.fromResult)),
        Effect.flatMap(Schema.decodeUnknownEffect(schema)),
        Effect.mapError(ToolError.of(file, 'does not read as the TOML table it declares')),
    );

const workspace = Effect.fnUntraced(
    function* (workspaceRoot: string, manifest: string, files: ReadonlySet<string>) {
        const path = yield* Path.Path;
        const metadata = yield* Effect.flatMap(printed('cargo', ['metadata', '--format-version', '1', '--no-deps', '--manifest-path', manifest], { cwd: workspaceRoot }), Schema.decodeEffect(Metadata));
        const root = path.relative(workspaceRoot, metadata.workspace_root) || '.';
        const types = Array.difference(Array.dedupe(metadata.packages.flatMap(({ targets }) => targets.flatMap((target) => target.crate_types))), ['lib', 'rlib']);
        const denied = nearest(path, files, root, DENY.config);
        const top = path.join(metadata.workspace_root, MANIFEST);
        const executable = ['bin'];
        const emitted = (kinds: readonly string[], flags: readonly string[]) => Effect.map(printed('rustc', ['--print', 'file-names', ...flags, '--crate-name', CRATE, ...kinds.flatMap((type) => ['--crate-type', type]), '-'], { cwd: path.join(workspaceRoot, root), input: '' }), (text) => text.split('\n'));
        const named = (kinds: readonly string[], names: readonly string[]) => (target: typeof Target.Type) => Array.getSomes(target.crate_types.map((type) => Option.map(Array.get(names, kinds.indexOf(type)), (file) => file.replace(CRATE, target.name))));
        const [read, lines, deny, configured, pinned] = yield* Effect.all(
            [
                Effect.map(
                    Effect.forEach(Array.dedupe([top, ...metadata.packages.map(({ manifest_path }) => manifest_path)]), (file) => Effect.map(toml(Manifest, file), (parsed) => [file, parsed] as const), { concurrency: 'unbounded' }),
                    Record.fromEntries,
                ),
                emitted(types, ['--print', 'host-tuple']),
                Effect.transposeOption(Option.map(denied, (file) => Effect.map(toml(Table, path.join(workspaceRoot, file)), (tables) => ({ files: [file, ...Option.toArray(nearest(path, files, root, DENY.exceptions))], tables: Record.keys(tables) })))),
                Effect.forEach(
                    ancestors(path, root)
                        .flatMap((at) => CONFIG.map((name) => path.join(at, name)))
                        .filter((file) => files.has(file)),
                    (file) => Effect.map(toml(Config, path.join(workspaceRoot, file)), ({ env }) => ({ base: path.dirname(path.dirname(file)), folder: env[CLIPPY_CONF_DIR] })),
                    { concurrency: 'unbounded' },
                ),
                Effect.option(Effect.flatMap(Effect.fromOption(nearest(path, files, root, TOOLCHAIN)), (file) => toml(Toolchain, path.join(workspaceRoot, file)))),
            ],
            { concurrency: 'unbounded' },
        );
        const cross = yield* Effect.forEach(
            Option.toArray(pinned).flatMap(({ toolchain }) => toolchain.targets),
            (triple) => Effect.map(emitted(executable, ['--target', triple]), (listed) => ({ triple: Option.some(triple), files: named(executable, listed) })),
            { concurrency: 'unbounded' },
        );
        const members = metadata.packages.map((crate) => ({ crate, root: path.relative(workspaceRoot, path.dirname(crate.manifest_path)) || '.' }));
        const declared = read[top]?.profile ?? {};
        const profiles = Record.keys(Record.filter(declared, ({ inherits }) => inherits !== undefined));
        const strategy = (profile: string): string => declared[profile]?.panic ?? UndefinedOr.match(declared[profile]?.inherits, { onUndefined: () => 'unwind', onDefined: strategy });
        const stated = (file: string) =>
            new Set(
                Option.toArray(Option.fromUndefinedOr(read[file]))
                    .flatMap((owned) => [owned, ...Object.values(owned.target)])
                    .flatMap((tables) => [tables.dependencies, tables['dev-dependencies'], tables['build-dependencies']])
                    .flatMap((table) => Record.keys(Record.filter(table, Schema.is(Pathed)))),
            );
        return {
            root,
            metadata,
            members,
            edges: members.flatMap(({ crate, root: at }) => {
                const own = stated(crate.manifest_path);
                return crate.dependencies.filter(Schema.is(Linked)).map(({ name, rename, path: to }): RawProjectGraphDependency => {
                    const target = path.relative(workspaceRoot, to) || '.';
                    return own.has(rename ?? name) ? { source: at, target, type: DependencyType.static, sourceFile: path.relative(workspaceRoot, crate.manifest_path) } : { source: at, target, type: DependencyType.implicit };
                });
            }),
            profiles,
            strategies: Array.dedupeWith(['release', ...profiles], (left, right) => strategy(left) === strategy(right)).filter((profile) => strategy(profile) !== strategy('dev')),
            deny,
            clippy: Array.findFirst(configured, ({ base, folder }) => (Predicate.isString(folder) ? Option.some(path.join(root, folder)) : Option.map(Option.fromUndefinedOr(folder), ({ value, relative }) => path.join(relative === true ? base : root, value)))),
            linted: ({ manifest_path }: typeof Package.Type): ReadonlySet<string> => {
                const lints = read[manifest_path]?.lints ?? {};
                return new Set([...Record.keys(lints), ...(lints.workspace === true ? Record.keys(read[top]?.workspace.lints ?? {}) : [])]);
            },
            manifests: metadata.packages.map(({ manifest_path }) => path.relative(workspaceRoot, manifest_path)),
            output: Option.liftPredicate(path.relative(workspaceRoot, metadata.target_directory), (at) => !(at.startsWith('..') || path.isAbsolute(at))),
            host: Array.findLast(lines, String.isNonEmpty),
            platforms: [{ triple: Option.none<string>(), files: named(types, lines) }, ...cross],
        };
    },
    (effect, _, manifest) => Effect.mapError(effect, ToolError.of(manifest, 'does not resolve as a Cargo workspace')),
);

type Workspace = Effect.Success<ReturnType<typeof workspace>>;

const workspaces = (workspaceRoot: string, manifests: readonly string[], files: ReadonlySet<string>) =>
    Effect.reduce(
        manifests,
        (): readonly (readonly [string, Result.Result<Workspace, ToolError>])[] => [],
        (done, manifest) => (done.some(([, resolved]) => Result.isSuccess(resolved) && resolved.success.manifests.includes(manifest)) ? Effect.succeed(done) : Effect.map(Effect.result(workspace(workspaceRoot, manifest, files)), (resolved) => [...done, [manifest, resolved] as const])),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { CLIPPY_CONF_DIR, CONFIG, DENY, LOCK, MANIFEST, Manifest, Metadata, nearest, TOOLCHAIN, toml, type Workspace, workspaces };
