namespace 釣りバカ日誌.Models
{
    /// <summary>
    /// 日誌モデル
    /// </summary>
    public class Dairy
    {
        /// <summary>
        /// ID(No)
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 日付
        /// </summary>
        public int fishingdate { get; set; }

        /// <summary>
        /// 都道府県
        /// </summary>
        public string prefecturecode { get; set; } = string.Empty;

        /// <summary>
        /// 都道府県名
        /// </summary>
        public string prefecture { get; set; } = string.Empty;

        /// <summary>
        /// 市区町村名
        /// </summary>
        public string municipalities { get; set; } = string.Empty;

        /// <summary>
        /// 地番
        /// </summary>
        public string addressnumber { get; set; } = string.Empty;

        /// <summary>
        /// 地点記号(潮汐)
        /// </summary>
        public string locationcode { get; set; } = string.Empty;

        /// <summary>
        /// 釣果
        /// </summary>
        public string fishingresults { get; set; } = string.Empty;

        /// <summary>
        /// 備考
        /// </summary>
        public string remarks { get; set; } = string.Empty;

        /// <summary>
        /// フォルダID
        /// </summary>
        public string folderid { get; set; } = string.Empty;

    }
}
