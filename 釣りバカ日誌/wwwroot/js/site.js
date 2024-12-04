// 非同期通信
async function asyncForm(url, data) {

    return await fetch(url, {
        method: "POST",
        body: createFormData(data)
    })
        .then(response => {
            // セッションタイムアウト時
            if (response.status == 401) {
                window.location.reload();
                return;
            }

            if (!response.ok) {
                alert("予期しないエラーが発生しました。");
                loadEnd();
                return;
            }
            return response;
        })
}

// フォームデータ作成
function createFormData(data) {

    const formData = new FormData();

    if (!data) {
        data = {};
    }

    for (const [key, value] of Object.entries(changeArrayFormat(data))) {

        formData.append(`${key}`, value);
    }
    return formData;
}

// 連想配列フォーマット変換
function changeArrayFormat(data) {
    let obj = new Object();
    const formatChange = (data, key) => {

        Object.keys(data).reduce((result, currentValue) => {
            let keyName = !key ? currentValue : key + `[${currentValue}]`;
            if (isObjorArray(data[currentValue])) {
                formatChange(data[currentValue], keyName);
            } else {
                obj[keyName] = data[currentValue];
            }
            return result;
        }, {});

        return obj;
    }
    return formatChange(data, null);
}

// 配列、連想配列判定
function isObjorArray(o) {
    return (o instanceof Object || o instanceof Array) ? true : false;
}

// Ajax送信
function baseAjax(url, data, contentType = 'application/x-www-form-urlencoded; charset=UTF-8') {

    loadStart();

    if (contentType == 'application/json') {
        data = JSON.stringify(data);
    }

    return new Promise((resolve, reject) => {
        $.ajax(
            {
                url: url,
                type: 'POST',
                data: data,
                contentType: contentType,
                cache: false,
            }
        ).then(
            function (result, status, xhr) {
                loadEnd();

                resolve(result);
            },
            function (result) {
                if (result.status == 401) {
                    window.location.reload();
                }
                alert("予期しないエラーが発生しました。");

                loadEnd();

                reject();
            }
        );
    })
}

// Edit遷移(0:新規, 1:更新)
function actionEdit(id, upd) {
    let form = document.createElement("form");
    form.method = "POST";
    form.action = "Home/Edit";
    form.style.display = "none";
    document.body.append(form);
    let input = document.createElement('input');
    input.type = "hidden";
    input.name = "id";
    input.value = id;
    form.appendChild(input);
    input = document.createElement('input');
    input.type = "hidden";
    input.name = "upd";
    input.value = upd;
    form.appendChild(input);

    form.submit();
}

function loadStart() {
    $("#overlay").fadeIn();
}

function loadEnd() {
    $("#overlay").fadeOut();
}