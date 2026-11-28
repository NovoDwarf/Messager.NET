using Messager.NET.Requests;

namespace Messager.NET.PureDI.Models;

public sealed class StringLengthRequest : IRequest<int, string>
{
    public int Invoke(string input)
    {
        return input.Length;
    }
}
