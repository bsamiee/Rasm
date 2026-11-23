# --- [FLAGS] ----------------------------------------------------------------------------

set(CMAKE_C_FLAGS_INIT "-fvisibility-ms-compat")
set(CMAKE_CXX_FLAGS_INIT "${CMAKE_C_FLAGS_INIT} -fvisibility-inlines-hidden -fno-rtti")

# --- [ENGINE] ---------------------------------------------------------------------------

if(CMAKE_HOST_APPLE)
    file(READ "$ENV{UNREAL_ENGINE_ROOT}/Engine/Config/Mac/Mac_SDK.json" sdk)
    string(JSON CMAKE_OSX_DEPLOYMENT_TARGET GET "${sdk}" DeploymentTarget)
    set(CMAKE_OSX_ARCHITECTURES "arm64;x86_64")
    set(CMAKE_C_COMPILER /usr/bin/clang)
    set(CMAKE_CXX_COMPILER /usr/bin/clang++)
    set(CMAKE_OBJCXX_COMPILER "${CMAKE_CXX_COMPILER}")
    set(CMAKE_OBJCXX_FLAGS_INIT "${CMAKE_CXX_FLAGS_INIT}")
else()
    file(READ "$ENV{UNREAL_ENGINE_ROOT}/Engine/Config/Linux/Linux_SDK.json" sdk)
    string(JSON release GET "${sdk}" MainVersion)
    set(CMAKE_SYSROOT "$ENV{UNREAL_ENGINE_ROOT}/Engine/Extras/ThirdPartyNotUE/SDKs/HostLinux/Linux_x64/${release}/x86_64-unknown-linux-gnu")
    set(CMAKE_C_COMPILER "${CMAKE_SYSROOT}/bin/clang")
    set(CMAKE_CXX_COMPILER "${CMAKE_SYSROOT}/bin/clang++")
    set(CMAKE_C_COMPILER_TARGET x86_64-unknown-linux-gnu)
    set(CMAKE_CXX_COMPILER_TARGET x86_64-unknown-linux-gnu)
    set(CMAKE_AR "${CMAKE_SYSROOT}/bin/llvm-ar")
    set(CMAKE_RANLIB "${CMAKE_SYSROOT}/bin/llvm-ranlib")
    set(CMAKE_TRY_COMPILE_TARGET_TYPE STATIC_LIBRARY)
    set(CMAKE_POSITION_INDEPENDENT_CODE ON)
    string(APPEND CMAKE_CXX_FLAGS_INIT " -nostdinc++ -isystem ${CMAKE_SYSROOT}/include -isystem ${CMAKE_SYSROOT}/include/c++/v1")
endif()
