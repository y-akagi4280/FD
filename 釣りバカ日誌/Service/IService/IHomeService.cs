using 釣りバカ日誌.Models;

namespace 釣りバカ日誌.Service.IService
{
    public interface IHomeService
    {
        /// <summary>
        /// Homeデータ取得処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HomeViewModel GetHomeData(HomeViewModel model);

        /// <summary>
        /// Editデータ取得処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EditViewModel GetEditData(EditViewModel model);

        /// <summary>
        /// 市町村リスト取得処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EditViewModel GetMunicipalities(EditViewModel model);

        /// <summary>
        /// 潮汐データ取得処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EditViewModel GetTideData(EditViewModel model);

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EditViewModel Update(EditViewModel model);

        /// <summary>
        /// 削除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HomeViewModel Delete(HomeViewModel model);
    }
}
