import type { Manifest } from '@rasm/uxp/manifest';
import { Option } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const manifest = {
    host: { app: 'ID', minVersion: '18.5.0' },
    requiredPermissions: { localFileSystem: Option.some('fullAccess') },
} as const satisfies Pick<Manifest, 'host'> & { readonly requiredPermissions: Pick<Manifest['requiredPermissions'], 'localFileSystem'> };

// --- [EXPORTS] -------------------------------------------------------------------------

export { manifest };
