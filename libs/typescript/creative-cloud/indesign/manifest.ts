import type { Manifest } from '@rasm/uxp/manifest';
import { Option } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const manifest = {
    host: { app: 'ID', minVersion: '18.5.0' },
    requiredPermissions: { localFileSystem: Option.some('fullAccess') },
} as const satisfies Pick<Parameters<typeof Manifest.make>[0], 'host' | 'requiredPermissions'>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { manifest };
