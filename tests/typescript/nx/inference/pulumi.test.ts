import { createNodes } from '@rasm/nx-pulumi';
import manifest from '@rasm/nx-pulumi/package.json' with { type: 'json' };
import { cases } from '#/inference/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases({
    plugin: { name: manifest.name, createNodes },
    files: {
        'package.json': '{ "name": "root", "workspaces": ["infra", "site"] }\n',
        'infra/package.json': '{ "name": "infra", "type": "module", "dependencies": { "@pulumi/pulumi": "*" }, "devDependencies": { "@rasm/nx-pulumi": "*" } }\n',
        'site/package.json': '{ "name": "site", "type": "module", "dependencies": { "infra": "*" } }\n',
    },
});
