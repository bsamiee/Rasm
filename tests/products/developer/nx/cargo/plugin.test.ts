import { execFileSync } from 'node:child_process';
import { readdirSync } from 'node:fs';
import { join, relative } from 'node:path';
import type { CreateNodesContext, ExecutorContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-cargo';
import { Cargo, execute } from '@rasm/nx-cargo/executors/cargo';
import { TempFs } from '@rasm/testing/temp-fs';
import { Array, Effect, Schema, String } from 'effect';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const [, createNodesFunction] = createNodes;

// --- [OPERATIONS] ----------------------------------------------------------------------

const help = (command: string) => ({ command: `${command} --help`, example: { args: ['--verbose'] } });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('cargo-plugin');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({
        nxJsonConfiguration: {
            namedInputs: {
                default: ['{projectRoot}/**/*'],
                production: ['!{projectRoot}/**/*.spec.ts'],
            },
        },
        workspaceRoot: tempFs.tempDir,
    }));

describe('@rasm/nx-cargo', { timeout: 120_000 }, () => {
    it('should create nodes', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'package.json': JSON.stringify({ name: 'repo' }),
            'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["proj/*"]\n',
            'proj/library/Cargo.toml': '[package]\nname = "cargo-tutorial"\nversion = "0.1.0"\nedition = "2024"\n',
            'proj/library/src/lib.rs': 'pub fn one() -> u32 {\n    1\n}\n',
            'proj/application/Cargo.toml': '[package]\nname = "application"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncargo-tutorial = { path = "../library" }\n',
            'proj/application/src/main.rs': 'fn main() {\n    println!("{}", cargo_tutorial::one());\n}\n',
        });

        const nodes = await createNodesFunction(['Cargo.toml', 'proj/library/Cargo.toml', 'proj/application/Cargo.toml'], undefined, context);

        const host = String.kebabToSnake(execFileSync('rustc', ['--print', 'host-tuple'], { encoding: 'utf-8' }).trim()).toUpperCase();
        const technologies = ['rust'];
        const sources = ['default', '^default', { externalDependencies: [] }];
        const messages = ['--keep-going', '--message-format', 'json-diagnostic-rendered-ansi'];
        const profiles = Array.cartesianWith(['DEV', 'RELEASE', 'TEST', 'BENCH'], ['INHERITS', 'OPT_LEVEL', 'DEBUG', 'SPLIT_DEBUGINFO', 'STRIP', 'DEBUG_ASSERTIONS', 'OVERFLOW_CHECKS', 'LTO', 'PANIC', 'CODEGEN_UNITS', 'RPATH', 'TRIM_PATHS', 'CODEGEN_BACKEND'], (profile, key) => `CARGO_PROFILE_${profile}_${key}`);
        const compiler = [
            'RUSTC',
            'RUSTC_WRAPPER',
            'RUSTC_WORKSPACE_WRAPPER',
            'RUSTDOC',
            'RUSTFLAGS',
            'CARGO_ENCODED_RUSTFLAGS',
            'RUSTDOCFLAGS',
            'CARGO_ENCODED_RUSTDOCFLAGS',
            'RUSTUP_TOOLCHAIN',
            'CARGO_BUILD_TARGET',
            'CARGO_BUILD_RUSTC',
            'CARGO_BUILD_RUSTC_WRAPPER',
            'CARGO_BUILD_RUSTC_WORKSPACE_WRAPPER',
            'CARGO_BUILD_RUSTDOC',
            'CARGO_BUILD_RUSTFLAGS',
            'CARGO_BUILD_RUSTDOCFLAGS',
            `CARGO_TARGET_${host}_LINKER`,
            `CARGO_TARGET_${host}_RUSTFLAGS`,
            ...profiles,
        ].map((env) => ({ env }));
        const namedInputs = { default: ['{projectRoot}/**/*', '{workspaceRoot}/Cargo.lock', '{workspaceRoot}/Cargo.toml', { runtime: 'rustc --print host-tuple' }, ...compiler] };
        const release = { version: { versionActions: '@rasm/nx-cargo/release/version-actions', versionActionsOptions: { lock: 'Cargo.lock' } } };
        const library = { root: 'proj/library', name: 'cargo-tutorial' };
        const cleaned = { command: 'cargo clean --help', example: { args: ['--dry-run'] } };
        const crate = (name: string, packages: readonly (typeof library)[]) => ({
            attach: { dependsOn: ['attach--*'], inputs: ['...', { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: [] }] },
            'attach--gh': {
                dependsOn: ['pack--cargo'],
                executor: '@rasm/nx-distribution:attach',
                inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                metadata: {
                    description: "Uploads the file pack--cargo wrote to the GitHub release of the version the project's manifest states with gh release upload",
                    help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
                    technologies,
                },
            },
            build: { dependsOn: ['build--*'], inputs: ['...', ...sources] },
            check: { dependsOn: ['check--*'], inputs: ['...', ...sources, { env: 'CLIPPY_CONF_DIR' }] },
            'check--clippy': {
                cache: true,
                executor: '@rasm/nx-cargo:cargo',
                inputs: [...sources, { env: 'CLIPPY_CONF_DIR' }],
                metadata: { description: `Reports the clippy findings of ${name} with cargo clippy`, help: help('cargo clippy'), technologies },
            },
            clean: { dependsOn: ['clean--*'], inputs: ['...', ...sources] },
            fix: { dependsOn: ['fix--*'], inputs: ['...', ...sources] },
            'fix--clippy': {
                executor: '@rasm/nx-cargo:cargo',
                inputs: sources,
                metadata: { description: `Applies the clippy suggestions for ${name} with cargo clippy --fix`, help: help('cargo clippy --fix'), technologies },
            },
            pack: { dependsOn: ['pack--*'], inputs: ['...', ...sources] },
            'pack--cargo': {
                cache: true,
                executor: '@rasm/nx-cargo:cargo',
                inputs: sources,
                metadata: { description: `Packs ${name} into a .crate archive with cargo package beside the workspace crates it depends on, without the verification build`, help: help('cargo package'), technologies },
                options: { commands: [{ args: ['package', '--no-verify', '--locked'], packages }], cwd: '.', package: name },
                outputs: [`{workspaceRoot}/target/package/${name}-0.1.0.crate`],
            },
            test: { dependsOn: ['test--*'], inputs: ['...', ...sources, { runtime: 'cargo llvm-cov --version' }] },
            'test--cargo': {
                cache: true,
                configurations: { coverage: { commands: [{ args: ['llvm-cov', 'test', '--lcov', '--locked'], packages }], outputs: [] } },
                executor: '@rasm/nx-cargo:cargo',
                inputs: [...sources, { runtime: 'cargo llvm-cov --version' }],
                metadata: { description: `Tests ${name} with cargo test`, help: help('cargo test'), technologies },
                options: { commands: [{ args: ['test', '--locked'] }], cwd: '.', package: name },
                outputs: [],
            },
            'nx-release-publish': {
                dependsOn: ['pack--cargo', '^nx-release-publish'],
                executor: '@rasm/nx-cargo:release-publish',
                inputs: sources,
                metadata: {
                    description: `Publishes ${name} with cargo publish at the version nx release tagged at the checked-out commit, skipping a version cargo info finds in its registry`,
                    help: { command: 'cargo publish --help', example: { options: { dryRun: true } } },
                    technologies,
                },
                options: { cwd: '.', package: name, packages },
            },
        });
        const application = crate('application', [library]);
        const tutorial = crate('cargo-tutorial', []);

        expect(nodes).toStrictEqual([
            [
                'Cargo.toml',
                {
                    projects: {
                        'proj/application': {
                            root: 'proj/application',
                            name: 'application',
                            metadata: { technologies },
                            namedInputs,
                            release,
                            tags: ['release'],
                            targets: {
                                ...application,
                                'build--cargo': {
                                    cache: true,
                                    configurations: {
                                        release: { commands: [{ args: ['build', '--profile', 'release', ...messages] }], outputs: ['{workspaceRoot}/target/release/application', '{workspaceRoot}/target/release/application.d'] },
                                    },
                                    executor: '@rasm/nx-cargo:cargo',
                                    inputs: sources,
                                    metadata: { description: 'Builds application with cargo build, bringing Cargo.lock in line with the manifests', help: help('cargo build'), technologies },
                                    options: { commands: [{ args: ['build', ...messages] }], cwd: '.', outputs: ['{workspaceRoot}/target/debug/application', '{workspaceRoot}/target/debug/application.d'], package: 'application' },
                                    outputs: ['{options.outputs.0}', '{options.outputs.1}'],
                                },
                                'check--clippy': { ...application['check--clippy'], options: { commands: [{ args: ['clippy', '--bins', '--tests', '--benches', '--all-features', '--locked', ...messages] }], cwd: '.', package: 'application' } },
                                'clean--cargo': {
                                    executor: '@rasm/nx-distribution:clean',
                                    inputs: sources,
                                    metadata: { description: 'Removes the build artifacts of application in profiles dev, release with cargo clean --profile', help: cleaned, technologies },
                                    options: {
                                        commands: [
                                            ['cargo', 'clean', '--package', 'application', '--profile', 'dev', '--locked'],
                                            ['cargo', 'clean', '--package', 'application', '--profile', 'release', '--locked'],
                                        ],
                                        cwd: '.',
                                        targets: ['build--cargo', 'pack--cargo'],
                                    },
                                },
                                'fix--clippy': { ...application['fix--clippy'], options: { commands: [{ args: ['clippy', '--fix', '--allow-dirty', '--bins', '--tests', '--benches', '--all-features', ...messages] }], cwd: '.', package: 'application' } },
                                install: { dependsOn: ['install--*'], inputs: ['...', ...sources] },
                                'install--cargo': {
                                    executor: '@rasm/nx-cargo:cargo',
                                    inputs: sources,
                                    metadata: { description: "Installs the binaries of application into Cargo's install root with cargo install, in release unless a configuration names a profile", help: help('cargo install'), technologies },
                                    options: { commands: [{ args: ['install', '--path', 'proj/application', '--locked'] }], cwd: '.' },
                                },
                            },
                        },
                        'proj/library': {
                            root: 'proj/library',
                            name: 'cargo_tutorial',
                            metadata: { technologies },
                            namedInputs,
                            release,
                            tags: ['release'],
                            targets: {
                                ...tutorial,
                                'build--cargo': {
                                    cache: true,
                                    configurations: { release: { commands: [{ args: ['build', '--profile', 'release', ...messages] }], outputs: [] } },
                                    executor: '@rasm/nx-cargo:cargo',
                                    inputs: sources,
                                    metadata: { description: 'Builds cargo-tutorial with cargo build, bringing Cargo.lock in line with the manifests', help: help('cargo build'), technologies },
                                    options: { commands: [{ args: ['build', ...messages] }], cwd: '.', outputs: [], package: 'cargo-tutorial' },
                                    outputs: [],
                                },
                                'check--clippy': { ...tutorial['check--clippy'], options: { commands: [{ args: ['clippy', '--lib', '--bins', '--tests', '--benches', '--all-features', '--locked', ...messages] }], cwd: '.', package: 'cargo-tutorial' } },
                                'clean--cargo': {
                                    executor: '@rasm/nx-distribution:clean',
                                    inputs: sources,
                                    metadata: { description: 'Removes the build artifacts of cargo-tutorial in profiles dev, release with cargo clean --profile', help: cleaned, technologies },
                                    options: {
                                        commands: [
                                            ['cargo', 'clean', '--package', 'cargo-tutorial', '--profile', 'dev', '--locked'],
                                            ['cargo', 'clean', '--package', 'cargo-tutorial', '--profile', 'release', '--locked'],
                                        ],
                                        cwd: '.',
                                        targets: ['pack--cargo'],
                                    },
                                },
                                'fix--clippy': { ...tutorial['fix--clippy'], options: { commands: [{ args: ['clippy', '--fix', '--allow-dirty', '--lib', '--bins', '--tests', '--benches', '--all-features', ...messages] }], cwd: '.', package: 'cargo-tutorial' } },
                            },
                        },
                    },
                },
            ],
        ]);
    });

    it('should write every output build--cargo declares, and nothing outside the target folder', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'Cargo.toml': '[package]\nname = "tiny"\nversion = "0.1.0"\nedition = "2024"\n',
            'src/main.rs': 'fn main() {}\n',
        });
        execFileSync('cargo', ['generate-lockfile'], { cwd: tempFs.tempDir });
        const nodes = await createNodesFunction(['Cargo.toml'], undefined, context);
        const options = await Effect.runPromise(Schema.decodeUnknownEffect(Cargo)(nodes[0]?.[1].projects?.['.']?.targets?.['build--cargo']?.options));
        const before = new Set(
            readdirSync(tempFs.tempDir, { recursive: true, withFileTypes: true })
                .filter((entry) => entry.isFile())
                .map((entry) => relative(tempFs.tempDir, join(entry.parentPath, entry.name))),
        );
        const executed: ExecutorContext = {
            root: tempFs.tempDir,
            cwd: tempFs.tempDir,
            isVerbose: false,
            projectName: 'tiny',
            targetName: 'build--cargo',
            projectGraph: { nodes: { tiny: { name: 'tiny', type: 'app', data: { root: '.' } } }, dependencies: {} },
            projectsConfigurations: { version: 2, projects: {} },
            nxJsonConfiguration: {},
        };

        const result = await execute(options, executed);

        const written = readdirSync(tempFs.tempDir, { recursive: true, withFileTypes: true })
            .filter((entry) => entry.isFile())
            .map((entry) => relative(tempFs.tempDir, join(entry.parentPath, entry.name)))
            .filter((file) => !before.has(file));
        expect(result).toStrictEqual({ success: true });
        expect(options.outputs).toStrictEqual(['{workspaceRoot}/target/debug/tiny', '{workspaceRoot}/target/debug/tiny.d']);
        expect(Array.difference(['target/debug/tiny', 'target/debug/tiny.d'], written)).toStrictEqual([]);
        expect(written.filter((file) => !file.startsWith('target/'))).toStrictEqual([]);
    });
});
