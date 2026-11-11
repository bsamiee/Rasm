import { findPackageJSON } from 'node:module';
import { env } from 'node:process';
import { fileURLToPath } from 'node:url';
import type { ProjectGraph } from '@nx/devkit';
import { Completed } from '@rasm/process';
import { Array, Cause, Effect, FileSystem, Path, Predicate, Record, Schema } from 'effect';
import { ChildProcess } from 'effect/process';
import type { NxJsonConfiguration } from 'nx/src/config/nx-json';
import { checkout } from '#/nx/inference';

// --- [TYPES] ---------------------------------------------------------------------------

interface Held {
    readonly file: string;
    readonly manifest: typeof Manifest.Type;
}

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);
const Requirements = Schema.optionalKey(Schema.Record(Schema.String, Schema.String));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, version: Schema.optionalKey(Schema.String), bin: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])), dependencies: Requirements, peerDependencies: Requirements }));
const Printed = Schema.fromJsonString(Schema.Struct({ graph: Schema.Struct({ nodes: Schema.Record(Schema.String, Schema.Any), dependencies: Schema.Record(Schema.String, Schema.Any) }) })).annotate({
    description: 'JSON nx graph --print writes: the project graph Nx built, nodes and dependencies in the shape of its ProjectGraph type',
});
const Hashes = Schema.fromJsonString(
    Schema.Record(
        Schema.String,
        Schema.Struct({
            project: Schema.String,
            target: Schema.String,
            hash: Schema.String,
            cache: Schema.Boolean,
            waits: Schema.Array(Schema.String),
            files: Schema.Array(Schema.String),
            runtime: Schema.Array(Schema.String),
            environment: Schema.Array(Schema.String),
            depOutputs: Schema.Array(Schema.String),
            external: Schema.Array(Schema.String),
        }),
    ),
).annotate({ description: 'Per task id: its project and target, the hash Nx gives the task, whether Nx caches it, the continuous tasks it waits on, and the inputs Nx hashed' });
const Run = Schema.fromJsonString(Schema.Struct({ tasks: Schema.Array(Schema.Struct({ taskId: Schema.String, projectName: Schema.String, target: Schema.String, hash: Schema.String, cacheStatus: Schema.Literals(['cache-miss', 'local-cache-hit', 'remote-cache-hit']), status: Schema.Int })) })).annotate({
    description: 'run.json Nx writes into its cache folder after each run: the hash, cache status, and exit code of every task',
});

// --- [ERRORS] --------------------------------------------------------------------------

class NxRefusal extends Schema.TaggedError<NxRefusal, { readonly brand: unique symbol }>()('NxRefusal', { argv: Schema.Array(Schema.String), code: Schema.Int, output: Schema.String }) {
    override get message() {
        return `nx ${this.argv.join(' ')} exited ${this.code}: ${this.output}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const stated = Effect.fnUntraced(function* (name: string, from: string | URL) {
    const fs = yield* FileSystem.FileSystem;
    const file = yield* Effect.flatMap(
        Effect.try(() => findPackageJSON(name, from)),
        Effect.fromNullishOr,
    );
    const manifest = yield* Effect.flatMap(fs.readFileString(file), Schema.decodeEffect(Manifest));
    return { file, manifest };
});

const closure = Effect.fnUntraced(function* (seeds: ReadonlyArray<readonly [name: string, from: string | URL]>, held: Record.ReadonlyRecord<string, Held>): Effect.fn.Return<Record.ReadonlyRecord<string, Held>, never, FileSystem.FileSystem> {
    const read = yield* Effect.forEach(
        Array.dedupeWith(seeds, ([left], [right]) => left === right).filter(([name]) => !(name in held)),
        ([name, from]) => Effect.option(Effect.map(stated(name, from), (entry) => [name, entry] as const)),
    );
    const found = Record.fromEntries(Array.getSomes(read));
    const required = Object.values(found).flatMap(({ file, manifest }) => Record.keys({ ...manifest.dependencies, ...manifest.peerDependencies }).map((name) => [name, file] as const));
    return yield* Record.isEmptyRecord(found) ? Effect.succeed(held) : closure(required, { ...held, ...found });
});

const installed = Effect.fn('installed')(function* (files: Record.ReadonlyRecord<string, string>, packages: readonly string[], resolved: string | URL, nxJson: NxJsonConfiguration, ignored: readonly string[] = []) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const linked = Array.dedupe(['nx', ...packages]);
    const held = yield* closure(
        linked.map((name) => [name, resolved] as const),
        {},
    );
    const manifests = yield* Effect.forEach(linked, (name) => Effect.map(Effect.fromNullishOr(held[name]), ({ manifest }) => manifest));
    const lock = {
        name: 'root',
        lockfileVersion: 3,
        requires: true,
        packages: { '': { name: 'root' }, ...Record.mapEntries(held, ({ manifest: { version = '0.0.0' } }, name) => [`node_modules/${name}`, { version, resolved: `https://registry.npmjs.org/${name}/-/${path.basename(name)}-${version}.tgz`, integrity: 'sha512-0' }]) },
    };
    const [settings, locked] = yield* Effect.all([Schema.encodeUnknownEffect(Json)(nxJson), Schema.encodeUnknownEffect(Json)(lock)]);
    const root = yield* checkout({ '.gitignore': ['.nx', 'node_modules', ...ignored, ''].join('\n'), 'nx.json': settings, 'package-lock.json': locked, ...files }, linked, resolved);
    yield* fs.makeDirectory(path.join(root, 'node_modules/.bin'));
    yield* Effect.forEach(
        Array.zip(linked, manifests).flatMap(([name, { bin = {} }]) => Record.collect(Predicate.isString(bin) ? { [path.basename(name)]: bin } : bin, (command, file) => [command, path.join(root, 'node_modules', name, file)] as const)),
        ([command, file]) => fs.symlink(file, path.join(root, 'node_modules/.bin', command)),
        { discard: true },
    );
    return root;
});

const called = Effect.fn('called')(function* (root: string, command: string, args: readonly string[], set: Record.ReadonlyRecord<string, string>) {
    const { code, stdout, stderr } = yield* Completed.run(ChildProcess.make(command, args, { cwd: root, stdin: 'ignore', extendEnv: false, env: { ...Record.filter(env, (_, name) => !name.startsWith('NX_')), NX_DAEMON: 'false', NX_TUI: 'false', FORCE_COLOR: '0', ...set } }));
    return { code, stdout, stderr, output: `${stdout}${stderr}` };
});

const committed = Effect.fn('committed')(function* (root: string, tags: readonly string[] = []) {
    const rows = [['init', '--quiet'], ['config', 'user.name', 'suite'], ['config', 'user.email', 'suite@example.com'], ['config', 'commit.gpgsign', 'false'], ['config', 'tag.gpgsign', 'false'], ['add', '--all'], ['commit', '--quiet', '--message', 'base'], ...tags.map((tag) => ['tag', tag])];
    yield* Effect.forEach(
        rows,
        (args) =>
            Effect.filterOrFail(
                called(root, 'git', args, {}),
                ({ code }) => code === 0,
                ({ code, output }) => new Cause.UnknownError(output, `git ${args.join(' ')} exited ${code}: ${output}`),
            ),
        { discard: true },
    );
});

const nx = (root: string, args: readonly string[], set: Record.ReadonlyRecord<string, string> = {}) => Effect.flatMap(Path.Path, (path) => called(root, path.join(root, 'node_modules/.bin/nx'), args, set));

const answered = (root: string, args: readonly string[], set: Record.ReadonlyRecord<string, string> = {}) => Effect.flatMap(nx(root, args, set), ({ code, stdout, output }) => (code === 0 ? Effect.succeed(stdout) : Effect.fail(NxRefusal.make({ argv: args, code, output }))));

const graph = (root: string, set: Record.ReadonlyRecord<string, string> = {}) =>
    answered(root, ['show', 'projects'], set).pipe(
        Effect.andThen(answered(root, ['graph', '--print'], set)),
        Effect.flatMap(Schema.decodeEffect(Printed)),
        Effect.map((printed): ProjectGraph => printed.graph),
    );

const ran = Effect.fn('ran')(function* (root: string, args: readonly string[], set: Record.ReadonlyRecord<string, string> = {}) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { code, output } = yield* nx(root, [...args, '--output-style=static'], set);
    const { tasks } = yield* fs.readFileString(path.join(root, '.nx/cache/run.json')).pipe(
        Effect.flatMap(Schema.decodeEffect(Run)),
        Effect.mapError(() => NxRefusal.make({ argv: args, code, output })),
    );
    return { code, output, tasks };
});

const hashed = Effect.fn('hashed')(function* (root: string, set: Record.ReadonlyRecord<string, string> = {}) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const data = yield* fs.makeTempDirectoryScoped({ prefix: 'nx-hash-read-' });
    const script = fileURLToPath(new URL(`./hashes${path.extname(import.meta.url)}`, import.meta.url));
    const { code, stdout, output } = yield* called(root, 'node', [script], { NX_WORKSPACE_DATA_DIRECTORY: data, ...set });
    return yield* code === 0 ? Schema.decodeEffect(Hashes)(stdout) : Effect.fail(NxRefusal.make({ argv: [script], code, output }));
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { answered, called, committed, graph, hashed, installed, NxRefusal, nx, ran };
