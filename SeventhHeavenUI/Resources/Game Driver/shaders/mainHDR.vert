#version 110

uniform vec3 lights_position[MAX_LIGHTS];
varying vec3 light_reflection[MAX_LIGHTS];
varying vec4 normal_light_vector[MAX_LIGHTS];

uniform vec3 LightDir;
uniform vec4 vViewPosition;
uniform mat4 matViewProjection;

varying vec2 texCoord;
varying vec3 normal;
varying vec3 lightDirInTangent;
varying vec3 viewDirInTangent;

attribute vec3 rm_Tangent;
attribute vec3 rm_Binormal

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
varying vec3 TRed, TGreen, TBlue;
varying vec3 reflectedVector, refractedVector;
varying float refFactor;

uniform float fresnelBias, fresnelScale, fresnelPower;

varying mat3 tbn_matrix;
uniform float etaRatio;

attribute vec3 tangent;
attribute vec3 binormal;

in int gl_VertexID;
in int gl_InstanceID;

out gl_PerVertex {
	vec4 gl_Position;
	float gl_PointSize;
	float gl_ClipDistance[];
};

void main()
{
	texCoord = gl_MultiTexCoord0.xy;
 
    mat3 tangentMat = mat3(rm_Tangent, rm_Binormal, gl_Normal);
    lightDirInTangent = normalize(LightDir) * tangentMat;
    viewDirInTangent  = normalize(vViewPosition-gl_Position).xyz * tangentMat;
	
	vec4 reflectedColor = textureCube(env, R);
	vec4 refractedColor;
	
	/*d3dviewport_matrix = width;
	d3dprojection_matrix = height;
	gl_ModelViewMatrix = size;*/
	
	refractedColor.r = textureCube(env, TRed).r;
	refractedColor.g = textureCube(env, TGreen).g;
	refractedColor.b = textureCube(env, TBlue).b;

	vec3 I = normalize(gl_Vertex.xyz - eyePos.xyz);
	vec3 N = normalize(gl_Normal);
	vec4 pos = gl_Vertex;
	reflectedVector = reflect(I, N);
	refractedVector = refract(I, N, etaRatio);
	
	R = reflect(I, gl_Normal);
	R = refract(I, gl_Normal, etaRatio);
	R = reflect(I, N);
	
	TRed = refract(I, N, etaRatioRGB.r);
	TGreen = refract(I, N, etaRatioRGB.g);
	TBlue = refract(I, N, etaRatioRGB.b);
	
	refFactor = fresnelBias+fresnelScale*pow(1+dot(I,N), fresnelPower);
	
	gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_Position = ftransform();
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
	
	gl_Position = pos;
	gl_FragColor = lightDirInTangent+(mix(refractedColor, reflectedColor, refFactor));
}
