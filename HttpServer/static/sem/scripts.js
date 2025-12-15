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

    initFilterSections();
    initPriceSlider();
    initSinglePriceRange();
});


function initFilterSections() {
    document.querySelectorAll('.aside .fg .fh').forEach(h => {
        h.addEventListener('click', () => {
            h.parentElement.classList.toggle('collapsed');
        });
    });
}

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
}

function paintRange(input) {
    const min = +input.min || 0;
    const max = +input.max || 100;
    const val = +input.value || 0;
    const pct = ((val - min) * 100) / (max - min);
    input.style.background =
        `linear-gradient(to right,var(--accent) ${pct}%, #e5e7eb ${pct}%)`;
}

function initPriceSlider() {
    const wrap = document.getElementById('priceFilter');
    if (!wrap) return;

    const minI = wrap.querySelector('#priceMin');
    const maxI = wrap.querySelector('#priceMax');
    const fill = wrap.querySelector('.range-fill');
    const outMin = wrap.querySelector('#minOut');
    const outMax = wrap.querySelector('#maxOut');

    if (!minI || !maxI || !fill || !outMin || !outMax) return;

    const num = (x) => parseFloat(String(x).replace(',', '.'));

    const MIN = num(minI.min);
    const MAX = num(minI.max);
    const GAP = 1;

    function clamp() {
        const a = num(minI.value);
        const b = num(maxI.value);
        if (isNaN(a) || isNaN(b)) return;

        if (b - a < GAP) {
            if (document.activeElement === minI) {
                minI.value = String(b - GAP);
            } else {
                maxI.value = String(a + GAP);
            }
        }
    }

    function paint() {
        const aV = num(minI.value);
        const bV = num(maxI.value);

        if (isNaN(MIN) || isNaN(MAX) || isNaN(aV) || isNaN(bV) || (MAX - MIN) <= 0) {
            outMin.textContent = '$' + (minI.value ?? '');
            outMax.textContent = '$' + (maxI.value ?? '');
            return;
        }

        const a = ((aV - MIN) * 100) / (MAX - MIN);
        const b = ((bV - MIN) * 100) / (MAX - MIN);

        fill.style.left = a + '%';
        fill.style.width = (b - a) + '%';

        outMin.textContent = '$' + Math.round(aV).toLocaleString();
        outMax.textContent = '$' + Math.round(bV).toLocaleString();

        minI.style.zIndex = (aV > MAX - 50) ? 5 : 3;
    }

    ['input', 'change'].forEach(ev => {
        minI.addEventListener(ev, () => { clamp(); paint(); });
        maxI.addEventListener(ev, () => { clamp(); paint(); });
    });

    paint();
}


