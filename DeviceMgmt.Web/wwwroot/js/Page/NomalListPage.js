const ids = [];
var table_data = new Array();
var tableIns;
const check_data = new Array();
var searchParam = {};
function isSuccessCode(code) { return code === 0 || code === 200; }

const __COLUMN_TITLE_MAP__ = {
    Id: "ID",
    Code: "编码",
    Name: "名称",
    Number: "编号",
    BillNo: "单据编号",
    BillDate: "单据日期",
    BillType: "单据类型",
    TempID: "模板ID",
    MaintainType: "保养方式",
    Status: "状态",
    Type: "类型",
    Maker: "制单人",
    Checker: "审核人",
    CheckDate: "审核时间",
    CreateDate: "创建时间",
    CreateUserId: "创建人",
    LastUpdateDate: "更新时间",
    LastUpdateUserId: "更新人",
    Project: "项目",
    CheckMethod: "方法",
    UpkeepMethod: "保养方法",
    Remark: "备注",
    FacilityID: "设备ID",
    FacilityCode: "设备编码",
    FacilityName: "设备名称",
    FacilityType: "设备类型",
    ControlType: "控件类型",
    MaxValue: "最大值",
    MinValue: "最小值",
    StdMaxValue: "标准最大值",
    StdMinValue: "标准最小值",
    HNumber: "模板编号",
    HName: "模板名称",
    Hdate: "模板日期",
    HContent: "项目",
    HMethods: "方法",
    HStandard: "标准描述",
    MaintenanceType: "保养类型",
    Maintenance_level: "保养等级",
    BeginDate: "开始时间",
    EndDate: "结束时间",
    Dispatch: "派工状态",
    DispatchDate: "派工时间",
    RepairStaff: "维修人员",
    RepairStaffDate: "维修时间"
};

function normalizeColumnTitles(cols) {
    if (!Array.isArray(cols)) return cols;
    for (let i = 0; i < cols.length; i++) {
        const c = cols[i];
        if (!c || typeof c !== "object") continue;
        const field = c.field || "";
        const title = (c.title || "").toString();
        const mapped = __COLUMN_TITLE_MAP__[field];
        const shouldReplace =
            !!mapped &&
            (
                title.length === 0 ||
                title === field ||
                title.indexOf("Facility_") >= 0 ||
                title.indexOf("Mold_") >= 0 ||
                title.indexOf("鐐") >= 0 ||
                title.indexOf("淇") >= 0 ||
                title.indexOf("宸") >= 0
            );
        if (shouldReplace) c.title = mapped;
    }
    return cols;
}

layui.use(['table', 'element', 'form', 'laydate', 'tableSelect', 'upload'], function () {
    var table = layui.table,
        form = layui.form,
        upload = layui.upload,
        laydate = layui.laydate, layer = layui.layer,
        layerTips = parent.layer === undefined ? layui.layer : parent.layer;
    var editIndex = 0;
    form.render();
    InitSelect2()
    
    $("input[tableselect='true']").each(function () {
        let tableSelect = layui.tableSelect;
        let fkey = $(this).data("key");
        let dataurl = $(this).data("url");
        let displayCol = $(this).data("displaycol").split(',');
        let displayText = $(this).data("displaytext").split(',');
        let displayWidth = $(this).data("displaywidth").split(',');
        let triggerControlIDs = $(this).data("extcontrolids");
        let triggerControlNames = $(this).data("extnames");
        let controlType = $(this).data("controltype");
        let callback = $(this).data("callback");
        let id = $(this).attr("id");
        let colList = [];
        colList.push({ type: controlType })
        //colList.push({ field: fkey, title: fkey })triggercontrolids
        for (let i = 0; i < displayCol.length; i++) {
            colList.push({ field: displayCol[i], title: displayText[i], width: displayWidth[i] })
        }
        tableSelect.render({
            elem: "#" + id,	//定义输入框input对象
            checkedKey: fkey, //表格的唯一建值，非常重要，影响到选中状态 必填
            searchKey: 'key',	//搜索输入框的name值 默认keyword
            extCondition: () => {
                let condition = [];
                if (triggerControlIDs) {
                    let controls = triggerControlIDs.split(',');
                    let fieldName = triggerControlNames.split(',');
                    for (let i = 0; i < controls.length; i++) {
                        let name = fieldName[i] || $("#" + controls[i]).attr("name");
                        condition.push({ field: name, value: $("#" + controls[i]).val() })
                    }
                }
                return condition;
            },
            //searchPlaceholder: '关键词搜索',	//搜索输入框的提示文字 默认关键词搜索
            table: {	//定义表格参数，与LAYUI的TABLE模块一致，只是无需再定义表格elem
                url: dataurl,
                cols: [colList],
                parseData: function (res) {
                    if (!res.data) {
                        res = eval('(' + res + ')');
                    }
                    if (res && res.code === 200) res.code = 0;
                    //console.log(res.Data.ds);
                    return res
                },
                response: {
                    //statusName: 'status' //规定数据状态的字段名称，默认：code
                    statusCode: 0 //规定成功的状态码，默认：0
                    //, msgName: 'hint' //规定状态信息的字段名称，默认：msg
                    //, countName: 'total' //规定数据总数的字段名称，默认：count
                    //, dataName: 'rows' //规定数据列表的字段名称，默认：data
                }
            },
            done: function (elem, data) {
                //选择完后的回调，包含2个返回值 elem:返回之前input对象；data:表格返回的选中的数据 []
                //拿到data[]后 就按照业务需求做想做的事情啦~比如加个隐藏域放ID...
                $(elem).trigger("change");
                eval(callback + "(elem, data.data)");
            }
        })
        $(this).parent().addClass("iconTableselect");
    });

    //年选择器
    $('input[lay-verify*=year]').each(function () {
        laydate.render({
            type: 'year',
            elem: this
        });
        $(this).parent().addClass("iconDate");
    });

    //年月选择器
    $('input[lay-verify*=month]').each(function () {
        laydate.render({
            type: 'month',
            elem: this
        });
        $(this).parent().addClass("iconDate");
    });
    $('input[lay-verify*=daterange]').each(function () {
        laydate.render({
            type: 'date',
            elem: this,
            range: '~'
        });
        $(this).parent().addClass("iconDate");
    });
    //日期时间
    $('input[lay-verify*=datetime]').each(function () {
        laydate.render({
            type: 'datetime',
            elem: this
        });
        $(this).parent().addClass("iconDate");
    });

    //日期选择器
    $('input[lay-verify*=date]').each(function () {
        laydate.render({
            type: 'date',
            elem: this
        });
        $(this).parent().addClass("iconDate");
    });

    //时间选择器
    $('input[lay-verify*=time]').each(function () {
        laydate.render({
            type: 'time',
            elem: this
        });
        $(this).parent().addClass("iconDate");
    });
    $('.layui-file').each(function () {
        var callbackChoose = $(this).data("choose");
        var callbackBefore = $(this).data("before");
        var callbackDone = $(this).data("done");
        var callbackError = $(this).data("error");
        var code = $(this).data("code");
        
        var uploadInst = upload.render({
            elem: this, //绑定元素
            choose: function (obj) {
                if (callbackChoose)
                    eval(callbackChoose + "(obj)");
                else {

                }
            },
            before: function (obj) {
                if (callbackBefore)
                    eval(callbackBefore + "(obj)");
                else {
                    layer.load(2, {
                        zIndex: layui.layer.zIndex,
                        success: function (layerol) {
                            layer.setTop(layerol);
                        }
                    });
                    var demoListView = $('#res_' + code);

                    var files = this.files = obj.pushFile();
                    obj.preview(function (index, file, result) {
                        var prevDom = "";
                        if (file.type.indexOf("image") < 0)
                            prevDom = "<i class='layui-icon'>&#xe61d;</i>";
                        else
                            prevDom = '<img src="' + result + '" style="width:70px;height:30px" class="layui-upload-img">';
                        var tr = $(['<tr id="upload-' + index + '">'
                            , '<td style="width:70px;text-align:center">' + prevDom + '</td>'
                            , '<td style="word-break: break-all;min-width: 52px;">' + file.name + '</td>'
                            , '<td style="width:100px">' + (file.size / 1014).toFixed(1) + 'KB</td>'
                            , '<td style="width:20px"><i class="layui-icon layui-icon-loading layui-anim layui-anim-rotate layui-anim-loop"></i></td>'
                            , '<td style="width:80px;text-align:center">'
                            , '<button type="button" data-index="" class="layui-btn layui-btn-xs demo-reload layui-hide"><i class="layui-icon layui-icon-upload"></i></button>'
                            , '<button type="button" data-index="" class="layui-btn layui-btn-xs layui-btn-danger demo-delete"><i class="layui-icon layui-icon-delete"></i></button>'
                            , '</td>'
                            , '</tr>'].join(''));
                        //单个重传
                        tr.find('.demo-reload').on('click', function () {
                            obj.upload(index, file);
                        });
                        //删除
                        tr.find('.demo-delete').on('click', function () {
                            delete files[index]; //删除对应的文件
                            let fileJson = $("input[name='" + code + "']").val();
                            if (fileJson != "") {
                                fileJson = JSON.parse(fileJson);
                                let str3 = [];
                                for (var i = 0; i < fileJson.length; i++) {
                                    if (tr.html().indexOf(fileJson[i].RelativeUrl) > -1)
                                        continue;
                                    if (fileJson[i] != null)
                                        str3.push(fileJson[i]);
                                }
                                console.log(str3);
                                $("input[name='" + code + "']").val(JSON.stringify(str3));
                            }
                            tr.remove();
                        });
                        if (demoListView.html().indexOf(index) < 0)
                            demoListView.append(tr);
                    });
                }
            },
            done: function (res,index,upload) {
                //上传完毕回调
                if (callbackDone)
                    eval(callbackDone + "(res,index,upload)");
                else {
                    layer.closeAll('loading'); 
                    var demoListView = $('#res_' + code);
                    if (isSuccessCode(res.code)) {
                        let fileJson = $("input[name='" + code + "']").val();
                        if (fileJson != "")
                            fileJson = JSON.parse(fileJson);
                        let str3 = [];
                        for (let i = 0; i < fileJson.length; i++) {
                            if (fileJson[i] != null)
                                str3.push(fileJson[i]);
                        }
                        str3.push(res.data);
                        console.log(str3);

                        $("input[name='" + code + "']").val(JSON.stringify(str3));

                        let tr = demoListView.find('tr#upload-' + index), tds = tr.children();

                        tds.eq(3).html('<span style="color: #5FB878;"><i class="layui-icon layui-icon-ok-circle"></i></span>');
                        tds.eq(4).find('.demo-reload').addClass('layui-hide');
                        tds.eq(4).prepend('<a href="' + res.data.Url + '" target="_blank" class="layui-btn layui-btn-xs layui-btn-normal"><i class="layui-icon layui-icon-link"></i></a>'); //清空操作
                        delete this.files[index]; //删除文件队列已经上传成功的文件
                        //layer.closeAll('loading');
                        return;
                    }
                    else {
                        this.error(index, upload);
                    }
                }
            },
            error: function (index,upload) {
                //请求异常回调
                if (callbackError)
                    eval(callbackError + "(index,upload)");
                else {
                    layer.closeAll('loading'); 
                    var demoListView = $('#res_' + code);
                    var tr = demoListView.find('tr#upload-' + index), tds = tr.children();
                    tds.eq(3).html('<span style="color: #FF5722;"><i class="layui-icon layui-icon-close-fill"></i></span>');
                    tds.eq(4).find('.demo-reload').removeClass('layui-hide'); //显示重传
                }
            }
        });
    });

    $("input[data-inputmask]").inputmask({
        autoUnmask: true
    });

    if (window.URLThisGetList) {
        Cols = normalizeColumnTitles(Cols);
        tableIns = table.render({
            elem: '#GridMain',
            method: 'POST',
            url: URLThisGetList,
            where: { "key": $("#demoReload").val() },
            cols: [Cols],
            page: true,
            toolbar: true,
            height: 'full-80',
            defaultToolbar: ['filter'],
            autoSort: false,
            totalRow: JSON.stringify(Cols).indexOf('totalRow') > -1,
            size: 'sm',
            even: true,
            limit: TableLimit,
            limits: TableLimits,
            parseData: function (res) {
                if (!res.data) {
                    res = eval('(' + res + ')');
                }
                if (res && res.code === 200) res.code = 0;
                //console.log(res.Data.ds);
                return res
            },
            response: {
                statusCode: 0 //规定成功的状态码，默认：0
            },
            done: function (res, curr, count) {
                //数据表格加载完成时调用此函数
                //如果是异步请求数据方式，res即为你接口返回的信息。
                //如果是直接赋值的方式，res即为：{data: [], count: 99} data为当前页数据、count为数据总长度
                //设置全部数据到全局变量
                table_data = res.data;
                //在缓存中找到id ,然后设置data表格中的选中状态
                //循环所有数据，找出对应关系，设置checkbox选中状态
                for (var i = 0; i < res.data.length; i++) {
                    for (var j = 0; j < ids.length; j++) {
                        //数据id和要勾选的id相同时checkbox选中
                        if (res.data[i].Id == ids[j]) {
                            //这里才是真正的有效勾选
                            res.data[i]["LAY_CHECKED"] = 'true';
                            //找到对应数据改变勾选样式，呈现出选中效果
                            var index = res.data[i]['LAY_TABLE_INDEX'];
                            $('.layui-table-fixed-l tr[data-index=' + index + '] input[type="checkbox"]').prop('checked', true);
                            $('.layui-table-fixed-l tr[data-index=' + index + '] input[type="checkbox"]').next().addClass('layui-form-checked');
                        }
                    }
                }
                //设置全选checkbox的选中状态，只有改变LAY_CHECKED的值， table.checkStatus才能抓取到选中的状态
                var checkStatus = table.checkStatus('my-table');
                if (checkStatus.isAll) {
                    $('.layui-table-header th[data-field="0"] input[type="checkbox"]').prop('checked', true);
                    $('.layui-table-header th[data-field="0"] input[type="checkbox"]').next().addClass('layui-form-checked');
                }
                if (buttons) {
                    for (let i = 0; i < buttons.length; i++) {
                        if (buttons[i].ModuleId == GetQueryString("moduleId")) {
                            $("*[powerid='" + buttons[i].DomId+"']").removeClass("layui-hide");
                        }
                    }
                }
            }
        });

        layui.use(['extend_table'], function () {
            extendTable = layui.extend_table;
            extendTable.init(tableIns);
        })
        //table.reload('GridMain', {});
        //console.log(extTable)
        table.on('tool(GridMain)', function (obj) {
            var data = obj.data;
            var event = obj.event;
            switch (event) {
                case 'detail':
                    layRowDetail(obj);
                    break;
                case 'del':
                    layRowDel(obj);
                    break;
                case 'edit':
                    layRowEdit(obj,0);
                    break;
                default:
                    eval(obj.event + "(obj)");
                    break;
            }
        });

        table.on('sort(GridMain)', function (obj) { //注：tool是工具条事件名，GridMain是table原始容器的属性 lay-filter="对应的值"
            console.log(obj.field); //当前排序的字段名
            console.log(obj.type); //当前排序类型：desc（降序）、asc（升序）、null（空对象，默认排序）
            console.log(this); //当前排序的 th 对象
            //尽管我们的 table 自带排序功能，但并没有请求服务端。
            //有些时候，你可能需要根据当前排序的字段，重新向服务端发送请求，从而实现服务端排序，如：
            if (searchParam == undefined)
                searchParam = {};
            searchParam.sfield = obj.field;
            searchParam.sorder = obj.type;
            table.reload('GridMain', {
                initSort: obj, //记录初始排序，如果不设的话，将无法标记表头的排序状态。 layui 2.1.1 新增参数
                where: searchParam
            });


        });

        table.on('checkbox(GridMain)', function (obj) {
            if (obj.checked == true) {
                if (obj.type == 'one') {
                    ids.push(obj.data.Id);
                    check_data.push(obj.data);
                } else {
                    for (let i = 0; i < table_data.length; i++) {
                        ids.push(table_data[i].Id);
                        check_data.push(table_data[i]);
                    }
                }
            } else {
                if (obj.type == 'one') {
                    for (let i = 0; i < ids.length; i++) {
                        if (ids[i] == obj.data.Id) {
                            ids.remove(i);
                            check_data.remove(i);
                        }
                    }
                } else {
                    for (let i = 0; i < ids.length; i++) {
                        for (let j = 0; j < table_data.length; j++) {
                            if (ids[i] == table_data[j].Id) {
                                ids.remove(i);
                                check_data.remove(i);
                            }
                        }
                    }
                }
            }
        });

        table.on('rowDouble(GridMain)', function (obj) {
            //obj 同上
            //if ($("#barDemo a[lay-event='edit']").length > 0 && !($("#barDemo a[lay-event='edit']").hasClass("layui-hide"))) {
            //    layRowEdit(obj, 0);
            //}
            //if ($("#barDemo a").html() == '编辑' && !($("#barDemo a[lay-event='edit']").hasClass("layui-hide"))) {
            //    layRowEdit(obj, 0);
            //}
            let list = $(obj.tr[0]).last().find("a");
            var canEdit = false;
            for (var i = 0; i < list.length; i++) {
                if (list[i].innerText == (typeof Glb_Edit=="undefined"?"编辑":Glb_Edit) && list[i].className.indexOf('layui-hide') < 0) {
                    canEdit = true;
                }
            }

            let innerH = $("#barDemo").html();

            $(innerH).each(function (ind) {
                let text = $(this).text();
                let dbclick = $(this).attr("dbclick");
                if (text == (typeof Glb_Edit=="undefined"?"编辑":Glb_Edit) || dbclick=="true") {
                    let eventName = $(this).attr("lay-event");
                    //let isHide = $(this).hasClass("layui-hide");
                    if (canEdit) {
                        if (eventName == "edit") {
                            layRowEdit(obj, 0);
                        }
                        else {
                            eval(eventName + "(obj)");
                        }
                    }
                }

            })
        });
        //console.log($("select[select2='true']"))
    }
    form.on('submit(saveItem)', function (data) {
        var loadIndex = layerTips.load(2, {
            zIndex: layui.layer.zIndex,
            success: function (layerol) {
                layer.setTop(layerol);
            }
        });
        $.ajax({
            url: URLThisSaveItem,
            data: $("#ItemDetail").serialize(),
            type: "POST",
            success: function (res) {
                /*res.data = data.field;
                console.log(res);*/
                layerTips.close(loadIndex);
                if (isSuccessCode(res.code)) {
                    table.reload('GridMain');
                    layer.closeAll();
                    layerTips.msg((typeof Glb_SaveSuccess == "undefined" ? "保存成功" : Glb_SaveSuccess));
                } else {
                    layerTips.alert((typeof Glb_SaveFail=="undefined"?"保存失败":Glb_SaveFail)+':' + res.msg);
                }
            },
            error: function (xhr, txtStatus, err) {
                layerTips.close(loadIndex);
                layerTips.alert((typeof Glb_ServerErr=="undefined"?"服务器出错":Glb_ServerErr)+': ' + txtStatus);
            }
        })
        return false;
    });
    $("#btn_Add").on("click", function () {
        var addIndex = layer.open({
            type: 1,
            title: typeof Glb_Add == "undefined" ? "新增" : Glb_Add,
            area: ['90%'],
            maxHeight: $(window).height() - 40,
            scrollbar: false,
            content: $('#pnl_edit'),
            btn: [typeof Glb_Save=="undefined"?"保存":Glb_Save],
            success: function (index, layero) {
                $('#pnl_edit *[name][name!=""]').val("");
                $("#pnl_edit *[tableselect='true']").each(function (index) {
                    $(this).attr("ts-selected", 0)
                    InitTableSelect($(this), "-1")
                })
                $("#pnl_edit *[select2='true']").each(function (index) {
                    GetSelectedOption($(this).attr("name"), 0, "#pnl_edit");
                })
                $('#pnl_edit *[name][name=Id]').val(0);
                $('#pnl_edit *[name][name=Status]').val(0);
                form.render();
                InitFileAttachments()
                $('#pnl_edit input:visible,#pnl_edit select:visible').first().focus()
                $('#pnl_edit').parent().css({ "overflow": "inherit" });
            },
            yes: function (index, layero) {
                $("#saveItem").trigger('click');
                //layer.close(addIndex);
            }
        });

        typeof URLThisGetBillNo !=="undefined" && $.ajax({
            url: URLThisGetBillNo,
            type: "POST",
            success: function (res) {
                $('#pnl_edit input[name="Code"]').val(res.eSerialCodeR);
            },
            error: function (xhr, txtStatus, err) {
                layui.layer.alert((typeof Glb_ServerErr=="undefined"?"服务器出错":Glb_ServerErr)+': ' + txtStatus);
            }
        })
    });
    $("#btn_Export").on("click", function () {
        var data = searchParam ? searchParam : "data";
        $.download(URLThisExportList, data, "POST");
    })
    $("#btn_Search").on("click", function () {
        table.reload('GridMain', {
            where: {
                "q": $("#demoReload").val()
            }
        })
    });
    $("#btn_DeleteList").on("click", function () {
        var checkStatus = table.checkStatus('GridMain'); //GridMain即为基础参数id对应的值
        layer.confirm(typeof Glb_DelConfirm=="undefined"?"真的删除这些记录吗":Glb_DelConfirm, function (index) {
            layer.close(index);
            var loadIndex = layerTips.load(2, { time: 20 * 1000 });
            $.ajax({
                url: URLThisDeleteItemList,
                data: { "data": checkStatus.data },
                type: "POST",
                success: function (res) {
                    layerTips.close(loadIndex);
                    if (isSuccessCode(res.code)) {
                        table.reload('GridMain');
                    } else {
                        layerTips.alert((typeof Glb_DelFail=="undefined"?"删除失败":Glb_DelFail)+': ' + res.msg);
                    }
                },
                error: function (xhr, txtStatus, err) {
                    layerTips.close(loadIndex);
                    layerTips.alert((typeof Glb_ServerErr=="undefined"?"服务器出错":Glb_ServerErr)+': ' + txtStatus);
                }
            });
        });
    });
    $("#btn_Print").on("click", function () {
        var checkStatus = table.checkStatus('GridMain');
        console.log(checkStatus)
        var index = layer.open({
            title: "打印预览",
            area: ['210mm', '500px'],
            type: 1,
            btn: ["打印"],
            content: '<div id=printArea></div>',
            success: function (index, layero) {
                $.jsontotable(checkStatus.data, { id: '#printArea', className: 'layui-table', header: false });
            },
            yes: function (index, layero) {
                $("#printArea").printArea()
            }
        });
    });
    
});
function InitSelect2() {
    $.fn.select2.defaults.set("theme", "bootstrap");
    $("select[select2='true']").each(function () {
        //console.log($(this).data("foreigntable"));
        $(this).select2({
            ajax: {
                url: URLGlobalGetSelect2Data,
                dataType: 'json',
                delay: 500,
                type: "POST",
                data: function (params) {
                    console.log(params);
                    return {
                        q: params.term, // search term
                        page: params.page,
                        "table": $(this).data("foreigntable"),
                        "fit": $(this).data("foreignfit"),
                        "valueColumn": $(this).data("foreignvalue"),
                        "displayColumn": $(this).data("foreigndisplay"),
                        "optional": $(this).attr("optional")
                    };
                },
                processResults: function (data, params) {
                    // parse the results into the format expected by Select2
                    // since we are using custom formatting functions we do not need to
                    // alter the remote JSON data, except to indicate that infinite
                    // scrolling can be used
                    params.page = params.page || 1;
                    //var data1 = $.map(data.groups, function (obj) {
                    //    obj.id = obj.value; // replace pk with your identifier
                    //    obj.text = obj.display;
                    //    return obj;
                    //});
                    //console.log(data1);
                    return {
                        results: data.data,
                        pagination: {
                            more: (params.page * 10) < data.total_count
                        }
                    };
                },
                cache: false
            },
            //escapeMarkup: function (markup) { return markup; }, // let our custom formatter work
            minimumInputLength: 0,
            disabled: $(this).attr("readonly")
            //templateResult: formatRepoGroup//, // omitted for brevity, see the source of this page
        });

    });
}
function layRowDel(obj) {
    var form = layui.form;
    var layerTips = parent.layer === undefined ? layui.layer : parent.layer;
    layer.confirm((typeof Glb_DelConfirm=="undefined"?"真的删除这些记录吗？":Glb_DelConfirm), function (index) {
        var loadIndex = layerTips.load(2, {
            zIndex: layui.layer.zIndex,
            success: function (layerol) {
                layer.setTop(layerol);
            }
        });
        //console.log(data)
        $.ajax({
            url: URLThisDeleteItem,
            data: obj.data,
            type: "POST",
            success: function (res) {
                layerTips.close(loadIndex);
                if (isSuccessCode(res.code)) {
                    obj.del();
                } else {
                    layerTips.alert((typeof Glb_DelSuccess=="undefined"?"删除失败":Glb_DelSuccess)+':' + res.msg);
                }
            },
            error: function (xhr, txtStatus, err) {
                layerTips.close(loadIndex);
                layerTips.alert((typeof Glb_ServerErr=="undefined"?"服务器出错":Glb_ServerErr)+':' + err);
            }
        });
        layer.close(index);
    });
}
function layRowDetail(obj) {
    var form = layui.form;
    var data = obj.data;
    var detailIndex = layer.open({
        type: 1,
        title: "查看",
        area: ['90%'],
        maxHeight: $(window).height()-40,
        content: $('#pnl_edit'),
        btn: ["关闭"],
        success: function (index, layero) {
            $.post(URLThisEditItem, data, function (resultData) {
                for (var key in resultData) {
                    $('#pnl_edit *[name][name=' + key + ']').val(resultData[key]);
                    GetSelectedOption(key, resultData[key],"#pnl_edit");
                    $("#pnl_edit input[tableselect='true'][id='" + key + "']").attr("ts-selected", resultData[key])
                    InitTableSelect($("#pnl_edit input[tableselect='true'][id='" + key + "']"), resultData[key])
                }
                InitFileAttachments(true);
                form.render();
                $('#pnl_edit').append('<div class="shadowView" style="position:absolute;top:0;left:0;z-index:20191001;width:100%;height:' + $('#pnl_edit').height() + 'px"></div>');
            });
        },
        yes: function (index, layero) {
            layer.close(detailIndex);
        },
        end: function () {
            $(".shadowView").remove();
        }
    })
}
function layRowEdit(obj,flag) {
    var form = layui.form;
    var data;
    if (flag == 0)
        data = obj.data;
    else
        data = obj;
    var editIndex = layer.open({
        type: 1,
        title: (typeof Glb_Edit=="undefined"?"编辑":Glb_Edit),
        area: ['90%'],
        maxHeight: $(window).height()-40,
        content: $('#pnl_edit'),
        scrollbar: false,
        btn: [Glb_Save],
        success: function (index, layero) {
            $.post(URLThisEditItem, data, function (resultData) {
                for (var key in resultData) {
                    $('#pnl_edit *[name][name=' + key + ']').val(resultData[key]);
                    GetSelectedOption(key, resultData[key],"#pnl_edit");
                    $("#pnl_edit input[tableselect='true'][id='" + key + "']").attr("ts-selected", resultData[key])
                    InitTableSelect($("#pnl_edit input[tableselect='true'][id='" + key + "']"), resultData[key])
                }
                InitFileAttachments();
                form.render();
                $('#pnl_edit input:visible,#pnl_edit select:visible').first().focus()
            });
            $('#pnl_edit').parent().css({ "overflow": "inherit" });
        },
        yes: function (index, layero) {
            $("#saveItem").trigger('click');
            //layer.close(editIndex);
        }
    })
}
function InitFileAttachments(viewMode) {
    if (viewMode == undefined)
        viewMode = false;
    $(".layui-form input[type='hidden'][class='lay-file-hidden']").each(function () {
        //console.log($(this).val())
        var initFunc = $(this).data("init");
        if (initFunc == undefined || initFunc == null || initFunc == "") {
            var strFileList = $(this).val();
            var listName = "res_" + $(this).attr("name");
            $("#" + listName +" tbody").empty();
            if (strFileList.length > 0) {
                var that = this;
                var fileList = JSON.parse($(this).val());
                for (let i = 0; i < fileList.length; i++) {
                    if (fileList[i] != null) {
                        var tr = $(['<tr id="upload-' + i + '">'
                            , '<td style="width:70px;text-align:center"><i class="layui-icon">&#xe61d;</i></td>'
                            , '<td style="word-break: break-all;min-width: 52px;">' + fileList[i].FileName + '</td>'
                            , '<td style="width:100px">' + (fileList[i].ContentLength / 1014).toFixed(1) + 'KB</td>'
                            , '<td style="width:70px"><span style="color: #5FB878;"><i class="layui-icon layui-icon-ok-circle"></i></span></td>'
                            , '<td style="width:100px;text-align:center;z-index:20191010">'
                            , '<a href="' + fileList[i].Url + '" target="_blank" class="layui-btn layui-btn-xs layui-btn-normal"><i class="layui-icon layui-icon-link"></i></a>'
                            , '<button type="button" data-index="' + i + '" class="layui-btn layui-btn-xs layui-btn-danger demo-delete"><i class="layui-icon layui-icon-delete"></i></button>'
                            , '</td>'
                            , '</tr>'].join(''));

                        $("#" + listName).append(tr);
                    }
                }
                $("#" + listName).find('.demo-delete').on('click', function () {
                    var index = $(this).data("index");
                    delete fileList[index]; //删除对应的文件
                    var tr = $(this).parents("tr")
                    tr.remove();
                    fileList = fileList.filter(function (e, i) { return e != null; });
                    $(that).val(JSON.stringify(fileList));
                });
            }
            if ($(this).attr("readonly") == "readonly" || viewMode) {
                $("#f_" + $(this).attr("name")).hide();
                $(".demo-delete").hide();
            }
            else {
                $("#f_" + $(this).attr("name")).show();
                $(".demo-delete").show();
            }
        } else {
            eval(initFunc + "(this)");
        }
    })
}
function InitTableSelect(elem, value) {
    if (elem.length > 0) {
        var url = $(elem).data("url");
        var key = $(elem).data("key");
        var callback = $(elem).data("callback");
        if (typeof value != "undefined" && value.length > 0)
            $.post(url, { key: value.toString(), page: 1, limit: 1000 }, res => {
                eval(callback + "(elem,res.data)");;
            });
    }
}

function GetSelectedOption(name, selectedId, parentTag) {
    if (parentTag == undefined || parentTag == null) {
        parentTag = "";
    }
    else {
        parentTag = parentTag + " ";
    }   
    $(parentTag + "select[name='" + name + "']").val(selectedId);
    if (typeof selectedId == "undefined" || selectedId == "") {
        $(parentTag + "select[select2='true'][name='" + name + "']").html("");
        $(parentTag + "select[select2='true'][name='" + name + "']").val(null).trigger("change");
        return;
    }
    if ($(parentTag + "select[select2='true'][name='" + name + "']").length > 0) {
        //$("select[select2='true'][name='" + name + "']").val(selectedId);
        var tableName = $(parentTag + "select[name='" + name + "']").data("foreigntable");
        var valueColumn = $(parentTag + "select[name='" + name + "']").data("foreignvalue");
        var displayColumn = $(parentTag + "select[name='" + name + "']").data("foreigndisplay");
        var fit = $(parentTag + "select[name='" + name + "']").data("foreignfit");
        $.ajax({
            url: URLGlobalGetSelect2InitItem,
            type: "POST",
            data: { 'name': name, 'selectedId': selectedId, tableName: tableName, valueColumn: valueColumn, displayColumn: displayColumn, fit: fit },
            cache: false,
            success: function (res) {
                $(parentTag + "select[name='" + name + "']").html("");
                for (var i = 0; i < res.data.length; i++) {
                    var $option = $("<option selected></option>").val(res.data[i].id).text(res.data[i].text);
                    $(parentTag + "select[name='" + name + "']").append($option);
                    $(parentTag + "select[name='" + name + "']").trigger("select2:select");
                }
            }
        })
    }
}

function NumberToLocalString(s, n) {
    if (s == "" || s == undefined || s == null)
        s = "0.0";
    n = n >= 0 && n <= 20 ? n : 2;
    s = parseFloat((s + "").replace(/[^\d\.-]/g, "")).toFixed(n) + "";
    let l = s.split(".")[0].split("").reverse(),
        r = s.split(".")[1];
    let t = "";
    for (let i = 0; i < l.length; i++) {
        t += l[i] + ((i + 1) % 3 == 0 && (i + 1) != l.length ? "," : "");
    }
    return t.split("").reverse().join("") + "." + r;
}

Array.prototype.remove = function (dx) {
    if (isNaN(dx) || dx > this.length) { return false; }
    for (var i = 0, n = 0; i < this.length; i++) {
        if (this[i] != this[dx]) {
            this[n++] = this[i]
        }
    }
    this.length -= 1
}