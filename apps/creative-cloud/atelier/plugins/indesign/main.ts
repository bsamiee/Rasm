import { linkInDesign } from '@rasm/indesign/uxp';
import { UxpPlugin } from '@rasm/uxp/uxp';
import { Effect } from 'effect';
import type { panels } from '#/panels';
import { typesetting } from '#/typesetting';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { linked, scripted } = Effect.runSync(linkInDesign);

UxpPlugin.serve({ link: linked, panels: { typesetting: typesetting(scripted) } satisfies Record<keyof typeof panels, unknown> });
