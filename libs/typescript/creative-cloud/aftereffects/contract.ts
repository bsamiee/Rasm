import { Requests } from '@rasm/automation/requests';
import { ExtensionManifest } from '@rasm/cep/manifest';

// --- [COMPOSITION] ---------------------------------------------------------------------

const AfterEffects = { identifier: 'com.adobe.AfterEffects', Contract: Requests, host: { app: 'AEFT', minVersion: '26.0' }, script: '@rasm/aftereffects/extendscript', target: ExtensionManifest.target } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { AfterEffects };
