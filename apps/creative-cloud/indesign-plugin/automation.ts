// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { BridgeError } from '@rasm/creative-cloud-server/errors';
import { bundle } from '@rasm/creative-cloud-server/hosts';
import { Enums } from '@rasm/creative-cloud-server/indesign/jobs';
import { doScript, read, reply } from '@rasm/creative-cloud-server/osascript';
import { absent, camel, dictionary, fourcc, MANIPULATION, pascal, type SdefClass, type SdefEnumeration, type SdefProperty, sorted } from '@rasm/creative-cloud-server/sdef';
import { attached, declared, install, POLL, probed, protocol, relaunch, server, until } from '@rasm/creative-cloud-server/uxp';
import { HOSTS } from '@rasm/creative-cloud-server/values';
import { Array, Cause, Clock, Console, Effect, FileSystem, Filter, flow, HashSet, identity, Match, Option, Order, Path, Predicate, pipe, Record, Result, Schema, String, Struct } from 'effect';
import { Command } from 'effect/unstable/cli';
import { ChildProcess } from 'effect/unstable/process';
import {
    type ClassDeclaration,
    type ExpressionWithTypeArguments,
    type GetAccessorDeclarationStructure,
    type InterfaceDeclaration,
    type InterfaceDeclarationStructure,
    type MethodSignatureStructure,
    type OptionalKind,
    Project,
    type PropertySignatureStructure,
    type SetAccessorDeclarationStructure,
    StructureKind,
    VariableDeclarationKind,
    type WriterFunction,
} from 'ts-morph';
import { bridge } from './uxp.config.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Declaration extends InterfaceDeclarationStructure {
    readonly extends: string[];
    readonly properties: OptionalKind<PropertySignatureStructure>[];
    readonly getAccessors: OptionalKind<GetAccessorDeclarationStructure>[];
    readonly setAccessors: OptionalKind<SetAccessorDeclarationStructure>[];
    readonly methods: OptionalKind<MethodSignatureStructure>[];
}

interface Slot {
    readonly name: string;
    readonly type: string;
    readonly description: string;
}

interface Lookup {
    readonly tables: Readonly<Record<string, Readonly<Record<string, Slot>>>>;
    readonly parents: Readonly<Record<string, readonly string[]>>;
}

type Enumerations = Readonly<Record<string, Readonly<Record<string, number>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST = HOSTS.indesign;
const _DECLARATIONS = ['Contents', 'Resources', 'UXP', 'com.adobe.indesign.creative-assistant', 'tsValidation', 'indesign.d.ts'] as const;
const _OUTPUT = ['..', 'server', 'indesign', 'indesign.ts'] as const;
const _ENUMERATIONS = 'enumerations.ts';
const _TYPINGS = { uxp: '@adobe-uxp-types/uxp' } as const;
const _ENUMERATOR_DOC = /(?<name>[A-Z][A-Za-z0-9]*) enumerator/gu;
const _PRIMITIVES: Readonly<Record<string, string>> = {
    any: 'any',
    boolean: 'boolean',
    date: 'Date',
    file: 'File | string',
    integer: 'number',
    number: 'number',
    real: 'number',
    record: 'object',
    specifier: 'any',
    text: 'string',
    type: 'string',
};
const _DOCKED_MS = 5000;
const _DOCKED = `${doScript(`var label = ${JSON.stringify(bridge.panel.label.default)}; var docked = app.panels.itemByName(label).isValid; if (!docked) app.menuActions.itemByName(label).invoke(); docked`)} language javascript`;
const _SCOPE = 'return { TextEncoder: typeof TextEncoder, TextDecoder: typeof TextDecoder, queueMicrotask: typeof queueMicrotask, hrtimeBigint: typeof process.hrtime.bigint };';
const _LIVE =
    "const m = require('indesign'); const names = Object.getOwnPropertyNames(m); return { enumerations: names.filter((n) => { const v = m[n]; return typeof v === 'object' && v !== null && v.constructor.name === 'Enumeration'; }), functions: names.filter((n) => typeof m[n] === 'function') };";

// --- [ERRORS] --------------------------------------------------------------------------

const _strings = Schema.Array(Schema.String);
const _Live = Schema.Struct({ enumerations: _strings, functions: _strings });

const AutomationError = Schema.TaggedUnion({
    unreconciled: { undeclared: Schema.Struct({ enumerations: _strings, constants: Schema.Record(Schema.String, Schema.NonEmptyArray(Schema.String)) }), unvalued: _strings },
});

// --- [DICTIONARY] ----------------------------------------------------------------------

const _Enum = Schema.Struct({ name: Schema.String, members: Schema.Array(Schema.Struct({ name: Schema.String, initializer: Schema.NumberFromString })) });

// --- [NAMES] ---------------------------------------------------------------------------

const _constant = (name: string): string => (name === 'default' ? 'DEFAULT_VALUE' : Array.join(Array.map(String.split(Array.headNonEmpty(String.split(name, '.')), ' '), String.toUpperCase), '_'));

const _mentioned = (description: string): readonly string[] =>
    Array.filterMap(
        Array.fromIterable(description.matchAll(_ENUMERATOR_DOC)),
        Filter.fromPredicateOption((found) => Option.fromNullishOr(found.groups?.['name'])),
    );

const _typeNames = (property: SdefProperty): readonly string[] => [...Option.toArray(property.attributes.type), ...Array.map(property.type, (row) => row.attributes.type)];

// --- [DECLARATIONS] --------------------------------------------------------------------

const _text = (type: string | WriterFunction | undefined): string => (Predicate.isString(type) ? type : 'any');

const _description = (docs: InterfaceDeclarationStructure['docs']): string =>
    Array.join(
        Array.map(Array.flatten(Array.fromNullishOr(docs)), (doc) => String.trim(Predicate.isString(doc) ? doc : _text(doc.description))),
        ' ',
    );

const _declaration =
    (names: HashSet.HashSet<string>) =>
    (name: string, parents: readonly ExpressionWithTypeArguments[], node: ClassDeclaration | InterfaceDeclaration): Declaration => {
        const union = (type: string | WriterFunction | undefined): string => {
            const parts = String.split(_text(type), ' & ');
            return parts.length > 1 && Array.every(parts, (part) => HashSet.has(names, part)) ? Array.join(parts, ' | ') : _text(type);
        };
        const accepted = (type: string | WriterFunction | undefined): string => {
            const text = union(type);
            return Array.contains(String.split(text, ' | '), 'File') ? `${text} | string` : text;
        };
        return {
            kind: StructureKind.Interface,
            name,
            isExported: true,
            docs: Array.map(node.getJsDocs(), (doc) => doc.getStructure()),
            extends: Array.map(parents, (parent) => parent.getText()),
            properties: Array.map(node.getProperties(), (property) => {
                const structure = property.getStructure();
                return { ...Struct.pick(structure, ['name', 'isReadonly', 'hasQuestionToken', 'docs']), type: union(structure.type) };
            }),
            getAccessors: Array.map(node.getGetAccessors(), (accessor) => {
                const structure = accessor.getStructure();
                return { ...structure, returnType: union(structure.returnType) };
            }),
            setAccessors: Array.map(node.getSetAccessors(), (accessor) => {
                const structure = accessor.getStructure();
                return { ...structure, parameters: Array.map(Array.flatten(Array.fromNullishOr(structure.parameters)), (parameter) => ({ ...parameter, type: accepted(parameter.type) })) };
            }),
            methods: Array.map(node.getMethods(), (method) => {
                const structure = method.getStructure();
                return {
                    ...Struct.pick(structure, ['docs', 'hasQuestionToken']),
                    name: method.getName(),
                    parameters: Array.map(Array.flatten(Array.fromNullishOr(structure.parameters)), (parameter) => ({ ...parameter, type: accepted(parameter.type) })),
                    returnType: union(structure.returnType),
                };
            }),
        };
    };

const _members = (row: Declaration): readonly Slot[] => [
    ...Array.map(row.properties, (field) => ({ name: field.name, type: _text(field.type), description: _description(field.docs) })),
    ...Array.map(row.getAccessors, (getter) => ({ name: getter.name, type: _text(getter.returnType), description: _description(getter.docs) })),
];

const _slots = (row: Declaration): readonly Slot[] => [
    ..._members(row),
    ...Array.map(row.setAccessors, (setter) => ({
        name: setter.name,
        type: _text(Option.getOrElse(Option.map(Array.head(Array.flatten(Array.fromNullishOr(setter.parameters))), Struct.get('type')), () => 'any')),
        description: _description(setter.docs),
    })),
];

const _table = (row: Declaration): Readonly<Record<string, Slot>> =>
    Record.fromIterableBy(
        [..._members(row), ...Array.map(row.methods, (method) => ({ name: method.name, type: `(${JSON.stringify(method.parameters)}) => ${JSON.stringify(method.returnType)}`, description: '' }))],
        Struct.get('name'),
    );

const _lookup = (rows: Readonly<Record<string, Declaration>>): Lookup => ({ tables: Record.map(rows, _table), parents: Record.map(rows, Struct.get('extends')) });

const _writable = (row: Declaration): Readonly<Record<string, boolean>> =>
    Record.fromEntries([
        ...Array.map(row.properties, (field) => [field.name, field.isReadonly !== true] as const),
        ...Array.map(row.getAccessors, (getter) => [getter.name, false] as const),
        ...Array.map(row.setAccessors, (setter) => [setter.name, true] as const),
    ]);

const _ancestors = (lookup: Lookup, name: string): readonly string[] => {
    const parents = Option.getOrElse(Record.get(lookup.parents, name), () => []);
    return Array.dedupe([...parents, ...Array.flatMap(parents, (parent) => _ancestors(lookup, parent))]);
};

const _flattened = (rows: Readonly<Record<string, Declaration>>, lookup: Lookup, name: string): Readonly<Record<string, boolean>> =>
    Record.fromEntries([
        ...Array.flatMap(Array.reverse(_ancestors(lookup, name)), (ancestor) => Record.toEntries(Option.match(Record.get(rows, ancestor), { onNone: () => ({}), onSome: _writable }))),
        ...Record.toEntries(Option.match(Record.get(rows, name), { onNone: () => ({}), onSome: _writable })),
    ]);

const _find = (lookup: Lookup, name: string, member: string): Option.Option<Slot> =>
    Option.orElse(Option.flatMap(Record.get(lookup.tables, name), Record.get(member)), () =>
        Array.head(
            Array.filterMap(
                Option.getOrElse(Record.get(lookup.parents, name), () => []),
                Filter.fromPredicateOption((parent) => _find(lookup, parent, member)),
            ),
        ),
    );

const _only = <A, B>(self: Readonly<Record<string, A>>, that: Readonly<Record<string, B>>): Record<string, A> => Record.filter(self, (_, key) => !Record.has(that, key));

const _parent = (lookup: Lookup, row: Declaration, parent: string): string =>
    Array.match(sorted(Record.keys(Record.filter(_table(row), (own, name) => Option.exists(_find(lookup, parent, name), (inherited) => inherited.type !== own.type)))), {
        onEmpty: () => parent,
        onNonEmpty: (names) =>
            `Omit<${parent}, ${Array.join(
                Array.map(names, (name) => `'${name}'`),
                ' | ',
            )}>`,
    });

const _candidates =
    (lookup: Lookup, adobeNames: readonly string[]) =>
    (pair: { readonly klass: SdefClass; readonly property: SdefProperty }): readonly string[] =>
        Option.match(_find(lookup, pascal(pair.klass.attributes.name), camel(pair.property.attributes.name)), {
            onNone: () => [],
            onSome: (slot) => Array.intersection([...Array.map(String.split(slot.type, '|'), String.trim), ..._mentioned(slot.description)], adobeNames),
        });

const _nameOf =
    (adobe: Enumerations, referenced: HashSet.HashSet<string>, candidates: (code: string) => readonly string[]) =>
    (row: SdefEnumeration): Result.Result<{ readonly name: string; readonly row: SdefEnumeration }, string> =>
        Result.fromOption(
            Option.map(
                Option.orElse(
                    _identity(
                        adobe,
                        Array.match(candidates(row.attributes.code), { onEmpty: () => Record.keys(adobe), onNonEmpty: identity }),
                        Array.map(row.enumerator, (member) => fourcc(member.attributes.code)),
                    ),
                    () => (HashSet.has(referenced, row.attributes.code) ? Option.some(pascal(row.attributes.name)) : Option.none()),
                ),
                (name) => ({ name, row }),
            ),
            () => row.attributes.code,
        );

const _sdefType =
    (named: (name: string) => string) =>
    (property: SdefProperty): string =>
        Array.match(property.type, {
            onEmpty: () => named(Option.getOrElse(property.attributes.type, () => 'any')),
            onNonEmpty: (rows) => Array.join(Array.dedupe(Array.map(rows, (type) => (Option.isSome(type.attributes.list) ? `${named(type.attributes.type)}[]` : named(type.attributes.type)))), ' | '),
        });

const _fields =
    (sdefType: (property: SdefProperty) => string) =>
    (row: SdefClass): OptionalKind<PropertySignatureStructure>[] =>
        Array.map(
            Array.filter(row.property, (property) => Option.isNone(property.attributes.hidden)),
            (property) => ({
                name: camel(property.attributes.name),
                type: sdefType(property),
                isReadonly: Option.isSome(property.attributes.access),
                docs: Array.filter(Option.toArray(property.attributes.description), String.isNonEmpty),
            }),
        );

const _identity = (adobe: Enumerations, candidates: readonly string[], values: readonly number[]): Option.Option<string> => {
    const ranked = Array.sort(
        Array.filterMap(
            candidates,
            Filter.fromPredicateOption((name) =>
                Option.map(Record.get(adobe, name), (constants) => ({ name, count: Array.intersection(Record.values(constants), values).length, size: Record.size(constants) })),
            ),
        ),
        Order.Struct({ count: Order.flip(Order.Number), size: Order.Number }),
    );
    return Option.map(
        Option.filter(Array.head(ranked), (best) => best.count > 0 && Option.match(Array.get(ranked, 1), { onNone: () => true, onSome: (next) => next.count < best.count || next.size > best.size })),
        Struct.get('name'),
    );
};

// --- [GENERATE] ------------------------------------------------------------------------

const _generate = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const bundlePath = yield* bundle(_HOST);
    const parsed = yield* Effect.flatMap(Effect.orDie(reply(ChildProcess.make('sdef', [bundlePath]))), dictionary);
    const project = new Project({ useInMemoryFileSystem: true, manipulationSettings: MANIPULATION });
    const adobe = project.createSourceFile('adobe.d.ts', yield* Effect.orDie(fs.readFileString(path.join(bundlePath, ..._DECLARATIONS))));
    const classes = Array.filter(Array.flatMap(parsed.dictionary.suite, Struct.get('class')), (row) => Option.isNone(row.attributes.hidden));
    const enumerations = Record.values(Record.fromIterableBy(Array.flatMap(parsed.dictionary.suite, Struct.get('enumeration')), (row) => row.attributes.code));
    const adobeEnumerations: Enumerations = Record.fromIterableWith(yield* Schema.decodeUnknownEffect(Schema.Array(_Enum))(Array.map(adobe.getEnums(), (node) => node.getStructure())), (row) => [
        row.name,
        Record.fromIterableWith(row.members, (member) => [member.name, member.initializer]),
    ]);
    const adobeNames = Record.keys(adobeEnumerations);
    const declaration = _declaration(HashSet.fromIterable([...Array.map(adobe.getClasses(), (node) => node.getNameOrThrow()), ...Array.map(adobe.getInterfaces(), (node) => node.getName())]));
    const typed: Readonly<Record<string, Declaration>> = Record.fromIterableBy(
        [
            ...Array.map(adobe.getClasses(), (node) => declaration(node.getNameOrThrow(), Array.fromNullishOr(node.getExtends()), node)),
            ...Array.map(adobe.getInterfaces(), (node) => declaration(node.getName(), node.getExtends(), node)),
        ],
        Struct.get('name'),
    );
    const lookup = _lookup(typed);
    const pairs = Array.flatMap(classes, (klass) => Array.map(klass.property, (property) => ({ klass, property })));
    const candidates = _candidates(lookup, adobeNames);
    const [unidentified, identified] = Array.separate(
        Array.map(
            enumerations,
            _nameOf(adobeEnumerations, HashSet.fromIterable(Array.flatMap(pairs, (pair) => _typeNames(pair.property))), (code) =>
                sorted(
                    Array.flatMap(
                        Array.filter(pairs, (pair) => Array.contains(_typeNames(pair.property), code)),
                        candidates,
                    ),
                ),
            ),
        ),
    );
    const enumerationNames = Record.fromIterableWith(identified, ({ name, row }) => [row.attributes.code, name]);
    const classNames = Record.fromIterableWith(classes, (row) => [row.attributes.name, pascal(row.attributes.name)]);
    const sdefTable: Enumerations = Record.fromIterableWith(identified, ({ name, row }) => [
        name,
        Record.fromIterableWith(row.enumerator, (member) => [_constant(member.attributes.name), fourcc(member.attributes.code)]),
    ]);
    const phrases = Record.fromIterableWith(identified, ({ name, row }) => [name, Record.fromIterableWith(row.enumerator, (member) => [_constant(member.attributes.name), member.attributes.name])]);
    const shared = Record.toEntries(
        Record.intersection(sdefTable, adobeEnumerations, (sdef, theirs) => ({
            added: Record.keys(_only(sdef, theirs)),
            mismatched: Record.keys(
                Record.filter(
                    Record.intersection(sdef, theirs, (value, known) => value !== known),
                    identity,
                ),
            ),
        })),
    );
    const completed: Enumerations = Record.union(adobeEnumerations, sdefTable, (theirs, sdef) => ({ ...theirs, ..._only(sdef, theirs) }));
    const fields = _fields(
        _sdefType((name) =>
            pipe(
                Record.get(enumerationNames, name),
                Option.orElse(() => Record.get(classNames, name)),
                Option.orElse(() => Record.get(_PRIMITIVES, name)),
                Option.getOrElse(() => 'any'),
            ),
        ),
    );
    const additions = pipe(
        Array.flatMap(classes, (row) => Array.map(fields(row), (field) => ({ owner: pascal(row.attributes.name), field }))),
        Array.filter(({ owner, field }) => Record.has(typed, owner) && Option.isNone(_find(lookup, owner, field.name))),
        Array.dedupeWith((left, right) => left.owner === right.owner && left.field.name === right.field.name),
        Array.groupBy(Struct.get('owner')),
    );
    const missing = Array.filter(classes, (row) => !Record.has(typed, pascal(row.attributes.name)));
    const widened: Readonly<Record<string, Declaration>> = {
        ...Record.map(typed, (row) => ({ ...row, properties: [...row.properties, ...Array.map(Array.flatten(Option.toArray(Record.get(additions, row.name))), Struct.get('field'))] })),
        ...Record.fromIterableBy(
            Array.map(
                missing,
                (row): Declaration => ({
                    kind: StructureKind.Interface,
                    name: pascal(row.attributes.name),
                    isExported: true,
                    docs: Array.filter(Option.toArray(row.attributes.description), String.isNonEmpty),
                    extends: Array.map(Option.toArray(row.attributes.inherits), pascal),
                    properties: fields(row),
                    getAccessors: [],
                    setAccessors: [],
                    methods: [],
                }),
            ),
            Struct.get('name'),
        ),
    };
    const widenedLookup = _lookup(widened);
    const properties = Record.map(Array.groupBy(Array.flatMap(Record.values(widened), _slots), Struct.get('name')), (group) => {
        const types = sorted(Array.flatMap(group, (slot) => Array.map(String.split(slot.type, '|'), String.trim)));
        return {
            types,
            list: Array.some(types, String.endsWith('[]')),
            enumerations: sorted(Array.intersection([...types, ..._mentioned(Array.join(Array.map(group, Struct.get('description')), ' '))], Record.keys(completed))),
        };
    });
    const collections = Record.fromIterableWith(
        Array.filterMap(
            classes,
            Filter.fromPredicateOption((row) => row.attributes.plural),
        ),
        (plural) => [camel(plural), pascal(plural)],
    );
    const ancestors = Record.map(widened, (_, name) => _ancestors(widenedLookup, name));
    const out = project.createSourceFile('indesign.ts', {
        statements: [
            'declare const enumerationName: unique symbol;',
            'export interface Enumerator<Name extends string> { readonly [enumerationName]?: Name; equals(other: Enumerator<Name>): boolean; toString(): string; }',
            ...Array.map(Record.keys(completed), (name): InterfaceDeclarationStructure => ({ kind: StructureKind.Interface, name, isExported: true, extends: [`Enumerator<'${name}'>`] })),
            ...Array.map(
                Record.toEntries(completed),
                ([name, constants]) =>
                    `export declare const ${name}: { ${Array.join(
                        Array.map(Record.keys(constants), (constant) => `readonly ${constant}: ${name};`),
                        ' ',
                    )} };`,
            ),
            { kind: StructureKind.TypeAlias, name: 'Real', type: 'number' },
            { kind: StructureKind.TypeAlias, name: 'Strings', type: 'readonly string[]' },
            ...Array.map(adobe.getTypeAliases(), (node) => node.getStructure()),
            ...Array.map(Record.values(widened), (row) => ({ ...row, extends: Array.map(row.extends, (parent) => _parent(widenedLookup, row, parent)) })),
            { kind: StructureKind.VariableStatement, declarationKind: VariableDeclarationKind.Const, hasDeclareKeyword: true, isExported: true, declarations: [{ name: 'app', type: 'Application' }] },
            `export const enumerations: Readonly<Record<string, Readonly<Record<string, number>>>> = ${JSON.stringify(completed, null, 4)};`,
            `export const phrases: Readonly<Record<string, Readonly<Record<string, string>>>> = ${JSON.stringify(phrases, null, 4)};`,
            `export const properties: Readonly<Record<string, { readonly types: readonly string[]; readonly list: boolean; readonly enumerations: readonly string[] }>> = ${JSON.stringify(properties, null, 4)};`,
            `export const collections: Readonly<Record<string, string>> = ${JSON.stringify(collections, null, 4)};`,
            `export const members = ${JSON.stringify(
                Record.map(widened, (_, name) => _flattened(widened, widenedLookup, name)),
                null,
                4,
            )};`,
            `export const ancestors: Readonly<Record<string, readonly string[]>> = ${JSON.stringify(ancestors, null, 4)};`,
            `export const preferences = ${JSON.stringify(
                pipe(
                    Record.get(widenedLookup.tables, 'Application'),
                    Option.getOrElse((): Readonly<Record<string, Slot>> => ({})),
                    Record.filter((slot) => slot.type === 'Preference' || Option.exists(Record.get(ancestors, slot.type), Array.contains('Preference'))),
                    Record.map(Struct.get('type')),
                ),
                null,
                4,
            )};`,
        ],
    });
    yield* fs.writeFileString(path.join(import.meta.dirname, ..._OUTPUT), out.getFullText());
    const registered = sorted(Record.keys(completed));
    const table = project.createSourceFile(_ENUMERATIONS, {
        statements: [
            { kind: StructureKind.ImportDeclaration, moduleSpecifier: 'adobe:indesign', namedImports: Array.map(registered, (name) => ({ name })) },
            `const registered: Readonly<Record<string, object>> = { ${Array.join(registered, ', ')} };`,
            'export { registered };',
        ],
    });
    yield* fs.writeFileString(path.join(import.meta.dirname, _ENUMERATIONS), table.getFullText());
    const typings = yield* Effect.all(Record.map(_TYPINGS, (name, module) => declared(new URL(import.meta.resolve(`${name}/package.json`)), module, import.meta.dirname)));
    yield* Console.log(
        JSON.stringify(
            {
                dictionary: Option.map(parsed.dictionary.attributes, Struct.get('title')),
                classes: {
                    sdef: classes.length,
                    adobe: Record.size(typed),
                    added: Array.map(missing, (row) => pascal(row.attributes.name)),
                    addedProperties: Array.reduce(Record.values(additions), 0, (total, group) => total + group.length),
                },
                enumerations: {
                    sdef: Record.size(sdefTable),
                    adobe: Record.size(adobeEnumerations),
                    added: Record.keys(_only(sdefTable, adobeEnumerations)),
                    addedConstants: Array.flatMap(shared, ([name, row]) => Array.map(row.added, (constant) => `${name}.${constant}`)),
                    valueMismatches: Array.flatMap(shared, ([name, row]) => Array.map(row.mismatched, (constant) => `${name}.${constant}`)),
                    adobeOnly: Array.difference(adobeNames, Record.keys(sdefTable)),
                    unidentified,
                },
                properties: Record.size(properties),
                collections: Record.size(collections),
                typings,
            },
            null,
            4,
        ),
    );
});

// --- [DEPLOY] --------------------------------------------------------------------------

const _Enumerated = Schema.Struct({ result: Schema.Union([Enums, Schema.Struct({ kind: Schema.Literal('error'), error: BridgeError })]).pipe(Schema.toTaggedUnion('kind')) });

const _deploy = Effect.gen(function* () {
    const placed = yield* install(bridge.manifest, import.meta.dirname);
    const mcp = yield* server(_HOST, bridge.manifest);
    const launchedAt = yield* relaunch(_HOST, 'quit saving ask');
    const docked = yield* read(_HOST.id, _HOST.bundleId, _DOCKED_MS, _DOCKED, Option.none()).pipe(
        Effect.retry({ while: Predicate.or(Predicate.isTagged('hostNotRunning'), Predicate.isTagged('hostUnresponsive')), schedule: POLL }),
        Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Boolean))),
    );
    const link = yield* until(mcp.health, attached);
    const attachedAt = yield* Clock.currentTimeMillis;
    const probe = yield* until(mcp.health, probed);
    const probedAt = yield* Clock.currentTimeMillis;
    const scope = yield* mcp.execute(_SCOPE);
    yield* Console.log(
        JSON.stringify({ ...placed, docked, attachedAfterLaunchMs: attachedAt - launchedAt, probedAfterAttachMs: probedAt - attachedAt, link: link.link, probe: probe.probe, scope }, null, 4),
    );
});

// --- [RECONCILE] -----------------------------------------------------------------------

const _reconcile = Effect.gen(function* () {
    const mcp = yield* server(_HOST, bridge.manifest);
    yield* until(mcp.health, probed);
    const generated = yield* Effect.promise(() => import('../server/indesign/indesign.ts'));
    const live = yield* Effect.flatMap(mcp.execute(_LIVE), Schema.decodeUnknownEffect(_Live));
    const listed = yield* Effect.flatMap(mcp.call('indesign_list_enums', {}, _Enumerated), ({ result }) =>
        Match.value(result).pipe(Match.discriminatorsExhaustive('kind')({ enums: (row) => Effect.succeed(row.enums), error: (row) => Effect.fail(row.error) })),
    );
    const registered = Record.map(Record.fromIterableBy(listed, Struct.get('name')), (row) => sorted(Array.map(row.constants, Struct.get('name'))));
    const tabled = Record.map(generated.enumerations, flow(Record.keys, sorted));
    const valued = (enumeration: string, constant: { readonly name: string; readonly value: Option.Option<number> }): boolean =>
        Option.exists(constant.value, (value) => Option.contains(Option.flatMap(Record.get(generated.enumerations, enumeration), Record.get(constant.name)), value));
    const unvalued = Array.flatMap(listed, (row) =>
        Array.map(
            Array.filter(
                row.constants,
                Predicate.not((constant) => valued(row.name, constant)),
            ),
            (constant) => `${row.name}.${constant.name}`,
        ),
    );
    const report = {
        undeclared: { enumerations: Array.difference(live.enumerations, Record.keys(tabled)), constants: absent(registered, tabled) },
        unregistered: absent(tabled, registered),
        unvalued,
        generated: Record.size(tabled),
        runtime: live.enumerations.length,
        constants: Array.reduce(listed, 0, (total, row) => total + row.constants.length),
        collections: { generated: Record.size(generated.collections), unconfirmed: Array.difference(Record.values(generated.collections), live.functions) },
    };
    yield* Console.log(JSON.stringify(report, null, 4));
    return yield* report.undeclared.enumerations.length === 0 && Record.isEmptyRecord(report.undeclared.constants) && unvalued.length === 0
        ? Console.log('list_enums equals the generated table over every runtime enumeration, constant, and value')
        : Effect.fail(AutomationError.cases.unreconciled.make({ undeclared: report.undeclared, unvalued }));
});

// --- [ENTRY] ---------------------------------------------------------------------------

Command.run(
    Command.make('automation').pipe(
        Command.withSubcommands([
            Command.make('generate', {}, () => _generate),
            Command.make('deploy', {}, () => Effect.provide(Effect.scoped(_deploy), protocol)),
            Command.make('reconcile', {}, () => Effect.provide(Effect.scoped(_reconcile), protocol)),
        ]),
    ),
    { version: bridge.manifest.version },
).pipe(
    Effect.tapError((error) => Console.error(Cause.pretty(Cause.fail(error)))),
    Effect.tapDefect(flow(Cause.die, Cause.pretty, Console.error)),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
