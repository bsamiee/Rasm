import { assert } from '@effect/vitest';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import manifest from '@rasm/nx-cargo/package.json' with { type: 'json' };
import { Array, Effect, Record } from 'effect';
import { cases, edges, inferred, roots, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    tools: ['cargo', 'rustc'],
    state: ['target'],
    locked: [
        ['Cargo.toml', 'cargo', 'generate-lockfile'],
        ['tools/gen/Cargo.toml', 'cargo', 'generate-lockfile'],
    ],
    typed: true,
    runs: ['check'],
    compiled: ['crates/core/src/lib.rs', 'check--clippy'],
    changed: ['crates/core/src/main.rs', 'fn main() {}\n'],
    files: {
        'package.json': '{ "name": "root" }\n',
        'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
        'crates/core/Cargo.toml': '[package]\nname = "sheet-core"\nversion = "0.1.0"\nedition = "2024"\n',
        'crates/core/src/lib.rs': 'pub fn sheets() -> usize {\n    1\n}\n',
        'crates/cli/Cargo.toml': '[package]\nname = "sheet-cli"\nversion = "0.1.0"\nedition = "2024"\npublish = false\n\n[package.metadata.nx]\nimplicitDependencies = ["sheet-gen"]\n\n[dependencies]\nsheet-core = { path = "../core" }\n',
        'crates/cli/src/main.rs': 'fn main() {\n    println!("{}", sheet_core::sheets());\n}\n',
        'tools/gen/Cargo.toml': '[package]\nname = "sheet-gen"\nversion = "0.1.0"\nedition = "2024"\n\n[workspace]\n',
        'tools/gen/src/main.rs': 'fn main() {}\n',
    },
    broken: ['crates/cli/Cargo.toml', '[package\nname = "sheet-cli"\n'],
};

const CRATE = (name: string, tables: string) => `[package]\nname = "${name}"\nversion = "0.1.0"\nedition = "2024"\n${tables}`;
const TABLES = ['dependencies', 'dev-dependencies', 'build-dependencies', "target.'cfg(unix)'.dependencies"] as const;
const GONX = 'naxodev/oss@8b43d4a packages/gonx/src/graph/static-analysis/index.spec.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names no project at the folder of a workspace-only root manifest where no project marker sits beside it', () =>
        Effect.map(inferred({ ...subject, files: Record.remove(subject.files, 'package.json') }), (graph) => assert.notInclude(Object.values(roots(graph)), '.', 'nx-go/nx-go@1785753 packages/nx-go/src/graph/create-nodes.spec.ts root go.mod only with project.json')),
    );

    it.effect('names a project at the folder of a root manifest that holds a package beside its workspace', () =>
        inferred({ ...subject, files: { ...Record.remove(subject.files, 'package.json'), 'Cargo.toml': `[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n\n${CRATE('sheets', '')}`, 'src/lib.rs': 'pub fn sheets() -> usize {\n    0\n}\n' } }).pipe(
            Effect.map((graph) => assert.include(Object.values(roots(graph)), '.', 'nx-go/nx-go@1785753 packages/nx-go/src/graph/create-nodes.spec.ts root go.mod with project.json; Cargo reference "Root package"')),
        ),
    );

    it.effect('links a dependency a member inherits from the workspace table', () =>
        inferred({
            ...subject,
            files: {
                ...subject.files,
                'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n\n[workspace.dependencies]\nsheet-core = { path = "crates/core" }\n',
                'crates/cli/Cargo.toml': CRATE('sheet-cli', '\n[dependencies]\nsheet-core = { workspace = true }\n'),
            },
        }).pipe(
            Effect.map(edges),
            Effect.map((linked) => assert.deepInclude(linked, { source: 'sheet-cli', target: 'sheet_core', type: 'static' }, `${GONX} "static dependency through a replace directive"; Cargo reference "Inheriting a dependency from a workspace"`)),
        ),
    );

    it.effect.each(TABLES)('links a path dependency stated under %s once', (table) =>
        inferred({ ...subject, files: { ...subject.files, 'crates/cli/Cargo.toml': CRATE('sheet-cli', `\n[${table}]\nsheet-core = { path = "../core" }\n`), 'crates/cli/build.rs': 'fn main() {}\n' } }).pipe(
            Effect.map(edges),
            Effect.map(Array.filter(({ source }) => source === 'sheet-cli')),
            Effect.map((linked) => assert.deepStrictEqual(linked, [{ source: 'sheet-cli', target: 'sheet_core', type: 'static' }], `${GONX} "deduplicates edges"; Cargo reference "Specifying dependencies"`)),
        ),
    );

    it.effect('links a member to the members it depends on directly and to none it reaches through them', () =>
        inferred({
            ...subject,
            files: {
                'package.json': '{ "name": "root" }\n',
                'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
                'crates/leaf/Cargo.toml': CRATE('leaf', ''),
                'crates/leaf/src/lib.rs': 'pub fn leaf() -> usize {\n    1\n}\n',
                'crates/mid/Cargo.toml': CRATE('mid', '\n[dependencies]\nleaf = { path = "../leaf" }\n'),
                'crates/mid/src/lib.rs': 'pub fn mid() -> usize {\n    leaf::leaf()\n}\n',
                'crates/app/Cargo.toml': CRATE('app', '\n[dependencies]\nmid = { path = "../mid" }\n'),
                'crates/app/src/lib.rs': 'pub fn app() -> usize {\n    mid::mid()\n}\n',
            },
        }).pipe(
            Effect.map(edges),
            Effect.map(Array.map(({ source, target }) => `${source} ${target}`)),
            Effect.map((linked) => assert.deepStrictEqual(linked, ['app mid', 'mid leaf'], 'nrwl/nx@2a424d3 e2e/dotnet/src/dotnet-dependency-graph.test.ts "should only show direct dependencies, not transitive ones"')),
        ),
    );

    it.effect('names each library by its library target and every other package by its package name, links a path dependency from the manifest that states it, and links an implicit dependency its package.metadata.nx table states', () =>
        Effect.map(inferred(subject), (graph) =>
            assert.deepOwnInclude(
                { ...roots(graph), edges: edges(graph) },
                {
                    sheet_core: 'crates/core',
                    'sheet-cli': 'crates/cli',
                    'sheet-gen': 'tools/gen',
                    edges: [
                        { source: 'sheet-cli', target: 'sheet_core', type: 'static' },
                        { source: 'sheet-cli', target: 'sheet-gen', type: 'implicit' },
                    ],
                },
            ),
        ),
    );
});
