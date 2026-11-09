import { parse } from '@decimalturn/toml-patch';
import { DependencyType, type ProjectGraphExternalNode, type RawProjectGraphDependency } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Cargo.toml';
const CRATE = 'nx_cargo';
const UNWIND = 'unwind';
const DENY = Record.map({ config: 'deny', exceptions: 'deny.exceptions' }, (stem) => [`${stem}.toml`, `.${stem}.toml`, `.cargo/${stem}.toml`]);

// --- [MODELS] --------------------------------------------------------------------------

const Listed = Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Target = Schema.Struct({ kind: Schema.Array(Schema.String), crate_types: Schema.Array(Schema.String), name: Schema.String, src_path: Schema.String, test: Schema.Boolean, doctest: Schema.Boolean });
const Package = Schema.Struct({
    name: Schema.String,
    version: Schema.String,
    manifest_path: Schema.String,
    publish: Schema.NullOr(Schema.Array(Schema.String)),
    dependencies: Schema.Array(Schema.Struct({ name: Schema.String, path: Schema.optionalKey(Schema.String) })),
    targets: Schema.Array(Target),
});
const Metadata = Schema.fromJsonString(
    Schema.Struct({
        packages: Schema.Array(Package),
        workspace_root: Schema.String,
        target_directory: Schema.String,
        metadata: Schema.NullOr(Schema.Struct({ dylint: Schema.optionalKey(Schema.Struct({ libraries: Schema.Array(Schema.Struct({ path: Schema.optionalKey(Schema.String) })) })) })),
    }),
);
const Locked = Schema.Struct({ name: Schema.String, version: Schema.String, source: Schema.optionalKey(Schema.String), checksum: Schema.optionalKey(Schema.String), dependencies: Listed });
const Lock = Schema.Struct({ package: Schema.Array(Locked).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) });
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Tables = Table.pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.Struct({
    profile: Schema.Record(Schema.String, Schema.Struct({ inherits: Schema.optionalKey(Schema.String), panic: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    workspace: Schema.Struct({ lints: Tables }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ lints: {} }))),
    lints: Schema.StructWithRest(Schema.Struct({ workspace: Schema.optionalKey(Schema.Boolean) }), [Schema.Record(Schema.String, Schema.Unknown)]).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
});

// --- [ERRORS] --------------------------------------------------------------------------

class CargoError extends Schema.TaggedError<CargoError, { readonly brand: unique symbol }>()('CargoError', { subject: Schema.String, detail: Schema.String }) {
    static readonly of = (subject: string) => (cause: { readonly message: string }) => (Schema.is(CargoError)(cause) ? cause : CargoError.make({ subject, detail: cause.message }));

    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = (command: string, args: readonly string[], cwd: string, input?: string) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make(command, args, { cwd, extendEnv: true, ...(input === undefined ? {} : { stdin: Stream.encodeText(Stream.succeed(input)) }) });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* CargoError.make({ subject: [command, ...args].join(' '), detail: `exited ${code}: ${stderr.trim()}` });
        }),
    );

const ancestors = (path: Path.Path, folder: string): readonly string[] => Array.unfold(Option.some(folder), (held) => Option.map(held, (at) => [at, Option.liftPredicate(path.dirname(at), () => at !== '.')] as const));

const nearest = (path: Path.Path, files: ReadonlySet<string>, folder: string, names: readonly string[]) => Array.findFirst(ancestors(path, folder), (at) => Array.findFirst(names, (name) => files.has(path.join(at, name))).pipe(Option.map((name) => path.join(at, name))));

const toml = <S extends Schema.Decoder<unknown>>(schema: S, file: string) =>
    FileSystem.FileSystem.pipe(
        Effect.flatMap((fs) => fs.readFileString(file)),
        Effect.flatMap((text) => Effect.try({ try: (): unknown => parse(text), catch: (cause) => CargoError.make({ subject: file, detail: String(cause) }) })),
        Effect.flatMap(Schema.decodeUnknownEffect(schema)),
        Effect.mapError(CargoError.of(file)),
    );

const externals = (lock: readonly (typeof Locked.Type)[], roots: ReadonlyMap<string, string>) => {
    const node = (entry: typeof Locked.Type) => `cargo:${entry.name}@${entry.version}`;
    const named = Array.groupBy(lock, ({ name }) => name);
    const sourced = (spec: string) => {
        const [name = '', version] = spec.split(' ');
        return Option.filter(
            Array.findFirst(named[name] ?? [], (entry) => version === undefined || entry.version === version),
            ({ source }) => source !== undefined,
        );
    };
    return {
        nodes: Record.fromIterableWith(
            lock.filter(({ source }) => source !== undefined),
            (entry): readonly [string, ProjectGraphExternalNode] => [node(entry), { type: 'cargo', name: node(entry), data: { version: entry.version, packageName: entry.name, hash: entry.checksum ?? entry.source ?? entry.version } }],
        ),
        edges: lock.flatMap((entry) => {
            const source = entry.source === undefined ? roots.get(entry.name) : node(entry);
            return source === undefined ? [] : entry.dependencies.flatMap((spec) => Option.toArray(sourced(spec)).map((target): RawProjectGraphDependency => (entry.source === undefined ? { source, target: node(target), type: DependencyType.implicit } : { source, target: node(target), type: DependencyType.static })));
        }),
    };
};

const workspace = Effect.fnUntraced(
    function* (workspaceRoot: string, manifest: string, files: ReadonlySet<string>) {
        const path = yield* Path.Path;
        const metadata = yield* Effect.flatMap(spawned('cargo', ['metadata', '--format-version', '1', '--no-deps', '--manifest-path', manifest], workspaceRoot), Schema.decodeEffect(Metadata));
        const root = path.relative(workspaceRoot, metadata.workspace_root) || '.';
        const lockFile = path.join(root, 'Cargo.lock');
        const types = Array.difference(Array.dedupe(metadata.packages.flatMap(({ targets }) => targets.flatMap((target) => target.crate_types))), ['lib', 'rlib']);
        const denied = nearest(path, files, root, DENY.config);
        const top = path.join(metadata.workspace_root, MANIFEST);
        const [lock, read, printed, deny] = yield* Effect.all(
            [
                files.has(lockFile) ? Effect.map(toml(Lock, path.join(workspaceRoot, lockFile)), (held) => Option.some(held.package)) : Effect.succeed(Option.none<readonly (typeof Locked.Type)[]>()),
                Effect.map(
                    Effect.forEach(Array.dedupe([top, ...metadata.packages.map(({ manifest_path }) => manifest_path)]), (file) => Effect.map(toml(Manifest, file), (held) => [file, held] as const), { concurrency: 'unbounded' }),
                    Record.fromEntries,
                ),
                Effect.map(spawned('rustc', ['--print', 'file-names', '--crate-name', CRATE, ...types.flatMap((type) => ['--crate-type', type]), '-'], path.join(workspaceRoot, root), ''), (text) => text.split('\n')),
                Effect.transposeOption(Option.map(denied, (file) => Effect.map(toml(Table, path.join(workspaceRoot, file)), (tables) => ({ files: [file, ...Option.toArray(nearest(path, files, root, DENY.exceptions))], tables: Record.keys(tables) })))),
            ],
            { concurrency: 'unbounded' },
        );
        const members = metadata.packages.map((crate) => ({ crate, root: path.relative(workspaceRoot, path.dirname(crate.manifest_path)) || '.' }));
        const locked = Option.map(lock, (entries) => externals(entries, new Map(members.map(({ crate, root: at }) => [crate.name, at] as const))));
        const declared = read[top]?.profile ?? {};
        const profiles = Record.keys(Record.filter(declared, ({ inherits }) => inherits !== undefined));
        const strategy = (profile: string): string => declared[profile]?.panic ?? Option.match(Option.fromUndefinedOr(declared[profile]?.inherits), { onNone: () => UNWIND, onSome: strategy });
        return {
            root,
            metadata,
            members,
            externalNodes: Option.map(locked, ({ nodes }) => nodes),
            edges: [
                ...members.flatMap(({ crate, root: at }) => crate.dependencies.flatMap(({ path: to }): readonly RawProjectGraphDependency[] => (to === undefined ? [] : [{ source: at, target: path.relative(workspaceRoot, to) || '.', type: DependencyType.implicit }]))),
                ...Option.toArray(locked).flatMap(({ edges }) => edges),
            ],
            profiles,
            strategies: Array.dedupeWith(['release', ...profiles], (left, right) => strategy(left) === strategy(right)).filter((profile) => strategy(profile) !== strategy('dev')),
            deny,
            linted: ({ manifest_path }: typeof Package.Type): ReadonlySet<string> => {
                const lints = read[manifest_path]?.lints ?? {};
                return new Set([...Record.keys(lints), ...(lints.workspace === true ? Record.keys(read[top]?.workspace.lints ?? {}) : [])]);
            },
            manifests: metadata.packages.map(({ manifest_path }) => path.relative(workspaceRoot, manifest_path)),
            output: Option.liftPredicate(path.relative(workspaceRoot, metadata.target_directory), (at) => !(at.startsWith('..') || path.isAbsolute(at))),
            files: (target: typeof Target.Type) => target.crate_types.flatMap((type) => Option.toArray(Option.map(Array.get(printed, types.indexOf(type)), (file) => file.replace(CRATE, target.name)))),
        };
    },
    (effect, _, manifest) => Effect.mapError(effect, CargoError.of(manifest)),
);

type Workspace = Effect.Success<ReturnType<typeof workspace>>;

const workspaces = (workspaceRoot: string, manifests: readonly string[], files: ReadonlySet<string>): Effect.Effect<readonly (readonly [string, Result.Result<Workspace, CargoError>])[], never, Path.Path | FileSystem.FileSystem | ChildProcessSpawner.ChildProcessSpawner> =>
    Option.match(Array.head(manifests), {
        onNone: () => Effect.succeed([]),
        onSome: (manifest) =>
            Effect.flatMap(Effect.result(workspace(workspaceRoot, manifest, files)), (held) =>
                Effect.map(
                    workspaces(
                        workspaceRoot,
                        manifests.filter((other) => other !== manifest && !(Result.isSuccess(held) && held.success.manifests.includes(other))),
                        files,
                    ),
                    (rest) => [[manifest, held] as const, ...rest],
                ),
            ),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { ancestors, CargoError, DENY, MANIFEST, Metadata, nearest, spawned, toml, type Workspace, workspaces };
