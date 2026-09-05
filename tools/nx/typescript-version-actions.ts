// Version actions for a TypeScript package with no version in its source package.json, the built manifest under dist takes the tag version

// --- [IMPORTS] -------------------------------------------------------------------------

import type { Tree } from '@nx/devkit';
import jsRelease from '@nx/js/src/release/version-actions';
import { Effect, Layer, ManagedRuntime, Option, Predicate } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

// Nx consumes a promise from the method, the runtime translates the effect at that boundary
const _runtime = ManagedRuntime.make(Layer.empty);

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
        return _runtime.runPromise(
            Effect.map(
                Effect.promise(() => super.readCurrentVersionFromSourceManifest(tree)),
                (manifest) => ({
                    ...manifest,
                    currentVersion: Option.getOrElse(Option.fromNullable(manifest.currentVersion), () => '0.0.0'),
                }),
            ),
        );
    }
}
