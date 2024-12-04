namespace 釣りバカ日誌.Models
{
    public class Tide
    {
        /// <summary>
        /// 掲載年
        /// </summary>
        public int thisyear { get; set; }

        /// <summary>
        /// 地点記号
        /// </summary>
        public string locationcode { get; set; } = string.Empty;

        /// <summary>
        /// 番号
        /// </summary>
        public int number { get; set; }

        /// <summary>
        /// 地点名
        /// </summary>
        public string locationname { get; set; } = string.Empty;

        /// <summary>
        /// 都道府県コード
        /// </summary>
        public string prefecturecode { get; set; } = string.Empty;
    }
}
