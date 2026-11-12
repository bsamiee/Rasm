import { inferred, line, type Request, submodules, type Unit } from '@rasm/nx-devkit/plugin';
import { Effect, Schema } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: ['.gitmodules'],
    options: Schema.Struct({}),
    units: ({ files }: Request<unknown>) => Effect.succeed(files.map((file): Unit => ({ key: file, file }))),
    read: (missing, { context: { workspaceRoot }, path }: Request<unknown>) => Effect.forEach(missing, ({ file }) => Effect.result(Effect.map(submodules(path.join(workspaceRoot, path.dirname(file))), (report) => ({ report })))),
    projects: (held, { file }, { path }) =>
        held.map(({ name, root }) => ({
            file,
            root: path.join(path.dirname(file), root),
            name,
            technologies: ['git'],
            members: {
                build: {
                    git: {
                        command: `${line(['git', 'submodule', 'update', '--init', '--rebase', root])} && ${line(['git', '-C', root, 'lfs', 'pull'])}`,
                        cache: false,
                        inputs: [path.join('{workspaceRoot}', path.dirname(file), root)],
                        outputs: ['{projectRoot}'],
                        options: { cwd: path.dirname(file), env: { GIT_TERMINAL_PROMPT: '0' } },
                        metadata: {
                            description: `Fills ${root} at the commit the repository records for it with git submodule update, cloning it on first use and rebasing its local commits onto that commit, then downloads its LFS files with git lfs pull`,
                            help: { command: 'git submodule update --help', example: { args: ['--depth', '1'] } },
                        },
                    },
                },
            },
        })),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
