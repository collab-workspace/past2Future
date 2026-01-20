
import * as THREE from "three";
import { OrbitControls } from "three/addons/controls/OrbitControls.js";

import { getFresnelMat } from "./getFresnelMat.js";
import getStarfield from "./getStarfield.js";

// Entry point: builds the 3D scene (globe + stars) and renders DB events as clickable pins.


// All rendering happens inside #map-container (created in the Home/Index view).

const container = document.getElementById('map-container');

if (container && container.querySelector('p')) {
    container.querySelector('p').style.display = 'none';
}

if (!container) {
    throw new Error("Container error");
}

let containerWidth = container.clientWidth || 800;
let containerHeight = container.clientHeight || 600;

// Scene setup: camera + renderer configured for a globe-like view.
const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera(75, containerWidth / containerHeight, 0.1, 1000);
camera.position.z = 3;

const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false });
renderer.setClearColor(0x000000, 0);
renderer.setPixelRatio(window.devicePixelRatio || 1);
renderer.autoClear = true;

renderer.setSize(containerWidth, containerHeight);
container.appendChild(renderer.domElement);


renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.outputColorSpace = THREE.LinearSRGBColorSpace;


const earthGroup = new THREE.Group();
earthGroup.rotation.z = -23.4 * Math.PI / 180;
scene.add(earthGroup);


// User interaction: orbit controls with damping for smoother rotation.
const controls = new OrbitControls(camera, renderer.domElement);
controls.enableDamping = true;
controls.dampingFactor = 0.05; 
const detail = 12;
const loader = new THREE.TextureLoader();

const texturePath = "/textures/";

const geometry = new THREE.IcosahedronGeometry(1, detail);
const material = new THREE.MeshPhongMaterial({
    map: loader.load(texturePath + "00_earthmap1k.jpg"),
    specularMap: loader.load(texturePath + "02_earthspec1k.jpg"),
    bumpMap: loader.load(texturePath + "01_earthbump1k.jpg"),
    bumpScale: 0.04,
});

material.map.colorSpace = THREE.SRGBColorSpace;

const earthMesh = new THREE.Mesh(geometry, material);
earthGroup.add(earthMesh);

const cloudsMat = new THREE.MeshStandardMaterial({
    map: loader.load(texturePath + "04_earthcloudmap.jpg"),
    transparent: true,
    opacity: 0.5,
    blending: THREE.NormalBlending,
    alphaMap: loader.load(texturePath + '05_earthcloudmaptrans.jpg'),
});
const cloudsMesh = new THREE.Mesh(geometry, cloudsMat);
cloudsMesh.scale.setScalar(1.003);
earthGroup.add(cloudsMesh);

const fresnelMat = getFresnelMat();
const glowMesh = new THREE.Mesh(geometry, fresnelMat);
glowMesh.scale.setScalar(1.01);
earthGroup.add(glowMesh);

const stars = getStarfield({ numStars: 2000 });
scene.add(stars);

const ambientLight = new THREE.AmbientLight(0xffffff, 0.7);
scene.add(ambientLight);

const sunLight = new THREE.DirectionalLight(0xffffff, 1.5);
sunLight.position.set(-2, 0.5, 1.5);
scene.add(sunLight);

// CREATE TOOLTIP ELEMENT
// Tooltip is a plain HTML element positioned near the cursor on hover.
const tooltip = document.createElement('div');
tooltip.className = 'pin-tooltip';
document.body.appendChild(tooltip);


const imgPath = "/img/";



// Data source: window.dbEvents is injected by the server (Razor) into the page.

const rawData = window.dbEvents || [];

// Transform DB rows into a normalized pin model (lat/lon, title, details URL, flag URL).

const countries = rawData.map(item => {

    
    const latVal = Number(item.latitude);
    const lonVal = Number(item.longitude);

   
    if (isNaN(latVal) || isNaN(lonVal)) return null;

    // --- FLAG ---
    let countryClean = "";
    if (item.country) {
        countryClean = item.country.toLowerCase().trim().replace(/ /g, "");
    }
 

    const flagSrc = (item.flagUrl && item.flagUrl.trim() !== "") ? item.flagUrl : null;







    return {
        name: item.country || item.title,
        lat: latVal,
        lon: lonVal + 180, // Longitude offset for your map setup
        page: "/Events/Details/" + item.id,
        summary: item.description ? item.description.substring(0, 50) + "..." : "Click for details",
        flag: flagSrc 
    };
}).filter(item => item !== null); 


// Convert latitude/longitude into a 3D position on a sphere.

function latLonToVector3(lat, lon, radius) {
    const phi = (90 - lat) * (Math.PI / 180);
    const theta = (lon + 180) * (Math.PI / 180);

    const x = -(radius * Math.sin(phi) * Math.cos(theta));
    const z = (radius * Math.sin(phi) * Math.sin(theta));
    const y = (radius * Math.cos(phi));

    return new THREE.Vector3(x, y, z);
}

// Create pin
const pins = [];
const pinRadius = 1.04;

countries.forEach(country => {
    const pinGroup = new THREE.Group();

    // Main pin head
    const headGeometry = new THREE.SphereGeometry(0.025, 16, 16);
    const headMaterial = new THREE.MeshStandardMaterial({
        color: 0x00BFFF,
        emissive: 0x00BFFF,
        emissiveIntensity: 0.4,
        metalness: 0.5,
        roughness: 0.3
    });
    const head = new THREE.Mesh(headGeometry, headMaterial);
    head.position.y = 0.02;
    pinGroup.add(head);

    // Shadow effect
    const ringGeometry = new THREE.RingGeometry(0.025, 0.04, 32);
    const ringMaterial = new THREE.MeshBasicMaterial({
        color: 0x00BFFF,
        transparent: true,
        opacity: 0.4,
        side: THREE.DoubleSide
    });
    const ring = new THREE.Mesh(ringGeometry, ringMaterial);
    ring.rotation.x = -Math.PI / 2;
    pinGroup.add(ring);

    // The bottom part of the pin
    const coneGeometry = new THREE.ConeGeometry(0.015, 0.04, 16);
    const coneMaterial = new THREE.MeshStandardMaterial({
        color: 0x00BFFF,
        emissive: 0x0099CC,
        emissiveIntensity: 0.3,
        metalness: 0.6,
        roughness: 0.2
    });
    const cone = new THREE.Mesh(coneGeometry, coneMaterial);
    cone.position.y = -0.02;
    pinGroup.add(cone);

    // glow dot
    const glowGeometry = new THREE.SphereGeometry(0.01, 16, 16);
    const glowMaterial = new THREE.MeshBasicMaterial({
        color: 0xFFFFFF,
        transparent: true,
        opacity: 0.9
    });
    const glow = new THREE.Mesh(glowGeometry, glowMaterial);
    glow.position.set(0.008, 0.025, 0.008);
    pinGroup.add(glow);

    // glow effect
    const innerGlowGeometry = new THREE.SphereGeometry(0.03, 16, 16);
    const innerGlowMaterial = new THREE.MeshBasicMaterial({
        color: 0x00FFFF,
        transparent: true,
        opacity: 0.15,
        blending: THREE.AdditiveBlending
    });
    const innerGlow = new THREE.Mesh(innerGlowGeometry, innerGlowMaterial);
    innerGlow.position.y = 0.02;
    pinGroup.add(innerGlow);

    // Arrange the position
    const position = latLonToVector3(country.lat, country.lon, pinRadius);
    pinGroup.position.copy(position);

    // Orient the pin toward the globe center
    const direction = position.clone().normalize();
    const up = new THREE.Vector3(0, 1, 0);
    const quaternion = new THREE.Quaternion();
    quaternion.setFromUnitVectors(up, direction);
    pinGroup.setRotationFromQuaternion(quaternion);

   
    pinGroup.userData = {
        country: country.name,
        page: country.page,
        summary: country.summary,
        originalScale: 1,
        ringMesh: ring,
        headMesh: head,
        innerGlowMesh: innerGlow,
        flag: country.flag
    };

    head.userData = pinGroup.userData;
    ring.userData = pinGroup.userData;
    cone.userData = pinGroup.userData;
    glow.userData = pinGroup.userData;
    innerGlow.userData = pinGroup.userData;

    earthGroup.add(pinGroup);
    pins.push(pinGroup);
});


// Raycasting is used to detect which pin is under the mouse cursor.
const raycaster = new THREE.Raycaster();
const mouse = new THREE.Vector2();

// Click: navigate to the event details page for the selected pin.
function onMouseClick(event) {
    const rect = renderer.domElement.getBoundingClientRect();
    mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
    mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

    raycaster.setFromCamera(mouse, camera);
    const intersects = raycaster.intersectObjects(pins, true);

    if (intersects.length > 0) {
        let clickedPin = intersects[0].object;
        while (clickedPin.parent && clickedPin.parent.type !== 'Scene' && clickedPin.parent !== earthGroup) {
            clickedPin = clickedPin.parent;
        }
        if (clickedPin.userData.page) {
            window.location.href = clickedPin.userData.page;
        }
    }
}

let hoveredPin = null;

// Hover: show tooltip and apply subtle pin scaling/glow.
function onMouseMove(event) {
    const rect = renderer.domElement.getBoundingClientRect();
    mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
    mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

    raycaster.setFromCamera(mouse, camera);
    const intersects = raycaster.intersectObjects(pins, true);

    if (intersects.length > 0) {
        let newHovered = intersects[0].object;

        while (
            newHovered.parent &&
            newHovered.parent.type !== "Scene" &&
            newHovered.parent !== earthGroup
        ) {
            newHovered = newHovered.parent;
        }

        if (!newHovered.userData || !newHovered.userData.country) {
            hoveredPin = null;
            tooltip.style.opacity = "0";
            tooltip.style.display = "none";
            renderer.domElement.style.cursor = "default";
            return;
        }

        if (hoveredPin && hoveredPin !== newHovered) {
            hoveredPin.userData.hovered = false;
        }
        hoveredPin = newHovered;
        hoveredPin.userData.hovered = true;

        const country = hoveredPin.userData.country;
        const summary = hoveredPin.userData.summary || "";
        const flagSrc = hoveredPin.userData.flag || "";

        tooltip.innerHTML = `
            <div style="position: relative; padding-right: 40px;">
                <strong>${country}</strong><br>
                <span style="font-size: 12px; color: #444;">${summary}</span>
                ${flagSrc ? `<img class="pin-tooltip-flag" src="${flagSrc}" alt="${country} flag">` : ""}
            </div>
        `;

        tooltip.style.display = "block";
        tooltip.style.opacity = "1";

        requestAnimationFrame(() => {
            const pad = 10;

            const tw = tooltip.offsetWidth;
            const th = tooltip.offsetHeight;

            let left = event.clientX + 15;
            let top = event.clientY - 60;

           
            left = Math.min(left, window.innerWidth - tw - pad);
            left = Math.max(left, pad);

           
            top = Math.min(top, window.innerHeight - th - pad);
            top = Math.max(top, pad);

            tooltip.style.left = `${left}px`;
            tooltip.style.top = `${top}px`;
        });


        renderer.domElement.style.cursor = "pointer";
    } else {
        if (hoveredPin) {
            hoveredPin.userData.hovered = false;
            hoveredPin = null;
        }

        tooltip.style.opacity = "0";
        setTimeout(() => {
            if (tooltip.style.opacity === "0") {
                tooltip.style.display = "none";
            }
        }, 200);

        renderer.domElement.style.cursor = "default";
    }
}

renderer.domElement.addEventListener('click', onMouseClick, false);
renderer.domElement.addEventListener('mousemove', onMouseMove, false);

// Render loop: updates controls, animates stars/pins, then renders the scene.
function animate() {
    requestAnimationFrame(animate);

    // 1) Update controls (required when damping is enabled)
    controls.update();

    // 2) Starfield rotation
    stars.rotation.y -= 0.0002;
    // 3) Smooth pin animations (lerp)
    pins.forEach(pin => {
        const targetScale = pin.userData.hovered ? 1.5 : 1.0;
        const targetOpacity = pin.userData.hovered ? 0.7 : 0.4;
        const targetInnerOpacity = pin.userData.hovered ? 0.3 : 0.15;

        
        pin.scale.lerp(new THREE.Vector3(targetScale, targetScale, targetScale), 0.1);

        if (pin.userData.ringMesh) {
            pin.userData.ringMesh.material.opacity += (targetOpacity - pin.userData.ringMesh.material.opacity) * 0.1;
        }
        if (pin.userData.innerGlowMesh) {
            pin.userData.innerGlowMesh.material.opacity += (targetInnerOpacity - pin.userData.innerGlowMesh.material.opacity) * 0.1;
        }
    });

    // 4) Render (you can remove manual clear; renderer.autoClear is already true)
    renderer.clear(true, true, true);
    renderer.render(scene, camera);

}

animate();

// Keep the canvas responsive when the container size changes.
function handleWindowResize() {
    const newWidth = container.clientWidth;
    const newHeight = container.clientHeight;

    camera.aspect = newWidth / newHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(newWidth, newHeight);
}

window.addEventListener('resize', handleWindowResize, false);

window.onload = function () {
    handleWindowResize();
};

