using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTraining
{
    public class TexturedCube
    {
        public class Triangle
        {
            public int Index1 { get; set; }
            public int Index2 { get; set; }
            public int Index3 { get; set; }

            public Triangle(int i1, int i2, int i3)
            {
                Index1 = i1;
                Index2 = i2;
                Index3 = i3;
            }
            public override string ToString()
            {
                return string.Format("{0} {1} {2}", Index1, Index2, Index3);
            }
        }

        public VertexPositionNormalTexture[] Vertices;
        public Triangle[] Triangles;

        private int size;
        public int Size { get { return size; } set { size = value; } }

        public TexturedCube(int size)
        {
            this.Size = size;
            this.Vertices = new VertexPositionNormalTexture[14]
            {
                new VertexPositionNormalTexture(size*new Vector3(0,0,0), Vector3.Up, texCoordinates[0]),
                new VertexPositionNormalTexture(size*new Vector3(1,0,0), Vector3.Up, texCoordinates[1]),
                new VertexPositionNormalTexture(size*new Vector3(1,0,1), Vector3.Up, texCoordinates[2]),
                new VertexPositionNormalTexture(size*new Vector3(0,0,1), Vector3.Up, texCoordinates[3]),
                new VertexPositionNormalTexture(size*new Vector3(0,1,1), Vector3.Up, texCoordinates[4]),
                new VertexPositionNormalTexture(size*new Vector3(0,1,0), Vector3.Up, texCoordinates[5]),
                new VertexPositionNormalTexture(size*new Vector3(1,1,0), Vector3.Up, texCoordinates[6]),
                new VertexPositionNormalTexture(size*new Vector3(1,1,1), Vector3.Up, texCoordinates[7]),
                //Additional 0 vertices
                new VertexPositionNormalTexture(size*new Vector3(0,0,0), Vector3.Up, texCoordinates[8]),
                new VertexPositionNormalTexture(size*new Vector3(0,0,0), Vector3.Up, texCoordinates[9]),
                //Additional 1 vertice
                new VertexPositionNormalTexture(size*new Vector3(1,0,0), Vector3.Up, texCoordinates[10]),
                //Additional 5 vertices
                new VertexPositionNormalTexture(size*new Vector3(0,1,0), Vector3.Up, texCoordinates[11]),
                new VertexPositionNormalTexture(size*new Vector3(0,1,0), Vector3.Up, texCoordinates[12]),
                //Additional 6 vertice
                new VertexPositionNormalTexture(size*new Vector3(1,1,0), Vector3.Up, texCoordinates[13])
            };
            this.Triangles = new Triangle[12] {
                new Triangle(8,10,2),
                new Triangle(8,2,3),
                new Triangle(9,3,4),
                new Triangle(9,4,12),
                new Triangle(0,5,6),
                new Triangle(0,6,1),
                new Triangle(7,1,2),
                new Triangle(7,2,3),
                new Triangle(7,3,4),
                new Triangle(7,4,11),
                new Triangle(7,11,13),
                new Triangle(7,6,1)
            };
        }

        public VertexPositionNormalTexture[] TriangleVerticesList
        {
            get
            {
                List<VertexPositionNormalTexture> list = new List<VertexPositionNormalTexture>();
                foreach (var triangle in Triangles)
                {
                    list.Add(Vertices[triangle.Index1]);
                    list.Add(Vertices[triangle.Index2]);
                    list.Add(Vertices[triangle.Index3]);
                }
                return list.ToArray();
            }
        }

        private static Vector2[] texCoordinates = new Vector2[14] {
            new Vector2(0f,0.34f),
            new Vector2(0.25f,0.34f),
            new Vector2(0.5f, 0.34f),
            new Vector2(0.75f, 0.34f),
            new Vector2(0.75f, 0.66f),
            new Vector2(0.0f, 0.66f),
            new Vector2(0.25f, 0.66f),
            new Vector2(0.5f, 0.66f),
            new Vector2(0.75f, 0f),
            new Vector2(1.0f, 0.34f),
            new Vector2(0.5f, 0f),
            new Vector2(0.75f, 1.0f),
            new Vector2(1.0f, 0.66f),
            new Vector2(0.5f, 1.0f)
        };

    }
}
