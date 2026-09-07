// --- [IMPORTS] -------------------------------------------------------------------------

import assert from 'node:assert/strict';
import { execFileSync, spawnSync } from 'node:child_process';
import { mkdirSync, mkdtempDisposableSync, readFileSync, renameSync, symlinkSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { test } from 'node:test';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _script = resolve('.claude/skills/ast-grep/scripts/rule-checks.sh');
const _library = resolve('.cache/ast-grep/xml.so');

const _project = (directory: string, config: Record<string, unknown> = {}, fixtures: Record<string, unknown> = {}): Record<string, unknown> => {
    const configuration = {
        ruleDirs: ['rules'],
        testConfigs: [{ testDir: 'tests' }],
        customLanguages: { xml: { libraryPath: _library, extensions: ['csproj'] } },
        ...config,
    };
    for (const [file, value] of Object.entries({
        'rules/xml-name.yml': {
            id: 'xml-name',
            language: 'xml',
            severity: 'error',
            rule: { kind: 'Name', regex: '^Old$' },
            message: 'Use New',
            fix: 'New',
        },
        'tests/xml-name-test.yml': { id: 'xml-name', valid: ['<New/>'], invalid: ['<Old/>'] },
        'sgconfig.yml': configuration,
        ...fixtures,
    })) {
        const target = join(directory, file);
        mkdirSync(dirname(target), { recursive: true });
        writeFileSync(target, JSON.stringify(value));
    }
    return configuration;
};

// --- [TESTS] ---------------------------------------------------------------------------

test('rule checks preserve native platform maps and absolute parser paths', () => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    _project(directory.path);
    const original = readFileSync(join(directory.path, 'rules/xml-name.yml'), 'utf8');
    mkdirSync(join(directory.path, 'parsers'));
    symlinkSync(_library, join(directory.path, 'parsers/xml.so'));
    execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
    for (const libraryPath of [
        _library,
        {
            'aarch64-apple-darwin': 'parsers/xml.so',
            'x86_64-apple-darwin': 'parsers/xml.so',
            'aarch64-unknown-linux-gnu': 'parsers/xml.so',
            'x86_64-unknown-linux-gnu': 'parsers/xml.so',
            'x86_64-pc-windows-msvc': 'parsers/xml.so',
        },
    ]) {
        writeFileSync(
            join(directory.path, 'sgconfig.yml'),
            JSON.stringify({
                ruleDirs: [join(directory.path, 'rules')],
                testConfigs: [{ testDir: join(directory.path, 'tests') }],
                customLanguages: { xml: { libraryPath, extensions: ['csproj'] } },
            }),
        );
        assert.equal(execFileSync('bash', [_script, 'gate', 'csproj'], { cwd: directory.path, encoding: 'utf8' }), '');
        assert.equal(readFileSync(join(directory.path, 'rules/xml-name.yml'), 'utf8'), original);
    }
});

test('rule checks read every test directory and its configured snapshots', () => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    _project(
        directory.path,
        { testConfigs: [{ testDir: 'tests' }, { testDir: 'other-tests', snapshotDir: 'saved' }] },
        {
            'rules/xml-second.yml': {
                id: 'xml-second',
                language: 'xml',
                severity: 'error',
                rule: { kind: 'Name', regex: '^Before$' },
                message: 'Use After',
                fix: 'After',
            },
            'other-tests/xml-second-test.yml': { id: 'xml-second', valid: ['<After/>'], invalid: ['<Before/>'] },
        },
    );
    const cases = join(directory.path, 'other-tests/xml-second-test.yml');
    execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
    assert.equal(execFileSync('bash', [_script, 'gate', 'csproj'], { cwd: directory.path, encoding: 'utf8' }), '');
    writeFileSync(cases, JSON.stringify({ id: 'xml-second', valid: ['<After/>'], invalid: ['<After/>'] }));
    const failure = spawnSync('bash', [_script, 'gate', 'csproj'], { cwd: directory.path, encoding: 'utf8' });
    assert.equal(failure.status, 1);
    assert.equal(failure.stdout.includes('FAIL xml-second'), true);
    assert.equal(Array.from(failure.stdout.matchAll(/orphan or missing snapshot key: xml-second/gu)).length, 1);
});

test('declaration counts include helpers and exported definitions', () => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    _project(
        directory.path,
        { languageGlobs: { tsx: ['*.ts'] } },
        {
            'rules/no-fourth-nesting-level.yml': {
                id: 'no-fourth-nesting-level',
                language: 'tsx',
                severity: 'error',
                rule: { pattern: 'neverCalled()' },
                message: 'Reduce nesting',
            },
        },
    );
    writeFileSync(
        join(directory.path, 'input.ts'),
        `const first = 1;
let second = 2;
var third = 3;
function named() {}
function* generator() {}
export function exported() {}
abstract class Abstract {}
class Plain {}
interface Model {}
type Alias = string;
enum Kind { A }
declare function ambient(): void;
namespace Module {}
export default function() {}`,
    );
    writeFileSync(join(directory.path, 'arrow.ts'), 'export default () => 1;');
    assert.equal(
        execFileSync('bash', [_script, 'measure', 'ts', 'input.ts', 'arrow.ts'], { cwd: directory.path, encoding: 'utf8' }),
        'elements 15 nesting 0\n',
    );
});

test('parse checks apply expanded fixes without changing source rules or fixtures', () => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    _project(directory.path);
    const ruleFile = join(directory.path, 'rules/remove-json-member.yml');
    const rule = {
        id: 'remove-json-member',
        language: 'json',
        severity: 'error',
        files: ['input.json'],
        rule: { kind: 'number', regex: '^2$' },
        fix: { template: '', expandEnd: { regex: '^,$' } },
        message: 'Remove member',
    };
    writeFileSync(ruleFile, JSON.stringify(rule));
    const casesFile = join(directory.path, 'tests/remove-json-member-test.yml');
    writeFileSync(casesFile, JSON.stringify({ id: rule.id, valid: ['[1,3]'], invalid: ['[1,2,3]', '[2,4]', '[5,2,6]'] }));
    execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
    const snapshotFile = join(directory.path, 'tests/__snapshots__/remove-json-member-snapshot.yml');
    const original = [ruleFile, casesFile, snapshotFile].map((file) => readFileSync(file, 'utf8'));
    assert.equal(original[2]?.includes('[1,,3]'), true);
    assert.equal(execFileSync('bash', [_script, 'parse', 'json', '^remove-json-member$'], { cwd: directory.path, encoding: 'utf8' }), '');
    assert.deepEqual(
        [ruleFile, casesFile, snapshotFile].map((file) => readFileSync(file, 'utf8')),
        original,
    );
    writeFileSync(ruleFile, JSON.stringify({ ...rule, fix: { ...rule.fix, template: '@' } }));
    const failure = spawnSync('bash', [_script, 'parse', 'json', '^remove-json-member$'], { cwd: directory.path, encoding: 'utf8' });
    assert.equal(failure.status, 1);
    assert.equal(failure.stdout.includes('ERROR node in fixed remove-json-member'), true);
});

test('rule checks retain literal ampersands in temporary paths', () => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    _project(directory.path);
    const temporary = join(directory.path, 'temporary & literal');
    mkdirSync(temporary);
    execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
    assert.equal(
        execFileSync(
            'bash',
            [
                '-c',
                'export scratch_parent=$1; shift; mktemp() { command mktemp -d "$scratch_parent/tmp.XXXXXX"; }; export -f mktemp; exec bash "$@"',
                '--',
                temporary,
                _script,
                'gate',
                'csproj',
            ],
            { cwd: directory.path, encoding: 'utf8' },
        ),
        '',
    );
});

test('rule checks retain tabs and newlines in configured directories', () => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    _project(directory.path);
    const rules = 'rules\twith\nlines';
    const tests = 'tests\twith\nlines\n';
    renameSync(join(directory.path, 'rules'), join(directory.path, rules));
    renameSync(join(directory.path, 'tests'), join(directory.path, tests));
    writeFileSync(
        join(directory.path, 'sgconfig.yml'),
        JSON.stringify({
            ruleDirs: [rules],
            testConfigs: [{ testDir: tests }],
            customLanguages: { xml: { libraryPath: _library, extensions: ['csproj'] } },
        }),
    );
    execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
    assert.equal(execFileSync('bash', [_script, 'gate', 'csproj'], { cwd: directory.path, encoding: 'utf8' }), '');
    writeFileSync(join(directory.path, 'sgconfig.yml'), '{');
    assert.notEqual(spawnSync('bash', [_script, 'pairing'], { cwd: directory.path, encoding: 'utf8' }).status, 0);
});

test('mutation batches retain every caller from a shared fixture file', () => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    _project(
        directory.path,
        { utilDirs: ['utils'] },
        {
            'utils/xml-choice.yml': { id: 'xml-choice', language: 'xml', rule: { kind: 'Name', any: [{ regex: '^Old$' }, { regex: '^Older$' }] } },
        },
    );
    for (const id of ['xml-name', 'xml-other']) {
        writeFileSync(
            join(directory.path, `rules/${id}.yml`),
            JSON.stringify({
                id,
                language: 'xml',
                severity: 'error',
                rule: { matches: 'xml-choice' },
                message: 'Replace old name',
            }),
        );
    }
    const first = JSON.stringify({ id: 'xml-name', valid: ['<Safe/>'], invalid: ['<Old/>'] });
    const second = JSON.stringify({ id: 'xml-other', valid: ['<Safe/>'], invalid: ['<Older/>'] });
    const cases = join(directory.path, 'tests/xml-name-test.yml');
    writeFileSync(cases, `${first}\n---\n${second}\n`);
    execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
    assert.equal(execFileSync('bash', [_script, 'gate', 'csproj', '^xml-name$'], { cwd: directory.path, encoding: 'utf8' }), '');
    for (const replacement of ['Old', 'Unmatched']) {
        writeFileSync(cases, `${first}\n---\n${second.replace('Older', replacement)}\n`);
        const result = spawnSync('bash', [_script, 'arms', 'csproj', '^xml-name$'], { cwd: directory.path, encoding: 'utf8' });
        assert.equal(result.status, 1);
        assert.ok(result.stdout.includes('uncovered arm: xml-choice delete ["rule","any",1]'), result.stdout);
        assert.ok(replacement !== 'Unmatched' || result.stdout.includes('FAIL xml-other'), result.stdout);
    }
});

test('rule checks synthesize terminal brace extensions without changing rule scope', (): void => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-brace-checks-'));
    mkdirSync(join(directory.path, 'rules'));
    mkdirSync(join(directory.path, 'tests'));
    writeFileSync(join(directory.path, 'sgconfig.yml'), JSON.stringify({ ruleDirs: ['rules'], testConfigs: [{ testDir: 'tests' }] }));
    const ruleFile = join(directory.path, 'rules/yaml-value.yml');
    writeFileSync(
        join(directory.path, 'tests/yaml-value-test.yml'),
        JSON.stringify({ id: 'yaml-value', valid: ['value: good'], invalid: ['value: bad'] }),
    );
    for (const [glob, extension, ignored] of [
        ['.github/workflows/*.{yaml,yml}', 'yml', '**/*.yaml'],
        ['.github/workflows/*.{yml,yaml}', 'yaml', '**/*.yml'],
        ['.github/workflows/*.{yml,template}', 'yaml', '**/*.template'],
        ['**/*.{yaml,yml}', 'yml', '**/*.yaml'],
        ['**/nested/*.yml', 'yml', '**/*.yaml'],
        ['**/nested/deeper/*.{yaml,yml}', 'yml', '**/*.yaml'],
        ['.github/workflows/*.yml', 'yml', '**/*.yaml'],
    ] as const) {
        const rule = JSON.stringify({
            id: 'yaml-value',
            language: 'yaml',
            severity: 'error',
            files: [glob],
            ignores: [ignored],
            rule: { kind: 'string_scalar', regex: '^bad$' },
            message: 'Use good',
        });
        writeFileSync(ruleFile, rule);
        execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
        assert.equal(execFileSync('bash', [_script, 'gate', extension], { cwd: directory.path, encoding: 'utf8' }), '');
        assert.equal(readFileSync(ruleFile, 'utf8'), rule);
    }
});

for (const files of [['input.sh'], ['**/*.sh']]) {
    test(`expanded fixes preserve original syntax and source files (${files[0]})`, (): void => {
        using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
        _project(directory.path);
        const ruleFile = join(directory.path, 'rules/bash-glob.yml');
        const casesFile = join(directory.path, 'tests/bash-glob-test.yml');
        const rule = {
            id: 'bash-glob',
            language: 'bash',
            severity: 'error',
            files,
            rule: { pattern: 'echo old' },
            fix: { template: "printf '%s\\n' new;", expandEnd: { regex: '^;$' } },
            message: 'Use printf',
        };
        const cases = { id: rule.id, valid: ['printf new'], invalid: ['echo old; printf done', 'echo old; value=${ printf ok; }'] };
        writeFileSync(ruleFile, JSON.stringify(rule));
        writeFileSync(casesFile, JSON.stringify(cases));
        execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
        const sourceFiles = [ruleFile, casesFile, join(directory.path, 'tests/__snapshots__/bash-glob-snapshot.yml')];
        const original = sourceFiles.map((file) => readFileSync(file, 'utf8'));
        assert.equal(execFileSync('bash', [_script, 'parse', 'sh'], { cwd: directory.path, encoding: 'utf8' }), '');
        assert.deepEqual(
            sourceFiles.map((file) => readFileSync(file, 'utf8')),
            original,
        );
        writeFileSync(ruleFile, JSON.stringify({ ...rule, fix: { ...rule.fix, template: 'if' } }));
        const brokenFix = spawnSync('bash', [_script, 'parse', 'sh'], { cwd: directory.path, encoding: 'utf8' });
        assert.equal(brokenFix.status, 1);
        assert.equal(brokenFix.stdout.split('Bash syntax error in fixed bash-glob').length - 1, 2);
        assert.equal(brokenFix.stdout.includes('Bash syntax error in invalid'), false);
        writeFileSync(ruleFile, JSON.stringify(rule));
        writeFileSync(casesFile, JSON.stringify({ ...cases, invalid: ['echo old; if', cases.invalid[1]] }));
        const brokenOriginal = spawnSync('bash', [_script, 'parse', 'sh'], { cwd: directory.path, encoding: 'utf8' });
        assert.equal(brokenOriginal.status, 1);
        assert.equal(brokenOriginal.stdout.includes('Bash syntax error in invalid bash-glob case 1'), true);
        assert.equal(brokenOriginal.stdout.includes('Bash syntax error in fixed bash-glob case 1'), true);
    });
}

test('parse reports each malformed file once with newlines in temporary paths', (): void => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    _project(
        directory.path,
        {},
        {
            'rules/json-value.yml': { id: 'json-value', language: 'json', severity: 'error', rule: { kind: 'number' }, message: 'Use a string' },
            'tests/json-value-test.yml': { id: 'json-value', valid: ['"safe"'], invalid: ['[1,@,2,#,3]'] },
        },
    );
    const temporary = join(directory.path, 'temporary\nwith\nlines');
    mkdirSync(temporary);
    const result = spawnSync(
        'bash',
        [
            '-c',
            'export scratch_parent=$1; shift; mktemp() { command mktemp -d "$scratch_parent/tmp.XXXXXX"; }; export -f mktemp; exec bash "$@"',
            '--',
            temporary,
            _script,
            'parse',
            'json',
        ],
        { cwd: directory.path, encoding: 'utf8' },
    );
    assert.equal(result.status, 1);
    assert.equal(result.stdout, 'ERROR node in invalid json-value case 1\n');
    assert.equal(result.stderr, '');
});

test('mutation snapshot read failures leave arms unproven', (): void => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    _project(directory.path, {}, { 'tests/xml-name-test.yml': { id: 'xml-name', valid: ['<!--safe-->'], invalid: ['<Old/>'] } });
    execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
    const result = spawnSync(
        'bash',
        [
            '-c',
            'yq() { local file; for file; do [[ $file != */jobs/*/snapshots/* ]] || { printf "%s\\n" "generated snapshot unreadable" >&2; return 42; }; done; command yq "$@"; }; export -f yq; exec bash "$@"',
            '--',
            _script,
            'arms',
            'csproj',
        ],
        { cwd: directory.path, encoding: 'utf8' },
    );
    assert.equal(result.status, 1);
    assert.equal(result.stdout.includes('generated snapshot unreadable'), true);
    assert.equal(result.stdout.includes('job died, arms unproven: xml-name'), true);
    assert.equal(result.stdout.includes('uncovered arm:'), false);
});

test('fixture pairing rejects repeated ids, repeated source and non-string cases', (): void => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    _project(directory.path);
    const casesFile = join(directory.path, 'tests/xml-name-test.yml');
    const original = readFileSync(casesFile, 'utf8');
    execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
    for (const [document, diagnostic] of [
        [{ id: 'xml-name', valid: ['<New/>', '<New/>'], invalid: ['<Old/>'] }, 'duplicate or contradictory case: xml-name'],
        [{ id: 'xml-name', valid: ['<New/>'], invalid: ['<Old/>', '<Old/>'] }, 'duplicate or contradictory case: xml-name'],
        [{ id: 'xml-name', valid: ['<Old/>'], invalid: ['<Old/>'] }, 'duplicate or contradictory case: xml-name'],
        [{ id: 'xml-name', valid: ['<New/>', false], invalid: ['<Old/>'] }, 'non-string case or non-array side: xml-name valid'],
        [{ id: 'xml-name', valid: '<New/>', invalid: ['<Old/>'] }, 'non-string case or non-array side: xml-name valid'],
        [`${original}\n---\n${original}`, 'duplicate test id: xml-name'],
        [{ id: 'xml-name', valid: ['<New/>'], invalid: ['<Old/>'], filename: 'ignored.yml' }, 'unknown key in xml-name: filename'],
    ] as const) {
        writeFileSync(casesFile, typeof document === 'string' ? document : JSON.stringify(document));
        const result = spawnSync('bash', [_script, 'pairing'], { cwd: directory.path, encoding: 'utf8' });
        assert.equal(result.status, 1, diagnostic);
        assert.equal(result.stdout.includes(diagnostic), true, result.stdout + result.stderr);
        assert.equal(result.stderr, '');
    }
});

test('parameterized utilities retain global scope with one caller', (): void => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-checks-'));
    const ruleFile = join(directory.path, 'rules/xml-name.yml');
    const rule = {
        id: 'xml-name',
        language: 'xml',
        severity: 'error',
        rule: { matches: { 'xml-matching-name': { name: { regex: '^Old$' } } } },
        message: 'Use New',
    };
    const config = _project(
        directory.path,
        { utilDirs: ['utils'] },
        {
            'rules/xml-name.yml': rule,
            'utils/xml-matching-name.yml': { id: 'xml-matching-name', language: 'xml', arguments: ['name'], rule: { kind: 'Name', matches: 'name' } },
        },
    );
    execFileSync('ast-grep', ['test', '-U'], { cwd: directory.path, stdio: 'pipe' });
    assert.equal(execFileSync('bash', [_script, 'gate', 'csproj'], { cwd: directory.path, encoding: 'utf8' }), '');
    const configFile = join(directory.path, 'sgconfig.yml');
    const sourceFiles = ['rules/xml-name.yml', 'utils/xml-matching-name.yml'];
    const original = sourceFiles.map((file) => readFileSync(join(directory.path, file), 'utf8'));
    writeFileSync(
        configFile,
        JSON.stringify({
            ...config,
            ruleDirs: [join(directory.path, 'rules')],
            utilDirs: [join(directory.path, 'utils')],
        }),
    );
    assert.equal(execFileSync('bash', [_script, 'gate', 'csproj'], { cwd: directory.path, encoding: 'utf8' }), '');
    assert.deepEqual(
        sourceFiles.map((file) => readFileSync(join(directory.path, file), 'utf8')),
        original,
    );
    writeFileSync(
        join(directory.path, 'utils/xml-matching-name.yml'),
        JSON.stringify({ id: 'xml-matching-name', language: 'xml', rule: { kind: 'Name', regex: '^Old$' } }),
    );
    writeFileSync(ruleFile, JSON.stringify({ ...rule, rule: { matches: 'xml-matching-name' } }));
    const result = spawnSync('bash', [_script, 'arms', 'csproj'], { cwd: directory.path, encoding: 'utf8' });
    assert.equal(result.status, 1);
    assert.equal(result.stdout, 'one rule calls util: xml-matching-name\n');
});
