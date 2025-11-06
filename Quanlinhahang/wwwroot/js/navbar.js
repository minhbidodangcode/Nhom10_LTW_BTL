/* ==== AUTH STATE (giữ đăng nhập giữa các trang) ==== */
function saveAuthState(user, remember) {
  const data = JSON.stringify({
    username: user.username,
    fullName: user.fullName || user.username,
    ts: Date.now(),
  });
  if (remember) {
    localStorage.setItem("authUser", data);
    sessionStorage.removeItem("authUser");
  } else {
    sessionStorage.setItem("authUser", data);
    localStorage.removeItem("authUser");
  }
}
function clearAuthState() {
  localStorage.removeItem("authUser");
  sessionStorage.removeItem("authUser");
}
function getAuthState() {
  return (
    sessionStorage.getItem("authUser") ||
    localStorage.getItem("authUser") ||
    null
  );
}

/* Áp giao diện theo trạng thái */
function applyAuthUI() {
  const authRaw = getAuthState();
  const loginBtn = document.getElementById("loginBtn");
  const logoutBtn = document.getElementById("logoutBtn");
  const userGreeting = document.getElementById("userGreeting");

  if (!loginBtn || !logoutBtn || !userGreeting) return;

  if (authRaw) {
    const auth = JSON.parse(authRaw);
    userGreeting.textContent = `Xin chào ${auth.fullName} 👋`;
    userGreeting.style.display = "inline-block";
    logoutBtn.style.display = "inline-block";
    loginBtn.style.display = "none";
  } else {
    userGreeting.textContent = "";
    userGreeting.style.display = "none";
    logoutBtn.style.display = "none";
    loginBtn.style.display = "inline-block";
  }
}

/* ==== DỮ LIỆU TÀI KHOẢN DEMO ==== */
const validAccounts = [
  {
    username: "admin",
    password: "admin123",
    fullName: "Admin",
    email: "admin@btl.com",
  },
  {
    username: "user1",
    password: "pass123",
    fullName: "User One",
    email: "user1@btl.com",
  },
  {
    username: "demo",
    password: "demo123",
    fullName: "Demo User",
    email: "demo@btl.com",
  },
];

/* ==== HIỆU ỨNG SCROLL NAVBAR ==== */
const navbar = document.getElementById("navbar");
if (navbar) {
  window.addEventListener("scroll", () => {
    if (window.scrollY > 50) navbar.classList.add("scrolled");
    else navbar.classList.remove("scrolled");
  });
}

/* ==== LOGIN MODAL ==== */
const loginBtn = document.getElementById("loginBtn");
const loginModal = document.getElementById("loginModal");
const closeModal = document.getElementById("closeModal");
const errorMessage = document.getElementById("errorMessage");

if (loginBtn && loginModal && closeModal) {
  loginBtn.addEventListener("click", () => {
    loginModal.classList.add("active");
    errorMessage.classList.remove("show");
  });
  closeModal.addEventListener("click", () => {
    loginModal.classList.remove("active");
    errorMessage.classList.remove("show");
  });
  loginModal.addEventListener("click", (e) => {
    if (e.target === loginModal) {
      loginModal.classList.remove("active");
      errorMessage.classList.remove("show");
    }
  });
}

/* ==== PASSWORD TOGGLE LOGIN ==== */
const togglePassword = document.getElementById("togglePassword");
const passwordInput = document.getElementById("password");
if (togglePassword && passwordInput) {
  togglePassword.addEventListener("click", () => {
    if (passwordInput.type === "password") {
      passwordInput.type = "text";
      togglePassword.textContent = "🙈";
    } else {
      passwordInput.type = "password";
      togglePassword.textContent = "👁️";
    }
  });
}

/* ==== REGISTER MODAL ==== */
const registerModal = document.getElementById("registerModal");
const closeRegisterModal = document.getElementById("closeRegisterModal");
const registerLink = document.getElementById("registerLink");
const backToLogin = document.getElementById("backToLogin");
const registerForm = document.getElementById("registerForm");
const registerError = document.getElementById("registerError");
const registerSuccess = document.getElementById("registerSuccess");

if (registerLink && registerModal && loginModal) {
  registerLink.addEventListener("click", (e) => {
    e.preventDefault();
    loginModal.classList.remove("active");
    registerModal.classList.add("active");
    registerError.classList.remove("show");
    registerSuccess.classList.remove("show");
  });
}
if (closeRegisterModal) {
  closeRegisterModal.addEventListener("click", () => {
    registerModal.classList.remove("active");
    registerForm.reset();
  });
}
if (registerModal) {
  registerModal.addEventListener("click", (e) => {
    if (e.target === registerModal) {
      registerModal.classList.remove("active");
      registerForm.reset();
    }
  });
}
if (backToLogin) {
  backToLogin.addEventListener("click", (e) => {
    e.preventDefault();
    registerModal.classList.remove("active");
    loginModal.classList.add("active");
  });
}

/* ==== ĐĂNG KÝ ==== */
if (registerForm) {
  registerForm.addEventListener("submit", (e) => {
    e.preventDefault();
    const fullName = document.getElementById("regFullName").value.trim();
    const email = document.getElementById("regEmail").value.trim();
    const username = document.getElementById("regUsername").value.trim();
    const password = document.getElementById("regPassword").value;
    const confirmPassword = document.getElementById("regConfirmPassword").value;

    if (username.length < 4)
      return showRegisterError("Tên đăng nhập phải có ít nhất 4 ký tự!");
    if (password.length < 6)
      return showRegisterError("Mật khẩu phải có ít nhất 6 ký tự!");
    if (password !== confirmPassword)
      return showRegisterError("Mật khẩu xác nhận không khớp!");

    if (validAccounts.find((a) => a.username === username)) {
      return showRegisterError("Tên đăng nhập đã tồn tại!");
    }
    if (validAccounts.find((a) => a.email === email)) {
      return showRegisterError("Email đã được sử dụng!");
    }

    validAccounts.push({ username, password, fullName, email });
    showRegisterSuccess(`Đăng ký thành công! Chào mừng ${fullName} 🎉`);

    setTimeout(() => {
      registerForm.reset();
      registerModal.classList.remove("active");
      loginModal.classList.add("active");
      document.getElementById("username").value = username;
    }, 2000);
  });
}

function showRegisterError(msg) {
  registerError.textContent = msg;
  registerError.classList.add("show");
  registerSuccess.classList.remove("show");
}
function showRegisterSuccess(msg) {
  registerSuccess.textContent = msg;
  registerSuccess.classList.add("show");
  registerError.classList.remove("show");
}

/* ==== LOGIN FORM ==== */
const loginForm = document.getElementById("loginForm");
const usernameInput = document.getElementById("username");
const rememberMe = document.getElementById("rememberMe");
const logoutBtn = document.getElementById("logoutBtn");
const userGreeting = document.getElementById("userGreeting");

window.addEventListener("load", () => {
  const savedUsername = localStorage.getItem("rememberedUsername");
  if (savedUsername) {
    usernameInput.value = savedUsername;
    rememberMe.checked = true;
  }
});

if (loginForm) {
  loginForm.addEventListener("submit", (e) => {
    e.preventDefault();
    const username = usernameInput.value.trim();
    const password = passwordInput.value;

    const account = validAccounts.find(
      (acc) => acc.username === username && acc.password === password
    );
    if (!account) return showError("Sai tài khoản hoặc mật khẩu!");

    if (rememberMe.checked)
      localStorage.setItem("rememberedUsername", username);
    else localStorage.removeItem("rememberedUsername");

    saveAuthState(account, rememberMe.checked);
    applyAuthUI();

    alert(`Đăng nhập thành công! Chào mừng ${account.fullName}! 🎉`);
    loginModal.classList.remove("active");
    loginForm.reset();
  });
}

/* ==== ĐĂNG XUẤT ==== */
if (logoutBtn) {
  logoutBtn.addEventListener("click", () => {
    if (confirm("Bạn có chắc muốn đăng xuất?")) {
      clearAuthState();
      applyAuthUI();
      alert("Đăng xuất thành công! Hẹn gặp lại! 👋");
    }
  });
}

function showError(msg) {
  errorMessage.textContent = msg;
  errorMessage.classList.add("show");
}

/* ==== LIÊN HỆ CUỘN MƯỢT ==== */
document.addEventListener("DOMContentLoaded", () => {
  const lienHeLink = document.querySelector('a[href="#lien-he"]');
  if (lienHeLink) {
    lienHeLink.addEventListener("click", (e) => {
      e.preventDefault();
      const lienHeSection = document.querySelector("#lien-he");
      if (lienHeSection) lienHeSection.scrollIntoView({ behavior: "smooth" });
      else
        window.scrollTo({
          top: document.body.scrollHeight,
          behavior: "smooth",
        });
    });
  }
});

/* ==== NAVBAR MOBILE TOGGLE (<=815px) ==== */
function setup() {
  const navMenu =
    document.querySelector(".nav-menu") || document.getElementById("nav-menu");
  const navToggle =
    document.getElementById("navToggle") ||
    document.querySelector(".nav-toggle, [data-nav-toggle]");
  if (!navMenu || !navToggle) return;

  const openMenu = () => {
    navMenu.classList.add("open");
    navToggle.textContent = "✖";
    document.body.style.overflow = "hidden";
  };
  const closeMenu = () => {
    navMenu.classList.remove("open");
    navToggle.textContent = "☰";
    document.body.style.overflow = "";
  };
  const toggle = (e) => {
    e.stopPropagation();
    navMenu.classList.contains("open") ? closeMenu() : openMenu();
  };

  navToggle.addEventListener("click", toggle);
  navMenu.addEventListener("click", (e) => {
    if (e.target.closest("a")) closeMenu();
  });
  window.addEventListener("resize", () => {
    if (window.innerWidth > 815 && navMenu.classList.contains("open")) {
      closeMenu();
    }
  });
}

/* ==== CHẠY SAU KHI DOM SẴN ==== */
(function () {
  function run() {
    setup();
    applyAuthUI();
  }
  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", run, { once: true });
  } else {
    run();
  }
})();
