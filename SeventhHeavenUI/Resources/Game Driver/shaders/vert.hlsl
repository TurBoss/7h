
float4x4 ortho_matrix;
float4x4 worldprojview_matrix;
int blend_mode;

struct vertex
{
	float4 position : POSITION;
	float4 texcoord0 : TEXCOORD0;
	float4 color : COLOR0;
};

void lmain(in vertex IN, out vertex OUT)
{
	float4 pos = IN.position;
	float4 color = IN.color;
	
	pos = mul(pos, worldprojview_matrix);
	
	if(color.a > 0.5) color.a = 0.5;
	
	if(blend_mode == 4) color.a = 1.0;
	else if(blend_mode == 3) color.a = 0.25;
	
	OUT.texcoord0 = IN.texcoord0;
	OUT.position = pos;
	OUT.color = color;
}

void tlmain(in vertex IN, out vertex OUT)
{
	float4 pos = IN.position;
	float4 color = IN.color;
	
	pos.w = 1.0 / pos.w;
	pos.xyz *= pos.w;
	pos = mul(pos, ortho_matrix);
	
	if(blend_mode == 4) color.a = 1.0;
	else if(blend_mode == 3) color.a = 0.25;
	
	OUT.texcoord0 = IN.texcoord0;
	OUT.position = pos;
	OUT.color = color;
}
