import * as THREE from "three";
export default function getStarfield({ numStars = 500 } = {}) {

    // Random point on a spherical shell to distribute stars around the camera.
    function randomSpherePoint() {
        const radius = Math.random() * 25 + 25;
        const u = Math.random();
        const v = Math.random();
        const theta = 2 * Math.PI * u;
        const phi = Math.acos(2 * v - 1);
        let x = radius * Math.sin(phi) * Math.cos(theta);
        let y = radius * Math.sin(phi) * Math.sin(theta);
        let z = radius * Math.cos(phi);
        return {
            pos: new THREE.Vector3(x, y, z),
            hue: 0.6,
            saturation: 0.5,
        };
    }

    const verts = [];
    const colors = [];
    const geometry = new THREE.BufferGeometry();
    for (let i = 0; i < numStars; i++) {
        const p = randomSpherePoint();
        const { pos, hue, saturation } = p;
        verts.push(pos.x, pos.y, pos.z);
        colors.push(hue, saturation, 1);
    }
    const positions = new THREE.Float32BufferAttribute(verts, 3);
    const colorsAttribute = new THREE.Float32BufferAttribute(colors, 3);

    geometry.setAttribute("position", positions);
    geometry.setAttribute("color", colorsAttribute);

    const material = new THREE.PointsMaterial({
        size: 0.15,
        vertexColors: true,
        transparent: true,
        blending: THREE.AdditiveBlending,
    });

    const points = new THREE.Points(geometry, material);
    return points;
}
