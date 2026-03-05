document.addEventListener('DOMContentLoaded', function () {
    const headers = document.querySelectorAll('th.sortable');

    // 1. Reusable DOM Hydration for Default Sort
    const applyDefaultSortVisuals = () => {
        headers.forEach(th => {
            // Apply cursors
            th.style.cursor = 'pointer';
            th.title = "Click to sort";

            // Reset dataset and icons
            th.dataset.sort = '';
            const icon = th.querySelector('.sort-icon');
            if (icon) icon.remove();

            // Hydrate from data-sort-default
            if (th.hasAttribute('data-sort-default') && th.getAttribute('data-sort-default') !== '') {
                const defaultDir = th.getAttribute('data-sort-default');
                th.dataset.sort = defaultDir;
                const iconHtml = `<i data-lucide="${defaultDir === 'asc' ? 'arrow-up' : 'arrow-down'}" class="ms-1 sort-icon text-primary" style="width: 14px; height: 14px;"></i>`;
                th.insertAdjacentHTML('beforeend', iconHtml);
            }
        });
        if (window.lucide) window.lucide.createIcons();
    };

    // Run hydration immediately
    applyDefaultSortVisuals();

    // 2. Click Event Listeners
    headers.forEach(th => {
        th.addEventListener('click', () => {
            const column = th.getAttribute('data-column');
            if (!column) return; // Prevent hanging: abort if no mapping exists

            // Show overlay safely using a class instead of an ID for reusability
            const tableContainer = th.closest('.position-relative');
            if (tableContainer) {
                const overlay = tableContainer.querySelector('.table-loading-overlay');
                if (overlay) overlay.classList.remove('d-none');
            }

            // Determine next state: Unsorted -> asc -> desc -> reset
            let nextDir = 'asc';
            if (th.dataset.sort === 'asc') nextDir = 'desc';
            else if (th.dataset.sort === 'desc') nextDir = '';

            // Build URL
            const urlParams = new URLSearchParams(window.location.search);
            if (nextDir === '') {
                urlParams.delete('sortOrder');
            } else {
                urlParams.set('sortOrder', `${column}_${nextDir}`);
            }

            urlParams.set('p', '1'); // Reset pagination
            window.location.href = window.location.pathname + '?' + urlParams.toString();
        });
    });
});
