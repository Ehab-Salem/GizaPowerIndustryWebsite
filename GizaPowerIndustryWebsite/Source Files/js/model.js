$(document).ready(function () {
  // Initialize Magnific Popup for hotspots
  $('.hotspot').on('click', function (e) {
    e.preventDefault();
    e.stopPropagation();

    // Get data from the clicked hotspot
    var title = $(this).attr('data-title');

    var map = $(this).attr('data-map');
    let list = this.getAttribute('data-li');
    var mapTitle = $(this).attr('data-map-title');
    var mapSupTitle = $(this).attr('data-material');
    list = JSON.parse(list.replace(/'/g, '"'));

    // parse flags list
    let flags = this.getAttribute('data-flag');
    flags = JSON.parse(flags.replace(/'/g, '"'));
          const popup = document.getElementById('hotspot-popup');

      // title
    
      // material
      popup.querySelector('#popup-material').textContent = title || "";

      const ul = popup.querySelector('#popup-list');
    ul.innerHTML = '';

    listTitle=popup.querySelector('#list-title');
    if(list.length){
      listTitle.textContent="Raw Material Sourced From";
    }
    else{
      listTitle.textContent="";
    }

    list.forEach((country, index) => {
      const li = document.createElement('li');
      li.classList.add('pb-5px');

      const img = document.createElement('img');
      img.src = flags[index] || '';
      img.className = 'w-20px me-10px';
      // img.alt = country;

      li.appendChild(img);
      li.appendChild(document.createTextNode(country));

      ul.appendChild(li);
    });
      // list (array → readable string)

      // map image (background image)
      popup.querySelector('#map-title').textContent=mapTitle || "";
      popup.querySelector('#map-suptitle').textContent=mapSupTitle || "";
      popup.querySelector('#popup-image').style.backgroundImage = `url('${map}')`;


    // Update the popup content dynamically
    // Targeting the h4 for title and p for description inside the popup
    
    

    // Open the popup using Magnific Popup
    $.magnificPopup.open({
      items: {
        src: '#hotspot-popup'
      },
      type: 'inline',
      midClick: true, // Allow opening popup on middle mouse click. Always set it to true if you don't provide alternative source in href.
      mainClass: 'my-mfp-zoom-in', // Animation class
      removalDelay: 300,
      callbacks: {
        open: function () {
          // Optional: Animation or focus logic
        }
      }
    });
  });

  // Ensure the popup close button works (Magnific Popup handles standard .mfp-close, but we ensure it)
  $(document).on('click', '.mfp-close', function (e) {
    e.preventDefault();
    $.magnificPopup.close();
  });
});

function hideAllSVGs() {
  const svgs = document.querySelectorAll('svg');
  svgs.forEach(svg => {
    svg.style.display = 'none';
  });
}

function setupSVGLinks() {
  // Select all anchor tags with class 'svg-link'
  const links = document.querySelectorAll('a.svg-link');
  const svgs = document.querySelectorAll('svg');
const titleText = document.getElementById("titleText");
  links.forEach((link, index) => {
    link.addEventListener('click', function(event) {
      event.preventDefault(); // Prevent default anchor behavior

      // Hide all SVGs
      hideAllSVGs();

      // Show the corresponding SVG if it exists
      var title = $(this).attr('data-title');
      if (svgs[index]) {
        svgs[index].style.display = 'block';
        titleText.textContent=title;
      }
    });
  });
}

// Call this function once the DOM is loaded
document.addEventListener('DOMContentLoaded', setupSVGLinks);
