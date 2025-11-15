using Messager.NET.Models.Resources;
using Messager.NET.Tests.Objects;
using Messager.NET.Tests.Objects.Resources;

namespace Messager.NET.Tests.Models.Resources;

public partial class DisposableListTests
{
	[Test]
	public void Add_SingleDisposable_ShouldAddToList()
	{
		// Arrange
		var disposableList = new DisposableList();
		var disposable = new TestDisposable();

		// Act
		disposableList.Add(disposable);

		// Assert
		Assert.That(disposableList.Count, Is.EqualTo(1));
	}

	[Test]
	public void Add_MultipleDisposables_ShouldAddAllToList()
	{
		var disposableList = new DisposableList();
		IDisposable[] disposables = [new TestDisposable(), new TestDisposable(), new TestDisposable()];

		disposableList.Add(disposables);

		Assert.That(disposableList.Count, Is.EqualTo(3));
	}

	[Test]
	public void Add_NullArray_ShouldThrowArgumentNullException()
	{
		var disposableList = new DisposableList();

		Assert.Throws<ArgumentNullException>(() => disposableList.Add(null!));
	}

	[Test]
	public void Add_NullDisposableInArray_ShouldThrowArgumentNullException()
	{
		var disposableList = new DisposableList();
		var disposables = new IDisposable[] { new TestDisposable(), null!, new TestDisposable() };

		Assert.Throws<ArgumentNullException>(() => disposableList.Add(disposables));
	}

	[Test]
	public void Add_AfterDispose_ShouldThrowObjectDisposedException()
	{
		var disposableList = new DisposableList();
		disposableList.Dispose();

		Assert.Throws<ObjectDisposedException>(() => disposableList.Add(new TestDisposable()));
	}
}