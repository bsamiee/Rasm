import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import cargo from '@rasm/nx-cargo/release/version-actions';
import dotnet from '@rasm/nx-dotnet/release/version-actions';
import uv from '@rasm/nx-uv/release/version-actions';
import { spawned } from '@rasm/test-support/nx/tools';
import { Effect } from 'effect';
import type { VersionActions } from 'nx/release';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = 'packages/sheet';
const MANIFESTS = [
    { actions: cargo, options: { lock: 'Cargo.lock' }, file: 'Cargo.toml', text: '# sheet layout\n[package]\nname    = "sheet"   # crate\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\nserde = { version = "1.0.0" }\n' },
    { actions: uv, options: { lock: 'uv.lock' }, file: 'pyproject.toml', text: '# sheet layout\n[project]\nname    = "sheet"   # distribution\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[tool.ruff]\ntarget-version = "py313"\n' },
    {
        actions: dotnet,
        options: { projectFile: 'Sheet.csproj', releases: [] },
        file: 'Sheet.csproj',
        text: '<Project Sdk="Microsoft.NET.Sdk">\n    <!-- sheet layout -->\n    <PropertyGroup>\n        <TargetFramework>net11.0</TargetFramework>\n        <Version>0.1.0</Version>\n    </PropertyGroup>\n</Project>\n',
    },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const opened = Effect.fnUntraced(function* ({ actions: Actions, options, file, text }: (typeof MANIFESTS)[number], stated: boolean) {
    const tree = createTreeWithEmptyWorkspace({ formatter: 'none' });
    yield* Effect.sync(() => (stated ? tree.write(`${ROOT}/${file}`, text) : undefined));
    const actions: VersionActions = Reflect.construct(Actions, [{ name: '__default__' }, { name: 'sheet', type: 'lib', data: { root: ROOT } }, { versionActionsOptions: options, manifestRootsToUpdate: [{ path: ROOT, preserveLocalDependencyProtocols: false }] }]);
    yield* Effect.tryPromise(() => actions.init(tree));
    return { tree, actions };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('version actions', (methods) => {
    methods.effect.each(MANIFESTS)(
        '$file: reads the version its manifest states from the tree and starts no process',
        Effect.fnUntraced(function* (manifest) {
            const { tree, actions } = yield* opened(manifest, true);
            assert.deepStrictEqual(yield* spawned(Effect.tryPromise(() => actions.readCurrentVersionFromSourceManifest(tree))), [{ currentVersion: '0.1.0', manifestPath: `${ROOT}/${manifest.file}` }, []]);
        }),
    );

    methods.effect.each(MANIFESTS)(
        '$file: writes the new version into the tree, starts no process, and leaves every other byte of the manifest',
        Effect.fnUntraced(function* (manifest) {
            const { tree, actions } = yield* opened(manifest, true);
            const [logs, started] = yield* spawned(Effect.tryPromise(() => actions.updateProjectVersion(tree, '0.2.0')));
            assert.deepStrictEqual([tree.read(`${ROOT}/${manifest.file}`, 'utf8'), logs.length > 0, started], [manifest.text.replace('0.1.0', '0.2.0'), true, []]);
        }),
    );

    methods.effect.each(MANIFESTS)(
        '$file: refuses a project whose manifest is absent by naming the manifest',
        Effect.fnUntraced(function* (manifest) {
            const { tree, actions } = yield* opened(manifest, false);
            const refusal = yield* Effect.flip(Effect.tryPromise(() => actions.readCurrentVersionFromSourceManifest(tree)));
            assert.include(String(refusal.cause), manifest.file);
        }),
    );
});
