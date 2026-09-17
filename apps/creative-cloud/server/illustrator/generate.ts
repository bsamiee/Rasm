// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Console, Effect, FileSystem, Filter, Function, identity, Match, Option, Order, Path, type PlatformError, Predicate, pipe, Record, Result, Schema, String, Struct } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import {
    type BinaryExpression,
    type CallExpression,
    type FunctionDeclaration,
    ModuleDeclarationKind,
    Node,
    Project,
    type SourceFile,
    type StatementStructures,
    StructureKind,
    SyntaxKind,
} from 'ts-morph';
import type { BridgeError } from '../errors.ts';
import { bundle } from '../hosts.ts';
import { reply } from '../osascript.ts';
import { absent, camel, dictionary, fourcc, MANIPULATION, pascal, type SdefClass, type SdefEnumeration, type SdefProperty, sorted } from '../sdef.ts';
import { HOSTS } from '../values.ts';
import { dispatch, type Site, site } from './channel.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Access = 'readonly' | 'readwrite' | 'method';

interface Slot {
    readonly name: string;
    readonly access: Access;
    readonly enumeration: Option.Option<string>;
    readonly literal: Option.Option<string>;
    readonly parameters: readonly { readonly name: string; readonly enumeration: Option.Option<string> }[];
}

interface DomClass {
    readonly name: string;
    readonly kind: Kind;
    readonly slots: readonly Slot[];
}

interface Member {
    readonly name: string;
    readonly access: Access;
    readonly type: string;
    readonly parameters: readonly { readonly name: string; readonly type: string }[];
}

interface Delta {
    readonly declared: boolean;
    readonly members: readonly Member[];
}

interface Table {
    readonly sdef: SdefClass;
    readonly members: Readonly<Record<string, SdefProperty>>;
}

interface Sources {
    readonly sdefClasses: readonly SdefClass[];
    readonly sdefEnumerations: readonly SdefEnumeration[];
    readonly dom: Readonly<Record<string, DomClass>>;
    readonly domEnumerations: Readonly<Record<string, { readonly id: number; readonly members: Readonly<Record<string, number>> }>>;
    readonly known: Readonly<
        Record<string, { readonly parents: readonly string[]; readonly members: readonly string[]; readonly declared: readonly string[]; readonly types: Readonly<Record<string, string>> }>
    >;
    readonly knownEnumerations: Readonly<Record<string, readonly string[]>>;
}

interface Names {
    readonly classes: Readonly<Record<string, string>>;
    readonly enumerations: Readonly<Record<string, string>>;
    readonly typings: (name: string) => string;
}

type Kind = (typeof Kind)['Type'];
type Live = (typeof _Reconciliation)['Type'];
type Tables = Readonly<Record<string, Table>>;
type GenerateError = (typeof GenerateError)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST = HOSTS.illustrator;
const _SCRIPTS = ['..', '..', 'illustrator-scripts'] as const;
const _DICTIONARY = ['Contents', 'Resources', 'Adobe Illustrator.sdef'] as const;
const _FRAMEWORK = ['Contents', 'Frameworks', 'AIUXPExtensionHostAPI.framework', 'Versions', 'A', 'AIUXPExtensionHostAPI'] as const;
const _IMAGE = ['Contents', 'Resources', 'ai_cc_about.png'] as const;
const _TYPINGS = 'types-for-adobe/Illustrator/2022/index.d.ts';
const _MODEL = 'Generated from HostModel.xml';
const _SECTION = /^\s+sectname __cstring\n\s+segname __TEXT\n\s+addr \S+\n\s+size (?<size>0x[0-9a-f]+)\n\s+offset (?<offset>\d+)$/mu;
const _IDENTIFIER = /^[A-Za-z_$][\w$]*$/u;
const _HELPER = /_type_of|_instanceof|_extends|_to_consumable_array|_sliced_to_array|Symbol|Object\.keys|Object\.defineProperty|Array\.isArray/u;
const _TIMEOUT_MS = 300_000;
const _LITERALS: Readonly<Partial<Record<SyntaxKind, string>>> = {
    [SyntaxKind.NumericLiteral]: 'number',
    [SyntaxKind.StringLiteral]: 'string',
    [SyntaxKind.TrueKeyword]: 'boolean',
    [SyntaxKind.FalseKeyword]: 'boolean',
};
const _LIVE_TYPES: Readonly<Record<string, string>> = {
    any: 'unknown',
    array: 'unknown[]',
    boolean: 'boolean',
    filespec: 'File',
    number: 'number',
    point: 'number[]',
    rect: 'number[]',
    string: 'string',
};
const _SDEF_TYPES: Readonly<Record<string, string>> = {
    any: 'unknown',
    boolean: 'boolean',
    date: 'Date',
    'file specification': 'File',
    integer: 'number',
    list: 'unknown[]',
    real: 'number',
    record: 'unknown',
    specifier: 'unknown',
    text: 'string',
    type: 'string',
};

// --- [ERRORS] --------------------------------------------------------------------------

const GenerateError: Schema.TaggedUnion<{
    readonly bundleNotFound: Schema.TaggedStruct<'bundleNotFound', { readonly bundleId: Schema.String }>;
    readonly sectionNotFound: Schema.TaggedStruct<'sectionNotFound', { readonly framework: Schema.String }>;
    readonly scriptNotFound: Schema.TaggedStruct<'scriptNotFound', { readonly count: Schema.Number }>;
    readonly loweringHelper: Schema.TaggedStruct<'loweringHelper', { readonly helpers: Schema.$Record<Schema.String, Schema.String> }>;
}> = Schema.TaggedUnion({
    bundleNotFound: { bundleId: Schema.String },
    sectionNotFound: { framework: Schema.String },
    scriptNotFound: { count: Schema.Number },
    loweringHelper: { helpers: Schema.Record(Schema.String, Schema.String) },
});

// --- [MODELS] --------------------------------------------------------------------------

const _strings = Schema.Array(Schema.String);
const _flags = Schema.Array(Schema.Boolean);
const _section = Schema.decodeUnknownEffect(Schema.Struct({ offset: Schema.NumberFromString, size: Schema.NumberFromString }));
const Kind: Schema.Literals<readonly ['creatable', 'proxy', 'collection']> = Schema.Literals(['creatable', 'proxy', 'collection']);
const _Reconciliation = Schema.Struct({
    kind: Schema.Literal('reconciliation'),
    created: Schema.Json,
    classes: Schema.Record(
        Schema.String,
        Schema.Struct({ properties: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, dataType: Schema.String })), methods: _strings, probed: _flags }),
    ),
    enumerations: Schema.Array(Schema.Record(Schema.String, _flags)),
    globals: Schema.Array(Schema.Record(Schema.String, Schema.Boolean)),
    inherited: _strings,
    unavailable: Schema.Array(Schema.Json),
});

// --- [RECORDS] -------------------------------------------------------------------------

const _nonEmpty = Option.liftPredicate(Array.isReadonlyArrayNonEmpty);

const _byKey = <A>(self: Readonly<Record<string, A>>): readonly (readonly [string, A])[] => Array.sortWith(Record.toEntries(self), ([key]) => key, Order.String);

// --- [DOM_SCRIPT] ----------------------------------------------------------------------

const _stringOf = (node: Node | undefined): Option.Option<string> => Option.map(Option.liftPredicate(node, Node.isStringLiteral), (literal) => literal.getLiteralValue());

const _numberOf = (node: Node | undefined): Option.Option<number> => Option.map(Option.liftPredicate(node, Node.isNumericLiteral), (literal) => literal.getLiteralValue());

const _owner = (node: Node | undefined): Option.Option<string> =>
    Option.map(
        Option.filter(Option.liftPredicate(node, Node.isPropertyAccessExpression), (access) => access.getName() === 'prototype'),
        (access) => access.getExpression().getText(),
    );

const _enumSpec = (node: Node, parameter: Option.Option<string>): Option.Option<string> =>
    Option.flatMap(
        Array.findFirst(
            node.getDescendantsOfKind(SyntaxKind.CallExpression),
            (call) =>
                call.getExpression().getText() === '_enumSpec' &&
                Option.match(parameter, { onNone: () => true, onSome: (name) => Option.exists(Array.get(call.getArguments(), 1), (argument) => argument.getText() === name) }),
        ),
        (call) => _stringOf(Array.head(call.getArguments()).pipe(Option.getOrUndefined)),
    );

const _assignment = (node: BinaryExpression): Option.Option<{ readonly left: Node; readonly right: Node }> =>
    Option.map(
        Option.liftPredicate(node, (binary) => binary.getOperatorToken().getText() === '='),
        (binary) => ({ left: binary.getLeft(), right: binary.getRight() }),
    );

const _field = (node: BinaryExpression): Option.Option<Slot> =>
    Option.flatMap(_assignment(node), ({ left, right }) => {
        const value = Node.isConditionalExpression(right) ? right.getWhenFalse() : right;
        return Option.map(
            Option.filter(Option.liftPredicate(left, Node.isPropertyAccessExpression), (access) => Node.isThisExpression(access.getExpression())),
            (access) => ({ name: access.getName(), access: 'readwrite' as const, enumeration: Option.none(), literal: Option.fromNullishOr(_LITERALS[value.getKind()]), parameters: [] }),
        );
    });

const _accessor = (call: CallExpression): Option.Option<readonly [string, Slot]> => {
    const [target, name, descriptor] = call.getArguments();
    const setter = Option.flatMap(Option.liftPredicate(descriptor, Node.isObjectLiteralExpression), (object) => Option.fromNullishOr(object.getProperty('set')));
    return Option.map(Option.all({ owner: _owner(target), name: _stringOf(name) }), ({ owner, name: slot }) => [
        owner,
        { name: slot, access: Option.isSome(setter) ? 'readwrite' : 'readonly', enumeration: Option.flatMap(setter, (node) => _enumSpec(node, Option.none())), literal: Option.none(), parameters: [] },
    ]);
};

const _method = (node: BinaryExpression): Option.Option<readonly [string, Slot]> =>
    pipe(
        _assignment(node),
        Option.flatMap(({ left, right }) =>
            Option.all({
                access: Option.filter(Option.liftPredicate(left, Node.isPropertyAccessExpression), (access) => !access.getName().startsWith('_')),
                body: Option.liftPredicate(right, Node.isFunctionExpression),
            }),
        ),
        Option.flatMap(({ access, body }) =>
            Option.map(_owner(access.getExpression()), (owner) => [
                owner,
                {
                    name: access.getName(),
                    access: 'method',
                    enumeration: Option.none(),
                    literal: Option.none(),
                    parameters: Array.map(body.getParameters(), (parameter) => ({ name: parameter.getName(), enumeration: _enumSpec(body, Option.some(parameter.getName())) })),
                },
            ]),
        ),
    );

const _table = (node: BinaryExpression, table: string): Option.Option<readonly [string, Node]> =>
    Option.flatMap(_assignment(node), ({ left, right }) =>
        Option.flatMap(
            Option.filter(Option.liftPredicate(left, Node.isElementAccessExpression), (access) => access.getExpression().getText() === table),
            (access) => Option.map(_stringOf(access.getArgumentExpression()), (name) => [name, right]),
        ),
    );

const _numbers = (node: Node): Readonly<Record<string, number>> =>
    Record.fromEntries(
        Array.filterMap(
            Option.match(Option.liftPredicate(node, Node.isObjectLiteralExpression), { onNone: () => [], onSome: (object) => object.getProperties() }),
            Filter.fromPredicateOption((property) =>
                Option.flatMap(Option.liftPredicate(property, Node.isPropertyAssignment), (assignment) =>
                    Option.all([_stringOf(assignment.getNameNode()), _numberOf(assignment.getInitializer())] as const),
                ),
            ),
        ),
    );

const _kind = (declaration: FunctionDeclaration): Option.Option<Kind> =>
    Option.flatMap(Array.head(declaration.getParameters()), (parameter) =>
        Match.value(parameter.getName()).pipe(
            Match.withReturnType<Kind>(),
            Match.when('props', () => 'creatable'),
            Match.when('specifier', () =>
                Array.some(declaration.getBodyOrThrow().getDescendantsOfKind(SyntaxKind.NewExpression), (node) => node.getExpression().getText() === 'Proxy') ? 'collection' : 'proxy',
            ),
            Match.option,
        ),
    );

const _domScript = (source: SourceFile): Pick<Sources, 'dom' | 'domEnumerations'> => {
    const assignments = source.getDescendantsOfKind(SyntaxKind.BinaryExpression);
    const declarations = Array.filterMap(
        source.getDescendantsOfKind(SyntaxKind.FunctionDeclaration),
        Filter.fromPredicateOption((declaration) => Option.map(_kind(declaration), (kind) => ({ name: declaration.getNameOrThrow(), kind, declaration }))),
    );
    const slots = Array.groupBy(
        [
            ...Array.filterMap(
                source.getDescendantsOfKind(SyntaxKind.CallExpression),
                Filter.fromPredicateOption((call) =>
                    Option.flatMap(
                        Option.liftPredicate(call, (candidate) => candidate.getExpression().getText() === 'Object.defineProperty'),
                        _accessor,
                    ),
                ),
            ),
            ...Array.filterMap(assignments, Filter.fromPredicateOption(_method)),
            ...Array.flatMap(declarations, (row) =>
                row.kind === 'creatable'
                    ? Array.map(
                          Array.filterMap(row.declaration.getBodyOrThrow().getDescendantsOfKind(SyntaxKind.BinaryExpression), Filter.fromPredicateOption(_field)),
                          (slot): readonly [string, Slot] => [row.name, slot],
                      )
                    : [],
            ),
        ],
        ([owner]) => owner,
    );
    const ids = Record.fromEntries(
        Array.filterMap(
            assignments,
            Filter.fromPredicateOption((node) => Option.flatMap(_table(node, '_enumTypeIds'), ([name, right]) => Option.map(_numberOf(right), (id) => [name, id] as const))),
        ),
    );
    return {
        dom: Record.fromIterableBy(
            Array.map(declarations, (row): DomClass => ({ name: row.name, kind: row.kind, slots: Array.map(Array.flatten(Option.toArray(Record.get(slots, row.name))), ([, slot]) => slot) })),
            Struct.get('name'),
        ),
        domEnumerations: Record.fromEntries(
            Array.filterMap(
                assignments,
                Filter.fromPredicateOption((node) =>
                    Option.map(_table(node, '_enumNameToId'), ([name, right]) => [name, { id: Option.getOrElse(Record.get(ids, name), () => 0), members: _numbers(right) }] as const),
                ),
            ),
        ),
    };
};

// --- [TYPINGS] -------------------------------------------------------------------------

const _typings = (source: SourceFile): Pick<Sources, 'known' | 'knownEnumerations'> => ({
    known: Record.fromIterableWith(source.getClasses(), (node) => [
        node.getNameOrThrow(),
        {
            parents: Array.map(Array.fromNullishOr(node.getExtends()), (parent) => parent.getText()),
            members: Array.map(node.getType().getProperties(), (symbol) => symbol.getName()),
            declared: Array.map([...node.getProperties(), ...node.getGetAccessors(), ...node.getMethods()], (member) => member.getName()),
            types: Record.fromEntries(
                Array.filterMap(
                    node.getProperties(),
                    Filter.fromPredicateOption((property) => Option.map(Option.liftPredicate(property.getStructure().type, Predicate.isString), (type) => [property.getName(), type] as const)),
                ),
            ),
        },
    ]),
    knownEnumerations: Record.fromIterableWith(source.getEnums(), (node) => [node.getName(), Array.map(node.getMembers(), (member) => member.getName())]),
});

// --- [JOINS] ---------------------------------------------------------------------------

const _sdefMembers = (classes: readonly SdefClass[], row: SdefClass): Readonly<Record<string, SdefProperty>> =>
    Record.union(
        Record.fromIterableBy(row.property, (property) => camel(property.attributes.name)),
        Option.match(
            Option.flatMap(row.attributes.inherits, (parent) => Array.findFirst(classes, (candidate) => candidate.attributes.name === parent)),
            {
                onNone: () => ({}),
                onSome: (parent) => _sdefMembers(classes, parent),
            },
        ),
        identity,
    );

const _joined = (dom: Sources['dom'], tables: Tables): Readonly<Record<string, Table>> => {
    const frequency = Record.map(
        Array.groupBy(
            Array.flatMap(Record.values(tables), (table) => Record.keys(table.members)),
            identity,
        ),
        Array.length,
    );
    const weight = (name: string): number => 1 / Option.getOrElse(Record.get(frequency, name), () => 1);
    return Record.filterMap(
        dom,
        Filter.fromPredicateOption((row) => {
            const names = Array.map(row.slots, Struct.get('name'));
            const ranked = pipe(
                Record.toEntries(tables),
                Array.map(([name, table]) => ({
                    table,
                    score: Array.reduce(Array.intersection(Record.keys(table.members), names), name === row.name ? 1 : 0, (total, member) => total + weight(member)),
                })),
                Array.sortWith(Struct.get('score'), Order.flip(Order.Number)),
            );
            return Option.map(
                Option.filter(Array.head(ranked), (best) => best.score > 0 && Option.exists(Array.get(ranked, 1), (next) => next.score < best.score)),
                Struct.get('table'),
            );
        }),
    );
};

const _names = (sources: Sources, tables: Tables, joined: Readonly<Record<string, Table>>, live: Live): Names => {
    const renamed = Record.fromEntries(
        Array.filterMap(
            Array.flatMap(Record.toEntries(live.classes), ([owner, record]) => Array.map(record.properties, (property) => ({ owner, property }))),
            Filter.fromPredicateOption(({ owner, property }) =>
                pipe(
                    Record.get(sources.known, owner),
                    Option.flatMap((row) => Record.get(row.types, property.name)),
                    Option.filter((text) => Record.has(sources.known, text)),
                    Option.map((text) => [property.dataType, text] as const),
                ),
            ),
        ),
    );
    const aliases = Record.filter(renamed, (text, name) => Record.has(sources.dom, name) && !Record.has(sources.known, name) && text !== name);
    const typings = (name: string): string => Option.getOrElse(Record.get(aliases, name), () => name);
    return {
        classes: Record.union(
            Record.fromIterableWith(Record.toEntries(joined), ([name, table]) => [table.sdef.attributes.name, typings(name)]),
            Record.fromIterableWith(
                Array.filter(Record.toEntries(tables), ([name]) => Record.has(sources.known, name)),
                ([name, table]) => [table.sdef.attributes.name, name],
            ),
            identity,
        ),
        enumerations: Record.fromEntries(
            Array.filterMap(
                Record.toEntries(sources.domEnumerations),
                Filter.fromPredicateOption(([name, row]) =>
                    Option.map(
                        Array.findFirst(sources.sdefEnumerations, (sdef) => fourcc(sdef.attributes.code) === row.id),
                        (sdef) => [sdef.attributes.code, name] as const,
                    ),
                ),
            ),
        ),
        typings,
    };
};

const _sdefType =
    (table: Readonly<Record<string, string>>) =>
    (property: SdefProperty): Option.Option<string> =>
        pipe(
            Option.map(property.attributes.type, (name) => ({ name, list: false })),
            Option.orElse(() => Option.map(Array.head(property.type), (row) => ({ name: row.attributes.type, list: Option.isSome(row.attributes.list) }))),
            Option.flatMap(({ name, list }) => Option.map(Record.get(table, name), (text) => (list ? `${text}[]` : text))),
        );

const _resolver =
    (sources: Sources, names: Names) =>
    (slot: Option.Option<Slot>, sdef: Option.Option<SdefProperty>, live: Option.Option<string>): string =>
        pipe(
            Option.flatMap(slot, Struct.get('enumeration')),
            Option.orElse(() => Option.flatMap(sdef, _sdefType(names.enumerations))),
            Option.orElse(() => Option.flatMap(sdef, _sdefType(names.classes))),
            Option.orElse(() => Option.flatMap(live, (dataType) => Record.get(_LIVE_TYPES, dataType))),
            Option.orElse(() =>
                Option.map(
                    Option.filter(live, (dataType) => Record.has(sources.dom, dataType) || Record.has(sources.known, dataType)),
                    names.typings,
                ),
            ),
            Option.orElse(() => Option.flatMap(sdef, _sdefType(_SDEF_TYPES))),
            Option.orElse(() => Option.flatMap(slot, Struct.get('literal'))),
            Option.getOrElse(() => 'unknown'),
        );

// --- [DELTA] ---------------------------------------------------------------------------

const _members = (resolve: ReturnType<typeof _resolver>, sdef: Option.Option<Table>, inherited: readonly string[], row: DomClass, live: Live['classes'][string]): readonly Member[] => {
    const sdefMembers = Option.map(sdef, Struct.get('members'));
    const parameters = Record.fromIterableWith(
        Array.filter(row.slots, (slot) => slot.access === 'method'),
        (slot) => [slot.name, Array.map(slot.parameters, (parameter) => ({ name: parameter.name, type: Option.getOrElse(parameter.enumeration, () => 'unknown') }))],
    );
    const listed: readonly { readonly name: string; readonly access: Access; readonly dataType: Option.Option<string> }[] = Array.isReadonlyArrayNonEmpty(live.properties)
        ? [
              ...Array.map(live.properties, (property) => ({
                  name: property.name,
                  access: property.type === 'readonly' ? ('readonly' as const) : ('readwrite' as const),
                  dataType: Option.some(property.dataType),
              })),
              ...Array.map(live.methods, (name) => ({ name, access: 'method' as const, dataType: Option.none<string>() })),
          ]
        : Array.map(
              Array.filter(Array.zip(row.slots, live.probed), ([, present]) => present),
              ([slot]) => ({ name: slot.name, access: slot.access, dataType: Option.none<string>() }),
          );
    return pipe(
        listed,
        Array.filter((member) => _IDENTIFIER.test(member.name) && !Array.contains(inherited, member.name)),
        Array.dedupeWith((left, right) => left.name === right.name),
        Array.map(
            (member): Member => ({
                name: member.name,
                access: member.access,
                type:
                    member.access === 'method'
                        ? 'unknown'
                        : resolve(
                              Array.findFirst(row.slots, (slot) => slot.name === member.name),
                              Option.flatMap(sdefMembers, Record.get(member.name)),
                              member.dataType,
                          ),
                parameters: Option.getOrElse(Record.get(parameters, member.name), () => (member.access === 'method' ? [{ name: 'args', type: 'unknown[]' }] : [])),
            }),
        ),
    );
};

const _lifted = (
    sources: Sources,
    tables: Tables,
    joined: Readonly<Record<string, Table>>,
    names: Names,
    reached: Readonly<Record<string, readonly Member[]>>,
): Readonly<Record<string, readonly Member[]>> =>
    Record.filterMap(sources.known, (base, name) => {
        const children = Record.values(Record.filter(reached, (_, child) => Option.exists(Record.get(sources.known, names.typings(child)), (row) => Array.contains(row.parents, name))));
        const sdefBase = Option.map(
            Option.orElse(Record.get(joined, name), () => Record.get(tables, name)),
            Struct.get('members'),
        );
        return Result.fromOption(
            Option.flatMap(
                Option.filter(_nonEmpty(children), (rows) => rows.length > 1),
                (rows) =>
                    _nonEmpty(
                        Array.filter(
                            Array.reduce(
                                rows,
                                Array.headNonEmpty(rows),
                                Array.intersectionWith<Member>((left, right) => left.name === right.name && left.type === right.type),
                            ),
                            (row) => !Array.contains(base.members, row.name) && Option.exists(sdefBase, Record.has(row.name)),
                        ),
                    ),
            ),
            Function.constVoid,
        );
    });

const _declaration = (name: string, delta: Delta): StatementStructures => {
    const properties = Array.map(delta.members, (member) => ({
        name: member.name,
        type:
            member.access === 'method'
                ? `(${Array.join(
                      Array.map(member.parameters, (parameter) => `${parameter.name === 'args' ? '...' : ''}${parameter.name}${parameter.name === 'args' ? '' : '?'}: ${parameter.type}`),
                      ', ',
                  )}) => ${member.type}`
                : member.type,
        isReadonly: member.access !== 'readwrite',
    }));
    return delta.declared ? { kind: StructureKind.Interface, name, properties } : { kind: StructureKind.Class, name, properties };
};

// --- [GENERATE] ------------------------------------------------------------------------

const _sources = Effect.fnUntraced(function* (bundlePath: string, project: Project) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const framework = path.join(bundlePath, ..._FRAMEWORK);
    const sdef = yield* Effect.flatMap(fs.readFileString(path.join(bundlePath, ..._DICTIONARY)), dictionary);
    const listing = yield* Effect.orDie(reply(ChildProcess.make('otool', ['-l', framework])));
    const section = yield* Effect.flatMap(
        Effect.fromOption(Option.flatMapNullishOr(String.match(_SECTION)(listing), Struct.get('groups')), () => GenerateError.cases.sectionNotFound.make({ framework })),
        _section,
    );
    const script = yield* fs.readFile(framework).pipe(
        Effect.map((bytes) => Array.filter(String.split(new TextDecoder().decode(bytes.subarray(section.offset, section.offset + section.size)), '\0'), String.includes(_MODEL))),
        Effect.filterOrFail(
            (found) => found.length === 1,
            (found) => GenerateError.cases.scriptNotFound.make({ count: found.length }),
        ),
        Effect.map(Array.join('')),
    );
    const typings = yield* Effect.flatMap(path.fromFileUrl(new URL(import.meta.resolve(_TYPINGS))), (file) => fs.readFileString(file));
    const sources: Sources = {
        sdefClasses: Array.filter(Array.flatMap(sdef.dictionary.suite, Struct.get('class')), (row) => Option.isNone(row.attributes.hidden)),
        sdefEnumerations: Array.filter(Array.flatMap(sdef.dictionary.suite, Struct.get('enumeration')), (row) => Option.isNone(row.attributes.hidden)),
        ..._domScript(project.createSourceFile('illustrator-dom.js', script)),
        ..._typings(project.createSourceFile('adobe.d.ts', typings)),
    };
    return sources;
});

const _live = Effect.fnUntraced(function* (at: Site, bundlePath: string, sources: Sources, candidates: Readonly<Record<string, readonly string[]>>) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    yield* Effect.forEach(yield* fs.readDirectory(at.scripts), (name) =>
        Effect.map(fs.readFileString(path.join(at.scripts, name)), (text) => Option.map(String.match(_HELPER)(text), (match) => [name, match[0]] as const)),
    ).pipe(
        Effect.map(Array.getSomes),
        Effect.map(Record.fromEntries),
        Effect.filterOrFail(Record.isEmptyReadonlyRecord, (helpers) => GenerateError.cases.loweringHelper.make({ helpers })),
    );
    return yield* dispatch(
        at,
        _TIMEOUT_MS,
        'reconcile',
        'generate',
        {
            image: path.join(bundlePath, ..._IMAGE),
            creatable: Array.map(
                Array.filter(Record.values(sources.dom), (row) => row.kind === 'creatable'),
                Struct.get('name'),
            ),
            classes: Record.map(sources.dom, (row) => Array.map(row.slots, Struct.get('name'))),
            enumerations: Array.map(Record.toEntries(candidates), ([name, members]) => ({ name, members })),
        },
        _Reconciliation,
    );
});

const generate: () => Effect.Effect<
    void,
    GenerateError | BridgeError | PlatformError.BadArgument | PlatformError.PlatformError | Schema.SchemaError,
    FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner
> = Effect.fnUntraced(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const at = yield* site;
    const scripts = path.resolve(import.meta.dirname, ..._SCRIPTS);
    const bundlePath = yield* Effect.mapError(bundle(_HOST), () => GenerateError.cases.bundleNotFound.make({ bundleId: _HOST.bundleId }));
    const project = new Project({ useInMemoryFileSystem: true, compilerOptions: { lib: ['lib.es5.d.ts'], types: [] }, manipulationSettings: MANIPULATION });
    const sources = yield* _sources(bundlePath, project);
    const tables: Tables = Record.fromIterableWith(sources.sdefClasses, (sdef) => [pascal(sdef.attributes.name), { sdef, members: _sdefMembers(sources.sdefClasses, sdef) }]);
    const candidates = pipe(
        Record.map(sources.domEnumerations, (row) => Record.keys(row.members)),
        Record.union(sources.knownEnumerations, (mine, theirs) => [...mine, ...theirs]),
        Record.map(sorted),
    );
    const live = yield* _live(at, bundlePath, sources, candidates);
    const joined = _joined(sources.dom, tables);
    const names = _names(sources, tables, joined, live);
    const resolve = _resolver(sources, names);
    const reached = Record.filterMap(
        sources.dom,
        Filter.fromPredicateOption((row) => Option.map(Record.get(live.classes, row.name), (record) => _members(resolve, Record.get(joined, row.name), live.inherited, row, record))),
    );
    const lifted = _lifted(sources, tables, joined, names, reached);
    const liftedNames = pipe(Record.values(lifted), Array.flatten, Array.map(Struct.get('name')), Array.dedupe);
    const deltas: Readonly<Record<string, Delta>> = Record.union(
        Record.map(lifted, (members): Delta => ({ declared: true, members })),
        Record.fromEntries(
            Array.filterMap(
                Record.toEntries(reached),
                Filter.fromPredicateOption(([name, members]) => {
                    const typed = names.typings(name);
                    const known = Record.get(sources.known, typed);
                    const excluded = Option.match(known, { onNone: () => [], onSome: (row) => [...row.members, ...liftedNames] });
                    return Option.map(_nonEmpty(Array.filter(members, (member) => !Array.contains(excluded, member.name))), (rows): readonly [string, Delta] => [
                        typed,
                        { declared: Option.isSome(known), members: rows },
                    ]);
                }),
            ),
        ),
        (lift, own) => ({ declared: true, members: Array.appendAll(lift.members, own.members) }),
    );
    const confirmedEnumerations = Record.map(Record.fromEntries(Array.flatMap(live.enumerations, Record.toEntries)), (flags, name) =>
        pipe(
            Record.get(candidates, name),
            Option.getOrElse(() => []),
            Array.zip(flags),
            Array.filter(([, confirmed]) => confirmed),
            Array.map(([member]) => member),
        ),
    );
    const enumerationDeltas = absent(confirmedEnumerations, sources.knownEnumerations);
    const delta = project.createSourceFile('illustrator.ts', {
        statements: [
            {
                kind: StructureKind.Module,
                name: 'global',
                declarationKind: ModuleDeclarationKind.Global,
                hasDeclareKeyword: true,
                statements: [
                    ...Array.map(
                        _byKey(enumerationDeltas),
                        ([name, members]): StatementStructures => ({ kind: StructureKind.Enum, name, members: Array.map(members, (member) => ({ name: member })) }),
                    ),
                    ...Array.map(_byKey(deltas), ([name, row]) => _declaration(name, row)),
                ],
            },
        ],
    });
    const table = project.createSourceFile('dictionary.ts', {
        statements: [
            `const classes = ${JSON.stringify(
                Record.fromIterableWith(_byKey(reached), ([name, members]) => [
                    name,
                    Record.fromIterableWith(Array.sortWith(members, Struct.get('name'), Order.String), (member) => [member.name, { access: member.access, type: member.type }]),
                ]),
                null,
                4,
            )} as const;`,
            `const enumerations = ${JSON.stringify(Record.fromEntries(_byKey(confirmedEnumerations)), null, 4)} as const;`,
            'export { classes, enumerations };',
        ],
    });
    yield* fs.writeFileString(path.join(scripts, 'illustrator.ts'), delta.getFullText());
    yield* fs.writeFileString(path.join(import.meta.dirname, 'dictionary.ts'), table.getFullText());
    const domNames = Record.keys(sources.dom);
    const domClasses = Record.values(sources.dom);
    const domEnumerationNames = Record.keys(sources.domEnumerations);
    const liveNames = Record.keys(live.classes);
    const reachedNames = Record.keys(reached);
    const declared = (name: string): readonly string[] =>
        Option.match(Record.get(sources.known, name), { onNone: () => [], onSome: (row) => [...row.declared, ...Array.flatMap(row.parents, declared)] });
    const refuted = Record.filterMap(live.classes, (record, name) => {
        const typed = names.typings(name);
        return Result.fromOption(
            Option.flatMap(
                Option.filter(_nonEmpty(record.properties), () => Record.has(sources.known, typed)),
                (properties) => _nonEmpty(Array.difference(declared(typed), [...Array.map(properties, Struct.get('name')), ...record.methods, ...live.inherited])),
            ),
            Function.constVoid,
        );
    });
    yield* Console.log(
        JSON.stringify(
            {
                sdef: {
                    classes: sources.sdefClasses.length,
                    enumerations: sources.sdefEnumerations.length,
                    properties: Array.flatMap(sources.sdefClasses, Struct.get('property')).length,
                    enumerators: Array.flatMap(sources.sdefEnumerations, Struct.get('enumerator')).length,
                },
                script: {
                    classes: domNames.length,
                    kinds: Record.fromIterableWith(Kind.literals, (kind) => [kind, Array.countBy(domClasses, (row) => row.kind === kind)]),
                    members: Array.flatMap(domClasses, Struct.get('slots')).length,
                    enumerations: domEnumerationNames.length,
                    joined: Record.size(joined),
                    unjoined: sorted(Array.difference(domNames, Record.keys(joined))),
                    enumerationsUnjoined: sorted(Array.difference(domEnumerationNames, Record.values(names.enumerations))),
                },
                typings: {
                    classes: Record.size(sources.known),
                    enumerations: Record.size(sources.knownEnumerations),
                    renamed: Record.fromIterableWith(
                        Array.filter(reachedNames, (name) => names.typings(name) !== name),
                        (name) => [name, names.typings(name)],
                    ),
                    outsideTypings: sorted(Array.filter(reachedNames, (name) => !Record.has(sources.known, names.typings(name)))),
                },
                live: {
                    reached: reachedNames.length,
                    unreached: sorted(Array.difference(domNames, liveNames)),
                    outsideScript: sorted(Array.filter(Array.difference(liveNames, domNames), (name) => _IDENTIFIER.test(name))),
                    members: Array.flatten(Record.values(reached)).length,
                    rejectedMembers: Record.filterMap(
                        sources.dom,
                        Filter.fromPredicateOption((row) =>
                            Option.flatMap(Record.get(reached, row.name), (members) => _nonEmpty(Array.difference(Array.map(row.slots, Struct.get('name')), Array.map(members, Struct.get('name'))))),
                        ),
                    ),
                    refutedTypings: refuted,
                    enumerations: Record.size(confirmedEnumerations),
                    enumerationsAbsent: sorted(Array.difference(Array.union(domEnumerationNames, Record.keys(sources.knownEnumerations)), Record.keys(confirmedEnumerations))),
                    enumerators: Array.flatten(Record.values(confirmedEnumerations)).length,
                    enumeratorsRefuted: absent(
                        Record.filter(sources.knownEnumerations, (_, name) => Record.has(confirmedEnumerations, name)),
                        confirmedEnumerations,
                    ),
                    globalsOutsideScript: sorted(
                        Array.difference(
                            Array.flatMap(live.globals, (row) => Record.keys(Record.filter(row, identity))),
                            domEnumerationNames,
                        ),
                    ),
                    unavailable: live.unavailable,
                    created: live.created,
                },
                delta: {
                    interfaces: Record.size(Record.filter(deltas, Struct.get('declared'))),
                    classes: Record.size(Record.filter(deltas, (row) => !row.declared)),
                    lifted: Record.map(lifted, Array.map(Struct.get('name'))),
                    members: Array.flatMap(Record.values(deltas), Struct.get('members')).length,
                    enumerations: Record.size(enumerationDeltas),
                    enumerators: Array.flatten(Record.values(enumerationDeltas)).length,
                },
            },
            null,
            4,
        ),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { generate };
