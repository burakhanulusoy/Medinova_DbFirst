using Microsoft.ML.Data;

namespace Medinova.DTOs
{
    public class ComplainData
    {
        [LoadColumn(0)]
        public string Description { get; set; } // Örn: "Göğsüm sıkışıyor"

        [LoadColumn(1)]
        public bool IsUrgent { get; set; } // Örn: true (Acil)
    }

    public class ComplaintPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool IsUrgent { get; set; } // Yapay zekanın kararı: Acil mi?

        public float Probability { get; set; } // % kaç ihtimalle acil olduğunu söyler
        public float Score { get; set; }
    }
}