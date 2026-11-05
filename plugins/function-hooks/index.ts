// --- [EXPORTS] -------------------------------------------------------------------------

export { type AppInstance, applyChange, type Change, createdPids, detachedQuit, outputChange, storedInstances } from './hooks/app-instance.ts';
export { faultText, Maybe, Outcome } from './monads.ts';
export { type CommandRequest, commandDecision, policyFailure, type System } from './policies/command-decision.ts';
