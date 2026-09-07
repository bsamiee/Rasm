// --- [IMPORTS] ------------------------------------------------------------------------

import assert from 'node:assert/strict';
import { Buffer } from 'node:buffer';
import { execFileSync } from 'node:child_process';
import { mkdtempDisposableSync, readFileSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { test } from 'node:test';
import { Schema } from 'effect';

// --- [OUTLINE] ------------------------------------------------------------------------

const githubOutline = [
    '--no-default-outline-rules',
    ...['github-job', 'github-composite', 'github-step'].flatMap((id) => ['--outline-rules', `tools/ast-grep/outline/${id}.yml`]),
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

// --- [EXTRACTORS] -----------------------------------------------------------------------

test('outline JSON includes direct properties without nested descendants', (): void => {
    const actual = outline('json', '{"name":"π🙂","config":{"direct":{"nested":1},"other":2},"array":[{"excluded":1}]}', [
        '--outline-rules',
        'tools/ast-grep/outline/json-property.yml',
        '--outline-rules',
        'tools/ast-grep/outline/json-property-member.yml',
        '--view',
        'expanded',
    ]);
    assertOutline(actual, [
        { name: '"name"' },
        {
            members: [
                { name: '"direct"', signature: '"direct"' },
                { name: '"other"', signature: '"other"' },
            ],
            name: '"config"',
        },
        { name: '"array"' },
    ]);
});

test('configured XML targets accept quoted delimiters and direct tasks', (): void => {
    const actual = outline(
        'xml',
        `<Project>
  <Target Condition="'x' > 'a'" Name="Build">
    <Message Importance="high" Text="ok" />
    <PropertyGroup><Message>Not a task</Message></PropertyGroup>
  </Target>
  <Import Condition="'x' > 'a'" Project="build.props" />
</Project>`,
        [
            '--outline-rules',
            'tools/ast-grep/outline/msbuild-task.yml',
            '--outline-rules',
            'tools/ast-grep/outline/msbuild-import.yml',
            '--items',
            'all',
            '--view',
            'expanded',
        ],
    );
    assertOutline(actual, [
        {
            members: [
                {
                    name: 'Message',
                    signature: '<Message Importance="high" Text="ok" />',
                },
            ],
            name: 'Build',
            signature: '<Target Condition="\'x\' > \'a\'" Name="Build">',
        },
        { isExported: false, isImport: true, name: 'build.props' },
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
  <ItemGroup Label="Sources">
    <Compile Include="**/*.cs"><Visible>false</Visible><Raw><PropertyGroup><HiddenProperty>raw</HiddenProperty></PropertyGroup></Raw></Compile>
    <None Update="config.json" CopyToOutputDirectory="PreserveNewest" />
    <Compile Remove="generated/*.cs" />
  </ItemGroup>
  <Choose><When Condition="true"><Choose><Otherwise>
    <PropertyGroup><Chosen>yes</Chosen></PropertyGroup>
  </Otherwise></Choose></When></Choose>
  <ProjectExtensions><PropertyGroup><Extension>raw</Extension></PropertyGroup>
    <Project><PropertyGroup><EmbeddedProject>raw</EmbeddedProject></PropertyGroup></Project>
  </ProjectExtensions>
  <Target Name="Build"><PropertyGroup><Local>1</Local></PropertyGroup></Target>
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
            name: 'ItemGroup',
            members: [
                { name: 'Compile "**/*.cs"', signature: '<Compile Include="**/*.cs">' },
                { name: 'None "config.json"', signature: '<None Update="config.json" CopyToOutputDirectory="PreserveNewest" />' },
                { name: 'Compile "generated/*.cs"', signature: '<Compile Remove="generated/*.cs" />' },
            ],
        },
        { name: 'PropertyGroup', members: [{ name: 'Chosen', signature: '<Chosen>yes</Chosen>' }] },
    ]);
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
            .replaceAll('<export-kind>', 'export_statement')
            .replaceAll('<body-kind>', 'class_body'),
    );
    writeFileSync(
        member,
        readFileSync('.claude/skills/ast-grep/templates/outline-member.yml', 'utf8')
            .replaceAll('<member-extractor-id>', 'test-method')
            .replaceAll('<item-extractor-id>', 'test-class')
            .replaceAll('<language>', 'tsx')
            .replaceAll('<lsp-symbol-kind>', 'method')
            .replaceAll('<item-kind>', 'class_declaration')
            .replaceAll('<body-kind>', 'statement_block')
            .replaceAll('<container-kind>', 'class_body')
            .replaceAll('<member-kind>', 'method_definition')
            .replaceAll('<visibility-kind>', 'accessibility_modifier')
            .replaceAll('<public-marker>', '^public$'),
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
    const arguments_ = [
        '--outline-rules',
        'tools/ast-grep/outline/typescript-object-member.yml',
        '--outline-rules',
        'tools/ast-grep/outline/typescript-schema-member.yml',
        '--items',
        'structure',
    ];
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

test('MSBuild task parameters preserve declarations and direct members', (): void => {
    const source = `<Project>
  <!-- λ -->
  <UsingTask TaskName="WriteReport" Condition="'$(Count)' > '0'" TaskFactory="RoslynCodeTaskFactory" AssemblyFile="$(MSBuildToolsPath)/Microsoft.Build.Tasks.Core.dll">
    <ParameterGroup>
      <Source ParameterType="System.String" Required="true" />
      <Count ParameterType="System.Int32" Output="true" />
    </ParameterGroup>
    <Task><Code Type="Fragment" Language="cs"><![CDATA[Count = Source.Length;]]></Code></Task>
  </UsingTask>
  <UsingTask AssemblyFile="tasks.dll" TaskName='ImportedTask' />
  <Target Name="Build"><WriteReport Source="x" /></Target>
  <ProjectExtensions><UsingTask TaskName="Hidden"><ParameterGroup><No /></ParameterGroup></UsingTask><Project><UsingTask TaskName="Fake"><ParameterGroup><Nested /></ParameterGroup></UsingTask></Project></ProjectExtensions>
</Project>`;
    const arguments_ = [
        '--no-default-outline-rules',
        '--outline-rules',
        'tools/ast-grep/outline/msbuild-using-task.yml',
        '--outline-rules',
        'tools/ast-grep/outline/msbuild-task-parameter.yml',
        '--items',
        'all',
        '--type',
        'class',
    ];
    for (const view of ['names', 'signatures', 'digest']) {
        const actual = outline('xml', source, [...arguments_, '--view', view]);
        assertOutline(actual, [{ name: 'WriteReport' }, { name: 'ImportedTask' }], 'names');
    }
    const expanded = outline('xml', source, [...arguments_, '--view', 'expanded']);
    assertOutline(expanded, [
        {
            name: 'WriteReport',
            signature:
                '<UsingTask TaskName="WriteReport" Condition="\'$(Count)\' > \'0\'" TaskFactory="RoslynCodeTaskFactory" AssemblyFile="$(MSBuildToolsPath)/Microsoft.Build.Tasks.Core.dll">',
            isExported: true,
            range: { byteOffset: { start: 26, end: 466 }, start: { line: 2, column: 2 }, end: { line: 8, column: 14 } },
            members: [
                {
                    name: 'Source',
                    signature: '<Source ParameterType="System.String" Required="true" />',
                    isPublic: true,
                    range: { byteOffset: { start: 219, end: 275 }, start: { line: 4, column: 6 }, end: { line: 4, column: 62 } },
                },
                { name: 'Count', signature: '<Count ParameterType="System.Int32" Output="true" />', isPublic: true },
            ],
        },
        { name: 'ImportedTask', signature: '<UsingTask AssemblyFile="tasks.dll" TaskName=\'ImportedTask\' />', isExported: true },
    ]);
});

test('MSBuild maps only project declarations and preserves quoted header values', (): void => {
    const source = `<Project>
  <!-- λ -->
  <Import Project="top.props" Condition="'a  b' == 'a  b'" />
  <ImportGroup Condition="'x' == 'x'">
    <Import Project='group.targets' />
  </ImportGroup>
  <Target Name="Build" Condition="'a  b' == 'a  b'">
    <Message Text="two  spaces" />
    <PropertyGroup><Payload><Target Name="PropertyTarget"><HiddenTask /></Target><Import Project="property.props" /></Payload></PropertyGroup>
    <OnError ExecuteTargets="Recover" />
  </Target>
  <Target Name="Empty" />
  <ProjectExtensions><Project><Import Project="fake.props" /><ImportGroup><Import Project="fake-group.props" /></ImportGroup><Target Name="Fake"><FakeTask /></Target></Project></ProjectExtensions>
  <PropertyGroup><Nested><Target Name="Nested" /><Import Project="nested.props" /></Nested></PropertyGroup>
</Project>`;
    const arguments_ = [
        '--outline-rules',
        'tools/ast-grep/outline/msbuild-task.yml',
        '--outline-rules',
        'tools/ast-grep/outline/msbuild-import.yml',
        '--items',
        'all',
    ];
    for (const view of ['names', 'signatures', 'digest']) {
        const actual = outline('xml', source, [...arguments_, '--view', view]);
        assertOutline(actual, [{ name: 'top.props' }, { name: 'group.targets' }, { name: 'Build' }, { name: 'Empty' }], 'names');
    }
    const expanded = outline('xml', source, [...arguments_, '--view', 'expanded']);
    assertOutline(expanded, [
        { name: 'top.props', signature: '<Import Project="top.props" Condition="\'a  b\' == \'a  b\'" />' },
        { name: 'group.targets' },
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
                { name: 'frontend', signature: 'wait: frontend' },
                { name: 'wait-all', signature: 'wait-all:' },
                { name: 'monitor', signature: 'cancel: monitor' },
            ],
        },
    ]);
    const composite = 'runs: {using: composite, steps: [{parallel: [{run: hidden}]}]}';
    const expanded = outline('yaml', composite, [...githubOutline, '--view', 'expanded']);
    assertOutline(expanded, [{ name: 'runs' }]);
});

test('GitHub workflow outlines retain ordered jobs, calls, names and source ranges', (): void => {
    const source = `# π🙂
jobs:
  build:
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
        run: nx report
  reusable:
    uses: ./.github/workflows/native.yml
other:
  jobs:
    nested:
      steps: [{run: ignored}]
`;
    for (const view of ['names', 'signatures', 'digest']) {
        const actual = outline('yaml', source, [...githubOutline, '--view', view]);
        assertOutline(actual, [{ name: 'build' }, { name: 'reusable' }], 'names');
    }
    const expanded = outline('yaml', source, [...githubOutline, '--view', 'expanded']);
    assertOutline(expanded, [
        {
            name: 'build',
            signature: 'build',
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
        { name: 'reusable' },
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
    const source = `name: Setup
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
            name: 'runs',
            members: [
                { name: 'Install', signature: 'run: pnpm install' },
                { name: './another', signature: 'uses: ./another' },
            ],
        },
    ]);
    for (const nearMiss of [
        source.replace('using: composite', 'using: node24'),
        'data: {runs: {using: composite, steps: [{run: hidden}]}}',
        'steps: [{run: hidden}]',
    ]) {
        const expanded = outline('yaml', nearMiss, [...githubOutline, '--view', 'expanded']);
        assertOutline(expanded, []);
    }
    const quoted = outline('yaml', `{'runs': {"using": "composite", steps: [{run: nx lint}]}}`, [...githubOutline, '--view', 'expanded']);
    assertOutline(quoted, [{ name: "'runs'", members: [{ name: 'nx lint', signature: 'run: nx lint' }] }]);
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
