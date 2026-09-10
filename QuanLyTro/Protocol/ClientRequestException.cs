namespace QuanLyTro.Protocol;

/// <summary>
/// Server trả <c>Success = false</c>. <see cref="Exception.Message"/> là câu tiếng Việt
/// từ <c>BusinessRuleException</c> phía server, hiển thị THẲNG cho người dùng.
/// </summary>
public sealed class ClientRequestException : Exception
{
    public ClientRequestException(string message) : base(message) { }
}
