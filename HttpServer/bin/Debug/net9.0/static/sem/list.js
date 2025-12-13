document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('filterForm');
    var gridContainer = document.getElementById('gridContainer');
    var tabs = document.querySelectorAll('.tabs .tab');

    if (!form || !gridContainer) {
        return;
    }

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

    function updateTotalFromGrid() {
        var totalEl = document.getElementById('totalCount');
        if (!totalEl) return;

        var gridRoot = gridContainer.querySelector('.grid');
        if (!gridRoot) return;

        var total = gridRoot.getAttribute('data-total');
        if (!total) return;

        totalEl.textContent = total;
    }

    function loadGrid(pushState) {
        var query = buildQueryFromForm();
        var url = '/tours/partial' + (query ? ('?' + query) : '');

        fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(function (r) {
                return r.text();
            })
            .then(function (html) {
                gridContainer.innerHTML = html;
                updateTotalFromGrid();

                if (pushState) {
                    var fullUrl = '/tours' + (query ? ('?' + query) : '');
                    window.history.pushState({query: query}, '', fullUrl);
                }
            })
            .catch(function (err) {
                console.error('Error loading grid:', err);
            });
    }
    form.addEventListener('change', function () {
        loadGrid(true);
    });

    form.addEventListener('submit', function (e) {
        e.preventDefault();
        loadGrid(true);
    });

    tabs.forEach(function (tab) {
        tab.addEventListener('click', function () {
            var cat = this.getAttribute('data-category');

            tabs.forEach(function (t) {
                t.classList.toggle('active', t === tab);
            });

            var catInputs = form.querySelectorAll('input[name="category"]');
            catInputs.forEach(function (input) {
                input.checked = (input.value === cat);
            });

            loadGrid(true);
        });
    });

    window.addEventListener('popstate', function () {
        var url = '/tours/partial' + window.location.search;
        fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(function (r) {
                return r.text();
            })
            .then(function (html) {
                gridContainer.innerHTML = html;
                updateTotalFromGrid();
            })
            .catch(function (err) {
                console.error('Error loading grid (popstate):', err);
            });
    });

    updateTotalFromGrid();
});
