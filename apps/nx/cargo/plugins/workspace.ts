import { parse } from '@decimalturn/toml-patch';
import { DependencyType, type ProjectGraphExternalNode, type RawProjectGraphDependency } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Cargo.toml';
const CRATE = 'nx_cargo';

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
const Profiles = Schema.Struct({ profile: Schema.Record(Schema.String, Schema.Struct({ inherits: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) });

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

const ancestors = (path: Path.Path, folder: string): readonly string[] => [folder, ...(folder === '.' ? [] : ancestors(path, path.dirname(folder)))];

const nearest = (path: Path.Path, files: ReadonlySet<string>, folder: string, names: readonly string[]) =>
    Array.findFirst(ancestors(path, folder), (at) => Array.findFirst(names, (name) => files.has(path.join(at, name))).pipe(Option.map((name) => path.join(at, name))));

const toml = <S extends Schema.Decoder<unknown>>(schema: S, file: string) =>
    FileSystem.FileSystem.pipe(
        Effect.flatMap((fs) => fs.readFileString(file)),
        Effect.flatMap((text) => Effect.try({ try: (): unknown => parse(text), catch: (cause) => CargoError.make({ subject: file, detail: String(cause) }) })),
        Effect.flatMap(Schema.decodeUnknownEffect(schema)),
        Effect.mapError(CargoError.of(file)),
    );

const workspace = Effect.fnUntraced(function* (workspaceRoot: string, manifest: string, files: ReadonlySet<string>) {
    const path = yield* Path.Path;
    const metadata = yield* Effect.flatMap(spawned('cargo', ['metadata', '--format-version', '1', '--no-deps', '--offline', '--manifest-path', manifest], workspaceRoot), Schema.decodeEffect(Metadata));
    const root = path.relative(workspaceRoot, metadata.workspace_root) || '.';
    const lockFile = path.join(root, 'Cargo.lock');
    const types = Array.dedupe(metadata.packages.flatMap(({ targets }) => targets.flatMap((target) => target.crate_types)));
    const denied = nearest(path, files, root, ['deny.toml', '.deny.toml', '.cargo/deny.toml']);
    const [lock, profiles, printed, deny] = yield* Effect.all(
        [
            files.has(lockFile) ? Effect.map(toml(Lock, path.join(workspaceRoot, lockFile)), (held) => Option.some(held.package)) : Effect.succeed(Option.none<readonly (typeof Locked.Type)[]>()),
            Effect.map(toml(Profiles, path.join(workspaceRoot, root, MANIFEST)), ({ profile }) => Record.keys(Record.filter(profile, ({ inherits }) => inherits !== undefined))),
            Effect.map(spawned('rustc', ['--print', 'file-names', '--crate-name', CRATE, ...types.flatMap((type) => ['--crate-type', type]), '-'], path.join(workspaceRoot, root), ''), (text) => text.split('\n')),
            Effect.forEach(Option.toArray(denied), (file) => Effect.map(toml(Schema.Record(Schema.String, Schema.Unknown), path.join(workspaceRoot, file)), (tables) => ({ file, tables: Record.keys(tables) }))).pipe(Effect.map(Array.head)),
        ],
        { concurrency: 'unbounded' },
    );
    return {
        root,
        metadata,
        lock,
        profiles,
        deny,
        manifests: metadata.packages.map(({ manifest_path }) => path.relative(workspaceRoot, manifest_path)),
        output: Option.liftPredicate(path.relative(workspaceRoot, metadata.target_directory), (at) => !at.startsWith('..') && !path.isAbsolute(at)),
        files: (target: typeof Target.Type) => target.crate_types.flatMap((type) => Option.toArray(Option.map(Array.get(printed, types.indexOf(type)), (file) => file.replace(CRATE, target.name)))),
    };
}, (effect, _, manifest) => Effect.mapError(effect, CargoError.of(manifest)));

type Workspace = Effect.Success<ReturnType<typeof workspace>>;

const workspaces = (workspaceRoot: string, manifests: readonly string[], files: ReadonlySet<string>): Effect.Effect<readonly (readonly [string, Result.Result<Workspace, CargoError>])[], never, Path.Path | FileSystem.FileSystem | ChildProcessSpawner.ChildProcessSpawner> =>
    Option.match(Array.head(manifests), {
        onNone: () => Effect.succeed([]),
        onSome: (manifest) =>
            Effect.flatMap(Effect.result(workspace(workspaceRoot, manifest, files)), (held) =>
                Effect.map(
                    workspaces(workspaceRoot, manifests.filter((other) => other !== manifest && !(Result.isSuccess(held) && held.success.manifests.includes(other))), files),
                    (rest) => [[manifest, held] as const, ...rest],
                ),
            ),
    });

const externals = (lock: readonly (typeof Locked.Type)[]) => {
    const node = (entry: typeof Locked.Type) => `cargo:${entry.name}@${entry.version}`;
    const named = Array.groupBy(lock, ({ name }) => name);
    const sourced = (spec: string) => {
        const [name = '', version] = spec.split(' ');
        return Option.filter(Array.findFirst(named[name] ?? [], (entry) => version === undefined || entry.version === version), ({ source }) => source !== undefined);
    };
    return {
        nodes: Record.fromIterableWith(
            lock.filter(({ source }) => source !== undefined),
            (entry): readonly [string, ProjectGraphExternalNode] => [node(entry), { type: 'cargo', name: node(entry), data: { version: entry.version, packageName: entry.name, hash: entry.checksum ?? entry.source ?? entry.version } }],
        ),
        edges: lock.flatMap((entry) =>
            entry.dependencies.flatMap((spec) =>
                Option.toArray(sourced(spec)).map(
                    (target): RawProjectGraphDependency =>
                        entry.source === undefined ? { source: entry.name, target: node(target), type: DependencyType.implicit } : { source: node(entry), target: node(target), type: DependencyType.static },
                ),
            ),
        ),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ancestors, CargoError, externals, MANIFEST, Metadata, nearest, spawned, toml, type Workspace, workspaces };
