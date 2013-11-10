from math import cos, sin

n = 64
for i in range(n):
    angle = i * 2 * 3.141592654 / n
    print "v += abs(l - tex2D(SceneSampler, uv + float2(",
    print cos(angle), ",", sin(angle),
    print ") * d * 1.25).r);"
