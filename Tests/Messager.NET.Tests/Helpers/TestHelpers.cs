namespace Messager.NET.Tests.Helpers;

internal static class TestHelpers
{
    internal static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    internal static TaskCompletionSource NewSignal()
    {
        return new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    internal static void CollectGarbage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
