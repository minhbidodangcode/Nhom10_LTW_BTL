$(document).ready(function () {
    const $fieldset = $("#infoFieldset");
    const $form = $("#infoForm");
    const $btnEdit = $("#btnEdit");
    const $btnSave = $("#btnSave");
    const $btnCancel = $("#btnCancel");
    const $errorMsg = $("#infoError");
    const $successMsg = $("#infoSuccess");

    let originalData = {};

    // --- 1. Hàm tải dữ liệu người dùng ---
    function loadUserData() {
        console.log("Hàm loadUserData() đã được gọi.");

        const authRaw = getAuthState();
        if (!authRaw) {
            showError("Vui lòng đăng nhập để xem thông tin.");
            return;
        }

        const auth = JSON.parse(authRaw);

        // === SỬA LỖI 1: Điền Tên đăng nhập TẠI ĐÂY ===
        // Vì 'auth' chỉ tồn tại trong hàm này
        $("#regUsername").val(auth.username);
        // ==========================================

        $.ajax({
            url: "/Account/GetUserInfo",
            type: "GET",
            data: { username: auth.username },
            success: function (data) {
                console.log("AJAX Success (Đã nhận được dữ liệu):", data);
                originalData = data;
                populateForm(data); // Chỉ điền dữ liệu từ AJAX
            },
            error: function (xhr) {
                console.error("AJAX Error (Lỗi server):", xhr.status, xhr.responseJSON);
                showError(xhr.responseJSON?.message || "Lỗi tải thông tin người dùng.");
            }
        });

        console.log("Đã gửi yêu cầu AJAX...");
    }

    // Hàm phụ: Đổ dữ liệu vào form
    function populateForm(data) {
        // === SỬA LỖI 2: XÓA DÒNG LỖI TẠI ĐÂY ===
        // $("#regUsername").val(auth.username); // 'auth' không tồn tại ở đây
        // =======================================

        $("#regFullName").val(data.fullName);
        $("#regEmail").val(data.email);
        $("#regPhone").val(data.phone);
        $("#regAddress").val(data.address);
    }

    // --- 2. Hàm chuyển đổi chế độ (Xem / Sửa) ---
    function setEditMode(isEditing) {
        $fieldset.prop("disabled", !isEditing);
        if (isEditing) {
            $btnEdit.hide();
            $btnSave.show();
            $btnCancel.show();
            $errorMsg.hide();
            $successMsg.hide();
        } else {
            $btnEdit.show();
            $btnSave.hide();
            $btnCancel.hide();
        }
    }

    // --- 3. Gắn sự kiện Click ---
    $btnEdit.on("click", function () {
        setEditMode(true);
    });

    $btnCancel.on("click", function () {
        populateForm(originalData); // Khôi phục dữ liệu gốc (trừ username)
        setEditMode(false);
    });

    // Bấm nút "Lưu thay đổi" (Submit form)
    $form.on("submit", function (e) {
        e.preventDefault();
        const auth = JSON.parse(getAuthState());
        const updatedData = {
            username: auth.username,
            fullName: $("#regFullName").val(),
            email: $("#regEmail").val(),
            phone: $("#regPhone").val(),
            address: $("#regAddress").val()
        };

        $.ajax({
            url: "/Account/UpdateUserInfo",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(updatedData),
            beforeSend: function () {
                $btnSave.prop("disabled", true).text("Đang lưu...");
            },
            success: function (response) {
                if (response.success) {
                    showSuccess(response.message);

                    originalData = {
                        fullName: updatedData.fullName,
                        email: updatedData.email,
                        phone: updatedData.phone,
                        address: updatedData.address
                    };

                    // Cập nhật lại tên trên Navbar
                    auth.fullName = response.newFullName;

                    // === SỬA LỖI 3: Thêm tham số 'remember' ===
                    saveAuthState(auth, localStorage.getItem("authUser") != null);
                    applyAuthUI(); // Hàm này từ navbar.js
                    // ======================================

                    setEditMode(false);
                } else {
                    showError(response.message);
                }
            },
            error: function (xhr) {
                showError(xhr.responseJSON?.message || "Lỗi server.");
            },
            complete: function () {
                $btnSave.prop("disabled", false).text("Lưu thay đổi");
            }
        });
    });

    // --- 4. Hàm hiển thị thông báo ---
    function showError(msg) {
        $errorMsg.text(msg).slideDown();
        $successMsg.slideUp();
    }
    function showSuccess(msg) {
        $successMsg.text(msg).slideDown();
        $errorMsg.slideUp();
    }

    // --- 5. Chạy khi tải trang ---
    loadUserData();
});