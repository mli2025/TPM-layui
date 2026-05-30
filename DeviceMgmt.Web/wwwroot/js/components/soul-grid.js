/**
 * SoulGrid —— layui-table + soul-table 的统一封装
 *
 * 在这次"设备台账"验证过的配置基础上抽出公共组件，让各列表页用极少代码即可获得：
 *   - 表头去重下拉筛选(data) + 条件筛选(condition)
 *   - 列显示/隐藏（顶部齿轮，layui 原生）
 *   - 拖拽调列序、固定列、点列头排序
 *   - 前端导出（所见即所得：按当前筛选/排序/列序导出）
 *
 * 依赖：布局已全局注册 soulTable 模块并加载 soulTable.css。
 *
 * 用法（前端模式，适合主数据/档案表，数据量可控）：
 *   SoulGrid.loadAll(URL_LIST, where, function(rows){ decorate(rows); SoulGrid.render({ ... }); });
 * 用法（服务端分页模式，适合大数据表；去重下拉仅覆盖当前页）：
 *   SoulGrid.render({ elem:'#g', id:'g', url:URL_LIST, method:'post', where:where, cols:[[...]] });
 */
(function (window) {
    'use strict';

    function ok(res) { return res && (res.code === 0 || res.code === 200); }

    var SoulGrid = {
        // 默认配置（可被 render 的 opts 覆盖）
        defaults: function () {
            return {
                page: true,
                limit: 20,
                limits: [20, 50, 100, 200],
                even: true,
                drag: true,
                autoColumnWidth: false,
                toolbar: true,                       // 顶部工具栏（承载列显隐齿轮）
                defaultToolbar: ['filter'],          // filter = 列显示/隐藏面板
                // cache:false 关闭列布局本地缓存，避免列结构变更后 updateCols 崩溃
                filter: { cache: false, bottom: false, items: ['data', 'condition'] }
            };
        },

        /**
         * 渲染表格
         * @param {Object} opts layui table.render 的配置 + 可选 filename(导出文件名)
         * @returns 无（异步在 layui.use 内渲染）
         */
        render: function (opts) {
            opts = opts || {};
            layui.use(['table', 'soulTable'], function () {
                var table = layui.table, soulTable = layui.soulTable;
                var cfg = $.extend(true, SoulGrid.defaults(), opts);

                // 导出文件名
                cfg.excel = $.extend({ filename: (opts.filename || '导出数据') + '.xlsx' }, opts.excel || {});

                // 包裹 done：先渲染 soul-table，再执行调用方的 done
                var userDone = opts.done;
                cfg.done = function (res, curr, count) {
                    try { soulTable.render(this); } catch (e) { console.error('soulTable render error', e); }
                    if (typeof userDone === 'function') userDone.call(this, res, curr, count);
                };

                table.render(cfg);
            });
        },

        /** 导出当前"筛选后"的表格（绑定到导出按钮） */
        export: function (id) {
            layui.use(['soulTable'], function () {
                try { layui.soulTable.export(id); }
                catch (e) { layui.layer && layui.layer.msg('导出失败：' + (e && e.message || e), { icon: 2 }); }
            });
        },

        /**
         * 前端模式：一次性取回全部匹配行（去重下拉才能列出全部值）
         * @param {String} url 列表接口
         * @param {Object} where 查询条件（page/limit/searchParam）
         * @param {Function} cb  function(rows){ ... }
         * @param {Object} [ajaxOpt] 额外 ajax 配置（如 method）
         */
        loadAll: function (url, where, cb, ajaxOpt) {
            var w = $.extend({ page: 1, limit: 100000, searchParam: [] }, where || {});
            var loadIdx = layui.layer ? layui.layer.load(2) : 0;
            $.ajax($.extend({
                url: url, type: 'post', data: w, dataType: 'json'
            }, ajaxOpt || {})).done(function (res) {
                if (loadIdx) layui.layer.close(loadIdx);
                cb(ok(res) ? (res.data || []) : []);
            }).fail(function () {
                if (loadIdx) layui.layer.close(loadIdx);
                layui.layer && layui.layer.msg('数据加载失败', { icon: 2 });
                cb([]);
            });
        },

        /** 千分位：fmtThousand(12345.6, 2) => "12,345.60" */
        fmtThousand: function (v, dec) {
            if (v == null || v === '') return '';
            var n = Number(v); if (isNaN(n)) return v;
            return n.toLocaleString('en-US', { minimumFractionDigits: dec || 0, maximumFractionDigits: dec || 0 });
        },

        /** 日期格式：yyyy-MM-dd */
        fmtDate: function (s) { return s ? (s + '').replace('T', ' ').substring(0, 10) : ''; },
        /** 日期时间格式：yyyy-MM-dd HH:mm:ss */
        fmtDt: function (s) { return s ? (s + '').replace('T', ' ').substring(0, 19) : ''; }
    };

    window.SoulGrid = SoulGrid;
})(window);
