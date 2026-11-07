import { AfterEffects } from '@rasm/aftereffects';
import type { Installation } from '@rasm/applications';
import { Peer } from '@rasm/automation/peer';
import { Illustrator } from '@rasm/illustrator';
import { InDesign } from '@rasm/indesign';
import { Photoshop } from '@rasm/photoshop';
import { Array, Layer } from 'effect';
import { atelierLink } from '#/atelier-link.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Libraries = InDesign.Contract.merge(Photoshop.Contract);

// --- [OPERATIONS] ----------------------------------------------------------------------

const library = (installation: Installation) => Array.findFirst([InDesign, Photoshop, Illustrator, AfterEffects] as const, ({ identifier }) => installation.identifier.startsWith(identifier));

// --- [COMPOSITION] ---------------------------------------------------------------------

const listening = Peer.listen(Libraries, { path: atelierLink.path, middleware: Layer.empty });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Libraries, library, listening };
