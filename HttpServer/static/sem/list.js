
document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('filterForm');
    var gridContainer = document.getElementById('gridContainer');
    var tabs = document.querySelectorAll('.tabs .tab');

    // если мы не на странице /tours — тихо выходим
    if (!form || !gridContainer) {
        return;
    }

    // --------------------------------------------------------------------
    // helpers
    // --------------------------------------------------------------------

    // собираем query-string из формы
    function buildQueryFromForm() {
        var formData = new FormData(form);
        var params = new URLSearchParams();

        formData.forEach(function (value, key) {
            if (value === null || value === undefined || value === '') {
                return;
            }
            params.append(key, value.toString());
        });

        return params.toString();
    }

    // обновляем число "X Experiences" на основе data-total у .grid
    function updateTotalFromGrid() {
        var totalEl = document.getElementById('totalCount');
        if (!totalEl) return;

        var gridRoot = gridContainer.querySelector('.grid');
        if (!gridRoot) return;

        var total = gridRoot.getAttribute('data-total');
        if (!total) return;

        totalEl.textContent = total;
    }

    // загружаем partial /tours/partial и подменяем грид
    function loadGrid(pushState) {
        var query = buildQueryFromForm();
        var url = '/tours/partial' + (query ? ('?' + query) : '');

        fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(function (r) { return r.text(); })
            .then(function (html) {
                gridContainer.innerHTML = html;
                updateTotalFromGrid();

                if (pushState) {
                    var fullUrl = '/tours' + (query ? ('?' + query) : '');
                    window.history.pushState({ query: query }, '', fullUrl);
                }
            })
            .catch(function (err) {
                console.error('Error loading grid:', err);
            });
    }

    // --------------------------------------------------------------------
    // обработчики формы и табов
    // --------------------------------------------------------------------

    // любое изменение в форме -> перезагружаем грид
    form.addEventListener('change', function () {
        loadGrid(true);
    });

    // на всякий случай, если форма отправляется по Enter
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        loadGrid(true);
    });

    // клики по чипсам категорий
    tabs.forEach(function (tab) {
        tab.addEventListener('click', function () {
            var cat = this.getAttribute('data-category');

            // визуально активный таб
            tabs.forEach(function (t) {
                t.classList.toggle('active', t === tab);
            });

            // переключаем чекбоксы категорий в форме
            var catInputs = form.querySelectorAll('input[name="category"]');
            catInputs.forEach(function (input) {
                input.checked = (input.value === cat);
            });

            loadGrid(true);
        });
    });

    // поддержка кнопок Назад/Вперёд в браузере
    window.addEventListener('popstate', function () {
        var url = '/tours/partial' + window.location.search;
        fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(function (r) { return r.text(); })
            .then(function (html) {
                gridContainer.innerHTML = html;
                updateTotalFromGrid();
            })
            .catch(function (err) {
                console.error('Error loading grid (popstate):', err);
            });
    });

    // первоначальный вызов, чтобы наверняка привести число в соответствие
    updateTotalFromGrid();
});
