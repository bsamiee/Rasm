cmake_minimum_required(VERSION 4.5)

project(rasm LANGUAGES CXX)

# --- [PACKAGING] ------------------------------------------------------------------------

set(CMAKE_PROJECT_INCLUDE "${CMAKE_CURRENT_SOURCE_DIR}/.config/cpack.cmake")

# --- [MEMBERS] --------------------------------------------------------------------------

enable_testing()
file(
    GLOB manifests
    CONFIGURE_DEPENDS
    RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}"
    "apps/*/*/CMakeLists.txt"
    "libs/cpp/*/*/CMakeLists.txt"
    "tests/cpp/apps/*/*/CMakeLists.txt"
    "tests/cpp/libs/*/*/CMakeLists.txt"
)
foreach(manifest IN LISTS manifests)
    cmake_path(GET manifest PARENT_PATH member)
    add_subdirectory("${member}")
endforeach()
