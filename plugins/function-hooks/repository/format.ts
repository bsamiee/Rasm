import { type Fault, map, none, type Option, ok, type Result, some } from '../composition.ts';
import type { Invocation } from '../policies/invocation.ts';
import { inputs, matches, repository } from './nx.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Writer = readonly [globs: readonly string[], commands: readonly (readonly [Invocation, exits: readonly number[]])[]];

interface Formatter {
    readonly exec: (invocation: Invocation, exits: readonly number[]) => Promise<Result<string>>;
    readonly read: (path: string) => Promise<Result<string>>;
    readonly mtime: (path: string) => Promise<Option<number>>;
}
interface Reformatted {
    readonly context: Option<string>;
    readonly failed: readonly Fault[];
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const reformatted = async (host: Formatter, root: string, paths: readonly string[]): Promise<Reformatted> => {
    const stamped = async (files: readonly string[]): Promise<ReadonlyMap<string, number>> => new Map((await Promise.all(files.map(async (file) => [file, await host.mtime(`${root}/${file}`)] as const))).flatMap(([file, mtime]) => (mtime.kind === 'some' ? [[file, mtime.value] as const] : [])));
    const before = await stamped(paths.flatMap((path) => (path.startsWith(`${root}/`) ? [path.slice(root.length + 1)] : [])));
    const present = [...before.keys()];
    const writers =
        present.length === 0
            ? ok<readonly Writer[]>([])
            : map(await repository(host.read, root), (held): readonly Writer[] => {
                  const declared = (target: string): readonly string[] => inputs(target, held);
                  const clean = [0];
                  const remaining = [0, 1];
                  return [
                      [declared('lint:biome'), [[['biome', 'check', '--write', '--files-ignore-unknown=true', '--no-errors-on-unmatched'], remaining]]],
                      [
                          declared('lint:ruff-format'),
                          [
                              [['ruff', 'check', '--fix'], remaining],
                              [['ruff', 'format'], clean],
                          ],
                      ],
                      [declared('lint:yamlfmt'), [[['yamlfmt'], clean]]],
                      [declared('lint:google-java-format'), [[['google-java-format', '--aosp', '--replace'], clean]]],
                      [
                          ['**/*.swift'],
                          [
                              [['swiftlint', 'lint', '--fix', '--quiet'], clean],
                              [['xcrun', 'swift-format', 'format', '--in-place'], clean],
                          ],
                      ],
                  ];
              });
    if (writers.kind === 'fault') {
        return { context: none, failed: writers.faults };
    }
    const invocations = writers.value.flatMap(([globs, commands]) => {
        const operands = present.filter((file) => matches(globs, file));
        return operands.length === 0 ? [] : commands.map(([invocation, exits]) => [[...invocation, ...operands] satisfies Invocation, exits] as const);
    });
    const ran = await invocations.reduce<Promise<readonly Result<string>[]>>(async (earlier, [invocation, exits]) => [...(await earlier), await host.exec(invocation, exits)], Promise.resolve([]));
    const after = await stamped(present);
    const changed = present.filter((file) => before.get(file) !== after.get(file));
    return { context: changed.length === 0 ? none : some(`Writers reformatted ${changed.join(', ')}`), failed: ran.flatMap((result) => (result.kind === 'fault' ? result.faults : [])) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Formatter };
export { reformatted };
