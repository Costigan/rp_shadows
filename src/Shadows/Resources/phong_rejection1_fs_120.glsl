#version 120
uniform float centerX, centerY, angleFactor;
uniform sampler2D tex;
varying vec3 normal, eyeVec, lightDir;
void main (void)
{
  vec4 MaterialDiffuseColor = texture2D(tex, gl_TexCoord[0].st);
  vec4 MaterialAmbientColor = MaterialDiffuseColor;
  vec4 MaterialSpecularColor = gl_FrontMaterial.specular;

  vec3 N = normalize(normal);
  vec3 L = normalize(lightDir);
  float lambertTerm = dot(N,L);
  vec3 E = normalize(eyeVec);
  vec3 R = reflect(-L, N);

  // Cosine of the angle between the Eye vector and the Reflect vector,
  // clamped to 0
  //  - Looking into the reflection -> 1
  //  - Looking elsewhere -> < 1
  float cosAlpha = clamp( dot( E, R ), 0, 1 );

  float specularFactor = pow(cosAlpha, 
                         gl_FrontMaterial.shininess
						 //10
						 );

  vec4 color =
		// Ambient : simulates indirect lighting
		MaterialAmbientColor * gl_LightSource[0].ambient +
		// Diffuse : "color" of the object
		MaterialDiffuseColor * gl_LightSource[0].diffuse * lambertTerm +  // * gl_FrontMaterial.diffuse
		// Specular : reflective highlight, like a mirror
		MaterialSpecularColor * gl_LightSource[0].specular * specularFactor;

  vec2 point = gl_FragCoord.xy;
  vec2 center = vec2(centerX, centerY);
  float d = distance(point, center);
  float angle = d * angleFactor;
  float p = -0.3256*angle+0.3193;
  float rejection = clamp(pow(10.0, p), 0.001, 1);

  gl_FragColor = color * rejection;
}
