#version 120

uniform sampler2D tex0;
uniform sampler2D tex1;

varying vec3 normal, eyeVec, lightDir;

void main (void)
{
  vec4 nightColor = texture2D(tex1, gl_TexCoord[0].st);
  vec4 DayDiffuseColor = texture2D(tex0, gl_TexCoord[0].st);
  vec4 DayAmbientColor = DayDiffuseColor;
  vec4 MaterialSpecularColor = gl_FrontMaterial.specular;
  vec4 DayAmbientLight = gl_LightSource[0].ambient;
  vec4 DayDiffuseLight = gl_LightSource[0].diffuse;

  vec3 N = normalize(normal);
  vec3 L = normalize(lightDir);
  float lambertTerm = dot(N,L);

  float dayLambert = clamp(lambertTerm, 0, 1);
  float nightLambert = clamp(-lambertTerm, 0, 1);

  vec4 NightDiffuseColor = nightColor;
  vec4 NightAmbientColor = nightColor;
  vec4 NightAmbientLight = vec4(nightLambert, nightLambert, nightLambert, 1);

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
		DayAmbientColor * DayAmbientLight +
		NightAmbientColor * NightAmbientLight +
		// Diffuse : "color" of the object
		DayDiffuseColor * DayDiffuseLight * gl_FrontMaterial.diffuse * dayLambert +
		// Specular : reflective highlight, like a mirror
		MaterialSpecularColor * gl_LightSource[0].specular * specularFactor;

  gl_FragColor = color;

  //if (gl_LightSource[0].ambient[0] > 0.5)
  //gl_FragColor = vec4(1, 0, 0, 1);
}
