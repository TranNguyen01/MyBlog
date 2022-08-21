$(document).ready(function () {
    $("#avatar_input").on("change", (event) => {
        var input = event.target;
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#preview_avatar').attr('src', e.target.result);
            }
            reader.readAsDataURL(input.files[0]);
            var submitBtn = $("#save_avatar_submit_btn")
            submitBtn.removeClass('d-none')
            submitBtn.on("click", function () {
                console.log($("#save_avatar_form"))
                $("#save_avatar_form").submit();
            })
        } else {
            $("#save_avatar_submit_btn").addClass('d-none')
        }
    })
})
