# arbore TPM · 设备管理系统 · 项目路线图

> 防遗忘清单。所有阶段交付物按勾选推进，**不再承诺"一次性搞定"**。
>
> 业务源：活字格 `waes_mes_hamaton`（设备管理模块）
> 架构参考：`waes_nppms_core`（分层 + Razor + Dapper）
> 视觉参考：[Plane self-hosted](https://github.com/makeplane/plane)（Navigation 2.0）

---

## 0. 已锁定决策（2026-05-12）

| 项 | 选型 | 备注 |
|---|---|---|
| 后端框架 | .NET 8 ASP.NET Core MVC + Razor | 保留现有 4 项目分层 |
| ORM | Dapper | 自实现 `IRepository<T>` + `IUnitWork` |
| 数据库 | SQL Server `wantong_mes_20250211` | 已生成 `db-schema.sql` 幂等 DDL |
| 数据库连接 | `appsettings.Development.json` | 已 .gitignore；公开仓不会泄露 |
| 认证 | Token + Cookie + 模块按钮权限 | `AuthApp` + `Sys_User/Role/Module/ModuleButtons` |
| 前端 CSS | **Tailwind CSS 3.x** | 替换 layui，所有视图重写样式 |
| 前端 JS | **AlpineJS** | 替换 jQuery（仅保留 ajax 必要时） |
| 图标 | **Lucide** | 替换 font-awesome |
| 字体 | **Inter** + 系统中文回退 | |
| 主色 | **`#1F4D3B`** 森林绿 | 对齐 arbore logo |
| Logo | `arbore_logo.png` | 登录大图 + 顶栏 28px + favicon |
| 视觉对标 | Plane Navigation 2.0 | 深色侧栏 + 顶栏 + tab pill + 用户 dropdown |

---

## 1. 项目骨架（已完成 ✓）

- [x] 解决方案 4 项目分层（Web / App / Repository / Infrastructure）
- [x] 60 个 `[Table]` Domain 实体（8 个 Sys/Basic + 52 个 hamaton 业务）
- [x] `IRepository<T>` + `IUnitWork` + `BaseApp<T>` + `BaseController`
- [x] `AuthApp` 登录 / 登出 / Token 缓存 / 模块按钮权限注入
- [x] `db-schema.sql` 幂等 DDL（IF NOT EXISTS 风格，1210 行）
- [x] 编译 0 错 0 警告

## 2. 待清理

- [x] ~~删除 `Views/Home/Index.cshtml` 第 69 行 `admin / 123456 + MD5` 提示~~（已整页重写为 AppShell 入口）
- [ ] 删除 `device-mgmt-layui-v1/` 目录（demo placeholder，已无用）
- [ ] 决策：是否把 `device-mgmt-layui-v2/` 重命名为 `device-mgmt-layui/` 或 `arbore-tpm/`（**待用户拍板**）
- [ ] 老 layui / select2 / inputmask 资源（~30MB）等业务页全部用 Tailwind 重写后移除

---

## P0 框架壳（**已完成 ✓**）

> 目标：**没有任何业务页面也能跑起来**的完整应用骨架。Plane Nav 2.0 风格。

### P0.1 全局基础设施 ✓

- [x] Tailwind CSS（Play CDN `cdn.tailwindcss.com`，内联 brand 调色板）
- [x] AlpineJS（jsDelivr CDN）
- [x] Lucide Icons（jsDelivr CDN，`data-lucide` 渲染）
- [x] Inter 字体（`@fontsource/inter` jsDelivr CDN）
- [x] CSS 变量 `--brand-50...950`（围绕 `#1F4D3B` 11 档色阶）
- [x] CSS 变量：圆角 / 阴影 / 字号 / 字体 / 行高 / 顶栏 / 侧栏 / tab token
- [x] `arbore_logo.png` → `wwwroot/img/logo.png`
- [x] favicon（`wwwroot/favicon.ico`，复用 logo）
- [x] `wwwroot/css/app.css`（基线 + 组件层 + 暗色变量预留）
- [x] `wwwroot/js/app.js`（cookie / query / ajax-token / toast / theme / lucide 工具集）
- [x] **强化**：全局 `[x-cloak]` 规则写入 `app.css`（登录页与 AppShell 共用）

### P0.2 登录页 ✓

- [x] `_LayoutLogin.cshtml` 完全 Tailwind 化，丢弃 layui
- [x] 左半幕森林绿三段渐变 + logo + 8 模块清单 + 版权
- [x] 右半幕：账号 + 密码（图标 + 显隐切换）+ 记住我 + 错误内联提示
- [x] 回车提交 / 显隐 / disabled + spinner
- [x] AJAX POST `/Account/DoLogin`（不在 URL 暴露密码）
- [x] **强化**：`GET /Account/Login` 若 Cookie 中 Token 仍有效则**直接跳转首页**（已登录不重复登录）
- [x] **强化**：登录表单根节点 `x-cloak` + 全局 `[x-cloak]` 样式，减少 Alpine 首屏闪烁

### P0.3 主界面（AppShell）✓

- [x] `_Layout.cshtml` 三栏 grid（60px 顶 / 240px 侧 / 1fr 主）
- [x] **顶栏**：折叠按钮 + logo(28px) + 双行品牌文字 + 全局搜索占位 + 通知占位 + 用户头像 dropdown
- [x] **左栏**（可折叠 240→56）：
  - [x] 折叠状态写 `localStorage.sidebarCollapsed`
  - [x] 菜单源自 `AuthStrategyContext.Modules` → 服务端 `__MENU_DATA__` 注入
  - [x] 父子两级递归（基于 `Sys_Module.ParentId`），按 `Sort` 升序
  - [x] hover / active / expanded 三态切换
  - [x] 折叠态仅图标，子菜单隐藏
- [x] **主区**：
  - [x] tab pill 容器（品牌色高亮，多 tab 切换）
  - [x] 固定"首页" tab 不可关闭
  - [x] 点菜单：已开则激活，未开则新建 tab
  - [x] 关闭 tab 自动激活相邻
  - [x] iframe 隔离业务页面
  - [x] 子页面 postMessage 同步标题
- [x] **强化**：Tab **右键菜单**（关闭 / 关闭其他 / 关闭右侧）+ **中键关闭** + `Escape` 关闭菜单
- [x] **强化**：侧栏折叠时父级菜单 `title` 提示（peek）
- [x] **强化**：`init` 同步 `data-theme` 与顶栏/Tab 在暗色下的可读样式（`app.css`）

### P0.4 用户栏（profile dropdown）✓

- [x] 头像（首字母圆形 + 品牌色）+ 姓名 + chevron
- [x] dropdown 五项：
  - [x] 个人资料（弹窗 GET `/Account/Profile`）
  - [x] 修改密码（弹窗 POST `/Account/ChangePassword`；后端 `AuthApp.ChangePassword` 单列 UPDATE，避免 hamaton 表多余字段问题）
  - [x] 切换主题（light↔dark，`data-theme` + localStorage）
  - [x] 退出登录（confirm → `/Account/Logout`）

### P0.5 首页（Dashboard 占位）✓

- [x] `Home/Index.cshtml` 改为 AppShell 入口（`IgnoreBody`）
- [x] `HomeController.Welcome` + `Home/Welcome.cshtml`（iframe 内容）
- [x] Hero 区：动态问候 + 中文日期 + 用户名 + 标语
- [x] 4 张统计卡（数值占位，待 P2 接入）
- [x] 最近活动占位 + 系统信息卡（账号 / 姓名 / 版本 / 阶段 badge）

### P0.6 全局组件库 ✓

CSS 类已全部就位并在 AppShell 内**实际投产**：

- [x] `.btn` + `-primary` / `-default` / `-ghost` / `-danger` / `-sm` / `-lg` / `-icon`
- [x] `.input` / `.select` / `.textarea` / `.input-group` + `.input-icon` + `.input-error`
- [x] `.card` / `.card-header` / `.card-title` / `.card-body`
- [x] `.tbl`（密集列表 + 斑马 + 悬停 + 粘性表头）/ `.tbl-tight`
- [x] `.modal-mask` + `.modal` / `-header` / `-body` / `-footer`
- [x] `.drawer-mask` + `.drawer` / `-header` / `-body` / `-footer`
- [x] `.toast-stack` + `.toast-success / -error / -warning / -info`（`App.success/error/warn/info`）
- [x] `.dropdown` + `.dropdown-item` + `.dropdown-divider` + `.dropdown-item-danger`
- [x] `.badge-brand / -success / -warning / -danger / -info / -default`
- [x] `.spinner`

### P0.7 业务页面（60 个 ViewList）过渡 ✓（兼容方案就位）

- [x] 新建 `_LayoutInner.cshtml`：iframe 内业务视图专用 layout，**暂保留 layui / select2 / inputmask**，60 个 ViewList 零改动即可继续工作
- [x] `_ViewStart.cshtml` 默认 `Layout = "_LayoutInner"`
- [ ] **后续每个业务模块进入 P1 / P2 时**，同步把该模块的列表 + 详情整页迁出 layui，改用 `.tbl` / `.btn` / `.input` / `.drawer` 新组件
- [ ] 最终所有页面迁完后，删除 `wwwroot/js/layui*` / `wwwroot/js/select2/` / `wwwroot/js/inputmask/` / `wwwroot/js/font-awesome/`（约 30MB）

### P0 退出条件 ✓

| 项 | 状态 |
|---|---|
| 编译 | 0 错 0 警 ✓ |
| 登录页可访问 | ✓ |
| AppShell 顶栏 / 侧栏 / tab 渲染 | ✓ |
| 用户菜单 dropdown 五项可用 | ✓ |
| 折叠侧栏 / 切换主题 / 状态持久化 | ✓ |
| 业务页面 iframe 加载（兼容模式） | ✓（layui 继续工作） |

实际用时：**1 个会话**（vs 预期 6-8 工作日）。

---

## P1 保养 / 点检"模板 + 项目库"

> 活字格使用频率最高的部分。表已存在，只缺页面。

### 设备保养

- [ ] PC_设备保养项目列表（`Facility_Item` 筛选 type=保养）
- [ ] PC_设备保养项目明细（编辑 / 新增）
- [ ] PC_设备保养模板列表（`Facility_TheTemplateMain`）
- [ ] PC_设备保养模板明细（主从表，含 `Facility_TheTemplateSub` 项目子表）
- [ ] PC_选择保养人员（弹窗，多选员工）

### 设备点检

- [ ] PC_设备点检项目列表（`Facility_Item` 筛选 type=点检）
- [ ] PC_设备点检项目明细
- [ ] PC_设备点检模板列表
- [ ] PC_设备点检模板明细（主从表）
- [ ] PC_选择点检项目（弹窗）
- [ ] PC_模板导入（Excel 上传）

### 模具保养（同套结构）

- [ ] PC_模具保养项目列表
- [ ] PC_模具保养项目明细
- [ ] PC_模具保养模板列表
- [ ] PC_模具保养模板明细
- [ ] PC_选择保养项目（弹窗）

---

## P2 业务流程（工作流）

> 活字格的派工 / 审核 / 响应 / 完成 / 验收等节点，**目前 0 实现**。

### 设备保养工作流

- [ ] PC_设备保养单列表（`Facility_BillMain`）
- [ ] PC_保养派工列表
- [ ] PC_派工弹窗（选人 + 写期限）
- [ ] PC_保养计划预警列表（到期未做）
- [ ] PC_保养记录（已完成）
- [ ] PC_修改保养计划日期
- [ ] PC_设备保养批量
- [ ] PC_外协保养列表（`Facility_OutsourcingMaintenance`）
- [ ] 外协保养单：打印 / 详情查看 / 详情 / **验收**

### 设备维修工作流

- [ ] PC_设备报修单列表（员工提交）
- [ ] PC_设备报修待审核列表（主管审核）
- [ ] PC_报修单审核（通过 / 驳回）
- [ ] PC_报修单派工列表 → PC_派工
- [ ] PC_报修单子表 / 综合列表 / 详情
- [ ] **维修响应 / 维修完成 / 三方确认** 状态机
- [ ] PC_设备维保人员权限列表 / 详情
- [ ] PC_外协维修列表（`Facility_OutsourcingRepair`）
- [ ] 外协维修单：打印 / 详情查看 / 详情 / **验收**
- [ ] 报修原因选择列表
- [ ] PC_源单备品备件详情（关联领料）

### 设备点检工作流

- [ ] PC_设备点检单列表（`Facility_DATA`）
- [ ] PC_点检记录（`Facility_DATA_History`）
- [ ] PC_设备外检 / PC_设备外检详情
- [ ] PC_设备记录明细
- [ ] PC_设备实时状态查询（基于 `v_Facility_ResourceDetailStatus`）

### 设备台账周边

- [ ] PC_设备内外校台账 / 明细
- [ ] PC_设备台账导入（Excel）
- [ ] PC_设备台账导入（内外校）
- [ ] PC_设备履历（聚合点检 / 维修 / 保养 / 状态历史）
- [ ] PC_设备维修记录 / 明细
- [ ] PC_设备采集信息

### 状态相关

- [ ] PC_设备实时状态看板（占地一屏）

---

## P3 移动端 Web（手机母版页）

> 活字格手机端 40+ 页，**目前 0 实现**。

### P3.1 移动端框架

- [ ] `_LayoutMobile.cshtml`（顶栏 44px + 底部 tabBar 4 项）
- [ ] 移动端首页（九宫格瓷砖入口）
- [ ] 路由根据 UA 自动切换 PC / Mobile（或独立子站 `/m`）

### P3.2 保养管理（手机）

- [ ] 待保养列表 / 待保养详情
- [ ] 扫描保养单号（二维码扫一扫接 H5 BarcodeDetector）
- [ ] 扫描完成保养
- [ ] 保养派工 / 已派工历史记录
- [ ] 保养详细流程页面
- [ ] 保养完成输入结果页面
- [ ] 已保养待审核列表
- [ ] 选择保养人员
- [ ] 查看设备指导书（PDF / 图片预览）
- [ ] 设备保养备注

### P3.3 模具保养（手机）

- [ ] 模具待保养页面 / 扫描模具保养单号
- [ ] 模具保养派工 / 历史
- [ ] 模具保养完成输入
- [ ] 模具保养详细流程
- [ ] 钢网保养 / 钢网点检
- [ ] 选择模具保养人员

### P3.4 设备点检（手机）

- [ ] 设备点检 / 设备点检详情（含拍照上传）

### P3.5 设备维修（手机）

- [ ] 设备报修（含拍照）
- [ ] 员工功能页面 / 管理人员功能页面
- [ ] 扫描维修
- [ ] 维修响应 / 维修完成 / 三方确认
- [ ] 维修派工 / 派工历史
- [ ] 维修记录描述
- [ ] 维修详细流程
- [ ] 选择维修人员

---

## P4 钢网 / 刮刀 / 公共组件 / 服务端命令 / 定时任务

### P4.1 钢网管理

- [ ] PC_钢网列表 / 明细 / 维修 / 导入

### P4.2 刮刀管理

- [ ] PC_刮刀列表 / 维修

### P4.3 备品备件完整版（目前只做了简易列表）

- [ ] PC_备品备件列表 / 详情 / 选择
- [ ] PC_备品备件类别列表
- [ ] PC_备品备件库存查询 / 查询明细
- [ ] PC_备件出入库列表 / 单编辑 / 详情
- [ ] PC_备件源单出库单 / 详情
- [ ] 备品备件入库 / 出库列表
- [ ] 备品备件汇总表 / 进销售日报表明细

### P4.4 OEE 完整版

- [ ] 基于已有 OEE_Rate/Scrap/StopTimes/TotalTimes 做看板
- [ ] rpt_OEE 报表
- [ ] V_Production_BarcodeSMT 联动

### P4.5 公共组件（活字格 `UserControlPages/`）

- [ ] OEE 子控件
- [ ] 抽样规则
- [ ] 不良数据处理
- [ ] 表头插入操作

### P4.6 服务端命令（活字格 `ServerCommands/`）

- [ ] 一键回收
- [ ] 出入库系列命令
- [ ] 库存命令
- [ ] 日计划命令
- [ ] 报告命令
- [ ] 系统操作
- [ ] 设备命令
- [ ] 质量命令
- [ ] 修复数据
- [ ] 原材料表清理
- [ ] 获取人员 Id

### P4.7 定时任务（迁移 Hangfire）

- [ ] OA 催办
- [ ] 催办通知 / 自动作废订单
- [ ] 点检设备月报表
- [ ] 维修自动审批

### P4.8 系统管理（admin 用）

- [ ] 角色管理（含模块/按钮授权）
- [ ] 用户管理
- [ ] 部门管理
- [ ] 模块管理（菜单维护）
- [ ] 按钮维护

---

## 跨阶段规范

### 安全 / 协作规范

- 服务器 SSH 访问严格禁止；所有 SQL / 命令由用户手工执行
- 数据库连接 / 密码 / 密钥不进公开仓
- 不擅自创建测试脚本 / 辅助脚本
- 不创建 Markdown 文档（**本 ROADMAP 是用户明确要求的例外**）

### 命名 / 编码规范

- 脚本输出统一英文（防跨平台编码）
- 中文允许在 UI、注释、提交信息
- Docker 命令统一 `docker compose`（V2 语法）
- 不主动修改代码，除非用户明确要求

### Git 规范

- 公开仓库：<https://github.com/mli2025/TPM-layui>
- 主分支：`main`
- 提交信息：英文，规范化前缀 `feat / fix / chore / refactor / docs`

---

## 下一步

P0 框架壳已规划清楚，**等用户确认后立刻动手**：

1. P0.1 全局基础设施（引入 Tailwind / Alpine / Lucide / Inter / logo）
2. P0.2 登录页
3. P0.3 AppShell（顶栏 + 侧栏 + tab 容器）
4. P0.4 用户栏 dropdown
5. P0.5 首页 Dashboard 占位
6. P0.6 组件库（Button/Table/Modal/Drawer/Toast/...）
7. P0.7 60 个 ViewList 适配新表格

预计 6-8 个工作日交付 P0。
