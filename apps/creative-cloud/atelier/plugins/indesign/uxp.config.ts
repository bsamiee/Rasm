import { Manifest } from '@rasm/uxp/manifest';
import { Option } from 'effect';
import npm from '#/package.json' with { type: 'json' };
import { uxp } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, {
    daemon: 'atelier',
    manifest: Manifest.make({
        id: 'app.rasm.atelier.indesign',
        name: npm.description,
        version: npm.version,
        host: { app: 'ID', minVersion: '18.5.0', data: Option.some({ loadEvent: 'startup' }) },
        entrypoints: [],
        requiredPermissions: { localFileSystem: Option.some('fullAccess'), allowCodeGenerationFromStrings: Option.some(true) },
    }),
});
