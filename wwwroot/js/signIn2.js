
const signupForm = document.querySelector(".signup-form");
const signinForm = document.querySelector(".signin-form");

document.addEventListener("DOMContentLoaded", () => {

    const loginEmail = document.getElementById("login-email");
    const loginPassword = document.getElementById("login-password");
    // UX: disable password input until the user enters an email.
    if (loginEmail && loginPassword) {
   
        loginPassword.disabled = true;

        loginEmail.addEventListener("input", () => {
            if (loginEmail.value.trim() !== "") {
                loginPassword.disabled = false;
            } else {
                loginPassword.disabled = true;
                loginPassword.value = "";
            }
        });
    }


    // === SIGN UP PASSWORD LOCK ===
    const signupEmail = document.getElementById("register-email");
    const signupPass1 = document.getElementById("register-password");
    const signupPass2 = document.getElementById("confirm-password");

    if (signupEmail && signupPass1 && signupPass2) {
        signupPass1.disabled = true;
        signupPass2.disabled = true;

        signupEmail.addEventListener("input", () => {
            if (signupEmail.value.trim() !== "") {
                signupPass1.disabled = false;
                signupPass2.disabled = false;
            } else {
                signupPass1.disabled = true;
                signupPass2.disabled = true;

                signupPass1.value = "";
                signupPass2.value = "";
            }
        });
    }

    const passwordInput = document.getElementById("register-password");
    const confirmPasswordInput = document.getElementById("confirm-password");
    const phoneInput = document.getElementById("phone");
    const submitButton = document.querySelector(".signup-form .submit-btn");
    const signupForm = document.querySelector(".signup-form");

    // Robot validation part
    const robotQuestionEl = document.getElementById("robot-question");
    const robotAnswerInput = document.getElementById("robot-answer");
    const robotErrorEl = document.getElementById("robot-error");

    // Lightweight bot-check (math question) to reduce automated sign-ups.
    function generateRobotQuestion() {
        if (!robotQuestionEl || !robotAnswerInput) return;
        const a = Math.floor(Math.random() * 10) + 1;
        const b = Math.floor(Math.random() * 10) + 1;
        const ops = ["+", "-", "*"];
        const op = ops[Math.floor(Math.random() * ops.length)];

        let correct;
        switch (op) {
            case "+": correct = a + b; break;
            case "-": correct = a - b; break;
            case "*": correct = a * b; break;
        }

        robotQuestionEl.textContent = `${a} ${op} ${b} = ?`;
        signupForm.dataset.robotAnswer = String(correct);
        if (robotErrorEl) robotErrorEl.textContent = "";
        robotAnswerInput.value = "";
    }

   
    function shakeElement(el) {
        if (!el) return;
        const positions = [0, -4, 4, -4, 4, 0];
        let index = 0;
        const originalTransition = el.style.transition;
        el.style.transition = "transform 50ms";

        const interval = setInterval(() => {
            el.style.transform = `translateX(${positions[index]}px)`;
            index++;
            if (index >= positions.length) {
                clearInterval(interval);
                setTimeout(() => {
                    el.style.transform = "translateX(0)";
                    el.style.transition = originalTransition;
                }, 50);
            }
        }, 50);
    }
    // Validate the bot-check answer before allowing form submission.
    function validateRobot() {
        if (!robotAnswerInput || !signupForm) return true; 
        const correct = Number(signupForm.dataset.robotAnswer || NaN);
        const userAnswer = Number(String(robotAnswerInput.value).trim());

        if (Number.isNaN(userAnswer)) {
            if (robotErrorEl) robotErrorEl.textContent = "Please enter a number.";
            shakeElement(robotAnswerInput);
            robotAnswerInput.focus();
            return false;
        }

        if (userAnswer !== correct) {
            if (robotErrorEl) robotErrorEl.textContent = "Wrong answer. Please try again.";
            shakeElement(robotAnswerInput);
            robotAnswerInput.focus();
            return false;
        }

        if (robotErrorEl) robotErrorEl.textContent = "";
        return true;
    }

  
    if (signupForm) {
        generateRobotQuestion();
    }

    // Pressing Enter on the bot answer should trigger form submi
    if (robotAnswerInput && signupForm) {
        robotAnswerInput.addEventListener("keydown", (e) => {
            if (e.isComposing || e.keyCode === 229) return;
            if (e.key === "Enter") {
                e.preventDefault();
                const submitEvent = new Event("submit", { cancelable: true, bubbles: true });
                signupForm.dispatchEvent(submitEvent);
            }
        });
    }


    // Password rules: at least 8 chars, with both uppercase and lowercase letters.
    function checkPasswordStrength(password) {
        const hasUpper = /[A-Z]/.test(password);
        const hasLower = /[a-z]/.test(password);
        return password.length >= 8 && hasUpper && hasLower;
    }


    function checkPasswordMatch() {
        return passwordInput.value === confirmPasswordInput.value;
    }


    function validateForm() {
        const password = passwordInput.value;
        const confirmPassword = confirmPasswordInput.value;
        const isStrong = checkPasswordStrength(password);
        const passwordsMatch = checkPasswordMatch();

        
        let strengthWarning = document.getElementById("password-strength-warning");
        if (!strengthWarning) {
            strengthWarning = document.createElement("p");
            strengthWarning.id = "password-strength-warning";
            strengthWarning.style.fontSize = "0.9em";
            strengthWarning.style.marginTop = "4px";
            passwordInput.parentNode.appendChild(strengthWarning);
        }

        let matchWarning = document.getElementById("password-match-warning");
        if (!matchWarning) {
            matchWarning = document.createElement("p");
            matchWarning.id = "password-match-warning";
            matchWarning.style.fontSize = "0.9em";
            matchWarning.style.marginTop = "4px";
            confirmPasswordInput.parentNode.appendChild(matchWarning);
        }

        // Password strength message
        if (password.length === 0) {
            strengthWarning.textContent = "";
            passwordInput.style.borderColor = "";
        } else if (!isStrong) {
            strengthWarning.textContent = "❌ Password must be at least 8 characters and contain both uppercase and lowercase letters.";
            strengthWarning.style.color = "#ff6b6b";
            passwordInput.style.borderColor = "red";
        } else {
            strengthWarning.textContent = "✅ Strong password!";
            strengthWarning.style.color = "#51cf66";
            passwordInput.style.borderColor = "green";
        }

        // Password match message
        if (confirmPassword.length === 0) {
            matchWarning.textContent = "";
            confirmPasswordInput.style.borderColor = "";
        } else if (!passwordsMatch) {
            matchWarning.textContent = "❌ Passwords do not match!";
            matchWarning.style.color = "#ff6b6b";
            confirmPasswordInput.style.borderColor = "red";
        } else {
            matchWarning.textContent = "✅ Passwords match!";
            matchWarning.style.color = "#51cf66";
            confirmPasswordInput.style.borderColor = "green";
        }
    }

    // Re-check as the user types
    passwordInput.addEventListener("input", validateForm);
    confirmPasswordInput.addEventListener("input", validateForm);

    // Allow numeric input only
    if (phoneInput) {
        phoneInput.addEventListener("input", () => {
            phoneInput.value = phoneInput.value.replace(/[^0-9]/g, "");
        });
    }
    // Scale the submit button on hover
    if (submitButton) {
        submitButton.addEventListener("mouseover", () => {
            submitButton.style.transform = "scale(1.1)";
            submitButton.style.transition = "transform 0.2s";
        });

        submitButton.addEventListener("mouseout", () => {
            submitButton.style.transform = "scale(1)";
        });
    }

    // After submit, navigate to index.html
    if (signupForm && submitButton) {
        signupForm.addEventListener("submit", (event) => {
           

            
            if (!validateRobot()) {
                event.preventDefault();
                return; 
            }
            if (confirmPasswordInput && passwordInput && (passwordInput.value !== confirmPasswordInput.value)) {
                event.preventDefault();
                alert("Passwords do not match!");
                return;
            }


        
        });
    }

});
