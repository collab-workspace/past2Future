import * as THREE from "three";

// Fresnel-like shader material used for the subtle atmospheric glow.
export function getFresnelMat() {
    const vertexShader = `
    varying vec3 vNormal;
    varying vec3 vPositionWorld;
    void main() {
      vNormal = normalize(normalMatrix * normal);
      vPositionWorld = (modelMatrix * vec4(position, 1.0)).xyz;
      gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
    }
  `;

    const fragmentShader = `
    uniform vec3 color;
    varying vec3 vNormal;
    varying vec3 vPositionWorld;
    void main() {
      vec3 viewDirection = normalize(cameraPosition - vPositionWorld);
      float intensity = pow(0.55 - dot(vNormal, viewDirection), 4.0);
      gl_FragColor = vec4(color, 1.0) * intensity;
    }
  `;
    // Additive blending + back side rendering produce a soft rim glow around the globe.
    const fresnelMat = new THREE.ShaderMaterial({
        uniforms: {
            color: { value: new THREE.Color(0x00BFFF) }, 
        },
        vertexShader: vertexShader,
        fragmentShader: fragmentShader,
        blending: THREE.AdditiveBlending,
        transparent: true,
        side: THREE.BackSide, // Glow effect
    });

    return fresnelMat;
}