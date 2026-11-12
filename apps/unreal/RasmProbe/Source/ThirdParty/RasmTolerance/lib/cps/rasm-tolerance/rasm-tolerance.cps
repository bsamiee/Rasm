{
  "compat_version" : "0.1.0",
  "components" : 
  {
    "tolerance" : 
    {
      "compile_features" : [ "c++26" ],
      "file_sets" : 
      [
        {
          "extensions" : 
          {
            "cmake" : 
            {
              "name@v1" : "HEADERS"
            }
          },
          "files" : [ "rasm/tolerance.hpp" ],
          "root" : "@prefix@/include",
          "type" : "includes"
        }
      ],
      "includes" : [ "@prefix@/include" ],
      "link_requires" : [ "fmt:fmt" ],
      "type" : "archive"
    }
  },
  "cps_path" : "@prefix@/lib/cps/rasm-tolerance",
  "cps_version" : "0.15.0",
  "description" : "Floating-point comparison by units in the last place",
  "license" : "MIT",
  "name" : "rasm-tolerance",
  "requires" : 
  {
    "fmt" : 
    {
      "components" : [ "fmt" ],
      "hints" : [ "/Users/bardiasamiee/Developer/Rasm/.pixi/envs/cpp/lib/cmake/fmt" ],
      "version" : "12.2.1"
    }
  },
  "version" : "0.1.0",
  "website" : "https://github.com/bsamiee/Rasm"
}