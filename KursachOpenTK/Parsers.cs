using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace KursachOpenTK
{
    public static class WavefrontModelParser
    {

        public static WavefrontModel Parse(string path)
        {
            WavefrontModel model = new WavefrontModel();
            VertexIndex[] verticesIndex;
            string[] wavefrontFileData = File.ReadAllLines(path);
            int loopLength = wavefrontFileData.Length; // Squeeze out every last drop!
            for (int lines = 0; lines < loopLength; lines++)
            {
                string[] lineTokens = wavefrontFileData[lines].Split(' ');
                switch (lineTokens[0])
                {
                    case "v": // Vector
                        float x = Single.Parse(lineTokens[1]);
                        float y = Single.Parse(lineTokens[2]);
                        float z = Single.Parse(lineTokens[3]);
                        model.Vertices.Add(new Vector3(x, y, z));
                        break;
                    case "vt": // Texture Coordinate
                        float u = Single.Parse(lineTokens[1]);
                        float v = Single.Parse(lineTokens[2]);
                        model.TexCoords.Add(new Vector2(u, v));
                        break;
                    case "vn": // Normal
                        float normalX = Single.Parse(lineTokens[1]);
                        float normalY = Single.Parse(lineTokens[2]);
                        float normalZ = Single.Parse(lineTokens[3]);
                        model.Normals.Add(new Vector3(normalX, normalY, normalZ));
                        break;
                    case "f":
                        verticesIndex = new VertexIndex[3];
                        for (int i = 0; i < 3; i++)
                        {
                            string[] parameters = lineTokens[i + 1].Split('/');
                            int vertice = Int32.Parse(parameters[0]) - 1;
                            int texture = Int32.Parse(parameters[1]) - 1;
                            int normal = Int32.Parse(parameters[2]) - 1;
                            verticesIndex[i] = new VertexIndex(vertice, normal, texture);
                        }
                        model.Faces.Add(new Face(verticesIndex));
                        break;
                }
            }
            return model;
        }
    }
}
}
