// When the modal is shown, make an AJAX call to get the partial view and load it inside the modal
$('#createModal').on('show.bs.modal', function (event) {
    var modal = $(this);

    // Make the AJAX call to load the partial view
    $.ajax({
        url: '@Url.Action("Create", "Home")', // Assuming you have a Create action in Home controller
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
    e.preventDefault(); // Prevent default form submission

    var form = $("#timesheetForm")[0];

    if (form.checkValidity() === false) {
        $(form).addClass("was-validated"); // Highlight invalid fields
    } else {
        // If valid, submit the data via AJAX
        var formData = {
            UserName: $("#UserName").val(),
            Project: $("#Project").val(),
            Description: $("#Description").val(),
            HoursWorked: $("#HoursWorked").val(),
        };

        $.ajax({
            url: "/Home/Create", // Adjust based on your controller action
            type: "POST",
            data: formData,
            success: function (response) {
                $("#createModal").modal("hide"); // Close modal on success
                location.reload(); // Refresh the page or update UI dynamically
            },
            error: function () {
                alert("An error occurred. Please try again.");
            }
        });
    }
});