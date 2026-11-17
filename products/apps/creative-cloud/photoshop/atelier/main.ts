import { Link } from '@rasm/mcp';
import { type HostError, type Rasterize, Script } from '@rasm/object-model';
import { application, execute, objectModel, open, rasterize, session } from '@rasm/photoshop/uxp';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { Effect, Layer, Option, type Path, Semaphore } from 'effect';
import { ATELIER, Photoshop } from '#/tools';

// --- [COMPOSITION] ---------------------------------------------------------------------

const permit = Semaphore.makeUnsafe(1);
const hosting = Effect.map(
    Effect.context<Uxp | Path.Path>(),
    (services) =>
        <A, E, R>(effect: Effect.Effect<A, E, R>) =>
            Effect.scoped(Effect.andThen(session, effect)).pipe(Semaphore.withPermit(permit), Effect.provideContext(services)),
);
const tools = Link.serve(Photoshop).pipe(
    Layer.provide(
        Photoshop.toLayer(
            Effect.map(hosting, (hosted) => ({
                open: ({ file, label }: { readonly file: string; readonly label: string }) => hosted(open(file, label)),
                rasterize: ({ target, longestSide, options }: Rasterize) => hosted(Uxp.rendered((file) => rasterize(target, longestSide, options, file), 'rasterized.png')),
                object_model: ({ name }: { readonly name: Option.Option<string> }) => hosted(objectModel(name)),
            })),
        ),
    ),
);
const script = Script.toLayer(
    Effect.map(hosting, (hosted) => ({
        evaluate: ({ code, label }: { readonly code: string; readonly label: string }) =>
            execute(code, label).pipe(
                Effect.map((result) => ({ result })),
                hosted,
            ),
    })),
);
const linked = (extension: string, root: Effect.Effect<URL, HostError, Path.Path>) => Link.dial(Script, Layer.merge(tools, script), ATELIER, Effect.all([application, root]).pipe(Effect.map(([name, folder]) => Link.make({ application: name, extension, root: Option.some(folder) }))));

Plugin.serve({ link: linked });
