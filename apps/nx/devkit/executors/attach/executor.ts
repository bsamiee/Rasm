import { Effect } from 'effect';
import { CommandLine } from 'effect/process';
import { Dry, dried, executor, packaged, tagged } from '#/executor';
import { printed } from '#/process';

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Dry.annotate({
        title: 'Attach',
        description: "Uploads the files a project's packaging targets wrote to the GitHub release whose tag nx release gives the version its manifest states, through gh release upload --clobber",
    }),
    Effect.fnUntraced(function* (stated, context, project) {
        const [dry, files, tag] = yield* Effect.all([dried(stated), packaged(context), tagged(context, project)]);
        const upload = ['release', 'upload', tag, ...files, '--clobber'];
        yield* dry ? Effect.logInfo(`Would run ${CommandLine.display(['gh', ...upload])}`) : printed('gh', upload, { cwd: context.root });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
