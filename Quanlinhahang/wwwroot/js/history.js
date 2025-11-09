$(document).ready(function () {
    const $tabs = $(".history-tabs .history-tab-item");
    const $resultsContainer = $("#history-results");

    // Lấy thông tin người dùng
    const authRaw = getAuthState();
    if (!authRaw) {
        $resultsContainer.html('<p class="no-results">Vui lòng đăng nhập để xem lịch sử.</p>');
        return;
    }
    const auth = JSON.parse(authRaw);

    // --- 1. Hàm gọi AJAX để tải dữ liệu ---
    function fetchHistory(status) {
        $resultsContainer.html('<div class="loading-spinner">Đang tải...</div>');

        $.ajax({
            url: "/Account/GetHistoryData",
            type: "GET",
            data: {
                username: auth.username,
                status: status
            },
            success: function (data) {
                $resultsContainer.empty();

                if (data.length === 0) {
                    $resultsContainer.html('<p class="no-results">Không tìm thấy lịch sử nào.</p>');
                    return;
                }

                data.forEach(item => {
                    let statusClass = "status-cho-xac-nhan";
                    let statusText = item.trangThaiDatBan; // Mặc định là trạng thái đặt bàn

                    // 1. Ưu tiên kiểm tra ĐÃ THANH TOÁN
                    if (item.trangThaiThanhToan === "Đã thanh toán") {
                        statusClass = "status-da-thanh-toan";
                        statusText = "Đã thanh toán";
                    }
                    // 2. Kiểm tra trạng thái Đặt bàn còn lại
                    else if (item.trangThaiDatBan === "Đã xác nhận") {
                        statusClass = "status-da-xac-nhan";
                    }
                    else if (item.trangThaiDatBan === "Đang phục vụ") {
                        statusClass = "status-dang-phuc-vu";
                        statusText = "Đang phục vụ";
                    }
                    // Nếu là "Chờ xác nhận" thì dùng status-cho-xac-nhan

                    const itemHtml = `
                        <div class="history-item">
                            <div class="history-item-info">
                                <h4>Bàn: ${item.tenBanPhong}</h4>
                                <p>Ngày đến: ${item.ngayDen}</p>
                                <p>Số người: ${item.soNguoi}</p>
                            </div>
                            <div class="history-item-status ${statusClass}">
                                ${statusText}
                            </div>
                        </div>
                    `;
                    $resultsContainer.append(itemHtml);
                });
            },
            error: function (xhr) {
                $resultsContainer.html(`<p class="no-results">Lỗi tải dữ liệu: ${xhr.responseJSON?.message || 'Lỗi server'}</p>`);
            }
        });
    }

    // --- 2. Gắn sự kiện Click cho các Tab ---
    $tabs.on("click", function () {
        $tabs.removeClass("active");
        $(this).addClass("active");

        const status = $(this).data("status");
        fetchHistory(status);
    });

    // --- 3. Tải dữ liệu cho tab "Tất cả" khi mới vào trang ---
    fetchHistory("tất cả");

});