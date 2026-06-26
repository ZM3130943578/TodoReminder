你现在负责开发一个 Windows 11 本地桌面小工具。

项目目标：
开发一个 Windows 11 待办提醒小工具，支持全局快捷键显示/隐藏窗口、系统托盘、待办事项增删改查、完成/废弃状态、事项提醒、定时弹出、按天记录、未完成事项自动继承到下一天。

技术栈固定：
- .NET 8
- WPF
- C#
- MVVM
- SQLite
- Entity Framework Core 或 Dapper，优先 EF Core
- Serilog
- xUnit

禁止事项：
1. 不要使用 Electron。
2. 不要使用 Web 前端框架。
3. 不要使用 Python GUI。
4. 不要改成 Web 项目。
5. 不要引入云端数据库。
6. 不要引入复杂账号系统。
7. 不要一次性实现所有功能。
8. 不要随意更换技术栈。
9. 不要删除已有功能。
10. 不要绕过分层结构。

开发规则：
1. 每个阶段开始前，先阅读 docs/design.md 和当前代码。
2. 每个阶段只做当前阶段明确要求的功能。
3. 修改前先输出实现方案。
4. 等我确认后再写代码。
5. 写代码后必须运行 dotnet build。
6. 涉及业务逻辑时必须运行 dotnet test。
7. 出现错误时，先分析原因，再做最小必要修改。
8. 不要为了修一个错误大规模重构。
9. 每个阶段结束后，总结新增/修改的文件、实现内容、验证结果、遗留问题。
10. 保持项目能随时编译运行。

项目推荐结构：

TodoReminderTool
├── docs
│   └── design.md
├── src
│   ├── TodoReminder.App
│   ├── TodoReminder.Domain
│   ├── TodoReminder.Application
│   └── TodoReminder.Infrastructure
├── tests
│   └── TodoReminder.Tests
├── TodoReminderTool.sln
└── OPENCODE.md

分层职责：
- TodoReminder.App：WPF 界面、窗口、控件、托盘入口、用户交互。
- TodoReminder.Domain：实体、枚举、领域规则。
- TodoReminder.Application：业务服务、用例、DTO。
- TodoReminder.Infrastructure：SQLite、EF Core、Repository、Windows API、配置、日志。
- TodoReminder.Tests：业务逻辑测试。