import { assert } from '@effect/vitest';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import manifest from '@rasm/nx-cargo/package.json' with { type: 'json' };
import { Effect, Record } from 'effect';
import { cases, edges, inferred, roots, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    tools: ['cargo', 'rustc'],
    state: ['target'],
    typed: true,
    runs: ['check'],
    compiled: ['crates/core/src/lib.rs', 'check--cargo'],
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

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names no project at the folder of a workspace-only root manifest where no project marker sits beside it', () =>
        Effect.map(inferred({ ...subject, files: Record.remove(subject.files, 'package.json') }), (graph) => assert.notInclude(Object.values(roots(graph)), '.', 'nx-go/nx-go@1785753 packages/nx-go/src/graph/create-nodes.spec.ts root go.mod only with project.json')),
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
