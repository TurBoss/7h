
int make16bit;
int texture_flag;
int fb_texture;
sampler2D tex;

struct vertex
{
	float4 position : POSITION;
	float4 texcoord0 : TEXCOORD0;
	float4 color : COLOR0;
};

struct pixel
{
	float4 color : COLOR0;
};

void main(in vertex IN, out pixel OUT)
{
	float4 color = IN.color;
	float4 texture_color = tex2D(tex, IN.texcoord0.xy);
	
	if(texture_flag != 0)
	{
		if(texture_color.a == 0.0) discard;
		
		color *= texture_color;
	}

	if(make16bit != 0) color.rgb = floor(color.rgb * 32.0) / 32.0;
	
	OUT.color = color;
}

sampler2D tex_u;
sampler2D tex_v;

void yuv_main(in vertex IN, out pixel OUT)
{
	float4x4 conversion = {1.164,    1.164,   1.164,   0.0,
	                       0.0,   -0.391, 2.018, 0.0,
	                       1.596, -0.813, 0.0,   0.0,
	                       0.0,    0.0,   0.0,   1.0,};

	float y = tex2D(tex, IN.texcoord0.xy).r - (1.0 / 16.0);
	float u = tex2D(tex_u, IN.texcoord0.xy).r - 0.5;
	float v = tex2D(tex_v, IN.texcoord0.xy).r - 0.5;
	
	float4 yuv = {y, u, v, 1.0};

	float4 color = mul(yuv, conversion);

	if(make16bit != 0) color.rgb = floor(color.rgb * 32.0) / 32.0;
	
	OUT.color = color;
}
