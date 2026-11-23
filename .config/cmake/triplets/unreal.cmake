# --- [TARGET] ---------------------------------------------------------------------------

include("${CMAKE_CURRENT_LIST_DIR}/llvm.cmake")
if(CMAKE_HOST_APPLE)
    set(VCPKG_OSX_ARCHITECTURES "arm64;x86_64")
else()
    set(VCPKG_TARGET_ARCHITECTURE x64)
endif()

# --- [TOOLCHAIN] ------------------------------------------------------------------------

set(VCPKG_ENV_PASSTHROUGH UNREAL_ENGINE_ROOT)
set(VCPKG_HASH_ADDITIONAL_FILES "${CMAKE_CURRENT_LIST_DIR}/llvm.cmake")
set(VCPKG_CHAINLOAD_TOOLCHAIN_FILE "${CMAKE_CURRENT_LIST_DIR}/../toolchains/unreal.cmake")
