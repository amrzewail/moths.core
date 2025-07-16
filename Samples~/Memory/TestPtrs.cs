using UnityEngine;

namespace Moths.Memory.Samples
{
    public class TestPtrs : MonoBehaviour
    {
        private struct Data
        {
            public ManagedPtr<Transform> managed;
        }

        private Data _data;
        private Ptr<Data> _ptr;

        private void Start()
        {
            _data = new();
            _ptr = new Ptr<Data>(ref _data);
            _ptr.Ref.managed = new ManagedPtr<Transform>(Camera.main.transform);

            Debug.Log(_ptr.Ref.managed.IsAlive);

            Destroy(Camera.main.gameObject);
        }

        private void Update()
        {
            Debug.Log(_ptr.Ref.managed.IsAlive);
        }

        private void OnDestroy()
        {
            _ptr.Ref.managed.Dispose();
        }

        public void Log()
        {
            Debug.Log("Test ptr!!");
        }
    }
}