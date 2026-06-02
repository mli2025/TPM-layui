/**
 * 参照选择器：部门树 / 生产资源（放大镜）
 * 统一使用 layui layer 弹层 + 标准 layui tree / table，风格与系统一致。
 * 依赖：layui layer / tree / table / form
 */
(function (global) {
    'use strict';

    function ok(res) { return res && (res.code === 0 || res.code === 200); }

    // 旧版 layui(2.4.5) tree 使用 nodes/name，click 回调直接返回节点
    function buildDeptNodes(flat) {
        var byParent = {};
        (flat || []).forEach(function (d) {
            var p = String(d.ParentId != null ? d.ParentId : 0);
            (byParent[p] = byParent[p] || []).push(d);
        });
        function nodes(parentId) {
            return (byParent[String(parentId)] || []).map(function (d) {
                var children = nodes(d.Id);
                return {
                    id: d.Id,
                    name: (d.DeptName || '') + (d.DeptNumber ? ' (' + d.DeptNumber + ')' : ''),
                    deptName: d.DeptName || '',
                    spread: true,
                    children: children
                };
            });
        }
        return nodes(0);
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
            if ($disp.prop('readonly') && $disp.is('[disabled]')) return;
            layui.use(['layer', 'tree'], function () {
                var layer = layui.layer, tree = layui.tree;
                $.get(opts.deptUrl || '/Sys_Dept/List', function (res) {
                    if (!ok(res)) { layer.msg((res && res.msg) || '部门加载失败', { icon: 2 }); return; }
                    var nodes = buildDeptNodes(res.data || []);
                    var wrapId = 'refDeptTree_' + new Date().getTime();
                    var html = '<div style="padding:10px 14px;">'
                        + '<div id="' + wrapId + '" style="max-height:380px;overflow:auto;"></div>'
                        + '</div>';
                    var selected = null;
                    layer.open({
                        type: 1, title: '选择工作中心', area: ['400px', '500px'],
                        content: html, shadeClose: false,
                        btn: ['确定', '清空'],
                        success: function () {
                            tree.render({
                                elem: '#' + wrapId,
                                nodes: nodes,
                                click: function (node) {
                                    selected = node;
                                    var $box = $('#' + wrapId);
                                    $box.find('cite').css({ background: '', color: '' });
                                    $box.find('cite').filter(function () {
                                        return $(this).text() === node.name;
                                    }).css({ background: '#5FB878', color: '#fff', borderRadius: '2px' });
                                }
                            });
                        },
                        yes: function (idx) {
                            if (!selected) { layer.msg('请选择部门', { icon: 0 }); return; }
                            setVal(selected.id, selected.deptName || selected.name);
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
        initFacilityFormPickers: initFacilityFormPickers
    };
})(window);
