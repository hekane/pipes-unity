using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEditor.Profiling.Memory.Experimental;


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
        [Range(1,12)]
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
            if (height < 1)
            {
                height = 1;
            }
        }
        /// <summary>
        /// Get updated radius slider value from UI
        /// and change the radius based on calculated index 
        /// in sizes array.
        /// </summary>
        public void UpdateRadiusValue()
        {
            radius = sizes[uiManager.GetRadius()];
            if (radius <= 0f)
            {
                radius = sizes[3];
            }
        }
        /// <summary>
        /// Get updated detail slider value from UI
        /// </summary>
        public void UpdateDetailValue()
        {
            detail = uiManager.GetDetail();
            if (detail < 4)
            {
                detail = 4;
            }
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
            if (direction == -m_lastDirection) return;
            if(m_lastDirection==Vector3.zero) m_lastDirection = direction;
            float h = height;

            //Super BROKEN
            
            if (Vector3.Angle(m_lastDirection, direction) > 0f)
            {

                List<Vector3> positions = new List<Vector3>();
                Vector3 start = Vector3.zero;

                Vector3 rotationAxis = Vector3.Cross(m_lastDirection, direction);
                float sign = Vector3.Dot(rotationAxis, Vector3.one)<0?-1:1;

                Vector3 rotator = RotatorVector(direction, rotationAxis, 0);
                Vector3 rotatorOffset = RotatorOffset(direction);

                float angle = (c_tau / 4f)/ angleIterations * sign;
                Debug.Log("Axis: " + rotationAxis + " angle: " + angle);
                for (int i = 0; i <= angleIterations; i++)
                {
                    rotator = RotatorVector(direction, rotationAxis, angle*i);
                    if(start==Vector3.zero) start = rotator;
                    Debug.DrawLine(rotatorOffset, rotator+rotatorOffset, Color.red, 30f);
                    Debug.DrawLine(rotator+rotatorOffset, start+rotatorOffset, Color.green, 30f);
                    height = Vector3.Distance(rotator, start);
                    Debug.Log(height);
                    if (start!=rotator)
                        positions.Add(rotator - start );
                    start = rotator;
                }
                for (int i=0; i < angleIterations; i++)
                {
                    BuildSegment(positions[i].normalized, radius, radius);
                }
            }
            height = h;
            
            
            if(Mathf.Abs(radius - m_lastRadius) > Mathf.Epsilon)
            {
                height = m_lastRadius > radius ? m_lastRadius * 1.5f : radius * 1.5f;
                BuildSegment(direction, m_lastRadius, radius);
            }

            height = h;
            m_lastRadius = radius;
            BuildSegment(direction, radius, radius);

            height = h;
        }
        private Vector3 RotatorOffset(Vector3 direction)
        {
            return m_pipeCursorPosition + direction*1.5f*radius;
        }
        private Vector3 RotatorVector(Vector3 direction, Vector3 rotationAxis, float angle)
        {
            Vector3 rotator = 1.5f*radius*-direction;
            Vector3 cross = Vector3.Cross(rotator, rotationAxis);
            float dot = Vector3.Dot(rotator, rotationAxis);
            float sign = Vector3.Dot(rotationAxis, Vector3.one);
            angle = sign < 0 ? angle : -angle;


            //Rodrigues formula
            return rotator*Mathf.Cos(angle) + cross*Mathf.Sin(angle)+rotationAxis*dot*(1 - Mathf.Cos(angle));
        }
        /// <summary>
        /// Build a segment of the mesh.
        /// </summary>
        /// <param name="direction">Direction to build in</param>
        /// <param name="radius1">Starting radius</param>
        /// <param name="radius2">End radius</param>
        /// <param name="radiusChanges">Does the radius change?</param>
        private void BuildSegment(Vector3 direction, float radius1, float radius2)
        {
            direction = direction.normalized;
            Vector3 cursorOffset = direction * height;
            Vector3 axi = GetCenterAxis(direction);
            Vector3 offsetStart = axi * radius1;
            Vector3 offsetEnd = axi * radius2;

            for (int i = 0; i < detail; i++)
            {
                float angle1 = c_tau / detail * i;
                float angle2 = c_tau / detail * (i + 1);
                Debug.Log("angle 1: " + angle1 + ", angle 2: " + angle2);

                Vector3 v1 = RotateVertex(offsetStart, direction, angle1);
                Vector3 v2 = RotateVertex(offsetStart, direction, angle2);
                Vector3 vt1 = RotateVertex(offsetEnd, direction, angle1)+direction*height;
                Vector3 vt2 = RotateVertex(offsetEnd, direction, angle2)+direction*height;
                Vector3[] normals = new Vector3[] { 
                    new Vector3(v1.x, v1.y, v1.z).normalized,
                    new Vector3(v2.x, v2.y, v2.z).normalized,
                    new Vector3(v1.x, v1.y, v1.z).normalized,
                    new Vector3(v2.x, v2.y, v2.z).normalized };
                v1 += m_pipeCursorPosition;
                v2 += m_pipeCursorPosition;
                vt1 += m_pipeCursorPosition;
                vt2 += m_pipeCursorPosition;
                //Debug.DrawLine(v1, v2, Color.magenta, 30f);
                //await Task.Delay(1000);
                //Debug.DrawLine(v1, vt1, Color.magenta, 30f);
                //await Task.Delay(1000);
                //Debug.DrawLine(v2, vt2, Color.magenta, 30f);
                //await Task.Delay(1000);
                //Debug.DrawLine(vt1, vt2, Color.magenta, 30f);
                //await Task.Delay(1000);
                //Debug.Log("Vertices: " + v1 + ", " + v2 + ", " + vt1 + ", " + vt2);
                //await Task.Delay(1000);

                CreatePolygons(v1, v2, vt1, vt2, 1, direction, normals);

            }

            UpdateMesh(direction, cursorOffset);
            Debug.Log("Segment ready...");
        }
        private Vector3 GetCenterAxis(Vector3 direction)
        {
            if (direction.x == 0)
                return new Vector3(1,0,0);
            if (direction.y == 0)
                return new Vector3(0, 1, 0);
            if (direction.z == 0)
                return new Vector3(0,0,1);
            return Vector3.zero;
        }

        private void UpdateMesh(Vector3 direction, Vector3 cursorOffset)
        {
            m_Mesh.vertices = m_vertices;
            m_Mesh.uv = m_UV;
            m_Mesh.normals = m_normals;
            m_Mesh.triangles = m_triangles;
            m_MeshFilter.mesh = m_Mesh;
            m_pipeCursorPosition += cursorOffset;
            m_lastDirection = direction;
        }

        private Vector3 RotateVertex(Vector3 v, Vector3 k, float angle)
        {
            Vector3 cross = Vector3.Cross(k,v);
            float dot = Vector3.Dot(k, v);

            //Rodrigues formula
            return v * Mathf.Cos(angle) + cross * Mathf.Sin(angle) + k * dot * (1 - Mathf.Cos(angle));
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
        void CreatePolygons(Vector3 from1, Vector3 from2, Vector3 to1, Vector3 to2, float polyCount, Vector3 direction, Vector3[] normals)
        {
          
            for (int i = 0; i < polyCount; i++)
            {
                Vector3[] poly = new[]
                {
                    to1,
                    to2,
                    from1,
                    from2
                };
                CreatePolygon(poly, normals);
            }
            
        }
        /// <summary>
        /// Create a new polygon and update mesh properties.
        /// </summary>
        /// <param name="vertices">Polygon's vertices</param>
        /// <param name="surfaceNormal">Surface normal of the polygon</param>
        public void CreatePolygon(Vector3[] vertices, Vector3[] normals)
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
            for (int i=0; i<vertices.Length; i++)
            {
                Vector3 vec3 = vertices[i];
                vec.Add(vec3);
                nor.Add(normals[i]);
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
