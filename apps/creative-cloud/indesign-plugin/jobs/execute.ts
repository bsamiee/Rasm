// --- [IMPORTS] -------------------------------------------------------------------------

import { app } from 'adobe:indesign';
import { json, type Settled, settle, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { collections } from '@rasm/creative-cloud-server/indesign';
import { Bodies, type Body } from '@rasm/creative-cloud-server/indesign/jobs';
import { type AnyNode, type AssignmentExpression, type CallExpression, type Identifier, type Literal, type MemberExpression, type Node, parse } from 'acorn';
import { fullAncestor, make } from 'acorn-walk';
import { generate } from 'astring';
import { Array, Effect, HashSet, Match, MutableHashSet, Option, Predicate, Record, Schema } from 'effect';
import { constant, type Live, live, scripting, undoable } from '../host.ts';

// --- [SOURCE] --------------------------------------------------------------------------

const _ASYNC: new (...source: string[]) => (...scope: unknown[]) => Promise<unknown> = Object.getPrototypeOf(async (): Promise<void> => undefined).constructor;
const _SYNC: new (...source: string[]) => (...scope: unknown[]) => unknown = Object.getPrototypeOf((): void => undefined).constructor;

const _autocorrect = Effect.fnUntraced(function* (code: string, table: Live, helpers: Readonly<Record<'item' | 'constant', unknown>>) {
    const ast = yield* Effect.try({
        try: () => parse(code, { ecmaVersion: 'latest', sourceType: 'script', allowReturnOutsideFunction: true, allowAwaitOutsideFunction: true }),
        catch: thrown,
    });
    const sites: {
        readonly indexed: MemberExpression[];
        readonly assigned: { readonly node: AssignmentExpression; readonly target: MemberExpression; readonly literal: Literal }[];
        readonly names: string[];
        readonly references: MutableHashSet.MutableHashSet<Node>;
        readonly chains: MutableHashSet.MutableHashSet<Node>;
    } = { indexed: [], assigned: [], names: [], references: MutableHashSet.empty(), chains: MutableHashSet.empty() };
    fullAncestor(
        ast,
        (node, found, ancestors) => {
            const parent = ancestors.at(-2);
            const reference =
                MutableHashSet.has(found.references, node) ||
                ((parent?.type === 'ForInStatement' || parent?.type === 'ForOfStatement') && parent.left === node) ||
                (parent?.type === 'UpdateExpression' && parent.argument === node) ||
                (parent?.type === 'UnaryExpression' && parent.operator === 'delete' && parent.argument === node) ||
                (parent?.type === 'CallExpression' && parent.callee === node) ||
                (parent?.type === 'TaggedTemplateExpression' && parent.tag === node);
            if (node.type === 'Identifier') {
                found.names.push(node.name);
            }
            if (
                node.type === 'MemberExpression' &&
                node.computed &&
                !reference &&
                node.object.type !== 'Super' &&
                node.property.type !== 'TemplateLiteral' &&
                !(node.property.type === 'Literal' && !Predicate.isNumber(node.property.value)) &&
                !MutableHashSet.has(found.chains, node)
            ) {
                found.indexed.push(node);
            }
            if (node.type === 'AssignmentExpression' && node.operator === '=' && node.left.type === 'MemberExpression' && node.left.object.type !== 'Super' && node.right.type === 'Literal') {
                found.assigned.push({ node, target: node.left, literal: node.right });
            }
        },
        make<typeof sites>({
            ['ChainExpression']: (node, found, recurse) => {
                Array.forEach(
                    Array.dropWhile(
                        Array.reverse(
                            Array.unfold<AnyNode, AnyNode>(node.expression, (current) => {
                                if (current.type === 'MemberExpression') {
                                    return Option.some([current, current.object]);
                                }
                                return current.type === 'CallExpression' ? Option.some([current, current.callee]) : Option.none();
                            }),
                        ),
                        (current) => !((current.type === 'MemberExpression' || current.type === 'CallExpression') && current.optional),
                    ),
                    (current) => MutableHashSet.add(found.chains, current),
                );
                recurse(node.expression, found);
            },
            ['Pattern']: (node, found, recurse) => {
                MutableHashSet.add(found.references, node);
                recurse(node, found);
            },
        }),
        sites,
    );
    const suffix = '_'.repeat(Array.reduce(sites.names, 0, (longest, name) => Math.max(longest, name.length)));
    const names = Record.map(helpers, (_, key) => `__rasm_${key}${suffix}`);
    const properties = HashSet.fromIterable(Array.flatMap(table.owners, (owner) => Record.keys(Record.filter(owner.properties, (field) => field.enumerations.length > 0))));
    Array.forEach(sites.indexed, (node) => Object.assign(node, { type: 'CallExpression', callee: { type: 'Identifier', name: names.item }, arguments: [node.object, node.property], optional: false }));
    Array.forEach(sites.assigned, ({ node, target, literal }) => {
        const key = Match.value(target).pipe(
            Match.when({ computed: false, property: { type: 'Identifier' } }, ({ property }) => property.name),
            Match.when({ computed: true, property: { type: 'Literal' } }, ({ property }) => property.value),
            Match.option,
            Option.filter(Predicate.isString),
        );
        if (Option.isNone(key) || !HashSet.has(properties, key.value)) {
            return;
        }
        const receiver: Identifier = { type: 'Identifier', name: `${names.constant}_target`, start: 0, end: 0 };
        const right: CallExpression = {
            type: 'CallExpression',
            callee: { type: 'Identifier', name: names.constant, start: 0, end: 0 },
            arguments: [receiver, { type: 'Literal', value: key.value, start: 0, end: 0 }, literal],
            optional: false,
            start: node.start,
            end: node.end,
        };
        Object.assign(node, {
            type: 'CallExpression',
            callee: { type: 'ArrowFunctionExpression', params: [receiver], body: { ...node, left: { ...target, object: receiver }, right }, expression: true, async: false },
            arguments: [target.object],
            optional: false,
        });
    });
    return { corrected: generate(ast), bindings: Record.fromIterableWith(Record.toEntries(helpers), ([key, helper]) => [names[key], helper]) };
});

// --- [HANDLER] -------------------------------------------------------------------------

const _run: (request: Body<'execute'>) => Effect.Effect<Settled> = Effect.fnUntraced(function* ({ code, undoName }: Body<'execute'>) {
    const table = yield* live;
    const notes: Record<string, true> = {};
    const indexed = Array.getSomes(Array.map(Array.dedupe(Record.values(collections)), (name) => Option.liftPredicate(Reflect.get(table.host, name), Predicate.isFunction)));
    const helpers = {
        item: (value: Readonly<Record<PropertyKey, unknown>>, index: PropertyKey): unknown => {
            const collection = Number.isInteger(index) && Array.some(indexed, (constructor) => value instanceof constructor);
            if (!(collection && Predicate.isFunction(value['item']))) {
                return value[index];
            }
            notes[`${value.constructor.name}[index] → .item(index)`] = true;
            return value['item'](index);
        },
        constant: (value: Record<string, unknown>, key: string, literal: unknown): unknown =>
            Option.match(constant(table, value, key, literal), {
                onNone: () => literal,
                onSome: (found) => {
                    notes[`${key}: ${String(literal)} → ${found.enumeration}.${found.constant}`] = true;
                    return found.enumerator;
                },
            }),
    };
    const work = Effect.gen(function* () {
        const { corrected, bindings } = yield* _autocorrect(code, table, helpers);
        const [names, values] = Array.unzip(Record.toEntries({ app, ...table.enumerations, ...bindings }));
        const started = (): Promise<unknown> => new _ASYNC(...names, `return (async () => {\n${corrected}\n})();`)(...values);
        const synced = (): unknown => Option.getOrNull(Option.fromNullishOr(new _SYNC(...names, `return (() => {\n${corrected}\n})();`)(...values)));
        return yield* scripting(
            Effect.flatMap(Option.match(undoName, { onNone: () => Effect.tryPromise({ try: started, catch: thrown }), onSome: (name) => undoable(name, Schema.Unknown, synced) }), json),
        );
    });
    return { ...(yield* settle(work)), autocorrections: Option.some(Record.keys(notes)) };
});

const execute = (body: Schema.Json): Effect.Effect<Settled> =>
    Effect.matchEffect(Schema.decodeUnknownEffect(Bodies.fields.execute)(body), {
        onFailure: (cause) => settle(Effect.fail(HostRejection.cases.malformedParams.make({ cause }))),
        onSuccess: _run,
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
