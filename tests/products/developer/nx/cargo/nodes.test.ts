import { join } from 'node:path';
import type { CreateNodesContext } from '@nx/devkit';
import { resetWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-cargo';
import { refused } from '@rasm/testing/errors';
import { pathed, refusing } from '@rasm/testing/nx';
import { TempFs } from '@rasm/testing/temp-fs';
import { Array, Predicate } from 'effect';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));
vi.stubEnv('CARGO_INSTALL_ROOT', '/opt/cargo');

// --- [CONSTANTS] -----------------------------------------------------------------------

const [, createNodesFunction] = createNodes;

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('test');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        await tempFs.createFiles({
            'proj/Cargo.toml': '[package]\nname = "proj"\nversion = "0.1.0"\nedition = "2024"\nreadme = "README.md"\n',
            'proj/README.md': '# proj\n',
            'proj/src/lib.rs': 'pub fn one() -> u32 {\n    1\n}\n',
            'proj/src/bin/tool.rs': 'fn main() {\n    println!("{}", proj::one());\n}\n',
            'proj/tests/one.rs': '#[test]\nfn one() {\n    assert_eq!(proj::one(), 1);\n}\n',
            'proj/.config/nextest.toml': '[profile.default]\nfail-fast = false\n',
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({
        nxJsonConfiguration: {},
        workspaceRoot: tempFs.tempDir,
    }));

describe('@rasm/nx-cargo/plugin/nodes', { timeout: 60_000 }, () => {
    it('should create nodes based on cargo', async ({ context }) => {
        const results = await createNodesFunction(
            ['proj/Cargo.toml', 'proj/.config/nextest.toml'],
            {
                coveragePath: 'coverage/{projectName}',
            },
            context,
        );

        const project = results[0]?.[1].projects?.['proj'];
        const suites = ['--locked', '--cargo-message-format', 'json-diagnostic-rendered-ansi', '--message-format', 'libtest-json-plus', '--message-format-version', '0.1'];
        const env = { NEXTEST_EXPERIMENTAL_LIBTEST_JSON: '1' };
        expect(project?.namedInputs?.['default']?.filter(Predicate.isString)).toStrictEqual(['{projectRoot}/Cargo.toml', '{projectRoot}/src/**/*', '{projectRoot}/src/bin/**/*', '{projectRoot}/tests/**/*', '{projectRoot}/README.md', '{workspaceRoot}/proj/Cargo.lock', '{workspaceRoot}/proj/Cargo.toml']);
        expect(project?.targets?.['test--nextest']).toStrictEqual({
            cache: true,
            configurations: {
                coverage: {
                    commands: [{ args: ['llvm-cov', 'nextest', '--lcov', '--output-path', '../coverage/{projectName}/lcov.info', ...suites], env, packages: [] }],
                    outputs: ['{workspaceRoot}/coverage/{projectName}/lcov.info'],
                },
            },
            executor: '@rasm/nx-cargo:nextest',
            inputs: ['default', '^default', { externalDependencies: [] }, '{workspaceRoot}/proj/.config/nextest.toml', { env: 'NEXTEST_PROFILE' }, { runtime: 'cargo nextest -V' }, { runtime: 'cargo llvm-cov --version' }],
            metadata: { description: 'Tests proj with cargo nextest run', help: { command: 'cargo nextest run --help', example: { args: ['--verbose'] } }, technologies: ['rust'] },
            options: { commands: [{ args: ['nextest', 'run', ...suites], env }], cwd: 'proj', package: 'proj' },
            outputs: ['{options.outputs.0}'],
        });
    });

    it('should create nodes based on cargo for nested project root', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'nested/nested/proj/Cargo.toml': '[package]\nname = "nested-proj"\nversion = "0.1.0"\nedition = "2024"\n',
            'nested/nested/proj/src/main.rs': 'fn main() {}\n',
        });

        const results = await createNodesFunction(['nested/nested/proj/Cargo.toml'], {}, context);

        const project = results[0]?.[1].projects?.['nested/nested/proj'];
        expect(results.map(([file, { projects }]) => [file, Object.keys({ ...projects })])).toStrictEqual([['nested/nested/proj/Cargo.toml', ['nested/nested/proj']]]);
        expect(project?.name).toBe('nested_proj');
        expect(project?.release).toStrictEqual({ version: { versionActions: '@rasm/nx-cargo/release/version-actions', versionActionsOptions: { lock: 'nested/nested/proj/Cargo.lock' } } });
        expect(project?.namedInputs?.['default']?.filter(Predicate.isString)).toStrictEqual(['{projectRoot}/Cargo.toml', '{projectRoot}/src/**/*', '{workspaceRoot}/nested/nested/proj/Cargo.lock', '{workspaceRoot}/nested/nested/proj/Cargo.toml']);
        expect(project?.targets?.['build--cargo']?.options).toStrictEqual({
            commands: [{ args: ['build', '--keep-going', '--message-format', 'json-diagnostic-rendered-ansi'] }],
            cwd: 'nested/nested/proj',
            outputs: ['{workspaceRoot}/nested/nested/proj/target/debug/nested-proj', '{workspaceRoot}/nested/nested/proj/target/debug/nested-proj.d'],
            package: 'nested-proj',
        });
    });

    it('should tag a crate stating a version for release, and give the publish target to a crate its publish setting allows', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
            'crates/published/Cargo.toml': '[package]\nname = "published"\nversion = "0.1.0"\nedition = "2024"\n',
            'crates/published/src/lib.rs': '',
            'crates/internal/Cargo.toml': '[package]\nname = "internal"\nversion = "0.1.0"\nedition = "2024"\npublish = false\n',
            'crates/internal/src/lib.rs': '',
            'crates/draft/Cargo.toml': '[package]\nname = "draft"\nedition = "2024"\n',
            'crates/draft/src/lib.rs': '',
        });

        const results = await createNodesFunction(['Cargo.toml', 'crates/published/Cargo.toml', 'crates/internal/Cargo.toml', 'crates/draft/Cargo.toml'], {}, context);

        const projects = results[0]?.[1].projects;
        const released = (root: string) => ({ tags: projects?.[root]?.tags, targets: Array.intersection(Object.keys({ ...projects?.[root]?.targets }), ['pack--cargo', 'attach--gh', 'nx-release-publish']) });
        expect(released('crates/published')).toStrictEqual({ tags: ['release'], targets: ['pack--cargo', 'attach--gh', 'nx-release-publish'] });
        expect(released('crates/internal')).toStrictEqual({ tags: ['release'], targets: ['pack--cargo', 'attach--gh'] });
        expect(released('crates/draft')).toStrictEqual({ tags: undefined, targets: [] });
    });

    it('should add the install target a new binary source gives a library crate on the next read', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'lib/Cargo.toml': '[package]\nname = "lib"\nversion = "0.1.0"\nedition = "2024"\n',
            'lib/src/lib.rs': 'pub fn two() -> u32 {\n    2\n}\n',
        });
        const before = await createNodesFunction(['lib/Cargo.toml'], {}, context);
        await tempFs.createFile('lib/src/bin/report.rs', 'fn main() {\n    println!("{}", lib::two());\n}\n');
        resetWorkspaceContext();

        const after = await createNodesFunction(['lib/Cargo.toml'], {}, context);

        expect(before[0]?.[1].projects?.['lib']?.targets).not.toHaveProperty('install--cargo');
        expect(after[0]?.[1].projects?.['lib']?.targets?.['install--cargo']?.options).toStrictEqual({
            commands: [{ args: ['build', '--bins', '--profile', 'release', '--locked', '--keep-going', '--message-format', 'json-diagnostic-rendered-ansi', '-Zunstable-options', '--artifact-dir', '/opt/cargo/bin'] }],
            cwd: 'lib',
            package: 'lib',
        });
    });

    it.for([
        { name: 'a package.json holding nx', files: { 'package.json': JSON.stringify({ name: 'repo', nx: {} }) }, roots: ['.', 'crates/core'] },
        { name: 'a package.json beside a project.json', files: { 'package.json': JSON.stringify({ name: 'repo' }), 'project.json': '{}' }, roots: ['.', 'crates/core'] },
        { name: 'a package.json Nx core reads no project from', files: { 'package.json': JSON.stringify({ name: 'repo' }) }, roots: ['crates/core'] },
    ])('should create the virtual workspace root project only for $name', async ({ files, roots }, { tempFs, context }) => {
        await tempFs.createFiles({
            ...files,
            'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
            'crates/core/Cargo.toml': '[package]\nname = "core"\nedition = "2024"\n',
            'crates/core/src/lib.rs': '',
        });

        const results = await createNodesFunction(['Cargo.toml', 'crates/core/Cargo.toml'], {}, context);

        expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(roots);
    });

    it('should give the virtual workspace root project the workspace targets', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'package.json': JSON.stringify({ name: 'repo', nx: {} }),
            'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
            'crates/core/Cargo.toml': '[package]\nname = "core"\nedition = "2024"\n',
            'crates/core/src/lib.rs': '',
        });

        const results = await createNodesFunction(['Cargo.toml', 'crates/core/Cargo.toml'], {}, context);

        const technologies = ['rust'];
        const sources = ['default', '^default', { externalDependencies: [] }];
        const manifests = ['{workspaceRoot}/crates/core/Cargo.toml', '{workspaceRoot}/Cargo.toml', '{workspaceRoot}/Cargo.lock', { externalDependencies: [] }];
        expect(results[0]?.[1].projects?.['.']).toStrictEqual({
            root: '.',
            metadata: { technologies },
            targets: {
                clean: { dependsOn: ['clean--*'], inputs: ['...', ...sources] },
                'clean--cargo': {
                    executor: '@rasm/nx-distribution:clean',
                    inputs: sources,
                    metadata: { description: 'Removes the build folder of the workspace with cargo clean', help: { command: 'cargo clean --help', example: { args: ['--dry-run'] } }, technologies },
                    options: { commands: [['cargo', 'clean']], cwd: '.', targets: [] },
                },
                upgrade: { dependsOn: ['upgrade--*'], inputs: ['...', ...manifests] },
                'upgrade--cargo': {
                    executor: 'nx:run-commands',
                    inputs: manifests,
                    metadata: { description: 'Moves every dependency requirement to its newest release with cargo upgrade, then the lock with cargo update', help: { command: 'cargo upgrade --help', example: { args: ['--dry-run'] } }, technologies },
                    options: { commands: ['cargo upgrade --incompatible allow', 'cargo update'], cwd: '.', parallel: false },
                },
            },
        });
    });

    it.for(refusing('cargo'))('should report the command and $case of cargo as the error of the manifest', async ({ scripts, refusal }, { tempFs, context }) => {
        pathed(join(tempFs.tempDir, '.bin'), scripts);

        await expect(createNodesFunction(['proj/Cargo.toml'], {}, context)).rejects.toSatisfy(refused(refusal));
    });
});
