import { Requests } from '@rasm/automation';
import { ExtensionManifest } from '@rasm/cep';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Illustrator = { identifier: 'com.adobe.illustrator', Contract: Requests, script: '@rasm/illustrator/extendscript', target: ExtensionManifest.target } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Illustrator };
