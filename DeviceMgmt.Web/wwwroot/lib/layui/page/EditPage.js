function GetLayoutJsonString() {
    var jishu = 0;
    var idArr = '';
    $('.box').each(function () {
        var top = $(this).position().top;
        var left = $(this).position().left;
        var width = $(this).width();
        var height = $(this).height();
        //var content = $(this).children('p').html();
        var name = $(this).children('h3').html();
        var sql = $(this).children('input[name=sql]').val();
        var id = $(this).children('input[name=id]').val();
        var content = $(this).children('input[name=content]').val();
        idArr += '{"HInterID"' + ':' + id + ',' +
            '"HTop"' + ':' + top + ',' +
            '"HLeft"' + ':' + left + ',' +
            '"HWidth"' + ':' + width + ',' +
            '"HHeight"' + ':' + height + ',' +
            '"HContent"' + ':' + content + ',' +
            '"HLayoutID"' + ':' + GetQueryString("id") + ',' +
            '"HName"' + ':"' + name + '"},';

        jishu = jishu + 1;
    })
    idArr = "[" + idArr.substr(0, idArr.length - 1) + "]";
    return idArr;
}
function GetBoxTitle(type) {
    var boxtitle = "";
    switch (type*1) {
        case 0:
            boxtitle = "Label";
            break;
        case 1:
            boxtitle = "Table";
            break;
        case 2:
            boxtitle = "Image";
            break;
        case 3:
            boxtitle = "Line Chart";
            break;
        case 4:
            boxtitle = "Text";
            break;
        case 5:
            boxtitle = "Pie Chart";
            break;
        case 6:
            boxtitle = "Gauge Chart";
            break;
        case 7:
            boxtitle = "Progress";
            break;
        case 8:
            boxtitle = "Progress";
            break;
        default:
            boxtitle = "";
            break;
    }
    return boxtitle;
}
