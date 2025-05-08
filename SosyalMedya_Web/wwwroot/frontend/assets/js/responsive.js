// Responsive behavior for SosyalMedya application
document.addEventListener('DOMContentLoaded', function() {
    // Responsive navigation menu for mobile devices
    const handleResponsiveMenu = () => {
        const windowWidth = window.innerWidth;
        const leftSideMenu = document.getElementById('leftSideMenu');
        
        if (leftSideMenu) {
            if (windowWidth < 768) {
                leftSideMenu.classList.add('mobile-menu');
            } else {
                leftSideMenu.classList.remove('mobile-menu');
            }
        }
    };

    // Initial call
    handleResponsiveMenu();
    
    // Listen for window resize
    window.addEventListener('resize', handleResponsiveMenu);
    
    // Handle responsive tables
    const tables = document.querySelectorAll('table');
    tables.forEach(table => {
        table.classList.add('table-responsive');
    });
    
    // Handle responsive images
    const images = document.querySelectorAll('img:not(.rounded-circle)');
    images.forEach(img => {
        img.classList.add('img-fluid');
    });
}); 