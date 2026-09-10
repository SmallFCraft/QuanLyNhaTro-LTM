using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Forms;

namespace QuanLyTro.Tests;

/// <summary>
/// Kiểm tra escape CSV tạm trú (US-08) — Task 11 nhóm 3.
/// Quy tắc: bọc field trong "" và nhân đôi mọi " bên trong (RFC 4180).
/// </summary>
[TestClass]
public sealed class ReportCsvTests
{
    [TestMethod]
    public void Csv_WrapsPlainValueInQuotes()
    {
        Assert.AreEqual("\"Trần Văn B\"", ReportsForm.Csv("Trần Văn B"));
    }

    [TestMethod]
    public void Csv_DoublesEmbeddedQuote()
    {
        Assert.AreEqual("\"Nguyễn \"\"Tí\"\"\"", ReportsForm.Csv("Nguyễn \"Tí\""));
    }

    [TestMethod]
    public void Csv_NullBecomesEmptyQuotedField()
    {
        Assert.AreEqual("\"\"", ReportsForm.Csv(null));
    }

    [TestMethod]
    public void Initials_TakesFirstAndLastWord()
    {
        Assert.AreEqual("TB", MyInvoicesForm.Initials("Trần Văn B"));
        Assert.AreEqual("A", MyInvoicesForm.Initials("An"));
        Assert.AreEqual("?", MyInvoicesForm.Initials("   "));
    }
}
