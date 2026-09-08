// --- [IMPORTS] ------------------------------------------------------------------------

import assert from 'node:assert/strict';
import { Buffer } from 'node:buffer';
import { execFileSync } from 'node:child_process';
import { mkdtempDisposableSync, readdirSync, readFileSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { test } from 'node:test';
import { Schema } from 'effect';

// --- [OUTLINE] ------------------------------------------------------------------------

const githubOutline = [
    '--no-default-outline-rules',
    ...['github-workflow', 'github-job', 'github-composite', 'github-step'].flatMap((id) => ['--outline-rules', `tools/ast-grep/outline/${id}.yml`]),
    '--items',
    'structure',
];

function outline(language: string, source: string, arguments_: string[]): readonly unknown[] {
    return Schema.Struct({ items: Schema.Array(Schema.Unknown) }).pipe(
        (item) => Schema.Tuple(item),
        (schema) => Schema.decodeUnknownSync(Schema.parseJson(schema)),
    )(execFileSync('ast-grep', ['outline', '--stdin', '--lang', language, '--json=compact', ...arguments_], { encoding: 'utf8', input: source }))[0]
        .items;
}

function assertOutline(actual: readonly unknown[], expected: readonly unknown[], view: 'names' | 'members' = 'members'): void {
    const identity = Schema.Struct({ name: Schema.String });
    const project = (view === 'names' ? identity : Schema.Struct({ ...identity.fields, members: Schema.optional(Schema.Array(identity)) })).pipe(
        Schema.Array,
        Schema.decodeUnknownSync,
    );
    // biome-ignore-start lint/suspicious/noMisplacedAssertion: Registered tests call this shared synchronous assertion
    assert.deepStrictEqual(project(actual), project(expected));
    assert.partialDeepStrictEqual(actual, expected);
    // biome-ignore-end lint/suspicious/noMisplacedAssertion: Registered tests call this shared synchronous assertion
}

// --- [TARGET] ---------------------------------------------------------------------------

test('the outline target discovers every extractor under tools/ast-grep/outline once', (): void => {
    const ids = readdirSync('tools/ast-grep/outline')
        .filter((file) => file.endsWith('.yml'))
        .map((file) => file.slice(0, -'.yml'.length))
        .sort();
    const printed = execFileSync(
        'pnpm',
        ['exec', 'nx', 'run', 'rasm:outline', '--', 'tools/ast-grep/outline', '--items', 'structure', '--json=compact'],
        {
            encoding: 'utf8',
        },
    )
        .split('\n')
        .filter((line) => line.startsWith('[{'));
    const names = Schema.decodeUnknownSync(
        Schema.parseJson(Schema.Array(Schema.Struct({ items: Schema.Array(Schema.Struct({ name: Schema.String })) }))),
    )(printed[0])
        .flatMap((file) => file.items.map((item) => item.name))
        .sort();
    assert.equal(printed.length, 1);
    assert.deepStrictEqual(names, ids);
    assert.ok(ids.includes('ast-grep-rule') && ids.includes('msbuild-target'));
});

// --- [EXTRACTORS] -----------------------------------------------------------------------

test('configured XML targets accept quoted delimiters, direct tasks, and target-local groups', (): void => {
    const source = `<Project>
  <!-- π🙂 -->
  <Target Condition="'x' > 'a'" Name="Build">
    <Message Importance="high" Text="ok" />
    <PropertyGroup><Message>Not a task</Message></PropertyGroup>
    <ItemGroup Label='Local'><Compile Include="a.cs" /></ItemGroup>
    <Exec Command="echo"><PropertyGroup><Hidden>1</Hidden></PropertyGroup></Exec>
  </Target>
  <PropertyGroup><Top>1</Top></PropertyGroup>
  <ProjectExtensions><Project><Target Name="Fake"><PropertyGroup /></Target></Project></ProjectExtensions>
</Project>`;
    const actual = outline('xml', source, [
        '--outline-rules',
        'tools/ast-grep/outline/msbuild-task.yml',
        '--outline-rules',
        'tools/ast-grep/outline/msbuild-target-group.yml',
        '--items',
        'all',
        '--view',
        'expanded',
    ]);
    assertOutline(actual, [
        {
            members: [
                { name: 'Message', signature: '<Message Importance="high" Text="ok" />', symbolType: 'method' },
                {
                    name: 'PropertyGroup',
                    signature: '<PropertyGroup>',
                    symbolType: 'object',
                    isPublic: true,
                    range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('<PropertyGroup><Message>'))) } },
                },
                { name: 'ItemGroup', signature: "<ItemGroup Label='Local'>", symbolType: 'object' },
                { name: 'Exec', signature: '<Exec Command="echo">' },
            ],
            name: 'Build',
            signature: '<Target Condition="\'x\' > \'a\'" Name="Build">',
        },
    ]);
});

test('injected Bash outline ranges remain relative to the YAML host', (): void => {
    const source =
        '# π🙂\njobs:\n  build:\n    steps:\n      - shell: bash\n        run: |\n          first() { :; }\n      - shell: bash\n        run: |\n          second() { :; }\n';
    const actual = outline('yaml', source, ['--outline-rules', 'tools/ast-grep/outline/bash-function.yml', '--items', 'all', '--view', 'expanded']);
    assertOutline(actual, [
        {
            name: 'first',
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('first()'))) } },
        },
        {
            name: 'second',
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('second()'))) } },
        },
    ]);
});

test('Bash outline includes compound declarations without nested functions', (): void => {
    const source = `if true; then conditional() { :; }; fi
case value in value) selected() { :; } ;; esac
{ grouped() { :; }; }
outer() { nested() { :; }; }
( isolated() { :; }; )`;
    const actual = outline('bash', source, [
        '--outline-rules',
        'tools/ast-grep/outline/bash-function.yml',
        '--items',
        'structure',
        '--view',
        'expanded',
    ]);
    assertOutline(actual, [
        { name: 'conditional', isExported: false },
        { name: 'selected', isExported: false },
        { name: 'grouped', isExported: false },
        { name: 'outer', isExported: false },
        { name: 'isolated', isExported: false },
    ]);
    assert.equal(
        execFileSync('bash', ['-c', `${source}\ndeclare -F conditional selected grouped outer\nif declare -F nested isolated; then exit 1; fi`], {
            encoding: 'utf8',
        }),
        'conditional\nselected\ngrouped\nouter\n',
    );
});

test('native Markdown outline retains heading ranges and excludes code blocks', (): void => {
    const source =
        'π🙂\n\n# Title #\n\nBody\n\n## Child **bold**\n\nSetext\nheading\n-------\n\n> # Quoted\n\n- # Listed\n\n```markdown\n# Fenced\n```\n\n    # Indented\n\n###\n';
    const actual = outline('markdown', source, ['--items', 'structure']);
    assertOutline(actual, [
        {
            name: 'Title #',
            signature: '# Title #',
            isExported: true,
            range: {
                byteOffset: {
                    start: Buffer.byteLength(source.slice(0, source.indexOf('# Title'))),
                    end: Buffer.byteLength(source.slice(0, source.indexOf('\n\nBody') + 1)),
                },
            },
        },
        { name: 'Child **bold**', signature: '## Child **bold**' },
        {
            name: 'Setext\nheading',
            astKind: 'setext_heading',
            range: {
                byteOffset: {
                    start: Buffer.byteLength(source.slice(0, source.indexOf('Setext'))),
                    end: Buffer.byteLength(source.slice(0, source.indexOf('\n\n>') + 1)),
                },
            },
        },
        { name: 'Quoted' },
        { name: 'Listed' },
    ]);
});

test('MSBuild evaluation groups attach properties and item operations without target locals or metadata', (): void => {
    const source = `<Project>
  <!-- π🙂 -->
  <PropertyGroup Condition="'$(Mode)' == 'fast'">
    <Mode Condition="'$(Mode)' == ''">fast</Mode>
    <Empty />
    <Xml><ItemGroup><HiddenItem Include="raw" /></ItemGroup></Xml>
  </PropertyGroup>
  <ItemGroup Condition="'$(Mode)' == 'fast'" Label='Source
    Files'>
    <Compile Include="**/*.cs"><Visible>false</Visible><Raw><PropertyGroup><HiddenProperty>raw</HiddenProperty></PropertyGroup></Raw></Compile>
    <None Update="config.json" CopyToOutputDirectory="PreserveNewest" />
    <Compile Remove="generated/*.cs" />
  </ItemGroup>
  <Choose><When Condition="true"><Choose><Otherwise>
    <PropertyGroup Label="Chosen"><Chosen>yes</Chosen></PropertyGroup>
  </Otherwise></Choose></When></Choose>
  <ProjectExtensions><PropertyGroup><Extension>raw</Extension></PropertyGroup>
    <Project><PropertyGroup><EmbeddedProject>raw</EmbeddedProject></PropertyGroup></Project>
  </ProjectExtensions>
  <Target Name="Build"><PropertyGroup Label="Local"><Local>1</Local></PropertyGroup></Target>
</Project>`;
    const actual = outline('xml', source, [
        '--outline-rules',
        'tools/ast-grep/outline/msbuild-group.yml',
        '--outline-rules',
        'tools/ast-grep/outline/msbuild-property.yml',
        '--outline-rules',
        'tools/ast-grep/outline/msbuild-item.yml',
        '--type',
        'object',
        '--items',
        'structure',
        '--view',
        'expanded',
    ]);
    assertOutline(actual, [
        {
            name: 'PropertyGroup',
            signature: "<PropertyGroup Condition=\"'$(Mode)' == 'fast'\">",
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('<PropertyGroup'))) } },
            members: [
                { name: 'Mode', signature: "<Mode Condition=\"'$(Mode)' == ''\">fast</Mode>" },
                { name: 'Empty', signature: '<Empty />' },
                { name: 'Xml', signature: '<Xml><ItemGroup><HiddenItem Include="raw" /></ItemGroup></Xml>' },
            ],
        },
        {
            name: 'ItemGroup Source Files',
            signature: "<ItemGroup Condition=\"'$(Mode)' == 'fast'\" Label='Source\n    Files'>",
            members: [
                { name: 'Compile "**/*.cs"', signature: '<Compile Include="**/*.cs">' },
                { name: 'None "config.json"', signature: '<None Update="config.json" CopyToOutputDirectory="PreserveNewest" />' },
                { name: 'Compile "generated/*.cs"', signature: '<Compile Remove="generated/*.cs" />' },
            ],
        },
        { name: 'PropertyGroup Chosen', members: [{ name: 'Chosen', signature: '<Chosen>yes</Chosen>' }] },
    ]);
});

test('outline includes module type aliases with generic defaults', (): void => {
    const source = `# π🙂
type Result[T] = T | Exception
type Default[T = str] = tuple[T, ...]
type _Private = str
if enabled:
    type Conditional = int
class Container:
    type Member = str
    def method(self):
        type Nested = int
def factory():
    type Local = int
`;
    const actual = outline('python', source, [
        '--outline-rules',
        'tools/ast-grep/outline/python-type-alias.yml',
        '--items',
        'exports',
        '--view',
        'expanded',
        '--type',
        'struct',
    ]);
    assertOutline(actual, [
        {
            name: 'Result',
            symbolType: 'struct',
            signature: 'type Result[T] = T | Exception',
            isExported: true,
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('type Result'))) } },
        },
        { name: 'Default', signature: 'type Default[T = str] = tuple[T, ...]' },
        { name: '_Private', signature: 'type _Private = str' },
        { name: 'Conditional', signature: 'type Conditional = int' },
    ]);
});
test('TypeScript members preserve direct object and imported Struct fields', (): void => {
    const source = `// π🙂
import { Schema /* source */ as S } from 'effect';
import * as NS from 'effect/Schema';
import { Struct as shape, String as Text } from 'effect/Schema';
const Person = S.Struct(/* fields */ { name: S.String, nested: S.Struct({ hidden: S.String }), ...fields, [key]: S.Number }, /* index */ { key: S.String, value: S.Number });
const Other = NS.Struct({ other: NS.String });
export const Direct = shape({ text: Text });
const record = { read: (key: string) => key, nested: { hidden: true }, value } as const satisfies Record<string, unknown>;
const Schema = { Struct: (value: unknown) => value };
const Local = Schema.Struct({ hidden: true });
function inner(S: unknown) { const Hidden = S.Struct({ hidden: true }); }
const callback = (() => { const local = { localOnly: true }; return local; })();
const schemaCallback = (() => { const Local = S.Struct({ shadow: S.String }); return Local; })();
const wrapped = (S.Struct({ wrappedField: S.String })) satisfies object;
`;
    const arguments_ = ['--outline-rules', 'tools/ast-grep/outline/typescript-object-member.yml', '--items', 'structure'];
    const actual = outline('tsx', source, [...arguments_, '--view', 'expanded']);
    assertOutline(actual, [
        { name: 'Person', members: [{ name: 'name', signature: 'name: S.String' }, { name: 'nested' }, { name: '[key]' }] },
        { name: 'Other', members: [{ name: 'other' }] },
        { name: 'Direct', members: [{ name: 'text' }] },
        { name: 'record', members: [{ name: 'read' }, { name: 'nested' }, { name: 'value', signature: 'value' }] },
        { name: 'Schema', members: [{ name: 'Struct' }] },
        { name: 'Local' },
        { name: 'inner' },
        { name: 'callback' },
        { name: 'schemaCallback' },
        { name: 'wrapped', members: [{ name: 'wrappedField' }] },
    ]);
    assert.partialDeepStrictEqual(outline('tsx', source, [...arguments_, '--view', 'digest']), [
        { name: 'Person', members: [{ name: 'name', signature: '' }] },
    ]);
    for (const imported of ["import { Schema } from 'other';", "import type { Schema } from 'effect';", "import { type Schema } from 'effect';"]) {
        const expanded = outline('tsx', `${imported}\nconst Person = Schema.Struct({ hidden: true });`, [...arguments_, '--view', 'expanded']);
        assertOutline(expanded, [{ name: 'Person' }]);
    }
});

test('MSBuild maps only project declarations and preserves quoted header values', (): void => {
    const source = `<Project>
  <!-- λ -->
  <Target Name="Build" Condition="'a  b' == 'a  b'">
    <Message Text="two  spaces" />
    <PropertyGroup><Payload><Target Name="PropertyTarget"><HiddenTask /></Target></Payload></PropertyGroup>
    <OnError ExecuteTargets="Recover" />
  </Target>
  <Target Name="Empty" />
  <ProjectExtensions><Project><Target Name="Fake"><FakeTask /></Target></Project></ProjectExtensions>
  <PropertyGroup><Nested><Target Name="Nested" /></Nested></PropertyGroup>
</Project>`;
    const arguments_ = ['--outline-rules', 'tools/ast-grep/outline/msbuild-task.yml', '--items', 'all'];
    for (const view of ['names', 'signatures', 'digest']) {
        const actual = outline('xml', source, [...arguments_, '--view', view]);
        assertOutline(actual, [{ name: 'Build' }, { name: 'Empty' }], 'names');
    }
    const expanded = outline('xml', source, [...arguments_, '--view', 'expanded']);
    assertOutline(expanded, [
        {
            name: 'Build',
            signature: '<Target Name="Build" Condition="\'a  b\' == \'a  b\'">',
            members: [{ name: 'Message', signature: '<Message Text="two  spaces" />' }],
        },
        { name: 'Empty' },
    ]);
});

test('workflow outlines include nested parallel steps and synchronization operations', (): void => {
    const source = `jobs:
  build:
    steps:
      - parallel:
          - id: frontend
            run: nx build frontend
          - parallel:
              - run: nx build backend
      - run: |
          nx build docs
          nx test docs
      - wait: frontend
      - wait-all:
      - cancel: monitor
`;
    const actual = outline('yaml', source, [...githubOutline, '--view', 'expanded']);
    assertOutline(actual, [
        {
            name: 'build',
            members: [
                { name: 'frontend', signature: 'run: nx build frontend' },
                { name: 'nx build backend', signature: 'run: nx build backend' },
                { name: 'nx build docs', signature: 'run: |\n          nx build docs\n          nx test docs' },
                { name: 'frontend', signature: 'wait: frontend' },
                { name: 'wait-all', signature: 'wait-all:' },
                { name: 'monitor', signature: 'cancel: monitor' },
            ],
        },
    ]);
    const composite = 'name: Fixture\nruns: {using: composite, steps: [{parallel: [{run: hidden}]}]}';
    const expanded = outline('yaml', composite, [...githubOutline, '--view', 'expanded']);
    assertOutline(expanded, [{ name: 'Fixture' }]);
});

test('GitHub workflow outlines retain ordered jobs, calls, names and source ranges', (): void => {
    const source = `# π🙂
jobs:
  build:
    needs: [lint,
      check]
    if: |
      always()
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v7
      - name: Check
        id: check
        run: |
          nx lint
          nx test
        env:
          steps: [{name: Hidden, run: ignored}]
      - id: report
        if: failure()
        run: nx report
  reusable:
    uses: ./.github/workflows/native.yml
  bare:
    steps: []
other:
  jobs:
    nested:
      runs-on: hidden
      steps: [{run: ignored}]
`;
    for (const view of ['names', 'signatures', 'digest']) {
        const actual = outline('yaml', source, [...githubOutline, '--view', view]);
        assertOutline(actual, [{ name: 'build' }, { name: 'reusable' }, { name: 'bare' }], 'names');
    }
    const expanded = outline('yaml', source, [...githubOutline, '--view', 'expanded']);
    assertOutline(expanded, [
        {
            name: 'build',
            signature: 'needs: [lint, check], if: always(), runs-on: ubuntu-latest',
            isExported: true,
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('build:'))) } },
            members: [
                { name: 'actions/checkout@v7', signature: 'uses: actions/checkout@v7' },
                {
                    name: 'Check',
                    signature: 'run: |\n        nx lint\n        nx test',
                    range: {
                        byteOffset: {
                            start: Buffer.byteLength(source.slice(0, source.indexOf('name: Check'))),
                            end: Buffer.byteLength(source.slice(0, source.indexOf('      - id: report') - 1)),
                        },
                    },
                },
                { name: 'report', signature: 'run: nx report' },
            ],
        },
        { name: 'reusable', signature: 'uses: ./.github/workflows/native.yml' },
        { name: 'bare', signature: '' },
    ]);
});

test('GitHub outlines support flow mappings and quoted structural keys', (): void => {
    const source = `{"jobs": {'build': {"steps": [{"name": "Flow", 'run': nx lint}, {id: check, uses: ./action}]}}}`;
    const actual = outline('yaml', source, [...githubOutline, '--view', 'expanded']);
    assertOutline(actual, [
        {
            name: "'build'",
            members: [
                { name: '"Flow"', signature: "'run': nx lint" },
                { name: 'check', signature: 'uses: ./action' },
            ],
        },
    ]);
});

test('composite action outlines include direct steps and exclude JavaScript actions and nested data', (): void => {
    const source = `# π🙂
name: Setup
description: Installs the
  toolchain
inputs:
  rid:
    description: hidden
runs:
  using: composite
  steps:
    - name: Install
      run: pnpm install
      shell: bash
    - uses: ./another
      with:
        steps: [{run: hidden}]
`;
    const actual = outline('yaml', source, [...githubOutline, '--view', 'expanded']);
    assertOutline(actual, [
        {
            name: 'Setup',
            signature: 'Installs the toolchain',
            isExported: true,
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('runs:'))) } },
            members: [
                { name: 'Install', signature: 'run: pnpm install' },
                { name: './another', signature: 'uses: ./another' },
            ],
        },
    ]);
    for (const nearMiss of [
        source.replace('using: composite', 'using: node24'),
        'data: {name: Hidden, runs: {using: composite, steps: [{run: hidden}]}}',
        'steps: [{run: hidden}]',
    ]) {
        const expanded = outline('yaml', nearMiss, [...githubOutline, '--view', 'expanded']);
        assertOutline(expanded, []);
    }
    const quoted = outline('yaml', `{"name": "Quoted", 'runs': {"using": "composite", steps: [{run: nx lint}]}}`, [
        ...githubOutline,
        '--view',
        'expanded',
    ]);
    assertOutline(quoted, [{ name: '"Quoted"', signature: '', members: [{ name: 'nx lint', signature: 'run: nx lint' }] }]);
});

test('mixed YAML containers retain written anchor steps without expanding aliases', (): void => {
    const source = `jobs:
  mixed:
    steps:
      - &install {name: Install, run: pnpm install}
      - *install
      - {id: check, run: nx check, with: {steps: [{run: hidden}]}}
  flow:
    steps: [{run: nx report}]
`;
    const actual = outline('yaml', source, [...githubOutline, '--view', 'expanded']);
    assertOutline(actual, [
        {
            name: 'mixed',
            members: [
                { name: 'Install', signature: 'run: pnpm install' },
                { name: 'check', signature: 'run: nx check' },
            ],
        },
        { name: 'flow', members: [{ name: 'nx report', signature: 'run: nx report' }] },
    ]);
});

test('ast-grep rule outlines name the id and sign severity, files, util calls, and fix presence', (): void => {
    const ruleOutline = ['--no-default-outline-rules', '--outline-rules', 'tools/ast-grep/outline/ast-grep-rule.yml', '--items', 'structure'];
    const rule = `# yaml-language-server: $schema=https://raw.githubusercontent.com/ast-grep/ast-grep/main/schemas/rule.json
id: no-item
language: yaml
severity: error
files:
    - Item/**
    - Other/**
utils:
    item-shape: {kind: word}
rule: {matches: item-shape, has: {matches: {item-slot: {slot: {kind: word}}}}}
fix: Item
`;
    assertOutline(
        outline('yaml', rule, ruleOutline),
        [{ name: 'no-item', signature: 'error, files - Item/** - Other/**, matches item-shape, matches item-slot, fix', symbolType: 'module' }],
        'names',
    );
    assertOutline(
        outline('yaml', 'id: item-shape\nlanguage: yaml\nrule: {kind: word}\n', ruleOutline),
        [{ name: 'item-shape', signature: '' }],
        'names',
    );
    assertOutline(outline('yaml', 'id: no-item\nvalid:\n    - Item\n', ruleOutline), [], 'names');
});

test('ast-grep rule outlines attach local utils with their first line', (): void => {
    const source = `# π🙂
id: fixture-rule
language: yaml
utils:
    first: {kind: word}
    second: # A comment
        kind: word
        inside: {kind: sentence}
    'quoted': {kind: letter}
rule:
    matches: first
    has: {matches: {third: {slot: {kind: word}}}}
    utils:
        hidden: {kind: word}
constraints:
    utils: {hidden: 1}
`;
    const utilOutline = [
        '--no-default-outline-rules',
        '--outline-rules',
        'tools/ast-grep/outline/ast-grep-rule.yml',
        '--outline-rules',
        'tools/ast-grep/outline/ast-grep-util.yml',
        '--items',
        'structure',
    ];
    assertOutline(outline('yaml', source, [...utilOutline, '--view', 'expanded']), [
        {
            name: 'fixture-rule',
            signature: 'matches first, matches third',
            members: [
                {
                    name: 'first',
                    signature: 'first: {kind: word}',
                    symbolType: 'function',
                    isPublic: false,
                    range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('first:'))) } },
                },
                { name: 'second', signature: 'second: # A comment' },
                { name: "'quoted'", signature: "'quoted': {kind: letter}" },
            ],
        },
    ]);
    assertOutline(outline('yaml', '{id: flow, rule: {kind: word}, utils: {inline: {kind: word}}}', [...utilOutline, '--view', 'expanded']), [
        { name: 'flow', members: [{ name: 'inline', signature: 'inline: {kind: word}' }] },
    ]);
});

test('ast-grep test outlines name cases by their first lines and mark valid cases public', (): void => {
    const source = `# π🙂 header
id: fixture-rule
'valid':
    - one
    - |-
      two
      more
    - three
invalid:
    - '[[ x ]]'
    - |
      valid:
          - four
`;
    const testOutline = [
        '--no-default-outline-rules',
        '--outline-rules',
        'tools/ast-grep/outline/ast-grep-test.yml',
        '--outline-rules',
        'tools/ast-grep/outline/ast-grep-case.yml',
        '--items',
        'structure',
    ];
    assertOutline(outline('yaml', source, [...testOutline, '--view', 'expanded']), [
        {
            name: 'fixture-rule',
            signature: "'valid', invalid",
            symbolType: 'module',
            members: [
                {
                    name: 'one',
                    signature: 'one',
                    symbolType: 'string',
                    isPublic: true,
                    range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('- one'))) } },
                },
                { name: 'two', signature: 'two', isPublic: true },
                { name: 'three', signature: 'three', isPublic: true },
                { name: "'[[ x ]]'", signature: "'[[ x ]]'", isPublic: false },
                { name: 'valid:', signature: 'valid:', isPublic: false },
            ],
        },
    ]);
    assertOutline(outline('yaml', source, [...testOutline, '--pub-members', '--view', 'digest']), [
        { name: 'fixture-rule', members: [{ name: 'one' }, { name: 'two' }, { name: 'three' }] },
    ]);
    assertOutline(outline('yaml', '{id: flow-test, valid: [a], invalid: []}', [...testOutline, '--view', 'expanded']), [
        { name: 'flow-test', signature: 'valid, invalid' },
    ]);
    assertOutline(outline('yaml', 'id: fixture-rule\nsnapshots:\n  code:\n    fixed: x\n', testOutline), []);
});

test('GitHub workflow outlines name the workflow and sign its trigger keys', (): void => {
    const source = `# π🙂
name: Fixture
on:
    pull_request:
    push:
        branches: [main]
    workflow_dispatch:
        inputs:
            on: hidden
jobs:
    build:
        runs-on: ubuntu-24.04
        on: hidden
        steps:
            - run: echo
`;
    assertOutline(outline('yaml', source, [...githubOutline, '--view', 'expanded']), [
        {
            name: 'Fixture',
            signature: 'on: pull_request, push, workflow_dispatch',
            symbolType: 'module',
            isExported: true,
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('on:'))) } },
        },
        { name: 'build', signature: 'runs-on: ubuntu-24.04', members: [{ name: 'echo', signature: 'run: echo' }] },
    ]);
    assertOutline(outline('yaml', '{"name": "Flow", on: [push, "pull_request"], jobs: {}}', [...githubOutline, '--view', 'expanded']), [
        { name: '"Flow"', signature: 'on: push, "pull_request"' },
    ]);
    assertOutline(outline('yaml', 'name: Scalar\non: push\njobs: {}\n', [...githubOutline, '--view', 'expanded']), [
        { name: 'Scalar', signature: 'on: push' },
    ]);
    assertOutline(outline('yaml', 'on:\n    push:\n', [...githubOutline, '--view', 'expanded']), []);
});

test('Nx target outlines sign commands, executors, and dependencies with configuration members', (): void => {
    const source = `{
    "name": "π🙂",
    "nx": {
        "targets": {
            "build": {
                "command": "tsc -b",
                "dependsOn": ["^build",
                    {"projects": ["x"], "target": "restore"}],
                "options": {"command": "hidden"},
                "configurations": {
                    "fast": {"command": "tsc -b --fast"},
                    "slow": {"commands": ["one",
                        "two"], "dependsOn": ["hidden"]},
                    "text": "hidden"
                }
            },
            "empty": {},
            "text": "hidden",
            "check": {"executor": "nx:noop", "nested": {"targets": {"deep": {"command": "hidden"}}}}
        }
    }
}`;
    const nxOutline = [
        '--no-default-outline-rules',
        '--outline-rules',
        'tools/ast-grep/outline/nx-target.yml',
        '--outline-rules',
        'tools/ast-grep/outline/nx-target-configuration.yml',
        '--items',
        'structure',
    ];
    assertOutline(outline('json', source, [...nxOutline, '--view', 'expanded']), [
        {
            name: 'build',
            signature: 'tsc -b, dependsOn ["^build", {"projects": ["x"], "target": "restore"}]',
            symbolType: 'function',
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('"build"'))) } },
            members: [
                { name: 'fast', signature: 'tsc -b --fast', isPublic: true },
                { name: 'slow', signature: '["one", "two"]' },
            ],
        },
        { name: 'empty', signature: '' },
        { name: 'check', signature: 'nx:noop' },
    ]);
    const defaults = `{"targetDefaults": {"lint": [{"dependsOn": ["^build"], "configurations": {"ci": {"command": "lint --ci"}}}, {"cache": true}]}, "targets": {"root": {"executor": "run"}}}`;
    assertOutline(outline('json', defaults, [...nxOutline, '--view', 'expanded']), [
        { name: 'lint', signature: 'dependsOn ["^build"]', members: [{ name: 'ci', signature: 'lint --ci' }] },
        { name: 'root', signature: 'run' },
    ]);
});

test('Markdown section outlines name html blocks and underscore tag paragraphs as sections and sign the opening tag', (): void => {
    const source =
        'π🙂 intro\n\n# [TITLE]\n\n<role>\nYou are x.\n</role>\n\n<context_gathering>\nRead <not_tag> first.\n</context_gathering>\n\n- item\n\n  <nested>\n  inner\n  </nested>\n\n```md\n<fenced>\n```\n\n<gate>\n- `check`\n</gate>\n';
    const sectionOutline = ['--outline-rules', 'tools/ast-grep/outline/markdown-section.yml', '--items', 'structure'];
    assertOutline(outline('markdown', source, [...sectionOutline, '--view', 'expanded']), [
        { name: '[TITLE]' },
        {
            name: 'section',
            signature: '<role>',
            symbolType: 'namespace',
            isExported: true,
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('<role>'))) } },
        },
        {
            name: 'section',
            signature: '<context_gathering>',
            astKind: 'paragraph',
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('<context_gathering>'))) } },
        },
        { name: 'section', signature: '<nested>' },
        { name: 'section', signature: '<gate>' },
    ]);
    assertOutline(outline('markdown', source, [...sectionOutline, '--match', '<gate>', '--view', 'expanded']), [
        { name: 'section', signature: '<gate>' },
    ]);
});

test('TypeScript ambient module members sign headers without bodies and derive visibility from export', (): void => {
    const source = `// π🙂
declare module 'fixture' {
  export type Event<T = string> = { name: T };
  type Hidden = Record<string, { a: (b: Map<K, V>) => void }>;
  export interface Row<T> extends Base<T>,
    Other { m(a: { x: 1 }): void; }
  interface Plain { p: string }
  function f(a: { x: 1 }): Promise<void>;
  export namespace N { type Deep = 1; function g(): void; }
  export function exported(): void;
}
declare module 'second' { type Two = 2; }
type Outside = 1;
`;
    const arguments_ = ['--outline-rules', 'tools/ast-grep/outline/typescript-ambient-member.yml', '--items', 'structure'];
    assertOutline(outline('tsx', source, [...arguments_, '--view', 'expanded']), [
        {
            name: 'fixture',
            members: [
                {
                    name: 'Event',
                    signature: 'type Event<T = string>',
                    symbolType: 'struct',
                    isPublic: true,
                    range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf('type Event'))) } },
                },
                { name: 'Hidden', signature: 'type Hidden', isPublic: false },
                { name: 'Row', signature: 'interface Row<T> extends Base<T>, Other', isPublic: true },
                { name: 'Plain', signature: 'interface Plain', isPublic: false },
                { name: 'f', signature: 'function f(a: { x: 1 }): Promise<void>', isPublic: false },
                { name: 'exported', signature: 'function exported(): void', isPublic: true },
            ],
        },
        { name: 'second', members: [{ name: 'Two', signature: 'type Two' }] },
        { name: 'Outside' },
    ]);
    assertOutline(outline('tsx', source, [...arguments_, '--pub-members', '--view', 'digest']), [
        { name: 'fixture', members: [{ name: 'Event' }, { name: 'Row' }, { name: 'exported' }] },
        { name: 'second' },
        { name: 'Outside' },
    ]);
});

test('TypeScript hook registrations name the event and sign the selector without the handler', (): void => {
    const source = `// π🙂
export const stamps = (on: Register): void => {
    on('skill.prompt', { skill: 'ast-grep',
        nested: { on: 1 } }, async ($, e, next) => { on('inner.event', () => 1); return next(e); });
    on('prompt.submit', (e) =>
        e,
    );
    on('tool.call', handler);
    on(\`template\`, () => 1);
    off('not.on', () => 1);
};
const local = (on: Register): void => { on('local.event', () => 1); };
function plain(on: Register) { on('function.event', () => 1); }
`;
    const actual = outline('tsx', source, [
        '--outline-rules',
        'tools/ast-grep/outline/typescript-hook-registration.yml',
        '--items',
        'structure',
        '--view',
        'expanded',
    ]);
    assertOutline(actual, [
        {
            name: 'stamps',
            members: [
                {
                    name: 'skill.prompt',
                    signature: "on('skill.prompt', { skill: 'ast-grep', nested: { on: 1 } })",
                    symbolType: 'event',
                    isPublic: true,
                    range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf("on('skill"))) } },
                },
                { name: 'prompt.submit', signature: "on('prompt.submit')" },
                { name: 'tool.call', signature: "on('tool.call')" },
            ],
        },
        { name: 'local', members: [{ name: 'local.event', signature: "on('local.event')" }] },
        { name: 'plain' },
    ]);
});

test('TypeScript test outlines name suites and cases by their titles as non-exported items', (): void => {
    const source = `// π🙂
import { describe, it, test } from 'node:test';
describe('outer suite', () => {
    it('direct case', () => {});
    test.skip(\`template case\`, () => {});
    describe('inner', () => { it('nested case', () => {}); });
    helper('not a case', () => {});
});
test('top case', () => {});
it.only("double quoted", () => {});
function wrap() { test('hidden', () => {}); }
describe(dynamicTitle, () => {});
describe('table suite', () => {
    it.each([1, 2])('table case %j', (n) => n);
});
layer(NodeContext.layer)('layer suite', (test) => {
    test.effect('effect case', () => {});
});
`;
    const arguments_ = [
        '--outline-rules',
        'tools/ast-grep/outline/typescript-test.yml',
        '--outline-rules',
        'tools/ast-grep/outline/typescript-test-case.yml',
        '--type',
        'function,method',
    ];
    assertOutline(outline('tsx', source, [...arguments_, '--items', 'structure', '--view', 'expanded']), [
        {
            name: 'outer suite',
            signature: "describe('outer suite')",
            symbolType: 'function',
            isExported: false,
            range: { byteOffset: { start: Buffer.byteLength(source.slice(0, source.indexOf("describe('outer"))) } },
            members: [
                { name: 'direct case', signature: "it('direct case')", symbolType: 'method', isPublic: true },
                { name: 'template case', signature: 'test.skip(`template case`)' },
            ],
        },
        { name: 'top case', signature: "test('top case')" },
        { name: 'double quoted', signature: 'it.only("double quoted")' },
        { name: 'wrap' },
        { name: 'table suite', members: [{ name: 'table case %j', signature: "it.each([1, 2])('table case %j')" }] },
        {
            name: 'layer suite',
            signature: "layer(NodeContext.layer)('layer suite')",
            members: [{ name: 'effect case', signature: "test.effect('effect case')" }],
        },
    ]);
    assertOutline(outline('tsx', source, [...arguments_, '--items', 'exports']), []);
});

// --- [TEMPLATES] -----------------------------------------------------------------------

test('paired outline templates compile and derive visibility', (): void => {
    using directory = mkdtempDisposableSync(join(tmpdir(), 'ast-grep-outline-'));
    const item = join(directory.path, 'item.yml');
    const member = join(directory.path, 'member.yml');
    writeFileSync(
        item,
        readFileSync('.claude/skills/ast-grep/templates/outline-item.yml', 'utf8')
            .replaceAll('<item-extractor-id>', 'test-class')
            .replaceAll('<language>', 'tsx')
            .replaceAll('<lsp-symbol-kind>', 'class')
            .replaceAll('<item-kind>', 'class_declaration')
            .replaceAll('<module-kind>', 'program')
            .replaceAll('<export-kind>', 'export_statement'),
    );
    writeFileSync(
        member,
        readFileSync('.claude/skills/ast-grep/templates/outline-member.yml', 'utf8')
            .replaceAll('<member-extractor-id>', 'test-method')
            .replaceAll('<item-extractor-id>', 'test-class')
            .replaceAll('<sibling-item-extractor-id>', 'test-class')
            .replaceAll('<language>', 'tsx')
            .replaceAll('<lsp-symbol-kind>', 'method')
            .replaceAll('<item-kind>', 'class_declaration')
            .replaceAll('<container-kind>', 'class_body')
            .replaceAll('<member-kind>', 'method_definition')
            .replaceAll('<visibility-kind>', 'accessibility_modifier')
            .replaceAll('<private-marker>', '^(private|protected)$'),
    );
    const actual = outline('tsx', 'export class Item { public read() {} private hidden() {} }', [
        '--no-default-outline-rules',
        '--outline-rules',
        item,
        '--outline-rules',
        member,
        '--pub-members',
        '--view',
        'expanded',
    ]);
    assertOutline(actual, [
        {
            isExported: true,
            members: [{ isPublic: true, name: 'read', signature: 'public read()' }],
            name: 'Item',
            signature: 'class Item',
        },
    ]);
});
