layui.define(['element', 'common'], function (exports) {
    "use strict";
    var $ = layui.jquery,
        layer = parent.layer === undefined ? layui.layer : parent.layer,
        element = layui.element,
        common = layui.common,
        cacheName = 'tb_navbar';

    var Navbar = function () {
        /**
		 *  默认配置
		 */
        this.config = {
            elem: undefined, //容器
            data: undefined, //数据源
            url: undefined, //数据源地址
            type: 'GET', //读取方式
            cached: false, //是否使用缓存
            spreadOne: false //设置是否只展开一个二级菜单
        };
        this.v = '1.0.0';
    };
    //渲染
    Navbar.prototype.render = function () {
        var _that = this;
        var _config = _that.config;
        if (typeof (_config.elem) !== 'string' && typeof (_config.elem) !== 'object') {
            common.throwError('Navbar error: elem参数未定义或设置出错，具体设置格式请参考文档API.');
        }
        var $container;
        if (typeof (_config.elem) === 'string') {
            $container = $('' + _config.elem + '');
        }
        if (typeof (_config.elem) === 'object') {
            $container = _config.elem;
        }
        if ($container.length === 0) {
            common.throwError('Navbar error:找不到elem参数配置的容器，请检查.');
        }
        if (_config.data === undefined && _config.url === undefined) {
            common.throwError('Navbar error:请为Navbar配置数据源.')
        }
        if (_config.data !== undefined && typeof (_config.data) === 'object') {
            var html = getHtml(_config.data);
            $container.html(html);
            element.init();
            _that.config.elem = $container;
        } else {
            if (_config.cached) {
                var cacheNavbar = layui.data(cacheName);
                if (cacheNavbar.navbar === undefined) {
                    $.ajax({
                        type: _config.type,
                        url: _config.url,
                        async: false, //_config.async,
                        dataType: 'json',
                        success: function (result, status, xhr) {
                            //添加缓存
                            layui.data(cacheName, {
                                key: 'navbar',
                                value: result
                            });
                            var html = getHtml(result);
                            $container.html(html);
                            element.init();
                        },
                        error: function (xhr, status, error) {
                            common.msgError('Navbar error:' + error);
                        },
                        complete: function (xhr, status) {
                            _that.config.elem = $container;
                        }
                    });
                } else {
                    var html = getHtml(cacheNavbar.navbar);
                    $container.html(html);
                    element.init();
                    _that.config.elem = $container;
                }
            } else {
                //清空缓存
                layui.data(cacheName, null);
                $.ajax({
                    type: _config.type,
                    url: _config.url,
                    async: false, //_config.async,
                    dataType: 'json',
                    success: function (result, status, xhr) {
                        var html = getHtml(result);
                        $container.html(html);
                        element.init();
                    },
                    error: function (xhr, status, error) {
                        common.msgError('Navbar error:' + error);
                    },
                    complete: function (xhr, status) {
                        _that.config.elem = $container;
                    }
                });
            }
        }
        $('.layui-nav-item-2').on('click', function () {
            $(this).toggleClass('layui-nav-itemed-2');
            $('dd.layui-this').removeClass('layui-this');
            if ($(this).siblings('li.layui-nav-item.layui-nav-item-2').hasClass('layui-nav-item-2')) {
                $(this).siblings('li.layui-nav-item.layui-nav-item-2').removeClass('layui-nav-itemed');
                $(this).siblings('li.layui-nav-item.layui-nav-item-2').removeClass('layui-nav-itemed-2');
            }
        });

        $('dl.layui-nav-child > dd').on('click', function (e) {
            e.stopPropagation();
            if (!$(this).hasClass('layui-nav-item-2')) {
                $('li.layui-nav-item.layui-nav-item-2').removeClass('layui-nav-itemed');
                $('li.layui-nav-item.layui-nav-item-2').removeClass('layui-nav-itemed-2');
            }
        });
        //只展开一个二级菜单
        if (_config.spreadOne) {
            var $ul = $container.children('ul');
            $ul.find('li.layui-nav-item').each(function () {
                $(this).on('click', function () {
                    $(this).siblings().removeClass('layui-nav-itemed');
                });
            });
        }
        return _that;
    };
    /**
	 * 配置Navbar
	 * @param {Object} options
	 */
    Navbar.prototype.set = function (options) {
        var that = this;
        that.config.data = undefined;
        $.extend(true, that.config, options);
        return that;
    };
    /**
	 * 绑定事件
	 * @param {String} events
	 * @param {Function} callback
	 */
    Navbar.prototype.on = function (events, callback) {
        var that = this;
        var _con = that.config.elem;
        if (typeof (events) !== 'string') {
            common.throwError('Navbar error:事件名配置出错，请参考API文档.');
        }
        var lIndex = events.indexOf('(');
        var eventName = events.substr(0, lIndex);
        var filter = events.substring(lIndex + 1, events.indexOf(')'));
        if (eventName === 'click') {
            if (_con.attr('lay-filter') !== undefined) {
                _con.children('ul').find('li').each(function () {
                    var $this = $(this);
                    if ($this.find('dl').length > 0) {
                        var $dd = $this.find('dd').each(function () {
                            $(this).on('click', function () {
                                var $a = $(this).children('a');
                                var href = $a.data('url');
                                if (href == undefined || href == null || href == "") {
                                    return;
                                }
                                var icon = $a.children('i:first').data('icon');
                                var title = $a.children('cite').text();
                                var data = {
                                    elem: $a,
                                    field: {
                                        href: href,
                                        icon: icon,
                                        title: title
                                    }
                                }
                                callback(data);
                            });
                        });
                    } else {
                        $this.on('click', function () {
                            var $a = $this.children('a');
                            var href = $a.data('url');
                            if (href == undefined || href == null || href == "") {
                                return;
                            }
                            var icon = $a.children('i:first').data('icon');
                            var title = $a.children('cite').text();
                            var data = {
                                elem: $a,
                                field: {
                                    href: href,
                                    icon: icon,
                                    title: title
                                }
                            }
                            callback(data);
                        });
                    }
                });
            }
        }
    };
    /**
	 * 清除缓存
	 */
    Navbar.prototype.cleanCached = function () {
        layui.data(cacheName, null);
    };
    /**
	 * 获取html字符串
	 * @param {Object} data
	 */
    function getHtml(data) {
        //debugger;
        var ulHtml = '<ul class="layui-nav layui-nav-tree" lay-shrink="all">';
        for (let i = 0; i < data.length; i++) {
            let curNodeData = data[i].Item;
            let curNodeDataChildren = data[i].Children[0];
            //console.log(curNodeDataChildren)
            if (curNodeData.spread) {
                ulHtml += '<li data-id="' + curNodeData.Id +'" class="layui-nav-item layui-nav-itemed">';
            } else {
                ulHtml += '<li data-id="' + curNodeData.Id +'" class="layui-nav-item">';
            }
            if (curNodeDataChildren !== undefined && curNodeDataChildren !== null ) {
                ulHtml += '<a href="javascript:;">';
                if (curNodeData.IconName !== undefined && curNodeData.IconName !== '') {
                    if (curNodeData.IconName.indexOf('fa-') !== -1) {
                        ulHtml += '<i class="fa fa-fw ' + curNodeData.IconName + '" aria-hidden="true" data-icon="' + curNodeData.IconName + '"></i>';
                    } else {
                        ulHtml += '<i class="layui-icon" data-icon="' + curNodeData.IconName + '">' + curNodeData.IconName + '</i>';
                    }
                }
                ulHtml += '<cite>' + curNodeData.Name + '</cite>'
                ulHtml += '</a>';
                ulHtml += '<dl class="layui-nav-child">'

                for (let j = 0; j < data[i].Children.length; j++) {
                    //ulHtml += getHtml(subcurData);
                    let subcurData = data[i].Children[j].Item;
                    let subcurDataChildren = data[i].Children[j].Children;
                    if (subcurDataChildren == null || subcurDataChildren.length == 0) {
                        ulHtml += '<dd data-id="' + subcurData.Id +'" title="' + subcurData.Name + '">';
                        ulHtml += '<a href="javascript:;" data-url="' + subcurData.Url + '">';
                        if (subcurData.IconName !== undefined && subcurData.IconName !== '') {
                            if (subcurData.IconName.indexOf('fa-') !== -1) {
                                ulHtml += '<i class="fa fa-fw ' + subcurData.IconName + '" data-icon="' + subcurData.IconName + '" aria-hidden="true"></i>';
                            } else {
                                ulHtml += '<i class="layui-icon" data-icon="' + subcurData.IconName + '">' + subcurData.IconName + '</i>';
                            }
                        }
                        ulHtml += '<cite>' + subcurData.Name + '</cite>';
                        ulHtml += '</a>';
                        ulHtml += '</dd>';
                    }
                    else {
                        ulHtml += '<dd data-id="' + subcurData.Id +'" data-name="grid"  title="' + subcurData.Name + '">';
                        ulHtml += '<a  href="javascript:;" >';
                        if (subcurData.IconName !== undefined && subcurData.IconName !== '') {
                            if (subcurData.IconName.indexOf('fa-') !== -1) {
                                ulHtml += '<i class="fa fa-fw ' + subcurData.IconName + '" data-icon="' + subcurData.IconName + '" aria-hidden="true"></i>';
                            } else {
                                ulHtml += '<i class="layui-icon" data-icon="' + subcurData.IconName + '">' + subcurData.IconName + '</i>';
                            }
                        }
                        ulHtml += '<cite>' + subcurData.Name + '</cite>'
                        ulHtml += '</a>';
                        ulHtml += '<dl class="layui-nav-child">'
                        for (var k = 0; k < subcurDataChildren.length; k++) {
                            let subsubData = subcurDataChildren[k].Item;
                            ulHtml += '<dd data-id="' + subsubData.Id + '" title="' + subsubData.Name + '">';

                            ulHtml += '<a href="javascript:;" data-url="' + subsubData.Url + '">';
                            if (subsubData.IconName !== undefined && subsubData.IconName !== '') {
                                if (subsubData.IconName.indexOf('fa-') !== -1) {
                                    ulHtml += '<i class="fa fa-fw ' + subsubData.IconName + '" data-icon="' + subsubData.IconName + '" aria-hidden="true"></i>';
                                } else {
                                    ulHtml += '<i class="layui-icon" data-icon="' + subsubData.IconName + '">' + subsubData.IconName + '</i>';
                                }
                            }
                            ulHtml += '<cite>' + subsubData.Name + '</cite>';
                            ulHtml += '</a>';
                            ulHtml += '</dd>';
                        }
                        ulHtml += '</dl>'
                        ulHtml += '</dd>';
                    }
                }
                ulHtml += '</dl>';
            } else {
                var dataUrl = (curNodeData.Url !== undefined && curNodeData.Url !== '') ? 'data-url="' + curNodeData.Url + '"' : '';
                ulHtml += '<a href="javascript:;" ' + dataUrl + '>';
                if (curNodeData.IconName !== undefined && curNodeData.IconName !== '') {
                    if (curNodeData.IconName.indexOf('fa-') !== -1) {
                        ulHtml += '<i class="fa fa-fw ' + curNodeData.IconName + '" aria-hidden="true" data-icon="' + curNodeData.IconName + '"></i>';
                    } else {
                        ulHtml += '<i class="layui-icon" data-icon="' + curNodeData.IconName + '">' + curNodeData.IconName + '</i>';
                    }
                }
                ulHtml += '<cite>' + curNodeData.Name + '</cite>'
                ulHtml += '</a>';
            }
            ulHtml += '</li>';
        }
        ulHtml += '</ul>';
        return ulHtml;
    }

    var navbar = new Navbar();
    exports('navbar', function (options) {
        return navbar.set(options);
    });
});