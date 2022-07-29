#version 330 core

layout(location = 0) out vec4 FragColor;

in vec2 texCoord;

uniform float desaturation = 1f;
uniform float brightness = 0.05f;

uniform sampler2D texture1;

void main()
{
	FragColor = texture(texture1, texCoord);

	float L = 0.3f * FragColor.r + 0.6f * FragColor.g + 0.1f * FragColor.b;
	FragColor.r = (FragColor.r + desaturation * (L - FragColor.r)) * brightness;
	FragColor.g = (FragColor.g + desaturation * (L - FragColor.g)) * brightness;
	FragColor.b = (FragColor.b + desaturation * (L - FragColor.b)) * brightness;
}
