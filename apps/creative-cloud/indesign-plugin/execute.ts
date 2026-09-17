// --- [IMPORTS] -------------------------------------------------------------------------

import { app, ScriptLanguage, UndoModes } from 'adobe:indesign';
import { thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import type { Execute } from '@rasm/creative-cloud-server/frames';
import { collections, properties } from '@rasm/creative-cloud-server/indesign';
import { type AnyNode, type Expression, type Identifier, type ModuleDeclaration, type Node, type Pattern, type Program, parse, type Statement, type Super, type VariableDeclaration } from 'acorn';
import { fullAncestor } from 'acorn-walk';
import { generate } from 'astring';
import { Array, Effect, Filter, Function, Match, Option, Order, Predicate, Record, Result, Schema, Struct } from 'effect';
import { type Constant, coded, type Live, named } from './enums.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Names {
    readonly referenced: readonly string[];
    readonly declared: readonly string[];
}

type Classified = { readonly kind: 'collection'; readonly klass: string } | { readonly kind: 'array' } | { readonly kind: 'unknown' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NOTES = 10;
const _ITEM = '__item';
const _ITEM_HELPER = `const ${_ITEM} = (c, i, n) => c?.constructor?.name === n ? c.item(i) : c[i];`;
const _AsyncFunction: new (code: string) => () => Promise<unknown> = Object.getPrototypeOf(async () => undefined).constructor;
const _SyncFunction: new (code: string) => () => unknown = Object.getPrototypeOf(() => undefined).constructor;
const _byType = Match.discriminator('type');

// --- [PARSING] -------------------------------------------------------------------------

const _parsed = Option.liftThrowable((code: string): Program => parse(code, { ecmaVersion: 'latest', sourceType: 'script', allowReturnOutsideFunction: true, allowAwaitOutsideFunction: true }));

const _marshalled: (text: unknown) => Effect.Effect<Schema.Json, Schema.SchemaError> = Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Json));

const _source = (code: string, node: Node): string => code.slice(node.start, node.end);

const _identifier = (name: string): Identifier => ({ type: 'Identifier', name, start: 0, end: 0 });

const _member = (node: Expression | Super, property: string): boolean => node.type === 'MemberExpression' && !node.computed && node.property.type === 'Identifier' && node.property.name === property;

const _destructuring = (statement: Statement | ModuleDeclaration): statement is VariableDeclaration =>
    statement.type === 'VariableDeclaration' &&
    Option.exists(
        Array.head(statement.declarations),
        (declarator) =>
            declarator.id.type === 'ObjectPattern' &&
            declarator.init?.type === 'CallExpression' &&
            declarator.init.callee.type === 'Identifier' &&
            declarator.init.callee.name === 'require' &&
            Option.exists(Array.head(declarator.init.arguments), (argument) => argument.type === 'Literal' && argument.value === 'indesign'),
    );

const _bound = (node: Pattern): readonly string[] =>
    Match.value(node).pipe(
        _byType('Identifier', (identifier) => [identifier.name]),
        _byType('ObjectPattern', (pattern) => Array.flatMap(pattern.properties, (property) => _bound(property.type === 'RestElement' ? property.argument : property.value))),
        _byType('ArrayPattern', (pattern) => Array.flatMap(Array.getSomes(Array.map(pattern.elements, Option.fromNullishOr)), _bound)),
        _byType('RestElement', (rest) => _bound(rest.argument)),
        _byType('AssignmentPattern', (assignment) => _bound(assignment.left)),
        _byType('MemberExpression', (): readonly string[] => []),
        Match.orElseAbsurd,
    );

// --- [COLLECTIONS] ---------------------------------------------------------------------

const _classify =
    (variables: Readonly<Record<string, Classified>>) =>
    (node: Expression | Super): Classified => {
        if (node.type === 'ArrayExpression' || (node.type === 'CallExpression' && (_member(node.callee, 'getElements') || _member(node.callee, 'everyItem')))) {
            return { kind: 'array' };
        }
        if (node.type === 'Identifier') {
            return Option.getOrElse(Record.get(variables, node.name), (): Classified => ({ kind: 'unknown' }));
        }
        if (node.type !== 'MemberExpression' || node.computed || node.property.type !== 'Identifier') {
            return { kind: 'unknown' };
        }
        const { name } = node.property;
        if (Option.exists(Record.get(properties, name), Struct.get('list'))) {
            return { kind: 'array' };
        }
        return Option.match(Record.get(collections, name), { onNone: (): Classified => ({ kind: 'unknown' }), onSome: (klass): Classified => ({ kind: 'collection', klass }) });
    };

const _same = (left: Classified, right: Classified): boolean => left.kind === right.kind && (left.kind !== 'collection' || right.kind !== 'collection' || left.klass === right.klass);

const _variables = (program: Program): Readonly<Record<string, Classified>> => {
    const classify = _classify({});
    const seen: [string, Classified][] = [];
    fullAncestor(program, (node) => {
        if (node.type === 'VariableDeclarator' && node.id.type === 'Identifier' && node.init) {
            seen.push([node.id.name, classify(node.init)]);
        }
        if (node.type === 'AssignmentExpression' && node.operator === '=' && node.left.type === 'Identifier') {
            seen.push([node.left.name, classify(node.right)]);
        }
    });
    return Record.filterMap(
        Array.groupBy(seen, ([name]) => name),
        (pairs) =>
            Result.fromOption(
                Option.flatMap(
                    Option.liftPredicate(Array.dedupeWith(Array.map(pairs, Struct.get(1)), _same), (distinct) => distinct.length === 1),
                    Array.head,
                ),
                Function.constVoid,
            ),
    );
};

const _written = (node: AnyNode, parent: Option.Option<AnyNode>): boolean =>
    Option.exists(
        parent,
        (above) =>
            (above.type === 'AssignmentExpression' && above.left === node) ||
            (above.type === 'UpdateExpression' && above.argument === node) ||
            (above.type === 'UnaryExpression' && above.operator === 'delete' && above.argument === node),
    );

const _collections = (code: string, program: Program): readonly string[] => {
    const classify = _classify(_variables(program));
    const notes: string[] = [];
    fullAncestor(program, (node, _state, ancestors) => {
        if (node.type !== 'MemberExpression' || !node.computed || node.optional) {
            return;
        }
        const index = node.property;
        if (index.type === 'PrivateIdentifier' || index.type === 'TemplateLiteral' || (index.type === 'Literal' && Predicate.isString(index.value))) {
            return;
        }
        const base = _written(node, Array.get(ancestors, ancestors.length - 2)) ? { kind: 'unknown' as const } : classify(node.object);
        if (base.kind !== 'collection') {
            return;
        }
        notes.push(`${_source(code, node)} → ${_ITEM}(${_source(code, node.object)}, ${_source(code, index)}, ${JSON.stringify(base.klass)})`);
        Object.assign(node, {
            type: 'CallExpression',
            callee: _identifier(_ITEM),
            arguments: [node.object, index, { type: 'Literal', value: base.klass, raw: JSON.stringify(base.klass), start: 0, end: 0 }],
            optional: false,
        });
    });
    return notes;
};

// --- [ENUMERATIONS] --------------------------------------------------------------------

const _integer = (literal: unknown): literal is number => Predicate.isNumber(literal) && Number.isInteger(literal);

const _constant = (live: Live, candidates: readonly string[], literal: unknown): Option.Option<Constant> =>
    Option.orElse(
        Option.flatMap(Option.liftPredicate(literal, Predicate.isString), (text) => named(live, candidates, text)),
        () => Option.flatMap(Option.liftPredicate(literal, _integer), (code) => coded(live, candidates, code)),
    );

const _enumerations = (live: Live, code: string, program: Program): readonly { readonly note: string; readonly enumeration: string }[] => {
    const rewrites: { readonly note: string; readonly enumeration: string }[] = [];
    fullAncestor(program, (node) => {
        if (node.type !== 'AssignmentExpression' || node.operator !== '=') {
            return;
        }
        const { left, right } = node;
        if (left.type !== 'MemberExpression' || left.computed || left.property.type !== 'Identifier' || right.type !== 'Literal') {
            return;
        }
        const candidates = Option.match(Record.get(properties, left.property.name), { onNone: () => [], onSome: Struct.get('enumerations') });
        Option.map(_constant(live, candidates, right.value), ({ enumeration, constant }) => {
            rewrites.push({ note: `${_source(code, node)} → ${_source(code, left)} = ${enumeration}.${constant}`, enumeration });
            Object.assign(right, { type: 'MemberExpression', object: _identifier(enumeration), property: _identifier(constant), computed: false, optional: false });
        });
    });
    return rewrites;
};

// --- [IMPORTS_PASS] --------------------------------------------------------------------

const _declares = (declared: readonly string[]): Names => ({ referenced: [], declared });

const _names = (live: Live, node: AnyNode): Names =>
    Match.value(node).pipe(
        _byType(
            'MemberExpression',
            (member): Names => ({
                referenced: member.object.type === 'Identifier' && (member.object.name === 'app' || (!member.computed && Record.has(live, member.object.name))) ? [member.object.name] : [],
                declared: [],
            }),
        ),
        _byType('VariableDeclarator', (declarator) => _declares(_bound(declarator.id))),
        _byType('FunctionDeclaration', 'FunctionExpression', 'ArrowFunctionExpression', (fn) =>
            _declares([...Array.flatMap(fn.params, _bound), ...Option.toArray(Option.map(Option.fromNullishOr(fn.id), Struct.get('name')))]),
        ),
        _byType('CatchClause', (clause) => _declares(Option.match(Option.fromNullishOr(clause.param), { onNone: () => [], onSome: _bound }))),
        Match.orElse(() => _declares([])),
    );

const _imports = (live: Live, program: Program, rewritten: readonly string[]): Option.Option<{ readonly needed: Array.NonEmptyReadonlyArray<string>; readonly line: string }> => {
    const seen: Names[] = [];
    fullAncestor(program, (node) => {
        seen.push(_names(live, node));
    });
    const needed = Array.difference(Array.dedupe([...Array.flatMap(seen, Struct.get('referenced')), ...rewritten]), Array.flatMap(seen, Struct.get('declared')));
    return Option.map(Option.liftPredicate(needed, Array.isArrayNonEmpty), (names) => {
        const [rest, existing] = Array.partition(program.body, Filter.fromPredicate(_destructuring));
        program.body.splice(0, program.body.length, ...rest);
        const bound = Array.flatMap(existing, (statement) => Array.flatMap(statement.declarations, (declarator) => _bound(declarator.id)));
        return { needed: names, line: `const { ${Array.join(Array.sort(Array.dedupe([...bound, ...names]), Order.String), ', ')} } = require('indesign');` };
    });
};

// --- [AUTOCORRECT] ---------------------------------------------------------------------

const _capped = (notes: readonly string[]): readonly string[] => (notes.length <= _NOTES ? notes : [...Array.take(notes, _NOTES), `${notes.length - _NOTES} more`]);

const _autocorrect = (live: Live, code: string): { readonly code: string; readonly autocorrections: readonly string[] } =>
    Option.match(_parsed(code), {
        onNone: () => ({ code, autocorrections: [] }),
        onSome: (program) => {
            const indexing = _collections(code, program);
            const enumerations = _enumerations(live, code, program);
            const imports = _imports(live, program, Array.map(enumerations, Struct.get('enumeration')));
            const helper = Option.toArray(Option.liftPredicate(_ITEM_HELPER, () => indexing.length > 0));
            const notes = Array.dedupe([
                ...Array.map(helper, () => `prepends ${_ITEM} helper line`),
                ...indexing,
                ...Array.map(enumerations, Struct.get('note')),
                ...Array.map(Option.toArray(imports), ({ needed }) => `imports ${Array.join(needed, ', ')} from require('indesign')`),
            ]);
            return { code: notes.length === 0 ? code : Array.join([...Array.map(Option.toArray(imports), Struct.get('line')), ...helper, generate(program)], '\n'), autocorrections: _capped(notes) };
        },
    });

// --- [RUN] -----------------------------------------------------------------------------

const execute = (live: Live, { code, undoName }: Execute): { readonly autocorrections: readonly string[]; readonly run: Effect.Effect<unknown, HostRejection> } => {
    const corrected = _autocorrect(live, code);
    return {
        autocorrections: corrected.autocorrections,
        run: Option.match(undoName, {
            onNone: () => Effect.tryPromise({ try: () => new _AsyncFunction(corrected.code)(), catch: thrown }),
            onSome: (name) =>
                Effect.flatMap(
                    Effect.try({
                        try: (): unknown =>
                            app.doScript(
                                new _SyncFunction(`const value = (() => {\n${corrected.code}\n})(); return JSON.stringify(value === undefined ? null : value);`),
                                ScriptLanguage.UXPSCRIPT,
                                [],
                                UndoModes.ENTIRE_SCRIPT,
                                name,
                            ),
                        catch: thrown,
                    }),
                    (text) => Effect.mapError(_marshalled(text), (cause) => HostRejection.cases.resultNotJson.make({ cause })),
                ),
        }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
