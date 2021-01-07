using System;
using System.Collections.Generic;
using System.Text;

namespace LukeBot.Common.OAuth
{
    public class UserTokenResponse: PromiseData
    {
        public string code { get; set; }
        public List<string> scope { get; set; }
        public string state { get; set; }

        public override void Fill(PromiseData data)
        {
            UserTokenResponse r = (UserTokenResponse)data;

            code = r.code;
            scope = r.scope;
            state = r.state;
        }
    }
}
