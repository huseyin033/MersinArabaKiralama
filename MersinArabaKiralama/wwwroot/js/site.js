// Navbar scroll efekti
document.addEventListener('DOMContentLoaded', function() {
    const navbar = document.querySelector('.navbar');
    
    // Sayfa yüklendiğinde kontrol et
    if (window.scrollY > 50) {
        navbar.classList.add('scrolled');
    }
    
    // Scroll olayını dinle
    window.addEventListener('scroll', function() {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });
});
