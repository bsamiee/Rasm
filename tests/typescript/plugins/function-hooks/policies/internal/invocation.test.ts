import { expect, it } from '@effect/vitest';
import { operands } from '#/plugins/function-hooks/policies/internal/invocation';

// --- [COMPOSITION] ---------------------------------------------------------------------

it.each([
    [['ast-grep', 'scan', '--error=no-suppress-all', 'tools'], { inputs: ['scan', 'tools'], values: [['--error', 'no-suppress-all']] }],
    [['ast-grep', 'outline', '--outline-rules', 'catalog.yml', '--items', 'all', '--view', 'expanded', '--match', '^Item$', '--type', 'class', 'source.ts'], { inputs: ['outline', 'source.ts'] }],
    [['biome', 'check', 'first.ts', '--', '-second.ts'], { inputs: ['check', 'first.ts', '-second.ts'] }],
    [['biome', 'check', '--fix'], { options: ['--write'] }],
    [['ast-grep', 'scan', '-Ui', 'tools'], { options: ['--update-all', '--interactive'] }],
    [['find', 'tools', '-exec', 'printf', '%s', '{}', ';'], { roots: ['tools'], inner: ['printf', '%s', '{}'] }],
    [['find', 'tools', '-name', '*.log', '-delete'], { roots: ['tools'], inputs: ['tools'], patterns: ['*.log'], options: ['-name', '-delete'] }],
    [['fd', '-e', 'log', 'cache', 'tools'], { leading: ['cache'], inputs: ['tools'], patterns: ['log'] }],
    [['bash', '-n', '+o', 'noexec', '-c', 'rm -rf .', '-n'], { executesSource: true, inputs: ['rm -rf .', '-n'], values: [['+o', 'noexec']] }],
] as const)('parses native arguments %j', (invocation, expected) => expect(operands(invocation)).toMatchObject(expected));
