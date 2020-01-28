# Procedural Terrain Generator
A procedural terrain generator based on isosurface extraction. Currently uses the marching cubes algorithm.

![](https://raw.githubusercontent.com/tcgj/terrain-generator/master/Docs/Example.png)

Written in Unity 2019.2.15f1.

## Current issues
* Currently CPU based jobs approach is not implemented correctly, and rendering is taking too much CPU.
* LOD system is very recursive-based. There may be a more elegant solution here.
* Lack of textures. Perhaps triplanar shader?

![](https://raw.githubusercontent.com/tcgj/terrain-generator/master/Docs/Example2.png)

## Credits:
* https://github.com/SebLague/Marching-Cubes
* https://github.com/keijiro/NoiseShader
* http://paulbourke.net/geometry/polygonise/
* https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch01.html
