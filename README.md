# Merge Game 🧩 

A **merge-based puzzle game** built in Unity using C#. Players merge items to create higher-level objects and complete tasks. The game features an **inventory system, level-based progression, and producer mechanics**.

[![Watch the video](https://img.youtube.com/vi/UhQjHvKQyCU?feature=share/0.jpg)](https://youtube.com/shorts/UhQjHvKQyCU?feature=share)
---

## 📖 About the Project

This project is not intended for commercial use. It replicates the core mechanics of **Travel Town-style** merging games while implementing **clean code architecture** and efficient **design patterns**.

---

## 🛠️ Built With

- **Unity 2022.3.8**
- **C#**
- **Third-Party Libraries:**
  - `DoTween` – Tweening engine for smooth animations.
  - `UniTask` – Async/await utilities for better performance.
  - `Yellowpaper.SerializedDictionary` – Serializable dictionary support for Unity.
- **Custom Implementations:**
  - **Custom DI (Dependency Injection)** – Flexible and modular dependency management.

---

## 🧩 Architecture

This project follows the **Model-View-Presenter (MVP)** architecture to ensure:

✅ **Separation of Concerns** – Clear distinction between game logic and UI.  
✅ **Scalability** – Easy to expand with new mechanics.  
✅ **Maintainability** – Code remains clean and modular.  

### 🔹 Key Components

#### **1️⃣ Models**
- **Manage game data**, including:
  - **Grid structure** (8x8 board).
  - **Player inventory** and stored items.
  - **Task management system**.

#### **2️⃣ Views**
- **Handle UI interactions** and display game visuals.
- **Examples:**
  - **Game grid display**.
  - **Inventory UI**.
  - **Task UI**.

#### **3️⃣ Presenters**
- **Bridge between Models & Views**.
- Process **user input**, update models, and synchronize UI.
- **Examples:**
  - **InventoryPresenter** – Manages inventory interactions.
  - **MergePresenter** – Handles merging logic.
  - **TaskPresenter** – Tracks task progress.

#### **4️⃣ Handlers**
- **Abstract game logic** from Presenters.
- **Examples:**
  - **GridPawnFactoryHandler** – Handles factories.
  - **EffectHandler** – Handles visual effects.

#### **5️⃣ Factories**
- **Optimize object creation and pooling**.
- **Examples:**
  - **ApplianceFactory** – Creates and recycles appliances.
  - **ProducerFactory** – Manages producer generation.

---

## 🛠️ Features

### 🗺️ 8x8 Grid System
- Players can **move items by dragging**.
- Items should be **placed in the nearest empty cell**.
- **Game state is saved locally** after every operation.

### 📦 Inventory System
- **Players can store mergeable items** in inventory.
- Items can be **dragged and dropped back to the board**.
- **Inventory saves & loads using JSON**.
- Inventory has **unlimited space**.

### 🔄 Merge Mechanics
- **Merge 2 identical items** to create a **higher-level item**.
- **Appliance levels:** `2, 4, 8, 16, 32, ..., 2048`.
- **Merge Example:**  
  - `2 + 2 → 4`
  - `4 + 4 → 8`
  - `8 + 8 → 16`
  - **Level 2048 items should be removed when clicked.**

### 🏭 Producer Mechanics
- **Producer items generate Appliances** with every click.
- **Produced items appear in the nearest empty cell.**
- If the board is **full**, production is **blocked**.
- **Producers have a capacity**:
  - **Default max capacity:** `10`
  - **Starts with:** `10`
  - **Reduces by `1` with each production**.
  - **If capacity reaches `0`, the producer is replaced** with a **new random producer** on the board.
  - **Capacity increases every `30s` automatically**.

### 🎯 Task System
- **Maximum of 2 active tasks** at a time.
- **Tasks require merging specific appliances** (e.g., "Create Level 8 Appliance").
- **Tasks can be completed by clicking the required appliance**.
- **Completed tasks disappear, and new ones appear**.
- **Task UI shows required appliance levels**.
- **Cells with required appliances are highlighted in green**.
- **Tasks are saved in `PlayerPrefs`**.

### 🔥 Effects & Animations
- **Smooth merging animations** using **DoTween**.
- **Particle effects for merging & inventory interactions**.
- **Highlight effect for task-related items**.

---

## 🎮 Gameplay Summary

### 🔹 Game Flow
1️⃣ Players start on the **8x8 grid board**.  
2️⃣ They **drag and merge items** to create higher levels.  
3️⃣ **Producers generate appliances**, but they **consume capacity**.  
4️⃣ If a producer **runs out of capacity**, it is replaced with a **new producer** in a random location.  
5️⃣ Players complete **tasks by collecting required appliances**.  
6️⃣ Items can be **stored in inventory** for later use.  
7️⃣ **Progress is saved automatically**.

---

## 📂 Grid Structure

The grid state is stored in **`grid_data.json`**, allowing easy modifications.

### 🔹 JSON Grid Example
```json
{
  "grid_width": 8,
  "grid_height": 8,
  "tasks": [
    { "type": "ApplianceA", "level": 4, "capacity": -1 },
    { "type": "ProducerB", "level": 1, "capacity": 10 }
  ]
}
```

## iPad 本地爽玩改造（2026-10-04）

本副本基于 `Fisixus/merge-game` 的 `b9ad18e75fff26dce8511e7ae45a434bb6823bcb`。
保留 MVP、DI、对象池、3 个启动/主菜单/合成场景、8×8 棋盘、库存和 JSON/PlayerPrefs 存档。
上文是原作者的原版说明；本节说明改造后的实际行为。

### 已实现的行为

- 现有 ProducerA 系列（1 个类型、1 个等级，初始棋盘上的 5 个生产器）全部可用，轻点一次即可生产。
- 容量永不减少，无恢复计时、冷却或耗尽替换；旧存档中的零容量也按无限处理。棋盘满时仍需合成或存入库存。
- 原版没有体力门槛；生产仍无体力扣除，并显示无限体力。新增本地金币存档，初始 `99999999`。
- 原版没有钻石、商店、广告或内购系统，因此不新增这些系统。金币没有消费入口，也不更改订单难度或奖励规则。
- 保留 11 个物品等级（显示数值 2～2048）及 12 张有限订单；做完全部订单后仍可继续生产和合成，但不会自动编造新订单。
- 最高等级物品不再被轻点删除，可以用于最后一张订单。订单提交时重新匹配活跃棋盘中的物品，重复目标必须使用不同物品，防止重复提交和失效引用。
- 竖屏适配：UI 安全区容器、自适应棋盘视野、订单卡片、库存面板及可滚动库存。保留鼠标支持，触控使用同一主指针完成拾取/拖动/释放，并在失焦或取消时收回拖动。
- iOS 使用 IL2CPP、ARM64、设备 SDK、iPhone/iPad 通用目标，最低 iOS/iPadOS 15。保留反射 DI 所需的构造函数，防止 IL2CPP 裁剪导致启动失败。
- DI 的共享工厂只预初始化一次，棋盘坐标缓存随场景/棋盘重载刷新。

### 仓库与依赖检查

仓库约 73 MB（含初始 Git 历史），不是完整商业手游：没有地图装修或旅行剧情、联网服务或完整经济系统。
`Assets/Scripts` 为项目代码，`Assets/Prefabs` 和 `Resources` 提供棋子/订单/库存；`Assets/Scenes` 包含 3 个构建场景。
另外包含粒子示例、TMP 资源及 SerializedCollections 示例，它们不加入构建场景。

上游实际版本为 Unity **2022.3.8f1**。本副本采用同一 LTS 分支的 **2022.3.62f3**（官方 revision `96770f904ca7`），用于较新的 macOS/Xcode 构建。
已完成真实 Unity 导入、Play Mode 验证和 iOS 工程导出。Input System 1.6.3、TMP 3.0.6、Test Framework 1.1.33 保留原版；URP/Core/ShaderGraph 等随该 LTS 补丁解析的版本以已验证的 `Packages/packages-lock.json` 为准。UniTask 固定到上游 lock 已记录的提交 `8042b29ff87dd5506d7aad72bd6d8d7405985f27`。
未下载新的第三方游戏二进制或签名工具。保留上游已有 DOTween DLL 和 SerializedCollections 源码。

根许可证为 MIT，保留原作者版权。README 原作者说明为非商业项目；本次用途为个人本地游玩。
MIT 文件不等于所有随附素材都已独立完成版权审计；DOTween 自带版权/许可证链接，TMP 有自己的许可材料，随附粒子和人物素材没有独立的完整来源台账。本次不出售、不上架、不另行打包素材资产库。

参考：[Unity 官方版本](https://unity.com/releases/editor/whats-new/2022.3.62f3)、[Unity 官方安全修复说明](https://unity.com/security/sept-2025-01/remediation)。

### 当前验证事实与限制

用户已完成 Unity Dashboard 和本机官方 CLI 登录，确认符合 Personal 条件并同意条款；Personal 激活及 license status 已成功。Unity 2022.3.62f3/iOS 模块已安装到项目 work/，真实 Xcode 工程已导出。GitHub macOS/Xcode 归档成功，已生成真实未签名 IPA。用户仅有普通 Apple ID，没有开发者会员或可用 Mac；尚未进行本人 Apple 签名或设备安装。

本地已执行：

- `python3 ci/check-project.py`：38 项仓库/资源/版本/构建脚本检查通过，见 [原始结果](Tests/project-result.txt)。
- `dotnet run --project Tests/Portable/Portable.csproj -- .`：34 项检查通过，见 [原始结果](Tests/portable-result.txt)。编译并执行生产器、合成条件、订单匹配、任务模型、DI 和布局计算的真实源文件；Unity 引擎对象由轻量替身提供。
- 上述检查还使用 Roslyn 按 C# 9、Editor/iOS 两组条件符号解析所有 Assets C# 文件；**这是语法检查，不是完整 Unity 编译**。
- `python3 -m unittest discover -s Tests -p 'test_*.py' -v`：9 项打包/解压防护测试通过，见 [原始结果](Tests/artifact-result.txt)。测试使用临时合成结构，不生成可安装游戏 IPA，也不连接设备。
- `git diff --check`、工作流 YAML 解析、3 个 Bash 构建脚本语法检查通过。
- 布局数值验证覆盖 768×1024、810×1080、834×1194、820×1180、1024×1366、744×1133，以及 390×844。真实 Unity 检查还验证了 768×1024 GameView 的摄像机拾取、拖放与棋盘边界；真机渲染、字体、实际触控、多指取消和设备安全区尚待安装后验证。

.NET 检查工具位于本次工作区的 `work/`，未加入仓库，也不修改系统 PATH。
其首次启动自动生成的 localhost 开发证书已精确删除；后续运行禁用证书生成。未改动连接的 iPhone/iPad、配对、应用、开发者模式或信任设置。

### GitHub Actions 构建入口

工作流均为手动触发，不因推送自动消耗构建额度。

1. **Source checks**（`.github/workflows/source-checks.yml`）：Linux Runner，执行仓库、可移植逻辑和 IPA 防护测试，无需 Unity 许可证。
2. **iOS build**（`.github/workflows/ios-build.yml`）：默认使用 `macos-15-intel` 与 Xcode 16.4，执行真实 Unity 编译/Play Mode 验证，然后导出 iOS、Xcode archive、IPA 和 Xcode 工程压缩包。
3. **iOS archive from exported project**（`.github/workflows/ios-archive.yml`）：本机已激活的 Unity 导出工程，macOS Runner 只执行 Xcode 归档，不需要云端 Unity 许可证。优先输入公开 Release tag `ios-export-input` 和 `ios-export.zip`。

真实 Unity 验证入口：`LocalMerge.Editor.GameplaySmoke.Run`。它从原有 LoadScene 启动，通过主菜单进入棋盘，检查 100 次生产、实际屏幕坐标拾取/拖放合成、订单连点提交、存档、最高等级物品及棋盘视野。
测试使用随机独立的应用名称隔离存档，只允许在专用 batchmode 工作目录运行；失败返回非零退出码。
**该测试已在本机真实 Unity 中通过，115 次断言的原始结果保存在 `Tests/unity-playmode-result.txt`。**

`runner=hosted-pro` 必须由本人将合法 Unity Pro 构建凭据存入仓库 Actions Secrets：`UNITY_EMAIL`、`UNITY_PASSWORD`、`UNITY_SERIAL`。
只接受官方 Unity macOS 安装包，检查 Unity Developer ID 安装签名；授权在临时 Runner 中激活并在结束时归还。
Unity Personal 应通过本人在 Unity Hub 中登录激活，不能把旧式免费许可证生成/转移流程当作可靠的托管 Runner 方案。
`runner=personal-mac` 可使用本人已通过 Hub 激活、安装该 Unity 版本及 iOS 模块/Xcode 16.4 的专用 Mac Runner（标签 `self-hosted, macOS, local-merge`）；该模式不改用户钥匙串，只导出未签名包。

`signing=unsigned` 生成 `MergeSandbox-UNSIGNED.ipa`：这是**待签名**容器，不能直接安装。
`signing=signed` 使用自己的 Apple Development 证书、包含目标设备 UDID 的开发描述文件以及一致的 bundle ID：

| Actions Secret | 内容 |
| --- | --- |
| `APPLE_TEAM_ID` | 自己的 Apple 团队 ID |
| `IOS_CERTIFICATE_P12_BASE64` | 自己导出的 Apple Development P12，经 Base64 编码 |
| `IOS_CERTIFICATE_PASSWORD` | P12 导出密码 |
| `IOS_PROFILE_BASE64` | 自己的 development mobileprovision，经 Base64 编码 |

证书只进入临时 Runner 钥匙串，签名输入不上传为 artifact，结束后删除。
不要把账号密码、证书、描述文件或许可证提交到 Git，也不要粘贴到聊天中。
普通 Apple ID 的 Personal Team 与开发者会员签名能力不同；没有合适开发描述文件时，先使用未签名导出，再在合法 Mac/Xcode 环境用自己的 Apple ID 签名。

`IosBuild.Export` 可从 Unity 菜单 `Tools > Local Merge > Export iOS Xcode Project` 使用，或在已激活的 macOS Unity 中执行：

```bash
"$UNITY_EDITOR" -batchmode -quit -buildTarget iOS -projectPath "$PWD" \
  -executeMethod LocalMerge.Editor.IosBuild.Export -logFile Builds/unity-ios.log
bash ci/package-ios.sh unsigned
```

Linux 不能用原生 Xcode 完成 archive/Apple 签名。工作流文件已配置不代表云端 iOS 构建已经成功。
参考：[Unity 授权](https://docs.unity.com/en-us/engine/6000.3/manual/get-started/install-and-upgrade/licenses-and-activation/managing-your-unity-license)、[GitHub 官方 Runner](https://docs.github.com/en/actions/reference/runners/github-hosted-runners)、[Apple 签名与分发](https://developer.apple.com/documentation/xcode/distributing-your-app-for-beta-testing-and-releases)。

### 只有 Linux、没有 Unity Pro/Mac 时的官方云端方案

使用 [Unity Build Automation](https://docs.unity.com/en-us/build-automation)，由本人先在 [Unity Dashboard](https://cloud.unity.com/) 注册/登录和创建组织/项目，核对是否符合 Personal 条件及服务的当前免费额度。
官方 2026 定价列出每月 100 分钟 Mac Standard Compute；**不得自动开通付费或取消额度限制**，以实际账号 Dashboard 为准。
免费额度超限会锁定构建服务，不能据此保证本项目一定在免费分钟内完成。

待账号可用后，连接本 GitHub 副本，配置：

- 仓库根目录，分支 `main`；Unity 2022.3.62f3；iOS；Mac Standard；Xcode 16.4（若服务不提供此组合，应先确认可用的同分支版本，不盲目升级大版本）。
- 关闭 Auto-build；先手动构建一次。保留免费计划/构建分钟上限，不自动授权计费。
- 设置环境变量 `IOS_BUNDLE_ID` 为自己的签名标识。
- Pre-export method：`LocalMerge.Editor.CloudBuildHooks.PreExport`；Post-export method：`LocalMerge.Editor.CloudBuildHooks.PostExport`。
- 该官方云端服务的 iOS 签名配置需自己的 Apple 证书和描述文件；服务中提交这些敏感材料前，由本人授权/完成账号操作。
- 这些云端回调会配置 portrait/ARM64/iOS 15 并验证资源；不等于运行 GitHub 工作流中的 Play Mode 验证。

参考：[Unity 官方免费计划](https://docs.unity.com/en-us/devops/pricing/free-plan)、[当前定价](https://unity.com/products)、[iOS 云构建要求](https://docs.unity.com/en-us/build-automation/basic-build-configuration/set-up-an-ios-build-configuration)、[云端脚本回调](https://docs.unity.com/en-us/build-automation/advanced-build-configuration/run-custom-scripts-during-the-build-process)。

### 签名 IPA 生成后的 Linux USB 安装

用户最新声明连接的是 iPad；没有读取设备标识或改变设备状态。需要安装时先确认具体目标 iPhone/iPad 和 UDID，不能凭“只有一个 USB 设备”猜测。
只用自己的合法 Apple 签名。Apple ID 登录、双重认证、设备开发者模式和“信任”由本人完成。

`ci/install-signed-ipa.py` 已准备好：必须显式传入 IPA 和 UDID，未签名包在接触设备前就被拒绝。
它检查描述文件期限、应用标识、目标 UDID 和签名元数据，然后只安装指定应用；不卸载应用、不重置设备、不更改配对、不启用开发者模式。
需要时从 Linux 发行版的可信软件源安装 `libimobiledevice`/`ideviceinstaller`；本次未安装系统软件或启动安装脚本。

```bash
python3 ci/install-signed-ipa.py --ipa /absolute/path/game.ipa --udid TARGET_DEVICE_UDID --check-only
# 在设备和安装范围确认后，去掉 --check-only 才会实际安装。
```

包结构/描述文件检查不能代替 Apple 信任链或真机运行验证，设备在安装时验证实际签名。
参考：[ideviceinstaller 官方源码与用法](https://github.com/libimobiledevice/ideviceinstaller)、[Apple 开发者账号能力](https://developer.apple.com/help/account/basics/about-your-developer-account)。

### 继续工作的必要输入

代码改造、真实 Unity 验证、iOS 工程导出和 macOS 设备版归档已完成；GitHub 副本已推送，真实未签名 IPA 已生成。编译阶段完成；安装前只剩本人 Apple 签名，随后才能进行 USB 安装和真机验证。连接 USB 不会自动提供有效签名。

### 云端接续状态（2026-10-04）

- GitHub 副本：<https://github.com/xiaoyanzi191/merge-game-ipad>。当前已验证并导出的游戏源代码提交为 `e1c7c84`。
- [最新 GitHub Source checks](https://github.com/xiaoyanzi191/merge-game-ipad/actions/runs/37212142257) 已通过：仓库完整性、可移植逻辑及 IPA 防护测试三步均成功。真实 Unity 检查另见下方结果。
- Unity 项目名称 `Merge Game iPad`。Dashboard 显示 Free tier，包括 100 Mac Standard Minutes。未升级付费计划。
- 公开 HTTPS 拉取被 Unity 判为不可访问，已按用户单独授权添加只读部署密钥，并通过 SSH 成功连接本副本。密钥仅作用于该仓库，GitHub 验证 `read_only=true`；没有向 Unity 传输账号级 GitHub 令牌。
- iOS 表单草稿已选 Unity 2022.3.62f3、macOS Sequoia、Xcode 16.4、Mac Standard、Apple Silicon、`main`、`com.localmerge.sandbox`、strict mode、XCArchive 和本仓库的两个回调。自动构建/定时构建均关闭。
- 实际 UI 验证：iOS 的 `Credentials set` 为必填项，当前为空；表单拒绝保存，目标尚未创建，也未运行 iOS 构建。不是已经成功构建后只需补签名。
- 已保存 `Gameplay Validation WebGL` 验证目标：同一 Unity 版本/macOS/Xcode，Mac Standard 免费额度，strict mode/development build，关闭自动与定时构建。用于取得真实 Unity 编译和可交互玩法证据，不替代 IPA。
- 用户目前只有普通 Apple ID，没有会员或可用 Mac。本机 Unity 已通过官方 CLI 合法激活并安装；不自动购买开发者会员，不使用未知签名服务或他人证书。

### 本机导出与独立 Xcode 归档路径

已核实 Unity 官方 CLI 1.0.0-beta.12 支持 `license activate --personal --accept-eula`。用户本人完成登录确认，并明确同意条款；许可证激活和状态查询都成功。CLI 从 Unity 官方 CDN 下载且核对 SHA256，位于本次工作区 work/；没有执行会修改 shell 配置的安装脚本。

`.github/workflows/ios-archive.yml` 已加入：只需已导出的 Unity Xcode 工程，不在 macOS Runner 上运行 Unity，因此不需要把本机许可证转移到云端。输入指定的 Release/ZIP（或导出 ref/ZIP），解压保留执行权限，拒绝路径穿越和符号链接，调用 Xcode 生成设备归档与明确标记的未签名 IPA。

云端 WebGL 验证 #1 已实际执行：C# 日志只看到原版 unused-variable 警告，失败点为 Apple Silicon 无图形环境无法回退到 CPU 光照器，导出目录为空。耗时 5:48，其中构建计量 4:13，未获得可玩产物。原有 3 个场景已关闭 baked/realtime GI；没有为绕过错误关闭 strict mode。随后免费 Windows Micro（8 vCPU/16 GB）的 #2 复验成功，具体结果见下方。服务将 Windows WebGL 标为实验性，该结果不替代 iOS 设备版编译。

本机编辑器安装路径限定在 work/unity-editors；许可证和官方登录状态保存在 Unity 专属配置中，已获用户许可。所有签名/许可证资料留在 work/或官方专属配置，不进入公开仓库、源码 ZIP 或 Actions artifact。

参考：[Unity 官方 CLI 安装与登录](https://docs.unity.com/en-us/unity-cli/use-unity-cli)、[官方许可命令](https://github.com/Unity-Technologies/skills/blob/main/skills/unity-cli/references/auth-license-cloud.md)。

### 已完成的真实本机验证与导出

- Linux Unity 2022.3.62f3 已实际运行完整项目。修复加载场景主摄像机干扰拾取、动画回收后碰撞器状态残留，并让 smoke 等待加载遮罩结束后再进行输入验证。
- [真实 Play Mode 结果](Tests/unity-playmode-result.txt)：115 次断言、16 种行为通过，覆盖原场景启动、无限资源、零容量兼容、100 次实际生产、池化碰撞器恢复、正确摄像机拾取、屏幕坐标拖拽合成、JSON 存档、订单双提交保护/消耗/补充、最高等级物品与安全区。
- Unity 的 `IosBuild.Export` 已以 iOS 目标和 strict mode 成功执行，产生真实 `Unity-iPhone.xcodeproj`。导出 ZIP 约 215 MiB，CRC/根目录与签名资料排除检查通过。尚未得到可安装的签名 IPA。
- Editor 的 UIOrientation.Portrait 序列化值为 0；运行时 ScreenOrientation.Portrait 为 1。已按真实 Editor 设置和导出 Info.plist 修正仓库检查。
- Unity 本次导入更新了随 2022.3.62f3 分发的 2D/Burst/Core/ShaderGraph 等间接包及其序列化默认字段；保留实际验证过的 lock 与设置，没有引入新的第三方框架。
- Windows 云端 WebGL #2 成功（28:52 含等待，计量构建 19:37，产物 22.14 MB），源代码为 `96a6aa9`。其后本机已修复拾取/回收问题，并完成更完整的真实 Play Mode 检查。
- 用户已单独授权把未签名 Xcode 导出作为公开 Release 构建输入上传，使用 `.github/workflows/ios-archive.yml` 的 release 模式，避免大文件进入 Git 历史；只在 macOS Runner 上运行 Xcode。
- [公开构建资料 Release](https://github.com/xiaoyanzi191/merge-game-ipad/releases/tag/ios-export-input) 已上传真实 Xcode 导出，SHA256 为 `abd243ca10a6aa426bfc81e3c030ae2bd4a984af28552994f2249140e784d886`。
- [macOS iOS 归档 #1](https://github.com/xiaoyanzi191/merge-game-ipad/actions/runs/37212679612) 成功：`macos-15-intel`、Xcode 16.4、设备 SDK、Release，Xcode archive/未签名 IPA 打包/包结构检查全部通过，作业耗时 8:55。
- `MergeSandbox-UNSIGNED.ipa` 约 19.55 MiB，SHA256 为 `81e2a291ab500c0f10f6585d2c4215ff352d99a997e08cc7f255ce527e5177e2`。下载回本机后 CRC、IPA 结构、主程序及 UnityFramework 的 Mach-O ARM64、iPhoneOS、设备 family `[1,2]`、最低版本 15.0、iPad/通用竖屏与全屏配置均通过核对。
- IPA 没有描述文件或代码签名目录，也没有许可证、P12、私钥等排除材料。它是真实设备程序，但不能直接安装；iOS 启动、实际触控、真机布局仍待本人签名安装后验证。没有连接或修改用户设备。

### 画面素材调查

当前画面仍使用上游原型素材和 2～2048 数字棋子，没有新增商业游戏的地图、装修或剧情。改善观感可以保留玩法和存档，统一替换棋子主题、背景、按钮与订单面板。

- [Kenney UI Pack](https://kenney.nl/assets/ui-pack)：430 个 2D UI 元素，官方标明 CC0；适合按钮和面板，但不能单独补齐主题棋子。
- [Merge2 Core SDK](https://github.com/ArtemVetik/com.agava.merge2)：MIT 的逻辑包，不是带完整美术的成品，换用它不能直接改善画面。
- [GoodsSort](https://github.com/AmanitaDev/GoodsSort)：Match-3 项目，可参考布局；README 列出 GUI Pro、Odin 等资源，其源码 MIT 不等于这些素材都可直接搬用。

本次只完成调查，尚未下载或混入新的美术素材。

### Linux 本人签名准备（2026-10-05）

用户已本人完成 Apple 免费开发者账号准备，没有加入付费计划。现准备社区开源工具路线，尚未使用 Apple ID 登录工具、生成本人签名或访问设备。

- [iloader 2.3.5](https://github.com/nab138/iloader/releases/tag/v2.3.5) 的官方发布 DEB 下载至本项目 `work/iloader-2.3.5/` 并仅解压，没有系统安装。SHA256 `febd37874ece0e5880796e84cafced1f4a478537dd3d65cab20c0260e39f6ed8` 与发布资产摘要一致，动态运行依赖检查通过。
- 检查了该版本的账号、设备选择、配置存储代码以及 isideload 0.4.0 源码（源提交 `52b504c2cd706a9e415109b0be9e137168034f5e`）。工具不是 Apple 官方软件；这些检查不等于完整安全审计，也不等于账号登录或侧载已成功。
- [anisette-v3-server](https://github.com/Dadoum/anisette-v3-server) 维护者镜像的 amd64 manifest 固定为 `sha256:97f5fd1e6cb060f8fbda65d3e836e056c4d744e32a8372d954faf892fc68b544`，验证镜像层摘要后，仅提取服务程序至 `work/anisette-local/`。程序报告 v2.2.2，SHA256 `246213c1c20c27a472a5199a25e7d0cbdde9bfcb85bc36724afaf3758afe1e6e`；未修改 Docker 权限或创建容器。
- 服务初始化所需库来自 Apple 官方 `apps.mzstatic.com` 的 Apple Music Android APK，未修改这些库；运行目录、状态和日志均限制在本项目 `work/anisette-local/`。服务只监听 `127.0.0.1:16969`，`/v3/client_info` 实际返回 200，通过 v3 接口检查。此过程未使用用户 Apple ID。
- `work/iloader-2.3.5/open-iloader-login.sh` 将 XDG/TMP 状态限定在权限为 700 的私有项目目录，并隔离系统 D-Bus 钥匙串；只用本机验证服务，默认不记住密码。最初依赖 USB 环境变量的隔离失效：iloader 的设备列表使用 `UsbmuxdAddr::default()`，忽略环境变量，尝试只读连接 lockdown 后报错。已关闭本次工具，并改用系统已有 bubblewrap 的独立挂载命名空间隐藏 `/run/usbmuxd`，根文件系统只读、仅本工具私有目录可写。子进程实际验证该路径不再是 socket，脚本语法检查通过；修正后的图形入口未重启，不能声称已验证界面登录。
- 用户已明确授权使用本人 Apple ID 在此本机工具中登录，为本游戏创建个人开发签名资料并保存在项目私有目录，完成后仅安装到本人确认的目标 iPad。密码/验证码由本人在工具中输入，不进入聊天或命令。已有证书不得自动吊销；目标设备和安装范围还需在安装时确认。
- 本人登录尝试在取得 Xcode app token 时被 Apple 拒绝（`-22411: This action cannot be completed at this time`），未完成 DeveloperSession、签名或安装。该错误在 [iloader #363](https://github.com/nab138/iloader/issues/363) 有未解决的上游报告；重新检查维护者最新发布仍为 v2.3.5，没有可确认的自动修复，不重复提交密码。用户要求保持设备现有账号，本人签名账号另用；账号不同本身不能证明是错误原因，不要求退出或更换设备账号。
- 用户已要求沿用本项目后续工作的授权，不重复询问已授权的准备、签名和安装操作。授权仍限定本游戏和确认的目标 iPad，不涉及其他账号、应用或文件。密码不得写入源码、公开日志、GitHub 或长期记忆；本人验证码和设备信任等交互仍需在出现时完成。
- [Apple 官方免费 Personal Team 说明](https://developer.apple.com/help/account/basics/about-your-developer-account)规定开发描述文件有效期为 7 天，到期需要重新部署。离线、个人自用不会免除原生安装签名要求；当前构建产物仍为未签名 IPA。

任何账号、证书、私钥、描述文件、ADI 状态或相关日志都不得提交到公开仓库、Release 或 Actions。只有公开的游戏源码与已检查的无签名构建资料可公开。

### 离线主屏幕候选版（2026-10-05）

用户已选择优先通过 Safari 添加到主屏幕、离线游玩，不再等待 Apple 签名。保留 Unity 的原玩法、场景、MVP/DI/资源体系，增加 WebGL 导出和离线页面；不是另一份重写的 JavaScript 游戏。

- 官方 WebGL 模块安装到已有的工作区 Unity 2022.3.62f3。`LocalMerge.Editor.WebBuild.Export` 已真实成功，8 个静态文件约 42.7 MiB，ZIP 约 13.45 MiB；导出不需要 Apple 账号。
- 原型美术保留。页面在横向窗口中保持竖屏游戏区域，包含安全区、主屏幕 manifest、原创图标和完整缓存；首次需要联网下载全部游戏文件。
- 浏览器 JSON 存档同步备份到本地存储，Unity 的 `autoSyncPersistentDataPath` 负责 IndexedDB 文件同步；原生文件/PlayerPrefs 行为保持不变。缓存下载失败不会替换已可用的版本，更新不会抢占正在运行的旧游戏，只清理本应用 scope 的旧缓存。
- 实测发现逐帧检测可能漏掉同一轮更新中的快速按下/释放，已改为现有 Input System 的输入事件。实际 Unity Play Mode 新结果为 **122 次断言、19 种行为通过**，包含连续快速鼠标点击、触控点击和触控取消；见 `Tests/unity-playmode-result.txt`。
- 本机浏览器实际执行连续快速生产、2→4→8 拖拽合成、订单提交/消耗；关闭本机 HTTP 服务器后重新打开，游戏仍能启动，已完成订单进度也恢复。截图保留在工作区 outputs/。这不是 iPad/Safari 真机测试。
- `node Tests/web-offline.test.cjs`：10 项真实 JS 持久化/缓存行为检查通过，覆盖最新存档、保存失败反馈、离线资源与导航、完整下载、失败恢复及其他 scope 的缓存保护。
- `ci/prepare-web.py` 冻结导出资源与版本；`ci/unpack-web.py` 拒绝路径穿越、重复路径、符号链接和签名资料。`.github/workflows/web-publish.yml` 使用固定到提交的 GitHub 官方 Pages actions，部署已检查的公开静态 ZIP；当前 HTTPS 发布结果待验证。
- 原生 IPA 仍为先前 `e1c7c84` 的无签名产物，不包含本次快速点击修复；当前主要交付目标为离线网页。若以后继续原生安装，应以最新源码重新导出并由本人签名。

兼容边界：[Unity 2022.3 官方文档](https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-browsercompatibility.html)不保证移动设备 WebGL 支持。因此仍需在目标 iPad Safari 中检查实际触控、存档以及关闭网络后从主屏幕重开，不能用桌面模拟代替真机验证。首次添加到主屏幕后，应联网打开该主屏幕入口，等游戏下载完成，再测试离线。浏览器网站数据被清理时，本地存档和缓存也会被清理。
