/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * Copyright (c) 2025 BJMANIA
 */

using System.Buffers;
using System.Text;
using Microsoft.EntityFrameworkCore.Query.Internal;
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

    /// <summary>
    /// 中文转鹅语
    /// </summary>
    /// <param name="origin">中文</param>
    /// <returns></returns>
    private static string Convert1(string origin)
    {
        string traditional = ChineseConverter.Convert(origin, ChineseConversionDirection.SimplifiedToTraditional).Replace('爲', '為'); // 先转换为繁体
        byte[] bytes = EucJp.GetBytes(traditional); 
        string result = Gbk.GetString(bytes); // 转鹅语

        StringBuilder sb = new(result.Length);

        int length = Math.Min(result.Length, origin.Length); // 以原句和鹅语两者中短的为长度上限

        for (int i = 0; i < length; i++)
        {
            char originalChar = origin[i];
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
        return sb.ToString();
    }

    /// <summary>
    /// 鹅语转中文
    /// </summary>
    /// <param name="origin">鹅语</param>
    /// <returns></returns>
    private static string Convert2(string origin)
    {
        byte[] bytes = Gbk.GetBytes(origin);
        string chinese = EucJp.GetString(bytes); // 转中文
        string simplified = ChineseConverter.Convert(chinese, ChineseConversionDirection.TraditionalToSimplified); // 将转成的繁体中文转成简体

        StringBuilder sb = new(simplified.Length);

        int length = simplified.Length; // 以中文为长度上限，可能超过上限

        for (int i = 0; i < length; i++)
        {
            char glitchedChar = simplified[i];

            if (glitchedChar == '?')
            {
                sb.Append(origin[i]);
            }
            else
            {
                sb.Append(glitchedChar);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 中文转一阶鹅语指令
    /// </summary>
    /// <param name="context">所有参数（含空格）</param>
    /// <returns></returns>
    [CommandHandler("goose")]
    public async Task<IResponse?> Goose(MessageContext context)
    {
        var param = context.CommandLineResult.SimpleArgument.ToString();
        if (string.IsNullOrEmpty(param))
        {
            return null;
        }
        var sanitizedString = await GetSanitizedStringAsync(Convert1(param));
        return Reply(sanitizedString);
    }

    /// <summary>
    /// 鹅语转一阶中文指令
    /// </summary>
    /// <param name="context">所有参数（含空格）</param>
    /// <returns></returns>
    [CommandHandler("goose2")]
    public async Task<IResponse?> Goose2(MessageContext context)
    {
        var param = context.CommandLineResult.SimpleArgument.ToString();
        if (string.IsNullOrEmpty(param))
        {
            return null;
        }
        var sanitizedString = await GetSanitizedStringAsync(Convert2(param));
        return Reply(sanitizedString);
    }

    /// <summary>
    /// 中文转特征鹅语指令
    /// </summary>
    /// <param name="context">所有参数（含空格）</param>
    /// <returns></returns>
    [CommandHandler("goose3")]
    public async Task<IResponse?> Goose3(MessageContext context)
    {
        var param = context.CommandLineResult.SimpleArgument.ToString();
        if (string.IsNullOrEmpty(param))
        {
            return null;
        }
        string result = param;
        while (!(result == Convert1(result)))
        {
            result = Convert1(result);
        }

        var sanitizedString = await GetSanitizedStringAsync(result);
        return Reply(sanitizedString);
    }

    /// <summary>
    /// 鹅语转特征中文指令
    /// </summary>
    /// <param name="context">所有参数（含空格）</param>
    /// <returns></returns>
    [CommandHandler("goose4")]
    public async Task<IResponse?> Goose4(MessageContext context)
    {
        var param = context.CommandLineResult.SimpleArgument.ToString();
        if (string.IsNullOrEmpty(param))
        {
            return null;
        }
        string result = param;
        while (!(result == Convert2(result)))
        {
            result = Convert2(result);
        }

        var sanitizedString = await GetSanitizedStringAsync(result);
        return Reply(sanitizedString);
    }

    /// <summary>
    /// 中文转乱码鹅语指令（实际为鹅语加很多空格后转特征中文）
    /// </summary>
    /// <param name="context">所有参数（含空格）</param>
    /// <returns></returns>
    [CommandHandler("goose5")]
    public async Task<IResponse?> Goose5(MessageContext context)
    {
        var param = context.CommandLineResult.SimpleArgument.ToString();
        if (string.IsNullOrEmpty(param))
        {
            return null;
        }
        StringBuilder sb = new StringBuilder();
        sb.Append(param);
        sb.Append("          ");

        string result = sb.ToString();
        for (int i = 0; i < 10; i++)
        {
            result = Convert2(result);
        }

        var sanitizedString = await GetSanitizedStringAsync(result);
        return Reply(sanitizedString);
    }
    private async Task<string?> GetSanitizedStringAsync(string message)
    {
        var sensitiveScanResults = await sensitiveScanService.GetScanResultsAsync([message]);
        var sanitizedString = sensitiveScanService.SanitizeString(message, sensitiveScanResults[0]);
        return sanitizedString;
    }
}