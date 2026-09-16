// Description SVG Script
// Handles SVG loading and any interactive features

document.addEventListener('DOMContentLoaded', function() {
    const svgObject = document.querySelector('.description-svg');
    
    // Handle SVG load event
    if (svgObject) {
        svgObject.addEventListener('load', function() {
            console.log('SVG loaded successfully');
            
            // Get the SVG document
            const svgDoc = svgObject.contentDocument;
            if (svgDoc) {
                // You can add interactive features here if needed
                // For example, adding click handlers to SVG elements
                initializeSVGInteractions(svgDoc);
            }
        });
        
        // Handle SVG load errors
        svgObject.addEventListener('error', function() {
            console.error('Error loading SVG');
            showErrorMessage();
        });
    }
    
    // Initialize SVG interactions if needed
    function initializeSVGInteractions(svgDoc) {
        // Example: Add hover effects or click handlers
        // const paths = svgDoc.querySelectorAll('path');
        // paths.forEach(path => {
        //     path.addEventListener('mouseenter', function() {
        //         this.style.opacity = '0.8';
        //     });
        //     path.addEventListener('mouseleave', function() {
        //         this.style.opacity = '1';
        //     });
        // });
    }
    
    // Show error message if SVG fails to load
    function showErrorMessage() {
        const wrapper = document.querySelector('.description-wrapper');
        if (wrapper) {
            wrapper.innerHTML = `
                <div style="padding: 40px; text-align: center; color: #666;">
                    <h2>Unable to load description</h2>
                    <p>The SVG file could not be loaded. Please ensure the file exists and try again.</p>
                </div>
            `;
        }
    }
    
    // Optional: Add zoom functionality
    let scale = 1;
    const zoomIn = () => {
        scale += 0.1;
        if (svgObject) {
            svgObject.style.transform = `scale(${scale})`;
            svgObject.style.transformOrigin = 'top left';
        }
    };
    
    const zoomOut = () => {
        scale = Math.max(0.5, scale - 0.1);
        if (svgObject) {
            svgObject.style.transform = `scale(${scale})`;
            svgObject.style.transformOrigin = 'top left';
        }
    };
    
    const resetZoom = () => {
        scale = 1;
        if (svgObject) {
            svgObject.style.transform = 'scale(1)';
        }
    };
    
    // Keyboard shortcuts for zoom (optional)
    document.addEventListener('keydown', function(e) {
        if (e.ctrlKey || e.metaKey) {
            if (e.key === '+') {
                e.preventDefault();
                zoomIn();
            } else if (e.key === '-') {
                e.preventDefault();
                zoomOut();
            } else if (e.key === '0') {
                e.preventDefault();
                resetZoom();
            }
        }
    });
});

