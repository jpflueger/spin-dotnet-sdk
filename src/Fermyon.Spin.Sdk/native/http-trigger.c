#include <stdio.h>
#include <string.h>
#include <driver.h>

#include <spin-http.h>
#include <http-trigger-lookup.h>
#include <host-components.h>

int initialized = 0;

// This symbol's implementation is generated during the build
const char* dotnet_wasi_getentrypointassemblyname();

void _start();
void mono_wasm_invoke_method_ref(MonoMethod* method, MonoObject** this_arg_in, void* params[], MonoObject** _out_exc, MonoObject** out_result);

void ensure_initialized() {
    printf("ensure_initialized\n");
    if (!initialized) {
        _start();
        spin_attach_internal_calls();
        initialized = 1;
    }
}

spin_http_response_t internal_error(const char* message) {
    printf("internal_error\n");
    spin_http_response_t response;
    response.status = 500;
    response.headers.is_some = false;
    response.body.is_some = true;
    response.body.val.ptr = (uint8_t*)message;
    response.body.val.len = strlen(message);
    return response;
}

void spin_http_handle_http_request(spin_http_request_t *req, spin_http_response_t *ret0) {
    printf("spin_http_handle_http_request\n");
    ensure_initialized();

    printf("spin_http_handle_http_request: looking up method\n");
    MonoMethod* method = lookup_http_trigger_method();
    void* method_params[] = { req, ret0 };
    MonoObject* exception;
    MonoObject* result;

    printf("spin_http_handle_http_request: invoking method\n");
    mono_wasm_invoke_method_ref(method, NULL, method_params, &exception, &result);

    printf("spin_http_handle_http_request: status: %d\n", ret0->status);

    if (exception) {
        printf("spin_http_handle_http_request: exception\n");
        char* exception_string_utf8 = mono_string_to_utf8((MonoString*)result);
        *ret0 = internal_error(exception_string_utf8);
    }
}

__attribute__((export_name("wizer.initialize")))
void preinitialize() {
    printf("preinitialize\n");
    ensure_initialized();

    // To warm the interpreter, we need to run the main code path that is going to execute per-request. That way the preinitialized
    // binary is already ready to go at full speed.

    printf("preinitialize: looking up method\n");
    MonoMethod* method = lookup_http_trigger_method();

    printf("preinitialize: creating request\n");
    char* warmup_url = get_warmup_url();
    int warmup_url_len = strlen(warmup_url);

    // supply fake headers that would usually originate from the http trigger
    // we can't introspect on our own component config so we just make up some values
    char* fake_host = "127.0.0.1:3000";
    int fake_host_len = strlen(fake_host);
    char* warmup_url_full;
    int warmup_url_full_len = asprintf(&warmup_url_full, "http://%s%s", fake_host, warmup_url);
    spin_http_headers_t fake_headers = {.len = 10, .ptr = (spin_http_tuple2_string_string_t[]){
            {{"host", 4}, {fake_host, fake_host_len}},
            {{"user-agent", 10}, {"wizer", 5}},
            {{"accept", 6}, {"*/*", 3}},
            {{"spin-full-url", 13}, {warmup_url_full, warmup_url_full_len}},
            {{"spin-path-info", 14}, {warmup_url, warmup_url_len}},
            {{"spin-matched-route", 18}, {"/...", 3}},
            {{"spin-raw-component-route", 24}, {"/...", 3}},
            {{"spin-component-route", 20}, {"", 0}},
            {{"spin-base-path", 14}, {"/", 1}},
            {{"spin-client-addr", 14}, {fake_host, fake_host_len}}}};

    spin_http_request_t fake_req = {
        .method = SPIN_HTTP_METHOD_GET,
        .uri = { warmup_url, warmup_url_len },
        .headers = fake_headers,
        .body = { .is_some = 1, .val = { (void*)"Hello", 5 } }
    };
    spin_http_response_t fake_res;

    printf("preinitialize: invoking method\n");
    spin_http_handle_http_request(&fake_req, &fake_res);
}