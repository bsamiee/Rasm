import type { Installation } from '@rasm/applications';
import { Photoshop, Requests } from '@rasm/atelier-protocol';
import { InDesign } from '@rasm/atelier-protocol/indesign';
import { Peer } from '@rasm/atelier-protocol/peer';
import { ExtensionManifest } from '@rasm/cep';
import { UxpManifest } from '@rasm/uxp';
import { Array, Layer } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Libraries = InDesign.merge(Photoshop);

// --- [TABLES] --------------------------------------------------------------------------

const LIBRARIES = [
    { identifier: 'com.adobe.InDesign', Contract: InDesign, target: UxpManifest.target },
    { identifier: 'com.adobe.Photoshop', Contract: Photoshop, target: UxpManifest.target },
    { identifier: 'com.adobe.illustrator', Contract: Requests, target: ExtensionManifest.target },
    { identifier: 'com.adobe.AfterEffects', Contract: Requests, target: ExtensionManifest.target },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const library = (installation: Installation) => Array.findFirst(LIBRARIES, ({ identifier }) => installation.identifier.startsWith(identifier));

// --- [COMPOSITION] ---------------------------------------------------------------------

const listening = Peer.listen(Libraries, Layer.empty);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Libraries, library, listening };
