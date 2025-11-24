// Đảm bảo code chỉ chạy khi trang đã tải xong hoàn toàn
$(document).ready(function () {
    // Anti-Forgery Token setup (nếu có)
    var _token = $('form#__AjaxAntiForgeryForm input[name="__RequestVerificationToken"]').val();
    if (_token) {
        $.ajaxSetup({ headers: { 'RequestVerificationToken': _token } });
    }

    // Lấy URL để gọi action 'Add' của 'CartController' từ thuộc tính data-* trên HTML
    const addToCartUrl = $('#productListContainer').data('addToCartUrl');
    const noResultsMessage = $('#noResultsMessage');
    const productList = $('.product-card');

    // ===== LocalStorage Cart helpers =====
    function getStoredCart() {
        try { return JSON.parse(localStorage.getItem('cart') || '[]'); } catch (e) { return []; }
    }
    function setStoredCart(cart) { localStorage.setItem('cart', JSON.stringify(cart || [])); }
    function updateCartCount(cart){
        var c = (cart || getStoredCart()).reduce(function(sum, x){ return sum + (parseInt(x.qty)||0); }, 0);
        var $badge = $('.cart-count');
        if ($badge.length) { $badge.text(c); }
    }
    function addToLocalCart(item){
        var cart = getStoredCart();
        var exist = cart.filter(function(x){ return String(x.id) === String(item.id); })[0];
        if (exist) exist.qty = (parseInt(exist.qty)||0) + (parseInt(item.qty)||1);
        else cart.push({ id: item.id, name: item.name, price: item.price, image: item.image, qty: parseInt(item.qty)||1 });
        setStoredCart(cart);
        updateCartCount(cart);
    }
    function getProductData($btn){
        var $card = $btn.closest('.product-card');
        return {
            id: parseInt($card.data('id')) || 0,
            price: parseInt($card.data('price')) || 0,
            name: ($card.find('.card-title').text() || '').trim(),
            image: $card.find('img').attr('src') || ''
        };
    }

    function updateProductDisplay() {
        const selectedGender = $('input[name="gender"]:checked').val();
        const priceRange = $('#priceRange').val();
        const sortOption = $('#sortOptions').val();
        let visibleCount = 0;

        // --- 1. Lọc sản phẩm ---
        const visibleProducts = productList.filter(function () {
            const $card = $(this);
            const productGender = $card.data('gender').toString();
            const productPrice = parseInt($card.data('price')) || 0;

            const isGenderMatch = (selectedGender === 'Any') || (productGender.toLowerCase() === selectedGender.toLowerCase());

            let isPriceMatch = false;
            if (priceRange === 'all') isPriceMatch = true;
            else if (priceRange === 'lt200') isPriceMatch = productPrice < 200000;
            else if (priceRange === '200to500') isPriceMatch = productPrice >= 200000 && productPrice <= 500000;
            else if (priceRange === 'gt500') isPriceMatch = productPrice > 500000;

            return isGenderMatch && isPriceMatch;
        }).toArray(); // Chuyển kết quả filter thành một mảng để sắp xếp

        // Ẩn tất cả sản phẩm trước khi sắp xếp và hiển thị lại
        productList.hide();

        // --- 2. Sắp xếp các sản phẩm đã lọc ---
        if (sortOption !== 'default' && visibleProducts.length > 0) {
            visibleProducts.sort(function (a, b) {
                const priceA = parseInt($(a).data('price')) || 0;
                const priceB = parseInt($(b).data('price')) || 0;
                const idA = parseInt($(a).data('id')) || 0;
                const idB = parseInt($(b).data('id')) || 0;

                switch (sortOption) {
                    case 'price_asc': return priceA - priceB;
                    case 'price_desc': return priceB - priceA;
                    case 'newest': return idB - idA; // Giả sử ID lớn hơn là sản phẩm mới hơn
                    default: return 0;
                }
            });
        }

        // --- 3. Hiển thị lại sản phẩm và kiểm tra kết quả ---
        if (visibleProducts.length > 0) {
            const productContainer = $('#productListContainer .row');
            // Gắn lại các sản phẩm đã sắp xếp vào container
            $.each(visibleProducts, function (idx, item) {
                productContainer.append(item);
                $(item).show(); // Hiển thị sản phẩm
            });
            visibleCount = visibleProducts.length;
        }

        // Hiển thị hoặc ẩn thông báo "Không có kết quả"
        noResultsMessage.toggle(visibleCount === 0);
    }

    // Gán sự kiện (delegation) cho filter để tránh mất binding nếu DOM thay đổi
    $(document).on('change', '.filter-control', function () { updateProductDisplay(); });

    // Thêm vào giỏ hàng bằng AJAX
    $(document).on('click', '.btn-add-cart', function (e) {
        e.preventDefault();
        var data = getProductData($(this));
        if (!data.id) { return; }

        // 1) Cập nhật localStorage (không chờ server)
        addToLocalCart({ id: data.id, name: data.name, price: data.price, image: data.image, qty: 1 });

        // 2) SweetAlert2 + âm báo
        var beep = document.getElementById('notifySound');
        if (beep && beep.play) { try { beep.play(); } catch(e){} }
        if (window.Swal){
            Swal.fire({ icon:'success', title:'Đã thêm vào giỏ!', text:'Sản phẩm đã được thêm thành công', timer:1600, showConfirmButton:false });
        } else {
            alert('Đã thêm sản phẩm vào giỏ hàng');
        }

        // 3) Gọi server (nếu có URL)
        if (addToCartUrl) {
            $.post(addToCartUrl, { productId: data.id, quantity: 1 })
                .done(function (response) {
                    if (response && response.success) {
                        if (response.cartCount !== undefined) { $('.cart-count').text(response.cartCount); }
                    } else {
                        // Giữ localStorage, chỉ thông báo lỗi server nếu cần
                        if (response && response.message) console.warn(response.message);
                    }
                })
                .fail(function () {
                    // Giữ localStorage, log lỗi kết nối
                    console.warn('Không thể kết nối server để lưu giỏ hàng.');
                });
        }
    });

    // Gọi lần đầu để hiển thị đúng khi tải trang và đồng bộ badge giỏ
    updateProductDisplay();
    updateCartCount();
});