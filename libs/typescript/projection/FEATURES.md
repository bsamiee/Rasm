# [PROJECTION_FEATURES]

The realized capability list for the fold algebra. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[FOLD_ALGEBRA]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]            |
| :-----: | :----------------------------------------------------------------------------- | :----------------------- |
|   [1]   | One `StreamPolicy`: reconnect Schedule, buffer, throttle, groupedWithin, scan   | fold-algebra#FOLD_ALGEBRA |
|   [2]   | One key-discriminated `keyedFold` combinator into a SubscriptionRef keyed map    | fold-algebra#FOLD_ALGEBRA |
|   [3]   | Five stream stores: runtime, health, snapshot, progress, conflict-presence       | fold-algebra#FOLD_ALGEBRA |
|   [4]   | Standing-query window fold: tumbling/sliding/session with late-row retract        | fold-algebra#FOLD_ALGEBRA |
|   [5]   | LWW convergent fold: HLC last-write-wins with content-key idempotent replay       | fold-algebra#FOLD_ALGEBRA |

## [2]-[ENVELOPE_AND_EVIDENCE]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                              |
| :-----: | :----------------------------------------------------------------------------- | :----------------------------------------- |
|   [6]   | Envelope carrier binding every structured-text payload as one Schema factory     | envelope-and-evidence#ENVELOPE_AND_EVIDENCE |
|   [7]   | Receipt and HLC-ordered evidence folds over the compute-receipt union            | envelope-and-evidence#ENVELOPE_AND_EVIDENCE |
|   [8]   | Availability read gate folding commands-availability rows for the gateway         | envelope-and-evidence#ENVELOPE_AND_EVIDENCE |
|   [9]   | Clock-uncertainty SkewBand projection into a confidence interval                  | envelope-and-evidence#ENVELOPE_AND_EVIDENCE |
