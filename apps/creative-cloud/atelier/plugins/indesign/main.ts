import { link } from '@rasm/indesign/uxp';
import { Plugin } from '@rasm/uxp/plugin';
import { Effect } from 'effect';
import { typesetting } from '#/typesetting';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { linked, scripted } = Effect.runSync(link);

Plugin.serve({ link: linked, panels: { typesetting: typesetting(scripted) } });
