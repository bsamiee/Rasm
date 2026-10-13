import { Config, Effect } from 'effect';
import { uxp } from '../../../../vite.config.ts';
import npm from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

const origin = await Effect.runPromise(Effect.map(Config.URL('ATELIER_URL'), ({ port }) => `ws://localhost:${port}`));
const manifest = {
    manifestVersion: 5,
    id: 'rasm.galley',
    name: 'Galley',
    version: npm.version,
    host: { app: 'ID', minVersion: '18.5.0' },
    entrypoints: [{ type: 'panel', id: 'galley', label: { default: 'Galley' }, minimumSize: { width: 200, height: 60 } }],
    requiredPermissions: { network: { domains: [origin] } },
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export default { ...uxp(import.meta.dirname, { main: npm.main, manifest }), define: { 'import.meta.env.ATELIER': JSON.stringify(origin) } };
