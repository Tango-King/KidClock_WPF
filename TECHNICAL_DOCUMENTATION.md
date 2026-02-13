# KidClock 技术文档

## 1. 项目概述 (Project Overview)

**KidClock** 是一款基于 C# WPF 和 .NET 8 开发的儿童时钟认知教育软件。旨在通过交互式动画、生动的图形和趣味游戏，帮助 6-10 岁儿童直观理解时钟原理、时间关系，并培养时间观念。

### 核心功能
*   **时钟认知 (Clock Face)**: 展示实时走动的模拟时钟和数字时钟，支持 3D 风格视觉效果。
*   **时间魔法 (Time Magic)**: 演示时针、分针、秒针的联动关系（如齿轮比），支持加速演示和轨迹显示。
*   **拼图挑战 (Puzzle Challenge)**: 时钟组装游戏，将散落的数字拖拽到正确的位置。

---

## 2. 技术栈 (Tech Stack)

*   **开发语言**: C# 12
*   **框架**: .NET 8.0 (Windows Desktop)
*   **UI 框架**: WPF (Windows Presentation Foundation)
*   **架构模式**: MVVM (Model-View-ViewModel)
*   **核心库**:
    *   **Prism.DryIoc (8.1.97)**: 用于模块化开发、依赖注入、区域导航和事件聚合。
    *   **CommunityToolkit.Mvvm**: 轻量级 MVVM 工具包，用于 ViewModel 的实现。
    *   **Microsoft.Data.Sqlite**: 用于本地数据存储（游戏记录、设置）。

---

## 3. 系统架构 (Architecture)

项目采用 **Prism 模块化架构**，将功能拆分为独立的模块，降低耦合度。

### 3.1 解决方案结构

```text
KidClock_WPF/
├── KidClock/                   # [Shell] 启动项目，主窗口，容器配置
├── KidClock.Core/              # [Core] 共享接口、常量、事件、基类
├── KidClock.Services/          # [Services] 数据服务、业务逻辑服务
├── KidClock.Modules.ClockFace/ # [Module] 时钟认知模块
├── KidClock.Modules.Education/ # [Module] 时间魔法教育模块
└── KidClock.Modules.Games/     # [Module] 时间侦探游戏与拼图挑战模块
```

### 3.2 核心模式

*   **依赖注入 (DI)**: 使用 DryIoc 容器管理服务和 ViewModel 的生命周期。
*   **区域导航 (Region Navigation)**: 使用 Prism 的 `RegionManager` 在 `MainWindow` 的 `ContentControl` 中动态切换视图。
*   **数据绑定**: 广泛使用 WPF 数据绑定，ViewModel 实现 `INotifyPropertyChanged` (通过 `BindableBase`)。

---

## 4. 模块详解 (Module Details)

### 4.1 KidClock (Shell)
*   **App.xaml.cs**: 应用程序入口。负责：
    *   注册服务 (`IDataService`)。
    *   配置模块目录 (`ModuleCatalog`)。
    *   显式配置 ViewModelLocator。
    *   初始化导航（启动时加载 `ClockFaceView`）。
*   **MainWindow**: 主窗口，包含左侧导航栏和右侧内容区域 (`MainRegion`)。

### 4.2 KidClock.Core
*   **RegionNames**: 定义区域名称常量（如 `"MainRegion"`）。
*   **Interfaces**: 定义 `IDataService` 等接口，确保模块间解耦；同时提供游戏战绩写入/查询与周报聚合接口。
*   **Models**: 定义 `GameSessionResult`、`WeeklyReport` 等数据模型，用于战绩与周报展示。

### 4.3 KidClock.Services
*   **SqliteDataService**: 实现 `IDataService`。
    *   使用 `Microsoft.Data.Sqlite` 操作嵌入式数据库。
    *   负责初始化数据库表结构和保存/读取用户进度与游戏战绩。
    *   数据表：
        *   `Settings`: 轻量设置（Key/Value）。
        *   `GameSessionResults`: 单局战绩记录（游戏名、模式、难度、正确率/耗时、星级、错题标签等）。

### 4.4 KidClock.Modules.ClockFace
*   **功能**: 显示当前系统时间。
*   **实现**:
    *   使用 `DispatcherTimer` 每秒更新 ViewModel 时间。
    *   **动态绘制**: 表盘刻度和数字在 `ClockFaceView.xaml.cs` 中通过 C# 代码生成，以优化性能并解决 XAML `ItemsControl` 绑定 `Transform` 时可能出现的上下文问题。

### 4.5 KidClock.Modules.Education (时间魔法)
*   **功能**: 演示时间流逝的快慢。
*   **实现**:
    *   **动画逻辑**: ViewModel 维护虚拟时间，支持倍速播放（如 1小时/秒）。
    *   **轨迹系统**: 可选显示指针划过的轨迹，帮助理解角度变化。
    *   **View**: 同样采用 Code-Behind 绘制高精度刻度和数字。

### 4.6 KidClock.Modules.Games (游戏模块)
*   **功能**: 包含时间类互动游戏、拼图与记忆类游戏，以及面向6-8岁的益智游戏合集。
*   **时间侦探实现**:
    *   **鼠标交互**: 在 `TimeDetectiveView.xaml.cs` 中监听 `MouseMove` 事件。
    *   **角度计算**: 计算鼠标位置相对于表盘中心的角度变化 (`Math.Atan2`)。
    *   **时间映射**: 将角度增量转换为分钟数 (`6度 = 1分钟`) 并更新 ViewModel。
    *   **判定逻辑**: 比较用户拨动的时间与目标时间，允许一定的误差范围（如 ±2分钟）。
*   **拼图挑战实现**:
    *   **功能**: 将散落的数字 1-12 拖拽到表盘正确位置。
    *   **实现**: 使用 Canvas 覆盖层 (`DragCanvas`) 处理全屏拖拽逻辑。
    *   **交互**: 鼠标按下捕获数字块，移动时更新 Canvas 坐标，释放时检测是否在正确位置附近（吸附逻辑）。
    *   **布局**: 采用 3列网格布局动态排列右侧零件箱中的数字。

*   **益智游戏合集（6-8岁）**:
    *   **入口**: `LogicPackView` 展示各益智游戏入口与“家长周报（近7天）”。
    *   **游戏列表**:
        *   `SequenceTrainView`: 序列火车（序列推理）。
        *   `CategoryMarketView`: 分类小超市（分类归纳）。
        *   `PatternMatrixView`: 规律拼图（找规律）。
        *   `CommandMazeView`: 指令迷宫（简单迷宫+规划）。
    *   **通用会话基类**: `GameSessionViewModelBase` 统一实现练习/挑战模式、90秒倒计时、暂停/继续、自适应三档难度与星级评价。

---

## 5. 关键实现细节 (Key Implementation Details)

### 5.1 表盘动态绘制 (Dynamic Clock Face)
为了解决 WPF `ItemsControl` 在处理复杂 `Transform` 绑定时的局限性（如 `CanFreeze` 警告或绑定失效），并通过代码精确控制刻度样式，项目中采用了 **Code-Behind 生成** 策略。

代码位置: [TimeRelationView.xaml.cs](KidClock.Modules.Education/Views/TimeRelationView.xaml.cs) 和 [TimeDetectiveView.xaml.cs](KidClock.Modules.Games/Views/TimeDetectiveView.xaml.cs)

```csharp
// 伪代码示例
double angle = i * 6;
var tick = new Rectangle { ... };
// 关键：先计算位置，再设置旋转
double rad = (angle - 90) * Math.PI / 180;
double x = centerX + radius * Math.Cos(rad);
double y = centerY + radius * Math.Sin(rad);
tick.RenderTransform = new RotateTransform(angle);
Canvas.Children.Add(tick);
```

### 5.2 导航修复 (Navigation Fixes)
为了确保导航系统的稳定性：
1.  **显式注册**: 在 `App.xaml.cs` 中使用 `ViewModelLocationProvider.Register` 显式关联 View 和 ViewModel。
2.  **硬编码区域名**: 在 XAML 中使用字符串 `"MainRegion"` 而非绑定，避免初始化时序问题。
3.  **初始导航**: 在 `OnInitialized` 中手动触发第一次导航请求。

---

## 6. 构建与运行 (Build & Run)

### 环境要求
*   Visual Studio 2022 或 VS Code
*   .NET 8 SDK
*   Windows 10/11

### 构建步骤
1.  打开解决方案 `KidClock.sln`。
2.  还原 NuGet 包：`dotnet restore`。
3.  构建解决方案：`dotnet build`。
4.  运行 `KidClock` 项目。

---

## 7. 未来展望 (Future Improvements)

*   **用户系统**: 引入多用户存档支持。
*   **更多游戏**: 增加“时间计算”小游戏。
*   **数据分析**: 生成学习报告，展示儿童在不同模块的学习时长和正确率。
*   **UI 美化**: 引入更丰富的动画库（如 Lottie）增强视觉吸引力。

---

## 8. 核心事件流与代码执行 (Core Event Flows & Execution)

### 8.1 应用程序启动与初始化流 (App Startup & Initialization)

1.  **入口点 (Entry Point)**: `App.xaml` 启动，触发 `OnStartup`。
2.  **Prism 初始化 (Prism Bootstrapper)**:
    *   `CreateShell()`: 创建 `MainWindow` 实例。
    *   `RegisterTypes()`: 注册全局单例服务 (如 `IDataService`) 和导航视图。
    *   `ConfigureModuleCatalog()`: 扫描并添加模块 (`ClockFaceModule`, `EducationModule`, `GamesModule`)。
3.  **模块加载 (Module Loading)**: Prism 依次初始化各模块，各模块在 `OnInitialized` 中注册自己的视图到导航注册表。
4.  **初始导航 (Initial Navigation)**: `App.OnInitialized()` 调用 `RegionManager.RequestNavigate("MainRegion", "ClockFaceView")`，展示默认首页。

代码参考: [App.xaml.cs](KidClock/App.xaml.cs)

### 8.2 导航流 (Navigation Flow)

当用户点击左侧菜单按钮时：

1.  **UI 触发**: `MainWindow.xaml` 中的按钮绑定到 `NavigateCommand`。
2.  **命令执行**: `MainWindowViewModel.ExecuteNavigateCommand` 接收目标视图名称参数 (如 "TimeRelationView")。
3.  **请求导航**: 调用 `_regionManager.RequestNavigate(RegionNames.MainRegion, navigationPath)`。
4.  **视图解析**: Prism 容器根据名称查找对应的 View 实例。
5.  **视图注入**: 将 View 注入到 `ContentControl` (Region) 中，替换原有内容。
6.  **上下文激活**: 新 View 的 ViewModel (如有 `INavigationAware`) 触发 `OnNavigatedTo` 方法，加载特定数据。

代码参考: [MainWindowViewModel.cs](KidClock/ViewModels/MainWindowViewModel.cs)

### 8.3 时间侦探交互流 (Time Detective Interaction)

用户通过拖拽拨动时钟指针的游戏逻辑：

1.  **开始交互 (MouseDown)**:
    *   用户在表盘区域按下鼠标。
    *   `Canvas_MouseDown` 捕获鼠标 (`CaptureMouse`)，记录交互状态 `_isDragging = true`。
2.  **实时计算 (MouseMove)**:
    *   获取鼠标相对于表盘中心的坐标 `(dx, dy)`。
    *   计算角度: `double angle = Math.Atan2(dy, dx) * 180 / Math.PI`。
    *   **角度映射**: 将角度转换为分钟数 (0-720分钟)。
    *   **数据更新**: 更新 ViewModel 的 `CurrentTime` 属性。
    *   **UI 响应**: 数据绑定触发 `RotateTransform` 更新，指针跟随鼠标旋转。
3.  **结束交互 (MouseUp)**:
    *   释放鼠标捕获 (`ReleaseMouseCapture`)。
    *   触发验证逻辑: 比较 `CurrentTime` 与 `TargetTime`。
    *   **反馈**: 如果误差在允许范围内 (如 ±2分钟)，显示“成功”动画；否则提示重试。

代码参考: [TimeDetectiveView.xaml.cs](KidClock.Modules.Games/Views/TimeDetectiveView.xaml.cs)

### 8.4 时间魔法演示流 (Time Magic Animation)

演示时钟加速运转的逻辑：

1.  **触发演示**: 用户点击“快进 1 小时”按钮。
2.  **命令执行**: `TimeRelationViewModel` 接收命令，设置 `IsRunning = true`。
3.  **动画循环**:
    *   启动 `DispatcherTimer` 或进入后台线程循环。
    *   **时间步进**: 每一帧增加虚拟时间 `VirtualTime += Speed * DeltaTime`。
    *   **属性通知**: `VirtualTime` 变化触发 `PropertyChanged`。
4.  **UI 更新**:
    *   时针、分针、秒针的角度属性绑定到 `VirtualTime` (通过 Converter 转换)。
    *   **轨迹绘制**: 如果开启轨迹，`Canvas` 根据每帧的针尖坐标绘制点或线段。

代码参考: [TimeRelationViewModel.cs](KidClock.Modules.Education/ViewModels/TimeRelationViewModel.cs)

### 8.5 拼图挑战交互流 (Puzzle Challenge Interaction)

用户将数字拖拽到表盘的游戏逻辑：

1.  **开始拖拽 (MouseDown)**:
    *   用户在右侧零件箱点击数字块。
    *   `DragCanvas` 捕获点击事件，识别被点击的 `Border` 元素。
    *   记录起始位置 `_originalPosition` 和点击偏移量 `_clickOffset`。
    *   捕获鼠标并置顶元素 (`ZIndex`)。
2.  **拖动过程 (MouseMove)**:
    *   根据鼠标移动的增量，实时更新数字块在 `DragCanvas` 上的 `Left/Top` 坐标。
3.  **释放检测 (MouseUp)**:
    *   释放鼠标捕获。
    *   **计算目标位置**: 根据数字值 (1-12) 计算其在表盘圆周上的正确坐标。
    *   **距离判定**: 计算当前位置与目标位置的欧几里得距离。
    *   **吸附 (Snap)**: 如果距离小于阈值 (50px)，将数字块直接移动到目标位置，变为绿色并锁定。
    *   **回弹 (Bounce)**: 如果距离过远，将数字块动画移动回 `_originalPosition` (或直接重置坐标)。
4.  **游戏胜利**: ViewModel 计数 `_currentScore`，当达到 12 时显示胜利信息。
5.  **重置**: 点击重置按钮，ViewModel 通知 View 清空 Canvas 并重新生成洗牌后的数字块。

代码参考: [PuzzleView.xaml.cs](KidClock.Modules.Games/Views/PuzzleView.xaml.cs)
