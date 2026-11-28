import type { Element } from '@rasm/claude-code/element';
import { Plugin } from '@rasm/claude-code/events';
import { host } from '@rasm/claude-code/host';
import type { On, RenderElement } from 'claude-code';
import { Array, Context, Data, Effect, Layer, Number, Option, Order, type PlatformError, Record, Stream, SubscriptionRef } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORT = 'report';
const COMMAND = 'tools';

// --- [MODELS] --------------------------------------------------------------------------

class Seen extends Data.Class<{ readonly tools: Record.ReadonlyRecord<string, number>; readonly turn: { readonly calls: number; readonly refused: number } }> {}

// --- [SERVICES] ------------------------------------------------------------------------

class Calls extends Context.Service<Calls, { readonly seen: SubscriptionRef.SubscriptionRef<Seen> }>()('@rasm/sazandeh/skills/mod-ui/templates/transcript/Calls') {
    static readonly layer = Layer.effect(
        Calls,
        Effect.map(SubscriptionRef.make(new Seen({ tools: {}, turn: { calls: 0, refused: 0 } })), (seen) => ({ seen })),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const change = (update: (seen: Seen) => Seen) => Calls.use(({ seen }) => SubscriptionRef.update(seen, update));
const ranked = (tools: Record.ReadonlyRecord<string, number>) =>
    Array.sort(
        Record.toEntries(tools),
        Order.mapInput(Order.flip(Number.Order), ([, count]: readonly [string, number]) => count),
    );
const table = (rows: readonly (readonly [string, string])[]): Element => {
    const width = Math.max(0, ...rows.map(([label]) => label.length));
    return { _tag: 'Box', flexDirection: 'column', children: rows.map(([label, value]) => ({ _tag: 'Text', children: [{ _tag: 'Text', dimColor: true, children: label.padEnd(width + 2) }, value] })) };
};
const pairs = (source: string) =>
    source
        .split('\n')
        .map((line) => line.split(': '))
        .filter((parts) => parts.length > 1)
        .map(([label = '', ...value]) => [label, value.join(': ')] as const);

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    host(on, {
        layer: Calls.layer,
        hooks: {
            'turn.start': (e, next) =>
                e.agentId === undefined
                    ? Effect.andThen(
                          change((seen) => new Seen({ ...seen, turn: { calls: 0, refused: 0 } })),
                          next(e),
                      )
                    : next(e),
            'tool.call': (e, next) =>
                Effect.tap(next(e), (result) =>
                    change(
                        (seen) =>
                            new Seen({
                                tools: Record.set(seen.tools, e.tool, (seen.tools[e.tool] ?? 0) + 1),
                                turn: { calls: seen.turn.calls + 1, refused: seen.turn.refused + (result.deny === undefined ? 0 : 1) },
                            }),
                    ),
                ),
        },
        commands: {
            [COMMAND]: {
                description: 'Count the tool calls of this session by tool',
                run: () =>
                    Calls.use(({ seen }) =>
                        Effect.map(SubscriptionRef.get(seen), ({ tools }) => {
                            const rows = ranked(tools);
                            return { text: rows.map(([tool, count]) => `${tool}: ${count}`).join('\n'), context: rows.map(([tool, count]) => `tool ${tool}: ${count} calls`), isTextDisplayOnly: true as const };
                        }),
                    ),
            },
        },
        render: {
            state: Stream.unwrap(Calls.use(({ seen }) => Effect.succeed(seen.pipe(SubscriptionRef.changes)))),
            ToolUse: (e, seen, next) => (e.props.isExpanded === true ? next() : Effect.map(next(), (row): Element => ({ _tag: 'Box', flexDirection: 'column', children: [row, { _tag: 'Text', dimColor: true, children: `  ⎿ ${e.props.tool} calls in this session: ${seen.tools[e.props.tool] ?? 0}` }] }))),
            TurnDuration: (_e, { turn }, next) => (turn.calls === 0 ? next() : Effect.map(next(), (row): Element => ({ _tag: 'Box', flexDirection: 'column', children: [row, { _tag: 'Text', dimColor: true, children: `  calls ${turn.calls} · refused ${turn.refused}` }] }))),
            CodeBlock: (e, _seen, next) => (e.props.language === REPORT && e.props.isClosed ? Effect.succeed(table(pairs(e.props.source))) : next()),
            CommandOutput: (e, _seen, next) =>
                Plugin.use(({ name: plugin, commands }): Effect.Effect<Element | RenderElement, PlatformError.PlatformError> => (Option.exists(Record.get(commands, COMMAND), (name) => name === e.props.command) && !e.props.isErrored ? Effect.succeed(table(pairs(e.props.text.replace(`${plugin}: `, '')))) : next())),
        },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
