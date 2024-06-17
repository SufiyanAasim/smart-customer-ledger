// Smart Customer Ledger — Hyper-Interactive Client Scripts

document.addEventListener('DOMContentLoaded', () => {
    initializeThemeToggle();
    initializeQuickSearch();
    initializeStatusFilters();
    animateStatCounters();
    initializeCopyButtons();
});

/**
 * Handles Dark/Light Theme Switching with localStorage persistence
 */
function initializeThemeToggle() {
    const toggleBtn = document.getElementById('themeToggleBtn');
    const savedTheme = localStorage.getItem('theme-preference') || (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');

    applyTheme(savedTheme);

    if (toggleBtn) {
        toggleBtn.addEventListener('click', () => {
            const currentTheme = document.documentElement.getAttribute('data-bs-theme') || 'light';
            const nextTheme = currentTheme === 'dark' ? 'light' : 'dark';
            applyTheme(nextTheme);
            localStorage.setItem('theme-preference', nextTheme);
        });
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        document.body.setAttribute('data-bs-theme', theme);
        if (toggleBtn) {
            toggleBtn.innerHTML = theme === 'dark' ? '☀️' : '🌙';
            toggleBtn.title = theme === 'dark' ? 'Switch to Light Theme' : 'Switch to Dark Theme';
        }
    }
}

/**
 * Real-time live quick filter for tables
 */
function initializeQuickSearch() {
    const searchInputs = document.querySelectorAll('.cl-table-search');
    searchInputs.forEach(input => {
        const targetTableId = input.getAttribute('data-table-target');
        const table = targetTableId ? document.getElementById(targetTableId) : document.querySelector('.cl-table');
        if (!table) return;

        input.addEventListener('keyup', (e) => {
            const term = e.target.value.toLowerCase().trim();
            const rows = table.querySelectorAll('tbody tr');

            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                if (text.includes(term)) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        });
    });
}

/**
 * Status filter buttons
 */
function initializeStatusFilters() {
    const filterButtons = document.querySelectorAll('.cl-status-filter-btn');
    filterButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            filterButtons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            const status = btn.getAttribute('data-status-filter')?.toLowerCase() || 'all';
            const rows = document.querySelectorAll('.cl-table tbody tr');

            rows.forEach(row => {
                if (status === 'all') {
                    row.style.display = '';
                    return;
                }
                const badge = row.querySelector('.badge');
                if (!badge) {
                    row.style.display = '';
                    return;
                }
                const badgeText = badge.textContent.toLowerCase().trim();
                if (badgeText === status || badgeText.includes(status)) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        });
    });
}

/**
 * Animated stat counter
 */
function animateStatCounters() {
    const counters = document.querySelectorAll('.cl-counter');
    counters.forEach(counter => {
        const target = parseFloat(counter.getAttribute('data-target') || counter.textContent.replace(/[^0-9.-]+/g, ""));
        if (isNaN(target) || target <= 0) return;

        const isCurrency = counter.textContent.includes('$') || counter.textContent.includes('Rs') || counter.hasAttribute('data-currency');
        const duration = 1000;
        const start = 0;
        const startTime = performance.now();

        function updateCounter(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            const currentVal = start + (target - start) * progress;

            if (isCurrency) {
                counter.textContent = currentVal.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            } else {
                counter.textContent = Math.floor(currentVal).toLocaleString();
            }

            if (progress < 1) {
                requestAnimationFrame(updateCounter);
            }
        }

        requestAnimationFrame(updateCounter);
    });
}

/**
 * Copy to clipboard helpers
 */
function initializeCopyButtons() {
    const copyBtns = document.querySelectorAll('.cl-btn-copy');
    copyBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const text = btn.getAttribute('data-copy-text') || btn.previousElementSibling?.textContent;
            if (!text) return;

            navigator.clipboard.writeText(text.trim()).then(() => {
                const originalHtml = btn.innerHTML;
                btn.innerHTML = '✓ Copied!';
                btn.classList.add('btn-success');
                setTimeout(() => {
                    btn.innerHTML = originalHtml;
                    btn.classList.remove('btn-success');
                }, 1800);
            });
        });
    });
}
