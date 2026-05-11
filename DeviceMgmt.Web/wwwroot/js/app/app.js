// <reference path="../../lib/cordova/cordova.js" />
// <reference path="../../lib/cordova/cordova.js" />
// Dom7
var $$ = Dom7;
// Framework7 App main instance
var app = new Framework7({
    root: '#app', // App root element
    id: 'io.framework7.testapp', // App bundle ID
    name: 'Framework7', // App name
    theme: 'md', // Automatic theme detection
    dialog: {
        // change default "OK" button text
        buttonOk: '确定',
        buttonCancel:"返回"
    },
    // App root data
    data: function () {
        return {
            user: {
                firstName: 'John',
                lastName: 'Doe',
            },
            // Demo products for Catalog section
            products: [
                {
                    id: '1',
                    title: 'OEE',
                    description: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit. Nisi tempora similique reiciendis, error nesciunt vero, blanditiis pariatur dolor, minima sed sapiente rerum, dolorem corrupti hic modi praesentium unde saepe perspiciatis.'
                },
                {
                    id: '2',
                    title: 'IPPM',
                    description: 'Velit odit autem modi saepe ratione totam minus, aperiam, labore quia provident temporibus quasi est ut aliquid blanditiis beatae suscipit odio vel! Nostrum porro sunt sint eveniet maiores, dolorem itaque!'
                },
                {
                    id: '3',
                    title: 'FTQ',
                    description: 'Expedita sequi perferendis quod illum pariatur aliquam, alias laboriosam! Vero blanditiis placeat, mollitia necessitatibus reprehenderit. Labore dolores amet quos, accusamus earum asperiores officiis assumenda optio architecto quia neque, quae eum.'
                },
            ]
        };
    },
    // App root methods
    methods: {
        scanResource: function () {
            

        }
    },
    // App routes
    routes: routes,
});

// Init/Create views
var homeView = app.views.create('#view-home', {
    url: '/'
});
var eventView = app.views.create('#view-event', {
    url: '/EventList/'
});
var catalogView = app.views.create('#view-catalog', {
    url: '/catalog/'
});
//var traceView = app.views.create('#view-trace', {
//    url: '/trace/'
//});
var settingsView = app.views.create('#view-settings', {
    url: '/settings/' 
});


var repairView = app.views.create('#view-repair', {
    url: '/repair/'
});

var moldmaintainView  = app.views.create('#view-moldmantain', {
    url: '/moldMaintain/'
});
var facilitymaintainView = app.views.create('#view-facilitymaintain', {
    url: '/facilityMaintain/'
});
var moldView = app.views.create('#view-mold', {
    url: '/mold/'
});

//var logisticsView = app.views.create('#view-logistics', {
//    url: '/logistics/'
//});


$(function () {
    //loadScript("../lib/Cordova/cordova.js");
    if ('_cordovaNative' in window) {
        loadScript("../lib/Cordova/cordova.js");
    }
    $("body").on("click", "#scanResourceHome", function () {
        
        if ('_cordovaNative' in window) { // in cordova environment
            
            cordova.plugins.barcodeScanner.scan(
                function (result) {
                    if (!result.cancelled) {
                        $.get('./CheckFacility?code=' + result.text, function (res) {
                            if (res.code == 200) {
                                layui.data('Facility', {
                                    key: 'Id'
                                    , value: res.data
                                });
                                //$("#faName").html(res.data.Name)
                                app.views.main.router.navigate("/FacilityInfo/", { animate: false });
                                LoadClosedEvent(res.data.Id);
                                //app.views.main.router.navigate("/", { animate: false });
                            }
                            else {
                                app.dialog.alert(res.msg, "设备信息获取失败", function () {
                                });
                                //layui.layer.msg(res.msg);
                            }
                        })
                    }
                },
                function (error) {
                    app.dialog.alert(error, "扫描失败", function () {
                    });
                }
            );
        }
        else {
            app.dialog.alert("您的手持设备不支持扫描功能，请手动选择设备。", "应用环境异常", function () {
                app.views.main.router.navigate("/listResource/", { animate: true });
            });
        }
    });
})


// Login Screen Demo
$$('#my-login-screen .login-button').on('click', function () {
    var username = $$('#my-login-screen [name="username"]').val();
    var password = $$('#my-login-screen [name="password"]').val();

    // Close login screen
    app.loginScreen.close('#my-login-screen');

    // Alert username and password
    app.dialog.alert('Username: ' + username + '<br>Password: ' + password);
});
function loadScript(url, callback) {
    var script = document.createElement("script")
    script.type = "text/javascript";
    if (script.readyState) { //IE
        script.onreadystatechange = function () {
            if (script.readyState == "loaded" || script.readyState == "complete") {
                script.onreadystatechange = null;
                callback();
            }
        };
    } else { //Others
        script.onload = function () {
            callback();
        };
    }
    script.src = url;
    document.getElementsByTagName("head")[0].appendChild(script);
}
function LoadClosedEvent(facilityId) {
    app.request.get('./GetClosedEventList', { FacilityId: facilityId, page: 1, limit: 20 }, function (res) {
        maxItems = res.count;
        layui.use('util', function () {
            var util = layui.util;
            res = JSON.parse(res);
            var list = "";
            for (var i = 0; i < res.data.length; i++) {
                let item = res.data[i];
                list += '<li>';
                list += '    <a href="/EventDetailReadonly/' + item.Id + '/" class="item-link item-content">';
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
                list = '<li style="text-align:center"><a href="" class="item-link item-content"><div class="item-inner">这里没有事件</div></a></li>';
            }
            $("#EventHistory").html(list);
        });

    });
}
function chooseResource(resoureId) {
    //app.dialog.alert('Hello World! ' + resoureId);
    $.get('./CheckFacility?id=' + resoureId, function (res) {
        if (res.code == 200) {
            layui.data('Facility', {
                key: 'Id'
                , value: (res.data)
            });
            $("#faName").html(res.data.FacilityName);
            app.views.main.router.navigate("/FacilityInfo/", { animate: false });
            LoadClosedEvent(res.data.Id);
            
            //$("#reloadClosedEvent").trigger("click");
            //app.router.navigate("/", {
            //    reloadCurrent: true,
            //    ignoreCache: true,
            //});
            //fnUrlReplace('@Url.Action("Index")')
            //window.location.href = '@Url.Action("Index")';
        }
        else {
            layui.layer.msg(res.msg);
        }
    });
}

function chooseEvent(eventId) {
    console.log(route.params)
}

function DateAndTIme(dateTime = new Date()) {
    let timeArr = [];
    let localT = new Date(dateTime).getTime() + 8 * 3600 * 1000;
    let timeDate = new Date(localT).toISOString().substr(5, 5);
    timeArr.push(timeDate);
    timeArr.push(new Date(dateTime).toTimeString().substr(0, 8));
    return timeArr;
}
function occurrencesTimeline(eDetail, eStatus, startTime, endTime) {
    const status = eStatus === 100 ? "已结束" : "进行中";
    let timeStart = DateAndTIme(startTime);
    let timeEnd, endShow = '', statusStyle = 'style="color:#ff1407"';
    if(eStatus === 100) {
        statusStyle = 'style="color:#8e8e93"';
        timeEnd = DateAndTIme(endTime);
        endShow = '<p><small>结束时间:' + timeEnd[0] + ' ' + timeEnd[1] + '</small></p>'
    }
    let tempItem = `
        <div class="timeline-item">
        <div class="timeline-item-date">${timeStart[0]} <small>${timeStart[1]}</small></div>
        <div class="timeline-item-divider"></div>
        <div class="timeline-item-content">
            <div class="timeline-item-inner">
                <p>
                    事件：<strong>${eDetail}</strong>
                </p>
                <p>
                    <small> 状态：<span ${statusStyle}>${status}</span></small>
                </p>
                ${endShow}
            </div>
        </div>
    </div>
    `;
    return tempItem;
}

$("#view-fevent").on("click","#status-bar", function() {
    var fdata = layui.data('Facility', { key: 'Id' });
    app.request.get('./progressTime', { id: fdata.Id }, function (res) {
        let timeLineList = document.querySelector("#progressTL");
        let occurrences = JSON.parse(res).data;
        let listTextNode = '';
        for(let i =0; i < occurrences.length; i++ ){
            let li = occurrencesTimeline(occurrences[i]['事件'], occurrences[i]['@Localizer["col_Status"]'], occurrences[i]['上报时间'], occurrences[i]['结束时间']);
            listTextNode += li;
        }
        timeLineList.innerHTML = listTextNode;
    });
    app.popup.open('.popup-progress');
})

  $$('.close-progress-view').on('click', function() {
      app.popup.close('.popup-progress');
  })



