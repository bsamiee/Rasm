import indesign from 'adobe:indesign';
import { host } from '@rasm/indesign/host';
import { Plugin } from '@rasm/uxp/plugin';
import type { panels } from '#/panels';
import { typesetting } from '#/typesetting';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { linked, scripted } = host(indesign);

Plugin.serve({ link: linked, panels: { typesetting: typesetting(scripted) } satisfies Record<keyof typeof panels, unknown> });
