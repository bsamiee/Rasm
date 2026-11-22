# --- [ENGINE] ---------------------------------------------------------------------------

file(READ "$ENV{UNREAL_ENGINE_ROOT}/Engine/Config/Linux/Linux_SDK.json" sdk)
string(JSON release GET "${sdk}" MainVersion)
set(CMAKE_SYSROOT "$ENV{UNREAL_ENGINE_ROOT}/Engine/Extras/ThirdPartyNotUE/SDKs/HostLinux/Linux_x64/${release}/x86_64-unknown-linux-gnu")

# --- [COMPILERS] ------------------------------------------------------------------------

set(CMAKE_C_COMPILER "${CMAKE_SYSROOT}/bin/clang")
set(CMAKE_CXX_COMPILER "${CMAKE_SYSROOT}/bin/clang++")
set(CMAKE_C_COMPILER_TARGET x86_64-unknown-linux-gnu)
set(CMAKE_CXX_COMPILER_TARGET x86_64-unknown-linux-gnu)
set(CMAKE_AR "${CMAKE_SYSROOT}/bin/llvm-ar")
set(CMAKE_RANLIB "${CMAKE_SYSROOT}/bin/llvm-ranlib")
set(CMAKE_TRY_COMPILE_TARGET_TYPE STATIC_LIBRARY)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)
set(CMAKE_C_FLAGS_INIT "-fvisibility-ms-compat")
set(CMAKE_CXX_FLAGS_INIT "${CMAKE_C_FLAGS_INIT} -fvisibility-inlines-hidden -fno-rtti -nostdinc++ -isystem ${CMAKE_SYSROOT}/include -isystem ${CMAKE_SYSROOT}/include/c++/v1")
