var posts = [];
var postReports = [];
$(document).ready(function () {
    $.ajax({
        method: "GET",
        url: "/Report/GetList",
        success: function (res) {
            posts = res.data;
            for (let postReport of res.data) {
                appendPost(postReport.post)
            }
            getReport(res.data[0].post.id);
            $('#post_table').DataTable();
        },
        error: function (error) {
            console.log(error)
        }
    })
    
})

function appendPost(post) {
    $("#post_table tbody").append(`
        <tr id="${post.id}" role="button">
            <td  onclick="getReport('${post.id}')">${post.title}</td>
            <td  onclick="getReport('${post.id}')">${post.author?.lastName} ${post.author?.firstName}</td>
            <td  onclick="getReport('${post.id}')">${post.category?.name }</td>
            <td  onclick="getReport('${post.id}')">${formatString(post.createdAt)}</td>
            <td  onclick="getReport('${post.id}')">${genStatus(post)}</td>
            <td class="align-middle">
                <div class="d-flex flex-row justify-content-end flex-nowrap alig-items-center border-top-0">
                    <button class="icon-btn text-primary p-0 mr-3" title="detail"
                        onclick="viewPostDetail(event, '${post.id}')"
                        data-toggle="modal"
                        data-target="#post_detail_modal">
                        <i class="fa-regular fa-magnifying-glass"></i>
                    </button>
                    <button class="icon-btn text-danger p-0"
                        title="decline"
                        onclick="viewDeclinePost(event, '${post.id}')">
                        <i class="fa-regular fa-circle-exclamation"></i>
                    </button>
                </div>
            </td>
        </tr>`)
}

function genStatus(post) {
    let ele;
    switch (post.status) {
        case 1:
            ele = `<p class="text-success mb-0" >Phê duyệt</p>`
            break;
        case 0:
            ele = `<p class="text-primary mb-0">Chờ phê duyệt</p>`
            break;
        case -1:
            ele = `<p class="text-danger mb-0">Từ chối</p>`
            break;
        default:
            ele = `<p class="text-primary mb-0">Chờ phê duyệt</p>`
            break;
    }
    return ele;
}

function getReport(postId) {
    $.ajax({
        method: "GET",
        url: `/Report/Post?post=${postId}`,
        success: function (res) {
            postReports = res.data.items;
            $("#report_table tbody").empty()
            for (let postReport of res.data.items) {
                $("#report_table tbody").append(`
                    <tr id='report_${postReport.id}'>
                        <td class="align-middle">${formatString(postReport.createdAt) }</td>
                        <td class="align-middle">${postReport.reason.name}</td>
                        <td class="align-middle" >
                            <button onclick="showReportDetailModal('${postReport.id}')" class="icon-btn text-primary">
                            <i class="fa-regular fa-magnifying-glass"></i>
                            </button>
                        </td>
                    </tr>`
                )
            }
        },
        error: function (error) {
            console.log(error)
        }
    })
}


function viewPostDetail(event, postId) {
    let postReport = posts.find(p => p.postId == postId)
    let post = postReport.post
    let content = `<div class="container post-article" post-id="${post.id}">
            <div class="row justify-content-center">
                <div class="col-12 col-lg-12 col-xl-10 text-left">
                    <h1 class="font-weight-bold">${post.title}</h1>
                    <div class="text-left mt-3 mb-4">
                        <span class="font-weight-bold mr-2">${post.author.lastName} ${post.author.firstName}</span>
                        <time class="text-muted" datetime="${post.createdAt}"></time>
                    </div>
                </div>
            </div>
            <div class="row justify-content-center">
                <div class="col-12 col-lg-12 col-xl-10 mb-3">
                    ${post.content}
                </div> 
            </div>
        </div >`
    $("#post_detail_modal .modal-body").empty();
    $("#post_detail_modal .modal-body").append(content)
}

function viewDeclinePost(event, postId) {
    let postReport = posts.find(p => p.postId == postId)
    let post = postReport.post
    $.ajax({
        method: "GET",
        url: '/Reason',
        success: function (response) {
            appenDeclinePostModal(response.data, post);
            $('#decline_modal').modal('show');
            addSubmitDeclineEvent();
        },
        error: function (error) {
            console.log(error)
        }
    })
}


function appenDeclinePostModal(reason, post) {
    $("#decline_modal").remove();
    $("body").append(`
        <div class="modal" tabindex="-1" id="decline_modal">
            <div class="modal-dialog">
                <div class="modal-content p-3">
                    <form id="decline_post_form" method="POST">
                        <div class="modal-header border-bottom-0">
                            <h5 class="modal-title text-center text-danger">Xử lý bài viết!</h5>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body pt-0">
                            <p class="py-0">Xử lý bài viết: <strong>"${post.title}"</strong>. Vui lòng chọn lý do ngừng hiển thị bài viết này!</p>
                                <input type="text" name="postId" value="${post.id}" hidden/>
                                <input type="text" name="status" value="-1" hidden/>
                                <div class="form-group">
                                    <label for="reasonId" class="font-weight-bold">Lý do từ chối</label>
                                    <select class="form-control" name="reasonId">
                                    ${reason.map(item => `<option value="${item.id}">${item.name}</option>`)}
                                    </select>
                                </div>
                                <div class="form-group">
                                    <label for="comment" class="font-weight-bold">Nhận xét</label>
                                    <textarea class="form-control" name="comment" rows="3"></textarea>
                                </div>
                        </div>
                        <div class="modal-footer border-top-0">
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Hủy bỏ</button>
                            <button type="summit" id="confirm_delete_post" class="btn btn-danger">Từ chối</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>`
    )
}

function addSubmitDeclineEvent() {
    $("#decline_post_form").on("submit", function (event) {
        event.preventDefault();
        let form = $(this)
        const array = form.serializeArray();
        const json = {};
        $.each(array, function () {
            json[this.name] = this.value || "";
        });
        $.ajax({
            method: "POST",
            url: "CensorShip",
            data: JSON.stringify(json),
            contentType: "application/json; charset=utf-8",
            traditional: true,
            success: function (response) {
                if (response.code == 0) {
                    showAlert("success", "", `Từ chối bài viết thành công!`)
                    $("#decline_modal").modal("hide")
                } else {
                    showAlert("danger", "", response.message);
                    $("#decline_modal").modal("hide");
                }
            },
            error: function (error) {
                console.log(error)
            }
        })
    })
}

function formatString(dateString) {
    var currentDate = new Date(dateString);
    var month = currentDate.getMonth() + 1;
    if (month < 10) month = "0" + month;
    var dateOfMonth = currentDate.getDate();
    if (dateOfMonth < 10) dateOfMonth = "0" + dateOfMonth;
    var year = currentDate.getFullYear();
    var formattedDate = dateOfMonth + "/" + month + "/" + year;
    return formattedDate;
}


function showReportDetailModal(reportId) {
    let report = postReports.find(item => item.id == reportId);
    console.log("report", report);
    $("#report_detail_modal").remove();
    $("body").append(`
        <div class="modal" tabindex="-1" id="report_detail_modal">
            <div class="modal-dialog">
                <div class="modal-content p-3">
                    <form id="decline_post_form" method="POST">
                        <div class="modal-header border-bottom-0">
                            <h5 class="modal-title text-center text-danger">Báo cáo bài viết!</h5>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body pt-0">
                            <div class="form-group">
                                <label for="reasonId" class="font-weight-bold">Lý do báo cáo</label>
                                <input type="text" class="form-control" value="${report.reason.name}" readonly />
                            </div>

                            <div class="form-group">
                                <label for="comment" class="font-weight-bold">Nhận xét</label>
                                <textarea class="form-control" name="comment" rows="3" readonly value="${report.content ? report.content : `Không có nội dung`}"></textarea>
                            </div>
                        </div>
                        <div class="modal-footer border-top-0">
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Hủy bỏ</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>`)
    $("#report_detail_modal").modal("show");
}
