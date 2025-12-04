/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * Copyright (c) 2025 BJMANIA
 */

using System.Buffers;
using System.Text;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using MilkiBotFramework.Messaging;
using MilkiBotFramework.Plugining;
using MilkiBotFramework.Plugining.Attributes;
using MilkiBotFramework.Services;

namespace mjbot.Plugins;

[PluginIdentifier("CD18B6F0-A703-7A7D-98DA-1BCBE5585890", Index = 1, Authors = "isaax", Scope = "mjbot")]
public class Crocodile(ISensitiveScanService sensitiveScanService) : BasicPlugin
{
    private static readonly Encoding EucJp = Encoding.GetEncoding(51932);
    private static readonly Encoding Gbk = Encoding.GetEncoding(936);

    private static readonly SearchValues<char> ProtectedChars = SearchValues.Create([
        '，', '《', '》', '！', '？', '“', '”', '：', '；',
        '‘', '’', '【', '】', '…', '（', '）'
    ]);

    [CommandHandler("goose")]
    public async Task<IResponse?> Goose([Argument] string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        string? traditional = ChineseConverter.Convert(content, ChineseConversionDirection.SimplifiedToTraditional);
        byte[] bytes = EucJp.GetBytes(traditional);
        string result = Gbk.GetString(bytes);

        StringBuilder sb = new(result.Length);

        int length = Math.Min(result.Length, content.Length);

        for (int i = 0; i < length; i++)
        {
            char originalChar = content[i];
            char glitchedChar = result[i];

            if (glitchedChar == '?' || ProtectedChars.Contains(originalChar))
            {
                sb.Append(originalChar);
            }
            else
            {
                sb.Append(glitchedChar);
            }
        }

        var sanitizedString = await GetSanitizedStringAsync(sb.ToString());
        return Reply(sanitizedString);
    }

    private async Task<string?> GetSanitizedStringAsync(string message)
    {
        var sensitiveScanResults = await sensitiveScanService.GetScanResultsAsync([message]);
        var sanitizedString = sensitiveScanService.SanitizeString(message, sensitiveScanResults[0]);
        return sanitizedString;
    }
}