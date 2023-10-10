# Notes
The mono/wasi/runtime/driver.h seems to be out of sync with driver.c because the header defines methods that aren't implemented and the C file doesn't implement all of them or has a different signature.

Seems like we need to find replacements for:
- call_clr_request_handler
- mono_wasm_string_get_utf8 -> (Possibly?) mono_wasm_string_get_data
- mono_wasm_invoke_method

Had to remove the following methods because they're not defined anywhere anymore:
- dotnet_wasi_getbundledfile
- dotnet_wasi_registerbundledassemblies
