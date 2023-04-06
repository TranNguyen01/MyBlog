$(document).ready(function () {
    postChart();
    postDonutChart();
    lineChart("date");
    staticAll();
})

function getData(ajaxurl) {
    return $.ajax({
        url: ajaxurl,
        type: 'GET',
    });
};

async function staticAll() {
    try {
        const postStatisticRes = await getData('/Statistic/Post')
        const documentStatisticRes = await getData('/Statistic/Document')
        const userStatisticRes = await getData('/Statistic/User')
        const censorShipStatisticRes = await getData('/Statistic/CensorShip')
        $("#total_user_statistic").text(userStatisticRes.data[0].value)
        $("#total_post_statistic").text(postStatisticRes.data[0].value)
        $("#total_document_statistic").text(documentStatisticRes.data[0].value)
        $("#total_post_censor_static").text(censorShipStatisticRes.data[0].value)
        
    } catch (err) {
        console.log(err);
    }
}

async function postChart() {
    try {
        const postStatisticRes = await getData('/Statistic/Post?field=category')
        let postLlabels = postStatisticRes.data.map(item => item.name);
        let postData = postStatisticRes.data.map(item => item.value)

        const documentStatisticRes = await getData('/Statistic/Document?field=category')
        let documentLlabels = documentStatisticRes.data.map(item => item.name);
        let documentData = documentStatisticRes.data.map(item => item.value)

        const ctx = document.getElementById('myAreaChart');
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: postLlabels,
                datasets: [{
                    label: 'Số lượng bài viết được chia sẻ',
                    data: postData,
                    borderWidth: 1,
                    order: 1
                }, {
                    label: 'Số lượng tài liệu được chia sẻ',
                    data: documentData,
                    borderWidth: 1,
                    order: 2
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    } catch (err) {
        console.log(err);
    }
}



async function postDonutChart() {
    try {
        const postStatisticRes = await getData('/Statistic/Post?field=status')
        let postLlabels = postStatisticRes.data.map(item => item.name);
        let postData = postStatisticRes.data.map(item => item.value)

        const documentStatisticRes = await getData('/Statistic/Document?field=status')
        let documentLlabels = documentStatisticRes.data.map(item => item.name);
        let documentData = documentStatisticRes.data.map(item => item.value)

        const ctx1 = document.getElementById('postDougnutChart');
        new Chart(ctx1, {
            type: 'doughnut',
            data: {
                labels: postLlabels,
                datasets: [{
                    label: 'Số lượng bài viết',
                    data: postData,
                }]
            }
        });

        const ctx2 = document.getElementById('documentDoughnutChart');
        new Chart(ctx2, {
            type: 'doughnut',
            data: {
                labels: documentLlabels,
                datasets: [{
                    label: 'Số lượng tài liệu',
                    data: documentData,
                }]
            }
        });
    } catch (err) {
        console.log(err);
    }
}



async function lineChart(field) {
    try {

        let dates = getAllDaysInMonth(2023, 1)
        let toDate = new Date().setHours(0, 0, 0, 0);
        const postStatisticRes = await getData(`/Statistic/Post?field=${field}`)
        let postData = dates.map(date => {
            let dateArr = date.split('/')
            if (new Date(dateArr[2], dateArr[1] - 1, dateArr[0]).getTime() > toDate)
                return null
            let item = postStatisticRes.data.find(item => item.name.replaceAll(/\s/g, '') == date.replaceAll(/\s/g, ''));
            console.log(postStatisticRes.data, date)
            return item ? item.value : 0
        })

        console.log(postData)

        const documentStatisticRes = await getData(`/Statistic/Document?field=${field}`)
        let documentData = dates.map(date => {
            let dateArr = date.split('/')
            if (new Date(dateArr[2], dateArr[1] - 1, dateArr[0]).getTime() > toDate)
                return null
            let item = documentStatisticRes.data.find(item => item.name.replaceAll(/\s/g, '') == date.replaceAll(/\s/g, ''))
            return item ? item.value : 0
        })

        const userStatisticRes = await getData(`/Statistic/User?field=${field}`)
        let userData = dates.map(date => {
            let dateArr = date.split('/')
            if (new Date(dateArr[2], dateArr[1] - 1, dateArr[0]).getTime() > toDate)
                return null
            let item = userStatisticRes.data.find(item => item.name.replaceAll(/\s/g, '') == date.replaceAll(/\s/g, ''))
            return item ? item.value : 0
        })

        const ctx1 = document.getElementById('lineChart');
        new Chart(ctx1, {
            type: 'line',
            data: {
                labels: dates,
                datasets: [{
                    label: 'Số lượng bài viết',
                    data: postData,
                }, {
                    label: 'Số lượng tài liệu',
                    data: documentData,
                },
                {
                    label: 'Số lượng người dùng',
                    data: userData,
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                maintainAspectRatio: false
            }
        });

    } catch (err) {
        console.log(err);
    }
}



function getAllDaysInMonth(year, month) {
    const date = new Date(year, month - 1, 1);

    const dates = [];

    while (date.getMonth() == month - 1) {
        dates.push(`${date.getDate()}/${date.getMonth() + 1}/${date.getFullYear()}`);
        date.setDate(date.getDate() + 1);
    }

    return dates;
}