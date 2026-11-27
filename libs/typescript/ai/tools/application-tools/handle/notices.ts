import { ApplicationServer } from '@rasm/applications/client';
import { Container, Event, InstanceSnapshot, InstanceState } from '@rasm/applications/contract';
import { byInstance, Workspace } from '@rasm/applications/workspace';
import { Format, Line, Report } from '@rasm/mcp/report';
import { Server } from '@rasm/mcp/server';
import { Array, Context, Data, DateTime, Effect, Equal, Function, HashMap, HashSet, Layer, Number, Option, Order, Record, Ref, Schema, Stream, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Address, Indexed } from '#/handle/address';
import { Outline } from '#/handle/outline';
import { Target } from '#/handle/target';

// --- [TYPES] ---------------------------------------------------------------------------

interface Seen {
    readonly started: Record.ReadonlyRecord<string, string>;
    readonly stalled: HashMap.HashMap<InstanceSnapshot['instance'], Event['at']>;
    readonly concealed: HashSet.HashSet<InstanceSnapshot['instance']>;
    readonly secure: HashMap.HashMap<InstanceSnapshot['instance'], readonly Pick<typeof Event.fields.change.cases.asking.Type, 'container' | 'title'>[]>;
    readonly followed: HashSet.HashSet<InstanceSnapshot['instance']>;
}
type Raised = Data.TaggedEnum<{
    Changed: { readonly application: string; readonly event: Event; readonly seen: Seen; readonly next: Seen; readonly quit: boolean };
    Row: { readonly instance: InstanceSnapshot['instance']; readonly text: Option.Option<string> };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PRESENTED = Schema.is(Container.fields.kind.pick(['window', 'dialog', 'panel', 'sheet', 'popover', 'drawer', 'prompt']));
const UNSEEN: Seen = { started: Record.empty(), stalled: HashMap.empty(), concealed: HashSet.empty(), secure: HashMap.empty(), followed: HashSet.empty() };

// --- [MODELS] --------------------------------------------------------------------------

const Raised = Data.taggedEnum<Raised>();

// --- [SERVICES] ------------------------------------------------------------------------

const Proving = Context.Reference<HashMap.HashMap<{ readonly instance: InstanceSnapshot['instance']; readonly followed: number }, { readonly call: number; readonly step: number; readonly head: readonly string[] }>>('@rasm/application-tools/handle/notices/Proving', { defaultValue: HashMap.empty });
const Logged = Context.Reference<HashSet.HashSet<Pick<(typeof Event.fields.change.cases.logged.Type)['entry'], 'subsystem' | 'category' | 'eventMessage'>>>('@rasm/application-tools/handle/notices/Logged', { defaultValue: HashSet.empty });

// --- [OPERATIONS] ----------------------------------------------------------------------

const advance = (seen: Seen, { instance, at, change }: Event): Seen =>
    Event.fields.change.matchOrElse(
        change,
        {
            started: ({ name }) => ({ ...seen, started: UndefinedOr.match(name, { onUndefined: () => seen.started, onDefined: (announced) => Record.set(seen.started, instance, announced) }) }),
            asking: (asked) => (asked.secure === undefined ? seen : { ...seen, secure: HashMap.set(seen.secure, instance, [...Option.getOrElse(HashMap.get(seen.secure, instance), Array.empty).filter((each) => each.container !== asked.container), Struct.pick(asked, ['container', 'title'])]) }),
            closed: ({ container }) => ({ ...seen, secure: HashMap.modify(seen.secure, instance, (held) => held.filter((each) => each.container !== container)) }),
            hidden: ({ hidden }) => ({ ...seen, concealed: hidden ? HashSet.add(seen.concealed, instance) : HashSet.remove(seen.concealed, instance) }),
            status: ({ status }) => ({ ...seen, stalled: InstanceState.Status.isAnyOf(['unresponsive'])(status) ? HashMap.set(seen.stalled, instance, Option.getOrElse(HashMap.get(seen.stalled, instance), Function.constant(at))) : HashMap.remove(seen.stalled, instance) }),
            exited: () => ({ ...seen, started: Record.remove(seen.started, instance), stalled: HashMap.remove(seen.stalled, instance), concealed: HashSet.remove(seen.concealed, instance), secure: HashMap.remove(seen.secure, instance) }),
        },
        () => seen,
    );
const raise = (seen: Seen, { change, rows, aims }: { readonly change: Option.Option<Event>; readonly rows: readonly InstanceSnapshot[]; readonly aims: readonly (typeof Target)['Service'][] }): readonly [Seen, readonly Raised[]] => {
    const named = (instance: InstanceSnapshot['instance'], ...held: readonly Seen[]) => Option.getOrElse(Option.firstSomeOf([Option.map(Record.get(byInstance(rows), instance), (each) => each.name ?? each.identifier), ...held.map(({ started }) => Record.get(started, instance))]), () => 'a process outside the Dock');
    const row = (at: Seen) => (instance: InstanceSnapshot['instance']) =>
        Raised.Row({ instance, text: Option.map(Option.flatMap(HashMap.get(at.secure, instance), Array.last), ({ title }) => [`${named(instance, at)}: password needed`, ...Array.fromNullishOr(title).map((shown) => `in ${Format.quoted(shown)}`)].join(' ')) });
    return Option.match(change, {
        onNone: () => {
            const followed = HashSet.fromIterable(aims.flatMap((aim) => Array.fromIterable(aim.followed)));
            const due = HashSet.union(HashSet.difference(followed, seen.followed), seen.secure.pipe(HashMap.filter(Array.isReadonlyArrayNonEmpty), HashMap.keys, HashSet.fromIterable));
            return [{ ...seen, followed: HashSet.union(seen.followed, followed) }, Array.fromIterable(due).map(row(seen))];
        },
        onSome: (event) => {
            const next = advance(seen, event);
            const rowed = Event.fields.change.isAnyOf(['exited'])(event.change) || !Equal.equals(HashMap.get(seen.secure, event.instance), HashMap.get(next.secure, event.instance));
            const changed = Raised.Changed({ application: named(event.instance, next, seen), event, seen, next, quit: aims.some((aim) => aim.quitting(event.instance)) });
            return [next, [...(rowed ? [row(next)(event.instance)] : []), changed]];
        },
    });
};
const posted = ({ call, step, head }: HashMap.HashMap.Value<(typeof Proving)['Service']>) => ['call', `${call}`, 'step', `${step}`, ...head];
const notice = ({ application, event: { instance, sequence, at, change }, seen, next, quit }: Extract<Raised, { readonly _tag: 'Changed' }>, state: Context.Context<never>) => {
    const aim = Context.get(state, Target);
    const inside = Option.exists(aim.held, (held) => held.instance === instance) ? [] : [application];
    const indexed = Option.filter(Context.get(state, Indexed), (held) => held.instance === instance);
    const stalling = HashMap.has(next.stalled, instance);
    const said = (push: boolean, head: readonly string[], rest: Pick<Line, 'value' | 'facts'> = {}, left = state) => Option.some({ text: Report.text(new Report({ lines: [new Line({ head, ...rest })] })), push, state: left });
    const presented = ({ _tag, kind, container, title }: Extract<Event['change'], { readonly _tag: 'opened' | 'closed' }>, left = Option.none<Context.Context<never>>()) => {
        const read = Option.exists(indexed, ({ index }) => Option.exists(Record.get(index.reads, `${container}`), (held) => sequence <= held.sequence));
        return PRESENTED(kind) && !read
            ? said(
                  false,
                  [...inside, _tag, ...Outline.caption(kind, container, title)],
                  {},
                  Option.getOrElse(left, () => state),
              )
            : Option.map(left, (held) => ({ push: false, state: held }));
    };
    return Event.fields.change.matchOrElse(
        change,
        {
            opened: (opening) => presented(opening),
            closed: (closing) =>
                presented(
                    closing,
                    Option.map(indexed, (held) => Context.add(state, Indexed, Option.some({ ...held, index: Address.close(held.index, sequence, closing.container, [`${closing._tag} ${at}`]) }))),
                ),
            asking: ({ _tag, kind, container, title, text, answers, secure, presentation, ...roles }) => {
                const labels = Record.fromEntries(answers.flatMap(({ ref, label }) => Array.fromNullishOr(label).map((shown) => [`${ref}`, shown] as const)));
                const buttons = Record.collect(Record.getSomes({ default: Option.fromUndefinedOr(roles.default), cancel: Option.fromUndefinedOr(roles.cancel) }), (role, ref) => [role, Address.token('action', ref), ...Option.toArray(Record.get(labels, `${ref}`)).map(Format.quoted)].join(' '));
                const field = Array.fromNullishOr(secure).flatMap(({ ref, label }) => [['secure', Address.token('text', ref), ...Array.fromNullishOr(label).map(Format.quoted)].join(' '), 'person fills it']);
                const facts = [...buttons, ...field, ...(presentation?._tag === 'forward' ? [Outline.say.front] : [])];
                return said(true, [...inside, _tag, ...Outline.caption(kind, container, title)], Array.match(text, { onEmpty: () => ({ facts }), onNonEmpty: (lines) => ({ value: Format.quoted(lines.join(' ')), facts }) }));
            },
            hidden: ({ hidden }) => (HashSet.has(seen.concealed, instance) === HashSet.has(next.concealed, instance) ? Option.none() : said(false, [application, hidden ? 'hidden' : 'unhidden'])),
            status: ({ status }) => {
                const lasted = Option.map(Option.all([Option.flatMap(HashMap.get(seen.stalled, instance), DateTime.make), DateTime.make(at)]), ([began, ended]) => `stalled ${Format.duration(DateTime.distance(began, ended))}`);
                const facts = [...InstanceState.Status.matchOrElse(status, { unresponsive: ({ thread }) => [`main thread ${thread}`] }, Array.empty), ...Option.toArray(lasted)];
                return HashMap.has(seen.stalled, instance) === stalling ? Option.none() : Option.map(said(true, [application, status._tag], { facts }), (line) => ({ ...line, key: instance }));
            },
            announced: ({ _tag, text, priority }) => said(false, [...inside, _tag], { value: Format.quoted(text), facts: priority === 'high' ? [`priority ${priority}`] : [] }),
            proven: ({ followed, drawn }) => {
                const [open, key] = [Context.get(state, Proving), { instance, followed }];
                return Option.flatMap(HashMap.get(open, key), (step) => said(true, [...inside, ...posted(step)], { value: 'changed', facts: drawn ? ['drawn'] : [] }, Context.add(state, Proving, HashMap.remove(open, key))));
            },
            logged: ({ _tag, entry: { messageType, subsystem, category, eventMessage } }) => {
                const [told, key] = [Context.get(state, Logged), { subsystem, category, eventMessage }];
                return HashSet.has(told, key) ? Option.none() : said(false, [...inside, _tag, messageType.toLowerCase(), subsystem, category].filter(String.isNonEmpty), { value: Format.quoted(eventMessage) }, Context.add(state, Logged, HashSet.add(told, key)));
            },
            reported: ({ _tag, report }) => (stalling ? said(true, [application, _tag, report.kind], { value: Format.path(report.url), facts: Array.fromNullishOr(report.event).map((cause) => `event ${cause}`) }) : Option.none()),
            sampled: ({ _tag, report }) => (stalling ? said(true, [application, _tag], { value: Format.path(report) }) : Option.none()),
            exited: ({ _tag, termination }) => {
                const ended = InstanceState.Exited.match(termination, { status: ({ status }) => (status === 0 ? [] : [`status ${status}`]), signal: ({ signal }) => [`signal ${signal}`], statusFailure: ({ statusFailure }) => ['status unread', statusFailure] });
                const facts = [...ended, ...Array.fromNullishOr(termination.report).map((report) => `${report.kind} report ${Format.path(report.url)}`), ...(termination.reporting === true ? ['reporting'] : []), ...(quit ? ['quit by another agent'] : [])];
                const left = Context.add(state, Target, aim.without([instance]));
                return aim.quitting(instance) ? Option.some({ push: false, state: left }) : said(true, [application, _tag], { facts }, left);
            },
        },
        () => Option.none(),
    );
};
const route = (raised: Raised, state: Context.Context<never>) => {
    const { followed } = Context.get(state, Target);
    return Raised.$match(raised, {
        Row: ({ instance, text }) => Option.some(HashSet.has(followed, instance) && Option.isSome(text) ? { push: false, row: { key: instance, text: text.value } } : { push: false, clears: instance }),
        Changed: (changed) => (HashSet.has(followed, changed.event.instance) ? notice(changed, state) : Option.none()),
    });
};
const admit = (call: number, state: Context.Context<never>) => {
    const open = Context.get(state, Proving);
    const lapsed = HashMap.filter(open, (step) => step.call < call);
    const lines = Array.sort(HashMap.toValues(lapsed), Order.Struct({ call: Number.Order, step: Number.Order })).map((step) => new Line({ head: posted(step), value: 'no change seen' }));
    return Option.liftPredicate({ text: Report.text(new Report({ lines })), push: false, state: Context.add(state, Proving, HashMap.removeMany(open, HashMap.keys(lapsed))) }, () => Array.isReadonlyArrayNonEmpty(lines));
};
const press = (key: string) => ApplicationServer.use(({ activate }) => Effect.flatMap(Schema.decodeEffect(InstanceSnapshot.fields.instance)(key), (instance) => Effect.asVoid(activate({ instance }))));

// --- [COMPOSITION] ---------------------------------------------------------------------

const Notices = {
    layer: Layer.effectDiscard(
        Effect.gen(function* () {
            const [server, { changes, running }, served, aims] = yield* Effect.all([ApplicationServer, Workspace, Server, Ref.make<readonly (typeof Target)['Service'][]>([])]);
            const left = served.states(Target).pipe(
                Stream.changes,
                Stream.tap((held) => Ref.set(aims, held)),
                Stream.as(Option.none<Event>()),
            );
            const stream = Stream.merge(Stream.map(changes, Option.some), left).pipe(
                Stream.mapEffect((change) => Effect.zipWith(SubscriptionRef.get(running), Ref.get(aims), (rows, held) => ({ change, rows, aims: held }))),
                Stream.mapAccum(Function.constant(UNSEEN), raise),
            );
            yield* served.events({ stream, route, admit, press: (key) => Effect.provideService(press(key), ApplicationServer, server) });
        }),
    ),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Notices, Proving, press, route };
