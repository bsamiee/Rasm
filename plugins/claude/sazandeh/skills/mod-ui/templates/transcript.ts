import type { Element } from '@rasm/claude-code/element';
import { host } from '@rasm/claude-code/host';
import type { On } from 'claude-code';
import { Context, Data, Effect, Layer, Record, Stream, SubscriptionRef } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORT = 'report';

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
        render: {
            state: Stream.unwrap(Calls.use(({ seen }) => Effect.succeed(seen.pipe(SubscriptionRef.changes)))),
            ToolUse: (e, seen, next) => (e.props.isExpanded === true ? next() : Effect.map(next(), (row): Element => ({ _tag: 'Box', flexDirection: 'column', children: [row, { _tag: 'Text', dimColor: true, children: `  ⎿ ${e.props.tool} calls in this session: ${seen.tools[e.props.tool] ?? 0}` }] }))),
            TurnDuration: (_e, { turn }, next) => (turn.calls === 0 ? next() : Effect.map(next(), (row): Element => ({ _tag: 'Box', flexDirection: 'column', children: [row, { _tag: 'Text', dimColor: true, children: `  calls ${turn.calls} · refused ${turn.refused}` }] }))),
            CodeBlock: (e, _seen, next) => (e.props.language === REPORT && e.props.isClosed ? Effect.succeed(table(pairs(e.props.source))) : next()),
        },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
