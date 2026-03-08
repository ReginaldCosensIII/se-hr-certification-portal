// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Persistent Theater Mode (Replaces Native Fullscreen API)
document.addEventListener('DOMContentLoaded', function () {
    const fullscreenBtn = document.querySelector('[data-lte-toggle="fullscreen"]');

    if (fullscreenBtn) {
        const toggleTheaterMode = function () {
            document.body.classList.toggle('theater-mode');
            const isTheater = document.body.classList.contains('theater-mode');
            localStorage.setItem('theater-mode', isTheater);
            updateIcon(isTheater);
        };

        const updateIcon = function (isTheater) {
            if (isTheater) {
                fullscreenBtn.innerHTML = '<i data-lucide="minimize"></i>';
            } else {
                fullscreenBtn.innerHTML = '<i data-lucide="maximize"></i>';
            }
            if (typeof lucide !== 'undefined') {
                lucide.createIcons({ root: fullscreenBtn });
            }
        };

        // Initialize icon state on load (body class is handled by Layout FOUC script)
        if (localStorage.getItem('theater-mode') === 'true') {
            updateIcon(true);
        }

        fullscreenBtn.addEventListener('click', function (e) {
            e.preventDefault();
            toggleTheaterMode();
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'F11') {
                e.preventDefault(); // Stop native browser UI fullscreen
                toggleTheaterMode(); // Force persistent CSS Theater Mode instead
            }
        });
    }
});
