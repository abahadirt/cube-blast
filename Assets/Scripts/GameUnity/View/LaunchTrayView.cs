using Blast.GamePresentation.Contract;
using Blast.GameUnity.Registry;
using DG.Tweening;
using System.Collections.Generic;
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


        // TODO[P0]: yaklaþým review edilecek
        public void TempUpdateShooterAmmo(int shooterId, int ammo)
        {

            if (_registry.TryGet(shooterId, out ShooterView shooter))
            {
                shooter.SetAmmo(ammo);
            }
            else
            {
                Debug.LogWarning($"ShooterView bulunamadý. shooterId: {shooterId}. Bu log hayra alamet deðil...");
            }
            if (ammo <= 0)
            {
                //shooter.MoveToPosition(new Vector3(-3,-2.5f), 0.5f).OnComplete(() => Destroy(shooter.gameObject));
                TempPlayDepartureAnimation(shooter);
            }
        }


        // TODO[P0]: yaklaþým review edilecek, anim degisince kod da degisecek.
        public void TempPlayDepartureAnimation(ShooterView shooter)
        {
            Sequence exitSequence = DOTween.Sequence();
            exitSequence.AppendInterval(0.05f);

            exitSequence.Append(shooter.transform.DOMoveY(transform.position.y + 0.5f, 0.2f));

            // 2. Araya 0.2 saniye bekleme süresi (boþluk) ekle
            exitSequence.AppendInterval(0.05f);
            int exitX = -4;
            float speedX = 10f;
            float leftDuration = (shooter.transform.position.x - exitX) / speedX;
            // 3. Bekleme bittikten sonra sola git (0.4 saniye sürsün)
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
            if (!_registry.TryGet(shooterId, out ShooterView shooter)) // shooter otomatik initialize ediliyor, csharpa yeni gelmiþ herhalde
            {
                Debug.LogWarning($"ShooterView bulunamadý. shooterId: {shooterId}. Bu log hayra alamet deðil...");
                return;
            }

            shooter.MoveToPosition(GetSlotPosition(slotIndex), duration);
        }


        public void PlayMergeAnimation(int survivorShooterID, IReadOnlyList<int> consumedShooterIds, int totalAmmo)
        {

            if (!_registry.TryGet(survivorShooterID, out ShooterView survivorShooter))
            {
                Debug.LogWarning($"Survivor ShooterView bulunamadý. ID: {survivorShooterID}");
                return;
            }

            int completedCount = 0;
            int totalCount = consumedShooterIds.Count;


            Vector3 targetPos = survivorShooter.transform.position;
            Debug.Log($"Merge animasyonu baþlýyor. Survivor ID: {survivorShooterID}, Target Pos: {targetPos}");
            foreach (int consumedId in consumedShooterIds)
            {
                if (_registry.TryGet(consumedId, out ShooterView consumedShooter))
                {
                    consumedShooter.MoveToPosition(targetPos, _mergeAnimationDuration).OnComplete(() =>
                    {
                        Destroy(consumedShooter.gameObject);
                        completedCount++;
                        if (completedCount == totalCount)
                        {
                            survivorShooter.SetAmmo(totalAmmo);
                        }
                    });
                }
                else
                {
                    Debug.LogWarning($"ShooterView bulunamadý. consumedId: {consumedId}. Bu log hayra alamet deðil...");
                }
            }


        }

    }




}

