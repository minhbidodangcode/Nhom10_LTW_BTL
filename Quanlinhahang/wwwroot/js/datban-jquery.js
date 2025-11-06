// datban-jquery.js
const LS_CART_KEY = "cart";
const vnd = (n) => (n || 0).toLocaleString("vi-VN", { style: "currency", currency: "VND" });

function safeParse(raw) {
    try { return JSON.parse(raw); } catch { return null; }
}

// Trả về mảng item [{id,name,price,qty},...]
function getCartArray() {
    const raw = localStorage.getItem(LS_CART_KEY);
    if (!raw) return [];
    const parsed = safeParse(raw);
    if (!parsed) return [];
    // parsed có thể là object map {id: item, ...} hoặc mảng
    if (Array.isArray(parsed)) return parsed;
    return Object.values(parsed);
}

// Lưu mảng vào localStorage dưới dạng object map để dễ cập nhật
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
            <div class="summary-row">
                <div class="summary-col">${idx + 1}</div>
                <div class="summary-col">${item.name}</div>
                <div class="summary-col">${vnd(item.price)}</div>
                <div class="summary-col">${item.qty}</div>
                <div class="summary-col">${vnd(thanhTien)}</div>
            </div>
        `;
    });

    $body.html(html);
    $total.text(vnd(total));
}

$(document).ready(function () {
    renderSummary();

    // Một handler duy nhất cho submit
    $("#bookingForm").off("submit").on("submit", function (e) {
        e.preventDefault();

        // LẤY GIỎ: ưu tiên localStorage, nếu rỗng -> lấy từ server (session)
        let cart = getCartArray();

        function submitWithCart(cartToSend) {
            if (!cartToSend || cartToSend.length === 0) {
                alert("Giỏ hàng trống! Vui lòng chọn món trước khi đặt bàn.");
                return;
            }

            // VALIDATE INPUTS
            const name = $("#customerName").val().trim();
            const phone = $("#phone").val().trim();
            const email = $("#email").val().trim();
            const bookingDate = $("#bookingDate").val();
            const timeSlot = $("#timeSlot").val();
            const guestCount = parseInt($("#guestCount").val() || "1", 10);

            if (!name || name.length < 2) {
                alert("Vui lòng nhập họ và tên (ít nhất 2 ký tự).");
                return;
            }

            // phone: bắt đầu 0 hoặc +84, 10-11 chữ số (VN)
            const phoneRegex = /^(0|\+84)(\d{9,10})$/;
            if (!phoneRegex.test(phone)) {
                alert("Số điện thoại không hợp lệ. VD: 0912345678 hoặc +84912345678.");
                return;
            }

            // email nếu có phải đúng định dạng
            if (email && email.length > 0) {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(email)) {
                    alert("Email không hợp lệ.");
                    return;
                }
            }

            // bookingDate kiểm tra có chọn không
            if (!bookingDate) {
                alert("Vui lòng chọn ngày đặt bàn.");
                return;
            }

            // chuẩn bị payload
            const payload = {
                customerName: name,
                phone: phone,
                email: email,
                bookingDate: bookingDate,
                timeSlot: timeSlot,
                guestCount: guestCount,
                tableType: $("#tableType").val(),
                note: $("#note").val(),
                items: cartToSend.map(it => ({
                    id: it.id,
                    name: it.name,
                    price: Number(it.price || 0),
                    qty: Number(it.qty || 0)
                }))
            };

            // gửi AJAX
            $.ajax({
                url: "/DatBan/Submit",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (res) {
                    // server nên trả về { success: true } khi ok
                    if (res && res.success) {
                        // xóa giỏ local
                        localStorage.removeItem(LS_CART_KEY);
                        // show modal thành công
                        $("#bookingModal").fadeIn();
                    } else {
                        const msg = (res && res.message) ? res.message : "Đặt bàn thất bại. Vui lòng thử lại.";
                        alert(msg);
                    }
                },
                error: function (xhr) {
                    const txt = xhr && xhr.responseText ? xhr.responseText : "Lỗi kết nối";
                    // nếu server trả về JSON message, hãy parse nếu muốn
                    alert("Lỗi: " + txt);
                }
            });
        } // end submitWithCart

        if (!cart || cart.length === 0) {
            // fallback: lấy giỏ từ server session (Cart/GetCart)
            $.ajax({
                url: "/Cart/GetCart",
                type: "GET",
                success: function (serverCart) {
                    if (Array.isArray(serverCart) && serverCart.length > 0) {
                        // chuẩn hoá object server -> định dạng client
                        const mapped = serverCart.map(i => ({
                            id: (i.MonAnId || i.id || i.Id || i.ID || i.monAnId),
                            name: (i.TenMon || i.name || i.tên || i.tenMon),
                            price: Number(i.Gia || i.price || 0),
                            qty: Number(i.SoLuong || i.qty || 1)
                        }));
                        // optional: lưu vào localStorage để nhất quán
                        saveCartArray(mapped);
                        renderSummary();
                        submitWithCart(mapped);
                    } else {
                        alert("Giỏ hàng trống (không tìm thấy cả local và server).");
                    }
                },
                error: function () {
                    alert("Không lấy được giỏ từ server. Vui lòng kiểm tra kết nối.");
                }
            });
        } else {
            // đã có cart local -> submit luôn
            submitWithCart(cart);
        }
    });

    // modal đóng -> về menu
    $("#closeModalBtn").on("click", function () {
        $("#bookingModal").fadeOut();
        window.location.href = "/Home/Menu";
    });
});
