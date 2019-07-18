using System;
using System.IO;
using System.Linq;
using LiteDB;
using WaykDen.Cmdlets;
using WaykDen.Models;
using WaykDen.Utils;

namespace WaykDen.Controllers
{
    public class DenConfigController
    {
        private const string WAYK_DEN_CONFIG_KEY = "WAYK_DEN_CONFIG_KEY";
        private const string DEN_IMAGE_CONFIG_COLLECTION = "DenImageConfig";
        private const string DEN_MONGO_CONFIG_COLLECTION = "DenMongoConfig";
        private const string DEN_PICKY_CONFIG_COLLECTION = "DenPickyConfig";
        private const string DEN_LUCID_CONFIG_COLLECTION = "DenLucidConfig";
        private const string DEN_ROUTER_CONFIG_COLLECTION = "DenRouterConfig";
        private const string DEN_SERVER_CONFIG_COLLECTION = "DenServerConfig";
        private const string DEN_TRAEFIK_CONFIG_COLLECTION = "DenTraefikConfig";
        private const string DEN_DOCKER_CONFIG_COLLECTION = "DenDockerConfig";
        private const int DB_ID = 1;
        private string path;
        private string password;
        private string connString = string.Empty;
        public DenConfigController(string path, string password)
        {
            this.path = $"{path}/WaykDen.db";
            this.password = string.IsNullOrEmpty(password) ? string.Empty : password;
            this.connString = string.IsNullOrEmpty(password) ? $"Filename={this.path}; Mode=Exclusive" : $"Filename={this.path}; Password={this.password}; Mode=Exclusive";
            if(File.Exists(this.path))
            {
                this.TestConnString();
            }            

            BsonMapper.Global.EmptyStringToNull = false;
        }

        private void TestConnString()
        {
            try
            {
                using(var db = new LiteDatabase(this.connString))
                {
                    var collections = db.GetCollectionNames();
                }
            }
            catch(Exception)
            {
                throw new Exception("Invalid database password.");
            }
            
            if(!string.IsNullOrEmpty(this.password))
            {
                Environment.SetEnvironmentVariable(WAYK_DEN_CONFIG_KEY, this.password);
            }
        }

        public bool DbExists
        {
            get
            {
                using(var db = new LiteDatabase(this.connString))
                {
                    var collections = db.GetCollectionNames().ToArray();
                    if(collections.Length > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        public void StoreConfig(DenConfig config)
        {
            using(var db = new LiteDatabase(this.connString))
            {
                if(db.CollectionExists(DEN_IMAGE_CONFIG_COLLECTION))
                {
                    this.Update(db, config);
                } else this.Store(db ,config);
            }
        }

        private void Store(LiteDatabase db, DenConfig config)
        {
            this.StoreImage(db, config.DenImageConfigObject);
            this.StoreMongo(db, config.DenMongoConfigObject);
            this.StorePicky(db,config.DenPickyConfigObject);
            this.StoreLucid(db, config.DenLucidConfigObject);
            this.StoreRouter(db, config.DenRouterConfigObject);
            this.StoreServer(db, config.DenServerConfigObject);
            this.StoreTraefik(db, config.DenTraefikConfigObject);
            this.StoreDocker(db, config.DenDockerConfigObject);
        }

        private void Update(LiteDatabase db, DenConfig config)
        {
            this.UpdateImage(db, config.DenImageConfigObject);
            this.UpdateMongo(db, config.DenMongoConfigObject);
            this.UpdatePicky(db, config.DenPickyConfigObject);
            this.UpdateLucid(db, config.DenLucidConfigObject);
            this.UpdateRouter(db, config.DenRouterConfigObject);
            this.UpdateServer(db, config.DenServerConfigObject);
            this.UpdateTraefik(db, config.DenTraefikConfigObject);
            this.UpdateDocker(db, config.DenDockerConfigObject);
        }

        public DenConfig GetConfig()
        {
            if(!this.DbExists)
            {
                throw new Exception("Could not found WaykDen configuration in given path.Use New-WaykDenConfig or make sure WaykDen configuration is in current folder or set WAYK_DEN_HOME to the path of WaykDen configuration");
            }

            using(var db = new LiteDatabase(this.connString))
            {
                return new DenConfig()
                {
                    DenImageConfigObject = this.GetImage(db),
                    DenLucidConfigObject = this.GetLucid(db),
                    DenPickyConfigObject = this.GetPicky(db),
                    DenMongoConfigObject = this.GetMongo(db),
                    DenRouterConfigObject = this.GetRouter(db),
                    DenServerConfigObject = this.GetServer(db),
                    DenTraefikConfigObject = this.GetTraefik(db),
                    DenDockerConfigObject = this.GetDocker(db)
                };
            }
        }

        private DenImageConfigObject GetImage(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_IMAGE_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool mongoOk = values.TryGetValue(nameof(DenImageConfigObject.DenMongoImage), out var mongo);
            bool lucidOk = values.TryGetValue(nameof(DenImageConfigObject.DenLucidImage), out var lucid);
            bool pickyOk = values.TryGetValue(nameof(DenImageConfigObject.DenPickyImage), out var picky);
            bool routerOk = values.TryGetValue(nameof(DenImageConfigObject.DenRouterImage), out var router);
            bool serverOk = values.TryGetValue(nameof(DenImageConfigObject.DenServerImage), out var server);
            bool traefikOk = values.TryGetValue(nameof(DenImageConfigObject.DenTraefikImage), out var traefik);
            bool jetOk = values.TryGetValue(nameof(DenImageConfigObject.DevolutionsJetImage), out var jet);
            return new DenImageConfigObject()
            {
                DenMongoImage = mongoOk ? mongo?.ToString().Trim('\"') : string.Empty,
                DenLucidImage = lucidOk ? lucid?.ToString().Trim('\"') : string.Empty,
                DenPickyImage = pickyOk ? picky?.ToString().Trim('\"') : string.Empty,
                DenRouterImage = routerOk ? router?.ToString().Trim('\"') : string.Empty,
                DenServerImage = serverOk ? server?.ToString().Trim('\"') : string.Empty,
                DenTraefikImage = traefikOk ? traefik?.ToString().Trim('\"') : string.Empty,
                DevolutionsJetImage = jetOk ? jet?.ToString().Trim('\"') : string.Empty
            };
        }

        private DenMongoConfigObject GetMongo(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_MONGO_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool urlOk = values.TryGetValue(nameof(DenMongoConfigObject.Url), out var url);
            bool portOk = values.TryGetValue(nameof(DenMongoConfigObject.Port), out var port);
            return new DenMongoConfigObject()
            {
                Url = urlOk ? url.ToString().Trim('\"') : string.Empty,
                Port = portOk ? port.ToString().Trim('\"') : string.Empty
            };
        }

        private DenPickyConfigObject GetPicky(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_PICKY_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool realmOk = values.TryGetValue(nameof(DenPickyConfigObject.Realm), out var realm);
            bool apiKeyOk = values.TryGetValue(nameof(DenPickyConfigObject.ApiKey), out var apikey);
            bool backendOk = values.TryGetValue(nameof(DenPickyConfigObject.Backend), out var backend);
            return new DenPickyConfigObject()
            {
                Realm = realmOk ? realm.ToString().Trim('\"') : string.Empty,
                ApiKey = apiKeyOk ? apikey.ToString().Trim('\"') : string.Empty,
                Backend = backendOk ? backend.ToString().Trim('\"') : string.Empty
            };
        }

        private DenLucidConfigObject GetLucid(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_LUCID_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool apiKeyOk = values.TryGetValue(nameof(DenLucidConfigObject.ApiKey), out var apikey);
            bool adminSecretOk = values.TryGetValue(nameof(DenLucidConfigObject.AdminSecret), out var adminsecret);
            bool adminUsernameOk = values.TryGetValue(nameof(DenLucidConfigObject.AdminUsername), out var adminusername);
            return new DenLucidConfigObject()
            {
                ApiKey = apiKeyOk ? apikey.ToString().Trim('\"') : string.Empty,
                AdminSecret = adminSecretOk ? adminsecret.ToString().Trim('\"') : string.Empty,
                AdminUsername = adminUsernameOk ? adminusername.ToString().Trim('\"') : string.Empty
            };
        }

        private DenRouterConfigObject GetRouter(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_ROUTER_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            values.TryGetValue(nameof(DenRouterConfigObject.PublicKey), out var pubkey);
            return new DenRouterConfigObject()
            {
                PublicKey = pubkey
            };
        }

        private DenServerConfigObject GetServer(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_SERVER_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool apiKeyOk = values.TryGetValue(nameof(DenServerConfigObject.ApiKey), out var apikey);
            bool auditTrailsOK = values.TryGetValue(nameof(DenServerConfigObject.AuditTrails), out var auditTrails);
            bool externalUrlOk = values.TryGetValue(nameof(DenServerConfigObject.ExternalUrl), out var externalUrl);
            bool ldapServerTypeOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPServerType), out var ldapservertype);
            bool ldapPasswordOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPPassword), out var ldappassword);
            bool ldapServerUrlOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPServerUrl), out var ldapserverurl);
            bool ldapUserGroupOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPUserGroup), out var ldapusergroup);
            bool ldapUsernameOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPUsername), out var ldapusername);
            bool ldapBaseDnOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPBaseDN), out var ldapbasedn);
            values.TryGetValue(nameof(DenServerConfigObject.PrivateKey), out var privatekey);
            bool jetServerUrlOk = values.TryGetValue(nameof(DenServerConfigObject.JetServerUrl), out var jetServerUrl);
            bool loginRequiredOk = values.TryGetValue(nameof(DenServerConfigObject.LoginRequired), out var loginRequired);
            return new DenServerConfigObject()
            {
                ApiKey = apiKeyOk ? apikey.ToString().Trim('\"') : string.Empty,
                AuditTrails = auditTrailsOK ?  auditTrails.ToString().Trim('\"') : string.Empty,
                ExternalUrl = externalUrlOk ?  externalUrl.ToString().Trim('\"') : string.Empty,
                LDAPServerType = ldapServerTypeOk ?  ldapservertype.ToString().Trim('\"') : string.Empty,
                LDAPBaseDN = ldapBaseDnOk?  ldapbasedn.ToString().Trim('\"') : string.Empty,
                LDAPPassword = ldapPasswordOk ?  ldappassword.ToString().Trim('\"') : string.Empty,
                LDAPServerUrl = ldapServerUrlOk ?  ldapserverurl.ToString().Trim('\"') : string.Empty,
                LDAPUserGroup = ldapUserGroupOk ?  ldapusergroup.ToString().Trim('\"') : string.Empty,
                LDAPUsername = ldapUsernameOk ?  ldapusername.ToString().Trim('\"') : string.Empty,
                PrivateKey = privatekey,
                JetServerUrl = jetServerUrlOk ?  jetServerUrl.ToString().Trim('\"') : string.Empty,
                LoginRequired = loginRequiredOk ?  loginRequired.ToString().Trim('\"') : "false"
            };
        }

        private DenTraefikConfigObject GetTraefik(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_TRAEFIK_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool apiPortOk = values.TryGetValue(nameof(DenTraefikConfigObject.ApiPort), out var apiPort);
            bool waykDenPortOk =  values.TryGetValue(nameof(DenTraefikConfigObject.WaykDenPort), out var waykDenPort);
            bool certificateOk = values.TryGetValue(nameof(DenTraefikConfigObject.Certificate), out var certificate);
            bool privateKeyOk = values.TryGetValue(nameof(DenTraefikConfigObject.PrivateKey), out var privateKey);
            return new DenTraefikConfigObject
            {
                ApiPort = apiPortOk ? apiPort.ToString().Trim('\"') : string.Empty,
                WaykDenPort = waykDenPortOk ? waykDenPort.ToString().Trim('\"') : string.Empty,
                Certificate = certificateOk ? certificate.ToString().Trim('\"') : string.Empty,
                PrivateKey = privateKey
            };
        }

        private DenDockerConfigObject GetDocker(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_DOCKER_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool dockerclientUriOk = values.TryGetValue(nameof(DenDockerConfigObject.DockerClientUri), out var dockerclienturi);
            return new DenDockerConfigObject()
            {
                DockerClientUri = dockerclientUriOk ?  dockerclienturi.ToString().Trim('\"') : string.Empty
            };
        }

        private void StoreImage(LiteDatabase db, DenImageConfigObject images)
        {
            var col = db.GetCollection<DenImageConfigObject>(DEN_IMAGE_CONFIG_COLLECTION);
            col.Insert(DB_ID, images);
        }

        private void StoreMongo(LiteDatabase db, DenMongoConfigObject mongo)
        {
            var col = db.GetCollection<DenMongoConfigObject>(DEN_MONGO_CONFIG_COLLECTION);
            col.Insert(DB_ID, mongo);
        }

        private void StorePicky(LiteDatabase db, DenPickyConfigObject picky)
        {
            var col = db.GetCollection<DenPickyConfigObject>(DEN_PICKY_CONFIG_COLLECTION);
            col.Insert(DB_ID, picky);
        }

        private void StoreLucid(LiteDatabase db, DenLucidConfigObject lucid)
        {
            var col = db.GetCollection<DenLucidConfigObject>(DEN_LUCID_CONFIG_COLLECTION);
            col.Insert(DB_ID, lucid);
        }

        private void StoreRouter(LiteDatabase db, DenRouterConfigObject router)
        {
            var col = db.GetCollection<DenRouterConfigObject>(DEN_ROUTER_CONFIG_COLLECTION);
            col.Insert(DB_ID, router);
        }

        private void StoreServer(LiteDatabase db, DenServerConfigObject server)
        {
            var col = db.GetCollection<DenServerConfigObject>(DEN_SERVER_CONFIG_COLLECTION);
            col.Insert(DB_ID, server);
        }

        private void StoreTraefik(LiteDatabase db, DenTraefikConfigObject traefik)
        {
            var col = db.GetCollection<DenTraefikConfigObject>(DEN_TRAEFIK_CONFIG_COLLECTION);
            col.Insert(DB_ID, traefik);
        }

        private void StoreDocker(LiteDatabase db, DenDockerConfigObject docker)
        {
            var col = db.GetCollection<DenDockerConfigObject>(DEN_DOCKER_CONFIG_COLLECTION);
            col.Insert(DB_ID, docker);
        }

        private void UpdateImage(LiteDatabase db, DenImageConfigObject images)
        {
            var col = db.GetCollection<DenImageConfigObject>(DEN_IMAGE_CONFIG_COLLECTION);
            col.Update(DB_ID, images);
        }

        private void UpdateMongo(LiteDatabase db, DenMongoConfigObject mongo)
        {
            var col = db.GetCollection<DenMongoConfigObject>(DEN_MONGO_CONFIG_COLLECTION);
            col.Update(DB_ID, mongo);
        }

        private void UpdatePicky(LiteDatabase db, DenPickyConfigObject picky)
        {
            var col = db.GetCollection<DenPickyConfigObject>(DEN_PICKY_CONFIG_COLLECTION);
            col.Update(DB_ID, picky);
        }

        private void UpdateLucid(LiteDatabase db, DenLucidConfigObject lucid)
        {
            var col = db.GetCollection<DenLucidConfigObject>(DEN_LUCID_CONFIG_COLLECTION);
            col.Update(DB_ID, lucid);
        }

        private void UpdateRouter(LiteDatabase db, DenRouterConfigObject router)
        {
            var col = db.GetCollection<DenRouterConfigObject>(DEN_ROUTER_CONFIG_COLLECTION);
            col.Update(DB_ID, router);
        }

        private void UpdateServer(LiteDatabase db, DenServerConfigObject server)
        {
            var col = db.GetCollection<DenServerConfigObject>(DEN_SERVER_CONFIG_COLLECTION);
            col.Update(DB_ID, server);
        }

        private void UpdateTraefik(LiteDatabase db, DenTraefikConfigObject traefik)
        {
            var col = db.GetCollection<DenTraefikConfigObject>(DEN_TRAEFIK_CONFIG_COLLECTION);
            col.Update(DB_ID, traefik);
        }

        private void UpdateDocker(LiteDatabase db, DenDockerConfigObject docker)
        {
            var col = db.GetCollection<DenDockerConfigObject>(DEN_DOCKER_CONFIG_COLLECTION);
            col.Update(DB_ID, docker);
        }

        private string LoadPassword()
        {
            try
            {
                string dir = Directory.GetParent(this.path).FullName;
                string file = $"{dir}/WaykDen.key";
                if(!File.Exists(file))
                {
                    string pswd = DenServiceUtils.Generate(20);
                    File.WriteAllText(file, pswd.Replace("-", string.Empty));
                    return pswd;
                }

                return File.ReadAllText(file);
            }
            catch(Exception e)
            {
                e.ToString();
                return string.Empty;
            }
        }

        public void AddConfigKey(string newKey)
        {
            using(var db = new LiteDatabase($"Filename={path}; Mode=Exclusive"))
            {
                db.Engine.Shrink(newKey);
            }
        }

        public void RemoveConfigKey(string key)
        {
            using(var db = new LiteDatabase($"Filename={path}; Password={key}; Mode=Exclusive"))
            {
                db.Engine.Shrink();
                this.password = null;
                Environment.SetEnvironmentVariable(WAYK_DEN_CONFIG_KEY, this.password);
            }
        }

        public void ChangeConfigKey(string newKey, string oldKey)
        {
            using(var db = new LiteDatabase($"Filename={path}; Password={oldKey}; Mode=Exclusive"))
            {
                db.Engine.Shrink(newKey);
                this.password = newKey;
                Environment.SetEnvironmentVariable(WAYK_DEN_CONFIG_KEY, this.password);
            }
        }
    }
}