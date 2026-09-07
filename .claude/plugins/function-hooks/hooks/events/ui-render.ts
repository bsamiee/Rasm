// The AbovePrompt band holds the stored notice and the open finding count, with a button that hides the notice

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On, RenderElement } from 'claude-code';
import { fromBoolean, liftPredicate, map, toArray } from '../composition/option.ts';
import type { Options } from '../host/options.ts';
import { decodeFindings, decodeNotice, key, keys, type Notice } from '../host/store.ts';
import { open, openLine } from '../policies/findings.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SEPARATOR = '  ';

// --- [REGISTRATION] --------------------------------------------------------------------

const uiRender = (on: On, _options: Options): void => {
    // The band is terminal only and yields to a survey, the matcher pins both and the host passes every other instance to next(e) itself
    on('ui.render', { component: 'AbovePrompt', surface: 'terminal', props: { hasSurvey: false } }, async ($, e, next) => {
        const all = await $.store.keys();
        const [noticeValue, ...values] = await Promise.all([$.store.get(key('notice')), ...keys('findings')(all).map((name) => $.store.get(name))]);
        const openCount = open(decodeFindings(values));
        const text = [
            ...toArray(map((notice: Notice) => notice.text)(decodeNotice(noticeValue))),
            ...toArray(liftPredicate<string>(() => openCount > 0)(openLine(openCount))),
        ].join(_SEPARATOR);
        return fromBoolean(text !== '').match<Promise<RenderElement>>({
            some: async (): Promise<RenderElement> => {
                const t = await $.ui.resolve(e);
                return t.Box({
                    gap: 1,
                    children: [
                        t.Text({ children: text }),
                        t.Button({
                            hotkey: '0',
                            key: 'hide',
                            label: 'Hide',
                            // The press runs as its own async arm, the redraw follows the delete and a failed delete drops the redraw
                            onPress: (): void => {
                                (async (): Promise<void> => {
                                    await $.store.delete(key('notice'));
                                    $.ui.invalidate('ui.render');
                                })().catch(() => undefined);
                            },
                        }),
                    ],
                });
            },
            none: () => next(e),
        });
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { uiRender };
