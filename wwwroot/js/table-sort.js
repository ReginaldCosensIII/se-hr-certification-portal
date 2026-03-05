document.addEventListener('DOMContentLoaded', function () {
    const headers = document.querySelectorAll('th.sortable');
    const overlay = document.getElementById('tableLoadingOverlay');

    headers.forEach(th => {
        th.addEventListener('click', () => {
            // 1. Show the loading overlay instantly
            if (overlay) {
                overlay.classList.remove('d-none');
            }

            // 2. Determine the column name from a new data-column attribute
            const column = th.getAttribute('data-column');
            if (!column) return;

            // 3. Determine next state: Unsorted/Default -> asc -> desc -> reset
            let nextDir = 'asc';
            if (th.dataset.sort === 'asc') nextDir = 'desc';
            else if (th.dataset.sort === 'desc') nextDir = ''; // Empty string means reset

            // 4. Build the new URL
            const urlParams = new URLSearchParams(window.location.search);

            if (nextDir === '') {
                urlParams.delete('sortOrder');
            } else {
                urlParams.set('sortOrder', `${column}_${nextDir}`);
            }

            // Always reset to page 1 when sorting changes
            urlParams.set('p', '1');

            // 5. Fire the request to the server
            window.location.href = window.location.pathname + '?' + urlParams.toString();
        });
    });
});
