import { assert } from '@effect/vitest';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import manifest from '@rasm/nx-cargo/package.json' with { type: 'json' };
import { Effect } from 'effect';
import { cases, edges, inferred, roots, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    tools: ['cargo', 'rustc'],
    state: ['target'],
    files: {
        'package.json': '{ "name": "root" }\n',
        'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
        'crates/core/Cargo.toml': '[package]\nname = "sheet-core"\nversion = "0.1.0"\nedition = "2024"\n',
        'crates/core/src/lib.rs': 'pub fn sheets() -> usize {\n    1\n}\n',
        'crates/cli/Cargo.toml': '[package]\nname = "sheet-cli"\nversion = "0.1.0"\nedition = "2024"\npublish = false\n\n[dependencies]\nsheet-core = { path = "../core" }\n',
        'crates/cli/src/main.rs': 'fn main() {\n    println!("{}", sheet_core::sheets());\n}\n',
        'tools/gen/Cargo.toml': '[package]\nname = "sheet-gen"\nversion = "0.1.0"\nedition = "2024"\n\n[workspace]\n',
        'tools/gen/src/main.rs': 'fn main() {}\n',
    },
    broken: ['crates/cli/Cargo.toml', '[package\nname = "sheet-cli"\n'],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names each package by its package name and links a path dependency from the manifest that states it', () =>
        Effect.map(inferred(subject), (graph) => assert.deepOwnInclude({ ...roots(graph), edges: edges(graph) }, { 'sheet-core': 'crates/core', 'sheet-cli': 'crates/cli', 'sheet-gen': 'tools/gen', edges: [{ source: 'sheet-cli', target: 'sheet-core', type: 'static' }] })),
    );
});
