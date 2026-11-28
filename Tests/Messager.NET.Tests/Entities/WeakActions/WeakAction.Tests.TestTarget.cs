namespace Messager.NET.Tests.Entities.WeakActions;

public sealed partial class WeakAction_Tests
{
    private sealed class WeakActionTestTarget
    {
        public string LastValue { get; private set; } = string.Empty;

        public void InstanceMethod(string value)
        {
            LastValue = value;
        }

        public void AnotherMethod(string value)
        {
            LastValue = value + "_modified";
        }
    }
}
