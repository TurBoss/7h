#version 110

uniform int vertextype;
uniform bool fb_texture;
uniform int blend_mode;

uniform mat4 d3dprojection_matrix;
uniform mat4 d3dviewport_matrix;

void main()
{
	vec4 pos = gl_Vertex;
	
	gl_FrontColor.rgba = gl_Color.bgra;
	
	// TLVERTEX
	if(vertextype == 3)
	{
		pos.w = 1.0 / pos.w;
		pos.xyz *= pos.w;
		pos = gl_ProjectionMatrix * pos;
	}
	// LVERTEX and VERTEX
	else
	{
		pos = d3dviewport_matrix * d3dprojection_matrix * gl_ModelViewMatrix * pos;
		
		if(gl_FrontColor.a > 0.5) gl_FrontColor.a = 0.5;
	}
	
	// BLEND_NONE
	if(blend_mode == 4) gl_FrontColor.a = 1.0;
	// BLEND_25P
	else if(blend_mode == 3) gl_FrontColor.a = 0.25;
	
	gl_TexCoord[0] = gl_MultiTexCoord0;
	
	if(fb_texture) gl_TexCoord[0].t = 1.0 - gl_TexCoord[0].t;
	
	gl_Position = pos;
}
