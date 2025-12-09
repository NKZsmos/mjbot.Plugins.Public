/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * Copyright (c) 2025 BJMANIA
 */

using System.Diagnostics;
using System.Text;
using MilkiBotFramework.Messaging;
using MilkiBotFramework.Plugining.CommandLine;
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

        var crocodile = new Crocodile(new DummySensitiveScanService());
        var messageContext = new MessageContext(new DefaultRichMessageConverter())
        {
            TextMessage = input
        };
        var commandLineResult = new CommandLineResult(CommandLineAuthority.Public, null, [], [], input.AsMemory());
        typeof(MessageContext)
            .GetProperty(nameof(MessageContext.CommandLineResult))!
            .SetValue(messageContext, commandLineResult);

        var response = await crocodile.Goose(messageContext);
        Debug.Assert(response?.Message != null, "result.Message != null");
        var result = await response.Message.EncodeAsync();

        Assert.Equal(result, "笆办辣袋百办袋");
    }
}