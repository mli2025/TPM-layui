/**
 * 参照选择器：部门树、生产资源表格（放大镜）
 * 依赖 layui layer / tree / tableSelect
 */
(function (global) {
    'use strict';

    function ok(res) { return res && (res.code === 0 || res.code === 200); }

    function buildDeptTree(flat) {
        var byParent = {};
        (flat || []).forEach(function (d) {
            var p = String(d.ParentId != null ? d.ParentId : 0);
            (byParent[p] = byParent[p] || []).push(d);
        });
        function nodes(parentId) {
            return (byParent[String(parentId)] || []).map(function (d) {
                return {
                    id: d.Id,
                    title: (d.DeptName || '') + (d.DeptNumber ? ' (' + d.DeptNumber + ')' : ''),
                    spread: true,
                    children: nodes(d.Id)
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

        $btn.off('click.refDept').on('click.refDept', function () {
            layui.use(['layer', 'tree'], function () {
                var layer = layui.layer, tree = layui.tree;
                $.get(opts.deptUrl || '/Sys_Dept/List', function (res) {
                    if (!ok(res)) { layer.msg((res && res.msg) || '部门加载失败', { icon: 2 }); return; }
                    var treeData = buildDeptTree(res.data || []);
                    var html = '<div id="refDeptTreeWrap" style="padding:12px;max-height:360px;overflow:auto;"></div>';
                    layer.open({
                        type: 1, title: '选择工作中心', area: ['420px', '480px'], content: html,
                        btn: ['确定', '清空', '取消'],
                        success: function () {
                            tree.render({
                                elem: '#refDeptTreeWrap',
                                data: treeData,
                                click: function (obj) {
                                    $('#refDeptTreeWrap').data('sel', obj.data);
                                }
                            });
                        },
                        yes: function (idx) {
                            var sel = $('#refDeptTreeWrap').data('sel');
                            if (!sel) { layer.msg('请选择部门', { icon: 0 }); return; }
                            setVal(sel.id, sel.title);
                            layer.close(idx);
                        },
                        btn2: function (idx) { setVal('', ''); layer.close(idx); return false; }
                    });
                });
            });
        });

        if (opts.value) {
            var name = opts.text || '';
            if (!name && opts.deptMap && opts.value) name = opts.deptMap[String(opts.value)] || '';
            setVal(opts.value, name);
        }
    }

    function mountResourcePicker($root, opts) {
        opts = opts || {};
        var $hid = $root.find('input[name="ResourceId"]');
        var $disp = $root.find('.ref-resource-display');
        var $btn = $root.find('.ref-resource-btn');
        if (!$hid.length || !$disp.length) return;

        layui.use(['tableSelect'], function () {
            var tableSelect = layui.tableSelect;
            tableSelect.render({
                elem: $btn[0],
                checkedKey: 'Id',
                searchKey: 'key',
                searchQuery: 'query',
                searchPlaceholder: '编码/名称',
                table: {
                    url: opts.resourceUrl || '/Basic_Resource/GetPickerList',
                    method: 'post',
                    page: true,
                    limit: 15,
                    where: {},
                    cols: [[
                        { type: 'radio', fixed: 'left' },
                        { field: 'Code', title: '编码', width: 140 },
                        { field: 'Name', title: '名称', minWidth: 160 }
                    ]],
                    parseData: function (res) {
                        if (res && res.code === 200) res.code = 0;
                        return res;
                    }
                },
                done: function (elem, data) {
                    var row = (data.data && data.data[0]) || null;
                    if (!row) return;
                    $hid.val(row.Id);
                    $disp.val((row.Code || '') + ' / ' + (row.Name || ''));
                }
            });
        });

        if (opts.value && opts.text) {
            $hid.val(opts.value);
            $disp.val(opts.text);
        } else if (opts.value) {
            $.get('/Basic_Resource/GetPickerList', {
                page: 1, limit: 1,
                searchParam: [{ field: 'Id', conditional: '=', value: opts.value }]
            }, function (res) {
                if (ok(res) && res.data && res.data[0]) {
                    var r = res.data[0];
                    $disp.val((r.Code || '') + ' / ' + (r.Name || ''));
                }
            });
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
