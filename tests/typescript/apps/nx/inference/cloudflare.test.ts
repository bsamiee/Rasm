import { createNodes } from '@rasm/nx-cloudflare';
import manifest from '@rasm/nx-cloudflare/package.json' with { type: 'json' };
import { cases } from '#/inference/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases({
    plugin: { name: manifest.name, createNodes },
    files: {
        'package.json': '{ "name": "root", "workspaces": ["worker", "queue"] }\n',
        'worker/package.json': '{ "name": "worker", "type": "module" }\n',
        'worker/cloudflare.config.ts': 'export default {};\n',
        'queue/package.json': '{ "name": "queue", "type": "module" }\n',
        'queue/cloudflare.config.ts': 'export default {};\n',
    },
});
