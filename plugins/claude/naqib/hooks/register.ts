import type { Element } from '@rasm/claude-code/element';
import { host } from '@rasm/claude-code/host';
import { Panes } from '@rasm/claude-code/pane';
import { Watch } from '@rasm/claude-code/watch';
import type { On } from 'claude-code';
import { Array, Effect, Option, Stream, SubscriptionRef } from 'effect';
import { type Entry, Gathering, STANCES } from '#/hooks/gathering';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PANE = 'gather';
const BAND = 3;
const CUT = 48;

// --- [OPERATIONS] ----------------------------------------------------------------------

const cut = (text: string) => (text.length > CUT ? `${text.slice(0, CUT - 1)}…` : text);
const opens = (entry: Entry) => Gathering.use((gathering) => Option.match(entry.opening, { onNone: () => Effect.void, onSome: gathering.open }));
const gather = Effect.all([Panes.use((held) => held.open({ id: PANE, focus: true })), Gathering.use((gathering) => Effect.andThen(gathering.refresh('all'), gathering.follow))]);
const waited = Gathering.use((gathering) => gathering.refresh('waits'));
const held = (entries: readonly Entry[]) => STANCES.map((stance) => [stance, entries.filter((entry) => entry.stance === stance)] as const).filter(([, kept]) => Array.isReadonlyArrayNonEmpty(kept));
const row = (entry: Entry): Element<Gathering> => ({
    _tag: 'Box',
    flexDirection: 'row',
    gap: 2,
    children: [
        Option.match(entry.opening, {
            onNone: (): Element<Gathering> => ({ _tag: 'Text', children: entry.label }),
            onSome: (): Element<Gathering> => ({ _tag: 'Button', key: entry.key, plain: true, label: entry.label, onPress: () => opens(entry) }),
        }),
        { _tag: 'Text', dimColor: true, wrap: 'truncate-end', children: entry.detail },
    ],
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    host(on, {
        layer: Gathering.layer,
        hooks: {
            'turn.complete': (e, next) => (e.agentId === undefined ? Effect.tap(next(e), () => waited) : next(e)),
            'prompt.submit': (e, next) => Effect.andThen(waited, next(e)),
            'classic.Notification': (e, next) => Effect.andThen(waited, next(e)),
            'tool.call': (e, next) => (e.tool === 'Bash' ? Watch.use((watch) => watch.during(next(e))) : next(e)),
        },
        commands: {
            gather: {
                description: 'Gather the waiting panes, sessions, watch runs, and threads of this workspace in one pane',
                immediate: true,
                run: () => Effect.as(gather, {}),
            },
        },
        render: {
            state: Stream.unwrap(Gathering.use((gathering) => Effect.succeed(gathering.entries.pipe(SubscriptionRef.changes)))),
            AbovePrompt: (e, entries, next) => {
                const waits = entries.filter((entry) => entry.stance === 'waits');
                return e.props.hasSurvey || !Array.isReadonlyArrayNonEmpty(waits)
                    ? next()
                    : Effect.succeed({
                          _tag: 'Box',
                          flexDirection: 'row',
                          gap: 1,
                          children: [
                              { _tag: 'Text', dimColor: true, children: 'waits' },
                              ...waits.slice(0, BAND).map((entry): Element<Gathering | Panes> => ({ _tag: 'Button', key: entry.key, label: cut(entry.label), onPress: () => opens(entry) })),
                              ...(waits.length > BAND ? [{ _tag: 'Button', key: PANE, label: `+${waits.length - BAND}`, onPress: () => gather } as const] : []),
                          ],
                      });
            },
            Welcome: (_e, entries, next) => {
                const told = held(entries).map(([stance, kept]) => `${stance} ${kept.length}`);
                return Array.isReadonlyArrayNonEmpty(told) ? Effect.map(next(), (logo) => ({ _tag: 'Box', flexDirection: 'column', children: [logo, { _tag: 'Text', dimColor: true, children: ['elsewhere', ...told, '/gather'].join(' · ') }] })) : next();
            },
            Pane: {
                [PANE]: {
                    title: 'Gather',
                    columns: 64,
                    rows: 18,
                    closeOnEscape: true,
                    draw: (_e, entries) =>
                        Effect.succeed({
                            _tag: 'Box',
                            flexDirection: 'column',
                            children: Array.match(held(entries), {
                                onEmpty: () => [{ _tag: 'Text', dimColor: true, children: 'nothing waits, runs, or ended here in the last day' } as const],
                                onNonEmpty: (stances) => stances.flatMap(([stance, kept]) => [{ _tag: 'Text', weight: 'semibold', children: `${stance} ${kept.length}` } as const, ...kept.map(row)]),
                            }),
                        }),
                    closes: Gathering.use((gathering) => Effect.as(gathering.unfollow, true)),
                },
            },
        },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
