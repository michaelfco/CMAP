$('#createModal').on('show.bs.modal', function (event) {
    var modal = $(this);

    //load partial view - ajax
    $.ajax({
        url: '@Url.Action("Create", "Home")', 
        type: 'GET',
        success: function (data) {
            modal.find('#modalContent').html(data);
        },
        error: function (xhr, status, error) {
            console.error('Error loading the modal content: ' + error);
        }
    });
});


$("#saveTimesheet").on("click", function (e) {
    e.preventDefault();

    var form = $("#timesheetForm")[0];

    if (form.checkValidity() === false) {
        //highlight invalid fields
        $(form).addClass("was-validated");
    } else {
        //If valid submit the data via AJAX
        var formData = {
            UserName: $("#UserName").val(),
            Date: $("#Date").val(),
            Project: $("#Project").val(),
            Description: $("#Description").val(),
            HoursWorked: $("#HoursWorked").val(),
        };

        $.ajax({
            url: "/Home/Create",
            type: "POST",
            data: formData,
            success: function (response) {

                if (response.success) {
                    $("#createModal").modal("hide");
                    //show success message
                    localStorage.setItem("timesheetSuccess", "Timesheet entry added successfully!");
                    location.reload(); 
                    //$("#msgError").html('<div class="alert alert-success">Timesheet entry added successfully!</div>');
                    //setTimeout(function () {
                    //    $("#msgError").fadeOut("slow", function () {
                    //        location.reload(); 
                    //    });
                    //}, 1000);
                    
                } else {
                    //Display validation errors inside the modal
                    var errorContainer = $("#errorMessages");
                    //clear old errors
                    errorContainer.html(""); 
                    $.each(response.errors, function (index, error) {
                        errorContainer.append(`<div class="alert alert-danger">${error}</div>`);
                    });
                }
            },
            error: function () {
                alert("An error occurred. Please try again.");
            }
        });
    }
});

$(document).ready(function () {
    var successMessage = localStorage.getItem("timesheetSuccess");
    if (successMessage) {
        $("#msgError").html(`<div class="alert alert-success">${successMessage}</div>`);        
        localStorage.removeItem("timesheetSuccess");      
        setTimeout(function () {
            $("#msgError").fadeOut("slow");
        }, 3000);
    }
});