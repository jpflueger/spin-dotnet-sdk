using Microsoft.Build.Framework;
using Mono.Cecil;

namespace Fermyon.Spin.MSBuild;

public class GenerateHttpTriggerLookupTask : Microsoft.Build.Utilities.Task
{
    [Required]
    public string? AssemblyPath { get; set; }
    
    [Required]
    public string? OutputPath { get; set; }

    public override bool Execute()
    {
        try
        {
            if (AssemblyPath is null)
            {
                throw new ArgumentNullException(nameof(AssemblyPath));
            }

            if (OutputPath is null)
            {
                throw new ArgumentNullException(nameof(OutputPath));
            }

            var assemblyFileName = Path.GetFileName(AssemblyPath);
            var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath);

            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }
            else
            {
                foreach (var file in Directory.GetFiles(OutputPath, "*.c"))
                {
                    File.Delete(file);
                }
            }

            var httpHandlerMethod = HttpTriggerReflection.GetHttpTriggerMethod(assembly);
            var httpHandlerAttr = HttpTriggerReflection.GetHttpTriggerAttribute(httpHandlerMethod);

            var warmupUrl = httpHandlerAttr
                .Properties.SingleOrDefault(p => p.Name == "WarmupUrl")
                .Argument.Value as string;
            if (string.IsNullOrEmpty(warmupUrl))
            {
                warmupUrl = "/warmupz";
            }


            var assemblyName = assembly.Name.Name;
            var dtNamespace = httpHandlerMethod.DeclaringType.Namespace;
            var dtName = httpHandlerMethod.DeclaringType.Name;
            var methodName = httpHandlerMethod.Name;

            var httpTriggerLookupC = @$"
#include <mono/metadata/object.h>
#include <driver.h>
#include ""http-trigger-lookup.h""

MonoMethod* lookup_http_trigger_method()
{{
    MonoMethod* method = lookup_dotnet_method(""{assemblyName}"", ""{dtNamespace}"", ""{dtName}"", ""{methodName}"", -1);
    return method;
}}

char* get_warmup_url()
{{
    return ""{warmupUrl}"";
}}
";
            File.WriteAllText(Path.Combine(OutputPath, "http-trigger-lookup.c"), httpTriggerLookupC);

            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }
    }
}