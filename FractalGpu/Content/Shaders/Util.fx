float2 Div(float2 z, float2 w)
{
	if (w.x == 0 && w.y == 0) return float2(100000, 0);
	else return float2(z.x * w.x + z.y * w.y, z.y * w.x - z.x * w.y) / (w.x * w.x + w.y * w.y);
}

float2 Mult(float2 z, float2 w)
{
	return float2(z.x * w.x - z.y * w.y, z.x * w.y + z.y * w.x);
}

float MagSquared(float2 z)
{
	return z.x * z.x + z.y * z.y;
}
