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
                    var busSelect = $this.find(".select-bus")[0];
                    busSelect.selectedIndex = Math.floor(Math.random() * (busSelect.length - 1)) + 1;

                    var hourSelect = $this.find(".hourList")[0];
                    hourSelect.selectedIndex = data.hour;
                    var minuteSelect = $this.find(".minuteList")[0];
                    minuteSelect.selectedIndex = 6;
                });
                $("#employee-table").tablesorter();
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
