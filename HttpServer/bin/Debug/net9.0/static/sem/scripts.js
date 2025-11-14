// ВСТАВКА header/footer + инициализация всего остального
document.addEventListener('DOMContentLoaded', async () => {
    const mountHeader = document.querySelector('[data-include="header"]');
    const mountFooter = document.querySelector('[data-include="footer"]');

    if (mountHeader) {
        const html = await fetch('header.html').then(r => r.text());
        mountHeader.innerHTML = html;
    }

    if (mountFooter) {
        const html = await fetch('footer.html').then(r => r.text());
        mountFooter.innerHTML = html;
    }

    initHeaderShrink();
    initFilterSections();
    initPriceSlider();
    initSinglePriceRange();
});
document.getElementById("read-toggle")?.addEventListener("click", () => {
    const box = document.getElementById("desc-box");
    box.classList.remove("faded");
    box.style.maxHeight = "none";
    console.log("Description expanded");
});

// Липкий header + shrink
function initHeaderShrink() {
    const topEl = document.getElementById('siteTop');
    if (!topEl) return;

    let ticking = false;

    function onScroll() {
        if (ticking) return;
        requestAnimationFrame(() => {
            const scrolled = window.scrollY || document.documentElement.scrollTop;
            topEl.classList.toggle('shrink', scrolled > 16);
            ticking = false;
        });
        ticking = true;
    }

    window.addEventListener('scroll', onScroll, {passive: true});
    onScroll();
}

// сворачивание секций фильтров
function initFilterSections() {
    document.querySelectorAll('.aside .fg .fh').forEach(h => {
        h.addEventListener('click', () => {
            h.parentElement.classList.toggle('collapsed');
        });
    });
}

// одиночный range (если где-то используешь)
function initSinglePriceRange() {
    const r = document.getElementById('priceMax');
    const out = document.getElementById('priceMaxVal');
    if (!r || !out) return;

    const sync = () => {
        out.textContent = '$' + r.value;
        paintRange(r);
    };

    r.addEventListener('input', sync);
    sync();
}ф

function paintRange(input) {
    const min = +input.min || 0;
    const max = +input.max || 100;
    const val = +input.value || 0;
    const pct = ((val - min) * 100) / (max - min);
    input.style.background =
        `linear-gradient(to right,var(--accent) ${pct}%, #e5e7eb ${pct}%)`;
}

// двухползунковый диапазон цены
function initPriceSlider() {
    const wrap = document.getElementById('priceFilter');
    if (!wrap) return;

    const minI = wrap.querySelector('#priceMin');
    const maxI = wrap.querySelector('#priceMax');
    const fill = wrap.querySelector('.range-fill');
    const outMin = wrap.querySelector('#minOut');
    const outMax = wrap.querySelector('#maxOut');

    if (!minI || !maxI || !fill || !outMin || !outMax) return;

    const MIN = +minI.min;
    const MAX = +minI.max;
    const GAP = 1;

    function clamp() {
        if (+maxI.value - +minI.value < GAP) {
            if (document.activeElement === minI) {
                minI.value = +maxI.value - GAP;
            } else {
                maxI.value = +minI.value + GAP;
            }
        }
    }

    function paint() {
        const a = (+minI.value - MIN) / (MAX - MIN) * 100;
        const b = (+maxI.value - MIN) / (MAX - MIN) * 100;
        fill.style.left = a + '%';
        fill.style.width = (b - a) + '%';

        outMin.textContent = '$' + (+minI.value).toLocaleString();
        outMax.textContent = '$' + (+maxI.value).toLocaleString();

        // чтобы пальцы не конфликтовали
        minI.style.zIndex =
            (parseInt(minI.value, 10) > MAX - 50) ? 5 : 3;
    }

    ['input', 'change'].forEach(ev => {
        minI.addEventListener(ev, () => {
            clamp();
            paint();
        });
        maxI.addEventListener(ev, () => {
            clamp();
            paint();
        });
    });

    paint();

    // AJAX загрузка отзывов
    async function loadReviews(tourId) {
        const response = await fetch(`/api/reviews/${tourId}`);
        const reviews = await response.json();
        renderReviews(reviews);
    }

// Валидация формы бронирования
    function validateBooking(form) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(form.email.value)) {
            showError('Please enter a valid email');
            return false;
        }
        return true;
    }
}
