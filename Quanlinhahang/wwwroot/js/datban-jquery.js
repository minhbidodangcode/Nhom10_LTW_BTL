// datban-jquery.js

const LS_CART_KEY = "cart";
const vnd = (n) => (n || 0).toLocaleString("vi-VN", { style: "currency", currency: "VND" });

// --- CÁC HÀM HELPER ---
function safeParse(raw) {
    try { return JSON.parse(raw); } catch { return null; }
}
function getAuthState() {
    return localStorage.getItem("authUser") || sessionStorage.getItem("authUser") || null;
}

// --- QUẢN LÝ GIỎ HÀNG ---
function getCartArray() {
    const raw = localStorage.getItem(LS_CART_KEY);
    if (!raw) return [];
    const parsed = safeParse(raw);
    if (!parsed) return [];
    if (Array.isArray(parsed)) return parsed;
    return Object.values(parsed);
}

function saveCartArray(arr) {
    const map = {};
    (arr || []).forEach(it => {
        if (!it || !it.id) return;
        map[it.id] = { id: it.id, name: it.name, price: Number(it.price || 0), qty: Number(it.qty || 1) };
    });
    try {
        localStorage.setItem(LS_CART_KEY, JSON.stringify(map));
    } catch (e) {
        console.warn("Không lưu được cart:", e);
    }
}

function cartTotal(arr) {
    return (arr || []).reduce((s, it) => s + (Number(it.price || 0) * Number(it.qty || 0)), 0);
}

// === HÀM RENDER TÓM TẮT ===
function renderSummary() {
    const data = getCartArray();
    const $body = $("#summaryBody");
    const $total = $("#summaryTotal");

    if (!data || data.length === 0) {
        $body.html('<div class="empty-cart">Giỏ hàng đang trống. Vui lòng chọn món ở trang <a href="/Home/Menu">Thực đơn</a>.</div>');
        $total.text("0₫");
        return;
    }

    let html = "";
    let total = 0;
    data.forEach((item, idx) => {
        const thanhTien = Number(item.price || 0) * Number(item.qty || 0);
        total += thanhTien;

        html += `
            <div class="summary-row" data-id="${item.id}">
                <div class="summary-col">${idx + 1}</div>
                <div class="summary-col">${item.name}</div>
                <div class="summary-col">${vnd(item.price)}</div>
                <div class="summary-col qty-controls">
                    <button class="qty-btn minus" data-id="${item.id}" aria-label="Giảm">−</button>
                    <span class="qty">${item.qty}</span>
                    <button class="qty-btn plus" data-id="${item.id}" aria-label="Tăng">+</button>
                </div>
                <div class="summary-col">${vnd(thanhTien)}</div>
                <div class="summary-col">
                    <button class="remove-btn" data-id="${item.id}" aria-label="Xóa">&times;</button>
                </div>
            </div>
        `;
    });

    $body.html(html);
    $total.text(vnd(total));
}

// === HÀM KIỂM TRA ĐĂNG NHẬP ===
function renderUserGreeting() {
    const authRaw = getAuthState();
    const $box = $("#userGreetingBox");
    const $name = $("#loggedInName");

    if (authRaw) {
        const auth = safeParse(authRaw);
        if (auth && auth.fullName) {
            $name.text(auth.fullName);
            $box.show();
            return auth.username;
        }
    }

    $box.hide();
    return null;
}

$(document).ready(function () {
    renderSummary();

    const username = renderUserGreeting();
    if (!username) {
        $("#bookingForm").hide();
        $("#userGreetingBox").html('<p class="alert-error">Bạn cần đăng nhập để đặt bàn. Vui lòng đăng nhập hoặc <a href="/Account/Dangki">đăng ký</a>.</p>').show();
        return;
    }

    /* =====================================================
       XỬ LÝ MODAL CHỌN BÀN (ĐÃ SỬA TÊN CLASS)
       ===================================================== */
    const $tableMapModal = $("#tableMapModal");
    const $hiddenInput = $("#selectedBanPhongId");
    const $openTableMapBtn = $("#openTableMapBtn");

    // 1. Mở Modal
    $openTableMapBtn.on("click", function (e) {
        e.preventDefault();

        // Đồng bộ trạng thái đã chọn
        const currentId = $hiddenInput.val();
        $(".table-card.selected").removeClass("selected");
        if (currentId) {
            $(`.table-card[data-id='${currentId}']`).addClass("selected");
        }

        $tableMapModal.addClass("active");
    });

    // 2. Đóng Modal
    $("#closeTableMapModal").on("click", function () {
        $tableMapModal.removeClass("active");
    });

    // 3. Xử lý Click Chọn Bàn
    // SỬA LỖI: Thay .table-map-container bằng .table-map-grid
    $(".table-map-grid").on("click", ".table-card", function () {
        const $card = $(this);

        // Kiểm tra trạng thái bàn
        const isAvailable = $card.data("available");
        if (String(isAvailable).toLowerCase() !== "true") {
            alert("Bàn này đang bận hoặc đã đặt. Vui lòng chọn bàn khác.");
            return;
        }

        // Lấy ID và Tên
        const selectedId = $card.data("id");
        const selectedName = $card.data("name");

        // Nếu đang chọn chính bàn đó -> Bỏ chọn
        if ($card.hasClass("selected")) {
            $card.removeClass("selected");
            $hiddenInput.val("");
            $openTableMapBtn.text("Chọn bàn từ sơ đồ");
            $openTableMapBtn.removeClass("selected");
        } else {
            // Chọn bàn mới
            $(".table-card.selected").removeClass("selected"); // Xóa chọn cũ
            $card.addClass("selected"); // Chọn mới

            $hiddenInput.val(selectedId); // Lưu ID
            $openTableMapBtn.text(`Đã chọn: ${selectedName}`); // Cập nhật text nút
            $openTableMapBtn.addClass("selected"); // Đổi màu nút

            // Đóng modal sau 200ms
            setTimeout(() => {
                $tableMapModal.removeClass("active");
            }, 200);
        }
    });

    /* =====================================================
       XỬ LÝ GIỎ HÀNG (+ / - / Xóa)
       ===================================================== */
    $("#summaryBody").on("click", ".qty-btn", function (e) {
        e.preventDefault();
        const $btn = $(this);
        const id = $btn.data("id");
        let cart = getCartArray();
        const item = cart.find(i => String(i.id) === String(id));

        if (!item) return;

        if ($btn.hasClass("plus")) {
            item.qty++;
        } else if ($btn.hasClass("minus")) {
            item.qty--;
        }

        if (item.qty <= 0) {
            cart = cart.filter(i => String(i.id) !== String(id));
        }

        saveCartArray(cart);
        renderSummary();
    });

    $("#summaryBody").on("click", ".remove-btn", function (e) {
        e.preventDefault();
        const id = $(this).data("id");
        if (confirm("Xóa món này khỏi giỏ hàng?")) {
            let cart = getCartArray();
            cart = cart.filter(i => String(i.id) !== String(id));
            saveCartArray(cart);
            renderSummary();
        }
    });

    /* =====================================================
       XỬ LÝ SUBMIT FORM
       ===================================================== */
    $("#bookingForm").off("submit").on("submit", function (e) {
        e.preventDefault();
        let cart = getCartArray();

        function submitWithCart(cartToSend) {
            const authRaw = getAuthState();
            const auth = safeParse(authRaw);

            if (!auth || !auth.username) {
                alert("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");
                return;
            }
            if (!cartToSend || cartToSend.length === 0) {
                alert("Giỏ hàng trống! Vui lòng chọn món trước khi đặt bàn.");
                return;
            }

            const bookingDate = $("#bookingDate").val();
            const timeSlot = $("#timeSlot").val();
            const guestCount = parseInt($("#guestCount").val() || "1", 10);

            if (!bookingDate || !timeSlot) {
                alert("Vui lòng chọn ngày đặt và khung giờ.");
                return;
            }

            const payload = {
                username: auth.username,
                bookingDate: bookingDate,
                timeSlot: timeSlot,
                guestCount: guestCount,
                BanPhongId: parseInt($("#selectedBanPhongId").val()) || null,
                note: $("#note").val(),
                items: cartToSend.map(it => ({
                    id: it.id,
                    name: it.name,
                    price: Number(it.price || 0),
                    qty: Number(it.qty || 0)
                }))
            };

            const $submitBtn = $("#bookingForm .btn-submit");
            $submitBtn.text("Đang xác nhận...");
            $submitBtn.prop("disabled", true);

            $.ajax({
                url: "/DatBan/Submit",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (res) {
                    if (res && res.success) {
                        localStorage.removeItem(LS_CART_KEY);
                        renderSummary();
                        $("#bookingModal").addClass("active");
                    } else {
                        const msg = (res && res.message) ? res.message : "Đặt bàn thất bại. Vui lòng thử lại.";
                        alert("Lỗi: " + msg);
                    }
                },
                error: function (xhr) {
                    const errorJson = safeParse(xhr.responseText);
                    const msg = errorJson ? errorJson.message : "Lỗi kết nối Server. Mã lỗi: " + xhr.status;
                    alert("Đặt bàn thất bại: " + msg);
                },
                complete: function () {
                    $submitBtn.text("Xác nhận đặt bàn");
                    $submitBtn.prop("disabled", false);
                }
            });
        }

        if (!cart || cart.length === 0) {
            alert("Giỏ hàng trống! Không thể đặt bàn.");
            return;
        } else {
            submitWithCart(cart);
        }
    });

    // Đóng Modal Thành công
    $("#closeModalBtn").on("click", function () {
        $("#bookingModal").removeClass("active");
        window.location.href = "/Home/Menu";
    });
});