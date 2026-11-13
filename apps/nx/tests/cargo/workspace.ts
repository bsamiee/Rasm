import { layer } from '@effect/platform-node/NodeServices';
import plugin from '@rasm/nx-cargo/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { Array, Effect, Layer, Option, Path, Schema, String } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CARGO = {
    metadata: 'cargo metadata --format-version 1 --no-deps',
    crate: 'Cargo reference "The name field" of a package and of a target; cargo metadata packages[].targets[].name',
    messages: 'cargo build --message-format=json compiler-artifact filenames',
    depInfo: 'cargo dep-info file target/debug/<binary>.d',
    installed: 'cargo install --list --root <folder>',
    locked: 'cargo metadata --format-version 1 --locked',
    search: 'cargo search <name> --limit 1',
    dryRun: 'cargo publish --dry-run',
} as const;
const PACKAGE = 'edition.workspace = true\nlicense.workspace = true\ndescription = "Fixture crate"\n';
const files = {
    '.config/mise.lock': '# tool versions\n',
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*", "tools/generator"]\n\n[workspace.package]\nversion = "0.1.0"\nedition = "2024"\nlicense = "MIT"\n',
    'vendor/shim/Cargo.toml': `[package]\nname = "rasm-nx-fixture-shim"\nversion.workspace = true\n${PACKAGE}`,
    'vendor/shim/src/lib.rs': 'pub fn unit() -> u32 {\n    1\n}\n',
    'crates/core/Cargo.toml': `[package]\nname = "rasm-nx-fixture-core"\nversion = "0.1.0"\n${PACKAGE}\n[dependencies]\ncfg-if = "1"\nrasm-nx-fixture-shim = { path = "../../vendor/shim", version = "0.1.0" }\n`,
    'crates/core/src/lib.rs': 'pub fn one() -> u32 {\n    cfg_if::cfg_if! {\n        if #[cfg(unix)] {\n            rasm_nx_fixture_shim::unit()\n        } else {\n            1\n        }\n    }\n}\n\n#[cfg(test)]\nmod tests {\n    #[test]\n    fn one_is_one() {\n        assert_eq!(super::one(), 1);\n    }\n}\n',
    'crates/middle/Cargo.toml': `# Middle of the chain\n[package]\nname = "rasm-nx-fixture-middle"\nversion = "0.1.0" # released with the rest\n${PACKAGE}\n[dependencies]\nkernel = { package = "rasm-nx-fixture-core", path = "../core", version = "0.1.0" } # renamed\n`,
    'crates/middle/src/lib.rs': 'pub fn two() -> u32 {\n    kernel::one() + 1\n}\n',
    'crates/app/Cargo.toml': `[package]\nname = "rasm-nx-fixture-app"\nversion = "0.1.0"\n${PACKAGE}\n[dependencies.rasm-nx-fixture-middle]\npath = "../middle"\nversion = "0.1.0"\n`,
    'crates/app/src/main.rs': 'fn main() {\n    println!("{}", rasm_nx_fixture_middle::two());\n}\n',
    'tools/generator/Cargo.toml': `[package]\nname = "rasm-nx-fixture-generator"\nversion = "0.1.0"\npublish = false\n${PACKAGE}\n[dev-dependencies]\nrasm-nx-fixture-core = { path = "../../crates/core" }\n`,
    'tools/generator/src/main.rs': 'fn main() {}\n\n#[cfg(test)]\nmod tests {\n    #[test]\n    fn reads_core() {\n        assert_eq!(rasm_nx_fixture_core::one(), 1);\n    }\n}\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Dependency = Schema.Struct({ name: Schema.String, req: Schema.String, kind: Schema.NullOr(Schema.String), path: Schema.optionalKey(Schema.String) });
const Crate = Schema.Struct({
    id: Schema.String,
    name: Schema.String,
    version: Schema.String,
    manifest_path: Schema.String,
    publish: Schema.NullOr(Schema.Array(Schema.String)),
    targets: Schema.Array(Schema.Struct({ kind: Schema.Array(Schema.String), name: Schema.String, src_path: Schema.String })),
    dependencies: Schema.Array(Dependency),
});
const Metadata = Schema.fromJsonString(Schema.Struct({ packages: Schema.Array(Crate) })).annotate({
    description: 'JSON cargo metadata --format-version 1 --no-deps prints: each workspace member with its id, version, manifest, publish registries, targets, and declared dependencies',
});
const Message = Schema.fromJsonString(Schema.Struct({ reason: Schema.String, package_id: Schema.optionalKey(Schema.String), filenames: Schema.optionalKey(Schema.Array(Schema.String)) })).annotate({
    description: 'One JSON line cargo build --message-format=json prints: a compiler-artifact message names the package and the files its target wrote',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = (path: Path.Path, root: string, { path: at, req, kind }: typeof Dependency.Type) => Option.map(Option.fromNullishOr(at), (folder) => ({ root: path.relative(root, folder), req, kind }));

const described = (path: Path.Path, root: string, { manifest_path, targets, dependencies, publish, ...crate }: typeof Crate.Type) => {
    const [folder, needed] = [path.relative(root, path.dirname(manifest_path)), Array.getSomes(dependencies.map((dependency) => located(path, root, dependency)))];
    return {
        ...crate,
        crate: Option.match(
            Array.findFirst(targets, ({ kind }) => kind.includes('lib')),
            { onNone: () => String.kebabToSnake(crate.name), onSome: ({ name }) => name },
        ),
        root: folder,
        manifest: path.relative(root, manifest_path),
        open: publish === null,
        binary: targets.some(({ kind }) => kind.includes('bin')),
        sources: targets.map(({ src_path }) => path.relative(root, src_path)),
        dependencies: needed.map((dependency) => dependency.root),
        requirements: needed.map((dependency) => `${folder} -> ${dependency.root} ${dependency.req}`),
        waits: needed.filter(({ kind }) => kind !== 'dev').map((dependency) => `${folder} -> ${dependency.root}`),
    };
};

const members = Effect.gen(function* () {
    const [{ root, called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const { checked } = yield* called('cargo', ['metadata', '--format-version', '1', '--no-deps']);
    const { packages } = yield* Effect.flatMap(checked, Schema.decodeEffect(Metadata));
    return packages.map((crate) => described(path, root, crate));
}).pipe(Effect.orDie);

const member = Effect.fnUntraced(function* (root: string) {
    const stated = yield* members;
    return yield* Effect.orDie(Effect.fromOption(Array.findFirst(stated, (candidate) => candidate.root === root)));
});

const artifacts = Effect.fnUntraced(function* (id: string, name: string) {
    const [{ root, called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const { checked } = yield* called('cargo', ['build', '--package', name, '--message-format=json', '--locked']);
    const lines = yield* checked;
    const messages = yield* Effect.forEach(lines.trim().split('\n'), (line) => Schema.decodeEffect(Message)(line));
    return messages.filter(({ reason, package_id }) => reason === 'compiler-artifact' && package_id === id).flatMap(({ filenames = [] }) => filenames.map((file) => path.relative(root, file)));
}, Effect.orDie);

// --- [COMPOSITION] ---------------------------------------------------------------------

const locked = Layer.effectDiscard(
    Effect.gen(function* () {
        const { called, committed } = yield* Consumer;
        yield* called('cargo', ['generate-lockfile']);
        yield* committed('lock');
    }),
);

const workspace = (stated: Readonly<Record<string, string>> = {}, settings: object = {}) =>
    locked.pipe(Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name], release: { projects: ['tag:release'] }, ...settings }, ignored: ['target', '.installed'], files: { ...files, ...stated } })), Layer.provideMerge(Packed.layer(plugin.name)), Layer.provideMerge(layer));

// --- [EXPORTS] -------------------------------------------------------------------------

export { artifacts, CARGO, files, member, members, workspace };
