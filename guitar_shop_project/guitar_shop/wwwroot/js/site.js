// Cart dropdown functionality
document.addEventListener('DOMContentLoaded', () => {
    const cartLink = document.querySelector('.nav-kor-img');
    const cartDropdown = document.getElementById('cartDropdown');
    
    if (cartLink && cartDropdown) {
        cartLink.addEventListener('mouseenter', loadCartPreview);
        cartLink.addEventListener('click', (e) => {
            e.preventDefault();
            loadCartPreview();
            cartDropdown.classList.toggle('active');
        });
        
        document.addEventListener('click', (e) => {
            if (!cartDropdown.contains(e.target) && !cartLink.contains(e.target)) {
                cartDropdown.classList.remove('active');
            }
        });
    }
    
    updateCartCount();
});

async function loadCartPreview() {
    const preview = document.getElementById('cart-preview');
    if (!preview) return;
    
    try {
        const res = await fetch('/Cart/GetCartSummary');
        const data = await res.json();
        
        if (data.count === 0) {
            preview.innerHTML = '<p>Корзина пуста</p>';
            document.getElementById('cart-count').textContent = '0';
            return;
        }
        
        document.getElementById('cart-count').textContent = data.count;
        preview.innerHTML = data.items.map(item => `
            <div class="cart-menu-list-product">
                <span>${item.guitarName}</span>
                <span>${item.price}$ × ${item.quantity}</span>
            </div>
        `).join('') + `<div class="cart-menu-all-price"><strong>Итого: ${data.total}$</strong></div>`;
    } catch (e) {
        preview.innerHTML = '<p>Ошибка загрузки</p>';
    }
}

async function updateCartCount() {
    try {
        const res = await fetch('/Cart/GetCartSummary');
        const data = await res.json();
        const badge = document.getElementById('cart-count');
        if (badge) badge.textContent = data.count;
    } catch (e) {
        console.error('Cart count error:', e);
    }
}

// Back button with history
function goBack() {
    if (document.referrer && document.referrer.includes(window.location.hostname)) {
        history.back();
    } else {
        window.location.href = '/';
    }
}