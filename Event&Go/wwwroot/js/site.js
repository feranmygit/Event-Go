// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


let currentSlide = 0;
const slides = document.querySelectorAll(".image-slider img");
const dots = document.querySelectorAll(".dot");
let autoSlideInterval;

// Initialize the slider
function showSlide(index) {
    currentSlide = (index + slides.length) % slides.length; // Keep slides moving in one direction
    const offset = -currentSlide * (slides[0].clientWidth + 20); // Adjust for margin
    document.querySelector(".image-slider").style.transform = `translateX(${offset}px)`;

    // Update dots
    dots.forEach((dot, i) => {
        dot.classList.toggle("active", i === currentSlide);
    });
}

// Navigate to the next slide
function nextSlide() {
    showSlide(currentSlide + 1);
}

// Navigate to the previous slide
function prevSlide() {
    showSlide(currentSlide - 1);
}

// Go to a specific slide
function goToSlide(index) {
    showSlide(index);
}

// Auto-slide every 5 seconds
function startAutoSlide() {
    autoSlideInterval = setInterval(() => {
        nextSlide();
    }, 5000);
}

// Stop auto-slide on manual interaction
function stopAutoSlide() {
    clearInterval(autoSlideInterval);
}

// Initialize
showSlide(currentSlide);
startAutoSlide();

// Event Listeners for stopping auto-slide
document.querySelector(".image-slider-container").addEventListener("mouseover", stopAutoSlide);
document.querySelector(".image-slider-container").addEventListener("mouseout", startAutoSlide);





document.addEventListener('DOMContentLoaded', () => {
    const hamburgerMenu = document.querySelector('.hamburger-menu');
    const navbarLinks = document.querySelector('.navbar-redirect-links');

    hamburgerMenu.addEventListener('click', () => {
        navbarLinks.classList.toggle('active');
    });
});





function redirectToDetails(eventId) {
    window.location.href = `/eventstables/Details/${eventId}`;
}

















//alert("hello world");
//function redirectToDetails(eventId) {
//    window.location.href = `/eventstables/Details/${eventId}`;
//}

//let currentSlide = 0;
//const slides = document.querySelectorAll(".image-slider img");
//const dots = document.querySelectorAll(".dot");
//let autoSlideInterval;

//// Initialize the slider
//function showSlide(index) {
//    currentSlide = (index + slides.length) % slides.length; // Keep slides moving in one direction
//    const offset = -currentSlide * (slides[0].clientWidth + 20); // Adjust for margin
//    document.querySelector(".image-slider").style.transform = `translateX(${offset}px)`;

//    // Update dots
//    dots.forEach((dot, i) => {
//        dot.classList.toggle("active", i === currentSlide);
//    });
//}

//// Navigate to the next slide
//function nextSlide() {
//    showSlide(currentSlide + 1);
//}

//// Navigate to the previous slide
//function prevSlide() {
//    showSlide(currentSlide - 1);
//}

//// Go to a specific slide
//function goToSlide(index) {
//    showSlide(index);
//}

//// Auto-slide every 5 seconds
//function startAutoSlide() {
//    autoSlideInterval = setInterval(() => {
//        nextSlide();
//    }, 5000);
//}

//// Stop auto-slide on manual interaction
//function stopAutoSlide() {
//    clearInterval(autoSlideInterval);
//}

//// Initialize
//showSlide(currentSlide);
//startAutoSlide();

//// Event Listeners for stopping auto-slide
//document.querySelector(".image-slider-container").addEventListener("mouseover", stopAutoSlide);
//document.querySelector(".image-slider-container").addEventListener("mouseout", startAutoSlide);


//// Home page here

//document.addEventListener('DOMContentLoaded', () => {
//    const phrases = ["Plan Your Events", "Connect with Guests", "Simplify Scheduling", "Make Your Events Memorable"];
//    let phraseIndex = 0;
//    const animatedText = document.getElementById("animated-text");

//    setInterval(() => {
//        phraseIndex = (phraseIndex + 1) % phrases.length;
//        animatedText.textContent = phrases[phraseIndex];
//    }, 3000); // Changes text every 3 seconds
//});
