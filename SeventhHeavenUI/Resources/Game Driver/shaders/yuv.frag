#version 110

uniform sampler2D y_tex;
uniform sampler2D u_tex;
uniform sampler2D v_tex;

uniform bool full_range;

const mat3 mpeg_rgb_transform = mat3(
	1.164,  1.164,  1.164,
	0.0,   -0.392,  2.017,
	1.596, -0.813,  0.0
);

const mat3 jpeg_rgb_transform = mat3(
	1.0,  1.0,   1.0,
	0.0, -0.343, 1.765,
	1.4, -0.711, 0.0
);

void main()
{
	float y = texture2D(y_tex, gl_TexCoord[0].st).x;
	float u = texture2D(u_tex, gl_TexCoord[0].st).x - 0.5;
	float v = texture2D(v_tex, gl_TexCoord[0].st).x - 0.5;
	vec3 yuv_color = vec3(y, u, v);
	vec4 rgba_color;
	
	if(full_range) rgba_color = vec4(jpeg_rgb_transform * yuv_color, 1.0);
	else rgba_color = vec4(mpeg_rgb_transform * yuv_color, 1.0);
	
	gl_FragColor = rgba_color;
}
