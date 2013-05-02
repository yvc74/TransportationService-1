﻿$(document).ready(function () {
    $("#route-select").on("change", function (event) {
        var id = this.options[this.selectedIndex].value;
        if (id != '') {
            $.post("/EmployeeManagement/GetEmployeeInfo", { id: id }, function (data) {
                $("#employee-management-submit").removeClass('hide');
                $("#route-view").html(data.html);
                rollDown($("#route-view"));
                $("#employee-table").find("tr.employee-row").each(function () {
                    $this = $(this);
                    var stopSelect = $this.find(".select-stop")[0];
                    stopSelect.selectedIndex = Math.floor(Math.random() * (stopSelect.length - 1)) + 1;
                    if (stopSelect.selectedIndex == -1) stopSelect.selectedIndex = 0;
                    var busSelect = $this.find(".select-bus")[0];
                    busSelect.selectedIndex = Math.floor(Math.random() * (busSelect.length - 1)) + 1;
                    if (busSelect.selectedIndex == -1) busSelect.selectedIndexs = 0;

                    var hourSelect = $this.find(".hourList")[0];
                    hourSelect.selectedIndex = data.hour;
                    var minuteSelect = $this.find(".minuteList")[0];
                    minuteSelect.selectedIndex = 6;
                });
                $("#employee-table").tablesorter({
                    headers: {
                        0: {
                            sorter: false
                        },
                        2: {
                            sorter: false
                        },
                        3: {
                            sorter: false
                        },
                        4: {
                            sorter: false
                        }
                    }
                });
            });
        }
        else {
            $("#employee-management-submit").addClass('hide');
            rollUp($("#route-view"), function () {
                $("#route-view").html("");
            });
        }
    });
    var datePicker = $('#datepicker');
    var date = new Date();
    var strDate = date.getMonth() + 1 + "-" + date.getDate() + "-" + date.getFullYear();
    datePicker.data("date", strDate);
    datePicker.find(".span2").attr("value", strDate);
    datePicker.find(".span2").data("bind", strDate);
    datePicker.datepicker();
});
function submitRouteData() {
    var employeeDict = {};
    var s = document.getElementById("route-select");
    var validSelection = true;
    $("#employee-table").find("tr.employee-row").each(function () {
        $this = $(this);
        if ($this.find(".checkbox-sub")[0].checked) {
            var stopSelect = $this.find(".select-stop")[0];
            var busSelect = $this.find(".select-bus")[0];

            var hourSelect = $this.find(".hourList")[0];
            var minuteSelect = $this.find(".minuteList")[0];

            var strDate = $("#datepicker").data("date").split("-");
            var date = new Date(strDate[2],
                Number(strDate[0]) - 1,
                strDate[1],
                hourSelect[hourSelect.selectedIndex].value,
                minuteSelect[minuteSelect.selectedIndex].value);
            var entry = {
                stop: stopSelect[stopSelect.selectedIndex].value,
                bus: busSelect[busSelect.selectedIndex].value,
                date: date
            };
            if (entry.bus == "" || entry.stop == "") {
                validSelection = false;
            }
            else {
                employeeDict[this.dataset.id] = entry;
            }
        }
    });
    if (validSelection) {
        $.post("/EmployeeManagement/RecordInstance", { employees: JSON.stringify(employeeDict), routeId: s[s.selectedIndex].value }, function (data) {
            $.notify.addMessage("The data was successfully saved", { type: "success" });
            rollUp($("#route-view"), function () {
                $("#route-view").html("");
            });
            s.selectedIndex = 0;
        });
    } else {
        rollDown($("#instanceError"));
        setTimeout(function () {
            rollUp($("#instanceError"));
        }, 8000);
    }
}
function validateDate(d) {
    if (d.length == 1)
        return "0" + d;
    return d;
}

function viewRouteInformation(routeId) {
    $.post("/EmployeeManagement/GetRouteInformation",
        { routeId: routeId },
        function (html) {
            $("#view-container").html(html);
            $("table table").tablesorter();
            $("#route-information").find("tbody > tr").css("cursor", "pointer");
        });
}
function viewEmployees(employeeRowId) {
    var id = "#employees-" + employeeRowId;
    var row = $(id);
    if (row.hasClass("hide")) {
        row.removeClass("hide");
    } else {
        row.addClass("hide");
    }
}

function processFilter() {
    var filterByColumn = $("#filterType").val();
    var text = document.getElementById('filterUsage').value;
    var filterByColumn2 = $("#filterType2").val();
    var text2 = document.getElementById('filterUsage2').value;
    var date = document.getElementById('filterDateUsage').value;
    $(".activity-row").each(function () {
        var isHidden = true;
        var isHidden2 = true;
        var isHiddenDate = true;
        //check first filter
        if (filterByColumn == -1) {
            $(this).children("td").each(function () {
                if (-1 !== this.innerHTML.toLowerCase().indexOf(text.toLowerCase())) {
                    isHidden = false;
                }
            });
        } else {
            var length = text.length;
            $(this).children("td").each(function (ndx) {
                if (ndx == filterByColumn) {
                    if (this.innerHTML.substring(0, length + 3) == text + " - " || text == "") {
                        isHidden = false;
                    }
                }
            });
        }
        //check date
        $(this).children("td").each(function (ndx) {
            if (ndx == 4) {
                if (-1 !== this.innerHTML.toLowerCase().indexOf(date.toLowerCase())) {
                    isHiddenDate = false;
                }
            }
        });
        //check 2nd filter
        if (filterByColumn2 == -1) {
            $(this).children("td").each(function () {
                if (-1 !== this.innerHTML.toLowerCase().indexOf(text2.toLowerCase())) {
                    isHidden2 = false;
                }
            });
        } else {
            var length = text2.length;
            $(this).children("td").each(function (ndx) {
                if (ndx == filterByColumn2) {
                    if (this.innerHTML.substring(0, length + 3) == text2 + " - " || text2 == "") {
                        isHidden2 = false;
                    }
                }
            });
        }

        var hideColumn = isHidden || isHidden2 || isHiddenDate;
        if (hideColumn) {
            $(this).addClass("hide");
        }
        else {
            $(this).removeClass("hide");
        }
    });
    calculateActivityAmounts();
}

function calculateActivityAmounts() {
    var tbl = $("#system-usage-table");
    var arrs = [[], [], [], [], [], [], []];
    $(".activity-row:not('.hide')").each(function () {
        for (var i = 1; i < 7; i++) {
            if (i == 5)
                continue;
            $(this).find("td:nth-child(" + i + ")").each(function () {
                var s = this.innerHTML;
                var val = Number(s.substring(0, s.indexOf("-") - 1));
                if (arrs[i - 1].indexOf(val) == -1) {
                    arrs[i - 1].push(val);
                }
            });
        }
    });
    for (var i = 1; i < 7; i++) {
        if (i == 5)
            continue;
        tbl.find("tfoot > tr > td:nth-child(" + i + ")").text(arrs[i - 1].length);
    }
}

function toggleAll(source) {
    checkboxes = document.getElementsByClassName('checkbox-sub');
    for (var i = 0, n = checkboxes.length; i < n; i++) {
        checkboxes[i].checked = source.checked;
    }
}