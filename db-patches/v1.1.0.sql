/* =====================================================================
 * TPM v2 增量补丁：v1.0.0 -> v1.1.0
 *   只插入版本记录、切换 IsCurrent 标记，不修改任何表结构。
 *   在 TPM 数据库下执行：
 *     USE [TPM]; GO   (或你实际部署的数据库名)
 *     然后整段执行本文件
 * ===================================================================== */

USE [TPM];
GO

/* 1) 先把当前所有「当前版本」标记清空，保证 IX_Sys_Version_Current 唯一约束不冲突 */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO

/* 2) 若已存在 v1.1.0（重复执行时）则更新内容并置为当前；否则插入新行 */
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v1.1.0')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]       = N'保养工单全生命周期 + 附件组件兼容性修复',
           [Content]     =
              N'## 新增功能' + CHAR(10) +
              N'- 保养派工：列表行级「派工」按钮 + 顶部「批量派工」（状态=新建才允许），员工列表带部门 + 当前待办负载着色' + CHAR(10) +
              N'- 状态流转：派工(0→1) → 开始保养(1→2) → 完工(2→3) → 审核通过(3→4)，每步都有状态守卫' + CHAR(10) +
              N'- 查看视图按状态自动渲染对应动作按钮（开始保养 / 完工填结果 / 审核），无需切页面' + CHAR(10) +
              N'- 完工时收集「保养结果（正常/异常）」+「完工备注」，自动写 EndDate / LastMaintainTime / IsOK / 备注追加' + CHAR(10) +
              N'- 保养人列从工号显示升级为「姓名 (工号)」，从 Basic_Employee 实时映射' + CHAR(10) +
              N'## Bug 修复' + CHAR(10) +
              N'- BatchGenerate 不再让 BeginDate/EndDate = BillDate 占位，按周期自动生成执行窗口：' + CHAR(10) +
              N'  * WEEK   = 周一 00:00 ~ 周日 23:59:59' + CHAR(10) +
              N'  * MONTH  = 月初 00:00 ~ 月末 23:59:59' + CHAR(10) +
              N'  * QUARTER= 季初 00:00 ~ 季末 23:59:59' + CHAR(10) +
              N'  * YEAR   = 1.1 00:00 ~ 12.31 23:59:59' + CHAR(10) +
              N'- 看板 / 日历 / 甘特点击单据进入查看视图修复（ID 类型不匹配导致点击无响应）' + CHAR(10) +
              N'- 通用附件组件「上传附件」按钮无响应修复：内部自调 layui.use([upload, layer])，' + CHAR(10) +
              N'  不再依赖业务页面 layui.use 列表是否包含 upload；上传增加 loading 反馈与失败提示' + CHAR(10) +
              N'## 体验增强' + CHAR(10) +
              N'- 甘特图横条按 [BeginDate, EndDate] 跨格渲染 + 同设备多单 lane stacking，不再只显示在 BillDate 单格内',
           [IsCurrent]   = 1,
           [Author]      = N'arbore'
     WHERE [Version] = 'v1.1.0';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version], [ReleaseDate], [Title], [Content], [IsCurrent], [Author])
    VALUES
     ('v1.1.0', getdate(),
      N'保养工单全生命周期 + 附件组件兼容性修复',
      N'## 新增功能' + CHAR(10) +
      N'- 保养派工：列表行级「派工」按钮 + 顶部「批量派工」（状态=新建才允许），员工列表带部门 + 当前待办负载着色' + CHAR(10) +
      N'- 状态流转：派工(0→1) → 开始保养(1→2) → 完工(2→3) → 审核通过(3→4)，每步都有状态守卫' + CHAR(10) +
      N'- 查看视图按状态自动渲染对应动作按钮（开始保养 / 完工填结果 / 审核），无需切页面' + CHAR(10) +
      N'- 完工时收集「保养结果（正常/异常）」+「完工备注」，自动写 EndDate / LastMaintainTime / IsOK / 备注追加' + CHAR(10) +
      N'- 保养人列从工号显示升级为「姓名 (工号)」，从 Basic_Employee 实时映射' + CHAR(10) +
      N'## Bug 修复' + CHAR(10) +
      N'- BatchGenerate 不再让 BeginDate/EndDate = BillDate 占位，按周期自动生成执行窗口：' + CHAR(10) +
      N'  * WEEK   = 周一 00:00 ~ 周日 23:59:59' + CHAR(10) +
      N'  * MONTH  = 月初 00:00 ~ 月末 23:59:59' + CHAR(10) +
      N'  * QUARTER= 季初 00:00 ~ 季末 23:59:59' + CHAR(10) +
      N'  * YEAR   = 1.1 00:00 ~ 12.31 23:59:59' + CHAR(10) +
      N'- 看板 / 日历 / 甘特点击单据进入查看视图修复（ID 类型不匹配导致点击无响应）' + CHAR(10) +
      N'- 通用附件组件「上传附件」按钮无响应修复：内部自调 layui.use([upload, layer])，' + CHAR(10) +
      N'  不再依赖业务页面 layui.use 列表是否包含 upload；上传增加 loading 反馈与失败提示' + CHAR(10) +
      N'## 体验增强' + CHAR(10) +
      N'- 甘特图横条按 [BeginDate, EndDate] 跨格渲染 + 同设备多单 lane stacking，不再只显示在 BillDate 单格内',
      1, N'arbore');
END
GO

PRINT '==== Patch v1.1.0 applied ====';
SELECT [Version], [ReleaseDate], [Title], [IsCurrent], [Author]
  FROM [Sys_Version] ORDER BY [ReleaseDate] DESC;
GO
