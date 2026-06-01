; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category              | Severity | Notes
--------|-----------------------|----------|---------------------------------
CSP0001 | FunctionalDiscipline  | Error    | ImperativeControlFlow
CSP0002 | FunctionalDiscipline  | Error    | MatchCollapse
CSP0003 | TypeDiscipline        | Error    | PrimitiveSignature
CSP0004 | TypeDiscipline        | Error    | CollectionSignature
CSP0005 | SurfaceArea           | Error    | OverloadSpam
CSP0006 | AsyncDiscipline       | Error    | AsyncBlocking
CSP0007 | TimeDiscipline        | Error    | WallClock
CSP0008 | ResourceManagement    | Error    | HttpClientConstruction
CSP0009 | FunctionalDiscipline  | Error    | ExceptionControlFlow
CSP0010 | AsyncDiscipline       | Error    | AsyncVoid
CSP0011 | TypeDiscipline        | Error    | MutableCollection
CSP0012 | TypeDiscipline        | Error    | MutableAutoProperty
CSP0013 | PerformanceDiscipline | Error    | ClosureCapture
CSP0014 | AsyncDiscipline       | Error    | TaskRunFanOut
CSP0015 | TypeDiscipline        | Error    | VarInference
CSP0017 | PerformanceDiscipline | Error    | NonStaticHotPathClosure
CSP0104 | FunctionalDiscipline  | Error    | NullSentinel
CSP0201 | TypeDiscipline        | Error    | ArraySignature
CSP0202 | TypeDiscipline        | Error    | MutableField
CSP0203 | TypeDiscipline        | Error    | PublicCtorOnValidatedPrimitive
CSP0204 | TypeDiscipline        | Error    | ConcurrentCollectionInDomain
CSP0301 | AsyncDiscipline       | Error    | FireAndForgetTask
CSP0302 | AsyncDiscipline       | Error    | UnboundedWhenAll
CSP0303 | AsyncDiscipline       | Error    | RunInTransform
CSP0401 | ResourceManagement    | Error    | TimerConstruction
CSP0402 | ResourceManagement    | Error    | FluentValidationInDomain
CSP0403 | ResourceManagement    | Error    | FluentValidationValidateSync
CSP0404 | ResourceManagement    | Error    | ChannelUnboundedTopology
CSP0405 | ResourceManagement    | Error    | ChannelFullModeRequired
CSP0406 | ResourceManagement    | Error    | ScrutorScanRegistrationStrategy
CSP0501 | SurfaceArea           | Error    | InterfacePollution
CSP0502 | SurfaceArea           | Error    | PositionalDomainArguments
CSP0503 | SurfaceArea           | Error    | SingleUsePrivateHelper
CSP0504 | FunctionalDiscipline  | Error    | EffectReturnPolicy
CSP0505 | TypeDiscipline        | Error    | TypeClassStaticAbstractPolicy
CSP0506 | SurfaceArea           | Error    | ExtensionProjectionRequired
CSP0601 | PerformanceDiscipline | Error    | HotPathLinq
CSP0602 | PerformanceDiscipline | Error    | HotPathNonStaticLambda
CSP0603 | PerformanceDiscipline | Error    | LibraryImportRequired
CSP0604 | PerformanceDiscipline | Error    | TelemetryIdentityConstruction
CSP0605 | PerformanceDiscipline | Error    | HardcodedOtlpEndpoint
CSP0606 | PerformanceDiscipline | Error    | RegexStaticMethodCall
CSP0607 | PerformanceDiscipline | Error    | GeneratedRegexCharsetValidation
CSP0608 | AsyncDiscipline       | Error    | EnumeratorCancellationMissing
CSP0701 | TypeDiscipline        | Error    | PrimitiveShape
CSP0702 | TypeDiscipline        | Error    | DuShape
CSP0703 | TypeDiscipline        | Error    | ValidationType
CSP0704 | PerformanceDiscipline | Error    | RegexRuntimeConstruction
CSP0705 | FunctionalDiscipline  | Error    | MatchBoundaryOnlyStrict
CSP0706 | FunctionalDiscipline  | Error    | EarlyReturnGuardChain
CSP0707 | FunctionalDiscipline  | Error    | VariableReassignment
CSP0708 | SurfaceArea           | Error    | ApiSurfaceInflationByPrefix
CSP0709 | FunctionalDiscipline  | Error    | NullPatternSentinel
CSP0710 | FunctionalDiscipline  | Error    | FilterMapChainOnSeq
CSP0711 | FunctionalDiscipline  | Error    | AsyncAwaitInEff
CSP0712 | TypeDiscipline        | Error    | AtomRefAsProperty
CSP0713 | TypeDiscipline        | Error    | CreateFactoryReturnType
CSP0714 | TypeDiscipline        | Error    | DateTimeFieldInDomain
CSP0715 | TypeDiscipline        | Error    | AnemicEntityDetection
CSP0717 | TypeDiscipline        | Error    | WithExpressionBypass
CSP0718 | FunctionalDiscipline  | Error    | MutableAccumulatorInLoop
CSP0719 | TypeDiscipline        | Error    | UnsafeNumericConversion
CSP0720 | TypeDiscipline        | Error    | InitOnlyBypassOnValidated
CSP0723 | FunctionalDiscipline  | Error    | RhinoActiveDocLeak
CSP0724 | TypeDiscipline        | Error    | FlagsEnumOveruse
CSP0725 | FunctionalDiscipline  | Error    | ImperativeAccumulator
CSP0726 | SurfaceArea           | Error    | PositionalRecordConstructor
CSP0727 | FunctionalDiscipline  | Error    | SwitchExpressionPrecedence
CSP0728 | FunctionalDiscipline  | Error    | MapFailDiscardsException
CSP0729 | SurfaceArea           | Error    | OverloadAdjacency
CSP0802 | TypeDiscipline        | Error    | UnionOpsQualification
