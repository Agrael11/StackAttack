#version 330 core

layout(location = 0) out vec4 FragColor;

in vec2 texCoord;

uniform sampler2D texture1;

void main()
{
	FragColor = texture(texture1, texCoord);
}
