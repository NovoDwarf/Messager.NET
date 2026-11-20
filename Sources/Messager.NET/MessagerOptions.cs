using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Messager.NET;

public class MessagerOptions
{
	public bool EnableLogging { get; set; } = true;
	
	public ISet<Assembly> RequestAssemblies { get; } = new HashSet<Assembly>();

	public void AddRequestsFromAssemblyOf(Assembly assembly)
	{
		RequestAssemblies.Add(assembly);
	}
}
