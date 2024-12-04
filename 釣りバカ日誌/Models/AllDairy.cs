namespace 釣りバカ日誌.Models
{
    public class AllDairy
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

        /// <summary>
        /// 同伴者1
        /// </summary>
        public string companion1 { get; set; } = string.Empty;

        /// <summary>
        /// 同伴者2
        /// </summary>
        public string companion2 { get; set; } = string.Empty;

        /// <summary>
        /// 同伴者3
        /// </summary>
        public string companion3 { get; set; } = string.Empty;

        /// <summary>
        /// 同伴者4
        /// </summary>
        public string companion4 { get; set; } = string.Empty;

        /// <summary>
        /// 同伴者5
        /// </summary>
        public string companion5 { get; set; } = string.Empty;

        /// <summary>
        /// 写真ID
        /// </summary>
        public List<string> photoid { get; set; } = new List<string>();

    }
}
