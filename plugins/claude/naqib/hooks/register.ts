import type { Element } from '@rasm/claude-code/element';
import { host } from '@rasm/claude-code/host';
import { Panes } from '@rasm/claude-code/pane';
import { Watch } from '@rasm/claude-code/watch';
import type { On } from 'claude-code';
import { Array, Effect, Option, Record, Stream } from 'effect';
import { type Entry, type Gathered, Gathering } from '#/hooks/gathering';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PANE = 'gather';
const BAND = 3;

// --- [OPERATIONS] ----------------------------------------------------------------------

const gather = Effect.all([Panes.use((panes) => panes.open({ id: PANE, focus: true })), Gathering.use((gathering) => gathering.follow)]);
const held = (gathered: Gathered) => Record.toEntries(Record.filter(gathered, Array.isReadonlyArrayNonEmpty));
const control = (entry: Entry, face: { readonly plain?: true; readonly children: Element<Gathering> }): Element<Gathering> =>
    Option.match(entry.opening, {
        onNone: () => face.children,
        onSome: (opening) => ({ _tag: 'Button', key: entry.key, ...face, onPress: () => Gathering.use((gathering) => gathering.open(opening)) }),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    host(on, {
        layer: Gathering.layer,
        hooks: {
            'tool.call': (e, next) => (e.tool === 'Bash' ? Watch.use((watch) => watch.run(next(e))) : next(e)),
        },
        commands: {
            gather: {
                description: 'Gather the waiting panes, sessions, watch runs, and threads of this workspace in one pane',
                immediate: true,
                run: () => Effect.as(gather, {}),
            },
        },
        render: {
            state: Stream.unwrap(Gathering.use((gathering) => Effect.succeed(gathering.gathered))),
            AbovePrompt: (e, { waits }, next) =>
                e.props.hasSurvey || !Array.isReadonlyArrayNonEmpty(waits)
                    ? next()
                    : Effect.succeed({
                          _tag: 'Box',
                          flexDirection: 'row',
                          gap: 1,
                          children: [{ _tag: 'Text', dimColor: true, children: 'waits' }, ...Array.take(waits, BAND).map((entry) => control(entry, { children: { _tag: 'Text', wrap: 'truncate-end', children: entry.label } })), ...(waits.length > BAND ? [{ _tag: 'Button', key: PANE, label: `+${waits.length - BAND}`, onPress: () => gather } as const] : [])],
                      }),
            Welcome: (_e, gathered, next) => {
                const told = held(gathered).map(([stance, kept]) => `${stance} ${kept.length}`);
                return Array.isReadonlyArrayNonEmpty(told) ? Effect.map(next(), (logo) => ({ _tag: 'Box', flexDirection: 'column', children: [logo, { _tag: 'Text', dimColor: true, children: ['elsewhere', ...told, '/gather'].join(' · ') }] })) : next();
            },
            Pane: {
                [PANE]: {
                    title: 'Gather',
                    columns: 64,
                    rows: 18,
                    closeOnEscape: true,
                    draw: (_e, gathered) =>
                        Effect.succeed({
                            _tag: 'Box',
                            flexDirection: 'column',
                            children: Array.match(held(gathered), {
                                onEmpty: () => [{ _tag: 'Text', dimColor: true, children: 'nothing waits, runs, or ended here in the last day' } as const],
                                onNonEmpty: (stances) =>
                                    stances.flatMap(([stance, kept]) => [
                                        { _tag: 'Text', weight: 'semibold', children: `${stance} ${kept.length}` } as const,
                                        ...kept.map((entry) => control(entry, { plain: true, children: { _tag: 'Text', wrap: 'truncate-end', children: [entry.label, '  ', { _tag: 'Text', dimColor: true, children: entry.detail }] } })),
                                    ]),
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
