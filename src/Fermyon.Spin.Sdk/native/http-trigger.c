#include <spin-http.h>
#include <stdio.h>
#include <string.h>
#include <driver.h>

int dotnet_started = 0;
void _start();
void mono_wasm_invoke_method_ref(MonoMethod* method, MonoObject** this_arg_in, void* params[], MonoObject** _out_exc, MonoObject** out_result);

void ensure_dotnet_started() {
    if (!dotnet_started) {
        _start();
        dotnet_started = 1;
    }
}

spin_http_response_t internal_error(const char* message) {
    spin_http_response_t response;
    response.status = 500;
    response.headers.is_some = false;
    response.body.is_some = true;
    response.body.val.ptr = (uint8_t*)message;
    response.body.val.len = strlen(message);
    return response;
}

void spin_http_handle_http_request(spin_http_request_t *req, spin_http_response_t *ret0) {
    ensure_dotnet_started();

    //TODO: fix this to use the correct method
    MonoMethod* method = lookup_dotnet_method("SpiderLightning", "SpiderLightning", "HttpServer", "Export_HandleIncomingRequest", -1);
    void* method_params[] = { req, ret0 };
    MonoObject* exception;
    MonoObject* result;    
    mono_wasm_invoke_method_ref(method, NULL, method_params, &exception, &result);

    if (exception) {
        char* exception_string_utf8 = mono_string_to_utf8((MonoString*)result);
        *ret0 = internal_error(exception_string_utf8);
    }
}
