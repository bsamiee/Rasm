import { Plugin } from '@rasm/uxp/plugin';
import { Effect } from 'effect';
import { link } from '#/handlers';
import { typesetting } from '#/typesetting';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { linked, scripted } = Effect.runSync(link);

Plugin.serve({ link: linked, panels: { typesetting: typesetting(scripted) } });
