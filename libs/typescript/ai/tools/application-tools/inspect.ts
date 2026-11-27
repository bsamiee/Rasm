import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { Event } from '@rasm/applications/server/event';
import { Call, Format, Line, Report } from '@rasm/mcp/report';
import { Handle, Server } from '@rasm/mcp/server';
import { Array, Context, Duration, Effect, Fiber, HashSet, Match, Number, Option, Order, pipe, Record, Result, Schema, Stream, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { Act } from '#/act';
import { Address, type Index, Indexed, type Item, read } from '#/handle/address';
import { Outline } from '#/handle/outline';
import { current } from '#/handle/target';

// --- [MODELS] --------------------------------------------------------------------------

const tool = Tool.make('inspect', {
    description: 'Reads the interface of the target as an outline of containers and controls, each led by the reference act and view take: every container, one container or control alone, the clipboard, or a search of what shows now and what the interface map remembers',
    parameters: Schema.Struct({
        in: Schema.optionalKey(
            Schema.Union([Schema.Literal('clipboard'), Address.At]).annotate({
                description: 'Container, control, or remembered path read alone: a menu item reads its menu, a multi its rows, a text control its style runs with fonts and colors, any other control its value, range, state, frame, and actions; clipboard reads the clipboard; every container when absent',
            }),
        ),
        find: Schema.optionalKey(Schema.String.annotate({ description: 'Text searched in labels, values, and help of every container shown and remembered, or of in alone' })),
        kind: Schema.optionalKey(Element.Control.fields.kind.annotate({ description: 'Element.Control.fields.kind of control find keeps, or every control of that kind without find' })),
        unopened: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Lists the items whose menu, popover, dialog, or options the interface map never read' })),
        after: Schema.optionalKey(Address.At.annotate({ description: 'Reference the page starts after, as a more line names it' })),
        wait: Schema.optionalKey(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 60, exclusiveMinimum: true })).annotate({ description: 'Most seconds to wait for a change of the target, returning at once when it changed since your last read' })),
    }),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
}).annotateMerge(
    Context.make(Tool.Readonly, true).pipe(
        Context.add(Tool.Strict, true),
        Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true }),
        Context.add(Server.Instructions, ['Containers read in an application join its interface map: search it with inspect find before opening a menu or dialog, and act on a remembered hit by the path find prints.']),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const runOf = (held: Index, ref: number) =>
    held.shown.flatMap(({ controls }) =>
        pipe(
            controls,
            Array.dropWhile(({ control }) => control.ref !== ref),
            Array.drop(1),
            Array.takeWhile(({ control }) => control.kind === 'row' || control.row !== undefined),
        ),
    );
const rowsOf = (held: Index, ref: number) => runOf(held, ref).filter(({ control }) => control.kind === 'row');
const partialOf = (held: Index) =>
    held.shown
        .flatMap(({ controls }) => controls)
        .filter(({ control }) => control.kind === 'multi')
        .flatMap(({ control, item }) => {
            const from = rowsOf(held, control.ref).length;
            const count = (control.total ?? 0) - from;
            return count > 0 ? [{ ref: control.ref, from, count, token: item.token }] : [];
        });
const membersOf = (held: Index, { target }: Item): readonly Item[] =>
    Match.value(target).pipe(
        Match.tag('Container', ({ container }) => held.shown.filter((shown) => shown.container.ref === container.ref).flatMap(({ controls }) => controls.map(Struct.get('item')))),
        Match.tag('Control', ({ control }) => runOf(held, control.ref).map(Struct.get('item'))),
        Match.tag('RememberedContainer', ({ locator }) => Option.getOrElse(Record.get(held.remembered.controls, locator), Array.empty<Item>)),
        Match.orElse(() => held.items),
    );
const scopeOf = (start: Option.Option<Item>, whole: boolean) => {
    const from = start.pipe(
        Option.flatMap(({ target }) => (target._tag === 'Control' ? Option.fromUndefinedOr(target.control.index) : Option.none())),
        Option.match({ onNone: () => 0, onSome: Number.increment }),
    );
    return ({ target }: Item): Option.Option<typeof Element.Scope.Type> =>
        Match.value(target).pipe(
            Match.when({ _tag: 'Control', control: { kind: 'multi' } }, ({ control }) => Option.some({ _tag: 'rows' as const, ref: control.ref, from, count: whole ? Math.max(1, (control.total ?? Outline.rows) - from) : Outline.rows })),
            Match.when({ _tag: 'Control', control: { kind: 'text' } }, ({ control }) => Option.some({ _tag: 'text' as const, ref: control.ref })),
            Match.when({ _tag: 'Control', control: { kind: (kind: typeof Element.Control.fields.kind.Type) => kind !== 'choice', opens: Match.number } }, ({ control }) => Option.some({ _tag: 'menu' as const, ref: control.ref })),
            Match.tag('Container', 'Control', ({ container }) => Option.some({ _tag: 'container' as const, ref: container.ref })),
            Match.orElse(() => Option.none()),
        );
};
const scrollOf = (multi: string, fact: string) => new Line({ head: [multi], facts: [fact], call: new Call({ tool: Act.tool.name, given: { steps: [{ pointer: multi, by: [0, -400], button: 'none' }] } }), pinned: true });
const answerOf = ({ container, item, controls }: Index['shown'][number]) =>
    Option.all({ asking: item.token, ref: Option.fromUndefinedOr(container.default ?? controls.find(({ control }) => control.kind === 'action')?.control.ref) }).pipe(
        Option.map(({ asking, ref }) => new Line({ head: [asking, Outline.say.asking], call: new Call({ tool: Act.tool.name, given: { steps: [{ at: Address.token('action', ref) }] } }), pinned: true })),
    );
const Inspect = {
    tool,
    handle: Effect.fnUntraced(function* ({ in: within, find: query, kind, after, wait, unopened = false }: Tool.Parameters<typeof tool>) {
        const [server, aimed] = yield* Effect.all([ApplicationServer, current]);
        const { changes } = server;
        const { row, live } = aimed;
        if (within === 'clipboard') {
            const pasteboard = yield* server.pasteboard({ content: true });
            const [subject, clipboard] = yield* Effect.all([Outline.subject(row.name, live), Outline.clipped(live.instance, pasteboard, true)]);
            return new Report({ lines: [...subject, ...clipboard] });
        }
        const searched = Option.liftPredicate(Record.getSomes({ find: Option.fromUndefinedOr(query), kind: Option.fromUndefinedOr(kind), unopened: Option.liftPredicate<Schema.Json>(unopened, Boolean) }), (given) => !Record.isEmptyRecord(given));
        const last = Handle.use((handle) => handle.get(Indexed));
        const since = (yield* last).pipe(
            Option.filter(({ instance }) => instance === live.instance),
            Option.map(({ index }) => index.model.containers.flatMap(({ ref }) => Option.toArray(Record.get(index.reads, `${ref}`)).map(Struct.get('sequence')))),
            Option.filter(Array.isArrayNonEmpty),
            Option.map(Array.max(Order.Number)),
        );
        const next = yield* Option.fromUndefinedOr(wait).pipe(
            Option.map((seconds) =>
                changes.pipe(
                    Stream.filter(({ instance }) => instance === live.instance),
                    Stream.runHead,
                    Effect.timeoutOption(Duration.seconds(seconds)),
                    Effect.map(Option.flatten),
                    Effect.forkScoped({ startImmediately: true }),
                ),
            ),
            Effect.transposeOption,
        );
        const [opening, [elapsed, first], closing] = yield* Effect.all([server.traffic, Effect.timed(read(aimed)), server.traffic]);
        yield* Effect.when(
            Effect.annotateLogs(Effect.logInfo('Inspect read'), { application: row.identifier, build: Option.getOrElse(first.map.coverage.build, () => 'unmapped'), calls: closing.requests - opening.requests, bytes: closing.bytes - opening.bytes, milliseconds: Duration.toMillis(elapsed) }),
            Effect.succeed(within === undefined && Option.isNone(searched)),
        );
        const waited = Option.filter(next, () => Option.exists(since, (seen) => first.application.sequence <= seen));
        const event = Option.flatten(yield* Effect.transposeOption(Option.map(waited, Fiber.join)));
        const { application, map, indexed, level, base } = Option.isSome(waited) ? yield* read(aimed) : first;
        const changed = event.pipe(
            Option.flatMap(({ change }) => (Event.fields.change.isAnyOf(['focused', 'value', 'selection'])(change) ? Option.some(change.control) : Option.none())),
            Option.toArray,
            HashSet.fromIterable,
        );
        const recalled = Address.recall(base)(map);
        const lookup = (at: string | undefined) => Result.transposeMapOption(Option.fromUndefinedOr(at), (given) => Result.orElse(Address.resolve(base, given), () => Address.resolve(recalled, given)));
        const [scope, start] = yield* Effect.fromResult(Result.all([lookup(within), lookup(after)]));
        const shelved = scope.pipe(
            Option.flatMap(({ target }) => (target._tag === 'Control' ? Option.some(target.control) : Option.none())),
            Option.filter(({ opens }) => opens !== undefined && !Option.exists(Record.get(base.live, `${opens}`), ({ target }) => target._tag === 'Container')),
            Option.flatMap(({ locator }) => map.level(locator)),
            Option.flatMap(({ locator }) => Record.get(recalled.located, locator)),
            Option.flatMap(Array.findFirst((each: Item) => !each.live)),
        );
        const reading = Option.flatMap(
            Option.filter(scope, () => Option.isNone(shelved)),
            scopeOf(start, Option.isSome(searched)),
        );
        const fresh = yield* Effect.transposeOption(Option.map(reading, level));
        const index = Option.match(fresh, { onNone: () => (Option.isSome(shelved) || [scope, start].some(Option.exists((each: Item) => !each.live)) ? recalled : base), onSome: ({ containers }) => indexed(containers) });
        const opened = Option.filter(fresh, () => Option.exists(reading, Element.Scope.isAnyOf(['menu']))).pipe(
            Option.flatMap(({ containers }) => Array.head(containers)),
            Option.flatMap(({ ref }) => Record.get(index.live, `${ref}`)),
        );
        const partial = Option.isSome(searched) && Option.isNone(scope) && !unopened ? partialOf(base) : [];
        yield* Effect.forEach(partial, ({ ref, from, count }) => Effect.option(level({ _tag: 'rows', ref, from, count })), { discard: true });
        const merged = Option.match(yield* last, { onNone: () => base, onSome: Struct.get('index') });
        const unread = Array.isReadonlyArrayEmpty(partial) ? [] : partialOf(merged).flatMap(({ token, count }) => Option.toArray(token).map((multi) => scrollOf(multi, `${Format.count(count, Outline.nouns.row)} unread`)));
        const lines = yield* Effect.fromResult(
            Option.match(searched, {
                onNone: () => Outline.render(index, { in: Option.firstSomeOf([shelved, opened, scope]), after: Option.exists(reading, Element.Scope.isAnyOf(['rows'])) ? Option.none() : start, changed }),
                onSome: (given) => {
                    const pool = Match.value({ unopened, scope }).pipe(
                        Match.when({ unopened: true }, () => ({ ...recalled, items: map.coverage.openers.flatMap(({ control }) => Option.getOrElse(Record.get(recalled.located, control), Array.empty<Item>)) })),
                        Match.when({ scope: Option.isSome }, ({ scope: { value } }) => ({ ...index, items: membersOf(index, value) })),
                        Match.orElse(() => Address.recall(merged)(map)),
                    );
                    const hits = Address.find(pool, query ?? '', Option.fromUndefinedOr(kind));
                    const empty = Array.isReadonlyArrayEmpty(hits) ? [new Line({ head: [Outline.nouns.match.other], value: Format.count(hits.length) })] : [];
                    return Result.map(Outline.hits(hits, given, { after: start, opening: Option.some(recalled) }), Array.appendAll([...empty, ...unread]));
                },
            }),
        );
        const { tool: active, shortcut, help, holding = [], operations = [], cursor, draggingFrame } = application.pointer ?? {};
        const others = Option.fromUndefinedOr(cursor).pipe(
            Option.filter(Element.Pointer.fields.cursor.schema.guards.custom),
            Option.flatMap(({ digest }) => map.cursor(digest)),
            Option.map((tools) => Record.keys(tools).filter((named) => named !== active)),
            Option.filter(Array.isArrayNonEmpty),
        );
        const pointed = [
            ...Array.fromNullishOr(shortcut).map(Outline.shortcut),
            ...Array.match(holding, { onEmpty: Array.empty, onNonEmpty: (keys) => [`holding ${Format.list(keys)}`] }),
            ...Array.match(operations, { onEmpty: Array.empty, onNonEmpty: (running) => [`running ${running.join(' then ')}`] }),
            ...Array.fromNullishOr(cursor).map(Element.Pointer.fields.cursor.schema.match({ system: ({ name }) => `cursor ${name}`, custom: () => 'custom cursor' })),
            ...Option.toArray(others).map((named) => `seen with tools ${Format.list(named)}`),
            ...(draggingFrame === undefined ? [] : ['dragging']),
            ...Array.fromNullishOr(help).map((text) => `help: ${Format.quoted(text)}`),
        ];
        const rows = Option.all({ in: Option.flatMap(scope, Struct.get('token')), page: Option.filter(reading, Element.Scope.guards.rows) });
        const listed = Option.match(rows, { onNone: Array.empty, onSome: ({ page }) => rowsOf(index, page.ref) });
        const total = Option.flatMap(scope, ({ target }) => (target._tag === 'Control' ? Option.fromUndefinedOr(target.control.total) : Option.none()));
        const more = Option.all({ rows, at: Option.flatMap(Array.last(listed), ({ item }) => item.token) }).pipe(
            Option.filter(({ rows: { page } }) => Option.match(total, { onNone: () => listed.length >= page.count, onSome: (held) => page.from + listed.length < held })),
            Option.map(({ rows: { in: multi, page }, at }) => new Line({ head: ['more'], call: new Call({ tool: tool.name, given: { in: multi, after: at } }), pinned: true, ...Option.getOrUndefined(Option.map(total, (held) => ({ value: Format.count(held - page.from - listed.length) }))) })),
        );
        const unloaded = Option.all({ rows, at: Option.flatMap(start, Struct.get('token')) }).pipe(
            Option.filter(() => Array.isReadonlyArrayEmpty(listed)),
            Option.map(({ rows: { in: multi }, at }) => scrollOf(multi, `rows after ${at} not loaded`)),
        );
        return new Report({
            lines: [
                ...(yield* Outline.subject(row.name, live)),
                ...(active === undefined && Array.isReadonlyArrayEmpty(pointed) ? [] : [new Line({ head: ['pointer'], facts: pointed, ...(active === undefined ? {} : { value: active }) })]),
                ...lines,
                ...Option.toArray(more),
                ...Option.toArray(unloaded),
                ...Array.findFirst(index.shown, ({ container }) => container.asking).pipe(Option.flatMap(answerOf), Option.toArray),
            ],
        });
    }, Effect.scoped),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Inspect };
