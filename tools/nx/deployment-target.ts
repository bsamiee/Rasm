import type { Tree } from '@nx/devkit';
import { readNxJsonFromDisk } from '@nx/devkit/internal';
import { Array, Effect, Struct } from 'effect';
import { CST, Parser } from 'yaml';
import { openWorkspace, SWIFT_MANIFEST } from '#/tools/nx/internal/open-workspace';
import { runPlugin } from '#/tools/nx/internal/plugin-runtime';

// --- [CONSTANTS] -----------------------------------------------------------------------

const XCCONFIG_TARGET = /(?<=^MACOSX_DEPLOYMENT_TARGET = ).+$/mu;

// --- [COMPOSITION] ---------------------------------------------------------------------

const deploymentTarget = Effect.fnUntraced(function* (tree: Tree) {
    const { platforms } = yield* (yield* openWorkspace({ workspaceRoot: tree.root, nxJsonConfiguration: readNxJsonFromDisk(tree.root) })).swift;
    const { version } = yield* Effect.fromOption(Array.findFirst(platforms, ({ name }) => name === 'macos'));
    const setDeploymentTarget =
        (document: CST.Document): CST.Visitor =>
        (item, path) => {
            if (path.length === 2 && CST.resolveAsScalar(item.key)?.value === 'macOS_deployment_target' && CST.resolveAsScalar(CST.visit.itemAtPath(document, path.slice(0, -1))?.key)?.value === 'deployment_target' && CST.isScalar(item.value)) {
                CST.setScalarValue(item.value, version);
            }
        };
    const updates = {
        'Xcode.xcconfig': (text: string) => text.replace(XCCONFIG_TARGET, version),
        '.swiftlint.yml': (text: string) => {
            const tokens = [...new Parser().parse(text)];
            tokens.filter((token) => token.type === 'document').forEach((document) => CST.visit(document, setDeploymentTarget(document)));
            return tokens.map(CST.stringify).join('');
        },
    };
    const rows = yield* Effect.forEach(
        Object.entries(updates),
        Effect.fnUntraced(function* ([file, update]) {
            const text = yield* Effect.fromNullishOr(tree.read(file, 'utf-8'));
            return { file, text, next: update(text) };
        }),
    );
    const drifted = rows.filter(({ text, next }) => next !== text);
    yield* Effect.forEach(drifted, ({ file, next }) => Effect.sync(tree.write.bind(tree, file, next)), { discard: true });
    return Array.match(drifted, { onEmpty: () => undefined, onNonEmpty: (changed) => ({ outOfSyncMessage: `macOS deployment target differs from ${SWIFT_MANIFEST} ${version}`, outOfSyncDetails: changed.map(Struct.get('file')) }) });
}, runPlugin);

// --- [EXPORTS] -------------------------------------------------------------------------

export { deploymentTarget };
