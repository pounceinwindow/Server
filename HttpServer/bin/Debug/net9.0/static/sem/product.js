(function () {
    // Цена из модели
    var price = parseFloat("${root.Details.Price}".replace(",", "."));
    if (isNaN(price)) price = 0;

    var qtyEl = document.getElementById("qty");
    var totalEl = document.getElementById("total-price");
    var plus = document.getElementById("plus");
    var minus = document.getElementById("minus");

    function updateTotal() {
        var q = parseInt(qtyEl.textContent || "1", 10);
        if (!q || q < 1) q = 1;
        qtyEl.textContent = q;
        totalEl.textContent = (price * q).toFixed(2);
    }

    if (plus) plus.onclick = function () {
        qtyEl.textContent = parseInt(qtyEl.textContent || "1", 10) + 1;
        updateTotal();
    };

    if (minus) minus.onclick = function () {
        var q = parseInt(qtyEl.textContent || "1", 10) - 1;
        if (q < 1) q = 1;
        qtyEl.textContent = q;
        updateTotal();
    };

    updateTotal();


    // AJAX check availability — закрываем требование по AJAX
    var checkBtn = document.getElementById("check-btn");
    if (checkBtn) {
        checkBtn.addEventListener("click", function () {
            var experienceId = "${root.Experience.Id}";
            var xhr = new XMLHttpRequest();
            xhr.open("GET", "/api/check-availability?experienceId=" + encodeURIComponent(experienceId));
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    if (xhr.status === 200) {
                        alert("Dates are available for your selection.");
                    } else {
                        alert("Unable to check availability. Please try again.");
                    }
                }
            };
            xhr.send();
        });
    }
})();