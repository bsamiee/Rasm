import { existsSync } from 'node:fs';
import { basename, delimiter, join, matchesGlob } from 'node:path';
import { env } from 'node:process';
import type { layer } from '@effect/platform-node/NodeServices';
import type { Vitest } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { graph, installed } from '@rasm/testing/nx/cli';
import { Array, DateTime, Effect, FileSystem, type Layer, Option, Path, Predicate, Record } from 'effect';
import type { NxJsonConfiguration } from 'nx/src/config/nx-json';
import { getOutputsForTargetAndConfiguration } from 'nx/src/tasks-runner/utils';
import type { Subject } from '#/inference/cases';

// --- [TYPES] ---------------------------------------------------------------------------

type Methods = Vitest.MethodsNonLive<Layer.Success<typeof layer>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const SLOW = { timeout: 300_000 } as const;
const PATTERN = /\/?[*{].*$/u;
const MANIFESTS = ['package.json', 'project.json'];
const FOLDERS = Option.match(Record.get(Record.filter(env, Predicate.isNotUndefined), 'PATH'), { onNone: () => [], onSome: (folders) => folders.split(delimiter) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const row = ({ plugin, options }: Subject) => ({ plugin: plugin.name, ...(options === undefined ? {} : { options }) });

const present = ({ tools = [] }: Subject) => tools.every((tool) => FOLDERS.some((folder) => existsSync(join(folder, tool))));

const placed = (subject: Subject, files: Record.ReadonlyRecord<string, string> = {}, nxJson: NxJsonConfiguration = { plugins: [row(subject)] }) => installed({ ...subject.files, ...files }, [subject.plugin.name, ...(subject.packages ?? [])], import.meta.url, nxJson, subject.state);

const amended = Effect.fnUntraced(function* (root: string, file: string, text: string) {
    const [fs, path, now] = yield* Effect.all([FileSystem.FileSystem, Path.Path, DateTime.now]);
    const target = path.join(root, file);
    const later = DateTime.toDate(DateTime.add(now, { seconds: 2 }));
    yield* fs.makeDirectory(path.dirname(target), { recursive: true });
    yield* fs.writeFileString(target, text);
    yield* fs.utimes(target, later, later);
});

const fresh = (root: string, name: string) => ({ NX_WORKSPACE_DATA_DIRECTORY: join(root, '.nx', name) });

const data = (listed: ProjectGraph) => Record.map(listed.nodes, (node) => node.data);

const located = (listed: ProjectGraph) => Record.map(listed.nodes, (node) => node.data.root);

const sources = ({ plugin, files }: Subject) => Record.toEntries(Record.filter(files, (_, file) => !(matchesGlob(file, plugin.createNodes[0]) || MANIFESTS.includes(basename(file)))));

const declared = (listed: ProjectGraph) =>
    Array.dedupe(Object.values(listed.nodes).flatMap((node) => Record.keys(Record.filter(node.data.targets ?? {}, (target) => target.outputs !== undefined)).flatMap((target) => getOutputsForTargetAndConfiguration({ project: node.name, target }, {}, node).map((output) => output.replace(PATTERN, '')))));

const prepared = Effect.fnUntraced(function* (subject: Subject) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const root = yield* placed(subject);
    const listed = yield* graph(root);
    const folders = declared(listed).filter((folder) => !Record.keys(subject.files).some((file) => file === folder || file.startsWith(`${folder}/`)));
    const ignored = yield* fs.readFileString(path.join(root, '.gitignore'));
    yield* amended(root, '.gitignore', `${ignored}${folders.join('\n')}\n`);
    return { root, listed, folders, cached: (project: string, target: string) => listed.nodes[project]?.data.targets?.[target]?.cache === true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { amended, data, fresh, located, type Methods, placed, prepared, present, row, SLOW, sources };
