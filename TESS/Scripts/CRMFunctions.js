/**
 * 
 * This function will bind the jinplace jQuery plugin to every td.
 * jinplace is used to enable inline editing for table cells
 * 
 * @param [jQuery Object]       $td         Table cell to bind to
 * @param [string]              value       Table cell content value
 * @param [JavaScript Object]   dataObj     Object containing usefull meta information, used to update cell
 * @param [string]              targetUrl   Target url for ajax update request
 *
 * @return Nothing
 **/

var rows = [];

var addJInPlaceListener = function ($td, value, dataObj, targetUrl, deleteRowFunction) {

    $td.not('.crm-delete').attr("data-url", targetUrl)
            .attr("data-object", JSON.stringify(dataObj))
            .jinplace({
                "placeholder": "",
                "submitFunction": function (opts, value, self, editor) {
                    if (self.origValue != value || (existInRows(dataObj.type, dataObj.primaryKey) && dataObj.value == value))
                        addToRow(dataObj.primaryKey, value, dataObj.type);
                    self.onUpdate(editor, opts, value);
                    deleteRowFunction($td, dataObj); // Delete button fix
                    return null;
                }
            })
           .unbind('jinplace:done').unbind('jinplace:fail')
           .on('jinplace:done', function (ev, data, textStatus, jqxhr, opts) {

               console.log(opts);
               console.log('Successful edit!');
               $(document).trigger("clear-alerts");
               $(document).trigger("add-alerts", [
                 {
                     'message': 'Successful edit!',
                     'priority': 'success'
                 }
               ]);

           })
            .on('jinplace:fail', function (ev, jqxhr, textStatus, errorThrown, opts, data) {

                console.log('Failed to edit! ' + opts.attribute + ' has not been updated');
                var errorMsg = "unkown error. Try to update the website";
                if (data == 0) {
                    errorMsg = "invalid format.";
                }
                if (data == -2) {
                    errorMsg = "You can't edit this field because it is read-only!";
                }
                console.log('Failed with error:  ' + errorMsg);
                $(document).trigger("clear-alerts");
                $(document).trigger("add-alerts", [
                  {
                      'message': 'Failed to edit! Error: ' + errorMsg,
                      'priority': 'warning'
                  }
                ]);
            })
            .children().on("click", function (e) {
                return false;
            })
            
    

}
/**
 * 
 * This function validates if a key is already inserted
 * 
 * @param [string]   key         THe key that holds a specific value
 * @param [string]   primaryKey  The key that holds a unique value to seperate all the rows
 *
 * @return [bool] true if key found, else false
 **/
var existInRows = function(key, primaryKey)
{
    for (var i = 0; i < rows.length; i++) {
        if (rows[i]["primaryKey"] === primaryKey) {
            if (typeof rows[i][key] != "undefined")
                return true;
        }
    }
    return false;
}

/**
    Updates the the rows array to server and can do different things on success and fail depending on arguments
**/
var updateToServer = function(url, beforeSend, onSuccess)
{
    console.log(JSON.stringify(rows));
    getInsertValues();
    console.log(JSON.stringify(rows));
    if (rows.length > 0) {
        $.ajax(url, {
            type: "post",
            data: {
                "object": JSON.stringify(rows)
            },
            timeout: 6000,
            //data: {value: ""},
            dataType: 'text',
            // iOS 6 has a dreadful bug where POST requests are not sent to the
            // server if they are in the cache.
            headers: { 'Cache-Control': 'no-cache' }, // Apple!
            beforeSend: function()
            {
                if(typeof beforeSend != "undefined")
                    beforeSend();
            },
            success: function (data, textStatus, jqxhr) {
                if (typeof onSuccess != "undefined")
                    onSuccess(data, textStatus, jqxhr, rows);

                if (data > 0) { // Everything went well, update edited field and trigger jinplace:successs
                    rows = [];
                    //$td.trigger('jinplace:done', [null, textStatus, jqxhr, null]);
                }
                else {
                    //$td.trigger('jinplace:fail', [jqxhr, textStatus, null, null, data]);
                }

            },
        });
        return true;
    }
    else
        return false;
}

var getInsertValues = function()
{
    for(var i = 0; i < rows.length; i++)
    {
        if(typeof rows[i].insert !== "undefined")
        {
            var insert = rows[i].insert;
            $tr = $("#new-row-" + insert);
            $tds = $tr.find("td");
            //remove(rows, rows[i]);

            for(var j = 0; j < $tds.length; j++)
            {
                var $td = $($tds[j]);
                var $input = $td.find("input");
                var prop = $td.data("property");
                addToRow(null, $input.val(), prop, insert);
            }
        }
    }
}

var remove = function(arr, item) {
    for (var i = arr.length; i--;) {
        if (arr[i] === item) {
            arr.splice(i, 1);
        }
    }
}

var deleteRow = function (primaryKey, type, url, onSuccess, $row) {
    bootbox.confirm({
        title: 'Do you want to delete?',
        message: "Are you sure you want to delete row with " + type.replace("_", " ") + ": " + primaryKey + "?",
        buttons: {
            'cancel': {
                label: 'Cancel',
                className: 'btn-default'
            },
            'confirm': {
                label: 'Delete',
                className: 'btn-danger pull-right'
            }
        },
        callback: function (result) {
            if (result) {
                $.ajax(url, {
                    type: "post",
                    data: {
                        "primaryKey": primaryKey
                    },
                    timeout: 6000,
                    //data: {value: ""},
                    dataType: 'text',
                    // iOS 6 has a dreadful bug where POST requests are not sent to the
                    // server if they are in the cache.
                    headers: {
                        'Cache-Control': 'no-cache'
                    }, // Apple!
                    beforeSend: function () {
                    },
                    success: function (data, textStatus, jqxhr) {
                        onSuccess(data, textStatus, jqxhr, $row);
                    }
                });
            }
        }
    });

    /**/
}

/**
    adds a key value to the rows array
**/
var addToRow = function(primaryKey, value, type, insert)
{
    var foundRow = false;

    for(var i = 0; i < rows.length; i++)
    {
        if (rows[i]["primaryKey"] === primaryKey && primaryKey !== null)
        {
            rows[i][type] = value;
            foundRow = true;
        }
        else if (rows[i]["insert"] === insert && insert !== null && typeof insert !== "undefined")
        {
            rows[i][type] = value;
            foundRow = true;
        }
            
    }
    if (!foundRow)
    {
        var row = {}
        row[type] = value;

        if (primaryKey !== null)
            row.primaryKey = primaryKey;
        else
            row.insert = insert;

        rows.push(row);
    }
}

var addInsertToRow = function (insert) {
    var row = {}
    row.insert = insert;

    rows.push(row);
}

var addInsertRowToDatabaseListener = function()
{
    $(".crm-save").one("click", function () {
        console.log("NOTHGIN");
        insertRowToDatabase($($(this).parent().parent()));
    });
}
var insertRowToDatabase = function (element, newRowObj, insertUrl, $dataTable) {
    var newRowTemplate = newRowObj;
    var $nRow = $($(element).parent().parent());
    var $tds = $nRow.find("td");
    var tdsc = $tds.length;
    newRowTemplate.SSMA_timestamp = 0;
    for (var i = 0; i < tdsc; i++) {
        var $td = $($tds[i]);
        var $input = $td.find("input");
        var prop = $td.data("property");
        newRowTemplate[prop] = $input.val();
        //console.log($td.data("property") + ": " + $input.val());
    }
    $.ajax({
        "url": insertUrl,
        "type": "POST",
        "data": {
            "json": JSON.stringify(newRowTemplate)
        },
        "success": function (data) {
            console.log(data);
            if (data == "1")
                $dataTable.draw();
        }
    })
    console.log(newRowTemplate);
}

/**
    Returns a currency string with swedish format from a given number.
**/
var formatCurrency = function (n) {
    if (String(n).indexOf('.') > -1)
        return parseFloat(n).toFixed(2) + " kr";
    else {
        n = parseFloat(n);
        return n.toFixed(2).replace(/./g, function (c, i, a) {
            return i && c !== "." && ((a.length - i) % 3 === 0) ? ' ' + c : c;
        }) + " kr";
    }
}


var triggerAlert = function (message, priority) {
    $(document).trigger("clear-alerts");
    $(document).trigger("add-alerts", [
      {
          'message': message,
          'priority': priority,
      }
    ]);
}

function get_browser() {
    var ua = navigator.userAgent,
        tem,
        M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
    if (/trident/i.test(M[1])) {
        tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
        return 'IE';
    }
    if (M[1] === 'Chrome') {
        tem = ua.match(/\bOPR\/(\d+)/)
        if (tem != null) {
            return 'Opera'
        }
    }
    M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
    if ((tem = ua.match(/version\/(\d+)/i)) != null) {
        M.splice(1, 1, tem[1]);
    }
    return M[0];
}

var webkit = get_browser() == "Chrome" ||
    /iPad|iPhone|iPod/.test(navigator.userAgent) ||
    get_browser() == "Opera";

/* TinyMCE Default Settigns */
var tinyDefaultPlugins = [
        'advlist autolink lists link image charmap print preview anchor',
        'searchreplace visualblocks code fullscreen',
        'insertdatetime media table contextmenu paste code'
    ];
var tinyDefaultToolbars = 'insertfile undo redo | styleselect         \
    | bold italic | alignleft aligncenter alignright alignjustify |   \
    bullist numlist outdent indent | fullscreen';

function isJSONString(str) {
    try {
        JSON.parse(str);
    } catch (e) {
        return false;
    }
    return true;
}

/* CRMCookies Namespace */
var CRMCookie = function()
{
    // Constructor
    var _ic = Cookies.getJSON("CRMSessionData");
    this.sites = _ic === undefined || _ic.sites === undefined ? this.bake().sites : _ic.sites;
     
}

CRMCookie.prototype.bake = function () {
    console.log("Baking, mmm");
    var CRMSessionData = {
        sites: []
    }
    Cookies.set('CRMSessionData', CRMSessionData);

    return CRMSessionData;
}

CRMCookie.prototype.findSite = function (site) {
    for (var i = 0; i < this.sites.length; i++) {
        if (site == this.sites[i].location)
            return i;
    }
    return -1;
}

CRMCookie.prototype.hasSite = function (site) {
    if (this.findSite(site) != -1)
        return true;
    else
        return false;
}

CRMCookie.prototype.updateCookie = function () {
    Cookies.set('CRMSessionData', {
        sites: this.sites
    });
}


CRMCookie.prototype.getCurrentSiteName = function () {
    return window.location.pathname.split('/')[1];
}

CRMCookie.prototype.getCurrentSite = function () {
    // Make sure that the current site exists.
    this.appendCurrentSite();

    // Find the site and update its properties
    var _siteID = this.findSite(this.getCurrentSiteName());
    return this.sites[_siteID];
}

CRMCookie.prototype.appendCurrentSite = function () {
    var currentSiteName = this.getCurrentSiteName();
    if (!this.hasSite(currentSiteName)) {
        this.sites.push({
            location: currentSiteName,
            search: undefined,
            selectedId: undefined
        });
        return true;
    }
    else
        return false;     
}

/*
 * Update the current page with current search data and selected id.
 * @param [string]  search filter string
 * @param [int]     the id of the currently selected row
 */
CRMCookie.prototype.updateSite = function (search, selectedId) {
    var _search = search === undefined ? null : search;
    var _selectedId = selectedId === undefined ? null : selectedId;

    // Make sure that the current site exists.
    this.appendCurrentSite();

    // Find the site and update its properties
    var _siteID = this.findSite(this.getCurrentSiteName());
    if(_search != null)
        this.sites[_siteID].search = _search;
    if(_selectedId != null)
        if (isJSONString(_selectedId))
            this.sites[_siteID].selectedId = JSON.parse(_selectedId);
        else
            this.sites[_siteID].selectedId = _selectedId;
        
    // Update the cookie
    this.updateCookie();
}

//calculate what the value should be if it is precentage
var calculateValue = function (val, oldVal) {
    var index = val.indexOf("%");
    // (1-9)% (10-99)% (100-999)%
    if (index >= 1 && index <= 3) {
        val = parseInt(val.substring(0, index));
        //(100)%
        if (val <= 100)
            val = oldVal - oldVal * (val / 100);
        else
            val = oldVal;
    }
    else
        val = oldVal;

    return val;
}
