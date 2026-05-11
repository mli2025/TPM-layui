(function($) {
	$.jsontotable = function(data, options) {
		var settings = $.extend({
			id: null, // target element id
			header: true,
			className: null
		}, options);

		options = $.extend(settings, options);

		var obj = data;
		if (typeof obj === "string") {
			obj = $.parseJSON(obj);
		}

		if (options.id && obj.length) {

			var i, row;
			var table = $("<table></table>");

			if (options.className) {
				table.addClass(options.className);
			}

			$.fn.appendTr = function(rowData, isHeader) {
				var frameTag = isHeader ? "thead" : "tbody";
				var rowTag = isHeader ? "th" : "td";
				var rowi,key,cellObj,cell,j;

				/* if rowData is object, set the key and value as tr's properties */
				if ($.isPlainObject(rowData) && rowData._data) {
					row = '<tr';

					for (rowi in rowData) {
						if (rowi !== '_data') {
							row += ' ' + rowi + '="' + rowData[rowi] + '"';
						}
					}
					row += '></tr>';
					rowData = rowData._data;

				} else {
					row = "<tr></tr>";
				}

				row = $(row);

				for (key in rowData) {
					cellObj = rowData[key];

					if (typeof cellObj !== "function") { /* ADDED: this wrapper to account for people bootstrapping the ECMA Array model otherwise functions get converted to strings and show up in the object list / output */

						cell = '';

						/* if cellObj is object, set the key and value as cell's properties */
						if ($.isPlainObject(cellObj) && cellObj._data) {
							cell = "<" + rowTag;

							for (j in cellObj) {
								if (j !== '_data') {
									cell += ' ' + j + '="' + cellObj[j] + '"';
								}
							}

							cellObj = cellObj._data;

							cell += '>' + cellObj + "</" + rowTag + ">";

						} else {
							cell = "<" + rowTag + ">" + cellObj + "</" + rowTag + ">";
						}

						row.append(cell);
					}
				}

				if (isHeader) { /* ADDED: IF/ELSE to eliminate repetitive TBODY tags for every row */
					$(this).append($("<" + frameTag + "></" + frameTag + ">").append(row));

				} else {
					var tbody = $(this).find("tbody");
					if (tbody.length === 0) {
						tbody = $(this).append("<tbody></tbody>");
					}

					tbody.append(row); //always append data rows to the first tbody tag
				}

				return this;
			};

			if (options.header) {
				table.appendTr(obj[0], true);
			}

			for (i = options.header ? 1 : 0; i < obj.length; i++) { /* MODIFIED: options.header ? 1 : 0 --- to eliminate duplicating header as the first row of data */
				table.appendTr(obj[i], false, i);
			}

			$(options.id).append(table);
		}
	    // Builds the HTML Table out of myList.
		function buildHtmlTable(selector) {
		    var columns = addAllColumnHeaders(myList, selector);

		    for (var i = 0; i < myList.length; i++) {
		        var row$ = $('<tr/>');
		        for (var colIndex = 0; colIndex < columns.length; colIndex++) {
		            var cellValue = myList[i][columns[colIndex]];
		            if (cellValue == null) cellValue = "";
		            row$.append($('<td/>').html(cellValue));
		        }
		        $(selector).append(row$);
		    }
		}

	    // Adds a header row to the table and returns the set of columns.
	    // Need to do union of keys from all records as some records may not contain
	    // all records.
		function addAllColumnHeaders(myList, selector) {
		    var columnSet = [];
		    var headerTr$ = $('<tr/>');

		    for (var i = 0; i < myList.length; i++) {
		        var rowHash = myList[i];
		        for (var key in rowHash) {
		            if ($.inArray(key, columnSet) == -1) {
		                columnSet.push(key);
		                headerTr$.append($('<th/>').html(key));
		            }
		        }
		    }
		    $(selector).append(headerTr$);

		    return columnSet;
		}
		return this;
	};
}(jQuery));
jQuery.download = function (url, data, method) {    // 获得url和data
    if (url && data) {
        // data 是 string 或者 array/object
        data = typeof data == 'string' ? data : jQuery.param(data);        // 把参数组装成 form的  input
        var inputs = '';
        jQuery.each(data.split('&'), function () {
            var pair = this.split('=');
            inputs += '<input type="hidden" name="' + decodeURI(pair[0]) + '" value="' + pair[1] + '" />';
        });        // request发送请求
        jQuery('<form action="' + url + '" method="' + (method || 'post') + '">' + inputs + '</form>')
        .appendTo('body').submit().remove();
    };
};
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = decodeURI(window.location.search).substr(1).match(reg);
    if (r != null) return unescape(r[2]); return "";
}
function PrintPDF(pdfUrl) {
    if (navigator.userAgent.indexOf("Pody") > -1) {
        //$.download(res.data, "", "");
        $.download(pdfUrl, { name: "" }, "GET")
        //window.open(res.data);
    }
    else {
        window.open(pdfUrl);
    }
}