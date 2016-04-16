#version 120

varying vec3 normal, eyeVec, lightDir;
varying vec4 texCoords;
void main()
{
  gl_TexCoord[0] = gl_MultiTexCoord0;
  gl_Position = ftransform();		
  normal = gl_NormalMatrix * gl_Normal;
  vec4 vVertex = gl_ModelViewMatrix * gl_Vertex;
  eyeVec = -vVertex.xyz;
  lightDir = vec3(gl_LightSource[0].position.xyz - vVertex.xyz);
}
