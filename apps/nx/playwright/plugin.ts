import { inferred, line, NODE, type Request } from '@rasm/nx-devkit/plugin';
import { type Answer, type Asked, consulted, Timed } from '@rasm/nx-devkit/reader';
import { Function, Option, Record, Result, Schema, Struct } from 'effect';
import { Job, REPORTERS, Report } from '#/reader';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DIRECTED = Record.filterMap(REPORTERS, ([name, , variable]) => Result.map(Result.fromNullishOr(variable, Function.constVoid), (held) => [held, name] as const));

// --- [MODELS] --------------------------------------------------------------------------

const Options = Timed.mapFields(Struct.assign({ ciTargetName: Schema.optionalKey(Schema.String) })).annotate({
    description: 'deadline: longest wait for one config to answer before its reader child is stopped. ciTargetName: name of a target family running each test file as its own cached task, <ciTargetName>--<test file>, under a cached parent <ciTargetName>; absent infers none',
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: ['playwright.config.{js,ts,cjs,cts,mjs,mts}'],
    options: Options,
    ...consulted({
        entry: import.meta.resolve('#/reader'),
        job: Job,
        report: Report,
        launch: () => ({}),
        jobs: ({ files, path, placed }: Request<typeof Options.Type>) =>
            files.map((file) =>
                Job.make({
                    root: path.dirname(file),
                    file,
                    ...Option.match(placed('playwright'), { onNone: () => ({ flags: [], directed: [] }), onSome: (at) => ({ flags: ['--output', path.join(path.relative(path.dirname(file), at), 'test-results')], directed: Record.keys(DIRECTED) }) }),
                }),
            ),
    }),
    projects: ({ report: { folders, servers, files, output, merged }, loaded }: Answer<typeof Report.Type>, { job: { root, file, flags }, chain }: Asked<typeof Job>, { path, exec, placed, options: { ciTargetName } }) =>
        Option.toArray(Option.fromUndefinedOr(folders)).map((written) => {
            const within = Option.toArray(placed('playwright'));
            const directed = (to: string) => ({ env: Record.fromIterableWith(Object.values(DIRECTED), ([variable, name]) => [variable, path.join(path.relative(root, to), name)] as const) });
            const suite = {
                command: line(['playwright', 'test', ...flags]),
                options: Option.getOrElse(Option.map(placed('playwright'), directed), () => ({})),
                dependsOn: servers.map(({ project, target }) => ({ projects: [project], target, waitFor: 'ready' as const })),
                cache: true,
                inputs: ['default', '^default', ...chain, ...loaded.filter((at) => !at.startsWith(`${root}/`)).map((at) => path.join('{workspaceRoot}', at)), { env: 'CI' }, NODE, { externalDependencies: ['@playwright/test'] }],
                outputs: [...within, ...written.filter((at) => !within.some((folder) => at.startsWith(`${folder}/`)))].map((at) => path.join('{workspaceRoot}', at)),
                metadata: { description: 'Runs every Playwright project of the suite with playwright test', help: { command: `${exec} playwright test --help`, example: { args: ['--debug=cli'] } } },
            };
            const blobs = Option.flatMap(Option.fromUndefinedOr(merged), (stated) =>
                Option.match(placed('playwright'), {
                    onNone: () => Option.some(stated),
                    onSome: (at) => {
                        const named = Record.map(DIRECTED, ([, name]) => path.join(at, name));
                        return Option.map(Record.get(named, 'blob'), (blob) => ({ blob, reports: Object.values(Record.remove(named, 'blob')) }));
                    },
                }),
            );
            const alone = (at: string) => {
                const to = path.join(
                    Option.getOrElse(placed('playwright'), () => output),
                    at,
                );
                const zipped = Option.map(blobs, ({ blob }) => path.join(blob, `${at.replaceAll(/[/\\.]/gu, '-')}.zip`));
                const { env } = directed(to);
                return {
                    ...suite,
                    command: line(['playwright', 'test', at, '--output', path.join(path.relative(root, to), 'test-results')]),
                    options: { env: { ...env, ...Record.getSomes({ PLAYWRIGHT_BLOB_OUTPUT_FILE: Option.map(zipped, (blob) => path.relative(root, blob)) }) } },
                    outputs: [to, ...Option.toArray(zipped)].map((each) => path.join('{workspaceRoot}', each)),
                    metadata: { ...suite.metadata, description: `Runs ${at} alone with playwright test` },
                };
            };
            const merging = Option.all({ name: Option.fromUndefinedOr(ciTargetName), blobs });
            return {
                file,
                root,
                technologies: ['playwright'],
                clean: 'playwright',
                members: { test: { playwright: suite } },
                ...(ciTargetName !== undefined && { atoms: { name: ciTargetName, verb: 'test', tool: 'playwright', each: Record.fromIterableWith(files, (at) => [at, alone(at)] as const) } }),
                ...Record.getSomes({
                    targets: Option.map(merging, ({ name, blobs: { blob, reports } }) => ({
                        [`${name}--merge-reports`]: {
                            command: line(['playwright', 'merge-reports', path.relative(root, blob), '--config', path.basename(file)]),
                            options: { cwd: root, ...suite.options },
                            cache: true,
                            inputs: suite.inputs,
                            outputs: reports.map((at) => path.join('{workspaceRoot}', at)),
                            metadata: { technologies: ['playwright'], description: `Merges the blob reports the ${name}--* targets wrote into the reports of the config's other reporters`, help: { command: `${exec} playwright merge-reports --help`, example: { args: ['--reporter', 'html'] } } },
                        },
                    })),
                }),
            };
        }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
