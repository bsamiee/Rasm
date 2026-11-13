import { dirname, isAbsolute, join, relative } from 'node:path';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { ancestors } from '@rasm/nx-devkit/project';
import { Array, Effect, FileSystem, flow, Option, Record, Result, Schema, UndefinedOr } from 'effect';
import { Toml } from 'effect/encoding';

// --- [TYPES] ---------------------------------------------------------------------------

type Reading = Effect.Success<ReturnType<typeof read>>;
type Workspace = ReturnType<typeof resolved>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Cargo.toml';
const LOCK = 'Cargo.lock';
const CRATE = 'nx_cargo';
const DENY = Record.map({ config: 'deny', exceptions: 'deny.exceptions' }, (stem) => [`${stem}.toml`, `.${stem}.toml`, `.cargo/${stem}.toml`]);

// --- [MODELS] --------------------------------------------------------------------------

const Target = Schema.Struct({ kind: Schema.Array(Schema.String), crate_types: Schema.Array(Schema.String), name: Schema.String, src_path: Schema.String, test: Schema.Boolean, doctest: Schema.Boolean });
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
        build_directory: Schema.optionalKey(Schema.String),
        metadata: Schema.NullOr(Schema.Struct({ dylint: Schema.optionalKey(Schema.Struct({ libraries: Schema.Array(Schema.Struct({ path: Schema.optionalKey(Schema.String) })) })) })),
    }),
);
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Profiles = Schema.Struct({ profile: Schema.Record(Schema.String, Schema.Struct({ inherits: Schema.optionalKey(Schema.String), panic: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const nearest = (files: ReadonlySet<string>, folder: string, names: readonly string[]) => Array.findFirst(ancestors(folder), (at) => Array.findFirst(names, (name) => files.has(join(at, name))).pipe(Option.map((name) => join(at, name))));

const toml = <S extends Schema.Decoder<unknown>>(schema: S, file: string) =>
    FileSystem.FileSystem.pipe(
        Effect.flatMap((fs) => fs.readFileString(file)),
        Effect.flatMap(flow(Toml.parse, Effect.fromResult)),
        Effect.flatMap(Schema.decodeUnknownEffect(schema)),
        Effect.mapError(ToolError.of(file, 'does not read as the TOML table it declares')),
    );

const read = Effect.fnUntraced(
    function* (workspaceRoot: string, manifest: string, files: ReadonlySet<string>) {
        const metadata = yield* Effect.flatMap(printed('cargo', ['metadata', '--format-version', '1', '--no-deps', '--manifest-path', manifest], { cwd: workspaceRoot }), Schema.decodeEffect(Metadata));
        const root = relative(workspaceRoot, metadata.workspace_root) || '.';
        const types = Array.difference(Array.dedupe(metadata.packages.flatMap(({ targets }) => targets.flatMap((target) => target.crate_types))), ['lib', 'rlib']);
        const [{ profile }, names, denied] = yield* Effect.all(
            [
                toml(Profiles, join(metadata.workspace_root, MANIFEST)),
                Effect.map(printed('rustc', ['--print', 'file-names', '--crate-name', CRATE, ...types.flatMap((type) => ['--crate-type', type]), '-'], { cwd: metadata.workspace_root, input: '' }), (text) => text.split('\n')),
                Effect.transposeOption(Option.map(nearest(files, root, DENY.config), (file) => Effect.map(toml(Table, join(workspaceRoot, file)), (tables) => ({ files: [file, ...Option.toArray(nearest(files, root, DENY.exceptions))], tables: Record.keys(tables) })))),
            ],
            { concurrency: 'unbounded' },
        );
        return { metadata, profile, uplifted: Record.fromEntries(Array.zip(types, names)), deny: Option.toArray(denied) };
    },
    (effect, _, manifest) => Effect.mapError(effect, ToolError.of(manifest, 'does not resolve as a Cargo workspace')),
);

const resolved = (workspaceRoot: string, { metadata, profile, uplifted, deny }: Reading) => {
    const within = (at: string) => relative(workspaceRoot, at) || '.';
    const inside = (at: string) => Option.liftPredicate(within(at), (folder) => !(folder.startsWith('..') || isAbsolute(folder)));
    const root = within(metadata.workspace_root);
    const profiles = Record.keys(Record.filter(profile, ({ inherits }) => inherits !== undefined));
    const strategy = (name: string): string => profile[name]?.panic ?? UndefinedOr.match(profile[name]?.inherits, { onUndefined: () => 'unwind', onDefined: strategy });
    return {
        root,
        members: metadata.packages.map((crate) => ({ crate, root: within(dirname(crate.manifest_path)), manifest: within(crate.manifest_path) })),
        libraries: Array.getSomes((metadata.metadata?.dylint?.libraries ?? []).map(({ path: at }) => Option.map(Option.fromUndefinedOr(at), (folder) => join(root, folder)))),
        profiles,
        strategies: Array.dedupeWith(['release', ...profiles], (left, right) => strategy(left) === strategy(right)).filter((name) => strategy(name) !== strategy('dev')),
        deny: Array.head(deny),
        placed: Option.all({ artifacts: inside(metadata.target_directory), units: inside(metadata.build_directory ?? metadata.target_directory) }),
        uplifted: (target: typeof Target.Type) => Array.getSomes(target.crate_types.map((type) => Option.map(Record.get(uplifted, type), (file) => file.replace(CRATE, target.name)))),
    };
};

const workspaces = (workspaceRoot: string, manifests: readonly string[], files: ReadonlySet<string>) =>
    Effect.reduce(
        manifests.toSorted((left, right) => left.split('/').length - right.split('/').length || left.localeCompare(right)),
        (): readonly (readonly [string, Result.Result<Reading, ToolError>])[] => [],
        (done, manifest) =>
            done.some(([, reading]) => Result.isSuccess(reading) && reading.success.metadata.packages.some(({ manifest_path }) => relative(workspaceRoot, manifest_path) === manifest))
                ? Effect.succeed(done)
                : Effect.map(Effect.result(read(workspaceRoot, manifest, files)), (reading) => [...done, [manifest, reading] as const]),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { LOCK, MANIFEST, Metadata, type Reading, resolved, type Workspace, workspaces };
