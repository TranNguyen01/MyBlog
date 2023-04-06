$(document).ready(() => {
    let thumbnailImage = $("#thumbnail_image")
    if (thumbnailImage.attr("src") != "" && thumbnailImage.attr("src") != undefined) {
        $("#drop-label").addClass("d-none");
        $(".thumbnail_container").css("min-height", "0px")
    }

    $("#thumbnail").on("change", async (event) => {
        if (event.target.files.length == 0) {
            $("#drop-label").removeClass("d-none");
            $("#thumbnail_image").addClass("d-none")
            let progressbar = $("#progressbar")
            progressbar.css("width", "0%")
            progressbar.attr("aria-valuenow", 0)
            progressbar.css("opacity", 0)
            $(".thumbnail_container").css("min-height", "300px")
            $("#thumbnail_info").val("");
        }
        else {
            console.log($("#drop-label"))
            $("#drop-label").addClass("d-none");
            $(".thumbnail_container").css("min-height", "0px")
            await uploadImage(event);

        }
    })

    //$('#drop-area').on('dragenter dragover drop dragleave ', function (event) {
    //    event.preventDefault();
    //});

    $('#drop-area').bind('dragover dragenter', function () {
        $(this).addClass('highlight');
    });

    $('#drop-area').bind('dragleave', function () {
        $(this).removeClass('highlight');
    });


    $('#drop-area').bind('drop', function (event) {
        let dt = event.originalEvent.dataTransfer
        let files = dt.files
        console.log(files)
        $(this).removeClass('highlight');
    });
})

async function uploadImage(event) {
    const signResponse = await fetch('/image/signature');
    const signData = await signResponse.json();

    const files = event.target.files;
    
    if (files.length == 0) return;

    const formData = new FormData();
    let file = files[0];
    let thumnailImage = $("#thumbnail_image")
    thumnailImage.attr("src", URL.createObjectURL(file));
    thumnailImage.css("opacity", '5%')
    thumnailImage.removeClass("d-none")
    

    formData.append("file", file);
    formData.append("api_key", signData.api_key);
    formData.append("timestamp", signData.timestamp);
    formData.append("signature", signData.signature);
    formData.append("folder", "Blog");

    $.ajax({
        xhr: function () {
            var xhr = new window.XMLHttpRequest();
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    let percentComplete = (evt.loaded / evt.total) * 100;
                    console.log(percentComplete);
                    let progressbar = $("#progressbar") 
                    progressbar.css("width", `${percentComplete}%`)
                    progressbar.attr("aria-valuenow", percentComplete)
                    progressbar.css("opacity", `${percentComplete}%`)
                    thumnailImage.css("opacity", `${percentComplete}%`)
                }
            }, false);

            xhr.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total;
                    console.log(percentComplete)
                }
            }, false);

            return xhr;
        },
        type: 'POST',
        url: signData.url,
        contentType: false,
        processData: false,
        data: formData,
        success: function (data) {
            console.log(data);
            $("#thumbnail_info").val(JSON.stringify(data));
        },
        error: function (error) {
            $("#thumbnail_info").val("");
        }
    });
}

