import type { Element } from '@rasm/claude-code/element';
import { host } from '@rasm/claude-code/host';
import { Panes } from '@rasm/claude-code/pane';
import type { On, PaneOpenArgs, RenderPropsOf } from 'claude-code';
import { Context, Data, Duration, Effect, Layer, Option, type PlatformError, Predicate, Stream, SubscriptionRef } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Stance = (typeof STANCES)[number];
type Drawn = Element<Calls | Panes, PlatformError.PlatformError>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const STANCES = ['running', 'refused', 'ended'] as const;
const PANES = { list: 'calls', form: 'forget' } as const;

// --- [MODELS] --------------------------------------------------------------------------

class Call extends Data.Class<{ readonly key: string; readonly tool: string; readonly agent: Option.Option<string>; readonly stance: Stance; readonly took: Option.Option<Duration.Duration> }> {}
class Held extends Data.Class<{ readonly calls: readonly Call[]; readonly forgets: Stance; readonly tool: Option.Option<string> }> {
    get doomed() {
        return this.calls.filter(
            (call) =>
                call.stance === this.forgets &&
                Option.getOrElse(
                    Option.map(this.tool, (named) => named === call.tool),
                    () => true,
                ),
        );
    }
    get counts() {
        return STANCES.map((stance) => [stance, this.calls.filter((call) => call.stance === stance)] as const).filter(([, calls]) => calls.length > 0);
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Calls extends Context.Service<Calls, { readonly held: SubscriptionRef.SubscriptionRef<Held> }>()('@rasm/sazandeh/skills/mod-ui/templates/pane/Calls') {
    static readonly layer = Layer.effect(
        Calls,
        Effect.map(SubscriptionRef.make(new Held({ calls: [], forgets: 'ended', tool: Option.none() })), (held) => ({ held })),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const change = (update: (held: Held) => Held) => Calls.use(({ held }) => SubscriptionRef.update(held, update));
const close = (id: string) => Panes.use((panes) => panes.close({ id }));
const summary = (held: Held) => held.counts.map(([stance, calls]) => `${calls.length} ${stance}`).join(' · ');
const row = (call: Call): Drawn => ({
    _tag: 'Box',
    key: call.key,
    flexDirection: 'row',
    gap: 2,
    children: [
        { _tag: 'Text', children: call.tool },
        { _tag: 'Text', dimColor: true, wrap: 'truncate-end', children: [Option.getOrElse(call.agent, () => 'main'), ...Option.toArray(Option.map(call.took, (took) => Duration.format(Duration.millis(Math.round(Duration.toMillis(took))))))].join(' · ') },
    ],
});
const list = (held: Held, placement: RenderPropsOf['Pane']['placement']): Drawn =>
    placement === 'widget' || held.calls.length === 0
        ? { _tag: 'Text', dimColor: held.calls.length === 0, children: held.calls.length === 0 ? 'no tool call ran in this session yet' : summary(held) }
        : { _tag: 'Box', flexDirection: 'column', children: [{ _tag: 'Text', dimColor: true, children: summary(held) }, ...held.counts.flatMap(([stance, calls]): readonly Drawn[] => [{ _tag: 'Text', weight: 'semibold', children: stance }, ...calls.map(row)])] };
const form = (held: Held): Drawn => ({
    _tag: 'Box',
    flexDirection: 'column',
    children: [
        { _tag: 'Text', children: `forget ${held.forgets} calls of ${Option.getOrElse(held.tool, () => 'every tool')}` },
        { _tag: 'Select', key: 'stance', label: 'Stance', value: held.forgets, options: STANCES.map((stance) => ({ value: stance })), onSelect: (value) => change((kept) => new Held({ ...kept, forgets: STANCES.find((stance) => stance === value) ?? kept.forgets })) },
        { _tag: 'Input', key: 'tool', label: 'Tool', placeholder: 'every tool', onSubmit: (value) => change((kept) => new Held({ ...kept, tool: Option.liftPredicate(value.trim(), Predicate.isTruthy) })) },
        { _tag: 'Progress', label: 'Forgets', value: held.doomed.length, max: Math.max(held.calls.length, 1), valueText: `${held.doomed.length} of ${held.calls.length}` },
        {
            _tag: 'Box',
            flexDirection: 'row',
            gap: 2,
            children: [
                {
                    _tag: 'Button',
                    key: 'forget',
                    label: 'Forget',
                    variant: 'primary',
                    onPress: () =>
                        Effect.andThen(
                            change((kept) => new Held({ ...kept, calls: kept.calls.filter((call) => !kept.doomed.includes(call)) })),
                            close(PANES.form),
                        ),
                },
                { _tag: 'Button', key: 'cancel', label: 'Cancel', role: 'dismiss', onPress: () => close(PANES.form) },
            ],
        },
    ],
});
const opened = (pane: PaneOpenArgs) => Panes.use((panes) => Effect.map(panes.open(pane), (answer) => (answer.isPlaced ? {} : { text: `${pane.id}: ${answer.reason}`, isTextDisplayOnly: true as const })));

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    host(on, {
        layer: Calls.layer,
        hooks: {
            'tool.call': (e, next) => {
                const call = new Call({ key: e.tool_use_id, tool: e.tool, agent: Option.fromUndefinedOr(e.agentId), stance: 'running', took: Option.none() });
                return change((held) => new Held({ ...held, calls: [...held.calls, call] })).pipe(
                    Effect.andThen(Effect.timed(next(e))),
                    Effect.tap(([took, result]) => change((held) => new Held({ ...held, calls: held.calls.map((kept) => (kept.key === call.key ? new Call({ ...kept, stance: result.deny === undefined ? 'ended' : 'refused', took: Option.some(took) }) : kept)) }))),
                    Effect.map(([, result]) => result),
                );
            },
        },
        commands: {
            calls: { description: 'Show the tool calls of this session in a pane', immediate: true, run: () => opened({ id: PANES.list, focus: true }) },
            forget: { description: 'Forget the tool calls of one stance', immediate: true, run: () => opened({ id: PANES.form, focus: true, closeOnEscape: true, holdToasts: true }) },
        },
        render: {
            state: Stream.unwrap(Calls.use(({ held }) => Effect.succeed(held.pipe(SubscriptionRef.changes)))),
            Pane: {
                [PANES.list]: { title: 'Calls', columns: 48, rows: 12, closeOnEscape: true, draw: (e, held) => Effect.succeed(list(held, e.props.placement)) },
                [PANES.form]: { title: 'Forget', rows: 8, draw: (_e, held) => held.pipe(form, Effect.succeed) },
            },
        },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
