import { Requests } from '@rasm/automation';
import { ExtensionManifest } from '@rasm/cep';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Illustrator = { identifier: 'com.adobe.illustrator', Contract: Requests, host: { app: 'ILST', minVersion: '30.0' }, script: '@rasm/illustrator/extendscript', target: ExtensionManifest.target } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Illustrator };
