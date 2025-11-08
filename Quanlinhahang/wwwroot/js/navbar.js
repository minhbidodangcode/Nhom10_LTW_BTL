/* ================================
   NAVBAR.JS - ĐÃ SỬA LỖI VÀ TỐI ƯU HÓA
   ================================ */

$(document).ready(function () {

    // =========================================
    // 1. KHAI BÁO BIẾN (Tất cả ở đây)
    // =========================================
    const $loginBtn = $("#loginBtn");
    const $userGreeting = $("#userGreeting");

    // Biến cho Modal Đăng nhập
    const $loginModal = $("#loginModal");
    const $loginForm = $("#loginForm");
    const $usernameInput = $("#username");
    const $passwordInput = $("#password");
    const $rememberMe = $("#rememberMe");
    const $errorMessage = $("#loginError");
    const $loginSuccess = $("#loginSuccess");

    // Biến cho Modal Đăng ký
    const $registerModal = $("#registerModal");

    // Biến chung
    const $navbar = $(".navbar");

    // =========================================
    // 2. LOGIC AUTH STATE (Giữ nguyên)
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
        const $logoutBtn = $("#logoutBtn"); // Vẫn kiểm tra ở đây vì nó được tạo động

        if (authRaw) {
            const auth = JSON.parse(authRaw);
            $userGreeting.text(`Xin chào ${auth.fullName} 👋`).show();
            if ($logoutBtn.length === 0) {
                const logoutHtml = '<button class="login-btn" id="logoutBtn">Đăng xuất</button>';
                $("#userGreeting").after(logoutHtml);
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
    // 3. HÀM TRỢ GIÚP (Helpers)
    // =========================================

    /* Hàm cho Login Modal */
    function showError(msg) {
        $errorMessage.text(msg).slideDown();
        $loginSuccess.slideUp();
    }

    function showSuccess(msg) {
        $loginSuccess.text(msg).slideDown();
        $errorMessage.slideUp();
    }

    /* Hàm Đăng xuất */
    function handleLogout() {
        if (confirm("Bạn có chắc muốn đăng xuất?")) {
            clearAuthState();
            applyAuthUI();
            alert("Đăng xuất thành công! Hẹn gặp lại! 👋");
        }
    }

    /* Hàm Mobile Nav */
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
    // 4. KHỞI CHẠY (Initialization)
    // =========================================

    // Load tên đăng nhập đã ghi nhớ
    const savedUsername = localStorage.getItem("rememberedUsername");
    if (savedUsername) {
        $usernameInput.val(savedUsername);
        $rememberMe.prop("checked", true);
    }

    // Chạy các hàm khởi tạo
    setupMobileNav();
    applyAuthUI();

    // =========================================
    // 5. GẮN SỰ KIỆN (Event Handlers)
    // =========================================

    /* Mở Modal Đăng nhập */
    $loginBtn.on("click", function () {
        $loginModal.addClass("active");
        $errorMessage.hide().empty();
        $loginSuccess.hide().empty();
    });

    /* Đóng Modal Đăng nhập */
    $("#closeModal, #loginModal").on("click", function (e) {
        if ($(e.target).is("#closeModal") || $(e.target).is("#loginModal")) {
            $loginModal.removeClass("active");
            $errorMessage.hide().empty();
            $loginSuccess.hide().empty();
        }
    });

    /* Toggle Mật khẩu (Login) */
    $("#togglePassword").on("click", function () {
        if ($passwordInput.attr("type") === "password") {
            $passwordInput.attr("type", "text");
            $(this).text("🙈");
        } else {
            $passwordInput.attr("type", "password");
            $(this).text("👁️");
        }
    });

    /* Xử lý Submit Form Đăng nhập */
    $loginForm.on("submit", function (e) {
        e.preventDefault();

        const formData = {
            Username: $usernameInput.val().trim(),
            Password: $passwordInput.val(),
            RememberMe: $rememberMe.is(":checked")
        };

        if (formData.Username === "" || formData.Password === "") {
            return showError("Vui lòng nhập tài khoản và mật khẩu.");
        }

        $.ajax({
            url: "/Account/Login",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(formData),
            beforeSend: function () {
                $loginForm.find("button[type='submit']").prop("disabled", true).text("Đang đăng nhập...");
                $errorMessage.slideUp();
                $loginSuccess.slideUp();
            },
            success: function (response) {
                if (response.success && response.user) {
                    showSuccess("Đăng nhập thành công! Chào mừng " + response.user.fullName + " 👋");

                    if (formData.RememberMe) {
                        localStorage.setItem("rememberedUsername", formData.Username);
                    } else {
                        localStorage.removeItem("rememberedUsername");
                    }
                    saveAuthState(response.user, formData.RememberMe);

                    setTimeout(() => {
                        applyAuthUI();
                        $loginModal.removeClass("active");
                        $loginForm.trigger("reset");
                        $loginSuccess.hide();
                    }, 1500);
                } else {
                    showError(response.message || "Đăng nhập thất bại.");
                    $loginForm.find("button[type='submit']").prop("disabled", false).text("Đăng nhập");
                }
            },
            error: function (xhr) {
                let errorMsg = "Lỗi kết nối Server.";
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMsg = xhr.responseJSON.message;
                }
                showError(errorMsg);
                $loginForm.find("button[type='submit']").prop("disabled", false).text("Đăng nhập");
            }
        });
    });

    /* Xử lý Đăng xuất (Dùng event delegation) */
    $(document).on("click", "#logoutBtn", handleLogout);

    /* Chuyển sang Modal Đăng ký */
    $("#registerLink").on("click", function (e) {
        e.preventDefault();
        $loginModal.removeClass("active");
        $registerModal.addClass("active");
    });

    /* Hiệu ứng Scroll Navbar */
    if ($navbar.length) {
        $(window).on("scroll", function () {
            if ($(window).scrollTop() > 50) {
                $navbar.addClass("scrolled");
            } else {
                $navbar.removeClass("scrolled");
            }
        });
    }

    /* Cuộn mượt đến Liên hệ */
    $('a[href="#lien-he"]').on("click", function (e) {
        e.preventDefault();
        const target = $(this).attr("href");
        const $targetSection = $(target);

        if ($targetSection.length) {
            $("html, body").animate({
                scrollTop: $targetSection.offset().top
            }, 500);
        } else {
            $("html, body").animate({
                scrollTop: $(document).height()
            }, 500);
        }
    });

});