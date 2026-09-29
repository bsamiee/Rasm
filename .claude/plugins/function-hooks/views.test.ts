import { DatabaseSync, type SQLOutputValue } from 'node:sqlite';
import { afterAll, expect, it } from 'vitest';
import { open } from './hooks/observation/sql.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _memory = (): DatabaseSync => {
    const database = new DatabaseSync(':memory:');
    database.function('sha3', { deterministic: true, varargs: true }, () => {
        throw new Error('sha3 hashes rows in the sqlite3 shell alone');
    });
    return database;
};

const _values = (database: DatabaseSync, select: string): readonly string[] =>
    database
        .prepare(select)
        .all()
        .flatMap((row) => Object.values(row).map(String));

const _schema = (database: DatabaseSync): readonly Record<string, SQLOutputValue>[] =>
    database
        .prepare("select type, name, tbl_name, substr(sql, instr(sql, '(')) as body from sqlite_master union all select type, name, tbl_name, sql from sqlite_temp_master order by type, name")
        .all();

const _opened = (database: DatabaseSync): readonly (readonly string[])[] =>
    open()
        .split('\n')
        .reduce<{ readonly output: boolean; readonly delta: readonly string[]; readonly passes: readonly (readonly string[])[] }>(
            (shell, line) => {
                if (line === '.output') {
                    return { output: false, delta: shell.delta, passes: shell.passes.concat([shell.delta]) };
                }
                if (line.startsWith('.output ')) {
                    return { output: true, delta: [], passes: shell.passes };
                }
                if (shell.output) {
                    return { output: true, delta: shell.delta.concat(_values(database, line)), passes: shell.passes };
                }
                database.exec(line.startsWith('.read ') ? shell.delta.join('\n') : line);
                return shell;
            },
            { output: false, delta: [], passes: [] },
        ).passes;

// --- [FIXTURE] -------------------------------------------------------------------------

const db = _memory();
_opened(db);
const created = _values(db, "select name from sqlite_master where type = 'view'");
const recreated = new Set(created.map((name) => `drop view ${name};`));
afterAll(() => {
    db.close();
});

// --- [CASES] ---------------------------------------------------------------------------

it.each(created)('prepares %s', (name) => {
    expect(() => db.prepare(`select * from ${name} limit 0`)).not.toThrow();
});

it('second open drops and creates every view and leaves the schema unchanged', () => {
    const schema = _schema(db);
    const [delta] = _opened(db);
    expect(new Set(delta)).toStrictEqual(recreated);
    expect(_schema(db)).toStrictEqual(schema);
});

it('changed table and index, retired view, and retired lookup value converge to the declared schema by case-insensitive name', () => {
    using old = _memory();
    _opened(old);
    old.exec(
        "drop index finding_path; create index finding_path on finding(path, occurrence); drop index finding_category; create index FINDING_CATEGORY on finding(path); drop table judged_range; create table JUDGED_RANGE(kind text not null, lineage_key text not null) strict; create view retired as select 1; insert into range_kind(kind) values ('commit');",
    );
    _opened(old);
    expect(_schema(old)).toStrictEqual(_schema(db));
    expect(new Set(_values(old, 'select kind from range_kind'))).toStrictEqual(new Set(_values(db, 'select kind from range_kind')));
    const [reopened] = _opened(old);
    expect(new Set(reopened)).toStrictEqual(recreated);
});

it('transition table rebuild keeps finding ids and referenced lookup values, drops an undeclared column, and adds declared columns as null', () => {
    using old = _memory();
    old.exec(
        "create table transition_state(state text primary key) strict; insert into transition_state(state) values ('superseded'); create table finding_transition(finding_id text not null, state text not null, subject_hash text not null, start_line integer, start_column integer, end_line integer, end_column integer, occurrence integer, at integer not null, actor text not null, actor_id text, evidence text, verdict text, successor text) strict; insert into finding_transition(finding_id, state, subject_hash, start_line, occurrence, at, actor, actor_id, evidence, successor) values ('f1', 'confirmed', 'h1', 3, 1, 1, 'agent', 'a', 'present', null), ('f1', 'superseded', 'h1', null, null, 2, 'check', 'sqlite3', null, 'f2');",
    );
    const stored = old.prepare('select * from finding_transition order by at').all();
    const declared = _values(db, "select name from pragma_table_info('finding_transition')");
    _opened(old);
    expect(_schema(old)).toStrictEqual(_schema(db));
    expect(old.prepare('select * from finding_transition order by at').all()).toEqual(stored.map((row) => Object.fromEntries(declared.map((name) => [name, row[name] ?? null]))));
    expect(new Set(_values(old, 'select state from transition_state'))).toStrictEqual(new Set([..._values(db, 'select state from transition_state'), 'superseded']));
});
