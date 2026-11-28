import type { Element } from '@rasm/claude-code/element';
import { Engine } from '@rasm/claude-code/engine';
import { host } from '@rasm/claude-code/host';
import { Panes } from '@rasm/claude-code/pane';
import type { On } from 'claude-code';
import { Array, Context, Data, Duration, Effect, Layer, Option, type PlatformError, Schedule, Stream, SubscriptionRef } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PANE = 'running';
const BAND = 3;
const DRAWN = { every: Duration.millis(50), times: 20 } as const;

// --- [MODELS] --------------------------------------------------------------------------

class Running extends Data.Class<{ readonly key: string; readonly tool: string; readonly agent: Option.Option<string> }> {}

// --- [SERVICES] ------------------------------------------------------------------------

class Calls extends Context.Service<Calls, { readonly running: SubscriptionRef.SubscriptionRef<readonly Running[]> }>()('@rasm/sazandeh/skills/mod-ui/templates/above-prompt/Calls') {
    static readonly layer = Layer.effect(
        Calls,
        Effect.map(SubscriptionRef.make<readonly Running[]>([]), (running) => ({ running })),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const change = (update: (running: readonly Running[]) => readonly Running[]) => Calls.use(({ running }) => SubscriptionRef.update(running, update));
const reveal = (key: string) =>
    Effect.andThen(
        Panes.use((panes) => panes.open({ id: PANE })),
        Engine.use((engine) => engine.scroll({ in: PANE, to: { key } })).pipe(Effect.retry({ schedule: Schedule.spaced(DRAWN.every), times: DRAWN.times })),
    );
const button = (call: Running): Element<Panes | Engine, PlatformError.PlatformError> => ({ _tag: 'Button', key: call.key, onPress: () => reveal(call.key), children: { _tag: 'Text', wrap: 'truncate-end', children: call.tool } });
const line = (call: Running): Element => ({
    _tag: 'Box',
    key: call.key,
    flexDirection: 'row',
    gap: 2,
    children: [
        { _tag: 'Text', children: call.tool },
        { _tag: 'Text', dimColor: true, children: Option.getOrElse(call.agent, () => 'main') },
    ],
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    host(on, {
        layer: Calls.layer,
        hooks: {
            'tool.call': (e, next) => {
                const call = new Running({ key: e.tool_use_id, tool: e.tool, agent: Option.fromUndefinedOr(e.agentId) });
                return Effect.andThen(
                    change((running) => [...running, call]),
                    Effect.ensuring(
                        next(e),
                        change((running) => running.filter((held) => held.key !== call.key)),
                    ),
                );
            },
        },
        render: {
            state: Stream.unwrap(Calls.use(({ running }) => Effect.succeed(running.pipe(SubscriptionRef.changes)))),
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
