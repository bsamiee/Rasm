import { layer } from '@effect/platform-node/NodeServices';
import plugin from '@rasm/nx-uv/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { Array, Effect, FileSystem, Layer, Option, Path, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const UV = {
    metadata: 'uv workspace metadata',
    build: 'uv build --package <name> --out-dir <folder>',
    sdist: 'tar -tzf of the sdist uv build writes',
    version: 'uv version --package <name> --short',
    checked: 'uv lock --check',
    listed: 'uv tool list',
    latest: 'uv pip install --dry-run --no-deps --target <folder> <name>',
    dryRun: 'uv publish --dry-run',
} as const;
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

const members = Effect.gen(function* () {
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

const built = Effect.gen(function* () {
    const stated = yield* members;
    const packed = yield* Effect.forEach(stated, (entry) => Effect.map(distributions(entry.name, entry.root), ({ names, sources }) => ({ ...entry, names, sources })));
    return packed.filter(({ names }) => Array.isReadonlyArrayNonEmpty(names));
});

const member = Effect.fnUntraced(function* (root: string) {
    const stated = yield* built;
    return yield* Effect.orDie(Effect.fromOption(Array.findFirst(stated, (candidate) => candidate.root === root)));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const locked = Layer.effectDiscard(
    Effect.gen(function* () {
        const { called, committed } = yield* Consumer;
        yield* called('uv', ['lock']);
        yield* committed('lock');
    }),
);

const workspace = (stated: Readonly<Record<string, string>> = {}) =>
    locked.pipe(
        Layer.provideMerge(
            Consumer.layer({
                resolved: import.meta.url,
                settings: { plugins: [plugin.name], release: { projects: ['tag:release'] } },
                ignored: ['.venv', 'dist', '__pycache__', '.pytest_cache', '*.egg-info', '.tools', '.latest'],
                files: { ...files, ...stated },
            }),
        ),
        Layer.provideMerge(Packed.layer(plugin.name)),
        Layer.provideMerge(layer),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { built, distributions, files, member, members, UV, workspace };
