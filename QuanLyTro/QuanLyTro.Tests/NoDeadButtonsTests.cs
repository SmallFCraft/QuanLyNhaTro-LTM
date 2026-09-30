using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class NoDeadButtonsTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void EveryButton_EitherHasHandlerOrIsDisabled()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        var dead = new List<string>();
        foreach (Match m in Regex.Matches(html, "<button\\b[^>]*>"))
        {
            var tag = m.Value;
            if (tag.Contains("onclick") || tag.Contains("type=\"submit\"") || tag.Contains("disabled"))
                continue;
            dead.Add(Regex.Replace(tag, "\\s+", " "));
        }
        Assert.AreEqual(0, dead.Count,
            "Nút không có handler cũng không disabled:\n" + string.Join("\n", dead));
    }
}
