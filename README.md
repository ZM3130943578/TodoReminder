# TodoReminder

Windows 11 桌面待办提醒小工具。

## 下载

从 [Releases](https://github.com/ZM3130943578/TodoReminder/releases) 下载最新版本：

| 文件 | 说明 |
|---|---|
| `TodoReminder.App.exe` | 单文件版，下载后直接运行 |
| `TodoReminder.msi` | 安装版，安装后含开始菜单和桌面快捷方式 |

> 两种版本均为自包含，无需安装 .NET 运行时。

## 功能

- **待办事项管理**：新增、编辑、删除、完成、废弃
- **事项提醒**：为每条事项设置提醒时间，到点弹窗
- **定时弹出**：配置定时规则，到点自动显示主窗口
- **按天记录**：每天待办独立记录
- **自动继承**：未完成事项自动继承到下一天
- **系统托盘**：常驻托盘，后台运行
- **全局快捷键**：`Ctrl + Alt + Space` 切换窗口显示/隐藏
- **设置页**：快捷键、窗口、提醒、定时弹出、数据、启动

## 技术栈

- .NET 10 + WPF
- C# + MVVM (CommunityToolkit.Mvvm)
- SQLite + Entity Framework Core
- Serilog

## 使用

### 快捷键

| 快捷键 | 功能 |
|---|---|
| `Ctrl + Alt + Space` | 切换主窗口显示/隐藏 |

### 系统托盘

| 操作 | 功能 |
|---|---|
| 左键双击 | 切换窗口显示/隐藏 |
| 右键 | 打开菜单（打开/今日/新增/设置/退出） |

### 数据路径

- **数据库**：`%AppData%\TodoReminderTool\todo.db`
- **日志**：`%AppData%\TodoReminderTool\logs\todo-{date}.log`

## 自行构建

```bash
dotnet publish src\TodoReminder.App -c Release -r win-x64
```

发布文件位于：`src\TodoReminder.App\bin\Release\net10.0-windows\win-x64\publish\TodoReminder.App.exe`

### 构建安装包

```bash
wix build setup\Product.wxs -b "%CD%" -o setup\TodoReminder.msi
```

## 项目结构

```
src/
├── TodoReminder.Domain         # 实体、枚举、领域规则
├── TodoReminder.Application    # 业务服务
├── TodoReminder.Infrastructure # SQLite、EF Core、Windows API
└── TodoReminder.App            # WPF 界面、ViewModel、托盘
setup/
└── Product.wxs                 # WiX 安装包源码
```
