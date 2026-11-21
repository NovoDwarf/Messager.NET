using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET;

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
	
	
	public ISet<Assembly> RequestAssemblies { get; } = new HashSet<Assembly>();

	public void AddRequestsFromAssemblyOf(Assembly assembly)
	{
		RequestAssemblies.Add(assembly);
	}
}
