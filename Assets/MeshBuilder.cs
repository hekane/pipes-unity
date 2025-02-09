using UnityEngine;
using System.Collections.Generic;


namespace Piping
{
    /// <summary>
    /// This is a class for building pipes in Unity.
    /// Unfortunately the functionality to build angle
    /// joints is currently not working. Launch the scene
    /// in Unity and build copper piping with W,A,S,D,Q,Z keys.
    /// </summary>
    public class MeshBuilder : MonoBehaviour
    {
        public const float c_tau= Mathf.PI * 2;

        [SerializeField]
        Material m_Material;
        [SerializeField]
        [Tooltip("How many sides should the pipe have?")]
        float detail = 16;
        [SerializeField]
        [Tooltip("How tall of a pipe?")]
        float height = 10;
        [SerializeField]
        [Tooltip("Pipe radius?")]
        float radius = 0.250f;
        [SerializeField]
        [Tooltip("Angle iterations")]
        int angleIterations = 4;

        UIManager uiManager;
        MeshRenderer m_MeshRenderer;
        MeshFilter m_MeshFilter;
        Mesh m_Mesh;

        float[] sizes = {0.1f, 0.12f,0.16f,.2f,.25f,.315f,.4f,.56f,.63f,.8f,1f,1.3f,1.6f,2f };
        Vector3[] m_vertices;
        Vector2[] m_UV;
        Vector3[] m_normals;
        int[] m_triangles;
        Vector3 m_pipeCursorPosition = Vector3.zero;
        Vector3 m_lastDirection = Vector3.zero;
        
        float m_lastRadius;
        private void Awake()
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }
        void Start()
        {
            m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
            m_MeshFilter = gameObject.AddComponent<MeshFilter>();
            m_Mesh = new Mesh();
            m_Mesh.name = "Pipe example";
            m_MeshRenderer.material = m_Material;
            UpdateRadiusValue();
            m_lastRadius= radius;
            UpdateDetailValue();
            UpdateLengthvalue();
        }
        /// <summary>
        /// Get updated length slider value from UI
        /// </summary>
        public void UpdateLengthvalue()
        {
            height = uiManager.GetLenght();
        }
        /// <summary>
        /// Get updated radius slider value from UI
        /// and change the radius based on calculated index 
        /// in sizes array.
        /// </summary>
        public void UpdateRadiusValue()
        {
            radius = sizes[uiManager.GetRadius()];
        }
        /// <summary>
        /// Get updated detail slider value from UI
        /// </summary>
        public void UpdateDetailValue()
        {
            detail = uiManager.GetDetail();
        }
        /// <summary>
        /// Read user input and update mesh accordingly.
        /// </summary>
        private void Update()
        {
            UpdateLengthvalue();
            UpdateRadiusValue();
            UpdateDetailValue();
            if (Input.GetKeyDown(KeyCode.W))
            {
                UpdateMesh(Vector3.up);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                UpdateMesh(Vector3.left);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                UpdateMesh(Vector3.right);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                UpdateMesh(Vector3.down);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                UpdateMesh(Vector3.forward);
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                UpdateMesh(Vector3.back);
            }
        }
        /// <summary>
        /// Add polygons to mesh. Gets called on FixedUpdate.
        /// </summary>
        /// <param name="direction">The direction of growth.</param>
        private void UpdateMesh(Vector3 direction)
        {
            float h = height;

            if (Vector3.Angle(m_lastDirection, direction) > 0f)
            {
                Vector3 dir = m_lastDirection;
                height = h / angleIterations;
                for (int i=0; i < angleIterations; i++)
                {
                    BuildSegment(dir, radius, radius, false);
                    dir = (m_lastDirection + direction / angleIterations) - m_lastDirection;
                }
            }
            height = h;
            
            
            if(Mathf.Abs(radius - m_lastRadius) > Mathf.Epsilon)
            {
                height = m_lastRadius > radius ? m_lastRadius * 1.5f : radius * 1.5f;
                BuildSegment(direction, m_lastRadius, radius, true);
            }

            height = h;
            m_lastRadius = radius;
            BuildSegment(direction, radius, radius, false);

            height = h;
        }

        /// <summary>
        /// Build a segment of the mesh.
        /// </summary>
        /// <param name="direction">Direction to build in</param>
        /// <param name="radius1">Starting radius</param>
        /// <param name="radius2">End radius</param>
        /// <param name="radiusChanges">Does the radius change?</param>
        private void BuildSegment(Vector3 direction, float radius1, float radius2, bool radiusChanges)
        {
            Vector3 cursorOffset = direction*height;
            m_lastDirection = direction;

            for (int i = 0; i < detail; i++)
            {
                float angle1 = (c_tau / detail) * i;
                float angle2 = (c_tau / detail) * (i + 1);
                Vector3 v1 = Vector3.zero;
                Vector3 v2 = Vector3.zero;
                Vector3 vt1 = Vector3.zero;
                Vector3 vt2 = Vector3.zero;
                if (direction == Vector3.up || direction == Vector3.down)
                {
                    v1 = m_pipeCursorPosition + new Vector3(radius1 * Mathf.Cos(angle1), 0, radius1 * Mathf.Sin(angle1));
                    v2 = m_pipeCursorPosition + new Vector3(radius1 * Mathf.Cos(angle2), 0, radius1 * Mathf.Sin(angle2));
                }
                else if (direction == Vector3.left || direction == Vector3.right)
                {
                    v1 = m_pipeCursorPosition + new Vector3(0, radius1 * Mathf.Sin(angle1), radius1 * Mathf.Cos(angle1));
                    v2 = m_pipeCursorPosition + new Vector3(0, radius1 * Mathf.Sin(angle2), radius1 * Mathf.Cos(angle2));
                }
                else if (direction == Vector3.forward || direction == Vector3.back)
                {
                    v1 = m_pipeCursorPosition + new Vector3(radius1 * Mathf.Sin(angle1), radius1 * Mathf.Cos(angle1),0);
                    v2 = m_pipeCursorPosition + new Vector3(radius1 * Mathf.Sin(angle2), radius1 * Mathf.Cos(angle2),0);
                }
                if (radiusChanges)
                {
                    if (direction == Vector3.up || direction == Vector3.down)
                    {
                        vt1 = m_pipeCursorPosition + new Vector3(radius2 * Mathf.Cos(angle1), 0, radius2 * Mathf.Sin(angle1)) + direction * height;
                        vt2 = m_pipeCursorPosition + new Vector3(radius2 * Mathf.Cos(angle2), 0, radius2 * Mathf.Sin(angle2)) + direction * height;
                        CreatePolygons(v1, v2, vt1, vt2, 1, direction);
                    }
                    else if (direction == Vector3.left || direction == Vector3.right)
                    {
                        vt1 = m_pipeCursorPosition + new Vector3(0, radius2 * Mathf.Sin(angle1), radius2 * Mathf.Cos(angle1)) + direction * height;
                        vt2 = m_pipeCursorPosition + new Vector3(0, radius2 * Mathf.Sin(angle2), radius2 * Mathf.Cos(angle2)) + direction * height;
                        CreatePolygons(v1, v2, vt1, vt2, 1, direction);
                    }

                }
                else
                {
                    vt1 = v1 + direction * height;
                    vt2 = v2 + direction * height;
                    CreatePolygons(v1, v2, vt1, vt2, 1, direction);
                }
            }
            
            m_Mesh.vertices = m_vertices;
            m_Mesh.uv = m_UV;
            m_Mesh.normals = m_normals;
            m_Mesh.triangles = m_triangles;
            m_MeshFilter.mesh = m_Mesh;
            m_pipeCursorPosition += cursorOffset;
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
            return direction==Vector3.left||direction==Vector3.down || direction==Vector3.back?
                -Vector3.Cross(to1 - from1, from2 - from1): 
                Vector3.Cross(to1 - from1, from2 - from1);
        }

        /// <summary>
        /// Creates a range of polygons.
        /// </summary>
        /// <param name="from1">Vertex 1</param>
        /// <param name="from2">Vertex 2</param>
        /// <param name="to1">Vertex 3</param>
        /// <param name="to2">Vertex 4</param>
        /// <param name="polyCount">How many polygons to split it in?</param>
        /// <param name="direction">In which direction?</param>
        void CreatePolygons(Vector3 from1, Vector3 from2, Vector3 to1, Vector3 to2, float polyCount, Vector3 direction)
        {
            Vector3 normal = CalculateNormal(from1, from2, to1, direction);

          
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
}
