import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import plugin from '@rasm/nx-cargo/package.json' with { type: 'json' };
import uv from '@rasm/nx-uv/package.json' with { type: 'json' };
import { conforms } from '@rasm/testing/conformance';
import { Consumer, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, Layer, Option, Path, pipe, Schema, String } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORT = 'cargo metadata --format-version 1 --no-deps';
const [APP, CORE, GENERATOR] = ['crates/app', 'crates/core', 'tools/generator'] as const;
const SOURCES = { core: 'crates/core/src/lib.rs', generator: 'tools/generator/src/main.rs' } as const;
const PACKAGE = 'edition.workspace = true\nlicense.workspace = true\ndescription = "Fixture crate"\n';
const files = {
    '.config/mise.lock': '# tool versions\n',
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*", "tools/*"]\n\n[workspace.package]\nversion = "0.1.0"\nedition = "2024"\nlicense = "MIT"\n\n[workspace.dependencies]\nrasm-nx-fixture-middle = { path = "crates/middle", version = "0.1.0" }\n',
    'vendor/shim/Cargo.toml': `[package]\nname = "rasm-nx-fixture-shim"\nversion.workspace = true\n${PACKAGE}\n[lib]\nname = "shim"\n`,
    'vendor/shim/src/lib.rs': 'pub fn unit() -> u32 {\n    1\n}\n',
    'crates/core/Cargo.toml': `[package]\nname = "rasm-nx-fixture-core"\nversion = "0.1.0"\n${PACKAGE}\n[dependencies]\ncfg-if = "1"\n\n[dependencies.rasm-nx-fixture-shim]\npath = "../../vendor/shim"\nversion = "0.1.0"\n`,
    'crates/core/src/lib.rs': 'pub fn one() -> u32 {\n    cfg_if::cfg_if! {\n        if #[cfg(unix)] {\n            shim::unit()\n        } else {\n            1\n        }\n    }\n}\n\n#[cfg(test)]\nmod tests {\n    #[test]\n    fn one_is_one() {\n        assert_eq!(super::one(), 1);\n    }\n}\n',
    'crates/middle/Cargo.toml': `# Middle of the chain\n[package]\nname = "rasm-nx-fixture-middle"\nversion = "0.1.0" # released with the rest\n${PACKAGE}\n[dependencies]\nkernel = { package = "rasm-nx-fixture-core", path = "../core", version = "0.1.0" } # renamed\n`,
    'crates/middle/src/lib.rs': 'pub fn two() -> u32 {\n    kernel::one() + 1\n}\n',
    'crates/app/Cargo.toml': `[package]\nname = "rasm-nx-fixture-app"\nversion = "0.1.0"\n${PACKAGE}\n[dependencies]\nrasm-nx-fixture-middle.workspace = true\n`,
    'crates/app/src/main.rs': 'fn main() {\n    println!("{}", rasm_nx_fixture_middle::two());\n}\n',
    'tools/generator/Cargo.toml': `[package]\nname = "rasm-nx-fixture-generator"\nversion = "0.1.0"\npublish = false\n${PACKAGE}\n[dev-dependencies]\nrasm-nx-fixture-core = { path = "../../crates/core" }\n`,
    'tools/draft/Cargo.toml': `[package]\nname = "rasm-nx-fixture-draft"\n${PACKAGE}`,
    'tools/draft/src/lib.rs': 'pub fn none() {}\n',
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

const described = (path: Path.Path, root: string, { manifest_path, targets, dependencies, publish, name, ...crate }: typeof Crate.Type) => {
    const [folder, needed] = [path.relative(root, path.dirname(manifest_path)), Array.getSomes(dependencies.map((dependency) => located(path, root, dependency)))];
    const binary = targets.some(({ kind }) => kind.includes('bin'));
    return {
        ...crate,
        crate: name,
        name: String.kebabToSnake(name),
        root: folder,
        manifest: path.relative(root, manifest_path),
        released: crate.version !== '0.0.0',
        public: publish === null,
        verbs: ['build', 'test', 'check', 'fix', 'clean', ...(binary ? ['install'] : [])],
        sources: [path.relative(root, manifest_path), ...targets.map(({ src_path }) => path.relative(root, src_path))],
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

const member = (root: string) => Effect.flatMap(members, (stated) => Effect.orDie(Effect.fromOption(Array.findFirst(stated, (candidate) => candidate.root === root))));

const built = Effect.fnUntraced(function* ({ id, crate }: Effect.Success<typeof members>[number]) {
    const [{ root, called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const { checked } = yield* called('cargo', ['build', '--package', crate, '--message-format=json', '--locked']);
    const lines = yield* checked;
    const messages = yield* Effect.forEach(lines.trim().split('\n'), (line) => Schema.decodeEffect(Message)(line));
    return messages.filter(({ reason, package_id }) => reason === 'compiler-artifact' && package_id === id).flatMap(({ filenames = [] }) => filenames.map((file) => path.relative(root, file)));
}, Effect.orDie);

// --- [COMPOSITION] ---------------------------------------------------------------------

const bound = Duration.toMillis('5 minutes');
const options = { timeout: bound, excludeTestServices: true } as const;

const locked = Effect.fnUntraced(function* (locks: readonly (readonly [string, ...string[]])[]) {
    const { called, committed } = yield* Consumer;
    yield* Effect.forEach(locks, ([command, ...args]) => called(command, args));
    yield* committed('lock');
});

const workspace = (stated: Readonly<Record<string, string>> = {}, release: object = {}, plugins: readonly string[] = [plugin.name], locks: readonly (readonly [string, ...string[]])[] = [['cargo', 'generate-lockfile']]) =>
    Layer.effectDiscard(locked(locks)).pipe(
        Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [...plugins], release: { projects: ['tag:release'], ...release } }, ignored: ['target', '.installed', '.venv'], files: { ...files, ...stated } })),
        Layer.provideMerge(Packed.layer(...plugins)),
        Layer.provideMerge(layer),
    );

conforms({
    workspace,
    members,
    built,
    report: REPORT,
    tools: ['cargo'],
    verbs: ['build', 'test', 'check', 'fix', 'clean', 'install'],
    shared: ['Cargo.toml', 'Cargo.lock'],
    unlocked: ['Cargo.lock'],
    scratch: [],
    added: Consumer.use(({ wrote }) =>
        wrote({
            'crates/extra/Cargo.toml': '[package]\nname = "rasm-nx-fixture-extra"\nversion = "0.1.0"\nedition.workspace = true\n',
            'crates/extra/src/lib.rs': 'pub fn zero() -> u32 {\n    0\n}\n',
            'tools/generator/Cargo.toml': `${files['tools/generator/Cargo.toml']}rasm-nx-fixture-extra = { path = "../../crates/extra" }\n`,
        }),
    ),
    published: true,
    refused: { 'broken/Cargo.toml': '[package\nname = ' },
    manifests: (next) => ({
        'Cargo.toml': files['Cargo.toml'].replaceAll('"0.1.0"', `"${next}"`),
        'vendor/shim/Cargo.toml': files['vendor/shim/Cargo.toml'],
        'crates/core/Cargo.toml': files['crates/core/Cargo.toml'].replaceAll('"0.1.0"', `"${next}"`),
        'crates/middle/Cargo.toml': files['crates/middle/Cargo.toml'].replaceAll('"0.1.0"', `"${next}"`),
        'crates/app/Cargo.toml': files['crates/app/Cargo.toml'].replaceAll('"0.1.0"', `"${next}"`),
    }),
    edits: {
        verbs: ['build', 'test'],
        dependent: APP,
        upstream: (mark) => ({ [SOURCES.core]: `${files[SOURCES.core]}\npub fn probe_${mark}() -> u32 {\n    7\n}\n` }),
        outside: (mark) => ({ [SOURCES.generator]: `${files[SOURCES.generator]}\npub fn probe_${mark}() {}\n`, 'crates/app/notes.md': `${mark}\n` }),
        tested: CORE,
        failing: { [SOURCES.core]: files[SOURCES.core].replace('super::one(), 1', 'super::one(), 2') },
    },
});

it.layer(
    workspace(),
    options,
)((methods) => {
    methods.effect(
        'releases a crate stating a version without a publish target where publish is false, and no crate stating none',
        Effect.fnUntraced(function* () {
            const [{ read }, draft, generator] = yield* Effect.all([Consumer, member('tools/draft'), member(GENERATOR)]);
            const { nodes } = yield* read;
            const held = (name: string) => ({ tagged: (nodes[name]?.data.tags ?? []).includes('release'), targets: Array.intersection(Object.keys(nodes[name]?.data.targets ?? {}), ['pack--cargo', 'attach--gh', 'nx-release-publish']) });
            assert.deepStrictEqual(held(generator.name).tagged, true, 'README.md PRINCIPLES "Release projects are versioned, packed, and attached, and the switch their registry reads alone gives one a publish target"');
            assert.sameMembers(held(generator.name).targets, ['pack--cargo', 'attach--gh'], 'Cargo reference manifest "publish = false" prevents a package from being published to a registry');
            assert.deepStrictEqual(held(draft.name), { tagged: false, targets: [] }, 'README.md PRINCIPLES "Crates stating no version are no release project"; Cargo reference manifest "The version field is optional ... required for publishing packages"');
        }),
        bound,
    );

    methods.effect(
        'names a crate by its package name with hyphens as underscores, a [lib] name set apart from it included',
        Effect.fnUntraced(function* () {
            const [{ read }, { name, root }] = yield* Effect.all([Consumer, member('vendor/shim')]);
            const { nodes } = yield* read;
            assert.strictEqual(nodes[name]?.data.root, root, `${REPORT} packages[].name: Nx names are unique in a workspace, and a uv member can hold the same hyphenated name`);
        }),
        bound,
    );

    methods.effect(
        'gives a warm graph the install target a new binary source adds to a library crate',
        Effect.fnUntraced(function* () {
            const { read, wrote } = yield* Consumer;
            yield* read;
            yield* wrote({ 'crates/middle/src/bin/report.rs': 'fn main() {\n    println!("{}", rasm_nx_fixture_middle::two());\n}\n' });
            const [{ nodes }, { name, verbs }] = yield* Effect.all([read, member('crates/middle')]);
            assert.include(verbs, 'install', `${REPORT} packages[].targets[].kind`);
            assert.property(nodes[name]?.data.targets, 'install', 'nrwl/nx packages/dotnet/src/analyzer/analyzer-client.spec.ts "should rerun the analyzer when an evaluation input changed"');
        }),
        bound,
    );

    methods.effect(
        'hashes every source file cargo lists in the dep-info of a binary',
        Effect.fnUntraced(function* () {
            const [{ root, nx, text, inputs }, path, { name, crate }] = yield* Effect.all([Consumer, Path.Path, member(APP)]);
            const run = yield* nx('build', name, '--output-style=static');
            assert.strictEqual(run.code, 0, `${run.stdout}${run.stderr}`);
            const listed = yield* text(`target/debug/${crate}.d`);
            const read = listed
                .trim()
                .slice(listed.indexOf(': ') + 2)
                .split(' ')
                .map((file) => path.relative(root, file));
            const { files: hashed } = yield* inputs(`${name}:build`);
            assert.isAbove(read.length, 1, 'cargo dep-info file target/debug/<binary>.d');
            assert.deepStrictEqual(Array.difference(read, hashed), [], 'cargo dep-info file target/debug/<binary>.d; nrwl/nx astro-docs/src/content/docs/reference/inputs.mdoc');
        }),
        bound,
    );

    methods.effect.each(['--batch', '--no-batch'])(
        'builds every crate with %s',
        Effect.fnUntraced(function* (flag) {
            const [{ nx, cleaned, absent }, stated] = yield* Effect.all([Consumer, member(APP)]);
            yield* cleaned;
            const run = yield* nx('run-many', '--targets=build', '--skip-nx-cache', flag, '--output-style=static');
            assert.strictEqual(run.code, 0, `nrwl/nx e2e/gradle/src/gradle.test.ts batch runs: ${run.stdout}${run.stderr}`);
            assert.deepStrictEqual(yield* absent(yield* built(stated)), [], 'cargo build --message-format=json compiler-artifact filenames');
        }),
        bound,
    );

    methods.effect(
        'fails in one batch only the crate that does not compile, and prints the compiler error',
        Effect.fnUntraced(function* () {
            const [{ nx, wrote }, broken, sound] = yield* Effect.all([Consumer, member(GENERATOR), member(APP)]);
            yield* wrote({ [SOURCES.generator]: 'fn main() {\n    absent_function();\n}\n' });
            const run = yield* nx('run-many', '--targets=build', '--skip-nx-cache', '--batch', '--nx-bail=false', '--output-style=static');
            yield* wrote({ [SOURCES.generator]: files[SOURCES.generator] });
            const [printed, source] = [`${run.stdout}${run.stderr}`, 'nrwl/nx packages/gradle/src/executors/gradle/gradle-batch.impl.spec.ts: a batch reports one result per task'];
            const failed = printed.slice(printed.indexOf('Failed tasks:'));
            assert.notStrictEqual(run.code, 0, source);
            assert.include(printed, 'absent_function', source);
            assert.include(failed, `${broken.name}:build`, source);
            assert.notInclude(failed, `${sound.name}:build`, source);
        }),
        bound,
    );

    methods.effect(
        'installs a binary crate where cargo lists it as installed',
        Effect.fnUntraced(function* () {
            const [{ root, called }, path, { name, crate, version }] = yield* Effect.all([Consumer, Path.Path, member(APP)]);
            const folder = path.join(root, '.installed');
            const installed = yield* called('env', [`CARGO_INSTALL_ROOT=${folder}`, 'nx', 'install', name, '--output-style=static']);
            const listed = yield* called('cargo', ['install', '--list', '--root', folder]);
            assert.strictEqual(installed.code, 0, `${installed.stdout}${installed.stderr}`);
            assert.include(listed.stdout, `${crate} v${version}`, 'README.md TASKS "install puts a local build into its host"; cargo install --list --root <folder>');
        }),
        bound,
    );

    methods.effect(
        'removes the target folder with the clean of the workspace root',
        Effect.fnUntraced(function* () {
            const { read, nx, absent } = yield* Consumer;
            const { nodes } = yield* read;
            const root = yield* Effect.orDie(Effect.fromOption(Array.findFirst(Object.values(nodes), ({ data }) => data.root === '.')));
            yield* nx('run-many', '--targets=build', '--output-style=static');
            assert.deepStrictEqual(yield* absent(['target']), [], REPORT);
            const run = yield* nx('clean', root.name, '--output-style=static');
            assert.strictEqual(run.code, 0, `${run.stdout}${run.stderr}`);
            assert.deepStrictEqual(yield* absent(['target']), ['target'], 'cargo clean --help "Remove artifacts that cargo has generated in the past"; README.md TASKS "clean ... runs its tool\'s clean"');
        }),
        bound,
    );
});

it.layer(
    workspace(),
    options,
)((methods) => {
    methods.effect(
        'moves the requirement of every dependent to the new version, a renamed dependency, a table, and one inherited from the workspace included, and leaves a path-only one and the lock in line',
        Effect.fnUntraced(function* () {
            const { nx, called } = yield* Consumer;
            const versioned = yield* nx('release', 'version', '0.2.0');
            assert.strictEqual(versioned.code, 0, `${versioned.stdout}${versioned.stderr}`);
            const stated = yield* members;
            assert.sameMembers(
                stated.flatMap(({ requirements }) => requirements),
                ['crates/core -> vendor/shim ^0.2.0', 'crates/middle -> crates/core ^0.2.0', 'crates/app -> crates/middle ^0.2.0', 'tools/generator -> crates/core *'],
                `${REPORT} packages[].dependencies[].req; Cargo reference "Inheriting a dependency from a workspace"; nrwl/nx packages/nx/src/command-line/release/version/release-group-processor.spec.ts: dependents move with a fixed group`,
            );
            const inline = yield* called('cargo', ['metadata', '--format-version', '1', '--locked']);
            assert.strictEqual(inline.code, 0, `nrwl/nx e2e/release/src/lock-file-updates.test.ts; cargo metadata --locked: ${inline.stderr}`);
        }),
        bound,
    );

    methods.effect(
        'runs the dry run of cargo publish for every publishable crate under nx release publish --dry-run, uploading nothing',
        Effect.fnUntraced(function* () {
            const { nx, committed } = yield* Consumer;
            yield* committed('release');
            const published = yield* nx('release', 'publish', '--dry-run');
            const [printed, source] = [`${published.stdout}${published.stderr}`, 'cargo publish --dry-run; nrwl/nx astro-docs/src/content/docs/kb/publish-rust-crates.mdoc'];
            assert.strictEqual(published.code, 0, `${source}: ${printed}`);
            assert.include(printed, 'aborting upload due to dry run', source);
            assert.notInclude(printed, 'Uploaded', source);
        }),
        bound,
    );
});

it.layer(
    workspace({ 'crates/middle/Cargo.toml': '[package\nname = ' }),
    options,
)((methods) => {
    methods.effect(
        'names the member manifest cargo names in its refusal',
        Effect.fnUntraced(function* () {
            const { failures } = yield* Effect.flatMap(Consumer, ({ read }) => read);
            assert.include([...failures, ...failures.flatMap(({ files: named }) => named)].map(({ message }) => message).join('\n'), 'crates/middle/Cargo.toml', 'cargo metadata stderr; nrwl/nx packages/nx/src/project-graph/plugins/utils.spec.ts "should handle partial errors"');
        }),
        bound,
    );
});

it.layer(
    workspace(
        { 'pyproject.toml': '[tool.uv.workspace]\nmembers = ["packages/*"]\n', 'packages/shim/pyproject.toml': '[project]\nname = "rasm-nx-fixture-shim"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = []\n' },
        {},
        [plugin.name, uv.name],
        [
            ['cargo', 'generate-lockfile'],
            ['uv', 'lock'],
        ],
    ),
    options,
)((methods) => {
    methods.effect(
        'builds a graph holding a crate and a uv member of one package name as two projects',
        Effect.fnUntraced(function* () {
            const { nodes, failures } = yield* Effect.flatMap(Consumer, ({ read }) => read);
            assert.deepStrictEqual(failures, [], 'nrwl/nx packages/nx/src/project-graph/error-types.ts MultipleProjectsWithSameNameError');
            assert.includeMembers(
                Object.values(nodes).map(({ data }) => data.root),
                ['vendor/shim', 'packages/shim'],
                `${REPORT}; uv workspace list`,
            );
        }),
        bound,
    );
});

it.layer(
    Layer.effectDiscard(Effect.flatMap(Consumer, ({ called }) => called('cargo', ['generate-lockfile']))).pipe(
        Layer.provideMerge(
            Consumer.layer({
                resolved: import.meta.url,
                settings: { plugins: [plugin.name], release: { projects: ['tag:release'], version: { currentVersionResolver: 'registry' } } },
                ignored: ['target'],
                files: { 'Cargo.toml': '[package]\nname = "itoa"\nversion = "0.0.1"\nedition = "2024"\n', 'src/lib.rs': '' },
            }),
        ),
        Layer.provideMerge(Packed.layer(plugin.name)),
        Layer.provideMerge(layer),
    ),
    options,
)((methods) => {
    methods.effect(
        'reads the current version of a crate from the registry cargo searches',
        Effect.fnUntraced(function* () {
            const { nx, called } = yield* Consumer;
            const found = yield* called('cargo', ['search', 'itoa', '--limit', '1']);
            const latest = yield* pipe(
                Array.findFirst(found.stdout.split('\n'), (line) => line.startsWith('itoa = "')),
                Option.flatMap((line) => Array.get(line.split('"'), 1)),
                Effect.fromOption,
                Effect.orDie,
            );
            const shown = yield* nx('release', 'version', 'patch', '--dry-run');
            assert.include(`${shown.stdout}${shown.stderr}`, `Resolved the current version as ${latest} from the remote registry`, 'cargo search <name> --limit 1; nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.ts "registry"');
        }),
        bound,
    );
});
