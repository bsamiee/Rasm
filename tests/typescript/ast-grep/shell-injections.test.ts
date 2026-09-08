import assert from 'node:assert/strict';
import { execFileSync, spawnSync } from 'node:child_process';
import { mkdirSync, mkdtempDisposableSync, readFileSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { test } from 'node:test';
import { Schema } from 'effect';

const languageInjections = Schema.decodeUnknownSync(Schema.parseJson())(
    execFileSync('yq', ['-o=json', '.languageInjections', 'sgconfig.yml'], { encoding: 'utf8' }),
);

test('configured shell injections read the run steps of workflow jobs and composite actions', (): void => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'yaml-shell-'));
    const config = join(directory.path, 'sgconfig.yml');
    writeFileSync(
        config,
        JSON.stringify({
            ruleDirs: [resolve('tools/ast-grep/rules/bash/tooling'), resolve('tools/ast-grep/rules/yaml')],
            utilDirs: [resolve('tools/ast-grep/utils/bash'), resolve('tools/ast-grep/utils/yaml')],
            languageInjections,
        }),
    );
    mkdirSync(join(directory.path, '.github/workflows'), { recursive: true });
    const file = join(directory.path, '.github/workflows/test.yml');
    for (const [source, expected] of [
        // A job step, with or without a runner or a shell default, the repository's steps all run under bash
        ['jobs: {build: {runs-on: ubuntu-latest, steps: [{run: npm install}]}}', 1],
        ['jobs: {build: {steps: [{run: npm install}]}}', 1],
        ['defaults: {run: {shell: bash}}\njobs: {build: {defaults: {run: {working-directory: app}}, steps: [{run: npm install}]}}', 1],
        ['jobs: {build: {runs-on: windows-latest, steps: [{shell: bash, run: npm install}]}}', 1],
        ['jobs: {build: {runs-on: ubuntu-latest, steps: [{run: command npm install}]}}', 1],
        // Block style steps and a literal block scalar
        ['jobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - run: |\n          echo ready\n          npm install\n', 1],
        // A parallel group of steps, nested groups included
        ['jobs: {build: {runs-on: ubuntu-latest, steps: [{parallel: [{parallel: [{run: npx nx lint}]}]}]}}', 1],
        // A composite action step
        ['runs: {using: composite, steps: [{run: npm install, shell: bash}]}', 1],
        // Quoted and folded scalars hold escapes or joined lines, and stay outside the injection
        ['jobs: {build: {runs-on: ubuntu-latest, steps: [{run: "npm install"}]}}', 0],
        ['jobs:\n  build:\n    steps:\n      - run: >\n          npm install\n', 0],
        // A run mapping is the defaults entry, and a run key under data or under an action input is no step
        ['defaults: {run: {shell: bash}}\njobs: {build: {steps: [{uses: local/action}]}}', 0],
        ['data: {steps: [{run: npm install}]}', 0],
        ['jobs: {build: {runs-on: ubuntu-latest, steps: [{uses: local/action, with: {steps: [{run: npm install}]}}]}}', 0],
        ['runs: {using: composite, steps: [{parallel: [{run: npm install}]}]}', 0],
        ['jobs: {build: {runs-on: ubuntu-latest, steps: [{run: pnpm install}]}}', 0],
        ['jobs: {build: {runs-on: ubuntu-latest, steps: [{run: echo npm}]}}', 0],
    ] as const) {
        writeFileSync(file, source);
        const result = spawnSync('ast-grep', ['scan', '-c', config, '--filter', '^no-npm-', '--no-ignore', 'hidden', '--json=compact', file], {
            encoding: 'utf8',
        });
        assert.equal(result.status, expected === 0 ? 0 : 1, result.stderr);
        assert.equal(Schema.decodeUnknownSync(Schema.parseJson(Schema.Array(Schema.Unknown)))(result.stdout).length, expected, source);
    }
});

test('configured JSON injections read the command entries of Nx targets', (): void => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'json-shell-'));
    const config = join(directory.path, 'sgconfig.yml');
    writeFileSync(
        config,
        JSON.stringify({
            ruleDirs: [resolve('tools/ast-grep/rules/bash/tooling')],
            utilDirs: [resolve('tools/ast-grep/utils/bash')],
            languageInjections,
        }),
    );
    mkdirSync(join(directory.path, '.claude'));
    for (const [source, expected] of [
        // A command or commands entry of a target under targets or targetDefaults, at the document root or under the nx field
        ['{"targets": {"build": {"executor": "nx:run-commands", "options": {"command": "npm install"}}}}', 1],
        ['{"nx": {"targets": {"build": {"executor": "nx:run-commands", "options": {"command": "npm install"}}}}}', 1],
        ['{"targets": {"build": {"command": "npm install"}}}', 1],
        ['{"targets": {"build": {"executor": "nx:run-commands", "options": {"commands": ["npm install", {"command": "npm install"}]}}}}', 2],
        ['{"targets": {"build": {"executor": "nx:run-commands", "options": {}, "configurations": {"ci": {"command": "npm install"}}}}}', 1],
        ['{"targetDefaults": {"build": [{"executor": "nx:run-commands", "options": {"command": "npm install"}}]}}', 1],
        ['{"targets": {"build": {"executor": "other:task", "options": {"command": "npm install"}}}}', 1],
        // Target defaults keyed by the executor name, and comments beside a command pair
        ['{"targetDefaults": {"nx:run-commands": {"options": {"command": "npm install"}}}}', 1],
        ['{"targets": {"build": {"executor": "nx:run-commands", /* note */ "options": {"command": /* why */ "npm install"}}}}', 1],
        // Env values and metadata hold no command, and a targets key outside the root or the nx field owns no target
        ['{"targets": {"build": {"executor": "nx:run-commands", "options": {"env": {"command": "npm install"}}}}}', 0],
        ['{"targets": {"build": {"metadata": {"command": "npm install"}}}}', 0],
        ['{"command": "npm install", "commands": ["npm install"]}', 0],
        ['{"data": {"targets": {"build": {"executor": "nx:run-commands", "options": {"command": "npm install"}}}}}', 0],
        // A command array and an escaped string are no complete shell source
        ['{"targets": {"build": {"executor": "nx:run-commands", "options": {"command": ["npm install", "foo"]}}}}', 0],
        ['{"targets": {"build": {"executor": "nx:run-commands", "options": {"command": "npm install \\"foo\\""}}}}', 0],
        ['{"hooks": {"SessionStart": [{"hooks": [{"type": "command", "command": "npm install"}]}]}}', 0],
    ] as const) {
        const file = join(directory.path, source.includes('"hooks"') ? '.claude/settings.json' : 'project.json');
        writeFileSync(file, source);
        const result = spawnSync('ast-grep', ['scan', '-c', config, '--no-ignore', 'hidden', '--json=compact', file], { encoding: 'utf8' });
        assert.equal(result.status, expected === 0 ? 0 : 1, result.stderr);
        const findings = Schema.Struct({ ruleId: Schema.String, text: Schema.String }).pipe(Schema.Array, (schema) =>
            Schema.decodeUnknownSync(Schema.parseJson(schema)),
        )(result.stdout);
        assert.deepEqual(
            findings,
            Array.from({ length: expected }, () => ({ ruleId: 'no-npm-command', text: 'npm install' })),
            source,
        );
    }
});

test('Bash rewrites apply inside the injected regions', (): void => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'shell-rewrite-'));
    mkdirSync(join(directory.path, '.github/workflows'), { recursive: true });
    const config = join(directory.path, 'sgconfig.yml');
    writeFileSync(
        config,
        JSON.stringify({
            ruleDirs: [resolve('tools/ast-grep/rewrites/bash')],
            utilDirs: [resolve('tools/ast-grep/utils/bash')],
            languageInjections,
        }),
    );
    for (const [path, source, changes] of [
        // The run-commands executor spawns sh, so a Bash rewrite reads the JSON injection and changes nothing there
        ['project.json', '{"targets":{"task":{"command":"printf x | read value"}}}', false],
        ['.github/workflows/test.yml', 'jobs: {task: {runs-on: ubuntu-latest, steps: [{run: printf x | read value}]}}', true],
        ['.github/workflows/test.yml', 'jobs: {task: {steps: [{uses: local/action, with: {steps: [{run: printf x | read value}]}}]}}', false],
        ['input.sh', 'printf x | read value', true],
    ] as const) {
        const file = join(directory.path, path);
        writeFileSync(file, source);
        execFileSync(
            'ast-grep',
            [
                'scan',
                '-c',
                config,
                '--no-ignore',
                'hidden',
                '--filter',
                '^pipeline-reader-to-process-substitution$',
                '--error=pipeline-reader-to-process-substitution',
                '-U',
                file,
            ],
            { stdio: 'pipe' },
        );
        const fixed = readFileSync(file, 'utf8');
        assert.equal(fixed !== source, changes, source);
        assert.equal(fixed.includes('< <(printf x)'), changes, fixed);
    }
});
