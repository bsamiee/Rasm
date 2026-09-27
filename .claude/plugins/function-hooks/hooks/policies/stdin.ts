import type { Command } from '../command.ts';
import { type Decision, refuse } from '../composition.ts';
import { type Invocation, invocations, operands } from '../invocation.ts';
import { basename } from '../path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Reader {
    readonly always?: true;
    readonly sources?: readonly string[];
    readonly wholeWords?: true;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _reads = (invocation: Invocation): boolean => {
    const python: Reader = { sources: ['-c', '-m', '-V'] };
    const shell: Reader = { sources: ['-c'] };
    const sql: Reader = { sources: ['-c', '-s', '-f', '-h', '-help', '-version', '-no-stdin'], wholeWords: true };
    const readers: Readonly<Record<string, Reader>> = {
        cat: {},
        head: {},
        tail: {},
        sort: { sources: ['--files0-from'] },
        uniq: {},
        wc: {},
        cut: {},
        shasum: {},
        md5: { sources: ['-s'] },
        tr: { always: true },
        tee: { always: true },
        xargs: { always: true },
        pbcopy: { always: true },
        read: { always: true, sources: ['-u'] },
        base64: { always: true, sources: ['-i', '--input'] },
        grep: { sources: ['-r', '-R', '--recursive', '--dereference-recursive'] },
        rg: { sources: ['--files', '--type-list'] },
        sed: {},
        awk: {},
        jq: { sources: ['-n', '--null-input'] },
        yq: { sources: ['-n', '--null-input'] },
        sd: {},
        python,
        python3: python,
        node: { sources: ['-e', '-p', '-v', '--eval', '--print', '--test', '--run'] },
        bash: shell,
        sh: shell,
        zsh: shell,
        sqlite3: sql,
        duckdb: sql,
    };
    const reader = readers[basename(invocation[0])];
    const { inputs, options } = operands(invocation);
    return (
        reader !== undefined &&
        !(reader.wholeWords === true ? invocation : [...options, ...invocation]).some((word) => ['--help', '--version'].includes(word) || reader.sources?.includes(word) === true) &&
        (reader.always === true || inputs.length === 0 || inputs.includes('-'))
    );
};

// --- [POLICY] --------------------------------------------------------------------------

const stdinPolicy = (commands: readonly Command[]): (<E>(e: E) => Decision<E>) =>
    refuse(
        commands.flatMap((command) =>
            !command.fed && invocations(command.words).some(_reads)
                ? [`${command.words.join(' ')} reads standard input, and no operand, pipe, heredoc, herestring, or input redirect supplies it`]
                : [],
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { stdinPolicy };
