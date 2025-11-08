/* ================================
   NAVBAR.JS - PHIÊN BẢN CẬP NHẬT (Dropdown)
   ================================ */

$(document).ready(function () {

    // =========================================
    // 1. KHAI BÁO BIẾN
    // =========================================
    const $loginBtn = $("#loginBtn");

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

    // Biến cho Profile Dropdown (MỚI)
    const $profileContainer = $("#profileDropdownContainer");
    const $profileToggleBtn = $("#profileToggleBtn");
    const $profileDropdownMenu = $("#profileDropdownMenu");
    const $userGreetingName = $("#userGreetingName");

    // Biến chung
    const $navbar = $(".navbar");

    // =========================================
    // 2. LOGIC AUTH STATE
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

    /* === CẬP NHẬT applyAuthUI === */
    function applyAuthUI() {
        const authRaw = getAuthState();

        if (authRaw) {
            // ĐÃ ĐĂNG NHẬP
            const auth = JSON.parse(authRaw);
            $userGreetingName.text(auth.fullName); // Cập nhật tên
            $profileContainer.show(); // Hiển thị khu vực profile
            $loginBtn.hide(); // Ẩn nút "Đăng nhập"
        } else {
            // CHƯA ĐĂNG NHẬP
            $profileContainer.hide(); // Ẩn khu vực profile
            $loginBtn.show(); // Hiển thị nút "Đăng nhập"
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
            applyAuthUI(); // Cập nhật lại UI
            // (Tùy chọn: Chuyển hướng về trang chủ)
            // window.location.href = "/"; 
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

    setupMobileNav();
    applyAuthUI(); // Chạy ngay khi tải trang

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
                        applyAuthUI(); // Cập nhật Navbar
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

    /* === CẬP NHẬT: Xử lý Đăng xuất === */
    // Sự kiện click Đăng xuất giờ sẽ gắn vào #logoutLink (thay vì #logoutBtn)
    $(document).on("click", "#logoutLink", function (e) {
        e.preventDefault(); // Ngăn link tự nhảy trang
        $profileDropdownMenu.slideUp(200); // Đóng menu trước
        handleLogout();
    });

    /* === MỚI: Xử lý trượt menu profile === */
    $profileToggleBtn.on("click", function () {
        $profileDropdownMenu.slideToggle(200); // 200ms
    });

    // (Tùy chọn) Đóng menu khi click ra bên ngoài
    $(document).on("click", function (event) {
        // Kiểm tra xem click có nằm ngoài .profile-dropdown không
        if (!$profileDropdownMenu.is(":hidden") && !$(event.target).closest('#profileDropdownContainer').length) {
            $profileDropdownMenu.slideUp(200);
        }
    });

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
            $("html, body").animate({ scrollTop: $targetSection.offset().top }, 500);
        } else {
            $("html, body").animate({ scrollTop: $(document).height() }, 500);
        }
    });

});