using UnityEngine;

public class Jele : MonoBehaviour
{
   [SerializeField] private float stiffness = 5f;
    [SerializeField] private float damping = 1f;
    [SerializeField] private float mass = 1f;
    [SerializeField] private float forceMultiplier = 2f;

    private Mesh _originalMesh, _deformedMesh;
    private Vector3[] _originalVertices, _deformedVertices;
    private VertexData[] _vertexDatas;

    private void Start()
    {
        // Загружаем меш
        _originalMesh = GetComponent<MeshFilter>().mesh;
        _deformedMesh = Instantiate(_originalMesh);
        GetComponent<MeshFilter>().mesh = _deformedMesh;

        // Копируем вершины
        _originalVertices = _originalMesh.vertices;
        _deformedVertices = new Vector3[_originalVertices.Length];
        _vertexDatas = new VertexData[_originalVertices.Length];

        for (int i = 0; i < _originalVertices.Length; i++)
        {
            _deformedVertices[i] = _originalVertices[i];
            _vertexDatas[i] = new VertexData(transform.TransformPoint(_originalVertices[i]));
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < _vertexDatas.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(_originalVertices[i]);
            Vector3 velocity = _vertexDatas[i].velocity;

            // Пружинная сила
            Vector3 force = (worldPos - _vertexDatas[i].position) * stiffness;
            Vector3 dampingForce = velocity * -damping;

            Vector3 acceleration = (force + dampingForce) / mass;
            _vertexDatas[i].velocity += acceleration * Time.fixedDeltaTime;
            _vertexDatas[i].position += _vertexDatas[i].velocity * Time.fixedDeltaTime;

            // Обновляем вершину
            _deformedVertices[i] = transform.InverseTransformPoint(_vertexDatas[i].position);
        }

        // Применяем обновленный меш
        _deformedMesh.vertices = _deformedVertices;
        _deformedMesh.RecalculateNormals();
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            for (int i = 0; i < _vertexDatas.Length; i++)
            {
                float distance = Vector3.Distance(contact.point, _vertexDatas[i].position);
                float impactForce = forceMultiplier / (1f + distance);
                Vector3 forceDirection = (_vertexDatas[i].position - contact.point).normalized;

                _vertexDatas[i].velocity += forceDirection * impactForce;
            }
        }
    }

    private class VertexData
    {
        public Vector3 position;
        public Vector3 velocity;

        public VertexData(Vector3 pos)
        {
            position = pos;
            velocity = Vector3.zero;
        }
    }
}
