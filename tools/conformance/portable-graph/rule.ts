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

const faults = (root: string, project: string, text: string, cached: boolean) => {
    const outside = text.split(WORD).filter((word) => {
        const path = word.replaceAll('{workspaceRoot}', root).replaceAll('{projectRoot}', project);
        return (path.split('/').includes('..') || isAbsolute(path)) && [root, project].every((base) => relative(root, resolve(base, path)).startsWith('..'));
    });
    return [
        ...([root, ...FOLDERS].some((folder) => text.includes(folder)) || text.split('/').some((segment) => NAMES.has(segment)) || STAMP.test(text) ? [`holds ${text}, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}`] : []),
        ...(cached ? outside : []).map((word) => `names ${word} outside the workspace, which no other runner holds, move it inside`),
    ];
};

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule({
    name: 'portable-graph',
    category: 'reliability',
    description:
        'Holds in no target of a project with a cached target a checkout, home, or temporary folder, a user or host name segment, or a timestamp, as Nx hashes every target of a project into each of its tasks and one remote cache serves every runner, and in no cached target a path outside the workspace, which no other runner holds',
    implementation: ({ tree: { root }, projectGraph: { nodes } }) => {
        const hashed = Object.values(nodes)
            .filter(({ data }) => Object.values(data.targets ?? {}).some(({ cache }) => cache === true))
            .flatMap(({ name, data }) => Record.collect(data.targets ?? {}, (target, configuration) => ({ project: name, folder: join(root, data.root), cached: configuration.cache === true, held: strings(Struct.pick(configuration, ['inputs', 'outputs', 'options', 'configurations']), `${name}:${target}`) })));
        const violations = hashed.flatMap(({ project, folder, cached, held }) => held.flatMap(([at, text]) => faults(root, folder, text, cached).map((fault) => ({ sourceProject: project, message: `${at} ${fault}` }))));
        return Promise.resolve({ severity: 'high' as const, details: { violations } });
    },
});
