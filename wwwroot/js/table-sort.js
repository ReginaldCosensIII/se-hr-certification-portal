document.addEventListener('DOMContentLoaded', function () {
    const headers = document.querySelectorAll('th.sortable');
    headers.forEach(th => {
        th.style.cursor = 'pointer';
        th.title = "Click to sort";
        
        th.addEventListener('click', () => {
            const table = th.closest('table');
            const tbody = table.querySelector('tbody');
            const rows = Array.from(tbody.querySelectorAll('tr'));
            const index = Array.from(th.parentNode.children).indexOf(th);
            const isAscending = th.classList.contains('asc');
            const direction = isAscending ? -1 : 1;

            rows.sort((a, b) => {
                const aText = a.children[index].textContent.trim();
                const bText = b.children[index].textContent.trim();

                // Date check
                const aDate = Date.parse(aText);
                const bDate = Date.parse(bText);
                if (!isNaN(aDate) && !isNaN(bDate)) return (aDate - bDate) * direction;

                // Numeric check
                const aNum = parseFloat(aText.replace(/[^0-9.-]+/g,""));
                const bNum = parseFloat(bText.replace(/[^0-9.-]+/g,""));
                if (!isNaN(aNum) && !isNaN(bNum) && aText.match(/^[0-9.,$-]+$/) && bText.match(/^[0-9.,$-]+$/)) {
                    return (aNum - bNum) * direction;
                }

                // String fallback
                return aText.localeCompare(bText) * direction;
            });

            // Reset all icons and classes
            headers.forEach(h => {
                h.classList.remove('asc', 'desc');
                const icon = h.querySelector('.sort-icon');
                if (icon) icon.setAttribute('data-lucide', 'arrow-up-down');
            });

            // Set active sort icon and class
            th.classList.add(isAscending ? 'desc' : 'asc');
            const activeIcon = th.querySelector('.sort-icon');
            if (activeIcon) activeIcon.setAttribute('data-lucide', isAscending ? 'arrow-down' : 'arrow-up');
            if (window.lucide) window.lucide.createIcons();

            // Re-append rows to DOM
            rows.forEach(row => tbody.appendChild(row));
        });
    });
});
