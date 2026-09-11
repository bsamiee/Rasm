// --- [IMPORTS] -------------------------------------------------------------------------

import { DatabaseSync } from 'node:sqlite';
import { afterAll, expect, it } from 'vitest';
import { open } from './hooks/observation/sql.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _VIEW = /create view (?<name>\w+) as/gu;
const _COUNT = 21;
const _SEGMENTS = 4;
const _OLD_RANGE =
    'create table judged_range(kind text not null, lineage_key text not null, worktree text not null, from_ts integer not null, to_ts integer not null, agent_id text not null, at integer not null) strict;';
const _OLD_KIND = "create table range_kind(kind text); insert into range_kind(kind) values ('commit'), ('edit');";
const _OLD_INDEX = 'create index finding_path on finding(path, occurrence);';
// Open statement around its dot commands, which node:sqlite runs none of: the selects print between `.output` and `.output`, the temp tables drop, and `.read` applies what the selects printed
const _PARTS = open('.').split(/^\..*\n/gmu);
const [_BEGUN = '', _SELECTS = '', _DROPS = '', _APPLIED = ''] = _PARTS;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _binary = (left: string, right: string): number => Number(left > right) - Number(left < right);

// Hashing runs in the sqlite3 shell alone, absent from the library node:sqlite links, registered so the generated column of finding parses; no case inserts a finding
const _sha3 = (): never => {
    throw new Error('sha3 hashes rows in the sqlite3 shell alone');
};

const _sink = (): DatabaseSync => {
    const database = new DatabaseSync(':memory:');
    database.function('sha3', { deterministic: true, varargs: true }, _sha3);
    return database;
};

// Delta as the shell prints it to the file: one statement per row of the two selects
const _delta = (database: DatabaseSync): string => {
    database.exec(_BEGUN);
    return _SELECTS
        .trim()
        .split('\n')
        .flatMap((select) =>
            database
                .prepare(select)
                .all()
                .map((row) => String(Object.values(row)[0])),
        )
        .join('\n');
};

const _opened = (database: DatabaseSync): string => {
    const delta = _delta(database);
    database.exec(`${_DROPS}${delta}\n${_APPLIED}`);
    return delta;
};

const _views = (database: DatabaseSync): readonly string[] =>
    database
        .prepare("select name from sqlite_master where type = 'view' order by name")
        .all()
        .map((row) => String(row['name']));

const _columns = (database: DatabaseSync, table: string): readonly string[] =>
    database
        .prepare(`select name, hidden from pragma_table_xinfo('${table}')`)
        .all()
        .map((row) => `${String(row['name'])}${Number(row['hidden']) === 0 ? '' : ' generated'}`);

// --- [FIXTURE] -------------------------------------------------------------------------

const sink = _sink();
_opened(sink);
const created = _views(sink);
afterAll(() => {
    sink.close();
});

// --- [CASES] ---------------------------------------------------------------------------

it('creates every view the open statement declares', () => {
    expect(_PARTS).toHaveLength(_SEGMENTS);
    expect(created).toHaveLength(_COUNT);
    expect(created).toStrictEqual([...open('.').matchAll(_VIEW)].map((match) => String(match.groups?.['name'])).toSorted(_binary));
});

it.each(created)('prepares %s', (name) => {
    expect(() => sink.prepare(`select * from ${name} limit 0`)).not.toThrow();
});

it('drops every view and rebuilds no table on a second open over the same database, its temp tables gone', () => {
    const delta = _opened(sink);
    expect(delta.split('\n').toSorted(_binary)).toStrictEqual(created.map((name) => `drop view if exists ${name};`));
    expect(sink.prepare('select count(1) as n from sqlite_temp_master').get()?.['n']).toBe(0);
    for (const name of created) {
        expect(() => sink.prepare(`select * from ${name} limit 0`)).not.toThrow();
    }
});

it('rebuilds a table whose stored body differs, keeping the rows of its common columns, and recreates a changed index', () => {
    const old = _sink();
    old.exec(`${_OLD_RANGE}${_OLD_KIND}`);
    _opened(old);
    old.exec(`drop index finding_path; ${_OLD_INDEX} drop index finding_category; ${_OLD_INDEX.replace('finding_path', 'FINDING_CATEGORY')}`);
    old.exec(`${_OLD_RANGE.replace('create table', 'drop table judged_range; create table')}`);
    const delta = _opened(old);
    expect(delta).toContain('alter table judged_range__delta rename to judged_range;');
    expect(delta).toContain('drop index if exists finding_path;');
    expect(delta).toContain('drop index if exists FINDING_CATEGORY;');
    expect(old.prepare("select sql from sqlite_master where name = 'finding_path'").get()?.['sql']).toBe(
        'CREATE INDEX finding_path on finding(path)',
    );
    expect(old.prepare("select sql from sqlite_master where name = 'finding_category'").get()?.['sql']).toBe(
        'CREATE INDEX finding_category on finding(category)',
    );
    expect(_columns(old, 'judged_range')).toStrictEqual([
        'kind',
        'main_worktree',
        'worktree',
        'branch',
        'lineage_key generated',
        'from_ts',
        'to_ts',
        'agent_id',
        'at',
    ]);
    expect(old.prepare('select group_concat(kind) as kinds from (select kind from range_kind order by kind)').get()?.['kinds']).toBe('edit');
    expect(old.prepare("select count(1) as n from sqlite_master where name like '%__delta'").get()?.['n']).toBe(0);
    expect(_views(old)).toStrictEqual(created);
    expect(_opened(old)).not.toContain('create table');
    old.close();
});
