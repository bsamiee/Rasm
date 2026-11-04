cmake_minimum_required(VERSION 3.24)

# --- [COMPOSITION] ---------------------------------------------------------------------

project(sql LANGUAGES C CXX)

execute_process(COMMAND tree-sitter generate --output "${CMAKE_CURRENT_BINARY_DIR}/src" "${CMAKE_CURRENT_SOURCE_DIR}/grammar.js" COMMAND_ERROR_IS_FATAL ANY)

file(REAL_PATH "${NODE_MODULES_DIR}/tree-sitter-sqlite3" sqlite_source_dir)
file(REAL_PATH "${NODE_MODULES_DIR}/tree-sitter-typescript" tsx_source_dir)
set(sqlite_grammar_path "${sqlite_source_dir}/src/grammar.json")
set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/grammar.js" "${CMAKE_CURRENT_SOURCE_DIR}/../typescript/grammar.js" "${sqlite_grammar_path}")
file(READ "${sqlite_grammar_path}" sqlite_grammar)
string(JSON sqlite_external_count LENGTH "${sqlite_grammar}" externals)
file(GENERATE OUTPUT "${CMAKE_CURRENT_BINARY_DIR}/scanner-offset.h" CONTENT "#define SQL_EXTERNAL_COUNT ${sqlite_external_count}\n")

add_library(${PROJECT_NAME} MODULE "${CMAKE_CURRENT_BINARY_DIR}/src/parser.c" scanner.cc)
target_include_directories(${PROJECT_NAME} PRIVATE "${CMAKE_CURRENT_BINARY_DIR}" "${CMAKE_CURRENT_BINARY_DIR}/src" "$<PATH:GET_PARENT_PATH,${sqlite_source_dir}>" "$<PATH:GET_PARENT_PATH,${tsx_source_dir}>")
target_compile_features(${PROJECT_NAME} PRIVATE c_std_11 cxx_std_11)
set(output_dir "$<PATH:GET_PARENT_PATH,${CMAKE_CURRENT_BINARY_DIR}>")
set_target_properties(${PROJECT_NAME} PROPERTIES PREFIX "" SUFFIX ".so" LIBRARY_OUTPUT_DIRECTORY "${output_dir}" RUNTIME_OUTPUT_DIRECTORY "${output_dir}")
