# [RASM_FABRICATION_API_ROBOTS]

`Robots` (visose) is the host-neutral managed serial-chain industrial-robot kinematics + program owner — per-mechanism DH / Modified-DH forward kinematics, inverse kinematics, joint-limit / singularity / reach validation, multi-mechanism + external-axis (track / positioner) cells, the ABB / KUKA / UR / Staubli / FrankaEmika / Doosan / Fanuc / Igus / Jaka manufacturer post-processors, a look-ahead-planned motion `Program`, and a remote upload/play/pause channel. The Fabrication folder admits it as the SOLE robot-kinematics owner, superseding the hand-rolled DH-FK + damped-least-squares Jacobian IK formerly sketched in `Kinematics/cell.md`. The whole API geometry vocabulary is RHINO3DM (`Rhino.Geometry.Plane`, `Mesh`, `Point3d`, `Transform`, `Interval`, `File3dm` from the transitively-floored `Rhino3dm` package — its OWN `Rhino.Geometry.*` identity, distinct from RhinoCommon); `plan-cs` boundary-maps at the kinematics seam, consuming via `Plane`/`double[]` joint vectors and `KinematicSolution`, and NEVER passing a RhinoCommon geometry instance into a `Rhino3dm`-typed `Robots` parameter (the two `Rhino.Geometry` namespaces are binary-distinct types).

The PUBLIC solve contract is `RobotSystem.Kinematics(IReadOnlyList<Target> targets, IReadOnlyList<double[]?>? prevJoints) -> List<KinematicSolution>` — one batch call mapping a waypoint stream to per-waypoint joint + plane solutions. The analytic-vs-numerical and spherical-vs-offset-wrist solver SELECTION is made INTERNALLY by the concrete `RobotSystem` cell loaded from the library (`SphericalWristKinematics`/`OffsetWristKinematics`/`NumericalKinematics`/`TrackKinematics`/`PositionerKinematics`/`CustomKinematics`/`MechanicalGroupKinematics` are all `internal` — they are NOT a public surface and a design page cannot name them as types). The consumer drives the cell through `FileIO.LoadRobotSystem(name, basePlane)` / `FileIO.ParseRobotSystem(xml, basePlane)`, picks a `CartesianTarget` (Cartesian goal -> IK) or `JointTarget` (joint goal -> FK) per waypoint, and reads the `KinematicSolution`'s `Joints`/`Planes`/`Errors`/`Configuration`. There is NO typed exception rail: `KinematicSolution.Errors` (an `IReadOnlyList<string>`) carries unreachable / out-of-limit / singularity diagnostics, so the consumer folds a non-empty `Errors` into the typed band-2700 `FabricationFault` rather than catching.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Robots`
- package: `Robots` (visose)
- license: `MIT`
- assembly: `Robots` (`lib/net8.0/Robots.dll` — SINGLE TFM, the only lib asset; binds forward under `net10.0`)
- namespace: `Robots` (single flat namespace; `Robots.Commands.*` nested for command types)
- asset: pure-managed AnyCPU IL (no native asset of its OWN); the geometry substrate is the transitively-floored `Rhino3dm` (which DOES ship `runtimes/osx-arm64/native/librhino3dm_native.dylib`)
- dependencies (transitive floors, centrally pinned): `Rhino3dm` (the `Rhino.Geometry.*` substrate + osx-arm64 native dylib), `SSH.NET` (the robot-program SFTP upload path on the UR/Franka remotes), `BouncyCastle.Cryptography` (pulled by `SSH.NET`); `Microsoft.Extensions.Logging.Abstractions` resolves upward against the existing substrate row
- consumer-bind note: single-TFM `net8.0`, so the `net10.0` consumer binds the one asset directly; the `[API_TFM_RESOLUTION]` multi-target hazard does NOT apply
- rail: fabrication (`Kinematics/cell` + the cell-program emit path)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the robot-cell and solve contracts
- rail: fabrication

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      |
| :-----: | :-------------------- | :----------------- |
|  [01]   | `RobotSystem`         | abstract cell root |
|  [02]   | `KinematicSolution`   | solve receipt      |
|  [03]   | `RobotConfigurations` | `[Flags]` enum     |
|  [04]   | `Manufacturers`       | manufacturer enum  |
|  [05]   | `MechanicalGroup`     | multi-mechanism    |
|  [06]   | `Mechanism`           | abstract chain     |
|  [07]   | `RobotArm`            | arm specialization |
|  [08]   | `Joint`               | abstract axis      |

[RobotSystem]:

- Members: `Name`, `Controller`, abstract `Manufacturer`, `IO`, `ref Plane BasePlane`, `Mesh DisplayMesh`, `DefaultPose`, `IRemote? Remote`, and `int RobotJointCount`
- `BasePlane` is a REF-returning property over the system's own field, so `system.BasePlane = plane` repositions a loaded cell in place; a base-pose search assigns it per candidate rather than reloading the cell through `FileIO` per pose
- `Remote` is NULLABLE and populated only by the manufacturers shipping a remote driver, so a controller lane resolves it through the loaded system and fails typed on absence rather than admitting a raw `IRemote` from its caller
- Solve: `abstract List<KinematicSolution> Kinematics(IReadOnlyList<Target> targets, IReadOnlyList<double[]?>? prevJoints = null)`
- Boundaries: `abstract double[] PlaneToNumbers(Plane)`, `Plane NumbersToPlane(double[])`, and `virtual Plane CartesianLerp(Plane a, Plane b, double t, double min, double max)`
- Construction: `FileIO` builds `SystemAbb`, `SystemKuka`, `SystemUR`, `SystemStaubli`, `SystemFranka`, `SystemDoosan`, `SystemFanuc`, `SystemIgus`, `SystemJaka`, `CobotSystem`, and `IndustrialSystem`; consumers do not construct them directly.

[KinematicSolution]:

- Receipt: `double[] Joints` in radians, `Plane[] Planes` for the chain frames plus flange and TCP, and `RobotConfigurations Configuration` for the realized arm posture
- Fault channel: `IReadOnlyList<string> Errors` carries unreachable, limit, and singularity diagnostics; the design folds a non-empty collection into `FabricationFault`.

[RobotConfigurations]: `None = 0`, `Shoulder = 1`, `Elbow = 2`, `Wrist = 4`, and `Undefined = 8` are OR-combinable posture flags. A `CartesianTarget` pins this lefty/righty, up/down, and flip selection to disambiguate multi-solution IK.

[Manufacturers]: `ABB`, `KUKA`, `UR`, `Staubli`, `FrankaEmika`, `Doosan`, `Fanuc`, `Igus`, `Jaka`, and `All` select the cell vendor and post-processor dialect. The Franka member is `FrankaEmika`, not `Franka`.

[MechanicalGroup]: `RobotArm Robot`, `Mechanism[] Externals`, `Joint[] Joints`, `RobotJointCount`, and `ExternalJointCount` coordinate a robot with its track or positioner axes as one solved chain.

[Mechanism]: `Manufacturers Manufacturer`, `double Payload`, `ref Plane BasePlane`, `Mesh BaseMesh`, `DisplayMesh`, `Joint[] Joints`, `bool MovesRobot`, and `string Model` define a kinematic chain.

[RobotArm]: `RobotArm : Mechanism` specializes a six-axis arm through the per-vendor `RobotAbb`, `RobotKuka`, `RobotUR`, and corresponding concrete types.

[Joint]: `int Index`, `Number`, `Interval Range`, `double MaxSpeed`, `Plane Plane`, and `Mesh Mesh` define one revolute or prismatic axis. `Range` carries the radian joint limit, and `RevoluteJoint` and `PrismaticJoint` carry the DH parameters.

[PUBLIC_TYPE_SCOPE]: the target value family (the toolpath waypoint vocabulary)
- rail: fabrication
- note: `Target` is abstract and implements `IToolpath`; a `Program` is built over `IReadOnlyList<IToolpath>`. Every `TargetProperty` (`Tool`/`Frame`/`Speed`/`Zone`) ships a static `Default` and is an immutable `IEquatable` value with an `init`-only construction shape.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]     |
| :-----: | :---------------- | :---------------- |
|  [01]   | `Target`          | abstract waypoint |
|  [02]   | `CartesianTarget` | Cartesian goal    |
|  [03]   | `JointTarget`     | joint goal        |
|  [04]   | `Frame`           | work frame        |
|  [05]   | `Tool`            | TCP definition    |
|  [06]   | `Speed`           | velocity policy   |
|  [07]   | `Zone`            | blend radius      |
|  [08]   | `Command`         | inline command    |
|  [09]   | `Motions`         | motion-type enum  |

[Target]: `Target` implements `IToolpath` through `IReadOnlyList<Target> Targets`, which exposes the instance as a single-element toolpath. Its `init`-only surface carries `Tool`, `Frame`, `Speed`, `Zone`, `Command`, `double[] External`, and `string[]? ExternalCustom`; `static Target Default` supplies the default waypoint.

[CartesianTarget]: `Plane Plane`, optional `RobotConfigurations? Configuration`, and `Motions Motion` define a TCP-pose goal that triggers inverse kinematics. Its primary constructor is `(Plane, RobotConfigurations?, Motions, Tool?, Speed?, Zone?, Command?, Frame?, double[]? external, string[]?)`.

[JointTarget]: `double[] Joints` defines a radian joint goal that triggers forward kinematics. `static double[] Lerp(a, b, t, min, max)` and `GetAbsoluteJoint(double)` own joint interpolation.

[Frame]: `Plane Plane`, `int CoupledMechanism`, `CoupledMechanicalGroup`, `bool IsCoupled`, `bool UseController`, and `int? Number` define conveyor or positioner coupling. `static Frame Default` supplies `WorldXY`.

[Tool]: `Plane Tcp`, `double Weight`, `Point3d Centroid`, `Mesh Mesh`, `bool UseController`, and `int? Number` define the TCP; the constructor takes calibration planes, and `static Tool Default` supplies the default value.

[Speed]: `TranslationSpeed` in millimeters per second, `RotationSpeed` in radians per second, external-axis speeds, `TranslationAccel`, `AxisAccel`, and `Time` form the primary-constructor record value; `static Speed Default` supplies the default policy.

[Zone]: `Distance` carries the millimeter blend radius, `Rotation` and `RotationExternal` carry rotation limits, and `bool IsFlyBy` is true when `Distance > 0.001`; `static Zone Default` supplies a stop point.

[Command]: `Robots.Commands.*` emits digital or analog IO changes, waits, messages, and custom code at a waypoint. `Group` aggregates initialization commands for `Program`. `Command` also ships `static Command Default`, whose internal `Flatten()` yields nothing, so passing it is an explicit no-op rather than a null.

[Motions]: `Joint`, `Linear`, and `Process` select PTP joint-space, linear Cartesian, or process-coupled interpolation for a `CartesianTarget`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cell loading + the batch solve (the `RobotSystem` factory and kinematics call)
- rail: fabrication
- note: `FileIO` is the static cell/element factory — it parses the visose XML cell library + the `File3dm`-backed mechanism meshes into a concrete `RobotSystem`. `OnlineLibrary` async-fetches additional cell models. There is no public `RobotSystem` ctor.

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] |
| :-----: | :--------------------------- | :------------- |
|  [01]   | `FileIO.LoadRobotSystem`     | cell load      |
|  [02]   | `FileIO.ParseRobotSystem`    | cell parse     |
|  [03]   | `FileIO.List`                | library query  |
|  [04]   | `FileIO.LoadTool`            | library query  |
|  [05]   | `FileIO.LoadFrame`           | library query  |
|  [06]   | `RobotSystem.Kinematics`     | batch solve    |
|  [07]   | `RobotSystem.PlaneToNumbers` | frame boundary |
|  [08]   | `RobotSystem.NumbersToPlane` | frame boundary |
|  [09]   | `Mechanism.Kinematics`       | sub-solve      |
|  [10]   | `MechanicalGroup`            | chain access   |

[FileIO.LoadRobotSystem]: `FileIO.LoadRobotSystem(string name, Plane basePlane, bool loadMeshes = true, IPostProcessor? postProcessor = null)` positions a named local or online library cell. `loadMeshes` disables display-mesh loading for a headless solve.

[FileIO.ParseRobotSystem]: `FileIO.ParseRobotSystem(string xml, Plane basePlane, IPostProcessor? postProcessor = null)` builds an embedded `RobotSystem` from in-memory cell XML without a library lookup.

[FileIO.LIBRARY]: `FileIO.List(ElementType)` enumerates `ElementType.RobotSystem`, `Tool`, and `Frame`; `FileIO.LoadTool(string)` and `FileIO.LoadFrame(string)` load named definitions.

[RobotSystem.Kinematics]: `RobotSystem.Kinematics(IReadOnlyList<Target> targets, IReadOnlyList<double[]?>? prevJoints = null)` is the sole public batch solve. It maps `CartesianTarget` through IK and `JointTarget` through FK to `KinematicSolution`, while `prevJoints` preserves trajectory continuity across redundant-axis and wrist-flip choices.

[RobotSystem.FRAME_BOUNDARY]: `RobotSystem.PlaneToNumbers(Plane)` and `NumbersToPlane(double[])` convert a Rhino `Plane` to and from the controller's quaternion or dialect-specific Euler representation.

[Mechanism.Kinematics]: `Mechanism.Kinematics(Target, double[]? prevJoints, Plane? basePlane)` owns the public single-mechanism solve beneath the system solve. `MechanicalGroup` exposes `Robot`, `Externals`, and `Joints` for coordinated-chain inspection.

[ENTRYPOINT_SCOPE]: program assembly, look-ahead motion planning, collision check, and post emit
- rail: fabrication
- note: `Program` IS the toolpath compiler — its ctor runs the look-ahead motion planner (`ProgramMotionPlanner`) over the toolpath, solving every waypoint and timing the trajectory. `Code` holds the emitted per-group / per-file string lists; `Save` writes the dialect files.

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY]   |
| :-----: | :------------------------ | :--------------- |
|  [01]   | `Program`                 | program build    |
|  [02]   | `Program.Targets`         | program read     |
|  [03]   | `Program.Code`            | program read     |
|  [04]   | `Program.Duration`        | program read     |
|  [05]   | `Program.Warnings`        | diagnostics      |
|  [06]   | `Program.Errors`          | diagnostics      |
|  [07]   | `Program.CheckCollisions` | collision        |
|  [08]   | `Program.Save`            | post emit        |
|  [09]   | `Program.CustomCode`      | post replacement |
|  [10]   | `Program.Animate`         | simulation       |
|  [11]   | `CurrentSimulationPose`   | simulation       |
|  [12]   | `HasSimulation`           | simulation       |
|  [13]   | `SystemTarget`            | planned waypoint |
|  [14]   | `ProgramTarget`           | planned waypoint |

[Program.BUILD]: `new Program(string name, RobotSystem robotSystem, IReadOnlyList<IToolpath> toolpaths, Robots.Commands.Group? initCommands = null, IReadOnlyList<int>? multiFileIndices = null, double stepSize = 1.0)` solves every waypoint, runs look-ahead jerk- and acceleration-limited feedrate planning, and folds `Warnings` and `Errors` into the compiled program. `static bool Program.IsValidIdentifier(string name, out string error)` is the name gate the ctor also applies internally; the ctor silently repairs out-of-range `multiFileIndices` instead of faulting, so a consumer wanting a typed partition fault proves the range before construction.

[Program.READ]: `IReadOnlyList<SystemTarget> Targets` carries the planned trajectory; `toSeq(program.Targets).Map(t => t.Joints)` and `t.Planes` expose the receipt's joint and flange data. `List<List<List<string>>>? Code` carries dialect output by mechanical group, file, and line; `Duration` carries the planned cycle seconds, and `Warnings` and `Errors` carry validation diagnostics.

[Program.COLLISION]: `Program.CheckCollisions(IReadOnlyList<int>? first, IReadOnlyList<int>? second, Mesh? environment, int environmentPlane, double linearStep, double angularStep)` returns `Collision`, declaring `HasCollision`, `Mesh[] Meshes`, and `SystemTarget CollisionTarget`. TRAP — under the `Rhino3dm` geometry substrate this estate builds against, EVERY `Collision` member and its constructor throw `NotSupportedException("Collisions are not available when Robots is built against rhino3dm.")`; the surface exists in metadata and returns nothing at runtime. No fence composes it, robot self-collision has no `Robots`-side owner here, and a page reaching for cell collision evidence states the absence rather than calling through a throwing member.

[Program.POST]: `Program.Save(string folder)` writes the `Manufacturers` dialect files in RAPID, KRL, URScript, VAL3, DRL, and corresponding formats; `IProgram.Save` mirrors the interface. `Program.CustomCode(List<List<List<string>>>)` substitutes hand-authored output.

[Program.SIMULATION]: `Program.Animate(double time, bool isNormalized = true)` poses the cell at normalized or absolute time. `CurrentSimulationPose` exposes the pose, `HasSimulation` reports availability, and `MeshPoser` drives posed meshes.

[Program.WAYPOINTS]: `SystemTarget.Index`, `Planes`, `Joints`, `TotalTime`, and `DeltaTime` describe the planned waypoint, and `IReadOnlyList<ProgramTarget> ProgramTargets` carries one entry per mechanical group. `ProgramTarget.Kinematics` carries its `KinematicSolution`, while `WorldPlane`, `IsJointMotion`, and `ForcedConfiguration` complete the visualization and timing receipt.

[SystemTarget.FLATTENING]: `SystemTarget.Planes` and `Joints` are `ProgramTargets.FlattenToArray(...)` over EVERY mechanical group, so a positional index into them is group-ambiguous the moment a cell carries a positioner or track. A per-group read — the flange, the realized configuration, one group's joint vector — indexes `ProgramTargets[g].Kinematics.Planes`/`Joints`, never an offset into the flattened array.

[ENTRYPOINT_SCOPE]: mesh posing, remote upload, and the online cell library
- rail: fabrication

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] |
| :-----: | :-------------------------------------- | :------------- |
|  [01]   | `RhinoMeshPoser.Pose`                   | mesh pose      |
|  [02]   | `IRemote.Upload`                        | remote drive   |
|  [03]   | `IRemote.Play`                          | remote drive   |
|  [04]   | `IRemote.Pause`                         | remote drive   |
|  [05]   | `OnlineLibrary.UpdateLibraryAsync`      | library sync   |
|  [06]   | `OnlineLibrary.DownloadLibraryAsync`    | library sync   |
|  [07]   | `OnlineLibrary.RemoveDownloadedLibrary` | library sync   |
|  [08]   | `IPostProcessor`                        | post dialect   |
|  [09]   | `IRemote.Log`                           | drive evidence |

[RhinoMeshPoser.Pose]: `RhinoMeshPoser.Pose(RobotSystem robot, IReadOnlyList<KinematicSolution> solutions, IReadOnlyList<Target> targets)` transforms `Rhino3dm` cell display meshes to a solved pose. `new RhinoMeshPoser(robot).Pose(solutions, tools)` is the instance form, and `IMeshPoser` and `SimpleTrail` are the abstractions.

[IRemote]: `IRemote.Upload(IProgram)`, `Play()`, and `Pause()` drive the live controller through `RobotSystem.Remote`; `List<string> Log` retains controller exchange evidence. `RemoteAbb` uses RobotWare HTTP and sockets, `RemoteUR` uses a URScript socket plus `RemoteURFtp` SFTP, and `RemoteFranka` uses FTP through `SSH.NET`; robot-program SFTP upload is the `SSH.NET` transitive-floor consumer.

[OnlineLibrary]: `UpdateLibraryAsync()`, `DownloadLibraryAsync(LibraryItem)`, and `RemoveDownloadedLibrary(LibraryItem)` manage the online cell-model library and grow the runtime `Manufacturers` roster. The `IDisposable` surface carries the `Libraries` dictionary, `LibraryChanged` event, and `LibraryItem.IsOnline`, `IsDownloaded`, and `IsUpdateAvailable` state.

[IPostProcessor]: `RapidPostProcessor`, `KRLPostProcessor`, `URScriptPostProcessor`, `VAL3PostProcessor`, `FanucPostProcessor`, `IgusPostProcessor`, `JKSPostProcessor`, `DrlPostProcessor`, and `FrankxPostProcessor` emit one dialect per `Manufacturers` value. A custom `IPostProcessor` overrides the default through `LoadRobotSystem` or `ParseRobotSystem`.

## [04]-[IMPLEMENTATION_LAW]

[KINEMATICS_LAW]:
- The PUBLIC solve is the batch `RobotSystem.Kinematics(targets, prevJoints) -> List<KinematicSolution>`.
- The analytic `SphericalWristKinematics` and `OffsetWristKinematics`, iterative `NumericalKinematics`, external-axis `TrackKinematics` and `PositionerKinematics`, and group `MechanicalGroupKinematics` solvers are `internal`, so a design page cannot name them as types.
- The concrete `RobotSystem` cell loaded by `FileIO` selects the solver internally; public selection combines the cell with the `RobotConfigurations` branch hint on `CartesianTarget`.
- A `CartesianTarget` runs inverse kinematics from TCP `Plane` to joints, while a `JointTarget` runs forward kinematics from joints to chain `Plane` values.
- `KinematicSolution` carries radian `Joints`, the full link, flange, and TCP `Planes` chain, the realized `Configuration`, and `Errors`.
- `prevJoints` threads the prior solution's `Joints` forward, preserving a continuous waypoint trajectory across wrist-flip and redundant-axis multiplicity.
- There is NO typed packing/solve exception: feasibility, joint-limit, singularity, and reach faults populate `KinematicSolution.Errors` plus `Program.Errors`/`Warnings`. The Fabrication rail folds non-empty diagnostics into band-2700 `FabricationFault`; swept cutter and holder collision routes through `Toolpath/guard`, and `Program.CheckCollisions` is unavailable under this build per `[Program.COLLISION]`.
- Joint values are RADIANS internally; `MechanicalGroup.DegreeToRadian`/`RadianToDegree` and `RobotSystem.DegreeToRadian` convert at the boundary. The `Interval Range` on each `Joint` is the radian limit the solver validates against.

[GEOMETRY_BOUNDARY_LAW]:
- Every geometry parameter and result uses `Rhino3dm`'s `Rhino.Geometry.*`: `Plane`, `Mesh`, `Point3d`, `Transform`, `Interval`, and `File3dm`.
- `Rhino3dm` and RhinoCommon expose binary-distinct `Rhino.Geometry.*` types from separate assemblies and namespaces.
- `plan-cs` maps the canonical pose or frame to a `Rhino3dm` `Plane` at the kinematics ingress, then reads plain `Plane[]` and `double[]` values from `KinematicSolution.Planes` and `Joints` at egress.
- A RhinoCommon geometry instance never enters a `Robots` parameter, and a `Robots` or `Rhino3dm` geometry instance never escapes into a sibling signature expecting RhinoCommon.
- `Rhino3dm` (not `Robots`) is the package shipping the osx-arm64 native `librhino3dm_native.dylib`; `Robots` itself is pure-managed IL. `SSH.NET` + `BouncyCastle.Cryptography` are pulled only by the `IRemote` SFTP upload path (`RemoteUR`/`RemoteFranka`); a headless solve/post that never calls `IRemote.Upload` exercises neither.

[RAIL_LAW]:
- Package: `Robots` (visose; assembly `Robots`; geometry substrate `Rhino3dm`)
- Owns: host-neutral serial-chain robot kinematics — the `FileIO`-loaded `RobotSystem` cell, the batch `Kinematics(targets, prevJoints) -> List<KinematicSolution>` FK/IK solve over `CartesianTarget`/`JointTarget` waypoints with `RobotConfigurations` branch selection and previous-pose continuation, the look-ahead-planned `Program` (jerk/accel-limited feedrate, `CheckCollisions`, per-`Manufacturers` post emit, `Save`), the `IRemote` upload/play/pause channel, and the `OnlineLibrary` cell roster
- Accept: the `Kinematics/cell` solve consuming a kernel-canonical pose mapped to a `Rhino3dm` `Plane`/`double[]` joint vector, the result read back from `Program.Targets` / `KinematicSolution.Joints` / `Planes` / `Errors` / `Configuration` and folded to a `FabricationFault` on non-empty `Errors`; the cell-program emit through the matching `IPostProcessor` dialect; the shared `MotionDynamics` law mapped into target speed/blend policy at the Rhino3dm boundary
- Reject solver surface: naming any `internal` kinematics-solver class (`SphericalWristKinematics`/`OffsetWristKinematics`/`NumericalKinematics`/`TrackKinematics`/`PositionerKinematics`/`MechanicalGroupKinematics`/`CustomKinematics`) as a public type — the public lever is the cell + `RobotConfigurations`, not a solver instantiation
- Reject boundary drift: passing RhinoCommon geometry into a Rhino3dm-typed `Robots` parameter, escaping Rhino3dm geometry into a RhinoCommon sibling signature, exception-style control flow over `Errors`/`Warnings`, or active use of `Program.CheckCollisions` inside the cell lane instead of `Toolpath/guard`
