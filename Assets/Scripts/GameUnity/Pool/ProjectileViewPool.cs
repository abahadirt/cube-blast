using UnityEngine;
using UnityEngine.Pool;
using Blast.GameUnity.View;

namespace Blast.GameUnity.Pool
{
    public class ProjectileViewPool : MonoBehaviour
    {
        [SerializeField] private ProjectileView ballPrefab;
        [SerializeField] private int initialCapacity = 20;
        [SerializeField] private int maxCapacity = 100;

        private ObjectPool<ProjectileView> _pool;

        void Awake()
        {
            _pool = new ObjectPool<ProjectileView>(
                createFunc: () => Instantiate(ballPrefab, transform),
                actionOnGet: ball => ball.gameObject.SetActive(true),
                actionOnRelease: ball =>
                {
                    ball.transform.SetParent(transform); // hiyerarþiyi temiz tut
                    ball.gameObject.SetActive(false);
                },
                actionOnDestroy: ball => Destroy(ball.gameObject),
                collectionCheck: true,
                defaultCapacity: initialCapacity,
                maxSize: maxCapacity
            );

            Prewarm(initialCapacity);
        }

        private void Prewarm(int count)
        {
            var temp = new ProjectileView[count];
            for (int i = 0; i < count; i++)
                temp[i] = _pool.Get();

            for (int i = 0; i < count; i++)
                _pool.Release(temp[i]);
        }

        public ProjectileView Get() => _pool.Get();
        public void Release(ProjectileView ball) => _pool.Release(ball);
    }
}