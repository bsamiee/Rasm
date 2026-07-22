# [RASM_FABRICATION_API_ROBOTS]

`Robots` (visose) owns host-neutral managed serial-chain robot kinematics and motion-program emission: per-mechanism forward/inverse kinematics, joint-limit, singularity, and reach validation, multi-mechanism and external-axis cells, per-vendor post-processors, a look-ahead-planned `Program`, and a remote upload channel. Its entire geometry vocabulary is `Rhino3dm`'s `Rhino.Geometry.*`, binary-distinct from RhinoCommon, so `plan-cs` boundary-maps at the kinematics seam and never passes a RhinoCommon instance into a `Robots` parameter. Fabrication admits it as the sole robot-kinematics owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Robots`
- package: `Robots` (MIT, visose)
- assembly: `Robots` (single `net8.0` asset, binding forward under `net10.0`)
- namespace: `Robots`, `Robots.Commands`
- asset: pure-managed AnyCPU IL; the `Rhino3dm` substrate ships the osx-arm64 `librhino3dm_native.dylib`
- depends: `Rhino3dm` geometry carriers; `SSH.NET` + `BouncyCastle.Cryptography` on the `IRemote` SFTP upload path
- rail: fabrication

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the robot-cell and solve contracts

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      | [CAPABILITY]                                    |
| :-----: | :-------------------- | :----------------- | :---------------------------------------------- |
|  [01]   | `RobotSystem`         | abstract cell root | the `FileIO`-loaded cell owning the batch solve |
|  [02]   | `KinematicSolution`   | class receipt      | per-waypoint joints, planes, config, errors     |
|  [03]   | `RobotConfigurations` | flags enum         | OR-combinable arm-posture branch selection      |
|  [04]   | `Manufacturers`       | enum               | cell vendor and post-processor dialect          |
|  [05]   | `MechanicalGroup`     | class              | robot plus track/positioner as one solved chain |
|  [06]   | `Mechanism`           | abstract chain     | one kinematic chain: payload, joints, base pose |
|  [07]   | `RobotArm`            | class              | six-axis arm specialization of `Mechanism`      |
|  [08]   | `Joint`               | abstract axis      | one revolute/prismatic axis with a radian range |

[Manufacturers]: `ABB` `KUKA` `UR` `Staubli` `FrankaEmika` `Doosan` `Fanuc` `Igus` `Jaka` `All`
[RobotConfigurations]: `None` `Shoulder` `Elbow` `Wrist` `Undefined`

- `RobotSystem.BasePlane`: ref-returning over the system field, so `system.BasePlane = plane` repositions a loaded cell in place — a base-pose search assigns per candidate rather than reloading through `FileIO`.
- `RobotSystem.Remote`: nullable, populated only by vendors shipping a remote driver; a controller lane resolves it through the loaded system and fails typed on absence.
- `RobotConfigurations`: a `CartesianTarget` pins the posture branch (lefty/righty, up/down, flip) to disambiguate multi-solution IK.
- `Manufacturers`: its Franka member is `FrankaEmika`, never `Franka`.

[PUBLIC_TYPE_SCOPE]: the toolpath waypoint value family — `Target` is abstract and implements `IToolpath`; every `TargetProperty` (`Tool`/`Frame`/`Speed`/`Zone`) ships a static `Default` and is an immutable `init`-only `IEquatable` value.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]     | [CAPABILITY]                                    |
| :-----: | :---------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `Target`          | abstract waypoint | toolpath waypoint base over `IToolpath`         |
|  [02]   | `CartesianTarget` | class             | TCP-pose goal triggering inverse kinematics     |
|  [03]   | `JointTarget`     | class             | radian joint goal triggering forward kinematics |
|  [04]   | `Frame`           | value             | conveyor/positioner work-frame coupling         |
|  [05]   | `Tool`            | value             | TCP definition: pose, weight, centroid          |
|  [06]   | `Speed`           | value             | translation/rotation velocity and accel policy  |
|  [07]   | `Zone`            | value             | blend radius and fly-by policy                  |
|  [08]   | `Command`         | class             | inline IO/wait/message emit at a waypoint       |
|  [09]   | `Motions`         | enum              | interpolation type for a `CartesianTarget`      |

[Motions]: `Joint` `Linear` `Process`

- `Zone.IsFlyBy`: true only when `Distance > 0.001`.
- `Command.Default`: its internal `Flatten()` yields nothing, so passing it is an explicit no-op, never a null.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cell loading and the batch solve — `FileIO` is the static cell/element factory parsing the visose XML cell library and `File3dm` meshes into a concrete `RobotSystem`; there is no public `RobotSystem` constructor.

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `FileIO.LoadRobotSystem(string, Plane, bool, IPostProcessor?)`             | static   | position a named local or online cell          |
|  [02]   | `FileIO.ParseRobotSystem(string, Plane, IPostProcessor?)`                  | static   | build a cell from in-memory XML                |
|  [03]   | `FileIO.List(ElementType)`                                                 | static   | enumerate `RobotSystem`/`Tool`/`Frame`         |
|  [04]   | `FileIO.LoadTool(string)`                                                  | static   | load a named tool definition                   |
|  [05]   | `FileIO.LoadFrame(string)`                                                 | static   | load a named frame definition                  |
|  [06]   | `RobotSystem.Kinematics(IReadOnlyList<Target>, IReadOnlyList<double[]?>?)` | instance | batch FK/IK solve -> `List<KinematicSolution>` |
|  [07]   | `RobotSystem.PlaneToNumbers(Plane)`                                        | instance | plane -> controller quaternion/Euler           |
|  [08]   | `RobotSystem.NumbersToPlane(double[])`                                     | instance | controller numbers -> plane                    |
|  [09]   | `Mechanism.Kinematics(Target, double[]?, Plane?)`                          | instance | single-mechanism solve -> `KinematicSolution`  |

- `FileIO.LoadRobotSystem`: `loadMeshes: false` disables display-mesh loading for a headless solve.

[ENTRYPOINT_SCOPE]: program assembly, look-ahead planning, collision, and post emit — `Program`'s constructor IS the toolpath compiler, running the look-ahead motion planner over the toolpath, solving every waypoint and timing the trajectory.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `new Program(string, RobotSystem, IReadOnlyList<IToolpath>, Group?)` | ctor     | compile: solve, plan, fold diagnostics       |
|  [02]   | `Program.Targets`                                                    | property | planned `SystemTarget` trajectory            |
|  [03]   | `Program.Code`                                                       | property | dialect output by group, file, line          |
|  [04]   | `Program.Duration`                                                   | property | planned cycle seconds                        |
|  [05]   | `Program.Warnings`                                                   | property | non-fatal validation diagnostics             |
|  [06]   | `Program.Errors`                                                     | property | fatal validation diagnostics                 |
|  [07]   | `Program.CheckCollisions(...)`                                       | instance | collision query -> `Collision` (unavailable) |
|  [08]   | `Program.Save(string)`                                               | instance | write per-`Manufacturers` dialect files      |
|  [09]   | `Program.CustomCode(List<List<List<string>>>)`                       | instance | substitute hand-authored output              |
|  [10]   | `Program.Animate(double, bool)`                                      | instance | pose the cell at normalized/absolute time    |
|  [11]   | `Program.IsValidIdentifier(string, out string)`                      | static   | the name gate the ctor applies internally    |
|  [12]   | `SystemTarget` / `ProgramTarget`                                     | value    | per-group planned-waypoint receipt           |

- `Program`: its ctor takes optional `IReadOnlyList<int>? multiFileIndices` and `double stepSize`, silently repairing an out-of-range `multiFileIndices` rather than faulting, so a consumer wanting a typed partition fault proves the range before construction.
- `Program.CheckCollisions`: under the `Rhino3dm` substrate this estate builds against, every `Collision` member and its constructor throw `NotSupportedException`, so cell collision evidence states the absence rather than calling through.
- `SystemTarget.Planes`/`Joints`: `FlattenToArray` over every mechanical group, so a positional index is group-ambiguous once a cell carries a positioner or track; a per-group read indexes `ProgramTargets[g].Kinematics`.

[ENTRYPOINT_SCOPE]: mesh posing, remote upload, and the online cell library

| [INDEX] | [SURFACE]                                                                                   | [SHAPE]   | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------------------------ | :-------- | :--------------------------- |
|  [01]   | `RhinoMeshPoser.Pose(RobotSystem, IReadOnlyList<KinematicSolution>, IReadOnlyList<Target>)` | static    | pose cell display meshes     |
|  [02]   | `IRemote.Upload(IProgram)`                                                                  | instance  | upload a program to the cell |
|  [03]   | `IRemote.Play()`                                                                            | instance  | start the loaded program     |
|  [04]   | `IRemote.Pause()`                                                                           | instance  | halt the running program     |
|  [05]   | `IRemote.Log`                                                                               | property  | controller exchange evidence |
|  [06]   | `OnlineLibrary.UpdateLibraryAsync()`                                                        | instance  | sync the online cell roster  |
|  [07]   | `OnlineLibrary.DownloadLibraryAsync(LibraryItem)`                                           | instance  | fetch a cell model           |
|  [08]   | `OnlineLibrary.RemoveDownloadedLibrary(LibraryItem)`                                        | instance  | drop a downloaded cell       |
|  [09]   | `IPostProcessor`                                                                            | interface | per-`Manufacturers` dialect  |

- `IRemote`: resolved through `RobotSystem.Remote`; `RemoteAbb` uses RobotWare HTTP, `RemoteUR` a URScript socket with SFTP, `RemoteFranka` FTP through `SSH.NET` — the SFTP upload path is the `SSH.NET`/`BouncyCastle` consumer.
- `RhinoMeshPoser.Pose`: also carries an instance form, `new RhinoMeshPoser(robot).Pose(solutions, tools)`.
- `IPostProcessor`: `RapidPostProcessor` `KRLPostProcessor` `URScriptPostProcessor` `VAL3PostProcessor` `FanucPostProcessor` `IgusPostProcessor` `JKSPostProcessor` `DrlPostProcessor` `FrankxPostProcessor` emit one dialect each; a custom processor overrides via `LoadRobotSystem`/`ParseRobotSystem`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `RobotSystem.Kinematics(targets, prevJoints) -> List<KinematicSolution>` is the public batch solve; the `FileIO`-loaded concrete cell selects the analytic, numerical, external-axis, or group solver internally, so no solver type is a public surface — the public lever is the cell with the `RobotConfigurations` branch hint on `CartesianTarget`.
- A `CartesianTarget` runs IK from a TCP `Plane` to joints; a `JointTarget` runs FK from joints to chain planes; `prevJoints` threads the prior solution forward, holding a continuous trajectory across wrist-flip and redundant-axis multiplicity.
- Joint values are radians; the `Interval Range` on each `Joint` is the radian limit the solver validates against, and `DegreeToRadian`/`RadianToDegree` convert at the boundary.
- No typed solve exception exists: feasibility, joint-limit, singularity, and reach faults populate `KinematicSolution.Errors` and `Program.Errors`/`Warnings`, and the Fabrication rail folds a non-empty `Errors` into `FabricationFault` rather than catching.

[STACKING]:
- `Rhino3dm`(`.api/api-rhino3dm.md`): the geometry substrate — every `Robots` parameter and result is a `Rhino3dm` `Rhino.Geometry.*` (`Plane`, `Mesh`, `Point3d`, `Transform`, `Interval`, `File3dm`), binary-distinct from RhinoCommon behind the `R3` extern alias, and `File3dm` backs the cell mechanism meshes.
- `Kinematics/cell`: within-lib composer mapping a kernel-canonical pose to a `Rhino3dm` `Plane`/`double[]` joint vector at the kinematics ingress and reading `KinematicSolution.Planes`/`Joints` back at egress; the shared `MotionDynamics` law maps into target speed and blend policy at the same boundary.
- `SSH.NET` + `BouncyCastle.Cryptography`: the `IRemote` SFTP upload path (`RemoteUR`/`RemoteFranka`); a headless solve or post that never calls `IRemote.Upload` exercises neither.

[LOCAL_ADMISSION]:
- `plan-cs` boundary-maps at the kinematics seam: a RhinoCommon geometry instance never enters a `Robots` parameter, and a `Robots` or `Rhino3dm` instance never escapes into a RhinoCommon-typed sibling signature.
- Consumers drive the cell through `FileIO.LoadRobotSystem`/`ParseRobotSystem`, pick a `CartesianTarget` or `JointTarget` per waypoint, and read `Joints`/`Planes`/`Errors`/`Configuration` from the `KinematicSolution`.

[RAIL_LAW]:
- Package: `Robots`
- Owns: host-neutral serial-chain robot kinematics — the `FileIO`-loaded `RobotSystem` cell, the batch FK/IK `Kinematics` solve with `RobotConfigurations` branch selection and previous-pose continuation, the look-ahead-planned `Program` and its per-`Manufacturers` post emit, the `IRemote` upload channel, and the `OnlineLibrary` cell roster.
- Accept: the `Kinematics/cell` solve over a kernel pose mapped to a `Rhino3dm` `Plane`/`double[]` joint vector, read back from `Program.Targets`/`KinematicSolution` and folded to `FabricationFault` on non-empty `Errors`; the cell-program emit through the matching `IPostProcessor` dialect.
- Reject: naming any internal kinematics solver as a public type; passing RhinoCommon geometry into a `Rhino3dm`-typed `Robots` parameter; exception-style control flow over `Errors`/`Warnings`; and active use of `Program.CheckCollisions` in the cell lane instead of `Toolpath/guard`.
