# --- [TARGET] ---------------------------------------------------------------------------

include("${CMAKE_CURRENT_LIST_DIR}/llvm.cmake")
set(VCPKG_OSX_ARCHITECTURES "arm64;x86_64")

# --- [TOOLCHAIN] ------------------------------------------------------------------------

set(VCPKG_ENV_PASSTHROUGH "UNREAL_ENGINE_ROOT;HOME")
set(VCPKG_CHAINLOAD_TOOLCHAIN_FILE "${CMAKE_CURRENT_LIST_DIR}/../toolchains/unreal-${CMAKE_HOST_SYSTEM_NAME}.cmake")
