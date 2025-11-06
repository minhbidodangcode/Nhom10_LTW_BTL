$(function () {
    /* -------------------- FILTER & ANIMATION -------------------- */
    const $search = $('#search-box');
    const $filter = $('#filter-cat');
    const $cards = $('.menu-card');

    function formatVND(n) {
        if (isNaN(n)) return "0₫";
        return n.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".") + "₫";
    }

    function applyFiltersClient() {
        const q = ($search.val() || '').trim().toLowerCase();
        const cat = ($filter.val() || '').toLowerCase();
        let visibleCount = 0;

        $cards.each(function () {
            const $c = $(this);
            const hay = (($c.attr('data-search') || $c.text()) + '').toLowerCase();
            const itemCat = (($c.attr('data-cat') || '') + '').toLowerCase();

            const matchText = !q || hay.indexOf(q) !== -1;
            const matchCat = !cat || cat === '' || cat === 'all' || itemCat === cat;

            if (matchText && matchCat) {
                $c.show();
                visibleCount++;
            } else {
                $c.hide();
            }
        });

        if (visibleCount === 0) {
            if ($('#no-results').length === 0) {
                $('<p id="no-results" style="color:#b45c00;margin-top:10px">Không tìm thấy món phù hợp.</p>')
                    .insertAfter('#combo-section .menu-grid:first, #alacarte-section .menu-grid:first');
            }
        } else {
            $('#no-results').remove();
        }
    }

    $cards.each(function () {
        const $c = $(this);
        if (!$c.attr('data-search')) {
            const txt = ($c.find('h3, h4').text() + ' ' + $c.find('.desc').text()).trim();
            $c.attr('data-search', txt);
        }
        if (!$c.attr('data-cat')) {
            const sect = $c.closest('section').attr('id') || '';
            if (sect.indexOf('combo') !== -1) $c.attr('data-cat', 'combo');
            else if (sect.indexOf('alacarte') !== -1) $c.attr('data-cat', 'alacarte');
        }
    });

    $search.on('input', applyFiltersClient);
    $filter.on('change', applyFiltersClient);
    applyFiltersClient();

    $(window).on('scroll load', function () {
        $('.menu-card').each(function () {
            var cardTop = $(this).offset().top;
            var windowBottom = $(window).scrollTop() + $(window).height();
            if (cardTop < windowBottom - 100) $(this).addClass('revealed');
        });
    });

    /* -------------------- CART & DRAWER -------------------- */
    const LS_KEY = "cart"; // 🔄 đồng bộ với datban.js
    let cart = JSON.parse(localStorage.getItem(LS_KEY) || "{}");

    $(document).ready(function () {
        const $cartBtn = $("#cartBtn");
        const $cartDrawer = $("#cartDrawer");
        const $closeCart = $("#close-cart");

        $cartBtn.on("click", function (e) {
            e.stopPropagation();
            $cartDrawer.toggleClass("active");
        });

        $closeCart.on("click", function () {
            $cartDrawer.removeClass("active");
        });

        $(document).on("click", function (e) {
            if (!$(e.target).closest("#cartDrawer, #cartBtn").length) {
                $cartDrawer.removeClass("active");
            }
        });
    });

    function saveCart() {
        localStorage.setItem(LS_KEY, JSON.stringify(cart));
    }

    function updateCartDisplay() {
        const $items = $("#cartItems");
        $items.empty();
        let total = 0;

        Object.values(cart).forEach(item => {
            const lineTotal = item.qty * item.price;
            total += lineTotal;
            $items.append(`
                <div class="cart-item">
                    <div class="cart-info">
                        <span class="cart-name">${item.name}</span>
                        <span class="cart-price">${formatVND(item.price)}</span>
                    </div>
                    <div class="cart-controls">
                        <button class="qty-btn minus" data-id="${item.id}">−</button>
                        <span class="qty">${item.qty}</span>
                        <button class="qty-btn plus" data-id="${item.id}">+</button>
                        <span class="line-total">${formatVND(lineTotal)}</span>
                        <button class="remove-btn" data-id="${item.id}">&times;</button>
                    </div>
                </div>
            `);
        });

        $("#cartTotal").text(formatVND(total));
        $("#cart-count").text(Object.values(cart).reduce((a, i) => a + i.qty, 0));
        saveCart();
    }

    $(document).on("click", ".order-btn", function () {
        const id = $(this).data("id");
        const name = $(this).data("name");
        const price = Number($(this).data("price"));
        if (!cart[id]) cart[id] = { id, name, price, qty: 0 };
        cart[id].qty++;
        updateCartDisplay();

        const t = $('#toast');
        if (t.length) {
            t.text('Đã thêm "' + name + '" vào giỏ');
            t.addClass('show');
            setTimeout(() => t.removeClass('show'), 1400);
        }
    });

    $(document).on("click", ".qty-btn", function (e) {
        e.stopPropagation();
        const id = $(this).data("id");
        if (!cart[id]) return;
        if ($(this).hasClass("plus")) cart[id].qty++;
        if ($(this).hasClass("minus") && cart[id].qty > 1) cart[id].qty--;
        updateCartDisplay();
    });

    $(document).on("click", ".remove-btn", function (e) {
        e.stopPropagation();
        const id = $(this).data("id");
        delete cart[id];
        updateCartDisplay();
    });

    $("#clearCart").on("click", function (e) {
        e.stopPropagation();
        cart = {};
        updateCartDisplay();
    });

    $("#checkoutBtn").on("click", function (e) {
        e.stopPropagation();
        if (Object.keys(cart).length === 0) {
            alert("Giỏ hàng trống!");
            return;
        }
        saveCart();
        window.location.href = "/Home/DatBan";
    });

    updateCartDisplay();
});
