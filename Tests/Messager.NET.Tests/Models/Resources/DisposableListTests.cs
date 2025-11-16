using Messager.NET.Models.Resources;
using Messager.NET.Tests.Objects;
using Messager.NET.Tests.Objects.Resources;

namespace Messager.NET.Tests.Models.Resources;

using NUnit.Framework;

[TestFixture]
public partial class DisposableListTests
{
    [Test]
    public void Constructor_ShouldCreateEmptyList()
    {
        var disposableList = new DisposableList();

        Assert.That(disposableList.Count, Is.Zero);
        Assert.That(disposableList.IsDisposed, Is.False);
    }
    
    [Test]
    public void Count_ShouldReturnCorrectNumber()
    {
        var disposableList = new DisposableList();
        
        Assert.That(disposableList.Count, Is.Zero);
        
        disposableList.Add(new TestDisposable());
        Assert.That(disposableList.Count, Is.EqualTo(1));
        
        disposableList.Add(new TestDisposable(), new TestDisposable());
        Assert.That(disposableList.Count, Is.EqualTo(3));
    }

    [Test]
    public void IsDisposed_ShouldReflectDisposalState()
    {
        var disposableList = new DisposableList();
        
        Assert.That(disposableList.IsDisposed, Is.False);
        
        disposableList.Dispose();
        Assert.That(disposableList.IsDisposed, Is.True);
    }
}