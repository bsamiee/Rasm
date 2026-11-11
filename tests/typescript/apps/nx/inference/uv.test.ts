import { assert } from '@effect/vitest';
import { createDependencies, createNodes } from '@rasm/nx-uv';
import manifest from '@rasm/nx-uv/package.json' with { type: 'json' };
import { Effect } from 'effect';
import { cases, edges, inferred, roots, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    tools: ['uv'],
    state: ['.venv'],
    files: {
        'package.json': '{ "name": "root" }\n',
        'pyproject.toml': '[project]\nname = "sheets"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["sheets-core"]\n\n[tool.uv.workspace]\nmembers = ["packages/*"]\n\n[tool.uv.sources]\nsheets-core = { workspace = true }\n',
        'uv.lock':
            'version = 1\nrevision = 5\nrequires-python = ">=3.13"\n\n[manifest]\nmembers = [\n    "sheets",\n    "sheets-core",\n]\n\n[[package]]\nname = "sheets"\nversion = "0.1.0"\nsource = { virtual = "." }\ndependencies = [\n    { name = "sheets-core" },\n]\n\n[package.metadata]\nrequires-dist = [{ name = "sheets-core", editable = "packages/core" }]\n\n[[package]]\nname = "sheets-core"\nversion = "0.1.0"\nsource = { editable = "packages/core" }\n',
        'packages/core/pyproject.toml': '[project]\nname = "sheets-core"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[build-system]\nrequires = ["uv_build>=0.9,<1"]\nbuild-backend = "uv_build"\n',
        'packages/core/sheets_core/__init__.py': '',
        'tools/pyproject.toml': '[project]\nname = "sheet-tools"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n',
        'tools/uv.lock': 'version = 1\nrevision = 5\nrequires-python = ">=3.13"\n\n[[package]]\nname = "sheet-tools"\nversion = "0.1.0"\nsource = { virtual = "." }\n',
    },
    broken: ['packages/core/pyproject.toml', '[project\nname = "sheets-core"\n'],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names each project by its project name and links a workspace source from the pyproject.toml that states it', () =>
        Effect.map(inferred(subject), (graph) => assert.deepOwnInclude({ ...roots(graph), edges: edges(graph) }, { sheets: '.', 'sheets-core': 'packages/core', 'sheet-tools': 'tools', edges: [{ source: 'sheets', target: 'sheets-core', type: 'static' }] })),
    );
});
