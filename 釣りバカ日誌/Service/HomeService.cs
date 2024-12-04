using Google.Apis.Drive.v3;
using Newtonsoft.Json;
using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Transactions;
using 釣りバカ日誌.Models;
using 釣りバカ日誌.Service.IService;

namespace 釣りバカ日誌.Service
{
    public class HomeService : IHomeService
    {
        public IConfiguration _conf { get; set; }

        public HomeService(IConfiguration conf)
        {
            _conf = conf;
            FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\釣り日誌用";
        }

        private NpgsqlConnection Conn = new();

        static string[] Scopes = { DriveService.Scope.DriveFile };

        public string FolderPath { get; set; } = string.Empty;

        /// <summary>
        /// Homeデータ取得処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HomeViewModel GetHomeData(HomeViewModel model)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT *");
            sql.AppendLine("  FROM public.dairy AS t1");
            sql.AppendLine("  LEFT JOIN public.companion AS t2");
            sql.AppendLine("    ON t1.id = t2.id");
            sql.AppendLine(" ORDER BY fishingdate desc");

            List<Parameter> parameters = new List<Parameter>();

            model.dairies = FetchAll<AllDairy>(sql.ToString(), parameters);

            return model;
        }

        /// <summary>
        /// Editデータ取得処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EditViewModel GetEditData(EditViewModel model)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT t1.id AS id");
            sql.AppendLine("     , t1.title AS title");
            sql.AppendLine("     , t1.fishingdate AS fishingdate");
            sql.AppendLine("     , t1.prefecturecode AS prefecturecode");
            sql.AppendLine("     , t1.prefecture AS prefecture");
            sql.AppendLine("     , t1.municipalities AS municipalities");
            sql.AppendLine("     , t1.addressnumber AS addressnumber");
            sql.AppendLine("     , t1.locationcode AS locationcode");
            sql.AppendLine("     , t1.fishingresults AS fishingresults");
            sql.AppendLine("     , t1.remarks AS remarks");
            sql.AppendLine("     , t1.folderid AS folderid");
            sql.AppendLine("     , t2.companion1 AS companion1");
            sql.AppendLine("     , t2.companion2 AS companion2");
            sql.AppendLine("     , t2.companion3 AS companion3");
            sql.AppendLine("     , t2.companion4 AS companion4");
            sql.AppendLine("     , t2.companion5 AS companion5");
            sql.AppendLine("  FROM public.dairy AS t1");
            sql.AppendLine("  LEFT JOIN public.companion AS t2");
            sql.AppendLine("    ON t1.id = t2.id");
            sql.AppendLine(" WHERE t1.id = @id");

            List<Parameter> parameters = new List<Parameter>()
            {
                new Parameter("id", model.id, DbType.Int32)
            };

            model.dairy = FetchOne<AllDairy>(sql.ToString(), parameters);

            if (!string.IsNullOrWhiteSpace(model.dairy.folderid))
            {
                DriveService service = GetService();

                List<string> fileID = GetFileID(service, model.dairy.folderid);

                model.dairy.photoid = fileID;
            }

            return model;
        }

        /// <summary>
        /// 潮汐データ取得処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EditViewModel GetTideData(EditViewModel model)
        {
            try
            {
                var task = GetResponse($"https://www.data.jma.go.jp/kaiyou/data/db/tide/suisan/txt/{model.year}/{model.locationcode}.txt");

                var result = task.Result;

                string[] lines = result.Split('\n');

                string search = model.month.PadLeft(2) + model.day.PadLeft(2) + model.locationcode;
                foreach(string line in lines )
                {
                    if(0 <= line.IndexOf(search))
                    {
                        List<TimeData> timeDatas = new List<TimeData>();
                        for (int i = 0; i < 24; i++)
                        {
                            TimeData timeData = new TimeData();
                            timeData.Hour = i;
                            timeData.High = int.Parse(line.Substring(0 + i * 3, 3));

                            if (timeData.Hour == 999)
                            {
                                continue;
                            }

                            timeDatas.Add(timeData);
                        }

                        model.timedatas = timeDatas;

                        break;
                    }
                }
            }
            catch
            {
                Debug.WriteLine("潮汐データ取得エラー");
            }

            return model;
        }

        /// <summary>
        /// 市町村リスト取得処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EditViewModel GetMunicipalities(EditViewModel model)
        {
            try
            {
                var task = Get<municipalities>($"https://www.land.mlit.go.jp/webland/api/CitySearch?area={model.prefecturecode}");

                var result = task.Result;

                model.Municipalities = result;
            }
            catch
            {
                Debug.WriteLine("市町村取得エラー");
            }

            try
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("SELECT *");
                sql.AppendLine("  FROM public.tide");
                sql.AppendLine(" WHERE thisyear = '2024'");
                sql.AppendLine("   AND prefecturecode = @code");

                List<Parameter> parameters = new List<Parameter>() { 
                    new Parameter("code", model.prefecturecode)
                };

                model.Tides = FetchAll<Tide>(sql.ToString(), parameters);
            }
            catch
            {
                Debug.WriteLine("港コード取得エラー");
            }

            return model;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EditViewModel Update(EditViewModel model)
        {
            try
            {
                // 新規
                if (model.upd == 0)
                {
                    // アップロード
                    string folderId = string.Empty;
                    if (model.photoupd == 1)
                    {
                        folderId = Upload();
                    }

                    // 日誌マスター追加
                    StringBuilder sql = new StringBuilder();

                    sql.AppendLine("INSERT INTO public.dairy (");
                    sql.AppendLine("       id");
                    sql.AppendLine("     , title");
                    sql.AppendLine("     , fishingdate");
                    sql.AppendLine("     , prefecturecode");
                    sql.AppendLine("     , prefecture");
                    sql.AppendLine("     , municipalities");
                    sql.AppendLine("     , addressnumber");
                    sql.AppendLine("     , locationcode");
                    sql.AppendLine("     , fishingresults");
                    sql.AppendLine("     , remarks");
                    sql.AppendLine("     , folderid)");
                    sql.AppendLine("VALUES (");
                    sql.AppendLine("       @id");
                    sql.AppendLine("     , @title");
                    sql.AppendLine("     , @fishingdate");
                    sql.AppendLine("     , @prefecturecode");
                    sql.AppendLine("     , @prefecture");
                    sql.AppendLine("     , @municipalities");
                    sql.AppendLine("     , @addressnumber");
                    sql.AppendLine("     , @locationcode");
                    sql.AppendLine("     , @fishingresults");
                    sql.AppendLine("     , @remarks");
                    sql.AppendLine("     , @folderid)");

                    List<Parameter> parameters = new List<Parameter>() {
                        new Parameter("id", model.dairy.id, DbType.Int32),
                        new Parameter("title", model.dairy.title ?? ""),
                        new Parameter("fishingdate", model.dairy.fishingdate, DbType.Int32),
                        new Parameter("prefecturecode", model.dairy.prefecturecode ?? ""),
                        new Parameter("prefecture", model.dairy.prefecture ?? ""),
                        new Parameter("municipalities", model.dairy.municipalities ?? ""),
                        new Parameter("addressnumber", model.dairy.addressnumber ?? ""),
                        new Parameter("locationcode", model.dairy.locationcode ?? ""),
                        new Parameter("fishingresults", model.dairy.fishingresults ?? ""),
                        new Parameter("remarks", model.dairy.remarks ?? ""),
                        new Parameter("folderid", folderId),
                    };

                    UpSert(sql.ToString(), parameters);

                    // 同伴者マスター追加
                    sql = new StringBuilder();

                    sql.AppendLine("INSERT INTO public.companion");
                    sql.AppendLine("VALUES (");
                    sql.AppendLine("       @id");
                    sql.AppendLine("     , @companion1");
                    sql.AppendLine("     , @companion2");
                    sql.AppendLine("     , @companion3");
                    sql.AppendLine("     , @companion4");
                    sql.AppendLine("     , @companion5)");

                    parameters = new List<Parameter>() {
                        new Parameter("id", model.dairy.id, DbType.Int32),
                        new Parameter("companion1", model.dairy.companion1 ?? ""),
                        new Parameter("companion2", model.dairy.companion2 ?? ""),
                        new Parameter("companion3", model.dairy.companion3 ?? ""),
                        new Parameter("companion4", model.dairy.companion4 ?? ""),
                        new Parameter("companion5", model.dairy.companion5 ?? ""),
                    };

                    UpSert(sql.ToString(), parameters);

                }
                // 更新
                else if (model.upd == 1)
                {
                    // 日誌マスター更新
                    StringBuilder sql = new StringBuilder();

                    List<Parameter> parameters = new();

                    // アップロード
                    string folderId = string.Empty;
                    if (model.photoupd == 1)
                    {
                        DriveService service = GetService();

                        // 変更前フォルダ削除
                        DeleteFile(service, model.dairy.folderid);

                        folderId = Upload();

                        sql.AppendLine("UPDATE public.dairy");
                        sql.AppendLine("   SET title = @title");
                        sql.AppendLine("     , fishingdate = @fishingdate");
                        sql.AppendLine("     , prefecturecode = @prefecturecode");
                        sql.AppendLine("     , prefecture = @prefecture");
                        sql.AppendLine("     , municipalities = @municipalities");
                        sql.AppendLine("     , addressnumber = @addressnumber");
                        sql.AppendLine("     , locationcode = @locationcode");
                        sql.AppendLine("     , fishingresults = @fishingresults");
                        sql.AppendLine("     , remarks = @remarks");
                        sql.AppendLine("     , folderid = @folderid");
                        sql.AppendLine(" WHERE id = @id");

                        parameters = new List<Parameter>() {
                            new Parameter("title", model.dairy.title ?? ""),
                            new Parameter("fishingdate", model.dairy.fishingdate, DbType.Int32),
                            new Parameter("prefecturecode", model.dairy.prefecturecode ?? ""),
                            new Parameter("prefecture", model.dairy.prefecture ?? ""),
                            new Parameter("municipalities", model.dairy.municipalities ?? ""),
                            new Parameter("addressnumber", model.dairy.addressnumber ?? ""),
                            new Parameter("locationcode", model.dairy.locationcode ?? ""),
                            new Parameter("fishingresults", model.dairy.fishingresults ?? ""),
                            new Parameter("remarks", model.dairy.remarks ?? ""),
                            new Parameter("folderid", folderId),
                            new Parameter("id", model.dairy.id, DbType.Int32),
                        };

                        UpSert(sql.ToString(), parameters);
                    }
                    else
                    {
                        sql.AppendLine("UPDATE public.dairy");
                        sql.AppendLine("   SET title = @title");
                        sql.AppendLine("     , fishingdate = @fishingdate");
                        sql.AppendLine("     , prefecturecode = @prefecturecode");
                        sql.AppendLine("     , prefecture = @prefecture");
                        sql.AppendLine("     , municipalities = @municipalities");
                        sql.AppendLine("     , addressnumber = @addressnumber");
                        sql.AppendLine("     , locationcode = @locationcode");
                        sql.AppendLine("     , fishingresults = @fishingresults");
                        sql.AppendLine("     , remarks = @remarks");
                        sql.AppendLine(" WHERE id = @id");

                        parameters = new List<Parameter>() {
                            new Parameter("title", model.dairy.title ?? ""),
                            new Parameter("fishingdate", model.dairy.fishingdate, DbType.Int32),
                            new Parameter("prefecturecode", model.dairy.prefecturecode ?? ""),
                            new Parameter("prefecture", model.dairy.prefecture ?? ""),
                            new Parameter("municipalities", model.dairy.municipalities ?? ""),
                            new Parameter("addressnumber", model.dairy.addressnumber ?? ""),
                            new Parameter("locationcode", model.dairy.locationcode ?? ""),
                            new Parameter("fishingresults", model.dairy.fishingresults ?? ""),
                            new Parameter("remarks", model.dairy.remarks ?? ""),
                            new Parameter("id", model.dairy.id, DbType.Int32),
                        };

                        UpSert(sql.ToString(), parameters);
                    }

                    // 同伴者マスター更新
                    sql = new StringBuilder();

                    sql.AppendLine("UPDATE public.companion");
                    sql.AppendLine("   SET companion1 = @companion1");
                    sql.AppendLine("     , companion2 = @companion2");
                    sql.AppendLine("     , companion3 = @companion3");
                    sql.AppendLine("     , companion4 = @companion4");
                    sql.AppendLine("     , companion5 = @companion5");
                    sql.AppendLine(" WHERE id = @id");

                    parameters = new List<Parameter>() {
                        new Parameter("companion1", model.dairy.companion1 ?? ""),
                        new Parameter("companion2", model.dairy.companion2 ?? ""),
                        new Parameter("companion3", model.dairy.companion3 ?? ""),
                        new Parameter("companion4", model.dairy.companion4 ?? ""),
                        new Parameter("companion5", model.dairy.companion5 ?? ""),
                        new Parameter("id", model.dairy.id, DbType.Int32),
                    };

                    UpSert(sql.ToString(), parameters);
                }

                // 写真フォルダ削除
                DeleteAllFiles();
            }
            catch
            {
                Debug.WriteLine("更新エラー");
            }

            return model;
        }

        /// <summary>
        /// 削除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HomeViewModel Delete(HomeViewModel model)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT t1.folderid AS folderid");
            sql.AppendLine("  FROM public.dairy AS t1");
            sql.AppendLine(" WHERE t1.id = @id");

            List<Parameter> parameters = new List<Parameter>()
            {
                new Parameter("id", model.id, DbType.Int32)
            };

            Dairy dairy = FetchOne<Dairy>(sql.ToString(), parameters);

            if (!string.IsNullOrWhiteSpace(dairy.folderid))
            {
                DriveService service = GetService();

                // 変更前フォルダ削除
                DeleteFile(service, dairy.folderid);
            }

            // 日誌マスター削除
            sql = new StringBuilder();

            sql.AppendLine("DELETE FROM public.dairy");
            sql.AppendLine(" WHERE id = @id");

            parameters = new List<Parameter>() {
                new Parameter("id", model.id, DbType.Int32),
            };

            UpSert(sql.ToString(), parameters);

            // 同伴者マスター削除
            sql = new StringBuilder();

            sql.AppendLine("DELETE FROM public.companion");
            sql.AppendLine(" WHERE id = @id");

            parameters = new List<Parameter>() {
                new Parameter("id", model.id, DbType.Int32),
            };

            UpSert(sql.ToString(), parameters);

            return model;
        }

        public DriveService GetService()
        {
            FileStream fs = new FileStream("dairy-415802-9cd2c54ed13a.json", FileMode.Open, FileAccess.Read);
            Google.Apis.Auth.OAuth2.GoogleCredential credential;
            try
            {
                credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromStream(fs).CreateScoped(Scopes);
            }
            finally
            {
                fs.Close();
            }

            Google.Apis.Services.BaseClientService.Initializer init = new Google.Apis.Services.BaseClientService.Initializer();
            init.HttpClientInitializer = credential;
            init.ApplicationName = "Dairy";
            DriveService service = new DriveService(init);

            return service;
        }

        /// <summary>
        /// Google Drive アップロード
        /// </summary>
        /// <returns></returns>
        public string Upload()
        {
            DriveService service = GetService();

            string folderId = CreateFolder(service);

            if (string.IsNullOrEmpty(folderId))
            {
                return string.Empty;
            }

            Google.Apis.Upload.IUploadProgress prog;
            // 釣り日誌用フォルダ内のjpgファイルを取得
            string[] filesFullPath = Directory.GetFiles(FolderPath, "*.*", SearchOption.TopDirectoryOnly);

            foreach (string fileFullPath in filesFullPath)
            {
                FileStream fsu = new FileStream(fileFullPath, FileMode.Open);

                try
                {
                    Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider fpv = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
                    string ContentType;
                    fpv.TryGetContentType(fileFullPath, out ContentType);

                    Google.Apis.Drive.v3.Data.File meta = new Google.Apis.Drive.v3.Data.File();
                    meta.Name = Path.GetFileName(fileFullPath);
                    meta.MimeType = ContentType;
                    meta.Parents = new List<string>() { folderId };

                    Google.Apis.Drive.v3.FilesResource.CreateMediaUpload req = service.Files.Create(meta, fsu, ContentType);
                    req.Fields = "id, name";
                    //prog = (Google.Apis.Upload.IUploadProgress)req.UploadAsync();

                    var result = req.UploadAsync().Result;


                }
                finally
                {
                    fsu.Close();
                }

                //DoUpload(service, folderId, fileFullPath);

            }

            return folderId;
        }

        public async void DoUpload(DriveService service, string folderId, string fileFullPath)
        {
            Google.Apis.Upload.IUploadProgress prog;
            FileStream fsu = new FileStream(fileFullPath, FileMode.Open);

            try
            {
                Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider fpv = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
                string ContentType;
                fpv.TryGetContentType(fileFullPath, out ContentType);

                Google.Apis.Drive.v3.Data.File meta = new Google.Apis.Drive.v3.Data.File();
                meta.Name = Path.GetFileName(fileFullPath);
                meta.MimeType = ContentType;
                meta.Parents = new List<string>() { folderId };

                Google.Apis.Drive.v3.FilesResource.CreateMediaUpload req = service.Files.Create(meta, fsu, ContentType);
                req.Fields = "id, name";
                prog = await req.UploadAsync();

            }
            finally
            {
                fsu.Close();
            }

        }

        /// <summary>
        /// Drive内Folder作成
        /// </summary>
        /// <param name="service"></param>
        /// <returns>フォルダID</returns>
        public string CreateFolder(DriveService service)
        {
            Google.Apis.Drive.v3.Data.File meta = new Google.Apis.Drive.v3.Data.File();

            meta.Name = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            meta.MimeType = "application/vnd.google-apps.folder";
            meta.Parents = new List<string> { "1z0YgH6t0ryTAWGyiZdVA2IAs7MdIHpR4" }; // 特定のフォルダのサブフォルダとして作成する場合
            var request = service.Files.Create(meta);
            request.Fields = "id, name";
            var folder = request.ExecuteAsync().Result;

            return folder.Id;
        }

        /// <summary>
        /// フォルダ内ファイルID取得
        /// </summary>
        /// <param name="service"></param>
        /// <param name="folderId">フォルダID</param>
        /// <returns>ファイルID</returns>
        public List<string> GetFileID(DriveService service, string folderId)
        {
            List<string> fileID = new List<string>();

            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 1000;
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Q = string.Format("'{0}' in parents", folderId);

            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;

            if (files != null && files.Count > 0)
            {
                foreach (Google.Apis.Drive.v3.Data.File f in files)
                {
                    fileID.Add(f.Id);
                }
            }

            return fileID;
        }

        /// <summary>
        /// フォルダまたはファイル削除
        /// </summary>
        /// <param name="service"></param>
        /// <param name="fileid">フォルダまたはファイルID</param>
        public void DeleteFile(DriveService service, string fileid)
        {
            try
            {
                var request = service.Files.Delete(fileid);
                var result = request.ExecuteAsync().Result;
            }
            catch
            {

            }
        }

        /// <summary>
        /// 釣り日誌用フォルダ内ファイル削除
        /// </summary>
        public void DeleteAllFiles()
        {
            DirectoryInfo di = new DirectoryInfo(FolderPath);
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }
        }

        /// <summary>
        /// API Get(Response)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<string> GetResponse(string url)
        {
            using (var client = new HttpClient())
            {
                //GETリクエスト
                var res = await client.GetAsync(url);

                //取得
                var response = await res.Content.ReadAsStringAsync();

                return response;
            }
        }

        /// <summary>
        /// API Get(Json)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<T> Get<T>(string url) where T : class, new()
        {
            using (var client = new HttpClient())
            {
                //GETリクエスト
                var res = await client.GetAsync(url);

                //取得
                var response = await res.Content.ReadAsStringAsync();

                T obj = JsonConvert.DeserializeObject<T>(response) ?? new T();

                return obj;
            }
        }

        public class municipalities
        {
            public string status { get; set; } = string.Empty;

            public city[] data { get; set; } = new city[0];
        }

        public class city
        {
            public string id { get; set; } = string.Empty;

            public string name { get; set; } = string.Empty;
        }

        public void UpSert(string sql, IEnumerable<Parameter> parameters)
        {
            using (TransactionScope tran = new TransactionScope())
            {
                OpenConnection();
                using DbCommand cmd = Conn.CreateCommand();
                cmd.CommandText = sql;
                SetParameters(cmd, parameters);

                cmd.ExecuteNonQuery();

                tran.Complete();
            }
        }

        public T FetchOne<T>(string sql, IEnumerable<Parameter> parameters) where T : class, new()
        {

            OpenConnection();
            using DbCommand cmd = Conn.CreateCommand();
            cmd.CommandText = sql;
            SetParameters(cmd, parameters);

            T result = new();

            using DbDataReader reader = cmd.ExecuteReader();

            reader.Read();

            result = CreateInstance<T>(reader);

            return result;
        }

        public List<T> FetchAll<T>(string sql, IEnumerable<Parameter> parameters) where T : class, new()
        {

            OpenConnection();
            using DbCommand cmd = Conn.CreateCommand();
            cmd.CommandText = sql;
            SetParameters(cmd, parameters);

            List<T> result = new();

            using DbDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                result.Add(CreateInstance<T>(reader));
            }

            return result;
        }

        /// <summary>
        /// 取得したデータからインスタンスを生成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private T CreateInstance<T>(DbDataReader reader) where T : class, new()
        {

            List<string> columnNames = new();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }

            T result = new();

            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {

                // カラム名
                string columnName = propertyInfo.GetCustomAttribute<ColumnAttribute>()?.Name ?? propertyInfo.Name;

                if (!columnNames.Contains(columnName))
                {
                    continue;
                }

                object? value = reader[columnName];

                if (value is null || value == DBNull.Value)
                {
                    continue;
                }

                Type t = propertyInfo.PropertyType;
                t = Nullable.GetUnderlyingType(t) ?? t;

                if (t == typeof(string))
                {
                    value = value.ToString()?.TrimEnd();
                }

                propertyInfo.SetValue(result, Convert.ChangeType(value, t));
            }

            return result;

        }

        /// <summary>
        /// パラメータを登録
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        private void SetParameters(DbCommand cmd, IEnumerable<Parameter> parameters)
        {

            foreach (Parameter parameter in parameters)
            {
                DbParameter param = cmd.CreateParameter();

                param.ParameterName = parameter.Name;
                param.Value = parameter.Value is null ? DBNull.Value : parameter.Value;
                param.DbType = parameter.DbType;

                cmd.Parameters.Add(param);
            }

        }

        private void OpenConnection()
        {
            string ConnectionString = _conf.GetSection("ConnectionStrings").GetValue<string>("DefaultConnection");

            Conn = new NpgsqlConnection(ConnectionString);
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

        }

        /// <summary>
        /// パラメータ
        /// </summary>
        public class Parameter
        {

            public Parameter(string name, object? value)
            {
                Name = name;
                Value = value;
            }

            public Parameter(string name, object? value, DbType dbType)
            {
                Name = name;
                Value = value;
                DbType = dbType;
            }

            public object? Value { get; set; }

            public string Name { get; set; } = "";

            public DbType DbType { get; set; } = DbType.String;

        }

        public class TimeData
        {
            /// <summary>
            /// 時刻
            /// </summary>
            public int Hour { get; set; }

            /// <summary>
            /// 潮位
            /// </summary>
            public int High { get; set; }
        }

    }
}
