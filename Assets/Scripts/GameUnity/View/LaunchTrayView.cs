using Blast.GamePresentation.Contract;
using Blast.GameUnity.Registry;
using DG.Tweening;
using System.Linq;
using UnityEngine;


namespace Blast.GameUnity.View
{
    public class LaunchTrayView : MonoBehaviour, ILaunchTrayView
    {
        
        [SerializeField] private Transform[] slotPositions;

        [SerializeField] private float _mergeAnimationDuration;


        private ShooterViewRegistry _registry;
        public void Construct(ShooterViewRegistry registry)
        {
            _registry = registry;
        }


        public void UpdateShooterAmmo(int shooterId, int ammo)
        {
            if (_registry.TryGet(shooterId, out ShooterView shooter))
            {
                shooter.SetAmmo(ammo);
            }
            else
            {
                Debug.LogWarning($"[LaunchTrayView] ShooterView not found. shooterId: {shooterId}");
            }
        }


        public void PlayDepartureAnimation(int shooterId)
        {
            if (!_registry.TryGet(shooterId, out ShooterView shooter))
            {
                Debug.LogWarning($"[LaunchTrayView] Departure: ShooterView not found. shooterId: {shooterId}.");
                return;
            }

            _registry.Unregister(shooterId);

            const float exitX = -4f;
            const float speedX = 10f;
            float leftDuration = (shooter.transform.position.x - exitX) / speedX;

            DOTween.Sequence()
                .AppendInterval(0.05f)
                .Append(shooter.transform.DOMoveY(transform.position.y + 0.5f, 0.2f))
                .AppendInterval(0.05f)
                .Append(shooter.transform.DOMoveX(exitX, leftDuration))
                .OnComplete(() => Destroy(shooter.gameObject));
        }

        public Vector3 GetSlotPosition(int index)
        {
            return slotPositions[index].position;
        }

        public void PlayArrivalAnimation(int shooterId, int slotIndex, float duration)
        {
            if (!_registry.TryGet(shooterId, out ShooterView shooter)) // inline out variable initialization
            {
                Debug.LogWarning($"[LaunchTrayView] ShooterView not found. shooterId: {shooterId}.");
                return;
            }

            shooter.MoveToPosition(GetSlotPosition(slotIndex), duration);
        }


        public void PlayMergeAnimation(int survivorShooterId, int consumedShooterId1, int consumedShooterId2, int totalAmmo)
        {
            if (!_registry.TryGet(survivorShooterId, out ShooterView survivorShooter))
            {
                Debug.LogWarning($"[LaunchTrayView] Survivor ShooterView not found. ID: {survivorShooterId}");
                return;
            }

            int completedCount = 0;
            const int totalCount = 2;

            Vector3 targetPos = survivorShooter.transform.position;

            void AnimateAndDestroy(int consumedId)
            {
                if (!_registry.TryGet(consumedId, out ShooterView consumedShooter))
                {
                    Debug.LogWarning($"[LaunchTrayView] ShooterView not found. consumedId: {consumedId}.");
                    return;
                }

                consumedShooter.MoveToPosition(targetPos, _mergeAnimationDuration).OnComplete(() =>
                {
                    Destroy(consumedShooter.gameObject);
                    completedCount++;
                    if (completedCount == totalCount)
                        survivorShooter.SetAmmo(totalAmmo);
                });
            }

            AnimateAndDestroy(consumedShooterId1);
            AnimateAndDestroy(consumedShooterId2);
        }

    }




}

