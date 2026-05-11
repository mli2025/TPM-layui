var initData = {
    "ed_type": "0",
    "ed_title": "",
    "ed_showTitle": "1",
    "ed_fontFamily": "Microsoft Yahei UI",
    "ed_fontSize": "12",
    "ed_fontColor": "#000000",
    "ed_tableSkin": "",
    "ed_tableEven": "false",
    "ed_tableBgColor": "rgba(255,255,255,1)",
    "ed_tableEvColor": "rgba(242,242,242,1)",
    "ed_tableSize": "",
    "ed_tableChangeType": "0",
    "ed_tableColFormat": "",
    "ed_imgList": "[]",
    "ed_imgSize": "A",
    "ed_freshTime": "10000",
    "ed_sql": ""
};
var deepCopy = function (obj) {
    var newO = {};
    if (obj instanceof Array) {
        newO = [];
    }
    for (var key in obj) {
        var val = obj[key];
        newO[key] = typeof val === 'object' ? arguments.callee(val) : val;
    }
    return newO;
};
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return "0";
}

function PostMessageToP(func, id, name, content) {
    window.parent.postMessage(func + "|" + id + "|" + name + "|" + content, window.location.origin);
}
function PostMessageToC(func, id, name, content) {
    iframe.contentWindow.postMessage(func + "|" + id + "|" + name + "|" + content, window.location.origin);
}
var listTimeout = [];
var listRes = [];
function QueryOne(boxId, loadData) {
    if (loadData != false)
        loadData = true;
    for (var i = 0; i < listTimeout.length; i++) {
        if (listTimeout[i].name == boxId) {
            clearTimeout(listTimeout[i].time);
            listTimeout.splice(i, 1);
        }
    }
    if ($("#draggable" + boxId + " input[name=content]").length == 0)
        return;
    var boxContent = $("#draggable" + boxId + " input[name=content]").val().replace(/&quot;/g, "\"");
    var boxJson = JSON.parse(boxContent);
    try {
        if (boxJson.ed_type == 4) {
            GenerateText(boxId, boxJson);
            return;
        }
        if (boxJson.ed_type == 2 && boxJson.ed_sql.length == 0) {
            GenerateImage(boxId, boxJson, null);
            return;
        }
    } catch (es) {
        console.log(es)
    }
    if (loadData) {
        //postData(URLGetQueryString, { id: GetQueryString("id"), bid: boxId, data: boxContent })
        //    .then(
        //        res => {
        //            for (var i = 0; i < listRes.length; i++) {
        //                if (listRes[i].name == boxId) {
        //                    listRes.splice(i, 1);
        //                }
        //            }
        //            if (res.status == true || res.status == "true")
        //                listRes.push({ name: boxId, data: JSON.stringify(res) });
        //            try {
        //                ControlFill(boxId, boxJson, res);
        //            } catch (es) {
        //                console.log(es)
        //            }
        //            finally {
        //                var timeout = setTimeout(function () { QueryOne(boxId); }, boxJson.ed_freshTime * 1);
        //                listTimeout.push({ name: boxId, time: timeout });
        //            }
        //        }
        //    )
        //    .catch(exception => {
        //        console.log(exception);
        //        var timeout = setTimeout(function () { QueryOne(boxId); }, boxJson.ed_freshTime * 1);
        //        listTimeout.push({ name: boxId, time: timeout });
        //    });
        //fetch(URLGetQueryString)
        $.ajax({
            url: URLGetQueryString,
            data: { "id": GetQueryString("id"), "bid": boxId, "content": boxContent },
            cache: false,
            type: "POST",
            success: function (res) {
                for (var i = 0; i < listRes.length; i++) {
                    if (listRes[i].name == boxId) {
                        listRes.splice(i, 1);
                    }
                }
                if (res.code == 200)
                    listRes.push({ name: boxId, data: JSON.stringify(res) });
                try {
                    ControlFill(boxId, boxJson, res);
                } catch (es) {
                    console.log(es)
                }
            },
            complete: function (xhr, state) {
                var timeout = setTimeout(function () { QueryOne(boxId); }, boxJson.ed_freshTime * 1);
                listTimeout.push({ name: boxId, time: timeout });
            }
        });
    }
    else {
        var res;
        for (var i = 0; i < listRes.length; i++) {
            if (listRes[i].name == boxId) {
                res = JSON.parse(listRes[i].data);
                break;
            }
        }
        try {
            ControlFill(boxId, boxJson, res);
        } catch (es) {
            console.log(es)
        }
        finally {
            var timeout = setTimeout(function () { QueryOne(boxId); }, boxJson.ed_freshTime * 1);
            listTimeout.push({ name: boxId, time: timeout });
        }
    }
}
function postData(url, data) {
    var formData = new FormData();
    for (var key in data) {
        formData.append(key, data[key]);
    }
    return fetch(url, {
        body: formData, // must match 'Content-Type' header
        cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
        credentials: 'omit', //credentials: 'same-origin', // include, same-origin, *omit
        headers: {
            //'user-agent': 'Mozilla/4.0 MDN Example',
            //'content-type': 'application/json'
        },
        method: 'POST', // *GET, POST, PUT, DELETE, etc.
        //mode: 'cors', // no-cors, cors, *same-origin
        redirect: 'follow', // manual, *follow, error
        referrer: 'no-referrer', // *client, no-referrer
    }).then(response => response.json()) // parses response to JSON
}
function getData(url, data) {
    var formData = "";
    for (var key in data) {
        formData += ("&" + key + "=" + encodeURI(data[key]));
    }
    return fetch(url + "?rad=" + formData, {
        //body: formData, // must match 'Content-Type' header
        cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
        credentials: 'omit', //credentials: 'same-origin', // include, same-origin, *omit
        headers: {
            //'user-agent': 'Mozilla/4.0 MDN Example',
            //'content-type': 'application/json'
        },
        method: 'GET', // *GET, POST, PUT, DELETE, etc.
        //mode: 'cors', // no-cors, cors, *same-origin
        redirect: 'follow', // manual, *follow, error
        referrer: 'no-referrer', // *client, no-referrer
    }).then(response => response.json()) // parses response to JSON
}
function QueryOneView(boxId) {
    //for (var i = 0; i < listTimeout.length; i++) {
    //    if (listTimeout[i].name == boxId) {
    //        clearTimeout(listTimeout[i].time);
    //        listTimeout.splice(i, 1);
    //    }
    //}
    var boxContent = $("#draggable" + boxId + " input[name=content]").val().replace(/&quot;/g, "\"");
    var boxJson = JSON.parse(boxContent);
    try {
        if (boxJson.ed_type == 4) {
            GenerateText(boxId, boxJson);
            return;
        }
        if (boxJson.ed_type == 2 && boxJson.ed_sql.length == 0) {
            GenerateImage(boxId, boxJson, null);
            return;
        }
    } catch (es) {
        console.log(es)
    }
    //$.post('@Url.Action("GetLaySubQueryOne","Statement")', { id: GetQueryString("id"), bid: boxId, data: boxContent }, function (res) {
    //    //var res = JSON.parse(res);
    //    //for (var i = 0; i < listRes.length; i++) {
    //    //    if (listRes[i].name == boxId) {
    //    //        listRes.splice(i, 1);
    //    //    }
    //    //}
    //    //if (res.status == true || res.status == "true")
    //    //    listRes.push({ name: boxId, data: JSON.stringify(res) });
    //    try {
    //        ControlFill(boxId, boxJson, res);
    //    } catch (es) {
    //        console.log(es)
    //    }
    //    finally {
    //        var timeout = setTimeout(function () { QueryOneView(boxId); }, boxJson.ed_freshTime * 1);
    //        listTimeout.push({ name: boxId, time: timeout });
    //    }
    //}
    //)
    //    .catch(exception => {
    //        console.log(exception);
    //        var timeout = setTimeout(function () { QueryOneView(boxId); }, boxJson.ed_freshTime * 1);
    //        listTimeout.push({ name: boxId, time: timeout });
    //    });
    $.ajax({
        url: URLGetQueryString,
        data: { "id": GetQueryString("id"), "bid": boxId, "content": boxContent },
        cache: false,
        type: "POST",
        success: function (res) {
            try {
                controlfill(boxid, boxjson, res)
            } catch (es) {
                console.log(es)
            }
        },
        complete: function (xhr, state) {
            var boxJson = JSON.parse(boxContent);
            var timeout = setTimeout(function () { QueryOne(boxId); }, boxJson.ed_freshtime * 1);
            //listtimeout.push({ name: boxid, time: timeout });
        }
    });


}
function ControlFill(boxId, boxJson, res) {
    if (res.code == 200) {
        if (boxJson.ed_type == 0) {
            GenerateLabel(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 1) {
            GenerateTable(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 2) {
            GenerateImage(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 3) {
            GenerateLine(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 5) {
            GeneratePie(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 6) {
            GenerateGauge(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 7) {
            GenerateProgress(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 8) {
            GenerateProgressCircle(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 9) {
            GenerateLayer(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 10) {
            GenerateRadar(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 11) {
            GenerateSelect(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 12) {
            GenerateInput(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 13) {
            GenerateButton(boxId, boxJson, res);
        }
    }
    else {
        if (boxJson.ed_type == 2) {
            GenerateImage(boxId, boxJson, res);
        }
        if (boxJson.ed_type == 9) {
            GenerateLayer(boxId, boxJson, res);
        }
    }
}
function GenerateProgressCircle(boxId, boxJson, res) {
    $("#title_" + boxId).hide();
    $("#content_" + boxId).height($("#draggable" + boxId).height());
    $("#content_" + boxId).empty();
    var width = $("#content_" + boxId).width();
    var height = $("#content_" + boxId).height();
    var barWidth = boxJson.ed_PCWidth * 1;
    var pc = 0, sc = 0;
    if (res.data.length > 0) {
        for (var key in res.data[0]) {
            pc = (res.data[0][key].toString());
            break;
        }
    }

    var radius = width;
    if (radius > height)
        radius = height;
    radius = Math.floor(radius / 2) - barWidth;
    var beginColor = boxJson.ed_ProgbColor;
    var endColor = boxJson.ed_ProgeColor;
    var gradient = new gradientColor(beginColor, endColor, 101);

    var cindex = Math.floor((pc - boxJson.ed_PCMin) / (boxJson.ed_PCMax - boxJson.ed_PCMin) * 100)
    if (cindex > 100)
        cindex = 100;
    var radialObj = radialIndicator("#content_" + boxId, {
        radius: radius,
        barColor: gradient[cindex],
        barWidth: barWidth,
        initValue: pc,
        barBgColor: boxJson.ed_ProbgColor,
        minValue: boxJson.ed_PCMin,
        maxValue: boxJson.ed_PCMax,
        percentage: boxJson.ed_PCB * 1 == 1,
        displayNumber: boxJson.ed_showTitle * 1 == 1,
        fontFamily: boxJson.ed_fontFamily,
        fontSize: boxJson.ed_fontSize,
        fontColor: boxJson.ed_fontColor
    });
    //radialObj.value(pc);
}
function GenerateProgress(boxId, boxJson, res) {
    $("#title_" + boxId).css({ "font-family": boxJson.ed_fontFamily, "font-size": boxJson.ed_fontSize + "px", "color": boxJson.ed_fontColor, "text-align": boxJson.ed_fontAlign });
    var pc = 0, sc = 0;
    if (res.data.length > 0) {
        for (var key in res.data[0]) {
            pc = (res.data[0][key].toString());
            break;
        }
    }
    sc = pc;
    pc = pc * 100;

    var height = $("#draggable" + boxId).height() - $("#title_" + boxId).height() - 15;
    if (boxJson.ed_showTitle == 0) {
        $("#title_" + boxId).hide();
        height += $("#title_" + boxId).height();
    }
    else {
        $("#title_" + boxId).show();
    }
    var showtext = boxJson.ed_ProgressFormat;
    if (showtext.trim().length > 0)
        showtext = eval(showtext.replace(/{value}/g, sc));
    var beginColor = boxJson.ed_ProgbColor;
    var endColor = boxJson.ed_ProgeColor;
    var gradient = new gradientColor(beginColor, endColor, 101);
    var cindex = (pc.toFixed(0)) * 1;
    var color = gradient[cindex];
    $("#content_" + boxId).empty();
    $("#content_" + boxId).append('<div class="layui-progress" style="height:' + height + 'px;background-color:' + boxJson.ed_ProbgColor + '"><div class="layui-progress-bar layui-bg-red" style="background-color:' + color + ' !important;line-height:' + height + 'px;height:' + height + 'px;width:' + pc + '%" ><div>' + showtext + '</div></div></div>');
    //layui.element.render("progress");
    $("#draggable" + boxId).on("mouseup", function () {
        var height = $("#draggable" + boxId).height() - $("#title_" + boxId).height() - 15;
        if (boxJson.ed_showTitle == 0) {
            height += $("#title_" + boxId).height();
        }
        $("#draggable" + boxId + " .layui-progress").height(height)
        $("#draggable" + boxId + " .layui-progress .layui-progress-bar").height(height)
        $("#draggable" + boxId + " .layui-progress .layui-progress-bar").css("line-height", height + "px");
    })

}
function gradientColor(startColor, endColor, step) {
    startRGB = this.colorRgb(startColor);//转换为rgb数组模式
    startR = startRGB[0];
    startG = startRGB[1];
    startB = startRGB[2];

    endRGB = this.colorRgb(endColor);
    endR = endRGB[0];
    endG = endRGB[1];
    endB = endRGB[2];

    sR = (endR - startR) / step;//总差值
    sG = (endG - startG) / step;
    sB = (endB - startB) / step;

    var colorArr = [];
    for (var i = 0; i < step; i++) {
        //计算每一步的hex值 
        var hex = this.colorHex('rgb(' + parseInt((sR * i + startR)) + ',' + parseInt((sG * i + startG)) + ',' + parseInt((sB * i + startB)) + ')');
        colorArr.push(hex);
    }
    return colorArr;
}
// 将hex表示方式转换为rgb表示方式(这里返回rgb数组模式)
gradientColor.prototype.colorRgb = function (sColor) {
    var reg = /^#([0-9a-fA-f]{3}|[0-9a-fA-f]{6})$/;
    var sColor = sColor.toLowerCase();
    if (sColor && reg.test(sColor)) {
        if (sColor.length === 4) {
            var sColorNew = "#";
            for (var i = 1; i < 4; i += 1) {
                sColorNew += sColor.slice(i, i + 1).concat(sColor.slice(i, i + 1));
            }
            sColor = sColorNew;
        }
        //处理六位的颜色值
        var sColorChange = [];
        for (var i = 1; i < 7; i += 2) {
            sColorChange.push(parseInt("0x" + sColor.slice(i, i + 2)));
        }
        return sColorChange;
    } else {
        return sColor;
    }
};
// 将rgb表示方式转换为hex表示方式
gradientColor.prototype.colorHex = function (rgb) {
    var _this = rgb;
    var reg = /^#([0-9a-fA-f]{3}|[0-9a-fA-f]{6})$/;
    if (/^(rgb|RGB)/.test(_this)) {
        var aColor = _this.replace(/(?:(|)|rgb|RGB)*/g, "").split(",");
        var strHex = "#";
        for (var i = 0; i < aColor.length; i++) {
            var hex = Number(aColor[i]).toString(16);
            hex = hex < 10 ? 0 + '' + hex : hex;// 保证每个rgb的值为2位
            if (hex === "0") {
                hex += hex;
            }
            strHex += hex;
        }
        if (strHex.length !== 7) {
            strHex = _this;
        }
        return strHex;
    } else if (reg.test(_this)) {
        var aNum = _this.replace(/#/, "").split("");
        if (aNum.length === 6) {
            return _this;
        } else if (aNum.length === 3) {
            var numHex = "#";
            for (var i = 0; i < aNum.length; i += 1) {
                numHex += (aNum[i] + aNum[i]);
            }
            return numHex;
        }
    } else {
        return _this;
    }
}

function GenerateText(boxId, boxJson) {
    $("#title_" + boxId).show();
    $("#title_" + boxId).css({ "font-family": boxJson.ed_fontFamily, "font-size": boxJson.ed_fontSize + "px", "color": boxJson.ed_fontColor, "text-align": boxJson.ed_fontAlign });

}
function GenerateLabel(boxId, boxJson, res) {
    if (res.data.length > 0) {
        for (var key in res.data[0]) {
            console.log(res.data[0][key].toString());
            $("#content_" + boxId).html(res.data[0][key].toString());
        }
    }
    $("#content_" + boxId).css({ "font-family": boxJson.ed_fontFamily, "font-size": boxJson.ed_fontSize + "px", "color": boxJson.ed_fontColor, "text-align": boxJson.ed_fontAlign });
    if (boxJson.ed_showTitle == 0) {
        $("#title_" + boxId).hide();
    }
    else {
        $("#title_" + boxId).show();
    }
}
function GenerateTable(boxId, boxJson, res) {
    $("#title_" + boxId).hide();
    $("#content_" + boxId).empty();
    $("#content_" + boxId).append("<table id='tbSub_" + boxId + "' lay-filter='tbSub_" + boxId + "' ></table>")
    var tool;
    if (boxJson.ed_showExport == 1) {
        tool = "defult";
    }
    else
        tool = false;
    //    $("#content_" + boxId).prepend("<div class='layui-table-tool'><div class='layui-table-tool-self'><button id='btn_Export' type='button' class='layui-btn layui-btn-normal'>@Localizer["btn_Export"]</button></div></div>");
    //}
    //$("#btn_Export").on("click", function () {
    //    layui.table.exportFile(ins1.config.id, res.data, 'xls');
    //})
    var colsFormat = boxJson.ed_tableColFormat;
    if (colsFormat == undefined) {
        colsFormat = "";
    }
    var head = '[[';
    for (var key in res.data[0]) {
        var align = "left";
        if (colsFormat.indexOf(key) > -1) {
            align = eval("boxJson.colAlign_" + key);
        }
        var hide = false;
        if (colsFormat.indexOf(key) > -1) {
            hide = eval("boxJson.colHide_" + key) == 1;
        }
        var width = "";
        if (colsFormat.indexOf(key) > -1) {
            var expWidth = eval("boxJson.colWidth_" + key);
            if (expWidth != undefined && expWidth != null && expWidth != "") {
                width = expWidth;
            }
        }
        if (width.length > 0)
            head += '{"field":"' + key + '","title":"' + key + '","align":"' + align + '","hide":' + hide + ',"width":' + width + '},';
        else {
            head += '{"field":"' + key + '","title":"' + key + '","align":"' + align + '","hide":' + hide + '},';
        }
    }
    head = head.substr(0, head.length - 1);
    head += "]]";
    //console.log(res.data.length)
    var laypage;
    if (boxJson.ed_showPage == 1) {
        laypage = {
            count: res.data.length,
            limit: boxJson.ed_Pagelimit,
            layout: ['count', 'prev', 'page', 'next', 'skip']
        };
    }
    else
        laypage = false;
    
    var ins1=layui.table.render({
        elem: '#tbSub_' + boxId, //指定原始表格元素选择器（推荐id选择器）
        id:boxId,
        height: $("#draggable" + boxId).height(), //容器高度
        cols: JSON.parse(head), //设置表头
        data: res.data,
        page: laypage,
        limit: 90,
        toolbar:tool,
        skin: boxJson.ed_tableSkin,
        size: boxJson.ed_tableSize,
        even: (boxJson.ed_tableEven == "true"),
        done: function (res, curr, count) {
            var cssList = "#draggable" + boxId + " .layui-table[lay-even] tr:nth-child(even){background-color:" + boxJson.ed_tableEvColor + "}";
            cssList += "#draggable" + boxId + " .layui-table thead tr{background-color:" + boxJson.ed_tableEvColor + "}";
            if (laypage != false) {
                cssList += "#draggable" + boxId + " .layui-table-body.layui-table-main{overflow:auto}";
            }
            if ($("#cssMain").html().indexOf(cssList) < 0)
                $("#cssMain").append(cssList);
            $("#draggable" + boxId + " .layui-table-cell").css({ "font-family": boxJson.ed_fontFamily, "font-size": boxJson.ed_fontSize + "px", "color": boxJson.ed_fontColor });
            $("#draggable" + boxId + " .layui-table[lay-even] tr:nth-child(even)").css({ "background-color": boxJson.ed_tableEvColor });
            $("#draggable" + boxId + " .layui-table-view .layui-table").css({ "background-color": boxJson.ed_tableBgColor });
            $("#draggable" + boxId + " .layui-table thead tr").css({ "background-color": boxJson.ed_tableEvColor });
            var cellHeight = $("#draggable" + boxId + " .layui-table-view .layui-table .layui-table-cell").height();
            if (boxJson.ed_fontSize * 1 > cellHeight) {
                var heog = $("#draggable" + boxId + " .layui-table-view .layui-table .layui-table-cell span").height();
                $("#draggable" + boxId + " .layui-table-view .layui-table .layui-table-cell").css({ "height": heog + "px", "line-height": heog + "px" });
            }
            if (boxJson.ed_showTitle == 0) {
                $("#draggable" + boxId + " .layui-table-header").css({ "display": "none" });
                $("#draggable" + boxId + " .layui-table-view .layui-table-body.layui-table-main").height($("#draggable" + boxId + " .layui-table-view").height());
            }
            if (boxJson.ed_tableSkin == "nob") {
                $("#draggable" + boxId + " .layui-table-view").css("border-color", "transparent");
            }
            var trs = $("#draggable" + boxId + " .layui-table-view .layui-table-body.layui-table-main table tbody tr");
            var cols = colsFormat.split(',');
            for (var i = 0; i < trs.length; i++) {
                for (var j = 0; j < cols.length; j++) {
                    var count = eval("boxJson.ed_ConditionCount_" + cols[j]);
                    for (var k = 0; k < count; k++) {
                        var condition = eval("boxJson.colCondition_" + k + "_" + cols[j]);
                        var applyType = eval("boxJson.colApplyType_" + k + "_" + cols[j]);
                        if (condition.length > 0) {
                            var value = $(trs[i]).find("td[data-field='" + cols[j] + "'] div").text();
                            condition = condition.replace(/{value}/g, value);
                            //console.log(condition);
                            if (eval(condition)) {
                                if (applyType * 1 == 0) {
                                    $(trs[i]).find("td[data-field='" + cols[j] + "']").addClass(eval("boxJson.colbgColor_" + k + "_" + cols[j]));
                                    $(trs[i]).find("td[data-field='" + cols[j] + "'] div").addClass(eval("boxJson.coltextColor_" + k + "_" + cols[j]));
                                }
                                else {
                                    $(trs[i]).find("td").addClass(eval("boxJson.colbgColor_" + k + "_" + cols[j]));
                                    $(trs[i]).find("td div").addClass(eval("boxJson.coltextColor_" + k + "_" + cols[j]));
                                }
                            }
                        } else {
                            if (applyType * 1 == 0) {
                                $(trs[i]).find("td[data-field='" + cols[j] + "']").addClass(eval("boxJson.colbgColor_" + k + "_" + cols[j]));
                                $(trs[i]).find("td[data-field='" + cols[j] + "'] div").addClass(eval("boxJson.coltextColor_" + k + "_" + cols[j]));
                            }
                            else {
                                $(trs[i]).find("td").addClass(eval("boxJson.colbgColor_" + k + "_" + cols[j]));
                                $(trs[i]).find("td div").addClass(eval("boxJson.coltextColor_" + k + "_" + cols[j]));
                            }
                        }
                    }

                }
            }
            //if($("#draggable" + boxId + " .looping").val()==0)
            
            LoopData(boxId, boxJson, res.data.length);

        }
    });
}
function LoopData(boxId, boxJson, totalRow) {
    var loopi = 0;
    var perHeight = $("#draggable" + boxId + " .layui-table-body.layui-table-main table tr").height();
    if (boxJson.ed_tableChangeType == 2) {
        if ($("#draggable" + boxId + " .looping").val() == 0) {
            //clearTimeout();
            $("#draggable" + boxId + " .looping").val(1);
            var totalTime = boxJson.ed_freshTime;
            var totalHeight = $("#draggable" + boxId).height();
            var needHeight = $("#draggable" + boxId + " .layui-table-body.layui-table-main table").height();
            var top = $("#draggable" + boxId + " .layui-table-body.layui-table-main").scrollTop();
            var outHeight = needHeight - (totalHeight - perHeight);
            if ((top + totalHeight - perHeight) <= needHeight) {
                if (loopi == 0) {
                    setInterval(function () {
                        loopi++;
                        top = $("#draggable" + boxId + " .layui-table-body.layui-table-main").scrollTop();
                        $("#draggable" + boxId + " .layui-table-body.layui-table-main").animate({ 'scrollTop': loopi * perHeight }, 0);
                        if ((Math.ceil(outHeight / perHeight) + 1 == loopi)) {
                            $("#draggable" + boxId + " .layui-table-body.layui-table-main").animate({ 'scrollTop': 0 }, 0);
                            loopi = 0;
                            //console.log(loopi)
                        }
                    }, (totalTime) / (Math.ceil(outHeight / perHeight) + 1));
                }
            }
        }
        else {
            $("#draggable" + boxId + " .layui-table-body.layui-table-main").animate({ 'scrollTop': loopi * perHeight }, 0);
        }
    }
    if (boxJson.ed_tableChangeType == 1) {
        //if ($("#draggable" + boxId + " .looping").val() == 0) {
        //clearTimeout();
        $("#draggable" + boxId + " .looping").val(1);
        var totalTime = boxJson.ed_freshTime;
        var totalHeight = $("#draggable" + boxId).height();
        $("#draggable" + boxId + " .layui-table-body.layui-table-main").prepend("<div style='width:100%;height:" + totalHeight + "px'></div>");
        $("#draggable" + boxId + " .layui-table-body.layui-table-main").append("<div style='width:100%;height:" + totalHeight + "px'></div>");
        var needHeight = $("#draggable" + boxId + " .layui-table-body.layui-table-main table").height();
        //var top = $("#draggable" + boxId + " .layui-table-body.layui-table-main").scrollTop();
        var outHeight = needHeight + (totalHeight);
        setTimeout(function () {
            $("#draggable" + boxId + " .layui-table-body.layui-table-main").animate({ 'scrollTop': outHeight }, boxJson.ed_freshTime * 1, "linear");

        }, 0);

        //}//
    }
}

function GenerateImage(boxId, boxJson, res) {
    $("#content_" + boxId).empty();
    var imgList = JSON.parse(boxJson.ed_imgList);
    var imgSize = boxJson.ed_imgSize;
    $("#title_" + boxId).css({ "font-family": boxJson.ed_fontFamily, "font-size": boxJson.ed_fontSize + "px", "color": boxJson.ed_fontColor, "text-align": boxJson.ed_fontAlign });
    var allHeight = $("#draggable" + boxId).height();
    var titleHeight = $("#title_" + boxId).height();
    var picHeight = allHeight - titleHeight - 10;
    if (boxJson.ed_showTitle == 0) {
        $("#title_" + boxId).hide();
        picHeight = allHeight;
    }
    else {
        $("#title_" + boxId).show();
    }
    if (imgList.length > 1 && res != undefined && res != null && (res.code == 200)) {
        var imgChoosedName = "";
        for (var key in res.data) {
            imgChoosedName = (res.data[0][key]);
        }
        for (var k = 0; k < imgList.length; k++) {
            if (imgList[k].FileName == imgChoosedName) {
                $("#content_" + boxId).append("<img src='../content/Upload/" + imgList[k].RelativeUrl + "' />");
                switch (boxJson.ed_imgSize) {
                    case "A":
                        $("#content_" + boxId + " img").css({ "width": "100%", "height": picHeight + "px" });
                        break;
                    case "W":
                        $("#content_" + boxId + " img").css({ "height": picHeight + "px" });
                        break;
                    case "H":
                        $("#content_" + boxId + " img").css({ "width": "100%" });
                        break;
                }
            }
        }
    }
    else {
        $("#content_" + boxId).append("<img src='../content/Upload/" + imgList[0].RelativeUrl + "' />");
        switch (boxJson.ed_imgSize) {
            case "A":
                $("#content_" + boxId + " img").css({ "width": "100%", "height": picHeight + "px" });
                break;
            case "W":
                $("#content_" + boxId + " img").css({ "height": picHeight + "px" });
                break;
            case "H":
                $("#content_" + boxId + " img").css({ "width": "100%" });
                break;
        }

    }
}

function GenerateLine(boxId, boxJson, res) {
    $("#title_" + boxId).hide();
    $("#content_" + boxId).height($("#draggable" + boxId).height());
    var needboundaryGap = false;
    for (var key in boxJson) {
        if (boxJson[key] == "bar")
            needboundaryGap = true;
    }
    var xtemp = {
        name: "",
        type: boxJson.ed_ChartXType,
        boundaryGap: needboundaryGap,
        data: [],
        splitLine: {
            show: boxJson.ed_ChartXSL * 1 == 1
        }
    };
    var ytemp = {
        name: "",
        type: boxJson.ed_ChartYType,
        boundaryGap: needboundaryGap,
        data: [],
        splitLine: {
            show: boxJson.ed_ChartYSL * 1 == 1
        }
    };
    var dtemp = {
        name: "",
        data: [],
        type: 'line',
        smooth: false,
        barCategoryGap: '50%',
        stack: "",
        label: {
            normal: {
                show: false,
                position: 'inside',
                "formatter": (param => param.value > 0 ? param.value : "")
            }
        },
        itemStyle: {
            color: ""
        },
        areaStyle: null
    };

    var xAxisData = [];
    var yAxisData = [];
    var sData = [];
    var legendData = [];
    var xFields = boxJson.ed_ChartXField.toString().split(',');
    var yFields = boxJson.ed_ChartYField.toString().split(',');
    var dFields = boxJson.ed_ChartDField.toString().split(',');
    for (var j = 0; j < xFields.length; j++) {
        xtemp.name = xFields[j]
        xAxisData.push(JSON.parse(JSON.stringify(xtemp)));
    }
    for (var j = 0; j < yFields.length; j++) {
        ytemp.name = yFields[j]
        yAxisData.push(JSON.parse(JSON.stringify(ytemp)));
    }
    for (var j = 0; j < dFields.length; j++) {
        var dd = deepCopy(dtemp);
        dd.name = dFields[j]
        sData.push(dd);//JSON.parse(JSON.stringify()));
    }
    for (var j = 0; j < xAxisData.length; j++) {
        if (xAxisData[j].name != "{bt}") {
            for (var i = 0; i < res.data.length; i++) {
                xAxisData[j].data.push(res.data[i][xAxisData[j].name]);
            }
        }
        else {
            xAxisData[j].name = "";
            for (var key in res.data[0]) {
                if (yFields.indexOf(key) < 0)
                    xAxisData[j].data.push(key);
            }
        }
        if (xAxisData.length == 1) {
            xAxisData[j].name = "";
        }
        xAxisData[j].axisLabel = {};
        xAxisData[j].axisLabel.rotate = boxJson.ed_ChartXAngle * 1;
        xAxisData[j].axisLabel.show = boxJson.ed_ChartXLabel * 1 != 0;
        xAxisData[j].axisLine = {};
        xAxisData[j].axisLine.lineStyle = {};
        if (boxJson.ed_ChartXColor != undefined && boxJson.ed_ChartXColor != null && boxJson.ed_ChartXColor != "") {
            xAxisData[j].axisLine.lineStyle.color = boxJson.ed_ChartXColor;
            //xAxisData[j].axisLine.lineStyle.opacity = 0.6;
        }
        if (boxJson.ed_ChartXMax * 1 != 0) {
            xAxisData[j].max = boxJson.ed_ChartXMax * 1;
        }
    }

    for (var j = 0; j < yAxisData.length; j++) {
        if (yAxisData[j].name != "{bt}") {
            for (var i = 0; i < res.data.length; i++) {
                yAxisData[j].data.push(res.data[i][yAxisData[j].name]);
            }
        }
        else {
            yAxisData[j].name = "";
            for (var key in res.data[0]) {
                if (xFields.indexOf(key) < 0)
                    yAxisData[j].data.push(key);
            }
        }
        if (yAxisData.length == 1) {
            yAxisData[j].name = "";
        }
        yAxisData[j].axisLabel = {};
        yAxisData[j].axisLabel.rotate = boxJson.ed_ChartYAngle * 1;
        yAxisData[j].axisLabel.show = boxJson.ed_ChartYLabel * 1 != 0
        yAxisData[j].axisLine = {};
        yAxisData[j].axisLine.lineStyle = {};
        if (boxJson.ed_ChartYColor != undefined && boxJson.ed_ChartYColor != null && boxJson.ed_ChartYColor != "") {
            yAxisData[j].axisLine.lineStyle.color = boxJson.ed_ChartYColor;
        }
        //yAxisData[j].axisLine.lineStyle.opacity = 0.6;
        if (boxJson.ed_ChartYMax * 1 != 0) {
            yAxisData[j].max = boxJson.ed_ChartYMax * 1;
        }
    }
    for (var j = 0; j < sData.length; j++) {
        for (var i = 0; i < res.data.length; i++) {
            sData[j].data.push(res.data[i][sData[j].name]);
        }
        sData[j].type = boxJson["type_" + sData[j].name];
        sData[j].smooth = eval(boxJson["smooth_" + sData[j].name]);
        sData[j].areaStyle = JSON.parse(boxJson["areaStyle_" + sData[j].name]);
        sData[j].stack = eval(boxJson["stackName_" + sData[j].name]);
        var showType = (boxJson["labelShow_" + sData[j].name]);
        var barcolor = (boxJson["itemColor_" + sData[j].name]);
        var isShow = showType != "false";
        if (showType == undefined || showType == null)
            isShow = false;
        if (barcolor == undefined || barcolor == null)
            barcolor = "";
        sData[j].label.normal.show = isShow;
        sData[j].label.normal.position = showType;
        sData[j].itemStyle.color = barcolor;
        legendData.push(sData[j].name);
    }
    //console.log(legendData)
    var myChart = echarts.init(document.getElementById('content_' + boxId), "walden");
    var option = {
        legend: {
            //left:"right",
            show: boxJson.ed_chartLegend * 1 == 1,
            data: legendData,
            top: "5%",
            right: "5%"
        },
        tooltip: {
            show: true,
            trigger: "axis"
        },
        title: {
            show: boxJson.ed_showTitle * 1 == 1,
            text: boxJson.ed_title,
            textStyle: {
                color: boxJson.ed_fontColor,
                fontSize: boxJson.ed_fontSize,
                fontFamily: boxJson.ed_fontFamily
            }
        },
        grid: {
            left: "5%",
            right: "7%",
            top: "18%",
            bottom: 30,
            containLabel: true
        },
        xAxis: xAxisData,
        yAxis: yAxisData,
        series: sData
    };
    //console.log(option)
    myChart.setOption(option);
}

function GeneratePie(boxId, boxJson, res) {
    $("#title_" + boxId).hide();
    $("#content_" + boxId).height($("#draggable" + boxId).height());
    var data = res.data;
    var seriesData = [];
    for (var i = 0; i < data.length; i++) {
        //console.log(data[i])
        var name = "";
        var value = 0;
        var j = 0;
        for (var key in data[i]) {
            if (j == 0)
                name = data[i][key];
            if (j == 1)
                value = data[i][key];
            j++;
        }
        seriesData.push({ name: name, value: value });
    }
    //console.log(seriesData)
    var myChart = echarts.init(document.getElementById('content_' + boxId), "walden");
    var option = {
        title: {
            show: boxJson.ed_showTitle * 1 == 1,
            text: boxJson.ed_title,
            textStyle: {
                color: boxJson.ed_fontColor,
                fontSize: boxJson.ed_fontSize,
                fontFamily: boxJson.ed_fontFamily
            }
        },
        tooltip: {
            trigger: 'item',
            formatter: "{a} <br/>{b}: {c} ({d}%)"
        },
        legend: {
            show: boxJson.ed_chartLegend * 1 == 1,
            orient: 'vertical',
            x: 'right',
            //data: ['直接访问', '邮件营销', '联盟广告', '视频广告', '搜索引擎']
        },
        series: [
            {
                name: boxJson.ed_title,
                type: 'pie',
                radius: [boxJson.ed_chartPieInner + '%', boxJson.ed_chartPieOuter + '%'],
                roseType: boxJson.ed_ChartPNDGE == "false" ? false : boxJson.ed_ChartPNDGE,//area
                label: {
                    normal: {
                        show: boxJson.ed_chartPieLabel * 1 == 1
                    }
                },
                labelLine: {
                    normal: {
                        show: boxJson.ed_chartPieLabel * 1 == 1
                    }
                },
                data: seriesData
            }
        ]
    };
    myChart.setOption(option);
}
function GenerateGauge(boxId, boxJson, res) {
    $("#title_" + boxId).hide();
    $("#content_" + boxId).height($("#draggable" + boxId).height());
    var data = res.data;
    var seriesData = [];
    for (var i = 0; i < data.length; i++) {
        //console.log(data[i])
        for (var key in data[i]) {
            seriesData.push({ name: key, value: (data[i][key] * 1).toFixed(2) });
            break;
        }
        break;
    }
    var colorspan = [];
    for (var i = 0; i < boxJson.colGaugeCCount * 1; i++) {
        colorspan.push([eval("boxJson.colGaugeC_" + i) * 1, eval("boxJson.colGaugeColor_" + i)])
    }
    if (colorspan.length == 0) {
        colorspan = [[0.2, '#91c7ae'], [0.8, '#63869e'], [1, '#c23531']];
    }
    //console.log(colorspan)
    var myChart = echarts.init(document.getElementById('content_' + boxId), "walden");
    var option = {

        //backgroundColor: '#1b1b1b',
        tooltip: {
            formatter: "{a} <br/>{c} {b}"
        },
        series: [
            {
                name: boxJson.ed_title,
                type: 'gauge',
                clockwise: boxJson.ed_chartGaugeClockwise * 1 != 0,
                startAngle: boxJson.ed_chartGaugeAngleStart,
                endAngle: boxJson.ed_chartGaugeAngleEnd,
                min: boxJson.ed_chartGaugeMin * 1,
                max: boxJson.ed_chartGaugeMax * 1,
                splitNumber: boxJson.ed_chartGaugeSplitNumber * 1 == 0 ? -1 : boxJson.ed_chartGaugeSplitNumber * 1,
                center: ['50%', '50%'],
                radius: '90%',
                axisLine: {            // 坐标轴线
                    lineStyle: {       // 属性lineStyle控制线条样式
                        width: boxJson.ed_chartGaugeLineWidth,
                        color: colorspan.sort(),
                    }
                },
                splitLine: {           // 分隔线
                    length: boxJson.ed_chartGaugeTickWidth + "%",         // 属性length控制线长
                    lineStyle: {       // 属性lineStyle（详见lineStyle）控制线条样式
                        color: 'auto'
                    }
                },
                axisLabel: {
                    backgroundColor: boxJson.ed_chartGaugeTickBg,
                    borderRadius: 2,
                    color: boxJson.ed_chartGaugeTickBg == "auto" ? "#eee" : "auto",
                    padding: 3,
                },
                axisTick: {            // 坐标轴小标记
                    length: boxJson.ed_chartGaugeTickWidth + "%",        // 属性length控制线长
                    lineStyle: {       // 属性lineStyle控制线条样式
                        color: 'auto',
                        //shadowColor : '#fff', //默认透明
                        //shadowBlur: 1
                    }
                },
                title: {
                    show: boxJson.ed_showTitle * 1 == 1,
                    text: boxJson.ed_title,
                    textStyle: {
                        color: boxJson.ed_fontColor,
                        fontSize: boxJson.ed_fontSize,
                        fontFamily: boxJson.ed_fontFamily
                    }
                },
                detail: {
                    //backgroundColor: 'rgba(30,144,255,0.8)',
                    //borderWidth: 1,
                    //borderColor: '#fff',
                    //shadowColor : '#fff', //默认透明
                    //shadowBlur: 5,
                    //offsetCenter: [0, '50%'],       // x, y，单位px
                    formatter: boxJson.ed_chartGaugeFormat,
                    textStyle: {       // 其余属性默认使用全局文本样式，详见TEXTSTYLE
                        fontWeight: 'bolder',
                        fontSize: boxJson.ed_chartGaugeFontSize
                        //color: '#fff'
                    }
                },
                data: seriesData
            }
        ]
    };
    //console.log(JSON.stringify(option))
    myChart.setOption(option);
}
function GenerateLayer(boxId, boxJson, res) {
    var layer = layui.layer;
    layer.open({
        title: boxJson.ed_title,
        id: "lay" + boxId,
        maxmin: true,
        shade: false,
        shadeClose: true,
        type: 2,
        content: boxJson.ed_LayerContent,
        success: function (layero, index) {
            setTimeout(function () { layer.iframeAuto(index) }, 1000);

        }
    })
}
function GenerateRadar(boxId, boxJson, res) {
    $("#title_" + boxId).hide();
    $("#content_" + boxId).height($("#draggable" + boxId).height());
    var data = res.data;
    var indicator = [];
    for (var i = 0; i < data.length; i++) {
        //console.log(data[i])
        if (data[i]['row'] == 'max') {
            for (var key in data[i]) {
                if (key != 'row') {
                    indicator.push({ text: key, max: data[i][key] });
                }
            }
        }

    }

    //console.log(data)
    var myChart = echarts.init(document.getElementById('content_' + boxId), "walden");
    var option = {

        //backgroundColor: '#1b1b1b',
        tooltip: {
            trigger: 'item',
            backgroundColor: 'rgba(0,0,0,0.2)'
        },
        radar: {
            indicator: indicator,
            shape: boxJson.ed_RadarShape,
            name: {
                textStyle: {
                    color: boxJson.ed_fontColor,
                    fontSize: boxJson.ed_fontSize,
                    fontFamily: boxJson.ed_fontFamily
                }
            }
        },
        series: (function () {
            var series = [];
            for (var i = 0; i < data.length; i++) {
                let values = [];
                if (data[i]['row'] != 'max') {
                    for (var key in data[i]) {
                        if (key != 'row') {
                            values.push(data[i][key]);
                        }
                    }
                    series.push({
                        name: data[i]['row'],
                        type: 'radar',
                        symbol: 'none',
                        lineStyle: {
                            width: 1
                        },
                        areaStyle: {
                            opacity: 0.5
                        },
                        data: [
                            {
                                value: values,
                                name: data[i]['row']
                            }
                        ]
                    });
                }

            }
            return series;
        })()
    };
    //console.log(JSON.stringify(option))
    myChart.setOption(option);
}
function GenerateSelect(boxId, boxJson, res) {
    var data = res.data;
    $("#content_" + boxId).empty();
    if (data.length > 0) {
        var content = '<select id="tbSub_' + boxId + '" class="layui-select" name="select1">';
        for (var i = 0; i < data.length; i++) {
            for (var key in data[i]) {
                content += '<option value="' + data[i][key] + '">' + data[i][key] + '</option>';
            }
        }
        $("#content_" + boxId).append(content);
    }
    $("#content_" + boxId + "select").css({ "width": "100%", "height": $("#draggable" + boxId).height() + "px" });
    $("#title_" + boxId).show();
}
function GenerateInput(boxId, boxJson, res) {
    $("#content_" + boxId).empty();
    $("#content_" + boxId).append("<input id='tbSub_" + boxId + "' type='text' class='layui-input' ></input>");
    $("#content_" + boxId + "input").css({ "width": "100%", "height": $("#draggable" + boxId).height() + "px" });
}
function GenerateButton(boxId, boxJson, res) {
    $("#content_" + boxId).empty();
    if (res.data.length > 0) {
        for (var key in res.data[0]) {
            $("#content_" + boxId).append("<button id='tbSub_" + boxId + "' type='button' class='layui-btn layui-btn-normal'>" + res.data[0][key] + "</button>");
        }
    }
    $("#content_" + boxId + "button").css({ "width": "100%", "height": $("#draggable" + boxId).height() + "px" });
}