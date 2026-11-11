import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import type { ProjectGraph, Tree } from '@nx/devkit';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import cargo from '@rasm/nx-cargo/release/version-actions';
import dotnet from '@rasm/nx-dotnet/release/version-actions';
import uv from '@rasm/nx-uv/release/version-actions';
import { spawned } from '@rasm/testing/nx/tools';
import { Effect, FileSystem } from 'effect';
import type { VersionActions } from 'nx/release';
import type { AfterAllProjectsVersioned } from 'nx/src/command-line/release/version/version-actions';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = 'packages/sheet';
const CLOSER = /[\]>]/u;
const LIBRARY = 'packages/lib';
const SOURCE = {
    logged: 'cammisuli/monodon@50a2b5d packages/rust/src/release/version-actions.spec.ts updateProjectVersion log names the version and the manifest path',
    versionless: 'cammisuli/monodon@50a2b5d packages/rust/src/release/version-actions.spec.ts "throws when the version field is absent"; Cargo reference, manifest "The version field": optional, defaults to 0.0.0',
    declared: 'cammisuli/monodon@50a2b5d packages/rust/src/release/version-actions.spec.ts "finds a string-form entry in [dependencies]", "finds an object-form entry in [dev-dependencies]"',
    undeclared: 'cammisuli/monodon@50a2b5d packages/rust/src/release/version-actions.spec.ts "returns nulls when the dependency is not declared"',
    linked: 'naxodev/oss@8b43d4a packages/gonx/src/release/go-version-actions.spec.ts',
    malformed: 'nrwl/nx@2a424d3 packages/js/src/release/version-actions.spec.ts malformed and duplicate-key manifests (3)',
    versioned: 'cammisuli/monodon@50a2b5d packages/rust/src/release/version-actions.spec.ts "no-ops during a dry run", "no-ops when skipLockFileUpdate is set"',
    rewritten: 'cammisuli/monodon@50a2b5d packages/rust/src/release/version-actions.spec.ts updateProjectDependencies (4)',
    unchanged: 'cammisuli/monodon@50a2b5d packages/rust/src/release/version-actions.spec.ts "returns no log messages when nothing needs updating", lucasvieirasilva/nx-plugins@1c05d36 packages/nx-python/src/release/version-actions.spec.ts "returns an empty array when there are no dependencies to update"',
    required: 'lucasvieirasilva/nx-plugins@1c05d36 packages/nx-python/src/provider/uv/provider.spec.ts "rewrites version ranges across main, optional and group dependencies", "leaves dependencies without a version specifier untouched"',
    excluded: 'lucasvieirasilva/nx-plugins@1c05d36 packages/nx-python/src/provider/uv/provider.spec.ts "throws when the resulting specifier excludes the released version"',
} as const;
const csproj = (items: string) => `<Project Sdk="Microsoft.NET.Sdk">\n    <!-- sheet layout -->\n    <PropertyGroup>\n        <TargetFramework>net11.0</TargetFramework>\n${items}    </PropertyGroup>\n</Project>\n`;
const MANIFESTS = [
    {
        actions: cargo,
        options: { lock: 'Cargo.lock' },
        file: 'Cargo.toml',
        text: '# sheet layout\n[package]\nname    = "sheet"   # crate\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\nserde = { version = "1.0.0" }\n',
        versionless: '[package]\nname = "sheet"\nedition = "2024"\n',
        unversioned: /^reads 0\.0\.0$/u,
        library: '[package]\nname = "sheet-lib"\nversion = "1.2.3"\nedition = "2024"\n',
        linked: '[package]\nname = "sheet"\nversion = "0.1.0"\n\n[dependencies]\nsheet-lib = { path = "../lib" }\n',
    },
    {
        actions: uv,
        options: { lock: 'uv.lock' },
        file: 'pyproject.toml',
        text: '# sheet layout\n[project]\nname    = "sheet"   # distribution\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[tool.ruff]\ntarget-version = "py313"\n',
        versionless: '[project]\nname = "sheet"\nrequires-python = ">=3.13"\n',
        unversioned: /\bversion\b/iu,
        library: '[project]\nname = "sheet-lib"\nversion = "1.2.3"\nrequires-python = ">=3.13"\n',
        linked: '[project]\nname = "sheet"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["sheet-lib"]\n\n[tool.uv.sources]\nsheet-lib = { workspace = true }\n',
    },
    {
        actions: dotnet,
        options: { projectFile: 'Sheet.csproj', releases: [] },
        file: 'Sheet.csproj',
        text: csproj('        <Version>0.1.0</Version>\n'),
        versionless: csproj(''),
        unversioned: /\bversion\b/iu,
        library: csproj('        <PackageId>sheet-lib</PackageId>\n        <Version>1.2.3</Version>\n'),
        linked: csproj('        <Version>0.1.0</Version>\n').replace('</Project>', '    <ItemGroup>\n        <ProjectReference Include="../lib/Sheet.csproj" />\n    </ItemGroup>\n</Project>'),
    },
] as const;
const DECLARED = [
    { dependencies: '[dependencies]\nsheet-lib = "^1.2.3"\n', read: { currentVersion: '^1.2.3', dependencyCollection: 'dependencies' }, written: '[dependencies]\nsheet-lib = "^2.0.0"\n' },
    { dependencies: '[dependencies]\nsheet-lib = "~1.2.3"\n', read: { currentVersion: '~1.2.3', dependencyCollection: 'dependencies' }, written: '[dependencies]\nsheet-lib = "~2.0.0"\n' },
    { dependencies: '[dependencies]\nsheet-lib = "=1.2.3"\n', read: { currentVersion: '=1.2.3', dependencyCollection: 'dependencies' }, written: '[dependencies]\nsheet-lib = "=2.0.0"\n' },
    { dependencies: '[dependencies]\nsheet-lib = "1.2.3"\n', read: { currentVersion: '1.2.3', dependencyCollection: 'dependencies' }, written: '[dependencies]\nsheet-lib = "2.0.0"\n' },
    { dependencies: '[dev-dependencies]\nsheet-lib = "^1.2.3"\n', read: { currentVersion: '^1.2.3', dependencyCollection: 'dev-dependencies' }, written: '[dev-dependencies]\nsheet-lib = "^2.0.0"\n' },
    {
        dependencies: '[dependencies]\nsheet-lib = { version = "1.2.3", path = "../lib" }\n',
        read: { currentVersion: '1.2.3', dependencyCollection: 'dependencies' },
        written: '[dependencies]\nsheet-lib = { version = "2.0.0", path = "../lib" }\n',
    },
    {
        dependencies: '[dev-dependencies]\nsheet-lib = { version = "~1.2.3", features = ["a", "b"] }\n',
        read: { currentVersion: '~1.2.3', dependencyCollection: 'dev-dependencies' },
        written: '[dev-dependencies]\nsheet-lib = { version = "~2.0.0", features = ["a", "b"] }\n',
    },
] as const;
const VERSIONED = [
    { file: 'Cargo.toml', loaded: () => import('@rasm/nx-cargo/release/version-actions'), options: { lock: 'Cargo.lock' } },
    { file: 'pyproject.toml', loaded: () => import('@rasm/nx-uv/release/version-actions'), options: { lock: 'uv.lock' } },
] as const;
const REQUIRED = [
    { stated: 'project dependencies', tables: 'dependencies = ["sheet-lib~=1.1", "requests>=2.0"]\n', released: '1.4.0', written: 'dependencies = ["sheet-lib~=1.4", "requests>=2.0"]\n' },
    { stated: 'project dependencies with an upper bound', tables: 'dependencies = ["sheet-lib>=1.2,<2.0"]\n', released: '1.5.2', written: 'dependencies = ["sheet-lib>=1.5,<2.0"]\n' },
    { stated: 'an optional-dependencies extra', tables: '\n[project.optional-dependencies]\nextra = ["sheet-lib==1.0.0"]\n', released: '2.0.0', written: '\n[project.optional-dependencies]\nextra = ["sheet-lib==2.0.0"]\n' },
    { stated: 'a dependency group', tables: '\n[dependency-groups]\ndev = ["sheet-lib~=1.0"]\n', released: '1.9.0', written: '\n[dependency-groups]\ndev = ["sheet-lib~=1.9"]\n' },
    { stated: 'project dependencies without a specifier', tables: 'dependencies = ["sheet-lib"]\n', released: '1.4.0', written: 'dependencies = ["sheet-lib"]\n' },
] as const;
const distribution = (tables: string) => `[project]\nname = "sheet"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n${tables}\n[tool.uv.sources]\nsheet-lib = { workspace = true }\n`;
const crate = (dependencies: string) => `[package]\nname = "sheet"\nversion = "0.1.0"\n\n${dependencies}`;
const GRAPH: ProjectGraph = { nodes: { sheet: { name: 'sheet', type: 'lib', data: { root: ROOT } }, 'sheet-lib': { name: 'sheet-lib', type: 'lib', data: { root: LIBRARY } } }, dependencies: { sheet: [{ source: 'sheet', target: 'sheet-lib', type: 'static' }], 'sheet-lib': [] } };

// --- [OPERATIONS] ----------------------------------------------------------------------

const opened = Effect.fnUntraced(function* ({ actions: Actions, options, file, library }: (typeof MANIFESTS)[number], text?: string, tree: Tree = createTreeWithEmptyWorkspace({ formatter: 'none' })) {
    yield* Effect.sync(() => (text === undefined ? undefined : [tree.write(`${ROOT}/${file}`, text), tree.write(`${LIBRARY}/${file}`, library)]));
    const actions: VersionActions = Reflect.construct(Actions, [{ name: '__default__' }, { name: 'sheet', type: 'lib', data: { root: ROOT } }, { versionActionsOptions: options, manifestRootsToUpdate: [{ path: ROOT, preserveLocalDependencyProtocols: false }] }]);
    yield* Effect.tryPromise(() => actions.init(tree));
    return { tree, actions };
});

const required = Effect.fnUntraced(function* (manifest: (typeof MANIFESTS)[number], text: string) {
    const { tree, actions } = yield* opened(manifest, text);
    return yield* Effect.tryPromise(() => actions.readCurrentVersionOfDependency(tree, GRAPH, 'sheet-lib'));
});

const released = Effect.fnUntraced(function* (tables: string, version: string) {
    const [, manifest] = MANIFESTS;
    const { tree, actions } = yield* opened(manifest, distribution(tables));
    yield* Effect.sync(() => {
        tree.write(`${LIBRARY}/${manifest.file}`, manifest.library.replace('1.2.3', version));
        tree.write(
            'uv.lock',
            `version = 1\nrevision = 5\nrequires-python = ">=3.13"\n\n[[package]]\nname = "sheet"\nversion = "0.1.0"\nsource = { editable = "${ROOT}" }\ndependencies = [\n    { name = "sheet-lib" },\n]\n\n[[package]]\nname = "sheet-lib"\nversion = "${version}"\nsource = { editable = "${LIBRARY}" }\n`,
        );
    });
    yield* Effect.tryPromise(() => actions.updateProjectDependencies(tree, GRAPH, { 'sheet-lib': version }));
    return tree.read(`${ROOT}/${manifest.file}`, 'utf8');
});

const versioned = Effect.fnUntraced(function* (manifest: (typeof MANIFESTS)[number], text: string, tree: Tree) {
    const { actions } = yield* opened(manifest, text, tree);
    return yield* Effect.tryPromise(() => actions.updateProjectVersion(tree, '0.2.0'));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('version actions', (methods) => {
    methods.effect.each(MANIFESTS)(
        '$file: reads the version its manifest states from the tree and starts no process',
        Effect.fnUntraced(function* (manifest) {
            const { tree, actions } = yield* opened(manifest, manifest.text);
            assert.deepStrictEqual(yield* spawned(Effect.tryPromise(() => actions.readCurrentVersionFromSourceManifest(tree))), [{ currentVersion: '0.1.0', manifestPath: `${ROOT}/${manifest.file}` }, []]);
        }),
    );

    methods.effect.each(MANIFESTS)(
        '$file: writes the new version into the tree, starts no process, and leaves every other byte of the manifest',
        Effect.fnUntraced(function* (manifest) {
            const { tree, actions } = yield* opened(manifest, manifest.text);
            const [logs, started] = yield* spawned(Effect.tryPromise(() => actions.updateProjectVersion(tree, '0.2.0')));
            assert.deepStrictEqual([tree.read(`${ROOT}/${manifest.file}`, 'utf8'), logs.length > 0, started], [manifest.text.replace('0.1.0', '0.2.0'), true, []]);
        }),
    );

    methods.effect(
        'Cargo.toml: logs the new version and the manifest path once',
        Effect.fnUntraced(function* () {
            const [manifest] = MANIFESTS;
            const { tree, actions } = yield* opened(manifest, manifest.text);
            const logs = yield* Effect.tryPromise(() => actions.updateProjectVersion(tree, '0.2.0'));
            assert.lengthOf(logs, 1, SOURCE.logged);
            assert.include(logs.join('\n'), '0.2.0', SOURCE.logged);
            assert.include(logs.join('\n'), `${ROOT}/${manifest.file}`, SOURCE.logged);
        }),
    );

    methods.effect.each(MANIFESTS)(
        '$file: refuses a project whose manifest is absent by naming the manifest',
        Effect.fnUntraced(function* (manifest) {
            const { tree, actions } = yield* opened(manifest);
            const refusal = yield* Effect.flip(Effect.tryPromise(() => actions.readCurrentVersionFromSourceManifest(tree)));
            assert.include(String(refusal.cause), manifest.file);
        }),
    );

    methods.effect.each(MANIFESTS)(
        '$file: reads the version its tool gives a manifest that states none, or refuses the manifest by naming the version',
        Effect.fnUntraced(function* (manifest) {
            const { tree, actions } = yield* opened(manifest, manifest.versionless);
            const read = yield* Effect.match(
                Effect.tryPromise(() => actions.readCurrentVersionFromSourceManifest(tree)),
                { onSuccess: (stated) => `reads ${stated?.currentVersion}`, onFailure: ({ cause }) => String(cause) },
            );
            assert.match(read, manifest.unversioned, SOURCE.versionless);
        }),
    );

    methods.effect.each(MANIFESTS)(
        '$file: reads a null version and collection for a dependency the manifest does not declare, and for one a path or workspace source resolves',
        Effect.fnUntraced(function* (manifest) {
            const absent = { currentVersion: null, dependencyCollection: null };
            assert.deepStrictEqual(yield* required(manifest, manifest.text), absent, SOURCE.undeclared);
            assert.deepStrictEqual(yield* required(manifest, manifest.linked), absent, SOURCE.linked);
        }),
    );

    methods.effect.each(MANIFESTS)(
        '$file: changes no byte and returns no log where no dependency needs a new version',
        Effect.fnUntraced(function* (manifest) {
            const { tree, actions } = yield* opened(manifest, manifest.text);
            const logs = yield* Effect.tryPromise(() => actions.updateProjectDependencies(tree, GRAPH, {}));
            assert.deepStrictEqual([logs, tree.read(`${ROOT}/${manifest.file}`, 'utf8')], [[], manifest.text], SOURCE.unchanged);
        }),
    );

    methods.effect.each(MANIFESTS)(
        '$file: refuses a manifest it cannot parse and changes no byte of it',
        Effect.fnUntraced(function* (manifest) {
            const malformed = manifest.text.replace(CLOSER, '');
            const tree = createTreeWithEmptyWorkspace({ formatter: 'none' });
            const written = yield* Effect.exit(versioned(manifest, malformed, tree));
            assert.deepStrictEqual([written._tag, tree.read(`${ROOT}/${manifest.file}`, 'utf8')], ['Failure', malformed], SOURCE.malformed);
        }),
    );

    methods.effect.each(VERSIONED)(
        '$file: changes no file and starts no process after all projects are versioned on a dry run',
        Effect.fnUntraced(function* ({ loaded, options }) {
            const { afterAllProjectsVersioned } = yield* Effect.promise((): Promise<{ readonly afterAllProjectsVersioned: AfterAllProjectsVersioned }> => loaded());
            const root = yield* Effect.flatMap(FileSystem.FileSystem, (fs) => fs.makeTempDirectoryScoped());
            assert.deepStrictEqual(yield* spawned(Effect.tryPromise(() => afterAllProjectsVersioned(root, { dryRun: true, rootVersionActionsOptions: options }))), [{ changedFiles: [], deletedFiles: [] }, []], SOURCE.versioned);
        }),
    );

    methods.effect.each(DECLARED)(
        'Cargo.toml: reads the requirement $read.currentVersion from $read.dependencyCollection',
        Effect.fnUntraced(function* ({ dependencies, read }) {
            const [manifest] = MANIFESTS;
            const { tree, actions } = yield* opened(manifest, crate(dependencies));
            assert.deepStrictEqual(yield* Effect.tryPromise(() => actions.readCurrentVersionOfDependency(tree, GRAPH, 'sheet-lib')), read, SOURCE.declared);
        }),
    );

    methods.effect.each(DECLARED)(
        'Cargo.toml: rewrites the requirement $read.currentVersion in $read.dependencyCollection, keeping its operator and every other field',
        Effect.fnUntraced(function* ({ dependencies, written }) {
            const [manifest] = MANIFESTS;
            const { tree, actions } = yield* opened(manifest, crate(dependencies));
            const logs = yield* Effect.tryPromise(() => actions.updateProjectDependencies(tree, GRAPH, { 'sheet-lib': '2.0.0' }));
            assert.deepStrictEqual([tree.read(`${ROOT}/${manifest.file}`, 'utf8'), logs.length > 0], [crate(written), true], SOURCE.rewritten);
        }),
    );

    methods.effect.each(REQUIRED)('pyproject.toml: raises the lower bound of a workspace requirement in $stated to the released version, keeping its operator, precision, and every other requirement', ({ tables, released: version, written }) =>
        Effect.map(released(tables, version), (text) => assert.strictEqual(text, distribution(written), SOURCE.required)),
    );

    methods.effect(
        'pyproject.toml: refuses a workspace requirement whose upper bound excludes the released version by naming that version',
        Effect.fnUntraced(function* () {
            const refusal = yield* Effect.flip(released('dependencies = ["sheet-lib>=1.0,<1.3"]\n', '1.4.0'));
            assert.include(String(refusal.cause), '1.4.0', SOURCE.excluded);
        }),
    );
});
