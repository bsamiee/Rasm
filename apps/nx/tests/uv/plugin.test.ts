import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import plugin from '@rasm/nx-uv/package.json' with { type: 'json' };
import { conforms } from '@rasm/testing/conformance';
import { Consumer, fromCache, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, FileSystem, Layer, Option, Path, pipe, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORT = 'uv workspace metadata; uv build --package <name> --out-dir <folder>; tar -tzf of the sdist';
const [CORE, MIDDLE, APP] = ['packages/core', 'packages/middle', 'packages/app'] as const;
const SOURCES = { core: 'packages/core/src/rasm_nx_fixture_core/__init__.py', report: 'tools/report/report.py', tests: 'packages/middle/tests/test_middle.py' } as const;
const BACKEND = '\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n';
const PROJECT = 'version = "0.1.0"\ndescription = "Fixture member"\nrequires-python = ">=3.13"\n';
const files = {
    '.config/mise.lock': '# tool versions\n',
    'pyproject.toml': '[tool.uv.workspace]\nmembers = ["packages/*", "tools/report"]\n',
    'packages/core/pyproject.toml': `[project]\nname = "rasm-nx-fixture-core"\n${PROJECT}dependencies = []\n${BACKEND}`,
    'packages/core/src/rasm_nx_fixture_core/__init__.py': 'def one() -> int:\n    return 1\n',
    'packages/middle/pyproject.toml': `# Middle of the chain\n[project]\nname = "rasm-nx-fixture-middle"\n${PROJECT}dependencies = ["rasm-nx-fixture-core==0.1.0"]\n\n[dependency-groups]\ndev = ["pytest>=8"] # test runner\n\n[tool.uv.sources]\nrasm-nx-fixture-core = { workspace = true }\n${BACKEND}`,
    'packages/middle/src/rasm_nx_fixture_middle/__init__.py': 'from rasm_nx_fixture_core import one\n\n\ndef two() -> int:\n    return one() + 1\n',
    'packages/middle/tests/test_middle.py': 'from rasm_nx_fixture_middle import two\n\n\ndef test_two() -> None:\n    assert two() == 2\n',
    'packages/app/pyproject.toml': `[project]\nname = "rasm-nx-fixture-app"\n${PROJECT}dependencies = ["rasm-nx-fixture-middle"]\n\n[project.scripts]\nrasm-nx-fixture-app = "rasm_nx_fixture_app:main"\n\n[tool.uv.sources]\nrasm-nx-fixture-middle = { workspace = true }\n${BACKEND}`,
    'packages/app/src/rasm_nx_fixture_app/__init__.py': 'from rasm_nx_fixture_middle import two\n\n\ndef main() -> None:\n    print(two())\n',
    'tools/report/pyproject.toml': `[project]\nname = "rasm-nx-fixture-report"\n${PROJECT}dependencies = []\n\n[dependency-groups]\ndev = ["rasm-nx-fixture-core"]\n\n[tool.uv]\npackage = false\n\n[tool.uv.sources]\nrasm-nx-fixture-core = { workspace = true }\n`,
    'tools/report/report.py': 'from rasm_nx_fixture_core import one\n\nprint(one())\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Resolved = Schema.Struct({ name: Schema.optionalKey(Schema.String), version: Schema.optionalKey(Schema.String), dependencies: Schema.Array(Schema.Struct({ id: Schema.String })) });
const Stated = Schema.Struct({ name: Schema.String, path: Schema.String, id: Schema.String });
const Metadata = Schema.fromJsonString(Schema.Struct({ members: Schema.Array(Stated), resolution: Schema.Record(Schema.String, Resolved) })).annotate({
    description: 'JSON uv workspace metadata prints: each workspace member with its folder and id, and per resolved package and dependency group its name, version, and the ids it depends on',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const described = (path: Path.Path, root: string, folders: Record.ReadonlyRecord<string, string>, resolution: readonly (typeof Resolved.Type)[], { name, path: at }: typeof Stated.Type) => {
    const resolved = resolution.filter((entry) => entry.name === name);
    const needed = Array.dedupe(resolved.flatMap(({ dependencies }) => dependencies.map((dependency) => dependency.id)));
    const folder = path.relative(root, at);
    return {
        name,
        version: Array.getSomes(resolved.map(({ version }) => Option.fromNullishOr(version))).join(','),
        root: folder,
        manifest: path.join(folder, 'pyproject.toml'),
        tested: needed.some((dependency) => dependency.startsWith('pytest==')),
        dependencies: Array.difference(Array.getSomes(needed.map((dependency) => Record.get(folders, dependency))), [folder]),
    };
};

const reported = Effect.gen(function* () {
    const [{ root, called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const { checked } = yield* called('uv', ['workspace', 'metadata', '--preview-features', 'workspace-metadata']);
    const { members: stated, resolution } = yield* Effect.flatMap(checked, Schema.decodeEffect(Metadata));
    const folders = Record.fromIterableWith(stated, ({ id, path: folder }) => [id, path.relative(root, folder)]);
    return stated.map((entry) => described(path, root, folders, Object.values(resolution), entry));
}).pipe(Effect.orDie);

const distributions = Effect.fnUntraced(
    function* (name: string, root: string) {
        const [{ called }, fs, path] = yield* Effect.all([Consumer, FileSystem.FileSystem, Path.Path]);
        const folder = yield* fs.makeTempDirectoryScoped({ prefix: 'uv-build-' });
        const built = yield* called('uv', ['build', '--package', name, '--out-dir', folder]);
        const entries = Number(built.code) === 0 ? yield* fs.readDirectory(folder) : [];
        const names = entries.filter((entry) => ['.whl', '.gz'].includes(path.extname(entry)));
        const listed = yield* Effect.forEach(
            names.filter((entry) => path.extname(entry) === '.gz'),
            (archive) => called('tar', ['-tzf', path.join(folder, archive)]),
        );
        const packed = listed.flatMap(({ stdout }) => stdout.split('\n')).map((entry) => entry.split('/').slice(1).join('/'));
        return { names, sources: packed.filter((entry) => ['.py', '.toml'].includes(path.extname(entry))).map((entry) => path.join(root, entry)) };
    },
    Effect.scoped,
    Effect.orDie,
);

const members = Effect.gen(function* () {
    const stated = yield* reported;
    return yield* Effect.forEach(stated, ({ tested, ...entry }) =>
        Effect.map(distributions(entry.name, entry.root), ({ names, sources }) => ({
            ...entry,
            names,
            sources,
            released: Array.isReadonlyArrayNonEmpty(names),
            verbs: [...(Array.isReadonlyArrayNonEmpty(names) ? ['build'] : []), ...(tested ? ['test'] : [])],
        })),
    );
});

const member = (root: string) => Effect.flatMap(members, (stated) => Effect.orDie(Effect.fromOption(Array.findFirst(stated, (candidate) => candidate.root === root))));

const built = Effect.fnUntraced(function* ({ names }: Effect.Success<typeof members>[number]) {
    const [{ called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const { stdout } = yield* called('git', ['ls-files', '--others', '--', ':/', ':!node_modules', ':!.nx', ':!.venv']);
    return stdout.split('\n').filter((file) => names.includes(path.basename(file)));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const bound = Duration.toMillis('5 minutes');
const options = { timeout: bound, excludeTestServices: true } as const;

const locked = Layer.effectDiscard(
    Effect.gen(function* () {
        const { called, committed } = yield* Consumer;
        yield* called('uv', ['lock']);
        yield* committed('lock');
    }),
);

const workspace = (stated: Readonly<Record<string, string>> = {}, release: object = {}) =>
    locked.pipe(
        Layer.provideMerge(
            Consumer.layer({
                resolved: import.meta.url,
                settings: { plugins: [plugin.name], release: { projects: ['tag:release'], ...release } },
                ignored: ['.venv', 'dist', '__pycache__', '.pytest_cache', '*.egg-info', '.tools', '.latest'],
                files: { ...files, ...stated },
            }),
        ),
        Layer.provideMerge(Packed.layer(plugin.name)),
        Layer.provideMerge(layer),
    );

conforms({
    workspace,
    members,
    built,
    report: REPORT,
    tools: ['uv'],
    verbs: ['build', 'test'],
    shared: ['pyproject.toml', 'uv.lock'],
    unlocked: ['uv.lock', '.venv'],
    scratch: [],
    added: Effect.gen(function* () {
        const { called, wrote } = yield* Consumer;
        yield* wrote({
            'packages/extra/pyproject.toml': files['packages/core/pyproject.toml'].replace('rasm-nx-fixture-core', 'rasm-nx-fixture-extra'),
            'packages/extra/src/rasm_nx_fixture_extra/__init__.py': 'ZERO = 0\n',
            'tools/report/pyproject.toml': files['tools/report/pyproject.toml'].replace('dev = ["rasm-nx-fixture-core"]', 'dev = ["rasm-nx-fixture-core", "rasm-nx-fixture-extra"]').concat('rasm-nx-fixture-extra = { workspace = true }\n'),
        });
        yield* called('uv', ['lock']);
    }),
    published: true,
    refused: { 'broken/pyproject.toml': '[project\nname = ' },
    manifests: (next) => ({
        'packages/core/pyproject.toml': files['packages/core/pyproject.toml'].replaceAll('0.1.0', next),
        'packages/middle/pyproject.toml': files['packages/middle/pyproject.toml'].replaceAll('0.1.0', next),
        'packages/app/pyproject.toml': files['packages/app/pyproject.toml'].replaceAll('0.1.0', next),
    }),
    edits: {
        verbs: ['test'],
        dependent: MIDDLE,
        upstream: (mark) => ({ [SOURCES.core]: `${files[SOURCES.core]}\n\ndef probe_${mark}() -> int:\n    return 7\n` }),
        outside: (mark) => ({ [SOURCES.report]: `${files[SOURCES.report]}print("${mark}")\n` }),
        tested: MIDDLE,
        failing: { [SOURCES.tests]: files[SOURCES.tests].replace('two() == 2', 'two() == 3') },
    },
});

it.layer(
    workspace(),
    options,
)((methods) => {
    methods.effect(
        'hits the build cache of a member after a file uv packs into no distribution appears inside it',
        Effect.fnUntraced(function* () {
            const [{ nx, wrote }, { name }] = yield* Effect.all([Consumer, member(CORE)]);
            const run = nx('build', name, '--output-style=static');
            yield* run;
            yield* wrote({ 'packages/core/notes.md': 'not an input\n' });
            const { sources } = yield* member(CORE);
            assert.notInclude([...sources], 'packages/core/notes.md', REPORT);
            assert.match((yield* run).stdout, fromCache(`${name}:build--`), `${REPORT}; nrwl/nx e2e/gradle/src/gradle-cache.test.ts "should rebuild on source and resource changes and hit cache otherwise"`);
        }),
        bound,
    );

    methods.effect(
        'installs a member with a script where uv lists it as an installed tool',
        Effect.fnUntraced(function* () {
            const [{ root, called }, path, { name, version }] = yield* Effect.all([Consumer, Path.Path, member(APP)]);
            const folders = [`UV_TOOL_DIR=${path.join(root, '.tools')}`, `UV_TOOL_BIN_DIR=${path.join(root, '.tools/bin')}`];
            const installed = yield* called('env', [...folders, 'nx', 'install', name, '--output-style=static']);
            const listed = yield* called('env', [...folders, 'uv', 'tool', 'list']);
            assert.strictEqual(installed.code, 0, `${installed.stdout}${installed.stderr}`);
            assert.include(listed.stdout, `${name} v${version}`, 'README.md TASKS "install puts a local build into its host"; uv tool list');
        }),
        bound,
    );
});

it.layer(
    workspace(),
    options,
)((methods) => {
    methods.effect(
        'moves the pinned requirement of a dependent to the new version, and leaves the lock in line with the manifests',
        Effect.fnUntraced(function* () {
            const { nx, called, text } = yield* Consumer;
            const versioned = yield* nx('release', 'version', '0.2.0');
            assert.strictEqual(versioned.code, 0, `${versioned.stdout}${versioned.stderr}`);
            const checked = yield* called('uv', ['lock', '--check']);
            assert.include(yield* text('packages/middle/pyproject.toml'), '"rasm-nx-fixture-core==0.2.0"', 'nrwl/nx packages/nx/src/command-line/release/version/release-group-processor.spec.ts: dependents move with a fixed group');
            assert.strictEqual(checked.code, 0, `nrwl/nx e2e/release/src/lock-file-updates.test.ts; uv lock --check: ${checked.stderr}`);
        }),
        bound,
    );

    methods.effect(
        'runs the dry run of uv publish for every publishable member under nx release publish --dry-run, uploading nothing',
        Effect.fnUntraced(function* () {
            const { nx, committed } = yield* Consumer;
            yield* committed('release');
            const published = yield* nx('release', 'publish', '--dry-run');
            const [printed, source] = [`${published.stdout}${published.stderr}`, 'uv publish --dry-run; nrwl/nx e2e/release/src/release-publishable-libraries.test.ts'];
            assert.strictEqual(published.code, 0, `${source}: ${printed}`);
            assert.include(printed, 'Checking', source);
            assert.notInclude(printed, 'Uploading', source);
        }),
        bound,
    );
});

it.layer(
    workspace({ 'packages/middle/pyproject.toml': '[project\nname = ' }),
    options,
)((methods) => {
    methods.effect(
        'names the member manifest uv refuses in the graph failure',
        Effect.fnUntraced(function* () {
            const { failures } = yield* Effect.flatMap(Consumer, ({ read }) => read);
            assert.include([...failures, ...failures.flatMap(({ files: named }) => named)].map(({ message }) => message).join('\n'), 'packages/middle/pyproject.toml', 'uv stderr; nrwl/nx packages/nx/src/project-graph/plugins/utils.spec.ts "should handle partial errors"');
        }),
        bound,
    );
});

it.layer(
    Layer.effectDiscard(Effect.flatMap(Consumer, ({ called }) => called('uv', ['lock']))).pipe(
        Layer.provideMerge(
            Consumer.layer({
                resolved: import.meta.url,
                settings: { plugins: [plugin.name], release: { projects: ['tag:release'], version: { currentVersionResolver: 'registry' } } },
                ignored: ['.venv', 'dist', '.latest'],
                files: { 'packages/six/pyproject.toml': files['packages/core/pyproject.toml'].replace('rasm-nx-fixture-core', 'six'), 'packages/six/src/six/__init__.py': '' },
            }),
        ),
        Layer.provideMerge(Packed.layer(plugin.name)),
        Layer.provideMerge(layer),
    ),
    options,
)((methods) => {
    methods.effect(
        'reads the current version of a member from the index uv installs from',
        Effect.fnUntraced(function* () {
            const { root, nx, called } = yield* Consumer;
            const found = yield* called('uv', ['pip', 'install', '--dry-run', '--no-deps', '--target', `${root}/.latest`, 'six']);
            const latest = yield* pipe(
                Array.findFirst(`${found.stdout}${found.stderr}`.split('\n'), (line) => line.startsWith(' + six==')),
                Option.map((line) => line.replace(' + six==', '').trim()),
                Effect.fromOption,
                Effect.orDie,
            );
            const shown = yield* nx('release', 'version', 'patch', '--dry-run');
            assert.include(`${shown.stdout}${shown.stderr}`, `Resolved the current version as ${latest} from the remote registry`, 'uv pip install --dry-run --no-deps --target <folder> <name>; nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.ts "registry"');
        }),
        bound,
    );
});
