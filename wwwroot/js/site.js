// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Native Fullscreen API Interceptor for Header Toggle (State-Synced)
document.addEventListener('DOMContentLoaded', function () {
    const fullscreenBtn = document.querySelector('[data-lte-toggle="fullscreen"]');

    if (fullscreenBtn) {
        // 1. Handle the Click Action
        fullscreenBtn.addEventListener('click', function (e) {
            e.preventDefault();
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen().catch(err => {
                    console.error(`Error attempting to enable fullscreen: ${err.message}`);
                });
            } else {
                if (document.exitFullscreen) {
                    document.exitFullscreen();
                }
            }
        });

        // 2. Handle the Icon Rendering (Listens for Button Clicks, F11, and Esc keys)
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
