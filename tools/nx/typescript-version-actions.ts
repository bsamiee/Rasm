// Version actions for a TypeScript package with no version in its source package.json, the built manifest under dist takes the tag version

// --- [IMPORTS] -------------------------------------------------------------------------

import type { Tree } from '@nx/devkit';
import jsRelease from '@nx/js/src/release/version-actions';
import { Option, Predicate } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const _isVersionActions = (value: unknown): value is typeof jsRelease => Predicate.isFunction(value);

// Nx loads the module under Node type stripping, where the CommonJS default import is the module object and the class sits under its default key
const _JsVersionActions: typeof jsRelease = Option.liftPredicate(jsRelease, Predicate.hasProperty('default')).pipe(
    Option.flatMap((module) => Option.liftPredicate(module.default, _isVersionActions)),
    Option.getOrElse(() => jsRelease),
);

// biome-ignore lint/style/noDefaultExport: Nx loads the class from the default export of the versionActions module
export default class TypescriptVersionActions extends _JsVersionActions {
    // The source manifest holds no version, and a package without a tag starts its first release from 0.0.0
    override readCurrentVersionFromSourceManifest(tree: Tree): Promise<{ currentVersion: string; manifestPath: string }> {
        return super.readCurrentVersionFromSourceManifest(tree).then((manifest) => ({
            ...manifest,
            currentVersion: manifest.currentVersion ?? '0.0.0',
        }));
    }
}
