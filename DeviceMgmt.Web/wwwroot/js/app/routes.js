routes = [
    {
        path: '/',
        url: './default',
        on: {
            pageInit: function (e, page) {
                this.app.request.json('./CheckMoldStaffAuth/', function (res) {
                    if (res.code == 200) {
                        $("#moldDispatch").removeClass("disabled")
                    }
                })
            }
        }
    },
    {
        path: '/EventList/',
        url: './EventList',
        on: {
            pageInit: function (e, page) {
                setInterval(() => {
                    try {
                        app.request.get("./GetMyEvent", { page: 0, limit: 1000, messageStatus: 0 }, function (res) {
                            var count = JSON.parse(res).count;
                            if (count == 0) {
                                count = ""
                            }
                            $("#myeventnum").text(count)
                        });
                    } catch (e) {}
                }, 5000)

                $("#AllEventList").click(function () {
                    $("#hd_EventStatus").val(-1)
                    initMegList();
                })
                $("#UnReadEventList").click(function () {
                    $("#hd_EventStatus").val(0)
                    initMegList();
                })
                let $ptrContent = $$('#eventPreContent');

                let generateMsg = function (data, callback) {

                    var itemHTML = "";
                    for (var i = 0; i < data.length; i++) {
                        let msgSta = "";
                        if (data[i].SendStatus == 0)
                            msgSta = '<span class="layui-badge-dot layui-bg-green"></span> ';
                        itemHTML += '<li>'
                            + '  <a href="/EventDetail/' + data[i].Id + '" class="item-link item-content">'
                            + '    <div class="item-inner">'
                            + '      <div class="item-title-row">'
                            + ('        <div class="item-title">' + msgSta + data[i].CategoryName + '</div>')
                            + ('        <div class="item-after">' + data[i].ReportTime + '</div>')
                            + '      </div>'
                            //+ '      <div class="item-subtitle">New messages from John Doe</div>'
                            + ('      <div class="item-text">' + data[i].FacilityName + '(' + data[i].FacilityCode + ')' + '</div>')
                            + '    </div>'
                            + '  </a>'
                            + '</li>';
                    }

                    callback(itemHTML)
                }
                let allowInfinite = true;

                // Last loaded index
                let lastItemIndex = 1;

                // Max items to load
                let maxItems = 200;

                // Append items per load
                let itemsPerLoad = 20;
                let initMegList = function () {
                    $ptrContent.find('ul').html("");
                    app.request.get("./GetMyEvent", { page: lastItemIndex, limit: itemsPerLoad, messageStatus: $("#hd_EventStatus").val() }, function (res) {
                        res = JSON.parse(res);
                        maxItems = res.count;
                        if ($("#hd_EventStatus").val() == -1)
                            $("#allEvent").html(maxItems);
                        $("#unEvent").html(res.msg)
                        if (lastItemIndex * itemsPerLoad >= maxItems) {
                            // Nothing more to load, detach infinite scroll events to prevent unnecessary loadings
                            //app.infiniteScroll.destroy('.ptr-content');
                            // Remove preloader
                            $$('#eventloader').hide();
                        }
                        if (maxItems > 0) {
                            let data = res.data.sort((pre, nex) => { return new Date(nex.ReportTime) - new Date(pre.ReportTime) })
                            generateMsg(data, function (itemHTML) {
                                $ptrContent.find('ul').prepend(itemHTML);
                                app.ptr.done();
                            });
                        }
                        else {
                            $ptrContent.find('ul').prepend("<li style='padding:10px;text-align:center'><h4>没有事件</h4></li>");
                            $$('#eventloader').hide();
                            app.ptr.done();
                        }
                    });
                }
                initMegList();
                // Add 'refresh' listener on it
                $ptrContent.on('ptr:refresh', function (e) {
                    lastItemIndex = 1;
                    // Emulate 2s loading
                    $ptrContent.find('ul').html("");
                    app.request.get("./GetMyEvent", { page: lastItemIndex, limit: itemsPerLoad, messageStatus: $("#hd_EventStatus").val() }, function (res) {
                        res = JSON.parse(res);
                        maxItems = res.count;
                        if ($("#hd_EventStatus").val() == -1)
                            $("#allEvent").html(maxItems);
                        $("#unEvent").html(res.msg)
                        if (lastItemIndex * itemsPerLoad >= maxItems) {
                            // Nothing more to load, detach infinite scroll events to prevent unnecessary loadings
                            //app.infiniteScroll.destroy('.ptr-content');
                            // Remove preloader
                            $$('#eventloader').hide();
                        }
                        if (maxItems > 0) {
                            generateMsg(res.data, function (itemHTML) {
                                $ptrContent.find('ul').prepend(itemHTML);
                                app.ptr.done();
                            });
                            allowInfinite = true;
                            initInfinite();
                        }
                        else {
                            $ptrContent.find('ul').prepend("<li style='padding:10px;text-align:center'><h4>没有事件</h4></li>");
                            $$('#eventloader').hide();
                            app.ptr.done();
                        }
                    });
                });
                // Loading flag

                let initInfinite = function () {
                    // Attach 'infinite' event handler
                    $ptrContent.on('infinite', function () {
                        // Exit, if loading in progress
                        if (!allowInfinite) return;

                        // Set loading flag
                        allowInfinite = false;
                        lastItemIndex++;
                        if (lastItemIndex * itemsPerLoad >= maxItems) {
                            // Nothing more to load, detach infinite scroll events to prevent unnecessary loadings
                            //app.infiniteScroll.destroy('.ptr-content');
                            // Remove preloader
                            $$('#msgloader').hide();
                            return;
                        }
                        else {
                            $$('#msgloader').show();
                            app.request.get("./GetMyMessage", { page: lastItemIndex, limit: itemsPerLoad, messageStatus: $("#hd_MsgStatus").val() }, function (res) {
                                res = JSON.parse(res);
                                allowInfinite = true;
                                maxItems = res.count;
                                generateMsg(res.data, function (itemHTML) {
                                    $ptrContent.find('ul').append(itemHTML);
                                });
                            });
                        }
                    });
                }
                initInfinite();
            }
        }
    },
    {
        path: '/MessageList/',
        url: './MessageList',
        on: {
            pageInit: function (e, page) {
                $("#AllMsgList").click(function () {
                    $("#hd_MsgStatus").val(-1)
                    initMegList();
                })
                $("#UnReadMsgList").click(function () {
                    $("#hd_MsgStatus").val(0)
                    initMegList();
                })
                let $ptrContent = $$('#msgPreContent');

                let generateMsg = function (data, callback) {

                    var itemHTML = "";
                    for (var i = 0; i < data.length; i++) {
                        let msgSta = "";
                        if (data[i].SendStatus == 0)
                            msgSta = '<span class="layui-badge-dot layui-bg-green"></span> ';
                        itemHTML += '<li>'
                            + '  <a href="/MessageView/' + data[i].Id + '" class="item-link item-content">'
                            + '    <div class="item-inner">'
                            + '      <div class="item-title-row">'
                            + ('        <div class="item-title">' + msgSta + data[i].Title + '</div>')
                            + ('        <div class="item-after">' + data[i].SendTime + '</div>')
                            + '      </div>'
                            //+ '      <div class="item-subtitle">New messages from John Doe</div>'
                            + ('      <div class="item-text">' + data[i].MsgContent + '</div>')
                            + '    </div>'
                            + '  </a>'
                            + '</li>';
                    }

                    callback(itemHTML)
                }
                let allowInfinite = true;

                // Last loaded index
                let lastItemIndex = 1;

                // Max items to load
                let maxItems = 200;

                // Append items per load
                let itemsPerLoad = 20;
                let initMegList = function () {
                    $ptrContent.find('ul').html("");
                    app.request.get("./GetMyMessage", { page: lastItemIndex, limit: itemsPerLoad, messageStatus: $("#hd_MsgStatus").val() }, function (res) {
                        res = JSON.parse(res);
                        maxItems = res.count;
                        if ($("#hd_MsgStatus").val() == -1)
                            $("#allMsg").html(maxItems);
                        $("#unMsg").html(res.msg)
                        if (lastItemIndex * itemsPerLoad >= maxItems) {
                            // Nothing more to load, detach infinite scroll events to prevent unnecessary loadings
                            //app.infiniteScroll.destroy('.ptr-content');
                            // Remove preloader
                            $$('#msgloader').hide();
                        }
                        if (maxItems > 0) {
                            generateMsg(res.data, function (itemHTML) {
                                $ptrContent.find('ul').prepend(itemHTML);
                                app.ptr.done();
                            });
                        }
                        else {
                            $ptrContent.find('ul').prepend("<li style='padding:10px;text-align:center'><h4>没有消息</h4></li>");
                            $$('#msgloader').hide();
                            app.ptr.done();
                        }
                    });
                }
                initMegList();
                // Add 'refresh' listener on it
                $ptrContent.on('ptr:refresh', function (e) {
                    lastItemIndex = 1;
                    // Emulate 2s loading
                    $ptrContent.find('ul').html("");
                    app.request.get("./GetMyMessage", { page: lastItemIndex, limit: itemsPerLoad, messageStatus: $("#hd_MsgStatus").val() }, function (res) {
                        res = JSON.parse(res);
                        maxItems = res.count;
                        if ($("#hd_MsgStatus").val() == -1)
                            $("#allMsg").html(maxItems);
                        $("#unMsg").html(res.msg)
                        if (lastItemIndex * itemsPerLoad >= maxItems) {
                            // Nothing more to load, detach infinite scroll events to prevent unnecessary loadings
                            //app.infiniteScroll.destroy('.ptr-content');
                            // Remove preloader
                            $$('#msgloader').hide();
                        }
                        if (maxItems > 0) {
                            generateMsg(res.data, function (itemHTML) {
                                $ptrContent.find('ul').prepend(itemHTML);
                                app.ptr.done();
                            });
                            allowInfinite = true;
                            initInfinite();
                        }
                        else {
                            $ptrContent.find('ul').prepend("<li style='padding:10px;text-align:center'><h4>没有消息</h4></li>");
                            $$('#msgloader').hide();
                            app.ptr.done();
                        }
                    });
                });
                // Loading flag

                let initInfinite = function () {
                    // Attach 'infinite' event handler
                    $ptrContent.on('infinite', function () {
                        // Exit, if loading in progress
                        if (!allowInfinite) return;

                        // Set loading flag
                        allowInfinite = false;
                        lastItemIndex++;
                        if (lastItemIndex * itemsPerLoad >= maxItems) {
                            // Nothing more to load, detach infinite scroll events to prevent unnecessary loadings
                            //app.infiniteScroll.destroy('.ptr-content');
                            // Remove preloader
                            $$('#msgloader').hide();
                            return;
                        }
                        else {
                            $$('#msgloader').show();
                            app.request.get("./GetMyMessage", { page: lastItemIndex, limit: itemsPerLoad, messageStatus: $("#hd_MsgStatus").val() }, function (res) {
                                res = JSON.parse(res);
                                allowInfinite = true;
                                maxItems = res.count;
                                generateMsg(res.data, function (itemHTML) {
                                    $ptrContent.find('ul').append(itemHTML);
                                });
                            });
                        }
                    });
                }
                initInfinite();
            }
        }
    },
    {
        path: '/MessageView/:Id',
        url: './MessageView?Id={{Id}}',
        on: {
            pageInit: function (e, page) {
                let msgId = page.route.params.Id;
                app.request.get('./GetMessageInfo', { Id: msgId }, function (res) {
                    res = JSON.parse(res);
                    let dot = $("a[href='/MessageView/" + msgId + "']").find(".layui-badge-dot");
                    if (dot.length > 0) {
                        dot.remove();
                        $("#unMsg").html($("#unMsg").html() * 1 - 1)
                    }
                    $(".fromName").html(res.data.FromUserName);
                    $(".sendDate").html(res.data.SendTime);
                    $(".msgContents").html(res.data.MsgContent);
                    $(".msgTitle").html(res.data.Title)
                    switch (res.data.FromTable) {
                        case "Event":
                            $(".toLink").html("打开事件");
                            $(".toLink").attr("href", "/EventDetail/" + res.data.FromPKId);
                            break;
                        default:
                            $(".toLink").html("");
                            $(".toLink").attr("href", "#" + res.data.FromPKId);
                            break;
                    }

                })
            }
        }
    },
    {
        path: '/FacilityInfo/:Id',
        url: './FacilityInfo?Id={{Id}}',
        options: {
            reloadCurrent: true
            //reloadPrevious:true
            //history: false,
        },
        on: {
            pageInit: function (e, page) {
                let facId = page.route.params.Id;
                let fdata = {};
                fdata.Id = facId;
                //layui.data('Facility', { key: 'Id', value: facId });
                //时间戳转化时间格式
                var formatTime = function (number, format) {

                    var formateArr = ['Y', 'M', 'D', 'h', 'm', 's'];
                    var returnArr = [];

                    var date = new Date(number);
                    returnArr.push(date.getFullYear());
                    returnArr.push(formatNumber(date.getMonth() + 1));
                    returnArr.push(formatNumber(date.getDate()));

                    returnArr.push(formatNumber(date.getHours()));
                    returnArr.push(formatNumber(date.getMinutes()));
                    returnArr.push(formatNumber(date.getSeconds()));

                    for (var i in returnArr) {
                        format = format.replace(formateArr[i], returnArr[i]);
                    }
                    return format;
                }

                //数字格式规范化  
                var formatNumber = function (n) {
                    n = n.toString()
                    return n[1] ? n : '0' + n
                }
                //获取属性值
                function fetchComputedStyle(element, property) {
                    const computedStyles = getComputedStyle(element);
                    if (computedStyles) {
                        property = property.replace(/([A-Z])/g, '-$1').toLowerCase();
                        return computedStyles.getPropertyValue(property);
                    }
                }
                //计算秒数间隔
                function calcSecond(start = new Date(), end = new Date()) {
                    let time1 = new Date(start).getTime() / 1000;
                    let time2 = new Date(end).getTime() / 1000;
                    return time2 - time1;
                }
                //匹配事件类型（存在缺陷）
                function fetchEventType(type, status) {
                    switch (type) {
                        case "換型":
                            return "progress-bar-success";
                        case "停机":
                            return "progress-bar-danger";
                        case "低效":
                            return "progress-bar-warning";
                        default:
                            return "progress-bar-success";
                    }
                }
                //清除子节点
                function fnDelete(elem) {
                    while (elem.hasChildNodes()) //当elem下还存在子节点时 循环继续
                    {
                        elem.removeChild(elem.firstChild);
                    }
                }
                //添加新事件进度条
                function newProgress(width, className = '', offsetX = 0) {
                    let childNode = document.createElement('span');
                    childNode.style.display = "inline-block";
                    childNode.style.height = "90%";
                    childNode.style.width = width ? width : 0;
                    childNode.style.top = "5%";
                    childNode.style.left = offsetX ? offsetX : 0;
                    childNode.style.position = "absolute";
                    childNode.setAttribute("aria-valuenow", 100);
                    childNode.setAttribute("aria-valuemin", 0);
                    childNode.setAttribute("aria-valuemax", 100);
                    childNode.setAttribute("class", "progress-bar progress-bar-striped");
                    childNode.classList.add(className);
                    return childNode;
                }
                var duration = 0, clockIn = 0;              //班制持续时间（单位：秒）; 班制开始时间
                var workTBegin = document.querySelector("span[class='textLeft']");              //班制开始时间显示控件，位于进度条左下方
                var workTEnd = document.querySelector("span[class='textRight']");               //班制结束时间显示控件，位于进度条右下方
                var currentShift = document.querySelector("span[data-shifts='current']");       //设备下方的当前班次时间显示控件
                //var fdata = layui.data('Facility', { key: 'Id' });
                if (typeof fdata != "undefined") {

                    //获取当前机台的信息
                    app.request.get('./GetFacilityInfo', { id: fdata.Id }, function (res) {
                        res = JSON.parse(res);
                        $$("#allEv").html(res.data.split('|')[0]);
                        $$("#unEv").html(res.data.split('|')[1]);
                        $$("#faName").html(res.msg.split('|')[0]);
                    });
                    app.request.get('./GetFacilityParamValue', { id: fdata.Id }, function (res) {
                        res = JSON.parse(res);
                        $("#pnl_ParamList").empty();
                        for (let i = 0; i < res.data.length; i++) {
                            $("#pnl_ParamList").append(`<li>
                                <div class="item-content">
                                    <div class="item-inner">
                                        <div class="item-title">`+ res.data[i].DataAddress + `</div>
                                        <div class="item-after">`+ res.data[i].DataValue + `</div>
                                    </div>
                                </div>
                            </li>`);
                        }
                    });

                    //app.request.get('./GetTime', { id: fdata.Id }, function (res) {
                    //    res = JSON.parse(res);
                    //    if (res.data && res.data[0]["开班时间"] && res.data[0]["结班时间"]) {
                    //        clockIn = res.data[0]["开班时间"];
                    //        duration = calcSecond(res.data[0]["开班时间"], res.data[0]["结班时间"]);
                    //        window.localStorage.setItem("nowDataStr", res.data[0].开班时间);
                    //        window.localStorage.setItem("nowDataEnd", res.data[0].结班时间);
                    //        workTBegin.textContent = new Date(res.data[0]["开班时间"]).toTimeString().substr(0, 8);
                    //        workTEnd.textContent = new Date(res.data[0]["结班时间"]).toTimeString().substr(0, 8);
                    //        currentShift.textContent = workTBegin.textContent + ' - ' + workTEnd.textContent;
                    //    } else {
                    //        workTBegin.textContent = new Date(localStorage.getItem("nowDataStr")).toTimeString().substr(0, 8);
                    //        workTEnd.textContent = new Date(localStorage.getItem("nowDataEnd")).toTimeString().substr(0, 8);
                    //        clockIn = localStorage.getItem("nowDataStr");
                    //        duration = calcSecond(localStorage.getItem("nowDataStr"), localStorage.getItem("nowDataEnd"));
                    //    }

                    //    if (res.data) {
                    //        //当前机台的运行状态时间
                    //        var resTime = res.data[0];
                    //        //将各段时间添加到相应的位置
                    //        $$("#WorkingTime").html(resTime.总运行时间 + "分钟");
                    //        $$("#StopTime").html(resTime.停机总时间 + "分钟");
                    //        $$("#LowWorkingTime").html(resTime.低效运行总时间 + "分钟");
                    //        $$("#ChangeModelTime").html(resTime.换型总时间 + "分钟");
                    //    }
                    //    //一天的时间

                    //});
                    //app.request.get('./progressTime', { id: fdata.Id }, function (res) {
                    //    let progress = document.querySelector("#status-bar");
                    //    fnDelete(progress);
                    //    setTimeout(function () {
                    //        let occurrences = JSON.parse(res).data;
                    //        let proWidth = fetchComputedStyle(progress, 'width');
                    //        proWidth = proWidth.substring(0, proWidth.length - 2);
                    //        progress.appendChild(newProgress((calcSecond(clockIn) / duration) * 100 + '%', fetchEventType(""), 0));
                    //        if (occurrences) {
                    //            for (let i = 0; i < occurrences.length; i++) {
                    //                let length;
                    //                length = (calcSecond(occurrences[i]['上报时间']) / duration) * 100;
                    //                if (occurrences[i]["状态"] === 100) {
                    //                    length = (calcSecond(occurrences[i]['上报时间'], occurrences[i]['结束时间']) / duration) * 100;
                    //                }
                    //                let offsetX = (calcSecond(clockIn, occurrences[i]['上报时间']) / duration) * 100;
                    //                progress.appendChild(newProgress(length + '%', fetchEventType(occurrences[i]['事件类型']), offsetX + '%'));
                    //            }
                    //        }
                    //    }, 200);
                    //});
                }
            }
        }
    },
    {
        path: '/about/',
        url: './about',
    },
    {
        path: '/catalog/',
        url: './catalog',
        on: {
            pageInit: function (e, page) {
                var fdata = layui.data('Facility', { key: 'Id' });
                var maxItems = 200;
                var itemsPerLoad = 20;
                //if (typeof fdata == "undefined") {
                //    app.dialog.alert("请完善设备信息后继续。", "设备信息不完整", function () {
                //        app.views.main.router.navigate("/listResource/", { animate: true });
                //    });
                //}
                {
                    app.request.get('./GetBorad', { id: fdata.Id }, function (res) {
                        res = JSON.parse(res);
                        var li;
                        $.each(res.data, function (index, item) {
                            li = '<li>' +
                                '<a href="/ShowCatagory/" data-url="' + item.Url + '" class="item-link item-content" >' +
                                '<div class="item-inner">' +
                                '<div class="item-title-row">' +
                                '<div class="item-title">' + item.Name + '</div>' +
                                '</div > ' +
                                '</div>' +
                                '</a>' +
                                '</li>';
                            $("#borad").append(li);

                        })
                        $("#borad li a").on("click", function () {
                            let urlc = $(this).data("url");
                            window.localStorage.setItem("href", urlc)
                        });

                    });
                }
            }
        }
    },
    {
        path: '/trace/:Id',
        url: './trace',
        on: {
            pageInit: function (e, page) {
                let facId = page.route.params.Id;
                let fdata = {};
                fdata.Id = facId;
                //var fdata = layui.data('Facility', { key: 'Id' });
                var maxItems = 200;
                var itemsPerLoad = 20;
                //if (typeof fdata == "undefined") {
                //    app.dialog.alert("请完善设备信息后继续。", "设备信息不完整", function () {
                //        app.views.main.router.navigate("/listResource/", { animate: true });
                //    });
                //}
                //else {
                LoadClosedEvent(fdata.Id);
                //}

                var allowInfinite = true;
                // Last loaded index
                var lastItemIndex = 2;
                // Max items to load
                // Append items per load

                $$('.traceList').on('infinite', function () {
                    // Exit, if loading in progress
                    if (!allowInfinite) return;
                    // Set loading flag
                    allowInfinite = false;
                    app.request.get('./GetClosedEventList', { FacilityId: fdata.Id, page: lastItemIndex, limit: itemsPerLoad }, function (res) {
                        res = JSON.parse(res);
                        if (lastItemIndex * itemsPerLoad >= maxItems) {
                            // Nothing more to load, detach infinite scroll events to prevent unnecessary loadings
                            app.infiniteScroll.destroy('.traceList');
                            // Remove preloader
                            $$('.infinite-scroll-preloader').remove();
                            return;
                        }
                        // Generate new items HTML
                        var html = '';
                        layui.use('util', function () {
                            var util = layui.util;
                            for (var i = 0; i < res.data.length; i++) {
                                let item = res.data[i];
                                html += '<li>';
                                html += '    <a href="/EventDetailReadonly/' + item.Id + '/" class="item-link item-content">';
                                html += '        <div class="item-inner">';
                                html += '            <div class="item-title-row">';
                                html += '                <div class="item-title">' + item.CategoryName + '</div>';
                                html += '                <div class="item-after">' + util.timeAgo(item.ReportTime, false) + '</div>';
                                html += '            </div>';
                                html += '            <div class="item-subtitle">事件分类: ' + item.CategoryType + ' </div>';
                                html += '            <div class="item-text">' + item.ReporterDesc + '</div>';
                                html += '        </div>';
                                html += '    </a>';
                                html += '</li>';
                            }
                        });
                        // Append new items
                        $$('.closedEventList ul').append(html);
                        allowInfinite = true;
                        // Update last loaded index
                        lastItemIndex++;
                    });
                    // Emulate 1s loading
                    //setTimeout(function () {
                    //    // Reset loading flag
                    //}, 1000);
                });
            },
            pageAfterIn: function (e, page) {

            }
        }
    },
    {
        path: '/product/:id/',
        componentUrl: './settings',
    },
    {
        path: '/settings/',
        url: './settings/',
        on: {
            pageInit: function (e, page) {
                $("#faceSet").on("click", function () {
                    if (_faceServ == "") {
                        app.dialog.alert("系统没有开启面部识别功能。", "出错了");
                        return;
                    }
                    let urlface = _faceServ + "/FaceSet";
                    if ('_cordovaNative' in window) {
                        app.dialog.password('', "请输入密码验证身份", function (password) {
                            app.request.post("./CheckPsw", { psw: password }, function (re) {
                                re = JSON.parse(re);
                                if (re.code == 200) {
                                    cordova.plugins.FacePlugin.faceScan([], function (msg) {
                                        $.ajax({
                                            url: urlface,
                                            type: "POST",
                                            data: { face: msg, uid: _uid },
                                            crossDomain: true,
                                            beforeSend: function (e) {
                                                app.dialog.progress();
                                            },
                                            success: function (fsfds) {
                                                //alert(2);
                                                app.dialog.close();
                                                app.dialog.alert("可以使用人脸扫描登录了", "人脸信息录入成功");
                                                //alert(3);
                                            },
                                            error: function (xhr, status) {
                                                app.dialog.close();
                                                app.dialog.alert("Error code:" + status, "人脸信息录入失败");
                                                //alert('err' + status);
                                            }
                                        });
                                    }, function (msg) {
                                        app.dialog.alert("您已取消扫描或者" + msg, "扫描人脸失败");
                                    });
                                }
                                else {
                                    app.dialog.alert("您输入的密码不正确。", "出错了");
                                }
                            });
                        });
                    }
                    else {
                        app.dialog.alert("您的设备不支持面部识别功能。", "您的设备不支持此操作");
                    }
                });
            }
        }
    },
    // Page Loaders & Router
    {
        path: '/listResource/',
        templateUrl: './listResource',
        on: {
            pageInit: function (e, page) {

            }
        }
    },
    {
        //事件类型
        path: '/listEventType/:Id',
        templateUrl: './listEventType?facId={{Id}}',
        on: {
            pageInit: function (e, page) {
                let facId = page.route.params.Id;
                let fdata = {};
                fdata.Id = facId;
                //var fdata = layui.data('Facility', { key: 'Id' });
                if (typeof fdata == "undefined") {
                    app.dialog.alert("请完善设备信息后继续。", "设备信息不完整", function () {
                        app.views.main.router.navigate("/listResource/", { animate: true });
                    });
                }
            }
        }
    },
    {
        //事件类型
        path: '/paramChoose/:Id',
        url: './paramChoose?facId={{Id}}',
        on: {
            pageAfterIn: function (event, page) {
                $("#txt_searchParamKey").focus();
                // do something after page gets into the view
            },
            pageInit: function (e, page) {
                let facId = page.route.params.Id;
                let fdata = {};
                fdata.Id = facId;
                //var fdata = layui.data('Facility', { key: 'Id' });
                if (typeof fdata == "undefined") {
                    app.dialog.alert("请完善设备信息后继续。", "设备信息不完整", function () {
                        app.views.main.router.navigate("/listResource/", { animate: true });
                    });
                }
                let LoadSearchParamResult = function (key) {
                    app.dialog.progress('正在查询');
                    $.get('./GetMaterialIdbyCode?code=' + key, function (res) {
                        app.dialog.close();
                        if (res.code == 200 && res.data != null) {
                            $("#txt_searchParamKey").val("");
                            //$("#faName").html(res.data.Name)
                            app.views.current.router.navigate("/ParamSet/" + facId + "/" + res.data.Id, { animate: true, ignoreCache: true });
                        }
                        else {
                            app.dialog.alert(res.msg, "操作出错了", function () {
                            });
                            //layui.layer.msg(res.msg);
                        }
                    });
                    setTimeout(function () {
                        try {
                            app.dialog.close();
                        }
                        catch (ex) { console.log(ex) }
                    }, 20000);
                    //$("#pnl_SearchParamHis").hide();
                    //$("#pnl_SearchParamResult").show();
                };
                $("#searchParamHistory").html('');
                $("#pnl_SearchParamResult").hide();
                let localTest = layui.data('searchParamhis');
                if (localTest != null && typeof localTest != "undefined") {
                    let searchhis = localTest.lohis;
                    if (typeof searchhis != "undefined") {
                        searchhis = JSON.parse(searchhis);
                        for (var i = 0; i < searchhis.length; i++) {
                            $("#searchParamHistory").prepend('<div class="chip"><div class="chip-label searchParamchip">' + searchhis[i].keyname + '</div></div> ');
                        }
                    }
                }
                $(".searchParamchip").click(function () {
                    let key = $(this).html();
                    LoadSearchParamResult(key);
                });
                $("#txt_searchParamKey").on('keyup', function (e) {
                    if (e.which == 13) {
                        e.preventDefault();
                        $("#btn_SearchParam").trigger('click');
                        return;
                    }
                });
                $("#btn_SearchParam").click(function () {
                    let searchkey = $("#txt_searchParamKey").val();
                    let searchhis = [{ "keyname": searchkey }];
                    localTest = layui.data('searchParamhis');
                    if (localTest != null && typeof localTest != "undefined") {
                        searchhis = localTest.lohis;
                        if (typeof searchhis != "undefined") {
                            searchhis = JSON.parse(searchhis);
                            if (searchkey.length > 0)
                                searchhis.push({ "keyname": searchkey });
                            if (searchhis.length > 50) {
                                searchhis = searchhis.splice(0, 1);
                            }
                        }
                        else
                            searchhis = [{ "keyname": searchkey }];
                    }
                    else
                        searchhis = [{ "keyname": searchkey }];

                    layui.data('searchParamhis', {
                        key: 'lohis'
                        , value: JSON.stringify(searchhis)
                    });
                    LoadSearchParamResult(searchkey);
                });
                $("#txt_searchParamKey").focus();
            }
        }
    },
    {
        path: "/ParamSet/:facId/:matId",
        url: "./ParamSet?facId={{facId}}&matId={{matId}}",
        on: {
            pageInit: function (e, page) {
                let facId = page.route.params.facId;
                $("#linkParamIn").click(function () {
                    $("#saveNewParam").trigger("click");
                })
                $("#saveNewParam").click(function () {
                    let verifyErrCount = 0;
                    let verifyErrItem = "";
                    $('.verifyItem').each(function (index, item) {
                        let mtype = $(item).attr('type');
                        let mName = $(item).attr('name');
                        let mLength = $(item).data("length");
                        let mMax = $(item).data("max").toString();
                        let mMin = $(item).data("min").toString();
                        let mValue = $(item).val();
                        if (mtype.toLowerCase() == "number") {
                            if ((mMax != "" && mValue*1 > mMax*1) || (mMin != "" && mValue*1 < mMin*1)) {
                                verifyErrItem += mName + ", ";
                                verifyErrCount++;
                            }
                        }
                        if (mtype.toLowerCase() == "text") {
                            if (mLength > 0 && mValue.length > mLength) {
                                verifyErrItem += mName + ", ";
                                verifyErrCount++;
                            }                            
                        }
                    });
                    if (verifyErrCount > 0) {
                        app.dialog.alert("请验证不通过的参数：" + verifyErrItem.substring(0, verifyErrItem.length - 1), "数据校验失败", function () {
                        });
                        return;
                    }

                    var formData = app.form.convertToData('#paramDetail');
                    app.dialog.progress('正在写入');
                    app.request.post('./SaveParamValue', formData, function (res) {
                        app.dialog.close();
                        res = JSON.parse(res);
                        if (res.code == 200) {
                            app.dialog.progress('正在校验');
                            setTimeout(function () {
                                app.request.get('./GetFormulaParamValue', { id: $("#hd_formulaId").val(), facId: facId }, function (ress) {
                                    app.dialog.close();
                                    ress = JSON.parse(ress);
                                    $("#pnl_ParamList").empty();
                                    for (let i = 0; i < ress.data.length; i++) {
                                        $("#pnl_ParamList").append(`<li>
                                            <div class="item-content">
                                                <div class="item-inner">
                                                    <div class="item-title">`+ ress.data[i].ParamAddress + `</div>
                                                    <div class="item-after">`+ ress.data[i].DataValue + `</div>
                                                </div>
                                            </div>
                                        </li>`);
                                    }
                                    let errCount = 0;
                                    let errParam = "";
                                    for (let paramkey in formData) {
                                        if (paramkey == "FacilityId" || paramkey == "MaterialId" || paramkey == "FormulaId")
                                            continue;
                                        let resD = ress.data.filter(function (e) { return e.ParamAddress == paramkey });
                                        if (resD.length == 0) {
                                            errCount++;
                                            errParam += (paramkey + ", ");
                                            continue;
                                        }
                                        if (resD[0].DataValue != formData[paramkey]) {
                                            errCount++;
                                            errParam += (paramkey + ", ");
                                        }
                                    }
                                    if (errCount > 0) {
                                        app.dialog.alert("请人工检查由于网络延迟可能未写入成功的参数：" + errParam.substring(0, errParam.length - 1), "写入失败", function () {
                                        });
                                    }
                                    else {
                                        app.dialog.alert("已完成参数写入", "写入成功", function () { });
                                    }
                                });
                            }, 5000);
                            //app.dialog.alert("已完成参数写入", "写入成功", function () {
                            //});
                        }
                        else {
                            app.dialog.alert(res.msg, "写入失败", function () {
                            });
                        }
                    });
                    setTimeout(function () {
                        try {
                            app.dialog.close();
                        }
                        catch (ex) { console.log(ex) }
                    }, 20000);
                });
            }
        }
    },
    {
        //选择事件
        path: '/listEvent/:typeId/:facId',
        templateUrl: './listEvent?typeId={{typeId}}&facId={{facId}}',
        on: {
            pageInit: function (e, page) {
                let facId = page.route.params.facId;
                var that = this;
                var cid = that.url.split('/');
                cid = cid[cid.length - 1];
                //app.request.get('./GetFacilityInfo', {}, function (res) {
                //    res = JSON.parse(res);
                //});
            }
        }
    },
    {
        path: '/addEvent/:categoryId/:facId',
        templateUrl: './addEvent?typeId={{categoryId}}&facId={{facId}}',
        on: {
            pageInit: function (e, page) {
                let facId = page.route.params.facId;
                let cid = page.route.params.categoryId;
                var that = this;
                //var cid = that.url.split('/');
                //cid = cid[cid.length - 1];
                if (cid && cid != "") {
                    app.request.get('./GetCategoryInfo', { id: cid }, function (res) {
                        res = JSON.parse(res);
                        $$("#categoryName").val(res.data.CategoryName);
                        $$("#categoryTypeName").val(res.data.CategoryType);
                    });
                }


                $$('.convertformtodata').on('click', function () {
                    var formData = app.form.convertToData('#my-form');
                    //alert(JSON.stringify(formData));

                    //var fdata = layui.data('Facility', { key: 'Id' });
                    //if (typeof fdata == "undefined") {
                    //    app.dialog.alert("请完善设备信息后继续。", "设备信息不完整", function () {
                    //        app.views.main.router.navigate("/listResource/", { animate: true });
                    //    });
                    //    return;
                    //}
                    let remarkContent = document.querySelector("textarea[name='EventDesc']").value.trim();

                    if (!remarkContent) {
                        app.dialog.alert("请补充完备注信息再提交！", "提示");
                        return;
                    }
                    if (!/[^%&',;=?$\x22]+/.test(remarkContent)) {
                        app.dialog.alert("输入字符不合法！", "提示");
                        return;
                    }
                    formData.CategoryId = cid;
                    formData.FacilityId = facId;
                    app.request.post('./SaveEvent', formData, function (res) {
                        res = JSON.parse(res);
                        if (res.code == 200) {
                            app.dialog.confirm("确定返回首页，取消继续上报。", "事件上报成功", function () {
                                app.views.current.router.navigate("/FacilityInfo/" + facId, { animate: false });
                            }, function () {
                                app.views.current.router.navigate("/listEventType/" + facId, { animate: true });
                            });
                        } else if (res.code === 500) {
                            app.dialog.alert(res.msg);
                        }


                    });

                });
            }
        }
    },
    {
        path: '/listMyEvent/:Id',
        xhrCache: false,
        templateUrl: './listMyEvent?rad=' + Math.random(),
        on: {
            pageInit: function (e, page) {
                let facId = page.route.params.Id;
                let fdata = {};
                fdata.Id = facId;
                //var fdata = layui.data('Facility', { key: 'Id' });
                if (typeof fdata == "undefined") {
                    app.dialog.alert("请完善设备信息后继续。", "设备信息不完整", function () {
                        app.views.main.router.navigate("/listResource/", { animate: true });
                    });
                }
                else {
                    app.request.get('./GetMyEventList', { FacilityId: fdata.Id }, function (res) {
                        layui.use('util', function () {
                            var util = layui.util;
                            res = JSON.parse(res);
                            var list = "";
                            let data = res.data.sort((pre, nex) => { return new Date(nex.ReportTime) - new Date(pre.ReportTime) })
                            for (var i = 0; i < data.length; i++) {
                                let item = data[i];
                                list += '<li>';
                                list += '    <a href="/EventDetail/' + item.Id + '/" class="item-link item-content">';
                                list += '        <div class="item-inner">';
                                list += '            <div class="item-title-row">';
                                list += '                <div class="item-title">' + item.CategoryName + '</div>';
                                list += '                <div class="item-after">' + util.timeAgo(item.ReportTime, false) + '</div>';
                                list += '            </div>';
                                list += '            <div class="item-subtitle">事件分类: ' + item.CategoryType + ' </div>';
                                list += '            <div class="item-text">' + item.ReporterDesc + '</div>';
                                list += '        </div>';
                                list += '    </a>';
                                list += '</li>';
                            }
                            if (list == "") {
                                list = '<li style="text-align:center"><a href="/" class="item-link item-content"><div class="item-inner">这里没有事件</div></a></li>';
                            }
                            $("#eventList").html(list);
                        });
                    });
                }
            }
        }
    },
    {
        path: '/EventDetail/:Id',
        url: './EventDetail?Id={{Id}}',
        on: {
            pageInit: function (e, page) {
                let eventId = page.route.params.Id;
                app.request.get('./GetEventDetail', { EventId: eventId }, function (res) {
                    $$('.saveEventRecord').show()
                    layui.use('util', function () {
                        var util = layui.util;
                        res = JSON.parse(res);

                        var list = "";
                        for (var i = 0; i < res.data.EventRecordSteps.length; i++) {
                            let item = res.data.EventRecordSteps[i];
                            if (item.StepNum == res.data.Status) {
                                $$("#nowStepName").val(item.StepName);
                                app.request.get('./CheckStepAuth', { stepId: item.StepId, eventId: eventId }, function (ress) {
                                    ress = JSON.parse(ress)
                                    if (ress.code == 403) {
                                        $$('.saveEventRecord').hide()
                                    }
                                    else {
                                        $$('.saveEventRecord').show()
                                    }
                                })
                                break;
                            }
                            list += '<div class="timeline-item">';
                            list += '    <div class="timeline-item-date">' + item.DoTime.split(' ')[0].substr(5) + ' <small>' + item.DoTime.split(' ')[1] + '</small></div>';
                            list += '    <div class="timeline-item-divider"></div>';
                            list += '    <div class="timeline-item-content">';
                            list += '        <div class="timeline-item-inner">';
                            list += '            <p><i class="f7-icons" style="font-size:inherit">person</i> ' + item.DoUserName + '</p>';
                            list += '            <p><i class="f7-icons" style="font-size:inherit">tags_fill</i> ' + item.StepName + '</p>';
                            if (item.DoDesc != "")
                                list += '            <p><i class="f7-icons" style="font-size:inherit">chat_fill</i> ' + item.DoDesc + '</p>';
                            list += '        </div>';
                            list += '    </div>';
                            list += '</div>';

                        }
                        $("#eventTimeline").html(list);
                        $("#nowStepId").val(res.data.Status);
                        $("#categoryName").val(res.data.CategoryName);
                        $("#categoryType").val(res.data.CategoryType);
                        $("#FacilityName").val(res.data.FacilityName);
                        $("#FacilityCode").val(res.data.FacilityCode);
                        $("#FacilityId").val(res.data.FacilityId);
                        $("#ReporterDesc").val(res.data.ReporterDesc);
                        if (res.data.Status == 100) {
                            $$('.saveEventRecord').hide()
                            $("#nowStepName").val('关闭')
                        }
                        else {

                        }
                    });

                });
                $$('.saveEventRecord').on('click', function () {
                    var formData = {};
                    //alert(JSON.stringify(formData));

                    //var fdata = layui.data('Facility', { key: 'Id' });
                    //if (typeof fdata == "undefined") {
                    //    app.dialog.alert("请完善设备信息后继续。", "设备信息不完整", function () {
                    //        app.views.main.router.navigate("/listResource/", { animate: true });
                    //    });
                    //    return;
                    //}

                    let remarkContent = document.querySelector("#DoDesc").value.trim();
                    if (!remarkContent) {
                        app.dialog.alert("请补充完备注信息再提交！", "提示");
                        return;
                    }
                    if (!/[^%&',;=?$\x22]+/.test(remarkContent)) {
                        app.dialog.alert("输入字符不合法！", "提示");
                        return;
                    }
                    formData.EventId = eventId;
                    formData.StepNum = $("#nowStepId").val();
                    formData.FacilityId = $("#FacilityId").val();
                    formData.desc = $("#DoDesc").val();
                    app.request.post('./SaveEventStep', formData, function (res) {
                        res = JSON.parse(res);
                        if (res.code == 200) {
                            app.dialog.confirm("确定返回首页，取消返回事件列表。", "事件处理成功", function () {
                                app.views.current.router.navigate("/FacilityInfo/" + $("#FacilityId").val(), { animate: false });
                            }, function () {
                                app.views.current.router.navigate("/listMyEvent/" + $("#FacilityId").val(), { animate: true });
                            });
                        }


                    });

                });
            }
        }
    },
    {
        path: '/EventDetailReadonly/:Id',
        url: './EventDetailReadonly?Id={{Id}}',
        on: {
            pageInit: function (e, page) {
                let eventId = page.route.params.Id;
                app.request.get('./GetEventDetail', { EventId: eventId }, function (res) {
                    //layui.use('util', function () {
                    //var util = layui.util;
                    res = JSON.parse(res);

                    var list = "";
                    for (var i = 0; i < res.data.EventRecordSteps.length; i++) {
                        let item = res.data.EventRecordSteps[i];
                        if (item.StepNum == res.data.Status) {
                            $$("#nowStepName").val(item.StepName);
                            break;
                        }
                        list += '<div class="timeline-item">';
                        list += '    <div class="timeline-item-date">' + item.DoTime.split(' ')[0].substr(5) + ' <small>' + item.DoTime.split(' ')[1] + '</small></div>';
                        list += '    <div class="timeline-item-divider"></div>';
                        list += '    <div class="timeline-item-content">';
                        list += '        <div class="timeline-item-inner">';
                        list += '            <p><i class="f7-icons" style="font-size:inherit">person</i> ' + item.DoUserName + '</p>';
                        list += '            <p><i class="f7-icons" style="font-size:inherit">tags_fill</i> ' + item.StepName + '</p>';
                        if (item.DoDesc != "")
                            list += '            <p><i class="f7-icons" style="font-size:inherit">chat_fill</i> ' + item.DoDesc + '</p>';
                        list += '        </div>';
                        list += '    </div>';
                        list += '</div>';

                    }
                    $$("#eventTimelineRead").html(list);
                    $$("#categoryNameRead").val(res.data.CategoryName);
                    $$("#categoryTypeRead").val(res.data.CategoryType);
                    //});

                });
            }
        }
    },
    {
        path: '/ShowCatagory/',
        templateUrl: './ShowCatagory',
        on: {
            pageInit: function (e, page) {
                var fdata = layui.data('Facility', { key: 'Id' });
                if (typeof fdata == "undefined") {
                    app.dialog.alert("请完善设备信息后继续。", "设备信息不完整", function () {
                        app.views.main.router.navigate("/listResource/", { animate: true });
                    });
                    return;
                }
                var linkBoard = window.localStorage.getItem("href");
                if (linkBoard.indexOf("?") > 0)
                    linkBoard += "&facilityid=" + fdata.Id;
                else
                    linkBoard += "?facilityid=" + fdata.Id;
                var iframe = $('<iframe src="" id="mainContent" width="100%" height="100%"style="border:none"></iframe>')
                $("#iframeBoard").append(iframe);
                $('#iframeBoard>iframe').attr('src', linkBoard);
            }
        }
    },
    {
        path: '/repair/',
        url: './repair/',
        on: {
            pageAfterIn: function () {
                function renderStatus(status) {
                    switch (status) {
                        case 0:
                            return "未开始";
                            break;
                        case 1:
                            return "已接单";
                            break;
                        case 2:
                            return "维修中";
                            break;
                        case 3:
                            return "完成报修";
                            break;
                    }
                }
                function renderAction(status, id) {
                    switch (status) {
                        case 0:
                            return "/faRepair/" + id;
                            break;
                        case 1:
                            return "/faRepair/" + id;
                            break;
                        case 2:
                            return "/faRepairNow/" + id;
                            break;
                        default:
                            return ""
                    }
                }
                this.app.request.json('./getFacilityRepairBill/', function (res) {
                    $$("#repairdispatchlist").empty().append(`
                        ${res.data.data.map(item =>
                        `
                                <li>
                                    <a href="${renderAction(item.Status, item.Id)}" class="item-link item-content">
                                        <div class="item-inner">
                                            <div class="item-title-row">
                                                <div class="item-title">${item.FacilityName}(${item.FacilityCode})</div>
                                                
                                            </div>
                                            <div class="item-subtitle">${renderStatus(item.Status)}</div>
                                            <div class="item-subtitle">${item.CreateTime}</div>
                                        </div>
                                    </a>
                                </li>
                            `
                    ).join('')}
                     `)
                })
                var notification = app.notification.create({
                    title: '有新的保修单',
                    text: '请前往接单',
                    closeTimeout: 2000,
                    on: {
                        opened: function () {

                        }
                    }
                })
                this.app.request.json('./GetFacilityRepairBill2/', function (res) {
                    $$("#repairlist").empty().append(`
                        ${res.data.data.map(item =>
                        `
                                <li>
                                    <a href="${renderAction(item.Status, item.Id)}" class="item-link item-content">
                                        <div class="item-inner">
                                            <div class="item-title-row">
                                                <div class="item-title">${item.FacilityName}(${item.FacilityCode})</div>
                                                
                                            </div>
                                            <div class="item-subtitle">${renderStatus(item.Status)}</div>
                                            <div class="item-subtitle">${item.CreateTime}</div>
                                           
                                        </div>
                                    </a>
                                </li>
                            `
                    ).join('')}
                     `)
                })
                //setInterval(() => {
                //    this.app.request.json('./getFacilityRepairBill/', function (res) {
                //        let len = $$("#repairdispatchlist").find("li").length
                //        if (res.data.data.length > len) {
                //            notification.open()
                //        }
                //    })
                //}, 50000)
            }
        }
    },
    {
        path: '/faRepairDispatch/:id',
        template: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">维修接单</div>
                </div>
            </div>
            <div class="page-content">
              <div class="list media-list ">
                            <ul>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">设备</div>
                            <div class="item-input-wrap">
                                <input type="text" id="sfareName" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <div class="item-content item-input" >
                     <div class="item-inner">
                            <div class="item-title item-label">@Localizer["Col_Desc"]</div>
                            <div class="item-input-wrap">
                                <input type="text" id="fadesc" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
            </ul>
              <button type="button" class="button button-fill" id="btn_send_repair">接单</button>
                        </div>
            <input type="hidden" value={{$route.params.id}} id="sbillId">
            </div>
          </div>
        `,
        on: {
            pageInit: function () {
                this.app.request.json('./GetFacilityRepairBillInfo', {
                    billId: $$("#sbillId").val()
                }, (res) => {
                    if (res.code == 200) {
                        let data = res.data.data[0];
                        $$("#sfareName").val(data.FacilityName + "(" + data.FacilityCode + ")");
                        $$("#fadesc").val(data.Descr);
                    }
                })
                var notification = app.notification.create({
                    title: '接单成功',
                    text: '',
                    closeTimeout: 3000,
                    on: {
                        opened: function () {

                        }
                    }
                })
                $$("#btn_send_repair").click(() => {
                    app.dialog.prompt('', '请验证身份', function (name) {
                        this.app.request.json('./CheckCurUser', {
                            user: name
                        }, res => {
                            if (res.code == 200) {
                                this.app.request.json('./DispatchFacilityRepair', {
                                    billId: $("#sbillId").val()
                                }, (res) => {
                                    if (res.code == 200) {
                                        notification.open()
                                        repairView.router.back()
                                    } else {
                                        app.dialog.alert(res.msg);
                                    }
                                })
                            } else {
                                app.dialog.alert(res.msg);
                            }
                        })
                    });

                })
            }
        }
    },
    {
        path: '/faRepair/:id',
        template: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">开始维修</div>
                </div>
            </div>
            <div class="page-content">
            <div class="list no-hairlines-md">
            <ul>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">设备</div>
                            <div class="item-input-wrap">
                                <input type="text" id="fareName" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                <div class="item-content item-input" style="display:none">
                     <div class="item-inner">
                            <div class="item-title item-label">设备编码</div>
                            <div class="item-input-wrap">
                                <input type="text" id="faCode" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
               <div class="item-content item-input" >
                     <div class="item-inner">
                            <div class="item-title item-label">@Localizer["Col_Desc"]</div>
                            <div class="item-input-wrap">
                                <input type="text" id="redesc" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li style="display:none">
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">派单人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="fadispatchName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">维修人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="farepairStaffName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li class="align-top">
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <a href="#" class="button  button-large button-fill button-raised" id="btn_BeginRepair" >开始维修</a>
                            <a href="#" class="button  button-large button-fill button-raised" id="btn_EndRepair" style="display:none" >完成维修</a>
                        </div>
                    </div>
                </li>
            </ul>
        </div>
               <input type="hidden" value={{$route.params.id}} id="rebillId">
            </div>
          </div>
        `,
        on: {
            pageInit: function () {
                this.app.request.json('./GetFacilityRepairBillInfo', {
                    billId: $$("#rebillId").val()
                }, (res) => {
                    if (res.code == 200) {
                        let data = res.data.data[0];
                        $$("#fareName").val(data.FacilityName + "(" + data.FacilityCode + ")");
                        $$("#faCode").val(data.FacilityCode);
                        $$("#redesc").val(data.RepairReason)
                        $$("#fadispatchName").val(data.DispatchName);
                        $$("#farepairStaffName").val(data.RepairStaffName);
                        if (data.Status > 1) {
                            $$("#btn_BeginMaintain").hide();
                            $$("#btn_EndMaintain").show();
                        }
                    }
                })
                var notification = app.notification.create({
                    title: '维修成功',
                    text: '',
                    closeTimeout: 3000,
                    on: {
                        opened: function () {

                        }
                    }
                })
                $$("#btn_BeginRepair").click(() => {
                    app.dialog.prompt('', '请扫入设备二维码', function (name) {
                        if (name.trim().toLowerCase() == $$("#faCode").val().toLowerCase()) {
                            app.dialog.progress();
                            setTimeout(function () {
                                app.dialog.close();
                            }, 1500);
                            this.app.request.json('./BeginFacilityRepairBill', {
                                billId: $$("#rebillId").val()
                            }, res => {
                                if (res.code == 200) {
                                    $$("#btn_BeginRepair").hide();
                                    $$("#btn_EndRepair").show();
                                }
                            })
                        } else {
                            app.dialog.alert('', '并非本次维修的设备!');
                        }
                    });
                })
                $$("#btn_EndRepair").click(() => {
                    app.dialog.prompt('', '请扫入维修员的二维码', function (name) {
                        app.dialog.progress();
                        this.app.request.json('./EndFacilityRepairBill', {
                            billId: $$("#rebillId").val(),
                            checker: name
                        }, res => {
                            app.dialog.close();
                            if (res.code == 200) {
                                repairView.router.back()
                                notification.open()
                            } else {
                                app.dialog.alert('', '权限验证错误!');
                            }
                        })
                    });
                })
            }
        }
    },
    {
        path: '/faRepairNow/:id',
        template: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">开始维修</div>
                </div>
            </div>
            <div class="page-content">
            <div class="list no-hairlines-md">
            <ul>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">设备</div>
                            <div class="item-input-wrap">
                                <input type="text" id="fareName" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                <div class="item-content item-input" style="display:none">
                     <div class="item-inner">
                            <div class="item-title item-label">设备编码</div>
                            <div class="item-input-wrap">
                                <input type="text" id="faCode" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
               <div class="item-content item-input" >
                     <div class="item-inner">
                            <div class="item-title item-label">描述</div>
                            <div class="item-input-wrap">
                                <input type="text" id="redesc" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li style="display:none">
                    <div class="item-content item-input ">
                        <div class="item-inner layui-hide">
                            <div class="item-title item-label">派单人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="fadispatchName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">维修人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="farepairStaffName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li class="align-top">
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <a href="#" class="button  button-large button-fill button-raised" id="btn_EndRepair"  >完成维修</a>
                        </div>
                    </div>
                </li>
            </ul>
        </div>
               <input type="hidden" value={{$route.params.id}} id="rebillId">
            </div>
          </div>
        `,
        on: {
            pageInit: function () {
                this.app.request.json('./GetFacilityRepairBillInfo', {
                    billId: $$("#rebillId").val()
                }, (res) => {
                    if (res.code == 200) {
                        let data = res.data.data[0];
                        $$("#fareName").val(data.FacilityName + "(" + data.FacilityCode + ")");
                        $$("#faCode").val(data.FacilityCode);
                        $$("#redesc").val(data.RepairReason)
                        $$("#fadispatchName").val(data.DispatchName);
                        $$("#farepairStaffName").val(data.RepairStaffName);
                        if (data.Status > 1) {
                            $$("#btn_BeginMaintain").hide();
                            $$("#btn_EndMaintain").show();
                        }
                    }
                })
                var notification = app.notification.create({
                    title: '维修成功',
                    text: '',
                    closeTimeout: 3000,
                    on: {
                        opened: function () {

                        }
                    }
                })
                $$("#btn_EndRepair").click(() => {
                    app.dialog.prompt('', '请扫入维修员的二维码', function (name) {
                        app.dialog.progress();
                        this.app.request.json('./EndFacilityRepairBill', {
                            billId: $$("#rebillId").val(),
                            checker: name
                        }, res => {
                            app.dialog.close();
                            if (res.code == 200) {
                                repairView.router.back()
                                notification.open()
                            } else {
                                app.dialog.alert('', '权限验证错误!');
                            }
                        })
                    });
                })
            }
        }
    },
    {
        path: '/mold/',
        url: './mold/',
        on: {
            pageInit: function () {
                loopGetData()
                function loopGetData() {
                    this.app.request.json('./getMoldOutBill/', function (res) {
                        if ($$("#moldoutcount").find("span").length > 0) {
                            if (res.data.length == 0) {
                                $$("#moldoutcount span").remove()
                            } else {
                                $$("#moldoutcount span").text(res.data.length)
                            }
                        } else {
                            if (res.data.length > 0) {
                                $$("#moldoutcount").append(`<span class="badge color-red">${res.data.length}</span>`)
                            }
                        }
                    })
                }
                setInterval(() => {
                    loopGetData()
                }, 50000)
            }
        }
    },
    
    {
        path: '/moldMaintain/',
        url: './moldMaintain/',

        on: {
            pageAfterIn: function () {
                this.app.request.json('./CheckMoldMaintainAuth/', function (res) {
                    if (res.code == 200) {
                        $("#btn_MoldMaintainDispatch").removeClass("disabled")
                    }
                })

                function renderStatus(status) {
                    switch (status) {
                        case 0:
                            return "未派单";
                            break;
                        case 1:
                            return "已派单";
                            break;
                        case 2:
                            return "开始保养";
                            break;
                        case 3:
                            return "完成保养";
                            break;
                    }
                }
                function renderAction(status, id) {
                    switch (status) {
                        case 0:
                            return "/moldMaintainDispatch/" + id;
                            break;
                        case 1:
                            return "/moldmaintain/" + id;
                            break;
                        case 2:
                            return "/moldmaintain/" + id;
                            break;
                        case 3:
                            return "/moldmaintainrecord/" + id;
                            break;
                        default:
                            return ""
                    }
                }
                this.app.request.json('./getMoldMaintainBill/', function (res) {
                    $$("#molddispatchlist").empty().append(`
                        ${res.data.data.map(item =>
                        `
                                <li>
                                    <a href="${renderAction(item.Status, item.Id)}" class="item-link item-content">
                                        <div class="item-inner">
                                            <div class="item-title-row">
                                                <div class="item-title">${item.MoldName}(${item.MoldCode})</div>
                                                
                                            </div>
                                            <div class="item-subtitle">${renderStatus(item.Status)}</div>
                                            <div class="item-subtitle">${item.CreateDate}</div>
                                           
                                        </div>
                                    </a>
                                </li>
                            `
                    ).join('')}
                     `)
                })
                this.app.request.json('./GetMyMoldMaintainBill/', function (res) {
                    $$("#moldmaintainlist").empty().append(`
                        ${res.data.data.map(item =>
                        `
                            <li>
                                <a href="${renderAction(item.Status, item.Id)}" class="item-link item-content">
                                    <div class="item-inner">
                                        <div class="item-title-row">
                                            <div class="item-title">${item.MoldName}(${item.MoldCode})</div>
                                            
                                        </div>
                                        <div class="item-subtitle">${renderStatus(item.Status)}</div>
                                        <div class="item-subtitle">${item.CreateDate}</div>
                                           
                                    </div>
                                </a>
                            </li>
                        `
                    ).join('')}
                     `)
                })
                this.app.request.json('./GetMyMoldMaintainRecordBill/', function (res) {
                    $$("#moldmaintainrecordlist").empty().append(`
                        ${res.data.data.map(item =>
                        `
                            <li>
                                <a href="${renderAction(item.Status, item.Id)}" class="item-link item-content">
                                    <div class="item-inner">
                                        <div class="item-title-row">
                                            <div class="item-title">${item.MoldName}(${item.MoldCode})</div>
                                            
                                        </div>
                                        <div class="item-subtitle">${renderStatus(item.Status)}</div>
                                        <div class="item-subtitle">${item.CreateDate}</div>
                                           
                                    </div>
                                </a>
                            </li>
                        `
                    ).join('')}
                     `)
                })
            }
        }
    },
    {
        path: '/moldMaintainDispatch/:id',
        template: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">保养派单</div>
                </div>
            </div>
            <div class="page-content">
              <div class="list media-list ">
                            <ul id="stafflist">
                                
                            </ul>
                            <button type="button" class="button button-fill" id="btn_send">派单</button>
                        </div>
            <input type="hidden" value={{$route.params.id}} id="billId">
            </div>
          </div>
        `,
        on: {
            pageInit: function () {
                this.app.request.json('./getRepairStaff/', function (res) {
                    $("#stafflist").empty().append(`
                        ${res.data.data.map(item => `
                            <li>
                                <label class="item-radio item-content">
                                <input type="radio" name="mold_maintain_staff" value="${item.UserId}"  />
                                <i class="icon icon-radio" checked></i>
                                <div class="item-inner">
                                    <div class="item-title-row">
                                    <div class="item-title">${item.UserName}</div>
                                    <div class="item-after">休闲中</div>
                                    </div>
                                    <div class="item-subtitle">${item.Dept}</div>
                                </div>
                                </label>
                            </li>
                        `).join('')}
                    `)

                })
                var notification = app.notification.create({
                    title: '派单成功',
                    text: '',
                    closeTimeout: 3000
                })
                $$("#btn_send").click(() => {
                    if ($$("[name='mold_maintain_staff']:checked").val() == '') {
                        return false
                    }
                    this.app.request.json('./DispatchMoldMaintain', {
                        billId: $("#billId").val(),
                        userId: $$("[name='mold_maintain_staff']:checked").val()
                    }, (res) => {
                        if (res.code == 200) {
                            notification.open()
                            app.views.current.router.back()
                            //moldmaintainView.router.back("/moldMaintain/", { force: true })
                        }
                    })
                })
            }
        }
    },
    {
        path: '/moldmaintain/:id',
        template: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">开始保养</div>
                </div>
            </div>
            <div class="page-content">
            <div class="list no-hairlines-md">
            <ul>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">模具</div>
                            <div class="item-input-wrap">
                                <input type="text" id="moldName" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                <div class="item-content item-input" style="display:none">
                     <div class="item-inner">
                            <div class="item-title item-label">模具编码</div>
                            <div class="item-input-wrap">
                                <input type="text" id="moldCode" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
               
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner ">
                            <div class="item-title item-label">派单人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="mdispatchName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">保养人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="mrepairStaffName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li id='tempWrap' style="display:none">
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">保养内容</div>
                            <div class="item-input-wrap">
                                <div class="list media-list">
                                <ul id='tempContent' style="padding-left:0">
                                </ul>
                </div>
                </li>
                <li class="align-top">
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <a href="#" class="button  button-large button-fill button-raised" id="btn_BeginMaintain" >开始保养</a>
                            <a href="#" class="button  button-large button-fill button-raised" id="btn_EndMaintain" style="display:none" >完成保养</a>
                        </div>
                    </div>
                </li>
            </ul>
        </div>
               <input type="hidden" value={{$route.params.id}} id="billId">
            </div>
          </div>
        `,
        on: {
            pageInit: function () {
                var result = [];
                var html = [];
                this.app.request.json('./GetMoldMaintainBillInfo', {
                    billId: $$("#billId").val()
                }, (res) => {
                    if (res.code == 200) {
                        let data = res.data.data[0];
                        $$("#moldName").val(data.MoldName + "(" + data.MoldCode + ")");
                        $$("#moldCode").val(data.MoldCode);
                        $$("#mdispatchName").val(data.DispatchName);
                        $$("#mrepairStaffName").val(data.RepairStaffName);

                        this.app.request.json('./GetMoldMaintainBillTemp', {
                            tempId: data.TempID
                        }, (r) => {
                                if (r.code == 200) {
                                    r.data.data.map(item => {
                                        if (item.ControlType == 1) {
                                            html += `
                                            <li class="tempitem">
                                                <label class="item-checkbox item-content">
                                                <input type="checkbox" name="demo-media-checkbox" value="${item.Id}"/>
                                                <i class="icon icon-checkbox"></i>
                                                <div class="item-inner">
                                                    <div class="item-title-row">
                                                    <div class="item-title" style="white-space: normal !important;">${item.Project}</div>
                                                    <div class="item-after">检查方法：${item.CheckMethod}</div>
                                                    </div>
                                                    
                                                    <div class="item-text" style="white-space: normal !important;max-height: none;">保养方法：${item.UpkeepMethod}</div>
                                                    <input type="hidden" name="checkMoldNumber">
                                                    <div class="item-text">${item.Remark}</div>
                                                </div>
                                                </label>
                                            </li>
                                            `
                                        }
                                        if (item.ControlType == 0) {
                                            html += `
                                            <li class="tempitem">
                                                <label class="item-checkbox item-content">
                                                <input type="hidden" value="${item.Id}"/>
                                                <div class="item-inner">
                                                    <div class="item-title-row">
                                                    <div class="item-title" style="white-space: normal !important;">${item.Project}</div>
                                                    <div class="item-after">检查方法：${item.CheckMethod}</div>
                                                    </div>
                                                    <div class="item-text" style="white-space: normal !important;max-height: none;">保养方法：${item.UpkeepMethod}</div>
                                                    <input type="number" name="checkMoldNumber" placeholder="请输入数值" required validate>
                                                    <div class="item-text">${item.Remark}</div>
                                                </div>
                                                </label>
                                            </li>
                                            `
                                        }
                                    })
                                    $$("#tempContent").empty().append(`
                                    ${html}
                                    `)
                                }
                                result = r.data.data.map(item => {
                                    if (item.ControlType == 1) {
                                        item.Id = null;
                                        item.Result = "NG"
                                    }
                                    if (item.ControlType == 0) {
                                        item.Id = null;
                                        item.Result = "num"
                                    }
                                    return item;
                                })
                                $$('input[type="checkbox"]').change(function (e) {
                                    var targ = e.target
                                    var idx = $(targ).parents(".tempitem").index();
                                    if ($(targ).prop("checked")) {
                                        result[idx]["Result"] = "OK"
                                    } else {
                                        result[idx]["Result"] = "NG"
                                    }
                                })
                            })
                        if (data.Status > 1) {
                            $("#tempWrap").show()
                            $$("#btn_BeginMaintain").hide();
                            $$("#btn_EndMaintain").show();
                        }

                    }
                })



                $$("#btn_BeginMaintain").click(() => {
                    app.dialog.prompt('', '请扫入模具二维码', function (name) {
                        if (name.trim() == $$("#moldCode").val()) {
                            app.dialog.progress();
                            setTimeout(function () {
                                app.dialog.close();
                            }, 1500);
                            this.app.request.json('./BeginMoldMaintainBill', {
                                billId: $$("#billId").val(),
                            }, res => {
                                    if (res.code == 200) {
                                        $("#tempWrap").show()
                                        $$("#btn_BeginMaintain").hide();
                                        $$("#btn_EndMaintain").show();
                                    } else {
                                        setTimeout(() => {
                                            app.dialog.alert('', res.msg);
                                        }, 0)
                                    }
                            })
                        } else {
                            setTimeout(() => {
                                app.dialog.alert('', '并非本次保养的模具!');
                            },0)
                            
                        }
                    });
                })
                $$("#btn_EndMaintain").click(() => {
                    app.dialog.prompt('', '请扫入保养员的二维码', function (name) {
                        app.dialog.progress();
                        for (var i = 0; i < result.length; i++) {
                            console.log(result);
                            if (result[i].Result == "num") {
                                console.log($("input[name=checkMoldNumber]").eq(i).val())
                                if ($("input[name=checkMoldNumber]").eq(i).val() != "") {
                                    result[i].Result = $("input[name=checkMoldNumber]").eq(i).val();
                                }
                            }
                        }
                        this.app.request.post('./EndMoldMaintainBill', {
                            billId: $$("#billId").val(),
                            checker: name,
                            sub: result
                        }, res => {
                            res = JSON.parse(res);
                            app.dialog.close();
                            if (res.code == 200) {
                                app.views.current.router.back()
                            } else {
                                setTimeout(() => {
                                    app.dialog.alert('', res.msg);
                                }, 0)
                            }
                        })
                    });
                })
            }
        }
    },
    {
        path: '/moldmaintainrecord/:id',
        template: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">保养记录</div>
                </div>
            </div>
            <div class="page-content">
            <div class="list no-hairlines-md">
            <ul>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">模具</div>
                            <div class="item-input-wrap">
                                <input type="text" id="moldName" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                <div class="item-content item-input" style="display:none">
                     <div class="item-inner">
                            <div class="item-title item-label">模具编码</div>
                            <div class="item-input-wrap">
                                <input type="text" id="moldCode" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
               
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner ">
                            <div class="item-title item-label">派单人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="mdispatchName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                  <li>
                    <div class="item-content item-input">
                        <div class="item-inner ">
                            <div class="item-title item-label">派单时间</div>
                            <div class="item-input-wrap">
                                <input type="text" id="mdispatchTime" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">保养人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="mrepairStaffName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                 <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">开始保养时间</div>
                            <div class="item-input-wrap">
                                <input type="text" id="mBeginTime" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">结束保养时间</div>
                            <div class="item-input-wrap">
                                <input type="text" id="mEndTime" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li id='tempWrap'>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">保养内容</div>
                            <div class="item-input-wrap">
                                <div class="list media-list">
                                <ul id='mtempContent' style="padding-left:0">
                                </ul>
                </div>
                </li>
                <li class="align-top">
                    <div class="item-content item-input">
                        <div class="item-inner"> 
                        </div>
                    </div>
                </li>
            </ul>
        </div>
               <input type="hidden" value={{$route.params.id}} id="mbillId">
            </div>
          </div>
        `,
        on: {
            pageInit: function () {
                var result = [];
                var html = [];
                this.app.request.json('./GetMoldMaintainBillInfo', {
                    billId: $$("#mbillId").val()
                }, (res) => {
                    if (res.code == 200) {
                        let data = res.data.data[0];
                        $$("#moldName").val(data.MoldName + "(" + data.MoldCode + ")");
                        $$("#moldCode").val(data.MoldCode);
                        $$("#mdispatchName").val(data.DispatchName);
                        $$("#mdispatchTime").val(data.DispatchTime);
                        $$("#mrepairStaffName").val(data.RepairStaffName);
                        $$("#mBeginTime").val(data.BeginTime);
                        $$("#mEndTime").val(data.EndTime);
                        this.app.request.json('./GetMoldMaintainBillSubInfo', {
                            id: $$("#mbillId").val()
                        }, (r) => {
                                if (r.code == 200) {
                                    r.data.data.map(item => {
                                        if (item.ControlType == 1) {
                                            html += `
                                            <li class="tempitem">
                                                <label class="item-checkbox item-content">
                                                <input type="checkbox" disabled name="demo-media-checkbox" ${item.Result == 'OK' ? "checked" : ''} value="${item.Id}"/>
                                                <i class="icon icon-checkbox"></i>
                                                <div class="item-inner">
                                                    <div class="item-title-row">
                                                    <div class="item-title" style="white-space: normal !important;">${item.Project}</div>
                                                    <div class="item-after">检查方法：${item.CheckMethod}</div>
                                                    </div>
                                                    <div class="item-text" style="white-space: normal !important;max-height: none;">保养方法：${item.UpkeepMethod}</div>
                                                    <input type="hidden" name="checkMoldNumber">
                                                </div>
                                                </label>
                                            </li>
                                            `
                                        }
                                        if (item.ControlType == 0) {
                                            html += `
                                            <li class="tempitem">
                                                <label class="item-checkbox item-content">
                                                <input type="hidden" value="${item.Id}"/>
                                                <div class="item-inner">
                                                    <div class="item-title-row">
                                                    <div class="item-title" style="white-space: normal !important">${item.Project}</div>
                                                    <div class="item-after">检查方法：${item.CheckMethod}</div>
                                                    </div>
                                                    <div class="item-text" style="white-space: normal !important;max-height: none;">保养方法：${item.UpkeepMethod}</div>
                                                    <input type="number" name="checkFaciliyNumber" placeholder="请输入数值" value="${item.Result}" disabled>
                                                </div>
                                                </label>
                                            </li>
                                            `
                                        }
                                    })
                                    $$("#mtempContent").empty().append(`
                                    ${html}
                                    `)
                                }
                        })
                        if (data.Status > 1) {
                            $("#tempWrap").show()
                            $$("#btn_BeginMaintain").hide();
                            $$("#btn_EndMaintain").show();
                        }

                    }
                })
            }
        }
    },
    {
        path: '/facilityMaintain/',
        url: './facilityMaintain/',
        on: {
            pageAfterIn: function () {
                this.app.request.json('./CheckMaintainAuth/', function (res) {
                    if (res.code == 200) {
                        $("#btn_MaintainDispatch").removeClass("disabled")
                    }
                })
                
                function renderStatus(status) {
                    switch (status) {
                        case 0:
                            return "未派单";
                            break;
                        case 1:
                            return "已派单";
                            break;
                        case 2:
                            return "开始保养";
                            break;
                        case 3:
                            return "完成保养";
                            break;
                    }
                }
                function renderAction(status, id) {
                    switch (status) {
                        case 0:
                            return "/facilityMaintainDispatch/" + id;
                            break;
                        case 1:
                            return "/famaintain/" + id;
                            break;
                        case 2:
                            return "/famaintain/" + id;
                            break;
                        case 3:
                            return "/famaintaindetail/" + id;
                            break;
                        default:
                            return ""
                    }
                }
                let sel = app.smartSelect.get("#fadatetype");
                sel.on('closed', () => {
                    let type = sel.getValue()
                    this.app.request.json('./getFacilityMaintainBill/', { type }, function (res) {
                        $$("#dispatchlist").empty().append(`
                        ${res.data.data.map(item =>
                            `
                                <li>
                                    <a href="${renderAction(item.Status, item.Id)}" class="item-link item-content">
                                        <div class="item-inner">
                                            <div class="item-title-row">
                                                <div class="item-title">${item.FacilityName}(${item.FacilityCode})</div>
                                                <div class="item-after">${renderStatus(item.Status)}</div>
                                            </div>
                                            <div class="item-subtitle">${item.PlanDate.replace(' 00:00:00', '')}(${item.TempName})</div>
                                           
                                        </div>
                                    </a>
                                </li>
                            `
                        ).join('')}
                     `)
                    })
                })
                    this.app.request.json('./getFacilityMaintainBill/', { type: sel.getValue() }, function (res) {
                        $$("#dispatchlist").empty().append(`
                        ${res.data.data.map(item =>
                            `
                                <li>
                                    <a href="${renderAction(item.Status, item.Id)}" class="item-link item-content">
                                        <div class="item-inner">
                                            <div class="item-title-row">
                                                <div class="item-title">${item.FacilityName}(${item.FacilityCode})</div>
                                                <div class="item-after">${renderStatus(item.Status)}</div>
                                            </div>
                                            <div class="item-subtitle">${item.PlanDate.replace(' 00:00:00', '')}(${item.TempName})</div>
                                           
                                        </div>
                                    </a>
                                </li>
                            `
                        ).join('')}
                     `)
                    })
                this.app.request.json('./GetMyFacilityMaintainBill/', function (res) {
                    $$("#maintainlist").empty().append(`
                        ${res.data.data.map(item =>
                        `
                                <li>
                                    <a href="${renderAction(item.Status, item.Id)}" class="item-link item-content">
                                        <div class="item-inner">
                                            <div class="item-title-row">
                                                <div class="item-title">${item.FacilityName}(${item.FacilityCode})</div>
                                                <div class="item-after">${renderStatus(item.Status)}</div>
                                            </div>
                                            <div class="item-subtitle">${item.PlanDate.replace(' 00:00:00', '')}</div>
                                           
                                        </div>
                                    </a>
                                </li>
                            `
                    ).join('')}
                     `)
                })
                this.app.request.json('./GetMyFacilityMaintainBillHistory/', function (res) {
                    $$("#maintainhistorylist").empty().append(`
                        ${res.data.data.map(item =>
                            `
                                <li>
                                    <a href="${renderAction(item.Status, item.Id)}" class="item-link item-content">
                                        <div class="item-inner">
                                            <div class="item-title-row">
                                                <div class="item-title">${item.FacilityName}(${item.FacilityCode})</div>
                                                <div class="item-after">${renderStatus(item.Status)}</div>
                                            </div>
                                            <div class="item-subtitle">${item.EndTime}</div>
                                           
                                        </div>
                                    </a>
                                </li>
                            `
                        ).join('')}
                     `)
                })

                setInterval(() => {
                    try {
                        this.app.request.json('./GetMyFacilityMaintainBill/', { type: sel.getValue() }, function (res) {
                            if ($$("#famaintaincount span").length > 0) {
                                if (res.data.data.length == 0) {
                                    $$("#famaintaincount span").remove()
                                } else {
                                    $$("#famaintaincount span").text(res.data.data.length)
                                }
                            } else {
                                if (res.data.data.length > 0) {
                                    $$("#famaintaincount").append(`<span class="badge color-red">${res.data.data.length}</span>`)
                                }
                            }
                        })
                    } catch (e) {}
                }, 50000)
            }
        }
    },
    {
        path: '/facilityMaintainDispatch/:id',
        template: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">保养派单</div>
                </div>
            </div>
            <div class="page-content">
              <div class="list media-list ">
                            <ul id="mastafflist">
                                
                            </ul>
                            <button type="button" class="button button-fill" id="btn_send">派单</button>
                        </div>
            <input type="hidden" value={{$route.params.id}} id="fmbillId">
            </div>
          </div>
        `,
        on: {
            pageInit: function () {
                this.app.request.json('./getRepairStaff/', function (res) {
                    $("#mastafflist").empty().append(`
                        ${res.data.data.map(item => `
                            <li>
                                <label class="item-radio item-content">
                                <input type="radio" name="facility_maintain_staff" value="${item.UserId}"  />
                                <i class="icon icon-radio" checked></i>
                                <div class="item-inner">
                                    <div class="item-title-row">
                                    <div class="item-title">${item.UserName}</div>
                                    <div class="item-after">休闲中</div>
                                    </div>
                                    <div class="item-subtitle">${item.UserCode}</div>
                                </div>
                                </label>
                            </li>
                        `).join('')}
                    `)

                })
                var notification = app.notification.create({
                    title: '派单成功',
                    text: '',
                    closeTimeout: 3000,
                    on: {
                        opened: function () {

                        }
                    }
                })
                function renderAction(status, id) {
                    switch (status) {
                        case 0:
                            return "/facilityMaintainDispatch/" + id;
                            break;
                        case 1:
                            return "/famaintain/" + id;
                            break;
                        case 2:
                            return "/famaintain/" + id;
                            break;
                        default:
                            return ""
                    }
                }
                function renderStatus(status) {
                    switch (status) {
                        case 0:
                            return "未派单";
                            break;
                        case 1:
                            return "已派单";
                            break;
                        case 2:
                            return "开始保养";
                            break;
                        case 3:
                            return "完成保养";
                            break;
                    }
                }
                $$("#btn_send").click(() => {
                    if (!$$("[name='facility_maintain_staff']:checked").val()) {
                        return false
                    }

                    this.app.request.json('./DispatchFacilityMaintain', {
                        billId: $("#fmbillId").val(),
                        userId: $$("[name='facility_maintain_staff']:checked").val()
                    }, (res) => {
                        if (res.code == 200) {
                            notification.open()
                            let sel = app.smartSelect.get("#fadatetype");
                            let type = sel.getValue()
                            this.app.request.json('./getFacilityMaintainBill/', { type }, function (res) {
                                $$("#dispatchlist").empty().append(`
                                    ${res.data.data.map(item =>
                                    `
                                        <li>
                                            <a href="${renderAction(item.Status, item.Id)}" class="item-link item-content">
                                                <div class="item-inner">
                                                    <div class="item-title-row">
                                                        <div class="item-title">${item.FacilityName}(${item.FacilityCode})</div>
                                                        <div class="item-after">${renderStatus(item.Status)}</div>
                                                    </div>
                                                    <div class="item-subtitle">${item.PlanDate.replace(' 00:00:00', '')}</div>
                                           
                                                </div>
                                            </a>
                                        </li>
                                    `
                                ).join('')}
                                 `)
                                app.views.current.router.back()
                                //facilitymaintainView.router.back()
                            })

                            // facilitymaintainView.router.navigate("/facilityMaintain/", { animate: false });
                        }
                    })
                })
            }
        }
    },
    {
        path: '/famaintain/:id',
        template: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">开始保养</div>
                </div>
            </div>
            <div class="page-content">
            <div class="list no-hairlines-md">
            <ul>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">设备</div>
                            <div class="item-input-wrap">
                                <input type="text" id="facilityName" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                <div class="item-content item-input" style="display:none">
                     <div class="item-inner">
                            <div class="item-title item-label">设备编码</div>
                            <div class="item-input-wrap">
                                <input type="text" id="facilityCode" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
               
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner ">
                            <div class="item-title item-label">派单人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="dispatchName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">保养人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="repairStaffName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li id='tempWrap' style="display:none">
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">保养内容</div>
                            <div class="item-input-wrap">
                                <div class="list media-list">
                                <ul id='tempContent' style="padding-left:0">
                                </ul>
                </div>
                </li>
                <li style="display:none" id="maintainRemarkWrap">
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">备注</div>
                            <div class="item-input-wrap">
                                <input type="text" id="maintainRemark"  placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li class="align-top">
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <a href="#" class="button  button-large button-fill button-raised" id="btn_faBeginMaintain" >开始保养</a>
                            <a href="#" class="button  button-large button-fill button-raised" id="btn_EndMaintain" style="display:none" >完成保养</a>
                        </div>
                    </div>
                </li>
            </ul>
        </div>
               <input type="hidden" value={{$route.params.id}} id="fmabillId">
            </div>
          </div>
        `,
        on: {
            pageAfterIn: function () {
                var result = [];
                var html = [];
                this.app.request.json('./GetFacilityMaintainBillInfo', {
                    billId: $$("#fmabillId").val()
                }, (res) => {
                    if (res.code == 200) {
                        let data = res.data.data[0];
                        $$("#facilityName").val(data.FacilityName + "(" + data.FacilityCode + ")");
                        $$("#facilityCode").val(data.FacilityCode);
                        $$("#dispatchName").val(data.DispatchName);
                        $$("#repairStaffName").val(data.RepairStaffName);

                        this.app.request.json('./GetFacilityMaintainBillTemp', {
                            tempId: data.TempID
                        }, (r) => {
                                if (r.code == 200) {
                                    r.data.data.map(item => {
                                        if (item.ControlType == 1) {
                                            html += `
                                            <li class="tempitem">
                                                <label class="item-checkbox item-content">
                                                <input type="checkbox" name="demo-media-checkbox" value="${item.Id}"/>
                                              
                                                <i class="icon icon-checkbox"></i>
                                                <div class="item-inner">
                                                    <div class="item-title-row">
                                                    <div class="item-title" style="white-space: normal !important;">${item.Project}</div>
                                                    <div class="item-after">检查方法：${item.CheckMethod}</div>
                                                    </div>
                                                    <div class="item-text" style="white-space: normal !important;max-height: none;">保养方法：${item.UpkeepMethod}</div>
                                                    <input type="hidden" name="checkMoldNumber">
                                                    <div class="item-text">${item.Remark}</div>
                                                </div>
                                                </label>
                                            </li>
                                            `
                                        }
                                        if (item.ControlType == 0) {
                                            html += `
                                            <li class="tempitem">
                                                <label class="item-checkbox item-content">
                                                <input type="hidden" value="${item.Id}"/>
                                                <div class="item-inner">
                                                    <div class="item-title-row">
                                                    <div class="item-title" style="white-space: normal !important;">${item.Project}</div>
                                                    <div class="item-after">检查方法：${item.CheckMethod}</div>
                                                    </div>
                                                    <div class="item-text" style="white-space: normal !important;max-height: none;">保养方法：${item.UpkeepMethod}</div>
                                                    <input type="number" name="checkFaciliyNumber" placeholder="请输入数值" required validate>
                                                    <div class="item-text">${item.Remark}</div>
                                                </div>
                                                </label>
                                            </li>
                                            `
                                        }
                                    })
                                $$("#tempContent").empty().append(`
                                    ${html}
                                    `)
                            }
                                result = r.data.data.map(item => {
                                    if (item.ControlType == 1) {
                                        item.Id = null;
                                        item.Result = "NG"
                                    }
                                    if (item.ControlType == 0) {
                                        item.Id = null;
                                        item.Result = "num"
                                    }
                                    return item;
                            })
                            $$('input[type="checkbox"]').change(function (e) {
                                var targ = e.target
                                var idx = $(targ).parents(".tempitem").index();
                                if ($(targ).prop("checked")) {
                                    result[idx]["Result"] = "OK"
                                } else {
                                    result[idx]["Result"] = "NG"
                                }
                            })
                        })
                        if (data.Status > 1) {
                            $("#tempWrap").show()
                            $$("#btn_faBeginMaintain").hide();
                            $$("#btn_EndMaintain").show();
                            $$("#maintainRemarkWrap").show()
                        }
                    }
                })
                $$("#btn_faBeginMaintain").click(() => {
                    app.dialog.prompt('', '请扫入设备二维码', function (name) {
                        if (name.trim().toLowerCase() == $$("#facilityCode").val().toLowerCase()) {
                            app.dialog.progress();
                            setTimeout(function () {
                                app.dialog.close();
                            }, 1500);
                            this.app.request.json('./BeginFacilityMaintainBill', {
                                billId: $$("#fmabillId").val()
                            }, res => {
                                if (res.code == 200) {
                                    $("#tempWrap").show()
                                    $$("#btn_faBeginMaintain").hide();
                                    $$("#btn_EndMaintain").show();
                                    $$("#maintainRemarkWrap").show()
                                }
                            })
                        } else {
                            app.dialog.alert('', '并非本次保养的设备!', () => { return false }, () => { return false });
                        }
                    }, () => { return false });
                })
                $$("#btn_EndMaintain").click(() => {
                    app.dialog.prompt('', '请扫入保养员的二维码', function (name) {
                        app.dialog.progress();
                        for (var i = 0; i < result.length; i++) {
                            if (result[i].Result == "num") {
                                if ($("input[name=checkFaciliyNumber]").eq(i).val() != "") {
                                    result[i].Result = $("input[name=checkFaciliyNumber]").eq(i).val();
                                }
                            }
                        }
                            this.app.request.post('./EndFacilityMaintainBill', {
                                billId: $$("#fmabillId").val(),
                                checker: name,
                                remark: $$("#maintainRemark").val(),
                                sub: result
                            }, res => {
                                res = JSON.parse(res);
                                app.dialog.close();
                                if (res.code == 200) {
                                    app.views.current.router.back()
                                    //facilitymaintainView.router.back()
                                } else {
                                    app.dialog.alert('', res.msg);
                                }
                            })
                    });
                })
            }
        }
    },
    {
        path: '/famaintaindetail/:id',
        template: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">保养记录</div>
                </div>
            </div>
            <div class="page-content">
            <div class="list no-hairlines-md">
            <ul>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">设备</div>
                            <div class="item-input-wrap">
                                <input type="text" id="his_facilityName" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner ">
                            <div class="item-title item-label">派单人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="his_dispatchName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner ">
                            <div class="item-title item-label">派单时间</div>
                            <div class="item-input-wrap">
                                <input type="text" id="his_dispatchTime" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">保养人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="his_repairStaffName" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">开始保养时间</div>
                            <div class="item-input-wrap">
                                <input type="text" id="his_faBeginTime" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">结束保养时间</div>
                            <div class="item-input-wrap">
                                <input type="text" id="his_faEndTime" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li id='tempWrap'>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">保养内容</div>
                            <div class="item-input-wrap">
                                <div class="list media-list">
                                <ul id='his_tempContent' style="padding-left:0">
                                </ul>
                </div>
                </li>
                <li id="maintainRemarkWrap">
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">备注</div>
                            <div class="item-input-wrap">
                                <input type="text" id="his_maintainRemark" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
            </ul>
        </div>
               <input type="hidden" value={{$route.params.id}} id="fmabillId">
            </div>
          </div>
        `,
        on: {
            pageAfterIn: function () {
                var result = [];
                var html = [];
                this.app.request.json('./GetFacilityMaintainBillInfo', {
                    billId: $$("#fmabillId").val()
                }, (res) => {
                    if (res.code == 200) {
                        let data = res.data.data[0];
                        $$("#his_facilityName").val(data.FacilityName + "(" + data.FacilityCode + ")");
                        $$("#his_facilityCode").val(data.FacilityCode);
                        $$("#his_dispatchName").val(data.DispatchName);
                        $$("#his_repairStaffName").val(data.RepairStaffName);
                        $$("#his_dispatchTime").val(data.DispatchTime);
                        $$("#his_faBeginTime").val(data.BeginTime);
                        $$("#his_faEndTime").val(data.EndTime);
                        $$("#his_maintainRemark").val(data.Remark);
                        this.app.request.json('./GetFacilityMaintainBillSub', {
                            id: $$("#fmabillId").val()
                        }, (r) => {
                                if (r.code == 200) {
                                    r.data.map(item => {
                                        if (item.ControlType == 1) {
                                            html += `
                                            <li class="tempitem">
                                                <label class="item-checkbox item-content">
                                                <input type="checkbox" disabled name="demo-media-checkbox" ${item.Result == 'OK' ? "checked" : ''} value="${item.Id}"/>
                                                <i class="icon icon-checkbox"></i>
                                                <div class="item-inner">
                                                    <div class="item-title-row">
                                                    <div class="item-title" style="white-space: normal !important;">${item.Project}</div>
                                                    <div class="item-after">检查方法：${item.CheckMethod}</div>
                                                    </div>
                                                    <div class="item-text" style="white-space: normal !important;max-height: none;">保养方法：${item.UpkeepMethod}</div>
                                                    <input type="hidden" name="checkMoldNumber">
                                                </div>
                                                </label>
                                            </li>
                                            `
                                        }
                                        if (item.ControlType == 0) {
                                            html += `
                                            <li class="tempitem">
                                                <label class="item-checkbox item-content">
                                                <input type="hidden" value="${item.Id}"/>
                                                <div class="item-inner">
                                                    <div class="item-title-row">
                                                    <div class="item-title" style="white-space: normal !important;">${item.Project}</div>
                                                    <div class="item-after">检查方法：${item.CheckMethod}</div>
                                                    </div>
                                                    <div class="item-text" style="white-space: normal !important;max-height: none;">保养方法：${item.UpkeepMethod}</div>
                                                    <input type="number" name="checkFaciliyNumber" placeholder="请输入数值" value="${item.Result}" disabled>
                                                </div>
                                                </label>
                                            </li>
                                            `
                                        }
                                    })
                                    $$("#his_tempContent").empty().append(`
                                    ${html}
                                    `)
                            }
                        })
                    }
                })
            }
        }
    },
    {
        path: '/moldout/',
        content: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">模具出库</div>
                <div class="right">
               <div class="right">
                <a href="/moldout/" data-animate="false" data-reload-current="true" class="link not-animated"><i class="f7-icons">reload_round</i></a>
                </div>
                </div>
                    </div>
                </div>
            <div class="page-content">
              <div class="list media-list ">
                    <ul id="moldoutlist">
                                
                    </ul>
                </div>
            </div>
          </div>
        `,
        on: {
            pageAfterIn: function () {
                this.app.request.json('./getMoldOutBill/', function (res) {

                    if ($$("#moldoutcount").find("span").length > 0) {
                        if (res.data.length == 0) {
                            $$("#moldoutcount span").remove()
                        } else {
                            $$("#moldoutcount span").text(res.data.length)
                        }
                    } else {
                        if (res.data.length > 0) {
                            $$("#moldoutcount").append(`<span class="badge color-red">${res.data.length}</span>`)
                        }
                    }
                    $$("#moldoutlist").empty().append(`
                        ${res.data.map(item => {
                            return `
                                <li style="border-bottom:1px solid #e4eff4">
                                    <a href="/moldoutscan/${item.Id}/" class="item-link item-content">
                                        <div class="item-inner">
                                            <div class="item-title-row">
                                                <div class="item-title">换模请求</div>
                                                <div class="item-after">${item.CreateTime}</div>
                                            </div>
                                            <div class="item-subtitle">模具：${item.Name}</div>
                                            <div class="item-text ">
                                                <ul class="">
                                                    <li>
                                                        设备：${item.FacilityName}(${item.FacilityCode})
                                                    </li>
                                                    <li>
                                                        申请人：${item.Maker}
                                                    </li>
                                                </ul>
                                            </div>
                                        </div>
                                    </a>
                                </li>
                            `
                        }).join('')}
                     `)
                })

            }
        }
    },
    {
        path: '/moldoutscan/:id/',
        template: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">扫描模具</div>
                </div>
            </div>
            <div class="page-content">
                 <div class="list">
                    <ul>
                    <li class="item-content item-input">
                        <div class="item-media">扫描模具</div>
                        <div class="item-inner">
                        <div class="item-input-wrap">
                            <input type="text" name="name" id="moldscanBarcode" >
                            <i id='qrcode_moldscanBarcode' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                            <span class="input-clear-button"></span>
                        </div>
                        </div>
                    </li>
                    <li class="item-content item-input" id='partWrap' style="display:none">
                        <div class="item-media">配件</div>
                        <div class="item-inner">
                        <div class="item-input-wrap">
                            <input type="text" name="name" id="partBarcode" >
                            <span class="input-clear-button"></span>
                        </div>
                        </div>
                    </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">需求模具</div>
                            <div class="item-input-wrap">
                                <input type="text" id="needMold" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                <div class="item-content item-input" style="display:none">
                     <div class="item-inner">
                            <div class="item-title item-label">模具编码</div>
                            <div class="item-input-wrap">
                                <input type="text" id="needMoldCode" readonly placeholder="" value="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">需求设备</div>
                            <div class="item-input-wrap">
                                <input type="text" id="needFacility" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li>
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">需求人员</div>
                            <div class="item-input-wrap">
                                <input type="text" id="needMaker" readonly placeholder="" />
                            </div>
                        </div>
                    </div>
                </li>
                <li >
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">需求配件</div>
                            <div class="item-input-wrap">
                                <div class="list">
                                <ul id='needParts'>

                                </ul>
                </div>
                </li>
                <li >
                    <div class="item-content item-input">
                        <div class="item-inner">
                            <div class="item-title item-label">计划单号</div>
                            <div class="item-input-wrap">
                                <div class="list">
                                <ul id='needBills'>

                                </ul>
                </div>
                </li>
                    </ul>
                <button type="button" class="button button-fill" id="btn_cancel">取消出库</button>
                </div>
                <input type="hidden" value={{$route.params.id}}  id="messageId">
                <input type="hidden" id="needMoldId">
                <input type="hidden" id="needFacilityId">

            </div>
          </div>
        `,
        on: {
            pageInit: function () {
                let partsAll = [];
                let scaned = [];
                this.app.request.json('./getMoldOutInfo/', { Id: $$("#messageId").val() }, function (res) {
                    if (res.code != 200) {
                        app.dialog.alert('', res.msg, () => { });
                        return false;
                    }
                    if (res.count == 0) {
                        app.dialog.alert('', "该请求已经完成或者取消，请返回", () => { });
                        return false;
                    }
                    let content = res.data[0];
                    let materialName = content.materialName
                    $("#needMold").val(content.Name + "(" + content.Code+")")
                    $("#needMoldCode").val(content.Code)
                    $("#needFacility").val(content.FacilityName)
                    $("#needFacilityId").val(content.FacilityId)

                    $("#needMaker").val(content.Maker)
                    $("#needMoldId").val(content.MoldId)
                    //$("#needBills").empty().append(`
                    //    ${content.Plan.map((item, i) => `<li>${item.PlanNo}${materialName && materialName[i] && materialName[i].materialName ? "--" + materialName[i].materialName : ""}</li>`).join('')}
                    //`)
                    //$("#needParts").empty().append(`
                    //    ${content.Parts.map(item => `<li>${item.Name}</li>`).join('')}
                    //`)
                    //partsAll = content.Parts.map(({ Code }) => Code)
                })
                $$("#btn_cancel").click(function () {
                    app.dialog.confirm("", "确定要取消吗", function () {
                        app.dialog.prompt('', '请验证权限', function (name) {
                            this.app.request.json('./checkUser', {
                                user: name
                            }, res => {
                                if (res.code == 200) {
                                    this.app.request.json('./CancelBill', {
                                        id: $$("#messageId").val()
                                    }, (res) => {
                                        //moldView.router.back()
                                            app.views.current.router.back()
                                        //moldView.router.navigate("/mold/", { animate: false });
                                    })
                                } else {
                                    app.dialog.alert('', '权限验证错误!');
                                }
                            })
                        });
                    })
                })
                $$("#qrcode_moldscanBarcode").on("click", function (e) {
                    if ($$("#moldscanBarcode")[0].hasAttribute("readonly")) {
                        return false
                    }
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#moldscanBarcode").val(inputValue);
                                    CheckscanMold(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })
                function CheckscanMold() {
                    setTimeout(() => {
                        if ($$("#moldscanBarcode").val().trim().toLowerCase() != $("#needMoldCode").val().trim().toLowerCase()) {
                            app.dialog.alert('', "模具不匹配", function () {
                                $("#moldscanBarcode").val("").focus()
                            })
                        } else {
                            this.app.request.json('./CheckMoldStatus', {
                                moldId: $("#needMoldId").val()
                            }, res => {
                                if (res.code == 200) {
                                    if (partsAll.length == 0) {
                                        app.dialog.prompt('', '请验证权限', function (name) {
                                            app.dialog.progress();
                                            this.app.request.json('./EndMoldOutBill', {
                                                msgId: $$("#messageId").val(),
                                                checker: name,
                                                moldId: $("#needMoldId").val(),
                                                FacilityId: $("#needFacilityId").val(),
                                            }, res => {
                                                app.dialog.close();
                                                if (res.code == 200) {
                                                    //moldView.router.back()
                                                    app.views.current.router.back()
                                                } else {
                                                    app.dialog.alert('', '权限验证错误!', () => {
                                                        $$("#moldscanBarcode").val("").focus()
                                                    });
                                                }
                                            })
                                        });
                                    } else {
                                        $("#partWrap").show()
                                        $("#partBarcode").val("").focus();
                                    }

                                } else {
                                    app.dialog.alert('', res.msg, () => {
                                        $("#moldscanBarcode").val("").focus()
                                    });
                                    $("#moldscanBarcode").val("").focus()
                                }
                            })
                        }
                    })
                }
                $$("#moldscanBarcode").focus().on('keydown', e => {
                    if (e.keyCode == 13) {
                        if ($$("#moldscanBarcode").val() == "") {
                            return false;
                        }
                        CheckscanMold($$("#moldscanBarcode").val())
                    }
                })
                function equar(a, b) {
                    if (a.length !== b.length) {
                        return false
                    } else {
                        for (let i = 0; i < a.length; i++) {
                            if (a[i] !== b[i]) {
                                return false
                            }
                        }
                        return true;
                    }
                }
                $$("#partBarcode").on('keydown', e => {
                    if (e.keyCode == 13) {
                        if ($$("#partBarcode").val() == "") {
                            return false
                        }
                        setTimeout(() => {
                            if (!partsAll.includes($("#partBarcode").val())) {
                                app.dialog.alert('', '配件不匹配!', () => {
                                    $("#partBarcode").val("").focus()
                                });
                            } else {
                                scaned.push($("#partBarcode").val())
                                if (equar(scaned, partsAll)) {
                                    app.dialog.prompt('', '请验证权限', function (name) {
                                        app.dialog.progress();
                                        this.app.request.json('./EndMoldOutBill', {
                                            msgId: $$("#messageId").val(),
                                            checker: name,
                                            moldId: $("#needMoldId").val(),
                                            FacilityId: $("#needFacilityId").val(),
                                        }, res => {
                                            app.dialog.close();
                                            if (res.code == 200) {
                                                // moldView.router.back()
                                                app.views.current.router.back()
                                                //moldView.router.navigate("/mold/", { animate: false });
                                            } else {
                                                app.dialog.alert('权限验证错误!');
                                            }
                                        })
                                    });
                                } else {
                                    $("#partBarcode").val("").focus()
                                }
                            }
                        })
                    }
                })
            }
        }
    },
    {
        path: '/moldin/',
        content: `
          <div class="page">
            <div class="navbar">
                <div class="navbar-inner">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">模具入库</div>
                    <div class="subnavbar">
                        <div class="subnavbar-inner">
                            <div class="segmented segmented-raised">
                                <a class="button tab-link tab-link-active" href="#tabmoldin">入库</a>
                                <a id="btn_Moldinhistory" class="button tab-link " href="#tabmoldinhistory">我的入库记录</a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="page-content hide-navbar-on-scroll">
                <div class="tabs">
                    <div class="tab tab-active" id="tabmoldin">
                        <div class="block" id="">
                           <div class="list">
                                <ul>
                                <li class="item-content item-input">
                                    <div class="item-media">模具</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="moldinBarcode" >
                                        <i id='qrcode_moldinBarcode' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                                    </div>
                                    </div>
                                </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                    <div class="tab " id="tabmoldinhistory">
                        <div class="block" id="">
                            <div class="list media-list " style="margin:10px 0">
                                <ul id="mymouldinhistory"></ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

        `,
        on: {
            pageInit: function () {
                function scanMoldIn(bar) {
                    this.app.request.json('./moldInScanBarcode', { barcode: $$("#moldinBarcode").val() }, function (res) {
                        if (res.code == 200) {
                            app.dialog.alert('', "入库成功", () => {
                                $$("#moldinBarcode").val("").focus();
                            })
                        } else {
                            app.dialog.alert('', "入库失败:" + res.msg, () => {
                                $$("#moldinBarcode").val("").focus();
                            })
                        }
                    })
                }
                $$("#moldinBarcode").val("").focus();
                $$("#moldinBarcode").on('keydown', e => {
                    if ($$("#moldinBarcode").val() == "") {
                        return false
                    }
                    if (e.keyCode == 13) {
                        scanMoldIn($$("#moldinBarcode").val())

                    }
                })
                $$("#btn_Moldinhistory").on("click", e => {
                    this.app.request.json('./GetMyMoldInHistory', {  }, function (res) {
                        if (res.code == 200) {
                            $$("#mymouldinhistory").empty().append(`
                        ${res.data.map(item =>
                                    `
                                <li>
                                    <a href="#" class="item-link item-content">
                                        <div class="item-inner">
                                            <div class="item-title-row">
                                                <div class="item-title">模具:${item.Name}(${item.Code})</div>
                                                <div class="item-after">${item.FromName}</div>
                                            </div>
                                            <div class="item-subtitle">${item.CreateDate.slice(5)}</div>
                                           
                                        </div>
                                    </a>
                                </li>
                            `
                                ).join('')}
                     `)
                        } 
                    })
                })
                $$("#btn_clearMoldout").on("click", e => { })
                $("#qrcode_moldinBarcode").on("click", e => {
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#moldinBarcode").val(inputValue);
                                    scanMoldIn(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })
            }
        }
    },
    {
        path: '/moldrepairout/',
        content: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">模具出库</div>
                    <div class="subnavbar">
                        <div class="subnavbar-inner">
                            <div class="segmented segmented-raised">
                                <a class="button tab-link tab-link-active" href="#tabmoldout">出库</a>
                                <a id="btn_Moldouthistory" class="button tab-link " href="#tabmoldouthistory">我的出库记录</a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="page-content">
                <div class="tabs">
                    <div class="tab tab-active" id="tabmoldout">
                        <div class="block" id="">
                            <div class="list">
                                <ul>
                                <li class="item-content item-input">
                                    <div class="item-media">模具</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="moldoutBarcode" >
                                        <i id='qrcode_moldoutBarcode' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                                    </div>
                                    </div>
                                </li>
                                 <li class="item-content item-input" >
                                    <div class="item-media">设备</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="moldoutFacility" id="moldoutFacility" >
                                        <i id='qrcode_moldoutFacility' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                                    </div>
                                    </div>
                                </li>
                                </ul>
                                <button class="col button button-fill color-blue" id="btn_clearMoldout">清除</button>
                            </div>
                        </div>
                    </div>
                    <div class="tab " id="tabmoldouthistory">
                        <div class="block" id="">
                            <div class="list media-list " style="margin:10px 0">
                                <ul id="mymouldouthistory"></ul>
                            </div>
                        </div>
                    </div>
                </div>
               
            </div>
          </div>
        `,
        on: {
            pageInit: function () {
                var notification = app.notification.create({
                    title: '领出成功',
                    text: '',
                    closeTimeout: 3000,
                    on: {
                        opened: function () {

                        }
                    }
                })
                $$("#moldoutBarcode").val("").focus();
                $$("#moldoutFacility").val("").attr("readonly","readonly")
                $$("#moldoutBarcode").on('keydown', e => {
                    if (e.keyCode == 13) {
                        if ($$("#moldoutBarcode").val() == "") {
                            return false;
                        }
                        scanMold($$("#moldoutBarcode").val())
                    }
                })

                function scanMold(bar) {
                    this.app.request.json('./moldOutScanBarcode/', { barcode: bar }, function (res) {
                        if (res.code == 200) {
                            //notification.open();
                            //$$("#moldoutBarcode").val("").focus();
                            $$("#moldoutBarcode").attr("readonly", "readonly");
                            $$("#moldoutFacility").removeAttr("readonly").focus();
                        } else {
                            app.dialog.alert('', "失败:" + res.msg, () => {
                                $$("#moldoutBarcode").val("").focus();
                            })
                        }
                    })
                }
                function scanFacility(bar) {
                    this.app.request.json('./moldOutScanFacility/', { barcode: $$("#moldoutBarcode").val(), facility: bar }, function (res) {
                        if (res.code == 200) {
                            notification.open();
                            $$("#moldoutBarcode").removeAttr("readonly").val("").focus();
                            $$("#moldoutFacility").val("").attr("readonly", "readonly");
                        } else {
                            app.dialog.alert('', "出库失败:" + res.msg, () => {
                                $$("#moldoutFacility").val("").focus();
                            })
                        }
                    })
                }

                $$("#moldoutFacility").on('keydown', e => {
                    if (e.keyCode == 13) {
                        if ($$("#moldoutFacility").val() == "") {
                            return false;
                        }
                        scanFacility($$("#moldoutFacility").val())
                    }
                })
                $$("#btn_Moldouthistory").on("click", e => {
                    this.app.request.json('./GetMyMoldOutHistory', {}, function (res) {
                        if (res.code == 200) {
                            $$("#mymouldouthistory").empty().append(`
                        ${res.data.map(item =>
                                    `
                                <li>
                                    <a href="#" class="item-link item-content">
                                        <div class="item-inner">
                                            <div class="item-title-row">
                                                <div class="item-title">模具:${item.Name}(${item.Code})</div>
                                                <div class="item-after">${item.ToName}</div>
                                            </div>
                                            <div class="item-subtitle">${item.CreateDate.slice(5)}</div>
                                           
                                        </div>
                                    </a>
                                </li>
                            `
                                ).join('')}
                     `)
                        }
                    })
                })
                $$("#qrcode_moldoutBarcode").on("click", function (e) {
                    if ($$("#moldoutBarcode")[0].hasAttribute("readonly")) {
                        return false
                    }
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#moldoutBarcode").val(inputValue);
                                    scanMold(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })

                $$("#qrcode_moldoutFacility").on("click", function (e) {
                    if ($$("#moldoutFacility")[0].hasAttribute("readonly")) {
                        return false
                    }
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#moldoutFacility").val(inputValue);
                                    scanFacility(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })
            }
        }
    },
    {
        path: '/upmold/',
        template: `
                     <div class="page">
                         <div class="navbar">
                            <div class="navbar-inner sliding">
                                <div class="left">
                                    <a href="#" class="link back">
                                        <i class="icon icon-back"></i>
                                        <span class="ios-only">Back</span>
                                    </a>
                                </div>
                                <div class="title">上模</div>
                            </div>
                        </div>
                        <div class="page-content">
                           <div class="list">
                                <ul>
                                <li class="item-content item-input">
                                    <div class="item-media">设备</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="upmoldFacility" >
                                        <i id='qrcode_upmoldFacility' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                                        <span class="input-clear-button"></span>
                                        
                                    </div>
                                    </div>
                                </li>
                                <li class="item-content item-input">
                                    <div class="item-media">模具</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="upmoldBarcode" >
                                        <i id='qrcode_upmoldBarcode' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                                        <span class="input-clear-button"></span>
                                      
                                    </div>
                                    </div>
                                </li>
                                 <li class="item-content item-input">
                                    <div class="item-media">上模人员</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="upmoldEmployee" >
                                        <i id='qrcode_upmoldEmployee' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                                        <span class="input-clear-button"></span>
                                      
                                    </div>
                                    </div>
                                </li>
        
                         </ul>
                         <ul>
                             <li>
                                <div class="item-content item-input" >
                                   <div class="item-inner">
                                     <div class="item-title item-label">设备：</div>
                                        <div class="item-input-wrap">
                                            <input type="text" id="BindUpFacility" readonly placeholder="" value="" />
                                       </div>
                                    </div>
                               </div>
                            </li>
                            <li>
                                <div class="item-content item-input">
                                   <div class="item-inner">
                                     <div class="item-title item-label">模具：</div>
                                        <div class="item-input-wrap">
                                            <input type="text" id="BindUpMold" readonly placeholder="" value="" />
                                       </div>
                                    </div>
                               </div>
                            </li>
                            <li>
                                <div class="item-content item-input" /*style="display:none"*/>
                                   <div class="item-inner">
                                     <div class="item-title item-label">人员：</div>
                                        <div class="item-input-wrap">
                                            <input type="text" id="BindUpEmployee" readonly placeholder="" value="" />
                                       </div>
                                    </div>
                               </div>
                            </li>
                        </ul>
                            <input type="hidden" id="facilityId" />
                              <input type="hidden" id="moldId" />
                          <input type="hidden" id="upemployeeId" />
                            </div>
                        </div>
                      </div>
                `,
        on: {
            pageInit: function () {
                function upMold(moldId, facilityId, upemployeeId) {
                    this.app.request.json('./UpMold', { moldId, facilityId, upemployeeId }, res => {
                        if (res.code == 200) {
                            notification.open()
                            $$("#upmoldFacility").focus().val("");
                            $$("#upmoldBarcode").val("");
                            $$("#upmoldEmployee").val("");
                            app.views.current.router.back()
                            //moldView.router.navigate("/mold/", { animate: false });
                        } else {
                            app.dialog.alert('', res.msg)
                        }
                    })
                }
                var notification = app.notification.create({
                    title: '上模成功',
                    text: '',
                    closeTimeout: 3000,
                    on: {
                        opened: function () {

                        }
                    }
                })
                function CheckupFacility() {
                    var set = this;
                    this.app.request.json('./CheckUpFacility/', { barcode: $$("#upmoldFacility").val() }, function (res) {
                        if (res.code == 200) {
                            $$("#upmoldBarcode").val("").focus();
                            $$("#facilityId").val(res.data.Id)
                            set.app.request.json('./GetBindFacilityInfo', {
                                    facilityId: $$("#facilityId").val()
                                }, (res) => {
                                    if (res.code == 200) {
                                        $$("#BindUpFacility").val(res.data.FacilityName);
                                    }
                                })
                        } else {
                            app.dialog.alert('', res.msg, () => {3
                                $$("#upmoldFacility").val("").focus();
                            })
                        }
                    })
                }
                $$("#upmoldFacility").focus().on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#upmoldFacility").val() == "") {
                            return false
                        }
                        CheckupFacility($$("#upmoldFacility").val())
                        //var set = this;
                        //this.app.request.json('./CheckUpFacility', { barcode: $$("#upmoldFacility").val() }, function (res) {
                        //    if (res.code == 200) {
                        //        $$("#upmoldBarcode").removeAttr("readonly").val("").focus();
                        //        $$("#facilityId").val(res.data.Id)
                        //        set.app.request.json('./GetBindFacilityInfo', {
                        //            facilityId: $$("#facilityId").val()
                        //        }, (res) => {
                        //            if (res.code == 200) {
                        //                $$("#BindUpFacility").val(res.data.FacilityName);
                        //            }
                        //        })
                        //    } else {
                        //        app.dialog.alert('', res.msg, () => {
                        //            $$("#upmoldFacility").val("").focus()
                        //        })
                        //    }
                        //})
                    }
                })
                $$("#qrcode_upmoldFacility").on("click", function (e) {
                    if ($$("#upmoldFacility")[0].hasAttribute("readonly")) {
                        return false
                    }
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#upmoldFacility").val(inputValue);
                                    CheckupFacility(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })
                $$("#qrcode_upmoldBarcode").on("click", function (e) {
                    if ($$("#upmoldBarcode")[0].hasAttribute("readonly")) {
                        return false
                    }
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#upmoldBarcode").val(inputValue);
                                    CheckupMold(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })
                $$("#qrcode_upmoldEmployee").on("click", function (e) {
                    if ($$("#upmoldEmployee")[0].hasAttribute("readonly")) {
                        return false
                    }
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#upmoldEmployee").val(inputValue);
                                    CheckupEmployee(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })
                function CheckupMold() {
                    setTimeout(() => {
                        if ($$("#facilityId").val() == "") {
                            app.dialog.alert('', "请先扫描设备", () => {
                                $$("#upmoldFacility").val("").focus()
                            })
                        } else {
                            var set = this;
                            this.app.request.json('./CheckUpMold', { barcode: $$("#upmoldBarcode").val(), facilityId: $$("#facilityId").val() }, function (res) {
                                if (res.code == 200) {
                                    $$("#upmoldEmployee").val("").focus()
                                    $$("#moldId").val(res.data.Id)
                                    set.app.request.json('./GetBindMoldInfo', {
                                        moldId: $$("#moldId").val()
                                    }, (res) => {
                                        if (res.code == 200) {
                                            $$("#BindUpMold").val(res.data.Name);
                                        }
                                    })
                                    //app.dialog.prompt('', '请验证权限', function (name) {
                                    //    this.app.request.json('./checkUser', {
                                    //        user: name
                                    //    }, res => {
                                    //        if (res.code == 200) {
                                    //            upMold($$("#moldId").val(), $$("#facilityId").val())
                                    //        } else {
                                    //            app.dialog.alert('', '权限验证错误!');
                                    //            $$("#upmoldFacility").focus().val("");
                                    //            $$("#upmoldBarcode").val("");
                                    //        }
                                    //    })
                                    //});
                                } else {
                                    app.dialog.alert('', res.msg, () => {
                                        $$("#upmoldBarcode").val("").focus()
                                    })
                                }
                            })
                        }
                    })
                }
                $$("#upmoldBarcode").on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#upmoldBarcode").val() == "") {
                            return false
                        }
                        CheckupMold($$("#upmoldBarcode").val());
                    }
                })
                function CheckupEmployee() {
                    setTimeout(() => {
                        if ($$("#moldId").val() == "") {
                            app.dialog.alert('', "请先扫描模具", () => {
                                $$("#upmoldBarcode").val("")
                            })
                        } else {
                            var set = this;
                            this.app.request.json('./CheckUpEmployee', { barcode: $$("#upmoldEmployee").val(), moldId: $$("#moldId").val(), facilityId: $$("#facilityId").val() }, function (res) {
                                if (res.code == 200) {
                                    $$("#upemployeeId").val(res.data.Id)
                                    set.app.request.json('./GetBindUpEmployeeInfo', {
                                        upemployeeId: $$("#upemployeeId").val()
                                    }, (res) => {
                                        if (res.code == 200) {
                                            $$("#BindUpEmployee").val(res.data.Name);
                                            upMold($$("#moldId").val(), $$("#facilityId").val(), $$("#upemployeeId").val())
                                        }
                                    })
                                    //app.dialog.prompt('', '请验证权限', function (name) {
                                    //    this.app.request.json('./checkUser', {
                                    //        user: name
                                    //    }, res => {
                                    //        if (res.code == 200) {
                                    //            upMold($$("#moldId").val(), $$("#facilityId").val(),$$("#upemployeeId").val())
                                    //        } else {
                                    //            app.dialog.alert('', '权限验证错误!');
                                    //            $$("#upmoldFacility").focus().val("");
                                    //            $$("#upmoldBarcode").val("");
                                    //            $$("#upmoldEmployee").val("");
                                    //        }
                                    //    })
                                    //});
                                } else {
                                    app.dialog.alert('', res.msg, () => {
                                        $$("#upmoldEmployee").val("").focus()
                                    })
                                }
                            })
                        }
                    })
                }
                $$("#upmoldEmployee").on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#upmoldEmployee").val() == "") {
                            return false
                        }
                        CheckupEmployee($$("#upmoldEmployee").val())
                    }
                })
            }
        }
    },
    {
        path: '/downmold/',
        template: `
                     <div class="page">
                         <div class="navbar">
                            <div class="navbar-inner sliding">
                                <div class="left">
                                    <a href="#" class="link back">
                                        <i class="icon icon-back"></i>
                                        <span class="ios-only">Back</span>
                                    </a>
                                </div>
                                <div class="title">下模</div>
                            </div>
                        </div>
                        <div class="page-content">
                           <div class="list">
                                <ul>
                                <li class="item-content item-input">
                                    <div class="item-media">设备</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="downmoldFacility" >
                                        <i id='qrcode_downmoldFacility' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                                        <span class="input-clear-button"></span>
                                        
                                    </div>
                                    </div>
                                </li>
                                <li class="item-content item-input">
                                    <div class="item-media">模具</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="downmoldBarcode" >
                                        <i id='qrcode_downmoldBarcode' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                                        <span class="input-clear-button"></span>
                                      
                                    </div>
                                    </div>
                                </li>
                                <li class="item-content item-input">
                                    <div class="item-media">下模人员</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="downmoldEmployee" >
                                        <i id='qrcode_downmoldEmployee' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                                        <span class="input-clear-button"></span>
                                      
                                    </div>
                                    </div>
                                </li>
        
                                </ul>
                          <ul>
                             <li>
                                <div class="item-content item-input" >
                                   <div class="item-inner">
                                     <div class="item-title item-label">设备：</div>
                                        <div class="item-input-wrap">
                                            <input type="text" id="BindDownFacility" readonly placeholder="" value="" />
                                       </div>
                                    </div>
                               </div>
                            </li>
                            <li>
                                <div class="item-content item-input">
                                   <div class="item-inner">
                                     <div class="item-title item-label">模具：</div>
                                        <div class="item-input-wrap">
                                            <input type="text" id="BindDownMold" readonly placeholder="" value="" />
                                       </div>
                                    </div>
                               </div>
                            </li>
                            <li>
                                <div class="item-content item-input" /*style="display:none"*/>
                                   <div class="item-inner">
                                     <div class="item-title item-label">人员：</div>
                                        <div class="item-input-wrap">
                                            <input type="text" id="BindDownEmployee" readonly placeholder="" value="" />
                                       </div>
                                    </div>
                               </div>
                            </li>
                        </ul>
                            <input type="hidden" id="facilityId" />
                              <input type="hidden" id="moldId" />
                              <input type="hidden" id="downemployeeId" />
                            </div>
                        </div>
                      </div>
                `,
        on: {
            pageInit: function () {
                function downMold(moldId, facilityId, downemployeeId) {
                    this.app.request.json('./DownMold', { moldId, facilityId, downemployeeId }, res => {
                        if (res.code == 200) {
                            notification.open()
                            $$("#downmoldFacility").focus().val("");
                            $$("#downmoldBarcode").val("");
                            $$("#downmoldEmployee").val("");
                            app.views.current.router.back()
                            //moldView.router.navigate("/mold/", { animate: false });
                        } else {
                            app.dialog.alert('', res.msg)
                        }
                    })
                }
                var notification = app.notification.create({
                    title: '下模成功',
                    text: '',
                    closeTimeout: 3000,
                    on: {
                        opened: function () {

                        }
                    }
                })
                function CheckdownFacility() {
                    var set = this;
                    this.app.request.json('./CheckDownFacility', { barcode: $$("#downmoldFacility").val() }, function (res) {
                        if (res.code == 200) {
                            $$("#downmoldBarcode").val("").focus();
                            $$("#facilityId").val(res.data.Id)
                            set.app.request.json('./GetBindFacilityInfo', {
                                facilityId: $$("#facilityId").val()
                            }, (res) => {
                                if (res.code == 200) {
                                    $$("#BindDownFacility").val(res.data.FacilityName);

                                }
                            })
                        } else {
                            app.dialog.alert('', res.msg, () => {
                                $$("#downmoldFacility").val("").focus()
                            })
                        }
                    })
                }
                $$("#downmoldFacility").focus().on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#downmoldFacility").val() == "") {
                            return false
                        }
                        CheckdownFacility($$("#downmoldFacility").val())
                    }
                })
                
                $$("#qrcode_downmoldFacility").on("click", function (e) {
                    if ($$("#downmoldFacility")[0].hasAttribute("readonly")) {
                        return false
                    }
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#downmoldFacility").val(inputValue);
                                    CheckdownFacility(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })
                $$("#qrcode_downmoldBarcode").on("click", function (e) {
                    if ($$("#downmoldBarcode")[0].hasAttribute("readonly")) {
                        return false
                    }
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#downmoldBarcode").val(inputValue);
                                    CheckdownMold(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })
                $$("#qrcode_downmoldEmployee").on("click", function (e) {
                    if ($$("#downmoldEmployee")[0].hasAttribute("readonly")) {
                        return false
                    }
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#downmoldEmployee").val(inputValue);
                                    CheckdownEmployee(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })
                function CheckdownMold() {
                    setTimeout(() => {
                        if ($$("#facilityId").val() == "") {
                            app.dialog.alert('', "请先扫描设备", () => {
                                $$("#downmoldFacility").val("").focus()
                            })
                        } else {
                            var set = this;
                            this.app.request.json('./CheckDownMold', { barcode: $$("#downmoldBarcode").val(), facilityId: $$("#facilityId").val() }, function (res) {
                                if (res.code == 200) {
                                    $$("#downmoldEmployee").val("").focus()
                                    $$("#moldId").val(res.data.Id)
                                    set.app.request.json('./GetBindMoldInfo', {
                                        moldId: $$("#moldId").val()
                                    }, (res) => {
                                        if (res.code == 200) {
                                            $$("#BindDownMold").val(res.data.Name);
                                        }
                                    })
                                    //app.dialog.prompt('', '请验证权限', function (name) {
                                    //    this.app.request.json('./checkUser', {
                                    //        user: name
                                    //    }, res => {
                                    //        if (res.code == 200) {
                                    //            upMold($$("#moldId").val(), $$("#facilityId").val())
                                    //        } else {
                                    //            app.dialog.alert('', '权限验证错误!');
                                    //            $$("#upmoldFacility").focus().val("");
                                    //            $$("#upmoldBarcode").val("");
                                    //        }
                                    //    })
                                    //});
                                } else {
                                    app.dialog.alert('', res.msg, () => {
                                        $$("#downmoldBarcode").val("").focus()
                                    })
                                }
                            })
                        }
                    })
                }
                $$("#downmoldBarcode").on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#downmoldBarcode").val() == "") {
                            return false
                        }
                        CheckdownMold($$("#downmoldBarcode").val())
                    }
                })
                function CheckdownEmployee() {
                    setTimeout(() => {
                        if ($$("#moldId").val() == "") {
                            app.dialog.alert('', "请先扫描模具", () => {
                                $$("#downmoldBarcode").val("")
                            })
                        } else {
                            var set = this;
                            this.app.request.json('./CheckDownEmployee', { barcode: $$("#downmoldEmployee").val(), moldId: $$("#moldId").val(), facilityId: $$("#facilityId").val() }, function (res) {
                                if (res.code == 200) {
                                    $$("#downemployeeId").val(res.data.Id)
                                    set.app.request.json('./GetBindDownEmployeeInfo', {
                                        downemployeeId: $$("#downemployeeId").val()
                                    }, (res) => {
                                        if (res.code == 200) {
                                            $$("#BindDownEmployee").val(res.data.Name);
                                            downMold($$("#moldId").val(), $$("#facilityId").val(), $$("#downemployeeId").val())
                                        }
                                    })
                                    //app.dialog.prompt('', '请验证权限', function (name) {
                                    //    this.app.request.json('./checkUser', {
                                    //        user: name
                                    //    }, res => {
                                    //        if (res.code == 200) {
                                    //            downMold($$("#moldId").val(), $$("#facilityId").val(), $$("#downemployeeId").val())
                                    //        } else {
                                    //            app.dialog.alert('', '权限验证错误!');
                                    //            $$("#downmoldFacility").focus().val("");
                                    //            $$("#downmoldBarcode").val("");
                                    //            $$("#downmoldEmployee").val("");
                                    //        }
                                    //    })
                                    //});
                                } else {
                                    app.dialog.alert('', res.msg, () => {
                                        $$("#downmoldEmployee").val("").focus()
                                    })
                                }
                            })
                        }
                    })
                }
                $$("#downmoldEmployee").on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#downmoldEmployee").val() == "") {
                            return false
                        }
                        CheckdownEmployee($$("#downmoldEmployee").val())
                    }
                })
            }
        }
    },
    {
        path: '/logistics/',
        url: './logistics/',
    },
    {
        path: '/upmat/',
        template: `
                     <div class="page">
                         <div class="navbar">
                            <div class="navbar-inner sliding">
                                <div class="left">
                                    <a href="#" class="link back">
                                        <i class="icon icon-back"></i>
                                        <span class="ios-only">Back</span>
                                    </a>
                                </div>
                                <div class="title">上料</div>
                            </div>
                        </div>
                        <div class="page-content">
                           <div class="list">
                                <ul>
                                <li class="item-content item-input">
                                    <div class="item-media">机台</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="upmatFacility" >
                                        <span class="input-clear-button"></span>
                                        
                                    </div>
                                    </div>
                                </li>
                                <li class="item-content item-input">
                                    <div class="item-media">物料</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="upmatBarcode" >
                                        <span class="input-clear-button"></span>
                                      
                                    </div>
                                    </div>
                                </li>
              
        
                                </ul>
                            <input type="hidden" id="sfacilityId" />
                              <input type="hidden" id="matId" />
                            </div>
                        </div>
                      </div>
                `,
        on: {
            pageInit: function () {
                function upMat(matId, facilityId) {
                    this.app.request.json('./UpMat', { matId, facilityId }, res => {
                        if (res.code == 200) {
                            notification.open()
                            $$("#upmatFacility").focus().val("");
                            $$("#upmatBarcode").val("");
                        } else {
                            app.dialog.alert('', res.msg)
                        }
                    })
                }
                var notification = app.notification.create({
                    title: '上料成功',
                    text: '',
                    closeTimeout: 3000,
                    on: {
                        opened: function () {

                        }
                    }
                })
                $$("#upmatFacility").focus().on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#upmatFacility").val() == "") {
                            return false
                        }
                        this.app.request.json('./CheckUpFacility', { barcode: $$("#upmatFacility").val() }, function (res) {
                            if (res.code == 200) {
                                $$("#upmatBarcode").val("").focus();
                                $$("#sfacilityId").val(res.data.Id)
                            } else {
                                app.dialog.alert('', res.msg, () => {
                                    $$("#upmatFacility").val("").focus()
                                })
                            }
                        })
                    }
                })
                $$("#upmatBarcode").on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#upmatBarcode").val() == "") {
                            return false
                        }
                        setTimeout(() => {
                            if ($$("#sfacilityId").val() == "") {
                                app.dialog.alert('', "请先扫描机台", () => {
                                    $$("#upmatFacility").val("").focus()
                                })
                            } else {
                                this.app.request.json('./CheckUpMat', { barcode: $$("#upmatBarcode").val(), facilityId: $$("#sfacilityId").val() }, function (res) {
                                    if (res.code == 200) {
                                        $$("#matId").val(res.data[0].MaterialID)
                                        let po = app.dialog.prompt('', '请验证权限', function (name) {
                                            this.app.request.json('./checkUser', {
                                                user: name
                                            }, res => {
                                                console.log(po)
                                                if (res.code == 200) {
                                                    upMat($$("#matId").val(), $$("#sfacilityId").val())
                                                } else {
                                                    console.log(po)
                                                    app.dialog.alert('', '权限验证错误!', () => { return false }, () => { return false });
                                                }
                                            })
                                        });
                                    } else {
                                        app.dialog.alert('', res.msg, () => {
                                            $$("#upmatBarcode").val("").focus()
                                        })
                                    }
                                })
                            }
                        })
                    }
                })
            }
        }
    },
    {
        path: '/needmat/',
        template: `
                     <div class="page">
                         <div class="navbar">
                            <div class="navbar-inner sliding">
                                <div class="left">
                                    <a href="#" class="link back">
                                        <i class="icon icon-back"></i>
                                        <span class="ios-only">Back</span>
                                    </a>
                                </div>
                                <div class="title">领料</div>
                            </div>
                        </div>
                        <div class="page-content">
                           <div class="list">
                                <ul>
                                <li class="item-content item-input">
                                    <div class="item-media">任务单</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="billno" >
                                        <span class="input-clear-button"></span>
                                    </div>
                                    </div>
                                </li>
                                <li class="item-content item-input">
                                    <div class="item-media">货架</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="warearea" >
                                        <span class="input-clear-button"></span>
                                    </div>
                                    </div>
                                </li>
                                 <li class="item-content item-input">
                                    <div class="item-media">条码</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="hbarcode" >
                                        <span class="input-clear-button"></span>
                                    </div>
                                    </div>
                                </li>
        
                                </ul>
                            <input type="hidden" id="" />
                              <input type="hidden" id="" />
                            </div>
                        </div>
                      </div>
                `,
        on: {
            pageInit: function () {
                function upMat(matId, facilityId) {
                    this.app.request.json('./UpMat', { matId, facilityId }, res => {
                        if (res.code == 200) {
                            notification.open()
                            $$("#upmatFacility").focus().val("");
                            $$("#upmatBarcode").val("");
                        } else {
                            app.dialog.alert('', res.msg)
                        }
                    })
                }

                function CheckNeedBill() {
                    this.app.request.json('./CheckNeedBill', { barcode: $$("#billno").val() }, function (res) {
                        if (res.code == 200) {
                            $$("#upmatBarcode").val("").focus();
                            $$("#sfacilityId").val(res.data.Id)
                        } else {
                            app.dialog.alert('', res.msg, () => {
                                $$("#upmatFacility").val("").focus()
                            })
                        }
                    })
                }
                var notification = app.notification.create({
                    title: '领料成功',
                    text: '',
                    closeTimeout: 3000,
                    on: {
                        opened: function () {

                        }
                    }
                })
                $$("#billno").focus().on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#upmatFacility").val() == "") {
                            return false
                        }
                        
                    }
                })
                $$("#upmatBarcode").on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#upmatBarcode").val() == "") {
                            return false
                        }
                        setTimeout(() => {
                            if ($$("#sfacilityId").val() == "") {
                                app.dialog.alert('', "请先扫描机台", () => {
                                    $$("#upmatFacility").val("").focus()
                                })
                            } else {
                                this.app.request.json('./CheckUpMat', { barcode: $$("#upmatBarcode").val(), facilityId: $$("#sfacilityId").val() }, function (res) {
                                    if (res.code == 200) {
                                        $$("#matId").val(res.data[0].MaterialID)
                                        let po = app.dialog.prompt('', '请验证权限', function (name) {
                                            this.app.request.json('./checkUser', {
                                                user: name
                                            }, res => {
                                                console.log(po)
                                                if (res.code == 200) {
                                                    upMat($$("#matId").val(), $$("#sfacilityId").val())
                                                } else {
                                                    console.log(po)
                                                    app.dialog.alert('', '权限验证错误!', () => { return false }, () => { return false });
                                                }
                                            })
                                        });
                                    } else {
                                        app.dialog.alert('', res.msg, () => {
                                            $$("#upmatBarcode").val("").focus()
                                        })
                                    }
                                })
                            }
                        })
                    }
                })
            }
        }
    },
    {
        path: '/moldSelect/',
        content: `
          <div class="page">
             <div class="navbar">
                <div class="navbar-inner sliding">
                    <div class="left">
                        <a href="#" class="link back">
                            <i class="icon icon-back"></i>
                            <span class="ios-only">Back</span>
                        </a>
                    </div>
                    <div class="title">模具查询</div>
                <div class="right">
               <div class="right">
                <a href="/moldSelect/" data-animate="false" data-reload-current="true" class="link not-animated"><i class="f7-icons">reload_round</i></a>
                </div>
                </div>
                    </div>
                </div>
            <div class="page-content">
                <div class="tabs">
                    <div class="tab tab-active" id="tabmoldSelect">
                        <div class="block" id="">
                           <div class="list">
                                <ul>
                                <li class="item-content item-input">
                                    <div class="item-media">模具编码</div>
                                    <div class="item-inner">
                                    <div class="item-input-wrap">
                                        <input type="text" name="name" id="moldSelectBarcode" >
                                        <i id='qrcode_moldSelectBarcode' style='position:absolute;top:0;right:0;font-size:35px' class="f7-icons">qrcode</i>
                                    </div>
                                    </div>
                                </li>
                                </ul>
                            </div>
                        </div>
                    </div>
              <div class="list media-list ">
                    <ul id="moldSelectlist">
                                
                    </ul>
                </div>
            </div>
          </div>
        `,
        on: {
            pageAfterIn: function () {
                this.app.request.json('./getMoldSelectBill/', { barcode: 0 } , function (res) {
                    //{ barcode: $$("#downmoldBarcode").val() }

                    $$("#moldSelectlist").empty().append(`
                        ${res.data.data.map(item => {
                        return `
                                <li style="border-bottom:1px solid #e4eff4">
                                        <div class="item-inner">
                                            <div class="item-subtitle">模具：${item.Name}</div>
                                            <div class="item-text ">
                                                <ul class="">
                                                    <li>
                                                        仓库：${item.WareHouseName}
                                                    </li>
                                                    <li>
                                                        库位：${item.WareHouseName == "模具房" ? item.Code : item.WareAreaName}
                                                    </li>
                                                </ul>
                                            </div>
                                        </div>
                                </li>
                            `
                    }).join('')}
                     `)
                })
                $$("#moldSelectBarcode").on("keydown", (e) => {
                    if (e.keyCode == 13) {
                        if ($$("#moldSelectBarcode").val() == "") {
                            return false
                        }
                        setTimeout(() => {
                            this.app.request.json('./getMoldSelectBill/', { barcode: $$("#moldSelectBarcode").val() }, function (res) {
                                if (res.code == 200) {
                                    $$("#moldSelectlist").empty().append(`
                                        ${res.data.data.map(item => {
                                            return `
                                                <li style="border-bottom:1px solid #e4eff4">
                                                        <div class="item-inner">
                                                            <div class="item-subtitle">模具：${item.Name}</div>
                                                            <div class="item-text ">
                                                                <ul class="">
                                                                    <li>
                                                                        仓库：${item.WareHouseName}
                                                                    </li>
                                                                    <li>
                                                                        库位：${item.WareHouseName == "模具房" ? item.Code : item.WareAreaName}
                                                                    </li>
                                                                </ul>
                                                            </div>
                                                        </div>
                                                </li>
                                            `
                                        }).join('')}
                                     `)
                                } else {
                                    app.dialog.alert('', res.msg, () => {
                                        $$("#moldSelectBarcode").val("").focus()
                                    })
                                }
                            })
                        })
                    }
                })
                $$("#qrcode_moldSelectBarcode").on("click", function (e) {
                    if ($$("#moldSelectBarcode")[0].hasAttribute("readonly")) {
                        return false
                    }
                    if ('_cordovaNative' in window) {
                        cordova.plugins.barcodeScanner.scan(
                            function (result) {
                                if (!result.cancelled) {
                                    var inputValue = result.text;
                                    $$("#moldSelectBarcode").val(inputValue);
                                    scanMold(inputValue)
                                }
                            },
                            function (error) {
                                app.dialog.alert(error, "扫描失败", function () {
                                });
                            }
                        );
                    }
                })
            }
        }
    },
    {
        path: '(.*)',
        url: '../error',
    },
    
];
