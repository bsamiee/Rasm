import type { Element } from '@rasm/claude-code/element';
import { host } from '@rasm/claude-code/host';
import { Ledger, type Use } from '@rasm/claude-code/ledger';
import { Panes } from '@rasm/claude-code/pane';
import type { On } from 'claude-code';
import { Array, DateTime, Effect, HashMap, Layer, Option, Order, type PlatformError, Stream, SubscriptionRef } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PANE = 'running';
const BAND = 3;

// --- [OPERATIONS] ----------------------------------------------------------------------

const button = (use: Use): Element<Panes, PlatformError.PlatformError> => ({ _tag: 'Button', key: use.call, onPress: () => Panes.use((panes) => panes.open({ id: PANE }, { to: { key: use.call } })), children: { _tag: 'Text', wrap: 'truncate-end', children: use.tool } });
const line = (use: Use): Element => ({
    _tag: 'Box',
    key: use.call,
    flexDirection: 'row',
    gap: 2,
    children: [
        { _tag: 'Text', children: use.tool },
        { _tag: 'Text', dimColor: true, children: Option.getOrElse(use.agent, () => 'main') },
    ],
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    host(on, {
        layer: Layer.empty,
        hooks: {
            'tool.call': (e, next) => Ledger.use((ledger) => ledger.track(e, Option.none(), next(e))),
        },
        render: {
            state: Stream.unwrap(Ledger.use(({ state }) => Effect.succeed(state.pipe(SubscriptionRef.changes)))).pipe(
                Stream.map(({ uses }) =>
                    uses.pipe(
                        HashMap.toValues,
                        Array.filter((use) => Option.isNone(use.end)),
                        Array.sort(Order.mapInput(DateTime.Order, (use: Use) => use.start)),
                    ),
                ),
            ),
            AbovePrompt: (e, running, next) =>
                e.props.hasSurvey || !Array.isReadonlyArrayNonEmpty(running)
                    ? next()
                    : Effect.succeed({
                          _tag: 'Box',
                          flexDirection: 'row',
                          gap: 1,
                          children: [{ _tag: 'Text', dimColor: true, children: 'running' }, ...running.slice(0, BAND).map(button), ...(running.length > BAND ? [{ _tag: 'Button', key: PANE, label: `+${running.length - BAND}`, onPress: () => Panes.use((panes) => panes.open({ id: PANE })) } as const] : [])],
                      }),
            PromptHint: (_e, running, next) => (Array.isReadonlyArrayNonEmpty(running) ? next({ tail: 'ctrl+x tab reaches the running calls' }) : next()),
            Pane: {
                [PANE]: { title: 'Running', rows: 8, closeOnEscape: true, draw: (_e, running) => Effect.succeed({ _tag: 'Box', flexDirection: 'column', children: Array.match(running, { onEmpty: () => [{ _tag: 'Text', dimColor: true, children: 'no call runs now' }], onNonEmpty: (held) => held.map(line) }) }) },
            },
        },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
