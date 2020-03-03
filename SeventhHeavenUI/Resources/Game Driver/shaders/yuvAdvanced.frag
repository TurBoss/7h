#version 110

uniform sampler2D y_tex;
uniform sampler2D u_tex;
uniform sampler2D v_tex;

uniform bool full_range;

const mat3 rgb_transform = mat3(
	1.0,    1.0,   1.0,
	0.0,   -0.344, 1.77,
	1.403, -0.714, 0.0
);

void main()
{
	float y = texture2D(y_tex, gl_TexCoord[0].st).x;
	float u = texture2D(u_tex, gl_TexCoord[0].st).x - 0.5;
	float v = texture2D(v_tex, gl_TexCoord[0].st).x - 0.5;
	
	if(!full_range) y = (y - (1.0 / 255.0) * 16.0) * (255.0 / (255.0 - 16.0));
	
	vec3 yuv_color = vec3(y, u, v);
	vec4 rgba_color = vec4(rgb_transform * yuv_color, 1.0);
	gl_FragColor = rgba_color;
}
