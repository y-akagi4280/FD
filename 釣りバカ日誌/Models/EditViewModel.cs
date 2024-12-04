using static 釣りバカ日誌.Service.HomeService;

namespace 釣りバカ日誌.Models
{
    public class EditViewModel
    {
        /// <summary>
        /// 都道府県
        /// </summary>
        public List<Prefecture> Prefectures { get; set; } = new List<Prefecture>();

        /// <summary>
        /// 都道府県コード
        /// </summary>
        public string prefecturecode { get; set; } = string.Empty;

        /// <summary>
        /// 市町村リスト
        /// </summary>
        public municipalities Municipalities { get; set; } = new municipalities();

        /// <summary>
        /// 港コード
        /// </summary>
        public List<Tide> Tides { get; set; } = new List<Tide>();

        /// <summary>
        /// ID(No)
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 更新(0:新規, 1:更新)
        /// </summary>
        public int upd { get; set; }

        /// <summary>
        /// 写真更新(0:なし, 1:あり)
        /// </summary>
        public int photoupd { get; set; }

        /// <summary>
        /// 地点記号
        /// </summary>
        public string locationcode { get; set; } = string.Empty;

        /// <summary>
        /// 年
        /// </summary>
        public string year { get; set; } = string.Empty;

        /// <summary>
        /// 月
        /// </summary>
        public string month { get; set; } = string.Empty;

        /// <summary>
        /// 日
        /// </summary>
        public string day { get; set; } = string.Empty;

        /// <summary>
        /// 潮汐データ
        /// </summary>
        public List<TimeData> timedatas { get; set; } = new List<TimeData>();

        /// <summary>
        /// 更新用
        /// </summary>
        public AllDairy dairy { get; set; } = new AllDairy();
    }
}
