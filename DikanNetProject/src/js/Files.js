function uploadFile2 () {
    console.log("Change IdFile");
    var file = $("#IdFile").get(0).files;
    var data = new FormData;
    data.append(file[0].name, file[0]);
    console.log(data);
    $.ajax({
        type: "POST",
        url: "~/Student/UploadFile",
        data: data,
        contentType: false,
        proccessData: false,
        success: function (response) {
            alert("Success!");

        },
        error: function (err) {
            alert("Upload Failed!");
            console.log(err);
        }

    })
};