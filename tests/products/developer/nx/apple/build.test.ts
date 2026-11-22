import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-apple';
import { execute, Run } from '@rasm/nx-apple/executors/run';
import { it, outputs, shimmed, targetOptions } from '@rasm/testing/nx';
import { expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

it('writes every declared output of build--swift-build and nothing outside the build folder swift build keeps', { timeout: 300_000 }, async ({ workspace }) => {
    const graph = await workspace.write({ 'Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Tool", targets: [.executableTarget(name: "Tool")])\n', 'Sources/Tool/main.swift': 'print(1)\n' }).graph({ createNodes, createDependencies });
    const before = workspace.files('.build', 'tmp');

    const { success } = await execute(await targetOptions(Run, graph, 'Tool', 'build--swift-build'), workspace.context(graph, 'Tool', 'build--swift-build'));

    const declared = outputs(graph, 'Tool', 'build--swift-build');
    expect([success, declared, declared.every((output) => existsSync(join(workspace.root, output))), workspace.files('.build', 'tmp')]).toStrictEqual([true, ['.build/out/Products/Debug/Tool'], true, before]);
});

it.for([
    { case: 'runs from inside its bundle', running: '/Applications/Viewer.app/Contents/MacOS/Viewer', notices: ['Viewer runs the build it launched from, relaunch it to run this one'] },
    { case: 'runs from a bundle of another name', running: '/Applications/Viewer Helper.app/Contents/MacOS/Viewer', notices: [] },
])('asks for a relaunch after an install only where the installed application $case', async ({ running, notices }, { workspace, onTestFinished }) => {
    const log = vi.spyOn(console, 'log').mockReturnValue(undefined);
    onTestFinished(() => {
        log.mockRestore();
    });
    shimmed(join(workspace.root, 'bin'), { ps: `printf '%s\\n' /sbin/launchd '${running}'` });

    const { success } = await execute({ runs: [['true']], applications: ['Viewer.app'] }, workspace.context({ nodes: {}, dependencies: {} }, 'Viewer', 'install--xcodebuild'));

    expect([success, log.mock.calls.flat().filter((said) => String(said).includes('relaunch'))]).toStrictEqual([true, notices]);
});
