using Mono.Cecil;

namespace Fermyon.Spin.MSBuild;

public static class HttpTriggerReflection
{
    public static MethodDefinition GetHttpTriggerMethod(AssemblyDefinition assembly)
    {
        var httpHandlerMethods = assembly.MainModule.Types
            .SelectMany(x => x.Methods)
            .Where(m => m.IsStatic && m.CustomAttributes.Any(a => a.AttributeType.FullName == "Fermyon.Spin.Sdk.HttpHandlerAttribute"))
            .ToArray();

        if (!httpHandlerMethods.Any())
        {
            throw new InvalidOperationException($"No HttpHandler methods found in assembly {assembly.FullName}");
        }

        if (httpHandlerMethods.Length > 1)
        {
            throw new InvalidOperationException($"Only 1 HttpHandler method is allowed in {assembly.FullName}");
        }

        var httpHandlerMethod = httpHandlerMethods[0];

        if (httpHandlerMethod.Parameters.Count != 1)
        {
            throw new InvalidOperationException($"HttpHandler methods must have exactly 1 parameter in {assembly.FullName}");
        }

        if (httpHandlerMethod.Parameters[0].ParameterType.FullName != "Fermyon.Spin.Sdk.HttpRequest")
        {
            throw new InvalidOperationException($"HttpHandler methods must have exactly 1 parameter of type HttpRequest in {assembly.FullName}");
        }

        return httpHandlerMethod;
    }

    public static CustomAttribute GetHttpTriggerAttribute(MethodDefinition httpHandlerMethod)
    {
        // TODO: figure out how to reference the HttpHandlerAttribute assembly from here
        var httpHandlerAttr = httpHandlerMethod.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == "Fermyon.Spin.Sdk.HttpHandlerAttribute")
            ?? throw new InvalidOperationException("HttpHandlerAttribute not found");

        return httpHandlerAttr;
    }
}