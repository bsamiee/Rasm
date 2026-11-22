import { join } from 'node:path';
import { env } from 'node:process';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-distribution';
import { it, shimmed, type Workspace, taskGraph } from '@rasm/testing/nx';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('node:process'), async (actual) => {
    const platform: NodeJS.Platform = 'darwin';
    return { ...(await actual()), platform };
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const files = {
    'package.json': '{ "name": "repo", "private": true }',
    'nx.json': '{ "namedInputs": { "default": ["{projectRoot}/**/*"], "production": ["default", "!{projectRoot}/**/*.spec.js"] } }',
    'docs/notes.md': 'Notes\n',
    '.github/workflows/ci.yml': 'on: push\n',
    '.github/actions/setup/action.yml': 'runs:\n    using: composite\n    steps: []\n',
    'panel/manifest.json': '{ "manifestVersion": 5, "id": "com.example.panel", "name": "Panel", "version": "1.2.3", "host": { "app": "PS", "minVersion": "24.0.0" } }',
    'panel/main.js': 'export const main = 1;\n',
    'panel/main.spec.js': 'main;\n',
    'uv.lock': 'version = 1\n',
    'tools/blender_manifest.toml': 'schema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.2.3"\n',
    'Engine/Engine/Build/Build.version': '{ "MajorVersion": 5, "MinorVersion": 6, "PatchVersion": 0 }\n',
    'Engine2/Engine/Build/Build.version': '{ "MajorVersion": 5, "MinorVersion": 7, "PatchVersion": 0 }\n',
    'Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "../Engine", "Description": "Game", "Plugins": [{ "Name": "Marker", "Enabled": true }] }\n',
    'Game/Plugins/Marker/Marker.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "1.2.3" }\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashed = (workspace: Workspace, graph: ProjectGraph, task: string) => {
    const [project = '', target = ''] = task.split(':');
    const tasks = taskGraph(graph, [project], [target]);
    const found = tasks.tasks[task];
    assert(found !== undefined, `${task} is a task of the graph`);
    return createTaskHasher(graph, readNxJson(workspace.root)).hashTask(found, tasks, env);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('task hashes', () => {
    it.for([
        { task: 'com.example.panel:build--uxp', edited: { 'panel/main.js': 'export const main = 2;\n' }, changes: true },
        { task: 'com.example.panel:build--uxp', edited: { 'panel/main.spec.js': 'main();\n' }, changes: false },
        { task: 'com.example.panel:build--uxp', base: { 'nx.json': '{ "namedInputs": { "default": ["{projectRoot}/**/*"] } }' }, edited: { 'panel/main.spec.js': 'main();\n' }, changes: true },
        { task: 'marker_tools:build--blender', edited: { 'uv.lock': 'version = 2\n' }, changes: true },
        { task: 'repo:upgrade--jactionlint', edited: { '.github/workflows/ci.yml': 'on: [push, pull_request]\n' }, changes: true },
        { task: 'repo:upgrade--jactionlint', edited: { '.github/actions/setup/action.yml': 'runs:\n    using: composite\n    steps: [{ run: echo }]\n' }, changes: true },
        { task: 'repo:upgrade--jactionlint', edited: { 'docs/notes.md': 'More notes\n' }, changes: false },
        { task: 'repo:upgrade--jactionlint', edited: { 'tools/setup/action.yml': 'runs:\n    using: composite\n    steps: []\n' }, changes: false },
        { task: 'Game:build--unreal', edited: { 'Engine/Engine/Build/Build.version': '{ "MajorVersion": 5, "MinorVersion": 6, "PatchVersion": 1 }\n' }, changes: true },
        { task: 'Marker:build--unreal', edited: { 'Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "../Engine2", "Description": "Game", "Plugins": [{ "Name": "Marker", "Enabled": true }] }\n' }, changes: true },
        { task: 'Marker:build--unreal', edited: { 'Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "../Engine", "Description": "Shooter", "Plugins": [{ "Name": "Marker", "Enabled": true }] }\n' }, changes: false },
    ])('$task hash changes ($changes) after editing $edited', async ({ task, base = {}, edited, changes }, { workspace }) => {
        shimmed(join(workspace.root, '.bin'), { uv: 'echo 0f1e2d' });
        const before = await hashed(workspace, await workspace.write({ ...files, ...base }).graph({ createNodes }), task);

        const after = await hashed(workspace, await workspace.write(edited).graph({ createNodes }), task);

        expect(before.value === after.value).toBe(!changes);
    });
});
