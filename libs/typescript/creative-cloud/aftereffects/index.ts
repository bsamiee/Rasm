import { Requests } from '@rasm/automation';
import { ExtensionManifest } from '@rasm/cep';

// --- [COMPOSITION] ---------------------------------------------------------------------

const AfterEffects = { identifier: 'com.adobe.AfterEffects', Contract: Requests, script: '@rasm/aftereffects/extendscript', target: ExtensionManifest.target } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { AfterEffects };
