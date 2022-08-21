$(document).ready(function () {
    //$("#submit_comment_btn").on('click', function () {
    //    event.preventDefault();
    //    var form = $("#new_comment_form")
    //    console.log(form.serialize())

    //})

    $("#new_comment_form").submit(function (e) {

        e.preventDefault();

        var form = $(this);
        var actionUrl = form.attr('action');

        $.ajax({
            type: "POST",
            url: actionUrl,
            data: form.serialize(), 
            success: function (data) {
                console.log(data);
                if (data.success)
                    location.reload(true)
            },
            error: function (error) {
                console.log(error)
            }
        });

    });

    $(".delete-form").submit(function (e) {

        e.preventDefault();

        var form = $(this);
        var actionUrl = form.attr('action');

        $.ajax({
            type: "Delete",
            url: actionUrl,
            data: form.serialize(),
            success: function (data) {
                console.log(data);
                if (data.success)
                    location.reload(true)
            },
            error: function (error) {
                console.log(error)
            }
        });

    })
})