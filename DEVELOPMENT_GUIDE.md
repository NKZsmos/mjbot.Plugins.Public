# mjbot 插件开发指南

欢迎来到 mjbot 插件开发指南！本文档旨在帮助初次接触 mjbot 插件框架的开发者快速上手，了解如何开发、配置和调试插件。

## 1. 环境准备

在开始之前，请确保您的开发环境满足以下要求：

*   **操作系统**: Windows, Linux, or macOS
*   **开发工具**: Visual Studio 2022, JetBrains Rider, 或 VS Code
*   **SDK**: .NET 8.0 SDK

## 2. 项目结构

mjbot 的插件项目通常包含以下目录结构：

```
src/
├── mjbot.Plugins.Public/       # 插件主项目
│   ├── Configurations/         # 配置文件定义
│   ├── Plugins/                # 插件实现类
│   └── mjbot.Plugins.Public.csproj
```

核心依赖库位于 `dependencies/MilkiBotFramework`。

## 3. 创建你的第一个插件

一个基本的插件需要继承自 `BasicPlugin` 类，并使用 `[PluginIdentifier]` 属性进行标记。

### 3.1 新建插件类

在 `src/mjbot.Plugins.Public/Plugins` 目录下创建一个新的 `.cs` 文件，例如 `MyFirstPlugin.cs`。

```csharp
using System;
using System.Threading.Tasks;
using MilkiBotFramework.Messaging;
using MilkiBotFramework.Plugining;
using MilkiBotFramework.Plugining.Attributes;

namespace mjbot.Plugins;

// PluginIdentifier 用于唯一标识插件，GUID 需要生成一个新的，Index 决定加载顺序，一般默认不写即可
[PluginIdentifier("YOUR-GUID-HERE", Index = 10)]
public class MyFirstPlugin : BasicPlugin
{
    // 简单的指令处理
    [CommandHandler("hello")]
    public IResponse Hello()
    {
        return Reply("你好，世界！");
    }
}
```

### 3.2 依赖注入

mjbot 框架支持依赖注入。你可以通过构造函数注入服务，例如 `ILogger` 或配置接口。

```csharp
public class MyFirstPlugin(ILogger<MyFirstPlugin> logger) : BasicPlugin
{
    [CommandHandler("log")]
    public IResponse LogSomething()
    {
        logger.LogInformation("这是一条日志");
        return Reply("日志已记录");
    }
}
```

## 4. 指令系统 (Command System)

框架提供了强大的指令解析功能。

### 4.1 基本指令

使用 `[CommandHandler("command_name")]` 标记方法为指令处理函数。

```csharp
[CommandHandler("echo")]
public IResponse Echo([Argument] string content)
{
    return Reply(content);
}
```

### 4.2 参数 (Arguments) 与 选项 (Options)

*   `[Argument]`: 位置参数。
*   `[Option("short_name", "long_name")]`: 命名选项。

```csharp
[CommandHandler("greet")]
public IResponse Greet(
    [Argument] string name, 
    [Option("age", Abbreviate = 'a', DefaultValue = 18)] int age)
{
    return Reply($"你好, {name}! 你今年 {age} 岁了。");
}
```

### 4.3 权限控制

可以通过 `AllowedMessageType` 和 `Authority` 限制指令的使用范围和权限。

```csharp
// 仅限群聊使用
[CommandHandler("group_only", AllowedMessageType = MessageType.Channel)]
public IResponse GroupOnly() => Reply("这是群聊消息");

// 仅限管理员使用，官方QQ平台没有作用
[CommandHandler("admin_only", Authority = MessageAuthority.Admin)]
public IResponse AdminOnly() => Reply("管理员触发了这个命令");
```

## 5. 配置文件 (Configuration)

插件可以拥有自己的配置文件。

### 5.1 定义配置类

在 `Configurations` 目录下创建一个继承自 `ConfigurationBase` 的类。

```csharp
using MilkiBotFramework.Plugining.Configuration;
using System.ComponentModel;

namespace mjbot.Configurations;

public class MyPluginConfig : ConfigurationBase
{
    [Description("是否启用某项功能")]
    public bool EnableFeature { get; set; } = true;
}
```

### 5.2 在插件中使用配置

通过构造函数注入 `IConfiguration<T>`。

```csharp
public class MyConfigurablePlugin(IConfiguration<MyPluginConfig> config) : BasicPlugin
{
    [CommandHandler("check_config")]
    public IResponse CheckConfig()
    {
        return Reply($"功能开启状态: {config.Instance.EnableFeature}");
    }
    
    [CommandHandler("toggle")]
    public async Task<IResponse> Toggle()
    {
        config.Instance.EnableFeature = !config.Instance.EnableFeature;
        await config.Instance.SaveAsync(); // 保存配置
        return Reply("配置已更新");
    }
}
```

## 6. 消息监听

除了指令，你还可以重写 `OnMessageReceived` 方法来处理所有接收到的消息。这对于实现对话机器人或关键词触发非常有用。
对于官方QQ平台，该功能仅对AT消息触发

```csharp
public override async IAsyncEnumerable<IResponse> OnMessageReceived(MessageContext context)
{
    if (context.TextMessage.Contains("秘密"))
    {
        yield return Reply("我听到了秘密！");
    }
    
    // 注意：必须调用 base 或者 yield return，否则可能无法通过编译或运行（视具体实现而定）
    // 在 BasicPlugin 中，通常作为一个异步流返回 IResponse
}
```

## 7. 高级功能

### 7.1 多轮对话

可以使用 `context.GetNextMessageAsync()` 等待用户的下一条消息。

```csharp
[CommandHandler("quiz")]
public async IAsyncEnumerable<IResponse> Quiz(MessageContext context)
{
    yield return Reply("1 + 1 等于几？", out var nextMsgTask);
    
    var nextMsg = await nextMsgTask.GetNextMessageAsync(timeout: TimeSpan.FromSeconds(30));
    
    if (nextMsg?.TextMessage == "2")
    {
        yield return Reply("回答正确！");
    }
    else
    {
        yield return Reply("回答错误或超时。");
    }
}
```

## 8. 常见问题

*   **插件未加载**: 检查 `PluginIdentifier` 是否唯一，GUID 是否格式正确。
*   **指令无响应**: 检查指令名称是否冲突，或者是否抛出了未捕获的异常。

---

希望这份指南能帮助你开启 mjbot 插件开发之旅！如有更多问题，请参考 `DemoPlugin.cs` 示例代码。
