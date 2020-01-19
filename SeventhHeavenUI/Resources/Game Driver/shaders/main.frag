#version 110

uniform int vertextype;

uniform sampler2D tex;
uniform bool texture;
uniform bool fb_texture;
uniform bool modulate_alpha;

void main()
{
	vec4 color = gl_Color;
	vec4 texture_color;
	
	texture_color = texture2D(tex, gl_TexCoord[0].st);
	
	if(texture)
	{
		if(fb_texture && texture_color.rgb == vec3(0.0, 0.0, 0.0)) discard;
		
		if(texture_color.a == 0.0) discard;
		
		color *= texture_color;
		
		if(!modulate_alpha) color.a = texture_color.a;
	}
	
	gl_FragColor = color;
}
