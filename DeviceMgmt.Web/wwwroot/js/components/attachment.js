/* =====================================================================
 * Attachment widget (v1)
 *   依赖：jQuery, layui.upload, layui.layer
 *   用法：AttachmentWidget.mount({
 *     container: '#xxx',          // 容器选择器
 *     businessType: 'repair',     // 业务类型
 *     businessId: 123,            // 业务 id
 *     readonly: false,            // 是否只读（不显示上传/删除按钮）
 *     onChange: function(list) {} // 列表变化回调
 *   });
 * ===================================================================== */
(function (global) {
    var URL_UPLOAD = '/Sys_Attachment/Upload';
    var URL_LIST   = '/Sys_Attachment/List';
    var URL_DELETE = '/Sys_Attachment/Delete';
    var URL_PREVIEW = function (id) { return '/Sys_Attachment/Preview?id=' + id; };

    function fmtSize(b) {
        if (!b && b !== 0) return '';
        if (b < 1024) return b + ' B';
        if (b < 1024 * 1024) return (b / 1024).toFixed(1) + ' KB';
        return (b / 1024 / 1024).toFixed(2) + ' MB';
    }

    function isImageExt(ext) {
        return ['jpg','jpeg','png','gif','webp','bmp'].indexOf((ext || '').toLowerCase()) >= 0;
    }

    function escapeHtml(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }

    function render($el, list, opts) {
        if (!list || !list.length) {
            $el.find('.att-list').html('<div class="att-empty">暂无附件</div>');
            return;
        }
        var html = list.map(function (a) {
            var thumb = isImageExt(a.ext)
                ? '<img src="' + URL_PREVIEW(a.id) + '">'
                : '<div class="att-ext">' + (a.ext || '?').toUpperCase() + '</div>';
            var ops = opts.readonly ? '' :
                '<a class="att-op" data-act="del" data-id="' + a.id + '" title="删除"><i class="layui-icon">&#xe640;</i></a>';
            return '<div class="att-card" data-id="' + a.id + '">'
                + '<a class="att-thumb" href="' + URL_PREVIEW(a.id) + '" target="_blank">' + thumb + '</a>'
                + '<div class="att-info">'
                + '  <div class="att-name" title="' + escapeHtml(a.name) + '">' + escapeHtml(a.name) + '</div>'
                + '  <div class="att-meta">' + fmtSize(a.size) + ' · ' + (a.uploader || '') + ' · ' + (a.uploadDate || '') + '</div>'
                + '</div>'
                + '<div class="att-ops">'
                + '  <a class="att-op" data-act="download" data-id="' + a.id + '" title="下载"><i class="layui-icon">&#xe601;</i></a>'
                + ops
                + '</div>'
                + '</div>';
        }).join('');
        $el.find('.att-list').html(html);
    }

    function reload($el, opts) {
        $.get(URL_LIST, { businessType: opts.businessType, businessId: opts.businessId }, function (res) {
            if (res.code !== 0) return;
            render($el, res.data || [], opts);
            opts.onChange && opts.onChange(res.data || []);
        });
    }

    function injectStyle() {
        if (document.getElementById('att-widget-style')) return;
        var css = ''
            + '.att-widget { border:1px solid #e6e6e6; border-radius:6px; padding:10px 12px; background:#fafafa }'
            + '.att-widget .att-header { display:flex; align-items:center; justify-content:space-between; margin-bottom:8px }'
            + '.att-widget .att-title { font-weight:600; color:#1F4D3B }'
            + '.att-widget .att-empty { padding:18px; text-align:center; color:#999; border:1px dashed #ddd; border-radius:6px; background:#fff }'
            + '.att-widget .att-list { display:flex; flex-wrap:wrap; gap:10px }'
            + '.att-widget .att-card { width:240px; display:flex; gap:8px; padding:8px; background:#fff; border:1px solid #eee; border-radius:6px; position:relative }'
            + '.att-widget .att-thumb { width:54px; height:54px; flex:0 0 auto; border-radius:4px; overflow:hidden; background:#f0f0f0; display:flex; align-items:center; justify-content:center }'
            + '.att-widget .att-thumb img { width:100%; height:100%; object-fit:cover }'
            + '.att-widget .att-ext { font-weight:700; color:#888; font-size:14px }'
            + '.att-widget .att-info { flex:1; min-width:0 }'
            + '.att-widget .att-name { font-size:13px; color:#333; white-space:nowrap; overflow:hidden; text-overflow:ellipsis }'
            + '.att-widget .att-meta { font-size:11px; color:#999; margin-top:4px }'
            + '.att-widget .att-ops { display:flex; gap:4px; align-items:flex-start }'
            + '.att-widget .att-op { color:#555; padding:2px 4px; cursor:pointer }'
            + '.att-widget .att-op[data-act=del]:hover { color:#c00 }';
        var $style = document.createElement('style');
        $style.id = 'att-widget-style';
        $style.textContent = css;
        document.head.appendChild($style);
    }

    function getTokenCookie() {
        var m = document.cookie.match(/(?:^|; )Token=([^;]+)/);
        return m ? decodeURIComponent(m[1]) : '';
    }

    var Widget = {
        mount: function (opts) {
            opts = opts || {};
            if (!opts.container) return null;
            if (!opts.businessType || !opts.businessId) {
                console.warn('AttachmentWidget: businessType/businessId required');
                return null;
            }
            injectStyle();
            var $el = $(opts.container);
            if (!$el.length) return null;

            var domId = 'attup_' + Math.random().toString(36).slice(2, 8);
            var uploadBtn = opts.readonly ? '' :
                '<button type="button" id="' + domId + '" class="layui-btn layui-btn-sm">'
                + '<i class="layui-icon">&#xe681;</i> 上传附件</button>';

            $el.html(''
                + '<div class="att-widget">'
                + '  <div class="att-header">'
                + '    <span class="att-title">附件</span>'
                + '    ' + uploadBtn
                + '  </div>'
                + '  <div class="att-list"></div>'
                + '</div>');

            // 关键：layui 2.4.x 必须先 use('upload','layer') 才能拿到模块；
            // 业务页 layui.use 不一定包含 upload，所以在组件内部自包裹。
            layui.use(['upload', 'layer'], function () {
                var upload = layui.upload;
                var layer = layui.layer;

                $el.off('click.att').on('click.att', '[data-act=del]', function () {
                    var id = $(this).attr('data-id');
                    layer.confirm('确认删除此附件?', function (idx) {
                        $.post(URL_DELETE, { id: id }, function (r) {
                            if (r && r.code === 0) { layer.close(idx); reload($el, opts); layer.msg('已删除'); }
                            else layer.msg((r && r.msg) || '删除失败');
                        });
                    });
                }).on('click.att', '[data-act=download]', function () {
                    window.open('/Sys_Attachment/Download?id=' + $(this).attr('data-id'));
                });

                if (!opts.readonly) {
                    upload.render({
                        elem: '#' + domId,
                        url: URL_UPLOAD,
                        accept: 'file',
                        size: 0,
                        data: {
                            businessType: opts.businessType,
                            businessId: opts.businessId,
                            category: opts.category || ''
                        },
                        headers: { 'Token': getTokenCookie() },
                        before: function () { layer.load(2, { shade: 0.1 }); },
                        done: function (res) {
                            layer.closeAll('loading');
                            if (res && res.code === 0) {
                                reload($el, opts);
                                layer.msg('上传成功', { icon: 1 });
                            } else {
                                layer.msg((res && res.msg) || '上传失败', { icon: 2 });
                            }
                        },
                        error: function () {
                            layer.closeAll('loading');
                            layer.msg('上传失败（网络或权限错误）', { icon: 2 });
                        }
                    });
                }

                reload($el, opts);
            });

            return { reload: function () { reload($el, opts); } };
        }
    };

    global.AttachmentWidget = Widget;
})(window);
