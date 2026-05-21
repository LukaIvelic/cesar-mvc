(function () {
    function debounce(callback, delay) {
        let timer;
        return function () {
            window.clearTimeout(timer);
            timer = window.setTimeout(() => callback.apply(this, arguments), delay);
        };
    }

    function animateRows(target) {
        target.querySelectorAll('tr').forEach((row, index) => {
            row.classList.add('ajax-row-enter');
            row.style.animationDelay = `${Math.min(index * 25, 180)}ms`;
        });
    }

    function initAjaxSearch() {
        document.querySelectorAll('[data-ajax-search]').forEach(input => {
            const targetSelector = input.getAttribute('data-ajax-target');
            const url = input.getAttribute('data-ajax-url');
            const target = targetSelector ? document.querySelector(targetSelector) : null;

            if (!target || !url) return;

            const runSearch = debounce(async function () {
                const requestUrl = new URL(url, window.location.origin);
                requestUrl.searchParams.set('q', input.value);
                input.classList.add('ajax-loading');

                try {
                    const response = await fetch(requestUrl, {
                        headers: { 'X-Requested-With': 'XMLHttpRequest' }
                    });

                    if (!response.ok) return;

                    target.innerHTML = await response.text();
                    animateRows(target);
                } finally {
                    input.classList.remove('ajax-loading');
                }
            }, 220);

            input.addEventListener('input', runSearch);
        });
    }

    function initAutocomplete() {
        document.querySelectorAll('[data-autocomplete]').forEach(root => {
            const url = root.getAttribute('data-autocomplete-url');
            const requiredMessage = root.getAttribute('data-required-message') || 'Selection is required.';
            const hidden = root.querySelector('[data-autocomplete-value]');
            const input = root.querySelector('[data-autocomplete-input]');
            const menu = root.querySelector('[data-autocomplete-menu]');
            const error = root.querySelector('[data-autocomplete-error]');

            if (!url || !hidden || !input || !menu) return;

            function showError(message) {
                if (!error) return;
                error.textContent = message || '';
                input.classList.toggle('border-red-500', Boolean(message));
            }

            function closeMenu() {
                menu.classList.add('hidden');
                menu.innerHTML = '';
            }

            function renderResults(items) {
                menu.innerHTML = '';

                if (!items.length) {
                    const empty = document.createElement('div');
                    empty.className = 'px-3 py-2 text-sm text-neutral-500';
                    empty.textContent = 'No results';
                    menu.appendChild(empty);
                    menu.classList.remove('hidden');
                    return;
                }

                items.forEach(item => {
                    const option = document.createElement('button');
                    option.type = 'button';
                    option.className = 'block w-full text-left px-3 py-2 text-sm hover:bg-neutral-100';
                    option.textContent = item.text;
                    option.addEventListener('click', () => {
                        hidden.value = item.id;
                        input.value = item.text;
                        showError('');
                        hidden.dispatchEvent(new Event('change', { bubbles: true }));
                        closeMenu();
                    });
                    menu.appendChild(option);
                });

                menu.classList.remove('hidden');
            }

            const search = debounce(async function () {
                const term = input.value.trim();
                hidden.value = '';

                if (term.length < 1) {
                    closeMenu();
                    return;
                }

                const requestUrl = new URL(url, window.location.origin);
                requestUrl.searchParams.set('term', term);

                const response = await fetch(requestUrl, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) return;

                renderResults(await response.json());
            }, 180);

            input.addEventListener('input', search);
            input.addEventListener('blur', () => {
                window.setTimeout(() => {
                    if (!hidden.value) {
                        showError(requiredMessage);
                    }
                    closeMenu();
                }, 150);
            });

            input.closest('form')?.addEventListener('submit', event => {
                if (!hidden.value) {
                    event.preventDefault();
                    showError(requiredMessage);
                    input.focus();
                }
            });
        });
    }

    function initValidationOnBlur() {
        if (!window.jQuery || !window.jQuery.validator || !window.jQuery.validator.unobtrusive) {
            return;
        }

        window.jQuery('form').each(function () {
            window.jQuery.validator.unobtrusive.parse(this);
        });

        window.jQuery(document).on('blur change', 'input, textarea, select', function () {
            const form = window.jQuery(this).closest('form');
            if (form.data('validator')) {
                window.jQuery(this).valid();
            }
        });
    }

    function initDateTimePickers() {
        const culture = (navigator.language || 'en').toLowerCase();
        const isCroatian = culture.startsWith('hr');
        const monthFormatter = new Intl.DateTimeFormat(isCroatian ? 'hr-HR' : 'en-US', {
            month: 'long',
            year: 'numeric'
        });

        function pad(value) {
            return String(value).padStart(2, '0');
        }

        function formatDate(date) {
            const day = pad(date.getDate());
            const month = pad(date.getMonth() + 1);
            const year = date.getFullYear();
            const hour = pad(date.getHours());
            const minute = pad(date.getMinutes());

            return isCroatian
                ? `${day}.${month}.${year}. ${hour}:${minute}`
                : `${month}/${day}/${year} ${hour}:${minute}`;
        }

        function parseDate(value) {
            const text = (value || '').trim();
            const hrMatch = text.match(/^(\d{1,2})\.(\d{1,2})\.(\d{4})\.?\s+(\d{1,2}):(\d{2})$/);
            if (hrMatch) {
                return new Date(+hrMatch[3], +hrMatch[2] - 1, +hrMatch[1], +hrMatch[4], +hrMatch[5]);
            }

            const enMatch = text.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})\s+(\d{1,2}):(\d{2})$/);
            if (enMatch) {
                return new Date(+enMatch[3], +enMatch[1] - 1, +enMatch[2], +enMatch[4], +enMatch[5]);
            }

            const parsed = new Date(text);
            return Number.isNaN(parsed.getTime()) ? new Date() : parsed;
        }

        document.querySelectorAll('[data-date-time-control]').forEach(root => {
            const input = root.querySelector('[data-date-time-input]');
            const toggle = root.querySelector('[data-date-time-toggle]');
            const panel = root.querySelector('[data-date-time-panel]');
            const title = root.querySelector('[data-date-time-title]');
            const days = root.querySelector('[data-date-time-days]');
            const prev = root.querySelector('[data-date-time-prev]');
            const next = root.querySelector('[data-date-time-next]');
            const hour = root.querySelector('[data-date-time-hour]');
            const minute = root.querySelector('[data-date-time-minute]');
            const now = root.querySelector('[data-date-time-now]');

            if (!input || !toggle || !panel || !title || !days || !hour || !minute) return;

            let selected = parseDate(input.value);
            let visibleMonth = new Date(selected.getFullYear(), selected.getMonth(), 1);
            input.value = formatDate(selected);

            function syncTimeInputs() {
                hour.value = selected.getHours();
                minute.value = pad(selected.getMinutes());
            }

            function syncInput() {
                input.value = formatDate(selected);
                input.dispatchEvent(new Event('input', { bubbles: true }));
                input.dispatchEvent(new Event('change', { bubbles: true }));
            }

            function renderCalendar() {
                syncTimeInputs();
                title.textContent = monthFormatter.format(visibleMonth);
                days.innerHTML = '';

                const firstDay = new Date(visibleMonth.getFullYear(), visibleMonth.getMonth(), 1);
                const startOffset = (firstDay.getDay() + 6) % 7;
                const start = new Date(firstDay);
                start.setDate(firstDay.getDate() - startOffset);

                for (let i = 0; i < 42; i++) {
                    const current = new Date(start);
                    current.setDate(start.getDate() + i);

                    const button = document.createElement('button');
                    button.type = 'button';
                    button.className = 'text-sm rounded py-1 hover:bg-neutral-100';
                    button.textContent = current.getDate();

                    if (current.getMonth() !== visibleMonth.getMonth()) {
                        button.classList.add('text-neutral-300');
                    }

                    if (
                        current.getFullYear() === selected.getFullYear()
                        && current.getMonth() === selected.getMonth()
                        && current.getDate() === selected.getDate()
                    ) {
                        button.classList.add('bg-black', 'text-white', 'hover:bg-black');
                    }

                    button.addEventListener('click', () => {
                        selected = new Date(
                            current.getFullYear(),
                            current.getMonth(),
                            current.getDate(),
                            Number(hour.value || selected.getHours()),
                            Number(minute.value || selected.getMinutes()));
                        visibleMonth = new Date(selected.getFullYear(), selected.getMonth(), 1);
                        syncInput();
                        renderCalendar();
                    });

                    days.appendChild(button);
                }
            }

            function openPanel() {
                selected = parseDate(input.value);
                visibleMonth = new Date(selected.getFullYear(), selected.getMonth(), 1);
                panel.classList.remove('hidden');
                renderCalendar();
            }

            toggle.addEventListener('click', () => {
                if (panel.classList.contains('hidden')) {
                    openPanel();
                } else {
                    panel.classList.add('hidden');
                }
            });

            prev?.addEventListener('click', () => {
                visibleMonth = new Date(visibleMonth.getFullYear(), visibleMonth.getMonth() - 1, 1);
                renderCalendar();
            });

            next?.addEventListener('click', () => {
                visibleMonth = new Date(visibleMonth.getFullYear(), visibleMonth.getMonth() + 1, 1);
                renderCalendar();
            });

            [hour, minute].forEach(timeInput => {
                timeInput.addEventListener('change', () => {
                    selected.setHours(Math.max(0, Math.min(23, Number(hour.value || 0))));
                    selected.setMinutes(Math.max(0, Math.min(59, Number(minute.value || 0))));
                    syncInput();
                    renderCalendar();
                });
            });

            now?.addEventListener('click', () => {
                selected = new Date();
                visibleMonth = new Date(selected.getFullYear(), selected.getMonth(), 1);
                syncInput();
                renderCalendar();
            });

            input.addEventListener('blur', () => {
                selected = parseDate(input.value);
                input.value = formatDate(selected);
            });

            document.addEventListener('click', event => {
                if (!root.contains(event.target)) {
                    panel.classList.add('hidden');
                }
            });
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        initAjaxSearch();
        initAutocomplete();
        initDateTimePickers();
        initValidationOnBlur();
    });
})();
