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
        var htmlFiles = Directory.GetFiles(Wwwroot, "*.html", SearchOption.AllDirectories);
        Assert.IsTrue(htmlFiles.Length >= 4, "Phải quét ít nhất 4 trang (auth, landlord, police, tenant).");

        var dead = new List<string>();
        foreach (var file in htmlFiles)
        {
            var html = File.ReadAllText(file);
            var relative = Path.GetRelativePath(Wwwroot, file);
            foreach (Match m in Regex.Matches(html, "<button\\b[^>]*>"))
            {
                var tag = m.Value;
                if (tag.Contains("onclick") || tag.Contains("type=\"submit\"") || tag.Contains("disabled"))
                    continue;
                dead.Add($"{relative}: {Regex.Replace(tag, "\\s+", " ")}");
            }
        }
        Assert.AreEqual(0, dead.Count,
            "Nút không có handler cũng không disabled:\n" + string.Join("\n", dead));
    }
}
