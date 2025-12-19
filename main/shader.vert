#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;
layout (location = 2) in vec3 aInstancePos;

uniform mat4 uView;
uniform mat4 uProjection;

out vec3 vColor;

void main() {
    vec3 worldPos = aPos + aInstancePos;

    gl_Position = uProjection * uView * vec4(worldPos, 1.0);
    vColor = aColor;
}