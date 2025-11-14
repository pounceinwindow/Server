using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace MiniTemplateEngine.UnitTests;

[TestClass]
public class HtmlTemplateRendererTests
{
    private readonly HtmlTemplateRenderer _r = new();

    private object MakeUser(bool isActive = true)
    {
        return new
        {
            Name = "Alice",
            IsActive = isActive,
            Items = new[] { new { Name = "Item 1" }, new { Name = "Item 2" } },
            Profile = new { City = "Paris" }
        };
    }

    [TestMethod]
    public void Var_Simple()
    {
        var t = "Hello, ${Name}!";
        var res = _r.RenderFromString(t, new { Name = "Bob" });
        Assert.AreEqual("Hello, Bob!", res);
    }

    [TestMethod]
    public void Var_DottedPath()
    {
        var t = "City: ${Profile.City}";
        var res = _r.RenderFromString(t, MakeUser());
        Assert.AreEqual("City: Paris", res);
    }

    [TestMethod]
    public void If_True_Branch()
    {
        var t = "$if(IsActive)YES$elseNO$endif";
        var res = _r.RenderFromString(t, MakeUser());
        Assert.AreEqual("YES", res);
    }

    [TestMethod]
    public void If_False_Branch()
    {
        var t = "$if(IsActive)YES$elseNO$endif";
        var res = _r.RenderFromString(t, MakeUser(false));
        Assert.AreEqual("NO", res);
    }

    [TestMethod]
    public void If_Equality()
    {
        var t = "$if(Profile.City == 'Paris')OK$elseBAD$endif";
        var res = _r.RenderFromString(t, MakeUser());
        Assert.AreEqual("OK", res);
    }

    [TestMethod]
    public void Foreach_List_Items()
    {
        var t = "$foreach(var item in Items)<li>${item.Name}</li>$endfor";
        var res = _r.RenderFromString(t, MakeUser());
        Assert.AreEqual("<li>Item 1</li><li>Item 2</li>", res);
    }

    [TestMethod]
    public void Foreach_ShortHeaderWithoutVar()
    {
        var t = "$foreach(item in Items)[${item.Name}]$endfor";
        var res = _r.RenderFromString(t, MakeUser());
        Assert.AreEqual("[Item 1][Item 2]", res);
    }

    [TestMethod]
    public void Nested_If_Inside_Foreach()
    {
        var model = new
        {
            Items = new[]
            {
                new { Name = "A", Flag = true },
                new { Name = "B", Flag = false }
            }
        };
        var t = "$foreach(var it in Items)$if(it.Flag)+${it.Name}$else-${it.Name}$endif$endfor";
        var res = _r.RenderFromString(t, model);
        Assert.AreEqual("+A-B", res);
    }

    [TestMethod]
    public void Nested_Foreach_Inside_If()
    {
        var model = new { Ok = true, Items = new[] { new { Name = "X" }, new { Name = "Y" } } };
        var t = "$if(Ok)$foreach(var i in Items)${i.Name}$endfor$elseNONE$endif";
        var res = _r.RenderFromString(t, model);
        Assert.AreEqual("XY", res);
    }

    [TestMethod]
    public void This_Within_Foreach()
    {
        var model = new { Items = new[] { new { Name = "A" } } };
        var t = "$foreach(var row in Items)${this.Name}$endfor";
        var res = _r.RenderFromString(t, model);
        Assert.AreEqual("A", res);
    }

    [TestMethod]
    public void RenderFromFile_And_ToFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "MiniTplTests");
        Directory.CreateDirectory(dir);
        var inPath = Path.Combine(dir, "t.html");
        var outPath = Path.Combine(dir, "r.html");

        File.WriteAllText(inPath, "<p>${Name}</p>");
        var res = _r.RenderToFile(inPath, outPath, new { Name = "Zed" });

        Assert.AreEqual("<p>Zed</p>", res);
        Assert.IsTrue(File.Exists(outPath));
        Assert.AreEqual("<p>Zed</p>", File.ReadAllText(outPath));
    }

    [TestMethod]
    public void Unknown_Var_Is_Empty()
    {
        var t = "A=${Nope}";
        var res = _r.RenderFromString(t, new { A = 1 });
        Assert.AreEqual("A=", res);
    }

    [TestMethod]
    public void Empty_Collection_In_Foreach_Is_NoOutput()
    {
        var model = new { Items = Array.Empty<object>() };
        var t = "$foreach(var x in Items)X$endfor";
        var res = _r.RenderFromString(t, model);
        Assert.AreEqual(string.Empty, res);
    }
}