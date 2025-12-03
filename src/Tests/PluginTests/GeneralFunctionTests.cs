using System.Diagnostics;
using System.Text;
using mjbot.Plugins;

namespace PluginTests;

public class GeneralFunctionTests
{
    public GeneralFunctionTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Fact]
    public async Task TestGoose_InputOutput()
    {
        const string input = "以一星期为一期";

        var crocodile = new Crocodile();
        var response = crocodile.Goose(input);
        Debug.Assert(response?.Message != null, "result.Message != null");
        var result = await response.Message.EncodeAsync();

        Assert.Equal(result, "笆办辣袋百办袋");
    }
}