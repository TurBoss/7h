#version 110	//HDR GLSL vertex Shader - rewritten by Asmodean

uniform vec3 lights_position[MAX_LIGHTS];
varying vec3 light_reflection[MAX_LIGHTS];
varying vec4 normal_light_vector[MAX_LIGHTS];

varying vec3 lightDir;
uniform vec4 vViewPosition;
uniform mat4 matViewProjection;

varying vec2 texCoord;
varying vec3 normal;
attribute vec3 intan;
varying vec3 viewDir;

attribute vec3 rm_Tangent;
attribute vec3 rm_Binormal;

uniform bool normal_map;
uniform int vertextype;
uniform bool fb_texture;
uniform int blend_mode;

uniform mat4 d3dprojection_matrix;
uniform mat4 d3dviewport_matrix;

uniform vec3 etaRatioRGB;
varying vec3 eye_vector;
uniform vec3 eyePos;
varying vec3 R;
uniform samplerCube Environment;
varying vec3 TRed, TGreen, TBlue;
varying vec3 reflectedVector, refractedVector;
varying float refFactor;

uniform float fresnelBias, fresnelScale, fresnelPower;

varying mat3 tbn_matrix;
uniform float etaRatio;

attribute vec3 tangent;
attribute vec3 binormal;

void main()
{
	gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
	gl_TexCoord[0] = gl_MultiTexCoord0;

	vec3 vertexPos = vec3(gl_ModelViewMatrix * gl_Vertex);

	vec3 n = normalize(gl_NormalMatrix * gl_Normal);
	vec3 t = normalize(gl_NormalMatrix * intan);
	vec3 b = cross(n, t) ;

	mat3 tbnMatrix = mat3(t.x, b.x, n.x,
						  t.y, b.y, n.y,
						  t.z, b.z, n.z);

	lightDir = (gl_LightSource[1].position.xyz - vertexPos) / 1000.;
	lightDir = tbnMatrix * lightDir;

	viewDir = -vertexPos; 
	viewDir = tbnMatrix * viewDir;

	texCoord = gl_MultiTexCoord0.xy;
	vec4 pos = gl_Vertex;
	//gl_Position = pos;
    mat3 tangentMat = mat3(rm_Tangent, rm_Binormal, gl_Normal);
    lightDir = normalize(lightDir) * tangentMat;
    viewDir  = normalize(vViewPosition-gl_Position).xyz * tangentMat;
	
	vec4 reflectedColor = textureCube(Environment, R);
	vec4 refractedColor;
	
	/*d3dviewport_matrix = width;
	d3dprojection_matrix = height;
	gl_ModelViewMatrix = size;*/
	
	refractedColor.r = textureCube(Environment, TRed).r;
	refractedColor.g = textureCube(Environment, TGreen).g;
	refractedColor.b = textureCube(Environment, TBlue).b;

	vec3 I = normalize(gl_Vertex.xyz - eyePos.xyz);
	vec3 N = normalize(gl_Normal);
	reflectedVector = reflect(I, N);
	refractedVector = refract(I, N, etaRatio);
	
	R = reflect(I, gl_Normal);
	R = refract(I, gl_Normal, etaRatio);
	R = reflect(I, N);
	
	TRed = refract(I, N, etaRatioRGB.r);
	TGreen = refract(I, N, etaRatioRGB.g);
	TBlue = refract(I, N, etaRatioRGB.b);
	
	refFactor = fresnelBias+fresnelScale*pow(1.0+dot(I,N), fresnelPower);
	
	gl_TexCoord[0] = gl_MultiTexCoord0;
	//gl_Position = ftransform();
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
		
		if(vertextype == 1)
		{
			int i;
			
			eye_vector = vec3(gl_ModelViewMatrix * gl_Vertex);
			
			if(normal_map) tbn_matrix = mat3(normalize(gl_NormalMatrix * tangent), normalize(gl_NormalMatrix * binormal), normalize(gl_NormalMatrix * gl_Normal));
			else tbn_matrix[2] = normalize(gl_NormalMatrix * gl_Normal);
			
			for(i = 0; i < MAX_LIGHTS; i++)
			{
				vec3 light_vector = vec3(lights_position[i] - eye_vector);
				normal_light_vector[i].xyz = normalize(light_vector);
				
				if(!normal_map) light_reflection[i] = reflect(normal_light_vector[i].xyz, tbn_matrix[2]);
				
				normal_light_vector[i].w = length(light_vector);
			}
		}
		
		gl_FrontColor.a = clamp(gl_FrontColor.a, 0.0, 0.5);
	}
	
	// BLEND_NONE
	if(blend_mode == 4) gl_FrontColor.a = 1.0;
	// BLEND_25P
	else if(blend_mode == 3) gl_FrontColor.a = 0.25;
	
	gl_TexCoord[0] = gl_MultiTexCoord0;
	
	if(fb_texture) gl_TexCoord[0].t = 1.0 - gl_TexCoord[0].t;
	
	//gl_FragColor =(mix(refractedColor, reflectedColor, refFactor));
}
