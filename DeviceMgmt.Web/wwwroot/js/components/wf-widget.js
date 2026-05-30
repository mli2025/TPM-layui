/*
 * WfWidget —— 业务单据工作流挂接通用组件
 * 任意业务页一行调用即可：WfWidget.open('Facility_RepairBillMain', 工单Id, '维修工单审批')
 * 依赖：layui(layer)、jQuery。所有动作复用 /Wf_Instance/* 端点。
 */
(function (w) {
    var URL = {
        byBiz: '/Wf_Instance/GetByBiz',
        templates: '/Wf_Instance/GetTemplates',
        start: '/Wf_Instance/Start',
        approve: '/Wf_Instance/Approve',
        reject: '/Wf_Instance/Reject',
        withdraw: '/Wf_Instance/Withdraw'
    };
    var STATUS = { 0: '进行中', 1: '已完成', 2: '已驳回', 3: '已撤回' };

    function esc(s) { return (s == null ? '' : String(s)).replace(/[&<>"]/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]; }); }
    function ok(r) { return r && (r.code === 0 || r.code === 200); }
    function ajax(opt) { return layui.$.ajax(opt); }

    function render(bizType, bizId, title, layer, $) {
        ajax({ url: URL.byBiz, data: { bizType: bizType, bizId: bizId } }).done(function (res) {
            if (!ok(res)) { layer.msg((res && res.msg) || '加载失败', { icon: 2 }); return; }
            var inst = res.data && res.data.instance;
            var html = '<div style="padding:14px;">';
            if (!inst) {
                html += '<p style="color:#64748b;">该单据尚未发起审批流程。</p>';
                html += '<div class="layui-form-item"><label class="layui-form-label">选择流程</label><div class="layui-input-block"><select id="wfTpl"></select></div></div>';
                html += '<button class="layui-btn" id="wfStart">提交审批</button>';
            } else {
                var logs = res.data.logs || [];
                html += '<p><b>状态：</b><span class="layui-badge ' + (inst.Status === 1 ? 'layui-bg-green' : inst.Status === 2 ? 'layui-bg-red' : inst.Status === 0 ? 'layui-bg-blue' : '') + '">' + (STATUS[inst.Status] || inst.Status) + '</span>';
                html += ' &nbsp;<b>当前节点：</b>' + esc(inst.CurrentNode || '-') + ' &nbsp;<b>发起人：</b>' + esc(inst.InitiatorName) + '</p>';
                html += '<table class="layui-table" lay-size="sm"><thead><tr><th>时间</th><th>节点</th><th>审批人</th><th>结果</th><th>意见</th></tr></thead><tbody>';
                if (!logs.length) html += '<tr><td colspan="5" style="color:#94a3b8;">暂无审批记录</td></tr>';
                logs.forEach(function (l) {
                    html += '<tr><td>' + (l.ApproveTime ? String(l.ApproveTime).replace('T', ' ').substring(0, 19) : '') + '</td><td>' + esc(l.NodeKey) + '</td><td>' + esc(l.ApproverName) + '</td><td>' + (l.Result === 'agree' ? '<span style="color:#16a34a;">同意</span>' : '<span style="color:#dc2626;">驳回</span>') + '</td><td>' + esc(l.Opinion) + '</td></tr>';
                });
                html += '</tbody></table>';
                if (inst.Status === 0) {
                    html += '<div style="margin-top:10px;"><input id="wfOpinion" class="layui-input" placeholder="审批意见(可选)" style="margin-bottom:8px;">';
                    html += '<button class="layui-btn layui-btn-normal" id="wfApprove">同意</button>';
                    html += '<button class="layui-btn layui-btn-danger" id="wfReject">驳回</button>';
                    html += '<button class="layui-btn layui-btn-primary" id="wfWithdraw">撤回</button></div>';
                }
            }
            html += '</div>';

            var idx = layer.open({ type: 1, title: title || '工作流', area: ['720px', 'auto'], content: html });

            // 绑定提交审批
            if (!inst) {
                ajax({ url: URL.templates }).done(function (tr) {
                    var list = (tr && tr.data) || [];
                    var opt = '';
                    list.forEach(function (t) { opt += '<option value="' + t.Id + '">' + esc(t.Name) + '</option>'; });
                    $('#wfTpl').html(opt); layui.form && layui.form.render('select');
                });
                $(document).off('click.wfstart').on('click.wfstart', '#wfStart', function () {
                    var tplId = $('#wfTpl').val();
                    if (!tplId) { layer.msg('请选择流程'); return; }
                    ajax({ url: URL.start, method: 'POST', contentType: 'application/json', data: JSON.stringify({ TemplateId: parseInt(tplId, 10), BizType: bizType, BizId: bizId }) })
                        .done(function (sr) { if (ok(sr)) { layer.msg('已提交', { icon: 1 }); layer.close(idx); render(bizType, bizId, title, layer, $); } else layer.msg((sr && sr.msg) || '失败', { icon: 2 }); });
                });
            } else if (inst.Status === 0) {
                function act(url) {
                    ajax({ url: url, method: 'POST', data: { id: inst.Id, opinion: $('#wfOpinion').val() } })
                        .done(function (ar) { if (ok(ar)) { layer.msg('已处理', { icon: 1 }); layer.close(idx); render(bizType, bizId, title, layer, $); } else layer.msg((ar && ar.msg) || '失败', { icon: 2 }); });
                }
                $('#wfApprove').on('click', function () { act(URL.approve); });
                $('#wfReject').on('click', function () { act(URL.reject); });
                $('#wfWithdraw').on('click', function () { ajax({ url: URL.withdraw, method: 'POST', data: { id: inst.Id } }).done(function (wr) { if (ok(wr)) { layer.msg('已撤回', { icon: 1 }); layer.close(idx); render(bizType, bizId, title, layer, $); } else layer.msg((wr && wr.msg) || '失败', { icon: 2 }); }); });
            }
        });
    }

    w.WfWidget = {
        open: function (bizType, bizId, title) {
            layui.use(['layer', 'form'], function () { render(bizType, bizId, title, layui.layer, layui.$); });
        }
    };
})(window);
