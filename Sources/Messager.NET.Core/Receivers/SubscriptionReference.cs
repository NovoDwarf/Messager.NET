namespace Messager.NET.Receivers;

/// <summary>
/// Controls whether a subscription keeps callback targets alive.
/// </summary>
public enum SubscriptionReference
{
    /// <summary>
    /// Keeps callback targets alive until the subscription is removed.
    /// </summary>
    Strong,

    /// <summary>
    /// Allows callback targets to be collected even while the subscription handle is retained.
    /// Static callbacks remain callable until the subscription is removed.
    /// </summary>
    Weak
}
