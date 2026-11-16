import type { Installation } from '@rasm/applications';
import { aftereffects } from '@rasm/atelier-aftereffects-plugin/tools';
import { illustrator } from '@rasm/atelier-illustrator-plugin/tools';
import { InDesign } from '@rasm/atelier-indesign-plugin/contract';
import { indesign } from '@rasm/atelier-indesign-plugin/tools';
import { Photoshop } from '@rasm/atelier-photoshop-plugin/contract';
import { photoshop } from '@rasm/atelier-photoshop-plugin/tools';
import { Requests } from '@rasm/atelier-protocol';
import { Peer } from '@rasm/atelier-protocol/peer';
import { ExtensionManifest } from '@rasm/cep';
import { UxpManifest } from '@rasm/uxp';
import { Array, Layer } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Libraries = InDesign.merge(Photoshop);

// --- [TABLES] --------------------------------------------------------------------------

const LIBRARIES = [
    { identifier: 'com.adobe.InDesign', Contract: InDesign, target: UxpManifest.target, instructions: indesign.instructions },
    { identifier: 'com.adobe.Photoshop', Contract: Photoshop, target: UxpManifest.target, instructions: photoshop.instructions },
    { identifier: 'com.adobe.illustrator', Contract: Requests, target: ExtensionManifest.target, instructions: illustrator.instructions },
    { identifier: 'com.adobe.AfterEffects', Contract: Requests, target: ExtensionManifest.target, instructions: aftereffects.instructions },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const library = (installation: Installation) => Array.findFirst(LIBRARIES, ({ identifier }) => installation.identifier.startsWith(identifier));

// --- [COMPOSITION] ---------------------------------------------------------------------

const listening = Peer.listen(Libraries, Layer.empty);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Libraries, LIBRARIES, library, listening };
