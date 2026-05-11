/**
 * 动态为表格添加表头搜索功能
 * Created by li on 2017年09月23日19:46:19
 */
/**
 项目JS主入口
 以依赖Layui的layer和form模块为例
 **/
layui.define(['form', 'laydate'], function (exports) {
    var table = layui.table,
        form = layui.form,
        laydate = layui.laydate
        //, tools = layui.tools;
    let obj = {
        tableIns: [],
        searchConfig: {},
        unId:'',
        showType:"",
        /**
         * @param ins 完成初始化表单对象
         */
        init: function (ins, showType = "", searchId) {
            this.unId = ins.config.id;
            this.tableIns[this.unId] = ins;
            
            this.showType = showType;
            let tempConfig = [];
            if (undefined != this.tableIns[this.unId]) {
                $.each(this.tableIns[this.unId].config.cols[0], function (i, v) {
                    // console.log(v.field);
                    if (undefined != v.field) {
                        if (v.search) {
                            let temp = {
                                'field': v.field,
                                'type': v.search,
                                'text': v.title,
                                'selector': v.search_data || undefined,
                                'searchDefaultValue': v.searchDefaultValue
                            };
                            tempConfig.push(temp);
                        }
                    }
                });
                this.searchConfig[this.unId] = tempConfig;
                if (this.searchConfig[this.unId].length > 0) {
                    let options = '';
                    // 设置检索条目
                    $.each(this.searchConfig[this.unId], function (i, v) {
                        options += '<option value="' + v.field + '" type="' + v.type + '">' + v.text + '</option>'
                    });
                    let conditionalHtml = this.genConditionalOptions(this.searchConfig[this.unId][0].type);
                    let searchValueDefault = this.genSearchValue(this.searchConfig[this.unId][0].type);
                    let searchDivHtml =
                        '<div class="searchWidth" style="min-width: 465px;min-height:30px">' +
                        '<form class="layui-form layui-form-pane searchpanel" lay-filter="searchForm_' + this.unId + '" action="" method="post">' +
                        '<div class="layui-form-item">' +
                        '<div class="layui-input-inline" style="width: 110px">' +
                        '<select name="field" lay-filter="search_field" lay-search>' +
                        options +
                        '</select>' +
                        '</div>' +
                        '<div class="layui-input-inline" style="width: 65px">' +
                        conditionalHtml +
                        '</div>' +
                        '<div class="layui-input-inline" style="width:180px">' +
                        searchValueDefault +
                        '</div>' +
                        '<div class="layui-btn-group" style="line-height:31px"><button class="layui-btn layui-btn-sm " lay-submit="" lay-filter="searchBtn_' + this.unId + '" >查询</button>' +
                        '<button class="layui-btn layui-btn-sm layui-btn-warm" lay-filter="clearBtn" >重置</button>' +
                        '<a class="layui-btn layui-btn-primary layui-btn-sm searchmorebtn lay-search-more-btn_' + this.unId + '" data-unid="' + this.unId + '">高级检索</a></div>' +
                        '</div>' +
                        '</form>' +
                        '</div>';
                    if (showType == "grid") {
                        if (this.searchConfig[this.unId].length > 0) {
                            let formDiv = '';
                            let that = this;
                            $.each(this.searchConfig[this.unId], function (i, v) {
                                let lable = '<div class="layui-col-lg5 layui-col-md5 layui-col-sm5 layui-col-xs5"><label class="layui-form-label" style="width: 100%; padding: 9px 0;">' + v.text + '<input style="display: none" name="field" value="' + v.field + '"/></label></div>';
                                let conditionalHtml = that.genConditionalOptions(that.searchConfig[that.unId][i].type);
                                let searchValue = that.genSearchValue(v.type, v.selector, v.searchDefaultValue);
                                let inputDiv = '<div class="layui-input-inlie layui-hide layui-col-lg2 layui-col-md2 layui-col-sm2 layui-col-xs2">' +
                                    conditionalHtml +
                                    '</div>' +
                                    '<div class="layui-input-inlne layui-col-lg7 layui-col-md7 layui-col-sm7 layui-col-xs7">' +
                                    searchValue +
                                    '</div>';

                                formDiv += '<div class="layui-col-md3 layui-col-lg3 layui-col-sm4 layui-col-xs6 layui-row layui-col-space5" >' +
                                    lable +
                                    inputDiv +
                                    '</div>';
                            });

                            let html = '<div style="text-align: center;padding: 16px 0;"><form action="" method="post" class="layui-form  layui-col-space5 layui-fluid" lay-filter="searchForm_' + this.unId + '">' +
                                formDiv +
                                '<div class="layui-col-md3 layui-col-lg3 layui-col-sm4 layui-col-xs6 layui-row " style="text-align:right;float:right">' +
                                '<div class="layui-input-inline layui-col-space5 layui-btn-group">' +
                                '<button type="button" class="layui-btn layui-btn-sm" lay-submit="" lay-filter="searchBtn_' + this.unId + '" >查询</button>' +
                                '<button type="reset" class="layui-btn layui-btn-sm layui-btn-warm" >重置</button>' +
                                '<a class="layui-btn layui-btn-primary layui-btn-sm searchmorebtn lay-search-more-btn_' + this.unId + '" data-unid="' + this.unId + '">高级检索</a>' +
                                (typeof URLGlobaExport != "undefined" ? '<button type="button" class="layui-btn layui-btn-sm layui-btn-normal" lay-submit="" lay-filter="exportSearch" >导出</button>' : '') +
                                '</div></div></div>' +
                                '</form></div>';
                            searchDivHtml = html;
                            if (typeof searchId != "undefined") {
                                $(searchId).empty()
                                $(searchId).append(searchDivHtml)
                            }
                            else if ($("#searchPanel").length > 0) {
                                $("#searchPanel").empty()
                                $("#searchPanel").append(searchDivHtml)
                            }
                            else
                                $(this.tableIns[this.unId].config.elem).before(searchDivHtml);
                            form.render();
                            this.initMoreEvent(showType, this.unId);
                            setTimeout(function () {
                                that.renderDate()
                            }, 0)
                        }
                    }
                    else {
                        // 将表单数据放置正确的位置
                        if (typeof searchId != "undefined") {
                            $(searchId).empty()
                            $(searchId).append(searchDivHtml)
                        }
                        else if ($("#searchPanel").length > 0) {
                            $("#searchPanel").empty()
                            $("#searchPanel").append(searchDivHtml)
                        }
                        else
                            $(this.tableIns[this.unId].config.elem).before(searchDivHtml);
                        //$('.layui-table-body.layui-table-main').height($('.layui-table-body.layui-table-main').height() - 50);
                        // 动态初始化表单数据
                        form.render();
                        let that = this;
                        // 添加各种数据绑定
                        this.initEvent();
                        setTimeout(function () {
                            that.renderDate()
                        }, 0)
                    }
                }

            }
        },
        /**
         * 初始化简单检索事件绑定
         */
        initEvent: function () {
            let that = this;
            // 添加事件绑定效果
            form.on('select(search_field)', function (data) {
                // 获取检索类型
                let type = $(data.elem).find('[value=' + data.value + ']').attr('type');
                $(data.othis).parent().next().html(that.genConditionalOptions(type));
                let extra = undefined;
                $.each(that.searchConfig[that.unId], function (i, v) {
                    if (data.value == v.field) {
                        if (v.selector) {
                            extra = v.selector;
                        }
                        return;
                    }
                });
                $(data.othis).parent().next().next().html(that.genSearchValue(type, extra));
                form.render();
                setTimeout(function () {
                    that.renderDate()
                }, 0)
            });

            form.on('select(search_condition)', function (data) {
                let type = $(data.elem).attr('type');
                let flag = undefined;
                if ('between' == data.value) {
                    flag = true;
                }
                $(data.othis).parent().next().html(that.genSearchValue(type, flag));
                form.render();
                setTimeout(function () {
                    that.renderDate()
                }, 0)
            });

            form.on('submit(searchBtn_' + that.unId + ')', function (data) {
                //console.log(data.elem);
                let unid = $(data.elem).attr("lay-filter").replace('searchBtn_', '');
                if ('like' == data.field.conditional) {
                    // 结构为   %XXXX%
                    data.field.value = '' + data.field.value + '';
                } else if ('between' == data.field.conditional) {
                    // 结构为   XXX,XXXX
                    data.field.value = data.field.value + '\' and \'' + data.field.value1;
                    delete data.field.value1;
                }
                //if ('' != data.field.value)
                {
                    let insWhere = that.tableIns[unid].config.where;
                    insWhere.searchParam = [data.field];
                    that.tableIns[unid].reload({
                        where: insWhere,
                        page: {
                            curr: 1 //重新从第 1 页开始
                        }
                    });
                    searchParam = { 'searchParam': [data.field] };
                    //that.init(that.tableIns);
                }
                return false;
            });
            // 绑定高级检索安妮点击事件
            $('.lay-search-more-btn_' + that.unId).bind('click', function (e) {
                let unid = $(this).data("unid");
                //console.log(unid)
                if (that.searchConfig[unid].length > 0) {
                    let formDiv = '';
                    $.each(that.searchConfig[unid], function (i, v) {
                        let lable = '<div class="layui-col-lg3 layui-col-md3 layui-col-sm3 layui-col-xs3"><label class="layui-form-label">' + v.text + '<input style="display: none" name="field" value="' + v.field + '"/></label></div>';
                        let conditionalHtml = that.genConditionalOptions(v.type, v.field);
                        let searchValue = that.genSearchValue(v.type, v.selector);
                        let inputDiv = '<div class="layui-input-inlie layui-col-lg2 layui-col-md2 layui-col-sm2 layui-col-xs2">' +
                            conditionalHtml +
                            '</div>' +
                            '<div class="layui-input-inlne layui-col-lg7 layui-col-md7 layui-col-sm7 layui-col-xs7">' +
                            searchValue +
                            '</div>';

                        formDiv += '<div class="layui-form-item layui-row layui-col-space5" >' +
                            lable +
                            inputDiv +
                            '</div>';
                    });

                    let html = '<div style="text-align: center;padding: 16px 0"><form action="" method="post" class="layui-form layui-fluid" lay-filter="searchMoreForm">' +
                        formDiv +
                        '<div class="layui-input-inline layui-hide">' +
                        '<button type="button" class="layui-btn layui-btn-warm btn_advsearch searchMoreBtn_' + unid + '" lay-submit="" lay-filter="searchMoreBtn_' + unid + '" >查询</button>' +
                        '<button type="reset" class="layui-btn layui-btn-primary" >重置</button>' +
                        '</div>' +
                        '</form></div>';
                    let complexIndex = layer.open({
                        type: 1,
                        title: '高级检索',
                        area: ['600px', '400px'], //宽高
                        content: html,
                        id:"AdvanceSerach001",
                        shade: 0,
                        maxmin: true,
                        btn: ["高级检索"],
                        zIndex: layer.zIndex,
                        success: function (layero) {
                            layer.setTop(layero);
                        },
                        yes: function (index,layero) {
                            $(".btn_advsearch").trigger("click");
                        }
                    });
                    form.render();
                    that.initMoreEvent("normal", unid);
                    setTimeout(function () {
                        that.renderDate()
                    }, 0)
                }

            });

           
        },
        /**
         * 初始化高级检索事件绑定
         */
        initMoreEvent: function (type, unid) {
            let that = this;
            form.on('select(search_condition_moreSearch)', function (data) {
                let types = $(data.elem).attr('type');
                let flag = undefined;
                if ('between' == data.value) {
                    flag = true;
                }
                $(data.othis).parent().next().html(that.genSearchValue(types, flag));
                form.render();
                setTimeout(function () {
                    that.renderDate()
                },0)
            });
            
            form.on('submit(searchMoreBtn_' + unid + ')', function (data) {
                //console.log(unid)
                //let unid = $(data.elem).attr("lay-filter").replace('searchMoreBtn_', '');
                let formData = $('form[lay-filter=searchMoreForm]').serializeArray();
                let temp = { 'field': '', 'conditional': '', 'value': '' };
                let tempObj = [];
                for (let i = 0; i < formData.length; i++) {
                    if ("field" == formData[i].name) {
                        temp.field = formData[i].value;
                    } else if ("conditional" == formData[i].name) {
                        temp.conditional = formData[i].value;
                    } else {
                        if ('like' == temp.conditional) {
                            // 结构为   %XXXX%
                            temp.value = '' + formData[i].value + '';
                        } else if ('between' == temp.conditional) {
                            // 结构为   XXX,XXXX
                            temp.value = formData[i].value + '\' and \'' + formData[i + 1].value;
                            i++;
                        } else {
                            temp.value = formData[i].value;
                        }
                        if ('' != temp.value) {
                            tempObj.push(temp);
                        }
                        temp = { 'field': '', 'conditional': '', 'value': '' };
                    }
                }
                //if (tempObj.length > 0)
                {
                    let insWhere = that.tableIns[unid].config.where;
                    insWhere.searchParam = tempObj;
                    that.tableIns[unid].reload({
                        where: insWhere,
                        page: {
                            curr: 1 //重新从第 1 页开始
                        }
                    });
                    searchParam = { 'searchParam': tempObj }
                    //that.init(that.tableIns);
                }
                return false;
            });
            form.on('submit(searchBtn_' + unid + ')', function (data) {
                //let unid = $(data.elem).attr("lay-filter").replace('searchBtn_', '');
                let formData = $('form[lay-filter=searchForm_' + unid + ']').serializeArray();
                let temp = { 'field': '', 'conditional': '', 'value': '' };
                let tempObj = [];
                for (let i = 0; i < formData.length; i++) {
                    if ("field" == formData[i].name) {
                        temp.field = formData[i].value;
                    } else if ("conditional" == formData[i].name) {
                        temp.conditional = formData[i].value;
                    } else {
                        if ('like' == temp.conditional) {
                            // 结构为   %XXXX%
                            temp.value = '' + formData[i].value + '';
                        } else if ('between' == temp.conditional) {
                            // 结构为   XXX,XXXX
                            temp.value = formData[i].value + '\' and \'' + formData[i + 1].value;
                            i++;
                        } else {
                            temp.value = formData[i].value;
                        }
                        if ('' != temp.value) {
                            tempObj.push(temp);
                        }
                        temp = { 'field': '', 'conditional': '', 'value': '' };
                    }
                }
                //if (tempObj.length > 0)
                {
                    let insWhere = that.tableIns[unid].config.where;
                    insWhere.searchParam = tempObj;
                    that.tableIns[unid].reload({
                        where: insWhere,
                        page: {
                            curr: 1 //重新从第 1 页开始
                        }
                    });
                    searchParam = { 'searchParam': tempObj }
                    //that.init(that.tableIns);
                }
                return false;
            });
            form.on('submit(exportSearch)', function () {
                let formData = $('form[lay-filter=searchForm_' + unid + ']').serializeArray();
                let temp = { 'field': '', 'conditional': '', 'value': '' };
                let tempObj = [];
                for (let i = 0; i < formData.length; i++) {
                    if ("field" == formData[i].name) {
                        temp.field = formData[i].value;
                    } else if ("conditional" == formData[i].name) {
                        temp.conditional = formData[i].value;
                    } else {
                        if ('like' == temp.conditional) {
                            // 结构为   %XXXX%
                            temp.value = '' + formData[i].value + '';
                        } else if ('between' == temp.conditional) {
                            // 结构为   XXX,XXXX
                            temp.value = formData[i].value + '\' and \'' + formData[i + 1].value;
                            i++;
                        } else {
                            temp.value = formData[i].value;
                        }
                        if ('' != temp.value) {
                            tempObj.push(temp);
                        }
                        temp = { 'field': '', 'conditional': '', 'value': '' };
                    }
                }
                jQuery.download(URLGlobaExport, { 'searchParam': tempObj });
                return false;
            });
            if (type == "grid") {
                $('.lay-search-more-btn_' + unid).bind('click', function (e) {
                    if (that.searchConfig[unid].length > 0) {
                        let formDiv = '';
                        $.each(that.searchConfig[unid], function (i, v) {
                            let lable = '<div class="layui-col-lg3 layui-col-md3 layui-col-sm3 layui-col-xs3"><label class="layui-form-label">' + v.text + '<input style="display: none" name="field" value="' + v.field + '"/></label></div>';
                            let conditionalHtml = that.genConditionalOptions(v.type, v.field);
                            let searchValue = that.genSearchValue(v.type, v.selector, v.searchDefaultValue);
                            let inputDiv = '<div class="layui-input-inlie layui-col-lg2 layui-col-md2 layui-col-sm2 layui-col-xs2">' +
                                conditionalHtml +
                                '</div>' +
                                '<div class="layui-input-inlne layui-col-lg7 layui-col-md7 layui-col-sm7 layui-col-xs7">' +
                                searchValue +
                                '</div>';

                            formDiv += '<div class="layui-form-item layui-row layui-col-space5" >' +
                                lable +
                                inputDiv +
                                '</div>';
                        });

                        let html = '<div style="text-align: center;padding: 16px 0"><form action="" method="post" class="layui-form layui-fluid" lay-filter="searchMoreForm">' +
                            formDiv +
                            '<div class="layui-input-inline layui-hide">' +
                            '<button class="layui-btn layui-btn-warm searchMoreBtn_' + unid + '" lay-submit="" lay-filter="searchMoreBtn_' + unid + '" >查询</button>' +
                            '<button type="reset" class="layui-btn layui-btn-primary" >重置</button>' +
                            '</div>' +
                            '</form></div>';
                        let complexIndex = layer.open({
                            type: 1,
                            title: '高级检索',
                            area: ['600px', '400px'], //宽高
                            content: html,
                            id: "AdvanceSerach001",
                            shade: 0,
                            maxmin: true,
                            btn: ["高级检索"],
                            zIndex: layer.zIndex,
                            success: function (layero) {
                                layer.setTop(layero);
                            },
                            yes: function (index, layero) {
                                $(".searchMoreBtn_" + unid).trigger("click");
                            }
                        });
                        form.render();
                        that.initMoreEvent("normal", unid);
                        setTimeout(function () {
                            that.renderDate()
                        }, 0)
                    }

                });
            }
        },
        /**
         * 依据type生成表达式部分
         * @param type
         * @returns {string}
         */
        genConditionalOptions: function (type, field) {
            let moreSearchFlag = '';
            if (field == undefined) {
                field = '';
            } else {
                moreSearchFlag = '_moreSearch';
            }
            let selector_prifix = "<select type='" + type + "' name='conditional' field='" + field + "' lay-filter='search_condition" + moreSearchFlag + "' lay-search>";
            let selector_suffix = "</select>";
            let options = '';
            switch (type) {
                case "number":
                    options =
                        '<option value="=">等于</option>' +
                        '<option value=">">大于</option>' +
                        '<option value="<">小于</option>' +
                        '<option value="between">介于</option>';
                    break;
                case "input":
                    options =
                        '<option value="like">包含</option>' +
                        '<option value="=">等于</option>';
                    break;
                case "date":
                    options =
                        '<option value="=">等于</option>' +
                        '<option value="<">早于</option>' +                        
                        '<option value=">">晚于</option>' +
                        '<option value="between">介于</option>';
                    break;
                case "selector":
                    selector_prifix = "<input style='display:none' type='" + type + "' value='=' name='conditional' lay-filter='conditional'>";
                    options = '';
                    selector_suffix = "";
                    break;

            }
            return selector_prifix + options + selector_suffix;
        },
        /**
         * 依据type生成输入值部分
         * @param type
         * @returns {string}
         */
        genSearchValue: function (type, extra, defaultValue) {
            if (typeof defaultValue == "undefined")
                defaultValue = "";
            let html = undefined;

            switch (type) {
                case "number":
                    if (undefined != extra && extra) {
                        html = '<div class="layui-inline"><div class="layui-input-inline" style="width: 45%;float:none;margin-right:0">' +
                            '<input type="number" name="value" placeholder="输入数值" autocomplete="off" class="layui-input"></div>' +
                            '<div style="display:inline-block;width:10%">_</div><div class="layui-input-inline" style="width: 45%;margin-right:0;float:none">' +
                            '<input type="number" name="value1" placeholder="输入数值" autocomplete="off" class="layui-input"></div></div>';
                    } else {
                        html = '<input type="number" name="value" autocomplete="off" value="' + defaultValue + '" placeholder="输入数字" class="layui-input">';
                    }
                    break;
                case "date":
                    if (undefined != extra && extra) {
                        html = '<div class="layui-inline"  style="width:100%;margin-bottom: 0;"><div class="layui-input-inline" style="width: 45%;float:none;margin-right:0">' +
                            '<input type="text" name="value" placeholder="起始时间" autocomplete="off" class="layui-input" lay-verify="|date"></div>' +
                            '<div style="display:inline-block;width:10%">_</div><div class="layui-input-inline" style="width: 45%;margin-right:0;float:none">' +
                            '<input type="text" name="value1" placeholder="终止时间" autocomplete="off" class="layui-input" lay-verify="|date"></div></div>';
                    } else {
                        html = '<input name="value" value="' + defaultValue + '" placeholder="选择日期" lay-verify="date" autocomplete="off"  class="layui-input">';
                    }
                    break;
                case "selector":
                    if (undefined != extra) {
                        html = this.genSearchValueForSelector(extra, defaultValue);
                    } else {
                        html = '';
                    }
                    break;
                default:
                    html = '<input name="value" autocomplete="off" placeholder="" value="' + defaultValue + '" class="layui-input">';
                    break;

            }
            return html;
        },

        genSearchValueForSelector: function (extra, defaultValue) {
            let html = '';
            if (extra == undefined) {
                return html;
            } else {
                let options = '';
                let selector_prifix = "<select name='value' lay-search>";
                let selector_suffix = "</select>";
                let tempExtra = extra.replace(/'/g, '"');
                try {
                    let url = eval('(' + tempExtra + ')').url;
                    //if (url) {
                    //    tools.request(url, {}, function (v) {
                    //        extra = v;
                    //    }, function () {
                    //        extra = [];
                    //    });
                    //} else {
                        extra = eval('(' + tempExtra + ')');
                    //}
                    //console.log(extra);
                    // options +extra= '<option value="">请选择</option>';
                    $.each(extra, function (i, v) {
                        if (defaultValue == v.val)
                            options += '<option selected value="' + v.val + '">' + v.text + '</option>';
                        else
                            options += '<option value="' + v.val + '">' + v.text + '</option>';
                    });
                } catch (e) {
                    extra = {};
                }
                return selector_prifix + options + selector_suffix;
            }
            
        },
        renderDate:function(){
            $('input[lay-verify*=date]').each(function () {
                laydate.render({
                    type: 'date',
                    elem: this
                });
            });
        }
    };
    exports('extend_table', obj); //注意，这里是模块输出的核心，模块名必须和use时的模块名一致
});