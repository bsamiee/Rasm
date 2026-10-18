import { Unmatched } from '@rasm/mcp/server';
import { type AnyNode, getLineInfo, parse } from 'acorn';
import { fullAncestor } from 'acorn-walk';
import { Array, Cache, Effect, flow, Match, Option, Predicate, Record, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import MagicString from 'magic-string';
import { type Execute, InDesignError } from './contract.ts';
import { collection, encoded, exported, type Host, host, Model, plural, record, resolve, write } from './dom.ts';
import { previewed } from './render.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Unit = Omit<Rpc.Payload<typeof Execute>, 'run'>;
interface Splice {
    readonly node: AnyNode;
    readonly call: '__item' | '__set' | '__add';
    readonly note: string;
    readonly swaps: readonly (readonly [from: number, to: number, text: string])[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINE_BREAK = /\r\n?|[\n\p{Zl}\p{Zp}]/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const parsed = (json: string | undefined): Option.Option<Schema.Json> => Option.flatMap(Option.fromUndefinedOr(json), Schema.decodeOption(Schema.fromJsonString(Schema.Json)));
const script = Effect.fn('script')(function* (code: string, unit: Unit) {
    const [notes, lines, runSync] = [new Set<string>(), code.split(LINE_BREAK), Effect.runSyncWith(yield* Effect.context<Model>())];
    const failed = (position: Option.Option<number>) => InDesignError.refused(Option.map(position, (at) => [`line ${at}`, ...Option.toArray(Array.get(lines, at - 1))].join(': ')));
    const corrected = (site: string, target: Host, values: Record.ReadonlyRecord<string, unknown>): void => {
        runSync(write(target, values)).forEach((note) => {
            notes.add(`${site}.${note}`);
        });
    };
    const hooks = {
        __item: (site: string, target: { readonly [name: PropertyKey]: unknown }, key: PropertyKey): unknown => {
            const routed = collection(target) && !Predicate.isSymbol(key) && !(key in target);
            if (routed) {
                notes.add(site);
            }
            return routed ? target.item(key) : target[key];
        },
        __set: (site: string, target: object, key: PropertyKey, value: unknown): unknown => {
            if (Predicate.isSymbol(key) || !host(target)) {
                Reflect.set(target, key, value);
            } else {
                corrected(site, target, key === 'properties' && record(value) ? value : { [key]: value });
            }
            return value;
        },
        __add: (site: string, target: { readonly add: (...values: readonly unknown[]) => unknown }, ...values: readonly unknown[]): unknown => {
            const created = target.add(...values);
            if (host(created)) {
                values.filter(record).forEach((properties) => {
                    corrected(site, created, properties);
                });
            }
            return created;
        },
        resolve: flow(resolve, runSync),
    };
    const tree = yield* Effect.try({
        try: () => parse(code, { ecmaVersion: 2024, preserveParens: true, allowReturnOutsideFunction: true, allowAwaitOutsideFunction: Option.isNone(unit.preview) }),
        catch: (cause) => failed(Option.map(Schema.decodeUnknownOption(Schema.Struct({ loc: Schema.Struct({ line: Schema.Int }) }))(cause), ({ loc }) => loc.line))(cause),
    });
    const visits: { readonly node: AnyNode; readonly up: readonly AnyNode[] }[] = [];
    fullAncestor(tree, (node, _state, ancestors) => {
        visits.push({ node, up: Array.drop(Array.reverse(ancestors), 1).filter((above) => above.type !== 'ParenthesizedExpression') });
    });
    const source = (node: AnyNode): string => code.slice(node.start, node.end);
    const line = (at: number): number => getLineInfo(code, at).line;
    const bare = (node: AnyNode): AnyNode => (node.type === 'ParenthesizedExpression' ? bare(node.expression) : node);
    const optional = (node: AnyNode): boolean => (node.type === 'MemberExpression' && (node.optional || optional(node.object))) || (node.type === 'CallExpression' && (node.optional || optional(node.callee)));
    const referenced = (node: AnyNode, parent: AnyNode | undefined, grandparent: AnyNode | undefined): boolean =>
        (parent?.type === 'AssignmentExpression' && bare(parent.left) === node) ||
        ((parent?.type === 'CallExpression' || parent?.type === 'NewExpression') && bare(parent.callee) === node) ||
        (parent?.type === 'TaggedTemplateExpression' && bare(parent.tag) === node) ||
        ((parent?.type === 'ForInStatement' || parent?.type === 'ForOfStatement') && bare(parent.left) === node) ||
        (parent?.type === 'AssignmentPattern' && parent.left === node) ||
        (parent?.type === 'UnaryExpression' && parent.operator === 'delete') ||
        parent?.type === 'UpdateExpression' ||
        parent?.type === 'ArrayPattern' ||
        parent?.type === 'RestElement' ||
        grandparent?.type === 'ObjectPattern';
    const splices = Array.flatMap<(typeof visits)[number], Splice>(visits, ({ node, up: [parent, grandparent] }) =>
        Match.value(node).pipe(
            Match.withReturnType<readonly Splice[]>(),
            Match.when({ type: 'MemberExpression', computed: true }, (member) =>
                member.object.type === 'Super' || optional(member) || referenced(member, parent, grandparent)
                    ? []
                    : [
                          {
                              node: member,
                              call: '__item',
                              note: `${source(member)} read as ${source(member.object)}.item(${source(member.property)})`,
                              swaps: [
                                  [member.object.end, member.property.start, ', '],
                                  [member.property.end, member.end, ''],
                              ],
                          },
                      ],
            ),
            Match.when({ type: 'AssignmentExpression', operator: '=', left: { type: 'MemberExpression' } }, (assignment) => {
                const { object, property, computed } = assignment.left;
                const swaps = computed
                    ? ([
                          [object.end, property.start, ', '],
                          [property.end, assignment.right.start, ', '],
                      ] as const)
                    : ([[object.end, assignment.right.start, `, ${JSON.stringify(source(property))}, `]] as const);
                return object.type === 'Super' || property.type === 'PrivateIdentifier' ? [] : [{ node: assignment, call: '__set', note: source(object), swaps }];
            }),
            Match.when({ type: 'CallExpression', callee: { type: 'MemberExpression', computed: false, property: { type: 'Identifier', name: 'add' } } }, (call) => {
                const [first, last, from] = [call.arguments[0], call.arguments.at(-1), call.callee.object.end];
                const swaps =
                    first === undefined || last === undefined
                        ? ([[from, call.end, '']] as const)
                        : ([
                              [from, first.start, ', '],
                              [last.end, call.end, ''],
                          ] as const);
                return call.callee.object.type === 'Super' || optional(call) ? [] : [{ node: call, call: '__add', note: `${source(call.callee)}(…)`, swaps }];
            }),
            Match.orElse(() => []),
        ),
    );
    const swapped = (edited: MagicString, [start, end, put]: readonly [number, number, string]): MagicString => edited.update(start, end, `${put}${'\n'.repeat(line(end) - line(start))}`);
    const spliced = splices.reduce((prior, { node, call, note, swaps }) => swaps.reduce(swapped, prior.prependRight(node.start, `${call}(${JSON.stringify(`line ${line(node.start)}: ${note}`)}, `)).appendLeft(node.end, ')'), new MagicString(code));
    const used = new Set(Array.flatMap(visits, ({ node }) => (node.type === 'Identifier' ? [node.name] : [])));
    const nested = (above: AnyNode): boolean => above.type === 'FunctionDeclaration' || above.type === 'FunctionExpression' || above.type === 'ArrowFunctionExpression';
    const awaiting = Array.some(visits, ({ node, up }) => (node.type === 'AwaitExpression' || (node.type === 'ForOfStatement' && node.await)) && !Array.some(up, nested));
    const [given, text] = [{ ...hooks, ...Object.fromEntries(exported.filter(([name]) => used.has(name) && !(name in globalThis))) }, spliced.prepend('{\n').append('\n}').toString()];
    const compiled = yield* Effect.try({
        // biome-ignore lint/nursery/noImpliedEval: Galley Script compiles caller-supplied source to run in InDesign
        try: () => new Function(...Object.keys(given), awaiting ? `return (async () => ${text})();` : text),
        catch: failed(Option.none()),
    });
    const header = compiled.toString().slice(0, compiled.toString().indexOf(text)).split('\n').length;
    const thrown = (cause: unknown): InDesignError => {
        const prepare = Error.prepareStackTrace;
        Error.prepareStackTrace = (_error, frames) => frames.find((frame) => frame.isEval())?.getLineNumber();
        try {
            return failed(Option.map(Option.liftPredicate(Predicate.hasProperty(cause, 'stack') ? cause.stack : undefined, Predicate.isNumber), (at) => at - header))(cause);
        } finally {
            Error.prepareStackTrace = prepare;
        }
    };
    const stepped = yield* previewed(
        unit,
        Effect.gen(function* () {
            const value = yield* Effect.try({ try: (): unknown => Reflect.apply(compiled, undefined, Object.values(given)), catch: thrown });
            yield* Predicate.isPromiseLike(value) && Option.isSome(unit.preview) ? Effect.fail(InDesignError.make({ reasons: [{ _tag: 'Unsettled' }] })) : Effect.void;
            return Predicate.isPromiseLike(value) ? value : encoded(value, true);
        }),
    );
    const settled = stepped.result;
    const json = Predicate.isPromiseLike(settled) ? encoded(yield* Effect.tryPromise({ try: () => Promise.resolve(settled), catch: thrown }), true) : settled;
    return { result: parsed(json), corrections: Option.liftPredicate(Array.fromIterable(notes), Array.isArrayNonEmpty), preview: stepped.preview };
});
const set = Effect.fn('set')(function* ({ target, properties, add }: Extract<Rpc.Payload<typeof Execute>['run'], { readonly _tag: 'Set' }>, unit: Unit) {
    const model = yield* Model;
    const shaped = (owner: Host, fields: Record.ReadonlyRecord<string, unknown>): Record.ReadonlyRecord<string, unknown> =>
        Record.map(fields, (value, field) => {
            const held = owner[field];
            return record(value) && host(held) ? shaped(held, value) : held;
        });
    const added = Effect.fnUntraced(function* (owner: Host, field: string) {
        const items = owner[field];
        const fields = Effect.map(Effect.option(Cache.get(model, owner.constructorName)), Option.match({ onNone: () => ({}), onSome: Struct.get('fields') }));
        return collection(items)
            ? yield* Effect.try({ try: () => items.add(), catch: InDesignError.refused(Option.some(`${owner.toSpecifier()} ${field}.add()`)) })
            : yield* Unmatched.make({ subject: `collection of ${owner.toSpecifier()}`, query: field, candidates: Record.keys(Record.filter(yield* fields, ({ type }) => type !== undefined && plural(type))) });
    });
    const changed = Effect.fnUntraced(function* (member: Host) {
        const before = Option.isNone(add) ? shaped(member, properties) : {};
        const written = yield* write(member, properties);
        const after = shaped(member, properties);
        return { written, rows: Array.map(Record.keys(properties), (field) => ({ target: member, field, before: before[field] ?? null, after: after[field] })) };
    });
    const stepped = yield* previewed(
        unit,
        Effect.gen(function* () {
            const members = Array.ensure(yield* resolve(target));
            const results = yield* Effect.forEach(yield* Option.match(add, { onNone: () => Effect.succeed(members), onSome: (field) => Effect.forEach(members, (owner) => added(owner, field)) }), changed);
            return { json: encoded(Array.flatMap(results, Struct.get('rows')), true), notes: Array.flatMap(results, Struct.get('written')) };
        }),
    );
    return { result: parsed(stepped.result.json), corrections: Option.liftPredicate(stepped.result.notes, Array.isArrayNonEmpty), preview: stepped.preview };
});
const execute = ({ run, ...unit }: Rpc.Payload<typeof Execute>) => Match.valueTags(run, { Script: ({ code }) => script(code, unit), Set: (form) => set(form, unit) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
