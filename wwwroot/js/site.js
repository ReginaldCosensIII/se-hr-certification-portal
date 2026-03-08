// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Native Fullscreen API Interceptor for Header Toggle (State-Synced with F11 Override)
document.addEventListener('DOMContentLoaded', function () {
    const fullscreenBtn = document.querySelector('[data-lte-toggle="fullscreen"]');

    if (fullscreenBtn) {
        // 1. Unified Toggle Logic
        const toggleFullscreen = function () {
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen().catch(err => {
                    console.error(`Error attempting to enable fullscreen: ${err.message}`);
                });
            } else {
                if (document.exitFullscreen) {
                    document.exitFullscreen();
                }
            }
        };

        // 2. Handle Button Click
        fullscreenBtn.addEventListener('click', function (e) {
            e.preventDefault();
            toggleFullscreen();
        });

        // 3. Intercept F11 Keyboard Shortcut
        document.addEventListener('keydown', function (e) {
            if (e.key === 'F11') {
                e.preventDefault(); // Stop native browser UI fullscreen
                toggleFullscreen(); // Force HTML5 Fullscreen API instead
            }
        });

        // 4. Handle Icon Rendering (Listens for API changes and Esc key)
        document.addEventListener('fullscreenchange', function () {
            if (document.fullscreenElement) {
                fullscreenBtn.innerHTML = '<i data-lucide="minimize"></i>';
            } else {
                fullscreenBtn.innerHTML = '<i data-lucide="maximize"></i>';
            }

            // Instruct Lucide to immediately re-draw the SVG
            if (typeof lucide !== 'undefined') {
                lucide.createIcons({ root: fullscreenBtn });
            }
        });
    }
});
