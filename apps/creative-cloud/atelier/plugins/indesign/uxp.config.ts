import { Option } from 'effect';
import npm from '#/package.json' with { type: 'json' };
import { uxp } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, {
    npm,
    daemon: 'atelier',
    manifest: {
        host: { app: 'ID', minVersion: '18.5.0', data: Option.some({ loadEvent: 'startup' }) },
        entrypoints: [],
        requiredPermissions: { localFileSystem: Option.some('fullAccess'), allowCodeGenerationFromStrings: Option.some(true) },
    },
});
