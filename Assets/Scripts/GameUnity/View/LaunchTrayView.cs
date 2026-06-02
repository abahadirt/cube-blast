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


        // TODO[P0] : Review approach
        public void TempUpdateShooterAmmo(int shooterId, int ammo)
        {

            if (_registry.TryGet(shooterId, out ShooterView shooter))
            {
                shooter.SetAmmo(ammo);
            }
            else
            {
                Debug.LogWarning($"ShooterView not found. shooterId: {shooterId}. This is a bad sign...");
            }
            if (ammo <= 0)
            {
                //shooter.MoveToPosition(new Vector3(-3,-2.5f), 0.5f).OnComplete(() => Destroy(shooter.gameObject));
                TempPlayDepartureAnimation(shooter);
            }
        }


        // TODO[P0]: Review this approach. Code will need to be updated when the animation changes.
        public void TempPlayDepartureAnimation(ShooterView shooter)
        {
            Sequence exitSequence = DOTween.Sequence();
            exitSequence.AppendInterval(0.05f);

            exitSequence.Append(shooter.transform.DOMoveY(transform.position.y + 0.5f, 0.2f));

            // 2. Add a 0.2 seconds delay.
            exitSequence.AppendInterval(0.05f);
            int exitX = -4;
            float speedX = 10f;
            float leftDuration = (shooter.transform.position.x - exitX) / speedX;
            // 3. Move left after the delay (duration: 0.4 seconds).
            exitSequence.Append(shooter.transform.DOMoveX(-4, leftDuration));

            exitSequence.OnComplete(() =>
            {
                Destroy(shooter.gameObject);
            });
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

