function resolveAssetUrl(path) {
  if (!path || path.indexOf('~/') !== 0) {
    return path;
  }
  var root = window.APP_ROOT || '';
  return root + path.substring(1);
}

function parseDataArray(value) {
  if (!value) {
    return [];
  }
  try {
    return JSON.parse(value.replace(/'/g, '"'));
  } catch (error) {
    console.error('Failed to parse hotspot data:', value, error);
    return [];
  }
}

function getCableSvgs() {
  return document.querySelectorAll('svg.svg');
}

function hideAllCableSvgs() {
  getCableSvgs().forEach(function (svg) {
    svg.style.display = 'none';
  });
}

function showCableSvg(index, title) {
  var svgs = getCableSvgs();
  var titleText = document.getElementById('titleText');

  hideAllCableSvgs();

  if (svgs[index]) {
    svgs[index].style.display = 'block';
  }

  if (titleText && title) {
    titleText.textContent = title;
  }
}

function fixSvgAssetPaths() {
  getCableSvgs().forEach(function (svg) {
    svg.querySelectorAll('image[href^="~/"]').forEach(function (img) {
      img.setAttribute('href', resolveAssetUrl(img.getAttribute('href')));
    });
  });

  document.querySelectorAll('.hotspot[data-map^="~/"], .hotspot[data-flag]').forEach(function (el) {
    var map = el.getAttribute('data-map');
    if (map && map.indexOf('~/') === 0) {
      el.setAttribute('data-map', resolveAssetUrl(map));
    }

    var flags = el.getAttribute('data-flag');
    if (flags && flags.indexOf('~/') !== -1) {
      el.setAttribute('data-flag', flags.replace(/~\//g, (window.APP_ROOT || '') + '/'));
    }
  });
}

function canUseMagnificPopup() {
  return typeof $ !== 'undefined'
    && $.magnificPopup
    && typeof $.magnificPopup.open === 'function';
}

function showNativeHotspotPopup() {
  var popup = document.getElementById('hotspot-popup');
  if (!popup) {
    return;
  }

  popup.classList.remove('mfp-hide');
  popup.classList.add('hotspot-popup-active');
  document.body.classList.add('hotspot-popup-open');
}

function hideNativeHotspotPopup() {
  var popup = document.getElementById('hotspot-popup');
  if (!popup) {
    return;
  }

  popup.classList.add('mfp-hide');
  popup.classList.remove('hotspot-popup-active');
  document.body.classList.remove('hotspot-popup-open');
}

function openHotspotPopup(hotspot) {
  var popup = document.getElementById('hotspot-popup');
  if (!popup) {
    return;
  }

  var cableTitle = hotspot.getAttribute('data-title') || '';
  var map = resolveAssetUrl(hotspot.getAttribute('data-map'));
  var mapTitle = hotspot.getAttribute('data-map-title') || '';
  var mapSupTitle = hotspot.getAttribute('data-material') || '';
  var list = parseDataArray(hotspot.getAttribute('data-li'));
  var flags = parseDataArray(hotspot.getAttribute('data-flag'));

  popup.querySelector('#popup-material').textContent = mapTitle || cableTitle;
  popup.querySelector('#map-title').textContent = mapTitle || '';
  popup.querySelector('#map-suptitle').textContent = mapSupTitle || '';
  popup.querySelector('#popup-image').style.backgroundImage = map
    ? "url('" + map.replace(/'/g, '%27') + "')"
    : 'none';

  var ul = popup.querySelector('#popup-list');
  ul.innerHTML = '';

  var listTitle = popup.querySelector('#list-title');
  listTitle.textContent = list.length ? 'Raw Material Sourced From' : '';

  list.forEach(function (country, index) {
    var li = document.createElement('li');
    li.classList.add('pb-5px');

    var img = document.createElement('img');
    img.src = resolveAssetUrl(flags[index]) || '';
    img.className = 'w-20px me-10px';

    li.appendChild(img);
    li.appendChild(document.createTextNode(country));
    ul.appendChild(li);
  });

  if (canUseMagnificPopup()) {
    $.magnificPopup.open({
      items: {
        src: '#hotspot-popup'
      },
      type: 'inline',
      midClick: true,
      mainClass: 'my-mfp-zoom-in',
      removalDelay: 300,
      fixedContentPos: true,
      closeBtnInside: true
    });
    return;
  }

  showNativeHotspotPopup();
}

function setupHotspots() {
  document.addEventListener('click', function (event) {
    var hotspot = event.target.closest('.hotspot');
    if (!hotspot) {
      return;
    }

    var svg = hotspot.closest('svg.svg');
    if (!svg || window.getComputedStyle(svg).display === 'none') {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    openHotspotPopup(hotspot);
  });
}

function setupPopupCloseHandlers() {
  document.addEventListener('click', function (event) {
    if (event.target.closest('.mfp-close')) {
      event.preventDefault();
      if (canUseMagnificPopup()) {
        $.magnificPopup.close();
      } else {
        hideNativeHotspotPopup();
      }
    }
  });

  document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape' && document.body.classList.contains('hotspot-popup-open')) {
      hideNativeHotspotPopup();
    }
  });
}

function setupSVGLinks() {
  fixSvgAssetPaths();

  var links = document.querySelectorAll('a.svg-link');

  links.forEach(function (link, index) {
    var title = link.getAttribute('data-title');

    link.addEventListener('click', function (event) {
      event.preventDefault();
      showCableSvg(index, title);
    });

    var figure = link.closest('figure');
    if (figure) {
      figure.style.cursor = 'pointer';
      figure.addEventListener('click', function (event) {
        if (event.target.closest('a.svg-link')) {
          return;
        }
        event.preventDefault();
        showCableSvg(index, title);
      });
    }
  });

  if (links.length > 0) {
    showCableSvg(0, links[0].getAttribute('data-title'));
  }
}

function initSupplyChainPage() {
  setupHotspots();
  setupPopupCloseHandlers();
  setupSVGLinks();
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initSupplyChainPage);
} else {
  initSupplyChainPage();
}
