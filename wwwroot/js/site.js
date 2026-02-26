// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Native Fullscreen API Interceptor for Header Toggle (Lucide Icon Compatible)
document.addEventListener('DOMContentLoaded', function () {
    const fullscreenBtn = document.querySelector('[data-lte-toggle="fullscreen"]');

    if (fullscreenBtn) {
        fullscreenBtn.addEventListener('click', function (e) {
            e.preventDefault();

            if (!document.fullscreenElement) {
                // Enter Fullscreen
                document.documentElement.requestFullscreen().catch(err => {
                    console.error(`Error attempting to enable fullscreen: ${err.message}`);
                });
                // Swap to minimize icon
                fullscreenBtn.innerHTML = '<i data-lucide="minimize"></i>';
            } else {
                // Exit Fullscreen
                if (document.exitFullscreen) {
                    document.exitFullscreen();
                    // Swap back to maximize icon
                    fullscreenBtn.innerHTML = '<i data-lucide="maximize"></i>';
                }
            }

            // Instruct Lucide to immediately re-draw the SVG inside the button
            if (typeof lucide !== 'undefined') {
                lucide.createIcons({ root: fullscreenBtn });
            }
        });
    }
});
