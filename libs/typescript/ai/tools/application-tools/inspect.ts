import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { Event } from '@rasm/applications/server/event';
import { Call, Format, Line, Report } from '@rasm/mcp/report';
import { Handle, Server } from '@rasm/mcp/server';
import { Array, Context, Duration, Effect, Fiber, HashSet, Match, Number, Option, Record, Result, Schema, Stream, Struct, UndefinedOr } from 'effect';
import { Tool } from 'effect/ai';
import { Act } from '#/act';
import { Address } from '#/handle/address';
import { Outline } from '#/handle/outline';
import { Target } from '#/handle/target';

// --- [MODELS] --------------------------------------------------------------------------

const Clipboard = Schema.Literal('clipboard');
const tool = Tool.make('inspect', {
    description: 'Reads the target interface as an outline of containers and controls, each led by a reference act and view take, and searches what shows now with what its interface map remembers',
    parameters: Schema.Struct({
        in: Schema.optionalKey(
            Schema.Union([Clipboard, Address.At]).annotate({
                description: `Container or control read alone, by reference or path: a menu item reads its menu, a multi ${Outline.rows} of its rows, a text control its style runs, any other control its value, range, frame, and actions; clipboard reads the clipboard, and every container reads when absent`,
            }),
        ),
        find: Schema.optionalKey(Schema.String.annotate({ description: 'Text searched in labels, values, and help of everything shown and remembered, or of in alone; each hit prints the reference or path act takes' })),
        kind: Schema.optionalKey(Element.cases.control.fields.kind.annotate({ description: 'Kind of control find keeps, or every control of that kind without find' })),
        unopened: Schema.optionalKey(Schema.Boolean.annotate({ description: 'true lists the items whose menu, dialog, popover, or options the interface map never saw open' })),
        after: Schema.optionalKey(Address.At.annotate({ description: 'Reference a more line names, continuing its list' })),
        wait: Schema.optionalKey(
            Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 60, exclusiveMinimum: true })).annotate({
                description: 'Most seconds to wait for a change of the target, none when it changed since your last read; act results already list what their steps changed',
            }),
        ),
    }),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
}).annotateMerge(
    Context.make(Tool.Readonly, true).pipe(Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true }), Context.add(Server.Instructions, ['Containers read in an application join its interface map: search it with inspect find before opening a menu or dialog, and act on a remembered hit by the path find prints.'])),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const descend = (read: Address, ref: typeof Element.Ref.Type): ReturnType<typeof Address.list> => Option.toArray(Address.get(read, ref)).flatMap((node) => [node, ...node.element.children.flatMap((child) => descend(read, child.ref))]);
const list = (multi: typeof Element.cases.control.Type) => multi.children.filter(Element.isAnyOf(['control'])).filter((child) => child.kind === 'row');
const scroll = (multi: string, facts: readonly string[]) => new Line({ head: [multi], facts, call: new Call({ tool: Act.tool.name, given: { steps: [{ pointer: multi, by: [0, -400], button: 'none' }] } }), pinned: true });
const Inspect = {
    tool,
    run: Effect.fnUntraced(function* ({ in: within, find: query, kind, after, wait, unopened = false }: Tool.Parameters<typeof tool>) {
        const [server, target, handle] = yield* Effect.all([ApplicationServer, Target.current, Handle]);
        const { row, live } = target;
        if (within === Clipboard.literal) {
            const [subject, clipboard] = yield* Effect.all([Outline.subject(row.name, live), Effect.flatMap(server.pasteboard({ content: true }), (pasteboard) => Outline.clipboard(live.instance, pasteboard, true))]);
            return new Report({ lines: [...subject, ...clipboard] });
        }
        const searched = Option.liftPredicate(Record.getSomes({ find: Option.fromUndefinedOr(query), kind: Option.fromUndefinedOr(kind), unopened: Option.liftPredicate<Schema.Json>(unopened, Boolean) }), (given) => !Record.isEmptyRecord(given));
        const before = Option.filter(yield* handle.get(Address.Held), ({ instance }) => instance === live.instance);
        const next = yield* Option.fromUndefinedOr(wait).pipe(
            Option.map((seconds) =>
                server.changes.pipe(
                    Stream.filter(({ instance }) => instance === live.instance),
                    Stream.runHead,
                    Effect.timeoutOption(Duration.seconds(seconds)),
                    Effect.map(Option.flatten),
                    Effect.forkScoped({ startImmediately: true }),
                ),
            ),
            Effect.transposeOption,
        );
        const [opening, [elapsed, first], closing] = yield* Effect.all([server.traffic, Effect.timed(Address.read(target)), server.traffic]);
        yield* Effect.when(
            Effect.annotateLogs(Effect.logInfo('Inspect read'), { application: row.identifier, build: Option.getOrElse(first.map.coverage.build, () => 'unmapped'), calls: closing.requests - opening.requests, bytes: closing.bytes - opening.bytes, milliseconds: Duration.toMillis(elapsed) }),
            Effect.succeed(within === undefined && Option.isNone(searched)),
        );
        const waiting = Option.filter(next, () => Option.exists(before, ({ model }) => first.model.sequence <= model.sequence));
        const event = Option.flatten(yield* Effect.transposeOption(Option.map(waiting, Fiber.join)));
        const read = Option.isSome(waiting) ? yield* Address.read(target) : first;
        const idle = Option.fromUndefinedOr(wait).pipe(
            Option.filter(() => Option.isSome(waiting) && Option.isNone(event)),
            Option.map((seconds) => `no change in ${Format.duration(Duration.seconds(seconds))}`),
        );
        const changed = event.pipe(
            Option.map(({ change }) => change),
            Option.filter(Event.Changed.fields.change.isAnyOf(['focused', 'value', 'selection'])),
            Option.map(({ control }) => control),
            Option.toArray,
            HashSet.fromIterable,
        );
        const resolve = (at: string | undefined) => Result.transposeMapOption(Option.fromUndefinedOr(at), (given) => Address.resolve(read, given));
        const [scope, start] = yield* Effect.fromResult(Result.all([resolve(within), resolve(after)]));
        const shelved = scope.pipe(
            Option.flatMap((node) => (node.live && node.element._tag === 'control' ? Option.some(node.element) : Option.none())),
            Option.filter(({ opens }) => opens !== undefined && !Option.exists(Address.get(read, opens), ({ element }) => element.ref === opens)),
            Option.flatMap(({ locator }) => read.map.level(locator)),
            Option.flatMap(({ locator }) => Array.findFirst(Address.locate(read, locator), (each) => !each.live)),
        );
        const offset = start.pipe(
            Option.flatMap((node) => (node.live && node.element._tag === 'control' ? Option.fromUndefinedOr(node.element.index) : Option.none())),
            Option.match({ onNone: () => 0, onSome: Number.increment }),
        );
        const reading = Option.filter(scope, () => Option.isNone(shelved)).pipe(
            Option.flatMap((node): Option.Option<typeof Element.Scope.Type> =>
                Match.value(node).pipe(
                    Match.when({ live: true, element: { _tag: 'control', kind: 'multi' } }, ({ element }) => Option.some({ _tag: 'rows' as const, ref: element.ref, from: offset, count: Outline.rows })),
                    Match.when({ live: true, element: { _tag: 'control', kind: 'text' } }, ({ element }) => Option.some({ _tag: 'text' as const, ref: element.ref })),
                    Match.when({ live: true, element: { _tag: 'control', kind: (held: (typeof Element.cases.control.Type)['kind']) => held !== 'choice', opens: Match.number } }, ({ element }) => Option.some({ _tag: 'menu' as const, ref: element.ref })),
                    Match.when({ live: true }, ({ containers }) => Option.some({ _tag: 'container' as const, ref: Array.lastNonEmpty(containers).ref })),
                    Match.orElse(() => Option.none()),
                ),
            ),
        );
        const level = yield* Effect.transposeOption(Option.map(reading, (asked) => Address.read(target, asked)));
        const shown = Option.getOrElse(
            Option.filter(level, ({ model }) => Array.isReadonlyArrayNonEmpty(model.elements)),
            () => read,
        );
        const menu = Option.filter(level, () => Option.exists(reading, Element.Scope.isAnyOf(['menu']))).pipe(Option.flatMap((held) => Option.flatMap(Array.head(held.model.elements), ({ ref }) => Address.get(held, ref))));
        const page = Option.filter(reading, Element.Scope.isAnyOf(['rows']));
        const at = Option.firstSomeOf([shelved, menu, scope]);
        const resume = Option.isSome(page) ? Option.none() : start;
        const pages = yield* Effect.when(Address.fill(target), Effect.succeed(Option.isSome(searched) && Option.isNone(scope) && !unopened));
        const unread = Option.getOrElse(pages, () => []).flatMap(({ token, count, read: fetched }) => {
            const left = count - Result.getOrElse(fetched, () => 0);
            const refused = Result.match(fetched, { onSuccess: Array.empty<string>, onFailure: ({ message }) => [message] });
            return left > 0 ? [scroll(token, [`${Format.count(left, Outline.nouns.row)} unread`, ...refused])] : [];
        });
        const merged = Option.getOrElse(yield* handle.get(Address.Held), () => read);
        const lines = yield* Effect.fromResult(
            Option.match(searched, {
                onNone: () => Outline.render(shown, { in: at, after: resume, changed }),
                onSome: (given) => {
                    const under = Option.map(at, (node) => (node.live ? Array.drop(descend(shown, node.element.ref), 1) : Address.recall(merged, node.element.locator)));
                    const among = unopened ? merged.map.coverage.openers.flatMap(({ control }) => Address.locate(merged, control)) : Option.getOrUndefined(under);
                    const found = Address.find(merged, query ?? '', Option.fromUndefinedOr(kind), among);
                    const none = Array.isReadonlyArrayEmpty(found) ? [new Line({ head: [Outline.nouns.match.other], value: Format.count(found.length) })] : [];
                    return Result.map(Outline.hits(merged, found, { ...given, ...UndefinedOr.map(within, (held) => ({ in: held })) }, resume), Array.appendAll([...none, ...unread]));
                },
            }),
        );
        const { tool: active, shortcut, help, holding = [], operations = [], cursor, draggingFrame } = read.model.pointer ?? {};
        const others = Option.fromUndefinedOr(cursor).pipe(
            Option.filter(Element.Pointer.fields.cursor.schema.guards.custom),
            Option.flatMap(({ digest }) => read.map.cursor(digest)),
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
        const continuation = Option.gen(function* () {
            const asked = yield* page;
            const multi = yield* Option.filter(
                Option.map(Address.get(shown, asked.ref), ({ element }) => element),
                Element.isAnyOf(['control']),
            );
            const [rows, token] = [list(multi), Address.refer(multi.kind, multi.ref)];
            const left = UndefinedOr.map(multi.total, (total) => total - asked.from - rows.length);
            return yield* Option.match(Array.last(rows), {
                onNone: () =>
                    Option.map(
                        Option.flatMap(start, (node) => node.token),
                        (last) => scroll(token, [`rows after ${last} not loaded`]),
                    ),
                onSome: (last) =>
                    Option.liftPredicate(new Line({ head: ['more'], ...UndefinedOr.map(left, (count) => ({ value: Format.count(count) })), call: new Call({ tool: tool.name, given: { ...Option.getOrUndefined(searched), in: token, after: Address.refer(last.kind, last.ref) } }), pinned: true }), () =>
                        UndefinedOr.match(left, { onUndefined: () => rows.length >= asked.count, onDefined: (count) => count > 0 }),
                    ),
            });
        });
        const answer = Array.findFirst(Address.list(shown), ({ element }) => (element._tag === 'container' && element.asking ? Option.some(element) : Option.none())).pipe(
            Option.flatMap((held) =>
                Option.map(Option.fromUndefinedOr(held.default ?? held.cancel), (ref) => new Line({ head: [Address.refer(held.kind, held.ref)], facts: Struct.keys(Struct.pick(held, ['asking'])), call: new Call({ tool: Act.tool.name, given: { steps: [{ at: Address.refer('action', ref) }] } }), pinned: true })),
            ),
        );
        return new Report({
            lines: [
                ...(yield* Outline.subject(row.name, live, { facts: Option.toArray(idle) })),
                ...(active === undefined && Array.isReadonlyArrayEmpty(pointed) ? [] : [new Line({ head: ['pointer'], facts: pointed, ...(active === undefined ? {} : { value: active }) })]),
                ...lines,
                ...Option.toArray(continuation),
                ...Option.toArray(answer),
            ],
        });
    }, Effect.scoped),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Inspect };
