// --- [IMPORTS] -------------------------------------------------------------------------

import { app } from 'adobe:indesign';
import { json, type Settled, settle, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { Bodies, type Body } from '@rasm/creative-cloud-server/indesign/jobs';
import { type AssignmentExpression, type Identifier, type MemberExpression, type Node, parse } from 'acorn';
import { fullAncestor } from 'acorn-walk';
import { generate } from 'astring';
import { Array, Effect, Option, Predicate, Record, Schema } from 'effect';
import { constant, type Live, live, registered, scripting, undoable } from '../host.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ITEM = '__item';

// --- [SOURCE] --------------------------------------------------------------------------

const _constructorOf = <R>(sample: () => R): (new (...source: string[]) => (...scope: unknown[]) => R) => Object.getPrototypeOf(sample).constructor;

const _ASYNC = _constructorOf<Promise<unknown>>(async () => undefined);

const _SYNC = _constructorOf<unknown>(() => undefined);

const _identifier = (name: string): Identifier => ({ type: 'Identifier', name, start: 0, end: 0 });

const _autocorrect = (code: string, table: Live): { readonly corrected: string; readonly notes: readonly string[] } => {
    const parsed = Option.liftThrowable(parse)(code, { ecmaVersion: 'latest', sourceType: 'script', allowReturnOutsideFunction: true, allowAwaitOutsideFunction: true });
    if (Option.isNone(parsed)) {
        return { corrected: code, notes: [] };
    }
    const source = (node: Node): string => code.slice(node.start, node.end);
    const sites: { readonly indexed: MemberExpression[]; readonly assigned: { readonly node: AssignmentExpression; readonly key: string; readonly literal: unknown }[] } = {
        indexed: [],
        assigned: [],
    };
    fullAncestor(
        parsed.value,
        (node, found, ancestors) => {
            const above = ancestors.at(-2);
            const written =
                (above?.type === 'AssignmentExpression' && above.left === node) ||
                (above?.type === 'UpdateExpression' && above.argument === node) ||
                (above?.type === 'UnaryExpression' && above.operator === 'delete' && above.argument === node);
            if (
                node.type === 'MemberExpression' &&
                node.computed &&
                !node.optional &&
                !written &&
                node.property.type !== 'TemplateLiteral' &&
                !(node.property.type === 'Literal' && Predicate.isString(node.property.value))
            ) {
                found.indexed.push(node);
            }
            if (
                node.type === 'AssignmentExpression' &&
                node.operator === '=' &&
                node.left.type === 'MemberExpression' &&
                !node.left.computed &&
                node.left.property.type === 'Identifier' &&
                node.right.type === 'Literal'
            ) {
                found.assigned.push({ node, key: node.left.property.name, literal: node.right.value });
            }
        },
        undefined,
        sites,
    );
    const indexed = Array.map(sites.indexed, (node) => {
        const note = `${source(node)} → ${_ITEM}(${source(node.object)}, ${source(node.property)})`;
        Object.assign(node, { type: 'CallExpression', callee: _identifier(_ITEM), arguments: [node.object, node.property], optional: false });
        return note;
    });
    const resolved = Array.getSomes(Array.map(sites.assigned, (site) => Option.map(constant(table, site.key, site.literal), (found) => ({ site, found }))));
    const assigned = Array.map(resolved, ({ found: { enumeration, constant: name }, site: { node } }) => {
        const note = `${source(node)} → ${source(node.left)} = ${enumeration}.${name}`;
        Object.assign(node.right, { type: 'MemberExpression', object: _identifier(enumeration), property: _identifier(name), computed: false, optional: false });
        return note;
    });
    const notes = Array.dedupe([...indexed, ...assigned]);
    return {
        corrected: Array.match(notes, {
            onEmpty: () => code,
            onNonEmpty: () =>
                Array.join(
                    [...Option.toArray(Option.as(Array.head(indexed), `const ${_ITEM} = (c, i) => typeof c?.item === 'function' && !Array.isArray(c) ? c.item(i) : c[i];`)), generate(parsed.value)],
                    '\n',
                ),
        }),
        notes,
    };
};

// --- [HANDLER] -------------------------------------------------------------------------

const _run: (request: Body<'execute'>) => Effect.Effect<Settled> = Effect.fnUntraced(function* ({ code, undoName }: Body<'execute'>) {
    const [hosted, table] = yield* Effect.all([registered, live]);
    const [names, values] = Array.unzip(Record.toEntries({ app, ...hosted }));
    const { corrected, notes } = _autocorrect(code, table);
    const started = (): Promise<unknown> => new _ASYNC(...names, `return (async () => {\n${corrected}\n})();`)(...values);
    const synced = (): unknown => Option.getOrNull(Option.fromNullishOr(new _SYNC(...names, `return (() => {\n${corrected}\n})();`)(...values)));
    const settled = yield* settle(
        scripting(Effect.flatMap(Option.match(undoName, { onNone: () => Effect.tryPromise({ try: started, catch: thrown }), onSome: (name) => undoable(name, Schema.Unknown, synced) }), json)),
    );
    return { ...settled, autocorrections: Option.some(notes) };
});

const execute = (body: Schema.Json): Effect.Effect<Settled> =>
    Effect.matchEffect(Schema.decodeUnknownEffect(Bodies.fields.execute)(body), {
        onFailure: (cause) => settle(Effect.fail(HostRejection.cases.malformedParams.make({ cause }))),
        onSuccess: _run,
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
