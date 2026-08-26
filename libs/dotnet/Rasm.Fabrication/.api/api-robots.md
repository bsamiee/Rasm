# [RASM_FABRICATION_API_ROBOTS]

`Robots` (visose) owns host-neutral managed serial-chain robot kinematics and motion-program emission: per-mechanism forward/inverse kinematics, joint-limit, singularity, and reach validation, multi-mechanism and external-axis cells, per-vendor post-processors, a look-ahead-planned `Program`, and a remote upload channel. Its entire geometry vocabulary is `Rhino3dm`'s `Rhino.Geometry.*`, binary-distinct from RhinoCommon, so `plan-cs` boundary-maps at the kinematics boundary and never passes a RhinoCommon instance into a `Robots` parameter. Fabrication admits it as the sole robot-kinematics owner.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the robot-cell and solve contracts

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      | [CAPABILITY]                                    |
| :-----: | :-------------------- | :----------------- | :---------------------------------------------- |
|  [01]   | `RobotSystem`         | abstract cell root | the `FileIO`-loaded cell owning the batch solve |
|  [02]   | `KinematicSolution`   | class solution     | per-waypoint joints, planes, config, errors     |
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
- `Manufacturers`: its Franka member is `FrankaEmika`, never `Franka`; `All` is a filter token, not a vendor.

[PUBLIC_TYPE_SCOPE]: the cell topology roster at member depth — the declared payload, vendor, and per-link travel a provider-free equipment projection reads without naming a solver.

| [INDEX] | [SYMBOL]                            | [SHAPE]  | [CAPABILITY]                                                     |
| :-----: | :---------------------------------- | :------- | :--------------------------------------------------------------- |
|  [01]   | `IndustrialSystem.MechanicalGroups` | property | `IReadOnlyList<MechanicalGroup>`: the only public group roster   |
|  [02]   | `MechanicalGroup.Robot`             | property | `RobotArm`: the group's one arm, non-null by ctor invariant      |
|  [03]   | `MechanicalGroup.Externals`         | property | `Mechanism[]`: every track and positioner coupled to the arm     |
|  [04]   | `MechanicalGroup.Joints`            | property | `Joint[]`: arm and external links flattened, ordered by `Number` |
|  [05]   | `MechanicalGroup.RobotJointCount`   | property | `int`: `Robot.Joints.Length`, the arm block's own width          |
|  [06]   | `Mechanism.Manufacturer`            | property | `Manufacturers`: the vendor `RobotArm` inherits                  |
|  [07]   | `Mechanism.Payload`                 | property | `double` kilograms declared by the cell XML                      |
|  [08]   | `Mechanism.Model`                   | property | `string`: `"{manufacturer}.{model}"`                             |
|  [09]   | `Joint.Index`                       | property | `int`: position within its OWN mechanism, not the group          |
|  [10]   | `Joint.Number`                      | property | `int`: the cell-wide ordering key `Joints` sorts on              |
|  [11]   | `Joint.Range`                       | property | `Interval`: the admitted travel limit, `T0` below `T1`           |
|  [12]   | `Joint.MaxSpeed`                    | property | `double`: the per-link speed ceiling                             |

- `RobotSystem` keeps `GetRobot`/`GetJoints`/`Payload` internal, so a group roster read type-tests for `IndustrialSystem` and a system without one refuses rather than asserting a single arm.
- `Mechanism.InitJoints` normalizes at construction: a `RevoluteJoint`'s `Range` and `MaxSpeed` leave in radians and radians per second, while a `PrismaticJoint` keeps the cell XML's millimetres — the joint's runtime type IS the unit discriminator, and `RevoluteJoint`/`PrismaticJoint` add no members of their own.
- `Joint.Index` collides across mechanisms — an arm's first link and a track's first axis both read `0` — so an axis-seating correspondence indexes the arm chain and the external block separately rather than reading `Index` off the flattened `MechanicalGroup.Joints`.

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

- `Command.Default`: its internal `Flatten()` yields nothing, so passing it is an explicit no-op, never a null.

[PUBLIC_TYPE_SCOPE]: the motion-policy value members a dynamics law maps onto — both are `init`-only, so a caller derives from the static `Default` through a `with` expression and never constructs a partial policy.

| [INDEX] | [SYMBOL]                    | [UNIT]  | [CAPABILITY]                                               |
| :-----: | :-------------------------- | :------ | :--------------------------------------------------------- |
|  [01]   | `Speed.TranslationSpeed`    | mm/s    | TCP linear velocity ceiling                                |
|  [02]   | `Speed.RotationSpeed`       | rad/s   | TCP angular velocity ceiling                               |
|  [03]   | `Speed.TranslationExternal` | mm/s    | linear external-axis velocity ceiling                      |
|  [04]   | `Speed.RotationExternal`    | rad/s   | rotary external-axis velocity ceiling                      |
|  [05]   | `Speed.TranslationAccel`    | mm/s²   | TCP linear acceleration ceiling                            |
|  [06]   | `Speed.AxisAccel`           | rad/s²  | joint angular acceleration ceiling                         |
|  [07]   | `Speed.Time`                | s       | fixed waypoint duration; overrides the velocity ceilings   |
|  [08]   | `Speed.Default`             | static  | 100 mm/s, π rad/s, 5000/6π external, 2500 mm/s², 4π rad/s² |
|  [09]   | `Zone.Distance`             | mm      | blend radius at the waypoint                               |
|  [10]   | `Zone.Rotation`             | rad     | orientation blend tolerance                                |
|  [11]   | `Zone.RotationExternal`     | rad     | rotary external-axis blend tolerance                       |
|  [12]   | `Zone.IsFlyBy`              | derived | `Distance > 0.001`                                         |
|  [13]   | `Zone.Default`              | static  | zero distance, so the default target is an exact stop      |

- `Speed`: every axis is a ceiling the look-ahead planner respects, so a `Time` above zero pins the waypoint duration and the velocity columns stop governing — a dynamics law writes one or the other, never both.
- `Zone.Rotation`/`RotationExternal`: nullable ctor parameters defaulting off `Distance`, so a policy setting distance alone still yields a coherent orientation blend; the properties themselves read `double`.

## [02]-[ENTRYPOINTS]

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
|  [10]   | `Program.Animate(double time, bool isNormalized = true)`             | instance | pose the cell at normalized/absolute time    |
|  [11]   | `Program.IsValidIdentifier(string, out string)`                      | static   | the name gate the ctor applies internally    |
|  [12]   | `Program.HasSimulation`                                              | property | `bool`: the animation cursor exists          |
|  [13]   | `Program.CurrentSimulationPose`                                      | property | `SimulationPose` at the last `Animate` time  |

- `Program`: its ctor takes optional `IReadOnlyList<int>? multiFileIndices` and `double stepSize`, silently repairing an out-of-range `multiFileIndices` rather than faulting, so a consumer wanting a typed partition fault proves the range before construction.
- `Program.CheckCollisions`: under the `Rhino3dm` substrate this solution builds against, every `Collision` member and its constructor throw `NotSupportedException`, so cell collision evidence states the absence rather than calling through.
- `Program.CurrentSimulationPose`: THROWS `InvalidOperationException("This program cannot be animated.")` whenever the ctor produced no simulation, so every read gates on `Program.HasSimulation` first; the property is not `Option`-shaped and no `TryGet` sibling exists.
- `Program.Animate`: `isNormalized: true` reads `time` over `[0,1]`, `false` over `[0, Duration]` seconds; it advances the cursor `CurrentSimulationPose` then reports, so animate-then-read is one imperative pair, never two independent calls.

[PUBLIC_TYPE_SCOPE]: the planned-trajectory and simulation-cursor results at member depth — `SystemTarget` is the per-waypoint planner row, `SimulationPose` the between-waypoint cursor `Animate` advances.

| [INDEX] | [SYMBOL]                           | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :--------------------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `SystemTarget.ProgramTargets`      | property | `IReadOnlyList<ProgramTarget>`: one row per `MechanicalGroup`   |
|  [02]   | `SystemTarget.Index`               | property | `int`: ordinal into `Program.Targets`                           |
|  [03]   | `SystemTarget.TotalTime`           | property | `double`: planned seconds elapsed at this waypoint              |
|  [04]   | `SystemTarget.DeltaTime`           | property | `double`: planned seconds spent reaching it                     |
|  [05]   | `SystemTarget.Planes`              | property | `Plane[]` flattened over every group                            |
|  [06]   | `SystemTarget.Joints`              | property | `double[]` radians flattened over every group                   |
|  [07]   | `SimulationPose.Kinematics`        | property | `IReadOnlyList<KinematicSolution>`: one per group at the cursor |
|  [08]   | `SimulationPose.TargetIndex`       | property | `int`: the waypoint the cursor sits at or between               |
|  [09]   | `SimulationPose.CurrentTime`       | property | `double`: the provider's own clock reading in seconds           |
|  [10]   | `SimulationPose.GetLastPlane(int)` | instance | flange `Plane` for one group index                              |

- `SystemTarget.Planes`/`Joints`: `FlattenToArray` over every mechanical group, so a positional index is group-ambiguous once a cell carries a positioner or track; a per-group read indexes `ProgramTargets[g].Kinematics`.
- `SimulationPose.TargetIndex`: the member is `TargetIndex`, never `Index` — `SystemTarget` is the type carrying `Index`, and a cursor read spelling `Index` binds nothing.
- `SimulationPose`: every column is `internal set`, so the instance the property hands back is the live cursor the next `Animate` mutates; a census retaining poses across instants copies each column at the read.

[ENTRYPOINT_SCOPE]: mesh posing, remote upload, and the online cell library

| [INDEX] | [SURFACE]                                                                                   | [SHAPE]   | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------------------------ | :-------- | :--------------------------- |
|  [01]   | `RhinoMeshPoser.Pose(RobotSystem, IReadOnlyList<KinematicSolution>, IReadOnlyList<Target>)` | static    | pose cell display meshes     |
|  [02]   | `IRemote.Upload(IProgram)`                                                                  | instance  | upload a program to the cell |
|  [03]   | `IRemote.Play()`                                                                            | instance  | start the loaded program     |
|  [04]   | `IRemote.Pause()`                                                                           | instance  | halt the running program     |
|  [05]   | `IRemote.Log`                                                                               | property  | `List<string>` exchange log  |
|  [06]   | `IRemote.IP`                                                                                | property  | `string?` get/set controller |
|  [07]   | `OnlineLibrary.UpdateLibraryAsync()`                                                        | instance  | sync the online cell roster  |
|  [08]   | `OnlineLibrary.DownloadLibraryAsync(LibraryItem)`                                           | instance  | fetch a cell model           |
|  [09]   | `OnlineLibrary.RemoveDownloadedLibrary(LibraryItem)`                                        | instance  | drop a downloaded cell       |
|  [10]   | `OnlineLibrary.Libraries`                                                                   | property  | `Dictionary` of known cells  |
|  [11]   | `OnlineLibrary.LibraryChanged`                                                              | event     | `Action?` roster mutation    |
|  [12]   | `OnlineLibrary.Dispose()`                                                                   | instance  | releases the HTTP client     |
|  [13]   | `IPostProcessor`                                                                            | interface | per-`Manufacturers` dialect  |

- `IRemote`: resolved through `RobotSystem.Remote`; `RemoteAbb` uses RobotWare HTTP, `RemoteUR` a URScript socket with SFTP, `RemoteFranka` FTP through `SSH.NET` — the SFTP upload path is the `SSH.NET`/`BouncyCastle` consumer.
- `IRemote.IP`: settable and nullable, so a lane reads it as `Option<string>` evidence beside the exchange log rather than asserting a controller address the driver never resolved.
- `OnlineLibrary`: `IDisposable`, so every verb brackets one instance; `Libraries` is `Dictionary<string, LibraryItem>` under `StringComparer.OrdinalIgnoreCase`, and `LibraryChanged` fires on the same mutations the three verbs perform — a bracketed one-shot reads `Libraries` after the await instead of subscribing.
- `RhinoMeshPoser.Pose`: also carries an instance form, `new RhinoMeshPoser(robot).Pose(solutions, tools)`.
- `IPostProcessor`: `RapidPostProcessor` `KRLPostProcessor` `URScriptPostProcessor` `VAL3PostProcessor` `FanucPostProcessor` `IgusPostProcessor` `JKSPostProcessor` `DrlPostProcessor` `FrankxPostProcessor` emit one dialect each; a custom processor overrides via `LoadRobotSystem`/`ParseRobotSystem`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `RobotSystem.Kinematics(targets, prevJoints) -> List<KinematicSolution>` is the public batch solve; the `FileIO`-loaded concrete cell selects the analytic, numerical, external-axis, or group solver internally, so no solver type is a public surface — the public lever is the cell with the `RobotConfigurations` branch hint on `CartesianTarget`.
- A `CartesianTarget` runs IK from a TCP `Plane` to joints; a `JointTarget` runs FK from joints to chain planes; `prevJoints` threads the prior solution forward, holding a continuous trajectory across wrist-flip and redundant-axis multiplicity.
- Joint values are radians; the `Interval Range` on each `Joint` is the radian limit the solver validates against, and `DegreeToRadian`/`RadianToDegree` convert at the boundary.
- No typed solve exception exists: feasibility, joint-limit, singularity, and reach faults populate `KinematicSolution.Errors` and `Program.Errors`/`Warnings`, and the Fabrication pipeline folds a non-empty `Errors` into `FabricationFault` rather than catching.
- The simulation cursor is the ONE between-waypoint read: `Program.Targets` reports the planner's own waypoints while `Animate` plus `CurrentSimulationPose` resolves the cell at an arbitrary instant, and `Program.Duration` is the horizon a sampler divides. `HasSimulation` is the only non-throwing probe of cursor existence, so the gate precedes every cursor read and a program the ctor could not simulate refuses typed instead of surfacing an `InvalidOperationException`.

[STACKING]:
- `Rhino3dm`(`.api/api-rhino3dm.md`): the geometry substrate — every `Robots` parameter and result is a `Rhino3dm` `Rhino.Geometry.*` (`Plane`, `Mesh`, `Point3d`, `Transform`, `Interval`, `File3dm`), binary-distinct from RhinoCommon behind the `R3` extern alias, and `File3dm` backs the cell mechanism meshes.
- `Kinematics/cell`: the sole within-lib composer, mapping a kernel-canonical pose to a `Rhino3dm` `Plane`/`double[]` joint vector at the kinematics ingress and reading `KinematicSolution.Planes`/`Joints` back at egress; the shared `MotionDynamics` law maps into target speed and blend policy at the same boundary, and `IndustrialSystem.MechanicalGroups` projects into provider-free equipment rows there so no sibling page names a `Robots` type.
- `SSH.NET` + `BouncyCastle.Cryptography`: the `IRemote` SFTP upload path (`RemoteUR`/`RemoteFranka`); a headless solve or post that never calls `IRemote.Upload` exercises neither.

[LOCAL_ADMISSION]:
- `plan-cs` boundary-maps at the kinematics boundary: a RhinoCommon geometry instance never enters a `Robots` parameter, and a `Robots` or `Rhino3dm` instance never escapes into a RhinoCommon-typed sibling signature.
- Consumers drive the cell through `FileIO.LoadRobotSystem`/`ParseRobotSystem`, pick a `CartesianTarget` or `JointTarget` per waypoint, and read `Joints`/`Planes`/`Errors`/`Configuration` from the `KinematicSolution`.
