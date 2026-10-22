import indesign from 'adobe:indesign';
import { Contract, handlers } from '@rasm/indesign/contract.ts';
import { Model } from '@rasm/indesign/dom/model.ts';
import { script } from '@rasm/uxp/entrypoints.ts';
import { Layer } from 'effect';
import { RpcServer } from 'effect/rpc';

// --- [COMPOSITION] ---------------------------------------------------------------------

script(RpcServer.layer(Contract).pipe(Layer.provide(handlers), Layer.provide(Layer.effect(Model, Model.make(indesign)))));
