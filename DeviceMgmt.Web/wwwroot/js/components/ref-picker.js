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
                    layer.open({
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
                                layer.closeAll();
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

        if (opts.value) {
            var name = opts.text || '';
            if (!name && opts.deptMap) name = opts.deptMap[String(opts.value)] || '';
            setVal(opts.value, name);
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
                layer.open({
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
                            layer.closeAll();
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
                layer.open({
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
                            layer.closeAll();
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
    }

    global.RefPicker = {
        mountDeptPicker: mountDeptPicker,
        mountResourcePicker: mountResourcePicker,
        mountDevicePicker: mountDevicePicker,
        initFacilityFormPickers: initFacilityFormPickers
    };
})(window);
