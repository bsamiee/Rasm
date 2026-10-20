import { manifest } from '@rasm/indesign/manifest';
import { Manifest } from '@rasm/uxp/manifest';
import { Config, Effect, Option } from 'effect';
import { uxp } from '../../../../vite.config.ts';
import npm from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

const name = 'Galley';
const origin = await Effect.runPromise(Effect.map(Config.URL('ATELIER_URL'), Manifest.loopback));

// --- [EXPORTS] -------------------------------------------------------------------------

export default uxp(import.meta.dirname, {
    main: npm.main,
    manifest: Manifest.make({
        id: 'rasm.galley',
        name,
        version: npm.version,
        host: manifest.host,
        entrypoints: [{ type: 'panel', id: 'galley', label: { default: name }, minimumSize: Option.none() }],
        requiredPermissions: { ...manifest.requiredPermissions, network: Option.some({ domains: [origin] }) },
    }),
});
