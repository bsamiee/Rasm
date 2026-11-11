import { assert } from '@effect/vitest';
import { createNodes } from '@rasm/nx-distribution';
import manifest from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Effect } from 'effect';
import { cases, inferred, roots, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BLENDER = 'schema_version = "1.0.0"\nid = "sheet_layout"\nversion = "0.1.0"\nname = "Sheets"\ntagline = "Sheet layout"\nmaintainer = "Studio <studio@example.com>"\ntype = "add-on"\nblender_version_min = "5.0.0"\nlicense = ["SPDX:MIT"]\n';

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    tools: ['blender'],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["sheets", "panel"] }\n',
        'sheets/package.json': '{ "name": "sheets" }\n',
        'sheets/blender_manifest.toml': BLENDER,
        'sheets/__init__.py': 'def register():\n    pass\n\n\ndef unregister():\n    pass\n',
        'panel/package.json': '{ "name": "panel" }\n',
        'panel/manifest.json': '{ "manifestVersion": 5, "id": "com.example.panel", "name": "Panel", "version": "0.1.0", "main": "index.html", "host": { "app": "PS", "minVersion": "27.0.0" }, "entrypoints": [{ "type": "panel", "id": "panel", "label": { "default": "Panel" } }] }\n',
        'panel/index.html': '<!doctype html><html lang="en"><head><title>Panel</title></head><body></body></html>\n',
    },
    broken: ['sheets/blender_manifest.toml', 'schema_version = "1.0.0\nid = \n'],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names a project by the id its host manifest states where no other manifest names the folder', () =>
        Effect.map(inferred({ ...subject, files: { 'package.json': '{ "name": "root" }\n', 'sheets/blender_manifest.toml': BLENDER } }), (graph) => assert.deepOwnInclude(roots(graph), { sheet_layout: 'sheets' })),
    );
});
