import { homedir, hostname, tmpdir, userInfo } from 'node:os';
import { isAbsolute, join, relative, resolve } from 'node:path';
import { createConformanceRule } from '@nx/conformance';
import { Match, Predicate, Record, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STAMP = /\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/u;
const WORD = /[\s'"=]+/u;
const FOLDERS = [homedir(), tmpdir()];
const NAMES = new Set([userInfo().username, hostname()]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const strings = (value: unknown, at: string): readonly (readonly [at: string, text: string])[] =>
    Match.value(value).pipe(
        Match.when(Predicate.isObjectOrArray, (held) => Object.entries(held).flatMap(([key, inner]) => strings(inner, `${at}.${key}`))),
        Match.when(Predicate.isString, (text) => [[at, text] as const]),
        Match.orElse(() => []),
    );

const faults = (root: string, project: string, text: string) => {
    const outside = text.split(WORD).filter((word) => {
        const path = word.replaceAll('{workspaceRoot}', root).replaceAll('{projectRoot}', project);
        return (path.split('/').includes('..') || isAbsolute(path)) && [root, project].every((base) => relative(root, resolve(base, path)).startsWith('..'));
    });
    return [
        ...([root, ...FOLDERS].some((folder) => text.includes(folder)) || text.split('/').some((segment) => NAMES.has(segment)) || STAMP.test(text) ? [`holds ${text}, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}`] : []),
        ...outside.map((word) => `names ${word} outside the workspace, which no other runner holds, move it inside`),
    ];
};

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule({
    name: 'portable-graph',
    category: 'reliability',
    description: 'Hashes into no cached target a checkout, home, or temporary folder, a user or host name segment, a timestamp, or a path outside the workspace, as one remote cache serves every runner',
    implementation: ({ tree: { root }, projectGraph: { nodes } }) => {
        const hashed = Object.values(nodes).flatMap(({ name, data }) =>
            Record.collect(
                Record.filter(data.targets ?? {}, ({ cache }) => cache === true),
                (target, configuration) => ({ project: name, folder: join(root, data.root), held: strings(Struct.pick(configuration, ['inputs', 'outputs', 'options', 'configurations']), `${name}:${target}`) }),
            ),
        );
        const violations = hashed.flatMap(({ project, folder, held }) => held.flatMap(([at, text]) => faults(root, folder, text).map((fault) => ({ sourceProject: project, message: `${at} ${fault}` }))));
        return Promise.resolve({ severity: 'high' as const, details: { violations } });
    },
});
