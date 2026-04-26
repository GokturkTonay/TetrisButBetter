using UnityEngine;

namespace Tomino.View
{
    /// <summary>
    /// Her satır (colorRow) için ayrı bir pivot noktası tanımlanır.
    /// O satırdaki tüm DeckPieceCard'lar kendi satırının pivotuna yönelik döner.
    /// DeckPieceCard'ların kendi RectTransform pivot'ları (0.5,0.5) olduğu için
    /// kartlar görsel merkezlerinden döner.
    /// </summary>
    public class DeckFanLayout : MonoBehaviour
    {
        [Header("Satır Containers (DeckCardsManager ile aynı referanslar)")]
        public Transform colorRow_0;
        public Transform colorRow_1;
        public Transform colorRow_2;
        public Transform colorRow_3;

        [Header("Her Satırın Pivot Noktası")]
        [Tooltip("colorRow_0 içindeki kartlar bu noktaya yönelik döner.")]
        public Transform rowPivot_0;

        [Tooltip("colorRow_1 içindeki kartlar bu noktaya yönelik döner.")]
        public Transform rowPivot_1;

        [Tooltip("colorRow_2 içindeki kartlar bu noktaya yönelik döner.")]
        public Transform rowPivot_2;

        [Tooltip("colorRow_3 içindeki kartlar bu noktaya yönelik döner.")]
        public Transform rowPivot_3;

        [Header("Fan Sınır Açısı")]
        [Range(0f, 90f)]
        [Tooltip("Herhangi bir kartın alabileceği maksimum Z rotasyon açısı (derece).")]
        public float maxAngle = 45f;

        private void LateUpdate()
        {
            ApplyRowFan(colorRow_0, rowPivot_0);
            ApplyRowFan(colorRow_1, rowPivot_1);
            ApplyRowFan(colorRow_2, rowPivot_2);
            ApplyRowFan(colorRow_3, rowPivot_3);
        }

        /// <summary>
        /// Tüm kartları fan düzeninde hemen düzenle.
        /// </summary>
        public void ArrangeCards()
        {
            ApplyRowFan(colorRow_0, rowPivot_0);
            ApplyRowFan(colorRow_1, rowPivot_1);
            ApplyRowFan(colorRow_2, rowPivot_2);
            ApplyRowFan(colorRow_3, rowPivot_3);
        }

        private void ApplyRowFan(Transform row, Transform pivot)
        {
            if (row == null || pivot == null) return;

            var cards = row.GetComponentsInChildren<DeckPieceCard>(includeInactive: false);
            if (cards == null || cards.Length == 0) return;

            Vector3 pivotWorld = pivot.position;

            foreach (var card in cards)
            {
                if (card == null || !card.gameObject.activeInHierarchy) continue;

                // Pivot → Kart vektörü (kartın görsel merkezi etrafında döner,
                // DeckPieceCard.Initialize() zaten pivot'u (0.5,0.5) yapıyor)
                Vector3 dir = card.transform.position - pivotWorld;

                // Negatif: kartın alt yüzü pivota baksın
                float angle = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
                angle = Mathf.Clamp(angle, -maxAngle, maxAngle);

                card.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}
