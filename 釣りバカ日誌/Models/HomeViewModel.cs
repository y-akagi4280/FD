namespace 釣りバカ日誌.Models
{
    public class HomeViewModel
    {
        /// <summary>
        /// 日誌
        /// </summary>
        public List<AllDairy> dairies { get; set; } = new List<AllDairy>();

        /// <summary>
        /// ID(削除)
        /// </summary>
        public int id { get; set; }
    }
}
