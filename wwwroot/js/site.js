// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Native Fullscreen API Interceptor for Header Toggle
document.addEventListener('DOMContentLoaded', function () {
    const fullscreenBtn = document.querySelector('[data-widget="fullscreen"]');
    if (fullscreenBtn) {
        fullscreenBtn.addEventListener('click', function (e) {
            e.preventDefault();
            const icon = this.querySelector('i');
            
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen().catch(err => {
                    console.error(`Error attempting to enable fullscreen: ${err.message}`);
                });
                if (icon) {
                    icon.classList.remove('fa-expand-arrows-alt');
                    icon.classList.add('fa-compress-arrows-alt');
                }
            } else {
                if (document.exitFullscreen) {
                    document.exitFullscreen();
                    if (icon) {
                        icon.classList.remove('fa-compress-arrows-alt');
                        icon.classList.add('fa-expand-arrows-alt');
                    }
                }
            }
        });
    }
});
