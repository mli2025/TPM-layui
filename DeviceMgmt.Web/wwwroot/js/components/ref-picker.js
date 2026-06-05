/**
 * 参照选择器：部门树 / 生产资源（放大镜）
 * 统一使用 layui layer 弹层 + 标准 layui tree / table，风格与系统一致。
 * 依赖：layui layer / tree / table / form
 */
(function (global) {
    'use strict';

    function ok(res) { return res && (res.code === 0 || res.code === 200); }

    function escapeHtml(s) {
        return (s == null ? '' : String(s)).replace(/[&<>"]/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c];
        });
    }

    // 手写 HTML 部门树（与"部门管理"页一致，确保稳定显示）
    function buildDeptTreeHtml(flat) {
        var list = (flat || []).map(function (d) {
            return {
                id: String(d.Id != null ? d.Id : d.id),
                parentId: String((d.ParentId != null && d.ParentId !== '') ? d.ParentId : (d.parentId != null ? d.parentId : '0')),
                deptName: d.DeptName || d.deptName || '',
                deptNumber: d.DeptNumber || d.deptNumber || ''
            };
        });
        var byParent = {};
        list.forEach(function (d) { (byParent[d.parentId] = byParent[d.parentId] || []).push(d); });

        function nodeHtml(d, depth) {
            var children = byParent[d.id] || [];
            var indent = (depth * 18) + 'px';
            var label = escapeHtml(d.deptName) + (d.deptNumber ? ' <span style="color:#94a3b8;font-size:12px;">' + escapeHtml(d.deptNumber) + '</span>' : '');
            var html = '<div class="ref-dept-node" data-id="' + d.id + '" data-name="' + escapeHtml(d.deptName) + '"'
                + ' style="padding:7px 8px;padding-left:' + indent + ';border-bottom:1px dashed #eee;cursor:pointer;">'
                + label + '</div>';
            children.forEach(function (c) { html += nodeHtml(c, depth + 1); });
            return html;
        }
        var top = byParent['0'] || [];
        // 若没有 parentId=0 的根（数据异常），则把所有节点平铺，避免空白
        if (!top.length) top = list;
        return top.map(function (n) { return nodeHtml(n, 0); }).join('')
            || '<div style="color:#999;text-align:center;padding:30px;">暂无部门数据</div>';
    }

    function mountDeptPicker($root, opts) {
        opts = opts || {};
        var $hid = $root.find('input[name="DeptId"]');
        var $disp = $root.find('.ref-dept-display');
        var $btn = $root.find('.ref-dept-btn');
        if (!$hid.length || !$disp.length) return;

        function setVal(id, name) {
            $hid.val(id || '');
            $disp.val(name || '');
        }

        function openPicker() {
            if ($disp.is('[disabled]')) return;
            layui.use(['layer'], function () {
                var layer = layui.layer;
                $.get(opts.deptUrl || '/Sys_Dept/List', function (res) {
                    if (!ok(res)) { layer.msg((res && res.msg) || '部门加载失败', { icon: 2 }); return; }
                    var wrapId = 'refDeptTree_' + new Date().getTime();
                    var treeHtml = buildDeptTreeHtml(res.data || []);
                    var html = '<div style="padding:8px 4px;">'
                        + '<div id="' + wrapId + '" style="max-height:400px;overflow:auto;border:1px solid #eee;border-radius:2px;">'
                        + treeHtml + '</div></div>';
                    var selected = null;
                    var pickerIdx = layer.open({
                        type: 1, title: '选择部门', area: ['420px', '520px'],
                        content: html, shadeClose: false,
                        btn: ['确定', '清空'],
                        success: function () {
                            var $box = $('#' + wrapId);
                            $box.on('click', '.ref-dept-node', function () {
                                selected = { id: $(this).data('id'), name: $(this).data('name') };
                                $box.find('.ref-dept-node').css({ background: '', color: '' });
                                $(this).css({ background: '#5FB878', color: '#fff' });
                            });
                            $box.on('dblclick', '.ref-dept-node', function () {
                                setVal($(this).data('id'), $(this).data('name'));
                                layer.close(pickerIdx);
                            });
                        },
                        yes: function (idx) {
                            if (!selected) { layer.msg('请选择部门', { icon: 0 }); return; }
                            setVal(selected.id, selected.name);
                            layer.close(idx);
                        },
                        btn2: function (idx) { setVal('', ''); layer.close(idx); return false; }
                    });
                });
            });
        }

        $btn.off('click.refDept').on('click.refDept', openPicker);
        $disp.off('click.refDept').on('click.refDept', openPicker);

        var dv = (opts.value != null && opts.value !== '') ? opts.value : ($hid.val() || '');
        if (dv) {
            var name = opts.text || '';
            if (!name && opts.deptMap) name = opts.deptMap[String(dv)] || '';
            if (name) { setVal(dv, name); }
            else {
                $hid.val(dv);
                $.get(opts.deptUrl || '/Sys_Dept/List', function (res) {
                    var arr = (ok(res) && res.data) || [], hit = null;
                    for (var i = 0; i < arr.length; i++) {
                        var rid = arr[i].Id != null ? arr[i].Id : arr[i].id;
                        if (String(rid) === String(dv)) { hit = arr[i]; break; }
                    }
                    $disp.val(hit ? (hit.DeptName || hit.deptName || ('ID:' + dv)) : ('ID:' + dv));
                }).fail(function () { $disp.val('ID:' + dv); });
            }
        }
    }

    function mountResourcePicker($root, opts) {
        opts = opts || {};
        var $hid = $root.find('input[name="ResourceId"]');
        var $disp = $root.find('.ref-resource-display');
        var $btn = $root.find('.ref-resource-btn');
        if (!$hid.length || !$disp.length) return;
        var url = opts.resourceUrl || '/Basic_Resource/GetPickerList';

        function setVal(id, text) {
            $hid.val(id || '');
            $disp.val(text || '');
        }

        function openPicker() {
            if ($disp.prop('readonly') && $disp.is('[disabled]')) return;
            layui.use(['layer', 'table'], function () {
                var layer = layui.layer, table = layui.table;
                var tableId = 'refResTable_' + new Date().getTime();
                var html = '<div style="padding:10px 14px;">'
                    + '<div class="layui-form" style="margin-bottom:8px;">'
                    + '<div class="layui-input-inline" style="width:200px;">'
                    + '<input type="text" id="' + tableId + '_kw" class="layui-input" placeholder="按编码/名称搜索" autocomplete="off" />'
                    + '</div>'
                    + '<button type="button" class="layui-btn layui-btn-sm" id="' + tableId + '_search"><i class="layui-icon layui-icon-search"></i> 搜索</button>'
                    + '</div>'
                    + '<table id="' + tableId + '" lay-filter="' + tableId + '"></table>'
                    + '</div>';
                var selected = null;
                var pickerIdx = layer.open({
                    type: 1, title: '选择生产资源', area: ['620px', '560px'],
                    content: html, shadeClose: false,
                    btn: ['确定', '清空'],
                    success: function () {
                        function renderTable(kw) {
                            table.render({
                                elem: '#' + tableId,
                                url: url,
                                method: 'post',
                                where: { query: kw || '' },
                                page: true,
                                limit: 10,
                                height: 380,
                                cols: [[
                                    { type: 'radio', width: 50 },
                                    { field: 'Code', title: '编码', width: 160 },
                                    { field: 'Name', title: '名称', minWidth: 200 }
                                ]],
                                parseData: function (res) {
                                    if (res && res.code === 200) res.code = 0;
                                    return res;
                                },
                                text: { none: '暂无生产资源数据（可在 MES 中维护，或留空）' }
                            });
                        }
                        renderTable('');
                        table.on('radio(' + tableId + ')', function (obj) { selected = obj.data; });
                        table.on('rowDouble(' + tableId + ')', function (obj) {
                            selected = obj.data;
                            setVal(selected.Id, (selected.Code || '') + ' / ' + (selected.Name || ''));
                            layer.close(pickerIdx);
                        });
                        $('#' + tableId + '_search').on('click', function () {
                            renderTable($('#' + tableId + '_kw').val());
                        });
                        $('#' + tableId + '_kw').on('keydown', function (e) {
                            if (e.keyCode === 13) { renderTable($(this).val()); return false; }
                        });
                    },
                    yes: function (idx) {
                        if (!selected) { layer.msg('请选择一条生产资源', { icon: 0 }); return; }
                        setVal(selected.Id, (selected.Code || '') + ' / ' + (selected.Name || ''));
                        layer.close(idx);
                    },
                    btn2: function (idx) { setVal('', ''); layer.close(idx); return false; }
                });
            });
        }

        $btn.off('click.refRes').on('click.refRes', openPicker);
        $disp.off('click.refRes').on('click.refRes', openPicker);

        if (opts.value && opts.text) {
            setVal(opts.value, opts.text);
        } else if (opts.value) {
            $hid.val(opts.value);
            $.post(url, { page: 1, limit: 1, Id: opts.value }, function (res) {
                if (ok(res) && res.data && res.data[0]) {
                    var r = res.data[0];
                    $disp.val((r.Code || '') + ' / ' + (r.Name || ''));
                } else {
                    $disp.val('ID:' + opts.value);
                }
            }).fail(function () { $disp.val('ID:' + opts.value); });
        }
    }

    // 通用设备放大镜：在 $wrap 内需含 input[type=hidden].(hiddenName) + .disp(显示框) + .btn(图标)
    function mountDevicePicker(opts) {
        opts = opts || {};
        var $hid = opts.$hidden;      // 隐藏域(存 FacilityId)
        var $disp = opts.$display;    // 只读显示框
        var $btn = opts.$btn;         // 放大镜图标
        var url = opts.url || '/Facility_RepairBillMain/GetDevicePickerList';
        if (!$hid || !$hid.length || !$disp || !$disp.length) return;

        function setVal(id, text) { $hid.val(id || ''); $disp.val(text || ''); }

        function openPicker() {
            if ($disp.is('[disabled]')) return;
            layui.use(['layer', 'table'], function () {
                var layer = layui.layer, table = layui.table;
                var tid = 'devPick_' + new Date().getTime();
                var html = '<div style="padding:10px 14px;">'
                    + '<div class="layui-form" style="margin-bottom:8px;">'
                    + '<div class="layui-input-inline" style="width:220px;">'
                    + '<input type="text" id="' + tid + '_kw" class="layui-input" placeholder="按编码/名称/型号搜索" autocomplete="off" /></div>'
                    + '<button type="button" class="layui-btn layui-btn-sm" id="' + tid + '_search"><i class="layui-icon layui-icon-search"></i> 搜索</button>'
                    + '</div><table id="' + tid + '" lay-filter="' + tid + '"></table></div>';
                var selected = null;
                var pickerIdx = layer.open({
                    type: 1, title: '选择设备', area: ['680px', '580px'], content: html, shadeClose: false,
                    btn: ['确定', '取消'],
                    success: function () {
                        function renderTable(kw) {
                            table.render({
                                elem: '#' + tid, url: url, method: 'post',
                                where: { query: kw || '' }, page: true, limit: 10, height: 400,
                                cols: [[
                                    { type: 'radio', width: 50 },
                                    { field: 'FacilityCode', title: '设备编码', width: 150 },
                                    { field: 'FacilityName', title: '设备名称', minWidth: 180 },
                                    { field: 'Model', title: '型号', width: 130 }
                                ]],
                                parseData: function (res) { if (res && res.code === 200) res.code = 0; return res; },
                                text: { none: '暂无设备数据' }
                            });
                        }
                        renderTable('');
                        table.on('radio(' + tid + ')', function (obj) { selected = obj.data; });
                        table.on('rowDouble(' + tid + ')', function (obj) {
                            selected = obj.data;
                            setVal(selected.Id, (selected.FacilityName || '') + ' (' + (selected.FacilityCode || '') + ')');
                            layer.close(pickerIdx);
                        });
                        $('#' + tid + '_search').on('click', function () { renderTable($('#' + tid + '_kw').val()); });
                        $('#' + tid + '_kw').on('keydown', function (e) { if (e.keyCode === 13) { renderTable($(this).val()); return false; } });
                    },
                    yes: function (idx) {
                        if (!selected) { layer.msg('请选择一台设备', { icon: 0 }); return; }
                        setVal(selected.Id, (selected.FacilityName || '') + ' (' + (selected.FacilityCode || '') + ')');
                        layer.close(idx);
                    }
                });
            });
        }

        $btn && $btn.off('click.devPick').on('click.devPick', openPicker);
        $disp.off('click.devPick').on('click.devPick', openPicker);

        if (opts.value && opts.text) {
            setVal(opts.value, opts.text);
        } else if (opts.value) {
            $hid.val(opts.value);
            $.post(url, { page: 1, limit: 1, Id: opts.value }, function (res) {
                if (ok(res) && res.data && res.data[0]) {
                    var r = res.data[0];
                    $disp.val((r.FacilityName || '') + ' (' + (r.FacilityCode || '') + ')');
                } else { $disp.val('ID:' + opts.value); }
            }).fail(function () { $disp.val('ID:' + opts.value); });
        }
    }

    // 设备字段统一约定：容器含 .ref-dev-wrap，内部含 input[type=hidden] + .ref-dev-display + .ref-dev-btn
    // 用法：HTML 用 RefPicker.deviceFieldHtml(name,label) 生成；弹层 success 后调用 RefPicker.autoMount(layero)
    function deviceFieldHtml(name, label, required) {
        return '<div class="layui-inline ref-dev-wrap">'
            + '<label class="layui-form-label">' + escapeHtml(label || '设备') + (required ? ' *' : '') + '</label>'
            + '<div class="layui-input-inline" style="position:relative;">'
            + '<input type="hidden" name="' + escapeHtml(name || 'FacilityId') + '">'
            + '<input type="text" readonly class="layui-input ref-dev-display" placeholder="点击选择设备"'
            + ' style="cursor:pointer;padding-right:34px;background:#fff;">'
            + '<i class="layui-icon layui-icon-search ref-dev-btn" title="选择设备"'
            + ' style="position:absolute;right:1px;top:1px;width:32px;height:36px;line-height:36px;text-align:center;cursor:pointer;color:#5FB878;"></i>'
            + '</div></div>';
    }

    function mountDeviceWrap($wrap, opts) {
        opts = opts || {};
        var $hid = $wrap.find('input[type=hidden]');
        mountDevicePicker({
            $hidden: $hid,
            $display: $wrap.find('.ref-dev-display'),
            $btn: $wrap.find('.ref-dev-btn'),
            url: opts.url,
            value: (opts.value != null && opts.value !== '') ? opts.value : ($hid.val() || ''),
            text: opts.text
        });
    }

    // 通用表格放大镜：一次性拉取列表（GetMainList），客户端关键字过滤
    // opts: $hidden,$display,$btn,title,url,cols(数据列),idField,textFormat(row),searchFields,emptyText,area,allowClear,value,text
    function mountTablePicker(opts) {
        opts = opts || {};
        var $hid = opts.$hidden, $disp = opts.$display, $btn = opts.$btn;
        if (!$hid || !$hid.length || !$disp || !$disp.length) return;
        var idField = opts.idField || 'Id';
        var fmt = opts.textFormat || function (r) { return r[idField]; };
        var sf = opts.searchFields || [];

        function setVal(id, text) { $hid.val(id || ''); $disp.val(text || ''); }
        function picked(row) { if (opts.onPicked && row) { try { opts.onPicked(row); } catch (e) { } } }

        function openPicker() {
            if ($disp.is('[disabled]')) return;
            layui.use(['layer', 'table'], function () {
                var layer = layui.layer, table = layui.table;
                var tid = 'pk_' + new Date().getTime();
                var html = '<div style="padding:10px 14px;"><div class="layui-form" style="margin-bottom:8px;">'
                    + '<div class="layui-input-inline" style="width:240px;"><input type="text" id="' + tid + '_kw" class="layui-input" placeholder="输入关键字搜索" autocomplete="off"></div>'
                    + '<button type="button" class="layui-btn layui-btn-sm" id="' + tid + '_search"><i class="layui-icon layui-icon-search"></i> 搜索</button>'
                    + '</div><table id="' + tid + '" lay-filter="' + tid + '"></table></div>';
                var all = [], selected = null;
                var pickerIdx = layer.open({
                    type: 1, title: opts.title || '选择', area: opts.area || ['640px', '560px'],
                    content: html, shadeClose: false,
                    btn: ['确定', opts.allowClear ? '清空' : '取消'],
                    success: function () {
                        var cols = [[{ type: 'radio', width: 50 }].concat(opts.cols || [])];
                        function renderTable(rows) {
                            table.render({ elem: '#' + tid, data: rows || [], page: true, limit: 10, height: 400, cols: cols, text: { none: opts.emptyText || '暂无数据' } });
                        }
                        function doFilter(kw) {
                            kw = (kw || '').toLowerCase();
                            if (!kw) return all;
                            return all.filter(function (r) {
                                for (var i = 0; i < sf.length; i++) {
                                    if (String(r[sf[i]] == null ? '' : r[sf[i]]).toLowerCase().indexOf(kw) >= 0) return true;
                                }
                                return false;
                            });
                        }
                        $.post(opts.url, { page: 1, limit: 9999 }, function (res) { all = (res && res.data) || []; renderTable(all); }).fail(function () { renderTable([]); });
                        table.on('radio(' + tid + ')', function (obj) { selected = obj.data; });
                        table.on('rowDouble(' + tid + ')', function (obj) { selected = obj.data; setVal(selected[idField], fmt(selected)); picked(selected); layer.close(pickerIdx); });
                        $('#' + tid + '_search').on('click', function () { renderTable(doFilter($('#' + tid + '_kw').val())); });
                        $('#' + tid + '_kw').on('keydown', function (e) { if (e.keyCode === 13) { renderTable(doFilter($(this).val())); return false; } });
                    },
                    yes: function (idx) { if (!selected) { layer.msg('请选择一条', { icon: 0 }); return; } setVal(selected[idField], fmt(selected)); picked(selected); layer.close(idx); },
                    btn2: function (idx) { if (opts.allowClear) setVal('', ''); layer.close(idx); return false; }
                });
            });
        }

        $btn && $btn.off('click.pk').on('click.pk', openPicker);
        $disp.off('click.pk').on('click.pk', openPicker);

        var v = (opts.value != null && opts.value !== '') ? opts.value : ($hid.val() || '');
        if (v && opts.text) { setVal(v, opts.text); }
        else if (v) {
            $hid.val(v);
            $.post(opts.url, { page: 1, limit: 9999 }, function (res) {
                var arr = (res && res.data) || [], hit = null;
                for (var i = 0; i < arr.length; i++) { if (String(arr[i][idField]) === String(v)) { hit = arr[i]; break; } }
                $disp.val(hit ? fmt(hit) : ('ID:' + v));
            }).fail(function () { $disp.val('ID:' + v); });
        }
    }

    // 预置：备件 / 员工 / 维修工单
    function spareOpts($wrap, opts) {
        opts = opts || {};
        return {
            $hidden: $wrap.find('input[type=hidden]'), $display: $wrap.find('.ref-pk-display'), $btn: $wrap.find('.ref-pk-btn'),
            title: '选择备件', url: opts.url || '/Basic_Spare/GetMainList',
            cols: [{ field: 'Code', title: '编码', width: 150 }, { field: 'Name', title: '名称', minWidth: 180 }, { field: 'Specs', title: '规格', width: 140 }],
            searchFields: ['Code', 'Name', 'Specs'],
            textFormat: function (r) { return (r.Name || '') + (r.Code ? ' (' + r.Code + ')' : ''); },
            emptyText: '暂无备件数据', value: opts.value, text: opts.text, allowClear: opts.allowClear
        };
    }
    function empOpts($wrap, opts) {
        opts = opts || {};
        return {
            $hidden: $wrap.find('input[type=hidden]'), $display: $wrap.find('.ref-pk-display'), $btn: $wrap.find('.ref-pk-btn'),
            title: '选择员工', url: opts.url || '/Basic_Employee/GetMainList',
            cols: [{ field: 'EmployeeNumber', title: '员工号', width: 140 }, { field: 'Name', title: '姓名', width: 140 }, { field: 'DeptName', title: '部门', minWidth: 140 }],
            searchFields: ['EmployeeNumber', 'Name', 'DeptName'],
            textFormat: function (r) { return (r.Name || '') + (r.EmployeeNumber ? ' (' + r.EmployeeNumber + ')' : ''); },
            idField: opts.idField || 'Id', emptyText: '暂无员工数据', value: opts.value, text: opts.text, allowClear: opts.allowClear
        };
    }
    function whOpts($wrap, opts) {
        opts = opts || {};
        return {
            $hidden: $wrap.find('input[type=hidden]'), $display: $wrap.find('.ref-pk-display'), $btn: $wrap.find('.ref-pk-btn'),
            title: '选择仓库', url: opts.url || '/Basic_Warehouse/GetMainList',
            cols: [{ field: 'Code', title: '编码', width: 150 }, { field: 'Name', title: '名称', minWidth: 180 }, { field: 'Location', title: '位置', width: 160 }],
            searchFields: ['Code', 'Name', 'Location'],
            textFormat: function (r) { return (r.Name || '') + (r.Code ? ' (' + r.Code + ')' : ''); },
            emptyText: '暂无仓库数据', value: opts.value, text: opts.text, allowClear: opts.allowClear
        };
    }
    function billOpts($wrap, opts) {
        opts = opts || {};
        return {
            $hidden: $wrap.find('input[type=hidden]'), $display: $wrap.find('.ref-pk-display'), $btn: $wrap.find('.ref-pk-btn'),
            title: '选择维修工单', url: opts.url || '/Facility_RepairBillMain/GetMainList', area: ['720px', '560px'],
            cols: [{ field: 'BillNo', title: '单号', width: 180 }, { field: 'FaultDesc', title: '故障描述', minWidth: 220 }, { field: 'Status', title: '状态', width: 100 }],
            searchFields: ['BillNo', 'FaultDesc'],
            textFormat: function (r) { return (r.BillNo || ('单#' + r.Id)); },
            emptyText: '暂无维修工单', value: opts.value, text: opts.text, allowClear: opts.allowClear
        };
    }

    // 通用字段 HTML（隐藏域 + 只读显示框 + 放大镜图标），type: spare/emp/bill
    function pickerFieldHtml(name, label, required) {
        return '<input type="hidden" name="' + escapeHtml(name) + '">'
            + '<input type="text" readonly class="layui-input ref-pk-display" placeholder="点击选择' + (required ? '' : '(可选)') + '" style="cursor:pointer;padding-right:34px;background:#fff;">'
            + '<i class="layui-icon layui-icon-search ref-pk-btn" style="position:absolute;right:1px;top:1px;width:32px;height:36px;line-height:36px;text-align:center;cursor:pointer;color:#5FB878;"></i>';
    }

    // 扫描作用域内的参照字段自动挂载放大镜（设备/备件/员工/工单），读取隐藏域已有值用于编辑回显
    function autoMount($scope, opts) {
        opts = opts || {};
        $scope.find('.ref-dev-wrap').each(function () {
            var $w = $(this);
            if ($w.data('refMounted')) return;
            $w.data('refMounted', 1);
            mountDeviceWrap($w, { url: opts.deviceUrl, value: $w.attr('data-value') || '', text: $w.attr('data-text') || '' });
        });
        $scope.find('.ref-spare-wrap').each(function () {
            var $w = $(this); if ($w.data('refMounted')) return; $w.data('refMounted', 1);
            mountTablePicker(spareOpts($w, { value: $w.attr('data-value') || '', text: $w.attr('data-text') || '', allowClear: $w.attr('data-clear') === '1' }));
        });
        $scope.find('.ref-emp-wrap').each(function () {
            var $w = $(this); if ($w.data('refMounted')) return; $w.data('refMounted', 1);
            mountTablePicker(empOpts($w, { value: $w.attr('data-value') || '', text: $w.attr('data-text') || '', allowClear: $w.attr('data-clear') === '1' }));
        });
        $scope.find('.ref-bill-wrap').each(function () {
            var $w = $(this); if ($w.data('refMounted')) return; $w.data('refMounted', 1);
            mountTablePicker(billOpts($w, { value: $w.attr('data-value') || '', text: $w.attr('data-text') || '', allowClear: $w.attr('data-clear') === '1' }));
        });
        $scope.find('.ref-wh-wrap').each(function () {
            var $w = $(this); if ($w.data('refMounted')) return; $w.data('refMounted', 1);
            mountTablePicker(whOpts($w, { value: $w.attr('data-value') || '', text: $w.attr('data-text') || '', allowClear: $w.attr('data-clear') === '1' }));
        });
        $scope.find('.ref-dept-wrap').each(function () {
            var $w = $(this); if ($w.data('refMounted')) return; $w.data('refMounted', 1);
            mountDeptPicker($w, { value: $w.attr('data-value') || '', text: $w.attr('data-text') || '' });
        });
    }

    function initFacilityFormPickers($form, entity, deptMap) {
        mountDeptPicker($form.find('.ref-dept-wrap'), {
            value: entity && entity.DeptId,
            text: entity && entity.DeptName,
            deptMap: deptMap
        });
        mountResourcePicker($form.find('.ref-resource-wrap'), {
            value: entity && entity.ResourceId,
            text: entity && entity.ResourceDisplay
        });
        autoMount($form);
    }

    global.RefPicker = {
        mountDeptPicker: mountDeptPicker,
        mountResourcePicker: mountResourcePicker,
        mountDevicePicker: mountDevicePicker,
        mountTablePicker: mountTablePicker,
        deviceFieldHtml: deviceFieldHtml,
        pickerFieldHtml: pickerFieldHtml,
        mountDeviceWrap: mountDeviceWrap,
        autoMount: autoMount,
        initFacilityFormPickers: initFacilityFormPickers
    };
})(window);
