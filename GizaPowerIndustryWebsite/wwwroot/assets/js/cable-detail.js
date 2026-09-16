// Cable Detail Page - Tab Functionality

document.addEventListener('DOMContentLoaded', function () {
    // Tab switching functionality
    const tabs = document.querySelectorAll('.cable-detail-tab');
    const tabPanels = document.querySelectorAll('.cable-detail-tab-panel');

    tabs.forEach(tab => {
        tab.addEventListener('click', function () {
            const targetTab = this.getAttribute('data-tab');

            // Remove active class from all tabs
            tabs.forEach(t => t.classList.remove('active'));

            // Remove active class from all panels
            tabPanels.forEach(panel => panel.classList.remove('active'));

            // Add active class to clicked tab
            this.classList.add('active');

            // Add active class to corresponding panel
            const targetPanel = document.getElementById(targetTab);
            if (targetPanel) {
                targetPanel.classList.add('active');
            }
        });
    });

    // Set default active tab (Description)
    if (tabs.length > 0 && tabPanels.length > 0) {
        tabs[0].classList.add('active');
        tabPanels[0].classList.add('active');
    }
});
