import assert from 'node:assert/strict';
import { execFileSync } from 'node:child_process';
import { mkdirSync, mkdtempDisposableSync, readFileSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { test } from 'node:test';
import { Schema } from 'effect';
import { createPatch, Pointer } from 'rfc6902';

const decoded = (source: string): unknown =>
    Schema.decodeUnknownSync(Schema.parseJson())(
        execFileSync('yq', ['-o=json', '--yaml-fix-merge-anchor-to-spec', '.'], { input: source, encoding: 'utf8' }),
    );

for (const [path, removedKeys] of [
    ['yaml/github/no-unused-artifact-input', ['name', 'compression-level']],
    ['yaml/github/no-redundant-input-required', ['required']],
] satisfies ReadonlyArray<readonly [string, readonly string[]]>) {
    test(`YAML applied edits preserve decoded values and exclusions: ${path}`, (): void => {
        const cases = Schema.Struct({ id: Schema.String, valid: Schema.Array(Schema.String), invalid: Schema.Array(Schema.String) }).pipe((schema) =>
            Schema.decodeUnknownSync(Schema.parseJson(schema)),
        )(execFileSync('yq', ['-o=json', '.', `tools/ast-grep/tests/${path}-test.yml`], { encoding: 'utf8' }));
        using directory = mkdtempDisposableSync(join(tmpdir(), 'yaml-fixes-'));
        const workflow = join(directory.path, '.github/workflows/fixture.yml');
        mkdirSync(join(directory.path, '.github/workflows'), { recursive: true });
        const config = join(directory.path, 'sgconfig.yml');
        writeFileSync(
            config,
            JSON.stringify({
                ruleDirs: [resolve('tools/ast-grep/rules/yaml')],
                utilDirs: [resolve('tools/ast-grep/utils/yaml')],
            }),
        );
        const command = ['scan', '-c', config, '--filter', `^${cases.id}$`, `--error=${cases.id}`, '--no-ignore', 'hidden', '-U', workflow];
        for (const source of cases.valid) {
            writeFileSync(workflow, source);
            execFileSync('ast-grep', command, { stdio: 'pipe' });
            assert.equal(readFileSync(workflow, 'utf8'), source);
        }
        for (const source of cases.invalid) {
            writeFileSync(workflow, source);
            execFileSync('ast-grep', command, { stdio: 'pipe' });
            const fixed = readFileSync(workflow, 'utf8');
            assert.notEqual(fixed, source);
            const before = decoded(source);
            const differences = createPatch(before, decoded(fixed));
            assert.equal(differences.length, 1, source);
            const [difference] = differences;
            assert.ok(difference);
            assert.equal(difference.op, 'remove');
            const removed = Pointer.fromJSON(difference.path).evaluate(before);
            assert.ok(removedKeys.includes(removed.key));
            assert.ok(cases.id !== 'no-redundant-input-required' || removed.value === false);
            execFileSync('ast-grep', command, { stdio: 'pipe' });
            assert.equal(readFileSync(workflow, 'utf8'), fixed);
        }
    });
}
