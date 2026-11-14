# --- [ENGINE] ---------------------------------------------------------------------------

file(READ "$ENV{UNREAL_ENGINE_ROOT}/Engine/Config/Mac/Mac_SDK.json" sdk)
string(JSON CMAKE_OSX_DEPLOYMENT_TARGET GET "${sdk}" DeploymentTarget)
set(CMAKE_OSX_ARCHITECTURES "arm64;x86_64")

# --- [COMPILERS] ------------------------------------------------------------------------

set(CMAKE_C_COMPILER /usr/bin/clang)
set(CMAKE_CXX_COMPILER /usr/bin/clang++)
set(CMAKE_OBJCXX_COMPILER /usr/bin/clang++)
set(CMAKE_C_FLAGS_INIT "-fvisibility-ms-compat")
set(CMAKE_CXX_FLAGS_INIT "-fvisibility-ms-compat -fvisibility-inlines-hidden -fno-rtti")
set(CMAKE_OBJCXX_FLAGS_INIT "-fvisibility-ms-compat -fvisibility-inlines-hidden -fno-rtti")
