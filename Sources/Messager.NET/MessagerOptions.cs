using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET;

/// <summary>Configures dependency injection registrations for Messager.NET.</summary>
public class MessagerOptions
{
	/// <summary>
	/// Enable/disable default logging
	/// </summary>
	public bool EnableLogging { get; set; } = false;

	/// <summary>
	/// Use global instance in DI container. Currently, instance initialized with <see cref="NullLoggerFactory.Instance"/>
	/// </summary>
	public bool UseGlobal { get; set; } = false;

	/// <summary>Gets the assemblies scanned for request handler registrations.</summary>
	public ISet<Assembly> RequestAssemblies { get; } = new HashSet<Assembly>();

	/// <summary>Adds an assembly to the request handler scan.</summary>
	/// <param name="assembly">The assembly to scan during registration.</param>
	public void AddRequestsFromAssemblyOf(Assembly assembly)
	{
		RequestAssemblies.Add(assembly);
	}
}