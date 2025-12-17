/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * Copyright (c) 2025 BJMANIA
 */

using Microsoft.Extensions.Primitives;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using MilkiBotFramework.Messaging;
using MilkiBotFramework.Plugining;
using MilkiBotFramework.Plugining.Attributes;
using MilkiBotFramework.Services;
using System;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Text;

namespace mjbot.Plugins;

[PluginIdentifier("CD18B6F0-A703-7A7D-98DA-1BCBE5585890", Index = 1, Authors = "Isaax", Scope = "mjbot")]
[Description("鳄鱼插件")]
public class Crocodile(ISensitiveScanService sensitiveScanService) : BasicPlugin
{
    private const int MaxLoopCount = 1000;
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
        string traditional = ChineseConverter.Convert(origin, ChineseConversionDirection.SimplifiedToTraditional)
            .Replace('爲', '為'); // 先转换为繁体
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
        string simplified =
            ChineseConverter.Convert(chinese, ChineseConversionDirection.TraditionalToSimplified); // 将转成的繁体中文转成简体

        StringBuilder sb = new(simplified.Length);

        int length = Math.Min(simplified.Length, origin.Length); // 以原句和鹅语两者中短的为长度上限

        for (int i = 0; i < length; i++)
        {
            char glitchedChar = simplified[i];
            char originalChar = origin[i];

            if (glitchedChar == '・' || glitchedChar == '?' || ProtectedChars.Contains(originalChar))
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
    /// 中文转一阶鹅语指令
    /// </summary>
    [CommandHandler("goose")]
    [Description("中文转鹅语指令 /goose {阶数} [原文]")]
    public async Task<IResponse?> Goose(MessageContext context)
    {
        var param = context.CommandLineResult?.SimpleArgument.ToString();
        if (string.IsNullOrEmpty(param))
        {
            return null;
        }

        var sanitizedString = await GetSanitizedStringAsync(Convert1(param));
        if (string.IsNullOrWhiteSpace(sanitizedString)) return null;
        string[] parts = sanitizedString.Split(' ' , 2);
        if (int.TryParse(parts[0], out int number))
        {
            string origin = parts[1];
            string result = Convert1(origin);
            StringBuilder sb = new StringBuilder();
            sb.Append(result);
            for (int i = 0; i < number - 1; i++)
            {
                if (result != Convert1(result))
                {
                    sb.Append("\r\n");
                    sb.Append("↓");
                    sb.Append("\r\n");
                    result = Convert1(result);
                    sb.Append(result);
                }
                else
                {
                    break; 
                }
            }
            return Reply(sb.ToString());
        }
        else
        {
            return Reply(sanitizedString);
        }
            
    }

    /// <summary>
    /// 鹅语转一阶中文指令
    /// </summary>
    /// <param name="context">所有参数（含空格）</param>
    /// <returns></returns>
    [CommandHandler("goose2")]
    [Description("鹅语转一阶中文指令")]
    public async Task<IResponse?> Goose2(MessageContext context)
    {
        var param = context.CommandLineResult?.SimpleArgument.ToString();
        if (string.IsNullOrEmpty(param))
        {
            return null;
        }

        var sanitizedString = await GetSanitizedStringAsync(Convert2(param));
        if (string.IsNullOrWhiteSpace(sanitizedString)) return null;
        return Reply(sanitizedString);
    }

    /// <summary>
    /// 中文转特征鹅语指令
    /// </summary>
    /// <param name="context">所有参数（含空格）</param>
    /// <returns></returns>
    [CommandHandler("goose3")]
    [Description("中文转特征鹅语指令")]
    public async Task<IResponse?> Goose3(MessageContext context)
    {
        var param = context.CommandLineResult?.SimpleArgument.ToString();
        if (string.IsNullOrEmpty(param))
        {
            return null;
        }

        string result = param;
        int count = 0;
        while (result != Convert1(result) && count < MaxLoopCount)
        {
            result = Convert1(result);
            count++;
        }

        var sanitizedString = await GetSanitizedStringAsync(result);
        if (string.IsNullOrWhiteSpace(sanitizedString)) return null;
        return Reply(sanitizedString);
    }

    /// <summary>
    /// 鹅语转特征中文指令
    /// </summary>
    /// <param name="context">所有参数（含空格）</param>
    /// <returns></returns>
    [CommandHandler("goose4")]
    [Description("鹅语转特征中文指令")]
    public async Task<IResponse?> Goose4(MessageContext context)
    {
        var param = context.CommandLineResult?.SimpleArgument.ToString();
        if (string.IsNullOrEmpty(param))
        {
            return null;
        }

        string result = param;
        int count = 0;
        while (result != Convert2(result) && count < MaxLoopCount)
        {
            result = Convert2(result);
            count++;
        }

        var sanitizedString = await GetSanitizedStringAsync(result);
        if (string.IsNullOrWhiteSpace(sanitizedString)) return null;
        return Reply(sanitizedString);
    }

    /// <summary>
    /// 中文转乱码鹅语指令（和windows不一样，尽量别用）
    /// </summary>
    /// <param name="context">所有参数（含空格）</param>
    /// <returns></returns>
    [CommandHandler("goose5")]
    [Description("中文转乱码鹅语指令（和windows不一样，尽量别用）")]
    public async Task<IResponse?> Goose5(MessageContext context)
    {
        var param = context.CommandLineResult?.SimpleArgument.ToString();
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
        if (string.IsNullOrWhiteSpace(sanitizedString)) return null;
        return Reply(sanitizedString);
    }

    private async Task<string?> GetSanitizedStringAsync(string message)
    {
        var sensitiveScanResults = await sensitiveScanService.GetScanResultsAsync([message]);
        var sanitizedString = sensitiveScanService.SanitizeString(message, sensitiveScanResults[0]);
        return sanitizedString;
    }
}