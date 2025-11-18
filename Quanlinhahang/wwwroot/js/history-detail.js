// history-detail.js
const vnd = (n) => (n || 0).toLocaleString("vi-VN", { style: "currency", currency: "VND" });

$(document).ready(function () {
    const $fieldset = $("#infoFieldset");
    const $form = $("#updateBookingForm");
    const $btnEdit = $("#btnEdit");
    const $btnSave = $("#btnSave");
    const $btnCancel = $("#btnCancel");
    const $errorMsg = $("#infoError");
    const $successMsg = $("#infoSuccess");
    const $summaryBody = $("#summaryBodyItems"); // <tbody>
    const $summaryTotal = $("#summaryTotal");

    // Modal Bàn
    const $tableMapModal = $("#tableMapModal");
    const $hiddenInput = $("#selectedBanPhongId");
    const $displayArea = $("#selectedTableDisplay");

    // Modal Thêm Món
    const $addMonAnModal = $("#addMonAnModal");
    const $openAddMonAnModal = $("#openAddMonAnModal"); // Nút "+"

    const datBanId = parseInt(window.location.pathname.split('/').pop());

    // 'originalItemsData' được nhúng từ View (đã sửa lỗi C#)
    let currentItems = JSON.parse(JSON.stringify(originalItemsData));

    // 2. Hàm render danh sách món ăn
    function renderItems() {
        $summaryBody.empty();
        let total = 0;

        if (currentItems.length === 0) {
            $summaryBody.html('<tr><td colspan="5" class="empty-cart">Chưa có món ăn nào.</td></tr>');
            $summaryTotal.text(vnd(0));
            return;
        }

        currentItems.forEach((item, idx) => {
            // Sửa lỗi: Đảm bảo donGia được đọc đúng
            const donGia = item.donGia || item.DonGia || 0;
            const thanhTien = donGia * item.soLuong;
            total += thanhTien;

            const itemHtml = `
                <tr data-id="${item.monAnId}">
                    <td>${item.tenMon || 'Món không xác định'}</td>
                    <td class="align-right">${vnd(donGia)}</td>
                    <td style="text-align:center;">
                        <div class="qty-controls">
                            <button class="qty-btn minus" data-id="${item.monAnId}" aria-label="Giảm" disabled>−</button>
                            <span class="qty">${item.soLuong}</span>
                            <button class="qty-btn plus" data-id="${item.monAnId}" aria-label="Tăng" disabled>+</button>
                        </div>
                    </td>
                    <td class="align-right">${vnd(thanhTien)}</td>
                    <td style="text-align:center;">
                        <button class="remove-btn" data-id="${item.monAnId}" aria-label="Xóa" disabled>&times;</button>
                    </td>
                </tr>
            `;
            $summaryBody.append(itemHtml);
        });
        $summaryTotal.text(vnd(total));
    }

    // 3. Hàm chuyển chế độ (Xem / Sửa)
    function setEditMode(isEditing) {
        $fieldset.prop("disabled", !isEditing);
        $summaryBody.find(".qty-btn, .remove-btn").prop("disabled", !isEditing);

        // SỬA LỖI: Nhắm đúng ID của nút "+"
        if (isEditing) {
            $openAddMonAnModal.show();
            $btnEdit.hide();
            $btnSave.show();
            $btnCancel.show();
            $errorMsg.hide();
            $successMsg.hide();
        } else {
            $openAddMonAnModal.hide();
            $btnEdit.show();
            $btnSave.hide();
            $btnCancel.hide();
        }
    }

    // --- 4. Gắn sự kiện Click ---

    $btnEdit.on("click", function () {
        setEditMode(true);
    });

    $btnCancel.on("click", function () {
        $form[0].reset();
        currentItems = JSON.parse(JSON.stringify(originalItemsData));
        renderItems();
        setEditMode(false);
    });

    // Xử lý click nút +/-/x
    $summaryBody.on("click", ".qty-btn", function (e) {
        e.preventDefault();
        const id = $(this).data("id");
        const item = currentItems.find(i => i.monAnId === id);
        if (!item) return;

        if ($(this).hasClass("plus")) item.soLuong++;
        else if ($(this).hasClass("minus")) item.soLuong--;

        if (item.soLuong <= 0) item.soLuong = 1;

        renderItems();
        setEditMode(true);
    });

    $summaryBody.on("click", ".remove-btn", function (e) {
        e.preventDefault();
        const id = $(this).data("id");
        if (confirm("Xóa món này khỏi đơn?")) {
            currentItems = currentItems.filter(i => i.monAnId !== id);
            renderItems();
            setEditMode(true);
        }
    });

    // === LOGIC MODAL CHỌN BÀN ===
    $("#openTableMapBtn").on("click", function (e) {
        e.preventDefault();
        if ($fieldset.is(':disabled')) return;
        $tableMapModal.addClass("active");
    });
    $("#closeTableMapModal").on("click", function () {
        $tableMapModal.removeClass("active");
    });
    $(".table-map-container").on("click", ".table-card", function () {
        const $card = $(this);
        if ($card.data("available") !== true) {
            alert("Bàn này đang bận, vui lòng chọn bàn Trống.");
            return;
        }
        if ($card.hasClass("selected")) {
            $card.removeClass("selected");
            $hiddenInput.val("");
            $displayArea.text("Chưa chọn bàn");
        } else {
            $(".table-card.selected").removeClass("selected");
            $card.addClass("selected");
            $hiddenInput.val($card.data("id"));
            $displayArea.text(`Đã chọn: ${$card.data("name")}`);
            $tableMapModal.removeClass("active");
        }
    });

    // === LOGIC MODAL THÊM MÓN ===
    $openAddMonAnModal.on("click", function () {
        $addMonAnModal.addClass("active");
        $(".category-btn[data-category-id='all']").click();
    });
    $("#closeAddMonAnModal").on("click", function () {
        $addMonAnModal.removeClass("active");
    });

    $(".menu-modal-sidebar").on("click", ".category-btn", function () {
        const $btn = $(this);
        const categoryId = $btn.data("category-id");

        $(".category-btn.active").removeClass("active");
        $btn.addClass("active");

        if (categoryId === "all") {
            $("#addMonAnList .monan-item").show();
        } else {
            $("#addMonAnList .monan-item").hide();
            $(`#addMonAnList .monan-item[data-monan-category-id="${categoryId}"]`).show();
        }
    });

    $("#addMonAnList").on("click", ".btn-add-mon", function () {
        const $btn = $(this);
        const id = $btn.data("id");
        const name = $btn.data("name");
        const price = parseFloat($btn.data("price"));

        let existingItem = currentItems.find(i => i.monAnId === id);

        if (existingItem) {
            existingItem.soLuong++;
        } else {
            currentItems.push({
                monAnId: id,
                tenMon: name,
                soLuong: 1,
                donGia: price,
                thanhTien: price
            });
        }

        renderItems();
        $addMonAnModal.removeClass("active");
    });

    // Bấm nút "Lưu thay đổi"
    $form.on("submit", function (e) {
        e.preventDefault();

        // SỬA LỖI: Gọi getAuthState() để lấy thông tin user
        const authRaw = getAuthState();
        if (!authRaw) {
            showError("Lỗi: Phiên đăng nhập hết hạn. Vui lòng tải lại trang.");
            return;
        }
        const auth = JSON.parse(authRaw);

        const payload = {
            username: auth.username,
            datBanId: datBanId,
            bookingDate: $("#bookingDate").val(),
            timeSlot: $("#timeSlot").val(),
            guestCount: parseInt($("#guestCount").val()),
            banPhongId: parseInt($("#selectedBanPhongId").val()) || null,
            items: currentItems.map(i => ({
                monAnId: i.monAnId,
                soLuong: i.soLuong,
                donGia: i.donGia || i.Gia // Đảm bảo lấy đúng giá
            }))
        };

        $.ajax({
            url: "/Account/UpdateBooking",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            beforeSend: function () { $btnSave.prop("disabled", true).text("Đang lưu..."); },
            success: function (res) {
                if (res.success) {
                    showSuccess(res.message || "Cập nhật đơn hàng thành công!"); // Sửa lỗi 1: Chỉ gọi 1 lần
                    setEditMode(false);
                    // Cập nhật dữ liệu gốc
                    originalItemsData = JSON.parse(JSON.stringify(currentItems));

                    // SỬA LỖI 2: Cập nhật auth state (nếu tên thay đổi)
                    if (res.newFullName) {
                        auth.fullName = res.newFullName;
                        saveAuthState(auth, localStorage.getItem("authUser") != null);
                        applyAuthUI();
                    }
                } else {
                    showError(res.message);
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

    function showError(msg) { $errorMsg.text(msg).slideDown(); $successMsg.slideUp(); }
    function showSuccess(msg) { $successMsg.text(msg).slideDown(); $errorMsg.slideUp(); }

    // --- 5. Tải dữ liệu ban đầu ---
    renderItems();
});