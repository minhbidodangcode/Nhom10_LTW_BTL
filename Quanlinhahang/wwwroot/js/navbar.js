$(document).ready(function () {
    // =========================================
    // 1. AUTH STATE (giữ đăng nhập giữa các trang)
    // =========================================
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
        return sessionStorage.getItem("authUser") ||
            localStorage.getItem("authUser") ||
            null;
    }

    /* Áp giao diện theo trạng thái */
    function applyAuthUI() {
        const authRaw = getAuthState();
        const $loginBtn = $("#loginBtn");
        const $logoutBtn = $("#logoutBtn"); // Cần thêm nút Logout vào HTML
        const $userGreeting = $("#userGreeting");

        if (authRaw) {
            const auth = JSON.parse(authRaw);
            $userGreeting.text(`Xin chào ${auth.fullName} 👋`).show();
            // Nếu chưa có nút Logout trong HTML, thêm nó vào
            if ($logoutBtn.length === 0) {
                const logoutHtml = '<button class="login-btn" id="logoutBtn">Đăng xuất</button>';
                $("#userGreeting").after(logoutHtml);
                $("#logoutBtn").on("click", handleLogout);
            } else {
                $logoutBtn.show();
            }
            $loginBtn.hide();
        } else {
            $userGreeting.empty().hide();
            $logoutBtn.hide();
            $loginBtn.show();
        }
    }

    // =========================================
    // 2. DỮ LIỆU TÀI KHOẢN DEMO (Tạm thời)
    // =========================================
    // Lưu ý: Trong dự án .NET thực tế, phần này sẽ gọi API/Controller
    const validAccounts = [
        { username: "admin", password: "admin123", fullName: "Admin" },
        { username: "user1", password: "pass123", fullName: "User One", email: "user1@btl.com" },
        { username: "demo", password: "demo123", fullName: "Demo User", email: "demo@btl.com" },
    ];

    // =========================================
    // 3. HIỆU ỨNG & MODAL
    // =========================================

    /* Hiệu ứng Scroll Navbar */
    const $navbar = $(".navbar"); // Sử dụng class .navbar cho tiện
    if ($navbar.length) {
        $(window).on("scroll", function () {
            if ($(window).scrollTop() > 50) {
                $navbar.addClass("scrolled");
            } else {
                $navbar.removeClass("scrolled");
            }
        });
    }

    /* Mở/Đóng Modal Đăng nhập */
    const $loginModal = $("#loginModal");
    const $errorMessage = $("#loginError"); // Đổi từ errorMessage thành loginError
    const $loginSuccess = $("#loginSuccess");

    $("#loginBtn").on("click", function () {
        $loginModal.addClass("active");
        $errorMessage.hide().empty();
        $loginSuccess.hide().empty();
    });

    $("#closeModal, #loginModal").on("click", function (e) {
        // Chỉ đóng modal khi click vào nút đóng hoặc backdrop
        if ($(e.target).is("#closeModal") || $(e.target).is("#loginModal")) {
            $loginModal.removeClass("active");
            $errorMessage.hide().empty();
            $loginSuccess.hide().empty();
        }
    });

    /* Toggle Mật khẩu */
    $("#togglePassword").on("click", function () {
        const $passwordInput = $("#password");
        if ($passwordInput.attr("type") === "password") {
            $passwordInput.attr("type", "text");
            $(this).text("🙈");
        } else {
            $passwordInput.attr("type", "password");
            $(this).text("👁️");
        }
    });

    // =========================================
    // 4. LOGIC ĐĂNG NHẬP
    // =========================================
    function showError(msg) {
        $errorMessage.text(msg).slideDown();
        $loginSuccess.slideUp();
    }

    // Load tên đăng nhập đã ghi nhớ
    const $usernameInput = $("#username");
    const $passwordInput = $("#password");
    const $rememberMe = $("#rememberMe");

    const savedUsername = localStorage.getItem("rememberedUsername");
    if (savedUsername) {
        $usernameInput.val(savedUsername);
        $rememberMe.prop("checked", true);
    }

    $("#loginForm").on("submit", function (e) {
        e.preventDefault();
        const username = $usernameInput.val().trim();
        const password = $passwordInput.val();
        const remember = $rememberMe.is(":checked");

        // Tìm kiếm tài khoản
        const account = validAccounts.find(
            (acc) => acc.username === username && acc.password === password
        );

        if (!account) return showError("Sai tài khoản hoặc mật khẩu!");

        // Lưu/Xóa tên đăng nhập đã ghi nhớ
        if (remember) {
            localStorage.setItem("rememberedUsername", username);
        } else {
            localStorage.removeItem("rememberedUsername");
        }

        saveAuthState(account, remember);
        applyAuthUI();

        alert(`Đăng nhập thành công! Chào mừng ${account.fullName}! 🎉`);
        $loginModal.removeClass("active");
        $(this).trigger("reset");
    });

    // =========================================
    // 5. LOGIC ĐĂNG XUẤT
    // =========================================
    function handleLogout() {
        if (confirm("Bạn có chắc muốn đăng xuất?")) {
            clearAuthState();
            applyAuthUI();
            alert("Đăng xuất thành công! Hẹn gặp lại! 👋");
        }
    }
    // Gắn sự kiện cho nút logout ngay từ đầu (hoặc sau khi được thêm vào DOM bởi applyAuthUI)
    $(document).on("click", "#logoutBtn", handleLogout);

    // =========================================
    // 6. LIÊN HỆ CUỘN MƯỢT
    // =========================================
    $('a[href="#lien-he"]').on("click", function (e) {
        e.preventDefault();
        const target = $(this).attr("href");
        const $targetSection = $(target);

        if ($targetSection.length) {
            $("html, body").animate({
                scrollTop: $targetSection.offset().top
            }, 500);
        } else {
            // Cuộn xuống cuối trang nếu không tìm thấy ID #lien-he
            $("html, body").animate({
                scrollTop: $(document).height()
            }, 500);
        }
    });

    // =========================================
    // 7. NAVBAR MOBILE TOGGLE (Chuyển sang jQuery)
    // =========================================
    function setupMobileNav() {
        const $navMenu = $(".nav-menu");
        const $navToggle = $("#navToggle");
        if (!$navMenu.length || !$navToggle.length) return;

        $navToggle.on("click", function (e) {
            e.stopPropagation();
            if ($navMenu.hasClass("open")) {
                $navMenu.removeClass("open");
                $navToggle.text("☰");
                $("body").css("overflow", "");
            } else {
                $navMenu.addClass("open");
                $navToggle.text("✖");
                $("body").css("overflow", "hidden");
            }
        });

        $navMenu.on("click", "a", function () {
            $navMenu.removeClass("open");
            $navToggle.text("☰");
            $("body").css("overflow", "");
        });

        $(window).on("resize", function () {
            if ($(window).width() > 815 && $navMenu.hasClass("open")) {
                $navMenu.removeClass("open");
                $navToggle.text("☰");
                $("body").css("overflow", "");
            }
        });
    }

    // =========================================
    // 8. KHỞI CHẠY CHUNG
    // =========================================
    setupMobileNav();
    applyAuthUI();

    // =========================================
    // 9. LOGIC ĐĂNG KÝ
    // =========================================
    const $registerModal = $("#registerModal");
    const $registerError = $("#registerError");
    const $registerSuccess = $("#registerSuccess");
    const $registerForm = $("#registerForm");

    function showRegisterError(msg) {
        $registerError.text(msg).slideDown();
        $registerSuccess.slideUp();
    }
    function showRegisterSuccess(msg) {
        $registerSuccess.text(msg).slideDown();
        $registerError.slideUp();
    }

    // Mở Modal Đăng ký
    $(document).on("click", ".register-link a", function (e) {
        e.preventDefault();
        $loginModal.removeClass("active");
        $registerModal.addClass("active");
        $registerError.hide().empty();
        $registerSuccess.hide().empty();
    });

    // Đóng Modal Đăng ký
    $(document).on("click", "#closeRegisterModal, #registerModal", function (e) {
        if ($(e.target).is("#closeRegisterModal") || $(e.target).is("#registerModal")) {
            $registerModal.removeClass("active");
            $registerForm.trigger("reset");
        }
    });

    // Chuyển về Đăng nhập
    $(document).on("click", "#backToLogin", function (e) {
        e.preventDefault();
        $registerModal.removeClass("active");
        $loginModal.addClass("active");
    });

    // Xử lý submit Đăng ký
    $registerForm.on("submit", function (e) {
        e.preventDefault();
        const fullName = $("#regFullName").val().trim();
        const email = $("#regEmail").val().trim();
        const username = $("#regUsername").val().trim();
        const password = $("#regPassword").val();
        const confirmPassword = $("#regConfirmPassword").val();

        if (username.length < 4) return showRegisterError("Tên đăng nhập phải có ít nhất 4 ký tự!");
        if (password.length < 6) return showRegisterError("Mật khẩu phải có ít nhất 6 ký tự!");
        if (password !== confirmPassword) return showRegisterError("Mật khẩu xác nhận không khớp!");

        if (validAccounts.find((a) => a.username === username)) {
            return showRegisterError("Tên đăng nhập đã tồn tại!");
        }
        if (validAccounts.find((a) => a.email === email)) {
            return showRegisterError("Email đã được sử dụng!");
        }

        // **Lưu ý quan trọng**: Trong dự án .NET Core MVC, bước này phải gọi **AJAX POST** // đến Controller để lưu vào bảng KhachHang và TaiKhoan.
        validAccounts.push({ username, password, fullName, email });
        showRegisterSuccess(`Đăng ký thành công! Chào mừng ${fullName} 🎉`);

        setTimeout(() => {
            $registerForm.trigger("reset");
            $registerModal.removeClass("active");
            $loginModal.addClass("active");
            $("#username").val(username); // Điền sẵn tên đăng nhập
        }, 2000);
    });
});

