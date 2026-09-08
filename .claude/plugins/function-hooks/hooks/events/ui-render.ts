// The AbovePrompt band under the dispatch option, the stored notice and the open count of the summary row, with a button that hides the notice

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import type { Options } from '../host/options.ts';
import { decode, isNotice, isSummary, key } from '../host/store.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const _band = (on: On): void => {
    // The band is terminal only and yields to a survey, the matcher pins both and the host passes every other instance to next(e) itself
    on('ui.render', { component: 'AbovePrompt', surface: 'terminal', props: { hasSurvey: false } }, async ($, e, next) => {
        const [notice, summary] = await Promise.all([$.store.get(key('notice')), $.store.get(key('summary'))]);
        const open = decode(isSummary)(summary)?.open ?? 0;
        const line = [decode(isNotice)(notice)?.text ?? '', open > 0 ? `${open} open findings` : ''].filter((part) => part !== '').join('  ');
        if (line === '') {
            return next(e);
        }
        // The press runs as its own async arm, the redraw follows the delete and a failed delete drops the redraw
        const hide = (): void => {
            $.store
                .delete(key('notice'))
                .then(() => $.ui.invalidate('ui.render'))
                .catch(() => undefined);
        };
        const t = await $.ui.resolve(e);
        return t.Box({ gap: 1, children: [t.Text({ children: line }), t.Button({ hotkey: '0', key: 'hide', label: 'Hide', onPress: hide })] });
    });
};

// Every value the band draws is written under classify or dispatch, and the band registers while dispatch is on
const uiRender = (on: On, options: Options): void => {
    if (options.dispatch) {
        _band(on);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { uiRender };
