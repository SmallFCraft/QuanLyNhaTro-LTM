namespace QuanLyTro.Server.Services;

/// <summary>
/// Vi phạm quy tắc nghiệp vụ (BR-01..BR-14). Message là câu tiếng Việt hiển thị thẳng
/// cho người dùng — không lộ chi tiết kỹ thuật.
/// </summary>
public sealed class BusinessRuleException(string message) : Exception(message);
