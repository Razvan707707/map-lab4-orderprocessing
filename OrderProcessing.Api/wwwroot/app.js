// State local
const state = { orders: [], selectedId: null };

// Inițializare
document.addEventListener('DOMContentLoaded', () => {
    fetchOrders();

    // Evenimente UI Modal
    document.getElementById('btn-new-order').addEventListener('click', () => document.getElementById('modal').classList.remove('hidden'));
    document.getElementById('btn-close-modal').addEventListener('click', () => document.getElementById('modal').classList.add('hidden'));
    document.getElementById('new-order-form').addEventListener('submit', createOrder);

    // Evenimente butoane acțiuni
    document.querySelectorAll('.btn-action').forEach(btn => {
        btn.addEventListener('click', (e) => triggerAction(state.selectedId, e.target.dataset.action));
    });
});

// GET Toate Comenzile
async function fetchOrders() {
    try {
        const res = await fetch('/orders');
        state.orders = await res.json();
        renderOrdersList();

        if (state.selectedId) {
            const stillExists = state.orders.find(o => o.id === state.selectedId);
            if (stillExists) fetchOrder(state.selectedId);
        }
    } catch (e) { showToast('Eroare la încărcarea comenzilor', true); }
}

// GET Detalii Comandă
async function fetchOrder(id) {
    try {
        const res = await fetch(`/orders/${id}`);
        const order = await res.json();
        state.selectedId = id;
        renderOrdersList(); // refresh highlight
        renderOrderDetails(order);
    } catch (e) { showToast('Eroare la încărcarea detaliilor', true); }
}

// POST Creare Comandă
async function createOrder(e) {
    e.preventDefault();

    const select = document.getElementById('input-product');
    const option = select.options[select.selectedIndex];

    const payload = {
        customer: {
            name: document.getElementById('input-name').value,
            email: "contact@email.com",
            age: parseInt(document.getElementById('input-age').value),
            isTrusted: document.getElementById('input-trusted').checked
        },
        address: { street: "Str. Principala 1", city: "Bucuresti", zipCode: "010101", country: "RO" },
        items: [{
            productId: option.value,
            productName: option.text.split('(')[0].trim(),
            quantity: parseInt(document.getElementById('input-qty').value),
            unitPrice: parseFloat(option.dataset.price),
            hasAgeRestriction: option.dataset.age === 'true'
        }]
    };

    try {
        const res = await fetch('/orders', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.status === 201) {
            showToast('Comandă creată cu succes!', false);
            document.getElementById('modal').classList.add('hidden');
            fetchOrders();
        } else if (res.status === 400) {
            const errors = await res.json();
            showToast(errors[0], true); // Arătăm mesajul de la Chain of Responsibility
        }
    } catch (e) { showToast('Eroare rețea.', true); }
}

// POST Acțiune Tranziție
async function triggerAction(id, action) {
    if (!id) return;
    try {
        const res = await fetch(`/orders/${id}/${action}`, { method: 'POST' });
        if (res.status === 200) {
            const updatedOrder = await res.json();
            showToast(`Acțiunea '${action}' reușită!`, false);
            fetchOrder(id);
            fetchOrders();
        } else if (res.status === 409) {
            const errMsg = await res.text();
            showToast(errMsg.replace(/"/g, ''), true); // Arătăm mesajul de la State Pattern
        }
    } catch (e) { showToast('Eroare la procesare.', true); }
}

// RENDER: Lista din stânga
function renderOrdersList() {
    const list = document.getElementById('orders-list');
    document.getElementById('orders-count').innerText = `· ${state.orders.length}`;
    list.innerHTML = '';

    state.orders.forEach(o => {
        const li = document.createElement('li');
        li.className = `order-item ${o.id === state.selectedId ? 'active' : ''}`;
        li.innerHTML = `
            <div class="order-item-header">
                <strong>#${o.id.split('-')[0]}...</strong>
                <span class="badge ${o.status.toLowerCase()}">${o.status}</span>
            </div>
            <div style="font-size: 0.8rem; color: var(--text-muted)">${o.customer.name} - ${o.total} RON</div>
        `;
        li.onclick = () => fetchOrder(o.id);
        list.appendChild(li);
    });
}

// RENDER: Detalii dreapta
function renderOrderDetails(o) {
    document.getElementById('no-selection').classList.add('hidden');
    document.getElementById('order-details').classList.remove('hidden');

    document.getElementById('detail-id').innerText = o.id;
    document.getElementById('detail-status').className = `badge ${o.status.toLowerCase()}`;
    document.getElementById('detail-status').innerText = o.status;
    document.getElementById('detail-customer').innerText = `${o.customer.name} (${o.customer.age} ani)`;
    document.getElementById('detail-items').innerText = `${o.items.length} produse · ${o.total} RON`;
    document.getElementById('detail-address').innerText = `${o.address.street}, ${o.address.city}`;

    // Update diagramă inline
    const states = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered'];
    let html = '';
    states.forEach((s, i) => {
        const isActive = o.status === s;
        html += `<span class="state-node ${isActive ? 'active' : ''}">${s}</span>`;
        if (i < states.length - 1) html += `<span class="state-arrow">→</span>`;
    });
    if (o.status === 'Cancelled') {
        html += `<span class="state-arrow">→</span><span class="state-node active" style="background:var(--danger);color:white">Cancelled</span>`;
    }
    document.getElementById('state-diagram').innerHTML = html;

    // Update butoane contextuale
    const s = o.status;
    const btn = (action, enabled) => {
        const b = document.querySelector(`button[data-action="${action}"]`);
        b.disabled = !enabled;
    };

    btn('pay', s === 'Pending');
    btn('process', s === 'Confirmed');
    btn('ship', s === 'Processing');
    btn('deliver', s === 'Shipped');
    btn('cancel', ['Pending', 'Confirmed', 'Processing'].includes(s));

    // Update history
    const historyList = document.getElementById('history-list');
    historyList.innerHTML = '';
    [...o.history].reverse().forEach(h => {
        const li = document.createElement('li');
        li.innerText = h;
        historyList.appendChild(li);
    });
}

// Helper Toast Notification
function showToast(message, isError) {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${isError ? 'error' : 'success'}`;
    toast.innerText = message;
    toast.onclick = () => toast.remove();
    container.appendChild(toast);
    setTimeout(() => toast.remove(), 4000);
}