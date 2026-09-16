// Power Cables Page JavaScript
// Product Data
const cableProducts = [
    // Low Voltage Cables
    {
        id: 1,
        name: 'Single core Copper & Aluminum Conductors',
        voltage: '3.6 / 6 (7.2) kV',
        category: 'low-voltage',
        image: 'cable-image.png'
    },
    {
        id: 2,
        name: 'Armored Steel Wire Cables',
        voltage: '1 / 12 kV',
        category: 'low-voltage',
        image: 'cable-image.png'
    },
    {
        id: 3,
        name: 'Low Voltage Control Cables',
        voltage: '3.6 / 6 kV',
        category: 'low-voltage',
        image: 'cable-image.png'
    },
    {
        id: 4,
        name: 'Hybrid Fiber Optic Cables',
        voltage: 'Up to 10 Gb/s',
        category: 'low-voltage',
        image: 'cable-image.png'
    },
    {
        id: 5,
        name: 'Submersible Pump Cables',
        voltage: '1 / 3 kV',
        category: 'low-voltage',
        image: 'cable-image.png'
    },
    {
        id: 6,
        name: 'Flexible Extension Cords',
        voltage: 'Rated 16A',
        category: 'low-voltage',
        image: 'cable-image.png'
    },

    // Medium Voltage Cables
    {
        id: 7,
        name: 'XLPE Insulated Cables',
        voltage: '12 / 20 kV',
        category: 'medium-voltage',
        image: 'cable-image.png'
    },
    {
        id: 8,
        name: 'Medium Voltage Power Cables',
        voltage: '6 / 10 kV',
        category: 'medium-voltage',
        image: 'cable-image.png'
    },
    {
        id: 9,
        name: 'Armored Medium Voltage Cables',
        voltage: '12 / 20 (24) kV',
        category: 'medium-voltage',
        image: 'cable-image.png'
    },

    // High Voltage Cables
    {
        id: 10,
        name: 'High Voltage XLPE Cables',
        voltage: '66 / 110 kV',
        category: 'high-voltage',
        image: 'cable-image.png'
    },
    {
        id: 11,
        name: 'Extra High Voltage Cables',
        voltage: '132 / 220 kV',
        category: 'high-voltage',
        image: 'cable-image.png'
    },
    {
        id: 12,
        name: 'Submarine Power Cables',
        voltage: '33 / 66 kV',
        category: 'high-voltage',
        image: 'cable-image.png'
    }
];

// State
let currentCategory = 'low-voltage';
let searchQuery = '';

// DOM Elements
const tabButtons = document.querySelectorAll('.cable-tab');
const searchInput = document.getElementById('cableSearch');
const productsGrid = document.getElementById('productsGrid');
const noResults = document.getElementById('noResults');

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    renderProducts();
    setupEventListeners();
});

// Setup Event Listeners
function setupEventListeners() {
    // Tab switching
    tabButtons.forEach(tab => {
        tab.addEventListener('click', () => {
            const category = tab.getAttribute('data-category');
            switchTab(category);
        });
    });

    // Search functionality
    searchInput.addEventListener('input', (e) => {
        searchQuery = e.target.value.toLowerCase();
        renderProducts();
    });
}

// Switch Tab
function switchTab(category) {
    currentCategory = category;

    // Update active tab
    tabButtons.forEach(tab => {
        if (tab.getAttribute('data-category') === category) {
            tab.classList.add('active');
        } else {
            tab.classList.remove('active');
        }
    });

    // Reset search
    searchInput.value = '';
    searchQuery = '';

    // Render products
    renderProducts();
}

// Filter Products
function filterProducts() {
    return cableProducts.filter(product => {
        const matchesCategory = product.category === currentCategory;
        const matchesSearch = product.name.toLowerCase().includes(searchQuery) ||
            product.voltage.toLowerCase().includes(searchQuery);
        return matchesCategory && matchesSearch;
    });
}

// Render Products
function renderProducts() {
    const filteredProducts = filterProducts();

    if (filteredProducts.length === 0) {
        productsGrid.style.display = 'none';
        noResults.style.display = 'block';
        return;
    }

    productsGrid.style.display = 'grid';
    noResults.style.display = 'none';

    productsGrid.innerHTML = filteredProducts.map(product => `
        <div class="cable-product-card">
            <div class="cable-product-info">
                <h3 class="cable-product-title">${product.name}</h3>
                <div class="voltage-badge">
                    <img src="assets/images/bolt-icon.png" alt="Voltage Icon" class="voltage-icon">
                    <span>${product.voltage}</span>
                </div>
                <a href="cable-detail.html" class="btn-view-details">View Details</a>
            </div>
            <div class="cable-product-image">
                <img src="assets/images/cable-bg-ornament.png" alt="" class="cable-bg-ornament">
                <img src="assets/images/cable-product-placeholder.png" style="max-width: 140%;" alt="${product.name}" class="cable-img-main">
            </div>
        </div>
    `).join('');
}
