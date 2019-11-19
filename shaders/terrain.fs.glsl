#version 330
#include shadow-factor
#include calculate-light
#include fog

uniform sampler2DArray uTexture;
uniform sampler2DArray uSplatMap;
uniform sampler2D uNormalMap;

uniform int uSplatCount;
uniform int uGridLines;

uniform vec3 uCameraPosition;
uniform vec3 uMousePosition;
uniform float uSelectionSize;

in vec2 gs_texCoord;
in vec3 gs_position;
in vec4 gs_shadowCoords;
in vec3 gs_normal;
in float gs_noise;
in LightVectors gs_lightVectors;

out vec4 outputColor;

float circle() {
    float radius = uSelectionSize;
    float border = 0.08;
    float dist = distance(uMousePosition.xz, gs_position.xz);

    return 1.0 + smoothstep(radius, radius + border, dist)
               - smoothstep(radius - border, radius, dist);
}

float gridLine() {
    if (uGridLines == 0) return 0.0;

    vec2 coord = gs_position.xz;
    vec2 grid = abs(fract(coord - 0.5) - 0.5) / fwidth(coord);
    float line = min(grid.x, grid.y);

    return (1.0 - min(line, 1.0)) / 3;
}


vec3 getTriPlanarTexture(int textureId, vec3 n) {
    vec3 blending = abs(n);
    blending = normalize(max(blending, 0.00001));
    float b = (blending.x + blending.y + blending.z);
    blending /= vec3(b, b, b);

    vec3 yaxis = texture(uTexture, vec3(gs_position.xz * 0.1, textureId)).rgb;
    if (blending.y > 0.3) return yaxis;

    vec3 xaxis = texture(uTexture, vec3(gs_position.yz * 0.1, textureId)).rgb;
    vec3 zaxis = texture(uTexture, vec3(gs_position.xy * 0.1, textureId)).rgb;

    return xaxis * blending.x + yaxis * blending.y + zaxis * blending.z;
}

vec3 finalTexture(int index, vec3 normal, LightVectors lv) {

    vec3 n = normalize(texture(uTexture, vec3(index * 3 + 1)).rgb * 2.0 - 1.0);
    float r = texture(uTexture, vec3(index * 3 + 2)).r;
    return getTriPlanarTexture(index * 3, normal) * calculateLight(lv, n, 1.0, 1.0) * gs_noise;
}

void main() {
    vec3 color = vec3(0);
    for (int i = 0; i < uSplatCount; i++) {
        float intesity = texture(uSplatMap, vec3(gs_texCoord.x, gs_texCoord.y, i)).r;
        if (intesity > 0.02) {
            color += finalTexture(i, gs_normal, gs_lightVectors) * intesity;
        }
    }

    float dist = length(uCameraPosition - gs_position.xyz);
    color *= getShadowFactor(gs_shadowCoords, 0.3);
    color = fog(color, dist);

    vec3 terrainGridLines = mix(color, vec3(0.3, 0.3, 0.3), gridLine());
    outputColor = vec4(mix(vec3(1.0, 1.0, 1.0), terrainGridLines, circle()), 1.0);
}