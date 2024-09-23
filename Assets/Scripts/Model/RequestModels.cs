using Newtonsoft.Json;

public class GlbAssetRequest
{
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("layer")]
        public string Layer;

        [JsonProperty("glb")]
        public string Glb;

        [JsonProperty("pose")]
        public string Pose;

        [JsonProperty("data")]
        public string Data;
}

public class ImageAssetRequest
{
        public string Name;

        public string Layer;

        public string Pose;

        public string Data;

        public string Path;
}

public class UserLoginRequest
{
        [JsonProperty("identity")]
        public string Identity;

        [JsonProperty("password")]
        public string Password;
}