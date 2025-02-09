using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Assertions;

namespace Piping
{
    public class MeshBuilder : MonoBehaviour
    {
        public const float c_tau= Mathf.PI * 2;
        MeshRenderer m_MeshRenderer;
        MeshFilter m_MeshFilter;
        Mesh m_Mesh;
      
        [SerializeField]
        Material m_Material;
        [SerializeField]
        [Tooltip("How many sides should the pipe have?")]
        int detail = 16;
        [SerializeField]
        [Tooltip("How tall of a pipe?")]
        float height = 10;
        [SerializeField]
        [Tooltip("Pipe radius?")]
        float radius = 0.250f;
        Vector3[] m_vertices;
        Vector2[] m_UV;
        Vector3[] m_normals;
        int[] m_triangles;
        Vector3 m_pipeCursorPosition=Vector3.zero;
        UIManager uiManager;
        
        async void Start()
        {
            m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
            m_MeshFilter = gameObject.AddComponent<MeshFilter>();
            m_Mesh = new Mesh();
            m_Mesh.name = "Pipe example";
            m_MeshRenderer.material = m_Material;

            uiManager = FindFirstObjectByType<UIManager>();
            
            uiManager.m_pipeSegmentLenghtSlider.onValueChanged.AddListener(UpdateLengthvalue);
            uiManager.m_pipeRadiusSlider.onValueChanged.AddListener(UpdateRadiusValue);
            uiManager.m_detailSlider.onValueChanged.AddListener(UpdateDetailValue);

        }
        public void UpdateLengthvalue(float value)
        {
            height = uiManager.m_pipeSegmentLenghtSlider.value;
        }
        public void UpdateRadiusValue(float value)
        {
            height = uiManager.m_pipeRadiusSlider.value;
        }
        public void UpdateDetailValue(float value)
        {
            height = uiManager.m_detailSlider.value;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("UP");
                UpdateMesh(Vector3.up);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("LEFT");
                UpdateMesh(Vector3.left);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("RIGTH");
                UpdateMesh(Vector3.right);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("DOWN");
                UpdateMesh(Vector3.down);
            }
        }
        private void UpdateMesh(Vector3 direction)
        {
            for (int i = 0; i < detail; i++)
            {
                float angle1 = (c_tau / detail) * i;
                float angle2 = (c_tau / detail) * (i + 1);
                Vector3 v1 = Vector3.zero;
                Vector3 v2 = Vector3.zero;
                if (direction == Vector3.up||direction==Vector3.down)
                {
                    v1 = m_pipeCursorPosition + new Vector3(radius * Mathf.Cos(angle1), 0, radius * Mathf.Sin(angle1));
                    v2 = m_pipeCursorPosition + new Vector3(radius * Mathf.Cos(angle2), 0, radius * Mathf.Sin(angle2));
                }
                else if (direction == Vector3.left||direction==Vector3.right)
                {
                    v1 = m_pipeCursorPosition + new Vector3(0, radius * Mathf.Sin(angle1), radius * Mathf.Cos(angle1));
                    v2 = m_pipeCursorPosition + new Vector3(0, radius * Mathf.Sin(angle2), radius * Mathf.Cos(angle2));
                }
                Debug.Log(v1);
                Debug.Log(v2);
                Vector3 vt1 = v1 + direction * height;
                Vector3 vt2 = v2 + direction * height;
                Debug.Log(v1 + ", " + v2 + ", " + vt1 + ", " + vt2);

                CreatePolygons(v1, v2, vt1, vt2, 5f, 5f, 1, direction);
            }

            m_Mesh.vertices = m_vertices;
            m_Mesh.uv = m_UV;
            m_Mesh.normals = m_normals;
            m_Mesh.triangles = m_triangles;
            m_MeshFilter.mesh = m_Mesh;
            m_pipeCursorPosition += direction * height;
        }

        /// <summary>
        /// Calculate surface normal based on the four vertices passed in to the funcion.
        /// Vertice order should be bottom left, top left, bottom right
        /// </summary>
        /// <param name="from1">Bottom left vertex</param>
        /// <param name="from2">Bottom right vertex</param>
        /// <param name="to1">Top left vertex</param>
        /// <returns>Surface normal vector</returns>
        private Vector3 CalculateNormal(Vector3 from1, Vector3 from2, Vector3 to1, Vector3 direction)
        {

            return Vector3.Cross(to1 - from1, from2 - from1);
        }
        void CreatePolygons(Vector3 from1, Vector3 from2, Vector3 to1, Vector3 to2, float width, float height, float polyCount, Vector3 direction)
        {
            Vector3 normal = CalculateNormal(from1, from2, to1, direction);

            float offset = Vector3.Distance(from1, to1) / polyCount;
            for (int i = 0; i < polyCount; i++)
            {
                Vector3[] poly = new[]
                {
                    to1,
                    to2,
                    from1,
                    from2
                };
                CreatePolygon(poly, normal);
            }
            
        }
        /// <summary>
        /// Create a new polygon and update mesh properties.
        /// </summary>
        /// <param name="vertices">Polygon's vertices</param>
        /// <param name="surfaceNormal">Surface normal of the polygon</param>
        public void CreatePolygon(Vector3[] vertices, Vector3 surfaceNormal)
        {
            List<Vector3> vec = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector3> nor = new List<Vector3>();
            List<int> tri = new List<int>();

            if (m_vertices == null)
            {
                m_vertices = new Vector3[4];
            }
            else
            {
                foreach (Vector3 v in m_vertices)
                {
                    vec.Add(v);
                }
            }
            if (m_UV == null)
            {
                m_UV = new Vector2[4];
            }
            else
            {
                foreach (Vector2 uvs in m_UV)
                {
                    uv.Add(uvs);
                }
            }
            if (m_normals == null)
            {
                m_normals = new Vector3[4];
            }
            else
            {
                foreach (Vector3 n in m_normals)
                {
                    nor.Add(n);
                }
            }
            if (m_triangles == null)
            {
                m_triangles = new int[4];
            }
            else
            {
                foreach (int i in m_triangles)
                {
                    tri.Add(i);
                }
            }
            int[] indexes = new int[4];
            int counter = 0;
            foreach (Vector3 vec3 in vertices)
            {
                vec.Add(vec3);
                nor.Add(surfaceNormal);
                if (vec.Contains(vec3))
                {
                    indexes[counter] = vec.IndexOf(vec3);
                }
                else
                {
                    indexes[counter] = vec.Count + counter - 1;
                }
                counter++;
            }
            uv.Add(new Vector2(0, 1));
            uv.Add(new Vector2(1, 1));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(0, 0));
            tri.Add(indexes[0]);
            tri.Add(indexes[1]);
            tri.Add(indexes[2]);
            tri.Add(indexes[2]);
            tri.Add(indexes[1]);
            tri.Add(indexes[3]);
            m_vertices = vec.ToArray();
            m_normals = nor.ToArray();
            m_UV = uv.ToArray();
            m_triangles = tri.ToArray();
        }
    }
    public class Face
    {
        private Vector3[] vertices;
        private Vector3[] normals;
        private Vector2[] uv;
        private int[] triangles;

        public Face(Vector3[] vertices, Vector3[] normals, Vector2[] uv, int[] triangles)
        {
            this.vertices = vertices;
            this.normals = normals;
            this.uv = uv;
            this.triangles = triangles;
        }
    }
    public class Edge
    {
        private Vector3[] vertices;

        public Edge(Vector3[] verts)
        {
            vertices = verts;
        }
    }
}
