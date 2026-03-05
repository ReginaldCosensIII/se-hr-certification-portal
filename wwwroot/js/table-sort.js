document.addEventListener('DOMContentLoaded', function () {
    const tables = document.querySelectorAll('table');

    tables.forEach(table => {
        const tbody = table.querySelector('tbody');
        if (!tbody) return;

        // Snapshot the original server-rendered row order for the "Reset" state
        const originalRows = Array.from(tbody.querySelectorAll('tr'));
        const headers = table.querySelectorAll('th.sortable');

        headers.forEach(th => {
            th.style.cursor = 'pointer';
            th.title = "Click to sort";

            // Apply default sorting icon if the attribute is set by the server
            if (th.hasAttribute('data-sort-default')) {
                const defaultState = th.getAttribute('data-sort-default');
                th.dataset.sort = defaultState;
                const iconHtml = `<i data-lucide="${defaultState === 'asc' ? 'arrow-up' : 'arrow-down'}" class="ms-1 sort-icon text-danger" style="width: 14px; height: 14px;"></i>`;
                th.insertAdjacentHTML('beforeend', iconHtml);
                if (window.lucide) window.lucide.createIcons();
            }

            th.addEventListener('click', () => {
                const index = Array.from(th.parentNode.children).indexOf(th);

                // Determine next state: Unsorted -> Asc -> Desc -> Reset
                let nextState = 'asc';
                if (th.dataset.sort === 'asc') nextState = 'desc';
                else if (th.dataset.sort === 'desc') nextState = 'reset';

                // Clear all sort states and remove injected icons from all headers
                headers.forEach(h => {
                    h.dataset.sort = '';
                    const icon = h.querySelector('.sort-icon');
                    if (icon) icon.remove();
                });

                // Handle Reset State
                if (nextState === 'reset') {
                    originalRows.forEach(row => tbody.appendChild(row));
                    return;
                }

                // Apply new state and dynamically inject the active icon
                th.dataset.sort = nextState;
                const iconHtml = `<i data-lucide="${nextState === 'asc' ? 'arrow-up' : 'arrow-down'}" class="ms-1 sort-icon text-danger" style="width: 14px; height: 14px;"></i>`;
                th.insertAdjacentHTML('beforeend', iconHtml);
                if (window.lucide) window.lucide.createIcons();

                // Sort rows
                const direction = nextState === 'asc' ? 1 : -1;
                const rowsToSort = Array.from(tbody.querySelectorAll('tr'));

                rowsToSort.sort((a, b) => {
                    const aText = a.children[index].textContent.trim();
                    const bText = b.children[index].textContent.trim();

                    // Date Sort
                    const aDate = Date.parse(aText);
                    const bDate = Date.parse(bText);
                    if (!isNaN(aDate) && !isNaN(bDate) && isNaN(aText) && isNaN(bText)) return (aDate - bDate) * direction;

                    // Numeric/Currency Sort
                    const aNum = parseFloat(aText.replace(/[^0-9.-]+/g, ""));
                    const bNum = parseFloat(bText.replace(/[^0-9.-]+/g, ""));
                    if (!isNaN(aNum) && !isNaN(bNum) && aText.match(/^[-+]?[0-9.,$]+$/) && bText.match(/^[-+]?[0-9.,$]+$/)) {
                        return (aNum - bNum) * direction;
                    }

                    // String Sort
                    return aText.localeCompare(bText) * direction;
                });

                rowsToSort.forEach(row => tbody.appendChild(row));
            });
        });
    });
});
