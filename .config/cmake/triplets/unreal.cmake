# --- [TARGET] ---------------------------------------------------------------------------

if(CMAKE_HOST_APPLE)
    set(VCPKG_TARGET_ARCHITECTURE arm64)
    set(VCPKG_OSX_ARCHITECTURES "arm64;x86_64")
else()
    set(VCPKG_TARGET_ARCHITECTURE x64)
endif()
set(VCPKG_CMAKE_SYSTEM_NAME "${CMAKE_HOST_SYSTEM_NAME}")
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)
set(VCPKG_BUILD_TYPE release)

# --- [TOOLCHAIN] ------------------------------------------------------------------------

set(VCPKG_ENV_PASSTHROUGH UNREAL_ENGINE_ROOT)
set(VCPKG_CHAINLOAD_TOOLCHAIN_FILE "${CMAKE_CURRENT_LIST_DIR}/../toolchains/unreal-${CMAKE_HOST_SYSTEM_NAME}.cmake")
