// Theme toggle: persists light/dark mode in localStorage.
document.addEventListener("DOMContentLoaded", () => {
    const body = document.body;
    const toggleBtn = document.getElementById("theme-toggle");

    // dark theme
    let savedTheme = localStorage.getItem("theme");

    if (savedTheme === "light") {
        body.classList.add("light-mode");
    } else {
       
        body.classList.remove("light-mode");
        savedTheme = "dark";
        localStorage.setItem("theme", "dark");
    }

    updateButton();

    toggleBtn.addEventListener("click", () => {
        body.classList.toggle("light-mode");

        const isLight = body.classList.contains("light-mode");
        localStorage.setItem("theme", isLight ? "light" : "dark");

        updateButton();
    });

    function updateButton() {
        const isLight = body.classList.contains("light-mode");
        toggleBtn.textContent = isLight ? "🌙 Dark Mode" : "☀️ Light Mode";
    }
});

// Loading screen: ensures a minimum visible time to avoid flicker on fast loads.

window.addEventListener("load", () => {
    const loaderScreen = document.getElementById("loading-screen");

    const MIN_TIME = 800; // Minimum loader display duration (ms).

    const start = performance.now();

    const finalize = () => {
        const elapsed = performance.now() - start;
        const remaining = MIN_TIME - elapsed;

        setTimeout(() => {
            loaderScreen.classList.add("hide");

            setTimeout(() => loaderScreen.remove(), 600);
        }, remaining > 0 ? remaining : 0);
    };

    finalize();
});


