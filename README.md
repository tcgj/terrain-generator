# Procedural Terrain Generator
A procedural terrain generator based on isosurface extraction. Currently uses the marching cubes algorithm.

Written in Unity 2019.2.15f1.

## Current issues
* Main thread is blocked while compute shader on GPU is cube marching for a single chunk. (Solution may be to shift to CPU with multi-threading, so no CPU-GPU transfer of buffer data is required)
* LOD system is very recursive-based. There may be a more elegant solution here.
* Lack of textures. Perhaps triplanar shader?

## Credits:
* https://github.com/SebLague/Marching-Cubes
* https://github.com/keijiro/NoiseShader
* http://paulbourke.net/geometry/polygonise/
* https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch01.html
