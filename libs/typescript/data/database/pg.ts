import { PgClient, PgTypes } from '@effect/sql-pg';
import { traverse } from '@pgsql/traverse';
import type { Node } from '@pgsql/types';
import { Array, Config, Duration, Effect, Equal, Equivalence, Exit, Filter, Formatter, Function, HashSet, Layer, Option, Record, Result, Schema, Struct } from 'effect';
import { SqlClient, SqlSchema, Statement } from 'effect/sql';
import { deparseSync, loadModule, parseSync } from 'pgsql-parser';
import type { Catalog } from '#/catalog';
import type { Kind } from '#/column';
import { type Dialect, Engine, Rows } from '#/engine';
import { constant, diff, type Identity, identifier, literal, mergeTreeRefusals, methods, type referentialActions, renameTable, render, Stamp, unsupported } from '#/reconcile';
import { Tokenizer } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

type Matched = ReturnType<typeof diff<PgTable>>['matched'][number];
interface Storage {
    readonly type: string;
    readonly extract: '#>' | '#>>';
    readonly element: (sql: SqlClient.SqlClient, text: Statement.Fragment) => Statement.Fragment;
    readonly domain: (sql: SqlClient.SqlClient, subject: Statement.Fragment) => readonly Statement.Fragment[];
    readonly constant: Dialect['constant'];
}
interface Planning {
    readonly sql: SqlClient.SqlClient;
    readonly role: Statement.Fragment;
    readonly grantees: readonly { readonly grantee: string; readonly all: readonly string[] }[];
    readonly resolved: ReturnType<typeof diff<PgTable>>;
    readonly drifted: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const trigram = { extension: 'pg_trgm', classes: { gin: 'gin_trgm_ops', gist: 'gist_trgm_ops' } } as const;
const confdeltype = { 'NO ACTION': 'a', RESTRICT: 'r', CASCADE: 'c', 'SET NULL': 'n', 'SET DEFAULT': 'd' } as const satisfies { readonly [A in (typeof referentialActions)[number]]: string };
const privileges = { runtime: ['SELECT', 'INSERT', 'UPDATE', 'DELETE'], reader: ['SELECT'] } as const;
const stored = (type: string): Storage => ({ type, extract: '#>>', element: (sql, text) => sql`(${text})::${sql.literal(type)}`, domain: () => [], constant });
const bounded: Storage['domain'] = (sql, subject) => [sql`${subject} NOT IN ('NaN', 'Infinity', '-Infinity')`];
const kinds = {
    text: stored('text'),
    uuid: stored('uuid'),
    json: { ...stored('jsonb'), extract: '#>' },
    instant: { ...stored('timestamptz(3)'), domain: (sql, subject) => [sql`isfinite(${subject})`] },
    integer: stored('bigint'),
    decimal: { ...stored('numeric'), domain: bounded },
    real: { ...stored('double precision'), domain: bounded },
    boolean: { ...stored('boolean'), constant: (_, value) => `${value === 1}` },
    blob: { ...stored('bytea'), element: (sql, text) => sql`decode(${text}, 'hex')` },
} as const satisfies { readonly [K in Kind]: Storage };
const ordinary = Record.map(kinds, () => Option.none<{ readonly name: string; readonly extensions: readonly string[] }>());
const opclasses: { readonly [M in (typeof methods)[number]]: { readonly [K in Kind]?: (typeof ordinary)[K] } } = {
    btree: ordinary,
    gin: { text: Option.some({ name: trigram.classes.gin, extensions: [trigram.extension] }), json: Option.some({ name: 'jsonb_path_ops', extensions: [] }) },
    gist: { text: Option.some({ name: trigram.classes.gist, extensions: [trigram.extension] }) },
    brin: Struct.omit(ordinary, ['json', 'boolean']),
};

// --- [MODELS] --------------------------------------------------------------------------

const Names = Schema.Array(Schema.String);
const Role = Schema.Struct({ role: Schema.String });
const Configuration = Schema.Struct({ name: Schema.String, configuration: Schema.String, filters: Names });
const Prelude = Schema.Struct({ required: Names, filters: Names, declared: Schema.fromJsonString(Schema.Array(Configuration)) });
const Configurations = Schema.Struct({
    migrator: Schema.String,
    absent: Names,
    configurations: Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, configuration: Schema.String, configured: Schema.Boolean, drift: Schema.Array(Schema.Struct({ tokens: Schema.NonEmptyArray(Schema.String), dictionaries: Schema.OptionFromNullOr(Schema.NonEmptyArray(Schema.String)) })) }))),
});
class PgAttribute extends Schema.Class<PgAttribute, { readonly brand: unique symbol }>('PgAttribute')({
    name: Schema.String,
    comment: Schema.OptionFromNullOr(Schema.String),
    type: Schema.String,
    textual: Schema.Boolean,
    expression: Schema.OptionFromNullOr(Schema.String),
    generated: Schema.OptionFromNullOr(Schema.Literals(['STORED', 'VIRTUAL'])),
    required: Schema.Boolean,
    sources: Names,
}) {
    get identity() {
        return Option.flatMap(this.comment, Schema.decodeOption(Stamp));
    }
    derives(wants: PgAttribute) {
        return Option.isSome(Option.all([this.generated, wants.generated]));
    }
    differs(wants: PgAttribute) {
        return { type: this.type !== wants.type, required: this.required !== wants.required, generated: !Equal.equals(this.generated, wants.generated), expression: !Equal.equals(this.expression, wants.expression) };
    }
    declared(nullable: boolean) {
        const [derivation, preset] = [Option.all([this.generated, this.expression]), Option.filter(this.expression, () => Option.isNone(this.generated))];
        return [this.type, ...Option.toArray(derivation).map(([storage, expression]) => `GENERATED ALWAYS AS (${expression}) ${storage}`), ...(nullable ? [] : ['NOT NULL']), ...Option.toArray(preset).map((expression) => `DEFAULT ${expression}`)].join(' ');
    }
}
class PgTable extends Schema.Class<PgTable, { readonly brand: unique symbol }>('PgTable')({
    name: Schema.String,
    comment: Schema.OptionFromNullOr(Schema.String),
    rls: Schema.Boolean,
    force: Schema.Boolean,
    grants: Schema.Record(Schema.String, Names),
    columns: Schema.Array(PgAttribute),
    checks: Schema.Array(Schema.Struct({ name: Schema.String, definition: Schema.String, columns: Names })),
    references: Schema.Array(Schema.Struct({ name: Schema.String, columns: Schema.NonEmptyArray(Schema.String), target: Schema.String, onDelete: Schema.Literals(Object.values(confdeltype)).transform(Record.keys(confdeltype)) })),
    invalid: Names,
    unenforced: Names,
    indexes: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            valid: Schema.Boolean,
            definition: Schema.String,
            columns: Names,
            shape: Schema.Struct({ unique: Schema.Boolean, method: Schema.String, width: Schema.Int, keys: Names, classes: Names, predicate: Schema.NullOr(Schema.String) }),
        }),
    ),
    policies: Schema.Array(Schema.Struct({ name: Schema.String, using: Schema.NullOr(Schema.String), check: Schema.NullOr(Schema.String), roles: Names })),
}) {
    static readonly #reference: Node = { ColumnRef: { fields: [{ String: { sval: '$column' } }] } };
    static readonly #named = Function.memoize((table: PgTable) => Record.fromIterableBy(table.columns, Struct.get('name')));
    get identity() {
        return Option.flatMap(this.comment, Schema.decodeOption(Stamp));
    }
    get named() {
        return PgTable.#named(this);
    }
    checked(column: string) {
        return HashSet.fromIterable(
            this.checks
                .filter(({ columns }) => Equal.equals(columns, [column]))
                .map(({ definition }) => {
                    const parsed = parseSync(`ALTER TABLE _ ADD ${definition}`);
                    traverse(parsed, { ColumnRef: (ref) => (Option.contains(Array.last<Node>(ref.node.fields), { String: { sval: column } }) ? ref.replaceWith(PgTable.#reference) : undefined) });
                    return deparseSync(parsed);
                }),
        );
    }
    drifted(live: PgTable, now: PgAttribute, { definition, recorded }: Matched['fields'][number]) {
        return Option.exists(Record.get(this.named, definition.name), (wants) => {
            const [differs, hashed] = [now.differs(wants), Option.flatMapNullishOr(recorded, Struct.get('digest'))];
            const defined = Option.match(hashed, { onNone: () => !Equal.equals(live.checked(now.name), this.checked(definition.name)), onSome: (digest) => Option.isNone(definition.projection) && digest !== definition.digest });
            return !now.derives(wants) && (differs.type || differs.required || differs.generated || (Option.isNone(wants.generated) && differs.expression) || defined);
        });
    }
}
class Pg extends Schema.Class<Pg, { readonly brand: unique symbol }>('Pg')({ tables: Schema.Array(PgTable), scratch: Schema.Array(PgTable) }) {
    // --- [ROWS]

    static readonly member: Rows['Service']['member'] = (sql, field) => sql`e.value ->> ${sql.literal(literal(field))}`;
    static readonly element: Rows['Service']['element'] = (sql, { kind }, text) => kinds[kind].element(sql, text);
    static readonly texts: Rows['Service']['texts'] = (sql, body) => sql`jsonb_array_elements_text(${body}) WITH ORDINALITY e(value, position)`;
    static readonly insertion: Rows['Service']['insertion'] = (sql, body, order) => sql.join(' ', false)([sql`jsonb_array_elements(${body}) e(value)`, 'ORDER BY', order]);
    static readonly read: Rows['Service']['read'] = (sql, { name, textual }, scope) => (textual ? sql`${scope}${sql(name)}::text AS ${sql(name)}` : sql`${scope}${sql(name)}`);
    static readonly upsert: Rows['Service']['upsert'] = ({ sql, owner, unqualified, ignore, keys, reads, inserting, flags }, body, set) => {
        const parts = set.map(({ name }) => sql`${sql(name)}`);
        const [assigned, held, offered] = [sql.csv(parts.map((part) => sql`${part} = excluded.${part}`)), sql.csv(parts.map((part) => sql`${owner}.${part}`)), sql.csv(parts.map((part) => sql`excluded.${part}`))];
        const [update, inserted] = [sql`ON CONFLICT (${keys(unqualified)}) DO UPDATE SET ${assigned} WHERE (${held}) IS DISTINCT FROM (${offered})`, sql`${sql(flags.previous)} IS NULL AS ${sql(flags.inserted)}`];
        return inserting(body, Array.isReadonlyArrayNonEmpty(parts) ? update : ignore, sql`WITH (OLD AS ${sql(flags.previous)}) ${reads(unqualified, inserted)}`);
    };
    static readonly ensure: Rows['Service']['ensure'] = ({ sql, unqualified, keys, reads, inserting }, body) => {
        const [outer, held] = [sql.literal('t.'), inserting(body, sql`ON CONFLICT (${keys(unqualified)}) DO SELECT`, sql.literal('*'))];
        return sql`WITH held AS (${held}) SELECT DISTINCT ON (${keys(outer)}) ${reads(outer)} FROM held t ORDER BY ${keys(outer)}`;
    };
    static readonly atomic = Engine.transaction;
    static readonly configuration = ({ configuration, filters }: Pick<typeof Configuration.Type, 'configuration' | 'filters'>) => identifier([configuration, ...filters]);
    static readonly document = (sql: SqlClient.SqlClient, { columns, tokenizer }: Parameters<Dialect['fulltext']>[2]) => {
        const joined = sql.join(` || ' ' || `, false)(columns.map(({ name }) => sql`coalesce(${sql(name)}, '')`));
        return Tokenizer.$match(tokenizer, { Trigram: () => joined, Words: (words) => sql`to_tsvector(${sql.literal(literal(Pg.configuration(words)))}, ${joined})` });
    };
    static readonly #search = (extension: Effect.Effect<string, Effect.Error<ReturnType<Rows['Service']['search']>>, SqlClient.SqlClient>): Rows['Service']['search'] =>
        Effect.fnUntraced(function* (...[{ sql, owner, prefix, unqualified, keys, reads, flags }, searchable, text, pattern, marks, count]: Parameters<Rows['Service']['search']>) {
            const [body, rank] = [Pg.document(sql, searchable), sql(flags.rank)];
            const { score, source, filter, mark } = yield* Tokenizer.$match(searchable.tokenizer, {
                Trigram: () =>
                    Effect.map(Effect.provideService(extension, SqlClient.SqlClient, sql), (namespace) => ({
                        score: sql`${sql.literal(namespace)}.word_similarity(${text}, ${body})`,
                        source: owner,
                        filter: sql`(${body}) ILIKE ${pattern}`,
                        mark: (part: string, open: string, close: string) => sql`regexp_replace(${sql(part)}, ${['***=', text].join('')}, ${[open, close].map((marker) => marker.replaceAll('\\', '\\\\')).join('\\&')}, 'gi')`,
                    })),
                Words: (words) => {
                    const dictionary = sql.literal(literal(Pg.configuration(words)));
                    return Effect.succeed({
                        score: sql`ts_rank(${body}, q)`,
                        source: sql`${owner} CROSS JOIN plainto_tsquery(${dictionary}, ${text}) q`,
                        filter: sql`${body} @@ q`,
                        mark: (part: string, open: string, close: string) => sql`ts_headline(${dictionary}, ${sql(part)}, q, ${['StartSel="', open.replaceAll('"', '""'), '", StopSel="', close.replaceAll('"', '""'), '", HighlightAll=true'].join('')})`,
                    });
                },
            });
            const marked = Option.match(marks, { onNone: () => sql.literal('NULL'), onSome: ({ open, close }) => sql`jsonb_build_object(${sql.csv(searchable.columns.flatMap((entry) => [literal(entry.name), mark(entry.name, open, close)]))})` });
            return yield* sql`SELECT ${reads(unqualified)}, ${score} AS ${rank}, ${marked} AS ${sql(flags.highlight)} FROM ${source} WHERE ${filter} ORDER BY ${rank} DESC, ${keys(prefix)} LIMIT ${count}`;
        });

    // --- [DDL]

    static readonly type: Dialect['type'] = ({ kind }) => kinds[kind].type;
    static readonly constant: Dialect['constant'] = (entry, value) => kinds[entry.kind].constant(entry, value);
    static readonly codePoints: Dialect['codePoints'] = (sql, subject) => sql`char_length(${subject})`;
    static readonly cased: Dialect['cased'] = (sql, subject, _, casing) => sql`${sql.literal(casing)}(${subject}::text) = ${subject}::text`;
    static readonly domain: Dialect['domain'] = (sql, subject, kind) => kinds[kind].domain(sql, subject);
    static readonly projection: Dialect['projection'] = (sql, { kind }, { from, path }) => sql`(${sql(from)} ${sql.literal(kinds[kind].extract)} ARRAY[${sql.literal(path.map(literal).join(', '))}])::${sql.literal(kinds[kind].type)}`;
    static readonly generated: Dialect['generated'] = (sql, expression) => sql`GENERATED ALWAYS AS (${expression}) VIRTUAL`;
    static readonly constraint: Dialect['constraint'] = (sql, name, text) => sql`CONSTRAINT ${sql(name)} ${text}`;
    static readonly casefold: Dialect['casefold'] = (sql, name) => sql`casefold(${sql(name)} COLLATE pg_unicode_fast)`;
    static readonly opclass: Dialect['opclass'] = (method, kind) => Option.fromUndefinedOr(opclasses[method][kind]);
    static readonly projected: Dialect['projected'] = (sql, expression) => sql`(${expression})`;
    static readonly methods: Dialect['methods'] = methods;
    static readonly covering: Dialect['covering'] = (sql, _, include) => ({ keys: [], clause: Array.isReadonlyArrayNonEmpty(include) ? [sql`INCLUDE (${sql.csv(include.map((name) => Statement.fragment([sql(name)])))})`] : [] });
    static readonly suffix: Dialect['suffix'] = (sql) => sql.literal('');
    static readonly rename = renameTable;
    static readonly fulltext: Dialect['fulltext'] = (sql, owner, search) => {
        const [name, body] = [identifier([owner, 'search']), Pg.document(sql, search)];
        const words = Option.map(Option.liftPredicate(search.tokenizer, Tokenizer.$is('Words')), ({ configuration, filters }) => ({ name: Pg.configuration({ configuration, filters }), configuration, filters }));
        const key = Option.match(words, { onNone: () => sql`(${body}) ${sql.literal(trigram.classes.gin)}`, onSome: () => body });
        return {
            indexes: [
                {
                    name,
                    parts: ['search'],
                    extensions: Option.match(words, { onNone: () => [trigram.extension], onSome: () => [] }),
                    configuration: Option.filter(words, ({ filters }) => Array.isReadonlyArrayNonEmpty(filters)),
                    text: (modifiers, on) => sql`CREATE INDEX ${sql.join(' ', false)([...modifiers, Statement.fragment([sql(name)])])} ON ${on} USING gin (${key})`,
                },
            ],
            virtual: Option.none(),
        };
    };
    static readonly policy: Dialect['policy'] = (sql, owner, name, type, setting) => {
        const [policy, rule] = [identifier([owner, 'tenant']), sql`${sql(name)} = nullif(current_setting(${sql.literal(literal(setting))}, true), '')::${sql.literal(type)}`];
        return Option.some({ name: policy, parts: ['tenant'], rule, text: (on, role) => sql`CREATE POLICY ${sql(policy)} ON ${on} TO ${role} USING (${rule}) WITH CHECK (${rule})` });
    };
    static readonly #commented = (sql: SqlClient.SqlClient, relation: Statement.Fragment, column: Option.Option<string>, text: Option.Option<string>) => {
        const target = Option.match(column, { onNone: () => sql`TABLE ${relation}`, onSome: (name) => sql`COLUMN ${relation}.${sql(name)}` });
        return sql`COMMENT ON ${target} IS ${sql.literal(Option.match(text, { onNone: () => 'NULL', onSome: literal }))}`;
    };
    static readonly #stamped = (sql: SqlClient.SqlClient, relation: Statement.Fragment, column: Option.Option<string>, name: string, recorded: Option.Option<typeof Identity.Type>, identity: typeof Identity.Type) => {
        const text = Option.liftPredicate(Formatter.formatJson(identity), () => !Equal.equals(identity, { origin: name, generation: 0 }));
        return Option.contains(recorded, identity) || Option.isNone(Option.orElse(text, () => recorded)) ? [] : [Pg.#commented(sql, relation, column, text)];
    };
    static readonly comment: Dialect['comment'] = (sql, name, identity) => ({ inline: sql.literal(''), statements: (relation, column) => Pg.#stamped(sql, relation, column, name, Option.none(), identity) });

    // --- [RECONCILIATION]

    static readonly refusals: Dialect['refusals'] = (relation) => {
        const named = Record.fromIterableBy(Object.values(relation.columns), Struct.get('name'));
        const refused = relation.indexes.flatMap(({ keys, method }) => Array.getSomes(keys.map(({ column, casefold }) => Option.filter(Record.get(named, column), ({ kind }) => Option.isNone(Pg.opclass(method, casefold ? 'text' : kind))))));
        return [...unsupported(relation.name, 'opclass', Array.dedupe(refused), Function.constTrue), ...mergeTreeRefusals(relation)];
    };
    static readonly role: Dialect['role'] = (sql) => Effect.map(SqlSchema.findOne({ Request: Schema.Void, Result: Role, execute: () => sql`SELECT current_user AS role` })(undefined), Struct.get('role'));
    static readonly open: Dialect['open'] = () => Effect.void;
    static readonly execute = Engine.transaction;
    static readonly transactional = true;
    static readonly #block = (sql: SqlClient.SqlClient, statements: readonly Statement.Statement<object>[]) => sql`DO ${sql.literal(literal(['BEGIN', ...statements.map((statement) => [statement.compile(true)[0], ';'].join('')), 'END'].join(' ')))}`;
    static readonly #aligned = <W extends { readonly name: string; readonly parts: readonly string[] }, L extends { readonly name: string }>(wanted: readonly W[], live: readonly L[], prior: Matched['prior'], fits: (row: L, item: W) => boolean) => {
        const named = Record.fromIterableBy(live, ({ name }) => name);
        const placed = wanted.map((item) => {
            const [present, before] = [Record.get(named, item.name), Option.filter(Record.get(named, prior(item.parts)), ({ name }) => name !== item.name)];
            const kept = Option.exists(present, (row) => fits(row, item));
            const renamed = Option.filter(before, (row) => Option.isNone(present) && fits(row, item));
            const dropped = [...Option.toArray(present).filter(() => !kept), ...Option.toArray(before).filter(() => Option.isNone(renamed))];
            return { item, renamed: Option.map(renamed, ({ name }) => name), missing: !kept && Option.isNone(renamed), dropped: dropped.map(({ name }) => name) };
        });
        const claimed = new Set(wanted.flatMap((item) => [item.name, prior(item.parts)]));
        return { placed, foreign: live.filter(({ name }) => !claimed.has(name)) };
    };
    static readonly #constraints = ({ sql, resolved }: Planning, { declared, rendered, live, prior, renamed }: Matched) => {
        const checks = Array.getSomes(rendered.definitions.map(({ check }) => check));
        const labels = Pg.#aligned(checks, live.checks, prior, Function.constTrue);
        const links = Pg.#aligned(rendered.links, live.references, prior, (row, link) => Equal.equals({ columns: Array.map(row.columns, renamed), target: resolved.referenced(row.target).name, onDelete: row.onDelete }, Struct.pick(link, ['columns', 'target', 'onDelete'])));
        const renames = new Map(Array.getSomes([...labels.placed, ...links.placed].map(({ item, renamed: before }) => Option.map(before, (from) => [from, item.name] as const))));
        const relabeled = (constraint: string) => renames.get(constraint) ?? constraint;
        return {
            relabeled,
            relabels: Array.fromIterable(renames).map(([from, to]) => sql`ALTER TABLE ${rendered.relation} RENAME CONSTRAINT ${sql(from)} TO ${sql(to)}`),
            dropped: links.placed.flatMap(({ dropped }) => dropped),
            added: links.placed.filter(({ missing }) => missing).map(({ item }) => item),
            enforced: Array.intersection(live.unenforced.map(relabeled), [...checks, ...rendered.links].map(Struct.get('name'))),
            references: links.foreign.map(({ columns, target }) => ({ table: declared.name, columns: Array.map(columns, renamed), target: resolved.referenced(target).name })),
        };
    };
    static readonly #indexes = ({ sql, drifted }: Planning, { rendered, live, prior, renamed, retiredIndexes }: Matched, desired: PgTable) => {
        const wanted = Record.fromIterableBy(desired.indexes, Struct.get('name'));
        const { placed, foreign } = Pg.#aligned(rendered.indexes, live.indexes, prior, (row, index) => row.valid && Option.exists(Record.get(wanted, index.name), (target) => Equal.equals(Struct.evolve(row.shape, { keys: Array.map(renamed) }), target.shape)));
        const retired = Array.intersection(live.indexes.map(Struct.get('name')), retiredIndexes);
        const stale = placed.filter(({ item, missing }) => !missing && Option.exists(item.configuration, ({ name }) => drifted.includes(name)));
        return {
            relabels: Array.getSomes(placed.map(({ item, renamed: before }) => Option.map(before, (from) => sql`ALTER INDEX ${sql(from)} RENAME TO ${sql(item.name)}`))),
            concurrent: [
                ...[...placed.flatMap(({ dropped }) => dropped), ...retired].map((name) => sql`DROP INDEX CONCURRENTLY IF EXISTS ${sql(name)}`),
                ...placed.filter(({ missing }) => missing).map(({ item }) => item.text(['CONCURRENTLY', 'IF NOT EXISTS'])),
                ...stale.map(({ item }) => sql`REINDEX INDEX CONCURRENTLY ${sql(item.name)}`),
            ],
            indexes: Array.difference(foreign.map(Struct.get('name')), retired),
        };
    };
    static readonly #secured = ({ sql, role, grantees }: Planning, { relation, policy }: Matched['rendered'], held: Pick<PgTable, 'grants' | 'rls' | 'force' | 'policies'>, wanted: PgTable['policies'], prior: Matched['prior']) => {
        const rules = Record.fromIterableBy(wanted, Struct.get('name'));
        const { placed } = Pg.#aligned(Option.toArray(policy), held.policies, prior, (row, rule) => Option.contains(Record.get(rules, rule.name), { ...row, name: rule.name }));
        const granted = Array.filterMap(grantees, ({ grantee, all }) => {
            const missing = Array.difference(all, Option.getOrElse(Record.get(held.grants, grantee), Array.empty<string>));
            return Array.isReadonlyArrayNonEmpty(missing) ? Result.succeed(sql`GRANT ${sql.literal(missing.join(', '))} ON ${relation} TO ${sql(grantee)}`) : Result.failVoid;
        });
        return [
            ...granted,
            ...(Option.isSome(policy) && !held.rls ? [sql`ALTER TABLE ${relation} ENABLE ROW LEVEL SECURITY`] : []),
            ...(Option.isSome(policy) && !held.force ? [sql`ALTER TABLE ${relation} FORCE ROW LEVEL SECURITY`] : []),
            ...placed.flatMap(({ item, renamed, missing, dropped }) => [...Option.toArray(renamed).map((from) => sql`ALTER POLICY ${sql(from)} ON ${relation} RENAME TO ${sql(item.name)}`), ...dropped.map((name) => sql`DROP POLICY ${sql(name)} ON ${relation}`), ...(missing ? [item.text(role)] : [])]),
        ];
    };
    static readonly #columns = ({ owned, fields, live }: Matched, desired: PgTable) => {
        const located = Array.getSomes((owned ? fields : []).map(({ definition, found }) => Option.all({ wants: Record.get(desired.named, definition.name), now: found }).pipe(Option.map(Struct.assign({ definition })))));
        const [derived, held] = Array.partition(
            located,
            Filter.fromPredicate(({ wants, now }) => now.derives(wants)),
        );
        const pairs = held.filter(({ definition }) => definition.retype);
        const moved = new Set(pairs.filter(({ wants, now }) => now.type !== wants.type).map(({ now }) => now.name));
        const [expressed, regenerated] = Array.partition(
            derived,
            Filter.fromPredicate(({ wants, now }) => !Object.values(Struct.omit(now.differs(wants), ['expression'])).includes(true) && moved.isDisjointFrom(new Set(now.sources))),
        );
        const rebuilt = new Set(regenerated.map(({ now }) => now.name));
        return {
            pairs,
            regenerated,
            expressed: Array.getSomes(
                expressed.map(({ definition: { name }, wants, now }) =>
                    Option.map(
                        Option.filter(wants.expression, () => now.differs(wants).expression),
                        (expression) => ({ name, expression }),
                    ),
                ),
            ),
            dependents: live.columns.filter((row) => Option.isSome(row.generated) && !rebuilt.has(row.name) && !moved.isDisjointFrom(new Set(row.sources))),
        };
    };
    static readonly #retyped = (
        sql: SqlClient.SqlClient,
        { declared, rendered: { relation }, live, renamed }: Matched,
        desired: PgTable,
        { pairs, dependents }: { readonly pairs: readonly { readonly definition: { readonly name: string }; readonly wants: PgAttribute; readonly now: PgAttribute }[]; readonly dependents: readonly PgAttribute[] },
        relabeled: (constraint: string) => string,
    ) => {
        const [retyping, depending] = [HashSet.fromIterable(pairs.map(({ definition }) => [definition.name])), new Set(dependents.map(({ name }) => name))];
        const constrained = (rows: PgTable['checks']) => rows.filter(({ columns }) => HashSet.has(retyping, columns));
        const [had, needs] = [constrained(live.checks.map(Struct.evolve({ name: relabeled, columns: Array.map(renamed) }))), constrained(desired.checks)];
        const unmatched = Array.differenceWith(Equivalence.mapInput(Equivalence.String, ({ definition }: PgTable['checks'][number]) => definition));
        const [obsolete, missing] = [unmatched(had, needs), unmatched(needs, had)];
        const tightened = pairs.filter(({ wants, now }) => wants.required && !now.required).map(({ definition: { name: column } }) => ({ column, name: identifier([declared.name, column, 'not', 'null']) }));
        const changes = pairs.flatMap(({ definition: { name }, wants, now }) => {
            const [column, differs] = [sql(name), now.differs(wants)];
            const preset = Option.match(wants.expression, { onNone: () => sql`ALTER COLUMN ${column} DROP DEFAULT`, onSome: (expression) => sql`ALTER COLUMN ${column} SET DEFAULT ${sql.literal(expression)}` });
            return [...(differs.type ? [sql`ALTER COLUMN ${column} TYPE ${sql.literal(wants.type)} USING ${column}::${sql.literal(wants.type)}`] : []), ...(differs.expression && Option.isNone(wants.generated) ? [preset] : []), ...(now.required && !wants.required ? [sql`ALTER COLUMN ${column} DROP NOT NULL`] : [])];
        });
        const guards = pairs
            .filter(({ wants, now }) => now.type !== wants.type && !now.textual)
            .map(({ definition: { name }, wants, now }) =>
                Pg.#block(sql, [
                    sql`IF EXISTS (SELECT 1 FROM ${relation} WHERE (${sql(name)}::${sql.literal(wants.type)})::${sql.literal(now.type)} IS DISTINCT FROM ${sql(name)}) THEN RAISE EXCEPTION 'ALTER COLUMN %.% TYPE % changes stored values', ${sql.literal([declared.name, name, wants.type].map(literal).join(', '))} USING ERRCODE = 'check_violation'; END IF`,
                ]),
            );
        return {
            dropped: obsolete.map(({ name }) => name),
            validated: [...tightened, ...missing].map(({ name }) => name),
            statements: [
                ...obsolete.map(({ name }) => sql`ALTER TABLE ${relation} DROP CONSTRAINT ${sql(name)}`),
                ...dependents.map(({ name }) => sql`ALTER TABLE ${relation} DROP COLUMN ${sql(name)}`),
                ...guards,
                ...Array.match(changes, { onEmpty: () => [], onNonEmpty: (all) => [sql`ALTER TABLE ${relation} ${sql.csv(all)}`] }),
                ...tightened.map(({ column, name }) => sql`ALTER TABLE ${relation} ADD CONSTRAINT ${sql(name)} NOT NULL ${sql(column)} NOT VALID`),
                ...missing.map(({ name, definition }) => sql`ALTER TABLE ${relation} ADD CONSTRAINT ${sql(name)} ${sql.literal(definition)} NOT VALID`),
                ...dependents.flatMap((row) => [sql`ALTER TABLE ${relation} ADD COLUMN ${sql(row.name)} ${sql.literal(row.declared(!row.required))}`, ...Option.toArray(row.comment).map((text) => Pg.#commented(sql, relation, Option.some(row.name), Option.some(text)))]),
                ...live.indexes.filter(({ columns }) => !depending.isDisjointFrom(new Set(columns))).map(({ definition }) => sql`${sql.literal(definition)}`),
            ],
        };
    };
    static readonly #altered = (planning: Planning, entry: Matched, desired: PgTable) => {
        const [{ sql }, { declared, rendered, live, fields }] = [planning, entry];
        const [constraints, indexes, columns, owner] = [Pg.#constraints(planning, entry), Pg.#indexes(planning, entry, desired), Pg.#columns(entry, desired), rendered.relation];
        const retyped = Pg.#retyped(sql, entry, desired, columns, constraints.relabeled);
        const added = [...Array.getSomes(entry.added.map((definition) => Option.map(Record.get(desired.named, definition.name), (row) => ({ row, definition })))), ...columns.regenerated.map(({ definition, wants: row }) => ({ row, definition }))];
        const adding = new Set(added.map(({ definition }) => definition.name));
        const additions = desired.checks.filter(({ columns: checked }) => new Set(checked).isSubsetOf(adding));
        const validated = Array.difference([...live.invalid.map(constraints.relabeled), ...retyped.validated, ...[...additions, ...constraints.added].map(Struct.get('name'))], [...retyped.dropped, ...constraints.dropped]);
        const stamps = fields.filter(({ definition, found }) => Option.isSome(found) && !adding.has(definition.name));
        return {
            relabels: [...constraints.relabels, ...indexes.relabels],
            statements: [
                ...[...entry.dropping, ...columns.regenerated.map(({ definition }) => definition)].map(({ name }) => sql`ALTER TABLE ${owner} DROP COLUMN ${sql(name)}`),
                ...retyped.statements,
                ...added.map(({ definition, row }) => sql`ALTER TABLE ${owner} ADD COLUMN ${sql(definition.name)} ${sql.literal(row.declared(definition.nullable))}`),
                ...additions.map(({ name, definition }) => sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(name)} ${sql.literal(definition)} NOT VALID`),
                ...columns.expressed.map(({ name, expression }) => sql`ALTER TABLE ${owner} ALTER COLUMN ${sql(name)} SET EXPRESSION AS (${sql.literal(expression)})`),
                ...constraints.dropped.map((name) => sql`ALTER TABLE ${owner} DROP CONSTRAINT ${sql(name)}`),
                ...constraints.added.map(({ name, text }) => sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(name)} ${text} NOT VALID`),
                ...Pg.#secured(planning, rendered, live, desired.policies, entry.prior),
                ...Pg.#stamped(sql, owner, Option.none(), declared.name, entry.recorded, entry.identity),
                ...stamps.flatMap(({ definition, recorded, identity }) => Pg.#stamped(sql, owner, Option.some(definition.name), definition.name, recorded, { ...identity, digest: definition.digest })),
                ...added.flatMap(({ definition }) => definition.comment.statements(owner, Option.some(definition.name))),
            ],
            concurrent: [...indexes.concurrent, ...constraints.enforced.map((name) => sql`ALTER TABLE ${owner} ALTER CONSTRAINT ${sql(name)} ENFORCED`), ...validated.map((name) => sql`ALTER TABLE ${owner} VALIDATE CONSTRAINT ${sql(name)}`)],
            undeclared: { indexes: indexes.indexes, references: constraints.references },
        };
    };
    static readonly plan: Dialect['plan'] = Effect.fnUntraced(
        function* (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>) {
            const rendered = yield* render(sql, Pg, catalog);
            const indexes = rendered.flatMap(({ rendered: table }) => table.indexes);
            const owned = Array.dedupeWith(Array.getSomes(indexes.map(({ configuration }) => configuration)), Equivalence.mapInput(Equivalence.String, Struct.get('name')));
            const prelude = { required: Array.dedupe(indexes.flatMap(({ extensions }) => extensions)), filters: Array.dedupe(owned.flatMap(({ filters }) => filters)), declared: owned };
            const timeouts = Option.match(catalog.timeouts, { onNone: () => [], onSome: ({ lock, transaction }) => Record.collect({ lock_timeout: lock, transaction_timeout: transaction }, (name, limit) => ({ name, value: Duration.toMillis(limit).toString() })) });
            const preamble = timeouts.map(({ name, value }) => sql`SET LOCAL ${sql.literal(name)} = ${sql.literal(value)}`);
            yield* Effect.all(preamble, { discard: true });
            const { path } = yield* located(prelude);
            const environment = yield* configured(prelude);
            const existing = environment.configurations.filter(({ configured: present }) => present).map(({ name }) => name);
            const ready = ({ extensions, configuration }: (typeof indexes)[number]) => Array.isReadonlyArrayEmpty(Array.intersection(extensions, environment.absent)) && Option.toArray(configuration).every(({ name }) => existing.includes(name));
            const role = Option.match(runtime, { onNone: () => sql.literal('PUBLIC'), onSome: (name) => sql`${sql(name)}` });
            const temporary = (name: string) => sql`pg_temp.${sql(name)}`;
            const desired = rendered.flatMap(({ declared: { name }, rendered: table }) => [
                sql`${table.create({ target: temporary, inline: Function.constFalse })} ON COMMIT DROP`,
                ...table.indexes.filter(ready).map(({ text }) => text([], temporary(name))),
                ...Option.toArray(table.policy).map(({ text }) => text(role, temporary(name))),
            ]);
            yield* Pg.#block(sql, desired);
            const { observed: live } = yield* observed(undefined);
            const resolved = diff(sql, Pg, catalog, rendered, live.tables);
            const populated = yield* resolved.populated;
            yield* Effect.promise(loadModule);
            const grantees = [...Option.toArray(runtime).map((grantee) => ({ grantee, all: privileges.runtime })), ...Option.toArray(catalog.reader).map((grantee) => ({ grantee, all: privileges.reader }))].filter(({ grantee }) => grantee !== environment.migrator);
            const drifted = environment.configurations.filter(({ drift }) => Array.isReadonlyArrayNonEmpty(drift)).map(({ name }) => name);
            return yield* live.#reconcile({ sql, role, resolved, grantees, drifted }, environment, { populated, preamble, session: [...timeouts, { name: 'search_path', value: path }] });
        },
        (effect, sql) => Effect.provideService(effect, SqlClient.SqlClient, sql),
    );
    #reconcile(planning: Planning, { absent, configurations }: typeof Configurations.Type, parts: Pick<Parameters<Planning['resolved']['plan']>[0], 'populated' | 'preamble' | 'session'>) {
        const { sql, resolved } = planning;
        const scratch = Record.fromIterableBy(this.scratch, Struct.get('name'));
        const altered = Array.getSomes(resolved.matched.map((entry) => Option.map(Record.get(scratch, entry.declared.name), (desired) => Pg.#altered(planning, entry, desired))));
        const unsecured = { grants: {}, rls: false, force: false, policies: [] };
        const mapping = ({ tokens, dictionaries }: (typeof configurations)[number]['drift'][number]) => Option.match(dictionaries, { onNone: () => ['DROP MAPPING IF EXISTS FOR', tokens.join(', ')], onSome: (chain) => ['ALTER MAPPING FOR', tokens.join(', '), 'WITH', chain.join(', ')] });
        return resolved.plan({
            ...parts,
            findings: [],
            replaced: [],
            drifted: ({ declared, live }, field) => Option.exists(Option.all({ desired: Record.get(scratch, declared.name), now: field.found }), ({ desired, now }) => desired.drifted(live, now, field)),
            opening: [
                ...absent.map((extension) => sql`CREATE EXTENSION IF NOT EXISTS ${sql(extension)}`),
                ...configurations.filter(({ configured: present }) => !present).map(({ name, configuration }) => sql`CREATE TEXT SEARCH CONFIGURATION ${sql(name)} (COPY = ${sql.literal(configuration)})`),
                ...configurations.flatMap(({ name, drift }) => drift.map((change) => sql`ALTER TEXT SEARCH CONFIGURATION ${sql(name)} ${sql.literal(mapping(change).join(' '))}`)),
            ],
            statements: [
                ...altered.flatMap(({ relabels }) => relabels),
                ...resolved.created.flatMap(({ declared, rendered: table, deferred }) => [
                    table.create({ inline: (link) => !deferred.some(({ name }) => name === link.name) }),
                    ...table.comments,
                    ...table.indexes.map(({ text }) => text([])),
                    ...Pg.#secured(planning, table, unsecured, [], (labels) => identifier([declared.name, ...labels])),
                ]),
                ...resolved.created.flatMap(({ rendered: table, deferred }) => deferred.map((link) => sql`ALTER TABLE ${table.relation} ADD ${Pg.constraint(sql, link.name, link.text)}`)),
                ...altered.flatMap(({ statements }) => statements),
            ],
            concurrent: altered.flatMap(({ concurrent }) => concurrent),
            undeclared: { indexes: altered.flatMap(({ undeclared }) => undeclared.indexes), references: altered.flatMap(({ undeclared }) => undeclared.references) },
        });
    }

    // --- [LAYERS]

    static readonly #client = (names: Dialect['names']) => {
        const { OID, decode, encode } = PgTypes;
        const registry = PgTypes.makeRegistry();
        const register = (oid: number, read: PgTypes.Codec<unknown>['decode']) => registry.register(oid, { decode: read, encode: (value) => encode(value, oid) });
        register(OID.int8, (bytes) => Result.map(decode(bytes, OID.int8, 1), (value) => (Number.isSafeInteger(Number(value)) ? Number(value) : value)));
        register(OID.timestamptz, (bytes) => Result.flatMap(decode(bytes, OID.timestamptz, 1), (value) => Result.mapError(Schema.encodeUnknownResult(Schema.DateFromString)(value), (error) => new PgTypes.CodecError({ message: error.message }))));
        register(OID.jsonb, (bytes) => decode(bytes.subarray(1), OID.text, 1));
        register(OID.bool, (bytes) => Result.map(decode(bytes, OID.bool, 1), Number));
        return { ...names, transformJson: false, types: registry };
    };
    static readonly #engine = (names: Dialect['names']) => {
        const extension = Effect.cachedWithTTL(Effect.map(trigramNamespace(undefined), Struct.get('namespace')), (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero));
        return Layer.merge(
            Layer.succeed(Engine, { ...Pg, names }),
            Layer.effect(
                Rows,
                Effect.map(extension, (held) => ({ ...Pg, search: Pg.#search(held) })),
            ),
        );
    };
    static readonly layer = (config: Omit<PgClient.PgPoolConfig, 'types' | 'transformJson' | keyof Dialect['names']>, names: Dialect['names'] = Engine.names.snake) => Pg.#engine(names).pipe(Layer.provideMerge(PgClient.layer({ ...config, ...Pg.#client(names) })));
    static readonly layerConfig = (config: Config.Wrap<Omit<PgClient.PgPoolConfig, 'types' | 'transformJson' | keyof Dialect['names']>>, names: Dialect['names'] = Engine.names.snake) =>
        Pg.#engine(names).pipe(
            Layer.provideMerge(
                Config.unwrap(config).pipe(
                    Config.map((settings) => ({ ...settings, ...Pg.#client(names) })),
                    PgClient.layerConfig,
                ),
            ),
        );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const trigramNamespace = SqlSchema.findOne({
    Request: Schema.Void,
    Result: Schema.Struct({ namespace: Schema.String }),
    execute: () => SqlClient.SqlClient.use((sql) => sql`SELECT quote_ident(n.nspname) AS namespace FROM pg_extension e JOIN pg_namespace n ON n.oid = e.extnamespace WHERE e.extname = ${trigram.extension}`),
});
const located = SqlSchema.findOne({
    Request: Prelude,
    Result: Schema.Struct({ path: Schema.String }),
    execute: (names) =>
        SqlClient.SqlClient.use(
            (sql) => sql`SELECT pg_advisory_xact_lock(hashtextextended(current_schema(), 0))::text AS lock, set_config('search_path', concat_ws(', ', quote_ident(current_schema()),
                (SELECT string_agg(quote_ident(n.nspname), ', ' ORDER BY n.nspname) FROM pg_namespace n WHERE n.oid NOT IN (current_schema()::regnamespace, 'pg_catalog'::regnamespace)
                    AND (EXISTS (SELECT 1 FROM pg_extension e WHERE e.extnamespace = n.oid AND e.extname = ANY(${names.required}::text[])) OR EXISTS (SELECT 1 FROM pg_ts_dict d WHERE d.dictnamespace = n.oid AND d.dictname = ANY(${names.filters}::text[])))), 'pg_temp'), true) AS path`,
        ),
});
const configured = SqlSchema.findOne({
    Request: Prelude,
    Result: Configurations,
    execute: (names) =>
        SqlClient.SqlClient.use(
            (sql) => sql`WITH maps AS (SELECT m.mapcfg, m.maptokentype, array_agg(m.mapdict::regdictionary::text ORDER BY m.mapseqno) AS dictionaries FROM pg_ts_config_map m GROUP BY m.mapcfg, m.maptokentype),
                declared AS (SELECT d.name, d.configuration::regconfig::oid AS base, (SELECT c.oid FROM pg_ts_config c WHERE c.cfgnamespace = current_schema()::regnamespace AND c.cfgname = d.name) AS live, array(SELECT quote_ident(f) FROM unnest(d.filters) f) AS filters FROM json_to_recordset(${names.declared}::json) d(name text, configuration text, filters text[])),
                tokens AS (SELECT d.name, t.tokid, t.alias, CASE WHEN b.dictionaries IS NOT NULL THEN d.filters || b.dictionaries END AS wanted, l.dictionaries AS now
                    FROM declared d JOIN pg_ts_config c ON c.oid = d.base CROSS JOIN ts_token_type(c.cfgparser) t
                    LEFT JOIN maps b ON (b.mapcfg, b.maptokentype) = (d.base, t.tokid) LEFT JOIN maps l ON (l.mapcfg, l.maptokentype) = (coalesce(d.live, d.base), t.tokid))
                SELECT current_user::text AS migrator,
                    array(SELECT a.name FROM (SELECT x FROM unnest(${names.required}::text[]) x WHERE NOT EXISTS (SELECT 1 FROM pg_extension e WHERE e.extname = x)
                        UNION SELECT f FROM unnest(${names.filters}::text[]) f WHERE NOT EXISTS (SELECT 1 FROM pg_ts_dict d WHERE d.dictname = f) AND EXISTS (SELECT 1 FROM pg_available_extensions v WHERE v.name = f)) a(name) ORDER BY a.name) AS absent, array_to_json(array(SELECT json_build_object('name', d.name, 'configuration', d.base::regconfig::text, 'configured', d.live IS NOT NULL,
                    'drift', array(SELECT json_build_object('tokens', array_agg(k.alias ORDER BY k.tokid), 'dictionaries', k.wanted) FROM tokens k WHERE k.name = d.name AND k.now IS DISTINCT FROM k.wanted GROUP BY k.wanted)) FROM declared d))::text AS configurations`,
        ),
});
const observed = SqlSchema.findOne({
    Request: Schema.Void,
    Result: Schema.Struct({ observed: Schema.fromJsonString(Pg) }),
    execute: () =>
        SqlClient.SqlClient.use(
            (sql) => sql`WITH relations AS (SELECT c.oid, c.relname, c.relowner, c.relacl, c.relrowsecurity, c.relforcerowsecurity, c.relnamespace = pg_my_temp_schema() AS scratch FROM pg_class c WHERE c.relnamespace IN (current_schema()::regnamespace, pg_my_temp_schema()) AND c.relkind = 'r'),
                constraints AS (SELECT n.oid, n.conrelid, n.conname, n.contype, n.convalidated, n.conenforced, n.confrelid, n.confdeltype,
                    array(SELECT a.attname::text FROM unnest(n.conkey) WITH ORDINALITY k(number, position) JOIN pg_attribute a ON (a.attrelid, a.attnum) = (n.conrelid, k.number) ORDER BY k.position) AS columns
                    FROM pg_constraint n JOIN relations r ON r.oid = n.conrelid),
                described AS (SELECT r.scratch, json_build_object('name', r.relname, 'comment', obj_description(r.oid, 'pg_class'), 'rls', r.relrowsecurity, 'force', r.relforcerowsecurity,
                    'grants', coalesce((SELECT json_object_agg(g.grantee, g.held) FROM (SELECT o.rolname AS grantee, array_agg(a.privilege_type::text) AS held FROM aclexplode(r.relacl) a JOIN pg_roles o ON o.oid = a.grantee WHERE a.grantee <> r.relowner GROUP BY o.rolname) g), '{}'::json),
                    'columns', array(SELECT json_build_object('name', a.attname, 'comment', col_description(a.attrelid, a.attnum), 'type', format_type(a.atttypid, a.atttypmod), 'textual', t.typcategory = 'S', 'expression', pg_get_expr(d.adbin, d.adrelid), 'generated', CASE a.attgenerated WHEN 's' THEN 'STORED' WHEN 'v' THEN 'VIRTUAL' END,
                        'required', EXISTS (SELECT 1 FROM pg_constraint n WHERE n.conrelid = a.attrelid AND n.contype = 'n' AND n.conkey = ARRAY[a.attnum] AND n.convalidated),
                        'sources', array(SELECT s.attname::text FROM pg_depend p JOIN pg_attribute s ON (s.attrelid, s.attnum) = (p.refobjid, p.refobjsubid) WHERE p.classid = 'pg_attrdef'::regclass AND p.objid = d.oid AND p.refobjsubid > 0 AND s.attnum <> a.attnum))
                        FROM pg_attribute a JOIN pg_type t ON t.oid = a.atttypid LEFT JOIN pg_attrdef d ON (d.adrelid, d.adnum) = (a.attrelid, a.attnum) WHERE a.attrelid = r.oid AND a.attnum > 0 AND NOT a.attisdropped ORDER BY a.attnum),
                    'checks', array(SELECT json_build_object('name', n.conname, 'definition', pg_get_constraintdef(n.oid, true), 'columns', n.columns) FROM constraints n WHERE n.conrelid = r.oid AND n.contype = 'c'),
                    'references', array(SELECT json_build_object('name', n.conname, 'columns', n.columns, 'target', t.relname, 'onDelete', n.confdeltype::text) FROM constraints n JOIN pg_class t ON t.oid = n.confrelid WHERE n.conrelid = r.oid AND n.contype = 'f'),
                    'invalid', array(SELECT n.conname::text FROM constraints n WHERE n.conrelid = r.oid AND n.conenforced AND NOT n.convalidated),
                    'unenforced', array(SELECT n.conname::text FROM constraints n WHERE n.conrelid = r.oid AND NOT n.conenforced),
                    'indexes', array(SELECT json_build_object('name', i.relname, 'valid', x.indisvalid, 'definition', pg_get_indexdef(x.indexrelid),
                        'columns', array(SELECT a.attname::text FROM pg_depend p JOIN pg_attribute a ON (a.attrelid, a.attnum) = (p.refobjid, p.refobjsubid) WHERE p.objid = x.indexrelid AND p.classid = 'pg_class'::regclass AND p.refobjsubid > 0),
                        'shape', json_build_object('unique', x.indisunique, 'method', am.amname, 'width', x.indnkeyatts, 'predicate', pg_get_expr(x.indpred, x.indrelid, true),
                            'keys', array(SELECT coalesce(a.attname::text, pg_get_indexdef(x.indexrelid, k, true)) FROM generate_series(1, x.indnatts) k LEFT JOIN pg_attribute a ON a.attrelid = x.indrelid AND a.attnum = x.indkey[k - 1] ORDER BY k),
                            'classes', array(SELECT o.opcname::text FROM unnest(x.indclass) WITH ORDINALITY u(oid, position) JOIN pg_opclass o ON o.oid = u.oid ORDER BY u.position)))
                        FROM pg_index x JOIN pg_class i ON i.oid = x.indexrelid JOIN pg_am am ON am.oid = i.relam WHERE x.indrelid = r.oid AND NOT EXISTS (SELECT 1 FROM pg_constraint k WHERE k.conindid = x.indexrelid AND k.conrelid = x.indrelid)),
                    'policies', array(SELECT json_build_object('name', p.polname, 'using', pg_get_expr(p.polqual, p.polrelid, true), 'check', pg_get_expr(p.polwithcheck, p.polrelid, true), 'roles', p.polroles::regrole[]::text[]) FROM pg_policy p WHERE p.polrelid = r.oid)) AS relation
                    FROM relations r)
                SELECT json_build_object('tables', array(SELECT d.relation FROM described d WHERE NOT d.scratch), 'scratch', array(SELECT d.relation FROM described d WHERE d.scratch))::text AS observed`,
        ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Pg };
