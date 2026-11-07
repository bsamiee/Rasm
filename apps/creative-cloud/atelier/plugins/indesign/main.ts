import { link } from '@rasm/indesign/uxp';
import { Plugin } from '@rasm/uxp/plugin';
import { Effect } from 'effect';
import type { panels } from '#/panels.ts';
import { typesetting } from '#/typesetting.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { linked, scripted } = Effect.runSync(link);

Plugin.serve({ link: linked, panels: { typesetting: typesetting(scripted) } satisfies Record<keyof typeof panels, unknown> });
