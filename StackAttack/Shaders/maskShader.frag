#version 330 core

layout(location = 0) out vec4 FragColor;

in vec2 texCoord;

uniform sampler2D texture1;
uniform sampler2D texture2;

void main()
{
	vec4 mainColor = texture(texture1, texCoord);
	vec4 maskColor = texture(texture2, texCoord);
	FragColor = vec4(mainColor.r * maskColor.a, mainColor.g * maskColor.a, mainColor.b * maskColor.a, mainColor.a * maskColor.a);
}
