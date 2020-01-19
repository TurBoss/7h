#version 110

uniform bool lights_active[MAX_LIGHTS];
uniform vec3 lights_ambient[MAX_LIGHTS];
uniform vec3 lights_diffuse[MAX_LIGHTS];
uniform vec3 lights_specular[MAX_LIGHTS];
uniform vec3 lights_spotdir[MAX_LIGHTS];
uniform float lights_spotexp[MAX_LIGHTS];
uniform float lights_spotcutoff[MAX_LIGHTS];
uniform vec3 lights_att[MAX_LIGHTS];
varying vec4 normal_light_vector[MAX_LIGHTS];
varying vec3 light_reflection[MAX_LIGHTS];
uniform vec4 MipMix
varying vec3 lightDirInTangent;
varying vec3 viewDirInTangent;

uniform sampler2D source;
uniform float coefficients[25];
uniform float offset;
uniform vec4 matColor;
uniform samplerCube env;
uniform float reflectionFactor;
varying vec3 R, TRed, TGreen, TBlue;
varying vec3 reflectedVector, refractedVector;
varying float refFactor;
varying vec3 eye_vector;
varying mat3 tbn_matrix;

const float texDimension = 8192.0;
const float texScaler =  1.0/texDimension;
const float texOffset = -0.5/texDimension;

struct material
{
	bool active;
	vec3 emission;
	vec3 diffuse;
	vec3 specular;
	float shininess;
};

uniform material current_material;

uniform int vertextype;

uniform sampler2D tex, bloom;
uniform sampler2D nmap;
uniform sampler2D smap;
uniform sampler2D SrcColor;
uniform sampler2D Src;
uniform sampler2D SrcHDR;
uniform sampler2D SrcHDR1;
uniform sampler2D SrcHDR2;
uniform sampler2D SrcHDR3;
uniform sampler2D SrcHDR4;
uniform sampler2D Measure;
uniform sampler2D BumpMap;
uniform sampler2D ObjectMap;
uniform sampler2D SpecMap;

uniform bool normal_map;
uniform bool specular_map;
uniform bool texture;
uniform bool fb_texture;

uniform float exposure;
uniform float bloomFactor;
uniform float brightMax;
float gloomIntensity=1.0;

const vec4 gloomStart = vec4(0.95,0.95,0.95,0.95);

float sqr(float x) { return x*x; }
vec4 sqr(vec4 x) { return x*x; }

vec4 expand_Hdr(vec4 color)
{
return color*(sqr(color.a*2.0)+1.0);
}

void main()
{
	float d = 0.1;
    float left = gl_TexCoord[0].s - offset - offset;
    float top = gl_TexCoord[0].t - offset - offset;
    vec2 tc = vec2(left, top);
    vec4 c = vec4(0, 0, 0, 0);

    c += coefficients[ 0] * texture2D(source, tc); tc.x += offset;
    c += coefficients[ 1] * texture2D(source, tc); tc.x += offset;
    c += coefficients[ 2] * texture2D(source, tc); tc.x += offset;
    c += coefficients[ 3] * texture2D(source, tc); tc.x += offset;
    c += coefficients[ 4] * texture2D(source, tc); tc.y += offset;
    tc.x = left;
    c += coefficients[ 5] * texture2D(source, tc); tc.x += offset;
    c += coefficients[ 6] * texture2D(source, tc); tc.x += offset;
    c += coefficients[ 7] * texture2D(source, tc); tc.x += offset;
    c += coefficients[ 8] * texture2D(source, tc); tc.x += offset;
    c += coefficients[ 9] * texture2D(source, tc); tc.y += offset;
    tc.x = left;
    c += coefficients[10] * texture2D(source, tc); tc.x += offset;
    c += coefficients[11] * texture2D(source, tc); tc.x += offset;
    c += coefficients[12] * texture2D(source, tc); tc.x += offset;
    c += coefficients[13] * texture2D(source, tc); tc.x += offset;
    c += coefficients[14] * texture2D(source, tc); tc.y += offset;
    tc.x = left;
    c += coefficients[15] * texture2D(source, tc); tc.x += offset;
    c += coefficients[16] * texture2D(source, tc); tc.x += offset;
    c += coefficients[17] * texture2D(source, tc); tc.x += offset;
    c += coefficients[18] * texture2D(source, tc); tc.x += offset;
    c += coefficients[19] * texture2D(source, tc); tc.y += offset;
    tc.x = left;
    c += coefficients[20] * texture2D(source, tc); tc.x += offset;
    c += coefficients[21] * texture2D(source, tc); tc.x += offset;
    c += coefficients[22] * texture2D(source, tc); tc.x += offset;
    c += coefficients[23] * texture2D(source, tc); tc.x += offset;
    c += coefficients[24] * texture2D(source, tc);
	
	vec2 st = gl_TexCoord[0].st;
	vec4 color = texture2D(tex, st);
	vec4 colorBloom = texture2D(bloom, st);
	
	int textureSize (gsampler1D sampler, int lod)
	ivec2 textureSize (gsampler2D sampler, int lod)
	ivec3 textureSize (gsampler3D sampler, int lod)
	ivec2 textureSize (gsamplerCube sampler, int lod)
	int textureSize (sampler1DShadow sampler, int lod)
	ivec2 textureSize (sampler2DShadow sampler, int lod)
	ivec2 textureSize (samplerCubeShadow sampler, int lod)
	ivec3 textureSize (samplerCubeArray sampler, int lod)
	ivec3 textureSize (samplerCubeArrayShadow sampler, int lod)
	ivec2 textureSize (gsampler2DRect sampler)
	ivec2 textureSize (sampler2DRectShadow sampler)
	ivec2 textureSize (gsampler1DArray sampler, int lod)
	ivec3 textureSize (gsampler2DArray sampler, int lod)
	ivec2 textureSize (sampler1DArrayShadow sampler, int lod)
	ivec3 textureSize (sampler2DArrayShadow sampler, int lod)
	int textureSize (gsamplerBuffer sampler)
	ivec2 textureSize (gsampler2DMS sampler)
	ivec3 textureSize (gsampler2DMSArray sampler)
	
	vec2 textureQueryLod(gsampler1D sampler, float P)
	vec2 textureQueryLod(gsampler2D sampler, vec2 P)
	vec2 textureQueryLod(gsampler3D sampler, vec3 P)
	vec2 textureQueryLod(gsamplerCube sampler, vec3 P)
	vec2 textureQueryLod(gsampler1DArray sampler, float P)
	vec2 textureQueryLod(gsampler2DArray sampler, vec2 P)
	vec2 textureQueryLod(gsamplerCubeArray sampler, vec3 P)
	vec2 textureQueryLod(sampler1DShadow sampler, float P)
	vec2 textureQueryLod(sampler2DShadow sampler, vec2 P)
	vec2 textureQueryLod(samplerCubeShadow sampler, vec3 P)
	vec2 textureQueryLod(sampler1DArrayShadow sampler, float P)
	vec2 textureQueryLod(sampler2DArrayShadow sampler, vec2 P)
	vec2 textureQueryLod(samplerCubeArrayShadow sampler, vec3 P)
	
	gvec4 texture (gsampler1D sampler, float P [, float bias] )
	gvec4 texture (gsampler2D sampler, vec2 P [, float bias] )
	gvec4 texture (gsampler3D sampler, vec3 P [, float bias] )
	gvec4 texture (gsamplerCube sampler, vec3 P [, float bias] )
	float texture (sampler1DShadow sampler, vec3 P [, float bias] )
	float texture (sampler2DShadow sampler, vec3 P [, float bias] )
	float texture (samplerCubeShadow sampler, vec4 P [, float bias] )
	gvec4 texture (gsampler1DArray sampler, vec2 P [, float bias] )
	gvec4 texture (gsampler2DArray sampler, vec3 P [, float bias] )
	gvec4 texture (gsamplerCubeArray sampler, vec4 P [, float bias] )
	float texture (sampler1DArrayShadow sampler, vec3 P [, float bias] )
	float texture (sampler2DArrayShadow sampler, vec4 P)
	gvec4 texture (gsampler2DRect sampler, vec2 P)
	float texture (sampler2DRectShadow sampler, vec3 P)
	float texture (gsamplerCubeArrayShadow sampler, vec4 P, float compare)
	
	gvec4 textureProj (gsampler1D sampler, vec2 P [, float bias] )
	gvec4 textureProj (gsampler1D sampler, vec4 P [, float bias] )
	gvec4 textureProj (gsampler2D sampler, vec3 P [, float bias] )
	gvec4 textureProj (gsampler2D sampler, vec4 P [, float bias] )
	gvec4 textureProj (gsampler3D sampler, vec4 P [, float bias] )
	float textureProj (sampler1DShadow sampler, vec4 P
	[, float bias] )
	float textureProj (sampler2DShadow sampler, vec4 P
	[, float bias] )
	gvec4 textureProj (gsampler2DRect sampler, vec3 P)
	gvec4 textureProj (gsampler2DRect sampler, vec4 P)
	float textureProj (sampler2DRectShadow sampler, vec4 P)
	
	color += colorBloom * bloomFactor;
	
	float Y = dot(vec4(0.30, 0.59, 0.11, 0.0), color);
	float YD = exposure * (exposure/brightMax + 1.0) / (exposure + 1.0);
	color *= YD;
	gl_FragColor = color;

	vec4 reflectedColor = textureCube(env, reflectedVector);
	vec4 refractedColor = textureCube(env, refractedVector);
	
	vec4 color = gl_Color;
	vec4 texture_color;
	vec3 normal;
	
	refractedColor.r = textureCube(env, TRed).r;
	refractedColor.g = textureCube(env, TGreen).g;
	refractedColor.b = textureCube(env, TBlue).b;
	
	
	const float gauss0 = 1.0/32.0;
    const float gauss1 = 5.0/32.0;
    const float gauss2 =15.0/32.0;
    const float gauss3 =22.0/32.0;
    const float gauss4 =15.0/32.0;
    const float gauss5 = 5.0/32.0;
    const float gauss6 = 1.0/32.0; 

	vec4 gaussFilter[7];
    gaussFilter[0]  = vec4( -3.0*texScaler , 0.0, 0.0, gauss0).yxzw;
    gaussFilter[1]  = vec4( -2.0*texScaler , 0.0, 0.0, gauss1).yxzw;
    gaussFilter[2]  = vec4( -1.0*texScaler , 0.0, 0.0, gauss2).yxzw;
    gaussFilter[3]  = vec4(  0.0*texScaler , 0.0, 0.0, gauss3).yxzw;
    gaussFilter[4]  = vec4( +1.0*texScaler , 0.0, 0.0, gauss4).yxzw;
    gaussFilter[5]  = vec4( +2.0*texScaler , 0.0, 0.0, gauss5).yxzw;
    gaussFilter[6]  = vec4( +3.0*texScaler , 0.0, 0.0, gauss6).yxzw;
	
	vec3  n_lightDirInTangent = -normalize(lightDirInTangent);
    vec3  n_viewDirInTangent = normalize(viewDirInTangent);
    vec3  bump     = normalize(texture2D(BumpMap,st).xyz * 2.0 - 1.0);
	
	float lighting = dot(bump,n_lightDirInTangent);
    float blighting= n_lightDirInTangent.z;
	
	vec4 texColor = texture2D(ObjectMap,st);
    vec4 specColor = texture2D(SpecMap, st); 
    vec4 color = texColor * lighting 
               + float(lighting>0.0)*float(blighting>0.0) 
                 * SpecularIntensity*pow(specular,Shininess)*specColor;

	vec4 color  = texture2D(SrcColor,st);
    vec4 gloom  = mat4(
       texture2D(SrcHDR1,st),
       texture2D(SrcHDR2,st),
       texture2D(SrcHDR3,st),
       texture2D(SrcHDR4,st) ) * MipMix;

    int i;
    for (i=0;i<7;i++)
       color += texture2D(Src, st + gaussFilter[i].xy) * gaussFilter[i].w;
		
	
	if(normal_map)
	{
		vec3 tex_normal = vec3(texture2D(nmap, gl_TexCoord[0].st)) * 2.0 - 1.0;
		normal = normalize(tbn_matrix * tex_normal);
	}
	else normal = normalize(tbn_matrix[2]);
	
	if(texture)
	{
		texture_color = texture2D(tex, gl_TexCoord[0].st);
	
		if(fb_texture && texture_color.rgb == vec3(0.0, 0.0, 0.0)) discard;
		
		if(texture_color.a == 0.0) discard;
		
		color *= texture_color;
	}
	
	if(vertextype == 1)
	{
		int i;
		vec3 mat_emission;
		vec3 mat_diffuse;
		vec3 mat_specular;
		float mat_shininess;
		
		if(current_material.active)
		{
			mat_emission = current_material.emission;
			mat_diffuse = current_material.diffuse;
			mat_specular = current_material.specular;
			mat_shininess = current_material.shininess;
		}
		else
		{
			mat_emission = vec3(0.0, 0.0, 0.0);
			mat_diffuse = gl_Color.rgb;
			mat_specular = vec3(0.4, 0.4, 0.4);
			mat_shininess = 8.0;
		}
		
		if(texture) mat_diffuse *= texture_color.rgb;
		
		if(specular_map) mat_specular = vec3(texture2D(smap, gl_TexCoord[0].st));
		
		color.rgb = mat_emission;
		
		for(i = 0; i < MAX_LIGHTS; i++)
		{
			if(lights_active[i])
			{
				vec3 light_vector = normalize(normal_light_vector[i].xyz);
				
				float dist = normal_light_vector[i].w;
				float att = 1.0 / (lights_att[i].x + lights_att[i].y * dist + lights_att[i].z * dist * dist);;
				vec3 ambient = lights_ambient[i] * mat_diffuse;
				
				float lambert_term = clamp(dot(normal, light_vector), 0.0, 1.0);
				vec3 diffuse = lambert_term * lights_diffuse[i] * mat_diffuse;
				
				vec3 reflection = normal_map ? reflect(light_vector, normal) : normalize(light_reflection[i]);
				
				float rdoteye = clamp(dot(reflection, normalize(eye_vector)), 0.0, 1.0);
				vec3 specular = lights_specular[i] * mat_specular * pow(rdoteye, mat_shininess);
				
				color.rgb += att * (ambient + diffuse + specular);
			}
		}
	}
	
	gl_FragColor = color + (mix(refractedColor, reflectedColor, textureCube(env, R)) + c) + ((expand_Hdr(color*exposure)+gloom*16.0*gloomIntensity)*exposure);
}
