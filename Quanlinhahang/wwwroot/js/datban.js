const LS_CART_KEY = "cart";
const vnd = (n) => (n || 0).toLocaleString("vi-VN", { style: "currency", currency: "VND" });

function getCart() {
    try {
        const raw = localStorage.getItem(LS_CART_KEY);
        return raw ? Object.values(JSON.parse(raw)) : [];
    } catch {
        return [];
    }
}

function setCart(cart) {
    const obj = {};
    cart.forEach(it => obj[it.id] = it);
    localStorage.setItem(LS_CART_KEY, JSON.stringify(obj));
}

function cartTotal(cart) {
    return cart.reduce((sum, it) => sum + it.price * it.qty, 0);
}

function renderSummary() {
    const data = getCart();

    if (data.length === 0) {
        $("#summaryBody").html('<div class="empty-cart">Giỏ hàng đang trống. Vui lòng chọn món ở trang <a href="/Home/Menu">Thực đơn</a>.</div>');
        $("#summaryTotal").text("0₫");
        return;
    }

    let html = "";
    let total = 0;

    data.forEach((item, index) => {
        let thanhTien = item.price * item.qty;
        total += thanhTien;

        html += `
            <div class="summary-row">
                <div class="summary-col">${index + 1}</div>
                <div class="summary-col">${item.name}</div>
                <div class="summary-col">${vnd(item.price)}</div>
                <div class="summary-col">${item.qty}</div>
                <div class="summary-col">${vnd(thanhTien)}</div>
            </div>
        `;
    });

    $("#summaryBody").html(html);
    $("#summaryTotal").text(vnd(total));
}

$(document).ready(function () {
    renderSummary();

    $("#bookingForm").on("submit", function (e) {
        e.preventDefault();

        const cart = getCart();
        if (cart.length === 0) {
            alert("Giỏ hàng trống!");
            return;
        }

        const formData = {
            customerName: $("#customerName").val(),
            phone: $("#phone").val(),
            email: $("#email").val(),
            bookingDate: $("#bookingDate").val(),
            timeSlot: $("#timeSlot").val(),
            guestCount: parseInt($("#guestCount").val()),
            tableType: $("#tableType").val(),
            note: $("#note").val(),
            items: cart 
        };

        $.ajax({
            url: "/DatBan/Submit",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function (res) {
                if (res.success) {
                    localStorage.removeItem(LS_CART_KEY);

                    $("#bookingModal").fadeIn();
                } else {
                    alert("Đặt bàn thất bại: " + (res.message || "Lỗi không xác định"));
                }
            },
            error: function (xhr) {
                alert("Lỗi kết nối: " + xhr.responseText);
            }
        });
    });

    $("#closeModalBtn").click(function () {
        $("#bookingModal").fadeOut();
        window.location.href = "/Home/Menu";
    });
});
