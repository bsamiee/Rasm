# [RASM_APPHOST_API_GRPC_CORE_API]

`Grpc.Core.Api` owns the gRPC service-definition surface AppHost `ControlService` binds: `Marshaller` codec pairs, `Method<TReq,TResp>` descriptors keyed by `FullName`, the `MethodType` call-shape vocabulary, and the `ServerServiceDefinition` handler registry. The status, fault, and call-policy carriers this boundary types against are the branch client catalogue's.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Core.Api`
- package: `Grpc.Core.Api`
- assembly: `Grpc.Core.Api`
- namespace: `Grpc.Core`
- asset: runtime library
- rail: gRPC service-definition boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: marshallers, method descriptors, and the handler registry

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `Marshaller<T>`           | class         | serializer + deserializer pair per message type |
|  [02]   | `Marshallers`             | class         | marshaller factory and `StringMarshaller`       |
|  [03]   | `Method<TReq,TResp>`      | class         | method descriptor keyed by `FullName`           |
|  [04]   | `MethodType`              | enum          | the four call-shape cases                       |
|  [05]   | `ServerServiceDefinition` | class         | method-to-handler registration builder          |

- Registers the transitive `Grpc.Core.Api` fault and call-policy carriers(`libs/csharp/.api/api-grpc-client.md`): `Status`, `StatusCode`, `RpcException`, and `CallOptions` carry their construction, roster, read-back, and `With*` threading there and this boundary types against that spelling; the rows above are the carriers this boundary adds beyond it.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: marshaller, method descriptor, and service definition

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `Marshaller<T>(Func<T,byte[]>, Func<byte[],T>)`                                 | ctor     | byte-array codec pair                   |
|  [02]   | `Marshaller<T>(Action<T,SerializationContext>, Func<DeserializationContext,T>)` | ctor     | contextual pooling codec                |
|  [03]   | `Marshallers.Create<T>(serializer, deserializer)`                               | static   | byte-array or contextual shorthand      |
|  [04]   | `Marshallers.StringMarshaller`                                                  | static   | UTF-8 string marshaller                 |
|  [05]   | `Method<TReq,TResp>(MethodType, string, string, Marshaller, Marshaller)`        | ctor     | method descriptor                       |
|  [06]   | `Method.Type` / `.FullName` / `.RequestMarshaller`                              | property | call shape, dispatch key, codecs        |
|  [07]   | `ServerServiceDefinition.CreateBuilder()`                                       | factory  | begin registration                      |
|  [08]   | `Builder.AddMethod(Method<TReq,TResp>, handler)`                                | instance | register handler; 4 streaming overloads |
|  [09]   | `Builder.Build()`                                                               | instance | immutable `ServerServiceDefinition`     |
|  [10]   | `ServerServiceDefinition.BindService(ServiceBinderBase)`                        | instance | replay registrations into a binder      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every server method is one `Method<TReq,TResp>` descriptor; its `FullName` (`serviceName/name`) keys dispatch and matches the generated proto service name exactly.
- `ServerServiceDefinition.Builder.AddMethod` registers a handler per method; `Build()` rejects a duplicate `FullName` with `ArgumentException`.
- `Marshaller<T>` carries both a byte-array codec and a contextual codec (`ContextualSerializer` / `ContextualDeserializer`); the contextual pair binds buffer pooling.
- A registered handler folds every failure onto the substrate fault carriers, so `Status.Detail` with trailing `Metadata` is the only peer-visible fault channel this boundary produces.

[STACKING]:
- `Grpc.Net.Client`(`libs/csharp/.api/api-grpc-client.md`): the fault and call-policy half of one rail — a handler's `Status`/`RpcException` mint and the `CallOptions.With*` threading a propagated outbound call takes both read their members there, so this boundary spells only the descriptors it registers.
- `Grpc.Core.Api`(`../../Rasm.Compute/.api/api-grpc-common.md`): the per-call `ServerCallContext`, stream reader/writer, and `Metadata` surface the same `ControlService` binds is owned there, including the `ContextPropagationToken` a parent context mints for a deadline-inheriting downstream call.
- `Rasm.AppHost` `ControlService`: builds its service as one `ServerServiceDefinition` of `Method<TReq,TResp>` descriptors over `Marshallers.Create` codec pairs.

[LOCAL_ADMISSION]:
- Contextual marshallers are the admitted codec form on this rail; the byte-array pair enters only where a payload has no pooled writer.

[RAIL_LAW]:
- Package: `Grpc.Core.Api`
- Owns: the gRPC method descriptor, the service-definition registry, and the marshaller pairs the AppHost control rail registers
- Accept: hand-registered `Method` descriptors and contextual marshallers
- Reject: a parallel error DTO beside `Status`; the fault and call-policy member surface `libs/csharp/.api/api-grpc-client.md` owns; the per-call context, streaming, and metadata surface the Compute `api-grpc-common.md` catalog owns
