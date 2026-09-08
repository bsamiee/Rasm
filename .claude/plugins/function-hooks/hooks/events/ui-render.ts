// The AbovePrompt band under the dispatch option, the stored notice and the open count of the summary row, with a button that hides the notice

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On, RenderElement } from 'claude-code';
import { flatMap, liftPredicate, map, toArray } from '../composition/option.ts';
import { type Options, whenEnabled } from '../host/options.ts';
import { decodeNotice, decodeSummary, key, type Notice, type Summary } from '../host/store.ts';
import { openLine } from '../policies/findings.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SEPARATOR = '  ';

// --- [REGISTRATION] --------------------------------------------------------------------

const _band = (on: On): void => {
    // The band is terminal only and yields to a survey, the matcher pins both and the host passes every other instance to next(e) itself
    on('ui.render', { component: 'AbovePrompt', surface: 'terminal', props: { hasSurvey: false } }, async ($, e, next) => {
        const [noticeValue, summaryValue] = await Promise.all([$.store.get(key('notice')), $.store.get(key('summary'))]);
        // The summary row is written at session.start, the band reads it through its decoder alone
        const text = liftPredicate<string>((line) => line !== '')(
            [
                ...toArray(map((notice: Notice) => notice.text)(decodeNotice(noticeValue))),
                ...toArray(
                    flatMap((summary: Summary) => liftPredicate<string>(() => summary.open > 0)(openLine(summary.open)))(decodeSummary(summaryValue)),
                ),
            ].join(_SEPARATOR),
        );
        // The press runs as its own async arm, the redraw follows the delete and a failed delete drops the redraw
        const hide = (): void => {
            $.store
                .delete(key('notice'))
                .then(() => $.ui.invalidate('ui.render'))
                .catch(() => undefined);
        };
        return text.match<Promise<RenderElement>>({
            some: async (line): Promise<RenderElement> => {
                const t = await $.ui.resolve(e);
                return t.Box({
                    gap: 1,
                    children: [t.Text({ children: line }), t.Button({ hotkey: '0', key: 'hide', label: 'Hide', onPress: hide })],
                });
            },
            none: () => next(e),
        });
    });
};

// Every value the band draws is written under classify or dispatch, and the band registers while dispatch is on
const uiRender = (on: On, options: Options): void => {
    whenEnabled(options.dispatch, () => _band(on));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { uiRender };
