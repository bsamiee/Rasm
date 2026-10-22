import indesign from 'adobe:indesign';
import { Contract, handlers, Model } from '@rasm/indesign';
import { script } from '@rasm/uxp/entrypoints';
import { Layer } from 'effect';
import { RpcServer } from 'effect/rpc';

// --- [COMPOSITION] ---------------------------------------------------------------------

script(RpcServer.layer(Contract).pipe(Layer.provide(handlers), Layer.provide(Layer.effect(Model, Model.make(indesign)))));
