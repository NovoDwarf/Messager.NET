using Messager.NET.Exceptions;

namespace Messager.NET.Utilities;

internal static class RegistryHelper
{
	public static TBroker GetOrCreate<TKey, TBroker>(Dictionary<TKey, object> dictionary, TKey key, Func<TBroker> factory)
		where TKey : notnull
		where TBroker : class
	{
		ArgumentNullException.ThrowIfNull(dictionary);
		ArgumentNullException.ThrowIfNull(factory);

		if (dictionary.TryGetValue(key, out var existing))
			return existing as TBroker ?? throw new BrokerRegistryTypeMismatchException(key, typeof(TBroker), typeof(TKey));

		var broker = factory();
		dictionary.Add(key, broker);

		return broker;
	}
}