#define glext.h
#define glxext.h
#define wglext.h
#define glATI.h
#define wglATI.h

#define GL_FRAGMENT_PRECISION_HIGH 1
#extension GL_EXT_gpu_shader4 : enable

precision highp float;

#define _ENABLE_BUMP_MAPPING;
#define _ENABLE_GL_TEXTURE_2D;

varying vec4 vertColor;
attribute vec3 R5_tangent;
varying vec3 _normal;
varying vec3 _tangent;
varying vec2 _texCoord;

varying vec3 viewVector;
varying vec3 lightVector;
uniform vec3 sunPosition;
uniform vec3 moonPosition;
varying vec4 specMultiplier;
varying float distance;


int getTextureID(vec2 coord) {
    int i = int(floor(16*coord.s));
    int j = int(floor(16*coord.t));
    return i + 16*j;
}

uniform int worldTime;
uniform int renderType;

varying float texID;

varying float useCelestialSpecularMapping;

const float PI = 3.1415926535897932384626433832795;
const float PI2 = 6.283185307179586476925286766559;

in vec3 i1;
out vec3 e1;

//HDR GLSL vertex Shader - rewritten by Asmodean
varying vec3 lightDir;
uniform vec4 vViewPosition;
uniform mat4 matViewProjection;

centroid varying vec2 texCoord;
varying vec3 normal;
attribute vec3 intan;
varying vec3 viewDir;

uniform vec4 OGL2Param;
uniform vec4 OGL2InvSize;
vec4 offset;

uniform vec4 OGL2Size;

in int gl_VertexID;
in int gl_InstanceID;

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

out gl_PerVertex {
vec4 gl_Position;
float gl_PointSize;
float gl_ClipDistance[];
};

void main() {


	vec4 position = gl_Vertex;
	int tex = getTextureID(gl_MultiTexCoord0.st);
	float flatDistanceSquared = position.x * position.x + position.z * position.z;


	#ifdef _ENABLE_BUMP_MAPPING
		  distance = sqrt(flatDistanceSquared + position.y * position.y);
	#endif
	         
	 gl_Position = gl_ProjectionMatrix * position;

	#ifdef defined(_ENABLE_BUMP_MAPPING)

	   vec4 position = gl_ModelViewMatrix * gl_Vertex;
	   distance = sqrt(position.x * position.x + position.y * position.y + position.z * position.z);
	   gl_Position = gl_ProjectionMatrix * position;
	   
	#endif

	   vertColor = gl_Color;

	#ifdef _ENABLE_GL_TEXTURE_2D

	   texCoord = gl_MultiTexCoord0;

	#ifdef _ENABLE_BUMP_MAPPING

	   vec3 normal = normalize(gl_NormalMatrix * gl_Normal);
	   vec3 tangent;
	   vec3 binormal;
	   
	   useCelestialSpecularMapping = 1.0;

	   if (gl_Normal.x > 0.5) {
		  //  1.0,  0.0,  0.0
		  tangent  = normalize(gl_NormalMatrix * vec3( 0.0,  0.0, -1.0));
		  binormal = normalize(gl_NormalMatrix * vec3( 0.0, -1.0,  0.0));
	   } else if (gl_Normal.x < -0.5) {
		  // -1.0,  0.0,  0.0
		  tangent  = normalize(gl_NormalMatrix * vec3( 0.0,  0.0,  1.0));
		  binormal = normalize(gl_NormalMatrix * vec3( 0.0, -1.0,  0.0));
	   } else if (gl_Normal.y > 0.5) {
		  //  0.0,  1.0,  0.0
		  tangent  = normalize(gl_NormalMatrix * vec3( 1.0,  0.0,  0.0));
		  binormal = normalize(gl_NormalMatrix * vec3( 0.0,  0.0,  1.0));
	   } else if (gl_Normal.y < -0.5) {
		  //  0.0, -1.0,  0.0
		  useCelestialSpecularMapping = 0.0;
		  tangent  = normalize(gl_NormalMatrix * vec3( 1.0,  0.0,  0.0));
		  binormal = normalize(gl_NormalMatrix * vec3( 0.0,  0.0,  1.0));
	   } else if (gl_Normal.z > 0.5) {
		  //  0.0,  0.0,  1.0
		  tangent  = normalize(gl_NormalMatrix * vec3( 1.0,  0.0,  0.0));
		  binormal = normalize(gl_NormalMatrix * vec3( 0.0, -1.0,  0.0));
	   } else if (gl_Normal.z < -0.5) {
		  //  0.0,  0.0, -1.0
		  tangent  = normalize(gl_NormalMatrix * vec3(-1.0,  0.0,  0.0));
		  binormal = normalize(gl_NormalMatrix * vec3( 0.0, -1.0,  0.0));
	   }
	   
	   mat3 tbnMatrixA = mat3(tangent.x, binormal.x, normal.x,
							  tangent.y, binormal.y, normal.y,
							  tangent.z, binormal.z, normal.z);
	   
	   viewVector = (gl_ModelViewMatrix * gl_Vertex).xyz;
	   viewVector = normalize(tbnMatrixA * viewVector);

	   if (worldTime < 12000 || worldTime > 23250) {
		  lightVector = normalize(tbnMatrixA * -sunPosition);
		  specMultiplier = vec4(1.0, 1.0, 1.0, 1.0);
	   } else {
		  lightVector = normalize(tbnMatrixA * -moonPosition);
		  specMultiplier = vec4(0.5, 0.5, 0.5, 0.5);
	   }
	   specMultiplier *= clamp(abs(float(worldTime) / 500.0 - 46.0), 0.0, 1.0) * clamp(abs(float(worldTime) / 500.0 - 24.5), 0.0, 1.0);

	#endif // _ENABLE_GL_BUMP_MAPPING
	#endif // _ENABLE_GL_TEXTURE_2D

	   if (renderType != 0) {
		  texID = float(getTextureID(gl_MultiTexCoord0.st));
	   }
	   else {
		  texID = -1.0;
	   }
	   
	offset = vec4( OGL2InvSize.x, OGL2InvSize.y, OGL2InvSize.x*-1.0, OGL2InvSize.y*-1.0);
	gl_Position = ftransform();
	gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_TexCoord[1].xy = gl_TexCoord[0].xy + vec2(0.0, offset.w);
	gl_TexCoord[2].xy = gl_TexCoord[0].xy + vec2(offset.z, 0.0);
	gl_TexCoord[3].xy = gl_TexCoord[0].xy + vec2(offset.x, 0.0);
	gl_TexCoord[4].xy = gl_TexCoord[0].xy + vec2(0.0, offset.y);
	gl_TexCoord[1].zw = gl_TexCoord[1].xy + vec2(0.0, offset.w*2.0);
	gl_TexCoord[2].zw = gl_TexCoord[2].xy + vec2(offset.z*2.0, 0.0);
	gl_TexCoord[3].zw = gl_TexCoord[3].xy + vec2(offset.x*2.0, 0.0);
	gl_TexCoord[4].zw = gl_TexCoord[4].xy + vec2(0.0, offset.y*2.0);
	
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
	
	vec4 reflectedColor = texture(Environment, R);
	vec4 refractedColor;
	
	refractedColor.r = texture(Environment, TRed).r;
	refractedColor.g = texture(Environment, TGreen).g;
	refractedColor.b = texture(Environment, TBlue).b;

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
		}
		
		gl_FrontColor.a = clamp(gl_FrontColor.a, 0.0, 0.5);
	}
	
	// BLEND_NONE
	if(blend_mode == 4) gl_FrontColor.a = 1.0;
	// BLEND_25P
	else if(blend_mode == 3) gl_FrontColor.a = 0.25;
	
	gl_TexCoord[0] = gl_MultiTexCoord0;
	
	float x = (OGL2Size.x/40960.0)*OGL2Param.x;
	float y = (OGL2Size.y/40960.0)*OGL2Param.y;
	vec2 dg1 = vec2( x,y);  vec2 dg2 = vec2(-x,y);
	vec2 sd1 = dg1*0.5;     vec2 sd2 = dg2*0.5;
	vec2 ddx = vec2(x,0.0); vec2 ddy = vec2(0.0,y);
		
		gl_FrontColor = gl_Color;
		gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
		gl_Position = ftransform();
		gl_FogFragCoord = gl_Position.z;
		gl_TexCoord[0] = gl_MultiTexCoord0;
		gl_TexCoord[1].xy = gl_TexCoord[0].xy - sd1;
		gl_TexCoord[2].xy = gl_TexCoord[0].xy - sd2;
		gl_TexCoord[3].xy = gl_TexCoord[0].xy + sd1;
		gl_TexCoord[4].xy = gl_TexCoord[0].xy + sd2;
		gl_TexCoord[5].xy = gl_TexCoord[0].xy - dg1;
		gl_TexCoord[6].xy = gl_TexCoord[0].xy + dg1;
		gl_TexCoord[5].zw = gl_TexCoord[0].xy - dg2;
		gl_TexCoord[6].zw = gl_TexCoord[0].xy + dg2;
		gl_TexCoord[1].zw = gl_TexCoord[0].xy - ddy;
		gl_TexCoord[2].zw = gl_TexCoord[0].xy + ddx;
		gl_TexCoord[3].zw = gl_TexCoord[0].xy + ddy;
		gl_TexCoord[4].zw = gl_TexCoord[0].xy - ddx;
		_texCoord = (gl_TextureMatrix[0] * gl_MultiTexCoord0).xy;
		_normal   = gl_NormalMatrix * gl_Normal;
		_tangent  = gl_NormalMatrix * R5_tangent;
	}
