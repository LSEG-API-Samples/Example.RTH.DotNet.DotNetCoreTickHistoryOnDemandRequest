using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Example.DotNetCore.TRTHRESTAPI.Security
{

    public interface ODataContextInterface
    {
        [JsonProperty("odata.context", Order = 1)]
        string Metadata { get; set; }
    }
    public interface ODataTypeInterface
    {
        [JsonProperty("@odata.type", Order = 1)]

        string Metadata { get; set; }
    }
    [Serializable]
    class AuthorizeResponse : ODataContextInterface
    {
        public string Metadata { get; set; }
        public string value { get; set; }
    }
    [Serializable]
    class ValidateToken : ODataContextInterface
    {
        public string Metadata { get; set; }
        private bool _isValid;
        private DateTime _expires;

        public bool IsValid
        {
            get { return _isValid; }
            set { _isValid = value; }
        }
        public DateTime Expires
        {
            get { return _expires; }
            set { _expires = value; }
        }


    }
    [Serializable]
    public class TokenInfo
    {
        public TokenInfo() { }

        private string _token;
        private bool _isValid;
        private DateTime _dateTime;

        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }
        public bool IsValid
        {
            get { return _isValid; }
            set { _isValid = value; }
        }
        public DateTime Expires
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }
        public override string ToString()
        {
            return String.Format("====================\nToken={0}\nIsValid={1}\nExpires={2}\n====================",
                         Token, IsValid, Expires);
        }
    }

    [Serializable]
    public class Credentials
    {
        public Credentials()
        {
            _username = String.Empty;
            _password = String.Empty;
        }
        private String _username { get; set; }
        private String _password { get; set; }
        public String Username { get { return _username; } set { _username = value; } }
        public String Password { get { return _password; } set { _password = value; } }

    }
    [Serializable]
    public class Authentication
    {
        public Authentication()
        {
            _credential = new Credentials();
            _authenUri = new Uri("https://selectapi.datascope.refinitiv.com/RestApi/v1/Authentication/RequestToken");
        }
        private Credentials _credential;
        private Uri _authenUri;
        public Credentials Credentials { get { return _credential; } set { _credential = value; } }
        [JsonIgnore]
        public Uri AuthenUri { get { return _authenUri; } set { _authenUri = value; } }

        public async Task<string> GetToken()
        {
            return await GetToken(_credential.Username, _credential.Password, _authenUri);
        }
        public async Task<string> GetToken(string username, string password)
        {
            return await GetToken(username, password, _authenUri);
        }
        public async Task<string> GetToken(string username, string password,Uri authenUri)
        {

            _credential.Username = username;
            _credential.Password = password;
            var returnToken = "";
            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post,authenUri);
                request.Headers.Add("Prefer", "respond-async");
                request.Content = new StringContent(JsonConvert.SerializeObject(new Authentication() { Credentials = this._credential }, Formatting.Indented));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    returnToken = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthorizeResponse>(jsonData).value;
                }
                else
                {
                    throw new Exception(String.Format("Unable to Login to Tick Historical Server\n {0}", response.ToString()));
                }
                return returnToken;
            }

        }
        public async static Task<TokenInfo> IsValidToken(string token)
        {
            using (HttpClient client=new HttpClient())
            {
                var validateUri = new Uri("https://selectapi.datascope.refinitiv.com/RestApi/v1/Authentication/ValidateToken" + string.Format("(Token='{0}')",token));

                var resp=await client.GetAsync(validateUri);
                Console.WriteLine("Get Validate Token Result");
                var msg=await resp.Content.ReadAsStringAsync();
                var validateToken = Newtonsoft.Json.JsonConvert.DeserializeObject<ValidateToken>(msg);
                Console.WriteLine("IsValid={0} Expires={1}",validateToken.IsValid,validateToken.Expires);
                var tokenInfo = new TokenInfo();
                tokenInfo.Token = token;
                tokenInfo.IsValid = validateToken.IsValid;
                tokenInfo.Expires = validateToken.Expires;
                return tokenInfo;
            }
        }

    }
}
