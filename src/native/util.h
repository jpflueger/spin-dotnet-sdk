#ifndef __UTIL_H
#define __UTIL_H

#include <mono/metadata/object.h>

typedef uint8_t entry_points_err_t;

#define EP_ERR_OK 0
#define EP_ERR_NO_ENTRY_ASSEMBLY 1
#define EP_ERR_NO_HANDLER_METHOD 2

entry_points_err_t find_entry_points(const char* attr_name, MonoObject** attr_obj, MonoMethod** handler);

#endif
