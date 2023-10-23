#ifndef __HTTP_TRIGGER_LOOKUP_H
#define __HTTP_TRIGGER_LOOKUP_H

#include <mono/metadata/object.h>

// the implementations are generated at Build time
// see: src/Fermyon.Spin.MSBuild/GenerateHttpTriggerLookupTask.cs

MonoMethod* lookup_http_trigger_method();

char *get_warmup_url();

#endif
