import type { BaseCollection, Document, Enumeration, Enumerator, InDesignDOMObject, Page, PageItem, ReadonlyNamedDOMObject, Spread, Text, TextFrame } from 'adobe:indesign';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cache, Context, Effect, flow, Option, Predicate, pipe, Random, Record, Schema, Struct } from 'effect';
import { type Member, READER, Rows, unavailable } from '#/dom/dictionary';
import { InDesignError } from '#/error';

// --- [SERVICES] ------------------------------------------------------------------------

class Model extends Context.Service<Model>()('@rasm/indesign/dom/model', {
    make: Effect.fnUntraced(function* (indesign: typeof import('adobe:indesign')) {
        const model: typeof indesign = Object.create(indesign);
        const exported = Array.map(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)] as const);
        const classes = Array.flatMap(exported, ([name, value]) => (Predicate.isFunction(value) ? [name] : []));
        const enumerations = Array.flatMap(exported, ([name, value]) => (value instanceof indesign.Enumeration ? [name] : []));
        const enumeration = (name: string): Option.Option<Enumeration> => Option.liftPredicate(Reflect.get(indesign, name), (value): value is Enumeration => value instanceof indesign.Enumeration);
        const enumerator = (value: unknown): value is Enumerator => value instanceof indesign.Enumerator;
        const members = <E extends Enumeration>(found: E) => {
            const keyed = (member: unknown, key: string): member is Extract<E[keyof E], Enumerator> => enumerator(member) && String(member) === key;
            return Array.flatMap(Object.getOwnPropertyNames(Object.getPrototypeOf(found)), (key) => {
                const member: unknown = Reflect.get(found, key);
                return keyed(member, key) ? [member] : [];
            });
        };
        const owning = <A>(member: keyof A & string) => {
            const owners = new Set(Array.flatMap(exported, ([, type]) => (Predicate.isFunction(type) && Object.hasOwn(type.prototype, member) ? [type.prototype] : [])));
            return <T>(value: T): value is T & A => Predicate.isObject(value) && owners.has(Object.getPrototypeOf(value));
        };
        const host = owning<InDesignDOMObject<unknown>>('toSpecifier');
        const document = owning<Document>('createTOC');
        const ancestors = (object: unknown): readonly InDesignDOMObject<unknown>[] => Array.unfold(object, (above: unknown) => (host(above) && above.constructorName !== 'Application' ? Option.some([above, above.parent] as const) : Option.none()));
        const listing = (kind: string): boolean => {
            const found: unknown = Reflect.get(indesign, kind);
            return Predicate.isFunction(found) && Object.hasOwn(found.prototype, 'everyItem');
        };
        const plural = ({ type }: Pick<typeof Member.Type, 'type'>): boolean => Option.exists(type, listing);
        const filled = <R extends { readonly types: readonly string[] }>(row: R) => ({
            ...row,
            members: pipe(
                row.types,
                Array.map(enumeration),
                Array.getSomes,
                Array.flatMap((found) => Array.map(members(found), String)),
                Array.dedupe,
                Option.liftPredicate(Array.isArrayNonEmpty),
            ),
        });
        const rows = yield* Cache.make({
            capacity: exported.length,
            lookup: (kind: string) =>
                Effect.try({ try: (): unknown => indesign.app.doScript(READER, indesign.ScriptLanguage.JAVASCRIPT, [kind]), catch: InDesignError.refused(`dictionary class ${kind}`) }).pipe(
                    Effect.flatMap(flow(Schema.decodeUnknownEffect(Schema.NullOr(Rows)), Effect.orDie)),
                    Effect.filterOrFail(Predicate.isNotNull, () => Unmatched.make({ subject: 'class', query: Option.some(kind), candidates: classes })),
                    Effect.map(({ fields, methods, ...declared }) => ({
                        ...declared,
                        fields: Record.map(fields, (row) => ({ ...filled(row), unavailable: unavailable(kind, row.name, Option.none(), false) })),
                        methods: Record.map(methods, (method) => ({ ...method, parameters: Array.map(method.parameters, filled) })),
                    })),
                ),
        });
        const lineage = (name: string): Effect.Effect<readonly string[], Unmatched | InDesignError> => Effect.flatMap(Cache.get(rows, name), ({ parent }) => Option.match(parent, { onNone: () => Effect.succeed([]), onSome: (above) => Effect.map(lineage(above), (rest) => [above, ...rest]) }));
        const element = (kind: string) => Effect.map(Cache.get(rows, kind), ({ methods }) => Option.flatMap(Record.get(methods, 'item'), ({ returns }) => Option.flatMap(returns, Struct.get('type'))));
        const catalog = yield* Cache.make({
            capacity: exported.length,
            lookup: Effect.fnUntraced(function* (kind: string) {
                const { fields } = yield* Cache.get(rows, 'Document');
                const listed = Array.findFirst(Record.toEntries(fields), ([, { array, type }]) => Option.isSome(array) && Option.contains(type, kind));
                const held = pipe(
                    Record.map(fields, ({ type }) => Option.filter(type, listing)),
                    Record.getSomes,
                    Record.toEntries,
                    Effect.findFirst(([, each]) => Effect.map(element(each), Option.contains(kind))),
                    Effect.map(Option.map(([field]) => field)),
                );
                return yield* Option.match(listed, { onSome: ([field]) => Effect.succeedSome(field), onNone: () => held });
            }),
        });
        return Object.assign(model, {
            exported,
            classes,
            enumerations,
            owning,
            host,
            collection: owning<BaseCollection<unknown, unknown, InDesignDOMObject<unknown, 'plural'> & { readonly getElements: () => InDesignDOMObject<unknown>[] }>>('everyItem'),
            named: owning<ReadonlyNamedDOMObject<unknown>>('name'),
            document,
            page: owning<Page>('documentOffset'),
            spread: owning<Spread>('spreadHidden'),
            item: owning<PageItem>('itemLayer'),
            text: owning<Text>('parentTextFrames'),
            frame: owning<TextFrame>('textFrameIndex'),
            ancestors,
            owner: (object: unknown): Option.Option<Document> => Array.findFirst(ancestors(object), document),
            enumeration,
            enumerator,
            members,
            plural,
            rows,
            lineage,
            catalog,
        });
    }),
}) {
    static readonly active = Model.use(({ app }) => Effect.try({ try: () => app.activeDocument, catch: () => Unmatched.make({ subject: 'active document', query: Option.none(), candidates: app.documents.everyItem().name }) }));
    static readonly doScript = <S extends Schema.Top>(schema: S, subject: string, source: string, ...given: readonly string[]) =>
        Model.use(({ app, ScriptLanguage }) => Effect.try({ try: (): unknown => app.doScript(source, ScriptLanguage.JAVASCRIPT, [...given]), catch: InDesignError.refused(subject) })).pipe(Effect.flatMap(flow(Schema.decodeUnknownEffect(schema), Effect.orDie)));
    static readonly file = (owner: InDesignDOMObject<unknown>, field: string) => Model.doScript(Schema.OptionFromNullOr(Schema.String), `${owner.toSpecifier()} ${field}`, 'var value = resolve(arguments[0])[arguments[1]]; value === null ? null : value.fsName', owner.toSpecifier(), field);
    static readonly temporary = (name: string) =>
        Effect.acquireRelease(
            Effect.zipWith(Model.doScript(Schema.String, `temporary ${name}`, 'Folder.temp.fsName'), Random.nextInt, (folder, unique) => `${folder}/${unique}-${name}`),
            (path) => Effect.orDie(Model.doScript(Schema.Boolean, `removal of ${path}`, 'File(arguments[0]).remove()', path)),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Model };
