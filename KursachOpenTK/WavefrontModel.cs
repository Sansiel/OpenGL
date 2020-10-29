using System.Collections.Generic;
using OpenTK;
using KursachOpenTK;

namespace KursachOpenTK
{
    public class WavefrontModel
    {
        public List<Vector3> Vertices;
        public List<Vector2> TexCoords;
        public List<Vector3> Normals;
        public List<Face> Faces;
        public string ModelSource;

        public int TotalTriangles
        {
            get
            {
                return this.Vertices.Count / 3;
            }
        }

        public WavefrontModel()
        {
            this.Vertices = new List<Vector3>();
            this.TexCoords = new List<Vector2>();
            this.Normals = new List<Vector3>();
            this.Faces = new List<Face>();
        }

        public WavefrontModel(int buffer)
        {
            this.Vertices = new List<Vector3>(buffer);
            this.TexCoords = new List<Vector2>(buffer);
            this.Normals = new List<Vector3>(buffer);
            this.Faces = new List<Face>(buffer);
        }

        public WavefrontModel(string modelPath, bool loadImmediately)
        {
            this.ModelSource = modelPath;
            if (loadImmediately)
            {
                Load();
            }
        }

        private void Load()
        {
            WavefrontModel model = WavefrontModelParser.Parse(ModelSource);
            this.Vertices = model.Vertices;
            this.TexCoords = model.TexCoords;
            this.Normals = model.Normals;
            this.Faces = model.Faces;
        }
    }

}