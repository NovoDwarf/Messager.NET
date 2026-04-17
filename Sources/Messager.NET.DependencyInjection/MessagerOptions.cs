using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Messager.NET.DependencyInjection;

public class MessagerOptions
{
	public bool EnableLogging { get; set; } = true;
	public LogLevel LogLevel { get; set; } = LogLevel.Error;
	public ISet<Assembly> RequestAssemblies { get; } = new HashSet<Assembly>();
}
