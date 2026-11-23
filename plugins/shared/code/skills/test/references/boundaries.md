# [BOUNDARIES]

Test doubles are unverified claims about a real thing. Preference runs real dependency, fake, stub, spy, mock, by how much real behavior stays inside a test.

## [01]-[REAL_DEPENDENCIES]

[REACH]:
- Behavior under test is a query, transaction, serialization, timeout, or error mapping against a database, queue, file system, or HTTP peer

[KINDS]:

| [INDEX] | [KIND]            | [RUNS]                                                              |
| :-----: | :---------------- | :------------------------------------------------------------------ |
|  [01]   | Real engine       | Container, local service, embedded or in-process build of an engine |
|  [02]   | Wire-level double | Scripted server on loopback, whole client stack runs                |
|  [03]   | Recorded traffic  | Replay of real responses, real service at record time alone         |

[METHOD]:
- Drop leftovers before creating resources and delete them on success alone, a failed test leaves state to inspect

[FORMS]:

| [INDEX] | [FORM]                                                                 | [SIGN_OF_FAILURE]                                        |
| :-----: | :--------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | Real engine or wire-level double runs own serialization and mapping    | Test green with its dependency unreachable               |
|  [02]   | Engine under test is one production runs                               | In-memory substitute standing in for it                  |
|  [03]   | Faults injected through transport (disconnect, no response)            | Mocked exception of a type its client never raises       |
|  [04]   | Each test owns a namespace: database named from its path, unique names | Fixed path or port, assertion on a list other tests fill |
|  [05]   | Rolled-back transaction isolates code that opens none                  | Test passing alone and failing in its suite              |
|  [06]   | Dependency version pinned, each end of a supported range tested        | `latest` tag, red build from an upstream release         |
|  [07]   | Recorded traffic scrubbed of dates, request ids, and credentials       | Replay that stops matching                               |
|  [08]   | Recorded traffic has an owner and an account per backend               | Skip flag in a harness, case committed commented out     |
|  [09]   | Artifact that is the product (query text, bytes sent) pinned and run   | Golden text as sole assertion                            |

## [02]-[DOUBLES]

[REACH]:
- Neither a real engine nor a wire-level double fits a collaborator
- Clock, random source, or scheduler decides a result

[KINDS]:

| [INDEX] | [KIND]    | [REPLACES]           | [LIES_WHEN]                                          |
| :-----: | :-------- | :------------------- | :--------------------------------------------------- |
|  [01]   | Dummy     | Nothing that matters | Never                                                |
|  [02]   | Stub      | Input                | Canned answer is one a real collaborator never gives |
|  [03]   | Spy, mock | Verdict              | Passing means a call was made, whatever came of it   |
|  [04]   | Fake      | Behavior             | Fake drifts from its real implementation             |

[METHOD]:
- Pass dependencies in as parameters in place of replacing a global

[FORMS]:

| [INDEX] | [FORM]                                                               | [SIGN_OF_FAILURE]                                       |
| :-----: | :------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | Double sits behind a narrow owned interface, sized to calls made     | Fake copying vendor members no code calls               |
|  [02]   | Thin adapter over a real client tested with its real dependency      | Adapter no test reaches                                 |
|  [03]   | One set of behavioral cases runs against fake and real               | Fake stricter or looser than its service                |
|  [04]   | Fake's regression case joins that shared set                         | Fix to a fake with a fake-only test                     |
|  [05]   | Fake's divergence recorded as a test that fails once it ends         | Override with no assertion reported as passed           |
|  [06]   | Fake answers by rule from each request, tests assert outcomes        | Expectations per call, assertion on a fake's state      |
|  [07]   | Interaction asserted where it is the product (request, message sent) | Call verified where its effect shows at an interface    |
|  [08]   | Stubbed answer or error checked once against its real dependency     | Stubbed error of a kind its dependency never returns    |
|  [09]   | Every stub arrangement is reached by its test                        | Arrangement copied from another test, key never matched |
|  [10]   | Double generated from a real interface                               | Stub answering a member its interface lacks             |

## [03]-[PACKAGES]

[REACH]:
- Code under test calls a third-party package or a remote service

[KINDS]:

| [INDEX] | [ROUTE]              | [USE_WHEN]                                                      |
| :-----: | :------------------- | :-------------------------------------------------------------- |
|  [01]   | Real package         | Package runs in process (parser, codec, math)                   |
|  [02]   | Owner's double       | Owner publishes a fake, test mode, emulator, or scripted server |
|  [03]   | Owned interface      | No owner's double exists, fake held to real by shared cases     |
|  [04]   | Generated typed mock | Own error classification under a stubbed vendor error           |

[METHOD]:
- Take routes in table order, a later route needs a reason each earlier one fails

[FORMS]:

| [INDEX] | [FORM]                                                           | [SIGN_OF_FAILURE]                                        |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | Tests cover own mapping, query, retry, and error classification  | Assertion on what a package returns for its own input    |
|  [02]   | Double comes from its owner or is held to real by shared cases   | Hand-written vendor double with tests of its own alone   |
|  [03]   | Owner's documented limits read before its double is used         | False pass from a fake that skips validation or defaults |
|  [04]   | Small suite runs against a live service to detect contract drift | Contract drift found in production                       |
