/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * Copyright (c) 2025 BJMANIA
 */

using MilkiBotFramework.Data;
using MilkiBotFramework.Services;

namespace PluginTests;

public class DummySensitiveScanService : ISensitiveScanService
{
    public async Task<List<SensitiveScanEntry>> GetScanResultsAsync(params IEnumerable<string> messageContent)
    {
        return messageContent.Select(k => new SensitiveScanEntry()).ToList();
    }

    public string? SanitizeString(string? originalString, SensitiveScanEntry scanResult)
    {
        return originalString;
    }
}