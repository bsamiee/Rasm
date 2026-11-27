import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { Event } from '@rasm/applications/server/event';
import { Instance } from '@rasm/applications/server/instance';
import { Call, Format, Line, Report } from '@rasm/mcp/report';
import { Server } from '@rasm/mcp/server';
import { Array, Context, DateTime, Effect, HashMap, HashSet, Layer, Number, Option, Order, Record, Ref, Result, Schema, Stream, String, Struct, UndefinedOr } from 'effect';
import { Address } from '#/handle/address';
import { Outline } from '#/handle/outline';
import { Target } from '#/handle/target';

// --- [TYPES] ---------------------------------------------------------------------------

type Standing = HashMap.HashMap<
    Instance['instance'],
    {
        readonly name: Option.Option<string>;
        readonly followed: boolean;
        readonly unresponsive: Option.Option<(typeof Event.Changed.Type)['at']>;
        readonly secure: readonly (typeof Event.Changed.fields.change.cases.asking.Type)[];
    }
>;
interface Occurrence {
    readonly instance: Instance['instance'];
    readonly changed: Option.Option<typeof Event.Changed.Type>;
    readonly before: Standing;
    readonly after: Standing;
    readonly targets: readonly (typeof Target)['Service'][];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WINDOWED = Schema.is(Element.cases.container.fields.kind.pick(['window', 'dialog', 'panel', 'sheet', 'popover', 'drawer', 'prompt']));

// --- [SERVICES] ------------------------------------------------------------------------

const Proving = Context.Reference<HashMap.HashMap<{ readonly instance: Instance['instance']; readonly followed: number }, { readonly call: number; readonly step: number; readonly head: readonly string[] }>>('@rasm/application-tools/handle/notice/Proving', { defaultValue: HashMap.empty });

// --- [OPERATIONS] ----------------------------------------------------------------------

const cite = ({ call, step, head }: HashMap.HashMap.Value<(typeof Proving)['Service']>) => ['call', `${call}`, 'step', `${step}`, ...head];
const notify = <Rest extends object>(push: boolean, lines: readonly Line[], rest?: Rest) => Option.some({ push, ...rest, ...(Array.isReadonlyArrayNonEmpty(lines) ? { text: Report.text(new Report({ lines })) } : {}) });
const observe = (standing: Standing, { changed, targets }: Pick<Occurrence, 'changed' | 'targets'>): readonly [Standing, readonly Occurrence[]] => {
    const write = (held: Standing, instance: Instance['instance'], change: (each: HashMap.HashMap.Value<Standing>) => HashMap.HashMap.Value<Standing>) =>
        HashMap.modifyAt(held, instance, (found) => Option.some(change(Option.getOrElse(found, () => ({ name: Option.none(), followed: false, unresponsive: Option.none(), secure: [] })))));
    return Option.match(changed, {
        onNone: () => {
            const fresh = targets.flatMap(({ followed }) => Array.fromIterable(followed)).filter((instance) => !Option.exists(HashMap.get(standing, instance), ({ followed }) => followed));
            const prompted = HashMap.keys(HashMap.filter(standing, ({ secure }) => Array.isReadonlyArrayNonEmpty(secure)));
            const after = fresh.reduce((held, instance) => write(held, instance, (each) => ({ ...each, followed: true })), standing);
            return [after, Array.dedupe([...fresh, ...prompted]).map((instance) => ({ instance, changed, before: standing, after, targets }))] as const;
        },
        onSome: ({ instance, at, change }) => {
            const after = Event.Changed.fields.change.matchOrElse(
                change,
                {
                    started: ({ name, identifier }) => write(standing, instance, (each) => ({ ...each, name: Option.orElse(Option.fromUndefinedOr(name ?? identifier), () => each.name) })),
                    asking: (asked) => (asked.secure === undefined ? standing : write(standing, instance, (each) => ({ ...each, secure: [...each.secure.filter(({ container }) => container !== asked.container), asked] }))),
                    closed: ({ container }) => HashMap.modify(standing, instance, (each) => ({ ...each, secure: each.secure.filter((asked) => asked.container !== container) })),
                    status: ({ status }) => write(standing, instance, (each) => ({ ...each, unresponsive: Instance.State.Status.isAnyOf(['unresponsive'])(status) ? Option.orElse(each.unresponsive, () => Option.some(at)) : Option.none() })),
                    exited: ({ termination }) => (termination.reportPending === true ? HashMap.modify(standing, instance, (each) => ({ ...each, unresponsive: Option.none(), secure: [] })) : HashMap.remove(standing, instance)),
                },
                () => standing,
            );
            return [after, [{ instance, changed, before: standing, after, targets }]] as const;
        },
    });
};
const route = ({ instance, changed, before, after, targets }: Occurrence, state: Context.Context<never>) => {
    const target = Context.get(state, Target);
    const followed = HashSet.has(target.followed, instance);
    const named = (each: Instance['instance']) => Option.flatMap(HashMap.get(before, each), Struct.get('name'));
    const application = Option.getOrElse(named(instance), () => instance);
    const stalled = Option.flatMap(HashMap.get(before, instance), Struct.get('unresponsive'));
    const held = Option.filter(Context.get(state, Address.Held), (read) => read.instance === instance);
    const stated = (container: number, sequence: number) => Option.exists(held, ({ reads }) => Option.exists(HashMap.get(reads, container), (read) => sequence <= read.sequence));
    const head = ({ _tag, kind, container, title }: Extract<(typeof Event.Changed.Type)['change'], { readonly container: number }>) => [application, _tag, ...Outline.caption(kind, container, title)];
    const ask = (asked: HashMap.HashMap.Value<Standing>['secure'][number]) => {
        const roles = Record.fromEntries(Record.collect(Record.getSomes({ default: Option.fromUndefinedOr(asked.default), cancel: Option.fromUndefinedOr(asked.cancel) }), (role, ref) => [`${ref}`, role] as const));
        const answers = asked.answers.map(({ ref, label }) => [...Array.fromNullishOr(label).map(Format.quoted), Address.refer('action', ref), ...Option.toArray(Record.get(roles, `${ref}`)).map((role) => `(${role})`)].join(' '));
        const field = Array.fromNullishOr(asked.secure).flatMap(({ ref, label }) => [['secure', Address.refer('text', ref), ...Array.fromNullishOr(label).map(Format.quoted)].join(' '), 'person fills it']);
        const answer = Option.map(Option.fromUndefinedOr(asked.default), (ref) => new Call({ tool: 'act', given: { steps: [{ at: Address.refer('action', ref) }] } }));
        const read = new Call({ tool: 'inspect', given: { in: Address.refer(asked.kind, asked.container) } });
        return new Line({
            head: head(asked),
            ...Array.match(asked.text, { onEmpty: () => ({}), onNonEmpty: (lines) => ({ value: Format.quoted(lines.join(' ')) }) }),
            facts: [...answers, ...field, ...Array.fromNullishOr(asked.presentation).flatMap(Instance.Presentation.matchOrElse({ forward: ({ _tag }) => [_tag] }, Array.empty<string>))],
            ...(asked.secure === undefined ? { call: Option.getOrElse(answer, () => read) } : {}),
        });
    };
    const prompt = Option.flatMap(HashMap.get(after, instance), ({ secure }) => Array.last(secure));
    const row = Option.match(followed ? prompt : Option.none(), { onNone: () => ({ clears: instance }), onSome: (asked) => ({ row: { key: instance, text: Report.text(new Report({ lines: [ask(asked)] })) } }) });
    const told = Option.filter(changed, () => followed);
    return Option.match(told, {
        onNone: () => (Option.isNone(changed) ? notify(false, [], row) : Option.none()),
        onSome: ({ sequence, at, change }) =>
            Event.Changed.fields.change.matchOrElse(
                change,
                {
                    opened: (opening) => (WINDOWED(opening.kind) && !stated(opening.container, sequence) ? notify(false, [new Line({ head: head(opening) })]) : Option.none()),
                    closed: (closing) => {
                        const drives = ({ holds }: (typeof Target)['Service']) => holds.some((hold) => Option.exists(named(hold.instance), (name) => name === closing.front));
                        const fronted = Array.fromNullishOr(closing.front).flatMap((front) => [`front ${Format.quoted(front)}`, ...(Array.countBy(targets, drives) > (drives(target) ? 1 : 0) ? ["another agent's call took the front"] : [])]);
                        const left = Option.match(held, { onNone: () => state, onSome: (read) => Context.add(state, Address.Held, Option.some(Address.close(read, sequence, closing.container, [`${closing._tag} ${at}`, ...fronted]))) });
                        const shown = (WINDOWED(closing.kind) || closing.front !== undefined) && !stated(closing.container, sequence);
                        return notify(false, shown ? [new Line({ head: head(closing), facts: fronted })] : [], { ...row, state: left });
                    },
                    asking: (asked) => notify(true, [ask(asked)], row),
                    consenting: ({ _tag, granted }) =>
                        UndefinedOr.match(granted, {
                            onUndefined: () => notify(true, [new Line({ head: [application, _tag], facts: ['person answers it'] })], { row: { key: instance, text: `${application}: Automation consent waits` } }),
                            onDefined: (answered) => notify(true, [new Line({ head: [application, _tag], facts: [answered ? 'granted' : 'denied'] })], { clears: instance }),
                        }),
                    hidden: ({ _tag, hidden }) => (targets.some(({ holds }) => holds.some((hold) => hold._tag === 'Launching' && hold.instance === instance)) ? Option.none() : notify(false, [new Line({ head: [application, _tag], value: `${hidden}` })])),
                    status: ({ status }) => {
                        const lasted = Option.zipWith(Option.flatMap(stalled, DateTime.make), DateTime.make(at), (began, ended) => `stalled ${Format.duration(DateTime.distance(began, ended))}`);
                        const facts = [...Instance.State.Status.matchOrElse(status, { unresponsive: ({ thread }) => [`main thread ${thread}`] }, Array.empty<string>), ...Option.toArray(lasted)];
                        return Option.isSome(stalled) === Instance.State.Status.isAnyOf(['unresponsive'])(status) ? Option.none() : notify(true, [new Line({ head: [application, status._tag], facts })], { key: instance });
                    },
                    announced: ({ _tag, text, priority }) => notify(false, [new Line({ head: [application, _tag], value: Format.quoted(text), facts: priority === 'high' ? [`priority ${priority}`] : [] })]),
                    proven: ({ _tag, followed: posted, drawn }) => {
                        const [owed, key] = [Context.get(state, Proving), { instance, followed: posted }];
                        return Option.flatMap(HashMap.get(owed, key), (step) => notify(true, [new Line({ head: [application, _tag, ...cite(step)], facts: drawn ? ['drawn'] : [] })], { state: Context.add(state, Proving, HashMap.remove(owed, key)) }));
                    },
                    reported: ({ _tag, report }) => Option.flatMap(stalled, () => notify(true, [new Line({ head: [application, _tag, report.kind], value: Format.path(report.url), facts: Array.fromNullishOr(report.event).map((cause) => `event ${cause}`) })])),
                    sampled: ({ _tag, report }) => Option.flatMap(stalled, () => notify(true, [new Line({ head: [application, _tag], value: Format.path(report) })])),
                    exited: ({ _tag, termination }) => {
                        const ended = Instance.State.Termination.match(termination, { status: ({ status }) => (status === 0 ? [] : [`status ${status}`]), signal: ({ signal }) => [`signal ${signal}`], unread: ({ failure }) => [failure.message] });
                        const pending = termination.reportPending === true;
                        const report = Array.fromNullishOr(termination.report).map((read) => Result.match(read, { onSuccess: ({ kind, url }) => `${kind} report ${Format.path(url)}`, onFailure: ({ message }) => message }));
                        const facts = [...ended, ...(termination.detail ?? []).map(String.noCase()), ...(pending ? ['report pending'] : []), ...report, ...(targets.some((each) => each.quitting(instance)) ? ['quit by another agent'] : [])];
                        const left = { ...row, key: instance, state: pending ? state : Context.add(state, Target, target.without([instance])) };
                        return target.quitting(instance) ? notify(false, [], left) : notify(true, [new Line({ head: [application, _tag], facts })], left);
                    },
                },
                () => Option.none(),
            ),
    });
};
const admit = (call: number, state: Context.Context<never>) => {
    const owed = Context.get(state, Proving);
    const lapsed = HashMap.filter(owed, (step) => step.call < call);
    const lines = Array.sort(HashMap.toValues(lapsed), Order.Struct({ call: Number.Order, step: Number.Order })).map((step) => new Line({ head: cite(step), value: 'no change seen' }));
    return Array.isReadonlyArrayEmpty(lines) ? Option.none() : notify(false, lines, { state: Context.add(state, Proving, HashMap.removeMany(owed, HashMap.keys(lapsed))) });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const Notice = {
    Proving,
    layer: Layer.effectDiscard(
        Effect.gen(function* () {
            const [server, served, targets] = yield* Effect.all([ApplicationServer, Server, Ref.make<Occurrence['targets']>([])]);
            const retargeted = served.states(Target).pipe(
                Stream.changes,
                Stream.tap((held) => Ref.set(targets, held)),
                Stream.as(Option.none<typeof Event.Changed.Type>()),
            );
            const stream = Stream.merge(Stream.map(server.changes, Option.some), retargeted).pipe(
                Stream.mapEffect((changed) => Effect.map(Ref.get(targets), (held) => ({ changed, targets: held }))),
                Stream.mapAccum((): Standing => HashMap.empty(), observe),
            );
            yield* served.events({ stream, route, admit, press: (key) => Effect.flatMap(Schema.decodeEffect(Instance.fields.instance)(key), (instance) => Effect.asVoid(server.activate({ instance }))) });
        }),
    ),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Notice };
