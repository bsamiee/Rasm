import { Requests } from '@rasm/automation/requests';
import { ExtensionManifest } from '@rasm/cep/manifest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const Illustrator = { identifier: 'com.adobe.illustrator', Contract: Requests, host: { app: 'ILST', minVersion: '30.0' }, script: '@rasm/illustrator/host', target: ExtensionManifest.target } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Illustrator };
