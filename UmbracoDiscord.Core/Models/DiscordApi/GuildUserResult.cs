using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UmbracoDiscord.Core.Models.DiscordApi
{
    public class GuildUserResult
    {
        [JsonProperty("roles")]
        public decimal[] Roles { get; set; }



        //{
        //    "roles": [
        //    "886620150458118145",
        //    "886907490426568754"
        //        ],
        //    "nick": null,
        //    "avatar": null,
        //    "premium_since": null,
        //    "joined_at": "2021-08-19T22:34:56.285000+00:00",
        //    "is_pending": false,
        //    "pending": false,
        //    "user": {
        //        "id": "153809289541058560",
        //        "username": "Migaroez",
        //        "avatar": "7640cd51b0c5e41b9709845ad9ee2348",
        //        "discriminator": "1510",
        //        "public_flags": 0
        //    },
        //    "mute": false,
        //    "deaf": false
        //}
    }
}
