import { linkInDesign } from '@rasm/indesign/uxp';
import { UxpPlugin } from '@rasm/uxp/uxp';
import type { panels } from '#/panels';
import { typesetting } from '#/typesetting';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { linked, scripted } = linkInDesign();

UxpPlugin.serve({ link: linked, panels: { typesetting: typesetting(scripted) } satisfies Record<keyof typeof panels, unknown> });
